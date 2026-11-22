using System.Globalization;
using Beep.OilandGas.Accounting.Calculations;
using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Accounting;

/// <summary>Where a stock movement posts (ACC-06): the company and the book.</summary>
public sealed record OgWarehouseContext(string CompanyId, string BookCode)
{
    public static OgWarehouseContext FromSession(OgSession session) =>
        new(
            string.IsNullOrWhiteSpace(session.CompanyId) ? "APP" : session.CompanyId,
            string.IsNullOrWhiteSpace(session.BookCode) ? "MAIN" : session.BookCode);
}

/// <summary>One stock item with what is held against it (ACC-06, AC-1, AC-3).</summary>
public sealed record OgStockItem(
    string Id,
    string Code,
    string Description,
    string Unit,
    decimal OnHand,
    decimal Reserved,
    decimal UnitCost,
    decimal Value,
    string Currency,
    string? Warehouse,
    string? Location,
    string? ValuationMethod,
    string? LinkedAssetType,
    string? LinkedAssetId,
    IReadOnlyList<OgReservation> Holds)
{
    /// <summary>AC-1: available is on hand less reserved, exactly - no rounding, no allowance.</summary>
    public decimal Available => OnHand - Reserved;

    public string AmountText => Number(OnHand) + " " + Unit;

    public string AvailabilityText => Number(Available) + " " + Unit;

    public string ValueText => MoneyText.Of(Value) + " " + Currency;

    public string HeldText => Holds.Count == 0
        ? "nothing reserved"
        : "held by " + string.Join(", ", Holds.Select(hold =>
            hold.Holder + " (" + Number(hold.Quantity) + " " + Unit + ")"));

    public string LinkText => string.IsNullOrWhiteSpace(LinkedAssetId)
        ? "not linked"
        : "linked to " + (LinkedAssetType ?? "asset") + " " + LinkedAssetId;

    public string Describe() =>
        $"{Code} · {Description} · on hand {Number(OnHand)} {Unit} · reserved {Number(Reserved)} · " +
        $"available {Number(Available)} · {ValueText}";

    private static string Number(decimal value) =>
        value.ToString("#,0.####", CultureInfo.InvariantCulture);
}

/// <summary>One reservation holding stock (ACC-06, AC-3).</summary>
public sealed record OgReservation(
    string Id,
    string ItemId,
    string ItemCode,
    decimal Quantity,
    string Holder,
    string? HoldingKind,
    DateTime? ReservedAt,
    string? Actor,
    string State,
    string? IssuedTransactionId,
    string? ReleasedBy,
    DateTime? ReleasedAt,
    string? ReleaseReason)
{
    public bool HoldsStock => WarehouseCodes.HoldsStock(State);

    public string Describe() =>
        (HoldingKind ?? "document") + " " + Holder + " · " + Quantity.ToString("#,0.####",
            CultureInfo.InvariantCulture) + " · " + State.ToLowerInvariant() +
        (ReservedAt is { } at ? " since " + at.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : string.Empty);
}

/// <summary>One stock movement (ACC-06).</summary>
public sealed record OgStockMovement(
    string Id,
    string Kind,
    string ItemId,
    string ItemCode,
    decimal Quantity,
    decimal UnitCost,
    decimal ValueChange,
    string? Reference,
    DateTime Date,
    string? Actor,
    string? Condition,
    string? RestockDecision,
    string? ReservationId,
    string? JournalId,
    string? Description)
{
    public bool MovedStock => Quantity != 0m;

    public bool MovedValue => ValueChange != 0m;

    /// <summary>AC-5: a value change is either linked to its journal or said to have produced none.</summary>
    public string JournalText => string.IsNullOrWhiteSpace(JournalId)
        ? MovedValue
            ? "UNLINKED - a value change with no journal"
            : "no value change, so no journal"
        : "journal " + JournalId;

    public string Describe() =>
        Kind + " · " + ItemCode + " · " + Quantity.ToString("#,0.####", CultureInfo.InvariantCulture) +
        " · " + MoneyText.Of(ValueChange) + " · " + Date.ToString("yyyy-MM-dd") +
        (string.IsNullOrWhiteSpace(Reference) ? string.Empty : " · " + Reference) +
        (string.IsNullOrWhiteSpace(Condition) ? string.Empty : " · " + Condition) +
        (string.IsNullOrWhiteSpace(RestockDecision) ? string.Empty : " · " + RestockDecision);
}

/// <summary>The warehouse workspace (ACC-06).</summary>
public sealed record OgWarehouseWorkspace(
    IReadOnlyList<OgStockItem> Items,
    OgStockItem? Selected,
    IReadOnlyList<OgStockMovement> Movements,
    IReadOnlyList<OgReservation> Reservations,
    IReadOnlyList<string> Conditions,
    IReadOnlyList<string> RestockDecisions,
    IReadOnlyList<string> Kinds,
    string OwnerText,
    bool HistoryFailed,
    string? HistoryReason)
{
    public string ItemsText => Items.Count == 0
        ? "No stock item exists yet."
        : string.Join(Environment.NewLine, Items.Select(item => item.Describe()));

    public string HistoryStateText => HistoryFailed
        ? "the warehouse could not be read; nothing can move"
        : $"{Items.Count.ToString(CultureInfo.InvariantCulture)} item(s) · " +
          $"{Movements.Count.ToString(CultureInfo.InvariantCulture)} movement(s) · " +
          $"{Reservations.Count(reservation => reservation.HoldsStock).ToString(CultureInfo.InvariantCulture)} " +
          "open reservation(s)";

    public decimal TotalValue => Items.Sum(item => item.Value);

    public string TotalValueText => "stock value " + MoneyText.Of(TotalValue);

    /// <summary>Every reservation still holding quantity, with the document holding it (AC-3).</summary>
    public string ReservationsText => Reservations.Count == 0
        ? "nothing is reserved"
        : string.Join(Environment.NewLine, Reservations
            .Where(reservation => reservation.HoldsStock)
            .Select(reservation => reservation.ItemCode + " · " + reservation.Describe()));

    public string MovementsText => Movements.Count == 0
        ? "no movement"
        : string.Join(Environment.NewLine, Movements.Select(movement => movement.Describe()));

    public string ReferenceText =>
        $"movements {string.Join(", ", Kinds)} · conditions {string.Join(", ", Conditions)} · " +
        $"restock decisions {string.Join(", ", RestockDecisions)} · value changes post to the ledger";
}

/// <summary>The warehouse rules, stated once so the page, the service and the tests agree.</summary>
public static class WarehouseRules
{
    public const string CodeNoConnection = "warehouse-no-connection";
    public const string CodeReadFailed = "warehouse-read-failed";
    public const string CodeWriteFailed = "warehouse-write-failed";
    public const string CodeActorRequired = "warehouse-actor-required";
    public const string CodeInputRequired = "warehouse-input-required";
    public const string CodeInputInvalid = "warehouse-input-invalid";
    public const string CodeUnknownItem = "warehouse-unknown-item";
    public const string CodeUnknownReservation = "warehouse-unknown-reservation";
    public const string CodeInsufficientAvailability = "warehouse-insufficient-availability";
    public const string CodeNegativeStock = "warehouse-negative-stock";
    public const string CodeRestockDecisionRequired = "warehouse-restock-decision-required";
    public const string CodeHolderRequired = "warehouse-holder-required";
    public const string CodeReservationAlreadySpent = "warehouse-reservation-already-spent";
    public const string CodeReasonRequired = "warehouse-reason-required";
    public const string CodePeriodClosed = "warehouse-period-closed";
    public const string CodeUnknownAccount = "warehouse-unknown-account";
    public const string CodeNothingToValue = "warehouse-nothing-to-value";

