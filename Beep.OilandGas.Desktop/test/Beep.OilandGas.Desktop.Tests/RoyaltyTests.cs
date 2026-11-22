using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.ProductionAccounting.Calculations;
using Beep.OilandGas.ProductionAccounting.Constants;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Accounting;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// ACC-07: the royalty arithmetic, the ownership-fraction gate, the JIB net, the allocation split and the records' own state -
/// the parts that need no database. The calculate/pay/dispute and raise/approve/post journey is proven in the gateway test.
/// </summary>
public sealed class RoyaltyTests
{
    private const string PageFileName = "RoyaltiesPageView.cs";

    [Fact]
    public void A_royalty_is_net_revenue_times_the_rate()
    {
        // AC-1: the amount is the net revenue times the rate, in decimal, and it is reproducible from its own row.
        var answer = RoyaltyMath.Calculate(grossValue: 10_000m, deductions: 1_000m, ratePercent: 12.5m);

        Assert.Equal(9_000m, answer.NetRevenue);
        Assert.Equal(0.125m, answer.Rate);
        Assert.Equal(1_125m, answer.Amount);

        // The same answer in the other form: value x rate less deductions x rate. Taken literally, "value x rate - the whole
        // deduction" would charge the mineral owner 100 % of the cost instead of their rate share, so the rate is applied to
        // the net - which is the owner's documented relation and algebraically the same as deducting the rate share.
        var byValueMinusDeductionShare =
            RoyaltyMath.Round(10_000m * 0.125m) - RoyaltyMath.Round(1_000m * 0.125m);

        Assert.Equal(answer.Amount, byValueMinusDeductionShare);
        Assert.NotEqual(RoyaltyMath.Round(10_000m * 0.125m) - 1_000m, answer.Amount);

        // A half-cent is rounded once, away from zero, so the stored figure is the figure the row derives.
        var odd = RoyaltyMath.Calculate(100m, 0m, 12.5m);

        Assert.Equal(12.5m, odd.Amount);
        Assert.Equal(RoyaltyMath.Round(odd.NetRevenue * (12.5m / 100m)), odd.Amount);

        // The gross value is the volume at the price, rounded once.
        Assert.Equal(3_500m, RoyaltyMath.GrossValue(50m, 70m));

        // A rate outside 0-100, a negative value or deductions above the value are refused rather than computed.
        Assert.Throws<ArgumentOutOfRangeException>(() => RoyaltyMath.Calculate(100m, 0m, 101m));
        Assert.Throws<ArgumentOutOfRangeException>(() => RoyaltyMath.Calculate(100m, 0m, -1m));
        Assert.Throws<ArgumentOutOfRangeException>(() => RoyaltyMath.Calculate(-1m, 0m, 12.5m));
        Assert.Throws<ArgumentException>(() => RoyaltyMath.Calculate(100m, 101m, 12.5m));
    }

    [Fact]
    public void A_missing_or_non_summing_ownership_fraction_is_refused()
    {
        // AC-2: the lease and the offending total are both named.
        var missing = OwnershipFractions.Validate("LSE-1", [], 0m);

        Assert.Equal(OwnershipFractions.Codes.MissingFraction, missing!.Value.Code);
        Assert.Contains("LSE-1", missing.Value.Message, StringComparison.Ordinal);
        Assert.Contains("no recorded ownership fraction", missing.Value.Message, StringComparison.Ordinal);

        var short_ = OwnershipFractions.Validate("LSE-1", [("OWNER-A", 60m)], 0m);

        Assert.Equal(OwnershipFractions.Codes.FractionsDoNotSum, short_!.Value.Code);
        Assert.Contains("totals 60 %", short_.Value.Message, StringComparison.Ordinal);
        Assert.Contains("OWNER-A 60 %", short_.Value.Message, StringComparison.Ordinal);
        Assert.Contains("not 100 %", short_.Value.Message, StringComparison.Ordinal);

        // Over-owned is refused the same way.
        Assert.Equal(
            OwnershipFractions.Codes.FractionsDoNotSum,
            OwnershipFractions.Validate("LSE-1", [("A", 70m), ("B", 40m)], 0m)!.Value.Code);

        // A complete division passes, and so does one with a declared residual.
        Assert.Null(OwnershipFractions.Validate("LSE-1", [("A", 75m), ("B", 25m)], 0m));
        Assert.Null(OwnershipFractions.Validate("LSE-1", [("A", 75m)], 25m));

        // A negative fraction or residual is its own refusal.
        Assert.Equal(
            OwnershipFractions.Codes.NegativeFraction,
            OwnershipFractions.Validate("LSE-1", [("A", -1m), ("B", 101m)], 0m)!.Value.Code);
        Assert.Equal(
            OwnershipFractions.Codes.NegativeFraction,
            OwnershipFractions.Validate("LSE-1", [("A", 25m)], -25m)!.Value.Code);
    }

