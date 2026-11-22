using System.Globalization;
using Beep.OilandGas.HydraulicPumps.Calculations;
using Beep.OilandGas.HydraulicPumps.Constants;
using Beep.OilandGas.HydraulicPumps.Exceptions;
using Beep.OilandGas.HydraulicPumps.Validation;
using Beep.OilandGas.Models.Data.HydraulicPumps;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Lift;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// LFT-06: the reference jet efficiency, the cavitation index, the clamped efficiency as a finding, the findings with their
/// severity, the standard sizes and the inputs a saved design keeps - the parts that do not need a database. Saving and
/// reloading are proven in the gateway test.
/// </summary>
public sealed class HydraulicPumpTests
{
    private const string PageFileName = "HydraulicPumpPageView.cs";

    [Fact]
    public void A_reference_jet_design_reproduces_the_modules_own_efficiency()
    {
        // AC-1: the expected efficiency is the module's M x N, computed from the design's own reported numbers and compared
        // with the owner's own method.
        var design = OgHydraulicPumpService.Build(Clean());

        Assert.Equal(HydraulicPumpKinds.Jet, design.Kind);
        Assert.True(design.ProductionRate > 0m);
        Assert.True(design.DischargePressure > design.IntakePressure);

        var productionRatio = design.ProductionRate / design.Inputs.PowerFluidRate!.Value;
        var headRatio = (design.DischargePressure - design.IntakePressure) /
                        (design.Inputs.PowerFluidPressure!.Value - design.DischargePressure);
        var expected = productionRatio * headRatio;

        var found = HydraulicPumpCalculator.TryCalculateJetPumpPerformance(
            design.ProductionRate,
            design.Inputs.PowerFluidRate.Value,
            design.IntakePressure,
            design.DischargePressure,
            design.Inputs.PowerFluidPressure.Value,
            out var raw,
            out var clamped,
            out var areaRatio);

        Assert.True(found);
        Assert.InRange(Math.Abs(raw - expected), 0m, 0.000001m);
        Assert.InRange(raw, 0m, 1m);
        Assert.Equal(raw, clamped);
        Assert.Equal(design.ProductionRatio!.Value, productionRatio, 6);
        Assert.InRange(areaRatio, 0m, 1m);

        // The design carries the ratio the module computed, and the system efficiency is the engine's own ratio.
        Assert.NotNull(design.RawSystemEfficiency);
        Assert.False(design.EfficiencyClamped);
        Assert.Equal(design.RawSystemEfficiency, design.SystemEfficiency);
        Assert.InRange(design.SystemEfficiency, 0m, 1m);
        Assert.Contains("inside 0 to 1", design.ClampText, StringComparison.Ordinal);
    }

    [Fact]
    public void Cavitation_is_reported_with_its_index_and_threshold()
    {
        // AC-2: a bottom hole pressure under the module's required NPSH.
        var cavitating = OgHydraulicPumpService.Build(Inputs() with
        {
            WellheadPressure = 10m, BottomHolePressure = 40m, WellDepthFt = 2000m,
        });

        Assert.True(cavitating.CavitationRisk);
        Assert.Equal(HydraulicPumpFindings.DefaultRequiredNpshPsi, cavitating.CavitationRequired);
        Assert.Equal(cavitating.CavitationAvailable - cavitating.CavitationRequired, cavitating.CavitationMargin);
        Assert.True(cavitating.CavitationMargin < 0m);

        Assert.Contains("39.74", cavitating.CavitationText, StringComparison.Ordinal);
        Assert.Contains("50", cavitating.CavitationText, StringComparison.Ordinal);
        Assert.Contains("cavitation predicted", cavitating.CavitationText, StringComparison.Ordinal);

        var finding = Assert.Single(cavitating.Findings, item => item.Code == "CAVITATION_PREDICTED");

        Assert.Equal("Warning", finding.Severity);
        Assert.Contains("available NPSH", finding.Message, StringComparison.Ordinal);
        Assert.Contains("the pump needs", finding.Message, StringComparison.Ordinal);

        // The index itself is the owner's, and it agrees with its own boolean check.
        var index = HydraulicPumpFindings.Cavitation(40m, HydraulicPumpFindings.VapourPressureAt60FPsi);

        Assert.Equal(39.744m, index.AvailablePsi, 3);
        Assert.Equal(50m, index.RequiredPsi);
        Assert.True(index.Risk);
        Assert.True(HydraulicPumpCalculator.CheckCavitation(40m, HydraulicPumpFindings.VapourPressureAt60FPsi, 50m));

        // A healthy well reports no cavitation and no such finding.
        var healthy = OgHydraulicPumpService.Build(Clean());

        Assert.False(healthy.CavitationRisk);
        Assert.DoesNotContain(healthy.Findings, item => item.Code == "CAVITATION_PREDICTED");
        Assert.Contains("no cavitation", healthy.CavitationText, StringComparison.Ordinal);
    }

