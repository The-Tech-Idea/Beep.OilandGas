using Beep.OilandGas.EconomicAnalysis.Calculations;
using Beep.OilandGas.EconomicAnalysis.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Economics;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// ECO-01: the closed-form NPV reference, the undefined IRR, decimal money, the missing fiscal term, the sensitivity
/// re-using the base engine, approval immutability and the scenario comparison - the parts that need no database. The
/// persistence, the scenario table and the approval round trip are proven in the gateway test.
/// </summary>
public sealed class EconomicsTests
{
    private const string ServiceFile = "OgEconomics.cs";

    [Fact]
    public void A_flat_reference_case_matches_the_closed_form_npv()
    {
        // AC-1 and SG-09-1. Ten flat years of 100 at 10 % with no capex, no fiscal terms and no escalation is the textbook
        // annuity: NPV = A (1 - (1 + r)^-n) / r. The engine sums the discounted terms; the closed form is computed here from
        // the annuity formula, so the two arrive at the answer differently.
        var result = OgEconomicsService.Build(
            Flat(),
            new FiscalRegime("none", 0m, 0m),
            "BASE");

        Assert.Equal(11, result.Periods.Count);
        Assert.Equal(10, result.Periods.Count(period => period.Period > 0));

        // (1.1)^10 by repeated decimal multiplication, then the annuity factor.
        var growth = 1m;
        for (var step = 0; step < 10; step++)
        {
            growth *= 1.10m;
        }

        var annuity = (1m - 1m / growth) / 0.10m;
        var expected = 100m * annuity;

        Assert.InRange(Math.Abs(result.Npv - expected), 0m, 0.000001m);

        // The published value of that annuity to four places, so the test is not only self-consistent.
        Assert.InRange(result.Npv, 614.4567m, 614.4568m);

        // The undiscounted total is the flat sum; nothing is ever out of pocket, so payback is immediate.
        Assert.Equal(1000m, result.UndiscountedTotal);
        Assert.Equal(0m, result.PeakFunding);
        Assert.Equal(0m, result.PaybackMonths);
    }

    [Fact]
    public void An_irr_that_does_not_exist_is_undefined_not_zero()
    {
        // AC-2: every flow positive - no rate can make the NPV zero.
        var positive = OgEconomicsService.Build(Flat(), new FiscalRegime("none", 0m, 0m), "BASE");

        Assert.False(positive.IrrDefined);
        Assert.Null(positive.Irr);
        Assert.Contains("undefined", positive.IrrText, StringComparison.Ordinal);
        Assert.DoesNotContain("0 %", positive.IrrText, StringComparison.Ordinal);
        Assert.Equal("undefined", positive.IrrStoredText);

        // A project with a capital outflow does have one, and it really is where the NPV is zero.
        var invested = OgEconomicsService.Build(
            Flat() with { AnnualCapex = 500m, UnitPrice = 100m, AnnualVolume = 1m },
            new FiscalRegime("none", 0m, 0m),
            "BASE");

        Assert.True(invested.IrrDefined);
        Assert.NotNull(invested.Irr);

        var money = invested.Periods
            .Select(period => new EconomicMoneyCalculator.MoneyPeriod(period.Period, period.Net))
            .ToList();

        Assert.InRange(Math.Abs(EconomicMoneyCalculator.Npv(money, invested.Irr!.Value)), 0m, 0.01m);

        // The engine agrees with its own bounds: this project recovers 500 against 100 a year, so the rate is positive.
        Assert.InRange(invested.Irr.Value, 0.14m, 0.16m);

        // A project that spends and never produces has no inflow at all, so there is no rate either.
        var barren = OgEconomicsService.Build(
            Flat() with { AnnualCapex = 1000m, AnnualVolume = 0m },
            new FiscalRegime("none", 0m, 0m),
            "BASE");

        Assert.False(barren.IrrDefined);
        Assert.Contains("undefined", barren.IrrStoredText, StringComparison.Ordinal);
        Assert.Equal(-1000m, barren.Npv);
    }