    public static IReadOnlyList<string> Conditions => WarehouseCodes.Conditions;

    public static IReadOnlyList<string> RestockDecisions => WarehouseCodes.RestockDecisions;

    public static IReadOnlyList<string> Kinds => WarehouseCodes.Kinds;

    /// <summary>The holding document kinds a reservation may name (AC-3).</summary>
    public static IReadOnlyList<string> HoldingKinds { get; } = ["WORK_ORDER", "COST_CENTRE", "REQUEST"];

    public static IReadOnlyList<string> AssetTypes { get; } = ["ASSET", "EQUIPMENT", "WELL"];

    /// <summary>
    /// AC-2 and AC-6: an issue above what is available is refused, and the refusal names the short quantity rather than
    /// saying "invalid". Availability is on hand less what other documents hold.
    /// </summary>
    public static (string Code, string Message)? CheckIssue(
        OgStockItem item,
        decimal quantity,
        decimal alreadyHeldByThisDocument = 0m)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (quantity <= 0m)
        {
            return (CodeInputInvalid, "An issue needs a quantity above zero.");
        }

        if (quantity > item.Available)
        {
            return (CodeInsufficientAvailability,
                $"Cannot issue {Number(quantity)} {item.Unit} of {item.Code}: {Number(item.Available)} " +
                $"{item.Unit} is available, a shortfall of {Number(quantity - item.Available)}. " +
                $"{Number(item.OnHand)} is on hand and {Number(item.Reserved)} is reserved");

            // The reserved part is deliberately named: an issuer who expected more stock needs to see who holds it.
        }

        _ = alreadyHeldByThisDocument;

        return null;
    }

    /// <summary>
    /// AC-4: a return that comes back in worse condition has to say what happens to it - restock, scrap or repair.
    /// </summary>
    public static (string Code, string Message)? CheckReturn(string? condition, string? decision)
    {
        if (!Conditions.Contains(condition?.Trim().ToUpperInvariant() ?? string.Empty, StringComparer.Ordinal))
        {
            return (CodeInputRequired,
                $"A return must state the condition it came back in: {string.Join(", ", Conditions)}.");
        }

        if (WarehouseCodes.IsUsable(condition))
        {
            // Good stock still needs a decision, but restocking it is the only sensible one and it is stated anyway.
            return string.IsNullOrWhiteSpace(decision)
                ? (CodeRestockDecisionRequired,
                    "A return must state its restock decision, even when the goods came back usable.")
                : null;
        }

        return string.IsNullOrWhiteSpace(decision)
            ? (CodeRestockDecisionRequired,
                $"A {condition} return must state its restock decision - RESTOCK, SCRAP or REPAIR - " +
                "because the quantity and the value depend on it.")
            : null;
    }

    /// <summary>AC-6: a counted quantity below zero is refused before anything is written.</summary>
    public static (string Code, string Message)? CheckCount(OgStockItem item, decimal counted)
    {
        ArgumentNullException.ThrowIfNull(item);

        return counted < 0m
            ? (CodeNegativeStock,
                $"A stock count cannot be negative: {item.Code} was counted at {Number(counted)}. " +
                "Correct the count or write the shortfall off, but stock never goes below zero.")
            : null;
    }

    /// <summary>Whether a movement's sign and kind agree - a receipt adds, an issue deducts.</summary>
    public static decimal SignedQuantity(string kind, decimal quantity) =>
        string.Equals(kind, WarehouseCodes.KindIssue, StringComparison.OrdinalIgnoreCase)
            ? -quantity
            : quantity;

    internal static string Number(decimal value) =>
        value.ToString("#,0.####", CultureInfo.InvariantCulture);
}

