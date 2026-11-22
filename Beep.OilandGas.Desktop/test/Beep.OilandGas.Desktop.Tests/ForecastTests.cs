using Beep.OilandGas.DCA.AdvancedDeclineMethods;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Reservoir;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// RES-07: the profile arithmetic, the ceiling refusal, the economic-limit end, the method rule, the units per stream and
/// determinism - the parts that do not need a database. Saving, versioning, approval and the identical reload (AC-1,
/// SG-06-7) are proven in the gateway test.
/// </summary>
public sealed class ForecastTests
{
    private static readonly DateTime Start = new(2026, 1, 1);

    [Fact]
    public void A_profile_is_generated_from_a_saved_fit_with_the_owner_arithmetic()
    {
        var forecast = Build(Inputs());

        Assert.NotNull(forecast);
        Assert.False(forecast!.IsRefused, forecast.RefusalText);
        Assert.Equal(24, forecast.Points.Count);
        Assert.Equal(ForecastStatuses.Draft, forecast.Status);

        // AC-2: the method and the source fit are on the record.
        Assert.Equal(nameof(ForecastType.Hyperbolic), forecast.Method);
        Assert.Equal("FIT-1", forecast.SourceFitId);

        // The first rate is the fit's qi and the profile declines from it.
        Assert.Equal(1000m, forecast.InitialRate);
        Assert.True(forecast.FinalRate < forecast.InitialRate);

        // The rate at each month is the owner's decline relation at that month's start.
        var monthFive = forecast.Points[4];
        var days = (monthFive.Date - Start).TotalDays;
        var expected = (decimal)ArpsDeclineMethods.HyperbolicDecline(1000.0, 0.01, days, 0.5);

        Assert.Equal(expected, monthFive.Oil, 4);

        // The volumes are the closed-form cumulative differences, so the total is their sum and the running total closes.
        Assert.Equal(forecast.Points.Sum(point => point.Volume), forecast.Total);
        Assert.Equal(forecast.Points[^1].Cumulative, forecast.Total);
        Assert.All(forecast.Points, point => Assert.True(point.Volume > 0m));
    }

    [Fact]
    public void The_forecast_is_for_one_stream_and_carries_a_unit_per_stream()
    {
        // AC-6: the fit describes one fluid; the other streams stay zero and only that stream carries a unit.
        var oil = Build(Inputs());

        Assert.NotNull(oil);
        Assert.Equal("OIL", oil!.Fluid);
        Assert.Equal("bbl", oil.OilUnit);
        Assert.Null(oil.GasUnit);
        Assert.Null(oil.WaterUnit);
        Assert.Contains("oil bbl", oil.UnitsText, StringComparison.Ordinal);
        Assert.Contains("gas (none)", oil.UnitsText, StringComparison.Ordinal);
        Assert.All(oil.Points, point => Assert.True(point.Oil > 0m && point.Gas == 0m && point.Water == 0m));

        var gas = Build(Inputs() with { SourceFitId = "FIT-GAS" });

        Assert.NotNull(gas);
        Assert.Equal("GAS", gas!.Fluid);
        Assert.Equal("MMscf", gas.GasUnit);
        Assert.All(gas.Points, point => Assert.True(point.Gas > 0m && point.Oil == 0m));
    }

    [Fact]
    public void A_ceiling_the_profile_breaks_is_refused_not_clamped()
    {
        // AC-5 and the state table: a constraint violation is a refusal, and the profile is not silently capped.
        var forecast = Build(Inputs() with { MaxRate = 900m });

        Assert.NotNull(forecast);
        Assert.True(forecast!.IsRefused);
        Assert.Equal(ForecastRules.CodeConstraintExceeded, forecast.RefusalCode);
        Assert.Contains("month 1", forecast.RefusalMessage!, StringComparison.Ordinal);
        Assert.Contains("refused rather than", forecast.RefusalMessage!, StringComparison.Ordinal);
        Assert.Empty(forecast.Points);

        // A ceiling above the initial rate is satisfied and the profile is unchanged.
        var accepted = Build(Inputs() with { MaxRate = 1000m });

        Assert.NotNull(accepted);
        Assert.False(accepted!.IsRefused, accepted.RefusalText);
        Assert.Equal(24, accepted.Points.Count);
    }

    [Fact]
    public void The_economic_limit_ends_the_profile_rather_than_breaking_a_constraint()
    {
        var forecast = Build(Inputs() with { EconomicLimit = 500m });

        Assert.NotNull(forecast);
        Assert.False(forecast!.IsRefused, forecast.RefusalText);
        Assert.InRange(forecast.Points.Count, 1, 23);

        var lastRate = forecast.Points[^1].Oil;
        var previousRate = forecast.Points[^2].Oil;

        Assert.True(lastRate <= 500m);
        Assert.True(previousRate > 500m);
        Assert.Equal(500m, forecast.EconomicLimit);

        // A limit at or above the initial rate would leave nothing, and is refused rather than producing an empty table.
        var impossible = Build(Inputs() with { EconomicLimit = 1000m });

        Assert.NotNull(impossible);
        Assert.Equal(ForecastRules.CodeConstraintInvalid, impossible!.RefusalCode);
    }

