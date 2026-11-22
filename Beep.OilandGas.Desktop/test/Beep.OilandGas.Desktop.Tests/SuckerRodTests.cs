using Beep.OilandGas.SuckerRodPumping.Constants;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Lift;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// LFT-04: the unsafe-string refusal, the grades from the owning constants, the pump card against the computed loads, the
/// reproducibility, the units and the density acceptance - the parts that do not need a database. Saving and reloading are
/// proven in the gateway test.
/// </summary>
public sealed class SuckerRodTests
{
    private const string PageFileName = "SuckerRodPageView.cs";

    [Fact]
    public async Task An_unsafe_string_is_refused_and_names_the_limiting_section()
    {
        // AC-1 and SG-07-5: the thin string's lowest safety factor is under the module's minimum.
        var refused = await Service().DesignAsync(Thin());

        Assert.False(refused.Success);
        Assert.Equal(SrpRules.CodeUnsafeString, refused.Reference);
        Assert.Null(refused.Value);

        Assert.Contains("below the module's", refused.Message!, StringComparison.Ordinal);
        Assert.Contains("section 1", refused.Message, StringComparison.Ordinal);
        Assert.Contains("0.625 in", refused.Message, StringComparison.Ordinal);
        Assert.Contains("psi", refused.Message, StringComparison.Ordinal);
        Assert.Contains("Goodman", refused.Message, StringComparison.Ordinal);
        Assert.Contains(SrpRules.RequiredSafetyFactor.ToString("0.###"), refused.Message, StringComparison.Ordinal);

        // The same string in a stronger grade is safe, which is what the message recommends.
        var stronger = OgSuckerRodService.Build(Thin() with { Grade = SuckerRodConstants.RodGradeHY });

        Assert.True(stronger.MinSafetyFactor >= stronger.RequiredSafetyFactor);
    }