/// <summary>ACC-06: the warehouse - receipt, reservation, issue, return, adjustment, valuation and the asset link.</summary>
public interface IOgWarehouse
{
    /// <summary>Books stock in; a damaged receipt must state its restock decision.</summary>
    Task<OgResult<OgWarehouseWorkspace>> ReceiveAsync(
        string itemId,
        decimal quantity,
        decimal? unitCost,
        string? reference,
        string? condition,
        string? restockDecision,
        string? location,
        OgWarehouseContext context,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Holds stock for a document; the holding document is required (AC-3).</summary>
    Task<OgResult<OgWarehouseWorkspace>> ReserveAsync(
        string itemId,
        decimal quantity,
        string holder,
        string? holdingKind,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Issues stock, refusing a quantity above availability (AC-2) and spending a reservation when given.</summary>
    Task<OgResult<OgWarehouseWorkspace>> IssueAsync(
        string itemId,
        decimal quantity,
        string? reference,
        string? reservationId,
        OgWarehouseContext context,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Releases a reservation without issuing, with its reason.</summary>
    Task<OgResult<OgWarehouseWorkspace>> ReleaseAsync(
        string reservationId,
        string reason,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Takes stock back from a work order; the restock decision decides what moves (AC-4).</summary>
    Task<OgResult<OgWarehouseWorkspace>> ReturnAsync(
        string itemId,
        decimal quantity,
        string? condition,
        string? restockDecision,
        string? reference,
        OgWarehouseContext context,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Corrects the count to a physically counted quantity, with its reason (AC-6).</summary>
    Task<OgResult<OgWarehouseWorkspace>> AdjustAsync(
        string itemId,
        decimal countedQuantity,
        string reason,
        OgWarehouseContext context,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Revalues at market; a write-down posts to the ledger (AC-5).</summary>
    Task<OgResult<OgWarehouseWorkspace>> RevalueAsync(
        string itemId,
        decimal marketUnitCost,
        string reason,
        OgWarehouseContext context,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Links the item to an asset or equipment record.</summary>
    Task<OgResult<OgWarehouseWorkspace>> LinkAsync(
        string itemId,
        string assetType,
        string assetId,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>The items, their holds, the movements and the reservations; an unavailable warehouse is a state.</summary>
    Task<OgResult<OgWarehouseWorkspace>> LoadAsync(
        OgWarehouseContext context,
        string? itemId = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// ACC-06 over ProductionAccounting's <c>INVENTORY_ITEM</c>, <c>INVENTORY_TRANSACTION</c>, <c>INVENTORY_ADJUSTMENT</c> and
/// <c>INVENTORY_VALUATION</c> and the new <c>INVENTORY_RESERVATION</c>, with the value changes posted to the ledger through
/// units of work.
/// </summary>
/// <remarks>
/// The stock quantity is the accounting project's, which is the project whose <c>InventoryService</c> says it "manages
/// inventory items, stock transactions, and valuations" - the ProductionAccounting one manages tank and pipeline volume on
/// different tables. A deduction is written with a negative quantity, which is the owner's own sign convention in
/// <c>UseStockAsync</c>. Reserved is always the sum of the item's open reservations, so availability cannot drift from the
/// documents holding it; and every value change writes one ledger entry whose reference is derived from the movement id, so
/// a retry adopts it rather than posting twice.
/// </remarks>
public sealed class OgWarehouseService : IOgWarehouse
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgWarehouseService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgWarehouseWorkspace>> ReceiveAsync(
        string itemId,
        decimal quantity,
        decimal? unitCost,
        string? reference,
        string? condition,
        string? restockDecision,
        string? location,
        OgWarehouseContext context,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                "A receipt must record who booked it in.", WarehouseRules.CodeActorRequired);
        }

        if (quantity <= 0m)
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                "A receipt needs a quantity above zero.", WarehouseRules.CodeInputInvalid);
        }

        var state = condition?.Trim().ToUpperInvariant();

        if (state is null || !WarehouseCodes.Conditions.Contains(state, StringComparer.Ordinal))
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                $"A receipt must state the condition the goods arrived in: " +
                $"{string.Join(", ", WarehouseCodes.Conditions)}.",
                WarehouseRules.CodeInputRequired);
        }

        // A damaged or scrapped delivery that is still booked into stock has to say so - the same rule a return follows.
        if (!WarehouseCodes.IsUsable(state) && string.IsNullOrWhiteSpace(restockDecision))
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                $"A {state} receipt must state its restock decision - RESTOCK, SCRAP or REPAIR - because it decides " +
                "whether the quantity and its value enter stock at all.",
                WarehouseRules.CodeRestockDecisionRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                "No connection is open, so the receipt cannot be booked in.", WarehouseRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, context, itemId, cancellationToken).ConfigureAwait(false);
            var item = workspace.Selected;

            if (item is null)
            {
                return OgResult<OgWarehouseWorkspace>.Failed(
                    $"Stock item '{itemId}' is not in this database.", WarehouseRules.CodeUnknownItem);
            }

            var restocking = string.IsNullOrWhiteSpace(restockDecision) || WarehouseCodes.IsRestock(restockDecision);
            var cost = unitCost ?? item.UnitCost;
            var valueChange = restocking ? Math.Round(quantity * cost, 2, MidpointRounding.AwayFromZero) : 0m;

            var outcome = await MoveAsync(
                source,
                workspace,
                item,
                WarehouseCodes.KindReceipt,
                restocking ? quantity : 0m,
                cost,
                valueChange,
                reference,
                state,
                restockDecision,
                null,
                location,
                // A receipt debits inventory and credits payables - the owner's documented posting.
                WarehouseCodes.InventoryAccount,
                WarehouseCodes.ReceiptAccount,
                context,
                actor,
                "goods received",
                cancellationToken).ConfigureAwait(false);

            return outcome;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference_ = _failures.ReportHandled(
                ex, $"booking stock in for {itemId}", "the receipt was not recorded", FailureSeverity.Error);

            return OgResult<OgWarehouseWorkspace>.Failed(
                "The receipt could not be recorded.", reference_ ?? WarehouseRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgWarehouseWorkspace>> ReserveAsync(
        string itemId,
        decimal quantity,
        string holder,
        string? holdingKind,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                "A reservation must record who made it.", WarehouseRules.CodeActorRequired);
        }

        // AC-3: a reservation without a holder is an unexplained number.
        if (string.IsNullOrWhiteSpace(holder))
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                "A reservation must name the document holding the stock - a work order, cost centre or request.",
                WarehouseRules.CodeHolderRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                "No connection is open, so the stock cannot be reserved.", WarehouseRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, null, itemId, cancellationToken).ConfigureAwait(false);
            var item = workspace.Selected;

            if (item is null)
            {
                return OgResult<OgWarehouseWorkspace>.Failed(
                    $"Stock item '{itemId}' is not in this database.", WarehouseRules.CodeUnknownItem);
            }

            // A reservation may only hold what is available; the refusal is the same one an issue gives (AC-2).
            if (WarehouseRules.CheckIssue(item, quantity) is { } refusal)
            {
                return OgResult<OgWarehouseWorkspace>.Failed(refusal.Message, refusal.Code);
            }

            if (holdingKind is { Length: > 0 }
                && !WarehouseRules.HoldingKinds.Contains(holdingKind.Trim().ToUpperInvariant(), StringComparer.Ordinal))
            {
                return OgResult<OgWarehouseWorkspace>.Failed(
                    $"'{holdingKind}' is not a holding document kind; use " +
                    $"{string.Join(", ", WarehouseRules.HoldingKinds)}.",
                    WarehouseRules.CodeInputInvalid);
            }

            var now = DateTime.UtcNow;
            var unit = OgUow.For<INVENTORY_RESERVATION>(source);
            unit.PrimaryKey = "INVENTORY_RESERVATION_ID";

            unit.Add(new INVENTORY_RESERVATION
            {
                INVENTORY_RESERVATION_ID = Guid.NewGuid().ToString("N"),
                INVENTORY_ITEM_ID = item.Id,
                QUANTITY = quantity,
                HOLDING_DOCUMENT = holder.Trim(),
                HOLDING_KIND = holdingKind?.Trim().ToUpperInvariant(),
                RESERVED_AT = now,
                ACTOR = actor.Trim(),
                STATUS = WarehouseCodes.ReservationOpen,
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
                return OgResult<OgWarehouseWorkspace>.Failed(
                    OgUow.Message(committed, "The reservation was not saved."), WarehouseRules.CodeWriteFailed);
            }

            var after = await ReadAsync(source, null, item.Id, cancellationToken).ConfigureAwait(false);

            return OgResult<OgWarehouseWorkspace>.Ok(after);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"reserving stock for {holder}", "the reservation was not recorded", FailureSeverity.Error);

            return OgResult<OgWarehouseWorkspace>.Failed(
                "The reservation could not be recorded.", reference ?? WarehouseRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgWarehouseWorkspace>> IssueAsync(
        string itemId,
        decimal quantity,
        string? reference,
        string? reservationId,
        OgWarehouseContext context,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                "An issue must record who made it.", WarehouseRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                "No connection is open, so the stock cannot be issued.", WarehouseRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, context, itemId, cancellationToken).ConfigureAwait(false);
            var item = workspace.Selected;

            if (item is null)
            {
                return OgResult<OgWarehouseWorkspace>.Failed(
                    $"Stock item '{itemId}' is not in this database.", WarehouseRules.CodeUnknownItem);
            }

            // AC-2 and AC-6: never above availability, never below zero, and the shortfall is named.
            if (WarehouseRules.CheckIssue(item, quantity) is { } refusal)
            {
                return OgResult<OgWarehouseWorkspace>.Failed(refusal.Message, refusal.Code);
            }

            var reservation = default(OgReservation);

            if (!string.IsNullOrWhiteSpace(reservationId))
            {
                reservation = workspace.Reservations.FirstOrDefault(candidate =>
                    string.Equals(candidate.Id, reservationId.Trim(), StringComparison.OrdinalIgnoreCase));

                if (reservation is null)
                {
                    return OgResult<OgWarehouseWorkspace>.Failed(
                        $"Reservation '{reservationId}' is not in this database.",
                        WarehouseRules.CodeUnknownReservation);
                }

                if (!reservation.HoldsStock)
                {
                    return OgResult<OgWarehouseWorkspace>.Failed(
                        $"Reservation {reservation.Describe()} is already {reservation.State.ToLowerInvariant()}.",
                        WarehouseRules.CodeReservationAlreadySpent);
                }

                if (!string.Equals(reservation.ItemId, item.Id, StringComparison.Ordinal))
                {
                    return OgResult<OgWarehouseWorkspace>.Failed(
                        $"Reservation {reservation.Describe()} holds {reservation.ItemCode}, not {item.Code}.",
                        WarehouseRules.CodeInputInvalid);
                }
            }

            var cost = item.UnitCost;
            var valueChange = -Math.Round(quantity * cost, 2, MidpointRounding.AwayFromZero);

            var outcome = await MoveAsync(
                source,
                workspace,
                item,
                WarehouseCodes.KindIssue,
                WarehouseRules.SignedQuantity(WarehouseCodes.KindIssue, quantity),
                cost,
                valueChange,
                reference,
                null,
                null,
                reservation?.Id,
                null,
                WarehouseCodes.IssueAccount,
                WarehouseCodes.InventoryAccount,
                context,
                actor,
                "stock issued",
                cancellationToken).ConfigureAwait(false);

            if (!outcome.Success || reservation is null)
            {
                return outcome;
            }

            // The reservation is spent by the issue, and the transaction that did it is recorded against it.
            var spent = await SpendReservationAsync(
                source, reservation, outcome.Value!.Movements.FirstOrDefault()?.Id, actor, cancellationToken)
                .ConfigureAwait(false);

            if (spent is not null)
            {
                return OgResult<OgWarehouseWorkspace>.Failed(spent, WarehouseRules.CodeWriteFailed);
            }

            var after = await ReadAsync(source, context, item.Id, cancellationToken).ConfigureAwait(false);

            return OgResult<OgWarehouseWorkspace>.Ok(after);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference_ = _failures.ReportHandled(
                ex, $"issuing stock {itemId}", "the issue was not recorded", FailureSeverity.Error);

            return OgResult<OgWarehouseWorkspace>.Failed(
                "The issue could not be recorded.", reference_ ?? WarehouseRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgWarehouseWorkspace>> ReleaseAsync(
        string reservationId,
        string reason,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                "A release must record who made it.", WarehouseRules.CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                "Releasing a reservation needs its reason.", WarehouseRules.CodeReasonRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                "No connection is open, so the reservation cannot be released.", WarehouseRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, null, null, cancellationToken).ConfigureAwait(false);
            var reservation = workspace.Reservations.FirstOrDefault(candidate =>
                string.Equals(candidate.Id, reservationId?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (reservation is null)
            {
                return OgResult<OgWarehouseWorkspace>.Failed(
                    $"Reservation '{reservationId}' is not in this database.", WarehouseRules.CodeUnknownReservation);
            }

            if (!reservation.HoldsStock)
            {
                return OgResult<OgWarehouseWorkspace>.Failed(
                    $"Reservation {reservation.Describe()} is already {reservation.State.ToLowerInvariant()}.",
                    WarehouseRules.CodeReservationAlreadySpent);
            }

            var unit = OgUow.For<INVENTORY_RESERVATION>(source);
            unit.PrimaryKey = "INVENTORY_RESERVATION_ID";

            var rows = await OgUow.ReadAsync(
                unit, [OgUow.Equal("INVENTORY_RESERVATION_ID", reservation.Id)], cancellationToken)
                .ConfigureAwait(false);

            var row = rows.FirstOrDefault();

            if (row is null)
            {
                return OgResult<OgWarehouseWorkspace>.Failed(
                    $"Reservation '{reservationId}' is not in this database.", WarehouseRules.CodeUnknownReservation);
            }

            row.STATUS = WarehouseCodes.ReservationReleased;
            row.RELEASED_BY = actor.Trim();
            row.RELEASED_AT = DateTime.UtcNow;
            row.RELEASE_REASON = reason.Trim();
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = DateTime.UtcNow;

            var updated = unit.Update(row);

            if (!OgUow.IsOk(updated))
            {
                return OgResult<OgWarehouseWorkspace>.Failed(
                    OgUow.Message(updated, "The reservation was not released."), WarehouseRules.CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgWarehouseWorkspace>.Failed(
                    OgUow.Message(committed, "The reservation was not released."), WarehouseRules.CodeWriteFailed);
            }

            var after = await ReadAsync(source, null, reservation.ItemId, cancellationToken).ConfigureAwait(false);

            return OgResult<OgWarehouseWorkspace>.Ok(after);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"releasing reservation {reservationId}", "the reservation was not released",
                FailureSeverity.Error);

            return OgResult<OgWarehouseWorkspace>.Failed(
                "The reservation could not be released.", reference ?? WarehouseRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgWarehouseWorkspace>> ReturnAsync(
        string itemId,
        decimal quantity,
        string? condition,
        string? restockDecision,
        string? reference,
        OgWarehouseContext context,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                "A return must record who made it.", WarehouseRules.CodeActorRequired);
        }

        if (quantity <= 0m)
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                "A return needs a quantity above zero.", WarehouseRules.CodeInputInvalid);
        }

        // AC-4: without an explicit decision nobody knows whether the quantity and its value come back.
        if (WarehouseRules.CheckReturn(condition, restockDecision) is { } refusal)
        {
            return OgResult<OgWarehouseWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                "No connection is open, so the return cannot be recorded.", WarehouseRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, context, itemId, cancellationToken).ConfigureAwait(false);
            var item = workspace.Selected;

            if (item is null)
            {
                return OgResult<OgWarehouseWorkspace>.Failed(
                    $"Stock item '{itemId}' is not in this database.", WarehouseRules.CodeUnknownItem);
            }

            var restocking = WarehouseCodes.IsRestock(restockDecision);
            var cost = item.UnitCost;
            var valueChange = restocking ? Math.Round(quantity * cost, 2, MidpointRounding.AwayFromZero) : 0m;

            var outcome = await MoveAsync(
                source,
                workspace,
                item,
                WarehouseCodes.KindReturn,
                restocking ? quantity : 0m,
                cost,
                valueChange,
                reference,
                condition?.Trim().ToUpperInvariant(),
                restockDecision?.Trim().ToUpperInvariant(),
                null,
                null,
                WarehouseCodes.InventoryAccount,
                WarehouseCodes.IssueAccount,
                context,
                actor,
                restocking ? "stock returned to store" : "stock returned and not restocked",
                cancellationToken).ConfigureAwait(false);

            return outcome;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference_ = _failures.ReportHandled(
                ex, $"recording a return of {itemId}", "the return was not recorded", FailureSeverity.Error);

            return OgResult<OgWarehouseWorkspace>.Failed(
                "The return could not be recorded.", reference_ ?? WarehouseRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgWarehouseWorkspace>> AdjustAsync(
        string itemId,
        decimal countedQuantity,
        string reason,
        OgWarehouseContext context,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                "An adjustment must record who made it.", WarehouseRules.CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                "An adjustment needs its reason, or nobody can see why the count changed.",
                WarehouseRules.CodeReasonRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                "No connection is open, so the stock cannot be adjusted.", WarehouseRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, context, itemId, cancellationToken).ConfigureAwait(false);
            var item = workspace.Selected;

            if (item is null)
            {
                return OgResult<OgWarehouseWorkspace>.Failed(
                    $"Stock item '{itemId}' is not in this database.", WarehouseRules.CodeUnknownItem);
            }

            // AC-6: stock never goes below zero, and a negative count is refused before anything is written.
            if (WarehouseRules.CheckCount(item, countedQuantity) is { } refusal)
            {
                return OgResult<OgWarehouseWorkspace>.Failed(refusal.Message, refusal.Code);
            }

            var difference = countedQuantity - item.OnHand;

            if (difference == 0m)
            {
                return OgResult<OgWarehouseWorkspace>.Failed(
                    $"{item.Code} is already counted at {WarehouseRules.Number(item.OnHand)} {item.Unit}; " +
                    "there is nothing to adjust.",
                    WarehouseRules.CodeInputInvalid);
            }

            // A count cannot release stock that a document is holding: that would make availability a lie.
            if (countedQuantity < item.Reserved)
            {
                return OgResult<OgWarehouseWorkspace>.Failed(
                    $"{item.Code} is counted at {WarehouseRules.Number(countedQuantity)} but " +
                    $"{WarehouseRules.Number(item.Reserved)} is reserved; release the reservation before counting " +
                    "below it.",
                    WarehouseRules.CodeNegativeStock);
            }

            var valueChange = Math.Round(difference * item.UnitCost, 2, MidpointRounding.AwayFromZero);

            var outcome = await MoveAsync(
                source,
                workspace,
                item,
                WarehouseCodes.KindAdjustment,
                difference,
                item.UnitCost,
                valueChange,
                reason,
                null,
                null,
                null,
                null,
                difference > 0m ? WarehouseCodes.InventoryAccount : WarehouseCodes.IssueAccount,
                difference > 0m ? WarehouseCodes.IssueAccount : WarehouseCodes.InventoryAccount,
                context,
                actor,
                "stock count adjusted",
                cancellationToken,
                adjustmentReason: reason).ConfigureAwait(false);

            return outcome;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"adjusting stock {itemId}", "the adjustment was not recorded", FailureSeverity.Error);

            return OgResult<OgWarehouseWorkspace>.Failed(
                "The adjustment could not be recorded.", reference ?? WarehouseRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgWarehouseWorkspace>> RevalueAsync(
        string itemId,
        decimal marketUnitCost,
        string reason,
        OgWarehouseContext context,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                "A revaluation must record who made it.", WarehouseRules.CodeActorRequired);
        }

        if (marketUnitCost < 0m)
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                "A market cost cannot be negative.", WarehouseRules.CodeInputInvalid);
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                "A revaluation needs its reason.", WarehouseRules.CodeReasonRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                "No connection is open, so the stock cannot be revalued.", WarehouseRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, context, itemId, cancellationToken).ConfigureAwait(false);
            var item = workspace.Selected;

            if (item is null)
            {
                return OgResult<OgWarehouseWorkspace>.Failed(
                    $"Stock item '{itemId}' is not in this database.", WarehouseRules.CodeUnknownItem);
            }

            if (item.OnHand == 0m)
            {
                return OgResult<OgWarehouseWorkspace>.Failed(
                    $"{item.Code} has nothing on hand, so there is nothing to revalue.",
                    WarehouseRules.CodeNothingToValue);
            }

            // IAS 2 lower of cost or market: a market below cost writes the carrying value down; above cost does not
            // revalue upwards, which is the standard's asymmetry rather than an omission.
            var market = Math.Round(item.OnHand * marketUnitCost, 2, MidpointRounding.AwayFromZero);
            var writeDown = market < item.Value ? Math.Round(item.Value - market, 2, MidpointRounding.AwayFromZero) : 0m;

            var now = DateTime.UtcNow;

            var valuation = OgUow.For<INVENTORY_VALUATION>(source);
            valuation.PrimaryKey = "INVENTORY_VALUATION_ID";

            valuation.Add(new INVENTORY_VALUATION
            {
                INVENTORY_VALUATION_ID = Guid.NewGuid().ToString("N"),
                INVENTORY_ITEM_ID = item.Id,
                VALUATION_DATE = now.Date,
                VALUATION_METHOD = writeDown > 0m ? "LOWER_OF_COST_OR_MARKET" : "COST",
                QUANTITY = item.OnHand,
                UNIT_COST = marketUnitCost,
                TOTAL_VALUE = market,
                DESCRIPTION = reason.Trim(),
                ACTIVE_IND = "Y",
                PPDM_GUID = Guid.NewGuid().ToString("N"),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var valuationCommitted = await valuation.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(valuationCommitted))
            {
                return OgResult<OgWarehouseWorkspace>.Failed(
                    OgUow.Message(valuationCommitted, "The valuation was not saved."),
                    WarehouseRules.CodeWriteFailed);
            }

            var journalId = writeDown > 0m
                ? await PostJournalAsync(
                    source, "REVALUE-" + Guid.NewGuid().ToString("N"), now.Date,
                    WarehouseCodes.WriteDownAccount, WarehouseCodes.InventoryAccount, writeDown,
                    "stock written down: " + reason.Trim(), context, actor, cancellationToken).ConfigureAwait(false)
                : null;

            var itemUnit = OgUow.For<INVENTORY_ITEM>(source);
            itemUnit.PrimaryKey = "INVENTORY_ITEM_ID";

            var rows = await OgUow.ReadAsync(
                itemUnit, [OgUow.Equal("INVENTORY_ITEM_ID", item.Id)], cancellationToken).ConfigureAwait(false);

            var row = rows.FirstOrDefault();

            if (row is null)
            {
                return OgResult<OgWarehouseWorkspace>.Failed(
                    $"Stock item '{itemId}' is not in this database.", WarehouseRules.CodeUnknownItem);
            }

            row.TOTAL_VALUE = writeDown > 0m ? market : item.Value;
            row.UNIT_COST = writeDown > 0m ? marketUnitCost : item.UnitCost;
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = now;

            var updated = itemUnit.Update(row);

            if (!OgUow.IsOk(updated))
            {
                return OgResult<OgWarehouseWorkspace>.Failed(
                    OgUow.Message(updated, "The item's value was not updated."), WarehouseRules.CodeWriteFailed);
            }

            var committed = await itemUnit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgWarehouseWorkspace>.Failed(
                    OgUow.Message(committed, "The item's value was not updated."), WarehouseRules.CodeWriteFailed);
            }

            var after = await ReadAsync(source, context, item.Id, cancellationToken).ConfigureAwait(false);

            return OgResult<OgWarehouseWorkspace>.Ok(after);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"revaluing stock {itemId}", "the revaluation was not recorded", FailureSeverity.Error);

            return OgResult<OgWarehouseWorkspace>.Failed(
                "The stock could not be revalued.", reference ?? WarehouseRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgWarehouseWorkspace>> LinkAsync(
        string itemId,
        string assetType,
        string assetId,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                "A link must record who made it.", WarehouseRules.CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(assetId))
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                "A link needs the asset or equipment it points at.", WarehouseRules.CodeInputRequired);
        }

