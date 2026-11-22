using Beep.OilandGas.PumpPerformance.Constants;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Lift;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// LFT-03: the refusal outside the published curve, the module's NPSH minimum, the extrapolation label, the units and the
/// stage multiplication - the parts that do not need a database. Saving and reloading are proven in the gateway test.
/// </summary>
public sealed class PumpTests
{
    private const string PageFileName = "PumpPageView.cs";

    [Fact]
    public async Task A_duty_point_outside_the_published_curve_is_refused_with_the_range()
    {
        // AC-1 and SG-07-4.
        var service = Service();

        var outside = await service.AnalyseAsync(Inputs() with { DutyRate = 2000m });

        Assert.False(outside.Success);
        Assert.Equal(PumpRules.CodeDutyOutsideCurve, outside.Reference);
        Assert.Contains("2,000 GPM", outside.Message!, StringComparison.Ordinal);
        Assert.Contains("0 to 1,500 GPM", outside.Message!, StringComparison.Ordinal);
        Assert.Null(outside.Value);

        // A negative rate is an input error before it is a curve error.
        var below = await service.AnalyseAsync(Inputs() with { DutyRate = -1m });

        Assert.False(below.Success);
        Assert.Equal(PumpRules.CodeInputInvalid, below.Reference);

        // The ends of the curve are inside it.
        foreach (var rate in new[] { 1m, 1500m })
        {
            var inside = await service.AnalyseAsync(Inputs() with { DutyRate = rate });

            Assert.True(inside.Success, inside.Message);
            Assert.Equal(rate, inside.Value!.Selected!.Rate);
        }

        // Zero rate is not a duty point, and it is refused as an input rather than as an engine failure.
        var zero = await service.AnalyseAsync(Inputs() with { DutyRate = 0m });

        Assert.False(zero.Success);
        Assert.Equal(PumpRules.CodeInputInvalid, zero.Reference);
        Assert.Contains("above zero", zero.Message!, StringComparison.Ordinal);
    }

    [Fact]
    public void The_duty_point_is_read_from_the_published_curve()
    {
        var result = OgPumpService.Build(Inputs());

        // At 750 GPM the curve runs from 110 ft at 500 to 90 ft at 1,000, so half way is 100 ft.
        Assert.Equal(750m, result.Rate);
        Assert.Equal(100m, result.Head);
        Assert.Equal("750 GPM", result.RateText);
        Assert.Equal("100 ft", result.HeadText);

        // The efficiency comes from the curve's own column, which the module fills from the published power.
        Assert.True(result.Efficiency is > 0m and <= 1m);
        Assert.Contains("efficiency is the published value", result.NotesText, StringComparison.Ordinal);
        Assert.True(result.BrakeHorsepower > 0m);
        Assert.True(result.PowerKilowatts > 0m);

        // The stage count multiplies the head of the string, not the curve's per-stage value.
        var three = OgPumpService.Build(Inputs() with { Stages = 3 });

        Assert.Equal(100m, three.Head);
        Assert.Equal(300m, three.TotalHead);
        Assert.Contains("3 stages", three.StageText, StringComparison.Ordinal);
        Assert.Contains("total 300 ft", three.StageText, StringComparison.Ordinal);
    }

    [Fact]
    public void The_npsh_margin_uses_the_modules_minimum_and_names_both_values()
    {
        // AC-2: the minimum is PumpConstants' own value, not a number written in the screen.
        Assert.Equal((decimal)PumpConstants.DefaultNpshSafetyMarginFeet, PumpRules.MinimumNpshMargin);

        var healthy = OgPumpService.Build(Inputs());

        Assert.Equal(PumpRules.MinimumNpshMargin, healthy.NpshMinimumMargin);
        Assert.False(healthy.NpshWarning);
        Assert.InRange(
            Math.Abs(healthy.NpshAvailable - healthy.NpshRequired - healthy.NpshMargin),
            0m,
            0.000001m);
        Assert.Contains("NPSHa", healthy.NpshText, StringComparison.Ordinal);
        Assert.Contains("NPSHr", healthy.NpshText, StringComparison.Ordinal);
        Assert.Contains("meets the 2 ft minimum", healthy.NpshText, StringComparison.Ordinal);

        // A suction lift deep enough to eat the margin is a warning, not a refusal, and both values are shown.
        var thin = OgPumpService.Build(Inputs() with { SuctionHead = 35m });

        Assert.True(thin.NpshWarning);
        Assert.True(thin.NpshMargin < thin.NpshMinimumMargin);
        Assert.Contains("below the 2 ft minimum", thin.NpshText, StringComparison.Ordinal);
        Assert.Contains("cavitation", thin.NpshText, StringComparison.Ordinal);
    }

