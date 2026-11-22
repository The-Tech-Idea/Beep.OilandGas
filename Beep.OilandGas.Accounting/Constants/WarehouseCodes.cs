using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Accounting.Constants;

/// <summary>
/// The warehouse vocabulary and accounts (ACC-06), stated once.
/// </summary>
/// <remarks>
/// <para>
/// <b>The open question, answered.</b> Two inventory services exist, and reading both settles which owns the quantity.
/// The <b>accounting</b> <c>InventoryService</c> says it "manages inventory items, stock transactions, and valuations" over
/// <c>INVENTORY_ITEM</c> and <c>INVENTORY_TRANSACTION</c>, and documents the postings this feature uses. The
/// <b>ProductionAccounting</b> service says it "manages tank and pipeline inventory ... tracks volume levels", which is the
/// crude/tank volume on <c>CRUDE_OIL_INVENTORY</c> and <c>TANK_INVENTORY</c> - a different fact on different tables, not a
/// rival answer. So the materials stock quantity is the accounting project's, and a warehouse movement is an
/// <c>INVENTORY_TRANSACTION</c> row with the owner's own sign convention: a deduction is written with a negative quantity,
/// exactly as <c>InventoryService.UseStockAsync</c> already does.
/// </para>
/// <para>
/// The transaction types the owner already writes are "RECEIPT", "USAGE" and "WORK_ORDER_MATERIAL_USE"; the ones this
/// feature adds are stated here so nothing is spelled twice, and a reservation is its own record rather than a transaction
/// because it moves no quantity.
/// </para>
/// </remarks>
public static class WarehouseCodes
{
    /// <summary>Stock arriving - the owner's own transaction type.</summary>
    public const string KindReceipt = "RECEIPT";

    /// <summary>Stock leaving for a work order or cost centre.</summary>
    public const string KindIssue = "ISSUE";

    /// <summary>Stock coming back from a work order, restocked or not.</summary>
    public const string KindReturn = "RETURN";

    /// <summary>A physical count correction.</summary>
    public const string KindAdjustment = "ADJUSTMENT";

    /// <summary>Every movement kind the warehouse writes.</summary>
    public static readonly string[] Kinds = [KindReceipt, KindIssue, KindReturn, KindAdjustment];

    /// <summary>A reservation holding stock for a document; it moves no quantity.</summary>
    public const string ReservationOpen = "OPEN";

    /// <summary>The reservation was issued: its stock has left and the reservation is spent.</summary>
    public const string ReservationIssued = "ISSUED";

    /// <summary>The reservation was released without issuing: the stock is available again.</summary>
    public const string ReservationReleased = "RELEASED";

    /// <summary>Every reservation state.</summary>
    public static readonly string[] ReservationStates = [ReservationOpen, ReservationIssued, ReservationReleased];

    /// <summary>Arrived in good order and usable.</summary>
    public const string ConditionNew = "NEW";

    /// <summary>Used but fit for service.</summary>
    public const string ConditionServiceable = "SERVICEABLE";

    /// <summary>Damaged: it may or may not be worth restocking.</summary>
    public const string ConditionDamaged = "DAMAGED";

    /// <summary>Beyond use.</summary>
    public const string ConditionScrap = "SCRAP";

    /// <summary>Every condition a receipt or return may state.</summary>
    public static readonly string[] Conditions =
        [ConditionNew, ConditionServiceable, ConditionDamaged, ConditionScrap];

    /// <summary>Put it back into stock and onto the books.</summary>
    public const string RestockRestock = "RESTOCK";

    /// <summary>Do not restock: the quantity never returns to stock and no value moves.</summary>
    public const string RestockScrap = "SCRAP";

    /// <summary>Hold it out of stock for repair; the decision is recorded and nothing moves yet.</summary>
    public const string RestockRepair = "REPAIR";

    /// <summary>Every restock decision.</summary>
    public static readonly string[] RestockDecisions = [RestockRestock, RestockScrap, RestockRepair];

    /// <summary>Whether a condition can be restocked without asking - only stock that arrived usable.</summary>
    public static bool IsUsable(string? condition) =>
        string.Equals(condition?.Trim(), ConditionNew, StringComparison.OrdinalIgnoreCase)
        || string.Equals(condition?.Trim(), ConditionServiceable, StringComparison.OrdinalIgnoreCase);

    /// <summary>The decision that actually puts the quantity back into stock.</summary>
    public static bool IsRestock(string? decision) =>
        string.Equals(decision?.Trim(), RestockRestock, StringComparison.OrdinalIgnoreCase);

    /// <summary>Whether a reservation still holds stock.</summary>
    public static bool HoldsStock(string? state) =>
        string.Equals(state?.Trim(), ReservationOpen, StringComparison.OrdinalIgnoreCase);

    // ---- the accounts a stock movement touches (the owner's documented postings) -----------------------------------

    /// <summary>Inventory - the balance-sheet side of every movement.</summary>
    public const string InventoryAccount = DefaultGlAccounts.Inventory;

    /// <summary>Cost of goods sold - the debit side of an issue.</summary>
    public const string IssueAccount = DefaultGlAccounts.CostOfGoodsSold;

    /// <summary>Accounts payable - the credit side of a receipt bought on account.</summary>
    public const string ReceiptAccount = DefaultGlAccounts.AccountsPayable;

    /// <summary>Impairment loss - the debit side of a lower-of-cost-or-market write-down.</summary>
    public const string WriteDownAccount = DefaultGlAccounts.ImpairmentLoss;
}
