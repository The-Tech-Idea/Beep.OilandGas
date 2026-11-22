using System.Globalization;
using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Accounting;

/// <summary>Who is asking, and for which books and period (ACC-01).</summary>
public sealed record OgDashboardContext(
    string CompanyId,
    string CompanyName,
    string BookCode,
    string Currency,
    string? PeriodCode,
    string? PeriodName,
    OgPermissions Permissions)
{
    public static OgDashboardContext FromSession(OgSession session, OgPermissions permissions) =>
        new(
            string.IsNullOrWhiteSpace(session.CompanyId) ? "APP" : session.CompanyId,
            session.CompanyName ?? string.Empty,
            string.IsNullOrWhiteSpace(session.BookCode) ? "MAIN" : session.BookCode,
            string.IsNullOrWhiteSpace(session.BaseCurrencyCode) ? "USD" : session.BaseCurrencyCode,
            string.IsNullOrWhiteSpace(session.PeriodId) ? null : session.PeriodId,
            session.PeriodName,
            permissions ?? OgPermissions.None);

    /// <summary>The session has a period, so AC-3's period strip can name one.</summary>
    public bool HasPeriod => !string.IsNullOrWhiteSpace(PeriodCode);
}

/// <summary>
/// The dashboard's numbers (ACC-01). Every figure is <b>nullable</b>: null means the records behind it could not be read,
/// which is a different answer from zero and is shown as one (AC-1).
/// </summary>
public sealed record OgAccountingKpis(
    decimal? OpenAp,
    decimal? OpenAr,
    decimal? Overdue,
    int? UnpostedJournals,
    string PeriodState,
    decimal? Cash,
    string Currency,
    decimal? CashOpening,
    decimal? CashMovement,
    IReadOnlyList<string> Unavailable)
{
    public string OpenApText => Money(OpenAp);

    public string OpenArText => Money(OpenAr);

    public string OverdueText => Money(Overdue);

    public string UnpostedJournalsText => UnpostedJournals is { } count
        ? count.ToString(CultureInfo.InvariantCulture)
        : UnavailableText("the journals");

    public string CashText => Money(Cash);

    public string CashBreakdownText => Cash is null
        ? UnavailableText("the cash position")
        : "opening " + Amount(CashOpening) + " + posted movement " + Amount(CashMovement) + " " + Currency;

    /// <summary>AC-1: a figure whose source failed says so, and never prints a zero for it.</summary>
    public string UnavailableText(string source) =>
        "unavailable - " + (Unavailable.Count == 0
            ? source + " could not be read"
            : string.Join("; ", Unavailable));

    public bool AnyUnavailable => Unavailable.Count > 0;

    private string Money(decimal? amount) =>
        amount is { } value ? MoneyText.Of(value) + " " + Currency : UnavailableText("the balance");

    private string Amount(decimal? value) =>
        value is { } amount ? MoneyText.Of(amount) : "unknown";
}

/// <summary>One queue panel on the dashboard (ACC-01, AC-2, AC-4).</summary>
public sealed record OgDashboardQueue(
    string Key,
    string Title,
    string Permission,
    int? Count,
    string? WithheldReason,
    string Detail)
{
    /// <summary>AC-4: a queue without the permission is omitted with its reason, not shown empty.</summary>
    public bool IsWithheld => WithheldReason is not null;

    public bool IsShown => !IsWithheld;

    public string CountText => IsWithheld
        ? "(withheld)"
        : Count is { } count
            ? count.ToString(CultureInfo.InvariantCulture)
            : "(not read)";

    /// <summary>
    /// AC-1 and "empty queue": a count of zero says nothing is awaiting you; a count that could not be read says that
    /// instead, because an unreadable queue is not an empty one.
    /// </summary>
    public string StateText => IsWithheld
        ? WithheldReason!
        : Count is not { } count
            ? Detail
            : count == 0
                ? "nothing awaiting you"
                : Detail;

    public string Describe() => IsWithheld
        ? Title + " - withheld: " + WithheldReason
        : Title + " · " + CountText + " · " + StateText;
}

