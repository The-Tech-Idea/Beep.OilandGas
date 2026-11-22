using Beep.OilandGas.DCA.AdvancedDeclineMethods;
using Beep.OilandGas.ProductionForecasting.Constants;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Reservoir;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// RES-06: the reference fit, the b bounds, the exclusions, the cross-check, the no-curve rule and determinism - the parts
/// that do not need a database. The PPDM reads and the saved fit are proven in the gateway test.
/// </summary>
public sealed class DeclineCurveTests
{
    private static readonly DateTime Start = new(2024, 1, 1);

    [Fact]
    public void A_synthetic_history_with_known_parameters_is_recovered()
    {
        // AC-1: qi 1000, Di 0.01/day, b 0.5, monthly for two years. The engine must return those parameters.
        var model = Model(Hyperbolic(1000.0, 0.01, 0.5));

        var fit = Service().Fit(model, Inputs());

        Assert.False(fit.IsRefused, fit.RefusalText);
        Assert.True(fit.HasCurve);
        Assert.Equal(OgDeclineFit.OkOutcome, fit.Outcome);

        Assert.Equal(1000m, fit.Qi!.Value, 2);
        Assert.Equal(0.01m, fit.DiPerDay!.Value, 5);
        Assert.Equal(0.5m, fit.B!.Value, 4);
        Assert.True(fit.Bounded);
        Assert.Equal(1m, fit.RSquared!.Value, 3);
        Assert.Equal(24, fit.PointsUsed);
        Assert.Equal(0, fit.PointsExcluded);

        // The EUR is the owner's closed form at the limit the caller states.
        Assert.NotNull(fit.Eur);
        Assert.True(fit.Eur > 0m);
        Assert.Contains("no economic limit given", Service().Fit(model, Inputs(limit: null)).EurText);
    }

    [Fact]
    public void An_exponential_history_fits_at_b_zero_and_a_harmonic_one_at_b_one()
    {
        var service = Service();

        var exponential = service.Fit(
            Model(Exponential(1000.0, 0.01)),
            Inputs(model: DeclineModels.Exponential));

        Assert.False(exponential.IsRefused, exponential.RefusalText);
        Assert.Equal(0m, exponential.B!.Value, 6);
        Assert.Equal(0.01m, exponential.DiPerDay!.Value, 5);

        var harmonic = service.Fit(
            Model(Harmonic()),
            Inputs(model: DeclineModels.Harmonic));

        Assert.False(harmonic.IsRefused, harmonic.RefusalText);
        Assert.Equal(1m, harmonic.B!.Value, 6);
        Assert.Equal(0.01m, harmonic.DiPerDay!.Value, 5);
    }

    [Fact]
    public void A_fixed_b_outside_the_bounds_is_refused_and_the_bounds_are_named()
    {
        // AC-2.
        var refusal = DeclineRules.Validate(Inputs() with { FixedB = 1.5m });

        Assert.Equal(DeclineRules.CodeBOutsideBounds, refusal!.Value.Code);
        Assert.Contains("0", refusal.Value.Message, StringComparison.Ordinal);
        Assert.Contains("1", refusal.Value.Message, StringComparison.Ordinal);

        var service = Service();

        Assert.Equal(
            DeclineRules.CodeBOutsideBounds,
            service.AnalyzeAsync(Inputs() with { FixedB = -0.1m }).GetAwaiter().GetResult().Reference);

        // The bounds on offer are the module's own Arps constants.
        Assert.Equal(ForecastAlgorithmConstants.ArpsBdfMinB, DeclineRules.BoundsFor(DeclineModels.Hyperbolic).Minimum);
        Assert.Equal(ForecastAlgorithmConstants.ArpsBdfMaxB, DeclineRules.BoundsFor(DeclineModels.Hyperbolic).Maximum);
        Assert.Equal(
            ForecastAlgorithmConstants.ArpsBdfSoftMaxB,
            DeclineRules.BoundsFor(DeclineModels.ModifiedHyperbolic).Maximum);
    }

    [Fact]
    public void A_history_that_wants_b_above_the_maximum_is_refused_rather_than_reported_at_the_bound()
    {
        // SG-06-6 and the state table's "b outside bounds". A b of 1.5 is outside the physical range, so a fit that
        // settles on the 1.0 bound is the data asking for something the model does not allow.
        var model = Model(RawHyperbolic(1000.0, 0.01, 1.5));

        var fit = Service().Fit(model, Inputs());

        Assert.True(fit.IsRefused);
        Assert.Equal(DeclineRules.CodeFitUnbounded, fit.RefusalCode);
        Assert.Contains("above the configured maximum 1", fit.RefusalMessage!, StringComparison.Ordinal);
        Assert.False(fit.Bounded);

        // AC-5: a refused fit draws no curve and offers no parameters.
        Assert.False(fit.HasCurve);
        Assert.Null(fit.Qi);
        Assert.Contains("no curve is drawn", fit.CurveText, StringComparison.Ordinal);
    }

