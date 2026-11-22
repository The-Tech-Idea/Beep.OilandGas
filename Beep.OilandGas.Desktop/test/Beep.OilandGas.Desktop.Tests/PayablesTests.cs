using Beep.OilandGas.Accounting.Calculations;
using Beep.OilandGas.Accounting.Constants;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Accounting;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// ACC-05: the tolerance arithmetic, the match's exception decisions, the receipt ceiling, the input rules and the record's
/// own state - the parts that need no database. The whole order-to-payment journey is proven in the gateway test.
/// </summary>
public sealed class PayablesTests
{
    private const string PageFileName = "PayablesPageView.cs";

    [Fact]
    public void A_price_is_inside_tolerance_only_inside_it()
    {
        // The arithmetic is the owner's scaled-integer comparison: nothing is rounded to decide it.
        Assert.True(ThreeWayMatch.WithinPercent(105m, 100m, 5m));
        Assert.True(ThreeWayMatch.WithinPercent(95m, 100m, 5m));
        Assert.True(ThreeWayMatch.WithinPercent(100m, 100m, 0m));
        Assert.False(ThreeWayMatch.WithinPercent(105.01m, 100m, 5m));
        Assert.False(ThreeWayMatch.WithinPercent(94.99m, 100m, 5m));

        // A price below the basis moves the same distance as one above it.
        Assert.False(ThreeWayMatch.WithinPercent(89m, 100m, 10m));
        Assert.True(ThreeWayMatch.WithinPercent(90m, 100m, 10m));

        // A basis of zero decides nothing rather than passing.
        Assert.False(ThreeWayMatch.WithinPercent(1m, 0m, 10m));
    }

    [Fact]
    public void An_invoice_above_the_received_quantity_is_an_exception()
    {
        // AC-1's rule at the invoice, and the stricter ceiling this match adds over the owner's own policy check.
        var result = ThreeWayMatch.Compare(
            orderedQuantity: 100m,
            orderedUnitPrice: 50m,
            receivedQuantity: 60m,
            invoicedQuantity: 80m,
            invoicedUnitPrice: 50m,
            quantityTolerancePercent: 5m,
            priceTolerancePercent: 5m);

        Assert.Equal(ProcurementCodes.MatchException, result.State);
        Assert.False(result.Matched);
        Assert.True(result.NeedsApproval);
        Assert.False(result.IsPayable);

        // Inside the order but above what arrived: the receipt ceiling is what was passed.
        Assert.Contains(result.Exceptions, exception => exception.Kind == ProcurementCodes.ExceptionQuantityAboveReceived);
        Assert.DoesNotContain(result.Exceptions, exception => exception.Kind == ProcurementCodes.ExceptionQuantityAboveOrder);
        Assert.Contains("invoiced above the received quantity", result.ExceptionsText, StringComparison.Ordinal);
    }

    [Fact]
    public void An_invoice_above_the_ordered_quantity_names_the_order_ceiling()
    {
        var result = ThreeWayMatch.Compare(100m, 50m, 120m, 110m, 50m, 5m, 5m);

        Assert.Equal(ProcurementCodes.MatchException, result.State);
        Assert.Contains(result.Exceptions, exception => exception.Kind == ProcurementCodes.ExceptionQuantityAboveOrder);
        Assert.Contains("invoiced above the ordered quantity", result.ExceptionsText, StringComparison.Ordinal);
    }

    [Fact]
    public void A_price_outside_tolerance_is_an_exception_not_a_pass()
    {
        // AC-2: 60 against 50 is 20 % above - outside a 5 % tolerance and therefore an exception.
        var outside = ThreeWayMatch.Compare(100m, 50m, 100m, 100m, 60m, 5m, 5m);

        Assert.Equal(ProcurementCodes.MatchException, outside.State);
        Assert.Contains(outside.Exceptions,
            exception => exception.Kind == ProcurementCodes.ExceptionPriceOutsideTolerance);
        Assert.Contains("tolerance", outside.ExceptionsText, StringComparison.Ordinal);

        // 52 against 50 is 4 % - inside.
        var inside = ThreeWayMatch.Compare(100m, 50m, 100m, 100m, 52m, 5m, 5m);

        Assert.Equal(ProcurementCodes.MatchMatched, inside.State);
        Assert.True(inside.Matched);
        Assert.True(inside.IsPayable);
        Assert.Empty(inside.Exceptions);
        Assert.Equal("no exception", inside.ExceptionsText);
    }

    [Fact]
    public void A_non_purchase_invoice_has_nothing_to_match()
    {
        // The owner's separate-review rule, stated rather than passed silently.
        var result = ThreeWayMatch.Compare(null, null, null, 10m, 100m, 5m, 5m);

        Assert.Equal(ProcurementCodes.MatchNotApplicable, result.State);
        Assert.Empty(result.Exceptions);
        Assert.True(result.IsPayable);
    }

