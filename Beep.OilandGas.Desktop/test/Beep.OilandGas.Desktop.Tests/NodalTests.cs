using TheTechIdea.Beep.OilandGas.Desktop.Data.Lift;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// LFT-01: the operating point on the engine's own curves, the no-intersection answer, the independent cross-check, the
/// correlation record and the tubing sweep - the parts that do not need a database. Saving and reloading are proven in the
/// gateway test.
/// </summary>
public sealed class NodalTests
{
    private const string PageFileName = "NodalPageView.cs";

    [Fact]
    public void A_reference_case_lands_where_the_two_engine_curves_meet()
    {
        // AC-1: the reported point must lie on both curves the engine produced. The check interpolates those curves
        // itself, so it does not trust either solver for the answer.
        var study = Service().Build(Inputs());

        Assert.True(study.HasOperatingPoint, study.Diagnostics);
        Assert.True(study.Converged);

        var rate = study.FlowRate!.Value;
        var pressure = study.FlowingPressure!.Value;

        var inflow = Interpolate(study.IprCurve, rate);
        var outflow = Interpolate(study.VlpCurve, rate);

        Assert.InRange(Math.Abs(inflow - pressure), 0m, pressure * 0.01m);
        Assert.InRange(Math.Abs(outflow - pressure), 0m, Math.Max(1m, pressure * 0.01m));

        // The point is inside both curves' ranges, not on an end because the search gave up.
        Assert.True(rate > 0m);
        Assert.True(rate < study.IprCurve.Max(point => point.Rate));
        Assert.True(rate > study.VlpCurve.Min(point => point.Rate));

        // The curves are the engine's shape: inflow falls with rate, outflow rises.
        Assert.True(study.IprCurve[0].Pressure > study.IprCurve[^1].Pressure);
        Assert.True(study.VlpCurve[0].Pressure < study.VlpCurve[^1].Pressure);
    }

    [Fact]
    public void No_intersection_is_reported_as_such_and_never_as_a_zero_rate()
    {
        // AC-2: this well cannot lift - the outflow needs more pressure than the reservoir supplies at every rate.
        var study = Service().Build(DeadInputs());

        Assert.False(study.Converged);
        Assert.False(study.HasOperatingPoint);
        Assert.Null(study.FlowRate);
        Assert.Null(study.FlowingPressure);
        Assert.Equal("no intersection", study.StatusText);
        Assert.Equal("no operating point", study.OperatingText);

        Assert.Contains("no intersection", study.Diagnostics, StringComparison.Ordinal);
        Assert.Contains("a gap of", study.Diagnostics, StringComparison.Ordinal);
        Assert.Contains("psia", study.Diagnostics, StringComparison.Ordinal);

        // AC-3 also covers this case: the independent solver must find no crossing either.
        Assert.True(study.CrossCheck.Available, study.CrossCheck.Text);
        Assert.True(study.CrossCheck.Agrees, study.CrossCheck.Text);
        Assert.Contains("also finds no crossing", study.CrossCheck.Text, StringComparison.Ordinal);
    }

    [Fact]
    public void The_independent_solver_agrees_with_the_study()
    {
        // AC-3.
        var study = Service().Build(Inputs());

        Assert.True(study.CrossCheck.Available, study.CrossCheck.Text);
        Assert.True(study.CrossCheck.FoundPoint);
        Assert.True(study.CrossCheck.Agrees, study.CrossCheck.Text);
        Assert.Contains("agrees", study.CrossCheck.Text, StringComparison.Ordinal);
        Assert.Equal("agrees", study.CrossCheck.AgreeText);

        Assert.NotNull(study.CrossCheck.FlowRate);
        Assert.InRange(
            Math.Abs(study.CrossCheck.FlowRate!.Value - study.FlowRate!.Value),
            0m,
            study.FlowRate.Value * NodalRules.RateAgreementTolerance);
    }

