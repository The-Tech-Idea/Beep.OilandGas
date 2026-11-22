using TheTechIdea.Beep.OilandGas.Desktop.Data.Lease;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// LSE-03: the NRI arithmetic, the rate precision, the expiry buckets and the validation rules - the parts that do
/// not need a database. The stored terms, obligations, payments and the renewal history are proven in the gateway
/// test.
/// </summary>
public sealed class LeaseFinancialTests
{
    private static readonly DateTime AsOf = new(2026, 6, 1);

    [Fact]
    public void The_nri_is_derived_and_a_sum_above_gross_is_refused()
    {
        var ok = OgLeaseNri.Compute(0.125m, 0.05m, [new OgLeaseBurden("landowner", 0.05m)], null, "LEASE");

        Assert.True(ok.Success);
        Assert.Equal(0.775m, ok.NetRevenueInterest);

        var refused = OgLeaseNri.Compute(0.8m, 0.3m, [], null, "LEASE");

        Assert.False(refused.Success);
        Assert.Equal(OgLeaseNri.CodeUnrepresentable, refused.Code);
        Assert.Contains("below zero", refused.Message!, StringComparison.Ordinal);

        // A net-profit lease records its NRI directly, so the subtraction rule does not apply to it.
        Assert.True(OgLeaseNri.Compute(0.8m, 0.3m, [], null, "NET_PROFIT").Success);
    }

    [Fact]
    public void A_supplied_nri_must_match_the_deductions()
    {
        var refused = OgLeaseNri.Compute(0.125m, 0.05m, [], 0.8m, "LEASE");

        Assert.False(refused.Success);
        Assert.Equal(OgLeaseNri.CodeUnrepresentable, refused.Code);
        Assert.Contains("0.825", refused.Message!, StringComparison.Ordinal);

        var ok = OgLeaseNri.Compute(0.125m, 0.05m, [], 0.825m, "LEASE");

        Assert.True(ok.Success);
        Assert.Equal(0.825m, ok.NetRevenueInterest);
    }

    [Fact]
    public void A_rate_is_a_fraction_with_at_most_eight_decimal_places()
    {
        Assert.Equal(OgLeaseNri.CodePrecision, OgLeaseNri.Compute(0.123456789m, 0m, [], null, "LEASE").Code);
        Assert.Equal(OgLeaseNri.CodeFraction, OgLeaseNri.Compute(1.2m, 0m, [], null, "LEASE").Code);
        Assert.Equal(OgLeaseNri.CodeFraction, OgLeaseNri.Compute(-0.1m, 0m, [], null, "LEASE").Code);
        Assert.Equal(
            OgLeaseNri.CodePrecision,
            OgLeaseNri.Compute(0.125m, 0m, [new OgLeaseBurden("bad", 0.123456789m)], null, "LEASE").Code);
    }

    [Fact]
    public void The_royalty_text_keeps_the_eight_place_fraction()
    {
        var terms = new OgLeaseTerms(
            1000m,
            "USD",
            0.125m,
            0.825m,
            0.05m,
            [new OgLeaseBurden("landowner", 0.05m)],
            AsOf,
            AsOf.AddYears(1));

        Assert.Equal("0.12500000", terms.RoyaltyText);
        Assert.Equal("12.5%", terms.RoyaltyPercentText);
        Assert.Contains("landowner 5%", terms.BurdensText, StringComparison.Ordinal);
    }

    [Fact]
    public void An_obligation_is_overdue_from_its_due_date_and_a_payment_clears_it()
    {
        var overdue = new OgLeaseObligation(
            "O-1", OgLeaseObligation.Rental, "Annual rental", AsOf.AddDays(-3), 500m, "USD",
            Paid: false, Overdue: true, DaysOverdue: 3);

        Assert.Equal("overdue 3 d", overdue.StatusText);
        Assert.Contains("500 USD", overdue.AmountText, StringComparison.Ordinal);

        var paid = overdue with { Paid = true, Overdue = false, DaysOverdue = null };

        Assert.Equal("paid", paid.StatusText);
    }

