using System.Globalization;
using Beep.OilandGas.PlungerLift.Constants;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Lift;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// LFT-05: the criterion panel, the infeasible/uneconomic distinction, the cycle arithmetic, the slug cap, the types from
/// the owning constants and the units - the parts that do not need a database. Saving and reloading are proven in the
/// gateway test.
/// </summary>
public sealed class PlungerTests
{
    private const string PageFileName = "PlungerPageView.cs";

    [Fact]
    public void Every_criterion_is_evaluated_with_its_value_and_its_range()
    {
        // AC-1: six criteria, each with the deciding value and the module's range.
        var result = OgPlungerService.Build(Inputs());

        Assert.Equal(6, result.Criteria.Count);
        Assert.Equal(
            ["GLR_RANGE", "PRESSURE_DIFFERENTIAL", "FALL_VELOCITY_RANGE", "RISE_VELOCITY_RANGE",
             "CYCLE_TIME_RANGE", "SLUG_SIZE_RANGE"],
            result.Criteria.Select(criterion => criterion.Code));

        Assert.All(result.Criteria, criterion =>
        {
            Assert.Contains("pass", criterion.Describe(), StringComparison.Ordinal);
            Assert.False(string.IsNullOrWhiteSpace(criterion.ValueText));
            Assert.False(string.IsNullOrWhiteSpace(criterion.RangeText));
            Assert.False(string.IsNullOrWhiteSpace(criterion.Message));
        });

        // The differential names the module's minimum rather than a band, and says the value meets it.
        var differentialPass = result.Criteria.Single(criterion => criterion.Code == "PRESSURE_DIFFERENTIAL");

        Assert.Contains("meets the module's", differentialPass.Message, StringComparison.Ordinal);
        Assert.Contains("minimum", differentialPass.Message, StringComparison.Ordinal);

        // The banded criteria say "inside" their range.
        Assert.All(
            result.Criteria.Where(criterion => criterion.Code != "PRESSURE_DIFFERENTIAL"),
            criterion => Assert.Contains("inside", criterion.Message, StringComparison.Ordinal));

        // A failed criterion says FAIL, with the value and the range it missed.
        var thin = OgPlungerService.Build(Inputs() with { CasingPressure = 199m });

        var differential = thin.Criteria.Single(criterion => criterion.Code == "PRESSURE_DIFFERENTIAL");

        Assert.False(differential.Passed);
        Assert.Equal("FAIL", differential.OutcomeText);
        Assert.Contains("is below the module's", differential.Message, StringComparison.Ordinal);
        Assert.Contains("49", differential.ValueText, StringComparison.Ordinal);
        Assert.Contains(
            PlungerLiftConstants.MinimumPressureDifferential.ToString("0.##", CultureInfo.InvariantCulture),
            differential.RangeText,
            StringComparison.Ordinal);
    }

    [Fact]
    public void An_infeasible_well_never_shows_a_rate()
    {
        // AC-2: the verdict is infeasible and there is no daily rate - not a zero.
        foreach (var infeasible in new[]
                 {
                     Inputs() with { CasingPressure = 199m },
                     Inputs() with { GasLiquidRatio = 100m },
                 })
        {
            var result = OgPlungerService.Build(infeasible);

            Assert.True(result.IsInfeasible, result.FailedText);
            Assert.False(result.IsFeasible);
            Assert.Null(result.DailyRate);
            Assert.NotEmpty(result.Failed);
            Assert.Contains("no rate", result.RateText, StringComparison.Ordinal);
            Assert.Contains("infeasible", result.RateText, StringComparison.Ordinal);
            Assert.False(result.Economic);
        }
    }