    [Fact]
    public void An_affinity_result_outside_the_curve_is_labelled_extrapolated()
    {
        // AC-3.
        var faster = OgPumpService.Build(Inputs() with { NewSpeedRpm = 8000m });

        Assert.True(faster.Extrapolated);
        Assert.Contains("EXTRAPOLATED", faster.AffinitySummary, StringComparison.Ordinal);
        Assert.Contains("8,000 rpm", faster.AffinitySummary, StringComparison.Ordinal);
        Assert.Contains("outside the published 0 to 1,500 GPM", faster.AffinitySummary, StringComparison.Ordinal);

        var slower = OgPumpService.Build(Inputs() with { NewSpeedRpm = 3000m });

        Assert.False(slower.Extrapolated);
        Assert.Contains("3,000 rpm", slower.AffinitySummary, StringComparison.Ordinal);
        Assert.DoesNotContain("EXTRAPOLATED", slower.AffinitySummary, StringComparison.Ordinal);

        // The affinity relation itself is the module's: q scales with speed, head with its square.
        Assert.Contains("642.857 GPM", slower.AffinitySummary, StringComparison.Ordinal);
        Assert.Contains("73.47 ft", slower.AffinitySummary, StringComparison.Ordinal);

        // No speed change means no affinity line at all, rather than a line saying nothing happened twice.
        Assert.Equal("no affinity change was applied", OgPumpService.Build(Inputs()).AffinitySummary);
    }

    [Fact]
    public void Every_result_states_its_unit()
    {
        // AC-5.
        var result = OgPumpService.Build(Inputs());

        Assert.Contains("GPM", result.RateText, StringComparison.Ordinal);
        Assert.Contains("ft", result.HeadText, StringComparison.Ordinal);
        Assert.Contains("hp", result.PowerText, StringComparison.Ordinal);
        Assert.Contains("kW", result.PowerText, StringComparison.Ordinal);
        Assert.Contains("%", result.EfficiencyText, StringComparison.Ordinal);
        Assert.Contains("ft", result.NpshText, StringComparison.Ordinal);
        Assert.Contains("GPM", result.RangeText, StringComparison.Ordinal);

        // The saved units are the module's own names, not the view's.
        Assert.Equal("GPM", OgPumpResult.RateUnit);
        Assert.Equal("ft", OgPumpResult.HeadUnit);
        Assert.Equal("hp", OgPumpResult.PowerUnit);
    }

    [Fact]
    public void The_result_records_the_curve_source_and_the_stages()
    {
        // AC-6.
        var result = OgPumpService.Build(Inputs() with { Stages = 4 });

        Assert.Equal(PumpRules.CurveSource, result.CurveSource);
        Assert.Equal(OgPumpResult.CurveFromEnteredPoints, result.CurveSource);
        Assert.Equal(4, result.Stages);
        Assert.Equal(4, result.Curve.Count);

        // The curves are the owner's: a source guard that the view calls none of the calculators.
        var sources = SourceRoot.AllSources();
        var page = File.ReadAllText(sources.Single(file => Path.GetFileName(file) == PageFileName));

        foreach (var calculator in new[]
                 {
                     "HeadQuantityCalculations", "PowerCalculations", "EfficiencyCalculations",
                     "NPSHCalculations", "AffinityLaws",
                 })
        {
            Assert.DoesNotContain(calculator, page, StringComparison.Ordinal);
        }

        var service = File.ReadAllText(sources.Single(file => Path.GetFileName(file) == "OgPump.cs"));

        Assert.Contains("HeadQuantityCalculations.", service, StringComparison.Ordinal);
        Assert.Contains("NPSHCalculations.", service, StringComparison.Ordinal);
        Assert.Contains("AffinityLaws.", service, StringComparison.Ordinal);
    }