    [Fact]
    public void The_curves_come_from_the_owner_and_the_view_holds_none_of_the_arithmetic()
    {
        // AC-4: the calculators are called in the data layer, never in the page.
        var sources = SourceRoot.AllSources();

        var page = File.ReadAllText(sources.Single(file => Path.GetFileName(file) == PageFileName));

        Assert.DoesNotContain("IPRCalculator", page, StringComparison.Ordinal);
        Assert.DoesNotContain("VLPCalculator", page, StringComparison.Ordinal);
        Assert.DoesNotContain("OperatingPointCalculator", page, StringComparison.Ordinal);

        var service = File.ReadAllText(sources.Single(file => Path.GetFileName(file) == "OgNodal.cs"));

        Assert.Contains("IPRCalculator.", service, StringComparison.Ordinal);
        Assert.Contains("VLPCalculator.", service, StringComparison.Ordinal);
        Assert.Contains("OperatingPointCalculator.TryFindOperatingPoint", service, StringComparison.Ordinal);
    }

    [Fact]
    public void The_study_records_both_correlations()
    {
        // AC-5.
        var study = Service().Build(Inputs());

        Assert.Equal(NodalIprModels.Vogel, study.IprModel);
        Assert.Equal(NodalVlpCorrelations.HagedornBrown, study.VlpCorrelation);
        Assert.Contains("Vogel", study.IprText, StringComparison.Ordinal);
        Assert.Contains("hagedorn", study.VlpText, StringComparison.Ordinal);

        var standing = Service().Build(Inputs() with { IprModel = NodalIprModels.Standing });

        Assert.Equal(NodalIprModels.Standing, standing.IprModel);
        Assert.Contains("Standing", standing.IprText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_sweep_reports_the_points_it_completed()
    {
        // AC-6.
        var swept = (await Service().SensitivityAsync(Inputs(), 1.995m, 4.5m, 5)).Value!;

        Assert.Equal(5, swept.Completed);
        Assert.Equal(5, swept.Requested);
        Assert.False(swept.Cancelled);
        Assert.Contains("complete", swept.StateText, StringComparison.Ordinal);
        Assert.Equal(5, swept.Points.Count);
        Assert.Equal(1.995m, swept.Points[0].TubingDiameter);
        Assert.Equal(4.5m, swept.Points[^1].TubingDiameter);

        // A cancelled sweep keeps what it finished rather than throwing it away.
        using var cancelled = new CancellationTokenSource();
        await cancelled.CancelAsync();

        var stopped = (await Service().SensitivityAsync(Inputs(), 1.995m, 4.5m, 5, cancelled.Token)).Value!;

        Assert.True(stopped.Cancelled);
        Assert.Equal(0, stopped.Completed);
        Assert.Equal(5, stopped.Requested);
        Assert.Contains("cancelled: 0 of 5", stopped.StateText, StringComparison.Ordinal);

        // The sweep says which diameter produced the best rate, or that none did.
        Assert.False(string.IsNullOrWhiteSpace(swept.BestText));
    }

    [Fact]
    public void A_well_that_cannot_lift_says_which_diameters_would_not_either()
    {
        var dead = DeadInputs();

        var swept = Service().SensitivityAsync(dead, 1.995m, 4.5m, 4).GetAwaiter().GetResult().Value!;

        Assert.Equal(4, swept.Completed);
        Assert.DoesNotContain(swept.Points, point => point.Converged);
        Assert.Contains("no diameter produced an operating point", swept.BestText, StringComparison.Ordinal);
        Assert.All(swept.Points, point => Assert.Equal("no intersection", point.ResultText));
    }

    [Fact]
    public void Every_input_is_required_and_named_when_missing()
    {
        var empty = new OgNodalInputs(null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null);

        var missing = NodalRules.Missing(empty);

        Assert.Contains("well", missing);
        Assert.Contains("reservoir pressure", missing);
        Assert.Contains("tubing diameter", missing);
        Assert.Contains("inflow model", missing);
        Assert.Contains("outflow correlation", missing);
        Assert.Equal(NodalRules.CodeWellRequired, NodalRules.Validate(empty)!.Value.Code);

        Assert.Equal(
            NodalRules.CodeInputRequired,
            NodalRules.Validate(Inputs() with { Depth = null })!.Value.Code);

        Assert.Equal(
            NodalRules.CodeIprUnknown,
            NodalRules.Validate(Inputs() with { IprModel = "MAGIC" })!.Value.Code);

        Assert.Equal(
            NodalRules.CodeVlpUnsupported,
            NodalRules.Validate(Inputs() with { VlpCorrelation = "BEGGS_BRILL" })!.Value.Code);

        Assert.Equal(
            NodalRules.CodeInputInvalid,
            NodalRules.Validate(Inputs() with { WaterCut = 1.5m })!.Value.Code);

        Assert.Equal(
            NodalRules.CodeInputInvalid,
            NodalRules.Validate(Inputs() with { BubblePointPressure = 5000m })!.Value.Code);

        Assert.Null(NodalRules.Validate(Inputs()));
    }

    [Fact]
    public async Task Commands_need_a_connection_and_an_actor()
    {
        var service = new OgNodalService(() => null, new Quiet());

        Assert.Equal(NodalRules.CodeNoConnection, (await service.LoadAsync()).Reference);
        Assert.Equal(NodalRules.CodeActorRequired, (await service.SaveAsync(Service().Build(Inputs()), " ")).Reference);

        // A study with no curve is not a study to keep.
        var nothing = new OgNodalStudy(
            string.Empty, "WELL-1", NodalIprModels.Vogel, NodalVlpCorrelations.HagedornBrown, null, null,
            false, null, string.Empty, [], [], new OgNodalCrossCheck("x", false, false, null, null, false, "x"),
            null, null);

        Assert.Equal(NodalRules.CodeNothingToSave, (await service.SaveAsync(nothing, "sara")).Reference);
        Assert.Equal(NodalRules.CodeNoConnection, (await service.SaveAsync(Service().Build(Inputs()), "sara")).Reference);
    }

    // ------------------------------------------------------------------ helpers

    private static OgNodalService Service() => new(() => null, new Quiet());

    /// <summary>The reference well: 4,500 psia reservoir, 6,000 ft of 3.958 in tubing, 200 psia at the wellhead.</summary>
    private static OgNodalInputs Inputs() =>
        new(
            "WELL-1",
            ReservoirPressure: 4500m,
            BubblePointPressure: 3600m,
            Temperature: 180m,
            ProductivityIndex: 1.5m,
            WaterCut: 0.2m,
            Gor: 600m,
            OilGravity: 35m,
            FormationVolumeFactor: 1.3m,
            OilViscosity: 1.5m,
            Depth: 6000m,
            TubingDiameter: 3.958m,
            WellheadPressure: 200m,
            GasSpecificGravity: 0.65m,
            TubingRoughness: 0.0006m,
            IprModel: NodalIprModels.Vogel,
            VlpCorrelation: NodalVlpCorrelations.HagedornBrown,
            Points: 20);

    /// <summary>
    /// A well that cannot lift: a 3,000 psia reservoir asked to produce against a 2,500 psia wellhead. The outflow needs
    /// more pressure than the inflow supplies at every rate, at any tubing size.
    /// </summary>
    private static OgNodalInputs DeadInputs() =>
        Inputs() with { ReservoirPressure = 3000m, BubblePointPressure = 2400m, WellheadPressure = 2500m };

    private static decimal Interpolate(IReadOnlyList<OgNodalPoint> curve, decimal rate)
    {
        for (var index = 1; index < curve.Count; index++)
        {
            if (rate <= curve[index].Rate)
            {
                var span = curve[index].Rate - curve[index - 1].Rate;

                return span <= 0m
                    ? curve[index].Pressure
                    : curve[index - 1].Pressure
                      + (rate - curve[index - 1].Rate) / span
                      * (curve[index].Pressure - curve[index - 1].Pressure);
            }
        }

        return curve[^1].Pressure;
    }

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