    [Fact]
    public void A_missing_policy_is_an_exception_rather_than_an_assumed_tolerance()
    {
        // The tolerance is configuration: with no recorded policy the calculator says so instead of guessing.
        var result = ThreeWayMatch.Compare(100m, 50m, 100m, 100m, 100m, null, null);

        Assert.Equal(ProcurementCodes.MatchException, result.State);
        Assert.Contains(result.Exceptions, exception => exception.Kind == ProcurementCodes.ExceptionNoPolicy);
        Assert.Contains("no approved matching policy", result.ExceptionsText, StringComparison.Ordinal);
    }

    [Fact]
    public void The_invoice_match_rolls_every_line_up_once()
    {
        var lineOne = ThreeWayMatch.Compare(100m, 50m, 100m, 100m, 50m, 5m, 5m);
        var lineTwo = ThreeWayMatch.Compare(10m, 500m, 10m, 10m, 700m, 5m, 5m);

        var invoice = ThreeWayMatch.CompareInvoice(
            currencyAgrees: false,
            [lineOne, lineTwo],
            5m,
            5m);

        Assert.Equal(ProcurementCodes.MatchException, invoice.State);

        // The currency is said once for the invoice, and the price exception survives from the line.
        Assert.Single(invoice.Exceptions,
            exception => exception.Kind == ProcurementCodes.ExceptionCurrencyMismatch);
        Assert.Single(invoice.Exceptions,
            exception => exception.Kind == ProcurementCodes.ExceptionPriceOutsideTolerance);
    }

    [Fact]
    public void A_receipt_above_the_order_is_refused_and_names_the_excess()
    {
        // AC-1 and SG-10-6, at the rule rather than the page.
        var order = new PayableRules.OgOrder(
            "PO-2026-0001",
            [
                new OgOrderLine("L1", 1, "Casing", 100m, "ea", 50m, 5_000m, 60m, 0m),
                new OgOrderLine("L2", 2, "Cement", 200m, "sx", 20m, 4_000m, 0m, 0m),
            ]);

        var over = PayableRules.CheckReceipt(order, "L1", 50m, 60m);

        Assert.Equal(PayableRules.CodeReceiptExceedsOrder, over!.Value.Code);
        Assert.Contains("10 more than was ordered", over.Value.Message, StringComparison.Ordinal);
        Assert.Contains("PO-2026-0001", over.Value.Message, StringComparison.Ordinal);

        // Exactly the remainder is allowed, and so is a first receipt on an untouched line.
        Assert.Null(PayableRules.CheckReceipt(order, "L1", 40m, 60m));
        Assert.Null(PayableRules.CheckReceipt(order, "L2", 200m, 0m));
        Assert.Null(PayableRules.CheckReceipt(order, "L1", 100m, 0m));

        // A quantity of zero, or a line that is not on the order, is a different refusal.
        Assert.Equal(PayableRules.CodeInputInvalid, PayableRules.CheckReceipt(order, "L1", 0m, 0m)!.Value.Code);
        Assert.Equal(PayableRules.CodeInputInvalid, PayableRules.CheckReceipt(order, "L9", 1m, 0m)!.Value.Code);
    }

    [Fact]
    public void An_order_needs_a_vendor_a_date_a_currency_and_a_priced_line()
    {
        Assert.Equal(
            ["vendor", "order date", "currency", "at least one line"],
            PayableRules.OrderMissing(new OgOrderInputs(null, null, null, null, null, null, [])));

        Assert.Null(PayableRules.ValidateOrder(OrderInputs()));
        Assert.Empty(PayableRules.OrderMissing(OrderInputs()));

        Assert.Equal(
            PayableRules.CodeCurrencyMismatch,
            PayableRules.ValidateOrder(OrderInputs() with { Currency = "XYZ" })!.Value.Code);

        Assert.Equal(
            PayableRules.CodeInputInvalid,
            PayableRules.ValidateOrder(OrderInputs() with
            {
                Lines = [new OgOrderLineInput("Casing", 0m, "ea", 50m)],
            })!.Value.Code);

        Assert.Contains("ea", PayableRules.QuantityUnits);
        Assert.Equal(ProcurementCodes.PaymentMethods, PayableRules.PaymentMethods);
        Assert.Equal(["DRAFT", "APPROVED", "CLOSED"], PayableRules.OrderStates);
    }

    [Fact]
    public void An_invoice_needs_its_supplier_number_and_lines()
    {
        Assert.Equal(
            ["vendor", "the supplier's invoice number", "invoice date", "currency", "at least one line"],
            PayableRules.InvoiceMissing(new OgSupplierInvoiceInputs(null, null, null, null, null, null, null, null, null, [])));

        Assert.Null(PayableRules.ValidateInvoice(InvoiceInputs()));
        Assert.Empty(PayableRules.InvoiceMissing(InvoiceInputs()));

        Assert.Equal(
            PayableRules.CodeInputInvalid,
            PayableRules.ValidateInvoice(InvoiceInputs() with { TaxAmount = -1m })!.Value.Code);

        Assert.Equal(
            PayableRules.CodeInputInvalid,
            PayableRules.ValidateInvoice(InvoiceInputs() with
            {
                Lines = [new OgInvoiceLineInput("Casing", 10m, "ea", -5m, null)],
            })!.Value.Code);
    }

