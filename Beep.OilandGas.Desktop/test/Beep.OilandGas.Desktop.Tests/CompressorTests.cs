using System.Globalization;
using Beep.OilandGas.CompressorAnalysis.Constants;
using Beep.OilandGas.CompressorAnalysis.Data;
using Beep.OilandGas.GasProperties.Calculations;
using Beep.OilandGas.Models.Data.Calculations;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Lift;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// LFT-08: the reference head and power, the interstage temperature limit, the reasons an infeasible machine is refused,
/// the tokens from <c>CompressorAnalysisWellKnown</c>, the units and the validation - the parts that do not need a database.
/// Saving and reloading are proven in the gateway test.
/// </summary>
public sealed class CompressorTests
{
    private const string PageFileName = "CompressorPageView.cs";

    [Fact]
    public void A_reference_centrifugal_case_reproduces_head_and_power()
    {
        // AC-1: the staged head and power are recomputed here from the published relations - the polytropic exponent
        // n/(n-1) = k/(k-1) * eta_p, the stage head H = Z R T_in / MW * n/(n-1) * (r^((n-1)/n) - 1), and the weight flow
        // W = Q/1440 * MW / 379 - taking only the Z-factor from the module's own correlation.
        var result = OgCompressorService.Build(Centrifugal());

        Assert.True(result.Feasible, result.ReasonsText);
        Assert.Equal(3, result.Stages);
        Assert.NotNull(result.Head);
        Assert.NotNull(result.Power);

        var inputs = Centrifugal();
        var eta = inputs.PolytropicEfficiency!.Value;
        var k = inputs.SpecificHeatRatio!.Value;
        var sg = inputs.GasSpecificGravity!.Value;
        var mw = sg * CompressorConstants.AirMolecularWeight;
        var suction = inputs.SuctionPressure!.Value;
        var ratio = inputs.DischargePressure!.Value / suction;
        var stages = result.Stages!.Value;
        var stageRatio = Math.Pow((double)ratio, 1.0 / stages);

        const decimal intercoolerApproach = 20m;

        var n = (double)(k * eta / (k * eta - (k - 1m)));
        var exponent = (n - 1.0) / n;

        var weightFlow = inputs.GasFlowMscfPerDay!.Value * CompressorConstants.MscfToScf /
                         CompressorConstants.MinutesPerDay * mw / CompressorConstants.StandardGasVolume;

        double expectedHead = 0.0;
        double expectedBrakeHorsepower = 0.0;
        var temperature = inputs.SuctionTemperatureF!.Value + 459.67m;
        var pressure = suction;

        for (var stage = 1; stage <= stages; stage++)
        {
            var discharge = pressure * (decimal)stageRatio;
            var z = ZFactorCalculator.CalculateBrillBeggs((pressure + discharge) / 2m, temperature, sg);

            var head = (double)(z * CompressorConstants.GasConstant * temperature / mw) *
                       1.0 / (1.0 - 1.0 / n) *
                       (Math.Pow(stageRatio, exponent) - 1.0);

            expectedHead += head;
            expectedBrakeHorsepower += (double)weightFlow * head / 33000.0 / (double)eta;

            temperature = inputs.SuctionTemperatureF.Value + 459.67m + intercoolerApproach;
            pressure = discharge;
        }

        var expectedMotorHorsepower = expectedBrakeHorsepower / (double)(inputs.MechanicalEfficiency!.Value);

        // The engine rounds each stage head to whole ft-lbf/lbm, so the comparison is relative.
        Assert.InRange(
            Math.Abs((double)result.Head!.Value - expectedHead) / expectedHead,
            0.0,
            0.01);

        Assert.InRange(
            Math.Abs((double)result.Power!.Value - expectedMotorHorsepower) / expectedMotorHorsepower,
            0.0,
            0.01);

        // The physics the earlier exponent broke: compression heats the gas.
        Assert.NotNull(result.DischargeTemperature);
        Assert.True(result.DischargeTemperature > inputs.SuctionTemperatureF!.Value + 459.67m);
    }

    [Fact]
    public void Staging_respects_the_interstage_temperature_limit()
    {
        // AC-2: every allowed stage count is inside both limits and every rejected one is outside at least one.
        var result = OgCompressorService.Build(Centrifugal());

        Assert.NotEmpty(result.Staging);
        Assert.Equal(CompressorStagingLimits.MaximumDischargeTemperatureR, result.StageTemperatureLimitR);
        Assert.Equal(CompressorStagingLimits.MaximumRatioPerStage, result.StageRatioLimit);

        Assert.All(result.Staging, stage =>
        {
            var insideTemperature = stage.HottestR <= result.StageTemperatureLimitR!.Value;
            var insideRatio = stage.Ratio <= result.StageRatioLimit!.Value;

            Assert.Equal(insideTemperature && insideRatio, stage.Allowed);
        });

        // The recommendation is the fewest stages that satisfies both, and it is the one used.
        var fewestAllowed = result.Staging.Where(stage => stage.Allowed).Min(stage => stage.Stages);

        Assert.Equal(fewestAllowed, result.Stages);

        // The stage count it settled on really is inside the limit; one fewer is not.
        var chosen = result.Staging.Single(stage => stage.Stages == result.Stages);

        Assert.True(chosen.Allowed);
        Assert.True(chosen.HottestR <= CompressorStagingLimits.MaximumDischargeTemperatureR);

        var fewer = result.Staging.Where(stage => stage.Stages < result.Stages).ToList();

        Assert.All(fewer, stage => Assert.False(stage.Allowed));

        // The limit is printed with the result, in both units.
        Assert.Contains("interstage limit", result.StageText, StringComparison.Ordinal);
        Assert.Contains("R (", result.StageText, StringComparison.Ordinal);
        Assert.Contains(" F", result.StageText, StringComparison.Ordinal);
    }

