using Beep.OilandGas.PlungerLift.Constants;
using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Lift;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// LFT-05 against a real SQLite database: a feasible assessment saves with its plunger type, its cycle and every criterion
/// (AC-6), an infeasible one is refused and writes nothing (AC-2), an uneconomic one is stored as uneconomic with its basis
/// (AC-3), a save whose acknowledgement was lost is retained and retried, and an unavailable history is a state.
/// Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class PlungerGatewayTests
{
    private const string ConnectionName = "lft05-plunger";

    [IntegrationFact]
    public async Task A_feasible_assessment_saves_with_its_type_cycle_and_criteria()
    {        var root = Path.Combine(OgPaths.DataRoot, "lft05-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasLft05"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "plunger.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "plunger.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgPlungerService(() => source, reporter);

        // ---- the workspace ---------------------------------------------------
        var loaded = await service.LoadAsync();

        Assert.True(loaded.Success, loaded.Message);
        Assert.Empty(loaded.Value!.Designs);
        Assert.False(loaded.Value.HistoryFailed);
        Assert.Contains(PlungerLiftConstants.PlungerTypeBar, loaded.Value.PlungerTypes);
        Assert.Equal(PlungerLiftConstants.StandardTubingDiameters, loaded.Value.StandardTubingDiameters);
        Assert.Contains("ft/min", loaded.Value.ReferenceText, StringComparison.Ordinal);

        // ---- AC-2: an infeasible well is refused and writes nothing ----------
        var infeasible = (await service.AssessAsync(Inputs() with { GasLiquidRatio = 100m })).Value!.Selected!;

        Assert.True(infeasible.IsInfeasible);
        Assert.Null(infeasible.DailyRate);

        var refused = await service.SaveAsync(infeasible, "sara");

        Assert.False(refused.Success);
        Assert.Equal(PlungerRules.CodeNothingToSave, refused.Reference);
        Assert.Equal(0, CountRows(database));

        // ---- assess and save a feasible one ----------------------------------
        var assessed = await service.AssessAsync(Inputs());

        Assert.True(assessed.Success, assessed.Message + " :: " + string.Join(" | ", reporter.Failures));

        var result = assessed.Value!.Selected!;

        Assert.True(result.IsFeasible, result.FailedText);
        Assert.True(result.Economic);
        Assert.NotNull(result.DailyRate);

        var saved = await service.SaveAsync(result, "sara");

        Assert.True(saved.Success, saved.Message);
        Assert.Equal(1, CountRows(database));

        // AC-6: the type, the cycle, the verdict and every criterion are on the row.
        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM PLUNGER_DESIGN WHERE PLUNGER_TYPE = 'BAR' AND FEASIBILITY = 'FEASIBLE' " +
            "AND ECONOMIC_IND = 'Y' AND DAILY_RATE_BBLD IS NOT NULL AND TUBING_DIAMETER_IN = 2.375 " +
            "AND CYCLE_TIME_MIN = FALL_TIME_MIN + RISE_TIME_MIN + SHUT_IN_TIME_MIN " +
            "AND CRITERIA_TEXT LIKE '%glr_range%' AND CRITERIA_TEXT LIKE '%slug_size_range%' " +
            "AND FAILED_CRITERIA IS NULL AND ACTOR = 'sara';"));

        // ---- the reload keeps the cycle and the verdict ----------------------
        var reloaded = await service.LoadAsync("WELL-1");

        Assert.True(reloaded.Success, reloaded.Message);

        var again = reloaded.Value!.Selected!;

        Assert.True(again.IsFeasible);
        Assert.Equal(PlungerLiftConstants.PlungerTypeBar, again.PlungerType);
        // SQLite stores these as REAL, so the round trip is compared to 1e-3 rather than bit-exactly.
        Assert.InRange(Math.Abs(result.CycleTime - again.CycleTime), 0m, 0.001m);
        Assert.InRange(Math.Abs(result.CyclesPerDay - again.CyclesPerDay), 0m, 0.001m);
        Assert.InRange(Math.Abs(result.DailyRate!.Value - again.DailyRate!.Value), 0m, 0.001m);
        Assert.InRange(Math.Abs(result.RiseVelocityFpm - again.RiseVelocityFpm), 0m, 0.001m);
        Assert.Equal(2.375m, again.TubingDiameter);
        Assert.Equal("sara", again.Actor);
        Assert.Equal(6, again.Criteria.Count);

        // ---- AC-3: an uneconomic well is stored as uneconomic ---------------
        var uneconomic = (await service.AssessAsync(Inputs() with { LiquidRateBblPerDay = 400m }))
            .Value!.Selected!;

        Assert.True(uneconomic.IsUneconomic);
        Assert.False(uneconomic.IsInfeasible);

        var savedUneconomic = await service.SaveAsync(uneconomic, "sara");

        Assert.True(savedUneconomic.Success, savedUneconomic.Message);
        Assert.Equal(2, CountRows(database));
        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM PLUNGER_DESIGN WHERE FEASIBILITY = 'UNECONOMIC' AND ECONOMIC_IND = 'N' " +
            "AND ECONOMIC_BASIS LIKE '%against an inflow of%';"));

        // ---- the state table's "lost acknowledgement" ------------------------
        Drop(database);

        var lost = await service.SaveAsync(result, "sara");

        Assert.False(lost.Success);
        Assert.NotNull(service.Pending);
        Assert.Equal("WELL-1", service.Pending!.Result.WellId);

        CreateTable(database);

        var retried = await service.RetrySaveAsync("sara");

        Assert.True(retried.Success, retried.Message);
        Assert.Null(service.Pending);
        Assert.Equal(1, CountRows(database));

        // ---- an unavailable history is a state -------------------------------
        Drop(database);

        var unavailable = await service.LoadAsync();

        Assert.True(unavailable.Success, unavailable.Message);
        Assert.True(unavailable.Value!.HistoryFailed);
        Assert.Empty(unavailable.Value.Designs);
        Assert.Contains("could not be read", unavailable.Value.HistoryStateText, StringComparison.Ordinal);
    }

    private static void Seed(string database) => CreateTable(database);

    private static void CreateTable(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE PLUNGER_DESIGN (
                DESIGN_ID TEXT NOT NULL PRIMARY KEY,
                WELL_UWI TEXT NULL,
                DESIGN_DATE TEXT NULL,
                PLUNGER_TYPE TEXT NOT NULL,
                DEPTH_FT REAL NOT NULL,
                TUBING_DIAMETER_IN REAL NOT NULL,
                CASING_PRESSURE_PSIA REAL NOT NULL,
                LINE_PRESSURE_PSIA REAL NOT NULL,
                GAS_RATE_MSCFD REAL NOT NULL,
                LIQUID_RATE_BBLD REAL NOT NULL,
                GLR_SCF_BBL REAL NOT NULL,
                FALL_VELOCITY_FPM REAL NOT NULL,
                RISE_VELOCITY_FPM REAL NOT NULL,
                FALL_TIME_MIN REAL NOT NULL,
                RISE_TIME_MIN REAL NOT NULL,
                SHUT_IN_TIME_MIN REAL NOT NULL,
                CYCLE_TIME_MIN REAL NOT NULL,
                CYCLES_PER_DAY REAL NOT NULL,
                RATE_PER_CYCLE_BBL REAL NOT NULL,
                DAILY_RATE_BBLD REAL NULL,
                FEASIBILITY TEXT NOT NULL,
                ECONOMIC_IND TEXT NOT NULL,
                ECONOMIC_BASIS TEXT NULL,
                CRITERIA_TEXT TEXT NULL,
                FAILED_CRITERIA TEXT NULL,
                ACTOR TEXT NULL,
                RECORDED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);
            """;
        command.ExecuteNonQuery();
    }

    private static OgPlungerInputs Inputs() =>
        new(
            "WELL-1",
            PlungerType: PlungerLiftConstants.PlungerTypeBar,
            DepthFt: 6000m,
            TubingDiameterIn: 2.375m,
            CasingPressure: 250m,
            LinePressure: 150m,
            GasRateMscfPerDay: 30m,
            LiquidRateBblPerDay: 10m,
            GasLiquidRatio: 3000m);

    private static long Scalar(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        return Convert.ToInt64(command.ExecuteScalar());
    }

    private static long CountRows(string database) => Scalar(database, "SELECT COUNT(*) FROM PLUNGER_DESIGN;");

    private static void Drop(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DROP TABLE PLUNGER_DESIGN;";
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
