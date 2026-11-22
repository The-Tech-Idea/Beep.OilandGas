using System.Globalization;
using Beep.OilandGas.Accounting.Calculations;
using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Accounting;

/// <summary>Where a supplier document belongs (ACC-05): the company and the book its journal posts to.</summary>
public sealed record OgPayableContext(string CompanyId, string BookCode)
{
    public static OgPayableContext FromSession(OgSession session) =>
        new(
            string.IsNullOrWhiteSpace(session.CompanyId) ? "APP" : session.CompanyId,
            string.IsNullOrWhiteSpace(session.BookCode) ? "MAIN" : session.BookCode);
}

/// <summary>One line of a purchase order as the buyer enters it (ACC-05).</summary>
public sealed record OgOrderLineInput(
    string? Description,
    decimal? Quantity,
    string? Unit,
    decimal? UnitPrice);

/// <summary>The inputs of a purchase order (ACC-05).</summary>
public sealed record OgOrderInputs(
    string? VendorId,
    DateTime? Date,
    DateTime? ExpectedDelivery,
    string? Currency,
    string? AfeId,
    string? Description,
    IReadOnlyList<OgOrderLineInput> Lines);

/// <summary>One order line as stored, with what has arrived and what has been invoiced (ACC-05).</summary>
public sealed record OgOrderLine(
    string Id,
    int LineNumber,
    string? Description,
    decimal Quantity,
    string? Unit,
    decimal UnitPrice,
    decimal LineTotal,
    decimal Received,
    decimal Invoiced)
{
    /// <summary>What is still to arrive - never negative, because receipts above the order are refused (AC-1).</summary>
    public decimal Open => Math.Max(0m, Quantity - Received);

    public string Describe() =>
        $"{LineNumber.ToString(CultureInfo.InvariantCulture)} · {Description ?? "(no description)"} · " +
        Number(Quantity) + " " + (Unit ?? string.Empty) + " at " + Money(UnitPrice) + " · received " +
        Number(Received) + " · invoiced " + Number(Invoiced) + " · open " + Number(Open);

    private static string Number(decimal value) => value.ToString("#,0.####", CultureInfo.InvariantCulture);

    private static string Money(decimal value) => MoneyText.Of(value);
}

/// <summary>One purchase order (ACC-05).</summary>
public sealed record OgPurchaseOrder(
    string Id,
    string Number,
    string VendorId,
    DateTime Date,
    DateTime? ExpectedDelivery,
    string Currency,
    decimal Total,
    string? AfeId,
    string State,
    string? Author,
    string? ApprovedBy,
    DateTime? ApprovedAt,
    string? Description,
    IReadOnlyList<OgOrderLine> Lines)
{
    public bool IsDraft => string.Equals(State, ProcurementCodes.OrderDraft, StringComparison.OrdinalIgnoreCase);

    public bool IsApproved => string.Equals(State, ProcurementCodes.OrderApproved,
        StringComparison.OrdinalIgnoreCase);

    public bool IsClosed => string.Equals(State, ProcurementCodes.OrderClosed, StringComparison.OrdinalIgnoreCase);

    /// <summary>Whether goods may be booked in against it.</summary>
    public bool AcceptsReceipts => IsApproved;

    public decimal ReceivedValue =>
        Lines.Sum(line => line.Received * line.UnitPrice);

    public string AmountText => MoneyText.Of(Total) + " " + Currency;

    public string StateText => State.ToLowerInvariant() +
                               (IsApproved ? " - accepts receipts" : IsClosed ? " - closed" : " - not approved");

    public string ApprovalText => ApprovedAt is { } at
        ? "approved by " + (ApprovedBy ?? "(unknown)") + " on " +
          at.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
        : "not approved";

    public string LinesText => Lines.Count == 0
        ? "no line"
        : string.Join(Environment.NewLine, Lines.Select(line => line.Describe()));

    public string Describe() =>
        $"{Number} · {Date:yyyy-MM-dd} · {VendorId} · {AmountText} · {StateText}" +
        (string.IsNullOrWhiteSpace(AfeId) ? string.Empty : " · AFE " + AfeId);
}

/// <summary>One supplier invoice line as the clerk enters it (ACC-05).</summary>
public sealed record OgInvoiceLineInput(
    string? Description,
    decimal? Quantity,
    string? Unit,
    decimal? UnitPrice,
    string? PoLineItemId);

/// <summary>The inputs of a supplier invoice (ACC-05).</summary>
public sealed record OgSupplierInvoiceInputs(
    string? VendorId,
    string? Number,
    DateTime? Date,
    DateTime? DueDate,
    string? Currency,
    decimal? TaxAmount,
    string? OrderId,
    string? AfeId,
    string? Description,
    IReadOnlyList<OgInvoiceLineInput> Lines);

/// <summary>One supplier invoice line as stored (ACC-05).</summary>
public sealed record OgInvoiceLine(
    string Id,
    int LineNumber,
    string? Description,
    decimal Quantity,
    string? Unit,
    decimal UnitPrice,
    decimal LineTotal,
    string? PoLineItemId)
{
    public string Describe() =>
        $"{LineNumber.ToString(CultureInfo.InvariantCulture)} · {Description ?? "(no description)"} · " +
        Quantity.ToString("#,0.####", CultureInfo.InvariantCulture) + " " + (Unit ?? string.Empty) + " at " +
        MoneyText.Of(UnitPrice) + " · " + MoneyText.Of(LineTotal) +
        (string.IsNullOrWhiteSpace(PoLineItemId) ? " · no order line" : " · order line " + PoLineItemId);
}

/// <summary>One supplier invoice with its match (ACC-05).</summary>
public sealed record OgApInvoice(
    string Id,
    string Number,
    string VendorId,
    DateTime Date,
    DateTime? DueDate,
    string Currency,
    decimal Subtotal,
    decimal Tax,
    decimal Total,
    decimal Paid,
    decimal Balance,
    string Status,
    string MatchState,
    IReadOnlyList<MatchException> Exceptions,
    string? PolicyId,
    string? Author,
    string? ExceptionApprovedBy,
    DateTime? ExceptionApprovedAt,
    string? OrderId,
    string? AfeId,
    IReadOnlyList<OgInvoiceLine> Lines)
{
    public bool IsPaid => Balance <= 0m;

    /// <summary>Whether the match authorises payment (AC-2, AC-6).</summary>
    public bool IsPayable => ProcurementCodes.IsPayable(MatchState) && Balance > 0m;

    public bool IsMatched => string.Equals(MatchState, ProcurementCodes.MatchMatched,
        StringComparison.OrdinalIgnoreCase);

    public bool NeedsExceptionApproval => string.Equals(MatchState, ProcurementCodes.MatchException,
        StringComparison.OrdinalIgnoreCase);

    public bool IsNonPo => string.Equals(MatchState, ProcurementCodes.MatchNotApplicable,
        StringComparison.OrdinalIgnoreCase);

    public string TotalsText =>
        "subtotal " + MoneyText.Of(Subtotal) + " · tax " + MoneyText.Of(Tax) + " · total " + MoneyText.Of(Total);

    public string BalanceText =>
        "paid " + MoneyText.Of(Paid) + " · balance " + MoneyText.Of(Balance) + " " + Currency;

    public string MatchText => MatchState switch
    {
        ProcurementCodes.MatchMatched => "matched: order, receipt and invoice agree inside the recorded tolerance",
        ProcurementCodes.MatchNotApplicable => "non-PO invoice: nothing to match, reviewed on its own",
        ProcurementCodes.MatchExceptionApproved =>
            "exception approved by " + (ExceptionApprovedBy ?? "(unknown)") + " on " +
            (ExceptionApprovedAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "an unrecorded date"),
        ProcurementCodes.MatchException => "EXCEPTION - awaiting an independent approval",
        _ => MatchState,
    } + (string.IsNullOrWhiteSpace(PolicyId) ? string.Empty : " · policy " + PolicyId);

    public string ExceptionsText => Exceptions.Count == 0
        ? "no exception"
        : string.Join(Environment.NewLine, Exceptions.Select(exception => exception.Describe()));

    public string LinesText => Lines.Count == 0
        ? "no line"
        : string.Join(Environment.NewLine, Lines.Select(line => line.Describe()));

    public string Describe() =>
        $"{Number} · {Date:yyyy-MM-dd} · {VendorId} · {TotalsText} · {Status} · {MatchState}";
}

/// <summary>One payment run (ACC-05).</summary>
public sealed record OgPaymentRun(
    string Id,
    string Number,
    DateTime Date,
    decimal Total,
    string Currency,
    string Status,
    string? Author,
    string? ApprovedBy,
    DateTime? ApprovedAt,
    string? JournalId,
    string? PostedBy,
    DateTime? PostedAt,
    string? ReturnReason,
    IReadOnlyList<string> InvoiceNumbers)
{
    public bool IsDraft => string.Equals(Status, ProcurementCodes.RunDraft, StringComparison.OrdinalIgnoreCase);

    public bool IsApproved => string.Equals(Status, ProcurementCodes.RunApproved,
        StringComparison.OrdinalIgnoreCase);

    public bool IsPosted => string.Equals(Status, ProcurementCodes.RunPosted,
        StringComparison.OrdinalIgnoreCase);

    public bool HasJournal => !string.IsNullOrWhiteSpace(JournalId);

    public string AmountText => MoneyText.Of(Total) + " " + Currency;

    public string StateText => Status.ToLowerInvariant();

    public string JournalText => HasJournal
        ? "journal " + JournalId + " posted on " +
          (PostedAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "an unrecorded date")
        : "not posted - no journal yet";

    public string InvoicesText => InvoiceNumbers.Count == 0
        ? "no invoice"
        : string.Join(", ", InvoiceNumbers);

    public string Describe() =>
        (string.IsNullOrWhiteSpace(Number) ? "(unnumbered)" : Number) + " · " + Date.ToString("yyyy-MM-dd") + " · " +
        AmountText + " · " + StateText + " · " + InvoiceNumbers.Count.ToString(CultureInfo.InvariantCulture) +
        " invoice(s)";
}

