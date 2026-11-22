using Beep.OilandGas.Accounting.Constants;
using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Accounting;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// ACC-01 against a real SQLite database: the dashboard composes the seven accounting services, so every count it shows is
/// the count its own page lists (AC-2), a balance whose records could not be read is null and says so rather than printing
/// zero (AC-1), the period strip is the session's period (AC-3), a queue without its permission is withheld with the reason
/// (AC-4), and the refresh stamp is on the page either way (AC-5). Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class AccountingDashboardGatewayTests
{
    private const string ConnectionName = "acc01-dashboard";

    [IntegrationFact]
    public async Task The_dashboard_agrees_with_the_pages_it_links_to()
    {        var root = Path.Combine(OgPaths.DataRoot, "acc01-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasAcc01"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "dashboard.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "dashboard.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgAccountingDashboardService(
            () => source,
            new OgRevenueService(() => source, reporter),
            new OgReceivableService(() => source, reporter),
            new OgPayablesService(() => source, reporter),
            new OgLedgerService(() => source, reporter),
            new OgTaxService(() => source, reporter),
            new OgRoyaltiesService(() => source, reporter),
            new OgWarehouseService(() => source, reporter),
            reporter);

        var context = new OgDashboardContext(
            "SMOKE",
            "Smoke Oil",
            "MAIN",
            "USD",
            "2026-11",
            "November 2026",
            new OgPermissions([OgRoleNames.SystemAdmin], [], permissionSourceAvailable: true));

        // ---- the balances ---------------------------------------------------
        var loaded = await service.LoadAsync(context);

        Assert.True(loaded.Success, loaded.Message + " :: " + string.Join(" | ", reporter.Failures));
        Assert.False(loaded.Value!.ReadFailed);

        var dashboard = loaded.Value;

        // Open AR is the two issued invoices' balances: 1,000 + 1,500. The draft is not receivable yet.
        Assert.Equal(2_500m, dashboard.Kpis.OpenAr);

        // Open AP is the two unpaid supplier invoices: 3,000 + 700.
        Assert.Equal(3_700m, dashboard.Kpis.OpenAp);

        // Overdue is the past-due sales invoice (1,000) plus the past-due supplier invoice (3,000).
        Assert.Equal(4_000m, dashboard.Kpis.Overdue);

        // One draft journal, in the session's period.
        Assert.Equal(1, dashboard.Kpis.UnpostedJournals);

        // Cash: the chart's opening balance 5,000 plus the posted 250 debit on the cash account.
        Assert.Equal(5_250m, dashboard.Kpis.Cash);
        Assert.Equal(5_000m, dashboard.Kpis.CashOpening);
        Assert.Equal(250m, dashboard.Kpis.CashMovement);
        Assert.Contains("5,250.00", dashboard.Kpis.CashText, StringComparison.Ordinal);

        // AC-1: nothing is unknown here, so no figure says unavailable.
        Assert.False(dashboard.Kpis.AnyUnavailable);
        Assert.DoesNotContain("unavailable", dashboard.Kpis.OpenArText, StringComparison.Ordinal);

        // ---- AC-2: every count is the count the queue's own page will list ---
        var revenue = await new OgRevenueService(() => source, reporter)
            .LoadAsync(new OgRevenueContext("SMOKE", "MAIN"));
        var receivable = await new OgReceivableService(() => source, reporter)
            .LoadAsync(new OgRevenueContext("SMOKE", "MAIN"));
        var payables = await new OgPayablesService(() => source, reporter)
            .LoadAsync(new OgPayableContext("SMOKE", "MAIN"));
        var ledger = await new OgLedgerService(() => source, reporter)
            .LoadAsync(new OgLedgerContext("SMOKE", "Smoke Oil", "MAIN", "USD"));
        var tax = await new OgTaxService(() => source, reporter)
            .LoadAsync(new OgTaxContext("SMOKE", "MAIN"));
        var royalties = await new OgRoyaltiesService(() => source, reporter)
            .LoadAsync(new OgRoyaltyContext("SMOKE", "MAIN"));
        var warehouse = await new OgWarehouseService(() => source, reporter)
            .LoadAsync(new OgWarehouseContext("SMOKE", "MAIN"));

        Assert.True(revenue.Success, revenue.Message);
        Assert.True(receivable.Success, receivable.Message);
        Assert.True(payables.Success, payables.Message);
        Assert.True(ledger.Success, ledger.Message);
        Assert.True(tax.Success, tax.Message);
        Assert.True(royalties.Success, royalties.Message);
        Assert.True(warehouse.Success, warehouse.Message);

        // The composed counts, compared with the same predicate over the same workspace.
        Assert.Equal(revenue.Value!.Invoices.Count(invoice => invoice.IsDraft),
            Queue(dashboard, "sales-draft").Count);
        Assert.Equal(revenue.Value.Invoices.Count(invoice => invoice.IsIssued && !invoice.IsApproved),
            Queue(dashboard, "sales-review").Count);
        Assert.Equal(payables.Value!.Invoices.Count(invoice => !invoice.IsPaid),
            Queue(dashboard, "ap-open").Count);
        Assert.Equal(payables.Value.Invoices.Count(invoice => invoice.NeedsExceptionApproval),
            Queue(dashboard, "ap-exceptions").Count);
        Assert.Equal(payables.Value.Orders.Count(order => order.IsDraft),
            Queue(dashboard, "po-draft").Count);
        Assert.Equal(receivable.Value!.Receipts.Count(receipt => receipt.IsDraft),
            Queue(dashboard, "receipts-draft").Count);
        Assert.Equal(receivable.Value.Receipts.Count(receipt => receipt.Unapplied > 0m),
            Queue(dashboard, "unapplied-cash").Count);
        Assert.Equal(ledger.Value!.Journals.Count(journal => journal.IsDraft),
            Queue(dashboard, "journals-draft").Count);
        Assert.Equal(tax.Value!.Computations.Count(computation => !computation.IsPosted),
            Queue(dashboard, "tax-unposted").Count);
        Assert.Equal(royalties.Value!.Jibs.Count(jib => !jib.IsPosted),
            Queue(dashboard, "jib-unposted").Count);
        Assert.Equal(royalties.Value.Royalties.Count(royalty => royalty.CanPay),
            Queue(dashboard, "royalties-payable").Count);
        Assert.Equal(warehouse.Value!.Reservations.Count(reservation => reservation.HoldsStock),
            Queue(dashboard, "stock-holds").Count);

        // ... and the counts are the expected ones from the seed.
        Assert.Equal(1, Queue(dashboard, "sales-draft").Count);
        Assert.Equal(2, Queue(dashboard, "sales-review").Count);
        Assert.Equal(2, Queue(dashboard, "ap-open").Count);
        Assert.Equal(1, Queue(dashboard, "ap-exceptions").Count);
        Assert.Equal(1, Queue(dashboard, "po-draft").Count);
        Assert.Equal(1, Queue(dashboard, "receipts-draft").Count);
        Assert.Equal(1, Queue(dashboard, "unapplied-cash").Count);
        Assert.Equal(1, Queue(dashboard, "journals-draft").Count);
        Assert.Equal(0, Queue(dashboard, "tax-unposted").Count);
        Assert.Equal(0, Queue(dashboard, "jib-unposted").Count);
        Assert.Equal(0, Queue(dashboard, "royalties-payable").Count);
        Assert.Equal(0, Queue(dashboard, "stock-holds").Count);

        // The balances are the sums of the workspaces the pages themselves would show.
        Assert.Equal(
            revenue.Value.Invoices.Where(invoice => invoice.IsIssued).Sum(invoice => invoice.BalanceDue),
            dashboard.Kpis.OpenAr);
        Assert.Equal(
            payables.Value.Invoices.Where(invoice => !invoice.IsPaid).Sum(invoice => invoice.Balance),
            dashboard.Kpis.OpenAp);

        // ---- AC-3: the period strip is the session's period ------------------
        Assert.Equal("open", dashboard.Period.State);
        Assert.True(dashboard.Period.Found);
        Assert.Equal(1, dashboard.Period.BlockingItems);
        Assert.Contains("November 2026", dashboard.Period.Text, StringComparison.Ordinal);
        Assert.Contains("2026-11", dashboard.Period.Text, StringComparison.Ordinal);
        Assert.Contains("draft journal", dashboard.Period.Text, StringComparison.Ordinal);

        // ---- AC-5: the stamp is on the page ----------------------------------
        Assert.False(string.IsNullOrWhiteSpace(dashboard.RefreshedText));
        Assert.Contains("last refreshed", dashboard.RefreshedText, StringComparison.Ordinal);

        // ---- AC-4: a queue without its permission is withheld, naming it -----
        var restricted = await service.LoadAsync(context with
        {
            Permissions = new OgPermissions([OgRoleNames.Viewer], [], permissionSourceAvailable: true),
        });

        Assert.True(restricted.Success, restricted.Message);

        var withheldTax = Queue(restricted.Value!, "tax-unposted");

        Assert.True(withheldTax.IsWithheld);
        Assert.Null(withheldTax.Count);
        Assert.Contains("accounting.tax.post", withheldTax.WithheldReason!, StringComparison.Ordinal);
        Assert.Contains("withheld", withheldTax.CountText, StringComparison.Ordinal);

        // An administrator still sees it.
        Assert.True(Queue(restricted.Value!, "journals-draft").IsWithheld);

        // A failed lookup is unknown, not denied (AC-4's own rule).
        var unknown = await service.LoadAsync(context with { Permissions = OgPermissions.None });

        Assert.True(unknown.Success, unknown.Message);
        Assert.Contains("not denied", Queue(unknown.Value!, "tax-unposted").WithheldReason!,
            StringComparison.Ordinal);

        // ---- AC-3: closing the period is reported as closed ------------------
        Execute(database,
            "UPDATE COMPANY_LEDGER_PERIOD SET STATUS = 'CLOSED' WHERE PERIOD_CODE = '2026-11';");

        var closed = await service.LoadAsync(context);

        Assert.True(closed.Success, closed.Message);
        Assert.Equal("closed", closed.Value!.Period.State);
        Assert.Contains("is closed", closed.Value.Period.Text, StringComparison.Ordinal);

        // ---- AC-1: an unreadable register is null with the reason, never zero -
        Execute(database, "DROP TABLE AR_INVOICE;");
        Execute(database, "DROP TABLE AR_INVOICE_LINE;");
        Execute(database, "DROP TABLE AR_CREDIT_MEMO;");

        var broken = await service.LoadAsync(context);

        Assert.True(broken.Success, broken.Message);

        var partial = broken.Value!;

        // The receivable balance is unknown, not zero, and the figure says so.
        Assert.Null(partial.Kpis.OpenAr);
        Assert.Contains("unavailable", partial.Kpis.OpenArText, StringComparison.Ordinal);
        Assert.DoesNotContain("0.00", partial.Kpis.OpenArText, StringComparison.Ordinal);
        Assert.True(partial.Kpis.AnyUnavailable);

        // Overdue needs both sides, so it is unknown too.
        Assert.Null(partial.Kpis.Overdue);

        // The payables side is unaffected and still read.
        Assert.Equal(3_700m, partial.Kpis.OpenAp);
        Assert.Equal(1, partial.Kpis.UnpostedJournals);

        // The unreadable queue says the records could not be read, and is not shown as empty.
        var salesQueue = Queue(partial, "sales-draft");

        Assert.Null(salesQueue.Count);
        Assert.False(salesQueue.IsWithheld);
        Assert.Equal("the records could not be read", salesQueue.StateText);
        Assert.Contains("the sales invoices", partial.FailureText, StringComparison.Ordinal);
        Assert.DoesNotContain("0.00", partial.Kpis.OverdueText, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ the schema

    private static void Seed(string database) =>
        Execute(database,
            """
            CREATE TABLE AR_INVOICE (
                AUTHOR TEXT,
                SUBTOTAL_AMOUNT REAL,
                TAX_AMOUNT REAL,
                TAX_RATE REAL,
                CONTRACT_ID TEXT,
                JOURNAL_ENTRY_ID TEXT,
                ISSUED_BY TEXT,
                ISSUED_AT TEXT,
                CREDITED_AMOUNT REAL,
                APPROVED_BY TEXT,
                APPROVED_AT TEXT,
                RETURN_REASON TEXT,
                OWNER_TENANT_ID TEXT,
                OWNER_COMPANY_BA_ID TEXT,
                FIELD_ID TEXT,
                SOURCE_INVOICE_ID TEXT,
                CURRENCY_CODE TEXT,
                PREPARATION_WORKFLOW_JSON TEXT,
                REVIEW_WORKFLOW_JSON TEXT,
                AR_INVOICE_ID TEXT PRIMARY KEY,
                INVOICE_NUMBER TEXT,
                CUSTOMER_BA_ID TEXT,
                INVOICE_DATE TEXT,
                DUE_DATE TEXT,
                TOTAL_AMOUNT REAL,
                PAID_AMOUNT REAL,
                BALANCE_DUE REAL,
                STATUS TEXT,
                ROW_ID TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE AR_INVOICE_LINE (
                AR_INVOICE_LINE_ID TEXT PRIMARY KEY,
                AR_INVOICE_ID TEXT,
                LINE_NUMBER INTEGER,
                COMMODITY TEXT,
                DESCRIPTION TEXT,
                QUANTITY REAL,
                QUANTITY_OUOM TEXT,
                UNIT_PRICE REAL,
                PRICE_OUOM TEXT,
                NET_AMOUNT REAL,
                TAX_AMOUNT REAL,
                LINE_TOTAL REAL,
                VOLUME_REFERENCE TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE AR_CREDIT_MEMO (
                AR_CREDIT_MEMO_ID TEXT PRIMARY KEY,
                AR_INVOICE_ID TEXT,
                CREDIT_MEMO_NUMBER TEXT,
                CREDIT_MEMO_DATE TEXT,
                CREDIT_AMOUNT REAL,
                REASON TEXT,
                STATUS TEXT,
                DESCRIPTION TEXT,
                ROW_ID TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE AR_RECEIPT (
                AR_RECEIPT_ID TEXT PRIMARY KEY,
                RECEIPT_NUMBER TEXT,
                CUSTOMER_BA_ID TEXT,
                RECEIPT_DATE TEXT,
                AMOUNT REAL,
                UNAPPLIED_AMOUNT REAL,
                CURRENCY_CODE TEXT,
                REFERENCE TEXT,
                BANK_ACCOUNT TEXT,
                STATUS TEXT,
                AUTHOR TEXT,
                JOURNAL_ENTRY_ID TEXT,
                POSTED_BY TEXT,
                POSTED_AT TEXT,
                APPROVED_BY TEXT,
                APPROVED_AT TEXT,
                RETURN_REASON TEXT,
                OWNER_TENANT_ID TEXT,
                OWNER_COMPANY_BA_ID TEXT,
                BOOK_ID TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE AR_PAYMENT (
                RECEIPT_ID TEXT,
                AR_PAYMENT_ID TEXT PRIMARY KEY,
                AR_INVOICE_ID TEXT,
                PAYMENT_NUMBER TEXT,
                PAYMENT_DATE TEXT,
                PAYMENT_AMOUNT REAL,
                PAYMENT_METHOD TEXT,
                CHECK_NUMBER TEXT,
                OWNER_TENANT_ID TEXT,
                OWNER_COMPANY_BA_ID TEXT,
                BOOK_ID TEXT,
                FIELD_ID TEXT,
                CURRENCY_CODE TEXT,
                PAYER_BA_ID TEXT,
                EXTERNAL_REFERENCE TEXT,
                EVIDENCE_REFERENCE TEXT,
                STATUS TEXT,
                SOURCE_REVISION TEXT,
                PREPARATION_WORKFLOW_JSON TEXT,
                REVIEW_WORKFLOW_JSON TEXT,
                ROW_ID TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE PURCHASE_ORDER (
                AUTHOR TEXT,
                APPROVED_BY TEXT,
                APPROVED_AT TEXT,
                AFE_ID TEXT,
                CLOSED_AT TEXT,
                CURRENCY_CODE TEXT,
                OWNER_TENANT_ID TEXT,
                OWNER_COMPANY_BA_ID TEXT,
                PURCHASE_ORDER_ID TEXT PRIMARY KEY,
                PO_NUMBER TEXT,
                VENDOR_BA_ID TEXT,
                PO_DATE TEXT,
                EXPECTED_DELIVERY_DATE TEXT,
                TOTAL_AMOUNT REAL,
                STATUS TEXT,
                DESCRIPTION TEXT,
                ROW_ID TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE PO_LINE_ITEM (
                INVENTORY_LINK_HISTORY_REVISION TEXT,
                INVENTORY_LINK_DECISION_JSON TEXT,
                INVENTORY_LINK_REVIEW_JSON TEXT,
                INVENTORY_ITEM_ID TEXT,
                QUANTITY_UOM TEXT,
                PO_LINE_ITEM_ID TEXT PRIMARY KEY,
                PURCHASE_ORDER_ID TEXT,
                LINE_NUMBER INTEGER,
                ITEM_DESCRIPTION TEXT,
                QUANTITY REAL,
                UNIT_PRICE REAL,
                LINE_TOTAL REAL,
                ROW_ID TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE PO_RECEIPT (
                ACCEPTED_QUANTITY REAL,
                REJECTED_QUANTITY REAL,
                RECEIVER_DECISION_JSON TEXT,
                QUANTITY_UOM TEXT,
                PO_RECEIPT_ID TEXT PRIMARY KEY,
                PURCHASE_ORDER_ID TEXT,
                PO_LINE_ITEM_ID TEXT,
                RECEIPT_DATE TEXT,
                RECEIVED_QUANTITY REAL,
                RECEIVED_BY TEXT,
                RECEIPT_STATUS TEXT,
                DESCRIPTION TEXT,
                ROW_ID TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE AP_INVOICE (
                AUTHOR TEXT,
                PURCHASE_ORDER_ID TEXT,
                AFE_ID TEXT,
                SUBTOTAL_AMOUNT REAL,
                TAX_AMOUNT REAL,
                MATCH_STATE TEXT,
                MATCH_EXCEPTIONS_TEXT TEXT,
                MATCH_POLICY_ID TEXT,
                EXCEPTION_APPROVED_BY TEXT,
                EXCEPTION_APPROVED_AT TEXT,
                VARIANCE_PROPOSAL_JSON TEXT,
                VARIANCE_DECISION_JSON TEXT,
                CORRECTION_HISTORY_JSON TEXT,
                OWNER_TENANT_ID TEXT,
                OWNER_COMPANY_BA_ID TEXT,
                CURRENCY_CODE TEXT,
                AP_INVOICE_ID TEXT PRIMARY KEY,
                INVOICE_NUMBER TEXT,
                VENDOR_BA_ID TEXT,
                INVOICE_DATE TEXT,
                DUE_DATE TEXT,
                TOTAL_AMOUNT REAL,
                PAID_AMOUNT REAL,
                BALANCE_DUE REAL,
                STATUS TEXT,
                ROW_ID TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE AP_INVOICE_LINE (
                AP_INVOICE_LINE_ID TEXT PRIMARY KEY,
                AP_INVOICE_ID TEXT,
                LINE_NUMBER INTEGER,
                ITEM_DESCRIPTION TEXT,
                QUANTITY REAL,
                QUANTITY_UOM TEXT,
                UNIT_PRICE REAL,
                LINE_TOTAL REAL,
                CURRENCY_CODE TEXT,
                PURCHASE_ORDER_ID TEXT,
                PO_LINE_ITEM_ID TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE AP_PAYMENT (
                RUN_ID TEXT,
                AP_PAYMENT_ID TEXT PRIMARY KEY,
                AP_INVOICE_ID TEXT,
                PAYMENT_NUMBER TEXT,
                PAYMENT_DATE TEXT,
                PAYMENT_AMOUNT REAL,
                PAYMENT_METHOD TEXT,
                CHECK_NUMBER TEXT,
                ROW_ID TEXT,
                PAYMENT_STATUS TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE AP_PAYMENT_RUN (
                AP_PAYMENT_RUN_ID TEXT PRIMARY KEY,
                RUN_NUMBER TEXT,
                RUN_DATE TEXT,
                TOTAL_AMOUNT REAL,
                CURRENCY_CODE TEXT,
                PAYMENT_METHOD TEXT,
                STATUS TEXT,
                AUTHOR TEXT,
                APPROVED_BY TEXT,
                APPROVED_AT TEXT,
                JOURNAL_ENTRY_ID TEXT,
                POSTED_BY TEXT,
                POSTED_AT TEXT,
                RETURN_REASON TEXT,
                OWNER_TENANT_ID TEXT,
                OWNER_COMPANY_BA_ID TEXT,
                BOOK_ID TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE ACCOUNTING_METHOD (
                OWNER_TENANT_ID TEXT,
                OWNER_COMPANY_BA_ID TEXT,
                MATCHING_POLICY_COMMAND_JSON TEXT,
                MATCHING_POLICY_JSON TEXT,
                MATCHING_POLICY_STATUS TEXT,
                ACCOUNTING_METHOD_ID TEXT PRIMARY KEY,
                FIELD_ID TEXT,
                COST_CENTER_ID TEXT,
                METHOD_TYPE TEXT,
                DESCRIPTION TEXT,
                ROW_ID TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE AFE_COMMITMENT (
                AFE_COMMITMENT_ID TEXT PRIMARY KEY,
                AFE_ID TEXT,
                REFERENCE TEXT,
                AMOUNT REAL,
                CURRENCY_CODE TEXT,
                COMMITTED_AT TEXT,
                ACTOR TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE GL_ACCOUNT (
                CONFIGURATION_COMMAND_JSON TEXT,
                OWNER_TENANT_ID TEXT,
                OWNER_COMPANY_BA_ID TEXT,
                BOOK_ID TEXT,
                CURRENCY_CODE TEXT,
                GL_ACCOUNT_ID TEXT PRIMARY KEY,
                ACCOUNT_NUMBER TEXT,
                ACCOUNT_NAME TEXT,
                ACCOUNT_TYPE TEXT,
                PARENT_ACCOUNT_ID TEXT,
                NORMAL_BALANCE TEXT,
                OPENING_BALANCE REAL,
                CURRENT_BALANCE REAL,
                DESCRIPTION TEXT,
                ROW_ID TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE JOURNAL_ENTRY (
                AUTHOR TEXT,
                POSTED_BY TEXT,
                POSTED_AT TEXT,
                PERIOD_CODE TEXT,
                REVERSAL_OF_ENTRY_ID TEXT,
                REVERSED_AT TEXT,
                JOURNAL_ENTRY_ID TEXT PRIMARY KEY,
                ENTRY_NUMBER TEXT,
                ENTRY_DATE TEXT,
                ENTRY_TYPE TEXT,
                STATUS TEXT,
                DESCRIPTION TEXT,
                REFERENCE_NUMBER TEXT,
                SOURCE_MODULE TEXT,
                TOTAL_DEBIT REAL,
                TOTAL_CREDIT REAL,
                ROW_ID TEXT,
                JOURNAL_DATE TEXT,
                JOURNAL_TYPE TEXT,
                JOURNAL_DESCRIPTION TEXT,
                SOURCE_SYSTEM TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE JOURNAL_ENTRY_LINE (
                JOURNAL_ENTRY_LINE_ID TEXT PRIMARY KEY,
                JOURNAL_ENTRY_ID TEXT,
                GL_ACCOUNT_ID TEXT,
                LINE_NUMBER INTEGER,
                DEBIT_AMOUNT REAL,
                CREDIT_AMOUNT REAL,
                DESCRIPTION TEXT,
                ROW_ID TEXT,
                COST_CENTER_ID TEXT,
                CURRENCY_CODE TEXT,
                EXCHANGE_RATE REAL,
                SHADOW_DEBIT_AMOUNT REAL,
                SHADOW_CREDIT_AMOUNT REAL,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE COMPANY_LEDGER_PERIOD (
                COMPANY_LEDGER_PERIOD_ID TEXT PRIMARY KEY,
                OWNER_TENANT_ID TEXT,
                OWNER_COMPANY_BA_ID TEXT,
                BOOK_ID TEXT,
                CURRENCY_CODE TEXT,
                CALENDAR_ID TEXT,
                PERIOD_CODE TEXT,
                START_DATE TEXT,
                END_DATE TEXT,
                STATUS TEXT,
                AUTHORITY_JSON TEXT,
                STATE_HISTORY_JSON TEXT,
                CLOSED_BY TEXT,
                CLOSED_AT TEXT,
                REOPENED_BY TEXT,
                REOPENED_AT TEXT,
                REOPEN_REASON TEXT,
                CLOSE_NOTE TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE GOVERNMENTAL_TAX_DATA (
                GOVERNMENTAL_TAX_DATA_ID TEXT PRIMARY KEY,
                SEVERANCE_TAX REAL,
                AD_VALOREM_TAX REAL,
                TOTAL_TAXES REAL,
                ROW_ID TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE TAX_TRANSACTION (
                TAX_RATE REAL,
                TAX_BASIS REAL,
                BASIS_TEXT TEXT,
                RATE_EFFECTIVE_DATE TEXT,
                RATE_SOURCE TEXT,
                PERIOD_FROM TEXT,
                PERIOD_TO TEXT,
                FISCAL_YEAR TEXT,
                CURRENCY_CODE TEXT,
                STATUS TEXT,
                AUTHOR TEXT,
                JOURNAL_ENTRY_ID TEXT,
                POSTED_BY TEXT,
                POSTED_AT TEXT,
                TAX_TRANSACTION_ID TEXT PRIMARY KEY,
                PROPERTY_ID TEXT,
                TAX_TYPE TEXT,
                TAX_DATE TEXT,
                TAX_AMOUNT REAL,
                TAX_JURISDICTION TEXT,
                DESCRIPTION TEXT,
                ROW_ID TEXT,
                REMARKS TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE DEFERRED_TAX_BALANCE (
                DEFERRED_TAX_BALANCE_ID TEXT PRIMARY KEY,
                PROPERTY_ID TEXT,
                PERIOD_END_DATE TEXT,
                DEFERRED_TAX_ASSET REAL,
                DEFERRED_TAX_LIABILITY REAL,
                NOTES TEXT,
                ROW_ID TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE ROYALTY_CALCULATION (
                DISPUTED_IND TEXT,
                DISPUTE_REFERENCE TEXT,
                DISPUTE_REASON TEXT,
                DISPUTED_BY TEXT,
                DISPUTED_AT TEXT,
                DISPUTE_RESOLVED_BY TEXT,
                DISPUTE_RESOLVED_AT TEXT,
                PAID_AT TEXT,
                PAID_BY TEXT,
                PAID_AMOUNT REAL,
                PAYMENT_JOURNAL_ENTRY_ID TEXT,
                AUTHOR TEXT,
                CURRENCY_CODE TEXT,
                JOURNAL_ENTRY_ID TEXT,
                ROYALTY_CALCULATION_ID TEXT PRIMARY KEY,
                CALCULATION_DATE TEXT,
                PROPERTY_OR_LEASE_ID TEXT,
                ALLOCATION_RESULT_ID TEXT,
                ROYALTY_INTEREST_ID TEXT,
                ROYALTY_OWNER_ID TEXT,
                GROSS_REVENUE REAL,
                ROYALTY_DEDUCTIONS_ID TEXT,
                NET_REVENUE REAL,
                ROYALTY_INTEREST REAL,
                ROYALTY_AMOUNT REAL,
                ALLOCATION_DETAIL_ID TEXT,
                TRANSPORTATION_COST REAL,
                AD_VALOREM_TAX REAL,
                SEVERANCE_TAX REAL,
                ROYALTY_STATUS TEXT,
                ROYALTY_TYPE TEXT,
                ROYALTY_BASIS TEXT,
                FLUID_TYPE TEXT,
                PRODUCTION_PERIOD_START TEXT,
                GROSS_VOLUME REAL,
                GROSS_VOLUME_OUOM TEXT,
                PRICE_PER_UNIT REAL,
                PAYMENT_STATUS TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE ROYALTY_DEDUCTIONS (
                ROYALTY_DEDUCTIONS_ID TEXT PRIMARY KEY,
                ROW_ID TEXT,
                PRODUCTION_TAXES REAL,
                TRANSPORTATION_COSTS REAL,
                PROCESSING_COSTS REAL,
                MARKETING_COSTS REAL,
                OTHER_DEDUCTIONS REAL,
                TOTAL_DEDUCTIONS REAL,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE OWNERSHIP_INTEREST (
                OWNERSHIP_ID TEXT PRIMARY KEY,
                OWNER_ID TEXT,
                PROPERTY_OR_LEASE_ID TEXT,
                WORKING_INTEREST REAL,
                NET_REVENUE_INTEREST REAL,
                ROYALTY_INTEREST REAL,
                OVERRIDING_ROYALTY_INTEREST REAL,
                EFFECTIVE_START_DATE TEXT,
                EFFECTIVE_END_DATE TEXT,
                DIVISION_ORDER_ID TEXT,
                ROW_ID TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE JOINT_INTEREST_STATEMENT (
                STATUS TEXT,
                PARTICIPANT_ID TEXT,
                AUTHOR TEXT,
                APPROVED_BY TEXT,
                APPROVED_AT TEXT,
                POSTED_BY TEXT,
                POSTED_AT TEXT,
                RETURN_REASON TEXT,
                JOURNAL_ENTRY_ID TEXT,
                SOURCE_VOLUME_REFERENCE TEXT,
                CURRENCY_CODE TEXT,
                JOINT_INTEREST_STATEMENT_ID TEXT PRIMARY KEY,
                REPORT_TYPE TEXT,
                REPORT_PERIOD_START TEXT,
                REPORT_PERIOD_END TEXT,
                GENERATION_DATE TEXT,
                GENERATED_BY TEXT,
                JIB_ID TEXT,
                OPERATOR TEXT,
                TOTAL_CHARGES REAL,
                TOTAL_CREDITS REAL,
                NET_AMOUNT REAL,
                ROW_ID TEXT,
                STATEMENT_ID TEXT,
                PROPERTY_OR_LEASE_ID TEXT,
                STATEMENT_PERIOD_START TEXT,
                STATEMENT_PERIOD_END TEXT,
                GENERATED_DATE TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE JIB_CHARGE (
                JIB_CHARGE_ID TEXT PRIMARY KEY,
                JOINT_INTEREST_STATEMENT_ID TEXT,
                DESCRIPTION TEXT,
                AMOUNT REAL,
                CATEGORY TEXT,
                ROW_ID TEXT,
                CHARGE_ID TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE JIB_CREDIT (
                JIB_CREDIT_ID TEXT PRIMARY KEY,
                JOINT_INTEREST_STATEMENT_ID TEXT,
                DESCRIPTION TEXT,
                AMOUNT REAL,
                CATEGORY TEXT,
                ROW_ID TEXT,
                CREDIT_ID TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE COST_ALLOCATION (
                COST_ALLOCATION_ID TEXT PRIMARY KEY,
                COST_TRANSACTION_ID TEXT,
                PROPERTY_ID TEXT,
                WELL_ID TEXT,
                COST_CENTER_ID TEXT,
                ALLOCATED_AMOUNT REAL,
                ALLOCATION_PERCENTAGE REAL,
                ALLOCATION_METHOD TEXT,
                DESCRIPTION TEXT,
                ROW_ID TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE INVENTORY_ITEM (
                LINKED_ASSET_TYPE TEXT,
                LINKED_ASSET_ID TEXT,
                OWNER_TENANT_ID TEXT,
                OWNER_COMPANY_BA_ID TEXT,
                WAREHOUSE_ID TEXT,
                LOCATION_ID TEXT,
                CURRENCY_CODE TEXT,
                INVENTORY_ITEM_ID TEXT PRIMARY KEY,
                ITEM_NUMBER TEXT,
                ITEM_NAME TEXT,
                ITEM_TYPE TEXT,
                UNIT_OF_MEASURE TEXT,
                QUANTITY_ON_HAND REAL,
                UNIT_COST REAL,
                TOTAL_VALUE REAL,
                VALUATION_METHOD TEXT,
                DESCRIPTION TEXT,
                ROW_ID TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE INVENTORY_TRANSACTION (
                CONDITION_CODE TEXT,
                RESTOCK_DECISION TEXT,
                RESERVATION_ID TEXT,
                JOURNAL_ENTRY_ID TEXT,
                ACTOR TEXT,
                WORK_ORDER_MATERIAL_USAGE_JSON TEXT,
                PURCHASE_RECEIPT_STOCK_JSON TEXT,
                INVENTORY_TRANSACTION_ID TEXT PRIMARY KEY,
                INVENTORY_ITEM_ID TEXT,
                TRANSACTION_TYPE TEXT,
                TRANSACTION_DATE TEXT,
                QUANTITY REAL,
                UNIT_COST REAL,
                TOTAL_COST REAL,
                REFERENCE_NUMBER TEXT,
                DESCRIPTION TEXT,
                NOTES TEXT,
                ROW_ID TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE INVENTORY_RESERVATION (
                INVENTORY_RESERVATION_ID TEXT PRIMARY KEY,
                INVENTORY_ITEM_ID TEXT,
                QUANTITY REAL,
                HOLDING_DOCUMENT TEXT,
                HOLDING_KIND TEXT,
                RESERVED_AT TEXT,
                ACTOR TEXT,
                STATUS TEXT,
                ISSUED_TRANSACTION_ID TEXT,
                RELEASED_BY TEXT,
                RELEASED_AT TEXT,
                RELEASE_REASON TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            INSERT INTO GL_ACCOUNT (GL_ACCOUNT_ID, ACCOUNT_NUMBER, ACCOUNT_NAME, ACCOUNT_TYPE, NORMAL_BALANCE,
                CURRENCY_CODE, OPENING_BALANCE, ACTIVE_IND) VALUES
                ('A-1000', '1000', 'Cash', 'ASSET', 'DEBIT', 'USD', 5000, 'Y'),
                ('A-1110', '1110', 'Accounts receivable', 'ASSET', 'DEBIT', 'USD', 0, 'Y'),
                ('A-4001', '4001', 'Revenue', 'REVENUE', 'CREDIT', 'USD', 0, 'Y'),
                ('A-2000', '2000', 'Accounts payable', 'LIABILITY', 'CREDIT', 'USD', 0, 'Y'),
                ('A-6001', '6001', 'Operating expense', 'EXPENSE', 'DEBIT', 'USD', 0, 'Y');

            INSERT INTO AR_INVOICE (AR_INVOICE_ID, INVOICE_NUMBER, CUSTOMER_BA_ID, INVOICE_DATE, DUE_DATE,
                CURRENCY_CODE, TOTAL_AMOUNT, PAID_AMOUNT, BALANCE_DUE, CREDITED_AMOUNT, STATUS, AUTHOR,
                ACTIVE_IND) VALUES
                ('AR-1', 'INV-1', 'BUYER-1', '2026-02-01', '2026-03-01', 'USD', 1000, 0, 1000, 0, 'ISSUED',
                 'sara', 'Y'),
                ('AR-2', 'INV-2', 'BUYER-2', '2026-11-01', '2026-12-31', 'USD', 2000, 500, 1500, 0, 'ISSUED',
                 'sara', 'Y'),
                ('AR-3', 'INV-3', 'BUYER-3', '2026-11-05', '2026-12-15', 'USD', 500, 0, 500, 0, 'DRAFT',
                 'sara', 'Y');

            INSERT INTO AR_RECEIPT (AR_RECEIPT_ID, RECEIPT_NUMBER, CUSTOMER_BA_ID, RECEIPT_DATE, AMOUNT,
                UNAPPLIED_AMOUNT, CURRENCY_CODE, REFERENCE, BANK_ACCOUNT, STATUS, AUTHOR, OWNER_TENANT_ID,
                OWNER_COMPANY_BA_ID, BOOK_ID, ACTIVE_IND) VALUES
                ('RCP-1', 'RCPT-1', 'BUYER-1', '2026-11-06', 800, 300, 'USD', 'REF-1', 'BANK-1', 'DRAFT',
                 'sara', 'SMOKE', 'SMOKE', 'MAIN', 'Y');

            INSERT INTO AP_INVOICE (AP_INVOICE_ID, INVOICE_NUMBER, VENDOR_BA_ID, INVOICE_DATE, DUE_DATE,
                CURRENCY_CODE, TOTAL_AMOUNT, PAID_AMOUNT, BALANCE_DUE, STATUS, MATCH_STATE, AUTHOR, ACTIVE_IND) VALUES
                ('AP-1', 'BILL-1', 'VENDOR-1', '2026-01-15', '2026-02-01', 'USD', 4000, 1000, 3000, 'ISSUED',
                 'MATCHED', 'sara', 'Y'),
                ('AP-2', 'BILL-2', 'VENDOR-1', '2026-01-20', '2026-02-20', 'USD', 2000, 2000, 0, 'PAID',
                 'MATCHED', 'sara', 'Y'),
                ('AP-3', 'BILL-3', 'VENDOR-2', '2026-10-20', '2026-11-20', 'USD', 700, 0, 700, 'ISSUED',
                 'EXCEPTION', 'sara', 'Y');

            INSERT INTO PURCHASE_ORDER (PURCHASE_ORDER_ID, PO_NUMBER, VENDOR_BA_ID, PO_DATE, CURRENCY_CODE,
                TOTAL_AMOUNT, STATUS, AUTHOR, ACTIVE_IND) VALUES
                ('PO-1', 'PO-2026-0001', 'VENDOR-1', '2026-10-01', 'USD', 5000, 'DRAFT', 'sara', 'Y');

            INSERT INTO JOURNAL_ENTRY (JOURNAL_ENTRY_ID, ENTRY_NUMBER, ENTRY_DATE, ENTRY_TYPE, STATUS,
                DESCRIPTION, SOURCE_MODULE, TOTAL_DEBIT, TOTAL_CREDIT, PERIOD_CODE, AUTHOR, ACTIVE_IND) VALUES
                ('JE-1', 'JE-2026-0001', '2026-11-05', 'GENERAL', 'DRAFT', 'draft adjustment', 'MANUAL',
                 100, 100, '2026-11', 'sara', 'Y'),
                ('JE-2', 'JE-2026-0002', '2026-11-06', 'RECEIPT', 'POSTED', 'cash received', 'AR_RECEIPT',
                 250, 250, '2026-11', 'sara', 'Y');

            INSERT INTO JOURNAL_ENTRY_LINE (JOURNAL_ENTRY_LINE_ID, JOURNAL_ENTRY_ID, GL_ACCOUNT_ID, LINE_NUMBER,
                DEBIT_AMOUNT, CREDIT_AMOUNT, DESCRIPTION, ACTIVE_IND) VALUES
                ('JL-1', 'JE-2', 'A-1000', 1, 250, 0, 'cash debit', 'Y'),
                ('JL-2', 'JE-2', 'A-4001', 2, 0, 250, 'revenue credit', 'Y');

            INSERT INTO COMPANY_LEDGER_PERIOD (COMPANY_LEDGER_PERIOD_ID, OWNER_TENANT_ID, OWNER_COMPANY_BA_ID,
                BOOK_ID, CURRENCY_CODE, CALENDAR_ID, PERIOD_CODE, START_DATE, END_DATE, STATUS, AUTHORITY_JSON)
            VALUES
                ('P-11', 'SMOKE', 'SMOKE', 'MAIN', 'USD', 'MONTHLY', '2026-11', '2026-11-01', '2026-11-30',
                 'OPEN', '{}'),
                ('P-12', 'SMOKE', 'SMOKE', 'MAIN', 'USD', 'MONTHLY', '2026-12', '2026-12-01', '2026-12-31',
                 'OPEN', '{}');
            """);

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

    private static OgDashboardQueue Queue(OgAccountingDashboard dashboard, string key) =>
        dashboard.Queues.Single(queue => string.Equals(queue.Key, key, StringComparison.Ordinal));

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
