using System.Globalization;
using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Accounting;

/// <summary>Who and where a ledger entry belongs (ACC-02): the company, the book and the currency.</summary>
public sealed record OgLedgerContext(string CompanyId, string CompanyName, string BookCode, string Currency)
{
    public static OgLedgerContext FromSession(OgSession session) =>
        new(
            string.IsNullOrWhiteSpace(session.CompanyId) ? "APP" : session.CompanyId,
            session.CompanyName ?? string.Empty,
            string.IsNullOrWhiteSpace(session.BookCode) ? "MAIN" : session.BookCode,
            string.IsNullOrWhiteSpace(session.BaseCurrencyCode) ? "USD" : session.BaseCurrencyCode);
}

/// <summary>One line of a journal as the engineer enters it (ACC-02).</summary>
public sealed record OgJournalLineInput(string? AccountNumber, decimal? Debit, decimal? Credit, string? Description)
{
    public decimal DebitAmount => Debit ?? 0m;

    public decimal CreditAmount => Credit ?? 0m;
}

/// <summary>The inputs of one journal (ACC-02).</summary>
public sealed record OgJournalInputs(
    DateTime? Date,
    string? Type,
    string? Source,
    string? Reference,
    string? Description,
    IReadOnlyList<OgJournalLineInput> Lines);

/// <summary>One posted or draft line, with the account it hits (ACC-02, AC-7).</summary>
public sealed record OgJournalLine(
    string LineId,
    string JournalId,
    string AccountNumber,
    string AccountName,
    int LineNumber,
    decimal Debit,
    decimal Credit,
    string? Description)
{
    public string Describe() =>
        $"{AccountNumber} {AccountName} · debit {Money(Debit)} · credit {Money(Credit)}" +
        (string.IsNullOrWhiteSpace(Description) ? string.Empty : " · " + Description);

    private static string Money(decimal value) => MoneyText.Of(value);
}

/// <summary>One journal (ACC-02), matching the plan's <c>OgJournal</c> plus the posting audit.</summary>
public sealed record OgJournal(
    string Id,
    string Number,
    DateTime Date,
    string Type,
    string Source,
    string? Reference,
    string? Description,
    string Status,
    decimal TotalDebit,
    decimal TotalCredit,
    string? Author,
    string? PostedBy,
    DateTime? PostedAt,
    string PeriodCode,
    string? ReversalOf,
    IReadOnlyList<OgJournalLine> Lines)
{
    /// <summary>AC-1: the exact amount by which the two sides differ; zero when it balances.</summary>
    public decimal Imbalance => LedgerCodes.Imbalance(TotalDebit, TotalCredit);

    public bool Balances => Imbalance == 0m;

    public bool IsDraft => LedgerCodes.IsDraft(Status);

    public bool IsPosted => LedgerCodes.IsPosted(Status) ||
                            string.Equals(Status, LedgerCodes.JournalReversed, StringComparison.OrdinalIgnoreCase);

    /// <summary>A posted journal that has not itself been reversed - the only state a counter-entry may follow.</summary>
    public bool IsReversible => string.Equals(Status, LedgerCodes.JournalPosted, StringComparison.OrdinalIgnoreCase);

    public string AmountText => MoneyText.Of(TotalDebit) + " / " + MoneyText.Of(TotalCredit);

    public string BalanceText => Balances
        ? "balanced"
        : "UNBALANCED by " + MoneyText.Of(Imbalance);

    public string PostingText => IsPosted
        ? "posted by " + (PostedBy ?? "(unknown)") + " on " +
          (PostedAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "an unrecorded date")
        : Status.ToLowerInvariant() + " · not posted";

    public string Describe() =>
        $"{Number} · {Date:yyyy-MM-dd} · {Type} · {Source} · {AmountText} · {Status} · {BalanceText}";
}

/// <summary>One trial-balance line, with the journal lines behind it (AC-5, AC-7).</summary>
public sealed record OgTrialBalanceRow(
    string AccountNumber,
    string AccountName,
    decimal Debit,
    decimal Credit,
    IReadOnlyList<OgJournalLine> Lines)
{
    public decimal Balance => Debit - Credit;

    public string Describe() =>
        $"{AccountNumber} {AccountName} · debit {MoneyText.Of(Debit)} · credit {MoneyText.Of(Credit)} · " +
        $"balance {MoneyText.Of(Balance)} · {Lines.Count.ToString(CultureInfo.InvariantCulture)} line(s)";
}

/// <summary>One period and its lock (ACC-02, AC-3, AC-6).</summary>
public sealed record OgPeriod(
    string Id,
    string Code,
    DateTime Start,
    DateTime End,
    string Status,
    string? ClosedBy,
    DateTime? ClosedAt,
    string? ReopenedBy,
    DateTime? ReopenedAt,
    string? ReopenReason,
    string? Note)
{
    public bool IsOpen => LedgerCodes.IsOpen(Status);

    public string StateText => Status.ToLowerInvariant() + (IsOpen ? " - accepts journals" : " - no journal may be written");

    public string CloseText => ClosedAt is { } at
        ? "closed by " + (ClosedBy ?? "(unknown)") + " on " +
          at.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
        : "never closed";

    public string ReopenText => ReopenedAt is { } at
        ? "reopened by " + (ReopenedBy ?? "(unknown)") + " on " +
          at.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) + " because " +
          (string.IsNullOrWhiteSpace(ReopenReason) ? "(no reason recorded)" : ReopenReason)
        : "never reopened";

    public string Describe() =>
        $"{Code} · {Start:yyyy-MM-dd} to {End:yyyy-MM-dd} · {StateText} · {CloseText}" +
        (ReopenedAt is null ? string.Empty : " · " + ReopenText);
}