    [Fact]
    public void An_infeasible_machine_lists_every_reason_and_presents_no_power()
    {
        // AC-3, and the state table's "no power figure presented as achieved".
        var result = OgCompressorService.Build(Centrifugal() with
        {
            DischargePressure = 3600m, Stages = 1,
        });

        Assert.False(result.Feasible);
        Assert.Null(result.Power);
        Assert.Null(result.PowerKilowatts);
        Assert.Contains("no power figure", result.PowerText, StringComparison.Ordinal);

        Assert.True(result.Reasons.Count >= 3, result.ReasonsText);

        Assert.Contains(result.Reasons, reason => reason.Contains("per-stage ratio of 12", StringComparison.Ordinal));
        Assert.Contains(result.Reasons, reason => reason.Contains("above the interstage limit of 660 R", StringComparison.Ordinal));
        Assert.Contains(result.Reasons, reason => reason.Contains("above the module's maximum of 10", StringComparison.Ordinal));

        // Every reason is printed, one per line.
        foreach (var reason in result.Reasons)
        {
            Assert.Contains(reason, result.ReasonsText, StringComparison.Ordinal);
        }

        // A feasible machine has no reason against it.
        var feasible = OgCompressorService.Build(Centrifugal());

        Assert.True(feasible.Feasible);
        Assert.Empty(feasible.Reasons);
        Assert.Contains("meets the duty", feasible.ReasonsText, StringComparison.Ordinal);
    }

    [Fact]
    public void The_kinds_and_tokens_come_from_the_well_known_type()
    {
        // AC-4.
        Assert.Equal(CompressorAnalysisWellKnown.CompressorType.Centrifugal, CompressorKinds.Centrifugal);
        Assert.Equal(CompressorAnalysisWellKnown.CompressorType.Reciprocating, CompressorKinds.Reciprocating);
        Assert.Equal(
            [
                CompressorAnalysisWellKnown.CompressorType.Centrifugal,
                CompressorAnalysisWellKnown.CompressorType.Reciprocating,
            ],
            CompressorKinds.All);

        Assert.Contains(CompressorAnalysisWellKnown.AnalysisType.Power, CompressorKinds.AnalysisTypes);
        Assert.Contains(CompressorAnalysisWellKnown.AnalysisType.Pressure, CompressorKinds.AnalysisTypes);
        Assert.Contains(CompressorAnalysisWellKnown.AnalysisType.Efficiency, CompressorKinds.AnalysisTypes);

        // An unknown kind is refused, naming the tokens.
        var refused = CompressorRules.Validate(Centrifugal() with { Kind = "SCREW" });

        Assert.Equal(CompressorRules.CodeKindUnknown, refused!.Value.Code);
        Assert.Contains("CENTRIFUGAL", refused.Value.Message, StringComparison.Ordinal);

        // The view binds the workspace's lists and names no token or table of its own.
        var page = File.ReadAllText(SourceRoot.AllSources().Single(file => Path.GetFileName(file) == PageFileName));

        Assert.Contains("workspace.Kinds", page, StringComparison.Ordinal);
        Assert.DoesNotContain("\"CENTRIFUGAL\"", page, StringComparison.Ordinal);
        Assert.DoesNotContain("COMPRESSOR_DESIGN", page, StringComparison.Ordinal);
        Assert.DoesNotContain("COMPRESSOR_PERFORMANCE", page, StringComparison.Ordinal);
    }

    [Fact]
    public void Power_and_head_carry_their_units()
    {
        // AC-5.
        var result = OgCompressorService.Build(Centrifugal());

        Assert.Equal("ft-lbf/lbm", OgCompressorResult.HeadUnitText);
        Assert.Equal("hp", OgCompressorResult.PowerUnitText);
        Assert.Equal(OgCompressorResult.HeadUnitText, result.HeadUnit);
        Assert.Equal(OgCompressorResult.PowerUnitText, result.PowerUnit);

        Assert.Contains("hp", result.PowerText, StringComparison.Ordinal);
        Assert.Contains("kW", result.PowerText, StringComparison.Ordinal);
        Assert.Contains("ft-lbf/lbm", result.HeadText, StringComparison.Ordinal);
        Assert.Contains("%", result.EfficiencyText, StringComparison.Ordinal);
        Assert.Contains("R (", result.DischargeTemperatureText, StringComparison.Ordinal);
    }

