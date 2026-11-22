using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.ProductionAccounting.Calculations;
using Beep.OilandGas.ProductionAccounting.Constants;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Accounting;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// ACC-08: the rate normalisation and the tax arithmetic, the remark contract, the fiscal-year gate, and the rules that refuse
/// a missing or not-yet-effective rate - the parts that need no database. The compute/post journey and the postings are proven
/// in the gateway test.
/// </summary>
public sealed class TaxTests
{
    private const string PageFileName = "TaxPageView.cs";

    [Fact]
    public void A_rate_is_normalised_the_owners_way()
    {
        // The owner's ProductionTaxService rule: above 1 is a percentage, at or below 1 is a fraction, zero is no rate.
        Assert.Equal(0.125m, TaxMath.NormalizeRate(12.5m));
        Assert.Equal(0.125m, TaxMath.NormalizeRate(0.125m));
        Assert.Equal(0.5m, TaxMath.NormalizeRate(50m));
        Assert.Equal(0.5m, TaxMath.NormalizeRate(0.5m));
        Assert.Null(TaxMath.NormalizeRate(0m));
        Assert.Null(TaxMath.NormalizeRate(null));

        // The tax on a basis at a normalised rate, rounded once, away from zero.
        Assert.Equal(700m, TaxMath.Amount(10_000m, 0.07m));
        Assert.Equal(393.75m, TaxMath.Amount(3_150m, 0.125m));
        Assert.Equal(0.01m, TaxMath.Amount(0.05m, 0.125m));

        // A negative basis, or a rate outside 0-1, is refused rather than computed.
        Assert.Throws<ArgumentOutOfRangeException>(() => TaxMath.Amount(-1m, 0.1m));
        Assert.Throws<ArgumentOutOfRangeException>(() => TaxMath.Amount(100m, 1.5m));
    }

    [Fact]
    public void A_deferred_provision_posts_its_movement_not_its_balance()
    {
        // The owner's RecordDeferredTaxBalance convention: the change against the prior period is what charges income tax
        // expense, so a second provision for the same period cannot double-count.
        var first = TaxMath.Movement(asset: 0m, liability: 1_000m, priorAsset: 0m, priorLiability: 0m);

        Assert.Equal(1_000m, first.LiabilityDelta);
        Assert.Equal(0m, first.AssetDelta);
        Assert.Equal(1_000m, first.Expense);
        Assert.True(first.Moved);

        var second = TaxMath.Movement(asset: 0m, liability: 1_200m, priorAsset: 0m, priorLiability: 1_000m);

        Assert.Equal(200m, second.LiabilityDelta);
        Assert.Equal(200m, second.Expense);

        // An asset increase is a benefit: it reduces the charge.
        var benefit = TaxMath.Movement(asset: 400m, liability: 0m, priorAsset: 0m, priorLiability: 0m);

        Assert.Equal(400m, benefit.AssetDelta);
        Assert.Equal(-400m, benefit.Expense);

        // A movement of nothing says so, and no journal is expected.
        var still = TaxMath.Movement(0m, 500m, 0m, 500m);

        Assert.False(still.Moved);
        Assert.Equal(0m, still.Expense);
    }