/// <summary>The ledger workspace (ACC-02).</summary>
public sealed record OgLedgerWorkspace(
    IReadOnlyList<OgJournal> Journals,
    OgJournal? Selected,
    IReadOnlyList<OgPeriod> Periods,
    OgPeriod? SelectedPeriod,
    IReadOnlyList<OgTrialBalanceRow> TrialBalance,
    string PeriodCode,
    decimal TrialDebit,
    decimal TrialCredit,
    IReadOnlyList<string> AccountNumbers,
    IReadOnlyList<string> JournalTypes,
    bool HistoryFailed,
    string? HistoryReason)
{
    public string JournalsText => Journals.Count == 0
        ? "No journal exists yet."
        : string.Join(Environment.NewLine, Journals.Select(journal => journal.Describe()));

    public string HistoryStateText => HistoryFailed
        ? "the ledger could not be read; no posting is possible"
        : $"{Journals.Count.ToString(CultureInfo.InvariantCulture)} journal(s) · " +
          $"{Journals.Count(journal => journal.IsDraft).ToString(CultureInfo.InvariantCulture)} draft · " +
          $"{Journals.Count(journal => journal.IsPosted).ToString(CultureInfo.InvariantCulture)} posted";

    /// <summary>AC-5: the trial balance totals, and whether they agree for the period shown.</summary>
    public string TrialBalanceText => TrialDebit == TrialCredit
        ? $"{PeriodCode}: debit {MoneyText.Of(TrialDebit)} equals credit {MoneyText.Of(TrialCredit)}"
        : $"{PeriodCode}: debit {MoneyText.Of(TrialDebit)} does NOT equal credit {MoneyText.Of(TrialCredit)} " +
          $"by {MoneyText.Of(TrialDebit - TrialCredit)}";

    public bool TrialBalances => TrialDebit == TrialCredit;

    public string TrialRowsText => TrialBalance.Count == 0
        ? "no posted line in this period"
        : string.Join(Environment.NewLine, TrialBalance.Select(row => row.Describe()));

    public string PeriodText => SelectedPeriod?.Describe() ?? "no period is selected";
}

/// <summary>The ledger rules, stated once so the page, the service and the tests agree.</summary>
public static class LedgerRules
{
    public const string CodeNoConnection = "ledger-no-connection";
    public const string CodeReadFailed = "ledger-read-failed";
    public const string CodeWriteFailed = "ledger-write-failed";
    public const string CodeActorRequired = "ledger-actor-required";
    public const string CodeInputRequired = "ledger-input-required";
    public const string CodeInputInvalid = "ledger-input-invalid";
    public const string CodeUnbalanced = "ledger-unbalanced";
    public const string CodeDuplicateReference = "ledger-duplicate-reference";
    public const string CodePeriodClosed = "ledger-period-closed";
    public const string CodeUnknownJournal = "ledger-unknown-journal";
    public const string CodeUnknownPeriod = "ledger-unknown-period";
    public const string CodeAlreadyPosted = "ledger-already-posted";
    public const string CodeAuthorCannotPost = "ledger-author-cannot-post";
    public const string CodePeriodNotBalanced = "ledger-period-not-balanced";
    public const string CodeDraftsOpen = "ledger-drafts-open";
    public const string CodeReasonRequired = "ledger-reason-required";

    public static IReadOnlyList<string> JournalTypes { get; } = ["GENERAL", "ACCRUAL", "REVERSAL", "ADJUSTMENT"];

    /// <summary>The lines a journal needs before it can be checked.</summary>
    public static IReadOnlyList<string> Missing(OgJournalInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var missing = new List<string>();

        if (inputs.Date is null)
        {
            missing.Add("date");
        }

        if (string.IsNullOrWhiteSpace(inputs.Type))
        {
            missing.Add("type");
        }

        if (string.IsNullOrWhiteSpace(inputs.Source))
        {
            missing.Add("source");
        }

        if (inputs.Lines is null || inputs.Lines.Count == 0)
        {
            missing.Add("at least one line");
        }

        return missing;
    }

    /// <summary>
    /// AC-1 and SG-10-1: the journal must balance, and the refusal names the exact imbalance rather than "invalid".
    /// </summary>
    public static (string Code, string Message)? CheckBalance(decimal totalDebit, decimal totalCredit)
    {
        var imbalance = LedgerCodes.Imbalance(totalDebit, totalCredit);

        if (imbalance == 0m)
        {
            return null;
        }

        var direction = imbalance > 0m ? "debit" : "credit";

        return (CodeUnbalanced,
            "The journal does not balance: debit " + MoneyText.Of(totalDebit) + " against credit " +
            MoneyText.Of(totalCredit) + ", a " + MoneyText.Of(Math.Abs(imbalance)) + " excess on the " + direction +
            " side. Post it only when the two sides agree exactly.");
    }
}

