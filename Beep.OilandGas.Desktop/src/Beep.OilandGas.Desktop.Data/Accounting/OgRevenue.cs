using System.Globalization;
using Beep.OilandGas.Accounting.Calculations;
using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Accounting;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Accounting;

/// <summary>Where a sales invoice belongs (ACC-03): the company and the book its journal posts to.</summary>
public sealed record OgRevenueContext(string CompanyId, string BookCode)
{
    public static OgRevenueContext FromSession(OgSession session) =>
        new(
            string.IsNullOrWhiteSpace(session.CompanyId) ? "APP" : session.CompanyId,
            string.IsNullOrWhiteSpace(session.BookCode) ? "MAIN" : session.BookCode);
}

/// <summary>One line of a sales invoice as the clerk enters it (ACC-03).</summary>
public sealed record OgSalesLineInput(
    string? Commodity,
    string? Description,
    decimal? Quantity,
    string? Unit,
    decimal? UnitPrice,
    string? VolumeReference);

/// <summary>The inputs of a sales invoice (ACC-03).</summary>
public sealed record OgInvoiceInputs(
    string? BuyerId,
    string? ContractId,
    string? ContractCurrency,
    DateTime? Date,
    DateTime? DueDate,
    string? Currency,
    decimal? TaxRate,
    IReadOnlyList<OgSalesLineInput> Lines);

/// <summary>One invoice line (ACC-03), with its money and the volume it came from.</summary>
public sealed record OgSalesLine(
    string Id,
    int LineNumber,
    string? Commodity,
    string? Description,
    decimal Quantity,
    string? Unit,
    decimal UnitPrice,
    decimal Net,
    decimal Tax,
    decimal Total,
    string? VolumeReference)
{
    public string Describe() =>
        $"{LineNumber.ToString(CultureInfo.InvariantCulture)} · {Commodity ?? "(no commodity)"} · " +
        Number(Quantity) + " " + (Unit ?? string.Empty) + " at " + Money(UnitPrice) + " · net " + Money(Net) +
        " · tax " + Money(Tax) + " · volume " + (VolumeReference ?? "(none)");

    private static string Number(decimal value) =>
        value.ToString("#,0.####", CultureInfo.InvariantCulture);

    private static string Money(decimal value) => MoneyText.Of(value);
}

/// <summary>One credit note (ACC-03).</summary>
public sealed record OgCreditNote(
    string Id,
    string InvoiceId,
    string Number,
    DateTime Date,
    decimal Amount,
    string? Reason,
    string Status,
    string? Actor)
{
    public string Describe() =>
        $"{Number} · {Date:yyyy-MM-dd} · {MoneyText.Of(Amount)} · {Status} · " + (Reason ?? "(no reason)");
}

/// <summary>One sales invoice (ACC-03), matching the plan's <c>OgSalesInvoice</c> plus its lines and credits.</summary>
public sealed record OgSalesInvoice(
    string Id,
    string Number,
    string? BuyerId,
    string? ContractId,
    DateTime Date,
    DateTime? DueDate,
    string Currency,
    decimal Subtotal,
    decimal Tax,
    decimal Total,
    decimal Paid,
    decimal Credited,
    decimal BalanceDue,
    string State,
    string? Author,
    string? JournalId,
    string? IssuedBy,
    DateTime? IssuedAt,
    string? ApprovedBy,
    DateTime? ApprovedAt,
    string? ReturnReason,
    IReadOnlyList<OgSalesLine> Lines,
    IReadOnlyList<OgCreditNote> CreditNotes)
{
    public bool IsDraft => RevenueCodes.IsDraft(State);

    public bool IsIssued => RevenueCodes.IsIssued(State);

    public bool IsApproved => string.Equals(State, RevenueCodes.InvoiceApproved, StringComparison.OrdinalIgnoreCase);

    public bool HasJournal => !string.IsNullOrWhiteSpace(JournalId);

    /// <summary>The invoice is only deletable while nothing has been paid or credited and it is still a draft.</summary>
    public bool CanDelete => IsDraft && Paid == 0m && Credited == 0m;

    public string TotalsText => "subtotal " + Format(Subtotal) + " · tax " + Format(Tax) + " · total " + Format(Total);

    public string BalanceText => "paid " + Format(Paid) + " · credited " + Format(Credited) + " · balance " +
                                 Format(BalanceDue) + " " + Currency;

    public string StateText => RevenueCodes.Describe(State);

    public string JournalText => HasJournal
        ? "journal " + JournalId + " posted on issue"
        : "not issued - no journal yet";

    public string LinesText => Lines.Count == 0
        ? "no line"
        : string.Join(Environment.NewLine, Lines.Select(line => line.Describe()));

    public string CreditsText => CreditNotes.Count == 0
        ? "no credit note"
        : string.Join(Environment.NewLine, CreditNotes.Select(note => note.Describe()));

    public string Describe() =>
        (string.IsNullOrWhiteSpace(Number) ? "(unnumbered)" : Number) + " · " + Date.ToString("yyyy-MM-dd") + " · " +
        (BuyerId ?? "(no buyer)") + " · " + Format(Total) + " " + Currency + " · " + StateText;

    private string Format(decimal value) => MoneyText.Of(value) + "";
}