    [Fact]
    public void A_rate_row_remark_is_read_the_owners_way()
    {
        // The owner's ProductionTaxService splits the remark on ; , whitespace and matches keys case-insensitively.
        var remark = "JURISDICTION=TX; DEFERRED_TAX_RATE=21; PROPERTY_ID=PR-1";

        Assert.Equal("TX", TaxRemark.Value(remark, ProductionTaxRemarkKeys.Jurisdiction));
        Assert.Equal(21m, TaxRemark.Rate(remark, ProductionTaxRemarkKeys.DeferredTaxRate));
        Assert.Equal("PR-1", TaxRemark.Value(remark, ProductionTaxRemarkKeys.PropertyId));

        // Key casing, and the other separators, are the same format.
        Assert.Equal("TX", TaxRemark.Value("jurisdiction=TX", ProductionTaxRemarkKeys.Jurisdiction));
        Assert.Equal("TX", TaxRemark.Value("a=1,JURISDICTION=TX", ProductionTaxRemarkKeys.Jurisdiction));
        Assert.Equal("TX", TaxRemark.Value("a=1 JURISDICTION=TX", ProductionTaxRemarkKeys.Jurisdiction));

        // A token without a value or a key is ignored rather than becoming an empty jurisdiction.
        Assert.Null(TaxRemark.Value("JURISDICTION=;a=b", ProductionTaxRemarkKeys.Jurisdiction));
        Assert.Null(TaxRemark.Value("JURISDICTION", ProductionTaxRemarkKeys.Jurisdiction));
        Assert.Null(TaxRemark.Value(null, ProductionTaxRemarkKeys.Jurisdiction));

        // A non-numeric rate is absent, not zero, so AC-2 blocks rather than taxing at nothing.
        Assert.Null(TaxRemark.Rate("JURISDICTION=TX; DEFERRED_TAX_RATE=high", ProductionTaxRemarkKeys.DeferredTaxRate));
    }

    [Fact]
    public void A_missing_or_not_yet_effective_rate_blocks_the_computation()
    {
        // AC-2: the jurisdiction is named and the reader is told rates are configuration.
        var missing = TaxRules.CheckRate(
            "TX", recordedRate: null, effectiveFrom: null, periodEnd: new DateTime(2026, 12, 31),
            TaxComputationCodes.ProvisionCurrent);

        Assert.Equal(TaxRules.CodeNoRate, missing!.Value.Code);
        Assert.Contains("TX", missing.Value.Message, StringComparison.Ordinal);
        Assert.Contains("configuration", missing.Value.Message, StringComparison.Ordinal);

        // A rate recorded as zero is not a rate.
        Assert.Equal(
            TaxRules.CodeNoRate,
            TaxRules.CheckRate("TX", 0m, null, new DateTime(2026, 12, 31), TaxComputationCodes.ProvisionCurrent)!
                .Value.Code);

        // The rate-effective-date state: a rate that starts after the period is refused naming the date.
        var early = TaxRules.CheckRate(
            "TX", 21m, new DateTime(2027, 1, 1), new DateTime(2026, 12, 31), TaxComputationCodes.ProvisionCurrent);

        Assert.Equal(TaxRules.CodeRateNotEffective, early!.Value.Code);
        Assert.Contains("2027-01-01", early.Value.Message, StringComparison.Ordinal);
        Assert.Contains("2026-12-31", early.Value.Message, StringComparison.Ordinal);

        // Effective on or before the period end is fine.
        Assert.Null(TaxRules.CheckRate("TX", 21m, new DateTime(2026, 1, 1), new DateTime(2026, 12, 31),
            TaxComputationCodes.ProvisionCurrent));
        Assert.Null(TaxRules.CheckRate("TX", 21m, null, new DateTime(2026, 12, 31),
            TaxComputationCodes.ProvisionCurrent));
    }

    [Fact]
    public void A_provision_is_refused_until_its_fiscal_year_is_closed()
    {
        // AC-3 and SG-10-8: every month of the year has to be a closed period.
        var periods = new List<COMPANY_LEDGER_PERIOD>();
        var yearEnd = new DateTime(2026, 12, 31);

        var nothing = OgTaxService.IncompleteFiscalYear(yearEnd, periods);

        Assert.Contains("2026-01", nothing!, StringComparison.Ordinal);
        Assert.Contains("does not exist", nothing!, StringComparison.Ordinal);

        // Eleven closed months and one open: the open one is named.
        for (var month = 1; month <= 12; month++)
        {
            periods.Add(new COMPANY_LEDGER_PERIOD
            {
                PERIOD_CODE = new DateTime(2026, month, 1).ToString("yyyy-MM"),
                STATUS = month == 8 ? LedgerCodes.PeriodOpen : LedgerCodes.PeriodClosed,
            });
        }

        var august = OgTaxService.IncompleteFiscalYear(yearEnd, periods);

        Assert.Contains("2026-08", august!, StringComparison.Ordinal);
        Assert.Contains("open", august!, StringComparison.Ordinal);

        // Locked counts as complete too, and the period-end month may still be open - the provision is one of its
        // closing entries, so demanding December be closed first would make a year-end provision unpostable.
        periods[7].STATUS = LedgerCodes.PeriodLocked;
        periods[11].STATUS = LedgerCodes.PeriodOpen;

        Assert.Null(OgTaxService.IncompleteFiscalYear(yearEnd, periods));

        // Any other open month still blocks it.
        periods[10].STATUS = LedgerCodes.PeriodOpen;

        Assert.Contains("2026-11", OgTaxService.IncompleteFiscalYear(yearEnd, periods)!, StringComparison.Ordinal);

        periods[10].STATUS = LedgerCodes.PeriodClosed;

        // A missing month is a different answer than an open one.
        periods.RemoveAt(4);

        var may = OgTaxService.IncompleteFiscalYear(yearEnd, periods);

        Assert.Contains("2026-05", may!, StringComparison.Ordinal);
        Assert.Contains("does not exist", may!, StringComparison.Ordinal);
    }