/// <summary>The payables workspace (ACC-05).</summary>
public sealed record OgPayablesWorkspace(
    IReadOnlyList<OgPurchaseOrder> Orders,
    OgPurchaseOrder? SelectedOrder,
    IReadOnlyList<OgApInvoice> Invoices,
    OgApInvoice? SelectedInvoice,
    IReadOnlyList<OgPaymentRun> Runs,
    OgPaymentRun? SelectedRun,
    IReadOnlyList<string> OrderStates,
    IReadOnlyList<string> MatchStates,
    IReadOnlyList<string> PaymentMethods,
    IReadOnlyList<string> Currencies,
    string? PolicyId,
    string PolicyText,
    bool HistoryFailed,
    string? HistoryReason)
{
    public string OrdersText => Orders.Count == 0
        ? "No purchase order is raised."
        : string.Join(Environment.NewLine, Orders.Select(order => order.Describe()));

    public string InvoicesText => Invoices.Count == 0
        ? "No supplier invoice is captured."
        : string.Join(Environment.NewLine, Invoices.Select(invoice => invoice.Describe()));

    public string RunsText => Runs.Count == 0
        ? "No payment run exists."
        : string.Join(Environment.NewLine, Runs.Select(run => run.Describe()));

    public string HistoryStateText => HistoryFailed
        ? "the payables ledger could not be read; nothing can be captured or paid"
        : $"{Invoices.Count.ToString(CultureInfo.InvariantCulture)} invoice(s) · " +
          $"{Invoices.Count(invoice => invoice.NeedsExceptionApproval).ToString(CultureInfo.InvariantCulture)} " +
          $"awaiting exception approval · " +
          $"{Invoices.Count(invoice => invoice.IsPayable).ToString(CultureInfo.InvariantCulture)} payable · " +
          $"{Orders.Count.ToString(CultureInfo.InvariantCulture)} order(s)";

    /// <summary>AC-4's other half: what the captured invoices commit against their authorisations.</summary>
    public decimal Committed => Invoices.Where(invoice => !invoice.IsPaid).Sum(invoice => invoice.Subtotal);

    public string CommittedText => "committed by open invoices " + MoneyText.Of(Committed);

    public string PolicyText2 => PolicyText;

    public string ReferenceText =>
        $"orders {string.Join(", ", OrderStates)} · matches {string.Join(", ", MatchStates)} · " +
        "tolerances come from the recorded AP_MATCHING policy";
}

/// <summary>The payables rules, stated once so the page, the service and the tests agree.</summary>
public static class PayableRules
{
    public const string CodeNoConnection = "payable-no-connection";
    public const string CodeReadFailed = "payable-read-failed";
    public const string CodeWriteFailed = "payable-write-failed";
    public const string CodeActorRequired = "payable-actor-required";
    public const string CodeInputRequired = "payable-input-required";
    public const string CodeInputInvalid = "payable-input-invalid";
    public const string CodeCurrencyMismatch = "payable-currency-mismatch";
    public const string CodeUnknownOrder = "payable-unknown-order";
    public const string CodeUnknownInvoice = "payable-unknown-invoice";
    public const string CodeUnknownRun = "payable-unknown-run";
    public const string CodeOrderNotApproved = "payable-order-not-approved";
    public const string CodeReceiptExceedsOrder = "payable-receipt-exceeds-order";
    public const string CodeDuplicateInvoiceNumber = "payable-duplicate-invoice-number";
    public const string CodeCommitmentExceedsAfe = "payable-commitment-exceeds-afe";
    public const string CodeNoMatchingPolicy = "payable-no-matching-policy";
    public const string CodeExceptionNotApproved = "payable-exception-not-approved";
    public const string CodeAuthorCannotApprove = "payable-author-cannot-approve";
    public const string CodeReturnReasonRequired = "payable-return-reason-required";
    public const string CodeRunNothingDue = "payable-run-nothing-due";
    public const string CodeRunNotApproved = "payable-run-not-approved";
    public const string CodeRunAlreadyPosted = "payable-run-already-posted";
    public const string CodePeriodClosed = "payable-period-closed";
    public const string CodeUnknownAccount = "payable-unknown-account";

    public static IReadOnlyList<string> Currencies { get; } = ["USD", "EUR", "GBP", "SAR", "AED"];

    public static IReadOnlyList<string> QuantityUnits => ProcurementCodes.QuantityUnits;

    public static IReadOnlyList<string> PaymentMethods => ProcurementCodes.PaymentMethods;

    public static IReadOnlyList<string> OrderStates => ProcurementCodes.OrderStates;

    public static IReadOnlyList<string> MatchStates => ProcurementCodes.MatchStates;

    public static IReadOnlyList<string> OrderMissing(OgOrderInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(inputs.VendorId))
        {
            missing.Add("vendor");
        }

        if (inputs.Date is null)
        {
            missing.Add("order date");
        }

        if (string.IsNullOrWhiteSpace(inputs.Currency))
        {
            missing.Add("currency");
        }

        if (inputs.Lines is null || inputs.Lines.Count == 0)
        {
            missing.Add("at least one line");
        }

        return missing;
    }

    public static (string Code, string Message)? ValidateOrder(OgOrderInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var missing = OrderMissing(inputs);

        if (missing.Count > 0)
        {
            return (CodeInputRequired, $"The order needs {string.Join(", ", missing)}.");
        }

        if (!Currencies.Contains(inputs.Currency!.Trim().ToUpperInvariant(), StringComparer.Ordinal))
        {
            return (CodeCurrencyMismatch,
                $"'{inputs.Currency}' is not a currency this app buys in; use {string.Join(", ", Currencies)}.");
        }

        foreach (var line in inputs.Lines)
        {
            if (line.Quantity is null || line.Quantity <= 0m)
            {
                return (CodeInputInvalid, "Every order line needs a quantity above zero.");
            }

            if (line.UnitPrice is null || line.UnitPrice < 0m)
            {
                return (CodeInputInvalid, "Every order line needs a unit price, which cannot be negative.");
            }
        }

        return null;
    }

    public static IReadOnlyList<string> InvoiceMissing(OgSupplierInvoiceInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(inputs.VendorId))
        {
            missing.Add("vendor");
        }

        if (string.IsNullOrWhiteSpace(inputs.Number))
        {
            missing.Add("the supplier's invoice number");
        }

        if (inputs.Date is null)
        {
            missing.Add("invoice date");
        }

        if (string.IsNullOrWhiteSpace(inputs.Currency))
        {
            missing.Add("currency");
        }

        if (inputs.Lines is null || inputs.Lines.Count == 0)
        {
            missing.Add("at least one line");
        }

        return missing;
    }

    public static (string Code, string Message)? ValidateInvoice(OgSupplierInvoiceInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var missing = InvoiceMissing(inputs);

        if (missing.Count > 0)
        {
            return (CodeInputRequired, $"The invoice needs {string.Join(", ", missing)}.");
        }

        if (!Currencies.Contains(inputs.Currency!.Trim().ToUpperInvariant(), StringComparer.Ordinal))
        {
            return (CodeCurrencyMismatch,
                $"'{inputs.Currency}' is not a currency this app pays in; use {string.Join(", ", Currencies)}.");
        }

        if (inputs.TaxAmount is < 0m)
        {
            return (CodeInputInvalid, "The tax cannot be negative.");
        }

        foreach (var line in inputs.Lines)
        {
            if (line.Quantity is null || line.Quantity <= 0m)
            {
                return (CodeInputInvalid, "Every invoice line needs a quantity above zero.");
            }

            if (line.UnitPrice is null || line.UnitPrice < 0m)
            {
                return (CodeInputInvalid, "Every invoice line needs a unit price, which cannot be negative.");
            }
        }

        return null;
    }

    /// <summary>
    /// AC-1: a receipt that takes the cumulative accepted quantity past the ordered quantity is refused, and the refusal
    /// names both numbers and the line.
    /// </summary>
    public static (string Code, string Message)? CheckReceipt(
        OgOrder order,
        string lineId,
        decimal quantity,
        decimal alreadyReceived)
    {
        ArgumentNullException.ThrowIfNull(order);

        var line = order.Lines.FirstOrDefault(candidate =>
            string.Equals(candidate.Id, lineId?.Trim(), StringComparison.OrdinalIgnoreCase));

        if (line is null)
        {
            return (CodeInputInvalid, $"'{lineId}' is not a line on {order.Number}.");
        }

        if (quantity <= 0m)
        {
            return (CodeInputInvalid, "A receipt needs a quantity above zero.");
        }

        var total = alreadyReceived + quantity;

        return total > line.Quantity
            ? (CodeReceiptExceedsOrder,
                $"Receiving {Number(quantity)} against line {line.LineNumber.ToString(CultureInfo.InvariantCulture)} " +
                $"would take {order.Number} to {Number(total)} of the {Number(line.Quantity)} ordered - " +
                $"{Number(total - line.Quantity)} more than was ordered. Raise a supplement to the order first.")
            : null;
    }

    internal static string Number(decimal value) =>
        value.ToString("#,0.####", CultureInfo.InvariantCulture);

    /// <summary>The order an AC-1 check runs against: its number and its lines.</summary>
    public sealed record OgOrder(string Number, IReadOnlyList<OgOrderLine> Lines);
}

