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
/// ACC-04 against a real SQLite database: the ageing buckets come from due dates (AC-2); an allocation above the invoice
/// balance is refused with the excess (AC-4); a receipt keeps its unapplied cash (AC-1); posting writes exactly one journal
/// (AC-5) and leaves a partial payment's correct balance (AC-3); a retry after a lost acknowledgement neither duplicates the
/// journal nor applies the cash twice; and the author cannot approve their own receipt (AC-6).
/// Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class ReceivableGatewayTests
{
    private const string ConnectionName = "acc04-receivable";

    [IntegrationFact]
    public async Task A_receipt_posts_one_journal_and_reduces_the_invoices_by_its_allocations()
    {        var root = Path.Combine(OgPaths.DataRoot, "acc04-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasAcc04"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "receivable.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "receivable.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgReceivableService(() => source, reporter);
        var context = new OgRevenueContext("SMOKE", "MAIN");
        var asOf = new DateTime(2026, 4, 15);

        // ---- AC-2: the ageing comes from due dates ---------------------------
        var loaded = await service.LoadAsync(context, asOf: asOf);

        Assert.True(loaded.Success, loaded.Message);
        Assert.False(loaded.Value!.HistoryFailed);
        Assert.Equal(3, loaded.Value.OpenInvoices.Count);
        Assert.Equal(2_000m, loaded.Value.AgeingTotals.Current);
        Assert.Equal(11_000m, loaded.Value.AgeingTotals.Days30);
        Assert.Equal(0m, loaded.Value.AgeingTotals.Days60);
        Assert.Equal(5_000m, loaded.Value.AgeingTotals.Days90);
        Assert.Equal(18_000m, loaded.Value.AgeingTotals.Total);

        // The bucket on the invoice row itself, and the sentence the grid prints.
        var invoiceA = loaded.Value.OpenInvoices.Single(invoice => invoice.Number == "INV-A");
        var invoiceB = loaded.Value.OpenInvoices.Single(invoice => invoice.Number == "INV-B");

        Assert.Equal("30", invoiceA.Bucket);
        Assert.Equal("90+", invoiceB.Bucket);
        Assert.Contains("as at 2026-04-15", loaded.Value.AgeingSummaryText, StringComparison.Ordinal);
        Assert.Contains("90+ 5,000.00", loaded.Value.AgeingSummaryText, StringComparison.Ordinal);

        // ---- AC-4: an allocation above the invoice balance names the excess ---
        var overAllocated = await service.RecordAsync(
            Inputs() with { Amount = 12_000m, Allocations = [new OgAllocationInput(invoiceA.Id, 12_000m)] },
            context,
            "sara");

        Assert.False(overAllocated.Success);
        Assert.Equal(ReceivableRules.CodeOverAllocation, overAllocated.Reference);
        Assert.Contains("excess of 1,000.00", overAllocated.Message!, StringComparison.Ordinal);
        Assert.Equal(0, Count(database, "AR_RECEIPT"));

        // The allocation cannot exceed the receipt either, and the currency has to agree.
        var exceeds = await service.RecordAsync(
            Inputs() with { Amount = 1_000m, Allocations = [new OgAllocationInput(invoiceA.Id, 2_000m)] },
            context,
            "sara");

        Assert.Equal(ReceivableRules.CodeExceedsReceipt, exceeds.Reference);

        var wrongCurrency = await service.RecordAsync(
            Inputs() with { Allocations = [new OgAllocationInput("E-1", 100m)] },
            context,
            "sara");

        Assert.Equal(ReceivableRules.CodeCurrencyMismatch, wrongCurrency.Reference);
        Assert.Equal(0, Count(database, "AR_RECEIPT"));

        // ---- AC-1: a receipt with unapplied cash -----------------------------
        var recorded = await service.RecordAsync(
            Inputs() with
            {
                Amount = 8_000m,
                Allocations =
                [
                    new OgAllocationInput(invoiceA.Id, 6_000m),
                    new OgAllocationInput(invoiceB.Id, 1_000m),
                ],
            },
            context,
            "sara");

        Assert.True(recorded.Success, recorded.Message + " :: " + string.Join(" | ", reporter.Failures));

        var receipt = recorded.Value!.Selected!;

        Assert.Equal(ReceivableCodes.Draft, receipt.State);
        Assert.Equal(8_000m, receipt.Amount);
        Assert.Equal(1_000m, receipt.Unapplied);
        Assert.Equal(7_000m, receipt.Allocated);
        Assert.Equal(2, receipt.Allocations.Count);
        Assert.Contains("unapplied 1,000.00 USD", receipt.AllocationText, StringComparison.Ordinal);

        Assert.Equal(1, Count(database, "AR_RECEIPT WHERE AMOUNT = 8000 AND UNAPPLIED_AMOUNT = 1000 AND STATUS = 'DRAFT'"));
        Assert.Equal(2, Count(database, "AR_PAYMENT WHERE RECEIPT_ID IS NOT NULL AND STATUS = 'DRAFT'"));

        // ---- AC-5: posting writes exactly one journal ------------------------
        var posted = await service.PostAsync(receipt.Id, context, "sara");

        Assert.True(posted.Success, posted.Message);

        var postedReceipt = posted.Value!.Selected!;

        Assert.Equal(ReceivableCodes.Posted, postedReceipt.State);
        Assert.StartsWith("RCPT-", postedReceipt.Number, StringComparison.Ordinal);
        Assert.True(postedReceipt.HasJournal);
        Assert.Equal(1, Count(database, "JOURNAL_ENTRY WHERE SOURCE_MODULE = 'AR_RECEIPT'"));
        Assert.Equal(1, Count(database,
            "JOURNAL_ENTRY_LINE WHERE DEBIT_AMOUNT = 8000 AND GL_ACCOUNT_ID = 'A-1000'"));
        Assert.Equal(1, Count(database,
            "JOURNAL_ENTRY_LINE WHERE CREDIT_AMOUNT = 8000 AND GL_ACCOUNT_ID = 'A-1110'"));
        Assert.Equal(1, Count(database,
            $"AR_RECEIPT WHERE AR_RECEIPT_ID = '{receipt.Id}' AND STATUS = 'POSTED' " +
            "AND POSTED_BY = 'sara' AND JOURNAL_ENTRY_ID IS NOT NULL"));
        Assert.Equal(2, Count(database, "AR_PAYMENT WHERE RECEIPT_ID IS NOT NULL AND STATUS = 'POSTED'"));

        // ---- AC-3: a partial payment leaves exactly the remainder ------------
        Assert.Equal(1, Count(database,
            $"AR_INVOICE WHERE AR_INVOICE_ID = '{invoiceA.Id}' AND PAID_AMOUNT = 6000 AND BALANCE_DUE = 5000"));
        Assert.Equal(1, Count(database,
            $"AR_INVOICE WHERE AR_INVOICE_ID = '{invoiceB.Id}' AND PAID_AMOUNT = 1000 AND BALANCE_DUE = 4000"));

        // The unapplied cash is still held, not spent.
        var afterPost = await service.LoadAsync(context, asOf: asOf);

        Assert.Equal(1_000m, afterPost.Value!.Unapplied);
        // 11,000 + 5,000 + 2,000 open, less the 6,000 and 1,000 applied by this receipt = 11,000 still outstanding.
        Assert.Equal(11_000m, afterPost.Value.AgeingTotals.Total);

        // ---- posting again writes no second journal --------------------------
        var again = await service.PostAsync(receipt.Id, context, "sara");

        Assert.True(again.Success, again.Message);
        Assert.Equal(1, Count(database, "JOURNAL_ENTRY WHERE SOURCE_MODULE = 'AR_RECEIPT'"));

        // ---- a lost acknowledgement neither duplicates nor re-applies --------
        Execute(database,
            $"UPDATE AR_RECEIPT SET JOURNAL_ENTRY_ID = NULL, STATUS = 'DRAFT', RECEIPT_NUMBER = NULL " +
            $"WHERE AR_RECEIPT_ID = '{receipt.Id}';");

        var recovered = await service.PostAsync(receipt.Id, context, "sara");

        Assert.True(recovered.Success, recovered.Message);
        Assert.Equal(1, Count(database, "JOURNAL_ENTRY WHERE SOURCE_MODULE = 'AR_RECEIPT'"));

        // The invoices were not reduced a second time.
        Assert.Equal(1, Count(database,
            $"AR_INVOICE WHERE AR_INVOICE_ID = '{invoiceA.Id}' AND PAID_AMOUNT = 6000 AND BALANCE_DUE = 5000"));
        Assert.Equal(1, Count(database,
            $"AR_INVOICE WHERE AR_INVOICE_ID = '{invoiceB.Id}' AND PAID_AMOUNT = 1000 AND BALANCE_DUE = 4000"));

        // ---- AC-6: the author cannot approve ---------------------------------
        var byAuthor = await service.ReviewAsync(receipt.Id, approve: true, reason: "ok", actor: "sara");

        Assert.False(byAuthor.Success);
        Assert.Equal(ReceivableRules.CodeAuthorCannotApprove, byAuthor.Reference);
        Assert.Contains("sara", byAuthor.Message!, StringComparison.Ordinal);

        var approved = await service.ReviewAsync(receipt.Id, approve: true, reason: "checked", actor: "nadia");

        Assert.True(approved.Success, approved.Message);
        Assert.Equal(ReceivableCodes.Approved, approved.Value!.Selected!.State);
        Assert.Equal(1, Count(database,
            $"AR_RECEIPT WHERE AR_RECEIPT_ID = '{receipt.Id}' AND STATUS = 'APPROVED' " +
            "AND APPROVED_BY = 'nadia' AND APPROVED_AT IS NOT NULL"));

        // ---- a returned receipt carries its reason ---------------------------
        var second = (await service.RecordAsync(
            Inputs() with
            {
                CustomerId = "BUYER-2",
                Amount = 500m,
                Allocations = [new OgAllocationInput(invoiceA.Id, 500m)],
            },
            context,
            "sara")).Value!.Selected!;

        Assert.True((await service.PostAsync(second.Id, context, "sara")).Success);

        var noReason = await service.ReviewAsync(second.Id, approve: false, reason: " ", actor: "nadia");

        Assert.False(noReason.Success);
        Assert.Equal(ReceivableRules.CodeReturnReasonRequired, noReason.Reference);

        var returned = await service.ReviewAsync(
            second.Id, approve: false, reason: "Allocation does not match the remittance", actor: "nadia");

        Assert.True(returned.Success, returned.Message);
        Assert.Equal(ReceivableCodes.Returned, returned.Value!.Selected!.State);
        Assert.Contains("remittance", returned.Value.Selected.ReturnReason!, StringComparison.Ordinal);

        // ---- an unavailable register is a state ------------------------------
        Execute(database, "DROP TABLE AR_PAYMENT;");
        Execute(database, "DROP TABLE AR_RECEIPT;");

        var unavailable = await service.LoadAsync(context);

        Assert.True(unavailable.Success, unavailable.Message);
        Assert.True(unavailable.Value!.HistoryFailed);
        Assert.Empty(unavailable.Value.Receipts);
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

            CREATE TABLE AR_PAYMENT (
                AR_PAYMENT_ID TEXT NOT NULL PRIMARY KEY,
                AR_INVOICE_ID TEXT NULL,
                PAYMENT_NUMBER TEXT NULL,
                PAYMENT_DATE TEXT NULL,
                PAYMENT_AMOUNT REAL NULL,
                PAYMENT_METHOD TEXT NULL,
                CHECK_NUMBER TEXT NULL,
                OWNER_TENANT_ID TEXT NULL,
                OWNER_COMPANY_BA_ID TEXT NULL,
                BOOK_ID TEXT NULL,
                FIELD_ID TEXT NULL,
                CURRENCY_CODE TEXT NULL,
                PAYER_BA_ID TEXT NULL,
                EXTERNAL_REFERENCE TEXT NULL,
                EVIDENCE_REFERENCE TEXT NULL,
                STATUS TEXT NULL,
                SOURCE_REVISION TEXT NULL,
                PREPARATION_WORKFLOW_JSON TEXT NULL,
                REVIEW_WORKFLOW_JSON TEXT NULL,
                ROW_ID TEXT NULL,
                RECEIPT_ID TEXT NULL,
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

            CREATE TABLE AR_RECEIPT (
                AR_RECEIPT_ID TEXT NOT NULL PRIMARY KEY,
                RECEIPT_NUMBER TEXT NULL,
                CUSTOMER_BA_ID TEXT NOT NULL,
                RECEIPT_DATE TEXT NOT NULL,
                AMOUNT REAL NOT NULL,
                UNAPPLIED_AMOUNT REAL NOT NULL,
                CURRENCY_CODE TEXT NOT NULL,
                REFERENCE TEXT NULL,
                BANK_ACCOUNT TEXT NULL,
                STATUS TEXT NOT NULL,
                AUTHOR TEXT NULL,
                JOURNAL_ENTRY_ID TEXT NULL,
                POSTED_BY TEXT NULL,
                POSTED_AT TEXT NULL,
                APPROVED_BY TEXT NULL,
                APPROVED_AT TEXT NULL,
                RETURN_REASON TEXT NULL,
                OWNER_TENANT_ID TEXT NULL,
                OWNER_COMPANY_BA_ID TEXT NULL,
                BOOK_ID TEXT NULL,
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
                ('A-1110', '1110', 'Accounts receivable', 'ASSET', 'DEBIT', 'USD', 'Y');

            INSERT INTO AR_INVOICE (AR_INVOICE_ID, INVOICE_NUMBER, CUSTOMER_BA_ID, INVOICE_DATE, DUE_DATE,
                CURRENCY_CODE, TOTAL_AMOUNT, PAID_AMOUNT, CREDITED_AMOUNT, BALANCE_DUE, STATUS, ACTIVE_IND) VALUES
                ('A-1', 'INV-A', 'BUYER-1', '2026-03-01', '2026-03-31', 'USD', 11000, 0, 0, 11000, 'ISSUED', 'Y'),
                ('B-1', 'INV-B', 'BUYER-1', '2026-03-01', '2026-01-15', 'USD', 5000, 0, 0, 5000, 'ISSUED', 'Y'),
                ('E-1', 'INV-E', 'BUYER-2', '2026-04-01', '2026-05-01', 'EUR', 2000, 0, 0, 2000, 'ISSUED', 'Y');
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

    private static OgReceiptInputs Inputs() =>
        new(
            "BUYER-1",
            new DateTime(2026, 4, 15),
            8_000m,
            "USD",
            "BANK-REF-1",
            "BANK-1",
            [new OgAllocationInput("A-1", 6_000m)]);

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