    [Fact]
    public void The_inputs_are_validated_before_the_engine_is_called()
    {
        Assert.Equal(PumpRules.CodeWellRequired, PumpRules.Validate(Inputs() with { WellId = " " })!.Value.Code);
        Assert.Equal(PumpRules.CodeInputRequired, PumpRules.Validate(Inputs() with { DutyRate = null })!.Value.Code);
        Assert.Equal(PumpRules.CodeInputRequired, PumpRules.Validate(Inputs() with { Curve = [] })!.Value.Code);
        Assert.Equal(PumpRules.CodeTypeUnknown, PumpRules.Validate(Inputs() with { PumpType = "WINDMILL" })!.Value.Code);
        Assert.Equal(PumpRules.CodeInputInvalid, PumpRules.Validate(Inputs() with { Stages = 0 })!.Value.Code);
        Assert.Equal(PumpRules.CodeInputInvalid, PumpRules.Validate(Inputs() with { SpecificGravity = 0m })!.Value.Code);
        Assert.Equal(PumpRules.CodeInputInvalid, PumpRules.Validate(Inputs() with { NewSpeedRpm = 0m })!.Value.Code);

        // A curve whose rates do not increase cannot be read at a duty rate.
        var flat = Inputs() with
        {
            Curve = [new OgPumpCurvePoint(500m, 100m, null), new OgPumpCurvePoint(500m, 90m, null)],
        };

        Assert.Equal(PumpRules.CodeCurveInvalid, PumpRules.Validate(flat)!.Value.Code);

        var negative = Inputs() with
        {
            Curve = [new OgPumpCurvePoint(0m, -10m, null), new OgPumpCurvePoint(100m, 90m, null)],
        };

        Assert.Equal(PumpRules.CodeCurveInvalid, PumpRules.Validate(negative)!.Value.Code);

        Assert.Null(PumpRules.Validate(Inputs()));
    }

    [Fact]
    public async Task Commands_need_a_connection_and_an_actor_and_keep_a_lost_save()
    {
        var service = new OgPumpService(() => null, new Quiet());
        var result = OgPumpService.Build(Inputs());

        Assert.Equal(PumpRules.CodeNoConnection, (await service.LoadAsync()).Reference);
        Assert.Equal(PumpRules.CodeActorRequired, (await service.SaveAsync(result, " ")).Reference);
        Assert.Equal(PumpRules.CodeNoConnection, (await service.SaveAsync(result, "sara")).Reference);
        Assert.Equal(PumpRules.CodeNothingPending, (await service.RetrySaveAsync("sara")).Reference);
        Assert.Null(service.Pending);

        var empty = result with { Curve = [] };

        Assert.Equal(PumpRules.CodeNothingToSave, (await service.SaveAsync(empty, "sara")).Reference);
    }

    // ------------------------------------------------------------------ helpers

    private static OgPumpService Service() => new(() => null, new Quiet());

    /// <summary>A four-point published curve from 0 to 1,500 GPM, checked at 750 GPM.</summary>
    private static OgPumpInputs Inputs() =>
        new(
            "WELL-1",
            PumpTypes.Esp,
            [
                new OgPumpCurvePoint(0m, 120m, 20m),
                new OgPumpCurvePoint(500m, 110m, 30m),
                new OgPumpCurvePoint(1000m, 90m, 40m),
                new OgPumpCurvePoint(1500m, 60m, 50m),
            ],
            Stages: 1,
            DutyRate: 750m,
            SpecificGravity: 1.0m,
            SpeedRpm: 3500m,
            NewSpeedRpm: null,
            AtmosphericPressure: 14.696m,
            VaporPressure: 0.256m,
            SuctionHead: 20m,
            SuctionFriction: 2m);

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