/// <summary>The revenue workspace (ACC-03).</summary>
public sealed record OgRevenueWorkspace(
    IReadOnlyList<OgSalesInvoice> Invoices,
    OgSalesInvoice? Selected,
    IReadOnlyList<string> States,
    IReadOnlyList<string> QuantityUnits,
    IReadOnlyList<string> Currencies,
    string LedgerText,
    bool HistoryFailed,
    string? HistoryReason)
{
    public string InvoicesText => Invoices.Count == 0
        ? "No sales invoice exists yet."
        : string.Join(Environment.NewLine, Invoices.Select(invoice => invoice.Describe()));

    public string HistoryStateText => HistoryFailed
        ? "the invoices could not be read; nothing can be issued"
        : $"{Invoices.Count.ToString(CultureInfo.InvariantCulture)} invoice(s) · " +
          $"{Invoices.Count(invoice => invoice.IsDraft).ToString(CultureInfo.InvariantCulture)} draft · " +
          $"{Invoices.Count(invoice => invoice.HasJournal).ToString(CultureInfo.InvariantCulture)} issued";

    public decimal Outstanding => Invoices.Where(invoice => invoice.IsIssued).Sum(invoice => invoice.BalanceDue);

    public string OutstandingText =>
        "outstanding " + MoneyText.Of(Outstanding) + " across issued invoices";

    public string ReferenceText =>
        $"states {string.Join(", ", States)} · tax computed by SalesInvoiceTax in the accounting project · " +
        "Invoice numbers are server-owned";
}

/// <summary>The revenue rules, stated once so the page, the service and the tests agree.</summary>
public static class RevenueRules
{
    public const string CodeNoConnection = "revenue-no-connection";
    public const string CodeReadFailed = "revenue-read-failed";
    public const string CodeWriteFailed = "revenue-write-failed";
    public const string CodeActorRequired = "revenue-actor-required";
    public const string CodeInputRequired = "revenue-input-required";
    public const string CodeInputInvalid = "revenue-input-invalid";
    public const string CodeCurrencyMismatch = "revenue-currency-mismatch";
    public const string CodeVolumeWithoutPrice = "revenue-volume-without-price";
    public const string CodeUnknownInvoice = "revenue-unknown-invoice";
    public const string CodeUnknownAccount = "revenue-unknown-account";
    public const string CodePeriodClosed = "revenue-period-closed";
    public const string CodeAlreadyIssued = "revenue-already-issued";
    public const string CodeAuthorCannotApprove = "revenue-author-cannot-approve";
    public const string CodeReturnReasonRequired = "revenue-return-reason-required";
    public const string CodeCreditTooLarge = "revenue-credit-too-large";
    public const string CodeCreditNeedsIssued = "revenue-credit-needs-issued";
    public const string CodeNotDeletable = "revenue-not-deletable";

    public static IReadOnlyList<string> Currencies { get; } = ["USD", "EUR", "GBP", "SAR", "AED"];

    public static IReadOnlyList<string> QuantityUnits => RevenueCodes.QuantityUnits;

    public static IReadOnlyList<string> States => RevenueCodes.States;

    public static IReadOnlyList<string> Missing(OgInvoiceInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(inputs.BuyerId))
        {
            missing.Add("buyer");
        }

        if (inputs.Date is null)
        {
            missing.Add("invoice date");
        }

        if (inputs.DueDate is not null && inputs.DueDate < inputs.Date)
        {
            missing.Add("a due date not before the invoice date");
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

    /// <summary>
    /// The line checks the workbench applies before anything is built: a volume without a price is blocked with the reason,
    /// naming the volume (the state table's first row).
    /// </summary>
    public static (string Code, string Message)? CheckLines(IReadOnlyList<OgSalesLineInput> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);

        foreach (var line in lines)
        {
            if (line.Quantity is null || line.Quantity <= 0m)
            {
                return (CodeInputInvalid,
                    "Every line needs a quantity above zero." +
                    (string.IsNullOrWhiteSpace(line.VolumeReference)
                        ? string.Empty
                        : " Volume " + line.VolumeReference + " has none."));
            }

            if (line.UnitPrice is null || line.UnitPrice <= 0m)
            {
                return (CodeVolumeWithoutPrice,
                    "Volume " + (line.VolumeReference ?? "(no reference)") +
                    " has no price, so its line cannot be invoiced. Price it on the contract or the workbench first.");
            }
        }

        return null;
    }

    /// <summary>The state table's "currency mismatch with contract - refused".</summary>
    public static (string Code, string Message)? CheckCurrency(OgInvoiceInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        if (string.IsNullOrWhiteSpace(inputs.ContractCurrency))
        {
            return null;
        }

        return string.Equals(inputs.ContractCurrency.Trim(), inputs.Currency?.Trim(),
            StringComparison.OrdinalIgnoreCase)
            ? null
            : (CodeCurrencyMismatch,
                $"The contract is in {inputs.ContractCurrency.Trim()} and the invoice is in {inputs.Currency}; " +
                "an invoice must be raised in its contract's currency.");
    }
}