    [Fact]
    public void Zero_and_negative_rates_are_excluded_with_their_reason_and_counted()
    {
        // AC-3.
        var readings = Hyperbolic(1000.0, 0.01, 0.5).ToList();

        readings[3] = readings[3] with { Rate = 0m };
        readings[7] = readings[7] with { Rate = -25m };

        var fit = Service().Fit(Model(readings), Inputs());

        Assert.False(fit.IsRefused, fit.RefusalText);
        Assert.Equal(2, fit.PointsExcluded);
        Assert.Equal(22, fit.PointsUsed);

        var excluded = fit.Points.Where(point => !point.Used).ToList();

        Assert.Equal(2, excluded.Count);
        Assert.All(excluded, point => Assert.Contains("cannot be fitted on a log scale", point.Reason!, StringComparison.Ordinal));
        Assert.Contains(excluded[0].DateText, fit.ExclusionsText, StringComparison.Ordinal);
        Assert.Contains("2 excluded", fit.PointsText, StringComparison.Ordinal);

        // An excluded point is not in the curve.
        Assert.DoesNotContain(fit.Points.Where(point => !point.Used), point => point.Fitted is not null);
    }

    [Fact]
    public void Too_few_usable_points_is_refused_with_the_minimum_stated()
    {
        var model = Model(Hyperbolic(1000.0, 0.01, 0.5).Take(3).ToList());

        var fit = Service().Fit(model, Inputs());

        Assert.True(fit.IsRefused);
        Assert.Equal(DeclineRules.CodeTooFewPoints, fit.RefusalCode);
        Assert.Contains(
            ForecastAlgorithmConstants.MinHistoryPointsForFit.ToString(),
            fit.RefusalMessage!,
            StringComparison.Ordinal);
        Assert.False(fit.HasCurve);
    }

    [Fact]
    public void The_independent_fitter_agrees_on_the_reference_history()
    {
        // AC-4: the tools' DeclineFitter is a second implementation and is asked the same question.
        var fit = Service().Fit(Model(Hyperbolic(1000.0, 0.01, 0.5)), Inputs());

        Assert.True(fit.CrossCheck.Available, fit.CrossCheck.Text);
        Assert.True(fit.CrossCheck.Agrees, fit.CrossCheck.Text);
        Assert.Contains("agrees", fit.CrossCheck.Text, StringComparison.Ordinal);

        Assert.Equal(1000m, fit.CrossCheck.Qi!.Value, 2);
        Assert.Equal(0.5m, fit.CrossCheck.B!.Value, 4);
        Assert.Equal("agrees", fit.CrossCheck.AgreeText);

        // The tolerance is stated, not implied.
        Assert.Equal(0.05m, DeclineRules.BAgreementTolerance);
        Assert.True(DeclineRules.Agrees(1000m, 0.5m, 1000m, 0.54m));
        Assert.False(DeclineRules.Agrees(1000m, 0.5m, 1000m, 0.56m));
        Assert.False(DeclineRules.Agrees(1000m, 0.5m, 1200m, 0.5m));
    }

    [Fact]
    public void The_same_history_and_model_give_the_same_fit()
    {
        // AC-6.
        var service = Service();

        var first = service.Fit(Model(Hyperbolic(1000.0, 0.01, 0.5)), Inputs());
        var second = service.Fit(Model(Hyperbolic(1000.0, 0.01, 0.5)), Inputs());

        Assert.Equal(first.Describe(), second.Describe());
        Assert.Equal(first.Qi, second.Qi);
        Assert.Equal(first.B, second.B);
        Assert.Equal(first.Eur, second.Eur);
        Assert.Contains("no random or clock-dependent step", DeclineRules.DeterminismText, StringComparison.Ordinal);
    }

    [Fact]
    public void A_gap_in_the_history_is_named()
    {
        var readings = Hyperbolic(1000.0, 0.01, 0.5).ToList();

        // Remove four consecutive months, leaving a 120-day gap.
        readings.RemoveRange(8, 4);

        var fit = Service().Fit(Model(readings), Inputs());

        Assert.Contains("days with no recorded volume", fit.GapsText, StringComparison.Ordinal);
        Assert.Contains("2024-0", fit.GapsText, StringComparison.Ordinal);
    }