    [Fact]
    public async Task A_safe_string_reports_the_loads_stress_and_safety_factor()
    {
        var designed = await Service().DesignAsync(Inputs());

        Assert.True(designed.Success, designed.Message);

        var result = designed.Value!.Selected!;

        Assert.False(result.IsUnsafe);
        Assert.True(result.MinSafetyFactor > result.RequiredSafetyFactor);
        Assert.Equal(SrpRules.RequiredSafetyFactor, result.RequiredSafetyFactor);
        Assert.Equal((decimal)SuckerRodConstants.MinimumSafetyFactor, SrpRules.RequiredSafetyFactor);

        Assert.True(result.PeakLoad > 0m);
        Assert.True(result.MinLoad > 0m);
        Assert.True(result.FluidLoad > 0m);
        Assert.True(result.MaxStress > 0m);
        Assert.True(result.StressRange > 0m);
        Assert.True(result.ProductionRate > 0m);
        Assert.True(result.MotorHorsepower > 0m);

        // Every section is analysed and the critical one is the weakest.
        Assert.Equal(3, result.SectionResults.Count);
        Assert.Equal(
            result.SectionResults.Min(section => section.SafetyFactor),
            result.MinSafetyFactor);

        var critical = result.SectionResults.Single(section => section.Order == result.CriticalSection);

        Assert.Equal(result.MinSafetyFactor, critical.SafetyFactor);

        // The critical section is named with the values that decided it.
        Assert.Contains("section " + result.CriticalSection.ToString(), critical.Describe(), StringComparison.Ordinal);
        Assert.Contains("safety factor", critical.Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public void The_pump_card_matches_the_computed_loads()
    {
        // AC-3.
        var result = OgSuckerRodService.Build(Inputs());

        Assert.NotEmpty(result.CardPoints);
        Assert.Equal(result.PeakLoad, result.CardPeak);
        Assert.Equal(result.MinLoad, result.CardMinimum);

        // The card's sampled extremes track those loads: the owner's card carries an analytic peak and minimum and a
        // sampled set of points, so the samples are within 1% rather than identical.
        Assert.InRange(
            Math.Abs(result.CardPoints.Max(point => point.Load) - result.CardPeak),
            0m, result.CardPeak * 0.01m);
        Assert.InRange(
            Math.Abs(result.CardPoints.Min(point => point.Load) - result.CardMinimum),
            0m, result.CardMinimum * 0.01m);
        Assert.Contains("card runs from", result.CardText, StringComparison.Ordinal);
        Assert.Contains("lb", result.CardText, StringComparison.Ordinal);
    }

    [Fact]
    public void The_grades_come_from_the_owning_constants()
    {
        // AC-2: no literal grade list in the view, and the module's set is what the service offers.
        Assert.Equal(
            [
                SuckerRodConstants.RodGradeC,
                SuckerRodConstants.RodGradeD,
                SuckerRodConstants.RodGradeHL,
                SuckerRodConstants.RodGradeHY,
            ],
            RodGrades.All);

        Assert.DoesNotContain(SuckerRodConstants.RodGradeAuto, RodGrades.All);
        Assert.True(RodGrades.IsOptimizer(SuckerRodConstants.RodGradeAuto));
        Assert.Contains("Grade D", RodGrades.NameOf(SuckerRodConstants.RodGradeD), StringComparison.Ordinal);

        var page = File.ReadAllText(SourceRoot.AllSources().Single(file => Path.GetFileName(file) == PageFileName));

        // The view reads the grade list from the workspace and names none of the grades itself.
        Assert.Contains("workspace.Grades", page, StringComparison.Ordinal);
        Assert.DoesNotContain("\"HL\"", page, StringComparison.Ordinal);
        Assert.DoesNotContain("\"HY\"", page, StringComparison.Ordinal);

        // An unknown grade is refused rather than defaulted.
        Assert.Equal(SrpRules.CodeGradeUnknown, SrpRules.Validate(Inputs() with { Grade = "Z" })!.Value.Code);
    }

    [Fact]
    public void The_design_is_reproducible()
    {
        // AC-4: the same inputs give the same string and the same numbers.
        var first = OgSuckerRodService.Build(Inputs());
        var second = OgSuckerRodService.Build(Inputs());

        Assert.Equal(first.Describe(), second.Describe());
        Assert.Equal(first.MinSafetyFactor, second.MinSafetyFactor);
        Assert.Equal(first.PeakLoad, second.PeakLoad);
        Assert.Equal(first.MaxStress, second.MaxStress);
        Assert.Equal(first.CardPeak, second.CardPeak);
        Assert.Equal(first.SectionResults.Select(section => section.SafetyFactor),
            second.SectionResults.Select(section => section.SafetyFactor));
    }

    [Fact]
    public void Lengths_and_diameters_carry_units()
    {
        // AC-5.
        var result = OgSuckerRodService.Build(Inputs());

        Assert.Equal("in", OgSrpResult.DiameterUnit);
        Assert.Equal("ft", OgSrpResult.LengthUnit);
        Assert.Equal("lb", OgSrpResult.LoadUnit);
        Assert.Equal("psi", OgSrpResult.StressUnit);

        Assert.All(result.Sections, section =>
        {
            Assert.Contains(" in × ", section.Describe(), StringComparison.Ordinal);
            Assert.Contains(" ft", section.Describe(), StringComparison.Ordinal);
        });

        Assert.Contains("diameters in in", result.UnitsText, StringComparison.Ordinal);
        Assert.Contains("lengths in ft", result.UnitsText, StringComparison.Ordinal);
        Assert.Contains("lb", result.PeakText, StringComparison.Ordinal);
        Assert.Contains("psi", result.MaxStressText, StringComparison.Ordinal);
        Assert.Contains("STB/d", result.RateText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_missing_density_is_a_default_that_must_be_accepted()
    {
        // The state table: "missing density - default offered but must be accepted explicitly".
        var notAccepted = Inputs() with { RodDensity = null, AcceptDefaultDensity = false };

        Assert.Contains(
            SrpRules.Missing(notAccepted),
            item => item.StartsWith("the rod density", StringComparison.Ordinal));
        Assert.Equal(SrpRules.CodeDensityNotAccepted, SrpRules.Validate(notAccepted)!.Value.Code);

        var refused = await Service().DesignAsync(notAccepted);

        Assert.False(refused.Success);
        Assert.Equal(SrpRules.CodeDensityNotAccepted, refused.Reference);

        var accepted = await Service().DesignAsync(notAccepted with { AcceptDefaultDensity = true });

        Assert.True(accepted.Success, accepted.Message);

        var result = accepted.Value!.Selected!;

        Assert.True(result.DensityAssumed);
        Assert.Contains(
            SuckerRodConstants.SteelDensity.ToString("#,0.##"),
            result.DensityText,
            StringComparison.Ordinal);
        Assert.Contains("accepted explicitly", result.DensityText, StringComparison.Ordinal);

        // An entered density is not "assumed".
        var typed = OgSuckerRodService.Build(Inputs() with { RodDensity = 495m, AcceptDefaultDensity = true });

        Assert.False(typed.DensityAssumed);
        Assert.Contains("was entered", typed.DensityText, StringComparison.Ordinal);
    }

    [Fact]
    public void The_inputs_are_validated_before_the_engine_is_called()
    {
        Assert.Equal(SrpRules.CodeWellRequired, SrpRules.Validate(Inputs() with { WellId = " " })!.Value.Code);
        Assert.Equal(SrpRules.CodeInputRequired, SrpRules.Validate(Inputs() with { PumpDepthFt = null })!.Value.Code);
        Assert.Equal(SrpRules.CodeInputInvalid, SrpRules.Validate(Inputs() with { StrokesPerMinute = 0m })!.Value.Code);
        Assert.Equal(SrpRules.CodeInputInvalid, SrpRules.Validate(Inputs() with { WaterCut = 1.5m })!.Value.Code);
        Assert.Equal(SrpRules.CodeInputRequired, SrpRules.Validate(Inputs() with { Sections = [] })!.Value.Code);

        var zeroSection = Inputs() with { Sections = [new OgRodSection(1.0m, 0m)] };

        Assert.Equal(SrpRules.CodeStringIncomplete, SrpRules.Validate(zeroSection)!.Value.Code);

        Assert.Null(SrpRules.Validate(Inputs()));
    }

    [Fact]
    public async Task Commands_need_a_connection_and_an_actor_and_refuse_an_unsafe_string_at_the_door()
    {
        var service = new OgSuckerRodService(() => null, new Quiet());
        var result = OgSuckerRodService.Build(Inputs());

        Assert.Equal(SrpRules.CodeNoConnection, (await service.LoadAsync()).Reference);
        Assert.Equal(SrpRules.CodeActorRequired, (await service.SaveAsync(result, " ")).Reference);
        Assert.Equal(SrpRules.CodeNoConnection, (await service.SaveAsync(result, "sara")).Reference);
        Assert.Equal(SrpRules.CodeNothingPending, (await service.RetrySaveAsync("sara")).Reference);
        Assert.Null(service.Pending);

        // An unsafe result is refused even if a caller builds one, and no connection is needed to say so.
        var unsafeResult = result with { MinSafetyFactor = 1.0m };

        Assert.Equal(SrpRules.CodeUnsafeString, (await service.SaveAsync(unsafeResult, "sara")).Reference);

        var empty = result with { Sections = [] };

        Assert.Equal(SrpRules.CodeNothingToSave, (await service.SaveAsync(empty, "sara")).Reference);
    }

    // ------------------------------------------------------------------ helpers

    private static OgSuckerRodService Service() => new(() => null, new Quiet());

    private static IReadOnlyList<OgRodSection> Taper() =>
    [
        new OgRodSection(1.0m, 2500m),
        new OgRodSection(0.875m, 1500m),
        new OgRodSection(0.75m, 1000m),
    ];

    private static OgSrpInputs Inputs() =>
        new(
            "WELL-1",
            PumpDiameterIn: 2.0m,
            StrokeLengthIn: 48m,
            StrokesPerMinute: 10m,
            PumpDepthFt: 5000m,
            FluidLevelFt: 4800m,
            TubingDiameterIn: 2.875m,
            OilGravity: 35m,
            WaterCut: 0.3m,
            GasOilRatio: 300m,
            GasSpecificGravity: 0.65m,
            RodDensity: null,
            AcceptDefaultDensity: true,
            Grade: SuckerRodConstants.RodGradeD,
            Sections: Taper());

    /// <summary>The same well with thin rods: the string's safety factor falls under the module's minimum.</summary>
    private static OgSrpInputs Thin() =>
        Inputs() with
        {
            Sections =
            [
                new OgRodSection(0.625m, 2500m),
                new OgRodSection(0.625m, 1500m),
                new OgRodSection(0.625m, 1000m),
            ],
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