/// <summary>One item needing attention (ACC-01).</summary>
public sealed record OgDashboardAttention(
    string Kind,
    string Subject,
    string Detail,
    decimal? Amount,
    DateTime? Due)
{
    public string Describe() =>
        Kind + " · " + Subject + " · " + Detail +
        (Amount is { } amount ? " · " + MoneyText.Of(amount) : string.Empty) +
        (Due is { } due ? " · due " + due.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : string.Empty);
}

/// <summary>The period strip (ACC-01, AC-3).</summary>
public sealed record OgDashboardPeriod(
    string Code,
    string Name,
    string State,
    int BlockingItems,
    bool Found,
    string Text)
{
    public string Describe() => Text;
}

/// <summary>The accounting dashboard as one read (ACC-01).</summary>
public sealed record OgAccountingDashboard(
    OgAccountingKpis Kpis,
    IReadOnlyList<OgDashboardQueue> Queues,
    IReadOnlyList<OgDashboardAttention> Attention,
    OgDashboardPeriod Period,
    DateTime RefreshedAt,
    bool ReadFailed,
    IReadOnlyList<string> Failures)
{
    public IReadOnlyList<OgDashboardQueue> Shown => Queues.Where(queue => queue.IsShown).ToList();

    public IReadOnlyList<OgDashboardQueue> Withheld => Queues.Where(queue => queue.IsWithheld).ToList();

    public string QueuesText => Queues.Count == 0
        ? "no queue is configured"
        : string.Join(Environment.NewLine, Queues.Select(queue => queue.Describe()));

    public string AttentionText => Attention.Count == 0
        ? "nothing needs attention"
        : string.Join(Environment.NewLine, Attention.Select(item => item.Describe()));

    /// <summary>AC-5: the last-refreshed time is always printable, whether or not the read worked.</summary>
    public string RefreshedText =>
        "last refreshed " + RefreshedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) +
        (ReadFailed ? " (the refresh failed)" : string.Empty);

    public string FailureText => Failures.Count == 0
        ? string.Empty
        : "what could not be read: " + string.Join("; ", Failures);

    public string SummaryText =>
        $"{Kpis.OpenArText} receivable · {Kpis.OpenApText} payable · {Kpis.UnpostedJournalsText} unposted journal(s)";
}

/// <summary>The dashboard rules, stated once so the page, the service and the tests agree (ACC-01).</summary>
public static class DashboardRules
{
    public const string CodeNoConnection = "dashboard-no-connection";
    public const string CodeReadFailed = "dashboard-read-failed";

    /// <summary>The cash account whose position the dashboard shows - the configured chart's cash account.</summary>
    public const string CashAccount = DefaultGlAccounts.Cash;

    /// <summary>How many attention rows the list shows before it says there are more.</summary>
    public const int AttentionLimit = 25;

    /// <summary>
    /// AC-4: whether a queue may be shown, and the reason when it may not.
    /// </summary>
    /// <remarks>
    /// A permission set with <see cref="OgPermissions.PermissionSourceAvailable"/> false is <b>unknown</b>, not empty: the
    /// lookup failed, so the queue is withheld and the reason says that rather than implying the person lacks the right.
    /// </remarks>
    public static (bool Shown, string? Reason) Access(OgPermissions? permissions, string permission, string title)
    {
        if (permissions is null)
        {
            return (false, "no permission set was supplied, so " + title + " is withheld rather than shown empty");
        }

        if (!permissions.PermissionSourceAvailable)
        {
            return (false,
                "the permission lookup failed, so " + title + " is withheld rather than shown empty - it is unknown, " +
                "not denied");
        }

        if (permissions.IsSystemAdmin || permissions.HasPermission(permission))
        {
            return (true, null);
        }

        return (false, "requires " + permission + ", which your account does not hold");
    }

    /// <summary>Whether an invoice is overdue: it still owes something and its due date has passed.</summary>
    public static bool IsOverdue(decimal balance, DateTime? dueDate, DateTime today) =>
        balance > 0m && dueDate is { } due && due.Date < today.Date;

