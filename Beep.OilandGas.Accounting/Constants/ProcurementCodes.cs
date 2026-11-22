using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Accounting.Constants;

/// <summary>
/// The procure-to-pay states and accounts (ACC-05), and the match vocabulary the desk uses.
/// </summary>
/// <remarks>
/// The owner already had a purchase-order state (<c>PURCHASE_ORDER.STATUS</c> is written as "DRAFT" by
/// <c>PurchaseOrderService</c>), a receipt state ("RECEIVED"), and the accounts-payable invoice statuses in
/// <see cref="InvoiceStatuses"/>. Those are reused rather than respelled. What did not exist is the <b>match</b>
/// vocabulary: nothing in the project names a match outcome or an exception kind, so it is stated here once and the screen
/// holds no literal of its own.
/// </remarks>
public static class ProcurementCodes
{
    // ---- purchase order ------------------------------------------------------------------------------------------

    /// <summary>Raised and not yet approved - the owner's own value.</summary>
    public const string OrderDraft = "DRAFT";

    /// <summary>Approved: goods may be received against it.</summary>
    public const string OrderApproved = "APPROVED";

    /// <summary>Closed: nothing further may be received or invoiced.</summary>
    public const string OrderClosed = "CLOSED";

    /// <summary>Every order state.</summary>
    public static readonly string[] OrderStates = [OrderDraft, OrderApproved, OrderClosed];

    // ---- goods receipt -------------------------------------------------------------------------------------------

    /// <summary>Received in full - the owner's own receipt value.</summary>
    public const string ReceiptReceived = "RECEIVED";

    /// <summary>Received short: fewer units arrived than were booked in on this receipt.</summary>
    public const string ReceiptPartial = "PARTIAL";

    /// <summary>Every receipt state.</summary>
    public static readonly string[] ReceiptStates = [ReceiptReceived, ReceiptPartial];

    // ---- three-way match -----------------------------------------------------------------------------------------

    /// <summary>The order, the receipt and the invoice agree inside the recorded tolerances.</summary>
    public const string MatchMatched = "MATCHED";

    /// <summary>One or more exceptions were found and need an independent approval.</summary>
    public const string MatchException = "EXCEPTION";

    /// <summary>An exception was reviewed and approved by someone other than the author.</summary>
    public const string MatchExceptionApproved = "EXCEPTION_APPROVED";

    /// <summary>A non-PO invoice, which has nothing to match and is reviewed on its own (the owner's separate-review rule).</summary>
    public const string MatchNotApplicable = "NON_PO";

    /// <summary>Every match state.</summary>
    public static readonly string[] MatchStates =
        [MatchMatched, MatchException, MatchExceptionApproved, MatchNotApplicable];

    /// <summary>More was invoiced than was ordered on the line.</summary>
    public const string ExceptionQuantityAboveOrder = "QUANTITY_ABOVE_ORDER";

    /// <summary>More was invoiced than was received.</summary>
    public const string ExceptionQuantityAboveReceived = "QUANTITY_ABOVE_RECEIVED";

    /// <summary>The invoiced unit price moved outside the policy's price tolerance.</summary>
    public const string ExceptionPriceOutsideTolerance = "PRICE_OUTSIDE_TOLERANCE";

    /// <summary>The invoice's currency is not the order's - the owner's exact-recorded-currency rule.</summary>
    public const string ExceptionCurrencyMismatch = "CURRENCY_MISMATCH";

    /// <summary>No approved matching policy covers this currency and date.</summary>
    public const string ExceptionNoPolicy = "NO_MATCHING_POLICY";

    /// <summary>Every exception kind.</summary>
    public static readonly string[] ExceptionKinds =
    [
        ExceptionQuantityAboveOrder,
        ExceptionQuantityAboveReceived,
        ExceptionPriceOutsideTolerance,
        ExceptionCurrencyMismatch,
        ExceptionNoPolicy,
    ];

    /// <summary>An exception kind's name for a person.</summary>
    public static string DescribeException(string? kind) => kind switch
    {
        ExceptionQuantityAboveOrder => "invoiced above the ordered quantity",
        ExceptionQuantityAboveReceived => "invoiced above the received quantity",
        ExceptionPriceOutsideTolerance => "unit price outside the recorded tolerance",
        ExceptionCurrencyMismatch => "invoice currency is not the order's",
        ExceptionNoPolicy => "no approved matching policy covers this invoice",
        _ => kind ?? "(no exception)",
    };

    /// <summary>Whether a match state authorises payment.</summary>
    public static bool IsPayable(string? matchState) =>
        string.Equals(matchState?.Trim(), MatchMatched, StringComparison.OrdinalIgnoreCase)
        || string.Equals(matchState?.Trim(), MatchExceptionApproved, StringComparison.OrdinalIgnoreCase)
        || string.Equals(matchState?.Trim(), MatchNotApplicable, StringComparison.OrdinalIgnoreCase);

    // ---- payment run ---------------------------------------------------------------------------------------------

    /// <summary>Proposed: the invoices are selected and nothing has moved.</summary>
    public const string RunDraft = "DRAFT";

    /// <summary>Approved by someone other than the proposer.</summary>
    public const string RunApproved = "APPROVED";

    /// <summary>Posted: the single journal exists and the invoices are paid.</summary>
    public const string RunPosted = "POSTED";

    /// <summary>Returned by the approver with a reason.</summary>
    public const string RunReturned = "RETURNED";

    /// <summary>Every run state.</summary>
    public static readonly string[] RunStates = [RunDraft, RunApproved, RunPosted, RunReturned];

    // ---- accounts ------------------------------------------------------------------------------------------------

    /// <summary>Accounts payable - the debit side of a payment run.</summary>
    public const string PayableAccount = DefaultGlAccounts.AccountsPayable;

    /// <summary>Cash - the credit side of a payment run.</summary>
    public const string CashAccount = DefaultGlAccounts.Cash;

    /// <summary>Purchase expense - the debit side of a non-PO invoice that has no inventory to capitalise.</summary>
    public const string ExpenseAccount = DefaultGlAccounts.OperatingExpense;

    /// <summary>The quantity units a line may state.</summary>
    public static readonly string[] QuantityUnits = ["bbl", "Mscf", "boe", "ea", "ft", "m"];

    /// <summary>Every payment method the desk records.</summary>
    public static readonly string[] PaymentMethods = ["BANK_TRANSFER", "CHECK", "WIRE", "CASH"];
}
