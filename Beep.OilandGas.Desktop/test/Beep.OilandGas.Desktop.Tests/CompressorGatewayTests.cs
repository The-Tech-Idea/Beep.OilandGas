using Beep.OilandGas.CompressorAnalysis.Data;
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
/// LFT-08 against a real SQLite database: a result saves with its kind, staging and efficiency (AC-6), an infeasible
/// machine stores its reasons and no achieved power (AC-3), a save whose acknowledgement was lost is retained and
/// retried, and an unavailable history is a state. Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class CompressorGatewayTests
{
    private const string ConnectionName = "lft08-compressor";

    [IntegrationFact]
    public async Task A_result_saves_with_its_kind_staging_and_efficiency()
    {        var root = Path.Combine(OgPaths.DataRoot, "lft08-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasLft08"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "compressor.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "compressor.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgCompressorService(() => source, reporter);

        // ---- the workspace ---------------------------------------------------
        var loaded = await service.LoadAsync();

        Assert.True(loaded.Success, loaded.Message);
        Assert.Empty(loaded.Value!.Results);
        Assert.False(loaded.Value.HistoryFailed);
        Assert.Equal(CompressorStagingLimits.MaximumDischargeTemperatureR, loaded.Value.InterstageLimitR);
        Assert.Contains("CENTRIFUGAL", loaded.Value.Kinds);
        Assert.Contains("interstage limit", loaded.Value.ReferenceText, StringComparison.Ordinal);
        Assert.Contains("disabled", loaded.Value.KindNote, StringComparison.Ordinal);

        // ---- analyse and save a feasible centrifugal duty --------------------
        var analysed = await service.AnalyseAsync(Centrifugal());

        Assert.True(analysed.Success, analysed.Message + " :: " + string.Join(" | ", reporter.Failures));

        var result = analysed.Value!.Selected!;

        Assert.True(result.Feasible, result.ReasonsText);
        Assert.Equal(3, result.Stages);
        Assert.NotNull(result.Power);

        var saved = await service.SaveAsync(result, "sara");

        Assert.True(saved.Success, saved.Message);
        Assert.Equal(1, CountRows(database));

        // AC-6: the kind, the staging, the efficiency and the units are on the row.
        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM COMPRESSOR_POWER_RESULT WHERE KIND = 'CENTRIFUGAL' AND STAGES = 3 " +
            "AND STAGE_RATIO > 1 AND MAX_STAGE_TEMPERATURE_R > 0 " +
            "AND STAGE_TEMPERATURE_LIMIT_R = " +
            CompressorStagingLimits.MaximumDischargeTemperatureR.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            " AND STAGE_RATIO_LIMIT = " +
            CompressorStagingLimits.MaximumRatioPerStage.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            " AND FEASIBLE_IND = 'Y' AND POWER_ACHIEVED_IND = 'Y' AND OVERALL_EFFICIENCY > 0 " +
            "AND COMPRESSION_RATIO = 3 AND POLYTROPIC_HEAD > 0 AND MOTOR_HORSEPOWER > 0 " +
            "AND HEAD_OUOM = 'ft-lbf/lbm' AND POWER_OUOM = 'hp' AND REASONS IS NULL AND ACTOR = 'sara';"));

        // ---- the reload keeps the kind, the staging and the power -----------
        var reloaded = await service.LoadAsync();

        Assert.True(reloaded.Success, reloaded.Message);

        var again = reloaded.Value!.Selected!;

        Assert.Equal(CompressorKinds.Centrifugal, again.Kind);
        Assert.Equal(3, again.Stages);
        Assert.True(again.Feasible);
        Assert.InRange(Math.Abs(result.Power!.Value - again.Power!.Value), 0m, 0.01m);
        Assert.InRange(Math.Abs(result.Head!.Value - again.Head!.Value), 0m, 0.01m);
        Assert.Equal("sara", again.Actor);
        Assert.Empty(again.Reasons);

        // ---- AC-3: an infeasible machine stores its reasons and no power ----
        var infeasible = (await service.AnalyseAsync(Centrifugal() with
        {
            DischargePressure = 3600m, Stages = 1,
        })).Value!.Selected!;

        Assert.False(infeasible.Feasible);
        Assert.Null(infeasible.Power);

        var savedInfeasible = await service.SaveAsync(infeasible, "sara");

        Assert.True(savedInfeasible.Success, savedInfeasible.Message);
        Assert.Equal(2, CountRows(database));
        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM COMPRESSOR_POWER_RESULT WHERE FEASIBLE_IND = 'N' " +
            "AND POWER_ACHIEVED_IND = 'N' AND MOTOR_HORSEPOWER = 0 " +
            "AND REASONS LIKE '%above the interstage limit%' AND REASONS LIKE '%per-stage ratio%';"));

        // ---- a reciprocating machine stores its own kind --------------------
        var reciprocating = (await service.AnalyseAsync(Reciprocating())).Value!.Selected!;

        Assert.Equal(CompressorKinds.Reciprocating, reciprocating.Kind);
        Assert.True(reciprocating.Feasible, reciprocating.ReasonsText);

        Assert.True((await service.SaveAsync(reciprocating, "sara")).Success);
        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM COMPRESSOR_POWER_RESULT WHERE KIND = 'RECIPROCATING' " +
            "AND STAGES IS NULL AND STAGE_TEMPERATURE_LIMIT_R IS NULL AND MOTOR_HORSEPOWER > 0;"));

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
            CREATE TABLE COMPRESSOR_POWER_RESULT (
                COMPRESSOR_POWER_RESULT_ID TEXT NOT NULL PRIMARY KEY,
                COMPRESSOR_OPERATING_CONDITIONS_ID TEXT NULL,
                THEORETICAL_POWER REAL NOT NULL,
                BRAKE_HORSEPOWER REAL NOT NULL,
                MOTOR_HORSEPOWER REAL NOT NULL,
                POWER_CONSUMPTION_KW REAL NOT NULL,
                COMPRESSION_RATIO REAL NOT NULL,
                POLYTROPIC_HEAD REAL NOT NULL,
                ADIABATIC_HEAD REAL NOT NULL,
                DISCHARGE_TEMPERATURE REAL NOT NULL,
                OVERALL_EFFICIENCY REAL NOT NULL,
                POLYTROPIC_EFFICIENCY REAL NOT NULL,
                ADIABATIC_EFFICIENCY REAL NOT NULL,
                CYLINDER_DISPLACEMENT REAL NOT NULL,
                VOLUMETRIC_EFFICIENCY REAL NOT NULL,
                KIND TEXT NULL,
                STAGES INTEGER NULL,
                STAGE_RATIO REAL NULL,
                MAX_STAGE_TEMPERATURE_R REAL NULL,
                STAGE_TEMPERATURE_LIMIT_R REAL NULL,
                STAGE_RATIO_LIMIT REAL NULL,
                FEASIBLE_IND TEXT NOT NULL,
                REASONS TEXT NULL,
                POWER_ACHIEVED_IND TEXT NOT NULL,
                INLET_PRESSURE_PSIA REAL NULL,
                OUTLET_PRESSURE_PSIA REAL NULL,
                INLET_TEMPERATURE_R REAL NULL,
                GAS_FLOW_MSCFD REAL NULL,
                GAS_SPECIFIC_GRAVITY REAL NULL,
                SURGE_MARGIN REAL NULL,
                HEAD_OUOM TEXT NULL,
                POWER_OUOM TEXT NULL,
                ACTOR TEXT NULL,
                RECORDED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);
            """;
        command.ExecuteNonQuery();
    }

    private static OgCompressorInputs Centrifugal() =>
        new(
            "WELL-1",
            CompressorKinds.Centrifugal,
            SuctionPressure: 300m,
            DischargePressure: 900m,
            SuctionTemperatureF: 100m,
            DischargeTemperatureF: 120m,
            GasFlowMscfPerDay: 5000m,
            GasSpecificGravity: 0.65m,
            PolytropicEfficiency: 0.75m,
            SpecificHeatRatio: 1.3m,
            MechanicalEfficiency: 0.95m,
            Stages: null,
            Speed: 10000m,
            CylinderDiameter: null,
            StrokeLength: null,
            RotationalSpeed: null,
            Cylinders: null,
            VolumetricEfficiency: null,
            ClearanceFactor: null);

    private static OgCompressorInputs Reciprocating() =>
        Centrifugal() with
        {
            Kind = CompressorKinds.Reciprocating,
            CylinderDiameter = 10m,
            StrokeLength = 12m,
            RotationalSpeed = 900m,
            Cylinders = 2,
            VolumetricEfficiency = 0.85m,
            ClearanceFactor = 0.05m,
        };

    private static long Scalar(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        return Convert.ToInt64(command.ExecuteScalar());
    }

    private static long CountRows(string database) =>
        Scalar(database, "SELECT COUNT(*) FROM COMPRESSOR_POWER_RESULT;");

    private static void Drop(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DROP TABLE COMPRESSOR_POWER_RESULT;";
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
