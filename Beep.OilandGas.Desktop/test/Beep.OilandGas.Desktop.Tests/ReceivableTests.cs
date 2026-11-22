using Beep.OilandGas.Accounting.Calculations;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Accounting;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// ACC-04: the ageing bucket rule, the receipt and allocation rules, and the record's own money - the parts that need no
/// database. The record/post/apply journey, the one journal and the author rule are proven in the gateway test.
/// </summary>
public sealed class ReceivableTests
{
    private const string PageFileName = "ReceivablesPageView.cs";

    [Fact]
    public void The_ageing_buckets_come_from_due_dates()
    {
        // AC-2: current when not yet due, then 30, 60 and 90+ days past due, inclusive at each upper bound.
        var asOf = new DateTime(2026, 4, 15);
        var invoice = new DateTime(2026, 3, 1);

        Assert.Equal(ReceivableAgeing.Current, ReceivableAgeing.BucketOf(new DateTime(2026, 4, 15), invoice, asOf));
        Assert.Equal(ReceivableAgeing.Current, ReceivableAgeing.BucketOf(new DateTime(2026, 5, 1), invoice, asOf));
        Assert.Equal(ReceivableAgeing.Days30, ReceivableAgeing.BucketOf(new DateTime(2026, 4, 14), invoice, asOf));
        Assert.Equal(ReceivableAgeing.Days30, ReceivableAgeing.BucketOf(new DateTime(2026, 3, 16), invoice, asOf));
        Assert.Equal(ReceivableAgeing.Days60, ReceivableAgeing.BucketOf(new DateTime(2026, 3, 15), invoice, asOf));
        Assert.Equal(ReceivableAgeing.Days60, ReceivableAgeing.BucketOf(new DateTime(2026, 2, 14), invoice, asOf));
        Assert.Equal(ReceivableAgeing.Days90, ReceivableAgeing.BucketOf(new DateTime(2026, 2, 13), invoice, asOf));

        // 15 and 90 days exactly, from the rule rather than the boundaries above.
        Assert.Equal(15, ReceivableAgeing.DaysPastDue(new DateTime(2026, 3, 31), asOf));
        Assert.Equal(90, ReceivableAgeing.DaysPastDue(new DateTime(2026, 1, 15), asOf));

        // No due date falls back to the invoice date, and that is stated.
        Assert.Equal(ReceivableAgeing.Days30, ReceivableAgeing.BucketOf(null, new DateTime(2026, 3, 20), asOf));

        // The totals are the buckets added up.
        var totals = ReceivableAgeing.Of(
            [
                (new DateTime(2026, 5, 1), invoice, 100m),
                (new DateTime(2026, 4, 1), invoice, 200m),
                (new DateTime(2026, 3, 1), invoice, 300m),
                (new DateTime(2026, 1, 1), invoice, 400m),
            ],
            asOf);

        Assert.Equal(100m, totals.Current);
        Assert.Equal(200m, totals.Days30);
        Assert.Equal(300m, totals.Days60);
        Assert.Equal(400m, totals.Days90);
        Assert.Equal(1_000m, totals.Total);
    }

    [Fact]
    public void An_allocation_total_above_the_receipt_is_refused()
    {
        // AC-1's precondition and AC-4's receipt-level half.
        var tooMuch = ReceivableRules.Validate(
            Inputs() with
            {
                Amount = 1_000m,
                Allocations = [new OgAllocationInput("INV-1", 700m), new OgAllocationInput("INV-2", 500m)],
            });

        Assert.Equal(ReceivableRules.CodeExceedsReceipt, tooMuch!.Value.Code);
        Assert.Contains("1,200.00", tooMuch.Value.Message, StringComparison.Ordinal);
        Assert.Contains("1,000.00", tooMuch.Value.Message, StringComparison.Ordinal);

        Assert.Null(ReceivableRules.Validate(Inputs()));

        // An amount or a currency the app does not keep is refused.
        Assert.Equal(
            ReceivableRules.CodeInputInvalid,
            ReceivableRules.Validate(Inputs() with { Amount = 0m })!.Value.Code);
        Assert.Equal(
            ReceivableRules.CodeInputInvalid,
            ReceivableRules.Validate(Inputs() with { Currency = "XYZ" })!.Value.Code);
        Assert.Equal(
            ReceivableRules.CodeInputInvalid,
            ReceivableRules.Validate(Inputs() with { Allocations = [new OgAllocationInput("INV-1", 0m)] })!.Value.Code);

        Assert.Equal(
            ["customer", "receipt date", "amount", "currency"],
            ReceivableRules.Missing(new OgReceiptInputs(null, null, null, null, null, null, [])));

        Assert.Equal(ReceivableCodes.Draft, ReceivableCodes.States[0]);
        Assert.Equal(["DRAFT", "POSTED", "APPROVED", "RETURNED"], ReceivableCodes.States);
    }

