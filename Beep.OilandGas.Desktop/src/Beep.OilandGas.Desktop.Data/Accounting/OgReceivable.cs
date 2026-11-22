using System.Globalization;
using Beep.OilandGas.Accounting.Calculations;
using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Accounting;

/// <summary>The receipt states (ACC-04).</summary>
public static class ReceivableCodes
{
    /// <summary>Recorded, not yet posted: the allocations are visible but no cash has moved.</summary>
    public const string Draft = "DRAFT";

    /// <summary>Posted: the journal exists and the invoices have been reduced.</summary>
    public const string Posted = "POSTED";

    /// <summary>Reviewed and approved.</summary>
    public const string Approved = "APPROVED";

    /// <summary>Returned by the reviewer with a reason.</summary>
    public const string Returned = "RETURNED";

    /// <summary>Every receipt state.</summary>
    public static readonly string[] States = [Draft, Posted, Approved, Returned];

    /// <summary>The cash account an unallocated receipt posts to - the house default for cash.</summary>
    public const string CashAccount = DefaultGlAccounts.Cash;

    /// <summary>Receivables, the credit side of a receipt.</summary>
    public const string ReceivableAccount = DefaultGlAccounts.AccountsReceivable;

    public static bool IsDraft(string? status) =>
        string.Equals(status?.Trim(), Draft, StringComparison.OrdinalIgnoreCase);

    public static bool IsPosted(string? status) =>
        !string.IsNullOrWhiteSpace(status) && !IsDraft(status);

    public static string Describe(string? status) => status?.Trim().ToUpperInvariant() switch
    {
        Draft => "draft - not posted",
        Posted => "posted",
        Approved => "approved",
        Returned => "returned to the author",
        _ => status ?? "(no status)",
    };
}

/// <summary>One allocation of a receipt to an invoice (ACC-04).</summary>
public sealed record OgAllocationInput(string? InvoiceId, decimal? Amount);

/// <summary>The inputs of a cash receipt (ACC-04).</summary>
public sealed record OgReceiptInputs(
    string? CustomerId,
    DateTime? Date,
    decimal? Amount,
    string? Currency,
    string? Reference,
    string? BankAccount,
    IReadOnlyList<OgAllocationInput> Allocations);

/// <summary>One allocation as stored (ACC-04).</summary>
public sealed record OgAllocation(string PaymentId, string InvoiceId, decimal Amount, string? Status = null)
{
    /// <summary>Whether this allocation has already reduced its invoice - a retry must not apply it again.</summary>
    public bool IsApplied =>
        string.Equals(Status?.Trim(), ReceivableCodes.Posted, StringComparison.OrdinalIgnoreCase);

    public string Describe() =>
        InvoiceId + " · " + MoneyText.Of(Amount) +
        (IsApplied ? " · applied" : " · not yet applied");
}

/// <summary>One cash receipt (ACC-04), matching the plan's <c>OgReceipt</c>.</summary>
public sealed record OgReceipt(
    string Id,
    string Number,
    string CustomerId,
    DateTime Date,
    decimal Amount,
    decimal Unapplied,
    string Currency,
    string? Reference,
    string? BankAccount,
    string State,
    string? Author,
    string? JournalId,
    string? PostedBy,
    DateTime? PostedAt,
    string? ApprovedBy,
    DateTime? ApprovedAt,
    string? ReturnReason,
    IReadOnlyList<OgAllocation> Allocations)
{
    public bool IsDraft => ReceivableCodes.IsDraft(State);

    public bool IsPosted => ReceivableCodes.IsPosted(State);

    public bool IsApproved => string.Equals(State, ReceivableCodes.Approved, StringComparison.OrdinalIgnoreCase);

    public bool HasJournal => !string.IsNullOrWhiteSpace(JournalId);

    public decimal Allocated => Allocations.Sum(allocation => allocation.Amount);

    public string AmountText => MoneyText.Of(Amount) + " " + Currency;

    /// <summary>AC-1: the allocation total and what is left over, said out loud.</summary>
    public string AllocationText =>
        "allocated " + MoneyText.Of(Allocated) + " of " + AmountText + " · " +
        (Unapplied == 0m
            ? "fully allocated"
            : "unapplied " + MoneyText.Of(Unapplied) + " " + Currency + " (kept as customer credit)");

    public string StateText => ReceivableCodes.Describe(State);

    public string JournalText => HasJournal
        ? "journal " + JournalId + " posted on " + (PostedAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
            ?? "an unrecorded date")
        : "not posted - no journal yet";

    public string AllocationsText => Allocations.Count == 0
        ? "no allocation - the whole receipt is unapplied"
        : string.Join(Environment.NewLine, Allocations.Select(allocation => allocation.Describe()));

    public string Describe() =>
        (string.IsNullOrWhiteSpace(Number) ? "(unnumbered)" : Number) + " · " + Date.ToString("yyyy-MM-dd") + " · " +
        CustomerId + " · " + AmountText + " · " + StateText;
}

