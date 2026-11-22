using Beep.OilandGas.PipelineAnalysis.Constants;
using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.WellTest;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// WTA-03 against a real SQLite database: a result saves with its mode and inputs (AC-6), the erosion threshold and its
/// source are stored (AC-2), the regime is stored (AC-3), a liquid result keeps its own unit, a refused analysis writes
/// nothing, a save whose acknowledgement was lost is retained and retried, and an unavailable history is a state.
/// Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class PipelineGatewayTests
{
    private const string ConnectionName = "wta03-pipeline";

    [IntegrationFact]
    public async Task A_result_saves_with_its_mode_inputs_and_threshold_source()
    {        var root = Path.Combine(OgPaths.DataRoot, "wta03-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasWta03"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "pipeline.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "pipeline.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgPipelineService(() => source, reporter);

        // ---- the workspace ---------------------------------------------------
        var loaded = await service.LoadAsync();

        Assert.True(loaded.Success, loaded.Message);
        Assert.Empty(loaded.Value!.Results);
        Assert.False(loaded.Value.HistoryFailed);
        Assert.Equal(PipelineModes.All, loaded.Value.Modes);
        Assert.Equal(["SOLID", "INHIBITED", "CLEAN"], loaded.Value.ErosionServices);
        Assert.Contains("API RP 14E", loaded.Value.ErosionSource, StringComparison.Ordinal);
        Assert.Contains("slug flow", loaded.Value.DisabledText, StringComparison.Ordinal);

        // ---- a refused analysis writes nothing --------------------------------
        var refused = await service.AnalyseAsync(Gas() with { OutletPressure = 1000m });

        Assert.False(refused.Success);
        // The rule catches it before the engine: an outlet at the inlet pressure leaves nothing to analyse.
        Assert.Equal(PipelineRules.CodeInputInvalid, refused.Reference);
        Assert.Equal(0, CountRows(database));

        // ---- analyse and save the gas case ------------------------------------
        var analysed = await service.AnalyseAsync(Gas());

        Assert.True(analysed.Success, analysed.Message + " :: " + string.Join(" | ", reporter.Failures));

        var result = analysed.Value!.Selected!;

        Assert.Equal(PipelineModes.Gas, result.Mode);
        Assert.True(result.Capacity > 0m);
        Assert.Equal("TURBULENT", result.Regime);

        var saved = await service.SaveAsync(result, "sara");

        Assert.True(saved.Success, saved.Message);
        Assert.Equal(1, CountRows(database));

        // AC-6, AC-2 and AC-3: the mode, the inputs, the regime and the threshold's source are on the row.
        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM PIPELINE_ANALYSIS_REGISTER WHERE MODE = 'GAS' " +
            "AND CAPACITY_OUOM = 'Mscf/d' AND CAPACITY > 0 " +
            "AND FLOW_REGIME = 'TURBULENT' AND REYNOLDS_NUMBER > 0 " +
            "AND DIAMETER_IN = 12 AND LENGTH_MILES = 10 AND INLET_PRESSURE = 1000 AND OUTLET_PRESSURE = 800 " +
            "AND EROSION_THRESHOLD_FPS > 0 AND VELOCITY_RATIO > 0 " +
            "AND THRESHOLD_SOURCE LIKE 'API RP 14E (2007)%' AND EROSION_SERVICE = 'SOLID' " +
            "AND FEASIBLE_IND = 'Y' AND GAS_SPECIFIC_GRAVITY = 0.65 AND AVERAGE_TEMPERATURE_R = 530 " +
            "AND ACTOR = 'sara';"));

        // ---- the reload keeps the mode, the capacity and the source ----------
        var reloaded = await service.LoadAsync();

        Assert.True(reloaded.Success, reloaded.Message);

        var again = reloaded.Value!.Selected!;

        Assert.Equal(PipelineModes.Gas, again.Mode);
        Assert.Equal(OgPipelineResult.GasCapacityUnit, again.CapacityUnit);
        Assert.Equal("TURBULENT", again.Regime);
        Assert.Equal("SOLID", again.Inputs.ErosionService);
        Assert.NotNull(again.Erosion);
        Assert.Equal("API RP 14E (2007) 2.5 - erosional velocity Ve = C / sqrt(mixture density); " +
                     "C = 100 for solid or sand-laden service", again.Erosion!.Source);
        Assert.InRange(Math.Abs(result.Capacity - again.Capacity), 0m, 0.001m);
        Assert.Equal("sara", again.Actor);

        // ---- AC-2 and SG-08-6: a sand rate stores the model's name too -------
        var sandy = (await service.AnalyseAsync(Gas() with { SandRateBblPerDay = 5m })).Value!.Selected!;

        Assert.True((await service.SaveAsync(sandy, "sara")).Success);
        Assert.Equal(2, CountRows(database));
        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM PIPELINE_ANALYSIS_REGISTER WHERE EROSION_RATE_MPY > 0 " +
            "AND THRESHOLD_SOURCE LIKE 'API RP 14E%';"));

        // ---- a liquid result keeps its own unit ------------------------------
        var liquid = (await service.AnalyseAsync(Gas() with { Mode = PipelineModes.Liquid })).Value!.Selected!;

        Assert.Equal(OgPipelineResult.LiquidCapacityUnit, liquid.CapacityUnit);
        Assert.True((await service.SaveAsync(liquid, "sara")).Success);
        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM PIPELINE_ANALYSIS_REGISTER WHERE MODE = 'LIQUID' " +
            "AND CAPACITY_OUOM = 'bbl/d' AND LIQUID_VISCOSITY_CP = 5;"));
        Assert.Contains("screening figure", liquid.NotesText, StringComparison.Ordinal);

        // ---- the state table's "lost acknowledgement" ------------------------
        Drop(database);

        var lost = await service.SaveAsync(result, "sara");

        Assert.False(lost.Success);
        Assert.NotNull(service.Pending);
        Assert.Equal("PL-1", service.Pending!.Result.PipelineId);

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
            CREATE TABLE PIPELINE_ANALYSIS_REGISTER (
                REGISTER_ID TEXT NOT NULL PRIMARY KEY,
                MODE TEXT NOT NULL,
                WELL_UWI TEXT NULL,
                PIPELINE_ID TEXT NULL,
                ANALYSIS_DATE TEXT NULL,
                DIAMETER_IN REAL NULL,
                LENGTH_MILES REAL NULL,
                ROUGHNESS_FT REAL NULL,
                INLET_PRESSURE REAL NULL,
                OUTLET_PRESSURE REAL NULL,
                AVERAGE_TEMPERATURE_R REAL NULL,
                GAS_SPECIFIC_GRAVITY REAL NULL,
                BASE_PRESSURE REAL NULL,
                BASE_TEMPERATURE_R REAL NULL,
                LIQUID_SPECIFIC_GRAVITY REAL NULL,
                LIQUID_VISCOSITY_CP REAL NULL,
                CAPACITY REAL NOT NULL,
                CAPACITY_OUOM TEXT NOT NULL,
                FLOW_REGIME TEXT NOT NULL,
                REYNOLDS_NUMBER REAL NULL,
                FRICTION_FACTOR REAL NULL,
                PRESSURE_DROP_PSI REAL NULL,
                VELOCITY_FPS REAL NULL,
                EROSION_RATE_MPY REAL NULL,
                EROSION_THRESHOLD_FPS REAL NULL,
                VELOCITY_RATIO REAL NULL,
                THRESHOLD_SOURCE TEXT NULL,
                EROSION_SERVICE TEXT NULL,
                FEASIBLE_IND TEXT NOT NULL,
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

    private static OgPipelineInputs Gas() =>
        new(
            "LINE-1",
            "PL-1",
            PipelineModes.Gas,
            DiameterIn: 12m,
            LengthMiles: 10m,
            RoughnessFt: PipelineConstants.Roughness.CommercialSteel,
            InletPressure: 1000m,
            OutletPressure: 800m,
            AverageTemperatureR: 530m,
            GasSpecificGravity: 0.65m,
            BasePressure: 14.7m,
            BaseTemperatureR: 520m,
            LiquidSpecificGravity: 0.85m,
            LiquidViscosityCp: 5m,
            LiquidRateBblPerDay: 5000m,
            SandRateBblPerDay: null,
            ParticleDiameterMicron: 250m,
            ErosionService: PipelineStandards.DefaultServiceCode);

    private static long Scalar(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        return Convert.ToInt64(command.ExecuteScalar());
    }

    private static long CountRows(string database) =>
        Scalar(database, "SELECT COUNT(*) FROM PIPELINE_ANALYSIS_REGISTER;");

    private static void Drop(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DROP TABLE PIPELINE_ANALYSIS_REGISTER;";
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
