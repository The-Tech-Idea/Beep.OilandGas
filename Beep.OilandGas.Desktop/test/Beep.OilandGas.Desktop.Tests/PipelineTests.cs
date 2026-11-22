using System.Globalization;
using Beep.OilandGas.GasProperties.Calculations;
using Beep.OilandGas.PipelineAnalysis.Constants;
using TheTechIdea.Beep.OilandGas.Desktop.Data.WellTest;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// WTA-03: the reference capacity case, the erosion threshold's source, the regime, the experimental tabs and the export -
/// the parts that do not need a database. Saving and reloading are proven in the gateway test.
/// </summary>
public sealed class PipelineTests
{
    private const string PageFileName = "PipelinePageView.cs";

    [Fact]
    public void A_reference_gas_capacity_matches_the_weymouth_relation()
    {
        // AC-1 and SG-08-5. The Weymouth relation is published - Q = 433.5 (Tb/Pb) D^2.667 sqrt((P1^2-P2^2)/(L T Z SG)) -
        // so the expected capacity is computed here from it, taking only the Z-factor from the module's own correlation.
        var inputs = Gas();
        var result = Service().Build(inputs).Result!;

        Assert.Equal(PipelineModes.Gas, result.Mode);
        Assert.Equal(OgPipelineResult.GasCapacityUnit, result.CapacityUnit);
        Assert.True(result.Capacity > 0m);

        var pipeline = inputs;
        var averagePressure = (pipeline.InletPressure!.Value + pipeline.OutletPressure!.Value) / 2m;
        var z = ZFactorCalculator.CalculateBrillBeggs(
            averagePressure, pipeline.AverageTemperatureR!.Value, pipeline.GasSpecificGravity!.Value);

        var constant = 433.5m * pipeline.BaseTemperatureR!.Value / pipeline.BasePressure!.Value;
        var diameterTerm = (decimal)Math.Pow((double)pipeline.DiameterIn!.Value, 2.667);
        var difference = pipeline.InletPressure.Value * pipeline.InletPressure.Value -
                         pipeline.OutletPressure.Value * pipeline.OutletPressure.Value;
        var pressureTerm = (decimal)Math.Sqrt((double)(difference /
            (pipeline.LengthMiles!.Value * pipeline.AverageTemperatureR.Value * z *
             pipeline.GasSpecificGravity.Value)));

        var expectedMscfd = constant * diameterTerm * pressureTerm / 1000m;

        Assert.InRange(
            Math.Abs(result.Capacity - expectedMscfd) / expectedMscfd,
            0m,
            0.001m);

        // The pressure drop is the one the caller stated, and the velocity the capacity implies.
        Assert.Equal(pipeline.InletPressure - pipeline.OutletPressure, result.PressureDrop);
        Assert.NotNull(result.Velocity);
        Assert.True(result.Velocity > 0m);

        // A wider line carries more at the same drawdown - the capacity is a function of the diameter.
        var wider = Service().Build(Gas() with { DiameterIn = 24m }).Result!;

        Assert.True(wider.Capacity > result.Capacity);
    }

