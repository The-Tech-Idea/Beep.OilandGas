using Beep.OilandGas.Accounting.Constants;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Accounting;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// ACC-02: the balance rule and its exact imbalance, the journal record's derived state, the period lock rule and the
/// page's guard - the parts that need no database. The create/post/duplicate/close/reopen/reverse journey and the trial
/// balance are proven in the gateway test.
/// </summary>
public sealed class LedgerTests
{
    private const string PageFileName = "LedgerPageView.cs";

    [Fact]
    public void An_unbalanced_journal_names_the_exact_imbalance()
    {
        // AC-1 and SG-10-1: the refusal is the imbalance, not "invalid".
        Assert.Equal(1000m, LedgerCodes.Imbalance(6000m, 5000m));
        Assert.Equal(-0.01m, LedgerCodes.Imbalance(999.99m, 1000.00m));
        Assert.True(LedgerCodes.IsBalanced(1234.56m, 1234.56m));
        Assert.False(LedgerCodes.IsBalanced(1234.56m, 1234.55m));

        var debitHeavy = LedgerRules.CheckBalance(6000m, 5000m);

        Assert.Equal(LedgerRules.CodeUnbalanced, debitHeavy!.Value.Code);
        Assert.Contains("1,000.00 excess on the debit side", debitHeavy.Value.Message, StringComparison.Ordinal);
        Assert.Contains("debit 6,000.00", debitHeavy.Value.Message, StringComparison.Ordinal);

        var creditHeavy = LedgerRules.CheckBalance(5000m, 6000m);

        Assert.Contains("1,000.00 excess on the credit side", creditHeavy!.Value.Message, StringComparison.Ordinal);

        Assert.Null(LedgerRules.CheckBalance(250m, 250m));
    }

    [Fact]
    public void A_journal_reports_its_balance_and_posting_state()
    {
        var unbalanced = Journal() with { TotalDebit = 1000m, TotalCredit = 900m };

        Assert.False(unbalanced.Balances);
        Assert.Equal(100m, unbalanced.Imbalance);
        Assert.Contains("UNBALANCED by 100.00", unbalanced.BalanceText, StringComparison.Ordinal);

        var draft = Journal();

        Assert.True(draft.Balances);
        Assert.True(draft.IsDraft);
        Assert.False(draft.IsPosted);
        Assert.False(draft.IsReversible);
        Assert.Contains("not posted", draft.PostingText, StringComparison.Ordinal);

        // A posted journal may be reversed; a reversed one may not be reversed again.
        var posted = draft with { Status = LedgerCodes.JournalPosted, PostedBy = "omar", PostedAt = DateTime.UtcNow };

        Assert.True(posted.IsPosted);
        Assert.True(posted.IsReversible);
        Assert.Contains("posted by omar", posted.PostingText, StringComparison.Ordinal);

        var reversed = draft with { Status = LedgerCodes.JournalReversed };

        Assert.True(reversed.IsPosted);
        Assert.False(reversed.IsReversible);

        Assert.Equal("balanced", draft.BalanceText);
        Assert.Equal("1,000.00 / 1,000.00", draft.AmountText);
    }