    [Fact]
    public void The_method_must_match_the_decline_the_source_describes()
    {
        var mismatch = Build(Inputs() with { Method = nameof(ForecastType.Exponential) });

        Assert.NotNull(mismatch);
        Assert.Equal(ForecastRules.CodeMethodUnsupported, mismatch!.RefusalCode);
        Assert.Contains("mislabel", mismatch.RefusalMessage!, StringComparison.Ordinal);

        // Decline is accepted whatever b is, because it names the family rather than a specific curve.
        Assert.False(Build(Inputs() with { Method = nameof(ForecastType.Decline) })!.IsRefused);

        // And the method that matches b is accepted.
        Assert.False(Build(Inputs() with { Method = nameof(ForecastType.Hyperbolic) })!.IsRefused);
    }

    [Fact]
    public void A_method_with_no_source_in_this_screen_is_refused_with_the_reason()
    {
        var refusal = ForecastRules.Validate(
            Inputs() with { Method = nameof(ForecastType.PseudoSteadyStateSinglePhase) });

        Assert.Equal(ForecastRules.CodeMethodUnsupported, refusal!.Value.Code);
        Assert.Contains("Arps decline", refusal.Value.Message, StringComparison.Ordinal);
        Assert.Contains("Exponential", refusal.Value.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Typed_parameters_are_a_source_of_their_own()
    {
        var manual = Inputs() with
        {
            SourceFitId = null,
            ManualQi = 500m,
            ManualDiPerDay = 0.02m,
            ManualB = 0m,
            ManualFluid = ForecastStreams.Oil,
            ManualUnit = "bbl",
            Method = nameof(ForecastType.Exponential),
        };

        var forecast = Build(manual);

        Assert.NotNull(forecast);
        Assert.False(forecast!.IsRefused, forecast.RefusalText);
        Assert.Null(forecast.SourceFitId);
        Assert.Contains("manual parameters", forecast.SourceText, StringComparison.Ordinal);
        Assert.Equal(500m, forecast.InitialRate);

        // An exponential source declines by the owner's exponential relation.
        var days = (forecast.Points[4].Date - Start).TotalDays;
        var expected = (decimal)ArpsDeclineMethods.ExponentialDecline(500.0, 0.02, days);

        Assert.Equal(expected, forecast.Points[4].Oil, 4);

        // Without a fit the three parameters are required, and each is named when it is missing.
        Assert.Equal(
            ForecastRules.CodeNoSource,
            ForecastRules.Validate(manual with { ManualQi = null })!.Value.Code);
        Assert.Equal(
            ForecastRules.CodeManualIncomplete,
            ForecastRules.Validate(manual with { ManualDiPerDay = 0m })!.Value.Code);
        Assert.Equal(
            ForecastRules.CodeManualIncomplete,
            ForecastRules.Validate(manual with { ManualB = 1.5m })!.Value.Code);
    }

    [Fact]
    public void An_unknown_or_refused_source_fit_is_refused_with_its_reason()
    {
        var model = Model(RefusedFit());

        var unknown = OgForecastService.Build(Inputs(), model);

        Assert.NotNull(unknown);
        Assert.Equal(ForecastRules.CodeFitUnknown, unknown!.RefusalCode);

        var refusedFit = OgForecastService.Build(Inputs() with { SourceFitId = "FIT-REFUSED" }, Model(RefusedFit()));

        Assert.NotNull(refusedFit);
        Assert.Equal(ForecastRules.CodeFitRefused, refusedFit!.RefusalCode);
        Assert.Contains("refused", refusedFit.RefusalMessage!, StringComparison.Ordinal);
    }

    [Fact]
    public void The_same_source_and_horizon_give_the_same_profile()
    {
        var first = Build(Inputs());
        var second = Build(Inputs());

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Equal(first!.Describe(), second!.Describe());
        Assert.Equal(first.Total, second.Total);
        Assert.Equal(first.PointsText, second.PointsText);
    }

    [Fact]
    public void The_inputs_are_validated_before_anything_is_generated()
    {
        Assert.Equal(ForecastRules.CodeNameRequired, ForecastRules.Validate(Inputs() with { Name = " " })!.Value.Code);
        Assert.Equal(ForecastRules.CodeStartRequired, ForecastRules.Validate(Inputs() with { StartDate = null })!.Value.Code);
        Assert.Equal(ForecastRules.CodeMonthsInvalid, ForecastRules.Validate(Inputs() with { Months = 0 })!.Value.Code);
        Assert.Equal(
            ForecastRules.CodeMonthsInvalid,
            ForecastRules.Validate(Inputs() with { Months = ForecastRules.MaximumMonths + 1 })!.Value.Code);
        Assert.Equal(ForecastRules.CodeMethodUnknown, ForecastRules.Validate(Inputs() with { Method = "MAGIC" })!.Value.Code);
        Assert.Equal(
            ForecastRules.CodeConstraintInvalid,
            ForecastRules.Validate(Inputs() with { MaxRate = 0m })!.Value.Code);
        Assert.Equal(
            ForecastRules.CodeConstraintInvalid,
            ForecastRules.Validate(Inputs(width: 100m, floor: 200m))!.Value.Code);

        Assert.Null(ForecastRules.Validate(Inputs()));
    }

    [Fact]
    public void The_workspace_says_why_the_generator_is_disabled_and_lists_every_version()
    {
        var empty = new OgForecastWorkspace(
            [], null, [], ["Decline"], ForecastStatuses.Fallback,
            ["Unknown"], false, false, null);

        Assert.False(empty.HasFits);
        Assert.Contains("Fit the well on the decline curve screen first", empty.NoSourceReason, StringComparison.Ordinal);

        var withFits = empty with { Fits = [new OgDeclineFit(
            "FIT-1", "OK", "HYPERBOLIC", null, null, 1000m, 0.01m, 0.5m, true, 1000m, 10m, "bbl", 1m,
            24, 0, [], [], new OgDeclineCrossCheck("x", false, null, null, null, null, false, "x"),
            "OIL", "WELL-1", null, Start, Start.AddYears(2), "sara", Start)] };

        Assert.True(withFits.HasFits);
        Assert.Equal(string.Empty, withFits.NoSourceReason);
        Assert.Contains("no forecast named", withFits.VersionsText("Base case"), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Commands_need_a_connection_and_an_actor()
    {
        var service = new OgForecastService(() => null, new Quiet());

        Assert.Equal(ForecastRules.CodeNoConnection, (await service.GenerateAsync(Inputs())).Reference);
        Assert.Equal(ForecastRules.CodeNoConnection, (await service.LoadAsync()).Reference);

        var forecast = Build(Inputs())!;

        Assert.Equal(ForecastRules.CodeActorRequired, (await service.SaveAsync(forecast, " ")).Reference);

        var refused = Build(Inputs() with { MaxRate = 900m })!;

        Assert.Equal(ForecastRules.CodeNothingToSave, (await service.SaveAsync(refused, "sara")).Reference);
        Assert.Equal(ForecastRules.CodeNothingPending, (await service.RetrySaveAsync("sara")).Reference);
    }

    // ------------------------------------------------------------------ helpers

    private static OgForecast? Build(OgForecastInputs inputs) =>
        OgForecastService.Build(inputs, Model(Fit("FIT-1", "OIL", "bbl"), Fit("FIT-GAS", "GAS", "MMscf"), RefusedFit()));

    private static OgForecastInputs Inputs(
        decimal? width = null,
        decimal? floor = null) =>
        new(
            "Base case",
            "WELL-1",
            "F-1",
            nameof(ForecastType.Hyperbolic),
            "FIT-1",
            Start,
            24,
            width,
            floor,
            "Unknown");

    private static OgForecastService.ForecastModel Model(params DECLINE_FIT[] fits) =>
        new([], [], fits, []);

    private static DECLINE_FIT Fit(string id, string fluid, string unit) =>
        new()
        {
            FIT_ID = id,
            WELL_ID = "WELL-1",
            FLUID = fluid,
            MODEL = "HYPERBOLIC",
            OUTCOME = "OK",
            QI = 1000m,
            DI_PER_DAY = 0.01m,
            B = 0.5m,
            BOUNDED_IND = "Y",
            VOLUME_OUOM = unit,
            R_SQUARED = 1m,
            POINTS_USED = 24,
            POINTS_EXCLUDED = 0,
            ACTOR = "sara",
            RECORDED_AT = Start,
        };

    private static DECLINE_FIT RefusedFit() =>
        new()
        {
            FIT_ID = "FIT-REFUSED",
            WELL_ID = "WELL-1",
            MODEL = "HYPERBOLIC",
            OUTCOME = "REFUSED",
            REFUSAL_CODE = "decline-fit-unbounded",
            REFUSAL_MESSAGE = "the data wants b above 1",
            BOUNDED_IND = "N",
            POINTS_USED = 24,
            POINTS_EXCLUDED = 0,
            ACTOR = "sara",
            RECORDED_AT = Start,
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
