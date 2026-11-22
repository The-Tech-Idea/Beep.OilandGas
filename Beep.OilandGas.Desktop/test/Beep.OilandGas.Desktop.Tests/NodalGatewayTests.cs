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
/// LFT-01 against a real SQLite database: a study saves with both correlations, its inputs, its curves and the actor
/// (AC-5), a saved study reloads with the same operating point and curves, a no-intersection study is stored and read back
/// as no intersection rather than as a zero (AC-2), and an unavailable history is a state.
/// Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class NodalGatewayTests
{
    private const string ConnectionName = "lft01-nodal";

    [IntegrationFact]
    public async Task A_study_saves_with_both_correlations_its_curves_and_its_cross_check()
    {        var root = Path.Combine(OgPaths.DataRoot, "lft01-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasLft01"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "nodal.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "nodal.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgNodalService(() => source, reporter);

        // ---- the workspace ---------------------------------------------------
        var loaded = await service.LoadAsync();

        Assert.True(loaded.Success, loaded.Message);
        Assert.Empty(loaded.Value!.Studies);
        Assert.False(loaded.Value.HistoryFailed);
        Assert.Contains(NodalIprModels.Vogel, loaded.Value.IprModels);
        Assert.Equal([NodalVlpCorrelations.HagedornBrown], loaded.Value.VlpCorrelations);

        // ---- run and save the reference case ---------------------------------
        var run = await service.RunAsync(Inputs());

        Assert.True(run.Success, run.Message + " :: " + string.Join(" | ", reporter.Failures));

        var study = run.Value!.Selected!;

        Assert.True(study.HasOperatingPoint, study.Diagnostics);

        var saved = await service.SaveAsync(study, "sara");

        Assert.True(saved.Success, saved.Message);
        Assert.Equal(1, CountRows(database, "NODAL_ANALYSIS_RESULT"));
        Assert.Equal(study.IprCurve.Count, CountRows(database, "NODAL_IPR_POINT"));
        Assert.Equal(study.VlpCurve.Count, CountRows(database, "NODAL_VLP_POINT"));

        // AC-5: both correlations, the inputs and the actor are on the row.
        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM NODAL_ANALYSIS_RESULT WHERE IPR_MODEL = 'VOGEL' " +
            "AND VLP_CORRELATION = 'HAGEDORN_BROWN' AND ACTOR = 'sara' AND CONVERGED = 'Y' " +
            "AND INPUTS_JSON IS NOT NULL AND CROSS_CHECK_AGREES = 'Y';"));

        // ---- reload: the same operating point and the same curves ------------
        var reloaded = await service.LoadAsync("WELL-1");

        Assert.True(reloaded.Success, reloaded.Message);

        var again = reloaded.Value!.Selected!;

        Assert.Equal(study.IprModel, again.IprModel);
        Assert.Equal(study.VlpCorrelation, again.VlpCorrelation);
        Assert.True(again.HasOperatingPoint);
        Assert.Equal(study.FlowRate, again.FlowRate);
        Assert.Equal(study.FlowingPressure, again.FlowingPressure);
        Assert.Equal(study.IprCurve.Count, again.IprCurve.Count);
        Assert.Equal(study.VlpCurve.Count, again.VlpCurve.Count);
        Assert.Equal(study.IprCurve[0].Rate, again.IprCurve[0].Rate);
        Assert.Equal(study.IprCurve[^1].Pressure, again.IprCurve[^1].Pressure);
        Assert.Equal(study.VlpCurve[^1].Pressure, again.VlpCurve[^1].Pressure);
        Assert.Equal("sara", again.Actor);

        // ---- AC-2: a no-intersection study keeps saying so ------------------
        var dead = (await service.RunAsync(DeadInputs())).Value!.Selected!;

        Assert.False(dead.HasOperatingPoint);

        var savedDead = await service.SaveAsync(dead, "sara");

        Assert.True(savedDead.Success, savedDead.Message);
        Assert.Equal(2, CountRows(database, "NODAL_ANALYSIS_RESULT"));
        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM NODAL_ANALYSIS_RESULT WHERE CONVERGED = 'N' " +
            "AND OPERATING_FLOW_RATE IS NULL AND DIAGNOSTICS LIKE 'no intersection%';"));

        var deadReloaded = (await service.LoadAsync("WELL-1"))
            .Value!.Studies.Single(item => !item.Converged);

        Assert.False(deadReloaded.HasOperatingPoint);
        Assert.Null(deadReloaded.FlowRate);
        Assert.Equal("no intersection", deadReloaded.StatusText);
        Assert.Contains("no intersection", deadReloaded.Diagnostics, StringComparison.Ordinal);

        // ---- an unavailable history is a state -------------------------------
        Drop(database, "NODAL_ANALYSIS_RESULT");

        var unavailable = await service.LoadAsync();

        Assert.True(unavailable.Success, unavailable.Message);
        Assert.True(unavailable.Value!.HistoryFailed);
        Assert.Empty(unavailable.Value.Studies);
        Assert.Contains("could not be read", unavailable.Value.HistoryStateText, StringComparison.Ordinal);
    }

    private static void Seed(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE NODAL_ANALYSIS_RESULT (
                NODAL_ANALYSIS_RESULT_ID TEXT NOT NULL PRIMARY KEY,
                ANALYSIS_ID TEXT NOT NULL,
                WELL_UWI TEXT NULL,
                ANALYSIS_DATE TEXT NULL,
                OPERATING_FLOW_RATE REAL NULL,
                OPERATING_PRESSURE REAL NULL,
                STATUS TEXT NOT NULL,
                IPR_MODEL TEXT NULL,
                VLP_CORRELATION TEXT NULL,
                CONVERGED TEXT NOT NULL,
                ITERATIONS INTEGER NULL,
                BINDING_CONSTRAINT TEXT NULL,
                DIAGNOSTICS TEXT NULL,
                INPUTS_JSON TEXT NULL,
                RATE_OUOM TEXT NULL,
                CROSS_CHECK_SOURCE TEXT NULL,
                CROSS_CHECK_FLOW_RATE REAL NULL,
                CROSS_CHECK_PRESSURE REAL NULL,
                CROSS_CHECK_AGREES TEXT NULL,
                CROSS_CHECK_TEXT TEXT NULL,
                ACTOR TEXT NULL,
                RECORDED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE NODAL_IPR_POINT (
                NODAL_IPR_POINT_ID TEXT NOT NULL PRIMARY KEY,
                NODAL_ANALYSIS_ID TEXT NOT NULL,
                FLOW_RATE REAL NOT NULL,
                FLOWING_BOTTOMHOLE_PRESSURE REAL NOT NULL,
                PRESSURE REAL NOT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE NODAL_VLP_POINT (
                NODAL_VLP_POINT_ID TEXT NOT NULL PRIMARY KEY,
                NODAL_ANALYSIS_ID TEXT NOT NULL,
                FLOW_RATE REAL NOT NULL,
                REQUIRED_BOTTOMHOLE_PRESSURE REAL NOT NULL,
                PRESSURE REAL NOT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);
            """;
        command.ExecuteNonQuery();
    }

    private static OgNodalInputs Inputs() =>
        new(
            "WELL-1", 4500m, 3600m, 180m, 1.5m, 0.2m, 600m, 35m, 1.3m, 1.5m,
            6000m, 3.958m, 200m, 0.65m, 0.0006m,
            NodalIprModels.Vogel, NodalVlpCorrelations.HagedornBrown, 20);

    private static OgNodalInputs DeadInputs() =>
        Inputs() with { ReservoirPressure = 3000m, BubblePointPressure = 2400m, WellheadPressure = 2500m };

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