/// <summary>ACC-05: purchase orders, receipts, supplier invoices, the three-way match and payment runs.</summary>
public interface IOgPayables
{
    /// <summary>Raises a purchase order in draft.</summary>
    Task<OgResult<OgPayablesWorkspace>> RaiseOrderAsync(
        OgOrderInputs inputs,
        OgPayableContext context,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Approves an order; the author may not approve it.</summary>
    Task<OgResult<OgPayablesWorkspace>> ApproveOrderAsync(
        string orderId,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Books goods in against an approved order, refusing a receipt above the order (AC-1).</summary>
    Task<OgResult<OgPayablesWorkspace>> ReceiveAsync(
        string orderId,
        string lineId,
        decimal quantity,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Captures a supplier invoice: a duplicate number is refused (AC-3), an AFE commitment above the remaining is refused
    /// (AC-4), and the three-way match runs and records its exceptions (AC-2).
    /// </summary>
    Task<OgResult<OgPayablesWorkspace>> CaptureAsync(
        OgSupplierInvoiceInputs inputs,
        OgPayableContext context,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Approves a match exception; the author may not approve it (AC-6).</summary>
    Task<OgResult<OgPayablesWorkspace>> ApproveExceptionAsync(
        string invoiceId,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Proposes a run over the invoices that are due and payable.</summary>
    Task<OgResult<OgPayablesWorkspace>> ProposeRunAsync(
        DateTime date,
        OgPayableContext context,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Approves or returns a proposed run; the proposer may not approve it (AC-6).</summary>
    Task<OgResult<OgPayablesWorkspace>> ReviewRunAsync(
        string runId,
        bool approve,
        string reason,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Posts the run: exactly one journal for every invoice in it (AC-5).</summary>
    Task<OgResult<OgPayablesWorkspace>> PostRunAsync(
        string runId,
        OgPayableContext context,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>The orders, invoices and runs; an unavailable register is a state.</summary>
    Task<OgResult<OgPayablesWorkspace>> LoadAsync(
        OgPayableContext context,
        string? orderId = null,
        string? invoiceId = null,
        string? runId = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// ACC-05 over ProductionAccounting's orders, receipts, supplier invoices and payments, the AFE commitments ECO-02 owns, and
/// the ledger's journal, through units of work.
/// </summary>
/// <remarks>
/// The tolerances are the owner's recorded matching policy, read from <c>ACCOUNTING_METHOD</c> rows whose
/// <c>METHOD_TYPE</c> is <c>AP_MATCHING</c> and whose status is approved, via
/// <see cref="CompanyMatchingPolicyReview.Rules"/> - the owner's own parser, so the desk cannot invent a tolerance. Exactly
/// one policy must cover the invoice's currency and date, which is the owner's own rule ("no newest-row fallback"); when
/// none does, the capture is refused with the currency and date named rather than matching against an assumed tolerance.
/// </remarks>
public sealed class OgPayablesService : IOgPayables
{
    private const string MatchingMethodType = "AP_MATCHING";
    private const string ApprovedPolicyStatus = "APPROVED";

    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgPayablesService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgPayablesWorkspace>> RaiseOrderAsync(
        OgOrderInputs inputs,
        OgPayableContext context,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgPayablesWorkspace>.Failed(
                "An order must record who raised it.", PayableRules.CodeActorRequired);
        }

        if (PayableRules.ValidateOrder(inputs) is { } refusal)
        {
            return OgResult<OgPayablesWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgPayablesWorkspace>.Failed(
                "No connection is open, so the order cannot be raised.", PayableRules.CodeNoConnection);
        }

        try
        {
            var now = DateTime.UtcNow;
            var orderId = Guid.NewGuid().ToString("N");
            var currency = inputs.Currency!.Trim().ToUpperInvariant();
            var total = inputs.Lines.Sum(line => Math.Round(line.Quantity!.Value * line.UnitPrice!.Value, 2,
                MidpointRounding.AwayFromZero));

            var orders = await OgUow.ReadAllAsync<PURCHASE_ORDER>(source).ConfigureAwait(false);
            var number = NextNumber(orders.Select(row => row.PO_NUMBER).ToList(), "PO-", inputs.Date!.Value);

            var unit = OgUow.For<PURCHASE_ORDER>(source);
            unit.PrimaryKey = "PURCHASE_ORDER_ID";

            unit.Add(new PURCHASE_ORDER
            {
                PURCHASE_ORDER_ID = orderId,
                PO_NUMBER = number,
                VENDOR_BA_ID = inputs.VendorId!.Trim(),
                PO_DATE = inputs.Date.Value.Date,
                EXPECTED_DELIVERY_DATE = inputs.ExpectedDelivery?.Date,
                CURRENCY_CODE = currency,
                TOTAL_AMOUNT = total,
                STATUS = ProcurementCodes.OrderDraft,
                DESCRIPTION = inputs.Description,
                AUTHOR = actor.Trim(),
                AFE_ID = string.IsNullOrWhiteSpace(inputs.AfeId) ? null : inputs.AfeId.Trim(),
                OWNER_TENANT_ID = context.CompanyId,
                OWNER_COMPANY_BA_ID = context.CompanyId,
                ACTIVE_IND = "Y",
                PPDM_GUID = Guid.NewGuid().ToString("N"),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgPayablesWorkspace>.Failed(
                    OgUow.Message(committed, "The order was not saved."), PayableRules.CodeWriteFailed);
            }

            if (!await WriteOrderLinesAsync(source, orderId, inputs.Lines, currency, actor, cancellationToken)
                    .ConfigureAwait(false))
            {
                return OgResult<OgPayablesWorkspace>.Failed(
                    "The order's lines were not saved, so nothing was raised.", PayableRules.CodeWriteFailed);
            }

            var workspace = await ReadAsync(source, context, orderId, null, null, cancellationToken)
                .ConfigureAwait(false);

            return OgResult<OgPayablesWorkspace>.Ok(workspace);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "raising a purchase order", "the order was not raised", FailureSeverity.Error);

            return OgResult<OgPayablesWorkspace>.Failed(
                "The order could not be raised.", reference ?? PayableRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgPayablesWorkspace>> ApproveOrderAsync(
        string orderId,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgPayablesWorkspace>.Failed(
                "An approval must record who made it.", PayableRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgPayablesWorkspace>.Failed(
                "No connection is open, so the order cannot be approved.", PayableRules.CodeNoConnection);
        }

        try
        {
            var order = (await ReadAsync(source, null, orderId, null, null, cancellationToken).ConfigureAwait(false))
                .SelectedOrder;

            if (order is null)
            {
                return OgResult<OgPayablesWorkspace>.Failed(
                    $"Order '{orderId}' is not in this database.", PayableRules.CodeUnknownOrder);
            }

            if (!order.IsDraft)
            {
                return OgResult<OgPayablesWorkspace>.Failed(
                    $"{order.Describe()} is {order.StateText}.", PayableRules.CodeOrderNotApproved);
            }

            // The same segregation the money commands use: the person who raised it does not approve it.
            if (!string.IsNullOrWhiteSpace(order.Author)
                && string.Equals(order.Author.Trim(), actor.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgPayablesWorkspace>.Failed(
                    $"{order.Number} was raised by {order.Author}, who therefore cannot approve it. A different buyer " +
                    "must sign it.",
                    PayableRules.CodeAuthorCannotApprove);
            }

            var row = await ReadOrderRowAsync(source, order.Id, cancellationToken).ConfigureAwait(false);

            if (row is null)
            {
                return OgResult<OgPayablesWorkspace>.Failed(
                    $"Order '{orderId}' is not in this database.", PayableRules.CodeUnknownOrder);
            }

            row.STATUS = ProcurementCodes.OrderApproved;
            row.APPROVED_BY = actor.Trim();
            row.APPROVED_AT = DateTime.UtcNow;
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = DateTime.UtcNow;

            var written = await UpdateAsync(source, row).ConfigureAwait(false);

            if (written is not null)
            {
                return OgResult<OgPayablesWorkspace>.Failed(written, PayableRules.CodeWriteFailed);
            }

            var workspace = await ReadAsync(source, null, order.Id, null, null, cancellationToken)
                .ConfigureAwait(false);

            return OgResult<OgPayablesWorkspace>.Ok(workspace);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"approving purchase order {orderId}", "the order was not approved", FailureSeverity.Error);

            return OgResult<OgPayablesWorkspace>.Failed(
                "The order could not be approved.", reference ?? PayableRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgPayablesWorkspace>> ReceiveAsync(
        string orderId,
        string lineId,
        decimal quantity,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgPayablesWorkspace>.Failed(
                "A receipt must record who booked it in.", PayableRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgPayablesWorkspace>.Failed(
                "No connection is open, so the receipt cannot be booked in.", PayableRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, null, orderId, null, null, cancellationToken)
                .ConfigureAwait(false);
            var order = workspace.SelectedOrder;

            if (order is null)
            {
                return OgResult<OgPayablesWorkspace>.Failed(
                    $"Order '{orderId}' is not in this database.", PayableRules.CodeUnknownOrder);
            }

            if (!order.AcceptsReceipts)
            {
                return OgResult<OgPayablesWorkspace>.Failed(
                    $"{order.Number} is {order.StateText}; goods are booked in against an approved order.",
                    PayableRules.CodeOrderNotApproved);
            }

            var line = order.Lines.FirstOrDefault(candidate =>
                string.Equals(candidate.Id, lineId?.Trim(), StringComparison.OrdinalIgnoreCase));

            // AC-1 and SG-10-6: the cumulative received quantity may not pass the ordered quantity.
            if (PayableRules.CheckReceipt(
                    new PayableRules.OgOrder(order.Number, order.Lines), lineId, quantity, line?.Received ?? 0m)
                is { } refusal)
            {
                return OgResult<OgPayablesWorkspace>.Failed(refusal.Message, refusal.Code);
            }

            var now = DateTime.UtcNow;
            var unit = OgUow.For<PO_RECEIPT>(source);
            unit.PrimaryKey = "PO_RECEIPT_ID";

            unit.Add(new PO_RECEIPT
            {
                PO_RECEIPT_ID = Guid.NewGuid().ToString("N"),
                PURCHASE_ORDER_ID = order.Id,
                PO_LINE_ITEM_ID = line!.Id,
                RECEIPT_DATE = now.Date,
                RECEIVED_QUANTITY = quantity,
                ACCEPTED_QUANTITY = quantity,
                REJECTED_QUANTITY = 0m,
                QUANTITY_UOM = line.Unit,
                RECEIPT_STATUS = ProcurementCodes.ReceiptReceived,
                RECEIVED_BY = actor.Trim(),
                DESCRIPTION = "received against " + order.Number,
                ACTIVE_IND = "Y",
                PPDM_GUID = Guid.NewGuid().ToString("N"),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgPayablesWorkspace>.Failed(
                    OgUow.Message(committed, "The receipt was not saved."), PayableRules.CodeWriteFailed);
            }

            var after = await ReadAsync(source, null, order.Id, null, null, cancellationToken)
                .ConfigureAwait(false);

            return OgResult<OgPayablesWorkspace>.Ok(after);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"booking in goods against {orderId}", "the receipt was not recorded", FailureSeverity.Error);

            return OgResult<OgPayablesWorkspace>.Failed(
                "The receipt could not be recorded.", reference ?? PayableRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgPayablesWorkspace>> CaptureAsync(
        OgSupplierInvoiceInputs inputs,
        OgPayableContext context,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgPayablesWorkspace>.Failed(
                "An invoice must record who captured it.", PayableRules.CodeActorRequired);
        }

        if (PayableRules.ValidateInvoice(inputs) is { } refusal)
        {
            return OgResult<OgPayablesWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgPayablesWorkspace>.Failed(
                "No connection is open, so the invoice cannot be captured.", PayableRules.CodeNoConnection);
        }

        try
        {
            var currency = inputs.Currency!.Trim().ToUpperInvariant();
            var number = inputs.Number!.Trim();

            // AC-3: the supplier's own number is the identity of the document - a second capture is a duplicate, whatever
            // its totals say.
            var existingInvoices = await OgUow.ReadAllAsync<AP_INVOICE>(source).ConfigureAwait(false);

            var duplicate = existingInvoices.FirstOrDefault(row =>
                string.Equals(row.INVOICE_NUMBER?.Trim(), number, StringComparison.OrdinalIgnoreCase)
                && string.Equals(row.VENDOR_BA_ID?.Trim(), inputs.VendorId!.Trim(), StringComparison.OrdinalIgnoreCase));

            if (duplicate is not null)
            {
                return OgResult<OgPayablesWorkspace>.Failed(
                    $"Supplier invoice {number} from {inputs.VendorId} is already captured " +
                    $"({duplicate.AP_INVOICE_ID}, {duplicate.STATUS?.ToLowerInvariant()}); a second capture would pay " +
                    "the same document twice.",
                    PayableRules.CodeDuplicateInvoiceNumber);
            }

            var order = default(OgPurchaseOrder);

            if (!string.IsNullOrWhiteSpace(inputs.OrderId))
            {
                order = (await ReadAsync(source, null, inputs.OrderId, null, null, cancellationToken)
                    .ConfigureAwait(false)).SelectedOrder;

                if (order is null)
                {
                    return OgResult<OgPayablesWorkspace>.Failed(
                        $"Order '{inputs.OrderId}' is not in this database.", PayableRules.CodeUnknownOrder);
                }

                if (!string.Equals(order.Currency, currency, StringComparison.OrdinalIgnoreCase))
                {
                    return OgResult<OgPayablesWorkspace>.Failed(
                        $"{order.Number} is in {order.Currency} and the invoice is in {currency}; the owner's matching " +
                        "rule is an exact recorded currency.",
                        PayableRules.CodeCurrencyMismatch);
                }
            }

            var subtotal = inputs.Lines.Sum(line => Math.Round(line.Quantity!.Value * line.UnitPrice!.Value, 2,
                MidpointRounding.AwayFromZero));
            var tax = inputs.TaxAmount ?? 0m;
            var total = subtotal + tax;

            // AC-4: the commitment this invoice makes may not pass what the authorisation has left.
            var afeId = string.IsNullOrWhiteSpace(inputs.AfeId) ? order?.AfeId : inputs.AfeId!.Trim();

            if (!string.IsNullOrWhiteSpace(afeId))
            {
                var afe = await ReadAfeAsync(source, afeId!, cancellationToken).ConfigureAwait(false);

                if (afe is null)
                {
                    return OgResult<OgPayablesWorkspace>.Failed(
                        $"AFE '{afeId}' is not in this database.", PayableRules.CodeInputInvalid);
                }

                var alreadyCommitted = await ReadAfeCommittedAsync(source, afeId!, cancellationToken)
                    .ConfigureAwait(false);
                var remaining = (afe.APPROVED_AMOUNT ?? afe.ESTIMATED_COST ?? 0m) - alreadyCommitted;

                if (subtotal > remaining)
                {
                    return OgResult<OgPayablesWorkspace>.Failed(
                        $"Invoice {number} commits {MoneyText.Of(subtotal)} against AFE {afe.AFE_NUMBER}, which has " +
                        $"{MoneyText.Of(remaining)} left of {MoneyText.Of(afe.APPROVED_AMOUNT ?? 0m)} after " +
                        $"{MoneyText.Of(alreadyCommitted)} committed. Raise and approve a supplement before capturing " +
                        "this invoice.",
                        PayableRules.CodeCommitmentExceedsAfe);
                }
            }

            // AC-2: the match. Its tolerances are the recorded policy, never an assumed one.
            var policy = await SelectPolicyAsync(source, currency, inputs.Date!.Value, cancellationToken)
                .ConfigureAwait(false);

            if ((policy.Id is null || policy.Rules is null) && order is not null)
            {
                var why = policy.CandidateCount > 1
                    ? $"{policy.CandidateCount.ToString(CultureInfo.InvariantCulture)} approved policies overlap " +
                      $"{currency} on {inputs.Date.Value:yyyy-MM-dd}; exactly one must apply"
                    : policy.Malformed.Count > 0
                        ? "the approved policy that should apply cannot be read: " + string.Join("; ", policy.Malformed)
                        : $"no approved policy covers {currency} on {inputs.Date.Value:yyyy-MM-dd}";

                return OgResult<OgPayablesWorkspace>.Failed(
                    $"The three-way match cannot run because {why}. Record and approve one matching policy with the " +
                    "tolerances this invoice must be matched against before capturing a purchase-order invoice.",
                    PayableRules.CodeNoMatchingPolicy);
            }

            var matched = MatchInvoice(inputs, order, policy.Rules);

            var now = DateTime.UtcNow;
            var invoiceId = Guid.NewGuid().ToString("N");

            var unit = OgUow.For<AP_INVOICE>(source);
            unit.PrimaryKey = "AP_INVOICE_ID";

            unit.Add(new AP_INVOICE
            {
                AP_INVOICE_ID = invoiceId,
                INVOICE_NUMBER = number,
                VENDOR_BA_ID = inputs.VendorId!.Trim(),
                INVOICE_DATE = inputs.Date.Value.Date,
                DUE_DATE = inputs.DueDate?.Date,
                CURRENCY_CODE = currency,
                SUBTOTAL_AMOUNT = subtotal,
                TAX_AMOUNT = tax,
                TOTAL_AMOUNT = total,
                PAID_AMOUNT = 0m,
                BALANCE_DUE = total,
                STATUS = InvoiceStatuses.Issued,
                AUTHOR = actor.Trim(),
                PURCHASE_ORDER_ID = order?.Id,
                AFE_ID = string.IsNullOrWhiteSpace(afeId) ? null : afeId,
                MATCH_STATE = matched.State,
                MATCH_EXCEPTIONS_TEXT = matched.Exceptions.Count == 0
                    ? null
                    : string.Join("\n", matched.Exceptions.Select(exception => exception.Describe())),
                MATCH_POLICY_ID = policy.Id,
                OWNER_TENANT_ID = context.CompanyId,
                OWNER_COMPANY_BA_ID = context.CompanyId,
                ACTIVE_IND = "Y",
                PPDM_GUID = Guid.NewGuid().ToString("N"),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgPayablesWorkspace>.Failed(
                    OgUow.Message(committed, "The invoice was not saved."), PayableRules.CodeWriteFailed);
            }

            if (!await WriteInvoiceLinesAsync(
                    source, invoiceId, inputs.Lines, order, currency, actor, cancellationToken).ConfigureAwait(false))
            {
                return OgResult<OgPayablesWorkspace>.Failed(
                    "The invoice's lines were not saved, so nothing was captured.", PayableRules.CodeWriteFailed);
            }

            // The commitment is written so the AFE shows what this invoice commits, which is what AC-4 measures next time.
            if (!string.IsNullOrWhiteSpace(afeId)
                && !await WriteAfeCommitmentAsync(source, afeId!, number, subtotal, actor, cancellationToken)
                    .ConfigureAwait(false))
            {
                return OgResult<OgPayablesWorkspace>.Failed(
                    "The invoice was captured but its AFE commitment was not recorded, so the authorisation does not " +
                    "show it. Nothing further can be captured against that AFE until it does.",
                    PayableRules.CodeWriteFailed);
            }

            var workspace = await ReadAsync(source, context, order?.Id, invoiceId, null, cancellationToken)
                .ConfigureAwait(false);

            return OgResult<OgPayablesWorkspace>.Ok(workspace);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"capturing supplier invoice {inputs.Number}", "the invoice was not captured",
                FailureSeverity.Error);

            return OgResult<OgPayablesWorkspace>.Failed(
                "The invoice could not be captured.", reference ?? PayableRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgPayablesWorkspace>> ApproveExceptionAsync(
        string invoiceId,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgPayablesWorkspace>.Failed(
                "An exception approval must record who made it.", PayableRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgPayablesWorkspace>.Failed(
                "No connection is open, so the exception cannot be approved.", PayableRules.CodeNoConnection);
        }

        try
        {
            var invoice = (await ReadAsync(source, null, null, invoiceId, null, cancellationToken)
                .ConfigureAwait(false)).SelectedInvoice;

            if (invoice is null)
            {
                return OgResult<OgPayablesWorkspace>.Failed(
                    $"Invoice '{invoiceId}' is not in this database.", PayableRules.CodeUnknownInvoice);
            }

            if (!invoice.NeedsExceptionApproval)
            {
                return OgResult<OgPayablesWorkspace>.Failed(
                    $"{invoice.Describe()} is {invoice.MatchState}; there is no exception awaiting approval.",
                    PayableRules.CodeExceptionNotApproved);
            }

            // AC-6: the person who captured the invoice cannot approve its exception.
            if (!string.IsNullOrWhiteSpace(invoice.Author)
                && string.Equals(invoice.Author.Trim(), actor.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgPayablesWorkspace>.Failed(
                    $"{invoice.Number} was captured by {invoice.Author}, who therefore cannot approve its exception. " +
                    "A different controller must review it.",
                    PayableRules.CodeAuthorCannotApprove);
            }

            var row = await ReadInvoiceRowAsync(source, invoice.Id, cancellationToken).ConfigureAwait(false);

            if (row is null)
            {
                return OgResult<OgPayablesWorkspace>.Failed(
                    $"Invoice '{invoiceId}' is not in this database.", PayableRules.CodeUnknownInvoice);
            }

            row.MATCH_STATE = ProcurementCodes.MatchExceptionApproved;
            row.EXCEPTION_APPROVED_BY = actor.Trim();
            row.EXCEPTION_APPROVED_AT = DateTime.UtcNow;
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = DateTime.UtcNow;

            var written = await UpdateAsync(source, row).ConfigureAwait(false);

            if (written is not null)
            {
                return OgResult<OgPayablesWorkspace>.Failed(written, PayableRules.CodeWriteFailed);
            }

            var workspace = await ReadAsync(source, null, null, invoice.Id, null, cancellationToken)
                .ConfigureAwait(false);

            return OgResult<OgPayablesWorkspace>.Ok(workspace);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"approving a match exception on {invoiceId}", "the exception was not approved",
                FailureSeverity.Error);

            return OgResult<OgPayablesWorkspace>.Failed(
                "The exception could not be approved.", reference ?? PayableRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgPayablesWorkspace>> ProposeRunAsync(
        DateTime date,
        OgPayableContext context,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgPayablesWorkspace>.Failed(
                "A payment run must record who proposed it.", PayableRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgPayablesWorkspace>.Failed(
                "No connection is open, so the run cannot be proposed.", PayableRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, context, null, null, null, cancellationToken)
                .ConfigureAwait(false);

            // Only invoices that are due, payable and in one currency: a run pays one currency with one journal, so mixing
            // currencies would need an exchange rate nobody stated.
            var due = workspace.Invoices
                .Where(invoice => invoice.IsPayable)
                .Where(invoice => (invoice.DueDate ?? invoice.Date).Date <= date.Date)
                .ToList();

            if (due.Count == 0)
            {
                return OgResult<OgPayablesWorkspace>.Failed(
                    $"No captured invoice is both payable and due on or before {date:yyyy-MM-dd}, so there is nothing " +
                    "to pay. An invoice with an unapproved exception is not payable.",
                    PayableRules.CodeRunNothingDue);
            }

            var currencies = due.Select(invoice => invoice.Currency).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            if (currencies.Count > 1)
            {
                return OgResult<OgPayablesWorkspace>.Failed(
                    $"Due invoices span {string.Join(", ", currencies)}; a run pays one currency with one journal. " +
                    "Propose a run per currency.",
                    PayableRules.CodeCurrencyMismatch);
            }

            var now = DateTime.UtcNow;
            var runId = Guid.NewGuid().ToString("N");
            var currency = currencies[0];

            var runs = await OgUow.ReadAllAsync<AP_PAYMENT_RUN>(source).ConfigureAwait(false);
            var number = NextNumber(runs.Select(row => row.RUN_NUMBER).ToList(), "PAY-", date);

            var unit = OgUow.For<AP_PAYMENT_RUN>(source);
            unit.PrimaryKey = "AP_PAYMENT_RUN_ID";

            unit.Add(new AP_PAYMENT_RUN
            {
                AP_PAYMENT_RUN_ID = runId,
                RUN_NUMBER = number,
                RUN_DATE = date.Date,
                TOTAL_AMOUNT = due.Sum(invoice => invoice.Balance),
                CURRENCY_CODE = currency,
                PAYMENT_METHOD = ProcurementCodes.PaymentMethods[0],
                STATUS = ProcurementCodes.RunDraft,
                AUTHOR = actor.Trim(),
                OWNER_TENANT_ID = context.CompanyId,
                OWNER_COMPANY_BA_ID = context.CompanyId,
                BOOK_ID = context.BookCode,
                ACTIVE_IND = "Y",
                PPDM_GUID = Guid.NewGuid().ToString("N"),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgPayablesWorkspace>.Failed(
                    OgUow.Message(committed, "The run was not proposed."), PayableRules.CodeWriteFailed);
            }

            var payments = OgUow.For<AP_PAYMENT>(source);
            payments.PrimaryKey = "AP_PAYMENT_ID";

            var sequence = 1;

            foreach (var invoice in due)
            {
                payments.Add(new AP_PAYMENT
                {
                    AP_PAYMENT_ID = Guid.NewGuid().ToString("N"),
                    AP_INVOICE_ID = invoice.Id,
                    RUN_ID = runId,
                    PAYMENT_NUMBER = number + "-" + sequence.ToString("00", CultureInfo.InvariantCulture),
                    PAYMENT_DATE = date.Date,
                    PAYMENT_AMOUNT = invoice.Balance,
                    PAYMENT_METHOD = ProcurementCodes.PaymentMethods[0],
                    PAYMENT_STATUS = ProcurementCodes.RunDraft,
                    ACTIVE_IND = "Y",
                    PPDM_GUID = Guid.NewGuid().ToString("N"),
                    ROW_CREATED_BY = actor.Trim(),
                    ROW_CREATED_DATE = now,
                    ROW_CHANGED_BY = actor.Trim(),
                    ROW_CHANGED_DATE = now,
                });

                sequence++;
            }

            var paymentsCommitted = await payments.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(paymentsCommitted))
            {
                return OgResult<OgPayablesWorkspace>.Failed(
                    OgUow.Message(paymentsCommitted, "The run's payments were not saved."),
                    PayableRules.CodeWriteFailed);
            }

            var after = await ReadAsync(source, context, null, null, runId, cancellationToken)
                .ConfigureAwait(false);

            return OgResult<OgPayablesWorkspace>.Ok(after);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "proposing a payment run", "the run was not proposed", FailureSeverity.Error);

            return OgResult<OgPayablesWorkspace>.Failed(
                "The run could not be proposed.", reference ?? PayableRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgPayablesWorkspace>> ReviewRunAsync(
        string runId,
        bool approve,
        string reason,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgPayablesWorkspace>.Failed(
                "A review must record who made it.", PayableRules.CodeActorRequired);
        }

        if (!approve && string.IsNullOrWhiteSpace(reason))
        {
            return OgResult<OgPayablesWorkspace>.Failed(
                "Returning a run needs its reason.", PayableRules.CodeReturnReasonRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgPayablesWorkspace>.Failed(
                "No connection is open, so the run cannot be reviewed.", PayableRules.CodeNoConnection);
        }

        try
        {
            var run = (await ReadAsync(source, null, null, null, runId, cancellationToken).ConfigureAwait(false))
                .SelectedRun;

            if (run is null)
            {
                return OgResult<OgPayablesWorkspace>.Failed(
                    $"Payment run '{runId}' is not in this database.", PayableRules.CodeUnknownRun);
            }

            if (!run.IsDraft)
            {
                return OgResult<OgPayablesWorkspace>.Failed(
                    $"{run.Describe()} is {run.StateText}; only a proposed run is reviewed.",
                    PayableRules.CodeRunNotApproved);
            }

            // AC-6: the person who proposed the run cannot approve it.
            if (approve && !string.IsNullOrWhiteSpace(run.Author)
                && string.Equals(run.Author.Trim(), actor.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgPayablesWorkspace>.Failed(
                    $"{run.Number} was proposed by {run.Author}, who therefore cannot approve it. A different " +
                    "controller must release the payment.",
                    PayableRules.CodeAuthorCannotApprove);
            }

            var row = await ReadRunRowAsync(source, run.Id, cancellationToken).ConfigureAwait(false);

            if (row is null)
            {
                return OgResult<OgPayablesWorkspace>.Failed(
                    $"Payment run '{runId}' is not in this database.", PayableRules.CodeUnknownRun);
            }

            row.STATUS = approve ? ProcurementCodes.RunApproved : ProcurementCodes.RunReturned;
            row.APPROVED_BY = approve ? actor.Trim() : null;
            row.APPROVED_AT = approve ? DateTime.UtcNow : null;
            row.RETURN_REASON = approve ? null : reason.Trim();
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = DateTime.UtcNow;

            var written = await UpdateAsync(source, row).ConfigureAwait(false);

            if (written is not null)
            {
                return OgResult<OgPayablesWorkspace>.Failed(written, PayableRules.CodeWriteFailed);
            }

            var workspace = await ReadAsync(source, null, null, null, run.Id, cancellationToken)
                .ConfigureAwait(false);

            return OgResult<OgPayablesWorkspace>.Ok(workspace);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"reviewing payment run {runId}", "the run was not reviewed", FailureSeverity.Error);

            return OgResult<OgPayablesWorkspace>.Failed(
                "The run could not be reviewed.", reference ?? PayableRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgPayablesWorkspace>> PostRunAsync(
        string runId,
        OgPayableContext context,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgPayablesWorkspace>.Failed(
                "A posting must record who made it.", PayableRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgPayablesWorkspace>.Failed(
                "No connection is open, so the run cannot be posted.", PayableRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, context, null, null, runId, cancellationToken)
                .ConfigureAwait(false);
            var run = workspace.SelectedRun;

            if (run is null)
            {
                return OgResult<OgPayablesWorkspace>.Failed(
                    $"Payment run '{runId}' is not in this database.", PayableRules.CodeUnknownRun);
            }

            // Posting is a transition of the run that exists, so a second post cannot create a second journal.
            if (run.IsPosted && run.HasJournal)
            {
                return OgResult<OgPayablesWorkspace>.Ok(workspace);
            }

            if (!run.IsApproved && !run.IsPosted)
            {
                return OgResult<OgPayablesWorkspace>.Failed(
                    $"{run.Describe()} is {run.StateText}; a run is posted after it is approved.",
                    PayableRules.CodeRunNotApproved);
            }

            var periodCode = LedgerCodes.PeriodCodeOf(run.Date);
            var periods = await OgUow.ReadAllAsync<COMPANY_LEDGER_PERIOD>(source).ConfigureAwait(false);
            var period = periods.FirstOrDefault(row =>
                string.Equals(row.PERIOD_CODE, periodCode, StringComparison.OrdinalIgnoreCase));

            if (period is not null && !LedgerCodes.IsOpen(period.STATUS))
            {
                return OgResult<OgPayablesWorkspace>.Failed(
                    $"Period {periodCode} is {period.STATUS.ToLowerInvariant()}, so {run.Number} cannot be posted " +
                    "into it. Reopen the period first.",
                    PayableRules.CodePeriodClosed);
            }

            var accounts = await OgUow.ReadAllAsync<GL_ACCOUNT>(source).ConfigureAwait(false);
            var payable = Find(accounts, ProcurementCodes.PayableAccount);
            var cash = Find(accounts, ProcurementCodes.CashAccount);

            if (payable is null || cash is null)
            {
                return OgResult<OgPayablesWorkspace>.Failed(
                    $"The chart of accounts has no account " +
                    $"{(payable is null ? ProcurementCodes.PayableAccount : ProcurementCodes.CashAccount)}, so " +
                    $"{run.Number} cannot be posted. Add the account before posting.",
                    PayableRules.CodeUnknownAccount);
            }

            // The journal's reference is derived from the run id, which exists before any numbering, so a retry after a
            // lost acknowledgement adopts the entry it already wrote (AC-5).
            var journalReference = "PAYRUN-" + run.Id;
            var journals = await OgUow.ReadAllAsync<JOURNAL_ENTRY>(source).ConfigureAwait(false);

            var existing = journals.FirstOrDefault(entry =>
                string.Equals(entry.SOURCE_MODULE, "AP_PAYMENT_RUN", StringComparison.OrdinalIgnoreCase)
                && string.Equals(entry.REFERENCE_NUMBER, journalReference, StringComparison.OrdinalIgnoreCase));

            var now = DateTime.UtcNow;
            string journalId;

            if (existing is not null)
            {
                journalId = existing.JOURNAL_ENTRY_ID!;
            }
            else
            {
                journalId = Guid.NewGuid().ToString("N");

                // AC-5: one entry for the whole run, balanced by construction - payables down, cash down.
                var journal = OgUow.For<JOURNAL_ENTRY>(source);
                journal.PrimaryKey = "JOURNAL_ENTRY_ID";

                journal.Add(new JOURNAL_ENTRY
                {
                    JOURNAL_ENTRY_ID = journalId,
                    ENTRY_NUMBER = NextJournalNumber(journals, run.Date),
                    ENTRY_DATE = run.Date,
                    ENTRY_TYPE = "PAYMENT",
                    STATUS = LedgerCodes.JournalPosted,
                    DESCRIPTION = "Payment run " + run.Number,
                    REFERENCE_NUMBER = journalReference,
                    SOURCE_MODULE = "AP_PAYMENT_RUN",
                    TOTAL_DEBIT = run.Total,
                    TOTAL_CREDIT = run.Total,
                    AUTHOR = actor.Trim(),
                    POSTED_BY = actor.Trim(),
                    POSTED_AT = now,
                    PERIOD_CODE = periodCode,
                    ACTIVE_IND = "Y",
                    PPDM_GUID = Guid.NewGuid().ToString("N"),
                    ROW_CREATED_BY = actor.Trim(),
                    ROW_CREATED_DATE = now,
                    ROW_CHANGED_BY = actor.Trim(),
                    ROW_CHANGED_DATE = now,
                });

                var journalCommitted = await journal.Commit().ConfigureAwait(false);

                if (!OgUow.IsOk(journalCommitted))
                {
                    return OgResult<OgPayablesWorkspace>.Failed(
                        OgUow.Message(journalCommitted, "The run's journal was not written; nothing was paid."),
                        PayableRules.CodeWriteFailed);
                }

                var lines = OgUow.For<JOURNAL_ENTRY_LINE>(source);
                lines.PrimaryKey = "JOURNAL_ENTRY_LINE_ID";

                lines.Add(EntryLine(journalId, payable!, run.Total, 0m, 1, "payment run", actor, now));
                lines.Add(EntryLine(journalId, cash!, 0m, run.Total, 2, "payment run", actor, now));

                var linesCommitted = await lines.Commit().ConfigureAwait(false);

                if (!OgUow.IsOk(linesCommitted))
                {
                    return OgResult<OgPayablesWorkspace>.Failed(
                        "The journal's lines were not saved.", PayableRules.CodeWriteFailed);
                }
            }

            // Each invoice in the run is reduced by its own payment - read through the unit that writes.
            foreach (var invoiceNumber in run.InvoiceNumbers)
            {
                var invoice = workspace.Invoices.FirstOrDefault(candidate =>
                    string.Equals(candidate.Number, invoiceNumber, StringComparison.OrdinalIgnoreCase));

                if (invoice is null || invoice.Balance <= 0m)
                {
                    continue;
                }

                var invoiceUnit = OgUow.For<AP_INVOICE>(source);
                invoiceUnit.PrimaryKey = "AP_INVOICE_ID";

                var rows = await OgUow.ReadAsync(
                    invoiceUnit, [OgUow.Equal("AP_INVOICE_ID", invoice.Id)], cancellationToken)
                    .ConfigureAwait(false);

                var invoiceRow = rows.FirstOrDefault();

                if (invoiceRow is null)
                {
                    continue;
                }

                var paid = (invoiceRow.PAID_AMOUNT ?? 0m) + invoice.Balance;
                var balance = (invoiceRow.TOTAL_AMOUNT ?? 0m) - paid;

                invoiceRow.PAID_AMOUNT = paid;
                invoiceRow.BALANCE_DUE = balance;
                invoiceRow.STATUS = balance <= 0m ? InvoiceStatuses.Paid : InvoiceStatuses.PartiallyPaid;
                invoiceRow.ROW_CHANGED_BY = actor.Trim();
                invoiceRow.ROW_CHANGED_DATE = now;

                var updated = invoiceUnit.Update(invoiceRow);

                if (!OgUow.IsOk(updated))
                {
                    return OgResult<OgPayablesWorkspace>.Failed(
                        OgUow.Message(updated, "An invoice's paid amount was not updated."),
                        PayableRules.CodeWriteFailed);
                }

                var committed = await invoiceUnit.Commit().ConfigureAwait(false);

                if (!OgUow.IsOk(committed))
                {
                    return OgResult<OgPayablesWorkspace>.Failed(
                        OgUow.Message(committed, "An invoice's paid amount was not updated."),
                        PayableRules.CodeWriteFailed);
                }
            }

            // The payments and the run header become posted.
            var paymentUnit = OgUow.For<AP_PAYMENT>(source);
            paymentUnit.PrimaryKey = "AP_PAYMENT_ID";

            var payments = await OgUow.ReadAsync(
                paymentUnit, [OgUow.Equal("RUN_ID", run.Id)], cancellationToken).ConfigureAwait(false);

            foreach (var payment in payments)
            {
                payment.PAYMENT_STATUS = ProcurementCodes.RunPosted;
                payment.ROW_CHANGED_BY = actor.Trim();
                payment.ROW_CHANGED_DATE = now;

                var updated = paymentUnit.Update(payment);

                if (!OgUow.IsOk(updated))
                {
                    return OgResult<OgPayablesWorkspace>.Failed(
                        OgUow.Message(updated, "The run's payments were not marked posted."),
                        PayableRules.CodeWriteFailed);
                }
            }

            await paymentUnit.Commit().ConfigureAwait(false);

            var header = await ReadRunRowAsync(source, run.Id, cancellationToken).ConfigureAwait(false);

            if (header is null)
            {
                return OgResult<OgPayablesWorkspace>.Failed(
                    $"Payment run '{runId}' is not in this database.", PayableRules.CodeUnknownRun);
            }

            header.STATUS = ProcurementCodes.RunPosted;
            header.JOURNAL_ENTRY_ID = journalId;
            header.POSTED_BY = actor.Trim();
            header.POSTED_AT = now;
            header.ROW_CHANGED_BY = actor.Trim();
            header.ROW_CHANGED_DATE = now;

            var written = await UpdateAsync(source, header).ConfigureAwait(false);

            if (written is not null)
            {
                return OgResult<OgPayablesWorkspace>.Failed(written, PayableRules.CodeWriteFailed);
            }

            var after = await ReadAsync(source, context, null, null, run.Id, cancellationToken)
                .ConfigureAwait(false);

            return OgResult<OgPayablesWorkspace>.Ok(after);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"posting payment run {runId}", "the run was not posted", FailureSeverity.Error);

            return OgResult<OgPayablesWorkspace>.Failed(
                "The run could not be posted.", reference ?? PayableRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgPayablesWorkspace>> LoadAsync(
        OgPayableContext context,
        string? orderId = null,
        string? invoiceId = null,
        string? runId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var source = _source();

        if (source is null)
        {
            return OgResult<OgPayablesWorkspace>.Failed(
                "No connection is open, so the payables ledger cannot be read.", PayableRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, context, orderId, invoiceId, runId, cancellationToken)
                .ConfigureAwait(false);

            return OgResult<OgPayablesWorkspace>.Ok(workspace);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading the payables ledger", "the payables ledger is unavailable", FailureSeverity.Degraded);

            return OgResult<OgPayablesWorkspace>.Ok(new OgPayablesWorkspace(
                [], null, [], null, [], null,
                PayableRules.OrderStates, PayableRules.MatchStates, PayableRules.PaymentMethods,
                PayableRules.Currencies, null, string.Empty, true,
                $"the payables ledger could not be read (reference {reference})"));
        }
    }

    // ------------------------------------------------------------------ the match

    /// <summary>
    /// Runs the match for a captured invoice against its order and receipts, using the recorded policy's tolerances
    /// (AC-2). A non-PO invoice has nothing to match and is marked as such rather than silently passed.
    /// </summary>
    public static MatchResult MatchInvoice(
        OgSupplierInvoiceInputs inputs,
        OgPurchaseOrder? order,
        CompanyMatchingPolicyRules? rules)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        if (order is null)
        {
            return new MatchResult(ProcurementCodes.MatchNotApplicable, []);
        }

        var perLine = new List<MatchResult>();

        foreach (var line in inputs.Lines)
        {
            var orderLine = order.Lines.FirstOrDefault(candidate =>
                string.Equals(candidate.Id, line.PoLineItemId?.Trim(), StringComparison.OrdinalIgnoreCase));

            perLine.Add(ThreeWayMatch.Compare(
                orderLine?.Quantity,
                orderLine?.UnitPrice,
                orderLine?.Received,
                line.Quantity,
                line.UnitPrice,
                rules?.QuantityTolerancePercent,
                rules?.PriceTolerancePercent));
        }

        return ThreeWayMatch.CompareInvoice(
            currencyAgrees: true,
            perLine,
            rules?.QuantityTolerancePercent,
            rules?.PriceTolerancePercent);
    }

    /// <summary>
    /// The one approved matching policy that covers this currency and date.
    /// </summary>
    /// <remarks>
    /// The rows are the owner's (<c>ACCOUNTING_METHOD</c> with <c>METHOD_TYPE</c> of <c>AP_MATCHING</c>), and the rules are
    /// read with the owner's own parser. Exactly one must cover the invoice, which is the owner's rule - "no newest-row
    /// fallback is applied" - so an overlapping policy is a configuration error the desk reports rather than a coin toss.
    /// </remarks>
    private async Task<PolicySelection> SelectPolicyAsync(
        IDataSource source,
        string currency,
        DateTime date,
        CancellationToken cancellationToken)
    {
        var rows = await OgUow.ReadAllAsync<ACCOUNTING_METHOD>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var candidates = new List<(string Id, CompanyMatchingPolicyRules Rules)>();
        var malformed = new List<string>();

        foreach (var row in rows)
        {
            if (!string.Equals(row.METHOD_TYPE, MatchingMethodType, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(row.MATCHING_POLICY_STATUS, ApprovedPolicyStatus, StringComparison.OrdinalIgnoreCase)
                || string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // A malformed row is an ordinary answer to "is there a policy I can use?", so it is skipped and named -
            // and the failure is reported with a reference, because that is what a broken configuration deserves.
            if (!TryReadPolicy(row, out var rules, out var problem) || rules is null)
            {
                malformed.Add(problem ?? row.ACCOUNTING_METHOD_ID);
                continue;
            }

            if (!string.Equals(rules.CurrencyCode, currency, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (row.EFFECTIVE_DATE is { } effective && effective.Date > date.Date)
            {
                continue;
            }

            if (row.EXPIRY_DATE is { } expiry && expiry.Date < date.Date)
            {
                continue;
            }

            candidates.Add((row.ACCOUNTING_METHOD_ID, rules));
        }

        return new PolicySelection(
            candidates.Count == 1 ? candidates[0].Id : null,
            candidates.Count == 1 ? candidates[0].Rules : null,
            candidates.Count,
            malformed);
    }

    /// <summary>
    /// What the policy lookup found: the one policy that covers the invoice, how many did, and any approved row whose rules
    /// could not be read - so a refusal can name the configuration defect instead of saying "none found".
    /// </summary>
    private sealed record PolicySelection(
        string? Id,
        CompanyMatchingPolicyRules? Rules,
        int CandidateCount,
        IReadOnlyList<string> Malformed);

    // ------------------------------------------------------------------ the reads

    private async Task<OgPayablesWorkspace> ReadAsync(
        IDataSource source,
        OgPayableContext? context,
        string? orderId,
        string? invoiceId,
        string? runId,
        CancellationToken cancellationToken)
    {
        var orderRows = await OgUow.ReadAllAsync<PURCHASE_ORDER>(source).ConfigureAwait(false);
        var lineRows = await OgUow.ReadAllAsync<PO_LINE_ITEM>(source).ConfigureAwait(false);
        var receiptRows = await OgUow.ReadAllAsync<PO_RECEIPT>(source).ConfigureAwait(false);
        var invoiceRows = await OgUow.ReadAllAsync<AP_INVOICE>(source).ConfigureAwait(false);
        var invoiceLineRows = await OgUow.ReadAllAsync<AP_INVOICE_LINE>(source).ConfigureAwait(false);
        var runRows = await OgUow.ReadAllAsync<AP_PAYMENT_RUN>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var invoiceByOrder = invoiceRows
            .Where(row => !string.IsNullOrWhiteSpace(row.PURCHASE_ORDER_ID))
            .GroupBy(row => row.PURCHASE_ORDER_ID!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);

        var orderLines = lineRows
            .Where(row => !string.IsNullOrWhiteSpace(row.PURCHASE_ORDER_ID))
            .GroupBy(row => row.PURCHASE_ORDER_ID!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(row => row.LINE_NUMBER).ToList(),
                StringComparer.OrdinalIgnoreCase);

        var orders = orderRows
            .Where(row => !string.IsNullOrWhiteSpace(row.PURCHASE_ORDER_ID))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(row => row.PO_DATE)
            .ThenBy(row => row.PO_NUMBER, StringComparer.Ordinal)
            .Select(row => ToOrder(
                row,
                orderLines.TryGetValue(row.PURCHASE_ORDER_ID, out var lines) ? lines : [],
                receiptRows,
                invoiceLineRows))
            .ToList();

        var invoiceLines = invoiceLineRows
            .Where(row => !string.IsNullOrWhiteSpace(row.AP_INVOICE_ID))
            .GroupBy(row => row.AP_INVOICE_ID!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(row => row.LINE_NUMBER).ToList(),
                StringComparer.OrdinalIgnoreCase);

        var invoices = invoiceRows
            .Where(row => !string.IsNullOrWhiteSpace(row.AP_INVOICE_ID))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(row => row.INVOICE_DATE)
            .ThenBy(row => row.INVOICE_NUMBER, StringComparer.Ordinal)
            .Select(row => ToInvoice(
                row, invoiceLines.TryGetValue(row.AP_INVOICE_ID, out var lines) ? lines : []))
            .ToList();

        var runs = runRows
            .Where(row => !string.IsNullOrWhiteSpace(row.AP_PAYMENT_RUN_ID))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(row => row.RUN_DATE)
            .ThenBy(row => row.RUN_NUMBER, StringComparer.Ordinal)
            .Select(row => new OgPaymentRun(
                row.AP_PAYMENT_RUN_ID,
                row.RUN_NUMBER ?? string.Empty,
                row.RUN_DATE,
                row.TOTAL_AMOUNT,
                row.CURRENCY_CODE,
                row.STATUS,
                row.AUTHOR,
                row.APPROVED_BY,
                row.APPROVED_AT,
                row.JOURNAL_ENTRY_ID,
                row.POSTED_BY,
                row.POSTED_AT,
                row.RETURN_REASON,
                // The invoices a run covers are its payments, so the run's list cannot drift from what it pays.
                []))
            .ToList();

        // The invoice numbers behind each run come from the payments, which is the only record of what it settles.
        var paymentRows = await OgUow.ReadAllAsync<AP_PAYMENT>(source).ConfigureAwait(false);

        var numbersByRun = paymentRows
            .Where(row => !string.IsNullOrWhiteSpace(row.RUN_ID))
            .GroupBy(row => row.RUN_ID!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(payment => invoices.FirstOrDefault(invoice =>
                        string.Equals(invoice.Id, payment.AP_INVOICE_ID, StringComparison.OrdinalIgnoreCase))?.Number)
                    .Where(number => !string.IsNullOrWhiteSpace(number))
                    .Select(number => number!)
                    .ToList(),
                StringComparer.OrdinalIgnoreCase);

        runs = runs
            .Select(run => numbersByRun.TryGetValue(run.Id, out var numbers)
                ? run with { InvoiceNumbers = numbers }
                : run)
            .ToList();

        var selectedOrder = string.IsNullOrWhiteSpace(orderId)
            ? orders.FirstOrDefault()
            : orders.FirstOrDefault(order =>
                  string.Equals(order.Id, orderId.Trim(), StringComparison.OrdinalIgnoreCase))
              ?? orders.FirstOrDefault();

        var selectedInvoice = string.IsNullOrWhiteSpace(invoiceId)
            ? invoices.FirstOrDefault()
            : invoices.FirstOrDefault(invoice =>
                  string.Equals(invoice.Id, invoiceId.Trim(), StringComparison.OrdinalIgnoreCase))
              ?? invoices.FirstOrDefault();

        var selectedRun = string.IsNullOrWhiteSpace(runId)
            ? runs.FirstOrDefault()
            : runs.FirstOrDefault(run => string.Equals(run.Id, runId.Trim(), StringComparison.OrdinalIgnoreCase))
              ?? runs.FirstOrDefault();

        var policies = await OgUow.ReadAllAsync<ACCOUNTING_METHOD>(source).ConfigureAwait(false);

        var policy = policies.FirstOrDefault(row =>
            string.Equals(row.METHOD_TYPE, MatchingMethodType, StringComparison.OrdinalIgnoreCase)
            && string.Equals(row.MATCHING_POLICY_STATUS, ApprovedPolicyStatus, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase));

        var policyText = policy is null
            ? "no approved AP matching policy is recorded, so a purchase-order invoice cannot be matched - record and " +
              "approve one before capturing against an order"
            : DescribePolicy(policy);

        _ = context;

        return new OgPayablesWorkspace(
            orders,
            selectedOrder,
            invoices,
            selectedInvoice,
            runs,
            selectedRun,
            PayableRules.OrderStates,
            PayableRules.MatchStates,
            PayableRules.PaymentMethods,
            PayableRules.Currencies,
            policy?.ACCOUNTING_METHOD_ID,
            policyText,
            false,
            null);
    }

    private string DescribePolicy(ACCOUNTING_METHOD row)
    {
        if (!TryReadPolicy(row, out var rules, out var problem) || rules is null)
        {
            // One corrupt policy row must not take the page down; it is named with its reference instead.
            return "the recorded matching policy cannot be applied: " + (problem ?? row.ACCOUNTING_METHOD_ID);
        }

        return "policy " + row.ACCOUNTING_METHOD_ID + " · " + rules.CurrencyCode +
               " · quantity tolerance " + Number(rules.QuantityTolerancePercent) + " %" +
               " · price tolerance " + Number(rules.PriceTolerancePercent) + " %" +
               " · " + rules.UnitRule + " · " + rules.CurrencyRule;
    }

    /// <summary>
    /// Reads a recorded policy's rules, answering rather than throwing - and reporting the row when it is unreadable, so a
    /// broken configuration is not silent. The person is given the reference, never the exception's text.
    /// </summary>
    private bool TryReadPolicy(
        ACCOUNTING_METHOD row,
        out CompanyMatchingPolicyRules? rules,
        out string? problem)
    {
        rules = null;
        problem = null;

        try
        {
            rules = CompanyMatchingPolicyReview.Rules(row);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"reading matching policy {row.ACCOUNTING_METHOD_ID}",
                "the policy cannot be applied, so its tolerances are not used",
                FailureSeverity.Degraded);

            problem = $"policy {row.ACCOUNTING_METHOD_ID} could not be read (reference {reference})";
            return false;
        }
    }

    private static string Number(decimal? value) =>
        value?.ToString("#,0.####", CultureInfo.InvariantCulture) ?? "n/a";

    private static OgPurchaseOrder ToOrder(
        PURCHASE_ORDER row,
        IReadOnlyList<PO_LINE_ITEM> lines,
        IReadOnlyList<PO_RECEIPT> receipts,
        IReadOnlyList<AP_INVOICE_LINE> invoiceLines)
    {
        var mapped = lines
            .Select(line => new OgOrderLine(
                line.PO_LINE_ITEM_ID,
                line.LINE_NUMBER ?? 0,
                line.ITEM_DESCRIPTION,
                line.QUANTITY ?? 0m,
                line.QUANTITY_UOM,
                line.UNIT_PRICE ?? 0m,
                line.LINE_TOTAL ?? Math.Round((line.QUANTITY ?? 0m) * (line.UNIT_PRICE ?? 0m), 2,
                    MidpointRounding.AwayFromZero),
                // Received is the accepted quantity on the receipts for this line; the rejected quantity is not usable.
                receipts
                    .Where(receipt => string.Equals(receipt.PO_LINE_ITEM_ID, line.PO_LINE_ITEM_ID,
                        StringComparison.OrdinalIgnoreCase))
                    .Sum(receipt => receipt.ACCEPTED_QUANTITY ?? receipt.RECEIVED_QUANTITY ?? 0m),
                invoiceLines
                    .Where(invoice => string.Equals(invoice.PO_LINE_ITEM_ID, line.PO_LINE_ITEM_ID,
                        StringComparison.OrdinalIgnoreCase))
                    .Sum(invoice => invoice.QUANTITY ?? 0m)))
            .ToList();

        return new OgPurchaseOrder(
            row.PURCHASE_ORDER_ID,
            row.PO_NUMBER ?? string.Empty,
            row.VENDOR_BA_ID ?? string.Empty,
            row.PO_DATE ?? DateTime.MinValue,
            row.EXPECTED_DELIVERY_DATE,
            row.CURRENCY_CODE ?? string.Empty,
            row.TOTAL_AMOUNT,
            row.AFE_ID,
            row.STATUS ?? ProcurementCodes.OrderDraft,
            row.AUTHOR ?? row.ROW_CREATED_BY,
            row.APPROVED_BY,
            row.APPROVED_AT,
            row.DESCRIPTION,
            mapped);
    }

    private static OgApInvoice ToInvoice(AP_INVOICE row, IReadOnlyList<AP_INVOICE_LINE> lines)
    {
        var mapped = lines
            .Select(line => new OgInvoiceLine(
                line.AP_INVOICE_LINE_ID,
                line.LINE_NUMBER ?? 0,
                line.ITEM_DESCRIPTION,
                line.QUANTITY ?? 0m,
                line.QUANTITY_UOM,
                line.UNIT_PRICE ?? 0m,
                line.LINE_TOTAL ?? Math.Round((line.QUANTITY ?? 0m) * (line.UNIT_PRICE ?? 0m), 2,
                    MidpointRounding.AwayFromZero),
                line.PO_LINE_ITEM_ID))
            .ToList();

        return new OgApInvoice(
            row.AP_INVOICE_ID,
            row.INVOICE_NUMBER ?? string.Empty,
            row.VENDOR_BA_ID ?? string.Empty,
            row.INVOICE_DATE ?? DateTime.MinValue,
            row.DUE_DATE,
            row.CURRENCY_CODE ?? string.Empty,
            row.SUBTOTAL_AMOUNT ?? mapped.Sum(line => line.LineTotal),
            row.TAX_AMOUNT ?? 0m,
            row.TOTAL_AMOUNT ?? 0m,
            row.PAID_AMOUNT ?? 0m,
            row.BALANCE_DUE ?? 0m,
            row.STATUS ?? InvoiceStatuses.Draft,
            row.MATCH_STATE ?? ProcurementCodes.MatchNotApplicable,
            ReadExceptions(row.MATCH_EXCEPTIONS_TEXT),
            row.MATCH_POLICY_ID,
            row.AUTHOR ?? row.ROW_CREATED_BY,
            row.EXCEPTION_APPROVED_BY,
            row.EXCEPTION_APPROVED_AT,
            row.PURCHASE_ORDER_ID,
            row.AFE_ID,
            mapped);
    }

    /// <summary>
    /// The stored exception text, back into rows the screen can show. The text is the record; the numbers in it are the
    /// ones that decided the match, so the screen does not recompute them.
    /// </summary>
    private static IReadOnlyList<MatchException> ReadExceptions(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        return text
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line =>
            {
                var kind = ProcurementCodes.ExceptionKinds.FirstOrDefault(candidate =>
                    line.StartsWith(ProcurementCodes.DescribeException(candidate), StringComparison.OrdinalIgnoreCase));

                return new MatchException(kind ?? line, null, null, null, null);
            })
            .ToList();
    }

    private static async Task<PURCHASE_ORDER?> ReadOrderRowAsync(
        IDataSource source,
        string orderId,
        CancellationToken cancellationToken)
    {
        var unit = OgUow.For<PURCHASE_ORDER>(source);
        unit.PrimaryKey = "PURCHASE_ORDER_ID";

        var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("PURCHASE_ORDER_ID", orderId)], cancellationToken)
            .ConfigureAwait(false);

        return rows.FirstOrDefault();
    }

    private static async Task<AP_INVOICE?> ReadInvoiceRowAsync(
        IDataSource source,
        string invoiceId,
        CancellationToken cancellationToken)
    {
        var unit = OgUow.For<AP_INVOICE>(source);
        unit.PrimaryKey = "AP_INVOICE_ID";

        var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("AP_INVOICE_ID", invoiceId)], cancellationToken)
            .ConfigureAwait(false);

        return rows.FirstOrDefault();
    }

    private static async Task<AP_PAYMENT_RUN?> ReadRunRowAsync(
        IDataSource source,
        string runId,
        CancellationToken cancellationToken)
    {
        var unit = OgUow.For<AP_PAYMENT_RUN>(source);
        unit.PrimaryKey = "AP_PAYMENT_RUN_ID";

        var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("AP_PAYMENT_RUN_ID", runId)], cancellationToken)
            .ConfigureAwait(false);

        return rows.FirstOrDefault();
    }

    private static async Task<AFE?> ReadAfeAsync(
        IDataSource source,
        string afeId,
        CancellationToken cancellationToken)
    {
        var unit = OgUow.For<AFE>(source);
        unit.PrimaryKey = "AFE_ID";

        var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("AFE_ID", afeId)], cancellationToken)
            .ConfigureAwait(false);

        return rows.FirstOrDefault();
    }

    private static async Task<decimal> ReadAfeCommittedAsync(
        IDataSource source,
        string afeId,
        CancellationToken cancellationToken)
    {
        var commitments = await OgUow.ReadAllAsync<AFE_COMMITMENT>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return commitments
            .Where(row => string.Equals(row.AFE_ID, afeId, StringComparison.OrdinalIgnoreCase))
            .Sum(row => row.AMOUNT);
    }

    // ------------------------------------------------------------------ the writes

    private static async Task<bool> WriteOrderLinesAsync(
        IDataSource source,
        string orderId,
        IReadOnlyList<OgOrderLineInput> inputs,
        string currency,
        string actor,
        CancellationToken cancellationToken)
    {
        var unit = OgUow.For<PO_LINE_ITEM>(source);
        unit.PrimaryKey = "PO_LINE_ITEM_ID";

        var now = DateTime.UtcNow;
        var number = 1;

        foreach (var line in inputs)
        {
            unit.Add(new PO_LINE_ITEM
            {
                PO_LINE_ITEM_ID = Guid.NewGuid().ToString("N"),
                PURCHASE_ORDER_ID = orderId,
                LINE_NUMBER = number,
                ITEM_DESCRIPTION = line.Description ?? "(no description)",
                QUANTITY = line.Quantity!.Value,
                QUANTITY_UOM = line.Unit,
                UNIT_PRICE = line.UnitPrice!.Value,
                LINE_TOTAL = Math.Round(line.Quantity.Value * line.UnitPrice.Value, 2,
                    MidpointRounding.AwayFromZero),
                ACTIVE_IND = "Y",
                PPDM_GUID = Guid.NewGuid().ToString("N"),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            number++;
        }

        _ = currency;

        cancellationToken.ThrowIfCancellationRequested();

        var committed = await unit.Commit().ConfigureAwait(false);

        return OgUow.IsOk(committed);
    }

    private static async Task<bool> WriteInvoiceLinesAsync(
        IDataSource source,
        string invoiceId,
        IReadOnlyList<OgInvoiceLineInput> inputs,
        OgPurchaseOrder? order,
        string currency,
        string actor,
        CancellationToken cancellationToken)
    {
        var unit = OgUow.For<AP_INVOICE_LINE>(source);
        unit.PrimaryKey = "AP_INVOICE_LINE_ID";

        var now = DateTime.UtcNow;
        var number = 1;

        foreach (var line in inputs)
        {
            unit.Add(new AP_INVOICE_LINE
            {
                AP_INVOICE_LINE_ID = Guid.NewGuid().ToString("N"),
                AP_INVOICE_ID = invoiceId,
                LINE_NUMBER = number,
                ITEM_DESCRIPTION = line.Description ?? "(no description)",
                QUANTITY = line.Quantity!.Value,
                QUANTITY_UOM = line.Unit,
                UNIT_PRICE = line.UnitPrice!.Value,
                LINE_TOTAL = Math.Round(line.Quantity.Value * line.UnitPrice.Value, 2,
                    MidpointRounding.AwayFromZero),
                CURRENCY_CODE = currency,
                PURCHASE_ORDER_ID = order?.Id,
                PO_LINE_ITEM_ID = string.IsNullOrWhiteSpace(line.PoLineItemId) ? null : line.PoLineItemId.Trim(),
                ACTIVE_IND = "Y",
                PPDM_GUID = Guid.NewGuid().ToString("N"),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            number++;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var committed = await unit.Commit().ConfigureAwait(false);

        return OgUow.IsOk(committed);
    }

    private static async Task<bool> WriteAfeCommitmentAsync(
        IDataSource source,
        string afeId,
        string reference,
        decimal amount,
        string actor,
        CancellationToken cancellationToken)
    {
        var unit = OgUow.For<AFE_COMMITMENT>(source);
        unit.PrimaryKey = "AFE_COMMITMENT_ID";

        var now = DateTime.UtcNow;

        unit.Add(new AFE_COMMITMENT
        {
            AFE_COMMITMENT_ID = Guid.NewGuid().ToString("N"),
            AFE_ID = afeId,
            REFERENCE = "AP-" + reference,
            AMOUNT = amount,
            COMMITTED_AT = now,
            ACTOR = actor.Trim(),
            ROW_CREATED_BY = actor.Trim(),
            ROW_CREATED_DATE = now,
            ROW_CHANGED_BY = actor.Trim(),
            ROW_CHANGED_DATE = now,
        });

        cancellationToken.ThrowIfCancellationRequested();

        var committed = await unit.Commit().ConfigureAwait(false);

        return OgUow.IsOk(committed);
    }

    private static async Task<string?> UpdateAsync(IDataSource source, PURCHASE_ORDER row)
    {
        var unit = OgUow.For<PURCHASE_ORDER>(source);
        unit.PrimaryKey = "PURCHASE_ORDER_ID";

        var updated = unit.Update(row);

        if (!OgUow.IsOk(updated))
        {
            return OgUow.Message(updated, "The order was not saved.");
        }

        var committed = await unit.Commit().ConfigureAwait(false);

        return OgUow.IsOk(committed) ? null : OgUow.Message(committed, "The order was not saved.");
    }

    private static async Task<string?> UpdateAsync(IDataSource source, AP_INVOICE row)
    {
        var unit = OgUow.For<AP_INVOICE>(source);
        unit.PrimaryKey = "AP_INVOICE_ID";

        var updated = unit.Update(row);

        if (!OgUow.IsOk(updated))
        {
            return OgUow.Message(updated, "The invoice was not saved.");
        }

        var committed = await unit.Commit().ConfigureAwait(false);

        return OgUow.IsOk(committed) ? null : OgUow.Message(committed, "The invoice was not saved.");
    }

    private static async Task<string?> UpdateAsync(IDataSource source, AP_PAYMENT_RUN row)
    {
        var unit = OgUow.For<AP_PAYMENT_RUN>(source);
        unit.PrimaryKey = "AP_PAYMENT_RUN_ID";

        var updated = unit.Update(row);

        if (!OgUow.IsOk(updated))
        {
            return OgUow.Message(updated, "The run was not saved.");
        }

        var committed = await unit.Commit().ConfigureAwait(false);

        return OgUow.IsOk(committed) ? null : OgUow.Message(committed, "The run was not saved.");
    }

    private static JOURNAL_ENTRY_LINE EntryLine(
        string journalId,
        GL_ACCOUNT account,
        decimal debit,
        decimal credit,
        int number,
        string description,
        string actor,
        DateTime now) =>
        new()
        {
            JOURNAL_ENTRY_LINE_ID = Guid.NewGuid().ToString("N"),
            JOURNAL_ENTRY_ID = journalId,
            GL_ACCOUNT_ID = account.GL_ACCOUNT_ID,
            LINE_NUMBER = number,
            DEBIT_AMOUNT = debit,
            CREDIT_AMOUNT = credit,
            DESCRIPTION = description,
            CURRENCY_CODE = account.CURRENCY_CODE,
            ACTIVE_IND = "Y",
            PPDM_GUID = Guid.NewGuid().ToString("N"),
            ROW_CREATED_BY = actor.Trim(),
            ROW_CREATED_DATE = now,
            ROW_CHANGED_BY = actor.Trim(),
            ROW_CHANGED_DATE = now,
        };

    private static GL_ACCOUNT? Find(IReadOnlyList<GL_ACCOUNT> accounts, string number) =>
        accounts.FirstOrDefault(account =>
            string.Equals(account.ACCOUNT_NUMBER?.Trim(), number, StringComparison.OrdinalIgnoreCase));

    /// <summary>The next document number for a year, from a prefix: PO-yyyy-0001, PAY-yyyy-0001.</summary>
    private static string NextNumber(IReadOnlyList<string?> existing, string prefix, DateTime date)
    {
        var full = prefix + date.Year.ToString(CultureInfo.InvariantCulture) + "-";

        var highest = existing
            .Where(number => number is not null && number.StartsWith(full, StringComparison.OrdinalIgnoreCase))
            .Select(number => int.TryParse(number![full.Length..], NumberStyles.Integer,
                CultureInfo.InvariantCulture, out var sequence)
                ? sequence
                : 0)
            .DefaultIfEmpty(0)
            .Max();

        return full + (highest + 1).ToString("0000", CultureInfo.InvariantCulture);
    }

    private static string NextJournalNumber(IReadOnlyList<JOURNAL_ENTRY> existing, DateTime date)
    {
        var prefix = "JE-" + date.Year.ToString(CultureInfo.InvariantCulture) + "-";

        var highest = existing
            .Select(row => row.ENTRY_NUMBER)
            .Where(number => number is not null && number.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Select(number => int.TryParse(number![prefix.Length..], NumberStyles.Integer,
                CultureInfo.InvariantCulture, out var sequence)
                ? sequence
                : 0)
            .DefaultIfEmpty(0)
            .Max();

        return prefix + (highest + 1).ToString("0000", CultureInfo.InvariantCulture);
    }
}