    [Fact]
    public void A_reciprocating_machine_uses_its_own_calculator()
    {
        var result = OgCompressorService.Build(Reciprocating());

        Assert.True(result.Feasible, result.ReasonsText);
        Assert.Equal(CompressorKinds.Reciprocating, result.Kind);
        Assert.NotNull(result.Head);
        Assert.NotNull(result.Power);
        Assert.Empty(result.Staging);

        // No interstage limit is applied to a reciprocating machine: the module states 660 R only for its centrifugal
        // staging plan, and a reciprocating discharge of 261.5 F is ordinary. The temperature is reported, not judged.
        Assert.Null(result.StageTemperatureLimitR);
        Assert.NotNull(result.DischargeTemperature);
        Assert.Contains("R (", result.DischargeTemperatureText, StringComparison.Ordinal);

        // A ratio above the module's own maximum is refused.
        var tooFar = OgCompressorService.Build(Reciprocating() with { DischargePressure = 4000m });

        Assert.False(tooFar.Feasible);
        Assert.Null(tooFar.Power);
        Assert.Contains(tooFar.Reasons, reason => reason.Contains("above the module's maximum of 10", StringComparison.Ordinal));
    }

    [Fact]
    public void The_analysis_is_reproducible()
    {
        var first = OgCompressorService.Build(Centrifugal());
        var second = OgCompressorService.Build(Centrifugal());

        Assert.Equal(first.Describe(), second.Describe());
        Assert.Equal(first.Head, second.Head);
        Assert.Equal(first.Power, second.Power);
        Assert.Equal(first.ReasonsText, second.ReasonsText);
        Assert.Equal(first.StagingText, second.StagingText);
    }

    [Fact]
    public void The_inputs_are_validated_and_an_unselected_kind_disables_the_analysis()
    {
        // The state table's "type not selected - analysis disabled with the reason".
        var noKind = CompressorRules.Validate(Centrifugal() with { Kind = null });

        Assert.Equal(CompressorRules.CodeKindRequired, noKind!.Value.Code);
        Assert.Contains("disabled", noKind.Value.Message, StringComparison.Ordinal);
        Assert.Contains("Choose the machine", noKind.Value.Message, StringComparison.Ordinal);

        Assert.Equal(CompressorRules.CodeWellRequired, CompressorRules.Validate(Centrifugal() with { WellId = " " })!.Value.Code);
        Assert.Equal(
            CompressorRules.CodeInputRequired,
            CompressorRules.Validate(Centrifugal() with { GasFlowMscfPerDay = null })!.Value.Code);
        Assert.Equal(
            CompressorRules.CodeInputInvalid,
            CompressorRules.Validate(Centrifugal() with { DischargePressure = 300m })!.Value.Code);
        Assert.Equal(
            CompressorRules.CodeInputInvalid,
            CompressorRules.Validate(Centrifugal() with { PolytropicEfficiency = 1.5m })!.Value.Code);
        Assert.Equal(
            CompressorRules.CodeInputInvalid,
            CompressorRules.Validate(Centrifugal() with { SpecificHeatRatio = 0.9m })!.Value.Code);
        Assert.Equal(
            CompressorRules.CodeInputInvalid,
            CompressorRules.Validate(Centrifugal() with { Stages = 9 })!.Value.Code);

        // A reciprocating machine needs its cylinder, stroke and speed.
        Assert.Contains(
            CompressorRules.Missing(Reciprocating() with { CylinderDiameter = null }),
            item => item == "cylinder diameter");

        Assert.Null(CompressorRules.Validate(Centrifugal()));
        Assert.Null(CompressorRules.Validate(Reciprocating()));
    }

    [Fact]
    public async Task Commands_need_a_connection_and_an_actor()
    {
        var service = new OgCompressorService(() => null, new Quiet());
        var result = OgCompressorService.Build(Centrifugal());

        Assert.Equal(CompressorRules.CodeNoConnection, (await service.LoadAsync()).Reference);
        Assert.Equal(CompressorRules.CodeActorRequired, (await service.SaveAsync(result, " ")).Reference);
        Assert.Equal(CompressorRules.CodeNoConnection, (await service.SaveAsync(result, "sara")).Reference);
        Assert.Equal(CompressorRules.CodeNothingPending, (await service.RetrySaveAsync("sara")).Reference);
        Assert.Null(service.Pending);

        Assert.Equal(
            CompressorRules.CodeNothingToSave,
            (await service.SaveAsync(result with { Kind = string.Empty }, "sara")).Reference);
    }

    // ------------------------------------------------------------------ helpers

    /// <summary>A 5 MMscf/d machine from 300 to 900 psia at 0.75 polytropic efficiency.</summary>
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

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