    [Fact]
    public void Only_an_open_period_accepts_a_write()
    {
        // AC-3's rule, stated once: open accepts, closed and locked do not.
        Assert.True(LedgerCodes.IsOpen(LedgerCodes.PeriodOpen));
        Assert.False(LedgerCodes.IsOpen(LedgerCodes.PeriodClosed));
        Assert.False(LedgerCodes.IsOpen(LedgerCodes.PeriodLocked));
        Assert.False(LedgerCodes.IsOpen(null));

        // The two journal states the desk writes are the owner's own values.
        Assert.Equal("DRAFT", LedgerCodes.JournalDraft);
        Assert.Equal("POSTED", LedgerCodes.JournalPosted);
        Assert.Contains("REVERSED", LedgerCodes.JournalStatuses);
        Assert.Contains("CLOSED", LedgerCodes.PeriodStates);

        // AC-6: the period sentence carries the close and the reopen.
        var period = new OgPeriod(
            "P1", "2026-03", new DateTime(2026, 3, 1), new DateTime(2026, 3, 31),
            LedgerCodes.PeriodOpen, "sara", new DateTime(2026, 4, 2, 9, 0, 0), "omar",
            new DateTime(2026, 4, 5, 11, 0, 0), "late invoice from the field", "March close");

        Assert.True(period.IsOpen);
        Assert.Contains("closed by sara", period.CloseText, StringComparison.Ordinal);
        Assert.Contains("reopened by omar", period.ReopenText, StringComparison.Ordinal);
        Assert.Contains("late invoice from the field", period.ReopenText, StringComparison.Ordinal);
        Assert.Contains("accepts journals", period.StateText, StringComparison.Ordinal);

        var closed = period with { Status = LedgerCodes.PeriodClosed };

        Assert.False(closed.IsOpen);
        Assert.Contains("no journal may be written", closed.StateText, StringComparison.Ordinal);
    }

    [Fact]
    public void A_journal_needs_its_date_type_source_and_a_line()
    {
        Assert.Equal(
            ["date", "type", "source", "at least one line"],
            LedgerRules.Missing(new OgJournalInputs(null, null, null, null, null, [])));

        Assert.Empty(LedgerRules.Missing(Inputs()));
        Assert.Contains("ACCRUAL", LedgerRules.JournalTypes);
        Assert.Contains("REVERSAL", LedgerRules.JournalTypes);
    }

    [Fact]
    public void A_trial_balance_row_keeps_its_lines_for_the_drill_down()
    {
        // AC-5 and AC-7 at the record's surface: the row's balance is debit less credit, and its lines are the drill-down.
        var lines = new[]
        {
            new OgJournalLine("L1", "J1", "1000", "Cash", 1, 750.25m, 0m, "cash in"),
            new OgJournalLine("L2", "J1", "1000", "Cash", 2, 249.75m, 0m, "cash in"),
        };

        var row = new OgTrialBalanceRow("1000", "Cash", 1000.00m, 0m, lines);

        Assert.Equal(1000.00m, row.Balance);
        Assert.Equal(2, row.Lines.Count);
        Assert.Contains("2 line(s)", row.Describe(), StringComparison.Ordinal);
        Assert.Contains("balance 1,000.00", row.Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public void The_page_shows_the_balances_and_names_no_status_literal()
    {
        // The trial balance sentence, the drill-down and the period state come from the workspace; the page holds no
        // journal-status string of its own.
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => Path.GetFileName(source) == PageFileName));

        Assert.Contains("workspace.TrialBalanceText", page, StringComparison.Ordinal);
        Assert.Contains("workspace.TrialBalance", page, StringComparison.Ordinal);
        Assert.Contains("row.Lines", page, StringComparison.Ordinal);
        Assert.Contains("IsReversible", page, StringComparison.Ordinal);

        Assert.DoesNotContain("\"POSTED\"", page, StringComparison.Ordinal);
        Assert.DoesNotContain("\"CLOSED\"", page, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ helpers

    private static OgJournalInputs Inputs() =>
        new(
            new DateTime(2026, 3, 15),
            "GENERAL",
            "MANUAL",
            "INV-1",
            "Cash received",
            [
                new OgJournalLineInput("1000", 1000m, null, "cash in"),
                new OgJournalLineInput("4001", null, 1000m, "revenue"),
            ]);

    private static OgJournal Journal() =>
        new(
            "J1",
            "JE-2026-0001",
            new DateTime(2026, 3, 15),
            "GENERAL",
            "MANUAL",
            "INV-1",
            "Cash received",
            LedgerCodes.JournalDraft,
            1000m,
            1000m,
            "sara",
            null,
            null,
            "2026-03",
            null,
            []);
}
