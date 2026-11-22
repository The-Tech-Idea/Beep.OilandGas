using TheTechIdea.Beep.OilandGas.Desktop.Data.Reservoir;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// RES-02: the reference case, the out-of-range marking, the gas-gravity rule, the bubble-point consistency rule, the
/// screening banner and the validation - the parts that do not need a database. The saved result is proven in the
/// gateway test.
/// </summary>
public sealed class OilPropertiesTests
{
    /// <summary>
    /// The reference case: 35 API oil, gas gravity 0.65, 200 degF, Rs 600 scf/STB, at 3000 psia. The expected values
    /// are worked out by hand from the published relations the engine implements:
    /// <list type="bullet">
    /// <item>Pb = 18.2 [(Rs/Gg)^0.83 10^(0.00091 T - 0.0125 API) - 1.4] = <b>2896.78 psia</b></item>
    /// <item>Rs(P) = Gg [((P/18.2) + 1.4) 10^(0.0125 API - 0.00091 T)]^1.2048 = <b>625.56 scf/STB</b></item>
    /// <item>Bo = 0.9759 + 0.00012 [Rs (Gg/Go)^0.5 + 1.25 T]^1.2 = <b>1.32759 rb/STB</b>, Go = 141.5/166.5</item>
    /// <item>dead viscosity (Beggs-Robinson) = <b>1.7854 cp</b></item>
    /// <item>saturated viscosity = A mu_od^B = <b>0.5139 cp</b></item>
    /// </list>
    /// The tolerance is half a percent, which is the precision the published correlations themselves carry.
    /// </summary>
    private const decimal ReferencePb = 2896.7799m;
    private const decimal ReferenceRsAtPressure = 625.5567m;
    private const decimal ReferenceBo = 1.327592m;
    private const decimal ReferenceDeadViscosity = 1.7854m;
    private const decimal ReferenceSaturatedViscosity = 0.5139m;
    private const decimal Tolerance = 0.005m;

    [Fact]
    public async Task A_reference_case_reproduces_Pb_Rs_and_Bo()
    {
        var run = await Service().RunAsync(Inputs());

        Assert.True(run.Success, run.Message);

        var result = run.Value!;

        Assert.False(result.IsFailed, result.FailureText);

        Within(ReferencePb, result.BubblePoint!.Value);
        Within(ReferenceRsAtPressure, result.SolutionGor!.Value);
        Within(ReferenceBo, result.FormationVolumeFactor!.Value);
        Within(ReferenceDeadViscosity, result.DeadOilViscosity!.Value);
        Within(ReferenceSaturatedViscosity, result.SaturatedViscosity!.Value);

        // A 35 API oil of 0.85 specific gravity gives about 40 lb/ft3 at Bo 1.33.
        Assert.InRange(result.Density!.Value, 35m, 45m);
        Assert.Contains("Pb 2,896.78 psia", result.Describe(), StringComparison.Ordinal);
        Assert.Contains("rb/STB", result.FormationVolumeFactorText, StringComparison.Ordinal);
        Assert.False(result.OutsideRange);
    }

    [Fact]
    public async Task The_same_physical_case_in_Rankine_gives_the_same_answer()
    {
        var service = Service();

        var fahrenheit = await service.RunAsync(Inputs(temperature: 200m, unit: OilUnits.Fahrenheit));
        var rankine = await service.RunAsync(Inputs(temperature: OilUnits.ToFahrenheit(659.67m, OilUnits.Rankine), unit: OilUnits.Fahrenheit));

        Assert.True(fahrenheit.Success && rankine.Success);
        Assert.Equal(fahrenheit.Value!.BubblePoint, rankine.Value!.BubblePoint);

        // And the screen accepts a Rankine input directly, converting it once.
        var fromRankine = await service.RunAsync(Inputs(temperature: 659.67m, unit: OilUnits.Rankine));

        Assert.True(fromRankine.Success, fromRankine.Message);
        Assert.Equal(200m, fromRankine.Value!.TemperatureF is { } t ? Math.Round(t, 2) : 0m);
        Within(ReferencePb, fromRankine.Value.BubblePoint!.Value);
    }