    [Fact]
    public void A_feasible_well_that_cannot_keep_up_is_uneconomic_not_infeasible()
    {
        // AC-3: the third state, with the basis that decided it. Four hundred barrels a day cannot be lifted in cycles
        // whose slug is capped at the module's maximum, but nothing mechanical has failed.
        var result = OgPlungerService.Build(Inputs() with { LiquidRateBblPerDay = 400m });

        Assert.True(result.IsUneconomic, result.Describe());
        Assert.False(result.IsInfeasible);
        Assert.False(result.IsFeasible);
        Assert.False(result.Economic);
        Assert.Empty(result.Failed);

        // The rate it can lift is reported - unlike the infeasible case - and it is below the inflow.
        Assert.NotNull(result.DailyRate);
        Assert.True(result.DailyRate < 400m);
        Assert.Equal(PlungerLiftConstants.MaximumLiquidSlugSize, result.RatePerCycle);

        Assert.Contains("against an inflow of", result.EconomicBasis, StringComparison.Ordinal);
        Assert.Contains("400", result.EconomicBasis, StringComparison.Ordinal);
        Assert.Contains("no price basis", result.EconomicBasis, StringComparison.Ordinal);
        Assert.Contains("uneconomic", result.EconomicText, StringComparison.Ordinal);

        // The same well with a rate the cycle can lift is feasible and economic.
        var feasible = OgPlungerService.Build(Inputs());

        Assert.True(feasible.IsFeasible);
        Assert.True(feasible.Economic);
        Assert.NotNull(feasible.DailyRate);
        Assert.Equal(10m, feasible.DailyRate!.Value, 3);
        Assert.Contains("economic for plunger lift", feasible.EconomicText, StringComparison.Ordinal);
    }

    [Fact]
    public void The_three_phases_add_up_to_the_cycle_and_the_day()
    {
        // AC-5.
        var result = OgPlungerService.Build(Inputs());

        Assert.Equal(result.FallTime + result.RiseTime + result.ShutInTime, result.CycleTime);
        Assert.Equal(PlungerLiftConstants.StandardShutInTime, result.ShutInTime);
        Assert.Equal(
            PlungerLiftConstants.MinutesPerDay / result.CycleTime,
            result.CyclesPerDay);

        // The phase times are the depth over the velocity, in minutes, for both directions.
        Assert.Equal(result.DepthFt / result.FallVelocityFpm, result.FallTime);
        Assert.Equal(result.DepthFt / result.RiseVelocityFpm, result.RiseTime);

        Assert.Contains("fall", result.CycleText, StringComparison.Ordinal);
        Assert.Contains("rise", result.CycleText, StringComparison.Ordinal);
        Assert.Contains("shut-in", result.CycleText, StringComparison.Ordinal);
        Assert.Contains("= cycle", result.CycleText, StringComparison.Ordinal);
        Assert.Contains("per day", result.CycleText, StringComparison.Ordinal);

        // The daily rate is the slug the cycle carries times the cycles it runs.
        Assert.Equal(result.RatePerCycle * result.CyclesPerDay, result.DailyRate);
    }

    [Fact]
    public void The_velocities_come_from_the_owner_and_the_limits_are_the_modules_own()
    {
        // The rise estimate is the calculator's, in ft/min, inside the constants' range by construction.
        var result = OgPlungerService.Build(Inputs());

        Assert.InRange(
            result.RiseVelocityFpm,
            PlungerLiftConstants.MinimumRiseVelocity,
            PlungerLiftConstants.MaximumRiseVelocity);
        Assert.InRange(
            result.FallVelocityFpm,
            PlungerLiftConstants.MinimumFallVelocity,
            PlungerLiftConstants.MaximumFallVelocity);

        // The fall velocity depends on the plunger type, and a different type moves it.
        var bar = OgPlungerService.Build(Inputs() with { PlungerType = PlungerTypes.All[0] });
        var pad = OgPlungerService.Build(Inputs() with { PlungerType = PlungerTypes.All[1] });

        Assert.NotEqual(bar.FallVelocityFpm, pad.FallVelocityFpm);

        // The criterion's range text is the module's own limit, not a number written in the screen.
        var rise = result.Criteria.Single(criterion => criterion.Code == "RISE_VELOCITY_RANGE");

        Assert.Contains(
            PlungerLiftConstants.MinimumRiseVelocity.ToString("#,0.##", CultureInfo.InvariantCulture),
            rise.RangeText,
            StringComparison.Ordinal);
        Assert.Contains(
            PlungerLiftConstants.MaximumRiseVelocity.ToString("#,0.##", CultureInfo.InvariantCulture),
            rise.RangeText,
            StringComparison.Ordinal);

        Assert.DoesNotContain("rise", bar.FailedText, StringComparison.Ordinal);
    }

