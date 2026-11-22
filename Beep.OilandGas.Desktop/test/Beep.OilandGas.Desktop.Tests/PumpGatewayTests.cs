using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Lift;
using Beep.OilandGas.PumpPerformance.Constants;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// LFT-03 against a real SQLite database: a duty point saves with the curve source and the stage count (AC-6), the units
/// and the module's NPSH minimum (AC-2, AC-5), an affinity result is stored with its extrapolation flag (AC-3), a duty rate
/// outside the published curve is refused and writes nothing (AC-1, SG-07-4), and a save whose acknowledgement was lost is
/// retained and retried. Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class PumpGatewayTests
{
    private const string ConnectionName = "lft03-pump";

    [IntegrationFact]
    public async Task A_duty_point_saves_with_its_curve_source_stages_and_npsh_margin()
    {        var root = Path.Combine(OgPaths.DataRoot, "lft03-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasLft03"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "pump.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "pump.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgPumpService(() => source, reporter);

        // ---- the workspace ---------------------------------------------------
        var loaded = await service.LoadAsync();

        Assert.True(loaded.Success, loaded.Message);
        Assert.Empty(loaded.Value!.Designs);
        Assert.False(loaded.Value.HistoryFailed);
        Assert.Contains(PumpTypes.Esp, loaded.Value.PumpTypes);
        Assert.Contains("no pump-curve catalog", loaded.Value.CurveSourceNote, StringComparison.Ordinal);

        // ---- AC-1: outside the curve is refused and nothing is written -------
        var outside = await service.AnalyseAsync(Inputs() with { DutyRate = 2000m });

        Assert.False(outside.Success);
        Assert.Equal(PumpRules.CodeDutyOutsideCurve, outside.Reference);
        Assert.Null(outside.Value);
        Assert.Equal(0, CountRows(database));

        // ---- analyse and save ------------------------------------------------
        var analysed = await service.AnalyseAsync(Inputs());

        Assert.True(analysed.Success, analysed.Message + " :: " + string.Join(" | ", reporter.Failures));

        var result = analysed.Value!.Selected!;

        Assert.Equal(750m, result.Rate);
        Assert.Equal(100m, result.Head);
        Assert.True(result.Extrapolated);

        var saved = await service.SaveAsync(result, "sara");

        Assert.True(saved.Success, saved.Message);
        Assert.Equal(1, CountRows(database));

        // AC-6, AC-2, AC-5 and AC-3 are all on the stored row.
        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM PUMP_DESIGN WHERE CURVE_SOURCE = 'ENTERED' AND STAGES = 3 " +
            "AND RATE_OUOM = 'GPM' AND HEAD_OUOM = 'ft' AND POWER_OUOM = 'hp' " +
            "AND NPSH_MINIMUM_MARGIN = " +
            PumpConstants.DefaultNpshSafetyMarginFeet.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            " AND OPERATING_POINT_VALID = 'Y' AND ACTOR = 'sara' AND CURVE_POINTS LIKE '%,%';"));

        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM PUMP_DESIGN WHERE EXTRAPOLATED_IND = 'Y' AND AFFINITY_TEXT LIKE '%extrapolated%';"));

        // ---- the reload keeps the duty point and the curve -------------------
        var reloaded = await service.LoadAsync("WELL-1");

        Assert.True(reloaded.Success, reloaded.Message);

        var again = reloaded.Value!.Selected!;

        Assert.Equal(750m, again.Rate);
        Assert.Equal(100m, again.Head);
        Assert.Equal(300m, again.TotalHead);
        Assert.Equal(3, again.Stages);
        Assert.Equal(4, again.Curve.Count);
        Assert.Equal(result.NpshAvailable, again.NpshAvailable);
        Assert.Equal(result.NpshMinimumMargin, again.NpshMinimumMargin);
        Assert.True(again.Extrapolated);
        Assert.Equal("sara", again.Actor);

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

    private static void Seed(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE PUMP_DESIGN (
                DESIGN_ID TEXT NOT NULL PRIMARY KEY,
                WELL_UWI TEXT NULL,
                DESIGN_DATE TEXT NULL,
                PUMP_TYPE TEXT NOT NULL,
                CURVE_SOURCE TEXT NOT NULL,
                CURVE_POINTS TEXT NULL,
                STAGES INTEGER NOT NULL,
                DUTY_RATE REAL NOT NULL,
                DUTY_HEAD REAL NOT NULL,
                TOTAL_HEAD REAL NOT NULL,
                EFFICIENCY REAL NOT NULL,
                BRAKE_HORSEPOWER REAL NOT NULL,
                POWER_KW REAL NOT NULL,
                NPSH_AVAILABLE REAL NOT NULL,
                NPSH_REQUIRED REAL NOT NULL,
                NPSH_MARGIN REAL NOT NULL,
                NPSH_MINIMUM_MARGIN REAL NOT NULL,
                NPSH_WARNING_IND TEXT NOT NULL,
                OPERATING_POINT_VALID TEXT NOT NULL,
                RATE_OUOM TEXT NULL,
                HEAD_OUOM TEXT NULL,
                POWER_OUOM TEXT NULL,
                AFFINITY_TEXT TEXT NULL,
                EXTRAPOLATED_IND TEXT NOT NULL,
                NOTES TEXT NULL,
                ACTOR TEXT NULL,
                RECORDED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);
            """;
        command.ExecuteNonQuery();
    }

    private static void CreateTable(string database) => Seed(database);

    private static OgPumpInputs Inputs() =>
        new(
            "WELL-1",
            PumpTypes.Esp,
            [
                new OgPumpCurvePoint(0m, 120m, 20m),
                new OgPumpCurvePoint(500m, 110m, 30m),
                new OgPumpCurvePoint(1000m, 90m, 40m),
                new OgPumpCurvePoint(1500m, 60m, 50m),
            ],
            Stages: 3,
            DutyRate: 750m,
            SpecificGravity: 1.0m,
            SpeedRpm: 3500m,
            NewSpeedRpm: 8000m,
            AtmosphericPressure: 14.696m,
            VaporPressure: 0.256m,
            SuctionHead: 20m,
            SuctionFriction: 2m);

    private static long Scalar(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        return Convert.ToInt64(command.ExecuteScalar());
    }

    private static long CountRows(string database) => Scalar(database, "SELECT COUNT(*) FROM PUMP_DESIGN;");

    private static void Drop(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DROP TABLE PUMP_DESIGN;";
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