    [Fact]
    public async Task A_result_outside_the_correlation_range_is_marked_and_still_returned()
    {
        // Rs 4000 pushes both the bubble point above the correlation's maximum and Bo above 3.0.
        var run = await Service().RunAsync(Inputs(api: 40m, gravity: 0.8m, temperature: 250m, pressure: 12000m, gor: 4000m));

        Assert.True(run.Success, run.Message);

        var result = run.Value!;

        Assert.False(result.IsFailed);
        Assert.True(result.OutsideRange);
        Assert.True(result.BubblePoint > 10000m);
        Assert.NotEmpty(result.RangeNotes);

        Assert.Contains("bubble point", result.RangeText, StringComparison.Ordinal);
        Assert.Contains("outside", result.RangeText, StringComparison.Ordinal);

        // The result is still a result: every value is present, which is what "marked" means here.
        Assert.NotNull(result.FormationVolumeFactor);
        Assert.NotNull(result.SaturatedViscosity);
    }

    [Fact]
    public async Task A_gas_gravity_is_never_applied_silently()
    {
        var missing = Inputs() with { GasSpecificGravity = null, UseDefaultGasGravity = false };

        // AC-3: it is a missing input, not a default.
        Assert.Contains(
            "gas specific gravity (or an explicit acceptance of the default)",
            OilPropertyRules.Missing(missing));

        Assert.Equal(
            OilPropertyRules.CodeGasGravityRequired,
            OilPropertyRules.Validate(missing)!.Value.Code);

        var refused = await Service().RunAsync(missing);

        Assert.False(refused.Success);
        Assert.Equal(OilPropertyRules.CodeGasGravityRequired, refused.Reference);

        // Accepted explicitly: it runs, and the result says the default was used.
        var accepted = await Service().RunAsync(
            missing with { UseDefaultGasGravity = true });

        Assert.True(accepted.Success, accepted.Message);
        Assert.True(accepted.Value!.GasGravityAssumed);
        Assert.Equal(0.65m, accepted.Value.GasSpecificGravity);
        Assert.Contains("the default, accepted explicitly", accepted.Value.GasGravityText, StringComparison.Ordinal);
        Assert.Contains("default", accepted.Value.Describe(), StringComparison.Ordinal);

        // An explicit value is not "assumed", even with the box ticked.
        var typed = await Service().RunAsync(Inputs(gravity: 0.7m) with { UseDefaultGasGravity = true });

        Assert.True(typed.Success, typed.Message);
        Assert.False(typed.Value!.GasGravityAssumed);
        Assert.Equal(0.7m, typed.Value.GasSpecificGravity);
    }

    [Fact]
    public async Task A_pressure_below_the_bubble_point_is_refused_with_both_numbers()
    {
        // Rs 600 at 35 API and 200 degF bubbles at about 2897 psia, so 1000 psia is undersaturated.
        var run = await Service().RunAsync(Inputs(pressure: 1000m));

        Assert.False(run.Success);
        Assert.Equal(OilPropertyRules.CodeBelowBubblePoint, run.Reference);
        Assert.Contains("1,000 psia is below the bubble point 2,896.78 psia", run.Message!, StringComparison.Ordinal);
        Assert.Contains("do not apply", run.Message!, StringComparison.Ordinal);

        // At or above the bubble point it runs.
        Assert.True((await Service().RunAsync(Inputs(pressure: 2897m))).Success);
    }

    [Fact]
    public void The_screening_banner_is_a_sentence_the_page_can_show_in_every_state()
    {
        // AC-4.
        Assert.Contains("not a PVT report", OilPropertyRules.ScreeningBanner, StringComparison.Ordinal);
        Assert.Contains("does not replace", OilPropertyRules.ScreeningBanner, StringComparison.Ordinal);
    }