/// <summary>One open invoice in the receivables list (ACC-04).</summary>
public sealed record OgOpenInvoice(
    string Id,
    string Number,
    string? CustomerId,
    DateTime Date,
    DateTime? DueDate,
    string Currency,
    decimal Total,
    decimal Paid,
    decimal Credited,
    decimal Balance,
    string Bucket)
{
    public string Describe() =>
        (string.IsNullOrWhiteSpace(Number) ? "(unnumbered)" : Number) + " · due " +
        (DueDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "not stated") + " · " +
        MoneyText.Of(Balance) + " " + Currency + " open · " + Bucket;
}

/// <summary>One customer's ageing (ACC-04 AC-2).</summary>
public sealed record OgAgeingRow(
    string CustomerId,
    decimal Current,
    decimal Days30,
    decimal Days60,
    decimal Days90)
{
    public decimal Total => Current + Days30 + Days60 + Days90;

    public string Describe() =>
        $"{CustomerId} · current {MoneyText.Of(Current)} · 30 {MoneyText.Of(Days30)} · " +
        $"60 {MoneyText.Of(Days60)} · 90+ {MoneyText.Of(Days90)} · total {MoneyText.Of(Total)}";
}

/// <summary>The receivables workspace (ACC-04).</summary>
public sealed record OgReceivableWorkspace(
    IReadOnlyList<OgReceipt> Receipts,
    OgReceipt? Selected,
    IReadOnlyList<OgOpenInvoice> OpenInvoices,
    IReadOnlyList<OgAgeingRow> Ageing,
    ReceivableAgeingTotals AgeingTotals,
    DateTime AsOf,
    IReadOnlyList<string> States,
    IReadOnlyList<string> Currencies,
    bool HistoryFailed,
    string? HistoryReason)
{
    public string ReceiptsText => Receipts.Count == 0
        ? "No receipt recorded yet."
        : string.Join(Environment.NewLine, Receipts.Select(receipt => receipt.Describe()));

    public string HistoryStateText => HistoryFailed
        ? "the receipts could not be read; no cash can be recorded"
        : $"{Receipts.Count.ToString(CultureInfo.InvariantCulture)} receipt(s) · " +
          $"{Receipts.Count(receipt => receipt.IsDraft).ToString(CultureInfo.InvariantCulture)} draft · " +
          $"{Receipts.Count(receipt => receipt.HasJournal).ToString(CultureInfo.InvariantCulture)} posted";

    public string OpenInvoicesText => OpenInvoices.Count == 0
        ? "No invoice is open."
        : string.Join(Environment.NewLine, OpenInvoices.Select(invoice => invoice.Describe()));

    public string AgeingText => Ageing.Count == 0
        ? "nothing outstanding to age"
        : string.Join(Environment.NewLine, Ageing.Select(row => row.Describe())) + Environment.NewLine +
          "total " + MoneyText.Of(AgeingTotals.Total);

    /// <summary>AC-2: the four buckets, with the as-of date they were measured at.</summary>
    public string AgeingSummaryText =>
        "as at " + AsOf.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + " · current " +
        MoneyText.Of(AgeingTotals.Current) + " · 30 " + MoneyText.Of(AgeingTotals.Days30) + " · 60 " +
        MoneyText.Of(AgeingTotals.Days60) + " · 90+ " + MoneyText.Of(AgeingTotals.Days90) + " · total " +
        MoneyText.Of(AgeingTotals.Total);

    public decimal Unapplied => Receipts.Sum(receipt => receipt.Unapplied);

    public string UnappliedText => "unapplied cash held " + MoneyText.Of(Unapplied);

    public string ReferenceText =>
        $"states {string.Join(", ", States)} · ageing buckets from due dates: current, 30, 60, 90+ · " +
        "a receipt posts one journal and its allocations reduce the invoices";
}

/// <summary>The receivables rules, stated once so the page, the service and the tests agree.</summary>
public static class ReceivableRules
{
    public const string CodeNoConnection = "receivable-no-connection";
    public const string CodeReadFailed = "receivable-read-failed";
    public const string CodeWriteFailed = "receivable-write-failed";
    public const string CodeActorRequired = "receivable-actor-required";
    public const string CodeInputRequired = "receivable-input-required";
    public const string CodeInputInvalid = "receivable-input-invalid";
    public const string CodeUnknownInvoice = "receivable-unknown-invoice";
    public const string CodeUnknownReceipt = "receivable-unknown-receipt";
    public const string CodeCurrencyMismatch = "receivable-currency-mismatch";
    public const string CodeOverAllocation = "receivable-over-allocation";
    public const string CodeExceedsReceipt = "receivable-exceeds-receipt";
    public const string CodeAlreadyPosted = "receivable-already-posted";
    public const string CodeAuthorCannotApprove = "receivable-author-cannot-approve";
    public const string CodeReturnReasonRequired = "receivable-return-reason-required";
    public const string CodePeriodClosed = "receivable-period-closed";
    public const string CodeUnknownAccount = "receivable-unknown-account";

    public static IReadOnlyList<string> Currencies { get; } = ["USD", "EUR", "GBP", "SAR", "AED"];

    public static IReadOnlyList<string> States => ReceivableCodes.States;

    public static IReadOnlyList<string> Missing(OgReceiptInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(inputs.CustomerId))
        {
            missing.Add("customer");
        }

        if (inputs.Date is null)
        {
            missing.Add("receipt date");
        }

