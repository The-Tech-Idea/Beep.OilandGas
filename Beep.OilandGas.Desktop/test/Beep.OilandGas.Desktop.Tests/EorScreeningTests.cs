using Beep.OilandGas.EnhancedRecovery.Constants;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Reservoir;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// RES-04: the deciding rule, insufficient data against ineligible, the constants coming from the owner, the economics
/// honesty and the refusal paths - the parts that do not need a database. The saved scheme is proven in the gateway test.
/// </summary>
public sealed class EorScreeningTests
{
    private const string PageFileName = "EorScreeningPageView.cs";

    [Fact]
    public async Task Every_method_states_the_rule_that_decided_it()
    {
        // AC-1.
        var screened = await Service().ScreenAsync(Inputs());

        Assert.True(screened.Success, screened.Message);

        var candidates = screened.Value!.Candidates;

        Assert.Equal(EorRules.Methods.Count, candidates.Count);
        Assert.All(candidates, candidate => Assert.False(string.IsNullOrWhiteSpace(candidate.DecidingRule)));

        // The rule names the state it produced and the window it applied, so it cannot be a bare label.
        foreach (var candidate in candidates)
        {
            Assert.Contains(candidate.StateText, candidate.DecidingRule, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(EorRules.WindowOf(candidate.Method).Basis, candidate.DecidingRule, StringComparison.Ordinal);
        }

        // A light, deep oil: the miscible methods pass and the thermal ones do not, and each says why.
        var co2 = candidates.Single(candidate => candidate.Method == EorMethodConstants.CO2Miscible);

        Assert.True(co2.IsEligible, co2.DecidingRule);
        Assert.Contains("API gravity 35", co2.DecidingRule, StringComparison.Ordinal);
        Assert.Contains("depth 8,000 ft", co2.DecidingRule, StringComparison.Ordinal);

        var steam = candidates.Single(candidate => candidate.Method == EorMethodConstants.Steam);

        Assert.True(steam.IsIneligible, steam.DecidingRule);
        Assert.Contains("above the 25 maximum", steam.DecidingRule, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Insufficient_data_is_a_different_answer_from_ineligible()
    {
        // AC-2: with no viscosity the methods that need one cannot be judged - and none of them may be called ineligible.
        var viscosityUnknown = await Service().ScreenAsync(Inputs(viscosity: null));

        Assert.True(viscosityUnknown.Success, viscosityUnknown.Message);

        var candidates = viscosityUnknown.Value!.Candidates;

        Assert.DoesNotContain(candidates, candidate => candidate.IsIneligible);
        Assert.Contains(candidates, candidate => candidate.IsInsufficient);

        var polymer = candidates.Single(candidate => candidate.Method == EorMethodConstants.Polymer);

        Assert.True(polymer.IsInsufficient);
        Assert.Contains("insufficient data", polymer.DecidingRule, StringComparison.Ordinal);
        Assert.Contains("oil viscosity", polymer.DecidingRule, StringComparison.Ordinal);

        // Water flooding needs no property at all, so it is still judged.
        Assert.True(candidates.Single(candidate => candidate.Method == EorMethodConstants.WaterFlood).IsEligible);

        // With nothing at all to judge on, the screen refuses rather than guessing.
        var nothing = await Service().ScreenAsync(Inputs(api: null, viscosity: null));

        Assert.False(nothing.Success);
        Assert.Equal(EorRules.CodeInputsRequired, nothing.Reference);

        // And a counted state: the summary distinguishes the three answers.
        Assert.True(viscosityUnknown.Value.InsufficientCount > 0);
        Assert.Equal(0, viscosityUnknown.Value.IneligibleCount);
    }

    [Fact]
    public void The_thresholds_come_from_the_owner_and_not_from_this_feature()
    {
        // AC-3: every method has a window in EnhancedRecoveryConstants, and the screen reads it rather than holding one.
        foreach (var method in EorRules.Methods)
        {
            Assert.True(
                EnhancedRecoveryConstants.ScreeningWindows.ContainsKey(method),
                $"{method} has no window in EnhancedRecoveryConstants");

            var window = EorRules.WindowOf(method);

            Assert.Equal(EnhancedRecoveryConstants.ScreeningWindows[method], window);
        }

        // The recovery factors are the module's own screening defaults.
        Assert.Equal(EnhancedRecoveryConstants.RecoveryFactorWaterFlood, EorRules.RecoveryFactorOf(EorMethodConstants.WaterFlood));
        Assert.Equal(EnhancedRecoveryConstants.RecoveryFactorCO2Miscible, EorRules.RecoveryFactorOf(EorMethodConstants.CO2Miscible));
        Assert.Equal(EnhancedRecoveryConstants.RecoveryFactorThermal, EorRules.RecoveryFactorOf(EorMethodConstants.Steam));

        // And the page holds none of those numbers as a literal.
        var page = File.ReadAllText(SourceRoot.AllSources().Single(file => Path.GetFileName(file) == PageFileName));

        foreach (var literal in new[] { "22m", "600m", "2500m", "3300m", "4000m", "35m", "150m", "200000m", "5000m" })
        {
            Assert.DoesNotContain(literal, page, StringComparison.Ordinal);
        }

        Assert.DoesNotContain("EnhancedRecoveryConstants", page, StringComparison.Ordinal);
    }

    [Fact]
    public async Task The_economics_panel_never_invents_a_value()
    {
        // AC-4.
        var screened = await Service().ScreenAsync(Inputs());

        Assert.True(screened.Success);

        var workspace = screened.Value!;

        Assert.False(workspace.EconomicsAvailable);
        Assert.Contains("ECO-01", workspace.EconomicsText, StringComparison.Ordinal);
        Assert.Contains("not shown rather than estimated", workspace.EconomicsText, StringComparison.Ordinal);

        // The chemical methods carry the module's chemical cost with its basis spelled out.
        var polymer = workspace.Candidates.Single(candidate => candidate.Method == EorMethodConstants.Polymer);

        Assert.Equal((decimal)ChemicalTypeConstants.CostPolymer, polymer.CostValue);
        Assert.Contains("not a cost per incremental barrel", polymer.CostText, StringComparison.Ordinal);

        // A method the module states no cost for says so instead of showing a number.
        var steam = workspace.Candidates.Single(candidate => candidate.Method == EorMethodConstants.Steam);

        Assert.Null(steam.CostValue);
        Assert.StartsWith("unavailable", steam.CostText, StringComparison.Ordinal);

        // The recovery figure is labelled as a screening default, never as a forecast.
        Assert.Contains("screening default", polymer.RecoveryText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task The_co2_depth_rule_follows_the_owners_gravity_bands()
    {
        // 30 API needs 3,300 ft in the owner's depth-versus-gravity table.
        var shallow = await Service().ScreenAsync(Inputs(api: 30m, viscosity: 1m, depth: 2000m));

        Assert.True(shallow.Success);

        var co2 = shallow.Value!.Candidates.Single(candidate => candidate.Method == EorMethodConstants.CO2Miscible);

        Assert.True(co2.IsIneligible, co2.DecidingRule);
        Assert.Contains("3,300 ft", co2.DecidingRule, StringComparison.Ordinal);

        // At the right depth for the same oil it passes.
        var deep = await Service().ScreenAsync(Inputs(api: 30m, viscosity: 1m, depth: 3400m));

        Assert.True(deep.Value!.Candidates.Single(candidate => candidate.Method == EorMethodConstants.CO2Miscible).IsEligible);

        // Below 22 API the method fails whatever the depth, and the rule says so.
        var tooHeavy = await Service().ScreenAsync(Inputs(api: 18m, viscosity: 1m, depth: 9000m));

        var heavyCo2 = tooHeavy.Value!.Candidates.Single(candidate => candidate.Method == EorMethodConstants.CO2Miscible);

        Assert.True(heavyCo2.IsIneligible);
        Assert.Contains("below the 22", heavyCo2.DecidingRule, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_heavy_oil_reverses_the_answer_for_the_thermal_methods()
    {
        var heavy = await Service().ScreenAsync(Inputs(api: 12m, viscosity: 3000m, depth: 2000m));

        Assert.True(heavy.Success);

        var candidates = heavy.Value!.Candidates;

        Assert.True(candidates.Single(candidate => candidate.Method == EorMethodConstants.Steam).IsEligible);
        Assert.True(candidates.Single(candidate => candidate.Method == EorMethodConstants.Thermal).IsEligible);
        Assert.True(candidates.Single(candidate => candidate.Method == EorMethodConstants.CO2Miscible).IsIneligible);
        Assert.True(candidates.Single(candidate => candidate.Method == EorMethodConstants.HydrocarbonMiscible).IsIneligible);
    }

    [Fact]
    public async Task The_best_eligible_method_is_the_highest_scored_one()
    {
        var screened = await Service().ScreenAsync(Inputs());

        Assert.True(screened.Success);

        var eligible = screened.Value!.Candidates.Where(candidate => candidate.IsEligible).ToList();

        Assert.NotEmpty(eligible);
        Assert.NotNull(screened.Value.Best);
        Assert.True(screened.Value.Best!.IsEligible);
        Assert.Equal(eligible.Max(candidate => candidate.Score), screened.Value.Best.Score);

        // The score is a relative margin inside the window, so it is always between 0 and 1.
        // Only the methods with a numeric window are scored; water flooding has none and says so.
        Assert.All(
            eligible.Where(candidate => candidate.Score is not null),
            candidate => Assert.InRange(candidate.Score!.Value, 0m, 1m));

        var unscored = screened.Value.Candidates.First(candidate => candidate.Score is null);

        Assert.True(unscored.IsEligible);
        Assert.Equal("not scored", unscored.ScoreText);
    }

    [Fact]
    public async Task The_same_inputs_produce_the_same_screen()
    {
        var service = Service();

        var first = await service.ScreenAsync(Inputs());
        var second = await service.ScreenAsync(Inputs());

        Assert.Equal(first.Value!.CandidateText, second.Value!.CandidateText);
        Assert.Equal(first.Value.Summary, second.Value.Summary);
    }

    [Fact]
    public async Task A_scheme_needs_an_actor_a_candidate_and_an_eligible_method()
    {
        var service = Service();
        var screened = (await service.ScreenAsync(Inputs())).Value!;

        Assert.Equal(
            EorRules.CodeActorRequired,
            (await service.SaveAsync(screened, EorMethodConstants.CO2Miscible, " ")).Reference);

        var empty = screened with { Candidates = [] };

        Assert.Equal(
            EorRules.CodeNothingToSave,
            (await service.SaveAsync(empty, null, "sara")).Reference);

        Assert.Equal(
            EorRules.CodeChosenUnknown,
            (await service.SaveAsync(screened, "MAGIC", "sara")).Reference);

        // An ineligible method is refused as a scheme, with the rule that rejected it.
        var refused = await service.SaveAsync(screened, EorMethodConstants.Steam, "sara");

        Assert.False(refused.Success);
        Assert.Equal(EorRules.CodeChosenIneligible, refused.Reference);
        Assert.Contains("ineligible", refused.Message!, StringComparison.OrdinalIgnoreCase);

        // With no connection the eligible case cannot be written, and says so rather than pretending.
        Assert.Equal(
            EorRules.CodeNoConnection,
            (await service.SaveAsync(screened, EorMethodConstants.CO2Miscible, "sara")).Reference);
    }

    [Fact]
    public void The_screen_states_which_methods_it_covers()
    {
        Assert.Equal(9, EorRules.Methods.Count);
        Assert.Contains(EorMethodConstants.CO2Miscible, EorRules.Methods);
        Assert.Equal("CO2 miscible", EorRules.NameOf(EorMethodConstants.CO2Miscible));
        Assert.Contains("water", EorRules.NameOf(EorMethodConstants.WaterFlood), StringComparison.OrdinalIgnoreCase);
    }

    private static OgEorScreeningService Service() => new(() => null, new Quiet());

    private static OgEorInputs Inputs(
        decimal? api = 35m,
        decimal? viscosity = 2m,
        decimal? depth = 8000m) =>
        new("F-1", "P-1", api, viscosity, depth, 180m);

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
