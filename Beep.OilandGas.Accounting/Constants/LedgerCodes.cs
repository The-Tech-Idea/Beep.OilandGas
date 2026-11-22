using System;

namespace Beep.OilandGas.Accounting.Constants;

/// <summary>
/// The ledger's journal statuses and period states, with the balance rule (ACC-02).
/// </summary>
/// <remarks>
/// The values are the ones the owner's own journal workflow already uses - <c>DRAFT</c>, <c>APPROVED</c>, <c>REJECTED</c>
/// and <c>POSTED</c> appear in <c>JournalEntryService</c>'s workflow source - so the screen does not invent a second set.
/// The period states are the ones <c>PeriodClosingService</c> transitions the company ledger period through.
///
/// <para>
/// <b>Which period-close service owns the lock (the feature's open question).</b> The accounting project's
/// <c>PeriodClosingService</c> says in its own header that it "owns native company/book period records and controlled
/// period operations", and its transitions are the ones that write <c>COMPANY_LEDGER_PERIOD.STATUS</c>. The
/// ProductionAccounting one validates month-end readiness and tracks unreconciled items - it is a <b>production close</b>,
/// not the ledger lock. So the ledger lock is the accounting period record, and that is what a write is checked against.
/// </para>
/// </remarks>
public static class LedgerCodes
{
    /// <summary>Written but not posted: the only state whose lines may still be edited.</summary>
    public const string JournalDraft = "DRAFT";

    /// <summary>Reviewed and waiting for the poster.</summary>
    public const string JournalApproved = "APPROVED";

    /// <summary>Posted: the entry is in the ledger and must not be edited.</summary>
    public const string JournalPosted = "POSTED";

    /// <summary>Posted and then reversed by a counter-entry.</summary>
    public const string JournalReversed = "REVERSED";

    /// <summary>Sent back by the reviewer.</summary>
    public const string JournalRejected = "REJECTED";

    /// <summary>Every journal status.</summary>
    public static readonly string[] JournalStatuses =
        [JournalDraft, JournalApproved, JournalRejected, JournalPosted, JournalReversed];

    /// <summary>The period accepts writes.</summary>
    public const string PeriodOpen = "OPEN";

    /// <summary>The period is closed: no journal may be created or posted into it.</summary>
    public const string PeriodClosed = "CLOSED";

    /// <summary>The period is locked outright - the accounting close's final state.</summary>
    public const string PeriodLocked = "LOCKED";

    /// <summary>Every period state.</summary>
    public static readonly string[] PeriodStates = [PeriodOpen, PeriodClosed, PeriodLocked];

    /// <summary>Whether a period state accepts a write.</summary>
    public static bool IsOpen(string? status) =>
        string.Equals(status?.Trim(), PeriodOpen, StringComparison.OrdinalIgnoreCase);

    /// <summary>Whether a journal may be edited or posted.</summary>
    public static bool IsDraft(string? status) =>
        string.Equals(status?.Trim(), JournalDraft, StringComparison.OrdinalIgnoreCase);

    /// <summary>Whether a journal is already in the ledger.</summary>
    public static bool IsPosted(string? status) =>
        string.Equals(status?.Trim(), JournalPosted, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// The journal's imbalance: debit less credit. Zero means the entry balances; anything else is the exact amount by
    /// which it does not, and that is what a refusal names (SG-10-1).
    /// </summary>
    public static decimal Imbalance(decimal totalDebit, decimal totalCredit) => totalDebit - totalCredit;

    /// <summary>Whether the two sides agree to the last decimal place.</summary>
    public static bool IsBalanced(decimal totalDebit, decimal totalCredit) =>
        Imbalance(totalDebit, totalCredit) == 0m;

    /// <summary>The period code a date falls in, as <c>yyyy-MM</c>, for a monthly calendar.</summary>
    public static string PeriodCodeOf(DateTime date) =>
        date.ToString("yyyy-MM", System.Globalization.CultureInfo.InvariantCulture);
}