    [Fact]
    public void The_plunger_types_come_from_the_owning_constants()
    {
        // AC-4.
        Assert.Equal(
            [
                PlungerLiftConstants.PlungerTypeBar,
                PlungerLiftConstants.PlungerTypePad,
                PlungerLiftConstants.PlungerTypeBrush,
                PlungerLiftConstants.PlungerTypeContinuous,
            ],
            PlungerTypes.All);

        Assert.Contains("Bar stock", PlungerTypes.NameOf(PlungerLiftConstants.PlungerTypeBar), StringComparison.Ordinal);
        Assert.Equal("Bar stock", OgPlungerService.Build(Inputs()).TypeText);

        // The view binds the workspace's list and names no type itself.
        var page = File.ReadAllText(SourceRoot.AllSources().Single(file => Path.GetFileName(file) == PageFileName));

        Assert.Contains("workspace.PlungerTypes", page, StringComparison.Ordinal);
        Assert.DoesNotContain("\"BAR\"", page, StringComparison.Ordinal);
        Assert.DoesNotContain("\"PAD\"", page, StringComparison.Ordinal);

        // An unknown type is refused rather than defaulted.
        Assert.Equal(PlungerRules.CodeTypeUnknown, PlungerRules.Validate(Inputs() with { PlungerType = "MAGNET" })!.Value.Code);
    }