    [Fact]
    public void All_money_is_decimal_end_to_end()
    {
        // AC-3: the money the screen shows is decimal, and neither the decimal engine nor the service holds a double.
        var result = OgEconomicsService.Build(
            Flat() with { AnnualCapex = 500m },
            new FiscalRegime("Concession", 0.125m, 0.30m),
            "BASE");

        Assert.IsType<decimal>(result.Npv);
        Assert.IsType<decimal>(result.PeakFunding);
        Assert.IsType<decimal>(result.UndiscountedTotal);
        Assert.IsType<decimal>(result.RoyaltyRate);
        Assert.IsType<decimal>(result.TaxRate);

        Assert.All(result.Periods, period =>
        {
            Assert.IsType<decimal>(period.Revenue);
            Assert.IsType<decimal>(period.Royalty);
            Assert.IsType<decimal>(period.Opex);
            Assert.IsType<decimal>(period.Capex);
            Assert.IsType<decimal>(period.Tax);
            Assert.IsType<decimal>(period.Net);
            Assert.IsType<decimal>(period.Discounted);
            Assert.IsType<decimal>(period.Cumulative);
        });

        Assert.All(result.Drivers, driver =>
        {
            Assert.IsType<decimal>(driver.LowNpv);
            Assert.IsType<decimal>(driver.BaseNpv);
            Assert.IsType<decimal>(driver.HighNpv);
            Assert.IsType<decimal>(driver.Swing);
        });

        // The desktop service holds no double at all.
        var service = File.ReadAllText(
            SourceRoot.AllSources().Single(source => Path.GetFileName(source) == ServiceFile));

        Assert.DoesNotContain("double", service, StringComparison.Ordinal);
        Assert.DoesNotContain("Math.Pow", service, StringComparison.Ordinal);

        // The decimal engine is in the owning project, so its surface is checked directly: no method takes or returns a
        // double, and the NPV is decimal.
        var methods = typeof(EconomicMoneyCalculator).GetMethods();

        Assert.NotEmpty(methods);
        Assert.All(methods, method =>
        {
            Assert.NotEqual(typeof(double), method.ReturnType);
            Assert.All(method.GetParameters(), parameter =>
                Assert.NotEqual(typeof(double), parameter.ParameterType));
        });

        Assert.Equal(typeof(decimal), typeof(EconomicMoneyCalculator)
            .GetMethod(nameof(EconomicMoneyCalculator.Npv))!.ReturnType);
    }