    [Fact]
    public void A_computation_needs_its_jurisdiction_kind_period_and_basis()
    {
        Assert.Equal(
            ["jurisdiction", "the tax kind", "the period", "currency", "gross revenue"],
            TaxRules.Missing(new OgTaxInputs(null, null, null, null, null, null, null, null, null, null)));

        Assert.Empty(TaxRules.Missing(Production()));
        Assert.Empty(TaxRules.Missing(Current()));

        // A deferred provision needs both closing balances; a current one needs taxable income.
        Assert.Contains("the closing deferred tax asset",
            TaxRules.Missing(Deferred() with { DeferredTaxAsset = null }));
        Assert.Contains("taxable income", TaxRules.Missing(Current() with { TaxableIncome = null }));

        Assert.Equal(
            TaxRules.CodeCurrencyMismatch,
            TaxRules.Validate(Production() with { Currency = "XYZ" })!.Value.Code);
        Assert.Equal(
            TaxRules.CodeInputInvalid,
            TaxRules.Validate(Production() with { Kind = "PICNIC" })!.Value.Code);
        Assert.Equal(
            TaxRules.CodeInputInvalid,
            TaxRules.Validate(Production() with
            {
                PeriodFrom = new DateTime(2026, 12, 31),
                PeriodTo = new DateTime(2026, 1, 1),
            })!.Value.Code);

        // A negative basis is refused.
        Assert.Equal(
            TaxRules.CodeInputInvalid,
            TaxRules.Validate(Production() with { GrossRevenue = -1m })!.Value.Code);
    }

    [Fact]
    public void A_computation_says_whether_it_names_its_basis()
    {
        var computation = Computation();

        Assert.True(computation.NamesItsBasis);
        Assert.Contains("12.5 %", computation.RateText, StringComparison.Ordinal);
        Assert.Contains("GOVERNMENTAL_TAX_DATA", computation.RateText, StringComparison.Ordinal);
        Assert.Contains("2026-01-01", computation.RateText, StringComparison.Ordinal);
        Assert.Contains("gross revenue 10,000.00 USD", computation.BasisDescription, StringComparison.Ordinal);
        Assert.Contains("2026-01-01 to 2026-12-31", computation.PeriodText, StringComparison.Ordinal);

        // A row missing the rate source or its effective date is caught rather than displayed as complete.
        Assert.False((computation with { RateSource = null }).NamesItsBasis);
        Assert.False((computation with { EffectiveFrom = null }).NamesItsBasis);
        Assert.False((computation with { Rate = 0m }).NamesItsBasis);

        Assert.False(computation.IsPosted);
        Assert.Contains("not posted", computation.JournalText, StringComparison.Ordinal);
        Assert.Contains("journal J1", (computation with { JournalId = "J1", PostedBy = "nadia" }).JournalText,
            StringComparison.Ordinal);
    }