    [Fact]
    public void The_inputs_are_validated_before_anything_is_fitted()
    {
        Assert.Equal(DeclineRules.CodeWellRequired, DeclineRules.Validate(Inputs(well: " "))!.Value.Code);
        Assert.Equal(DeclineRules.CodeFluidInvalid, DeclineRules.Validate(Inputs(fluid: "MUD"))!.Value.Code);
        Assert.Equal(DeclineRules.CodeModelUnknown, DeclineRules.Validate(Inputs(model: "MAGIC"))!.Value.Code);
        Assert.Equal(
            DeclineRules.CodeEconomicLimitInvalid,
            DeclineRules.Validate(Inputs(limit: 0m))!.Value.Code);

        Assert.Null(DeclineRules.Validate(Inputs()));
        Assert.Equal(DeclineModels.Hyperbolic, DeclineRules.CanonicalModel("hyperbolic"));
        Assert.Equal(
            [ForecastAlgorithmConstants.ArpsBdfMinB, ForecastAlgorithmConstants.ArpsBdfSoftMaxB],
            [DeclineRules.BoundsFor(DeclineModels.ModifiedHyperbolic).Minimum,
             DeclineRules.BoundsFor(DeclineModels.ModifiedHyperbolic).Maximum]);
    }

    [Fact]
    public async Task Commands_need_a_connection_an_actor_and_a_successful_fit()
    {
        var service = new OgDeclineCurveService(() => null, new Quiet());

        Assert.Equal(DeclineRules.CodeNoConnection, (await service.AnalyzeAsync(Inputs())).Reference);
        Assert.Equal(DeclineRules.CodeNoConnection, (await service.LoadAsync()).Reference);

        var refused = service.Fit(Model(RawHyperbolic(1000.0, 0.01, 1.5)), Inputs());

        Assert.Equal(
            DeclineRules.CodeActorRequired,
            (await service.SaveAsync(refused, " ")).Reference);

        Assert.Equal(
            DeclineRules.CodeNothingToSave,
            (await service.SaveAsync(refused, "sara")).Reference);

        var good = service.Fit(Model(Hyperbolic(1000.0, 0.01, 0.5)), Inputs());

        Assert.Equal(
            DeclineRules.CodeNoConnection,
            (await service.SaveAsync(good, "sara")).Reference);
    }

    // ------------------------------------------------------------------ helpers

    private static OgDeclineCurveService Service() => new(() => null, new Quiet());

    private static IReadOnlyList<OgDeclineCurveService.DeclineReading> Hyperbolic(double qi, double di, double b) =>
        Enumerable.Range(0, 24)
            .Select(month =>
            {
                var date = Start.AddMonths(month);
                var days = (date - Start).TotalDays;

                return new OgDeclineCurveService.DeclineReading(
                    "WELL-1", date, DeclineFluids.Oil,
                    (decimal)ArpsDeclineMethods.HyperbolicDecline(qi, di, days, b), "bbl");
            })
            .ToList();

    /// <summary>
    /// A hyperbolic history at a b the engine's own validator rejects - the only way to build data whose best fit wants
    /// to leave the physical range, which is what SG-06-6 is about.
    /// </summary>
    private static IReadOnlyList<OgDeclineCurveService.DeclineReading> RawHyperbolic(double qi, double di, double b) =>
        Enumerable.Range(0, 24)
            .Select(month =>
            {
                var date = Start.AddMonths(month);
                var days = (date - Start).TotalDays;
                var rate = qi / Math.Pow(1.0 + b * di * days, 1.0 / b);

                return new OgDeclineCurveService.DeclineReading(
                    "WELL-1", date, DeclineFluids.Oil, (decimal)rate, "bbl");
            })
            .ToList();

    private static IReadOnlyList<OgDeclineCurveService.DeclineReading> Exponential(double qi, double di) =>
        Enumerable.Range(0, 24)
            .Select(month =>
            {
                var date = Start.AddMonths(month);
                var days = (date - Start).TotalDays;

                return new OgDeclineCurveService.DeclineReading(
                    "WELL-1", date, DeclineFluids.Oil,
                    (decimal)ArpsDeclineMethods.ExponentialDecline(qi, di, days), "bbl");
            })
            .ToList();

    private static IReadOnlyList<OgDeclineCurveService.DeclineReading> Harmonic() =>
        Enumerable.Range(0, 24)
            .Select(month =>
            {
                var date = Start.AddMonths(month);
                var days = (date - Start).TotalDays;

                return new OgDeclineCurveService.DeclineReading(
                    "WELL-1", date, DeclineFluids.Oil,
                    (decimal)ArpsDeclineMethods.HarmonicDecline(1000.0, 0.01, days), "bbl");
            })
            .ToList();

    private static OgDeclineCurveService.DeclineModel Model(
        IReadOnlyList<OgDeclineCurveService.DeclineReading> readings) =>
        new([], [], [], readings);

    private static OgDeclineInputs Inputs(
        string? well = "WELL-1",
        string? fluid = DeclineFluids.Oil,
        string? model = DeclineModels.Hyperbolic,
        decimal? fixedB = null,
        decimal? limit = 10m) =>
        new(well, fluid, Start, Start.AddYears(3), model, fixedB, limit);

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
