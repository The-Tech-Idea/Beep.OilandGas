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
/// ACC-06 against a real SQLite database: the warehouse journey - receipt, hold, issue, return, count, revaluation and link -
/// and the six rules that keep the stock honest: available is on hand less reserved (AC-1), an issue above availability is
/// refused naming the shortfall (AC-2), a hold names its document (AC-3), a return states its restock decision (AC-4), every
/// value change posts a linked journal (AC-5), and stock never goes below zero (AC-6).
/// Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class WarehouseGatewayTests
{
    private const string ConnectionName = "acc06-warehouse";

    [IntegrationFact]
    public async Task The_warehouse_holds_issues_returns_and_never_goes_negative()
    {        var root = Path.Combine(OgPaths.DataRoot, "acc06-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasAcc06"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "warehouse.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "warehouse.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgWarehouseService(() => source, reporter);
        var context = new OgWarehouseContext("SMOKE", "MAIN");

        // ---- the workspace, and who owns the quantity -------------------------
        var loaded = await service.LoadAsync(context);

        Assert.True(loaded.Success, loaded.Message);
        Assert.False(loaded.Value!.HistoryFailed);
        Assert.Single(loaded.Value.Items);

        var seeded = loaded.Value.Selected!;

        Assert.Equal("CAS-1", seeded.Code);
        Assert.Equal(100m, seeded.OnHand);
        Assert.Equal(0m, seeded.Reserved);
        Assert.Equal(100m, seeded.Available);
        Assert.Equal(1_000m, seeded.Value);
        Assert.Contains("accounting InventoryService", loaded.Value.OwnerText, StringComparison.Ordinal);

        // ---- a receipt books stock in and posts its value (AC-5) --------------
        var received = await service.ReceiveAsync(
            seeded.Id, 50m, 10m, "REC-1", WarehouseCodes.ConditionNew, null, "BIN-B", context, "sara");

        Assert.True(received.Success, received.Message + " :: " + string.Join(" | ", reporter.Failures));

        var afterReceipt = received.Value!.Selected!;

        Assert.Equal(150m, afterReceipt.OnHand);
        Assert.Equal(1_500m, afterReceipt.Value);
        Assert.Equal(150m, afterReceipt.Available);

        var receiptMovement = received.Value.Movements.Single(movement =>
            movement.Kind == WarehouseCodes.KindReceipt);

        Assert.Equal(50m, receiptMovement.Quantity);
        Assert.Equal(500m, receiptMovement.ValueChange);
        Assert.True(receiptMovement.MovedValue);
        Assert.NotNull(receiptMovement.JournalId);
        Assert.Contains("journal", receiptMovement.JournalText, StringComparison.Ordinal);

        Assert.Equal(1, Count(database,
            "JOURNAL_ENTRY WHERE SOURCE_MODULE = 'INVENTORY' AND TOTAL_DEBIT = 500 AND TOTAL_CREDIT = 500"));
        Assert.Equal(1, Count(database,
            "JOURNAL_ENTRY_LINE WHERE DEBIT_AMOUNT = 500 AND GL_ACCOUNT_ID = 'A-1300'"));
        Assert.Equal(1, Count(database,
            "JOURNAL_ENTRY_LINE WHERE CREDIT_AMOUNT = 500 AND GL_ACCOUNT_ID = 'A-2000'"));
        Assert.Equal(1, Count(database,
            "INVENTORY_TRANSACTION WHERE CONDITION_CODE = 'NEW' AND JOURNAL_ENTRY_ID IS NOT NULL"));

        // The bin the goods went into is on the item.
        Assert.Equal(1, Count(database, "INVENTORY_ITEM WHERE LOCATION_ID = 'BIN-B'"));

        // ---- a damaged delivery must state its decision -----------------------
        var damaged = await service.ReceiveAsync(
            seeded.Id, 5m, 10m, "REC-2", WarehouseCodes.ConditionDamaged, null, null, context, "sara");

        Assert.False(damaged.Success);
        Assert.Equal(WarehouseRules.CodeRestockDecisionRequired, damaged.Reference);
        Assert.Equal(1, Count(database, "INVENTORY_TRANSACTION"));

        // ---- AC-3: a reservation names the document holding it ----------------
        var nameless = await service.ReserveAsync(seeded.Id, 40m, "  ", "WORK_ORDER", "sara");

        Assert.False(nameless.Success);
        Assert.Equal(WarehouseRules.CodeHolderRequired, nameless.Reference);
        Assert.Equal(0, Count(database, "INVENTORY_RESERVATION"));

        var reserved = await service.ReserveAsync(seeded.Id, 40m, "WO-2026-014", "WORK_ORDER", "sara");

        Assert.True(reserved.Success, reserved.Message);

        var held = reserved.Value!.Selected!;

        Assert.Equal(40m, held.Reserved);
        Assert.Equal(110m, held.Available);
        Assert.Contains("held by WO-2026-014 (40 ea)", held.HeldText, StringComparison.Ordinal);
        Assert.Equal(1, Count(database,
            "INVENTORY_RESERVATION WHERE HOLDING_DOCUMENT = 'WO-2026-014' AND STATUS = 'OPEN' AND QUANTITY = 40"));

        // ---- AC-2: an issue above availability names the shortfall ------------
        var tooMuch = await service.IssueAsync(seeded.Id, 120m, "WO-2026-014", null, context, "sara");

        Assert.False(tooMuch.Success);
        Assert.Equal(WarehouseRules.CodeInsufficientAvailability, tooMuch.Reference);
        Assert.Contains("a shortfall of 10", tooMuch.Message!, StringComparison.Ordinal);
        Assert.Contains("150 is on hand and 40 is reserved", tooMuch.Message!, StringComparison.Ordinal);
        Assert.Equal(1, Count(database, "INVENTORY_TRANSACTION"));

        // ---- issuing spends the reservation and posts the cost ----------------
        var issued = await service.IssueAsync(seeded.Id, 30m, "WO-2026-014", reserved.Value.Selected.Holds[0].Id,
            context, "sara");

        Assert.True(issued.Success, issued.Message);

        var afterIssue = issued.Value!.Selected!;

        Assert.Equal(120m, afterIssue.OnHand);
        Assert.Equal(0m, afterIssue.Reserved);
        Assert.Equal(120m, afterIssue.Available);
        Assert.Equal(1_200m, afterIssue.Value);

        var issueMovement = issued.Value.Movements.Single(movement => movement.Kind == WarehouseCodes.KindIssue);

        // The owner's sign convention: a deduction carries a negative quantity.
        Assert.Equal(-30m, issueMovement.Quantity);
        Assert.Equal(-300m, issueMovement.ValueChange);
        Assert.NotNull(issueMovement.JournalId);

        Assert.Equal(1, Count(database,
            "JOURNAL_ENTRY_LINE WHERE DEBIT_AMOUNT = 300 AND GL_ACCOUNT_ID = 'A-5000'"));
        Assert.Equal(1, Count(database,
            "JOURNAL_ENTRY_LINE WHERE CREDIT_AMOUNT = 300 AND GL_ACCOUNT_ID = 'A-1300'"));
        Assert.Equal(1, Count(database,
            "INVENTORY_RESERVATION WHERE STATUS = 'ISSUED' AND ISSUED_TRANSACTION_ID IS NOT NULL"));

        // The spent reservation cannot be issued against again.
        Assert.Equal(
            WarehouseRules.CodeReservationAlreadySpent,
            (await service.IssueAsync(seeded.Id, 1m, null, reserved.Value.Selected.Holds[0].Id, context, "sara"))
            .Reference);

        // ---- AC-4: a return in worse condition does not restock --------------
        var scrapped = await service.ReturnAsync(
            seeded.Id, 10m, WarehouseCodes.ConditionDamaged, WarehouseCodes.RestockScrap, "WO-2026-014",
            context, "sara");

        Assert.True(scrapped.Success, scrapped.Message);

        var afterScrap = scrapped.Value!.Selected!;

        Assert.Equal(120m, afterScrap.OnHand);
        Assert.Equal(1_200m, afterScrap.Value);

        Assert.Equal(1, Count(database,
            "INVENTORY_TRANSACTION WHERE TRANSACTION_TYPE = 'RETURN' AND RESTOCK_DECISION = 'SCRAP' " +
            "AND QUANTITY = 0 AND JOURNAL_ENTRY_ID IS NULL"));

        // The same quantity restocked, by decision, does move the stock and the value.
        var restocked = await service.ReturnAsync(
            seeded.Id, 10m, WarehouseCodes.ConditionServiceable, WarehouseCodes.RestockRestock, "WO-2026-014",
            context, "sara");

        Assert.True(restocked.Success, restocked.Message);
        Assert.Equal(130m, restocked.Value!.Selected!.OnHand);
        Assert.Equal(1_300m, restocked.Value.Selected.Value);
        Assert.Equal(1, Count(database,
            "JOURNAL_ENTRY_LINE WHERE DEBIT_AMOUNT = 100 AND GL_ACCOUNT_ID = 'A-1300' AND JOURNAL_ENTRY_ID IN " +
            "(SELECT JOURNAL_ENTRY_ID FROM JOURNAL_ENTRY WHERE SOURCE_MODULE = 'INVENTORY')"));

        // ---- AC-6: a count cannot go below zero, or below what is held -------
        var again = await service.ReserveAsync(seeded.Id, 40m, "WO-2026-015", "WORK_ORDER", "omar");

        Assert.True(again.Success, again.Message);

        var negative = await service.AdjustAsync(seeded.Id, -1m, "bad count", context, "sara");

        Assert.False(negative.Success);
        Assert.Equal(WarehouseRules.CodeNegativeStock, negative.Reference);
        Assert.Contains("never goes below zero", negative.Message!, StringComparison.Ordinal);

        // A count below what a document holds would make availability a lie.
        var belowHeld = await service.AdjustAsync(seeded.Id, 30m, "counted short", context, "sara");

        Assert.False(belowHeld.Success);
        Assert.Equal(WarehouseRules.CodeNegativeStock, belowHeld.Reference);
        Assert.Contains("reserved", belowHeld.Message!, StringComparison.Ordinal);

        var released = await service.ReleaseAsync(
            again.Value!.Selected.Holds.First(hold => hold.HoldsStock).Id, "work cancelled", "omar");

        Assert.True(released.Success, released.Message);
        Assert.Equal(0m, released.Value!.Selected!.Reserved);
        Assert.Equal(1, Count(database,
            "INVENTORY_RESERVATION WHERE STATUS = 'RELEASED' AND RELEASE_REASON = 'work cancelled' " +
            "AND RELEASED_BY = 'omar'"));

        // ---- a count adjustment moves stock and posts the difference ----------
        var adjusted = await service.AdjustAsync(seeded.Id, 125m, "annual count", context, "sara");

        Assert.True(adjusted.Success, adjusted.Message);

        var afterAdjust = adjusted.Value!.Selected!;

        Assert.Equal(125m, afterAdjust.OnHand);
        Assert.Equal(1_250m, afterAdjust.Value);
        Assert.Equal(1, Count(database,
            "INVENTORY_ADJUSTMENT WHERE ADJUSTMENT_TYPE = 'COUNT_LOSS' AND QUANTITY_ADJUSTMENT = -5 " +
            "AND REASON = 'annual count'"));
        Assert.Equal(1, Count(database,
            "JOURNAL_ENTRY_LINE WHERE CREDIT_AMOUNT = 50 AND GL_ACCOUNT_ID = 'A-1300'"));

        // ---- AC-5: a lower-of-cost-or-market write-down posts to the ledger ---
        var revalued = await service.RevalueAsync(seeded.Id, 8m, "market fell to 8", context, "nadia");

        Assert.True(revalued.Success, revalued.Message);

        var afterRevalue = revalued.Value!.Selected!;

        Assert.Equal(1_000m, afterRevalue.Value);
        Assert.Equal(8m, afterRevalue.UnitCost);
        Assert.Equal(1, Count(database,
            "INVENTORY_VALUATION WHERE VALUATION_METHOD = 'LOWER_OF_COST_OR_MARKET' AND TOTAL_VALUE = 1000"));
        Assert.Equal(1, Count(database,
            "JOURNAL_ENTRY_LINE WHERE DEBIT_AMOUNT = 250 AND GL_ACCOUNT_ID = 'A-6300'"));

        // ---- the link panel ---------------------------------------------------
        var linked = await service.LinkAsync(seeded.Id, "EQUIPMENT", "PUMP-7", "sara");

        Assert.True(linked.Success, linked.Message);
        Assert.Equal(1, Count(database,
            "INVENTORY_ITEM WHERE LINKED_ASSET_TYPE = 'EQUIPMENT' AND LINKED_ASSET_ID = 'PUMP-7'"));
        Assert.Contains("linked to EQUIPMENT PUMP-7", linked.Value!.Selected!.LinkText, StringComparison.Ordinal);

        // ---- every value change carries a journal, and only those -------------
        Assert.Equal(5, Count(database, "JOURNAL_ENTRY WHERE SOURCE_MODULE = 'INVENTORY'"));
        Assert.Equal(0, Count(database,
            "INVENTORY_TRANSACTION WHERE TOTAL_COST <> 0 AND JOURNAL_ENTRY_ID IS NULL"));
        Assert.Equal(1, Count(database,
            "INVENTORY_TRANSACTION WHERE TOTAL_COST = 0 AND JOURNAL_ENTRY_ID IS NULL"));

        // ---- a closed period refuses a movement ------------------------------
        var period = DateTime.UtcNow.ToString("yyyy-MM", System.Globalization.CultureInfo.InvariantCulture);
        Execute(database,
            $"INSERT INTO COMPANY_LEDGER_PERIOD (COMPANY_LEDGER_PERIOD_ID, OWNER_TENANT_ID, OWNER_COMPANY_BA_ID, " +
            $"BOOK_ID, CURRENCY_CODE, CALENDAR_ID, PERIOD_CODE, START_DATE, END_DATE, STATUS, AUTHORITY_JSON) VALUES " +
            $"('P-1', 'SMOKE', 'SMOKE', 'MAIN', 'USD', 'MONTHLY', '{period}', '2026-11-01', '2026-11-30', " +
            "'CLOSED', '{}');");

        var closedPeriod = await service.ReceiveAsync(
            seeded.Id, 1m, 8m, "REC-3", WarehouseCodes.ConditionNew, null, null, context, "sara");

        Assert.False(closedPeriod.Success);
        Assert.Equal(WarehouseRules.CodePeriodClosed, closedPeriod.Reference);

        // ---- an unavailable warehouse is a state -----------------------------
        Execute(database, "DROP TABLE INVENTORY_TRANSACTION;");
        Execute(database, "DROP TABLE INVENTORY_ITEM;");

        var unavailable = await service.LoadAsync(context);

        Assert.True(unavailable.Success, unavailable.Message);
        Assert.True(unavailable.Value!.HistoryFailed);
        Assert.Empty(unavailable.Value.Items);
        Assert.Contains("could not be read", unavailable.Value.HistoryStateText, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ the schema

    private static void Seed(string database) =>
        Execute(database,
            """
            CREATE TABLE INVENTORY_ITEM (
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
                LINKED_ASSET_TYPE TEXT,
                LINKED_ASSET_ID TEXT,
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
                CONDITION_CODE TEXT,
                RESTOCK_DECISION TEXT,
                RESERVATION_ID TEXT,
                JOURNAL_ENTRY_ID TEXT,
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

            CREATE TABLE INVENTORY_ADJUSTMENT (
                INVENTORY_ADJUSTMENT_ID TEXT PRIMARY KEY,
                INVENTORY_ITEM_ID TEXT,
                ADJUSTMENT_DATE TEXT,
                ADJUSTMENT_TYPE TEXT,
                QUANTITY_ADJUSTMENT REAL,
                UNIT_COST_ADJUSTMENT REAL,
                REASON TEXT,
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

            CREATE TABLE INVENTORY_VALUATION (
                INVENTORY_VALUATION_ID TEXT PRIMARY KEY,
                INVENTORY_ITEM_ID TEXT,
                VALUATION_DATE TEXT,
                VALUATION_METHOD TEXT,
                QUANTITY REAL,
                UNIT_COST REAL,
                TOTAL_VALUE REAL,
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

            INSERT INTO GL_ACCOUNT (GL_ACCOUNT_ID, ACCOUNT_NUMBER, ACCOUNT_NAME, ACCOUNT_TYPE, NORMAL_BALANCE,
                CURRENCY_CODE, ACTIVE_IND) VALUES
                ('A-1300', '1300', 'Inventory', 'ASSET', 'DEBIT', 'USD', 'Y'),
                ('A-2000', '2000', 'Accounts payable', 'LIABILITY', 'CREDIT', 'USD', 'Y'),
                ('A-5000', '5000', 'Cost of goods sold', 'EXPENSE', 'DEBIT', 'USD', 'Y'),
                ('A-6300', '6300', 'Impairment loss', 'EXPENSE', 'DEBIT', 'USD', 'Y');

            INSERT INTO INVENTORY_ITEM (INVENTORY_ITEM_ID, ITEM_NUMBER, ITEM_NAME, ITEM_TYPE, UNIT_OF_MEASURE,
                QUANTITY_ON_HAND, UNIT_COST, TOTAL_VALUE, VALUATION_METHOD, CURRENCY_CODE, WAREHOUSE_ID,
                LOCATION_ID, ACTIVE_IND) VALUES
                ('I-1', 'CAS-1', 'Casing 9-5/8', 'MATERIAL', 'ea', 100, 10, 1000, 'FIFO', 'USD', 'WH-1',
                 'BIN-A', 'Y');
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