    [Fact]
    public void A_missing_fiscal_term_blocks_the_run_with_its_name()
    {
        // AC-4 and SG-09-2.
        var noRoyalty = EconomicRules.Validate(Flat(), new FiscalRegime("Concession", null, 0.30m));

        Assert.Equal(EconomicRules.CodeFiscalTermMissing, noRoyalty!.Value.Code);
        Assert.Contains("royalty rate", noRoyalty.Value.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("tax rate missing", noRoyalty.Value.Message, StringComparison.Ordinal);

        var noTax = EconomicRules.Validate(Flat(), new FiscalRegime("Concession", 0.125m, null));

        Assert.Equal(EconomicRules.CodeFiscalTermMissing, noTax!.Value.Code);
        Assert.Contains("tax rate", noTax.Value.Message, StringComparison.Ordinal);

        var neither = EconomicRules.Validate(Flat(), new FiscalRegime("Concession", null, null));

        Assert.Equal(EconomicRules.CodeFiscalTermMissing, neither!.Value.Code);
        Assert.Contains("royalty rate", neither.Value.Message, StringComparison.Ordinal);
        Assert.Contains("tax rate", neither.Value.Message, StringComparison.Ordinal);

        // A stated zero is a value, not a missing term.
        Assert.True(new FiscalRegime("none", 0m, 0m).IsComplete);
        Assert.Null(EconomicRules.Validate(Flat(), new FiscalRegime("none", 0m, 0m)));

        // A rate outside [0, 1] is refused rather than read as 1250 %.
        var outOfRange = EconomicRules.Validate(Flat(), new FiscalRegime("Concession", 12.5m, 0.30m));

        Assert.Equal(EconomicRules.CodeFiscalTermOutOfRange, outOfRange!.Value.Code);
        Assert.Contains("0.125", outOfRange.Value.Message, StringComparison.Ordinal);

        // The sentence names every missing term, so a blocked panel can print it as it stands.
        var regime = new FiscalRegime("Concession", null, null);

        Assert.Contains("royalty rate", regime.BlockedReason, StringComparison.Ordinal);
        Assert.Contains("tax rate", regime.BlockedReason, StringComparison.Ordinal);
        Assert.Equal(["royalty rate", "tax rate"], FiscalRegime.RequiredTerms);
    }

    [Fact]
    public void The_sensitivity_re_uses_the_base_engine()
    {
        // AC-5: every driver's base NPV is the run's own NPV, and its low and high cases are the same BuildPeriods plus the
        // same NPV - only one input scaled.
        var inputs = Flat() with { AnnualCapex = 500m, OpexPerUnit = 12m };
        var regime = new FiscalRegime("Concession", 0.125m, 0.30m);
        var result = OgEconomicsService.Build(inputs, regime, "BASE");

        Assert.Equal(5, result.Drivers.Count);
        Assert.All(result.Drivers, driver => Assert.Equal(result.Npv, driver.BaseNpv));

        // Largest swing first, so the tornado order is the table's order.
        var swings = result.Drivers.Select(driver => driver.SwingMagnitude).ToList();

        Assert.Equal(swings.OrderByDescending(value => value), swings);

        // Recompute the price driver from the public building blocks: it must agree with what the run reported.
        var price = result.Drivers.Single(driver => driver.Name == "unit price");
        var lowInputs = inputs with { UnitPrice = inputs.UnitPrice!.Value * 0.90m };
        var lowPeriods = OgEconomicsService.BuildPeriods(lowInputs, regime);
        var expectedLow = EconomicMoneyCalculator.Npv(
            lowPeriods.Select(period => new EconomicMoneyCalculator.MoneyPeriod(period.Period, period.Net)).ToList(),
            lowInputs.DiscountRate!.Value);

        Assert.Equal(expectedLow, price.LowNpv);

        // The discount driver really moves: its low and high differ, which a base-rate re-application would not show.
        var discount = result.Drivers.Single(driver => driver.Name == "discount rate");

        Assert.NotEqual(discount.LowNpv, discount.HighNpv);

        // A driver's sentence carries all three numbers.
        Assert.Contains("base", price.Describe(), StringComparison.Ordinal);
        Assert.Contains("swing", price.Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public void The_scenario_comparison_states_one_forecast_per_scenario()
    {
        // SG-09-3.
        var first = OgEconomicsService.Build(Flat(), new FiscalRegime("none", 0m, 0m), "BASE");
        var second = OgEconomicsService.Build(Flat() with { ForecastVersion = "FC-2026-02" },
            new FiscalRegime("none", 0m, 0m), "BEST");

        var workspace = new OgEconomicsWorkspace(
            [first, second], null, ["BASE", "BEST"], EconomicRules.Currencies, EconomicRules.VolumeUnits,
            new FiscalRegime("none", 0m, 0m), false, null);

        Assert.Contains("BASE: NPV", workspace.ComparisonText, StringComparison.Ordinal);
        Assert.Contains("forecast FC-2026-01", workspace.ComparisonText, StringComparison.Ordinal);
        Assert.Contains("forecast FC-2026-02", workspace.ComparisonText, StringComparison.Ordinal);
        Assert.DoesNotContain("not satisfied", workspace.ComparisonText, StringComparison.Ordinal);

        // Two versions for one scenario is flagged, not silently averaged.
        var conflicting = new OgEconomicsWorkspace(
            [first, second with { Scenario = "BASE" }], null, ["BASE"], EconomicRules.Currencies,
            EconomicRules.VolumeUnits, new FiscalRegime("none", 0m, 0m), false, null);

        Assert.Contains("one forecast per scenario is not satisfied for: BASE", conflicting.ComparisonText,
            StringComparison.Ordinal);
    }

    [Fact]
    public void An_approved_evaluation_is_immutable_and_states_its_forecast_version()
    {
        // AC-6 at the record's surface: the saved row carries the forecast version, and a save of an approved one is
        // refused before it reaches the database.
        var result = OgEconomicsService.Build(Flat(), new FiscalRegime("none", 0m, 0m), "BASE");

        Assert.Equal("FC-2026-01", result.ForecastVersion);
        Assert.Equal(OgEconomicResult.StatusDraft, result.Status);
        Assert.False(result.IsApproved);
        Assert.Contains("forecast FC-2026-01", result.ScenarioText, StringComparison.Ordinal);

        var approved = result with
        {
            Id = "E1",
            Status = OgEconomicResult.StatusApproved,
            ApprovedBy = "sara",
            ApprovedAt = new DateTime(2026, 5, 1),
        };

        Assert.True(approved.IsApproved);
        Assert.Contains("APPROVED by sara", approved.StatusText, StringComparison.Ordinal);

        var service = new OgEconomicsService(() => null, new Quiet());
        var refused = service.SaveAsync(approved, "sara").GetAwaiter().GetResult();

        Assert.False(refused.Success);
        Assert.Equal(EconomicRules.CodeApprovedImmutable, refused.Reference);
        Assert.Contains("immutable", refused.Message!, StringComparison.Ordinal);
    }

    [Fact]
    public void The_inputs_are_validated_and_a_currency_mismatch_is_refused()
    {
        // The state table's "currency mismatch - refused".
        var currency = EconomicRules.Validate(
            Flat() with { Currency = "XYZ" },
            new FiscalRegime("none", 0m, 0m));

        Assert.Equal(EconomicRules.CodeCurrencyMismatch, currency!.Value.Code);
        Assert.Contains("USD", currency.Value.Message, StringComparison.Ordinal);

        Assert.Equal(
            EconomicRules.CodeEntityRequired,
            EconomicRules.Validate(Flat() with { EntityId = " " }, new FiscalRegime("none", 0m, 0m))!.Value.Code);

        Assert.Equal(
            EconomicRules.CodeInputRequired,
            EconomicRules.Validate(Flat() with { ForecastVersion = null }, new FiscalRegime("none", 0m, 0m))!.Value.Code);

        Assert.Equal(
            EconomicRules.CodeInputInvalid,
            EconomicRules.Validate(Flat() with { DiscountRate = 10m }, new FiscalRegime("none", 0m, 0m))!.Value.Code);

        Assert.Equal(
            EconomicRules.CodeInputInvalid,
            EconomicRules.Validate(Flat() with { PeriodCount = 0 }, new FiscalRegime("none", 0m, 0m))!.Value.Code);

        Assert.Contains("volume unit", EconomicRules.Missing(Flat() with { VolumeUnit = null }));
        Assert.Null(EconomicRules.Validate(Flat(), new FiscalRegime("none", 0m, 0m)));
    }

    [Fact]
    public async Task Commands_need_a_connection_and_an_actor()
    {
        var service = new OgEconomicsService(() => null, new Quiet());
        var result = OgEconomicsService.Build(Flat(), new FiscalRegime("none", 0m, 0m), "BASE");

        Assert.Equal(EconomicRules.CodeNoConnection, (await service.LoadAsync()).Reference);
        Assert.Equal(EconomicRules.CodeActorRequired, (await service.SaveAsync(result, " ")).Reference);
        Assert.Equal(EconomicRules.CodeNoConnection, (await service.SaveAsync(result, "sara")).Reference);
        Assert.Equal(EconomicRules.CodeActorRequired, (await service.ApproveAsync(result.Id, " ")).Reference);
        Assert.Equal(EconomicRules.CodeNoConnection, (await service.ApproveAsync("E1", "sara")).Reference);
        Assert.Equal(EconomicRules.CodeNothingPending, (await service.RetrySaveAsync("sara")).Reference);
        Assert.Null(service.Pending);
    }

    // ------------------------------------------------------------------ helpers

    /// <summary>Ten flat years of one unit at 100, with nothing spent and no fiscal terms.</summary>
    private static OgEconomicInputs Flat() =>
        new(
            "WELL-1", "BASE", "FC-2026-01", "USD",
            DiscountRate: 0.10m,
            RegimeName: "none",
            RoyaltyRate: 0m,
            TaxRate: 0m,
            PeriodCount: 10,
            StartYear: 2026,
            AnnualVolume: 1m,
            VolumeUnit: "bbl",
            UnitPrice: 100m,
            PriceUnit: "USD/bbl",
            OpexPerUnit: 0m,
            AnnualCapex: 0m,
            EscalationRate: 0m,
            AbandonmentCost: 0m);

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