    [Fact]
    public void A_jib_net_is_its_charges_less_its_credits()
    {
        // AC-4, at the arithmetic the statement's header is computed from.
        Assert.Equal(1_500m, JibMath.Net([2_000m, 500m], [1_000m]));
        Assert.Equal(-250m, JibMath.Net([100m], [350m]));
        Assert.Equal(300m, JibMath.Net([100m, 200m], []));
        Assert.Equal(0m, JibMath.Net([], []));

        // Rounded once, to the cent.
        Assert.Equal(0.3m, JibMath.Net([0.1m, 0.2m], []));
    }

    [Fact]
    public void An_allocation_adds_back_to_its_pool_exactly()
    {
        // AC-5's precondition: the shares the preview shows are the shares the commit writes, and they add back.
        var shares = CostAllocationMath.Allocate(
            1_000m,
            [
                new CostAllocationMath.AllocationTarget("PR-1", "PROPERTY", 60m),
                new CostAllocationMath.AllocationTarget("PR-2", "PROPERTY", 40m),
            ]);

        Assert.Equal(600m, shares[0].Amount);
        Assert.Equal(400m, shares[1].Amount);
        Assert.Equal(1_000m, shares.Sum(share => share.Amount));
        Assert.Equal(60m, shares[0].Percentage);

        // A split that cannot land on whole cents still adds back: 100 across three equal shares.
        var thirds = CostAllocationMath.Allocate(
            100m,
            [
                new CostAllocationMath.AllocationTarget("A", "WELL", 1m),
                new CostAllocationMath.AllocationTarget("B", "WELL", 1m),
                new CostAllocationMath.AllocationTarget("C", "WELL", 1m),
            ]);

        Assert.Equal(100m, thirds.Sum(share => share.Amount));
        Assert.Equal(33.34m, thirds.Max(share => share.Amount));
        Assert.Equal(33.33m, thirds.Min(share => share.Amount));

        // Deterministic: the same input gives the same answer every time, ties broken on the target id.
        var again = CostAllocationMath.Allocate(
            100m,
            [
                new CostAllocationMath.AllocationTarget("A", "WELL", 1m),
                new CostAllocationMath.AllocationTarget("B", "WELL", 1m),
                new CostAllocationMath.AllocationTarget("C", "WELL", 1m),
            ]);

        Assert.Equal(thirds.Select(share => share.TargetId), again.Select(share => share.TargetId));
        Assert.Equal(thirds.First(share => share.TargetId == "A").Amount,
            again.First(share => share.TargetId == "A").Amount);

        // Guards: an empty set, a negative weight, an all-zero basis, a duplicate target, a negative pool.
        Assert.Throws<ArgumentException>(() => CostAllocationMath.Allocate(100m, []));
        Assert.Throws<ArgumentException>(() => CostAllocationMath.Allocate(
            100m, [new CostAllocationMath.AllocationTarget("A", "WELL", -1m)]));
        Assert.Throws<ArgumentException>(() => CostAllocationMath.Allocate(
            100m, [new CostAllocationMath.AllocationTarget("A", "WELL", 0m)]));
        Assert.Throws<ArgumentException>(() => CostAllocationMath.Allocate(
            100m,
            [
                new CostAllocationMath.AllocationTarget("A", "WELL", 1m),
                new CostAllocationMath.AllocationTarget("A", "WELL", 1m),
            ]));
        Assert.Throws<ArgumentOutOfRangeException>(() => CostAllocationMath.Allocate(-1m,
            [new CostAllocationMath.AllocationTarget("A", "WELL", 1m)]));
    }

