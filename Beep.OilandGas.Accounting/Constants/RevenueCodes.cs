using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Accounting.Constants;

/// <summary>
/// The revenue module's invoice states, credit-note states and the accounts an invoice posts to (ACC-03).
/// </summary>
/// <remarks>
/// The AR tables had no status vocabulary of their own in the accounting project, so it is stated once here, together with
/// the three accounts an issued invoice touches. The account numbers are the house defaults from
/// <see cref="DefaultGlAccounts"/> where one exists (receivables 1110, revenue 4001) and a stated sales-tax liability
/// account otherwise; the service resolves each against the chart of accounts and <b>refuses naming the account</b> when the
/// chart does not carry it, rather than posting to a number that is not there.
/// </remarks>
public static class RevenueCodes
{
    /// <summary>Built and not yet issued.</summary>
    public const string InvoiceDraft = "DRAFT";

    /// <summary>Issued: numbered, journalled and receivable.</summary>
    public const string InvoiceIssued = "ISSUED";

    /// <summary>Reviewed and approved by someone other than the author.</summary>
    public const string InvoiceApproved = "APPROVED";

    /// <summary>Returned by the reviewer with a reason.</summary>
    public const string InvoiceReturned = "RETURNED";

    /// <summary>Every invoice state.</summary>
    public static readonly string[] InvoiceStates =
        [InvoiceDraft, InvoiceIssued, InvoiceApproved, InvoiceReturned];

    /// <summary>A credit note is effective as soon as it is issued - it reduces what the buyer owes.</summary>
    public const string CreditMemoIssued = "ISSUED";

    /// <summary>Receivables - the debit side of an issued invoice.</summary>
    public const string AccountsReceivableAccount = DefaultGlAccounts.AccountsReceivable;

    /// <summary>Revenue - the credit side of the net amount.</summary>
    public const string RevenueAccount = DefaultGlAccounts.Revenue;

    /// <summary>
    /// Sales tax payable. The house defaults carry no sales-tax account, so this is the number the app posts collected tax
    /// to; it must exist in the chart or the issue is refused by name.
    /// </summary>
    public const string SalesTaxAccount = "2300";

    /// <summary>The quantity units an invoice line may state.</summary>
    public static readonly string[] QuantityUnits = ["bbl", "Mscf", "boe"];

    /// <summary>Whether an invoice can still be changed.</summary>
    public static bool IsDraft(string? status) =>
        string.Equals(status?.Trim(), InvoiceDraft, StringComparison.OrdinalIgnoreCase);

    /// <summary>Whether the invoice has been issued (and therefore has a journal).</summary>
    public static bool IsIssued(string? status) =>
        !string.IsNullOrWhiteSpace(status) && !IsDraft(status);

    /// <summary>A state's name for a person.</summary>
    public static string Describe(string? status) => status?.Trim().ToUpperInvariant() switch
    {
        InvoiceDraft => "draft",
        InvoiceIssued => "issued",
        InvoiceApproved => "approved",
        InvoiceReturned => "returned to the author",
        _ => status ?? "(no status)",
    };

    /// <summary>Every invoice state, as a list, for the workspace.</summary>
    public static IReadOnlyList<string> States => InvoiceStates;
}
