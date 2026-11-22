using Beep.OilandGas.ChokeAnalysis.Constants;
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
/// LFT-07 against a real SQLite database: a result saves with its correlation and regime (AC-6), its erosion threshold and
/// that threshold's source (AC-3), the fallback correlation is recorded as the one it used, a save whose acknowledgement was
/// lost is retained and retried, and an unavailable history is a state. Opt-in
/// (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class ChokeGatewayTests
{
    private const string ConnectionName = "lft07-choke";

    [IntegrationFact]
    public async Task A_result_saves_with_its_correlation_regime_and_threshold_source()
    {        var root = Path.Combine(OgPaths.DataRoot, "lft07-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasLft07"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "choke.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "choke.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgChokeService(() => source, reporter);

        // ---- the workspace ---------------------------------------------------
        var loaded = await service.LoadAsync();

        Assert.True(loaded.Success, loaded.Message);
        Assert.Empty(loaded.Value!.Results);
        Assert.False(loaded.Value.HistoryFailed);
        Assert.Contains(ChokeAnalysisReferenceCodes.CorrelationGilbert, loaded.Value.Correlations);
        Assert.Contains(nameof(ChokeType.Bean), loaded.Value.ChokeTypes);
        Assert.Contains("API RP 14E", loaded.Value.ThresholdSource, StringComparison.Ordinal);
        Assert.Contains("API RP 14E", loaded.Value.ReferenceText, StringComparison.Ordinal);

        // ---- analyse and save the gas case ----------------------------------
        var analysed = await service.AnalyseAsync(Gas());

        Assert.True(analysed.Success, analysed.Message + " :: " + string.Join(" | ", reporter.Failures));

        var result = analysed.Value!.Selected!;

        Assert.Equal(ChokeAnalysisReferenceCodes.RegimeSonic, result.Regime);
        Assert.Equal(ChokeAnalysisReferenceCodes.CorrelationGasSinglePhase, result.CorrelationUsed);
        Assert.False(result.CorrelationFellBack);

        var saved = await service.SaveAsync(result, "sara");

        Assert.True(saved.Success, saved.Message);
        Assert.Equal(1, CountRows(database));

        // AC-6 and AC-3: the correlation, the regime, the erosion figures and the threshold's source.
        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM CHOKE_FLOW_RESULT WHERE FLOW_REGIME = 'SONIC' " +
            "AND CORRELATION_CHOSEN = 'GAS_SINGLE_PHASE' AND CORRELATION_USED = 'GAS_SINGLE_PHASE' " +
            "AND CHOKE_TYPE = 'BEAN' AND CHOKE_DIAMETER_IN = 0.5 AND CHOKE_SIZE_64THS = 32 " +
            "AND FLOW_RATE > 0 AND PRESSURE_RATIO > 0 AND CRITICAL_PRESSURE_RATIO > 0 " +
            "AND EROSION_LIMIT_FPS > 0 AND THRESHOLD_SOURCE LIKE 'API RP 14E%' " +
            "AND RATE_OUOM = 'MMscf/d' AND PRESSURE_OUOM = 'psia' AND ACTOR = 'sara';"));

        // ---- the reload keeps the correlation, the regime and the erosion ---
        var reloaded = await service.LoadAsync();

        Assert.True(reloaded.Success, reloaded.Message);

        var again = reloaded.Value!.Selected!;

        Assert.Equal(ChokeAnalysisReferenceCodes.RegimeSonic, again.Regime);
        Assert.Equal(ChokeAnalysisReferenceCodes.CorrelationGasSinglePhase, again.CorrelationUsed);
        Assert.Equal(0.5m, again.ChokeDiameter);
        Assert.Equal(32m, again.ChokeSize64ths);
        Assert.Equal("API RP 14E erosional velocity, Ve = C / sqrt(mixture density)", again.Erosion!.ThresholdSource);
        Assert.InRange(Math.Abs(result.Rate - again.Rate), 0m, 0.001m);
        Assert.Equal("sara", again.Actor);

        // ---- the fallback correlation is stored as the one actually used ----
        var fallback = (await service.AnalyseAsync(Empirical() with
        {
            Correlation = ChokeAnalysisReferenceCodes.CorrelationPilehvari,
        })).Value!.Selected!;

        Assert.True(fallback.CorrelationFellBack);

        var savedFallback = await service.SaveAsync(fallback, "sara");

        Assert.True(savedFallback.Success, savedFallback.Message);
        Assert.Equal(2, CountRows(database));
        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM CHOKE_FLOW_RESULT WHERE CORRELATION_CHOSEN = 'PILEHVARI' " +
            "AND CORRELATION_USED = 'GILBERT' AND WARNING_IND = 'Y' " +
            "AND NOTES LIKE '%correlation_fallback%';"));

        // ---- a subsonic result stores its own regime ------------------------
        var subsonic = (await service.AnalyseAsync(Gas() with { DownstreamPressure = 2500m })).Value!.Selected!;

        Assert.Equal(ChokeAnalysisReferenceCodes.RegimeSubsonic, subsonic.Regime);
        Assert.True((await service.SaveAsync(subsonic, "sara")).Success);
        Assert.Equal(1, Scalar(database, "SELECT COUNT(*) FROM CHOKE_FLOW_RESULT WHERE FLOW_REGIME = 'SUBSONIC';"));

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
        Assert.Empty(unavailable.Value.Results);
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
            CREATE TABLE CHOKE_FLOW_RESULT (
                CHOKE_FLOW_RESULT_ID TEXT NOT NULL PRIMARY KEY,
                CHOKE_PROPERTIES_ID TEXT NULL,
                GAS_CHOKE_PROPERTIES_ID TEXT NULL,
                FLOW_RATE REAL NOT NULL,
                DOWNSTREAM_PRESSURE REAL NOT NULL,
                UPSTREAM_PRESSURE REAL NOT NULL,
                PRESSURE_RATIO REAL NOT NULL,
                FLOW_REGIME TEXT NOT NULL,
                CRITICAL_PRESSURE_RATIO REAL NOT NULL,
                CORRELATION_CHOSEN TEXT NULL,
                CORRELATION_USED TEXT NULL,
                CHOKE_DIAMETER_IN REAL NOT NULL,
                CHOKE_SIZE_64THS REAL NULL,
                CHOKE_TYPE TEXT NULL,
                DISCHARGE_COEFFICIENT REAL NULL,
                GAS_SPECIFIC_GRAVITY REAL NULL,
                TEMPERATURE_F REAL NULL,
                Z_FACTOR REAL NULL,
                LIQUID_RATE_BBLD REAL NULL,
                GAS_RATE_MSCFD REAL NULL,
                GLR_SCF_BBL REAL NULL,
                EROSION_VELOCITY_FPS REAL NULL,
                EROSION_LIMIT_FPS REAL NULL,
                EROSION_RATIO REAL NULL,
                EROSION_RISK TEXT NULL,
                THRESHOLD_SOURCE TEXT NULL,
                WARNING_IND TEXT NOT NULL,
                NOTES TEXT NULL,
                RATE_OUOM TEXT NULL,
                PRESSURE_OUOM TEXT NULL,
                ACTOR TEXT NULL,
                RECORDED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);
            """;
        command.ExecuteNonQuery();
    }

    private static OgChokeInputs Gas() =>
        new(
            "WELL-1",
            ChokeAnalysisReferenceCodes.CorrelationGasSinglePhase,
            ChokeType.Bean.ToString(),
            ChokeDiameterIn: 0.5m,
            DischargeCoefficient: null,
            UpstreamPressure: 3000m,
            DownstreamPressure: 1000m,
            TemperatureF: 200m,
            GasSpecificGravity: 0.65m,
            ZFactor: 0.9m,
            LiquidRate: 1000m,
            GasRateMscfPerDay: 600m,
            Glr: 600m,
            SandRate: null);

    private static OgChokeInputs Empirical() =>
        Gas() with { Correlation = ChokeAnalysisReferenceCodes.CorrelationGilbert };

    private static long Scalar(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        return Convert.ToInt64(command.ExecuteScalar());
    }

    private static long CountRows(string database) =>
        Scalar(database, "SELECT COUNT(*) FROM CHOKE_FLOW_RESULT;");

    private static void Drop(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DROP TABLE CHOKE_FLOW_RESULT;";
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