    [Fact]
    public void A_payment_run_authorises_payment_only_when_the_match_does()
    {
        Assert.True(ProcurementCodes.IsPayable(ProcurementCodes.MatchMatched));
        Assert.True(ProcurementCodes.IsPayable(ProcurementCodes.MatchExceptionApproved));
        Assert.True(ProcurementCodes.IsPayable(ProcurementCodes.MatchNotApplicable));
        Assert.False(ProcurementCodes.IsPayable(ProcurementCodes.MatchException));
        Assert.False(ProcurementCodes.IsPayable(null));

        var invoice = Invoice();

        Assert.True(invoice.IsPayable);
        Assert.True(invoice.IsMatched);
        Assert.Empty(invoice.Exceptions);

        var needsApproval = invoice with { MatchState = ProcurementCodes.MatchException };

        Assert.True(needsApproval.NeedsExceptionApproval);
        Assert.False(needsApproval.IsPayable);

        var approved = invoice with
        {
            MatchState = ProcurementCodes.MatchExceptionApproved,
            ExceptionApprovedBy = "nadia",
            ExceptionApprovedAt = new DateTime(2026, 5, 2, 8, 0, 0),
        };

        Assert.True(approved.IsPayable);
        Assert.Contains("exception approved by nadia", approved.MatchText, StringComparison.Ordinal);

        // A paid invoice is not payable again, however well it matched.
        var paid = invoice with { Balance = 0m, Paid = 11_000m };

        Assert.True(paid.IsPaid);
        Assert.False(paid.IsPayable);
    }

    [Fact]
    public void A_run_reports_what_it_pays_and_its_one_journal()
    {
        var run = new OgPaymentRun(
            "R1",
            "PAY-2026-0001",
            new DateTime(2026, 5, 1),
            11_000m,
            "USD",
            ProcurementCodes.RunDraft,
            "sara",
            null,
            null,
            null,
            null,
            null,
            null,
            ["INV-A", "INV-B"]);

        Assert.True(run.IsDraft);
        Assert.False(run.HasJournal);
        Assert.Contains("not posted", run.JournalText, StringComparison.Ordinal);
        Assert.Contains("INV-A, INV-B", run.InvoicesText, StringComparison.Ordinal);
        Assert.Contains("11,000.00 USD", run.AmountText, StringComparison.Ordinal);

        var posted = run with
        {
            Status = ProcurementCodes.RunPosted,
            JournalId = "J1",
            PostedAt = new DateTime(2026, 5, 1, 16, 0, 0),
        };

        Assert.True(posted.IsPosted);
        Assert.True(posted.HasJournal);
        Assert.Contains("journal J1", posted.JournalText, StringComparison.Ordinal);
    }

    [Fact]
    public void The_page_binds_the_workspace_and_names_no_state_literal()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        Assert.Contains("workspace.PolicyText", page, StringComparison.Ordinal);
        Assert.Contains("workspace.Orders", page, StringComparison.Ordinal);
        Assert.Contains("workspace.Invoices", page, StringComparison.Ordinal);
        Assert.Contains("workspace.Runs", page, StringComparison.Ordinal);
        Assert.Contains("invoice.MatchText", page, StringComparison.Ordinal);
        Assert.Contains("invoice.ExceptionsText", page, StringComparison.Ordinal);
        Assert.Contains("run.JournalText", page, StringComparison.Ordinal);

        // The vocabulary lives in the owner's codes, not in the view.
        Assert.DoesNotContain("\"MATCHED\"", page, StringComparison.Ordinal);
        Assert.DoesNotContain("\"EXCEPTION\"", page, StringComparison.Ordinal);
        Assert.DoesNotContain("\"POSTED\"", page, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ helpers

    private static OgOrderInputs OrderInputs() =>
        new(
            "VENDOR-1",
            new DateTime(2026, 4, 1),
            new DateTime(2026, 4, 15),
            "USD",
            "AFE-1",
            "Casing and cement",
            [
                new OgOrderLineInput("Casing", 100m, "ea", 50m),
                new OgOrderLineInput("Cement", 200m, "sx", 20m),
            ]);

    private static OgSupplierInvoiceInputs InvoiceInputs() =>
        new(
            "VENDOR-1",
            "SUP-1001",
            new DateTime(2026, 4, 20),
            new DateTime(2026, 5, 20),
            "USD",
            1_000m,
            "ORDER-1",
            "AFE-1",
            null,
            [new OgInvoiceLineInput("Casing", 100m, "ea", 50m, "L1")]);

    private static OgApInvoice Invoice() =>
        new(
            "AP-1",
            "SUP-1001",
            "VENDOR-1",
            new DateTime(2026, 4, 20),
            new DateTime(2026, 5, 20),
            "USD",
            10_000m,
            1_000m,
            11_000m,
            0m,
            11_000m,
            InvoiceStatuses.Issued,
            ProcurementCodes.MatchMatched,
            [],
            "POLICY-1",
            "sara",
            null,
            null,
            "ORDER-1",
            "AFE-1",
            []);
}