        if (inputs.Amount is null)
        {
            missing.Add("amount");
        }

        if (string.IsNullOrWhiteSpace(inputs.Currency))
        {
            missing.Add("currency");
        }

        return missing;
    }

    public static (string Code, string Message)? Validate(OgReceiptInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var missing = Missing(inputs);

        if (missing.Count > 0)
        {
            return (CodeInputRequired, $"The receipt needs {string.Join(", ", missing)}.");
        }

        if (inputs.Amount <= 0m)
        {
            return (CodeInputInvalid, "A receipt needs an amount above zero.");
        }

        if (!Currencies.Contains(inputs.Currency!.Trim().ToUpperInvariant(), StringComparer.Ordinal))
        {
            return (CodeInputInvalid,
                $"'{inputs.Currency}' is not a currency this app receives in; use {string.Join(", ", Currencies)}.");
        }

        if (inputs.Allocations.Any(allocation => allocation.Amount is null || allocation.Amount <= 0m))
        {
            return (CodeInputInvalid, "Every allocation needs an amount above zero.");
        }

        // AC-1 and AC-4: the allocations may not exceed the cash, and each is checked against its invoice later.
        var allocated = inputs.Allocations.Sum(allocation => allocation.Amount!.Value);

        return allocated > inputs.Amount
            ? (CodeExceedsReceipt,
                $"The allocations total {MoneyText.Of(allocated)} but the receipt is {MoneyText.Of(inputs.Amount!.Value)}; " +
                "cash cannot be applied twice.")
            : null;
    }
}