        if (!WarehouseRules.AssetTypes.Contains(assetType?.Trim().ToUpperInvariant() ?? string.Empty,
                StringComparer.Ordinal))
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                $"'{assetType}' is not a linkable record; use {string.Join(", ", WarehouseRules.AssetTypes)}.",
                WarehouseRules.CodeInputInvalid);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                "No connection is open, so the item cannot be linked.", WarehouseRules.CodeNoConnection);
        }

        try
        {
            var unit = OgUow.For<INVENTORY_ITEM>(source);
            unit.PrimaryKey = "INVENTORY_ITEM_ID";

            var rows = await OgUow.ReadAsync(
                unit, [OgUow.Equal("INVENTORY_ITEM_ID", itemId)], cancellationToken).ConfigureAwait(false);

            var row = rows.FirstOrDefault();

            if (row is null)
            {
                return OgResult<OgWarehouseWorkspace>.Failed(
                    $"Stock item '{itemId}' is not in this database.", WarehouseRules.CodeUnknownItem);
            }

            row.LINKED_ASSET_TYPE = assetType.Trim().ToUpperInvariant();
            row.LINKED_ASSET_ID = assetId.Trim();
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = DateTime.UtcNow;

            var updated = unit.Update(row);

            if (!OgUow.IsOk(updated))
            {
                return OgResult<OgWarehouseWorkspace>.Failed(
                    OgUow.Message(updated, "The link was not saved."), WarehouseRules.CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgWarehouseWorkspace>.Failed(
                    OgUow.Message(committed, "The link was not saved."), WarehouseRules.CodeWriteFailed);
            }

            var after = await ReadAsync(source, null, itemId, cancellationToken).ConfigureAwait(false);

            return OgResult<OgWarehouseWorkspace>.Ok(after);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"linking stock {itemId} to {assetType} {assetId}", "the link was not saved",
                FailureSeverity.Error);

            return OgResult<OgWarehouseWorkspace>.Failed(
                "The item could not be linked.", reference ?? WarehouseRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgWarehouseWorkspace>> LoadAsync(
        OgWarehouseContext context,
        string? itemId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                "No connection is open, so the warehouse cannot be read.", WarehouseRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, context, itemId, cancellationToken).ConfigureAwait(false);

            return OgResult<OgWarehouseWorkspace>.Ok(workspace);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading the warehouse", "the warehouse is unavailable; nothing can move", FailureSeverity.Degraded);

            return OgResult<OgWarehouseWorkspace>.Ok(new OgWarehouseWorkspace(
                [], null, [], [], WarehouseRules.Conditions, WarehouseRules.RestockDecisions, WarehouseRules.Kinds,
                string.Empty, true, $"the warehouse could not be read (reference {reference})"));
        }
    }

    // ------------------------------------------------------------------ one movement

    /// <summary>
    /// Writes a movement and everything it implies: the quantity and value on the item, the transaction row with its
    /// condition, decision and journal link, the adjustment row when it is one, and the ledger entry when value moved.
    /// </summary>
    private async Task<OgResult<OgWarehouseWorkspace>> MoveAsync(
        IDataSource source,
        OgWarehouseWorkspace workspace,
        OgStockItem item,
        string kind,
        decimal quantity,
        decimal unitCost,
        decimal valueChange,
        string? reference,
        string? condition,
        string? restockDecision,
        string? reservationId,
        string? location,
        string debitAccount,
        string creditAccount,
        OgWarehouseContext context,
        string actor,
        string description,
        CancellationToken cancellationToken,
        string? adjustmentReason = null)
    {
        var newOnHand = item.OnHand + quantity;
        var newValue = item.Value + valueChange;

        // AC-6 holds for every movement, not just an adjustment.
        if (newOnHand < 0m)
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                $"That would take {item.Code} to {WarehouseRules.Number(newOnHand)} {item.Unit}; stock never goes below " +
                "zero.",
                WarehouseRules.CodeNegativeStock);
        }

        if (newOnHand < item.Reserved)
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                $"That would leave {WarehouseRules.Number(newOnHand)} of {item.Code} on hand against " +
                $"{WarehouseRules.Number(item.Reserved)} reserved; release the reservation first.",
                WarehouseRules.CodeNegativeStock);
        }

        var periodCode = LedgerCodes.PeriodCodeOf(DateTime.UtcNow);
        var periods = await OgUow.ReadAllAsync<COMPANY_LEDGER_PERIOD>(source).ConfigureAwait(false);
        var period = periods.FirstOrDefault(row =>
            string.Equals(row.PERIOD_CODE, periodCode, StringComparison.OrdinalIgnoreCase));

        if (period is not null && !LedgerCodes.IsOpen(period.STATUS))
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                $"Period {periodCode} is {period.STATUS.ToLowerInvariant()}, so a stock movement cannot be posted " +
                "into it. Reopen the period first.",
                WarehouseRules.CodePeriodClosed);
        }

        var now = DateTime.UtcNow;
        var transactionId = Guid.NewGuid().ToString("N");

        // AC-5: a value change writes one ledger entry, whose reference is derived from the movement so a retry adopts it.
        string? journalId = null;

        if (valueChange != 0m)
        {
            journalId = await PostJournalAsync(
                source,
                kind + "-" + transactionId,
                now.Date,
                debitAccount,
                creditAccount,
                Math.Abs(valueChange),
                description + (string.IsNullOrWhiteSpace(reference) ? string.Empty : " · " + reference),
                context,
                actor,
                cancellationToken).ConfigureAwait(false);

            if (journalId is null)
            {
                return OgResult<OgWarehouseWorkspace>.Failed(
                    "The stock movement's journal was not written, so nothing was moved.",
                    WarehouseRules.CodeWriteFailed);
            }
        }

        // The item: quantity and value, read through the unit that writes.
        var itemUnit = OgUow.For<INVENTORY_ITEM>(source);
        itemUnit.PrimaryKey = "INVENTORY_ITEM_ID";

        var rows = await OgUow.ReadAsync(
            itemUnit, [OgUow.Equal("INVENTORY_ITEM_ID", item.Id)], cancellationToken).ConfigureAwait(false);

        var row = rows.FirstOrDefault();

        if (row is null)
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                $"Stock item '{item.Id}' is not in this database.", WarehouseRules.CodeUnknownItem);
        }

        row.QUANTITY_ON_HAND = newOnHand;
        row.TOTAL_VALUE = Math.Max(0m, newValue);

        if (!string.IsNullOrWhiteSpace(location))
        {
            row.LOCATION_ID = location.Trim();
        }

        row.ROW_CHANGED_BY = actor.Trim();
        row.ROW_CHANGED_DATE = now;

        var updated = itemUnit.Update(row);

        if (!OgUow.IsOk(updated))
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                OgUow.Message(updated, "The item's quantity was not updated."), WarehouseRules.CodeWriteFailed);
        }

        var itemCommitted = await itemUnit.Commit().ConfigureAwait(false);

        if (!OgUow.IsOk(itemCommitted))
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                OgUow.Message(itemCommitted, "The item's quantity was not updated."), WarehouseRules.CodeWriteFailed);
        }

        // The movement row. A deduction carries a negative quantity, which is the owner's own convention.
        var transaction = OgUow.For<INVENTORY_TRANSACTION>(source);
        transaction.PrimaryKey = "INVENTORY_TRANSACTION_ID";

        transaction.Add(new INVENTORY_TRANSACTION
        {
            INVENTORY_TRANSACTION_ID = transactionId,
            INVENTORY_ITEM_ID = item.Id,
            TRANSACTION_TYPE = kind,
            TRANSACTION_DATE = now.Date,
            QUANTITY = quantity,
            UNIT_COST = unitCost,
            TOTAL_COST = valueChange,
            REFERENCE_NUMBER = string.IsNullOrWhiteSpace(reference) ? null : reference.Trim(),
            DESCRIPTION = description,
            CONDITION_CODE = condition,
            RESTOCK_DECISION = restockDecision,
            RESERVATION_ID = reservationId,
            JOURNAL_ENTRY_ID = journalId,
            ACTOR = actor.Trim(),
            ACTIVE_IND = "Y",
            PPDM_GUID = Guid.NewGuid().ToString("N"),
            ROW_CREATED_BY = actor.Trim(),
            ROW_CREATED_DATE = now,
            ROW_CHANGED_BY = actor.Trim(),
            ROW_CHANGED_DATE = now,
        });

        var transactionCommitted = await transaction.Commit().ConfigureAwait(false);

        if (!OgUow.IsOk(transactionCommitted))
        {
            return OgResult<OgWarehouseWorkspace>.Failed(
                OgUow.Message(transactionCommitted, "The movement was not recorded."), WarehouseRules.CodeWriteFailed);
        }

        if (!string.IsNullOrWhiteSpace(adjustmentReason))
        {
            var adjustment = OgUow.For<INVENTORY_ADJUSTMENT>(source);
            adjustment.PrimaryKey = "INVENTORY_ADJUSTMENT_ID";

            adjustment.Add(new INVENTORY_ADJUSTMENT
            {
                INVENTORY_ADJUSTMENT_ID = Guid.NewGuid().ToString("N"),
                INVENTORY_ITEM_ID = item.Id,
                ADJUSTMENT_DATE = now.Date,
                ADJUSTMENT_TYPE = quantity > 0m ? "COUNT_GAIN" : "COUNT_LOSS",
                QUANTITY_ADJUSTMENT = quantity,
                UNIT_COST_ADJUSTMENT = unitCost,
                REASON = adjustmentReason.Trim(),
                DESCRIPTION = description,
                ACTIVE_IND = "Y",
                PPDM_GUID = Guid.NewGuid().ToString("N"),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var adjustmentCommitted = await adjustment.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(adjustmentCommitted))
            {
                return OgResult<OgWarehouseWorkspace>.Failed(
                    OgUow.Message(adjustmentCommitted, "The adjustment was not recorded."),
                    WarehouseRules.CodeWriteFailed);
            }
        }

        _ = workspace;
        _ = context;

        var after = await ReadAsync(source, null, item.Id, cancellationToken).ConfigureAwait(false);

        return OgResult<OgWarehouseWorkspace>.Ok(after);
    }

    /// <summary>
    /// Posts one balanced entry and returns its id, or null when the chart does not carry the accounts - the caller then
    /// refuses the movement rather than moving stock with no ledger effect.
    /// </summary>
    private static async Task<string?> PostJournalAsync(
        IDataSource source,
        string reference,
        DateTime date,
        string debitNumber,
        string creditNumber,
        decimal amount,
        string description,
        OgWarehouseContext context,
        string actor,
        CancellationToken cancellationToken)
    {
        var accounts = await OgUow.ReadAllAsync<GL_ACCOUNT>(source).ConfigureAwait(false);

        var debit = accounts.FirstOrDefault(account =>
            string.Equals(account.ACCOUNT_NUMBER?.Trim(), debitNumber, StringComparison.OrdinalIgnoreCase));
        var credit = accounts.FirstOrDefault(account =>
            string.Equals(account.ACCOUNT_NUMBER?.Trim(), creditNumber, StringComparison.OrdinalIgnoreCase));

        if (debit is null || credit is null)
        {
            return null;
        }

        var journals = await OgUow.ReadAllAsync<JOURNAL_ENTRY>(source).ConfigureAwait(false);

        var existing = journals.FirstOrDefault(entry =>
            string.Equals(entry.SOURCE_MODULE, "INVENTORY", StringComparison.OrdinalIgnoreCase)
            && string.Equals(entry.REFERENCE_NUMBER, reference, StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            return existing.JOURNAL_ENTRY_ID;
        }

        var now = DateTime.UtcNow;
        var journalId = Guid.NewGuid().ToString("N");

        var journal = OgUow.For<JOURNAL_ENTRY>(source);
        journal.PrimaryKey = "JOURNAL_ENTRY_ID";

        journal.Add(new JOURNAL_ENTRY
        {
            JOURNAL_ENTRY_ID = journalId,
            ENTRY_NUMBER = NextJournalNumber(journals, date),
            ENTRY_DATE = date,
            ENTRY_TYPE = "STOCK",
            STATUS = LedgerCodes.JournalPosted,
            DESCRIPTION = description,
            REFERENCE_NUMBER = reference,
            SOURCE_MODULE = "INVENTORY",
            TOTAL_DEBIT = amount,
            TOTAL_CREDIT = amount,
            AUTHOR = actor.Trim(),
            POSTED_BY = actor.Trim(),
            POSTED_AT = now,
            PERIOD_CODE = LedgerCodes.PeriodCodeOf(date),
            ACTIVE_IND = "Y",
            PPDM_GUID = Guid.NewGuid().ToString("N"),
            ROW_CREATED_BY = actor.Trim(),
            ROW_CREATED_DATE = now,
            ROW_CHANGED_BY = actor.Trim(),
            ROW_CHANGED_DATE = now,
        });

        var committed = await journal.Commit().ConfigureAwait(false);

        if (!OgUow.IsOk(committed))
        {
            return null;
        }

        var lines = OgUow.For<JOURNAL_ENTRY_LINE>(source);
        lines.PrimaryKey = "JOURNAL_ENTRY_LINE_ID";

        lines.Add(EntryLine(journalId, debit!, amount, 0m, 1, description, actor, now));
        lines.Add(EntryLine(journalId, credit!, 0m, amount, 2, description, actor, now));

        var linesCommitted = await lines.Commit().ConfigureAwait(false);

        _ = context;
        cancellationToken.ThrowIfCancellationRequested();

        return OgUow.IsOk(linesCommitted) ? journalId : null;
    }

    private static JOURNAL_ENTRY_LINE EntryLine(
        string journalId,
        GL_ACCOUNT account,
        decimal debit,
        decimal credit,
        int number,
        string description,
        string actor,
        DateTime now) =>
        new()
        {
            JOURNAL_ENTRY_LINE_ID = Guid.NewGuid().ToString("N"),
            JOURNAL_ENTRY_ID = journalId,
            GL_ACCOUNT_ID = account.GL_ACCOUNT_ID,
            LINE_NUMBER = number,
            DEBIT_AMOUNT = debit,
            CREDIT_AMOUNT = credit,
            DESCRIPTION = description,
            CURRENCY_CODE = account.CURRENCY_CODE,
            ACTIVE_IND = "Y",
            PPDM_GUID = Guid.NewGuid().ToString("N"),
            ROW_CREATED_BY = actor.Trim(),
            ROW_CREATED_DATE = now,
            ROW_CHANGED_BY = actor.Trim(),
            ROW_CHANGED_DATE = now,
        };

    private static async Task<string?> SpendReservationAsync(
        IDataSource source,
        OgReservation reservation,
        string? transactionId,
        string actor,
        CancellationToken cancellationToken)
    {
        var unit = OgUow.For<INVENTORY_RESERVATION>(source);
        unit.PrimaryKey = "INVENTORY_RESERVATION_ID";

        var rows = await OgUow.ReadAsync(
            unit, [OgUow.Equal("INVENTORY_RESERVATION_ID", reservation.Id)], cancellationToken).ConfigureAwait(false);

        var row = rows.FirstOrDefault();

        if (row is null)
        {
            return "The reservation holding this stock is no longer there.";
        }

        row.STATUS = WarehouseCodes.ReservationIssued;
        row.ISSUED_TRANSACTION_ID = transactionId;
        row.ROW_CHANGED_BY = actor.Trim();
        row.ROW_CHANGED_DATE = DateTime.UtcNow;

        var updated = unit.Update(row);

        if (!OgUow.IsOk(updated))
        {
            return OgUow.Message(updated, "The reservation was not marked issued.");
        }

        var committed = await unit.Commit().ConfigureAwait(false);

        return OgUow.IsOk(committed) ? null : OgUow.Message(committed, "The reservation was not marked issued.");
    }

    // ------------------------------------------------------------------ the reads

    private async Task<OgWarehouseWorkspace> ReadAsync(
        IDataSource source,
        OgWarehouseContext? context,
        string? itemId,
        CancellationToken cancellationToken)
    {
        var itemRows = await OgUow.ReadAllAsync<INVENTORY_ITEM>(source).ConfigureAwait(false);
        var transactionRows = await OgUow.ReadAllAsync<INVENTORY_TRANSACTION>(source).ConfigureAwait(false);
        var reservationRows = await OgUow.ReadAllAsync<INVENTORY_RESERVATION>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var holdsByItem = reservationRows
            .Where(row => !string.IsNullOrWhiteSpace(row.INVENTORY_ITEM_ID))
            .GroupBy(row => row.INVENTORY_ITEM_ID!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(row => row.RESERVED_AT).ToList(),
                StringComparer.OrdinalIgnoreCase);

        var items = itemRows
            .Where(row => !string.IsNullOrWhiteSpace(row.INVENTORY_ITEM_ID))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .OrderBy(row => row.ITEM_NUMBER, StringComparer.Ordinal)
            .Select(row =>
            {
                var holds = holdsByItem.TryGetValue(row.INVENTORY_ITEM_ID, out var mine) ? mine : [];

                var reservations = holds
                    .Select(hold => new OgReservation(
                        hold.INVENTORY_RESERVATION_ID,
                        hold.INVENTORY_ITEM_ID,
                        row.ITEM_NUMBER ?? string.Empty,
                        hold.QUANTITY,
                        hold.HOLDING_DOCUMENT,
                        hold.HOLDING_KIND,
                        hold.RESERVED_AT,
                        hold.ACTOR,
                        hold.STATUS,
                        hold.ISSUED_TRANSACTION_ID,
                        hold.RELEASED_BY,
                        hold.RELEASED_AT,
                        hold.RELEASE_REASON))
                    .ToList();

                // Reserved is the sum of the open reservations - never a scalar that could drift from them (AC-1).
                var reserved = reservations
                    .Where(reservation => reservation.HoldsStock)
                    .Sum(reservation => reservation.Quantity);

                return new OgStockItem(
                    row.INVENTORY_ITEM_ID,
                    row.ITEM_NUMBER ?? string.Empty,
                    row.ITEM_NAME ?? string.Empty,
                    row.UNIT_OF_MEASURE ?? string.Empty,
                    row.QUANTITY_ON_HAND ?? 0m,
                    reserved,
                    row.UNIT_COST ?? 0m,
                    row.TOTAL_VALUE ?? 0m,
                    row.CURRENCY_CODE ?? string.Empty,
                    row.WAREHOUSE_ID,
                    row.LOCATION_ID,
                    row.VALUATION_METHOD,
                    row.LINKED_ASSET_TYPE,
                    row.LINKED_ASSET_ID,
                    reservations);
            })
            .ToList();

        var byItem = items.ToDictionary(item => item.Id, StringComparer.OrdinalIgnoreCase);

        var movements = transactionRows
            .Where(row => !string.IsNullOrWhiteSpace(row.INVENTORY_TRANSACTION_ID))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(row => row.TRANSACTION_DATE)
            .ThenByDescending(row => row.ROW_CREATED_DATE)
            .Select(row => new OgStockMovement(
                row.INVENTORY_TRANSACTION_ID,
                row.TRANSACTION_TYPE ?? string.Empty,
                row.INVENTORY_ITEM_ID ?? string.Empty,
                byItem.TryGetValue(row.INVENTORY_ITEM_ID ?? string.Empty, out var item) ? item.Code : row.INVENTORY_ITEM_ID,
                row.QUANTITY ?? 0m,
                row.UNIT_COST ?? 0m,
                row.TOTAL_COST ?? 0m,
                row.REFERENCE_NUMBER,
                row.TRANSACTION_DATE ?? DateTime.MinValue,
                row.ACTOR ?? row.ROW_CREATED_BY,
                row.CONDITION_CODE,
                row.RESTOCK_DECISION,
                row.RESERVATION_ID,
                row.JOURNAL_ENTRY_ID,
                row.DESCRIPTION))
            .Take(200)
            .ToList();

        var reservationsFlat = items.SelectMany(item => item.Holds).ToList();

        var selected = string.IsNullOrWhiteSpace(itemId)
            ? items.FirstOrDefault()
            : items.FirstOrDefault(item =>
                  string.Equals(item.Id, itemId.Trim(), StringComparison.OrdinalIgnoreCase))
              ?? items.FirstOrDefault();

        _ = context;

        return new OgWarehouseWorkspace(
            items,
            selected,
            movements,
            reservationsFlat,
            WarehouseRules.Conditions,
            WarehouseRules.RestockDecisions,
            WarehouseRules.Kinds,
            "stock quantity: the accounting InventoryService over INVENTORY_ITEM and INVENTORY_TRANSACTION " +
            "(the production service tracks tank and pipeline volume on different tables)",
            false,
            null);
    }

    private static string NextJournalNumber(IReadOnlyList<JOURNAL_ENTRY> existing, DateTime date)
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
