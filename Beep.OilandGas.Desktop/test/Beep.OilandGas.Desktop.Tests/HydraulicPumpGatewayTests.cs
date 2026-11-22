using Beep.OilandGas.HydraulicPumps.Constants;
using Beep.OilandGas.HydraulicPumps.Validation;
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
/// LFT-06 against a real SQLite database: a jet design saves with its kind and every input (AC-6), its findings and
/// cavitation index are stored (AC-2, AC-4), a piston design saves as a piston, an error finding refuses both the run and
/// the save, a save whose acknowledgement was lost is retained and retried, and an unavailable history is a state.
/// Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class HydraulicPumpGatewayTests
{
    private const string ConnectionName = "lft06-hydraulic";

    [IntegrationFact]
    public async Task A_jet_design_saves_with_its_kind_every_input_and_its_findings()
    {        var root = Path.Combine(OgPaths.DataRoot, "lft06-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasLft06"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "hydraulic.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "hydraulic.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgHydraulicPumpService(() => source, reporter);

        // ---- the workspace ---------------------------------------------------
        var loaded = await service.LoadAsync();

        Assert.True(loaded.Success, loaded.Message);
        Assert.Empty(loaded.Value!.Designs);
        Assert.False(loaded.Value.HistoryFailed);
        Assert.Equal(HydraulicPumpConstants.StandardNozzleDiameters, loaded.Value.StandardNozzleDiameters);
        Assert.Equal(HydraulicPumpConstants.StandardThroatDiameters, loaded.Value.StandardThroatDiameters);
        Assert.Equal(HydraulicPumpConstants.StandardPistonDiameters, loaded.Value.StandardPistonDiameters);
        Assert.Equal(HydraulicPumpConstants.StandardStrokeLengths, loaded.Value.StandardStrokeLengths);
        Assert.Equal(HydraulicPumpFindings.DefaultRequiredNpshPsi, loaded.Value.RequiredNpshPsi);
        Assert.Contains("nozzles", loaded.Value.ReferenceText, StringComparison.Ordinal);

        // ---- AC-4: an error finding refuses the run and writes nothing -------
        var refused = await service.DesignAsync(Inputs() with { NozzleDiameter = 0.75m, ThroatDiameter = 0.5m });

        Assert.False(refused.Success);
        Assert.Equal(HydraulicRules.CodeInvalidCombination, refused.Reference);
        Assert.Contains("nozzle_not_less_than_throat", refused.Message!, StringComparison.Ordinal);
        Assert.Equal(0, CountRows(database));

        // ---- design and save a clean jet design ------------------------------
        var designed = await service.DesignAsync(Inputs());

        Assert.True(designed.Success, designed.Message + " :: " + string.Join(" | ", reporter.Failures));

        var design = designed.Value!.Selected!;

        Assert.Equal(HydraulicPumpKinds.Jet, design.Kind);
        Assert.False(design.HasBlockingFinding);
        Assert.True(design.ProductionRate > 0m);
        Assert.False(design.CavitationRisk);

        var saved = await service.SaveAsync(design, "sara");

        Assert.True(saved.Success, saved.Message);
        Assert.Equal(1, CountRows(database));

        // AC-6: the kind and every input are on the row, with the result, the index and the units.
        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM HYDRAULIC_PUMP_DESIGN WHERE KIND = 'JET' " +
            "AND WELL_DEPTH_FT = 3000 AND TUBING_DIAMETER_IN = 2.875 AND CASING_DIAMETER_IN = 7 " +
            "AND WELLHEAD_PRESSURE_PSIA = 200 AND BOTTOM_HOLE_PRESSURE_PSIA = 1000 " +
            "AND WELLHEAD_TEMPERATURE_R = 560 AND BOTTOM_HOLE_TEMPERATURE_R = 660 " +
            "AND OIL_GRAVITY_API = 35 AND WATER_CUT = 0.3 AND GAS_OIL_RATIO_SCF_STB = 300 " +
            "AND GAS_SPECIFIC_GRAVITY = 0.65 AND DESIRED_PRODUCTION_RATE_BBLD = 400 AND PUMP_DEPTH_FT = 3000 " +
            "AND NOZZLE_DIAMETER_IN = 0.375 AND THROAT_DIAMETER_IN = 0.75 AND DIFFUSER_DIAMETER_IN = 0.75 " +
            "AND POWER_FLUID_PRESSURE_PSIA = 3000 AND POWER_FLUID_RATE_BBLD = 600 " +
            "AND POWER_FLUID_SPECIFIC_GRAVITY = 1.07 " +
            "AND RATE_OUOM = 'bbl/d' AND PRESSURE_OUOM = 'psia' AND POWER_OUOM = 'hp' " +
            "AND CAVITATION_REQUIRED_PSI = " +
            HydraulicPumpFindings.DefaultRequiredNpshPsi.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            " AND ACTOR = 'sara';"));

        // ---- the reload keeps the kind, the inputs and the result -----------
        var reloaded = await service.LoadAsync("WELL-1");

        Assert.True(reloaded.Success, reloaded.Message);

        var again = reloaded.Value!.Selected!;

        Assert.Equal(HydraulicPumpKinds.Jet, again.Kind);
        Assert.Equal(3000m, again.Inputs.WellDepthFt);
        Assert.Equal(0.375m, again.Inputs.NozzleDiameter);
        Assert.Equal(0.75m, again.Inputs.ThroatDiameter);
        Assert.Equal(3000m, again.Inputs.PowerFluidPressure);
        Assert.Equal(600m, again.Inputs.PowerFluidRate);
        Assert.InRange(Math.Abs(design.ProductionRate - again.ProductionRate), 0m, 0.001m);
        Assert.InRange(Math.Abs(design.SystemEfficiency - again.SystemEfficiency), 0m, 0.000001m);
        Assert.Equal("sara", again.Actor);

        // ---- AC-3 and AC-4: a clamped design stores the clamp as a finding ---
        var clamped = (await service.DesignAsync(Inputs() with
        {
            WellDepthFt = 8000m, PowerFluidPressure = 4000m, PumpDepth = 8000m, BottomHolePressure = 2500m,
        })).Value!.Selected!;

        Assert.True(clamped.EfficiencyClamped || clamped.Findings.Any(finding => finding.Code == "JET_EFFICIENCY_CLAMPED"));

        var savedClamped = await service.SaveAsync(clamped, "sara");

        Assert.True(savedClamped.Success, savedClamped.Message);
        Assert.Equal(2, CountRows(database));
        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM HYDRAULIC_PUMP_DESIGN WHERE KIND = 'JET' " +
            "AND FINDINGS_TEXT LIKE '%jet_efficiency_clamped%' AND WORST_SEVERITY = 'WARNING';"));

        // ---- a piston design saves as a piston -------------------------------
        var piston = (await service.DesignAsync(Piston())).Value!.Selected!;

        Assert.Equal(HydraulicPumpKinds.Piston, piston.Kind);
        Assert.NotNull(piston.Displacement);

        var savedPiston = await service.SaveAsync(piston, "sara");

        Assert.True(savedPiston.Success, savedPiston.Message);
        Assert.Equal(3, CountRows(database));
        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM HYDRAULIC_PUMP_DESIGN WHERE KIND = 'PISTON' AND PISTON_DIAMETER_IN = 2 " +
            "AND ROD_DIAMETER_IN = 0.75 AND STROKE_LENGTH_IN = 36 AND STROKES_PER_MINUTE = 12 " +
            "AND PUMP_DISPLACEMENT_BBLD > 0 AND NOZZLE_DIAMETER_IN IS NULL;"));

        // ---- the state table's "lost acknowledgement" ------------------------
        Drop(database);

        var lost = await service.SaveAsync(design, "sara");

        Assert.False(lost.Success);
        Assert.NotNull(service.Pending);
        Assert.Equal("WELL-1", service.Pending!.Design.WellId);

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
            CREATE TABLE HYDRAULIC_PUMP_DESIGN (
                DESIGN_ID TEXT NOT NULL PRIMARY KEY,
                WELL_UWI TEXT NULL,
                DESIGN_DATE TEXT NULL,
                KIND TEXT NOT NULL,
                WELL_DEPTH_FT REAL NOT NULL,
                TUBING_DIAMETER_IN REAL NOT NULL,
                CASING_DIAMETER_IN REAL NOT NULL,
                WELLHEAD_PRESSURE_PSIA REAL NOT NULL,
                BOTTOM_HOLE_PRESSURE_PSIA REAL NOT NULL,
                WELLHEAD_TEMPERATURE_R REAL NOT NULL,
                BOTTOM_HOLE_TEMPERATURE_R REAL NOT NULL,
                OIL_GRAVITY_API REAL NOT NULL,
                WATER_CUT REAL NOT NULL,
                GAS_OIL_RATIO_SCF_STB REAL NOT NULL,
                GAS_SPECIFIC_GRAVITY REAL NOT NULL,
                DESIRED_PRODUCTION_RATE_BBLD REAL NOT NULL,
                PUMP_DEPTH_FT REAL NOT NULL,
                NOZZLE_DIAMETER_IN REAL NULL,
                THROAT_DIAMETER_IN REAL NULL,
                DIFFUSER_DIAMETER_IN REAL NULL,
                AREA_RATIO REAL NULL,
                PISTON_DIAMETER_IN REAL NULL,
                ROD_DIAMETER_IN REAL NULL,
                STROKE_LENGTH_IN REAL NULL,
                STROKES_PER_MINUTE REAL NULL,
                POWER_FLUID_PRESSURE_PSIA REAL NOT NULL,
                POWER_FLUID_RATE_BBLD REAL NOT NULL,
                POWER_FLUID_SPECIFIC_GRAVITY REAL NOT NULL,
                PRODUCTION_RATE_BBLD REAL NOT NULL,
                TOTAL_FLOW_RATE_BBLD REAL NOT NULL,
                PRODUCTION_RATIO REAL NULL,
                PUMP_EFFICIENCY REAL NOT NULL,
                SYSTEM_EFFICIENCY REAL NOT NULL,
                RAW_SYSTEM_EFFICIENCY REAL NULL,
                POWER_FLUID_HORSEPOWER REAL NOT NULL,
                HYDRAULIC_HORSEPOWER REAL NOT NULL,
                PUMP_INTAKE_PRESSURE_PSIA REAL NOT NULL,
                PUMP_DISCHARGE_PRESSURE_PSIA REAL NOT NULL,
                PUMP_DISPLACEMENT_BBLD REAL NULL,
                CAVITATION_AVAILABLE_PSI REAL NOT NULL,
                CAVITATION_REQUIRED_PSI REAL NOT NULL,
                CAVITATION_MARGIN_PSI REAL NOT NULL,
                CAVITATION_RISK_IND TEXT NOT NULL,
                FINDINGS_TEXT TEXT NULL,
                WORST_SEVERITY TEXT NOT NULL,
                RATE_OUOM TEXT NULL,
                PRESSURE_OUOM TEXT NULL,
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

    private static OgHydraulicInputs Inputs() =>
        new(
            "WELL-1",
            HydraulicPumpKinds.Jet,
            WellDepthFt: 3000m,
            TubingDiameterIn: 2.875m,
            CasingDiameterIn: 7m,
            WellheadPressure: 200m,
            BottomHolePressure: 1000m,
            WellheadTemperature: 560m,
            BottomHoleTemperature: 660m,
            OilGravity: 35m,
            WaterCut: 0.3m,
            GasOilRatio: 300m,
            GasSpecificGravity: 0.65m,
            DesiredRate: 400m,
            PumpDepth: 3000m,
            NozzleDiameter: 0.375m,
            ThroatDiameter: 0.75m,
            DiffuserDiameter: 0.75m,
            PistonDiameter: 2.0m,
            RodDiameter: 0.75m,
            StrokeLength: 36m,
            StrokesPerMinute: 12m,
            PowerFluidPressure: 3000m,
            PowerFluidRate: 600m,
            PowerFluidSpecificGravity: 1.07m);

    private static OgHydraulicInputs Piston() =>
        Inputs() with
        {
            Kind = HydraulicPumpKinds.Piston,
            NozzleDiameter = null,
            ThroatDiameter = null,
            DiffuserDiameter = null,
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
        Scalar(database, "SELECT COUNT(*) FROM HYDRAULIC_PUMP_DESIGN;");

    private static void Drop(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DROP TABLE HYDRAULIC_PUMP_DESIGN;";
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