    [Fact]
    public void A_royalty_record_says_what_it_nets_to_and_whether_it_can_be_paid()
    {
        var royalty = Royalty();

        Assert.True(royalty.NetsAsStated);
        Assert.True(royalty.CanPay);
        Assert.Equal("payable", royalty.PayableText);
        Assert.Contains("volume 50 bbl", royalty.TraceText, StringComparison.Ordinal);
        Assert.Contains("allocation AL-1", royalty.TraceText, StringComparison.Ordinal);

        // AC-3 at the record: a dispute blocks the payment and the sentence carries the reference.
        var disputed = royalty with
        {
            Disputed = true,
            DisputeReference = "DIS-2026-0001",
            DisputeReason = "Owner contests the severance deduction",
        };

        Assert.False(disputed.CanPay);
        Assert.Contains("BLOCKED by dispute DIS-2026-0001", disputed.PayableText, StringComparison.Ordinal);
        Assert.Contains("severance", disputed.PayableText, StringComparison.Ordinal);

        // A paid royalty cannot be paid again.
        var paid = royalty with { Status = RoyaltyStatus.Paid };

        Assert.True(paid.IsPaid);
        Assert.False(paid.CanPay);

        // A row whose amount does not derive from its own net and rate is caught rather than shown as fine.
        Assert.False((royalty with { Amount = royalty.Amount + 1m }).NetsAsStated);
    }

    [Fact]
    public void A_jib_record_says_what_it_nets_to_and_what_it_is_traceable_to()
    {
        var jib = Jib();

        Assert.True(jib.NetsAsStated);
        Assert.Equal(1_500m, jib.Net);
        Assert.Contains("charges 2,000.00", jib.TotalsText, StringComparison.Ordinal);
        Assert.Contains("net 1,500.00 USD", jib.TotalsText, StringComparison.Ordinal);
        Assert.Contains("traceable to ALLOC-1", jib.TraceText, StringComparison.Ordinal);

        // AC-6: a bill with no source volume says so instead of looking proved.
        Assert.Contains("NOT traceable", (jib with { SourceVolumeReference = null }).TraceText,
            StringComparison.Ordinal);

        // The statement's own numbers must agree with its lines, or it is caught.
        Assert.False((jib with { Net = jib.Net + 1m }).NetsAsStated);

        // Posting is a state, and the journal is named once it exists.
        Assert.True(jib.IsApproved);
        Assert.False(jib.HasJournal);
        Assert.Contains("not posted", jib.JournalText, StringComparison.Ordinal);
        Assert.Contains("journal J1", (jib with { JournalId = "J1" }).JournalText, StringComparison.Ordinal);
    }

    [Fact]
    public void An_allocation_preview_says_whether_it_adds_back()
    {
        var preview = new OgAllocationPreview(
            1_000m,
            "USD",
            [
                new OgAllocationRow("PR-1", "PROPERTY", 60m, 600m, 60m),
                new OgAllocationRow("PR-2", "PROPERTY", 40m, 400m, 40m),
            ]);

        Assert.True(preview.AddsBack);
        Assert.Equal(0m, preview.Difference);
        Assert.Contains("adds back exactly", preview.TotalsText, StringComparison.Ordinal);

        var short_ = preview with
        {
            Shares = [new OgAllocationRow("PR-1", "PROPERTY", 100m, 999.99m, 100m)],
        };

        Assert.False(short_.AddsBack);
        Assert.Contains("SHORT by 0.01", short_.TotalsText, StringComparison.Ordinal);
    }