    [Fact]
    public void A_clamped_efficiency_is_a_finding_not_a_silent_answer()
    {
        // AC-3: this design's M x N is above 1, so the engine clamps it - and both numbers are said out loud.
        var design = OgHydraulicPumpService.Build(Clamped());

        var finding = Assert.Single(design.Findings, item => item.Code == "JET_EFFICIENCY_CLAMPED");

        Assert.Equal("Warning", finding.Severity);
        Assert.Contains("clamps it to", finding.Message, StringComparison.Ordinal);
        Assert.Contains("1", finding.Message, StringComparison.Ordinal);

        // The engine's own result efficiency is inside [0, 1] whatever the raw ratio was.
        Assert.InRange(design.SystemEfficiency, 0m, 1m);

        // A design whose ratio is inside the range carries no clamp finding.
        Assert.DoesNotContain(
            OgHydraulicPumpService.Build(Clean()).Findings,
            item => item.Code == "JET_EFFICIENCY_CLAMPED");
    }

    [Fact]
    public void Findings_are_listed_with_their_severity_and_an_error_refuses_the_run()
    {
        // AC-4, and the state table's "invalid combination of nozzle/throat is refused by the validator".
        var refused = Service().DesignAsync(Inputs() with
        {
            NozzleDiameter = 0.75m, ThroatDiameter = 0.5m,
        }).GetAwaiter().GetResult();

        Assert.False(refused.Success);
        Assert.Equal(HydraulicRules.CodeInvalidCombination, refused.Reference);
        Assert.Contains("error", refused.Message!, StringComparison.Ordinal);
        Assert.Contains("nozzle_not_less_than_throat", refused.Message, StringComparison.Ordinal);

        // The finding's sentence is the validator's own, which is what a screen must not paraphrase.
        var well = Well();
        var jet = JetProperties(nozzle: 0.75m, throat: 0.5m);
        var result = HydraulicJetPumpCalculator.CalculatePerformance(well, jet);

        var finding = Assert.Single(
            HydraulicPumpFindings.EvaluateJet(well, jet, result),
            item => item.Code == "NOZZLE_NOT_LESS_THAN_THROAT");

        Assert.Equal(HydraulicPumpFindingSeverity.Error, finding.Severity);

        var thrown = Assert.Throws<InvalidPumpPropertiesException>(() => HydraulicPumpValidator.ValidateJetPumpProperties(jet));

        Assert.Equal(thrown.Message, finding.Message);

        // A warning is a warning: the narrow nozzle is under the module's efficiency minimum but nothing is refused.
        var lowEfficiency = OgHydraulicPumpService.Build(Inputs() with
        {
            WellDepthFt = 2000m, PowerFluidPressure = 2000m, PowerFluidRate = 1200m,
            BottomHolePressure = 800m, NozzleDiameter = 0.25m, ThroatDiameter = 0.75m,
        });

        var warning = Assert.Single(lowEfficiency.Findings, item => item.Code == "PUMP_EFFICIENCY_LOW");

        Assert.Equal("Warning", warning.Severity);
        Assert.False(warning.Blocking);
        Assert.Contains("below the module's", warning.Message, StringComparison.Ordinal);
        Assert.False(lowEfficiency.HasBlockingFinding);
        Assert.Equal("Warning", lowEfficiency.WorstSeverity);

        // Every finding states its severity in the text the row stores.
        Assert.Contains("warning ·", lowEfficiency.FindingsText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task The_nozzle_and_throat_come_from_the_standard_sets()
    {
        // AC-5.
        var service = Service();

        var nozzle = await service.DesignAsync(Clean() with { NozzleDiameter = 0.3m });

        Assert.False(nozzle.Success);
        Assert.Equal(HydraulicRules.CodeNozzleNotStandard, nozzle.Reference);
        Assert.Contains("0.125", nozzle.Message!, StringComparison.Ordinal);

        var throat = await service.DesignAsync(Clean() with { ThroatDiameter = 0.7m });

        Assert.False(throat.Success);
        Assert.Equal(HydraulicRules.CodeThroatNotStandard, throat.Reference);

        var piston = await service.DesignAsync(Piston() with { PistonDiameter = 2.1m });

        Assert.False(piston.Success);
        Assert.Equal(HydraulicRules.CodePistonNotStandard, piston.Reference);

        var stroke = await service.DesignAsync(Piston() with { StrokeLength = 20m });

        Assert.False(stroke.Success);
        Assert.Equal(HydraulicRules.CodeStrokeNotStandard, stroke.Reference);

        // The sizes the workspace offers are the module's own lists, and the required NPSH is the owner's.
        Assert.Contains(0.375m, HydraulicPumpConstants.StandardNozzleDiameters);
        Assert.Contains(0.75m, HydraulicPumpConstants.StandardThroatDiameters);
        Assert.Equal(HydraulicPumpFindings.DefaultRequiredNpshPsi, HydraulicRules.RequiredNpshPsi);

        // A viewer guard: the sizes are bound from the workspace, not written in the page.
        var page = File.ReadAllText(SourceRoot.AllSources().Single(file => Path.GetFileName(file) == PageFileName));

        Assert.Contains("StandardNozzleDiameters", page, StringComparison.Ordinal);
        Assert.DoesNotContain("0.375m", page, StringComparison.Ordinal);
    }

    [Fact]
    public void A_piston_design_uses_the_piston_calculator()
    {
        var design = OgHydraulicPumpService.Build(Piston());

        Assert.Equal(HydraulicPumpKinds.Piston, design.Kind);
        Assert.NotNull(design.Displacement);
        Assert.True(design.Displacement > 0m);
        Assert.True(design.ProductionRate > 0m);
        Assert.Null(design.ProductionRatio);
        Assert.Null(design.AreaRatio);
        Assert.Contains("piston", design.PistonText, StringComparison.Ordinal);
        Assert.Contains("displacement", design.PistonText, StringComparison.Ordinal);
        Assert.Equal("not a jet pump", design.JetText);
        Assert.InRange(design.PumpEfficiency, 0m, 1m);
        Assert.InRange(design.SystemEfficiency, 0m, 1m);
    }

    [Fact]
    public void The_designs_own_numbers_are_reproducible()
    {
        var first = OgHydraulicPumpService.Build(Clean());
        var second = OgHydraulicPumpService.Build(Clean());

        Assert.Equal(first.Describe(), second.Describe());
        Assert.Equal(first.ProductionRate, second.ProductionRate);
        Assert.Equal(first.SystemEfficiency, second.SystemEfficiency);
        Assert.Equal(first.FindingsText, second.FindingsText);
    }

    [Fact]
    public void The_units_are_stated_on_the_numbers()
    {
        var design = OgHydraulicPumpService.Build(Clean());

        Assert.Equal("bbl/d", OgHydraulicDesign.RateUnit);
        Assert.Equal("psia", OgHydraulicDesign.PressureUnit);
        Assert.Equal("hp", OgHydraulicDesign.PowerUnit);

        Assert.Contains("bbl/d", design.ProductionText, StringComparison.Ordinal);
        Assert.Contains("psia", design.CavitationText, StringComparison.Ordinal);
        Assert.Contains("psia", design.PowerFluidText, StringComparison.Ordinal);
        Assert.Contains("%", design.EfficiencyText, StringComparison.Ordinal);
    }

    [Fact]
    public void The_inputs_are_validated_and_a_missing_power_fluid_pressure_is_named()
    {
        Assert.Equal(HydraulicRules.CodeWellRequired, HydraulicRules.Validate(Inputs() with { WellId = " " })!.Value.Code);
        Assert.Equal(HydraulicRules.CodeKindUnknown, HydraulicRules.Validate(Inputs() with { Kind = "GEAR" })!.Value.Code);
        Assert.Equal(
            HydraulicRules.CodeInputRequired,
            HydraulicRules.Validate(Inputs() with { WellDepthFt = null })!.Value.Code);

        // The state table's "missing power fluid pressure - refused with the reason".
        var noPressure = HydraulicRules.Validate(Inputs() with { PowerFluidPressure = null });

        Assert.Equal(HydraulicRules.CodeInputRequired, noPressure!.Value.Code);
        Assert.Contains("power fluid pressure", noPressure.Value.Message, StringComparison.Ordinal);
        Assert.Contains("no power fluid horsepower", noPressure.Value.Message, StringComparison.Ordinal);

        Assert.Contains(HydraulicRules.Missing(Inputs() with { PowerFluidPressure = null }), item => item == "power fluid pressure");

        // A jet needs its nozzle and throat; a piston needs its piston, rod, stroke and rate.
        Assert.Contains(HydraulicRules.Missing(Clean() with { NozzleDiameter = null }), item => item == "nozzle diameter");
        Assert.Contains(HydraulicRules.Missing(Piston() with { StrokesPerMinute = null }), item => item == "strokes per minute");

        Assert.Null(HydraulicRules.Validate(Clean()));
        Assert.Null(HydraulicRules.Validate(Piston()));
    }

    [Fact]
    public async Task Commands_need_a_connection_and_an_actor_and_an_error_blocks_the_save()
    {
        var service = new OgHydraulicPumpService(() => null, new Quiet());
        var design = OgHydraulicPumpService.Build(Clean());

        Assert.Equal(HydraulicRules.CodeNoConnection, (await service.LoadAsync()).Reference);
        Assert.Equal(HydraulicRules.CodeActorRequired, (await service.SaveAsync(design, " ")).Reference);

        // A design carrying an error finding cannot be stored even if a caller builds one.
        var blocked = design with
        {
            Findings = [new OgHydraulicFinding("NOZZLE_NOT_LESS_THAN_THROAT", "Nozzle diameter must be less than throat diameter.", "Error")],
        };

        var refused = await service.SaveAsync(blocked, "sara");

        Assert.False(refused.Success);
        Assert.Equal(HydraulicRules.CodeBlockingFinding, refused.Reference);
        Assert.Contains("Nozzle diameter must be less than throat diameter.", refused.Message!, StringComparison.Ordinal);

        Assert.Equal(HydraulicRules.CodeNoConnection, (await service.SaveAsync(design, "sara")).Reference);
        Assert.Equal(HydraulicRules.CodeNothingPending, (await service.RetrySaveAsync("sara")).Reference);
        Assert.Null(service.Pending);
    }

    // ------------------------------------------------------------------ helpers

    private static OgHydraulicPumpService Service() => new(() => null, new Quiet());

    /// <summary>
    /// A 3,000 ft well whose jet design sits comfortably inside every limit: the M x N ratio is about 0.58, so nothing is
    /// clamped and no criterion is violated.
    /// </summary>
    private static OgHydraulicInputs Clean() =>
        Inputs() with
        {
            WellDepthFt = 3000m, PowerFluidPressure = 3000m, PowerFluidRate = 600m,
            BottomHolePressure = 1000m, WellheadPressure = 200m,
            NozzleDiameter = 0.375m, ThroatDiameter = 0.75m,
        };

    /// <summary>
    /// A deep well whose jet design pushes M x N above 1, so the engine clamps the efficiency (AC-3).
    /// </summary>
    private static OgHydraulicInputs Clamped() =>
        Inputs() with
        {
            WellDepthFt = 8000m, PowerFluidPressure = 4000m, PowerFluidRate = 600m,
            BottomHolePressure = 2500m, WellheadPressure = 200m,
            NozzleDiameter = 0.375m, ThroatDiameter = 0.75m,
        };

    private static OgHydraulicInputs Piston() =>
        Inputs() with
        {
            Kind = HydraulicPumpKinds.Piston,
            PistonDiameter = 2.0m, RodDiameter = 0.75m, StrokeLength = 36m, StrokesPerMinute = 12m,
            PowerFluidPressure = 3000m, PowerFluidRate = 600m,
            WellDepthFt = 3000m, BottomHolePressure = 1000m, WellheadPressure = 200m,
        };

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

    private static HYDRAULIC_PUMP_WELL_PROPERTIES Well() =>
        new()
        {
            WELL_DEPTH = 3000m,
            TUBING_DIAMETER = 2.875m,
            CASING_DIAMETER = 7m,
            WELLHEAD_PRESSURE = 200m,
            BOTTOM_HOLE_PRESSURE = 1000m,
            WELLHEAD_TEMPERATURE = 560m,
            BOTTOM_HOLE_TEMPERATURE = 660m,
            OIL_GRAVITY = 35m,
            WATER_CUT = 0.3m,
            GAS_OIL_RATIO = 300m,
            GAS_SPECIFIC_GRAVITY = 0.65m,
            DESIRED_PRODUCTION_RATE = 400m,
            PUMP_DEPTH = 3000m,
        };

    private static HYDRAULIC_JET_PUMP_PROPERTIES JetProperties(decimal nozzle, decimal throat) =>
        new()
        {
            NOZZLE_DIAMETER = nozzle,
            THROAT_DIAMETER = throat,
            POWER_FLUID_PRESSURE = 3000m,
            POWER_FLUID_RATE = 600m,
            POWER_FLUID_SPECIFIC_GRAVITY = 1.07m,
        };

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