    [Fact]
    public void The_page_has_no_rate_input()
    {
        // AC-5: rates are configuration. The view shows the rate table read-only and offers no field for a rate.
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        Assert.Contains("workspace.RateTableText", page, StringComparison.Ordinal);
        Assert.Contains("workspace.Jurisdictions", page, StringComparison.Ordinal);
        Assert.Contains("computation.RateText", page, StringComparison.Ordinal);
        Assert.Contains("computation.NamesItsBasis", page, StringComparison.Ordinal);

        // No editable rate control of any kind, and no writing of the rate table.
        Assert.DoesNotContain("Field(\"Severance", page, StringComparison.Ordinal);
        Assert.DoesNotContain("Field(\"Ad valorem", page, StringComparison.Ordinal);
        Assert.DoesNotContain("Field(\"Rate", page, StringComparison.Ordinal);
        Assert.DoesNotContain("Field(\"Income", page, StringComparison.Ordinal);
        Assert.DoesNotContain("GOVERNMENTAL_TAX_DATA", page, StringComparison.Ordinal);
    }

    [Fact]
    public void The_tax_kinds_are_the_owners_with_the_provisions_added()
    {
        // The production kinds are the owner's own codes; the provision kinds are the ones it did not have.
        Assert.Contains(ProductionTaxAssessmentCodes.Severance, TaxComputationCodes.Kinds);
        Assert.Contains(ProductionTaxAssessmentCodes.AdValorem, TaxComputationCodes.Kinds);
        Assert.Contains(TaxComputationCodes.ProvisionCurrent, TaxComputationCodes.Kinds);
        Assert.Contains(TaxComputationCodes.ProvisionDeferred, TaxComputationCodes.Kinds);

        Assert.True(TaxComputationCodes.IsProvision(TaxComputationCodes.ProvisionCurrent));
        Assert.True(TaxComputationCodes.IsProvision(TaxComputationCodes.ProvisionDeferred));
        Assert.False(TaxComputationCodes.IsProvision(ProductionTaxAssessmentCodes.Severance));

        Assert.Equal("current tax provision", TaxComputationCodes.Describe(TaxComputationCodes.ProvisionCurrent));
        Assert.Equal(["COMPUTED", "POSTED"], TaxComputationCodes.Statuses);

        // The accounts come from the configured chart, and are the ones the owner's own provision service posts to.
        Assert.Equal("6150", TaxPostingCodes.IncomeTaxExpenseAccount);
        Assert.Equal("2101", TaxPostingCodes.IncomeTaxPayableAccount);
        Assert.Equal("1160", TaxPostingCodes.DeferredTaxAssetAccount);
        Assert.Equal("2102", TaxPostingCodes.DeferredTaxLiabilityAccount);
    }

    // ------------------------------------------------------------------ helpers

    private static OgTaxInputs Production() =>
        new("TX", ProductionTaxAssessmentCodes.Severance, new DateTime(2026, 1, 1), new DateTime(2026, 12, 31),
            "USD", GrossRevenue: 10_000m, TaxableIncome: null, DeferredTaxAsset: null, DeferredTaxLiability: null,
            PropertyId: "PR-1");

    private static OgTaxInputs Current() =>
        Production() with
        {
            Kind = TaxComputationCodes.ProvisionCurrent,
            GrossRevenue = null,
            TaxableIncome = 100_000m,
        };

    private static OgTaxInputs Deferred() =>
        Production() with
        {
            Kind = TaxComputationCodes.ProvisionDeferred,
            GrossRevenue = null,
            TaxableIncome = 100_000m,
            DeferredTaxAsset = 0m,
            DeferredTaxLiability = 5_000m,
        };

    private static OgTaxComputation Computation() =>
        new(
            "T-1",
            "TX",
            ProductionTaxAssessmentCodes.Severance,
            new DateTime(2026, 12, 31),
            10_000m,
            "gross revenue",
            0.125m,
            1_250m,
            "USD",
            new DateTime(2026, 1, 1),
            "GOVERNMENTAL_TAX_DATA GT-1 · SEVERANCE_TAX",
            new DateTime(2026, 1, 1),
            new DateTime(2026, 12, 31),
            "2026",
            TaxComputationCodes.StatusComputed,
            "PR-1",
            "sara",
            null,
            null,
            null,
            "severance tax for TX");
}