    [Fact]
    public void The_rules_refuse_a_statement_with_no_source_volume()
    {
        // AC-6 at the rule: the source volume is required, not optional.
        var missingSource = RoyaltyRules.ValidateJib(
            "PARTNER-1", " ", [new OgJibLine("CHARGE", "Casing", "MATERIAL", 100m)], "USD");

        Assert.Equal(RoyaltyRules.CodeSourceVolumeRequired, missingSource!.Value.Code);

        Assert.Null(RoyaltyRules.ValidateJib(
            "PARTNER-1", "ALLOC-1", [new OgJibLine("CHARGE", "Casing", "MATERIAL", 100m)], "USD"));

        Assert.Equal(
            RoyaltyRules.CodeInputInvalid,
            RoyaltyRules.ValidateJib("PARTNER-1", "ALLOC-1",
                [new OgJibLine("CHARGE", "Casing", null, -1m)], "USD")!.Value.Code);

        Assert.Contains(JibStatus.Draft, RoyaltyRules.JibStates);
        Assert.Contains(RoyaltyStatus.Disputed, RoyaltyRules.RoyaltyStates);

        // The royalty inputs: a rate outside 0-100 and deductions above the value are refused.
        Assert.Equal(
            RoyaltyRules.CodeInputInvalid,
            RoyaltyRules.Validate(Inputs() with { RatePercent = 101m })!.Value.Code);

        Assert.Equal(
            RoyaltyRules.CodeInputInvalid,
            RoyaltyRules.Validate(Inputs() with
            {
                Deductions = new OgRoyaltyDeductions(0m, 0m, 0m, 0m, 6_000m),
            })!.Value.Code);
    }

    [Fact]
    public void The_page_binds_the_workspace_and_uses_the_owners_vocabulary()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        Assert.Contains("workspace.Royalties", page, StringComparison.Ordinal);
        Assert.Contains("workspace.Jibs", page, StringComparison.Ordinal);
        Assert.Contains("workspace.AllocationMethods", page, StringComparison.Ordinal);
        Assert.Contains("royalty.PayableText", page, StringComparison.Ordinal);
        Assert.Contains("royalty.TraceText", page, StringComparison.Ordinal);
        Assert.Contains("jib.TraceText", page, StringComparison.Ordinal);
        Assert.Contains("preview.Value.TotalsText", page, StringComparison.Ordinal);

        Assert.DoesNotContain("\"DISPUTED\"", page, StringComparison.Ordinal);
        Assert.DoesNotContain("\"POSTED\"", page, StringComparison.Ordinal);
        Assert.DoesNotContain("\"ACCRUED\"", page, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ helpers

    private static OgRoyaltyInputs Inputs() =>
        new(
            "LSE-1",
            "OWNER-A",
            "AL-1",
            "AR-1",
            50m,
            "bbl",
            70m,
            12.5m,
            0m,
            "USD",
            new OgRoyaltyDeductions(200m, 100m, 50m, 0m, 0m));

    private static OgRoyalty Royalty() =>
        new(
            "R-1",
            "LSE-1",
            "OWNER-A",
            new DateTime(2026, 5, 1),
            50m,
            "bbl",
            70m,
            3_500m,
            new OgRoyaltyDeductions(200m, 100m, 50m, 0m, 0m),
            3_150m,
            12.5m,
            393.75m,
            RoyaltyStatus.Accrued,
            "USD",
            false,
            null,
            null,
            null,
            null,
            null,
            null,
            "sara",
            "J-1",
            null,
            null,
            null,
            null,
            "AL-1",
            "AR-1");

    private static OgJib Jib() =>
        new(
            "J-1",
            "PARTNER-1",
            new DateTime(2026, 5, 1),
            new DateTime(2026, 5, 31),
            2_000m,
            500m,
            1_500m,
            "USD",
            JibStatus.Approved,
            "SMOKE",
            "sara",
            "nadia",
            new DateTime(2026, 6, 1),
            null,
            null,
            null,
            "ALLOC-1",
            null,
            [
                new OgJibLine("CHARGE", "Casing", "MATERIAL", 2_000m),
                new OgJibLine("CREDIT", "Cost recovery", "RECOVERY", 500m),
            ]);
}