    [Fact]
    public void The_erosion_threshold_always_names_its_source()
    {
        // AC-2 and SG-08-6.
        var result = Service().Build(Gas()).Result!;

        Assert.NotNull(result.Erosion);
        Assert.Equal(PipelineStandards.ErosionalVelocitySource, PipelineStandards.ErosionalVelocitySource);
        Assert.Contains("API RP 14E", result.Erosion!.Source, StringComparison.Ordinal);
        Assert.Contains("API RP 14E", result.ThresholdText, StringComparison.Ordinal);
        Assert.Contains("C = 100", result.Erosion.Source, StringComparison.Ordinal);
        Assert.True(result.Erosion.Threshold > 0m);
        Assert.True(result.Erosion.ActualVelocity > 0m);

        // The three services are the engine's own C-factors, and the limit scales with them.
        var solid = Service().Build(Gas() with { ErosionService = "SOLID" }).Result!;
        var clean = Service().Build(Gas() with { ErosionService = "CLEAN" }).Result!;

        Assert.Equal(
            PipelineStandards.ErosionServices.Select(service => service.CFactor),
            [100m, 125m, 150m]);

        Assert.Equal(
            clean.Erosion!.Threshold / solid.Erosion!.Threshold,
            150m / 100m,
            4);

        Assert.Contains("C = 150", clean.Erosion.Source, StringComparison.Ordinal);
        Assert.Contains("clean", clean.Erosion.Source, StringComparison.Ordinal);

        // A service that is not in the standards is refused, naming them.
        var unknown = PipelineRules.Validate(Gas() with { ErosionService = "WET" });

        Assert.Equal(PipelineRules.CodeServiceUnknown, unknown!.Value.Code);
        Assert.Contains("INHIBITED", unknown.Value.Message, StringComparison.Ordinal);

        // A line above the limit says so in words and in the verdict.
        var fast = Service().Build(Gas() with
        {
            DiameterIn = 4m, LengthMiles = 1m, InletPressure = 2000m, OutletPressure = 100m,
        }).Result!;

        Assert.True(fast.ErosionAboveThreshold);
        Assert.False(fast.Feasible);
        Assert.Contains("ABOVE it", fast.Erosion!.Describe(), StringComparison.Ordinal);
        Assert.Contains("above the erosional limit", fast.NotesText, StringComparison.Ordinal);

        // The sand rate, when one is given, carries its own model's name.
        var sandy = Service().Build(Gas() with { SandRateBblPerDay = 5m }).Result!;

        Assert.NotNull(sandy.Erosion!.SandRateMpy);
        Assert.Equal(PipelineStandards.SandErosionSource, sandy.Erosion.SandSource);
        Assert.Contains("Salama-Venkatesh", sandy.Erosion.Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public void The_flow_regime_is_displayed_with_its_thresholds()
    {
        // AC-3.
        var result = Service().Build(Gas()).Result!;

        Assert.True(result.RegimeDetermined);
        Assert.Contains(result.Regime, new[] { "LAMINAR", "TRANSITIONAL", "TURBULENT" });
        Assert.Contains("laminar below", result.RegimeText, StringComparison.Ordinal);
        Assert.Contains("2,000", result.RegimeText, StringComparison.Ordinal);
        Assert.Contains("4,000", result.RegimeText, StringComparison.Ordinal);
        Assert.NotNull(result.Reynolds);

        // The bands are the module's own constants, applied at their boundaries.
        Assert.Equal(
            PipelineStandards.RegimeLaminar,
            PipelineStandards.RegimeOf(PipelineConstants.LaminarFlowThreshold - 1m));
        Assert.Equal(
            PipelineStandards.RegimeTransitional,
            PipelineStandards.RegimeOf(PipelineConstants.LaminarFlowThreshold));
        Assert.Equal(
            PipelineStandards.RegimeTurbulent,
            PipelineStandards.RegimeOf(PipelineConstants.TurbulentFlowThreshold));
        Assert.Equal(PipelineStandards.RegimeUndetermined, PipelineStandards.RegimeOf(0m));

        // A regime that cannot be decided refuses the run and names the velocity it saw.
        var undetermined = new OgPipelineResult(
            string.Empty, "LINE-1", "PL-1", PipelineModes.Gas, 0m, OgPipelineResult.GasCapacityUnit,
            PipelineStandards.RegimeUndetermined, 0m, null, 0m, 0m, null, false, [], Gas(), null, null);

        Assert.False(undetermined.RegimeDetermined);
        Assert.Contains("UNDETERMINED", undetermined.RegimeText, StringComparison.Ordinal);
    }

    [Fact]
    public void The_experimental_tabs_are_disabled_and_labelled()
    {
        // AC-4.
        var workspace = new OgPipelineWorkspace(
            [], null, PipelineModes.All,
            PipelineStandards.ErosionServices.Select(service => service.Code).ToList(),
            ["slug flow", "simulation"],
            PipelineStandards.ErosionalVelocitySource, false, null);

        Assert.Contains("slug flow", workspace.DisabledText, StringComparison.Ordinal);
        Assert.Contains("simulation", workspace.DisabledText, StringComparison.Ordinal);
        Assert.Contains("disabled by default", workspace.DisabledText, StringComparison.Ordinal);
        Assert.Contains("experimental", workspace.DisabledText, StringComparison.Ordinal);

        // The page has no slug-flow or simulation panel: it only prints the workspace's disabled list.
        var page = File.ReadAllText(SourceRoot.AllSources().Single(file => Path.GetFileName(file) == PageFileName));

        Assert.Contains("workspace.DisabledText", page, StringComparison.Ordinal);
        Assert.DoesNotContain("SlugFlowCalculator", page, StringComparison.Ordinal);
        Assert.DoesNotContain("Simulation", page, StringComparison.Ordinal);
    }

    [Fact]
    public void The_export_carries_the_same_numbers_as_the_screen()
    {
        // AC-5, with a stated substitution: the module's own reporting service returns canned findings, so the export is
        // built from the same properties the panels print and cannot disagree with them.
        var result = Service().Build(Gas()).Result!;
        var export = result.ExportText;

        foreach (var shown in new[]
                 {
                     result.ModeText,
                     result.CapacityText,
                     result.RegimeText,
                     result.ReynoldsText,
                     result.FrictionText,
                     result.PressureDropText,
                     result.ThresholdText,
                     result.VerdictText,
                     result.NotesText,
                 })
        {
            Assert.Contains(shown, export, StringComparison.Ordinal);
        }

        Assert.Contains("Pipeline analysis", export, StringComparison.Ordinal);

        // The page reads the same properties (Describe carries the capacity and the verdict), so the panel and the export
        // cannot drift, and the page exposes the export from the result rather than formatting its own copy.
        var page = File.ReadAllText(SourceRoot.AllSources().Single(file => Path.GetFileName(file) == PageFileName));

        Assert.Contains("result.Describe()", page, StringComparison.Ordinal);
        Assert.Contains("result.RegimeText", page, StringComparison.Ordinal);
        Assert.Contains("result.ThresholdText", page, StringComparison.Ordinal);
        Assert.Contains("_result?.ExportText", page, StringComparison.Ordinal);
        Assert.DoesNotContain("PipelineAnalysisService", page, StringComparison.Ordinal);
    }

    [Fact]
    public void A_liquid_line_uses_darcy_weisbach_and_reports_its_screening_friction()
    {
        // The liquid path is the module's Darcy-Weisbach relation. Its friction factor is evaluated at a typical velocity,
        // so the answer is screened and says so - and the arithmetic still satisfies Q = v A for the f it reports.
        var inputs = Gas() with { Mode = PipelineModes.Liquid };
        var result = Service().Build(inputs).Result!;

        Assert.Equal(PipelineModes.Liquid, result.Mode);
        Assert.Equal(OgPipelineResult.LiquidCapacityUnit, result.CapacityUnit);
        Assert.True(result.Capacity > 0m);
        Assert.Contains("screening figure", result.NotesText, StringComparison.Ordinal);

        // v = sqrt(2 g D h / (f L)) with h in feet of the line's own liquid, then Q = v A - the two identities the module
        // solves, recomputed from its reported friction factor and velocity.
        var diameterFt = inputs.DiameterIn!.Value / 12m;
        var area = (decimal)Math.PI * diameterFt * diameterFt / 4m;
        var density = inputs.LiquidSpecificGravity!.Value * 62.4m;
        var head = (inputs.InletPressure!.Value - inputs.OutletPressure!.Value) * 144m / density;
        var lengthFt = inputs.LengthMiles!.Value * 5280m;

        var expectedVelocity = (decimal)Math.Sqrt((double)(2m * 32.174m * diameterFt * head /
                                                         (result.FrictionFactor!.Value * lengthFt)));

        Assert.InRange(Math.Abs(result.Velocity!.Value - expectedVelocity), 0m, 0.01m);
        Assert.InRange(Math.Abs(result.Velocity.Value * area * 86400m / 5.615m - result.Capacity), 0m, 1m);
    }

    [Fact]
    public void The_inputs_are_validated_and_missing_fluid_properties_are_named()
    {
        // The state table's "missing fluid properties - refused, links to RES-01".
        Assert.Equal(PipelineRules.CodeWellRequired, PipelineRules.Validate(Gas() with { WellId = " " })!.Value.Code);
        Assert.Equal(PipelineRules.CodeModeUnknown, PipelineRules.Validate(Gas() with { Mode = "STEAM" })!.Value.Code);
        Assert.Equal(PipelineRules.CodeInputRequired, PipelineRules.Validate(Gas() with { DiameterIn = null })!.Value.Code);

        Assert.Contains(PipelineRules.Missing(Gas() with { GasSpecificGravity = null }),
            item => item == "gas specific gravity");
        Assert.Contains(PipelineRules.Missing(Gas() with { Mode = PipelineModes.Liquid, LiquidViscosityCp = null }),
            item => item == "liquid viscosity");

        Assert.Equal(
            PipelineRules.CodeInputInvalid,
            PipelineRules.Validate(Gas() with { OutletPressure = 1000m })!.Value.Code);
        Assert.Equal(
            PipelineRules.CodeInputInvalid,
            PipelineRules.Validate(Gas() with { LengthMiles = 0m })!.Value.Code);

        // The engine's own refusal (outlet at or above inlet) is answered with a sentence and a reference, not a stack trace.
        var refused = Service().Build(Gas() with { OutletPressure = 1000m });

        Assert.Equal(PipelineRules.CodeAnalysisRefused, refused.Refusal!.Value.Code);
        Assert.Contains("reference", refused.Refusal.Value.Message, StringComparison.Ordinal);

        Assert.Null(PipelineRules.Validate(Gas()));
        Assert.Null(PipelineRules.Validate(Gas() with { Mode = PipelineModes.Liquid }));
    }

    [Fact]
    public async Task Commands_need_a_connection_and_an_actor_and_an_undecided_result_is_not_saved()
    {
        var service = new OgPipelineService(() => null, new Quiet());
        var result = Service().Build(Gas()).Result!;

        Assert.Equal(PipelineRules.CodeNoConnection, (await service.LoadAsync()).Reference);
        Assert.Equal(PipelineRules.CodeActorRequired, (await service.SaveAsync(result, " ")).Reference);
        Assert.Equal(PipelineRules.CodeNoConnection, (await service.SaveAsync(result, "sara")).Reference);
        Assert.Equal(PipelineRules.CodeNothingPending, (await service.RetrySaveAsync("sara")).Reference);
        Assert.Null(service.Pending);

        Assert.Equal(
            PipelineRules.CodeNothingToSave,
            (await service.SaveAsync(result with { Regime = PipelineStandards.RegimeUndetermined }, "sara")).Reference);
    }

    // ------------------------------------------------------------------ helpers

    private static OgPipelineService Service() => new(() => null, new Quiet());

    /// <summary>A 12 in, 10 mi gas line from 1,000 to 800 psia at 530 R.</summary>
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

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