/// <summary>ACC-02: the ledger - journals, posting, trial balance and the period close.</summary>
public interface IOgLedger
{
    /// <summary>Creates a draft journal, refusing an imbalance or a closed period.</summary>
    Task<OgResult<OgLedgerWorkspace>> CreateJournalAsync(
        OgJournalInputs inputs,
        OgLedgerContext context,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Posts a draft, refusing the author's own journal and a second posting (AC-2, AC-4).</summary>
    Task<OgResult<OgLedgerWorkspace>> PostAsync(
        string journalId,
        OgLedgerContext context,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Reverses a posted journal with a counter-entry.</summary>
    Task<OgResult<OgLedgerWorkspace>> ReverseAsync(
        string journalId,
        OgLedgerContext context,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Closes a period whose trial balance agrees and which has no draft journal.</summary>
    Task<OgResult<OgLedgerWorkspace>> ClosePeriodAsync(
        string periodCode,
        OgLedgerContext context,
        string note,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Reopens a closed period, recording the actor, the reason and the time (AC-6).</summary>
    Task<OgResult<OgLedgerWorkspace>> ReopenPeriodAsync(
        string periodCode,
        OgLedgerContext context,
        string reason,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>The ledger, its periods and the trial balance for a period (AC-5, AC-7).</summary>
    Task<OgResult<OgLedgerWorkspace>> LoadAsync(
        OgLedgerContext context,
        string? journalId = null,
        string? periodCode = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// ACC-02 over ProductionAccounting's <c>GL_ACCOUNT</c>, <c>JOURNAL_ENTRY</c> and <c>JOURNAL_ENTRY_LINE</c> and the
/// accounting project's <c>COMPANY_LEDGER_PERIOD</c>, through units of work.
/// </summary>
/// <remarks>
/// All money is <c>decimal</c> (the stage's rule). A journal must balance to the last place before it is written, posting
/// is a status transition on the row that already exists - so a second posting cannot create a second entry - and every
/// write checks the period's lock first, naming the period when it refuses. The lock is the accounting
/// <c>COMPANY_LEDGER_PERIOD</c> row, which its own <c>PeriodClosingService</c> documents as the owner of the company/book
/// period records; the production project's period close is a readiness close, not the ledger lock.
/// </remarks>
public sealed class OgLedgerService : IOgLedger
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgLedgerService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgLedgerWorkspace>> CreateJournalAsync(
        OgJournalInputs inputs,
        OgLedgerContext context,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgLedgerWorkspace>.Failed(
                "A journal must record who wrote it.", LedgerRules.CodeActorRequired);
        }

        if (LedgerRules.Missing(inputs) is { Count: > 0 } missing)
        {
            return OgResult<OgLedgerWorkspace>.Failed(
                $"The journal needs {string.Join(", ", missing)}.", LedgerRules.CodeInputRequired);
        }

        var totalDebit = inputs.Lines.Sum(line => line.DebitAmount);
        var totalCredit = inputs.Lines.Sum(line => line.CreditAmount);

        // AC-1 and SG-10-1: the imbalance is computed before anything is read, and it is the refusal's whole point.
        if (LedgerRules.CheckBalance(totalDebit, totalCredit) is { } unbalanced)
        {
            return OgResult<OgLedgerWorkspace>.Failed(unbalanced.Message, unbalanced.Code);
        }

        if (inputs.Lines.Any(line => line.DebitAmount < 0m || line.CreditAmount < 0m))
        {
            return OgResult<OgLedgerWorkspace>.Failed(
                "A line's debit and credit cannot be negative; use the other side.", LedgerRules.CodeInputInvalid);
        }

        if (inputs.Lines.Any(line => line.DebitAmount > 0m && line.CreditAmount > 0m))
        {
            return OgResult<OgLedgerWorkspace>.Failed(
                "A line carries either a debit or a credit, not both.", LedgerRules.CodeInputInvalid);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgLedgerWorkspace>.Failed(
                "No connection is open, so the journal cannot be written.", LedgerRules.CodeNoConnection);
        }

        try
        {
            var accounts = await OgUow.ReadAllAsync<GL_ACCOUNT>(source).ConfigureAwait(false);
            var periodCode = LedgerCodes.PeriodCodeOf(inputs.Date!.Value);
            var periods = await EnsurePeriodAsync(source, context, periodCode, inputs.Date.Value, actor)
                .ConfigureAwait(false);

            var period = periods.FirstOrDefault(candidate =>
                string.Equals(candidate.PERIOD_CODE, periodCode, StringComparison.OrdinalIgnoreCase));

            // AC-3 and SG-10-3: the lock is checked before any write, and the refusal names the period.
            if (period is not null && !LedgerCodes.IsOpen(period.STATUS))
            {
                return OgResult<OgLedgerWorkspace>.Failed(
                    $"Period {periodCode} is {period.STATUS.ToLowerInvariant()}, so no journal may be written into it. " +
                    "Reopen the period first.",
                    LedgerRules.CodePeriodClosed);
            }

            // AC-2's other half: the same source and reference must not produce a second entry.
            if (!string.IsNullOrWhiteSpace(inputs.Reference))
            {
                var existing = await OgUow.ReadAllAsync<JOURNAL_ENTRY>(source).ConfigureAwait(false);

                var duplicate = existing.FirstOrDefault(entry =>
                    string.Equals(entry.SOURCE_MODULE, inputs.Source!.Trim(), StringComparison.OrdinalIgnoreCase)
                    && string.Equals(entry.REFERENCE_NUMBER, inputs.Reference!.Trim(), StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(entry.STATUS, LedgerCodes.JournalReversed, StringComparison.OrdinalIgnoreCase));

                if (duplicate is not null)
                {
                    return OgResult<OgLedgerWorkspace>.Failed(
                        $"{inputs.Source} reference '{inputs.Reference}' is already journal {duplicate.ENTRY_NUMBER} " +
                        $"({duplicate.STATUS?.ToLowerInvariant()}); a duplicate posting is not created.",
                        LedgerRules.CodeDuplicateReference);
                }
            }

            var resolved = new List<(OgJournalLineInput Input, GL_ACCOUNT Account)>();
            var order = 1;

            foreach (var line in inputs.Lines)
            {
                var account = accounts.FirstOrDefault(candidate =>
                    string.Equals(candidate.ACCOUNT_NUMBER?.Trim(), line.AccountNumber?.Trim(),
                        StringComparison.OrdinalIgnoreCase));

                if (account is null)
                {
                    return OgResult<OgLedgerWorkspace>.Failed(
                        $"'{line.AccountNumber}' is not a ledger account.", LedgerRules.CodeInputInvalid);
                }

                resolved.Add((line, account));
                order++;
            }

            var now = DateTime.UtcNow;
            var journalId = Guid.NewGuid().ToString("N");
            var number = NextNumber(
                await OgUow.ReadAllAsync<JOURNAL_ENTRY>(source).ConfigureAwait(false), inputs.Date.Value);

            var unit = OgUow.For<JOURNAL_ENTRY>(source);
            unit.PrimaryKey = "JOURNAL_ENTRY_ID";

            unit.Add(new JOURNAL_ENTRY
            {
                JOURNAL_ENTRY_ID = journalId,
                ENTRY_NUMBER = number,
                ENTRY_DATE = inputs.Date.Value.Date,
                ENTRY_TYPE = inputs.Type!.Trim().ToUpperInvariant(),
                STATUS = LedgerCodes.JournalDraft,
                DESCRIPTION = inputs.Description,
                REFERENCE_NUMBER = string.IsNullOrWhiteSpace(inputs.Reference) ? null : inputs.Reference.Trim(),
                SOURCE_MODULE = inputs.Source!.Trim(),
                TOTAL_DEBIT = totalDebit,
                TOTAL_CREDIT = totalCredit,
                AUTHOR = actor.Trim(),
                PERIOD_CODE = periodCode,
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
                return OgResult<OgLedgerWorkspace>.Failed(
                    OgUow.Message(committed, "The journal was not saved."), LedgerRules.CodeWriteFailed);
            }

            if (!await WriteLinesAsync(source, journalId, resolved, actor, cancellationToken).ConfigureAwait(false))
            {
                return OgResult<OgLedgerWorkspace>.Failed(
                    "The journal's lines were not saved, so the entry was withdrawn; nothing was written.",
                    LedgerRules.CodeWriteFailed);
            }

            var workspace = await ReadAsync(source, context, journalId, periodCode, cancellationToken)
                .ConfigureAwait(false);

            return OgResult<OgLedgerWorkspace>.Ok(workspace);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "writing a journal",
                "the journal was not written",
                FailureSeverity.Error);

            return OgResult<OgLedgerWorkspace>.Failed(
                "The journal could not be written.", reference ?? LedgerRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgLedgerWorkspace>> PostAsync(
        string journalId,
        OgLedgerContext context,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgLedgerWorkspace>.Failed(
                "A posting must record who made it.", LedgerRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgLedgerWorkspace>.Failed(
                "No connection is open, so the journal cannot be posted.", LedgerRules.CodeNoConnection);
        }

        try
        {
            var journal = await ReadJournalAsync(source, journalId, cancellationToken).ConfigureAwait(false);

            if (journal is null)
            {
                return OgResult<OgLedgerWorkspace>.Failed(
                    $"Journal '{journalId}' is not in this database.", LedgerRules.CodeUnknownJournal);
            }

            // AC-2 and SG-10-2: posting is a transition of the row that exists, so a second attempt cannot create a
            // second entry - and it says so rather than pretending to post again.
            if (journal.IsPosted)
            {
                return OgResult<OgLedgerWorkspace>.Failed(
                    $"{journal.Number} is already {journal.Status.ToLowerInvariant()}; a duplicate posting creates no " +
                    "second entry.",
                    LedgerRules.CodeAlreadyPosted);
            }

            // AC-4 and SG-10-4: the author cannot post their own journal.
            if (!string.IsNullOrWhiteSpace(journal.Author)
                && string.Equals(journal.Author.Trim(), actor.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgLedgerWorkspace>.Failed(
                    $"{journal.Number} was written by {journal.Author}, who therefore cannot post it. A different " +
                    "controller must post it.",
                    LedgerRules.CodeAuthorCannotPost);
            }

            // AC-3: the period might have been closed between the draft and the posting.
            var periods = await OgUow.ReadAllAsync<COMPANY_LEDGER_PERIOD>(source).ConfigureAwait(false);

            var period = periods.FirstOrDefault(row =>
                string.Equals(row.PERIOD_CODE, journal.PeriodCode, StringComparison.OrdinalIgnoreCase));

            if (period is not null && !LedgerCodes.IsOpen(period.STATUS))
            {
                return OgResult<OgLedgerWorkspace>.Failed(
                    $"Period {journal.PeriodCode} is {period.STATUS.ToLowerInvariant()}, so {journal.Number} cannot be " +
                    "posted into it. Reopen the period first.",
                    LedgerRules.CodePeriodClosed);
            }

            if (!journal.Balances)
            {
                return OgResult<OgLedgerWorkspace>.Failed(
                    $"{journal.Number} does not balance by {MoneyText.Of(journal.Imbalance)} and cannot be posted.",
                    LedgerRules.CodeUnbalanced);
            }

            var row = await ReadEntryRowAsync(source, journal.Id, cancellationToken).ConfigureAwait(false);

            if (row is null)
            {
                return OgResult<OgLedgerWorkspace>.Failed(
                    $"Journal '{journalId}' is not in this database.", LedgerRules.CodeUnknownJournal);
            }

            row.STATUS = LedgerCodes.JournalPosted;
            row.POSTED_BY = actor.Trim();
            row.POSTED_AT = DateTime.UtcNow;
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = DateTime.UtcNow;

            var written = await UpdateAsync(source, row).ConfigureAwait(false);

            if (written is not null)
            {
                return OgResult<OgLedgerWorkspace>.Failed(written, LedgerRules.CodeWriteFailed);
            }

            var workspace = await ReadAsync(source, context, journal.Id, journal.PeriodCode, cancellationToken)
                .ConfigureAwait(false);

            return OgResult<OgLedgerWorkspace>.Ok(workspace);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"posting journal {journalId}",
                "the journal was not posted",
                FailureSeverity.Error);

            return OgResult<OgLedgerWorkspace>.Failed(
                "The journal could not be posted.", reference ?? LedgerRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgLedgerWorkspace>> ReverseAsync(
        string journalId,
        OgLedgerContext context,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgLedgerWorkspace>.Failed(
                "A reversal must record who made it.", LedgerRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgLedgerWorkspace>.Failed(
                "No connection is open, so the journal cannot be reversed.", LedgerRules.CodeNoConnection);
        }

        try
        {
            var journal = await ReadJournalAsync(source, journalId, cancellationToken).ConfigureAwait(false);

            if (journal is null)
            {
                return OgResult<OgLedgerWorkspace>.Failed(
                    $"Journal '{journalId}' is not in this database.", LedgerRules.CodeUnknownJournal);
            }

            if (!journal.IsPosted || string.Equals(journal.Status, LedgerCodes.JournalReversed,
                    StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgLedgerWorkspace>.Failed(
                    $"{journal.Number} is {journal.Status.ToLowerInvariant()}; only a posted journal can be reversed.",
                    LedgerRules.CodeInputInvalid);
            }

            var periods = await OgUow.ReadAllAsync<COMPANY_LEDGER_PERIOD>(source).ConfigureAwait(false);

            var period = periods.FirstOrDefault(row =>
                string.Equals(row.PERIOD_CODE, journal.PeriodCode, StringComparison.OrdinalIgnoreCase));

            if (period is not null && !LedgerCodes.IsOpen(period.STATUS))
            {
                return OgResult<OgLedgerWorkspace>.Failed(
                    $"Period {journal.PeriodCode} is {period.STATUS.ToLowerInvariant()}, so {journal.Number} cannot be " +
                    "reversed into it.",
                    LedgerRules.CodePeriodClosed);
            }

            var now = DateTime.UtcNow;
            var reversalId = Guid.NewGuid().ToString("N");
            var number = NextNumber(
                await OgUow.ReadAllAsync<JOURNAL_ENTRY>(source).ConfigureAwait(false), journal.Date);

            var unit = OgUow.For<JOURNAL_ENTRY>(source);
            unit.PrimaryKey = "JOURNAL_ENTRY_ID";

            unit.Add(new JOURNAL_ENTRY
            {
                JOURNAL_ENTRY_ID = reversalId,
                ENTRY_NUMBER = number,
                ENTRY_DATE = journal.Date,
                ENTRY_TYPE = "REVERSAL",
                STATUS = LedgerCodes.JournalPosted,
                DESCRIPTION = "Reversal of " + journal.Number,
                REFERENCE_NUMBER = "REV-" + journal.Number,
                SOURCE_MODULE = journal.Source,
                TOTAL_DEBIT = journal.TotalCredit,
                TOTAL_CREDIT = journal.TotalDebit,
                AUTHOR = actor.Trim(),
                POSTED_BY = actor.Trim(),
                POSTED_AT = now,
                PERIOD_CODE = journal.PeriodCode,
                REVERSAL_OF_ENTRY_ID = journal.Id,
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
                return OgResult<OgLedgerWorkspace>.Failed(
                    OgUow.Message(committed, "The reversal was not saved."), LedgerRules.CodeWriteFailed);
            }

            // The counter-entry swaps the sides, so the reversal balances by construction.
            var reversalLines = journal.Lines
                .Select(line => (Input: new OgJournalLineInput(line.AccountNumber, line.Credit, line.Debit,
                    "reverses " + line.Describe()), Account: (GL_ACCOUNT?)null))
                .ToList();

            var accounts = await OgUow.ReadAllAsync<GL_ACCOUNT>(source).ConfigureAwait(false);

            if (!await WriteLinesAsync(
                    source,
                    reversalId,
                    reversalLines
                        .Select(item => (item.Input, accounts.First(account => string.Equals(
                            account.ACCOUNT_NUMBER, item.Input.AccountNumber, StringComparison.OrdinalIgnoreCase))))
                        .ToList(),
                    actor,
                    cancellationToken).ConfigureAwait(false))
            {
                return OgResult<OgLedgerWorkspace>.Failed(
                    "The reversal's lines were not saved.", LedgerRules.CodeWriteFailed);
            }

            var original = await ReadEntryRowAsync(source, journal.Id, cancellationToken).ConfigureAwait(false);

            if (original is not null)
            {
                original.STATUS = LedgerCodes.JournalReversed;
                original.REVERSED_AT = now;
                original.ROW_CHANGED_BY = actor.Trim();
                original.ROW_CHANGED_DATE = now;

                await UpdateAsync(source, original).ConfigureAwait(false);
            }

            var workspace = await ReadAsync(source, context, reversalId, journal.PeriodCode, cancellationToken)
                .ConfigureAwait(false);

            return OgResult<OgLedgerWorkspace>.Ok(workspace);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"reversing journal {journalId}",
                "the journal was not reversed",
                FailureSeverity.Error);

            return OgResult<OgLedgerWorkspace>.Failed(
                "The journal could not be reversed.", reference ?? LedgerRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgLedgerWorkspace>> ClosePeriodAsync(
        string periodCode,
        OgLedgerContext context,
        string note,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgLedgerWorkspace>.Failed(
                "A close must record who made it.", LedgerRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgLedgerWorkspace>.Failed(
                "No connection is open, so the period cannot be closed.", LedgerRules.CodeNoConnection);
        }

        try
        {
            var periods = await OgUow.ReadAllAsync<COMPANY_LEDGER_PERIOD>(source).ConfigureAwait(false);

            var period = periods.FirstOrDefault(row =>
                string.Equals(row.PERIOD_CODE, periodCode?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (period is null)
            {
                return OgResult<OgLedgerWorkspace>.Failed(
                    $"Period '{periodCode}' does not exist, so it cannot be closed.",
                    LedgerRules.CodeUnknownPeriod);
            }

            var journals = await ReadJournalsAsync(source, cancellationToken).ConfigureAwait(false);

            var drafts = journals.Where(journal =>
                string.Equals(journal.PeriodCode, period.PERIOD_CODE, StringComparison.OrdinalIgnoreCase)
                && journal.IsDraft).ToList();

            if (drafts.Count > 0)
            {
                return OgResult<OgLedgerWorkspace>.Failed(
                    $"{period.PERIOD_CODE} still holds {drafts.Count.ToString(CultureInfo.InvariantCulture)} draft " +
                    $"journal(s) ({string.Join(", ", drafts.Select(journal => journal.Number))}); post or delete them " +
                    "before closing.",
                    LedgerRules.CodeDraftsOpen);
            }

            // AC-5: a period whose trial balance does not agree cannot be closed.
            var trial = TrialBalanceOf(journals, period.PERIOD_CODE);

            if (trial.Debit != trial.Credit)
            {
                return OgResult<OgLedgerWorkspace>.Failed(
                    $"{period.PERIOD_CODE} does not balance: debit {MoneyText.Of(trial.Debit)} against credit " +
                    $"{MoneyText.Of(trial.Credit)}, a difference of {MoneyText.Of(trial.Debit - trial.Credit)}. " +
                    "The period cannot be closed on an unbalanced trial balance.",
                    LedgerRules.CodePeriodNotBalanced);
            }

            period.STATUS = LedgerCodes.PeriodClosed;
            period.CLOSED_BY = actor.Trim();
            period.CLOSED_AT = DateTime.UtcNow;
            period.CLOSE_NOTE = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
            period.ROW_CHANGED_BY = actor.Trim();
            period.ROW_CHANGED_DATE = DateTime.UtcNow;

            var written = await UpdatePeriodAsync(source, period).ConfigureAwait(false);

            if (written is not null)
            {
                return OgResult<OgLedgerWorkspace>.Failed(written, LedgerRules.CodeWriteFailed);
            }

            var workspace = await ReadAsync(source, context, null, period.PERIOD_CODE, cancellationToken)
                .ConfigureAwait(false);

            return OgResult<OgLedgerWorkspace>.Ok(workspace);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"closing period {periodCode}",
                "the period was not closed",
                FailureSeverity.Error);

            return OgResult<OgLedgerWorkspace>.Failed(
                "The period could not be closed.", reference ?? LedgerRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgLedgerWorkspace>> ReopenPeriodAsync(
        string periodCode,
        OgLedgerContext context,
        string reason,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgLedgerWorkspace>.Failed(
                "A reopen must record who made it.", LedgerRules.CodeActorRequired);
        }

        // AC-6: a reopen without a reason is not an audit record.
        if (string.IsNullOrWhiteSpace(reason))
        {
            return OgResult<OgLedgerWorkspace>.Failed(
                "Reopening the books needs its reason recorded, or nobody can see why the period moved.",
                LedgerRules.CodeReasonRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgLedgerWorkspace>.Failed(
                "No connection is open, so the period cannot be reopened.", LedgerRules.CodeNoConnection);
        }

        try
        {
            var periods = await OgUow.ReadAllAsync<COMPANY_LEDGER_PERIOD>(source).ConfigureAwait(false);

            var period = periods.FirstOrDefault(row =>
                string.Equals(row.PERIOD_CODE, periodCode?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (period is null)
            {
                return OgResult<OgLedgerWorkspace>.Failed(
                    $"Period '{periodCode}' does not exist, so it cannot be reopened.",
                    LedgerRules.CodeUnknownPeriod);
            }

            if (LedgerCodes.IsOpen(period.STATUS))
            {
                return OgResult<OgLedgerWorkspace>.Failed(
                    $"{period.PERIOD_CODE} is already open.", LedgerRules.CodeInputInvalid);
            }

            period.STATUS = LedgerCodes.PeriodOpen;
            period.REOPENED_BY = actor.Trim();
            period.REOPENED_AT = DateTime.UtcNow;
            period.REOPEN_REASON = reason.Trim();
            period.ROW_CHANGED_BY = actor.Trim();
            period.ROW_CHANGED_DATE = DateTime.UtcNow;

            var written = await UpdatePeriodAsync(source, period).ConfigureAwait(false);

            if (written is not null)
            {
                return OgResult<OgLedgerWorkspace>.Failed(written, LedgerRules.CodeWriteFailed);
            }

            var workspace = await ReadAsync(source, context, null, period.PERIOD_CODE, cancellationToken)
                .ConfigureAwait(false);

            return OgResult<OgLedgerWorkspace>.Ok(workspace);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"reopening period {periodCode}",
                "the period was not reopened",
                FailureSeverity.Error);

            return OgResult<OgLedgerWorkspace>.Failed(
                "The period could not be reopened.", reference ?? LedgerRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgLedgerWorkspace>> LoadAsync(
        OgLedgerContext context,
        string? journalId = null,
        string? periodCode = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var source = _source();

        if (source is null)
        {
            return OgResult<OgLedgerWorkspace>.Failed(
                "No connection is open, so the ledger cannot be read.", LedgerRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, context, journalId, periodCode, cancellationToken)
                .ConfigureAwait(false);

            return OgResult<OgLedgerWorkspace>.Ok(workspace);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the ledger",
                "the ledger is unavailable; no posting is possible",
                FailureSeverity.Degraded);

            return OgResult<OgLedgerWorkspace>.Ok(new OgLedgerWorkspace(
                [], null, [], null, [], periodCode ?? string.Empty, 0m, 0m, [], LedgerRules.JournalTypes, true,
                $"the ledger could not be read (reference {reference})"));
        }
    }

    // ------------------------------------------------------------------ the reads

    private static async Task<OgLedgerWorkspace> ReadAsync(
        IDataSource source,
        OgLedgerContext context,
        string? journalId,
        string? periodCode,
        CancellationToken cancellationToken)
    {
        var accounts = await OgUow.ReadAllAsync<GL_ACCOUNT>(source).ConfigureAwait(false);
        var journalRows = await OgUow.ReadAllAsync<JOURNAL_ENTRY>(source).ConfigureAwait(false);
        var lineRows = await OgUow.ReadAllAsync<JOURNAL_ENTRY_LINE>(source).ConfigureAwait(false);
        var periodRows = await OgUow.ReadAllAsync<COMPANY_LEDGER_PERIOD>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var byNumber = accounts
            .Where(account => !string.IsNullOrWhiteSpace(account.ACCOUNT_NUMBER))
            .GroupBy(account => account.ACCOUNT_NUMBER.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        var linesByJournal = lineRows
            .Where(line => !string.IsNullOrWhiteSpace(line.JOURNAL_ENTRY_ID))
            .GroupBy(line => line.JOURNAL_ENTRY_ID!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(line => line.LINE_NUMBER).ToList(),
                StringComparer.OrdinalIgnoreCase);

        var journals = journalRows
            .Where(row => !string.IsNullOrWhiteSpace(row.JOURNAL_ENTRY_ID))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(row => row.ENTRY_DATE)
            .ThenBy(row => row.ENTRY_NUMBER, StringComparer.Ordinal)
            .Select(row => ToJournal(
                row,
                linesByJournal.TryGetValue(row.JOURNAL_ENTRY_ID, out var mine) ? mine : [],
                byNumber))
            .ToList();

        var periods = periodRows
            .Where(row => !string.IsNullOrWhiteSpace(row.PERIOD_CODE))
            .OrderByDescending(row => row.PERIOD_CODE, StringComparer.Ordinal)
            .Select(row => new OgPeriod(
                row.COMPANY_LEDGER_PERIOD_ID,
                row.PERIOD_CODE,
                row.START_DATE,
                row.END_DATE,
                row.STATUS,
                row.CLOSED_BY,
                row.CLOSED_AT,
                row.REOPENED_BY,
                row.REOPENED_AT,
                row.REOPEN_REASON,
                row.CLOSE_NOTE))
            .ToList();

        var selectedPeriod = string.IsNullOrWhiteSpace(periodCode)
            ? periods.FirstOrDefault()
            : periods.FirstOrDefault(row =>
                  string.Equals(row.Code, periodCode.Trim(), StringComparison.OrdinalIgnoreCase))
              ?? periods.FirstOrDefault();

        var shownPeriod = selectedPeriod?.Code ?? periodCode ?? string.Empty;
        var trial = TrialBalanceOf(journals, shownPeriod);

        var selected = string.IsNullOrWhiteSpace(journalId)
            ? journals.FirstOrDefault()
            : journals.FirstOrDefault(journal =>
                  string.Equals(journal.Id, journalId.Trim(), StringComparison.OrdinalIgnoreCase))
              ?? journals.FirstOrDefault();

        return new OgLedgerWorkspace(
            journals,
            selected,
            periods,
            selectedPeriod,
            trial.Rows,
            shownPeriod,
            trial.Debit,
            trial.Credit,
            byNumber.Keys.OrderBy(number => number, StringComparer.Ordinal).ToList(),
            LedgerRules.JournalTypes,
            false,
            null);
    }

    /// <summary>
    /// AC-5 and AC-7: the trial balance for a period, grouped by account, each row carrying the journal lines behind it.
    /// </summary>
    private static (IReadOnlyList<OgTrialBalanceRow> Rows, decimal Debit, decimal Credit) TrialBalanceOf(
        IReadOnlyList<OgJournal> journals,
        string periodCode)
    {
        var posted = journals
            .Where(journal => journal.IsPosted && !string.Equals(journal.Status, LedgerCodes.JournalReversed,
                StringComparison.OrdinalIgnoreCase))
            .Where(journal => string.IsNullOrWhiteSpace(periodCode)
                              || string.Equals(journal.PeriodCode, periodCode, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var rows = posted
            .SelectMany(journal => journal.Lines)
            .GroupBy(line => line.AccountNumber, StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => new OgTrialBalanceRow(
                group.Key,
                group.First().AccountName,
                group.Sum(line => line.Debit),
                group.Sum(line => line.Credit),
                group.OrderBy(line => line.JournalId).ThenBy(line => line.LineNumber).ToList()))
            .ToList();

        return (rows, rows.Sum(row => row.Debit), rows.Sum(row => row.Credit));
    }

    private static OgJournal ToJournal(
        JOURNAL_ENTRY row,
        IReadOnlyList<JOURNAL_ENTRY_LINE> lines,
        IReadOnlyDictionary<string, GL_ACCOUNT> byNumber)
    {
        var numberOf = byNumber
            .ToDictionary(pair => pair.Value.GL_ACCOUNT_ID ?? string.Empty, pair => pair.Key, StringComparer.Ordinal);

        var mapped = lines
            .Select(line =>
            {
                var accountId = line.GL_ACCOUNT_ID ?? string.Empty;
                var number = numberOf.TryGetValue(accountId, out var found) ? found : accountId;

                return new OgJournalLine(
                    line.JOURNAL_ENTRY_LINE_ID,
                    line.JOURNAL_ENTRY_ID ?? string.Empty,
                    number,
                    byNumber.TryGetValue(number, out var account) ? account.ACCOUNT_NAME ?? string.Empty : string.Empty,
                    line.LINE_NUMBER ?? 0,
                    line.DEBIT_AMOUNT ?? 0m,
                    line.CREDIT_AMOUNT ?? 0m,
                    line.DESCRIPTION);
            })
            .ToList();

        return new OgJournal(
            row.JOURNAL_ENTRY_ID,
            row.ENTRY_NUMBER ?? string.Empty,
            row.ENTRY_DATE ?? DateTime.MinValue,
            row.ENTRY_TYPE ?? string.Empty,
            row.SOURCE_MODULE ?? string.Empty,
            row.REFERENCE_NUMBER,
            row.DESCRIPTION,
            row.STATUS ?? LedgerCodes.JournalDraft,
            row.TOTAL_DEBIT ?? mapped.Sum(line => line.Debit),
            row.TOTAL_CREDIT ?? mapped.Sum(line => line.Credit),
            row.AUTHOR ?? row.ROW_CREATED_BY,
            row.POSTED_BY,
            row.POSTED_AT,
            row.PERIOD_CODE ?? (row.ENTRY_DATE is { } date ? LedgerCodes.PeriodCodeOf(date) : string.Empty),
            row.REVERSAL_OF_ENTRY_ID,
            mapped);
    }

    private static async Task<IReadOnlyList<OgJournal>> ReadJournalsAsync(
        IDataSource source,
        CancellationToken cancellationToken)
    {
        var workspace = await ReadAsync(
            source,
            new OgLedgerContext("APP", string.Empty, "MAIN", "USD"),
            null,
            null,
            cancellationToken).ConfigureAwait(false);

        return workspace.Journals;
    }

    private static async Task<OgJournal?> ReadJournalAsync(
        IDataSource source,
        string journalId,
        CancellationToken cancellationToken)
    {
        var journals = await ReadJournalsAsync(source, cancellationToken).ConfigureAwait(false);

        return journals.FirstOrDefault(journal =>
            string.Equals(journal.Id, journalId?.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private static async Task<JOURNAL_ENTRY?> ReadEntryRowAsync(
        IDataSource source,
        string journalId,
        CancellationToken cancellationToken)
    {
        var unit = OgUow.For<JOURNAL_ENTRY>(source);
        unit.PrimaryKey = "JOURNAL_ENTRY_ID";

        var rows = await OgUow.ReadAsync(
            unit,
            [OgUow.Equal("JOURNAL_ENTRY_ID", journalId)],
            cancellationToken).ConfigureAwait(false);

        return rows.FirstOrDefault();
    }

    private static async Task<IReadOnlyList<COMPANY_LEDGER_PERIOD>> EnsurePeriodAsync(
        IDataSource source,
        OgLedgerContext context,
        string periodCode,
        DateTime date,
        string actor)
    {
        var periods = await OgUow.ReadAllAsync<COMPANY_LEDGER_PERIOD>(source).ConfigureAwait(false);

        if (periods.Any(row => string.Equals(row.PERIOD_CODE, periodCode, StringComparison.OrdinalIgnoreCase)))
        {
            return periods;
        }

        var start = new DateTime(date.Year, date.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);
        var now = DateTime.UtcNow;

        var unit = OgUow.For<COMPANY_LEDGER_PERIOD>(source);
        unit.PrimaryKey = "COMPANY_LEDGER_PERIOD_ID";

        unit.Add(new COMPANY_LEDGER_PERIOD
        {
            COMPANY_LEDGER_PERIOD_ID = Guid.NewGuid().ToString("N"),
            OWNER_TENANT_ID = context.CompanyId,
            OWNER_COMPANY_BA_ID = context.CompanyId,
            BOOK_ID = context.BookCode,
            CURRENCY_CODE = context.Currency,
            CALENDAR_ID = "MONTHLY",
            PERIOD_CODE = periodCode,
            START_DATE = start,
            END_DATE = end,
            STATUS = LedgerCodes.PeriodOpen,
            // The authority note this app writes; the owner's guarded transition engine keeps its own richer history.
            AUTHORITY_JSON = "{\"source\":\"desktop\",\"actor\":\"" + actor.Trim().Replace("\"", "'") + "\"}",
            ACTIVE_IND = "Y",
            PPDM_GUID = Guid.NewGuid().ToString("N"),
            ROW_CREATED_BY = actor.Trim(),
            ROW_CREATED_DATE = now,
            ROW_CHANGED_BY = actor.Trim(),
            ROW_CHANGED_DATE = now,
        });

        await unit.Commit().ConfigureAwait(false);

        return await OgUow.ReadAllAsync<COMPANY_LEDGER_PERIOD>(source).ConfigureAwait(false);
    }

    // ------------------------------------------------------------------ the writes

    private static async Task<bool> WriteLinesAsync(
        IDataSource source,
        string journalId,
        IReadOnlyList<(OgJournalLineInput Input, GL_ACCOUNT Account)> lines,
        string actor,
        CancellationToken cancellationToken)
    {
        var unit = OgUow.For<JOURNAL_ENTRY_LINE>(source);
        unit.PrimaryKey = "JOURNAL_ENTRY_LINE_ID";

        var now = DateTime.UtcNow;
        var number = 1;

        foreach (var (input, account) in lines)
        {
            unit.Add(new JOURNAL_ENTRY_LINE
            {
                JOURNAL_ENTRY_LINE_ID = Guid.NewGuid().ToString("N"),
                JOURNAL_ENTRY_ID = journalId,
                GL_ACCOUNT_ID = account.GL_ACCOUNT_ID,
                LINE_NUMBER = number,
                DEBIT_AMOUNT = input.DebitAmount,
                CREDIT_AMOUNT = input.CreditAmount,
                DESCRIPTION = input.Description,
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

    private static async Task<string?> UpdateAsync(IDataSource source, JOURNAL_ENTRY row)
    {
        var unit = OgUow.For<JOURNAL_ENTRY>(source);
        unit.PrimaryKey = "JOURNAL_ENTRY_ID";

        var updated = unit.Update(row);

        if (!OgUow.IsOk(updated))
        {
            return OgUow.Message(updated, "The journal was not saved.");
        }

        var committed = await unit.Commit().ConfigureAwait(false);

        return OgUow.IsOk(committed) ? null : OgUow.Message(committed, "The journal was not saved.");
    }

    private static async Task<string?> UpdatePeriodAsync(IDataSource source, COMPANY_LEDGER_PERIOD row)
    {
        var unit = OgUow.For<COMPANY_LEDGER_PERIOD>(source);
        unit.PrimaryKey = "COMPANY_LEDGER_PERIOD_ID";

        var updated = unit.Update(row);

        if (!OgUow.IsOk(updated))
        {
            return OgUow.Message(updated, "The period was not saved.");
        }

        var committed = await unit.Commit().ConfigureAwait(false);

        return OgUow.IsOk(committed) ? null : OgUow.Message(committed, "The period was not saved.");
    }

    /// <summary>The next entry number for a year: JE-yyyy-0001.</summary>
    private static string NextNumber(IReadOnlyList<JOURNAL_ENTRY> existing, DateTime date)
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

/// <summary>Money as the ledger prints it, in one place so a line and a total cannot differ.</summary>
internal static class MoneyText
{
    public static string Of(decimal value) => value.ToString("#,0.00", CultureInfo.InvariantCulture);
}