    [Fact]
    public void A_saved_assessment_would_carry_the_type_and_the_criteria_outcome()
    {
        // AC-6 at the record's own surface: the values the save writes are the assessment's.
        var result = OgPlungerService.Build(Inputs());

        Assert.Equal(PlungerLiftConstants.PlungerTypeBar, result.PlungerType);
        Assert.Equal(result.Verdict, PlungerVerdicts.Feasible);
        Assert.True(result.Economic);
        Assert.Equal("no criterion failed", result.FailedText);

        // Every criterion code is in the text the row stores, so the outcome can be read without re-running the screen.
        foreach (var code in new[]
                 {
                     "GLR_RANGE", "PRESSURE_DIFFERENTIAL", "FALL_VELOCITY_RANGE", "RISE_VELOCITY_RANGE",
                     "CYCLE_TIME_RANGE", "SLUG_SIZE_RANGE",
                 })
        {
            Assert.Contains(code.ToLowerInvariant(), result.CriteriaText, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void The_units_are_stated_on_every_number()
    {
        var result = OgPlungerService.Build(Inputs());

        Assert.Equal("bbl/d", OgPlungerResult.RateUnit);
        Assert.Equal("bbl", OgPlungerResult.CycleUnit);
        Assert.Equal("ft/min", OgPlungerResult.VelocityUnit);
        Assert.Equal("min", OgPlungerResult.TimeUnit);
        Assert.Equal("scf/bbl", OgPlungerResult.GlrUnit);

        Assert.Contains("bbl/d", result.RateText, StringComparison.Ordinal);
        Assert.Contains("ft/min", result.FallText, StringComparison.Ordinal);
        Assert.Contains("ft/min", result.RiseText, StringComparison.Ordinal);
        Assert.Contains("scf/bbl", result.RatioText, StringComparison.Ordinal);
        Assert.Contains("bbl", result.PerCycleText, StringComparison.Ordinal);
    }

    [Fact]
    public void The_assessment_is_reproducible()
    {
        var first = OgPlungerService.Build(Inputs());
        var second = OgPlungerService.Build(Inputs());

        Assert.Equal(first.Describe(), second.Describe());
        Assert.Equal(first.CycleTime, second.CycleTime);
        Assert.Equal(first.DailyRate, second.DailyRate);
        Assert.Equal(first.CriteriaText, second.CriteriaText);
    }

    [Fact]
    public void The_inputs_are_validated_and_a_missing_ratio_is_named_as_the_deciding_one()
    {
        Assert.Equal(PlungerRules.CodeWellRequired, PlungerRules.Validate(Inputs() with { WellId = " " })!.Value.Code);
        Assert.Equal(PlungerRules.CodeInputRequired, PlungerRules.Validate(Inputs() with { DepthFt = null })!.Value.Code);
        Assert.Equal(PlungerRules.CodeInputInvalid, PlungerRules.Validate(Inputs() with { DepthFt = 0m })!.Value.Code);
        Assert.Equal(
            PlungerRules.CodeInputInvalid,
            PlungerRules.Validate(Inputs() with { LiquidRateBblPerDay = 0m })!.Value.Code);
        Assert.Equal(PlungerRules.CodeInputInvalid, PlungerRules.Validate(Inputs() with { CasingPressure = -1m })!.Value.Code);

        // The state table: a missing GLR is refused with the reason that it is the criterion that decides lift.
        var noRatio = PlungerRules.Validate(Inputs() with { GasLiquidRatio = null });

        Assert.Equal(PlungerRules.CodeInputRequired, noRatio!.Value.Code);
        Assert.Contains("gas-liquid ratio", noRatio.Value.Message, StringComparison.Ordinal);
        Assert.Contains("decides whether", noRatio.Value.Message, StringComparison.Ordinal);

        Assert.Contains(
            PlungerRules.Missing(Inputs() with { GasLiquidRatio = null }),
            item => item == "gas-liquid ratio");

        Assert.Null(PlungerRules.Validate(Inputs()));
    }

    [Fact]
    public async Task Commands_need_a_connection_and_an_actor_and_refuse_an_infeasible_save()
    {
        var service = new OgPlungerService(() => null, new Quiet());
        var feasible = OgPlungerService.Build(Inputs());
        var infeasible = OgPlungerService.Build(Inputs() with { GasLiquidRatio = 100m });

        Assert.Equal(PlungerRules.CodeNoConnection, (await service.LoadAsync()).Reference);
        Assert.Equal(PlungerRules.CodeActorRequired, (await service.SaveAsync(feasible, " ")).Reference);

        // An infeasible assessment is a finding, not an installation.
        var refused = await service.SaveAsync(infeasible, "sara");

        Assert.False(refused.Success);
        Assert.Equal(PlungerRules.CodeNothingToSave, refused.Reference);
        Assert.Contains("infeasible", refused.Message!, StringComparison.Ordinal);

        Assert.Equal(PlungerRules.CodeNoConnection, (await service.SaveAsync(feasible, "sara")).Reference);
        Assert.Equal(PlungerRules.CodeNothingPending, (await service.RetrySaveAsync("sara")).Reference);
        Assert.Null(service.Pending);
    }

    // ------------------------------------------------------------------ helpers

    /// <summary>A 6,000 ft well with 100 psi of casing-to-line differential, 3,000 scf/bbl and 10 bbl/d of liquid.</summary>
    private static OgPlungerInputs Inputs() =>
        new(
            "WELL-1",
            PlungerType: PlungerLiftConstants.PlungerTypeBar,
            DepthFt: 6000m,
            TubingDiameterIn: 2.375m,
            CasingPressure: 250m,
            LinePressure: 150m,
            GasRateMscfPerDay: 30m,
            LiquidRateBblPerDay: 10m,
            GasLiquidRatio: 3000m);

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
