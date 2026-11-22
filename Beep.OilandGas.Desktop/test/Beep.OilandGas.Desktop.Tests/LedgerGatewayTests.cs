using Beep.OilandGas.Accounting.Constants;
using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Accounting;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// ACC-02 against a real SQLite database: an unbalanced journal is refused with the exact imbalance (AC-1, SG-10-1); a
/// duplicate reference and a second posting create no second entry (AC-2, SG-10-2); a write into a closed period is
/// refused naming it (AC-3, SG-10-3); the author cannot post their own journal (AC-4, SG-10-4); the trial balance agrees
/// and drills down to the lines (AC-5, AC-7); and reopening records actor, reason and time (AC-6).
/// Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class LedgerGatewayTests
{
    private const string ConnectionName = "acc02-ledger";

    [IntegrationFact]
    public async Task The_ledger_posts_and_locks_without_a_second_entry()
    {        var root = Path.Combine(OgPaths.DataRoot, "acc02-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasAcc02"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "ledger.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "ledger.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgLedgerService(() => source, reporter);
        var context = new OgLedgerContext("SMOKE", "Smoke Energy", "MAIN", "USD");

        // ---- the workspace ---------------------------------------------------
        var loaded = await service.LoadAsync(context);

        Assert.True(loaded.Success, loaded.Message);
        Assert.Empty(loaded.Value!.Journals);
        Assert.False(loaded.Value.HistoryFailed);
        Assert.Contains("1000", loaded.Value.AccountNumbers);

        // ---- AC-1 and SG-10-1: the imbalance is the refusal ------------------
        var unbalanced = await service.CreateJournalAsync(
            Inputs() with
            {
                Lines =
                [
                    new OgJournalLineInput("1000", 1000m, null, "cash in"),
                    new OgJournalLineInput("4001", null, 900m, "revenue"),
                ],
            },
            context,
            "sara");

        Assert.False(unbalanced.Success);
        Assert.Equal(LedgerRules.CodeUnbalanced, unbalanced.Reference);
        Assert.Contains("100.00 excess on the debit side", unbalanced.Message!, StringComparison.Ordinal);
        Assert.Equal(0, Count(database, "JOURNAL_ENTRY"));

        // ---- a balanced journal is a draft -----------------------------------
        var created = await service.CreateJournalAsync(Inputs(), context, "sara");

        Assert.True(created.Success, created.Message + " :: " + string.Join(" | ", reporter.Failures));

        var journal = created.Value!.Selected!;

        Assert.Equal(LedgerCodes.JournalDraft, journal.Status);
        Assert.True(journal.Balances);
        Assert.Equal("sara", journal.Author);
        Assert.Equal(2, journal.Lines.Count);
        Assert.Equal(1, Count(database, "JOURNAL_ENTRY"));
        Assert.Equal(2, Count(database, "JOURNAL_ENTRY_LINE"));
        Assert.Equal(1, Count(database, "COMPANY_LEDGER_PERIOD"));

        // The entry carries no posting audit yet.
        Assert.Equal(1, Count(database,
            "JOURNAL_ENTRY WHERE STATUS = 'DRAFT' AND POSTED_BY IS NULL AND POSTED_AT IS NULL " +
            "AND PERIOD_CODE = '2026-03' AND AUTHOR = 'sara'"));

        // ---- AC-2 and SG-10-2: a duplicate reference creates nothing ----------
        var duplicate = await service.CreateJournalAsync(Inputs() with { Description = "again" }, context, "nadia");

        Assert.False(duplicate.Success);
        Assert.Equal(LedgerRules.CodeDuplicateReference, duplicate.Reference);
        Assert.Equal(1, Count(database, "JOURNAL_ENTRY"));

        // ---- AC-4 and SG-10-4: the author cannot post ------------------------
        var byAuthor = await service.PostAsync(journal.Id, context, "sara");

        Assert.False(byAuthor.Success);
        Assert.Equal(LedgerRules.CodeAuthorCannotPost, byAuthor.Reference);
        Assert.Contains("sara", byAuthor.Message!, StringComparison.Ordinal);
        Assert.Equal(0, Count(database, "JOURNAL_ENTRY WHERE STATUS = 'POSTED'"));

        // ---- posting, and posting again --------------------------------------
        var posted = await service.PostAsync(journal.Id, context, "omar");

        Assert.True(posted.Success, posted.Message);
        Assert.Equal(LedgerCodes.JournalPosted, posted.Value!.Selected!.Status);
        Assert.Equal("omar", posted.Value.Selected.PostedBy);
        Assert.NotNull(posted.Value.Selected.PostedAt);

        var again = await service.PostAsync(journal.Id, context, "nadia");

        Assert.False(again.Success);
        Assert.Equal(LedgerRules.CodeAlreadyPosted, again.Reference);
        Assert.Equal(1, Count(database, "JOURNAL_ENTRY WHERE STATUS = 'POSTED'"));

        // ---- AC-5 and AC-7: the trial balance agrees, and drills down --------
        var afterPost = await service.LoadAsync(context, periodCode: "2026-03");

        Assert.True(afterPost.Success, afterPost.Message);

        var trial = afterPost.Value!;

        Assert.True(trial.TrialBalances, trial.TrialBalanceText);
        Assert.Equal(1000m, trial.TrialDebit);
        Assert.Equal(1000m, trial.TrialCredit);
        Assert.Contains("equals credit", trial.TrialBalanceText, StringComparison.Ordinal);

        var cash = trial.TrialBalance.Single(row => row.AccountNumber == "1000");

        Assert.Equal(1000m, cash.Debit);
        Assert.Equal(0m, cash.Credit);
        Assert.Single(cash.Lines);

        var revenue = trial.TrialBalance.Single(row => row.AccountNumber == "4001");

        Assert.Equal(1000m, revenue.Credit);
        Assert.Single(revenue.Lines);

        // ---- AC-3 and SG-10-3: a closed period refuses a write and names it --
        var closed = await service.ClosePeriodAsync("2026-03", context, "March books", "sara");

        Assert.True(closed.Success, closed.Message);
        Assert.Equal(LedgerCodes.PeriodClosed, closed.Value!.SelectedPeriod!.Status);

        var intoClosed = await service.CreateJournalAsync(
            Inputs() with { Reference = "INV-2", Date = new DateTime(2026, 3, 20) },
            context,
            "nadia");

        Assert.False(intoClosed.Success);
        Assert.Equal(LedgerRules.CodePeriodClosed, intoClosed.Reference);
        Assert.Contains("2026-03", intoClosed.Message!, StringComparison.Ordinal);
        Assert.Equal(1, Count(database, "JOURNAL_ENTRY"));

        // ---- AC-6: reopening records actor, reason and time ------------------
        var noReason = await service.ReopenPeriodAsync("2026-03", context, "  ", "omar");

        Assert.False(noReason.Success);
        Assert.Equal(LedgerRules.CodeReasonRequired, noReason.Reference);

        var reopened = await service.ReopenPeriodAsync(
            "2026-03", context, "Late field ticket from the Woodford pad", "omar");

        Assert.True(reopened.Success, reopened.Message);

        var period = reopened.Value!.SelectedPeriod!;

        Assert.True(period.IsOpen);
        Assert.Equal("omar", period.ReopenedBy);
        Assert.NotNull(period.ReopenedAt);
        Assert.Contains("Late field ticket", period.ReopenText, StringComparison.Ordinal);
        Assert.Equal(1, Count(database,
            "COMPANY_LEDGER_PERIOD WHERE STATUS = 'OPEN' AND CLOSED_BY = 'sara' AND CLOSED_AT IS NOT NULL " +
            "AND REOPENED_BY = 'omar' AND REOPENED_AT IS NOT NULL " +
            "AND REOPEN_REASON LIKE 'Late field ticket%'"));

        // ---- a reversal posts a counter-entry and retires the original -------
        var april = await service.CreateJournalAsync(
            Inputs() with
            {
                Date = new DateTime(2026, 4, 10),
                Reference = "INV-3",
                Lines =
                [
                    new OgJournalLineInput("1000", 400m, null, "cash in"),
                    new OgJournalLineInput("4001", null, 400m, "revenue"),
                ],
            },
            context,
            "nadia");

        Assert.True(april.Success, april.Message);
        Assert.True((await service.PostAsync(april.Value!.Selected!.Id, context, "omar")).Success);

        var reversal = await service.ReverseAsync(april.Value.Selected.Id, context, "omar");

        Assert.True(reversal.Success, reversal.Message);

        var counter = reversal.Value!.Selected!;

        Assert.Equal("REVERSAL", counter.Type);
        Assert.Equal(april.Value.Selected.Id, counter.ReversalOf);
        Assert.Equal(400m, counter.TotalDebit);
        Assert.Equal(400m, counter.TotalCredit);
        Assert.Equal("POSTED", counter.Status);

        Assert.Equal(1, Count(database,
            $"JOURNAL_ENTRY WHERE JOURNAL_ENTRY_ID = '{april.Value.Selected.Id}' AND STATUS = 'REVERSED' " +
            "AND REVERSED_AT IS NOT NULL"));
        Assert.Equal(1, Count(database,
            $"JOURNAL_ENTRY WHERE REVERSAL_OF_ENTRY_ID = '{april.Value.Selected.Id}'"));

        // ---- an unavailable ledger is a state --------------------------------
        Execute(database, "DROP TABLE JOURNAL_ENTRY_LINE;");
        Execute(database, "DROP TABLE JOURNAL_ENTRY;");

        var unavailable = await service.LoadAsync(context);

        Assert.True(unavailable.Success, unavailable.Message);
        Assert.True(unavailable.Value!.HistoryFailed);
        Assert.Empty(unavailable.Value.Journals);
        Assert.Contains("could not be read", unavailable.Value.HistoryStateText, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ the schema

    private static void Seed(string database)
    {
        Execute(database,
            """
            CREATE TABLE GL_ACCOUNT (
                GL_ACCOUNT_ID TEXT NOT NULL PRIMARY KEY,
                CONFIGURATION_COMMAND_JSON TEXT NULL,
                OWNER_TENANT_ID TEXT NULL,
                OWNER_COMPANY_BA_ID TEXT NULL,
                BOOK_ID TEXT NULL,
                CURRENCY_CODE TEXT NULL,
                ACCOUNT_NUMBER TEXT NULL,
                ACCOUNT_NAME TEXT NULL,
                ACCOUNT_TYPE TEXT NULL,
                PARENT_ACCOUNT_ID TEXT NULL,
                NORMAL_BALANCE TEXT NULL,
                OPENING_BALANCE REAL NULL,
                CURRENT_BALANCE REAL NULL,
                DESCRIPTION TEXT NULL,
                ROW_ID TEXT NULL,
                ACTIVE_IND TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL,
                ROW_EFFECTIVE_DATE TEXT NULL,
                ROW_EXPIRY_DATE TEXT NULL,
                ROW_QUALITY TEXT NULL,
                PPDM_GUID TEXT NULL,
                EXPIRY_DATE TEXT NULL,
                EFFECTIVE_DATE TEXT NULL,
                REMARK TEXT NULL,
                SOURCE TEXT NULL);

            CREATE TABLE JOURNAL_ENTRY (
                JOURNAL_ENTRY_ID TEXT NOT NULL PRIMARY KEY,
                ENTRY_NUMBER TEXT NULL,
                ENTRY_DATE TEXT NULL,
                ENTRY_TYPE TEXT NULL,
                STATUS TEXT NULL,
                DESCRIPTION TEXT NULL,
                REFERENCE_NUMBER TEXT NULL,
                SOURCE_MODULE TEXT NULL,
                TOTAL_DEBIT REAL NULL,
                TOTAL_CREDIT REAL NULL,
                ROW_ID TEXT NULL,
                JOURNAL_DATE TEXT NULL,
                JOURNAL_TYPE TEXT NULL,
                JOURNAL_DESCRIPTION TEXT NULL,
                SOURCE_SYSTEM TEXT NULL,
                AUTHOR TEXT NULL,
                POSTED_BY TEXT NULL,
                POSTED_AT TEXT NULL,
                PERIOD_CODE TEXT NULL,
                REVERSAL_OF_ENTRY_ID TEXT NULL,
                REVERSED_AT TEXT NULL,
                ACTIVE_IND TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL,
                ROW_EFFECTIVE_DATE TEXT NULL,
                ROW_EXPIRY_DATE TEXT NULL,
                ROW_QUALITY TEXT NULL,
                PPDM_GUID TEXT NULL,
                EXPIRY_DATE TEXT NULL,
                EFFECTIVE_DATE TEXT NULL,
                REMARK TEXT NULL,
                SOURCE TEXT NULL);

            CREATE TABLE JOURNAL_ENTRY_LINE (
                JOURNAL_ENTRY_LINE_ID TEXT NOT NULL PRIMARY KEY,
                JOURNAL_ENTRY_ID TEXT NULL,
                GL_ACCOUNT_ID TEXT NULL,
                LINE_NUMBER INTEGER NULL,
                DEBIT_AMOUNT REAL NULL,
                CREDIT_AMOUNT REAL NULL,
                DESCRIPTION TEXT NULL,
                ROW_ID TEXT NULL,
                COST_CENTER_ID TEXT NULL,
                CURRENCY_CODE TEXT NULL,
                EXCHANGE_RATE REAL NULL,
                SHADOW_DEBIT_AMOUNT REAL NULL,
                SHADOW_CREDIT_AMOUNT REAL NULL,
                ACTIVE_IND TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL,
                ROW_EFFECTIVE_DATE TEXT NULL,
                ROW_EXPIRY_DATE TEXT NULL,
                ROW_QUALITY TEXT NULL,
                PPDM_GUID TEXT NULL,
                EXPIRY_DATE TEXT NULL,
                EFFECTIVE_DATE TEXT NULL,
                REMARK TEXT NULL,
                SOURCE TEXT NULL);

            CREATE TABLE COMPANY_LEDGER_PERIOD (
                COMPANY_LEDGER_PERIOD_ID TEXT NOT NULL PRIMARY KEY,
                OWNER_TENANT_ID TEXT NOT NULL,
                OWNER_COMPANY_BA_ID TEXT NOT NULL,
                BOOK_ID TEXT NOT NULL,
                CURRENCY_CODE TEXT NOT NULL,
                CALENDAR_ID TEXT NOT NULL,
                PERIOD_CODE TEXT NOT NULL,
                START_DATE TEXT NOT NULL,
                END_DATE TEXT NOT NULL,
                STATUS TEXT NOT NULL,
                AUTHORITY_JSON TEXT NOT NULL,
                STATE_HISTORY_JSON TEXT NULL,
                CLOSED_BY TEXT NULL,
                CLOSED_AT TEXT NULL,
                REOPENED_BY TEXT NULL,
                REOPENED_AT TEXT NULL,
                REOPEN_REASON TEXT NULL,
                CLOSE_NOTE TEXT NULL,
                ACTIVE_IND TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL,
                ROW_EFFECTIVE_DATE TEXT NULL,
                ROW_EXPIRY_DATE TEXT NULL,
                ROW_QUALITY TEXT NULL,
                PPDM_GUID TEXT NULL,
                EXPIRY_DATE TEXT NULL,
                EFFECTIVE_DATE TEXT NULL,
                REMARK TEXT NULL,
                SOURCE TEXT NULL);

            INSERT INTO GL_ACCOUNT (GL_ACCOUNT_ID, ACCOUNT_NUMBER, ACCOUNT_NAME, ACCOUNT_TYPE,
                NORMAL_BALANCE, CURRENCY_CODE, ACTIVE_IND) VALUES
                ('A-1000', '1000', 'Cash', 'ASSET', 'DEBIT', 'USD', 'Y'),
                ('A-4001', '4001', 'Revenue', 'REVENUE', 'CREDIT', 'USD', 'Y');
            """);
    }

    private static void Execute(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private static long Count(string database, string fromAndWhere)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM " + fromAndWhere + ";";

        return Convert.ToInt64(command.ExecuteScalar());
    }

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

    private sealed class Recording : IFailureReporter
    {
        public List<string> Failures { get; } = [];

        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error)
        {
            Failures.Add($"{operation} | {consequence} | {exception.Message}");
            return FailureReferenceFormat.New();
        }
    }
}
