using TheTechIdea.Beep.OilandGas.Desktop.Data.Reservoir;
using Beep.OilandGas.Models.Data.FlashCalculations;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// RES-03: the reference flash, the convergence rule, the feed-sum rule, the EOS mapping, the envelope exposure and
/// determinism - the parts that do not need a database. The saved run is proven in the gateway test.
/// </summary>
public sealed class FlashTests
{
    /// <summary>
    /// The reference case: C1 0.50, C2 0.20, C3 0.15, nC4 0.15 at 500 psia and 600 R. Wilson K-values
    /// (K = Pc/P exp[5.37 (1 + w)(1 - Tc/T)]) are 13.646078, 2.319017, 0.625939 and 0.186554; solving
    /// sum z(K-1)/(1+beta(K-1)) = 0 for beta gives <b>0.94143858</b>, with sum x = sum y = 1. Those numbers are worked
    /// out by hand from the published relations, not taken from the engine.
    /// </summary>
    private const decimal ReferenceVapourFraction = 0.94143858m;

    private static readonly decimal[] ReferenceK = [13.646078m, 2.319017m, 0.625939m, 0.186554m];

    [Fact]
    public async Task A_reference_flash_converges_and_matches_the_expected_vapour_fraction()
    {
        // AC-1.
        var run = await Service().RunAsync(Inputs());

        Assert.True(run.Success, run.Message);

        var result = run.Value!;

        Assert.False(result.IsFailed, result.FailureText);
        Assert.True(result.Converged);
        Assert.Equal(FlashStatusCodes.Success, result.StatusCode);
        Assert.Equal(FlashPhaseStates.TwoPhase, result.PhaseState);

        Assert.InRange(result.VapourFraction, ReferenceVapourFraction - 0.00001m, ReferenceVapourFraction + 0.00001m);
        Assert.InRange(result.LiquidFraction, 1m - ReferenceVapourFraction - 0.00001m,
            1m - ReferenceVapourFraction + 0.00001m);

        // The K-values the engine used are the Wilson values worked out by hand.
        Assert.Equal(4, result.Components.Count);

        for (var index = 0; index < 4; index++)
        {
            Assert.Equal(ReferenceK[index], result.Components[index].K, 4);
        }

        // The phase split satisfies its own material balance and each phase sums to one.
        Assert.InRange(result.Components.Sum(component => component.X), 0.99999m, 1.00001m);
        Assert.InRange(result.Components.Sum(component => component.Y), 0.99999m, 1.00001m);

        foreach (var component in result.Components)
        {
            var balanced = (1m - result.VapourFraction) * component.X + result.VapourFraction * component.Y;

            Assert.InRange(balanced, component.Z - 0.00001m, component.Z + 0.00001m);
            Assert.Equal(component.Y, component.K * component.X, 5);
        }

        Assert.True(result.Iterations > 0);
        Assert.Contains("two-phase", result.Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_run_the_solver_did_not_converge_is_failed_with_its_iteration_count()
    {
        // AC-2. The engine's own solver is robust on every valid feed tried, so the rule under test is the mapping from
        // the engine's convergence flag to what the screen shows - which is where the honesty lives.
        var engine = new FlashResult
        {
            Converged = false,
            Iterations = 50,
            VaporFraction = 0.55m,
            LiquidFraction = 0.45m,
            Status = "CONVERGENCE_FAILED",
            KValues =
            [
                new FlashComponentKValue { ComponentName = "C1", KValue = 9m },
            ],
            VaporComposition = [new FlashComponentFraction { ComponentName = "C1", MoleFraction = 0.9m }],
            LiquidComposition = [new FlashComponentFraction { ComponentName = "C1", MoleFraction = 0.1m }],
        };

        var run = OgFlashService.FromWilson(engine, Inputs(), "IDEAL_K", []);

        Assert.True(run.IsFailed);
        Assert.False(run.Converged);
        Assert.Equal(FlashStatusCodes.ConvergenceFailed, run.StatusCode);
        Assert.Equal(50, run.Iterations);
        Assert.Contains("50 iteration(s)", run.FailureText, StringComparison.Ordinal);
        Assert.Equal("not computed", run.VapourFractionText);
        Assert.Contains("no composition is shown", run.CompositionText, StringComparison.Ordinal);
        Assert.Empty(run.Components);
        Assert.StartsWith("FAILED", run.Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task The_mole_fractions_must_sum_to_one_within_the_stated_tolerance()
    {
        // AC-3.
        var offBy = new OgFlashInputs(
            [Component("C1", 0.50m), Component("C2", 0.40m)],
            500m, 600m, "IDEAL_K");

        Assert.Equal(
            FlashRules.CodeFeedSum,
            FlashRules.Validate(offBy, FlashRules.MappingEosCodes)!.Value.Code);

        var refused = await Service().RunAsync(offBy);

        Assert.False(refused.Success);
        Assert.Equal(FlashRules.CodeFeedSum, refused.Reference);
        Assert.Contains("sum to 0.9", refused.Message!, StringComparison.Ordinal);

        // Within the engine's own tolerance it is accepted.
        var nearly = new OgFlashInputs(
            [Component("C1", 0.50005m), Component("C2", 0.49995m)],
            500m, 600m, "IDEAL_K");

        Assert.Null(FlashRules.Validate(nearly, FlashRules.MappingEosCodes));
        Assert.Equal(0.0001m, FlashRules.FeedSumTolerance);
    }

    [Fact]
    public void The_equation_of_state_codes_come_from_the_mapping_not_the_view()
    {
        // AC-4: the owner's mapping is the single source, including its documented fallback.
        Assert.Equal("PR", FlashRules.EosCode("PR"));
        Assert.Equal("PR", FlashRules.EosCode("Peng-Robinson"));
        Assert.Equal("SRK", FlashRules.EosCode("Soave-Redlich-Kwong"));
        Assert.Equal("SRK_MODIFIED", FlashRules.EosCode("SRK modified alpha"));
        Assert.Equal("IDEAL_K", FlashRules.EosCode("ideal k-value"));
        Assert.Equal("IDEAL_K", FlashRules.EosCode(null));
        Assert.Equal("IDEAL_K", FlashRules.EosCode("something else"));

        // The engine's cubic path has no modified-SRK alpha function, so the modifier is refused rather than run as SRK.
        var refused = FlashRules.Validate(
            Inputs() with { EosCode = "SRK_MODIFIED" }, FlashRules.MappingEosCodes);

        Assert.Equal(FlashRules.CodeEosNotImplemented, refused!.Value.Code);
        Assert.Contains("AdvancedEOS.EosType", refused.Value.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_single_phase_answer_is_a_result_and_not_a_failure()
    {
        // Well above the bubble point the solver answers "all liquid" without iterating; that is a real answer.
        var run = await Service().RunAsync(Inputs() with { Pressure = 6000m });

        Assert.True(run.Success, run.Message);
        Assert.False(run.Value!.IsFailed);
        Assert.Equal(0m, run.Value.VapourFraction);
        Assert.Equal(FlashPhaseStates.AllLiquid, run.Value.PhaseState);
        Assert.Equal(0, run.Value.Iterations);
        Assert.Contains("all liquid", run.Value.Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task The_envelope_exposes_only_the_points_that_were_computed()
    {
        // AC-5.
        var envelope = await Service().EnvelopeAsync(Inputs(), 500m, 900m, 8);

        Assert.True(envelope.Success, envelope.Message);

        var value = envelope.Value!;

        Assert.NotEmpty(value.Points);

        // Every exposed point is a computed point: a positive pressure and a positive temperature.
        Assert.All(value.Points, point =>
        {
            Assert.True(point.PressurePsia > 0m);
            Assert.True(point.TemperatureR > 0m);
            Assert.Contains(point.Type.ToLowerInvariant(), new[] { "bubble", "dew" });
        });

        // What was not computed is counted and stated, never drawn.
        if (value.Partial)
        {
            Assert.True(value.Skipped > 0);
            Assert.Contains("not shown", value.LimitText, StringComparison.Ordinal);
            Assert.Contains("stops at the points the solver produced", value.LimitText, StringComparison.Ordinal);
        }
        else
        {
            Assert.Equal(0, value.Skipped);
            Assert.Equal("every requested point of the envelope was computed", value.LimitText);
        }

        Assert.Contains("bubble", value.Text, StringComparison.Ordinal);

        // A range with no width is refused rather than drawn.
        var degenerate = await Service().EnvelopeAsync(Inputs(), 900m, 500m, 8);

        Assert.False(degenerate.Success);
        Assert.Equal(FlashRules.CodeEnvelopeEmpty, degenerate.Reference);
    }

    [Fact]
    public async Task The_same_inputs_produce_the_same_output()
    {
        // AC-6.
        var service = Service();

        var first = await service.RunAsync(Inputs());
        var second = await service.RunAsync(Inputs());

        Assert.Equal(first.Value!.Describe(), second.Value!.Describe());
        Assert.Equal(first.Value.VapourFraction, second.Value.VapourFraction);

        var envelopeOne = await service.EnvelopeAsync(Inputs(), 500m, 900m, 6);
        var envelopeTwo = await service.EnvelopeAsync(Inputs(), 500m, 900m, 6);

        Assert.Equal(envelopeOne.Value!.Text, envelopeTwo.Value!.Text);
    }

    [Fact]
    public void The_feed_editor_reads_one_component_per_line()
    {
        Assert.True(FlashFeedText.TryParse(FlashFeedText.DefaultText, out var components, out var error));
        Assert.Null(error);
        Assert.Equal(4, components.Count);
        Assert.Equal("C1", components[0].Name);
        Assert.Equal(0.50m, components[0].MoleFraction);
        Assert.Equal(343.0m, components[0].CriticalTemperature);
        Assert.Equal(0.0104m, components[0].AcentricFactor);
        Assert.Equal(16.04m, components[0].MolecularWeight);

        Assert.False(FlashFeedText.TryParse("C1, 0.5, 343, 667, 0.01, 16\nC2, 0.5, 549.8", out var bad, out var badError));
        Assert.Empty(bad);
        Assert.Contains("Feed line 2 has 3 field(s)", badError!, StringComparison.Ordinal);

        Assert.False(FlashFeedText.TryParse("C1, x, 343, 667, 0.01, 16", out _, out var numberError));
        Assert.Contains("not a number", numberError!, StringComparison.Ordinal);

        Assert.False(FlashFeedText.TryParse("   \n# only a comment", out _, out var emptyError));
        Assert.Contains("feed is empty", emptyError!, StringComparison.Ordinal);
    }

    [Fact]
    public void A_run_needs_components_pressure_temperature_and_a_known_eos()
    {
        var none = new OgFlashInputs([], 500m, 600m, "IDEAL_K");

        Assert.Equal(FlashRules.CodeComponentsRequired, FlashRules.Validate(none, FlashRules.MappingEosCodes)!.Value.Code);

        Assert.Equal(
            FlashRules.CodeFractionInvalid,
            FlashRules.Validate(
                Inputs() with { Components = [Component("C1", 0m)] }, FlashRules.MappingEosCodes)!.Value.Code);

        Assert.Equal(
            FlashRules.CodePropertiesRequired,
            FlashRules.Validate(
                Inputs() with { Components = [Component("C1", 1m) with { MolecularWeight = 0m }] },
                FlashRules.MappingEosCodes)!.Value.Code);

        Assert.Equal(
            FlashRules.CodePressureRequired,
            FlashRules.Validate(Inputs() with { Pressure = 0m }, FlashRules.MappingEosCodes)!.Value.Code);

        Assert.Equal(
            FlashRules.CodeTemperatureRequired,
            FlashRules.Validate(Inputs() with { Temperature = null }, FlashRules.MappingEosCodes)!.Value.Code);

        Assert.Equal(
            FlashRules.CodeEosUnknown,
            FlashRules.Validate(Inputs() with { EosCode = "MAGIC" }, ["PR", "SRK"])!.Value.Code);
    }

    [Fact]
    public async Task Saving_needs_an_actor_and_a_converged_run_but_the_flash_needs_no_database()
    {
        var service = Service();

        var result = (await service.RunAsync(Inputs())).Value!;

        Assert.Equal(
            FlashRules.CodeActorRequired,
            (await service.SaveAsync(result, " ")).Reference);

        Assert.Equal(
            FlashRules.CodeNoConnection,
            (await service.LoadAsync()).Reference);
    }

    private static OgFlashService Service() => new(() => null, new Quiet());

    private static OgFlashComponent Component(string name, decimal fraction) =>
        new(name, fraction, 343.0m, 667.8m, 0.0104m, 16.04m);

    private static OgFlashInputs Inputs() =>
        new(
            [
                new OgFlashComponent("C1", 0.50m, 343.0m, 667.8m, 0.0104m, 16.04m),
                new OgFlashComponent("C2", 0.20m, 549.8m, 707.8m, 0.0986m, 30.07m),
                new OgFlashComponent("C3", 0.15m, 665.7m, 616.3m, 0.1524m, 44.10m),
                new OgFlashComponent("NC4", 0.15m, 765.3m, 550.7m, 0.2002m, 58.12m),
            ],
            500m,
            600m,
            "IDEAL_K");

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