    [Fact]
    public void The_page_writes_the_banner_once_and_never_clears_it()
    {
        // AC-4, as a guard: the banner text may be assigned only in the constructor and only from the rule's constant,
        // so no success, refusal or failure branch can blank it.
        var page = SourceRoot.AllSources()
            .Single(file => Path.GetFileName(file) == "OilPropertiesPageView.cs");

        var text = File.ReadAllText(page);

        // The banner is created once, in the constructor, from the rule's constant ...
        Assert.Equal(
            1,
            System.Text.RegularExpressions.Regex.Matches(
                text, @"_banner\s*=\s*Body\(OilPropertyRules\.ScreeningBanner\)").Count);

        // ... and no branch of the page ever writes to it again, so a failure cannot blank it.
        Assert.DoesNotContain("_banner.Text =", text, StringComparison.Ordinal);
        Assert.Contains("public string ScreeningBannerText => _banner.Text;", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Missing_inputs_are_named_and_the_correlation_is_stated_once()
    {
        var empty = new OgOilPropertyInputs(null, null, false, null, null, null, null, null);

        Assert.Equal(
            [
                "API gravity",
                "gas specific gravity (or an explicit acceptance of the default)",
                "temperature",
                "temperature unit",
                "pressure",
                "solution gas-oil ratio",
            ],
            OilPropertyRules.Missing(empty));

        Assert.Equal(OilPropertyRules.CodeApiRequired, OilPropertyRules.Validate(empty)!.Value.Code);

        Assert.Equal(
            OilPropertyRules.CodeApiInvalid,
            OilPropertyRules.Validate(Inputs(api: 120m))!.Value.Code);

        Assert.Equal(
            OilPropertyRules.CodeTemperatureUnitInvalid,
            OilPropertyRules.Validate(Inputs(unit: "K"))!.Value.Code);

        Assert.Equal(
            OilPropertyRules.CodeCorrelationUnknown,
            OilPropertyRules.Validate(Inputs() with { Correlation = "STANDING" })!.Value.Code);

        // The section-9 open question: the name comes from the shared type, not from a literal in the view.
        Assert.Equal("STANDING_BEGGS", OilCorrelations.Combined);
        Assert.Contains("Beggs-Robinson", OilCorrelations.Viscosity, StringComparison.Ordinal);
        Assert.Contains("Standing", OilCorrelations.BubblePointAndGor, StringComparison.Ordinal);
    }

    [Fact]
    public async Task The_same_inputs_produce_the_same_result()
    {
        var service = Service();

        var first = await service.RunAsync(Inputs());
        var second = await service.RunAsync(Inputs());

        Assert.Equal(first.Value!.Describe(), second.Value!.Describe());
        Assert.Equal(first.Value.BubblePoint, second.Value.BubblePoint);
        Assert.Equal(first.Value.FormationVolumeFactor, second.Value.FormationVolumeFactor);
    }

    [Fact]
    public async Task Saving_needs_an_actor_and_a_completed_run_but_the_screening_needs_no_database()
    {
        var service = Service();

        var result = (await service.RunAsync(Inputs())).Value!;

        Assert.Equal(
            OilPropertyRules.CodeActorRequired,
            (await service.SaveAsync(result, " ")).Reference);

        Assert.Equal(
            OilPropertyRules.CodeNotRun,
            (await service.SaveAsync(result with { BubblePoint = null }, "sara")).Reference);

        Assert.Equal(
            OilPropertyRules.CodeNoConnection,
            (await service.LoadAsync()).Reference);
    }

    private static OgOilPropertiesService Service() => new(() => null, new Quiet());

    private static void Within(decimal expected, decimal actual)
    {
        var low = expected * (1m - Tolerance);
        var high = expected * (1m + Tolerance);

        Assert.InRange(actual, low, high);
    }

    private static OgOilPropertyInputs Inputs(
        decimal? api = 35m,
        decimal? gravity = 0.65m,
        decimal? temperature = 200m,
        string? unit = OilUnits.Fahrenheit,
        decimal? pressure = 3000m,
        decimal? gor = 600m) =>
        new(api, gravity, false, temperature, unit, pressure, gor, OilCorrelations.Combined);

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