    /// <summary>Whether a period accepts journals - the state the strip reports.</summary>
    public static string PeriodStateOf(string? status) => status?.Trim().ToUpperInvariant() switch
    {
        LedgerCodes.PeriodOpen => "open",
        LedgerCodes.PeriodClosed => "closed",
        LedgerCodes.PeriodLocked => "locked",
        null or "" => "not opened",
        _ => status!.ToLowerInvariant(),
    };
}

/// <summary>ACC-01: the accounting dashboard.</summary>
public interface IOgAccountingDashboard
{
    /// <summary>Reads the queues, the balances, the period and the attention list. Nothing is written.</summary>
    Task<OgResult<OgAccountingDashboard>> LoadAsync(
        OgDashboardContext context,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// ACC-01 over the seven accounting features this stage built: the dashboard composes their services rather than
/// re-querying their tables.
/// </summary>
/// <remarks>
/// <para>
/// Composing the services is what makes AC-2 true by construction: a queue count is computed from the same records - and the
/// same predicates on those records - that the queue's own page lists, so the dashboard cannot disagree with the page it
/// links to. It is also what makes AC-1 honest: each service reports <c>HistoryFailed</c> when it could not read its records,
/// and the dashboard turns that into <b>null</b> with the reason, never into a zero balance.
/// </para>
/// <para>
/// The cash position is the only figure computed here rather than read from a service: the configured cash account's opening
/// balance from the chart plus the posted movements on that account from the ledger's journals, because the ledger service
/// reports a period's trial balance rather than a position across periods.
/// </para>
/// </remarks>
public sealed class OgAccountingDashboardService : IOgAccountingDashboard
{
    private readonly Func<IDataSource?> _source;
    private readonly IOgRevenue _revenue;
    private readonly IOgReceivable _receivable;
    private readonly IOgPayables _payables;
    private readonly IOgLedger _ledger;
    private readonly IOgTax _tax;
    private readonly IOgRoyalties _royalties;
    private readonly IOgWarehouse _warehouse;
    private readonly IFailureReporter _failures;

    public OgAccountingDashboardService(
        Func<IDataSource?> source,
        IOgRevenue revenue,
        IOgReceivable receivable,
        IOgPayables payables,
        IOgLedger ledger,
        IOgTax tax,
        IOgRoyalties royalties,
        IOgWarehouse warehouse,
        IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _revenue = revenue ?? throw new ArgumentNullException(nameof(revenue));
        _receivable = receivable ?? throw new ArgumentNullException(nameof(receivable));
        _payables = payables ?? throw new ArgumentNullException(nameof(payables));
        _ledger = ledger ?? throw new ArgumentNullException(nameof(ledger));
        _tax = tax ?? throw new ArgumentNullException(nameof(tax));
        _royalties = royalties ?? throw new ArgumentNullException(nameof(royalties));
        _warehouse = warehouse ?? throw new ArgumentNullException(nameof(warehouse));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgAccountingDashboard>> LoadAsync(
        OgDashboardContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var refreshedAt = DateTime.UtcNow;
        var failures = new List<string>();
        var today = DateTime.Today;

        try
        {
            var revenueContext = new OgRevenueContext(context.CompanyId, context.BookCode);
            var payableContext = new OgPayableContext(context.CompanyId, context.BookCode);
            var ledgerContext = new OgLedgerContext(context.CompanyId, context.CompanyName, context.BookCode,
                context.Currency);
            var taxContext = new OgTaxContext(context.CompanyId, context.BookCode);
            var royaltyContext = new OgRoyaltyContext(context.CompanyId, context.BookCode);
            var warehouseContext = new OgWarehouseContext(context.CompanyId, context.BookCode);

            var revenue = await _revenue.LoadAsync(revenueContext, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            var receivable = await _receivable.LoadAsync(revenueContext, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            var payables = await _payables.LoadAsync(payableContext, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            var ledger = await _ledger.LoadAsync(ledgerContext, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            var tax = await _tax.LoadAsync(taxContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            var royalties = await _royalties.LoadAsync(royaltyContext, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            var warehouse = await _warehouse.LoadAsync(warehouseContext, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            // A source is usable only when its read succeeded and its register actually came back.
            var revenueOk = Usable(revenue, "the sales invoices", failures);
            var receivableOk = Usable(receivable, "the receipts", failures);
            var payablesOk = Usable(payables, "the supplier invoices", failures);
            var ledgerOk = Usable(ledger, "the ledger", failures);
            var taxOk = Usable(tax, "the tax computations", failures);
            var royaltiesOk = Usable(royalties, "the royalties and bills", failures);
            var warehouseOk = Usable(warehouse, "the warehouse", failures);

            // ---- the balances (AC-1: null when the records could not be read) ----
            var issuedInvoices = revenueOk
                ? revenue.Value!.Invoices.Where(invoice => invoice.IsIssued).ToList()
                : [];

            var openAp = payablesOk
                ? payables.Value!.Invoices.Where(invoice => !invoice.IsPaid).Sum(invoice => invoice.Balance)
                : (decimal?)null;

            var openAr = revenueOk
                ? issuedInvoices.Sum(invoice => invoice.BalanceDue)
                : (decimal?)null;

            decimal? overdue = null;

            if (revenueOk && payablesOk)
            {
                var overdueAr = issuedInvoices
                    .Where(invoice => DashboardRules.IsOverdue(invoice.BalanceDue, invoice.DueDate, today))
                    .Sum(invoice => invoice.BalanceDue);

                var overdueAp = payables.Value!.Invoices
                    .Where(invoice => !invoice.IsPaid && DashboardRules.IsOverdue(invoice.Balance, invoice.DueDate, today))
                    .Sum(invoice => invoice.Balance);

                overdue = overdueAr + overdueAp;
            }

            var unpostedJournals = ledgerOk
                ? ledger.Value!.Journals.Count(journal => journal.IsDraft)
                : (int?)null;

            // ---- the period strip (AC-3) ----
            var period = BuildPeriod(context, ledgerOk ? ledger.Value : null, failures);

            // ---- the cash position: opening from the chart plus the posted movement ----
            var cash = ledgerOk
                ? await ReadCashAsync(context, ledger.Value!, failures, cancellationToken).ConfigureAwait(false)
                : (null, null, null);

            var kpis = new OgAccountingKpis(
                openAp,
                openAr,
                overdue,
                unpostedJournals,
                period.State,
                cash.Position,
                context.Currency,
                cash.Opening,
                cash.Movement,
                failures.Distinct(StringComparer.Ordinal).ToList());

            var queues = BuildQueues(context, revenue, revenueOk, receivable, receivableOk, payables, payablesOk,
                ledger, ledgerOk, tax, taxOk, royalties, royaltiesOk, warehouse, warehouseOk);

            var attention = BuildAttention(revenueOk ? issuedInvoices : [], payables, payablesOk, revenueOk,
                receivable, receivableOk, tax, taxOk, royalties, royaltiesOk, today);

            _ = ledger;
            _ = warehouse;

            return OgResult<OgAccountingDashboard>.Ok(new OgAccountingDashboard(
                kpis,
                queues,
                attention,
                period,
                refreshedAt,
                false,
                failures.Distinct(StringComparer.Ordinal).ToList()));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            // A read that threw produced no records at all, so the dashboard reports the failure and no balances (AC-1).
            var reference = _failures.ReportHandled(
                ex, "reading the accounting dashboard", "the dashboard is not shown; no balance is inferred",
                FailureSeverity.Error);

            return OgResult<OgAccountingDashboard>.Failed(
                "The accounting dashboard could not be read" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + "). " + failures[^1]),
                reference ?? DashboardRules.CodeReadFailed);
        }
    }

    // ------------------------------------------------------------------ the parts

    private static bool Usable<T>(OgResult<T> result, string what, List<string> failures)
    {
        if (result is null)
        {
            failures.Add(what + " were not read");

            return false;
        }

        if (!result.Success || result.Value is null)
        {
            failures.Add(what + " could not be read" +
                         (string.IsNullOrWhiteSpace(result.Message) ? string.Empty : ": " + result.Message));

            return false;
        }

        return !IsHistoryFailed(result.Value, what, failures);
    }

    private static bool IsHistoryFailed<T>(T value, string what, List<string> failures)
    {
        var property = typeof(T).GetProperty("HistoryFailed");

        if (property?.GetValue(value) is not bool failed || !failed)
        {
            return false;
        }

        var reason = typeof(T).GetProperty("HistoryReason")?.GetValue(value) as string;

        failures.Add(what + " could not be read" +
                     (string.IsNullOrWhiteSpace(reason) ? string.Empty : " (" + reason + ")"));

        return true;
    }

    private static OgDashboardPeriod BuildPeriod(
        OgDashboardContext context,
        OgLedgerWorkspace? ledger,
        List<string> failures)
    {
        // AC-3: the period is the session's, not the latest one in the database.
        if (!context.HasPeriod)
        {
            return new OgDashboardPeriod(string.Empty, string.Empty, "no period selected", 0, false,
                "the session has no period selected, so there is no current period to report");
        }

        var code = context.PeriodCode!;
        var name = string.IsNullOrWhiteSpace(context.PeriodName) ? code : context.PeriodName!;

        if (ledger is null)
        {
            return new OgDashboardPeriod(code, name, "unknown", 0, false,
                name + " (" + code + "): the ledger could not be read, so its state is unknown");
        }

        var period = ledger.Periods.FirstOrDefault(row =>
            string.Equals(row.Code, code, StringComparison.OrdinalIgnoreCase));

        if (period is null)
        {
            failures.Add("period " + code + " is not in the ledger, so its state is unknown");

            return new OgDashboardPeriod(code, name, "not opened", 0, false,
                name + " (" + code + "): no ledger period exists for it");
        }

        var blocking = ledger.Journals.Count(journal =>
            string.Equals(journal.PeriodCode, code, StringComparison.OrdinalIgnoreCase) && journal.IsDraft);

        var state = DashboardRules.PeriodStateOf(period.Status);

        return new OgDashboardPeriod(
            code,
            name,
            state,
            blocking,
            true,
            name + " (" + code + ") is " + state +
            (blocking == 0
                ? " · nothing blocks its close"
                : " · " + blocking.ToString(CultureInfo.InvariantCulture) + " draft journal(s) block its close"));
    }

    private async Task<(decimal? Position, decimal? Opening, decimal? Movement)> ReadCashAsync(
        OgDashboardContext context,
        OgLedgerWorkspace ledger,
        List<string> failures,
        CancellationToken cancellationToken)
    {
        var source = _source();

        if (source is null)
        {
            failures.Add("no connection is open, so the cash position is unknown");

            return (null, null, null);
        }

        try
        {
            var accounts = await OgUow.ReadAllAsync<GL_ACCOUNT>(source).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            var cash = accounts.FirstOrDefault(account =>
                string.Equals(account.ACCOUNT_NUMBER?.Trim(), DashboardRules.CashAccount, StringComparison.Ordinal));

            if (cash is null)
            {
                failures.Add("the chart has no cash account " + DashboardRules.CashAccount +
                             ", so the cash position is unknown");

                return (null, null, null);
            }

            var opening = cash.OPENING_BALANCE;

            // The posted movement on the cash account, across every period the ledger holds.
            var movement = ledger.Journals
                .Where(journal => journal.IsPosted
                                  && !string.Equals(journal.Status, LedgerCodes.JournalReversed,
                                      StringComparison.OrdinalIgnoreCase))
                .SelectMany(journal => journal.Lines)
                .Where(line => string.Equals(line.AccountNumber?.Trim(), DashboardRules.CashAccount,
                    StringComparison.OrdinalIgnoreCase))
                .Sum(line => line.Debit - line.Credit);

            var position = (opening ?? 0m) + movement;

            _ = context;

            return (Math.Round(position, 2, MidpointRounding.AwayFromZero),
                opening is null ? null : Math.Round(opening.Value, 2, MidpointRounding.AwayFromZero),
                Math.Round(movement, 2, MidpointRounding.AwayFromZero));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading the cash account", "the cash position is unknown", FailureSeverity.Degraded);

            failures.Add("the cash position could not be read (reference " + reference + ")");

            return (null, null, null);
        }
    }

    private static IReadOnlyList<OgDashboardQueue> BuildQueues(
        OgDashboardContext context,
        OgResult<OgRevenueWorkspace> revenue,
        bool revenueOk,
        OgResult<OgReceivableWorkspace> receivable,
        bool receivableOk,
        OgResult<OgPayablesWorkspace> payables,
        bool payablesOk,
        OgResult<OgLedgerWorkspace> ledger,
        bool ledgerOk,
        OgResult<OgTaxWorkspace> tax,
        bool taxOk,
        OgResult<OgRoyaltyWorkspace> royalties,
        bool royaltiesOk,
        OgResult<OgWarehouseWorkspace> warehouse,
        bool warehouseOk)
    {
        var queues = new List<OgDashboardQueue>();

        void Add(string key, string title, string permission, int? count, bool usable, string detail)
        {
            var (shown, reason) = DashboardRules.Access(context.Permissions, permission, title);

            // A queue whose records could not be read says so rather than showing an empty panel as if there were none.
            var withheld = shown ? null : reason ?? "withheld";
            var state = shown && !usable ? "the records could not be read" : detail;

            queues.Add(new OgDashboardQueue(
                key,
                title,
                permission,
                shown ? count : null,
                withheld,
                state));
        }

        Add("sales-draft", "Sales invoices awaiting issue", "accounting.revenue.write",
            revenueOk ? revenue.Value!.Invoices.Count(invoice => invoice.IsDraft) : null, revenueOk,
            "draft invoices are raised but not yet issued");

        Add("sales-review", "Sales invoices awaiting approval", "accounting.revenue.write",
            revenueOk
                ? revenue.Value!.Invoices.Count(invoice => invoice.IsIssued && !invoice.IsApproved)
                : null,
            revenueOk,
            "issued invoices have not been approved by a controller");

        Add("ap-open", "Supplier invoices open", "accounting.payable.write",
            payablesOk ? payables.Value!.Invoices.Count(invoice => !invoice.IsPaid) : null, payablesOk,
            "captured supplier invoices still owe money");

        Add("ap-exceptions", "Match exceptions awaiting approval", "accounting.payable.approve",
            payablesOk ? payables.Value!.Invoices.Count(invoice => invoice.NeedsExceptionApproval) : null, payablesOk,
            "invoices whose three-way match found an exception need a second person");

        Add("po-draft", "Purchase orders awaiting approval", "accounting.payable.write",
            payablesOk ? payables.Value!.Orders.Count(order => order.IsDraft) : null, payablesOk,
            "orders are raised and waiting for a buyer to approve them");

        Add("receipts-draft", "Receipts awaiting posting", "accounting.receivable.write",
            receivableOk ? receivable.Value!.Receipts.Count(receipt => receipt.IsDraft) : null, receivableOk,
            "cash has been recorded and not yet posted");

        Add("unapplied-cash", "Receipts with unapplied cash", "accounting.receivable.write",
            receivableOk ? receivable.Value!.Receipts.Count(receipt => receipt.Unapplied > 0m) : null, receivableOk,
            "cash is held against a customer and not yet applied to an invoice");

        Add("journals-draft", "Journals not posted", "accounting.revenue.write",
            ledgerOk ? ledger.Value!.Journals.Count(journal => journal.IsDraft) : null, ledgerOk,
            "draft journals are written and not in the ledger");

        Add("tax-unposted", "Tax computations not posted", "accounting.tax.post",
            taxOk ? tax.Value!.Computations.Count(computation => !computation.IsPosted) : null, taxOk,
            "computed tax has not reached the ledger");

        Add("jib-unposted", "Joint-interest bills not posted", "accounting.royalty.pay",
            royaltiesOk ? royalties.Value!.Jibs.Count(jib => !jib.IsPosted) : null, royaltiesOk,
            "partner bills are raised and not posted");

        Add("royalties-payable", "Royalties payable", "accounting.royalty.pay",
            royaltiesOk ? royalties.Value!.Royalties.Count(royalty => royalty.CanPay) : null, royaltiesOk,
            "royalties are accrued, undisputed and waiting to be paid");

        Add("stock-holds", "Stock reservations open", "accounting.stock.write",
            warehouseOk
                ? warehouse.Value!.Reservations.Count(reservation => reservation.HoldsStock)
                : null,
            warehouseOk,
            "stock is held for a work order or cost centre");

        return queues;
    }

    private static IReadOnlyList<OgDashboardAttention> BuildAttention(
        IReadOnlyList<OgSalesInvoice> issuedInvoices,
        OgResult<OgPayablesWorkspace> payables,
        bool payablesOk,
        bool revenueOk,
        OgResult<OgReceivableWorkspace> receivable,
        bool receivableOk,
        OgResult<OgTaxWorkspace> tax,
        bool taxOk,
        OgResult<OgRoyaltyWorkspace> royalties,
        bool royaltiesOk,
        DateTime today)
    {
        var attention = new List<OgDashboardAttention>();

        if (revenueOk)
        {
            foreach (var invoice in issuedInvoices
                         .Where(invoice => DashboardRules.IsOverdue(invoice.BalanceDue, invoice.DueDate, today))
                         .OrderBy(invoice => invoice.DueDate))
            {
                attention.Add(new OgDashboardAttention(
                    "sales invoice past due",
                    invoice.Number,
                    invoice.BuyerId ?? "(no buyer)",
                    invoice.BalanceDue,
                    invoice.DueDate));
            }
        }

        if (payablesOk)
        {
            foreach (var invoice in payables.Value!.Invoices
                         .Where(invoice => !invoice.IsPaid && DashboardRules.IsOverdue(invoice.Balance, invoice.DueDate, today))
                         .OrderBy(invoice => invoice.DueDate))
            {
                attention.Add(new OgDashboardAttention(
                    "supplier invoice past due",
                    invoice.Number,
                    invoice.VendorId,
                    invoice.Balance,
                    invoice.DueDate));
            }

            foreach (var invoice in payables.Value.Invoices.Where(invoice => invoice.NeedsExceptionApproval))
            {
                attention.Add(new OgDashboardAttention(
                    "match exception",
                    invoice.Number,
                    invoice.VendorId,
                    invoice.Total,
                    invoice.DueDate));
            }
        }

        if (receivableOk)
        {
            foreach (var receipt in receivable.Value!.Receipts.Where(receipt => receipt.Unapplied > 0m))
            {
                attention.Add(new OgDashboardAttention(
                    "unapplied cash",
                    receipt.Number,
                    receipt.CustomerId,
                    receipt.Unapplied,
                    receipt.Date));
            }
        }

        if (taxOk)
        {
            foreach (var computation in tax.Value!.Computations.Where(computation => !computation.IsPosted))
            {
                attention.Add(new OgDashboardAttention(
                    "tax not posted",
                    computation.Describe(),
                    computation.Jurisdiction,
                    computation.Amount,
                    computation.Date));
            }
        }

        if (royaltiesOk)
        {
            foreach (var royalty in royalties.Value!.Royalties.Where(royalty => royalty.IsDisputed))
            {
                attention.Add(new OgDashboardAttention(
                    "royalty disputed",
                    royalty.LeaseId,
                    royalty.DisputeReference ?? "(no reference)",
                    royalty.Amount,
                    null));
            }
        }

        return attention
            .OrderByDescending(item => item.Amount ?? 0m)
            .ThenBy(item => item.Kind, StringComparer.Ordinal)
            .Take(DashboardRules.AttentionLimit)
            .ToList();
    }
}