    [Fact]
    public void A_receipt_says_what_is_unapplied()
    {
        // AC-1: the allocation total equals the receipt or the remainder is shown, never discarded.
        var fully = Receipt(unapplied: 0m) with
        {
            Allocations = [new OgAllocation("P1", "INV-1", 1_000m, ReceivableCodes.Posted)],
        };

        Assert.Equal(1_000m, fully.Allocated);
        Assert.Contains("fully allocated", fully.AllocationText, StringComparison.Ordinal);
        Assert.DoesNotContain("unapplied", fully.AllocationText, StringComparison.Ordinal);

        var partial = Receipt(unapplied: 250m) with
        {
            Allocations = [new OgAllocation("P1", "INV-1", 750m, ReceivableCodes.Posted)],
        };

        Assert.Equal(750m, partial.Allocated);
        Assert.Contains("allocated 750.00", partial.AllocationText, StringComparison.Ordinal);
        Assert.Contains("unapplied 250.00 USD", partial.AllocationText, StringComparison.Ordinal);
        Assert.Contains("customer credit", partial.AllocationText, StringComparison.Ordinal);

        // The states read for a person, and a posted receipt says which journal posted it.
        Assert.Equal("draft - not posted", partial.StateText);
        Assert.Contains("not posted", partial.JournalText, StringComparison.Ordinal);

        var posted = partial with
        {
            State = ReceivableCodes.Posted,
            JournalId = "J1",
            PostedAt = new DateTime(2026, 4, 16, 9, 0, 0),
        };

        Assert.True(posted.IsPosted);
        Assert.True(posted.HasJournal);
        Assert.Contains("journal J1", posted.JournalText, StringComparison.Ordinal);
        Assert.Equal("posted", posted.StateText);

        // An allocation that has already been applied says so - that is what makes a retry safe.
        Assert.True(fully.Allocations[0].IsApplied);
        Assert.Contains("applied", fully.Allocations[0].Describe(), StringComparison.Ordinal);
        Assert.False(new OgAllocation("P2", "INV-2", 1m, ReceivableCodes.Draft).IsApplied);
    }

    [Fact]
    public void The_page_binds_the_workspace_and_names_no_state_literal()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => Path.GetFileName(source) == PageFileName));

        Assert.Contains("workspace.Ageing", page, StringComparison.Ordinal);
        Assert.Contains("workspace.OpenInvoices", page, StringComparison.Ordinal);
        Assert.Contains("workspace.Currencies", page, StringComparison.Ordinal);
        Assert.Contains("receipt.AllocationText", page, StringComparison.Ordinal);
        Assert.Contains("receipt.Allocations", page, StringComparison.Ordinal);

        Assert.DoesNotContain("\"POSTED\"", page, StringComparison.Ordinal);
        Assert.DoesNotContain("\"DRAFT\"", page, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ helpers

    private static OgReceiptInputs Inputs() =>
        new(
            "BUYER-1",
            new DateTime(2026, 4, 15),
            1_000m,
            "USD",
            "BANK-REF-1",
            "BANK-1",
            [new OgAllocationInput("INV-1", 1_000m)]);

    private static OgReceipt Receipt(decimal unapplied) =>
        new(
            "R1",
            string.Empty,
            "BUYER-1",
            new DateTime(2026, 4, 15),
            1_000m,
            unapplied,
            "USD",
            "BANK-REF-1",
            "BANK-1",
            ReceivableCodes.Draft,
            "sara",
            null,
            null,
            null,
            null,
            null,
            null,
            []);
}
