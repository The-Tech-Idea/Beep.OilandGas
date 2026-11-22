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
/// ACC-05 against a real SQLite database: the whole procure-to-pay journey - order, approval, receipt, invoice, match,
/// exception, run, payment - and the six refusals that keep it honest: a receipt above the order (AC-1), a price outside
/// the recorded tolerance turned into an exception rather than a pass (AC-2), a duplicate supplier invoice number (AC-3),
/// a commitment above the AFE remaining (AC-4), one journal for a whole run (AC-5), and the author refused as approver of
/// either the exception or the run (AC-6). Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class PayablesGatewayTests
{
    private const string ConnectionName = "acc05-payables";
    private const string PolicyJson =
        "{\"PolicyVersion\":\"v1\",\"CurrencyCode\":\"USD\",\"QuantityTolerancePercent\":5," +
        "\"PriceTolerancePercent\":5,\"UnitRule\":\"EXACT_RECORDED_UNITS\"," +
        "\"CurrencyRule\":\"EXACT_RECORDED_CURRENCY\",\"TaxTreatment\":\"SEPARATE_REVIEW_REQUIRED\"," +
        "\"RoundingRule\":\"EXACT_SOURCE_AMOUNTS\",\"NonPurchaseInvoiceRule\":\"SEPARATE_REVIEW_REQUIRED\"}";

    [IntegrationFact]
    public async Task The_order_to_payment_journey_refuses_every_shortcut()
    {        var root = Path.Combine(OgPaths.DataRoot, "acc05-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasAcc05"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "payables.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "payables.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgPayablesService(() => source, reporter);
        var context = new OgPayableContext("SMOKE", "MAIN");

        // ---- the workspace, and the policy whose tolerances will be applied ----
        var loaded = await service.LoadAsync(context);

        Assert.True(loaded.Success, loaded.Message);
        Assert.Empty(loaded.Value!.Orders);
        Assert.False(loaded.Value.HistoryFailed);
        Assert.Equal("POL-1", loaded.Value.PolicyId);
        Assert.Contains("quantity tolerance 5", loaded.Value.PolicyText, StringComparison.Ordinal);
        Assert.Contains("price tolerance 5", loaded.Value.PolicyText, StringComparison.Ordinal);

        // ---- raise the order ---------------------------------------------------
        var raised = await service.RaiseOrderAsync(OrderInputs(), context, "sara");

        Assert.True(raised.Success, raised.Message + " :: " + string.Join(" | ", reporter.Failures));

        var order = raised.Value!.SelectedOrder!;

        Assert.Equal(ProcurementCodes.OrderDraft, order.State);
        Assert.Equal("sara", order.Author);
        Assert.StartsWith("PO-", order.Number, StringComparison.Ordinal);
        Assert.Equal(9_000m, order.Total);
        Assert.Equal(2, order.Lines.Count);
        Assert.Equal(0m, order.Lines[0].Received);
        Assert.Equal(100m, order.Lines[0].Open);

        var casingLine = order.Lines.Single(line => line.Description == "Casing");

        // ---- a draft order accepts no goods ------------------------------------
        var tooEarly = await service.ReceiveAsync(order.Id, casingLine.Id, 10m, "sara");

        Assert.False(tooEarly.Success);
        Assert.Equal(PayableRules.CodeOrderNotApproved, tooEarly.Reference);
        Assert.Equal(0, Count(database, "PO_RECEIPT"));

        // ---- the author cannot approve their own order -------------------------
        var selfApproved = await service.ApproveOrderAsync(order.Id, "sara");

        Assert.False(selfApproved.Success);
        Assert.Equal(PayableRules.CodeAuthorCannotApprove, selfApproved.Reference);
        Assert.Equal(0, Count(database, "PURCHASE_ORDER WHERE STATUS = 'APPROVED'"));

        var approved = await service.ApproveOrderAsync(order.Id, "omar");

        Assert.True(approved.Success, approved.Message);
        Assert.Equal(ProcurementCodes.OrderApproved, approved.Value!.SelectedOrder!.State);
        Assert.Equal(1, Count(database,
            $"PURCHASE_ORDER WHERE PURCHASE_ORDER_ID = '{order.Id}' AND STATUS = 'APPROVED' " +
            "AND APPROVED_BY = 'omar' AND APPROVED_AT IS NOT NULL"));

        // ---- AC-1 and SG-10-6: a receipt above the order is refused ------------
        var overReceived = await service.ReceiveAsync(order.Id, casingLine.Id, 150m, "sara");

        Assert.False(overReceived.Success);
        Assert.Equal(PayableRules.CodeReceiptExceedsOrder, overReceived.Reference);
        Assert.Contains("50 more than was ordered", overReceived.Message!, StringComparison.Ordinal);
        Assert.Contains("PO-", overReceived.Message!, StringComparison.Ordinal);
        Assert.Equal(0, Count(database, "PO_RECEIPT"));

        // Exactly the ordered quantity is booked in, and one more is then refused.
        var received = await service.ReceiveAsync(order.Id, casingLine.Id, 100m, "sara");

        Assert.True(received.Success, received.Message);
        Assert.Equal(1, Count(database, "PO_RECEIPT WHERE RECEIVED_QUANTITY = 100 AND RECEIPT_STATUS = 'RECEIVED'"));
        Assert.Equal(100m, received.Value!.SelectedOrder!.Lines.Single(line => line.Id == casingLine.Id).Received);

        Assert.Equal(
            PayableRules.CodeReceiptExceedsOrder,
            (await service.ReceiveAsync(order.Id, casingLine.Id, 1m, "sara")).Reference);
        Assert.Equal(1, Count(database, "PO_RECEIPT"));

        // ---- capture an invoice that matches -----------------------------------
        var captured = await service.CaptureAsync(
            InvoiceInputs("SUP-1001", tax: 1_000m, price: 50m, quantity: 100m, casingLine.Id, order.Id),
            context,
            "sara");

        Assert.True(captured.Success, captured.Message);

        var matched = captured.Value!.SelectedInvoice!;

        Assert.Equal("SUP-1001", matched.Number);
        Assert.Equal(5_000m, matched.Subtotal);
        Assert.Equal(1_000m, matched.Tax);
        Assert.Equal(6_000m, matched.Total);
        Assert.Equal(6_000m, matched.Balance);
        Assert.Equal(ProcurementCodes.MatchMatched, matched.MatchState);
        Assert.True(matched.IsPayable);
        Assert.Empty(matched.Exceptions);
        Assert.Equal("POL-1", matched.PolicyId);
        Assert.Equal(1, Count(database,
            "AP_INVOICE WHERE INVOICE_NUMBER = 'SUP-1001' AND MATCH_STATE = 'MATCHED' " +
            "AND SUBTOTAL_AMOUNT = 5000 AND TAX_AMOUNT = 1000 AND TOTAL_AMOUNT = 6000 " +
            "AND MATCH_POLICY_ID = 'POL-1' AND AUTHOR = 'sara'"));

        // The commitment is written against the authorisation, so AC-4 has something to measure next time.
        Assert.Equal(1, Count(database, "AFE_COMMITMENT WHERE AFE_ID = 'AFE-1' AND AMOUNT = 5000"));

        // ---- AC-3: a duplicate supplier invoice number is refused --------------
        var duplicate = await service.CaptureAsync(
            InvoiceInputs("SUP-1001", tax: 1_000m, price: 50m, quantity: 100m, casingLine.Id, order.Id),
            context,
            "nadia");

        Assert.False(duplicate.Success);
        Assert.Equal(PayableRules.CodeDuplicateInvoiceNumber, duplicate.Reference);
        Assert.Contains("SUP-1001", duplicate.Message!, StringComparison.Ordinal);
        Assert.Equal(1, Count(database, "AP_INVOICE WHERE INVOICE_NUMBER = 'SUP-1001'"));

        // ---- AC-2: a price outside tolerance is an exception, not a pass -------
        var outside = await service.CaptureAsync(
            InvoiceInputs("SUP-1002", tax: 0m, price: 60m, quantity: 50m, casingLine.Id, order.Id),
            context,
            "sara");

        Assert.True(outside.Success, outside.Message);

        var exception = outside.Value!.SelectedInvoice!;

        Assert.Equal(ProcurementCodes.MatchException, exception.MatchState);
        Assert.True(exception.NeedsExceptionApproval);
        Assert.False(exception.IsPayable);
        Assert.Contains(exception.Exceptions,
            row => row.Kind == ProcurementCodes.ExceptionPriceOutsideTolerance);
        Assert.Contains("tolerance", exception.ExceptionsText, StringComparison.Ordinal);
        Assert.Equal(1, Count(database,
            "AP_INVOICE WHERE INVOICE_NUMBER = 'SUP-1002' AND MATCH_STATE = 'EXCEPTION' " +
            "AND MATCH_EXCEPTIONS_TEXT LIKE '%tolerance%'"));

        // ---- AC-6: the author cannot approve the exception ---------------------
        var byAuthor = await service.ApproveExceptionAsync(exception.Id, "sara");

        Assert.False(byAuthor.Success);
        Assert.Equal(PayableRules.CodeAuthorCannotApprove, byAuthor.Reference);
        Assert.Contains("sara", byAuthor.Message!, StringComparison.Ordinal);
        Assert.Equal(0, Count(database, "AP_INVOICE WHERE MATCH_STATE = 'EXCEPTION_APPROVED'"));

        var approvedException = await service.ApproveExceptionAsync(exception.Id, "nadia");

        Assert.True(approvedException.Success, approvedException.Message);
        Assert.Equal(ProcurementCodes.MatchExceptionApproved,
            approvedException.Value!.SelectedInvoice!.MatchState);
        Assert.True(approvedException.Value.SelectedInvoice.IsPayable);
        Assert.Equal(1, Count(database,
            $"AP_INVOICE WHERE AP_INVOICE_ID = '{exception.Id}' AND MATCH_STATE = 'EXCEPTION_APPROVED' " +
            "AND EXCEPTION_APPROVED_BY = 'nadia' AND EXCEPTION_APPROVED_AT IS NOT NULL"));

        // ---- AC-4: a commitment above the AFE remaining is refused -------------
        var overCommit = await service.CaptureAsync(
            InvoiceInputs("SUP-1003", tax: 0m, price: 50m, quantity: 4_000m, casingLine.Id, order.Id),
            context,
            "sara");

        Assert.False(overCommit.Success);
        Assert.Equal(PayableRules.CodeCommitmentExceedsAfe, overCommit.Reference);
        Assert.Contains("AFE-", overCommit.Message!, StringComparison.Ordinal);
        Assert.Contains("192,000.00", overCommit.Message!, StringComparison.Ordinal);
        Assert.Equal(0, Count(database, "AP_INVOICE WHERE INVOICE_NUMBER = 'SUP-1003'"));

        // ---- AC-5: a run pays several invoices with exactly one journal -------
        var proposed = await service.ProposeRunAsync(new DateTime(2026, 5, 25), context, "sara");

        Assert.True(proposed.Success, proposed.Message);

        var run = proposed.Value!.SelectedRun!;

        Assert.Equal(ProcurementCodes.RunDraft, run.Status);
        Assert.Equal(9_000m, run.Total);
        Assert.Equal(2, run.InvoiceNumbers.Count);
        Assert.StartsWith("PAY-", run.Number, StringComparison.Ordinal);
        Assert.Equal(2, Count(database, "AP_PAYMENT WHERE PAYMENT_STATUS = 'DRAFT'"));

        // ---- AC-6: the proposer cannot approve the run -------------------------
        var runByAuthor = await service.ReviewRunAsync(run.Id, approve: true, reason: string.Empty, actor: "sara");

        Assert.False(runByAuthor.Success);
        Assert.Equal(PayableRules.CodeAuthorCannotApprove, runByAuthor.Reference);
        Assert.Equal(0, Count(database, "AP_PAYMENT_RUN WHERE STATUS = 'APPROVED'"));

        var approvedRun = await service.ReviewRunAsync(run.Id, approve: true, reason: string.Empty, actor: "nadia");

        Assert.True(approvedRun.Success, approvedRun.Message);
        Assert.Equal(ProcurementCodes.RunApproved, approvedRun.Value!.SelectedRun!.Status);

        // A draft run is not posted; an approved one is.
        var posted = await service.PostRunAsync(run.Id, context, "nadia");

        Assert.True(posted.Success, posted.Message);

        var postedRun = posted.Value!.SelectedRun!;

        Assert.Equal(ProcurementCodes.RunPosted, postedRun.Status);
        Assert.True(postedRun.HasJournal);

        // One entry for the whole run, balanced: payables down, cash down.
        Assert.Equal(1, Count(database, "JOURNAL_ENTRY WHERE SOURCE_MODULE = 'AP_PAYMENT_RUN'"));
        Assert.Equal(1, Count(database,
            "JOURNAL_ENTRY WHERE SOURCE_MODULE = 'AP_PAYMENT_RUN' AND TOTAL_DEBIT = 9000 AND TOTAL_CREDIT = 9000"));
        Assert.Equal(1, Count(database,
            "JOURNAL_ENTRY_LINE WHERE DEBIT_AMOUNT = 9000 AND GL_ACCOUNT_ID = 'A-2000'"));
        Assert.Equal(1, Count(database,
            "JOURNAL_ENTRY_LINE WHERE CREDIT_AMOUNT = 9000 AND GL_ACCOUNT_ID = 'A-1000'"));
        Assert.Equal(2, Count(database, "AP_PAYMENT WHERE PAYMENT_STATUS = 'POSTED'"));

        // Both invoices are paid in full by it.
        Assert.Equal(1, Count(database,
            "AP_INVOICE WHERE INVOICE_NUMBER = 'SUP-1001' AND PAID_AMOUNT = 6000 AND BALANCE_DUE = 0 " +
            "AND STATUS = 'PAID'"));
        Assert.Equal(1, Count(database,
            "AP_INVOICE WHERE INVOICE_NUMBER = 'SUP-1002' AND PAID_AMOUNT = 3000 AND BALANCE_DUE = 0 " +
            "AND STATUS = 'PAID'"));

        // ---- posting again writes no second journal ---------------------------
        var again = await service.PostRunAsync(run.Id, context, "nadia");

        Assert.True(again.Success, again.Message);
        Assert.Equal(1, Count(database, "JOURNAL_ENTRY WHERE SOURCE_MODULE = 'AP_PAYMENT_RUN'"));

        // ---- a lost acknowledgement neither duplicates nor pays twice ---------
        Execute(database,
            $"UPDATE AP_PAYMENT_RUN SET JOURNAL_ENTRY_ID = NULL, STATUS = 'APPROVED', POSTED_BY = NULL, " +
            $"POSTED_AT = NULL WHERE AP_PAYMENT_RUN_ID = '{run.Id}';");

        var recovered = await service.PostRunAsync(run.Id, context, "nadia");

        Assert.True(recovered.Success, recovered.Message);
        Assert.Equal(1, Count(database, "JOURNAL_ENTRY WHERE SOURCE_MODULE = 'AP_PAYMENT_RUN'"));
        Assert.Equal(1, Count(database,
            "AP_INVOICE WHERE INVOICE_NUMBER = 'SUP-1001' AND PAID_AMOUNT = 6000 AND BALANCE_DUE = 0"));
        Assert.Equal(1, Count(database,
            "AP_INVOICE WHERE INVOICE_NUMBER = 'SUP-1002' AND PAID_AMOUNT = 3000 AND BALANCE_DUE = 0"));

        // ---- a non-PO invoice has nothing to match and is marked as such ------
        var nonPo = await service.CaptureAsync(
            InvoiceInputs("SUP-1004", tax: 0m, price: 100m, quantity: 2m, null, null),
            context,
            "sara");

        Assert.True(nonPo.Success, nonPo.Message);
        Assert.Equal(ProcurementCodes.MatchNotApplicable, nonPo.Value!.SelectedInvoice!.MatchState);
        Assert.True(nonPo.Value.SelectedInvoice.IsPayable);
        Assert.Empty(nonPo.Value.SelectedInvoice.Exceptions);

        // ---- a run with nothing due is refused --------------------------------
        Assert.Equal(
            PayableRules.CodeRunNothingDue,
            (await service.ProposeRunAsync(new DateTime(2026, 1, 1), context, "sara")).Reference);

        // ---- an unavailable register is a state -------------------------------
        Execute(database, "DROP TABLE AP_PAYMENT;");
        Execute(database, "DROP TABLE AP_PAYMENT_RUN;");

        var unavailable = await service.LoadAsync(context);

        Assert.True(unavailable.Success, unavailable.Message);
        Assert.True(unavailable.Value!.HistoryFailed);
        Assert.Empty(unavailable.Value.Orders);
        Assert.Contains("could not be read", unavailable.Value.HistoryStateText, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ the schema

    private static void Seed(string database) =>
        Execute(database,
            """
            CREATE TABLE PURCHASE_ORDER (
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
                AUTHOR TEXT,
                APPROVED_BY TEXT,
                APPROVED_AT TEXT,
                AFE_ID TEXT,
                CLOSED_AT TEXT,
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
                AP_PAYMENT_ID TEXT PRIMARY KEY,
                AP_INVOICE_ID TEXT,
                PAYMENT_NUMBER TEXT,
                PAYMENT_DATE TEXT,
                PAYMENT_AMOUNT REAL,
                PAYMENT_METHOD TEXT,
                CHECK_NUMBER TEXT,
                ROW_ID TEXT,
                PAYMENT_STATUS TEXT,
                RUN_ID TEXT,
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

            CREATE TABLE AFE (
                AFE_ID TEXT PRIMARY KEY,
                AFE_NUMBER TEXT,
                AFE_NAME TEXT,
                PROPERTY_ID TEXT,
                FIELD_ID TEXT,
                ESTIMATED_COST REAL,
                ACTUAL_COST REAL,
                STATUS TEXT,
                APPROVAL_DATE TEXT,
                DESCRIPTION TEXT,
                ROW_ID TEXT,
                AFE_KIND TEXT,
                ENTITY_ID TEXT,
                AUTHOR TEXT,
                APPROVER TEXT,
                APPROVED_AMOUNT REAL,
                REVISION_NO INTEGER,
                SUPERSEDES_AFE_ID TEXT,
                ROOT_AFE_ID TEXT,
                REVISION_REASON TEXT,
                CLOSED_AT TEXT,
                CLOSED_BY TEXT,
                CLOSE_NOTE TEXT,
                SOURCE_PROPOSAL_ID TEXT,
                CONDITIONS_TEXT TEXT,
                OWNER_TENANT_ID TEXT,
                OWNER_COMPANY_BA_ID TEXT,
                WORK_ORDER_ID TEXT,
                BOOK_ID TEXT,
                CURRENCY_CODE TEXT,
                WORK_ORDER_BUDGET_JSON TEXT,
                WORK_ORDER_BUDGET_REVISION TEXT,
                FDP_BASELINE_ID TEXT,
                DEVELOPMENT_BUDGET_JSON TEXT,
                DEVELOPMENT_BUDGET_REVISION TEXT,
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
                AUTHOR TEXT,
                POSTED_BY TEXT,
                POSTED_AT TEXT,
                PERIOD_CODE TEXT,
                REVERSAL_OF_ENTRY_ID TEXT,
                REVERSED_AT TEXT,
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

            INSERT INTO GL_ACCOUNT (GL_ACCOUNT_ID, ACCOUNT_NUMBER, ACCOUNT_NAME, ACCOUNT_TYPE,
                NORMAL_BALANCE, CURRENCY_CODE, ACTIVE_IND) VALUES
                ('A-1000', '1000', 'Cash', 'ASSET', 'DEBIT', 'USD', 'Y'),
                ('A-2000', '2000', 'Accounts payable', 'LIABILITY', 'CREDIT', 'USD', 'Y');

            INSERT INTO ACCOUNTING_METHOD (ACCOUNTING_METHOD_ID, METHOD_TYPE, MATCHING_POLICY_JSON,
                MATCHING_POLICY_STATUS, EFFECTIVE_DATE, ACTIVE_IND, DESCRIPTION) VALUES
                ('POL-1', 'AP_MATCHING', '{"PolicyVersion":"v1","CurrencyCode":"USD","QuantityTolerancePercent":5,"PriceTolerancePercent":5,"UnitRule":"EXACT_RECORDED_UNITS","CurrencyRule":"EXACT_RECORDED_CURRENCY","TaxTreatment":"SEPARATE_REVIEW_REQUIRED","RoundingRule":"EXACT_SOURCE_AMOUNTS","NonPurchaseInvoiceRule":"SEPARATE_REVIEW_REQUIRED"}', 'APPROVED', '2026-01-01', 'Y',
                 'USD 5 percent quantity and price tolerance');

            INSERT INTO AFE (AFE_ID, AFE_NUMBER, AFE_NAME, STATUS, ESTIMATED_COST, APPROVED_AMOUNT,
                CURRENCY_CODE, AUTHOR, ACTIVE_IND) VALUES
                ('AFE-1', 'AFE-2026-001', 'Woodford infill', 'APPROVED', 200000, 200000, 'USD', 'sara', 'Y');
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

    private static OgOrderInputs OrderInputs() =>
        new(
            "VENDOR-1",
            new DateTime(2026, 4, 1),
            new DateTime(2026, 4, 15),
            "USD",
            "AFE-1",
            "Casing and cement for the Woodford pad",
            [
                new OgOrderLineInput("Casing", 100m, "ea", 50m),
                new OgOrderLineInput("Cement", 200m, "sx", 20m),
            ]);

    private static OgSupplierInvoiceInputs InvoiceInputs(
        string number,
        decimal tax,
        decimal price,
        decimal quantity,
        string? orderLineId,
        string? orderId) =>
        new(
            "VENDOR-1",
            number,
            new DateTime(2026, 4, 20),
            new DateTime(2026, 5, 20),
            "USD",
            tax,
            orderId,
            "AFE-1",
            null,
            [new OgInvoiceLineInput("Casing", quantity, "ea", price, orderLineId)]);

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