/// <summary>ACC-03: revenue recognition, sales invoices, credit notes and their ledger effect.</summary>
public interface IOgRevenue
{
    /// <summary>Builds a draft invoice from priced volumes.</summary>
    Task<OgResult<OgRevenueWorkspace>> BuildAsync(
        OgInvoiceInputs inputs,
        OgRevenueContext context,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Issues it: server number, exactly one posted journal (AC-2, AC-6).</summary>
    Task<OgResult<OgRevenueWorkspace>> IssueAsync(
        string invoiceId,
        OgRevenueContext context,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Issues a credit note against an issued invoice (AC-1, SG-10-5).</summary>
    Task<OgResult<OgRevenueWorkspace>> CreditAsync(
        string invoiceId,
        decimal amount,
        string reason,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Approves or returns an issued invoice; the author may not approve (AC-4).</summary>
    Task<OgResult<OgRevenueWorkspace>> ReviewAsync(
        string invoiceId,
        bool approve,
        string reason,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a draft with no payments and no credits.</summary>
    Task<OgResult<OgRevenueWorkspace>> DeleteAsync(
        string invoiceId,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>The invoices, their lines and credits; an unavailable register is a state.</summary>
    Task<OgResult<OgRevenueWorkspace>> LoadAsync(
        OgRevenueContext context,
        string? invoiceId = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// ACC-03 over ProductionAccounting's <c>AR_INVOICE</c>, the new <c>AR_INVOICE_LINE</c> and <c>AR_CREDIT_MEMO</c>, and the
/// ledger's <c>JOURNAL_ENTRY</c>/<c>JOURNAL_ENTRY_LINE</c>, through units of work.
/// </summary>
/// <remarks>
/// The tax is never computed here: <see cref="SalesInvoiceTax"/> in the accounting project produces every net, tax and
/// total, and the service only stores them (AC-5). Issuing writes exactly one balanced journal and records its id on the
/// invoice, which is what makes a second issue and a retry after a lost acknowledgement both safe (AC-2, AC-6). A credit
/// note reduces the stored balance by exactly its amount (AC-1).
/// </remarks>
public sealed class OgRevenueService : IOgRevenue
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgRevenueService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgRevenueWorkspace>> BuildAsync(
        OgInvoiceInputs inputs,
        OgRevenueContext context,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgRevenueWorkspace>.Failed(
                "An invoice must record who built it.", RevenueRules.CodeActorRequired);
        }

        if (RevenueRules.Missing(inputs) is { Count: > 0 } missing)
        {
            return OgResult<OgRevenueWorkspace>.Failed(
                $"The invoice needs {string.Join(", ", missing)}.", RevenueRules.CodeInputRequired);
        }

        if (RevenueRules.CheckLines(inputs.Lines) is { } lineProblem)
        {
            return OgResult<OgRevenueWorkspace>.Failed(lineProblem.Message, lineProblem.Code);
        }

        if (RevenueRules.CheckCurrency(inputs) is { } currencyProblem)
        {
            return OgResult<OgRevenueWorkspace>.Failed(currencyProblem.Message, currencyProblem.Code);
        }

        if (inputs.TaxRate is < 0m or > 1m)
        {
            return OgResult<OgRevenueWorkspace>.Failed(
                "The tax rate is a fraction between 0 and 1; 12.5 % is entered as 0.125.",
                RevenueRules.CodeInputInvalid);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgRevenueWorkspace>.Failed(
                "No connection is open, so the invoice cannot be built.", RevenueRules.CodeNoConnection);
        }

        try
        {
            var rate = inputs.TaxRate ?? 0m;
            var now = DateTime.UtcNow;
            var invoiceId = Guid.NewGuid().ToString("N");

            // AC-5: every money figure comes from the owning project's tax helper.
            var lineMoney = inputs.Lines
                .Select(line => SalesInvoiceTax.Line(line.Quantity!.Value, line.UnitPrice!.Value, rate))
                .ToList();

            var totals = SalesInvoiceTax.Totals(lineMoney.Select(money => money.Net), rate);

            var unit = OgUow.For<AR_INVOICE>(source);
            unit.PrimaryKey = "AR_INVOICE_ID";

            unit.Add(new AR_INVOICE
            {
                AR_INVOICE_ID = invoiceId,
                INVOICE_NUMBER = null,
                CUSTOMER_BA_ID = inputs.BuyerId!.Trim(),
                INVOICE_DATE = inputs.Date!.Value.Date,
                DUE_DATE = inputs.DueDate?.Date,
                CURRENCY_CODE = inputs.Currency!.Trim().ToUpperInvariant(),
                TOTAL_AMOUNT = totals.Total,
                PAID_AMOUNT = 0m,
                BALANCE_DUE = totals.Total,
                STATUS = RevenueCodes.InvoiceDraft,
                AUTHOR = actor.Trim(),
                SUBTOTAL_AMOUNT = totals.Subtotal,
                TAX_AMOUNT = totals.Tax,
                TAX_RATE = rate,
                CONTRACT_ID = string.IsNullOrWhiteSpace(inputs.ContractId) ? null : inputs.ContractId.Trim(),
                CREDITED_AMOUNT = 0m,
                OWNER_TENANT_ID = context.CompanyId,
                OWNER_COMPANY_BA_ID = context.CompanyId,
                FIELD_ID = null,
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
                return OgResult<OgRevenueWorkspace>.Failed(
                    OgUow.Message(committed, "The invoice was not saved."), RevenueRules.CodeWriteFailed);
            }

            if (!await WriteLinesAsync(source, invoiceId, inputs.Lines, lineMoney, actor, cancellationToken)
                    .ConfigureAwait(false))
            {
                return OgResult<OgRevenueWorkspace>.Failed(
                    "The invoice's lines were not saved, so nothing was raised.", RevenueRules.CodeWriteFailed);
            }

            var workspace = await ReadAsync(source, context, invoiceId, cancellationToken).ConfigureAwait(false);

            return OgResult<OgRevenueWorkspace>.Ok(workspace);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "building a sales invoice",
                "the invoice was not built",
                FailureSeverity.Error);

            return OgResult<OgRevenueWorkspace>.Failed(
                "The invoice could not be built.", reference ?? RevenueRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgRevenueWorkspace>> IssueAsync(
        string invoiceId,
        OgRevenueContext context,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgRevenueWorkspace>.Failed(
                "An issue must record who made it.", RevenueRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgRevenueWorkspace>.Failed(
                "No connection is open, so the invoice cannot be issued.", RevenueRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, context, invoiceId, cancellationToken).ConfigureAwait(false);
            var invoice = workspace.Selected;

            if (invoice is null)
            {
                return OgResult<OgRevenueWorkspace>.Failed(
                    $"Invoice '{invoiceId}' is not in this database.", RevenueRules.CodeUnknownInvoice);
            }

            if (invoice.HasJournal && invoice.State != RevenueCodes.InvoiceDraft)
            {
                return OgResult<OgRevenueWorkspace>.Ok(workspace);
            }

            if (!invoice.IsDraft)
            {
                return OgResult<OgRevenueWorkspace>.Failed(
                    $"{invoice.Describe()} is already {invoice.StateText}.", RevenueRules.CodeAlreadyIssued);
            }

            // The period lock applies to the invoice's date, as it does to any other journal.
            var periodCode = LedgerCodes.PeriodCodeOf(invoice.Date);
            var periods = await OgUow.ReadAllAsync<COMPANY_LEDGER_PERIOD>(source).ConfigureAwait(false);
            var period = periods.FirstOrDefault(row =>
                string.Equals(row.PERIOD_CODE, periodCode, StringComparison.OrdinalIgnoreCase));

            if (period is not null && !LedgerCodes.IsOpen(period.STATUS))
            {
                return OgResult<OgRevenueWorkspace>.Failed(
                    $"Period {periodCode} is {period.STATUS.ToLowerInvariant()}, so {invoice.Describe()} cannot be " +
                    "issued into it. Reopen the period first.",
                    RevenueRules.CodePeriodClosed);
            }

            var accounts = await OgUow.ReadAllAsync<GL_ACCOUNT>(source).ConfigureAwait(false);

            var receivable = Find(accounts, RevenueCodes.AccountsReceivableAccount);
            var revenue = Find(accounts, RevenueCodes.RevenueAccount);
            var tax = invoice.Tax > 0m ? Find(accounts, RevenueCodes.SalesTaxAccount) : null;

            if (receivable is null || revenue is null || (invoice.Tax > 0m && tax is null))
            {
                var missingAccount = receivable is null
                    ? RevenueCodes.AccountsReceivableAccount
                    : revenue is null
                        ? RevenueCodes.RevenueAccount
                        : RevenueCodes.SalesTaxAccount;

                return OgResult<OgRevenueWorkspace>.Failed(
                    $"The chart of accounts has no account {missingAccount}, so {invoice.Describe()} cannot be posted. " +
                    "Add the account before issuing.",
                    RevenueRules.CodeUnknownAccount);
            }

            // The journal's reference is the invoice's own id, which exists before any numbering - so a retry after a lost
            // acknowledgement can find the entry it already wrote (AC-6) instead of writing a second one.
            var reference = "SALES-" + invoice.Id;
            var journals = await OgUow.ReadAllAsync<JOURNAL_ENTRY>(source).ConfigureAwait(false);

            var existing = journals.FirstOrDefault(entry =>
                string.Equals(entry.SOURCE_MODULE, "SALES_INVOICE", StringComparison.OrdinalIgnoreCase)
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

                // The entry is balanced by construction from the invoice's own totals, so it is written posted: issuing is
                // the posting. The period was checked above, which is the only ledger rule an invoice has to satisfy.
                var journal = OgUow.For<JOURNAL_ENTRY>(source);
                journal.PrimaryKey = "JOURNAL_ENTRY_ID";

                journal.Add(new JOURNAL_ENTRY
                {
                    JOURNAL_ENTRY_ID = journalId,
                    ENTRY_NUMBER = NextJournalNumber(journals, invoice.Date),
                    ENTRY_DATE = invoice.Date,
                    ENTRY_TYPE = "SALES",
                    STATUS = LedgerCodes.JournalPosted,
                    DESCRIPTION = "Sales invoice " + (string.IsNullOrWhiteSpace(invoice.Number)
                        ? invoice.Id
                        : invoice.Number),
                    REFERENCE_NUMBER = reference,
                    SOURCE_MODULE = "SALES_INVOICE",
                    TOTAL_DEBIT = invoice.Total,
                    TOTAL_CREDIT = invoice.Total,
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
                    return OgResult<OgRevenueWorkspace>.Failed(
                        OgUow.Message(journalCommitted, "The invoice journal was not written; nothing was issued."),
                        RevenueRules.CodeWriteFailed);
                }

                var entryLines = new List<(GL_ACCOUNT Account, decimal Debit, decimal Credit)>
                {
                    (receivable!, invoice.Total, 0m),
                    (revenue!, 0m, invoice.Subtotal),
                };

                if (invoice.Tax > 0m)
                {
                    entryLines.Add((tax!, 0m, invoice.Tax));
                }

                if (!await WriteEntryLinesAsync(source, journalId, entryLines, actor, cancellationToken)
                        .ConfigureAwait(false))
                {
                    return OgResult<OgRevenueWorkspace>.Failed(
                        "The journal's lines were not saved.", RevenueRules.CodeWriteFailed);
                }
            }

            var row = await ReadInvoiceRowAsync(source, invoice.Id, cancellationToken).ConfigureAwait(false);

            if (row is null)
            {
                return OgResult<OgRevenueWorkspace>.Failed(
                    $"Invoice '{invoiceId}' is not in this database.", RevenueRules.CodeUnknownInvoice);
            }

            row.INVOICE_NUMBER = string.IsNullOrWhiteSpace(row.INVOICE_NUMBER)
                ? NextNumber(await OgUow.ReadAllAsync<AR_INVOICE>(source).ConfigureAwait(false), invoice.Date)
                : row.INVOICE_NUMBER;
            row.STATUS = RevenueCodes.InvoiceIssued;
            row.JOURNAL_ENTRY_ID = journalId;
            row.ISSUED_BY = actor.Trim();
            row.ISSUED_AT = now;
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = now;

            var written = await UpdateAsync(source, row).ConfigureAwait(false);

            if (written is not null)
            {
                return OgResult<OgRevenueWorkspace>.Failed(written, RevenueRules.CodeWriteFailed);
            }

            var after = await ReadAsync(source, context, invoice.Id, cancellationToken).ConfigureAwait(false);

            return OgResult<OgRevenueWorkspace>.Ok(after);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"issuing sales invoice {invoiceId}",
                "the invoice was not issued",
                FailureSeverity.Error);

            return OgResult<OgRevenueWorkspace>.Failed(
                "The invoice could not be issued.", reference ?? RevenueRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgRevenueWorkspace>> CreditAsync(
        string invoiceId,
        decimal amount,
        string reason,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgRevenueWorkspace>.Failed(
                "A credit note must record who issued it.", RevenueRules.CodeActorRequired);
        }

        if (amount <= 0m)
        {
            return OgResult<OgRevenueWorkspace>.Failed(
                "A credit note needs an amount above zero.", RevenueRules.CodeInputInvalid);
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return OgResult<OgRevenueWorkspace>.Failed(
                "A credit note must record its reason.", RevenueRules.CodeInputRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgRevenueWorkspace>.Failed(
                "No connection is open, so the credit note cannot be issued.", RevenueRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, context: null, invoiceId, cancellationToken).ConfigureAwait(false);
            var invoice = workspace.Selected;

            if (invoice is null)
            {
                return OgResult<OgRevenueWorkspace>.Failed(
                    $"Invoice '{invoiceId}' is not in this database.", RevenueRules.CodeUnknownInvoice);
            }

            if (!invoice.IsIssued)
            {
                return OgResult<OgRevenueWorkspace>.Failed(
                    $"{invoice.Describe()} is {invoice.StateText}; a credit note is issued against an issued invoice.",
                    RevenueRules.CodeCreditNeedsIssued);
            }

            // AC-1 and SG-10-5: a credit may not take the balance below zero, and it reduces it by exactly its amount.
            if (amount > invoice.BalanceDue)
            {
                return OgResult<OgRevenueWorkspace>.Failed(
                    $"The credit of {MoneyText.Of(amount)} {invoice.Currency} is more than the " +
                    $"{MoneyText.Of(invoice.BalanceDue)} {invoice.Currency} outstanding on {invoice.Number}.",
                    RevenueRules.CodeCreditTooLarge);
            }

            var now = DateTime.UtcNow;
            var memoNumber = NextCreditNumber(
                await OgUow.ReadAllAsync<AR_CREDIT_MEMO>(source).ConfigureAwait(false), invoice.Date);

            var memo = OgUow.For<AR_CREDIT_MEMO>(source);
            memo.PrimaryKey = "AR_CREDIT_MEMO_ID";

            memo.Add(new AR_CREDIT_MEMO
            {
                AR_CREDIT_MEMO_ID = Guid.NewGuid().ToString("N"),
                AR_INVOICE_ID = invoice.Id,
                CREDIT_MEMO_NUMBER = memoNumber,
                CREDIT_MEMO_DATE = now.Date,
                CREDIT_AMOUNT = amount,
                REASON = reason.Trim(),
                STATUS = RevenueCodes.CreditMemoIssued,
                DESCRIPTION = "Credit against " + invoice.Number,
                ACTIVE_IND = "Y",
                PPDM_GUID = Guid.NewGuid().ToString("N"),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await memo.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgRevenueWorkspace>.Failed(
                    OgUow.Message(committed, "The credit note was not saved."), RevenueRules.CodeWriteFailed);
            }

            var row = await ReadInvoiceRowAsync(source, invoice.Id, cancellationToken).ConfigureAwait(false);

            if (row is null)
            {
                return OgResult<OgRevenueWorkspace>.Failed(
                    $"Invoice '{invoiceId}' is not in this database.", RevenueRules.CodeUnknownInvoice);
            }

            row.CREDITED_AMOUNT = (row.CREDITED_AMOUNT ?? 0m) + amount;
            row.BALANCE_DUE = (row.TOTAL_AMOUNT ?? 0m) - (row.PAID_AMOUNT ?? 0m) - row.CREDITED_AMOUNT;
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = now;

            var written = await UpdateAsync(source, row).ConfigureAwait(false);

            if (written is not null)
            {
                return OgResult<OgRevenueWorkspace>.Failed(written, RevenueRules.CodeWriteFailed);
            }

            var after = await ReadAsync(source, context: null, invoice.Id, cancellationToken).ConfigureAwait(false);

            return OgResult<OgRevenueWorkspace>.Ok(after);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"issuing a credit note on {invoiceId}",
                "the credit note was not issued",
                FailureSeverity.Error);

            return OgResult<OgRevenueWorkspace>.Failed(
                "The credit note could not be issued.", reference ?? RevenueRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgRevenueWorkspace>> ReviewAsync(
        string invoiceId,
        bool approve,
        string reason,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgRevenueWorkspace>.Failed(
                "A review must record who made it.", RevenueRules.CodeActorRequired);
        }

        if (!approve && string.IsNullOrWhiteSpace(reason))
        {
            return OgResult<OgRevenueWorkspace>.Failed(
                "Returning an invoice needs its reason, or the author cannot see what to fix.",
                RevenueRules.CodeReturnReasonRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgRevenueWorkspace>.Failed(
                "No connection is open, so the review cannot be recorded.", RevenueRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, context: null, invoiceId, cancellationToken).ConfigureAwait(false);
            var invoice = workspace.Selected;

            if (invoice is null)
            {
                return OgResult<OgRevenueWorkspace>.Failed(
                    $"Invoice '{invoiceId}' is not in this database.", RevenueRules.CodeUnknownInvoice);
            }

            if (!invoice.IsIssued)
            {
                return OgResult<OgRevenueWorkspace>.Failed(
                    $"{invoice.Describe()} is {invoice.StateText}; only an issued invoice is reviewed.",
                    RevenueRules.CodeInputInvalid);
            }

            // AC-4: the author cannot approve their own invoice.
            if (approve && !string.IsNullOrWhiteSpace(invoice.Author)
                && string.Equals(invoice.Author.Trim(), actor.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgRevenueWorkspace>.Failed(
                    $"{invoice.Number} was built by {invoice.Author}, who therefore cannot approve it. A different " +
                    "controller must review it.",
                    RevenueRules.CodeAuthorCannotApprove);
            }

            var row = await ReadInvoiceRowAsync(source, invoice.Id, cancellationToken).ConfigureAwait(false);

            if (row is null)
            {
                return OgResult<OgRevenueWorkspace>.Failed(
                    $"Invoice '{invoiceId}' is not in this database.", RevenueRules.CodeUnknownInvoice);
            }

            row.STATUS = approve ? RevenueCodes.InvoiceApproved : RevenueCodes.InvoiceReturned;
            row.RETURN_REASON = approve ? null : reason.Trim();
            row.APPROVED_BY = approve ? actor.Trim() : null;
            row.APPROVED_AT = approve ? DateTime.UtcNow : null;
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = DateTime.UtcNow;

            var written = await UpdateAsync(source, row).ConfigureAwait(false);

            if (written is not null)
            {
                return OgResult<OgRevenueWorkspace>.Failed(written, RevenueRules.CodeWriteFailed);
            }

            var after = await ReadAsync(source, context: null, invoice.Id, cancellationToken).ConfigureAwait(false);

            return OgResult<OgRevenueWorkspace>.Ok(after);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"reviewing sales invoice {invoiceId}",
                "the review was not recorded",
                FailureSeverity.Error);

            return OgResult<OgRevenueWorkspace>.Failed(
                "The review could not be recorded.", reference ?? RevenueRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgRevenueWorkspace>> DeleteAsync(
        string invoiceId,
        string actor,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgRevenueWorkspace>.Failed(
                "No connection is open, so the invoice cannot be deleted.", RevenueRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, context: null, invoiceId, cancellationToken).ConfigureAwait(false);
            var invoice = workspace.Selected;

            if (invoice is null)
            {
                return OgResult<OgRevenueWorkspace>.Failed(
                    $"Invoice '{invoiceId}' is not in this database.", RevenueRules.CodeUnknownInvoice);
            }

            // The state table's "invoice with payments - deletion refused".
            if (!invoice.CanDelete)
            {
                return OgResult<OgRevenueWorkspace>.Failed(
                    $"{invoice.Describe()} cannot be deleted: it is {invoice.StateText}, paid " +
                    $"{MoneyText.Of(invoice.Paid)} and credited {MoneyText.Of(invoice.Credited)}.",
                    RevenueRules.CodeNotDeletable);
            }

            var lineUnit = OgUow.For<AR_INVOICE_LINE>(source);
            lineUnit.PrimaryKey = "AR_INVOICE_LINE_ID";

            // Read through the unit of work so the rows it deletes are the ones it is tracking.
            var lines = await OgUow.ReadAsync(
                lineUnit,
                [OgUow.Equal("AR_INVOICE_ID", invoice.Id)],
                cancellationToken).ConfigureAwait(false);

            foreach (var line in lines)
            {
                var removed = lineUnit.Delete(line);

                if (!OgUow.IsOk(removed))
                {
                    return OgResult<OgRevenueWorkspace>.Failed(
                        OgUow.Message(removed, "The invoice's lines were not deleted; nothing was removed."),
                        RevenueRules.CodeWriteFailed);
                }
            }

            var linesCommitted = await lineUnit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(linesCommitted))
            {
                return OgResult<OgRevenueWorkspace>.Failed(
                    OgUow.Message(linesCommitted, "The invoice's lines were not deleted; nothing was removed."),
                    RevenueRules.CodeWriteFailed);
            }

            var unit = OgUow.For<AR_INVOICE>(source);
            unit.PrimaryKey = "AR_INVOICE_ID";

            var rows = await OgUow.ReadAsync(
                unit,
                [OgUow.Equal("AR_INVOICE_ID", invoice.Id)],
                cancellationToken).ConfigureAwait(false);

            var row = rows.FirstOrDefault();

            if (row is not null)
            {
                var removed = unit.Delete(row);

                if (!OgUow.IsOk(removed))
                {
                    return OgResult<OgRevenueWorkspace>.Failed(
                        OgUow.Message(removed, "The invoice was not deleted."), RevenueRules.CodeWriteFailed);
                }

                var committed = await unit.Commit().ConfigureAwait(false);

                if (!OgUow.IsOk(committed))
                {
                    return OgResult<OgRevenueWorkspace>.Failed(
                        OgUow.Message(committed, "The invoice was not deleted."), RevenueRules.CodeWriteFailed);
                }
            }

            var after = await ReadAsync(source, context: null, null, cancellationToken).ConfigureAwait(false);

            return OgResult<OgRevenueWorkspace>.Ok(after);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"deleting sales invoice {invoiceId}",
                "the invoice was not deleted",
                FailureSeverity.Error);

            return OgResult<OgRevenueWorkspace>.Failed(
                "The invoice could not be deleted.", reference ?? RevenueRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgRevenueWorkspace>> LoadAsync(
        OgRevenueContext context,
        string? invoiceId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var source = _source();

        if (source is null)
        {
            return OgResult<OgRevenueWorkspace>.Failed(
                "No connection is open, so the invoices cannot be read.", RevenueRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, context, invoiceId, cancellationToken).ConfigureAwait(false);

            return OgResult<OgRevenueWorkspace>.Ok(workspace);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the sales invoices",
                "the invoices are unavailable; nothing can be issued",
                FailureSeverity.Degraded);

            return OgResult<OgRevenueWorkspace>.Ok(new OgRevenueWorkspace(
                [], null, RevenueRules.States, RevenueRules.QuantityUnits, RevenueRules.Currencies,
                string.Empty, true, $"the invoices could not be read (reference {reference})"));
        }
    }

    // ------------------------------------------------------------------ the reads

    private static async Task<OgRevenueWorkspace> ReadAsync(
        IDataSource source,
        OgRevenueContext? context,
        string? invoiceId,
        CancellationToken cancellationToken)
    {
        var invoices = await OgUow.ReadAllAsync<AR_INVOICE>(source).ConfigureAwait(false);
        var lines = await OgUow.ReadAllAsync<AR_INVOICE_LINE>(source).ConfigureAwait(false);
        var memos = await OgUow.ReadAllAsync<AR_CREDIT_MEMO>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var linesByInvoice = lines
            .Where(line => !string.IsNullOrWhiteSpace(line.AR_INVOICE_ID))
            .GroupBy(line => line.AR_INVOICE_ID!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(line => line.LINE_NUMBER).ToList(),
                StringComparer.OrdinalIgnoreCase);

        var memosByInvoice = memos
            .Where(memo => !string.IsNullOrWhiteSpace(memo.AR_INVOICE_ID))
            .GroupBy(memo => memo.AR_INVOICE_ID!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);

        var rows = invoices
            .Where(row => !string.IsNullOrWhiteSpace(row.AR_INVOICE_ID))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(row => row.INVOICE_DATE)
            .ThenBy(row => row.INVOICE_NUMBER, StringComparer.Ordinal)
            .Select(row => ToInvoice(
                row,
                linesByInvoice.TryGetValue(row.AR_INVOICE_ID, out var mine) ? mine : [],
                memosByInvoice.TryGetValue(row.AR_INVOICE_ID, out var notes) ? notes : []))
            .ToList();

        var selected = string.IsNullOrWhiteSpace(invoiceId)
            ? rows.FirstOrDefault()
            : rows.FirstOrDefault(invoice =>
                  string.Equals(invoice.Id, invoiceId.Trim(), StringComparison.OrdinalIgnoreCase))
              ?? rows.FirstOrDefault();

        return new OgRevenueWorkspace(
            rows,
            selected,
            RevenueRules.States,
            RevenueRules.QuantityUnits,
            RevenueRules.Currencies,
            context is null
                ? string.Empty
                : "posting to book " + context.BookCode + " for company " + context.CompanyId,
            false,
            null);
    }

    private static OgSalesInvoice ToInvoice(
        AR_INVOICE row,
        IReadOnlyList<AR_INVOICE_LINE> lines,
        IReadOnlyList<AR_CREDIT_MEMO> memos)
    {
        var mapped = lines
            .Select(line => new OgSalesLine(
                line.AR_INVOICE_LINE_ID,
                line.LINE_NUMBER,
                line.COMMODITY,
                line.DESCRIPTION,
                line.QUANTITY,
                line.QUANTITY_OUOM,
                line.UNIT_PRICE,
                line.NET_AMOUNT,
                line.TAX_AMOUNT,
                line.LINE_TOTAL,
                line.VOLUME_REFERENCE))
            .ToList();

        var credits = memos
            .Select(memo => new OgCreditNote(
                memo.AR_CREDIT_MEMO_ID,
                memo.AR_INVOICE_ID,
                memo.CREDIT_MEMO_NUMBER ?? string.Empty,
                memo.CREDIT_MEMO_DATE ?? DateTime.MinValue,
                memo.CREDIT_AMOUNT,
                memo.REASON,
                memo.STATUS ?? RevenueCodes.CreditMemoIssued,
                memo.ROW_CREATED_BY))
            .OrderBy(note => note.Date)
            .ToList();

        return new OgSalesInvoice(
            row.AR_INVOICE_ID,
            row.INVOICE_NUMBER ?? string.Empty,
            row.CUSTOMER_BA_ID,
            row.CONTRACT_ID,
            row.INVOICE_DATE ?? DateTime.MinValue,
            row.DUE_DATE,
            row.CURRENCY_CODE ?? string.Empty,
            row.SUBTOTAL_AMOUNT ?? mapped.Sum(line => line.Net),
            row.TAX_AMOUNT ?? mapped.Sum(line => line.Tax),
            row.TOTAL_AMOUNT ?? 0m,
            row.PAID_AMOUNT ?? 0m,
            row.CREDITED_AMOUNT ?? credits.Sum(note => note.Amount),
            row.BALANCE_DUE ?? 0m,
            row.STATUS ?? RevenueCodes.InvoiceDraft,
            row.AUTHOR ?? row.ROW_CREATED_BY,
            row.JOURNAL_ENTRY_ID,
            row.ISSUED_BY,
            row.ISSUED_AT,
            row.APPROVED_BY,
            row.APPROVED_AT,
            row.RETURN_REASON,
            mapped,
            credits);
    }

    private static async Task<AR_INVOICE?> ReadInvoiceRowAsync(
        IDataSource source,
        string invoiceId,
        CancellationToken cancellationToken)
    {
        var unit = OgUow.For<AR_INVOICE>(source);
        unit.PrimaryKey = "AR_INVOICE_ID";

        var rows = await OgUow.ReadAsync(
            unit,
            [OgUow.Equal("AR_INVOICE_ID", invoiceId)],
            cancellationToken).ConfigureAwait(false);

        return rows.FirstOrDefault();
    }

    // ------------------------------------------------------------------ the writes

    private static async Task<bool> WriteLinesAsync(
        IDataSource source,
        string invoiceId,
        IReadOnlyList<OgSalesLineInput> inputs,
        IReadOnlyList<SalesInvoiceLineMoney> money,
        string actor,
        CancellationToken cancellationToken)
    {
        var unit = OgUow.For<AR_INVOICE_LINE>(source);
        unit.PrimaryKey = "AR_INVOICE_LINE_ID";

        var now = DateTime.UtcNow;

        for (var index = 0; index < inputs.Count; index++)
        {
            var line = inputs[index];
            var amounts = money[index];

            unit.Add(new AR_INVOICE_LINE
            {
                AR_INVOICE_LINE_ID = Guid.NewGuid().ToString("N"),
                AR_INVOICE_ID = invoiceId,
                LINE_NUMBER = index + 1,
                COMMODITY = line.Commodity,
                DESCRIPTION = line.Description,
                QUANTITY = line.Quantity!.Value,
                QUANTITY_OUOM = line.Unit,
                UNIT_PRICE = line.UnitPrice!.Value,
                NET_AMOUNT = amounts.Net,
                TAX_AMOUNT = amounts.Tax,
                LINE_TOTAL = amounts.Total,
                VOLUME_REFERENCE = line.VolumeReference,
                ACTIVE_IND = "Y",
                PPDM_GUID = Guid.NewGuid().ToString("N"),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });
        }

        cancellationToken.ThrowIfCancellationRequested();

        var committed = await unit.Commit().ConfigureAwait(false);

        return OgUow.IsOk(committed);
    }

    private static async Task<bool> WriteEntryLinesAsync(
        IDataSource source,
        string journalId,
        IReadOnlyList<(GL_ACCOUNT Account, decimal Debit, decimal Credit)> lines,
        string actor,
        CancellationToken cancellationToken)
    {
        var unit = OgUow.For<JOURNAL_ENTRY_LINE>(source);
        unit.PrimaryKey = "JOURNAL_ENTRY_LINE_ID";

        var now = DateTime.UtcNow;
        var number = 1;

        foreach (var (account, debit, credit) in lines)
        {
            unit.Add(new JOURNAL_ENTRY_LINE
            {
                JOURNAL_ENTRY_LINE_ID = Guid.NewGuid().ToString("N"),
                JOURNAL_ENTRY_ID = journalId,
                GL_ACCOUNT_ID = account.GL_ACCOUNT_ID,
                LINE_NUMBER = number,
                DEBIT_AMOUNT = debit,
                CREDIT_AMOUNT = credit,
                DESCRIPTION = "sales invoice",
                CURRENCY_CODE = account.CURRENCY_CODE,
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

    private static async Task<string?> UpdateAsync(IDataSource source, AR_INVOICE row)
    {
        var unit = OgUow.For<AR_INVOICE>(source);
        unit.PrimaryKey = "AR_INVOICE_ID";

        var updated = unit.Update(row);

        if (!OgUow.IsOk(updated))
        {
            return OgUow.Message(updated, "The invoice was not saved.");
        }

        var committed = await unit.Commit().ConfigureAwait(false);

        return OgUow.IsOk(committed) ? null : OgUow.Message(committed, "The invoice was not saved.");
    }

    private static GL_ACCOUNT? Find(IReadOnlyList<GL_ACCOUNT> accounts, string number) =>
        accounts.FirstOrDefault(account =>
            string.Equals(account.ACCOUNT_NUMBER?.Trim(), number, StringComparison.OrdinalIgnoreCase));

    /// <summary>The next invoice number for a year: INV-yyyy-0001, server-owned.</summary>
    private static string NextNumber(IReadOnlyList<AR_INVOICE> existing, DateTime date)
    {
        var prefix = "INV-" + date.Year.ToString(CultureInfo.InvariantCulture) + "-";

        var highest = existing
            .Select(row => row.INVOICE_NUMBER)
            .Where(number => number is not null && number.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Select(number => int.TryParse(number![prefix.Length..], NumberStyles.Integer,
                CultureInfo.InvariantCulture, out var sequence)
                ? sequence
                : 0)
            .DefaultIfEmpty(0)
            .Max();

        return prefix + (highest + 1).ToString("0000", CultureInfo.InvariantCulture);
    }

    /// <summary>The next journal entry number for a year: JE-yyyy-0001.</summary>
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

    /// <summary>The next credit-note number for a year: CN-yyyy-0001.</summary>
    private static string NextCreditNumber(IReadOnlyList<AR_CREDIT_MEMO> existing, DateTime date)
    {
        var prefix = "CN-" + date.Year.ToString(CultureInfo.InvariantCulture) + "-";

        var highest = existing
            .Select(row => row.CREDIT_MEMO_NUMBER)
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