/// <summary>ACC-04: receipts, allocations, ageing and the cash posting.</summary>
public interface IOgReceivable
{
    /// <summary>Records a receipt with its allocations; the unapplied remainder is kept (AC-1).</summary>
    Task<OgResult<OgReceivableWorkspace>> RecordAsync(
        OgReceiptInputs inputs,
        OgRevenueContext context,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Posts the cash: one journal, and every allocation reduces its invoice (AC-3, AC-5).</summary>
    Task<OgResult<OgReceivableWorkspace>> PostAsync(
        string receiptId,
        OgRevenueContext context,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Approves or returns a posted receipt; the author may not approve (AC-6).</summary>
    Task<OgResult<OgReceivableWorkspace>> ReviewAsync(
        string receiptId,
        bool approve,
        string reason,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>The receipts, the open invoices and the ageing (AC-2).</summary>
    Task<OgResult<OgReceivableWorkspace>> LoadAsync(
        OgRevenueContext context,
        string? receiptId = null,
        DateTime? asOf = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// ACC-04 over ProductionAccounting's <c>AR_RECEIPT</c>, its <c>AR_PAYMENT</c> rows used as the allocations, the
/// <c>AR_INVOICE</c> balances and the ledger's journal, through units of work.
/// </summary>
/// <remarks>
/// A receipt is a header with allocations beneath it, so a bank payment covering several invoices is one fact and the
/// unapplied remainder has an owner rather than being a convention (AC-1). Posting writes exactly one journal whose
/// reference is derived from the receipt id, so a retry after a lost acknowledgement adopts it instead of writing a second
/// (AC-5, the pattern ACC-03 established), and only then are the invoices reduced - every balance recomputed as total less
/// paid less credited, so a partial payment leaves exactly the remainder (AC-3).
/// </remarks>
public sealed class OgReceivableService : IOgReceivable
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgReceivableService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgReceivableWorkspace>> RecordAsync(
        OgReceiptInputs inputs,
        OgRevenueContext context,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgReceivableWorkspace>.Failed(
                "A receipt must record who took it.", ReceivableRules.CodeActorRequired);
        }

        if (ReceivableRules.Validate(inputs) is { } refusal)
        {
            return OgResult<OgReceivableWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgReceivableWorkspace>.Failed(
                "No connection is open, so the receipt cannot be recorded.", ReceivableRules.CodeNoConnection);
        }

        try
        {
            var invoices = await OgUow.ReadAllAsync<AR_INVOICE>(source).ConfigureAwait(false);
            var allocated = 0m;

            foreach (var allocation in inputs.Allocations)
            {
                var invoice = invoices.FirstOrDefault(row =>
                    string.Equals(row.AR_INVOICE_ID, allocation.InvoiceId?.Trim(), StringComparison.OrdinalIgnoreCase));

                if (invoice is null)
                {
                    return OgResult<OgReceivableWorkspace>.Failed(
                        $"Invoice '{allocation.InvoiceId}' is not in this database.",
                        ReceivableRules.CodeUnknownInvoice);
                }

                // The state table's "currency mismatch - refused".
                if (!string.Equals(invoice.CURRENCY_CODE, inputs.Currency!.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return OgResult<OgReceivableWorkspace>.Failed(
                        $"Invoice {invoice.INVOICE_NUMBER} is in {invoice.CURRENCY_CODE} and the receipt is in " +
                        $"{inputs.Currency}; a receipt settles invoices in its own currency.",
                        ReceivableRules.CodeCurrencyMismatch);
                }

                var balance = (invoice.TOTAL_AMOUNT ?? 0m) - (invoice.PAID_AMOUNT ?? 0m) -
                              (invoice.CREDITED_AMOUNT ?? 0m);

                // AC-4: the excess is named, not just the fact.
                if (allocation.Amount!.Value > balance)
                {
                    return OgResult<OgReceivableWorkspace>.Failed(
                        $"The allocation of {MoneyText.Of(allocation.Amount.Value)} " +
                        $"{invoice.CURRENCY_CODE} is more than the {MoneyText.Of(balance)} open on " +
                        $"{invoice.INVOICE_NUMBER} - an excess of " +
                        $"{MoneyText.Of(allocation.Amount.Value - balance)}.",
                        ReceivableRules.CodeOverAllocation);
                }

                allocated += allocation.Amount.Value;
            }

            var now = DateTime.UtcNow;
            var receiptId = Guid.NewGuid().ToString("N");

            var header = OgUow.For<AR_RECEIPT>(source);
            header.PrimaryKey = "AR_RECEIPT_ID";

            header.Add(new AR_RECEIPT
            {
                AR_RECEIPT_ID = receiptId,
                CUSTOMER_BA_ID = inputs.CustomerId!.Trim(),
                RECEIPT_DATE = inputs.Date!.Value.Date,
                AMOUNT = inputs.Amount!.Value,
                // AC-1: what is not allocated is kept on the receipt, explicitly.
                UNAPPLIED_AMOUNT = inputs.Amount.Value - allocated,
                CURRENCY_CODE = inputs.Currency!.Trim().ToUpperInvariant(),
                REFERENCE = string.IsNullOrWhiteSpace(inputs.Reference) ? null : inputs.Reference.Trim(),
                BANK_ACCOUNT = string.IsNullOrWhiteSpace(inputs.BankAccount) ? null : inputs.BankAccount.Trim(),
                STATUS = ReceivableCodes.Draft,
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

            var committed = await header.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgReceivableWorkspace>.Failed(
                    OgUow.Message(committed, "The receipt was not recorded."), ReceivableRules.CodeWriteFailed);
            }

            if (inputs.Allocations.Count > 0)
            {
                var payments = OgUow.For<AR_PAYMENT>(source);
                payments.PrimaryKey = "AR_PAYMENT_ID";

                var number = 1;

                foreach (var allocation in inputs.Allocations)
                {
                    payments.Add(new AR_PAYMENT
                    {
                        AR_PAYMENT_ID = Guid.NewGuid().ToString("N"),
                        AR_INVOICE_ID = allocation.InvoiceId!.Trim(),
                        RECEIPT_ID = receiptId,
                        PAYMENT_NUMBER = "RCPT-" + receiptId[..8] + "-" + number.ToString(CultureInfo.InvariantCulture),
                        PAYMENT_DATE = inputs.Date.Value.Date,
                        PAYMENT_AMOUNT = allocation.Amount!.Value,
                        CURRENCY_CODE = inputs.Currency.Trim().ToUpperInvariant(),
                        PAYER_BA_ID = inputs.CustomerId.Trim(),
                        EXTERNAL_REFERENCE = string.IsNullOrWhiteSpace(inputs.Reference) ? null : inputs.Reference.Trim(),
                        OWNER_TENANT_ID = context.CompanyId,
                        OWNER_COMPANY_BA_ID = context.CompanyId,
                        BOOK_ID = context.BookCode,
                        STATUS = ReceivableCodes.Draft,
                        ACTIVE_IND = "Y",
                        PPDM_GUID = Guid.NewGuid().ToString("N"),
                        ROW_CREATED_BY = actor.Trim(),
                        ROW_CREATED_DATE = now,
                        ROW_CHANGED_BY = actor.Trim(),
                        ROW_CHANGED_DATE = now,
                    });

                    number++;
                }

                var linesCommitted = await payments.Commit().ConfigureAwait(false);

                if (!OgUow.IsOk(linesCommitted))
                {
                    return OgResult<OgReceivableWorkspace>.Failed(
                        OgUow.Message(linesCommitted, "The receipt's allocations were not saved."),
                        ReceivableRules.CodeWriteFailed);
                }
            }

            var workspace = await ReadAsync(source, context, receiptId, DateTime.Today, cancellationToken)
                .ConfigureAwait(false);

            return OgResult<OgReceivableWorkspace>.Ok(workspace);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "recording a cash receipt",
                "the receipt was not recorded",
                FailureSeverity.Error);

            return OgResult<OgReceivableWorkspace>.Failed(
                "The receipt could not be recorded.", reference ?? ReceivableRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgReceivableWorkspace>> PostAsync(
        string receiptId,
        OgRevenueContext context,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgReceivableWorkspace>.Failed(
                "A posting must record who made it.", ReceivableRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgReceivableWorkspace>.Failed(
                "No connection is open, so the receipt cannot be posted.", ReceivableRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, context, receiptId, DateTime.Today, cancellationToken)
                .ConfigureAwait(false);
            var receipt = workspace.Selected;

            if (receipt is null)
            {
                return OgResult<OgReceivableWorkspace>.Failed(
                    $"Receipt '{receiptId}' is not in this database.", ReceivableRules.CodeUnknownReceipt);
            }

            if (receipt.IsPosted && receipt.HasJournal)
            {
                return OgResult<OgReceivableWorkspace>.Ok(workspace);
            }

            if (!receipt.IsDraft)
            {
                return OgResult<OgReceivableWorkspace>.Failed(
                    $"{receipt.Describe()} is already {receipt.StateText}.",
                    ReceivableRules.CodeAlreadyPosted);
            }

            // The period lock applies to the receipt's date.
            var periodCode = LedgerCodes.PeriodCodeOf(receipt.Date);
            var periods = await OgUow.ReadAllAsync<COMPANY_LEDGER_PERIOD>(source).ConfigureAwait(false);
            var period = periods.FirstOrDefault(row =>
                string.Equals(row.PERIOD_CODE, periodCode, StringComparison.OrdinalIgnoreCase));

            if (period is not null && !LedgerCodes.IsOpen(period.STATUS))
            {
                return OgResult<OgReceivableWorkspace>.Failed(
                    $"Period {periodCode} is {period.STATUS.ToLowerInvariant()}, so receipt {receipt.Describe()} cannot " +
                    "be posted into it. Reopen the period first.",
                    ReceivableRules.CodePeriodClosed);
            }

            var accounts = await OgUow.ReadAllAsync<GL_ACCOUNT>(source).ConfigureAwait(false);
            var cash = Find(accounts, ReceivableCodes.CashAccount);
            var receivable = Find(accounts, ReceivableCodes.ReceivableAccount);

            if (cash is null || receivable is null)
            {
                return OgResult<OgReceivableWorkspace>.Failed(
                    $"The chart of accounts has no account " +
                    $"{(cash is null ? ReceivableCodes.CashAccount : ReceivableCodes.ReceivableAccount)}, so the " +
                    "receipt cannot be posted. Add the account before posting.",
                    ReceivableRules.CodeUnknownAccount);
            }

            // The journal's reference is derived from the receipt id, which exists before any numbering, so a retry can find
            // what it already wrote.
            var reference = "RECEIPT-" + receipt.Id;
            var journals = await OgUow.ReadAllAsync<JOURNAL_ENTRY>(source).ConfigureAwait(false);

            var existing = journals.FirstOrDefault(entry =>
                string.Equals(entry.SOURCE_MODULE, "AR_RECEIPT", StringComparison.OrdinalIgnoreCase)
                && string.Equals(entry.REFERENCE_NUMBER, reference, StringComparison.OrdinalIgnoreCase));

            var now = DateTime.UtcNow;
            string journalId;

            if (existing is not null)
            {
                journalId = existing.JOURNAL_ENTRY_ID!;
            }
            else
            {
                journalId = Guid.NewGuid().ToString("N");

                var journal = OgUow.For<JOURNAL_ENTRY>(source);
                journal.PrimaryKey = "JOURNAL_ENTRY_ID";

                journal.Add(new JOURNAL_ENTRY
                {
                    JOURNAL_ENTRY_ID = journalId,
                    ENTRY_NUMBER = NextJournalNumber(journals, receipt.Date),
                    ENTRY_DATE = receipt.Date,
                    ENTRY_TYPE = "RECEIPT",
                    STATUS = LedgerCodes.JournalPosted,
                    DESCRIPTION = "Cash receipt from " + receipt.CustomerId,
                    REFERENCE_NUMBER = reference,
                    SOURCE_MODULE = "AR_RECEIPT",
                    // Cash in, receivable down - one debit and one credit for the whole receipt, including the unapplied part,
                    // which stays a customer credit on the receivable account until it is applied.
                    TOTAL_DEBIT = receipt.Amount,
                    TOTAL_CREDIT = receipt.Amount,
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
                    return OgResult<OgReceivableWorkspace>.Failed(
                        OgUow.Message(journalCommitted, "The receipt journal was not written; nothing was posted."),
                        ReceivableRules.CodeWriteFailed);
                }

                var entryLines = OgUow.For<JOURNAL_ENTRY_LINE>(source);
                entryLines.PrimaryKey = "JOURNAL_ENTRY_LINE_ID";

                entryLines.Add(EntryLine(journalId, cash, receipt.Amount, 0m, 1, actor, now));
                entryLines.Add(EntryLine(journalId, receivable, 0m, receipt.Amount, 2, actor, now));

                var linesCommitted = await entryLines.Commit().ConfigureAwait(false);

                if (!OgUow.IsOk(linesCommitted))
                {
                    return OgResult<OgReceivableWorkspace>.Failed(
                        "The journal's lines were not saved.", ReceivableRules.CodeWriteFailed);
                }
            }

            // Applying the cash: each allocation reduces its invoice, and the balance is recomputed from its own columns.
            foreach (var allocation in receipt.Allocations)
            {
                // Idempotent: an allocation that is already POSTED has already reduced its invoice, so a retry after a
                // lost acknowledgement adopts the journal and the applied amounts instead of applying them twice.
                if (allocation.IsApplied)
                {
                    continue;
                }

                // Read through the unit that updates, because Update() locates the row through the unit's own buffer.
                var invoiceUnit = OgUow.For<AR_INVOICE>(source);
                invoiceUnit.PrimaryKey = "AR_INVOICE_ID";

                var rows = await OgUow.ReadAsync(
                    invoiceUnit,
                    [OgUow.Equal("AR_INVOICE_ID", allocation.InvoiceId)],
                    cancellationToken).ConfigureAwait(false);

                var invoice = rows.FirstOrDefault();

                if (invoice is null)
                {
                    continue;
                }

                var paid = (invoice.PAID_AMOUNT ?? 0m) + allocation.Amount;
                var credited = invoice.CREDITED_AMOUNT ?? 0m;

                if (paid + credited > (invoice.TOTAL_AMOUNT ?? 0m))
                {
                    return OgResult<OgReceivableWorkspace>.Failed(
                        $"Applying {MoneyText.Of(allocation.Amount)} to {invoice.INVOICE_NUMBER} would take it past its " +
                        "total; nothing was posted.",
                        ReceivableRules.CodeOverAllocation);
                }

                invoice.PAID_AMOUNT = paid;
                // AC-3: total less paid less credited, exactly.
                invoice.BALANCE_DUE = (invoice.TOTAL_AMOUNT ?? 0m) - paid - credited;
                invoice.ROW_CHANGED_BY = actor.Trim();
                invoice.ROW_CHANGED_DATE = now;

                var updated = invoiceUnit.Update(invoice);

                if (!OgUow.IsOk(updated))
                {
                    return OgResult<OgReceivableWorkspace>.Failed(
                        OgUow.Message(updated, "The invoice balance was not updated."),
                        ReceivableRules.CodeWriteFailed);
                }

                var invoicesCommitted = await invoiceUnit.Commit().ConfigureAwait(false);

                if (!OgUow.IsOk(invoicesCommitted))
                {
                    return OgResult<OgReceivableWorkspace>.Failed(
                        OgUow.Message(invoicesCommitted, "The invoice balances were not updated."),
                        ReceivableRules.CodeWriteFailed);
                }
            }

            // The allocations become posted, and the receipt is numbered.
            var paymentUnit = OgUow.For<AR_PAYMENT>(source);
            paymentUnit.PrimaryKey = "AR_PAYMENT_ID";

            var payments = await OgUow.ReadAsync(
                paymentUnit,
                [OgUow.Equal("RECEIPT_ID", receipt.Id)],
                cancellationToken).ConfigureAwait(false);

            foreach (var payment in payments)
            {
                payment.STATUS = ReceivableCodes.Posted;
                payment.ROW_CHANGED_BY = actor.Trim();
                payment.ROW_CHANGED_DATE = now;

                var updated = paymentUnit.Update(payment);

                if (!OgUow.IsOk(updated))
                {
                    return OgResult<OgReceivableWorkspace>.Failed(
                        OgUow.Message(updated, "The allocations were not marked posted."),
                        ReceivableRules.CodeWriteFailed);
                }
            }

            await paymentUnit.Commit().ConfigureAwait(false);

            var headerUnit = OgUow.For<AR_RECEIPT>(source);
            headerUnit.PrimaryKey = "AR_RECEIPT_ID";

            var headers = await OgUow.ReadAsync(
                headerUnit,
                [OgUow.Equal("AR_RECEIPT_ID", receipt.Id)],
                cancellationToken).ConfigureAwait(false);

            var header = headers.FirstOrDefault();

            if (header is null)
            {
                return OgResult<OgReceivableWorkspace>.Failed(
                    $"Receipt '{receiptId}' is not in this database.", ReceivableRules.CodeUnknownReceipt);
            }

            header.RECEIPT_NUMBER = string.IsNullOrWhiteSpace(header.RECEIPT_NUMBER)
                ? NextReceiptNumber(await OgUow.ReadAllAsync<AR_RECEIPT>(source).ConfigureAwait(false), receipt.Date)
                : header.RECEIPT_NUMBER;
            header.STATUS = ReceivableCodes.Posted;
            header.JOURNAL_ENTRY_ID = journalId;
            header.POSTED_BY = actor.Trim();
            header.POSTED_AT = now;
            header.ROW_CHANGED_BY = actor.Trim();
            header.ROW_CHANGED_DATE = now;

            var written = headerUnit.Update(header);

            if (!OgUow.IsOk(written))
            {
                return OgResult<OgReceivableWorkspace>.Failed(
                    OgUow.Message(written, "The receipt was not marked posted."), ReceivableRules.CodeWriteFailed);
            }

            var committed = await headerUnit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgReceivableWorkspace>.Failed(
                    OgUow.Message(committed, "The receipt was not marked posted."), ReceivableRules.CodeWriteFailed);
            }

            var after = await ReadAsync(source, context, receipt.Id, DateTime.Today, cancellationToken)
                .ConfigureAwait(false);

            return OgResult<OgReceivableWorkspace>.Ok(after);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"posting cash receipt {receiptId}",
                "the receipt was not posted",
                FailureSeverity.Error);

            return OgResult<OgReceivableWorkspace>.Failed(
                "The receipt could not be posted.", reference ?? ReceivableRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgReceivableWorkspace>> ReviewAsync(
        string receiptId,
        bool approve,
        string reason,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgReceivableWorkspace>.Failed(
                "A review must record who made it.", ReceivableRules.CodeActorRequired);
        }

        if (!approve && string.IsNullOrWhiteSpace(reason))
        {
            return OgResult<OgReceivableWorkspace>.Failed(
                "Returning a receipt needs its reason.", ReceivableRules.CodeReturnReasonRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgReceivableWorkspace>.Failed(
                "No connection is open, so the review cannot be recorded.", ReceivableRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, null, receiptId, DateTime.Today, cancellationToken)
                .ConfigureAwait(false);
            var receipt = workspace.Selected;

            if (receipt is null)
            {
                return OgResult<OgReceivableWorkspace>.Failed(
                    $"Receipt '{receiptId}' is not in this database.", ReceivableRules.CodeUnknownReceipt);
            }

            if (!receipt.IsPosted)
            {
                return OgResult<OgReceivableWorkspace>.Failed(
                    $"{receipt.Describe()} is {receipt.StateText}; only a posted receipt is reviewed.",
                    ReceivableRules.CodeInputInvalid);
            }

            // AC-6: the author cannot approve their own receipt.
            if (approve && !string.IsNullOrWhiteSpace(receipt.Author)
                && string.Equals(receipt.Author.Trim(), actor.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgReceivableWorkspace>.Failed(
                    $"{receipt.Describe()} was recorded by {receipt.Author}, who therefore cannot approve it. A " +
                    "different controller must review it.",
                    ReceivableRules.CodeAuthorCannotApprove);
            }

            var unit = OgUow.For<AR_RECEIPT>(source);
            unit.PrimaryKey = "AR_RECEIPT_ID";

            var headers = await OgUow.ReadAsync(
                unit,
                [OgUow.Equal("AR_RECEIPT_ID", receipt.Id)],
                cancellationToken).ConfigureAwait(false);

            var header = headers.FirstOrDefault();

            if (header is null)
            {
                return OgResult<OgReceivableWorkspace>.Failed(
                    $"Receipt '{receiptId}' is not in this database.", ReceivableRules.CodeUnknownReceipt);
            }

            header.STATUS = approve ? ReceivableCodes.Approved : ReceivableCodes.Returned;
            header.RETURN_REASON = approve ? null : reason.Trim();
            header.APPROVED_BY = approve ? actor.Trim() : null;
            header.APPROVED_AT = approve ? DateTime.UtcNow : null;
            header.ROW_CHANGED_BY = actor.Trim();
            header.ROW_CHANGED_DATE = DateTime.UtcNow;

            var written = unit.Update(header);

            if (!OgUow.IsOk(written))
            {
                return OgResult<OgReceivableWorkspace>.Failed(
                    OgUow.Message(written, "The review was not recorded."), ReceivableRules.CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgReceivableWorkspace>.Failed(
                    OgUow.Message(committed, "The review was not recorded."), ReceivableRules.CodeWriteFailed);
            }

            var after = await ReadAsync(source, null, receipt.Id, DateTime.Today, cancellationToken)
                .ConfigureAwait(false);

            return OgResult<OgReceivableWorkspace>.Ok(after);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"reviewing cash receipt {receiptId}",
                "the review was not recorded",
                FailureSeverity.Error);

            return OgResult<OgReceivableWorkspace>.Failed(
                "The review could not be recorded.", reference ?? ReceivableRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgReceivableWorkspace>> LoadAsync(
        OgRevenueContext context,
        string? receiptId = null,
        DateTime? asOf = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var source = _source();

        if (source is null)
        {
            return OgResult<OgReceivableWorkspace>.Failed(
                "No connection is open, so the receipts cannot be read.", ReceivableRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, context, receiptId, asOf ?? DateTime.Today, cancellationToken)
                .ConfigureAwait(false);

            return OgResult<OgReceivableWorkspace>.Ok(workspace);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the cash receipts",
                "the receipts are unavailable; no cash can be recorded",
                FailureSeverity.Degraded);

            return OgResult<OgReceivableWorkspace>.Ok(new OgReceivableWorkspace(
                [], null, [], [], new ReceivableAgeingTotals(0m, 0m, 0m, 0m), asOf ?? DateTime.Today,
                ReceivableRules.States, ReceivableRules.Currencies, true,
                $"the receipts could not be read (reference {reference})"));
        }
    }

    // ------------------------------------------------------------------ the reads

    private static async Task<OgReceivableWorkspace> ReadAsync(
        IDataSource source,
        OgRevenueContext? context,
        string? receiptId,
        DateTime asOf,
        CancellationToken cancellationToken)
    {
        var headers = await OgUow.ReadAllAsync<AR_RECEIPT>(source).ConfigureAwait(false);
        var payments = await OgUow.ReadAllAsync<AR_PAYMENT>(source).ConfigureAwait(false);
        var invoices = await OgUow.ReadAllAsync<AR_INVOICE>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var allocationsByReceipt = payments
            .Where(payment => !string.IsNullOrWhiteSpace(payment.RECEIPT_ID))
            .GroupBy(payment => payment.RECEIPT_ID!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(payment => new OgAllocation(
                        payment.AR_PAYMENT_ID,
                        payment.AR_INVOICE_ID ?? string.Empty,
                        payment.PAYMENT_AMOUNT ?? 0m,
                        payment.STATUS))
                    .ToList(),
                StringComparer.OrdinalIgnoreCase);

        var receipts = headers
            .Where(row => !string.IsNullOrWhiteSpace(row.AR_RECEIPT_ID))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(row => row.RECEIPT_DATE)
            .ThenBy(row => row.RECEIPT_NUMBER, StringComparer.Ordinal)
            .Select(row => new OgReceipt(
                row.AR_RECEIPT_ID,
                row.RECEIPT_NUMBER ?? string.Empty,
                row.CUSTOMER_BA_ID,
                row.RECEIPT_DATE,
                row.AMOUNT,
                row.UNAPPLIED_AMOUNT,
                row.CURRENCY_CODE,
                row.REFERENCE,
                row.BANK_ACCOUNT,
                row.STATUS,
                row.AUTHOR ?? row.ROW_CREATED_BY,
                row.JOURNAL_ENTRY_ID,
                row.POSTED_BY,
                row.POSTED_AT,
                row.APPROVED_BY,
                row.APPROVED_AT,
                row.RETURN_REASON,
                allocationsByReceipt.TryGetValue(row.AR_RECEIPT_ID, out var allocated) ? allocated : []))
            .ToList();

        var open = invoices
            .Where(row => !string.IsNullOrWhiteSpace(row.AR_INVOICE_ID))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Select(row => new
            {
                Row = row,
                Balance = (row.TOTAL_AMOUNT ?? 0m) - (row.PAID_AMOUNT ?? 0m) - (row.CREDITED_AMOUNT ?? 0m),
            })
            .Where(item => item.Balance > 0m)
            .Select(item => new OgOpenInvoice(
                item.Row.AR_INVOICE_ID,
                item.Row.INVOICE_NUMBER ?? string.Empty,
                item.Row.CUSTOMER_BA_ID,
                item.Row.INVOICE_DATE ?? DateTime.MinValue,
                item.Row.DUE_DATE,
                item.Row.CURRENCY_CODE ?? string.Empty,
                item.Row.TOTAL_AMOUNT ?? 0m,
                item.Row.PAID_AMOUNT ?? 0m,
                item.Row.CREDITED_AMOUNT ?? 0m,
                item.Balance,
                ReceivableAgeing.BucketOf(item.Row.DUE_DATE, item.Row.INVOICE_DATE ?? DateTime.MinValue, asOf)))
            .OrderBy(invoice => invoice.DueDate ?? invoice.Date)
            .ToList();

        var ageing = open
            .GroupBy(invoice => invoice.CustomerId ?? "(no customer)", StringComparer.OrdinalIgnoreCase)
            .Select(group => new OgAgeingRow(
                group.Key,
                group.Where(invoice => invoice.Bucket == ReceivableAgeing.Current).Sum(invoice => invoice.Balance),
                group.Where(invoice => invoice.Bucket == ReceivableAgeing.Days30).Sum(invoice => invoice.Balance),
                group.Where(invoice => invoice.Bucket == ReceivableAgeing.Days60).Sum(invoice => invoice.Balance),
                group.Where(invoice => invoice.Bucket == ReceivableAgeing.Days90).Sum(invoice => invoice.Balance)))
            .OrderBy(row => row.CustomerId, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var totals = ReceivableAgeing.Of(
            open.Select(invoice => ((DateTime?)invoice.DueDate, invoice.Date, invoice.Balance)),
            asOf);

        var selected = string.IsNullOrWhiteSpace(receiptId)
            ? receipts.FirstOrDefault()
            : receipts.FirstOrDefault(receipt =>
                  string.Equals(receipt.Id, receiptId.Trim(), StringComparison.OrdinalIgnoreCase))
              ?? receipts.FirstOrDefault();

        return new OgReceivableWorkspace(
            receipts,
            selected,
            open,
            ageing,
            totals,
            asOf.Date,
            ReceivableRules.States,
            ReceivableRules.Currencies,
            false,
            null);
    }

    private static JOURNAL_ENTRY_LINE EntryLine(
        string journalId,
        GL_ACCOUNT account,
        decimal debit,
        decimal credit,
        int number,
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
            DESCRIPTION = "cash receipt",
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

    /// <summary>The next receipt number for a year: RCPT-yyyy-0001, server-owned.</summary>
    private static string NextReceiptNumber(IReadOnlyList<AR_RECEIPT> existing, DateTime date)
    {
        var prefix = "RCPT-" + date.Year.ToString(CultureInfo.InvariantCulture) + "-";

        var highest = existing
            .Select(row => row.RECEIPT_NUMBER)
            .Where(number => number is not null && number.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Select(number => int.TryParse(number![prefix.Length..], NumberStyles.Integer,
                CultureInfo.InvariantCulture, out var sequence)
                ? sequence
                : 0)
            .DefaultIfEmpty(0)
            .Max();

        return prefix + (highest + 1).ToString("0000", CultureInfo.InvariantCulture);
    }

    /// <summary>The next cash-receipt journal number for a year: CR-yyyy-0001.</summary>
    private static string NextJournalNumber(IReadOnlyList<JOURNAL_ENTRY> existing, DateTime date)
    {
        var prefix = "CR-" + date.Year.ToString(CultureInfo.InvariantCulture) + "-";

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
