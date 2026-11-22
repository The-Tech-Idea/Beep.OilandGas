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
/// ACC-03 against a real SQLite database: a volume without a price is blocked naming it, an invoice is built with the
/// owning calculator's tax, issuing writes exactly one journal (AC-2) that a retry cannot duplicate (AC-6), every line keeps
/// its volume (AC-3), a credit note reduces the balance by exactly its amount (AC-1, SG-10-5), the author cannot approve
/// (AC-4), and an invoice with payments or a journal cannot be deleted. Opt-in
/// (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class RevenueGatewayTests
{
    private const string ConnectionName = "acc03-revenue";

    [IntegrationFact]
    public async Task An_invoice_issues_one_journal_and_a_credit_reduces_the_balance_exactly()
    {        var root = Path.Combine(OgPaths.DataRoot, "acc03-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasAcc03"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "revenue.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "revenue.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgRevenueService(() => source, reporter);
        var context = new OgRevenueContext("SMOKE", "MAIN");

        // ---- the workspace ---------------------------------------------------
        var loaded = await service.LoadAsync(context);

        Assert.True(loaded.Success, loaded.Message);
        Assert.Empty(loaded.Value!.Invoices);
        Assert.False(loaded.Value.HistoryFailed);
        Assert.Contains("USD", loaded.Value.Currencies);
        Assert.Contains("bbl", loaded.Value.QuantityUnits);

        // ---- a volume without a price is blocked by name ----------------------
        var unpriced = await service.BuildAsync(
            Inputs() with
            {
                Lines =
                [
                    new OgSalesLineInput("OIL", null, 100m, "bbl", 70m, "VOL-1"),
                    new OgSalesLineInput("GAS", null, 50m, "Mscf", null, "VOL-2"),
                ],
            },
            context,
            "sara");

        Assert.False(unpriced.Success);
        Assert.Equal(RevenueRules.CodeVolumeWithoutPrice, unpriced.Reference);
        Assert.Contains("VOL-2", unpriced.Message!, StringComparison.Ordinal);
        Assert.Equal(0, Count(database, "AR_INVOICE"));

        // ---- a currency that disagrees with the contract ---------------------
        var wrongCurrency = await service.BuildAsync(
            Inputs() with { ContractCurrency = "EUR" }, context, "sara");

        Assert.False(wrongCurrency.Success);
        Assert.Equal(RevenueRules.CodeCurrencyMismatch, wrongCurrency.Reference);
        Assert.Equal(0, Count(database, "AR_INVOICE"));

        // ---- a valid draft, with the owning calculator's tax ------------------
        var built = await service.BuildAsync(Inputs(), context, "sara");

        Assert.True(built.Success, built.Message + " :: " + string.Join(" | ", reporter.Failures));

        var draft = built.Value!.Selected!;

        Assert.Equal(RevenueCodes.InvoiceDraft, draft.State);
        Assert.Equal(10_000m, draft.Subtotal);
        Assert.Equal(1_000m, draft.Tax);
        Assert.Equal(11_000m, draft.Total);
        Assert.Equal(11_000m, draft.BalanceDue);
        Assert.Equal(0m, draft.Paid);
        Assert.Equal(2, draft.Lines.Count);
        Assert.False(draft.HasJournal);

        // AC-3: each line carries the volume it came from, asserted on the stored row.
        Assert.Equal(1, Count(database,
            "AR_INVOICE_LINE WHERE VOLUME_REFERENCE = 'VOL-1' AND QUANTITY = 100 AND UNIT_PRICE = 70 " +
            "AND NET_AMOUNT = 7000 AND TAX_AMOUNT = 700"));
        Assert.Equal(1, Count(database,
            "AR_INVOICE_LINE WHERE VOLUME_REFERENCE = 'VOL-2' AND NET_AMOUNT = 3000 AND TAX_AMOUNT = 300"));

        // ---- AC-2: issuing writes exactly one journal -------------------------
        var issued = await service.IssueAsync(draft.Id, context, "sara");

        Assert.True(issued.Success, issued.Message);

        var invoice = issued.Value!.Selected!;

        Assert.Equal(RevenueCodes.InvoiceIssued, invoice.State);
        Assert.StartsWith("INV-", invoice.Number, StringComparison.Ordinal);
        Assert.True(invoice.HasJournal);
        Assert.Equal(1, Count(database, "JOURNAL_ENTRY"));
        Assert.Equal(3, Count(database, "JOURNAL_ENTRY_LINE"));

        Assert.Equal(1, Count(database,
            "JOURNAL_ENTRY WHERE STATUS = 'POSTED' AND SOURCE_MODULE = 'SALES_INVOICE' " +
            "AND TOTAL_DEBIT = 11000 AND TOTAL_CREDIT = 11000"));
        Assert.Equal(1, Count(database,
            "JOURNAL_ENTRY_LINE WHERE DEBIT_AMOUNT = 11000 AND GL_ACCOUNT_ID = 'A-1110'"));
        Assert.Equal(1, Count(database,
            "JOURNAL_ENTRY_LINE WHERE CREDIT_AMOUNT = 10000 AND GL_ACCOUNT_ID = 'A-4001'"));
        Assert.Equal(1, Count(database,
            "JOURNAL_ENTRY_LINE WHERE CREDIT_AMOUNT = 1000 AND GL_ACCOUNT_ID = 'A-2300'"));

        // The journal really balances: debit total equals credit total.
        Assert.Equal(11_000m, Sum(database, "SELECT SUM(DEBIT_AMOUNT) FROM JOURNAL_ENTRY_LINE"));
        Assert.Equal(11_000m, Sum(database, "SELECT SUM(CREDIT_AMOUNT) FROM JOURNAL_ENTRY_LINE"));

        // ---- issuing again creates no second journal --------------------------
        var again = await service.IssueAsync(draft.Id, context, "sara");

        Assert.True(again.Success, again.Message);
        Assert.Equal(1, Count(database, "JOURNAL_ENTRY"));

        // ---- AC-6: a lost acknowledgement is recovered, not duplicated --------
        Execute(database,
            $"UPDATE AR_INVOICE SET JOURNAL_ENTRY_ID = NULL, STATUS = 'DRAFT' WHERE AR_INVOICE_ID = '{draft.Id}';");

        var recovered = await service.IssueAsync(draft.Id, context, "sara");

        Assert.True(recovered.Success, recovered.Message);
        Assert.Equal(1, Count(database, "JOURNAL_ENTRY"));
        Assert.Equal(1, Count(database,
            $"AR_INVOICE WHERE AR_INVOICE_ID = '{draft.Id}' AND STATUS = 'ISSUED' " +
            "AND JOURNAL_ENTRY_ID IS NOT NULL AND ISSUED_BY = 'sara'"));

        // ---- AC-1 and SG-10-5: the credit reduces the balance by exactly its amount ----
        var credit = await service.CreditAsync(draft.Id, 1_500m, "Short delivery on the crude line", "omar");

        Assert.True(credit.Success, credit.Message);

        var credited = credit.Value!.Selected!;

        Assert.Equal(1_500m, credited.Credited);
        Assert.Equal(9_500m, credited.BalanceDue);
        Assert.Single(credited.CreditNotes);
        Assert.Equal(1, Count(database,
            $"AR_INVOICE WHERE AR_INVOICE_ID = '{draft.Id}' AND CREDITED_AMOUNT = 1500 AND BALANCE_DUE = 9500"));
        Assert.Equal(1, Count(database,
            "AR_CREDIT_MEMO WHERE CREDIT_AMOUNT = 1500 AND STATUS = 'ISSUED' " +
            "AND REASON = 'Short delivery on the crude line'"));

        // A credit larger than what is left is refused and changes nothing.
        var tooBig = await service.CreditAsync(draft.Id, 10_000m, "too much", "omar");

        Assert.False(tooBig.Success);
        Assert.Equal(RevenueRules.CodeCreditTooLarge, tooBig.Reference);
        Assert.Equal(1, Count(database, "AR_CREDIT_MEMO"));

        // ---- AC-4: the author cannot approve ---------------------------------
        var byAuthor = await service.ReviewAsync(draft.Id, approve: true, reason: "ok", actor: "sara");

        Assert.False(byAuthor.Success);
        Assert.Equal(RevenueRules.CodeAuthorCannotApprove, byAuthor.Reference);
        Assert.Contains("sara", byAuthor.Message!, StringComparison.Ordinal);

        var approved = await service.ReviewAsync(draft.Id, approve: true, reason: "checked", actor: "nadia");

        Assert.True(approved.Success, approved.Message);
        Assert.Equal(RevenueCodes.InvoiceApproved, approved.Value!.Selected!.State);
        Assert.Equal(1, Count(database,
            $"AR_INVOICE WHERE AR_INVOICE_ID = '{draft.Id}' AND STATUS = 'APPROVED' " +
            "AND APPROVED_BY = 'nadia' AND APPROVED_AT IS NOT NULL"));

        // ---- a returned invoice carries its reason ---------------------------
        var second = (await service.BuildAsync(
            Inputs() with { BuyerId = "BUYER-2" }, context, "sara")).Value!.Selected!;

        Assert.True((await service.IssueAsync(second.Id, context, "sara")).Success);

        var noReason = await service.ReviewAsync(second.Id, approve: false, reason: " ", actor: "nadia");

        Assert.False(noReason.Success);
        Assert.Equal(RevenueRules.CodeReturnReasonRequired, noReason.Reference);

        var returned = await service.ReviewAsync(
            second.Id, approve: false, reason: "Volume does not match the run ticket", actor: "nadia");

        Assert.True(returned.Success, returned.Message);
        Assert.Equal(RevenueCodes.InvoiceReturned, returned.Value!.Selected!.State);
        Assert.Contains("run ticket", returned.Value.Selected.ReturnReason!, StringComparison.Ordinal);

        // ---- deletion: a draft goes, an issued invoice does not ---------------
        var third = (await service.BuildAsync(
            Inputs() with { BuyerId = "BUYER-3" }, context, "sara")).Value!.Selected!;

        var deleted = await service.DeleteAsync(third.Id, "sara");

        Assert.True(deleted.Success, deleted.Message);
        Assert.DoesNotContain(deleted.Value!.Invoices, candidate => candidate.Id == third.Id);

        var refusedDelete = await service.DeleteAsync(draft.Id, "sara");

        Assert.False(refusedDelete.Success);
        Assert.Equal(RevenueRules.CodeNotDeletable, refusedDelete.Reference);

        // ---- an unavailable register is a state ------------------------------
        Execute(database, "DROP TABLE AR_INVOICE_LINE;");
        Execute(database, "DROP TABLE AR_INVOICE;");

        var unavailable = await service.LoadAsync(context);

        Assert.True(unavailable.Success, unavailable.Message);
        Assert.True(unavailable.Value!.HistoryFailed);
        Assert.Empty(unavailable.Value.Invoices);
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

            CREATE TABLE AR_INVOICE (
                AR_INVOICE_ID TEXT NOT NULL PRIMARY KEY,
                OWNER_TENANT_ID TEXT NULL,
                OWNER_COMPANY_BA_ID TEXT NULL,
                FIELD_ID TEXT NULL,
                SOURCE_INVOICE_ID TEXT NULL,
                CURRENCY_CODE TEXT NULL,
                PREPARATION_WORKFLOW_JSON TEXT NULL,
                REVIEW_WORKFLOW_JSON TEXT NULL,
                INVOICE_NUMBER TEXT NULL,
                CUSTOMER_BA_ID TEXT NULL,
                INVOICE_DATE TEXT NULL,
                DUE_DATE TEXT NULL,
                TOTAL_AMOUNT REAL NULL,
                PAID_AMOUNT REAL NULL,
                BALANCE_DUE REAL NULL,
                STATUS TEXT NULL,
                ROW_ID TEXT NULL,
                AUTHOR TEXT NULL,
                SUBTOTAL_AMOUNT REAL NULL,
                TAX_AMOUNT REAL NULL,
                TAX_RATE REAL NULL,
                CONTRACT_ID TEXT NULL,
                JOURNAL_ENTRY_ID TEXT NULL,
                ISSUED_BY TEXT NULL,
                ISSUED_AT TEXT NULL,
                CREDITED_AMOUNT REAL NULL,
                APPROVED_BY TEXT NULL,
                APPROVED_AT TEXT NULL,
                RETURN_REASON TEXT NULL,
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

            CREATE TABLE AR_INVOICE_LINE (
                AR_INVOICE_LINE_ID TEXT NOT NULL PRIMARY KEY,
                AR_INVOICE_ID TEXT NOT NULL,
                LINE_NUMBER INTEGER NOT NULL,
                COMMODITY TEXT NULL,
                DESCRIPTION TEXT NULL,
                QUANTITY REAL NOT NULL,
                QUANTITY_OUOM TEXT NULL,
                UNIT_PRICE REAL NOT NULL,
                PRICE_OUOM TEXT NULL,
                NET_AMOUNT REAL NOT NULL,
                TAX_AMOUNT REAL NOT NULL,
                LINE_TOTAL REAL NOT NULL,
                VOLUME_REFERENCE TEXT NULL,
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

            CREATE TABLE AR_CREDIT_MEMO (
                AR_CREDIT_MEMO_ID TEXT NOT NULL PRIMARY KEY,
                AR_INVOICE_ID TEXT NULL,
                CREDIT_MEMO_NUMBER TEXT NULL,
                CREDIT_MEMO_DATE TEXT NULL,
                CREDIT_AMOUNT REAL NULL,
                REASON TEXT NULL,
                STATUS TEXT NULL,
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
                ('A-1110', '1110', 'Accounts receivable', 'ASSET', 'DEBIT', 'USD', 'Y'),
                ('A-4001', '4001', 'Revenue', 'REVENUE', 'CREDIT', 'USD', 'Y'),
                ('A-2300', '2300', 'Sales tax payable', 'LIABILITY', 'CREDIT', 'USD', 'Y');
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

    private static decimal Sum(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        var value = command.ExecuteScalar();

        return value is null or DBNull ? 0m : Convert.ToDecimal(value);
    }

    private static OgInvoiceInputs Inputs() =>
        new(
            "BUYER-1",
            "CONTRACT-1",
            "USD",
            new DateTime(2026, 3, 10),
            new DateTime(2026, 4, 10),
            "USD",
            0.10m,
            [
                new OgSalesLineInput("OIL", "Woodford crude", 100m, "bbl", 70m, "VOL-1"),
                new OgSalesLineInput("GAS", "Woodford gas", 50m, "Mscf", 60m, "VOL-2"),
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
