using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Reservoir;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// RES-07 against a real SQLite database: a saved forecast reloads with identical points (AC-1, SG-06-7), every save is a
/// new version and none overwrites another (AC-3), the method and source fit are recorded (AC-2), a second person approves
/// and the approved forecast is immutable (AC-4), and a save whose acknowledgement was lost is retained and retried.
/// Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class ForecastGatewayTests
{
    private const string ConnectionName = "res07-forecast";
    private static readonly DateTime Start = new(2026, 1, 1);

    [IntegrationFact]
    public async Task A_forecast_is_saved_versioned_and_approved_and_reloads_identically()
    {        var root = Path.Combine(OgPaths.DataRoot, "res07-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasRes07"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "forecast.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "forecast.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgForecastService(() => source, reporter);

        // ---- the workspace ---------------------------------------------------
        var loaded = await service.LoadAsync("F-1", "Alpha field");

        Assert.True(loaded.Success, loaded.Message);
        Assert.Empty(loaded.Value!.Forecasts);
        Assert.True(loaded.Value.HasFits);
        Assert.True(loaded.Value.ReferenceFromTable);
        Assert.Equal(["Decline", "Hyperbolic"], loaded.Value.Methods.OrderBy(name => name).ToList());
        Assert.Contains(ForecastStatuses.Approved, loaded.Value.Statuses);
        Assert.Contains("Unknown", loaded.Value.RiskRatings);

        // ---- AC-2: generate from the RES-06 fit ------------------------------
        var inputs = new OgForecastInputs(
            "Base case", "WELL-1", "F-1", "Hyperbolic", "FIT-1", Start, 24, null, null, "Unknown");

        var generated = await service.GenerateAsync(inputs);

        Assert.True(generated.Success, generated.Message + " :: " + string.Join(" | ", reporter.Failures));

        var draft = generated.Value!.Selected!;

        Assert.False(draft.IsRefused, draft.RefusalText);
        Assert.Equal(24, draft.Points.Count);
        Assert.Equal("FIT-1", draft.SourceFitId);
        Assert.Equal("Hyperbolic", draft.Method);
        Assert.Equal("OIL", draft.Fluid);
        Assert.Equal("bbl", draft.OilUnit);

        // ---- save, then reload identically (AC-1, SG-06-7) -------------------
        var saved = await service.SaveAsync(draft, "sara");

        Assert.True(saved.Success, saved.Message);

        var stored = saved.Value!.Selected!;

        Assert.True(stored.IsSaved);
        Assert.Equal(1, stored.Version);
        Assert.Equal(ForecastStatuses.Draft, stored.Status);
        Assert.Equal("sara", stored.Actor);
        Assert.False(stored.IsApproved);
        Assert.Equal(1, CountRows(database, "PRODUCTION_FORECAST"));
        Assert.Equal(24, CountRows(database, "PRODUCTION_FORECAST_POINT"));
        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM PRODUCTION_FORECAST WHERE SOURCE_FIT_ID = 'FIT-1' AND VERSION = 1 " +
            "AND OIL_VOLUME_OUOM = 'bbl' AND GAS_VOLUME_OUOM IS NULL;"));

        var reloaded = await service.LoadAsync("F-1", "Alpha field", stored.Id);

        Assert.True(reloaded.Success, reloaded.Message);

        var again = reloaded.Value!.Selected!;

        // Point by point: the same date, the same three rates, the same volume and the same running total.
        Assert.Equal(stored.Points.Count, again.Points.Count);

        for (var index = 0; index < stored.Points.Count; index++)
        {
            Assert.Equal(stored.Points[index].Date, again.Points[index].Date);
            Assert.Equal(stored.Points[index].Oil, again.Points[index].Oil);
            Assert.Equal(stored.Points[index].Gas, again.Points[index].Gas);
            Assert.Equal(stored.Points[index].Water, again.Points[index].Water);
            Assert.Equal(stored.Points[index].Volume, again.Points[index].Volume);
            Assert.Equal(stored.Points[index].Cumulative, again.Points[index].Cumulative);
            Assert.Equal(stored.Points[index].Unit, again.Points[index].Unit);
        }

        Assert.Equal(stored.Total, again.Total);
        Assert.Equal(stored.PointsText, again.PointsText);

        // ---- AC-3: a second save is a new version, and the first is untouched -
        var second = await service.SaveAsync(draft, "sara");

        Assert.True(second.Success, second.Message);
        Assert.Equal(2, CountRows(database, "PRODUCTION_FORECAST"));
        Assert.Equal(48, CountRows(database, "PRODUCTION_FORECAST_POINT"));

        var versions = second.Value!.Forecasts
            .Where(forecast => forecast.Name == "Base case")
            .OrderBy(forecast => forecast.Version)
            .ToList();

        Assert.Equal(2, versions.Count);
        Assert.Equal(2, versions[1].Version);
        Assert.Equal(stored.Id, versions[1].VersionOf);

        // Version 1 still holds its own points and figures.
        var firstStill = (await service.LoadAsync("F-1", "Alpha field", stored.Id)).Value!.Selected!;

        Assert.Equal(1, firstStill.Version);
        Assert.Equal(stored.Total, firstStill.Total);
        Assert.Equal(stored.PointsText, firstStill.PointsText);

        // ---- AC-4: approval needs a second person, then locks the forecast ----
        var selfApproval = await service.ApproveAsync(stored.Id, "sara");

        Assert.False(selfApproval.Success);
        Assert.Equal(ForecastRules.CodeApproverIsCreator, selfApproval.Reference);

        var approved = await service.ApproveAsync(stored.Id, "reviewer");

        Assert.True(approved.Success, approved.Message);

        var approvedForecast = approved.Value!.Selected!;

        Assert.True(approvedForecast.IsApproved);
        Assert.True(approvedForecast.Immutable);
        Assert.Equal("reviewer", approvedForecast.ApprovedBy);
        Assert.NotNull(approvedForecast.ApprovedAt);
        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM PRODUCTION_FORECAST WHERE STATUS = 'Approved' AND IMMUTABLE_IND = 'Y' " +
            "AND APPROVED_BY = 'reviewer';"));

        // Approving again is refused: an approved forecast is immutable.
        var twice = await service.ApproveAsync(stored.Id, "reviewer");

        Assert.False(twice.Success);
        Assert.Equal(ForecastRules.CodeAlreadyApproved, twice.Reference);

        // ---- the state table's "save acknowledgement lost" -------------------
        Drop(database, "PRODUCTION_FORECAST_POINT");

        var lost = await service.SaveAsync(draft, "sara");

        Assert.False(lost.Success);
        Assert.NotNull(service.Pending);
        Assert.Equal("Base case", service.Pending!.Forecast.Name);
        Assert.Equal(2, CountRows(database, "PRODUCTION_FORECAST"));

        CreatePointTable(database);

        var retried = await service.RetrySaveAsync("sara");

        Assert.True(retried.Success, retried.Message);
        Assert.Null(service.Pending);
        Assert.Equal(3, CountRows(database, "PRODUCTION_FORECAST"));

        // The point table was dropped to cause the failure, so only the retried version's 24 points exist; the three
        // forecast rows are what proves the earlier versions were not rewritten.
        Assert.Equal(24, CountRows(database, "PRODUCTION_FORECAST_POINT"));

        // ---- an unavailable history is a state -------------------------------
        Drop(database, "PRODUCTION_FORECAST");

        var unavailable = await service.LoadAsync("F-1", "Alpha field");

        Assert.True(unavailable.Success, unavailable.Message);
        Assert.True(unavailable.Value!.HistoryFailed);
        Assert.Empty(unavailable.Value.Forecasts);
        Assert.Contains("could not be read", unavailable.Value.HistoryStateText, StringComparison.Ordinal);
    }

    private static void Seed(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE DECLINE_FIT (
                FIT_ID TEXT NOT NULL PRIMARY KEY,
                WELL_ID TEXT NULL,
                POOL_ID TEXT NULL,
                FLUID TEXT NULL,
                MODEL TEXT NOT NULL,
                OUTCOME TEXT NOT NULL,
                REFUSAL_CODE TEXT NULL,
                REFUSAL_MESSAGE TEXT NULL,
                QI REAL NULL,
                DI_PER_DAY REAL NULL,
                B REAL NULL,
                BOUNDED_IND TEXT NOT NULL,
                EUR REAL NULL,
                ECONOMIC_LIMIT REAL NULL,
                VOLUME_OUOM TEXT NULL,
                R_SQUARED REAL NULL,
                POINTS_USED INTEGER NOT NULL,
                POINTS_EXCLUDED INTEGER NOT NULL,
                EXCLUSIONS_TEXT TEXT NULL,
                HISTORY_FROM TEXT NULL,
                HISTORY_TO TEXT NULL,
                CROSS_CHECK_SOURCE TEXT NULL,
                CROSS_CHECK_QI REAL NULL,
                CROSS_CHECK_B REAL NULL,
                CROSS_CHECK_AGREES TEXT NULL,
                CROSS_CHECK_TEXT TEXT NULL,
                ACTOR TEXT NULL,
                RECORDED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE R_PRODUCTION_FORECASTING_REFERENCE_CODE (
                REFERENCE_SET TEXT NOT NULL,
                REFERENCE_CODE TEXT NOT NULL,
                LONG_NAME TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL,
                PRIMARY KEY (REFERENCE_SET, REFERENCE_CODE));

            INSERT INTO R_PRODUCTION_FORECASTING_REFERENCE_CODE (REFERENCE_SET, REFERENCE_CODE, LONG_NAME) VALUES
                ('PRODUCTION_FORECAST_METHOD', 'Decline', 'Decline'),
                ('PRODUCTION_FORECAST_METHOD', 'Hyperbolic', 'Hyperbolic'),
                ('PRODUCTION_FORECAST_RUN_STATUS', 'Draft', 'Draft - generated but not reviewed'),
                ('PRODUCTION_FORECAST_RUN_STATUS', 'Approved', 'Approved - reviewed and immutable'),
                ('PRODUCTION_FORECAST_RISK_RATING', 'Unknown', 'Unknown risk rating');

            INSERT INTO DECLINE_FIT (FIT_ID, WELL_ID, FLUID, MODEL, OUTCOME, QI, DI_PER_DAY, B, BOUNDED_IND,
                VOLUME_OUOM, R_SQUARED, POINTS_USED, POINTS_EXCLUDED, ACTOR, RECORDED_AT) VALUES
                ('FIT-1', 'WELL-1', 'OIL', 'HYPERBOLIC', 'OK', 1000, 0.01, 0.5, 'Y', 'bbl', 1, 24, 0, 'sara', '2026-01-01');
            """;
        command.ExecuteNonQuery();

        CreateForecastTable(database);
        CreatePointTable(database);
    }

    private static void CreateForecastTable(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE PRODUCTION_FORECAST (
                PRODUCTION_FORECAST_ID TEXT NOT NULL PRIMARY KEY,
                RESERVOIR_FORECAST_PROPERTIES_ID TEXT NULL,
                FORECAST_TYPE INTEGER NOT NULL,
                FORECAST_DURATION REAL NOT NULL,
                INITIAL_PRODUCTION_RATE REAL NOT NULL,
                FINAL_PRODUCTION_RATE REAL NOT NULL,
                TOTAL_CUMULATIVE_PRODUCTION REAL NOT NULL,
                AREA_ID TEXT NULL,
                AREA_TYPE TEXT NULL,
                BUSINESS_ASSOCIATE_ID TEXT NULL,
                FORECAST_ID TEXT NULL,
                WELL_UWI TEXT NULL,
                FIELD_ID TEXT NULL,
                FORECAST_NAME TEXT NULL,
                FORECAST_START_DATE TEXT NOT NULL,
                TOTAL_FORCAST_PRODUCTION REAL NOT NULL,
                AVERAGE_MONTHLY_PRODUCTION REAL NOT NULL,
                ECONOMIC_LIMIT_MONTH INTEGER NOT NULL,
                SOURCE_FIT_ID TEXT NULL,
                STATUS TEXT NOT NULL,
                RISK_RATING TEXT NULL,
                VERSION INTEGER NOT NULL,
                VERSION_OF TEXT NULL,
                IMMUTABLE_IND TEXT NOT NULL,
                ECONOMIC_LIMIT REAL NULL,
                MAX_RATE REAL NULL,
                OIL_VOLUME_OUOM TEXT NULL,
                GAS_VOLUME_OUOM TEXT NULL,
                WATER_VOLUME_OUOM TEXT NULL,
                ACTOR TEXT NULL,
                RECORDED_AT TEXT NULL,
                APPROVED_BY TEXT NULL,
                APPROVED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);
            """;
        command.ExecuteNonQuery();
    }

    private static void CreatePointTable(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE PRODUCTION_FORECAST_POINT (
                FORECAST_POINT_ID TEXT NOT NULL PRIMARY KEY,
                FORECAST_ID TEXT NOT NULL,
                FORECAST_DATE TEXT NULL,
                OIL_RATE REAL NULL,
                GAS_RATE REAL NULL,
                WATER_RATE REAL NULL,
                MONTHLY_VOLUME REAL NOT NULL,
                CUMULATIVE_VOLUME REAL NOT NULL,
                VOLUME_OUOM TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);
            """;
        command.ExecuteNonQuery();
    }

    private static long Scalar(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        return Convert.ToInt64(command.ExecuteScalar());
    }

    private static long CountRows(string database, string table) =>
        Scalar(database, $"SELECT COUNT(*) FROM {table};");

    private static void Drop(string database, string table)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = $"DROP TABLE {table};";
        command.ExecuteNonQuery();
    }

    private sealed class Recording : IFailureReporter
    {
        public List<string> Failures { get; } = [];

        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error)
        {
            Failures.Add($"{operation} | {consequence} | {exception.Message}");
            return FailureReferenceFormat.New();
        }
    }
}