    [Fact]
    public void The_expiry_calendar_uses_the_30_60_90_buckets()
    {
        var leases = new[]
        {
            Summary("L-30", AsOf.AddDays(10), 10),
            Summary("L-60", AsOf.AddDays(45), 45),
            Summary("L-90", AsOf.AddDays(80), 80),
            Summary("L-later", AsOf.AddDays(200), 200),
            Summary("L-expired", AsOf.AddDays(-1), -1),
        };

        var buckets = OgLeaseFinancialService.BuildExpiry(leases, AsOf);

        Assert.Equal(3, buckets.Count);
        Assert.Equal("L-30", Assert.Single(buckets[0].Leases).Id);
        Assert.Equal("L-60", Assert.Single(buckets[1].Leases).Id);
        Assert.Equal("L-90", Assert.Single(buckets[2].Leases).Id);
        Assert.DoesNotContain(buckets.SelectMany(bucket => bucket.Leases), lease => lease.Id == "L-later");
        Assert.DoesNotContain(buckets.SelectMany(bucket => bucket.Leases), lease => lease.Id == "L-expired");
    }

    [Fact]
    public void The_expiry_state_comes_from_the_days_to_expiry()
    {
        Assert.Equal(OgLeaseFinancialSummary.Expired, Summary("L-1", AsOf.AddDays(-1), -1).ExpiryState);
        Assert.Equal(OgLeaseFinancialSummary.Expiring, Summary("L-2", AsOf.AddDays(45), 45).ExpiryState);
        Assert.Equal(OgLeaseFinancialSummary.Active, Summary("L-3", AsOf.AddDays(200), 200).ExpiryState);
        Assert.Equal(OgLeaseFinancialSummary.Unclassified, Summary("L-4", null, null).ExpiryState);
    }

    [Fact]
    public void A_bonus_without_a_currency_says_so()
    {
        var terms = new OgLeaseTerms(1000m, null, 0.125m, 0.875m, 0m, [], null, null);

        Assert.Contains("(currency not recorded)", terms.BonusText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_renewal_needs_an_actor_a_rationale_and_a_new_date()
    {
        var service = new OgLeaseFinancialService(() => null, new Quiet());

        Assert.Equal(
            OgLeaseFinancialService.CodeActorRequired,
            (await service.RenewAsync("L-1", AsOf.AddYears(1), "why", " ")).Reference);

        Assert.Equal(
            OgLeaseFinancialService.CodeRationaleRequired,
            (await service.RenewAsync("L-1", AsOf.AddYears(1), " ", "sara")).Reference);

        Assert.Equal(
            OgLeaseFinancialService.CodeExpiryRequired,
            (await service.RenewAsync("L-1", default, "why", "sara")).Reference);

        Assert.Equal(
            OgLeaseFinancialService.CodeNothingPending,
            (await service.RetryAsync(AsOf)).Reference);
    }

    [Fact]
    public async Task A_payment_needs_a_positive_amount_and_a_currency()
    {
        var service = new OgLeaseFinancialService(() => null, new Quiet());

        Assert.Equal(
            OgLeaseFinancialService.CodeActorRequired,
            (await service.RecordPaymentAsync("L-1", 10m, "USD", "RENTAL", null, null, " ")).Reference);

        Assert.Equal(
            OgLeaseFinancialService.CodeAmountInvalid,
            (await service.RecordPaymentAsync("L-1", 0m, "USD", "RENTAL", null, null, "sara")).Reference);

        Assert.Equal(
            OgLeaseFinancialService.CodeCurrencyRequired,
            (await service.RecordPaymentAsync("L-1", 10m, " ", "RENTAL", null, null, "sara")).Reference);
    }

    [Fact]
    public async Task An_obligation_needs_a_description_and_a_due_date()
    {
        var service = new OgLeaseFinancialService(() => null, new Quiet());

        Assert.Equal(
            "lease-financial-obligation-required",
            (await service.RecordObligationAsync("L-1", "RENTAL", " ", AsOf, 0m, "USD", "sara")).Reference);

        Assert.Equal(
            OgLeaseFinancialService.CodeAmountInvalid,
            (await service.RecordObligationAsync("L-1", "RENTAL", "Annual", AsOf, -1m, "USD", "sara")).Reference);

        Assert.Equal(
            OgLeaseFinancialService.CodeCurrencyRequired,
            (await service.RecordObligationAsync("L-1", "RENTAL", "Annual", AsOf, 10m, " ", "sara")).Reference);
    }

    private static OgLeaseFinancialSummary Summary(string id, DateTime? expiry, int? days) =>
        new(
            id,
            "Lease " + id,
            "NORTH",
            AsOf,
            expiry,
            days,
            0.125m,
            0.775m,
            0.05m,
            1000m,
            "USD",
            0,
            null,
            "ACTIVE");

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
