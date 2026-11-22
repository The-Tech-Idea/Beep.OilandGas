using System.Globalization;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Work;

/// <summary>One labour line of a work order (WRK-02).</summary>
/// <param name="Cost">Quantity x the recorded rate, or null when the line records no rate - an unknown cost, never a zero.</param>
public sealed record OgLabourLine(
    string Id,
    string Person,
    string Duty,
    DateTime FromUtc,
    DateTime ToUtc,
    decimal Quantity,
    string Unit,
    string Actor,
    decimal? UnitCost,
    string Currency)
{
    public decimal? Cost => UnitCost is { } rate ? decimal.Round(Quantity * rate, 2, MidpointRounding.AwayFromZero) : null;

    public string Describe() =>
        Person + " · " + Duty + " · " + Quantity.ToString("0.####", CultureInfo.InvariantCulture) + " " + Unit +
        " · " + (Cost is { } cost
            ? cost.ToString("0.00", CultureInfo.InvariantCulture) + " " + Currency
            : "no rate recorded, so no cost") +
        " · " + Actor + " on " + FromUtc.ToLocalTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}

/// <summary>One material line of a work order (WRK-02).</summary>
public sealed record OgMaterialLine(
    string Id,
    string ItemId,
    string ItemName,
    decimal Quantity,
    string Unit,
    string Currency,
    decimal? UnitCost,
    DateTime UsedAtUtc,
    string? AllocationId)
{
    public decimal? Cost => UnitCost is { } rate ? decimal.Round(Quantity * rate, 2, MidpointRounding.AwayFromZero) : null;

    public string Describe() =>
        ItemName + " · " + Quantity.ToString("0.####", CultureInfo.InvariantCulture) + " " + Unit +
        " · " + (Cost is { } cost
            ? cost.ToString("0.00", CultureInfo.InvariantCulture) + " " + Currency
            : "no unit cost recorded, so no cost") +
        " · used " + UsedAtUtc.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}

/// <summary>The rolled-up actual cost of one work order (WRK-02, AC-2).</summary>
public sealed record OgWorkCost(string Currency, decimal Labour, decimal Materials)
{
    public decimal Total => Labour + Materials;

    /// <summary>True when every line in this currency could be costed; otherwise the total is only the costed lines.</summary>
    public bool Complete { get; init; }

    public int UncostedLines { get; init; }

    public string Describe() =>
        Total.ToString("0.00", CultureInfo.InvariantCulture) + " " + Currency +
        " = labour " + Labour.ToString("0.00", CultureInfo.InvariantCulture) +
        " + materials " + Materials.ToString("0.00", CultureInfo.InvariantCulture) +
        (Complete ? string.Empty : " (partial: " + UncostedLines.ToString(CultureInfo.InvariantCulture) +
                                    " line(s) record no cost)");
}

/// <summary>The execution permit's readiness for a work order (WRK-02, AC-3).</summary>
public sealed record OgPermitState(bool IsValid, string Sentence);

/// <summary>The evidence a completion needs (WRK-02, AC-4).</summary>
public sealed record OgCompletionEvidence(bool IsRecorded, string Sentence);

/// <summary>One work order with its lines, its cost and what may happen next (WRK-02).</summary>
public sealed record OgWorkOrderDetail(
    string Id,
    string Number,
    string Kind,
    string Subtype,
    string? FieldId,
    string? AssetId,
    string? Assignee,
    string State,
    string? StateRecordedBy,
    DateTime? RequestedAt,
    DateTime? DueAt,
    DateTime? CompletedAt,
    IReadOnlyList<OgLabourLine> Labour,
    IReadOnlyList<OgMaterialLine> Materials,
    IReadOnlyList<OgWorkCost> Costs,
    OgPermitState Permit,
    OgCompletionEvidence Evidence,
    IReadOnlyList<string> AvailableTransitions,
    IReadOnlyList<string> Blocks,
    DateTime ReadAt)
{
    public bool IsComplete => string.Equals(State, WorkOrderState.Completed, StringComparison.OrdinalIgnoreCase);

    /// <summary>AC-2: the actual cost is the lines' own total, never a number typed on the order.</summary>
    public string CostText => Costs.Count == 0
        ? "no labour or material line is recorded, so there is no actual cost"
        : string.Join(Environment.NewLine, Costs.Select(cost => cost.Describe()));

    public decimal? TotalFor(string currency) =>
        Costs.FirstOrDefault(cost => string.Equals(cost.Currency, currency, StringComparison.OrdinalIgnoreCase))?.Total;

    public string LabourText => Labour.Count == 0
        ? "no labour line is recorded"
        : string.Join(Environment.NewLine, Labour.Select(line => line.Describe()));

    public string MaterialText => Materials.Count == 0
        ? "no material line is recorded"
        : string.Join(Environment.NewLine, Materials.Select(line => line.Describe()));

    public string TransitionText => AvailableTransitions.Count == 0
        ? "This order has no available transition."
        : "Available: " + string.Join(", ", AvailableTransitions);

    /// <summary>The checklist, so the page and the command agree (AC-1, AC-3, AC-4).</summary>
    public string ChecklistText => Blocks.Count == 0
        ? "Nothing blocks the offered transitions."
        : string.Join(Environment.NewLine, Blocks.Select(block => "✗ " + block));

    public string ReadText =>
        "read at " + ReadAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
}

/// <summary>The work-order detail rules WRK-02 states once.</summary>
public static class WorkOrderDetailRules
{
    public const string CodeNoConnection = "workorder-no-connection";
    public const string CodeReadFailed = "workorder-read-failed";
    public const string CodeWriteFailed = "workorder-write-failed";
    public const string CodeActorRequired = "workorder-actor-required";
    public const string CodeUnknownOrder = "workorder-unknown-order";
    public const string CodeNoProject = "workorder-no-project";
    public const string CodeTransitionUnavailable = "workorder-transition-unavailable";
    public const string CodePermitRequired = "workorder-permit-required";
    public const string CodeEvidenceRequired = "workorder-evidence-required";
    public const string CodeSelfApproval = "workorder-self-approval";
    public const string CodeQuantityInvalid = "workorder-quantity-invalid";
    public const string CodeStockShort = "workorder-stock-short";
    public const string CodeItemUnknown = "workorder-item-unknown";
    public const string CodeLabourInvalid = "workorder-labour-invalid";

    /// <summary>
    /// The permit states that do not authorise execution. A permit that is inside its recorded window and not in this set is
    /// valid: the owner's permit status vocabulary is written by its own decision flow and is not enumerated for the desktop,
    /// so the rule names what refuses rather than guessing every positive spelling.
    /// </summary>
    public static IReadOnlyList<string> RefusingPermitStates { get; } =
        ["REJECTED", "WITHDRAWN", "CANCELLED", "EXPIRED", "PENDING", "DRAFT", "REVOKED"];

    /// <summary>AC-3: a permit authorises execution when it covers the moment and has not been refused.</summary>
    public static OgPermitState PermitOf(
        string? status,
        DateTime? validFromUtc,
        DateTime? validToUtc,
        DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return new OgPermitState(false,
                "No execution permit is recorded for this order, so execution cannot start.");
        }

        if (RefusingPermitStates.Contains(status.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            return new OgPermitState(false,
                "The recorded execution permit is " + status.Trim().ToLowerInvariant() + ", so it does not authorise execution.");
        }

        if (validFromUtc is null || validToUtc is null)
        {
            return new OgPermitState(false,
                "The execution permit records no validity window, so it cannot be shown to cover this moment.");
        }

        if (nowUtc < validFromUtc.Value || nowUtc > validToUtc.Value)
        {
            return new OgPermitState(false,
                "The execution permit is valid from " +
                validFromUtc.Value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) + " to " +
                validToUtc.Value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) +
                " UTC, which does not cover now.");
        }

        return new OgPermitState(true,
            "The execution permit is " + status.Trim().ToLowerInvariant() + " and valid until " +
            validToUtc.Value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) + " UTC.");
    }

    /// <summary>
    /// AC-4: completion needs recorded evidence. The owner's work-order condition table carries no status or result column, so
    /// a recorded close-out condition - with its inspector, its date and its remark - is what counts as evidence.
    /// </summary>
    public static OgCompletionEvidence EvidenceOf(IEnumerable<WORK_ORDER_CONDITION> conditions)
    {
        ArgumentNullException.ThrowIfNull(conditions);

        var closeOut = conditions
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => string.Equals(row.CONDITION_TYPE?.Trim(), ConditionType.CloseOut, StringComparison.OrdinalIgnoreCase))
            .Where(row => !string.IsNullOrWhiteSpace(row.BUSINESS_ASSOCIATE_ID) &&
                          !string.IsNullOrWhiteSpace(row.REMARK) &&
                          row.EFFECTIVE_DATE is not null)
            .ToList();

        return closeOut.Count > 0
            ? new OgCompletionEvidence(true,
                closeOut.Count.ToString(CultureInfo.InvariantCulture) +
                " recorded close-out condition(s) with an inspector, a date and a remark.")
            : new OgCompletionEvidence(false,
                "No close-out condition with an inspector, a date and a remark is recorded, so the completion has no evidence.");
    }

    /// <summary>AC-2: the actual cost, per currency, from the lines themselves.</summary>
    public static IReadOnlyList<OgWorkCost> CostsOf(
        IEnumerable<OgLabourLine> labour,
        IEnumerable<OgMaterialLine> materials)
    {
        ArgumentNullException.ThrowIfNull(labour);
        ArgumentNullException.ThrowIfNull(materials);

        var labourLines = labour.ToList();
        var materialLines = materials.ToList();

        var currencies = labourLines.Select(line => line.Currency)
            .Concat(materialLines.Select(line => line.Currency))
            .Where(currency => !string.IsNullOrWhiteSpace(currency))
            .Distinct(StringComparer.OrdinalIgnoreCase);

        return currencies.Select(currency =>
        {
            var mineLabour = labourLines.Where(line => string.Equals(line.Currency, currency, StringComparison.OrdinalIgnoreCase)).ToList();
            var mineMaterials = materialLines.Where(line => string.Equals(line.Currency, currency, StringComparison.OrdinalIgnoreCase)).ToList();

            var uncosted = mineLabour.Count(line => line.Cost is null) + mineMaterials.Count(line => line.Cost is null);

            return new OgWorkCost(
                currency,
                mineLabour.Sum(line => line.Cost ?? 0m),
                mineMaterials.Sum(line => line.Cost ?? 0m))
            {
                Complete = uncosted == 0,
                UncostedLines = uncosted,
            };
        }).OrderBy(cost => cost.Currency, StringComparer.Ordinal).ToList();
    }

    /// <summary>AC-1: the offered transitions, from the owner's own graph.</summary>
    public static IReadOnlyList<string> Transitions(string? state, string? subtype) =>
        WorkOrderWorkflow.Available(state?.Trim().ToUpperInvariant() ?? string.Empty, subtype?.Trim() ?? string.Empty);

    /// <summary>The available stock for one item: what is on hand, less what is already allocated.</summary>
    public static decimal Available(decimal onHand, IEnumerable<decimal> allocated) =>
        onHand - allocated.Sum();

    /// <summary>AC-5: the person who submitted the order cannot approve it.</summary>
    public static (string Code, string Message)? CheckApprover(
        string? submittedBy,
        string? actor,
        string? target)
    {
        if (!string.Equals(target?.Trim(), WorkOrderState.Completed, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(target?.Trim(), WorkOrderState.UnderReview, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return string.Equals(submittedBy?.Trim(), actor?.Trim(), StringComparison.OrdinalIgnoreCase)
            ? (CodeSelfApproval, submittedBy + " submitted this order and therefore cannot approve or complete it.")
            : null;
    }
}

/// <summary>WRK-02: a work order's lines, cost, transitions and gates.</summary>
public interface IOgWorkOrderDetail
{
    Task<OgResult<OgWorkOrderDetail>> LoadAsync(
        string orderId,
        DateTime asOf,
        CancellationToken cancellationToken = default);

    /// <summary>Moves the order to a target the owner's graph offers; refused otherwise (AC-1, AC-3, AC-4, AC-5).</summary>
    Task<OgResult<OgWorkOrderDetail>> TransitionAsync(
        string orderId,
        string target,
        string note,
        string actor,
        DateTime asOf,
        CancellationToken cancellationToken = default);

    /// <summary>Records a labour line with the rate it is costed at (AC-6).</summary>
    Task<OgResult<OgWorkOrderDetail>> AddLabourAsync(
        string orderId,
        string person,
        string duty,
        DateTime fromUtc,
        DateTime toUtc,
        decimal quantity,
        string unit,
        decimal unitCost,
        string currency,
        string actor,
        DateTime asOf,
        CancellationToken cancellationToken = default);

    /// <summary>Reserves material, refusing more than is available and naming the short quantity (SG-13-3).</summary>
    Task<OgResult<OgWorkOrderDetail>> ReserveMaterialAsync(
        string orderId,
        string itemId,
        decimal quantity,
        string unit,
        string warehouseId,
        string actor,
        DateTime asOf,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// WRK-02 over the owner's <c>WORK_ORDER</c>, <c>WORK_ORDER_COMPONENT</c>, <c>WORK_ORDER_LABOR_USAGE</c>,
/// <c>WORK_ORDER_MATERIAL_ALLOCATION</c>, <c>WORK_ORDER_MATERIAL_USAGE</c>, <c>WORK_ORDER_CONDITION</c>,
/// <c>WORK_ORDER_EXECUTION_PERMIT</c>, <c>INVENTORY_ITEM</c> and <c>PROJECT_STATUS</c>.
/// </summary>
/// <remarks>
/// The transitions come from the owner's own <see cref="WorkOrderWorkflow"/>, and a chosen one is appended to the linked
/// project's status history - the store the owner's workflow uses - rather than written to the header, which the owner says does
/// not establish operational state. A labour line's cost is its quantity times the rate recorded on the line; a material line's
/// cost is its quantity times the inventory item's recorded unit cost; the order's actual cost is their sum and is never typed.
/// </remarks>
public sealed class OgWorkOrderDetailService : IOgWorkOrderDetail
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgWorkOrderDetailService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgWorkOrderDetail>> LoadAsync(
        string orderId,
        DateTime asOf,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(orderId))
        {
            return OgResult<OgWorkOrderDetail>.Failed(
                "A work order is opened by its id; none is named.", WorkOrderDetailRules.CodeUnknownOrder);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWorkOrderDetail>.Failed(
                "No connection is open, so the work order cannot be read.", WorkOrderDetailRules.CodeNoConnection);
        }

        try
        {
            return await ReadAsync(source, orderId.Trim(), asOf, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading work order " + orderId, "the work order is not shown", FailureSeverity.Error);

            return OgResult<OgWorkOrderDetail>.Failed(
                "The work order could not be read" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? WorkOrderDetailRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgWorkOrderDetail>> TransitionAsync(
        string orderId,
        string target,
        string note,
        string actor,
        DateTime asOf,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgWorkOrderDetail>.Failed(
                "A transition must record who made it.", WorkOrderDetailRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWorkOrderDetail>.Failed(
                "No connection is open, so the order cannot change state.", WorkOrderDetailRules.CodeNoConnection);
        }

        try
        {
            var read = await ReadAsync(source, orderId.Trim(), asOf, cancellationToken).ConfigureAwait(false);

            if (!read.Success || read.Value is null)
            {
                return read;
            }

            var detail = read.Value;
            var normalized = target?.Trim().ToUpperInvariant() ?? string.Empty;

            // AC-1: the owner's graph decides, not this screen.
            if (!detail.AvailableTransitions.Contains(normalized, StringComparer.Ordinal))
            {
                return OgResult<OgWorkOrderDetail>.Failed(
                    "A work order in " + (string.IsNullOrWhiteSpace(detail.State) ? "(no recorded state)" : detail.State) +
                    " may move to " +
                    (detail.AvailableTransitions.Count == 0
                        ? "nothing - it is terminal or has no recorded subtype"
                        : string.Join(", ", detail.AvailableTransitions)) + ", not " + (target ?? "(none)") + ".",
                    WorkOrderDetailRules.CodeTransitionUnavailable);
            }

            // AC-3: execution start needs a permit that covers this moment.
            if (string.Equals(normalized, WorkOrderState.InProgress, StringComparison.Ordinal) && !detail.Permit.IsValid)
            {
                return OgResult<OgWorkOrderDetail>.Failed(detail.Permit.Sentence, WorkOrderDetailRules.CodePermitRequired);
            }

            // AC-4: a completion needs recorded evidence.
            if (string.Equals(normalized, WorkOrderState.Completed, StringComparison.Ordinal) && !detail.Evidence.IsRecorded)
            {
                return OgResult<OgWorkOrderDetail>.Failed(detail.Evidence.Sentence, WorkOrderDetailRules.CodeEvidenceRequired);
            }

            // AC-5: the person who submitted cannot approve or complete it.
            if (WorkOrderDetailRules.CheckApprover(detail.StateRecordedBy, actor, normalized) is { } self)
            {
                return OgResult<OgWorkOrderDetail>.Failed(self.Message, self.Code);
            }

            var projectId = await ProjectOfAsync(source, detail.Id, cancellationToken).ConfigureAwait(false);

            if (projectId is null)
            {
                return OgResult<OgWorkOrderDetail>.Failed(
                    detail.Number + " has no linked execution project, so its state cannot be recorded.",
                    WorkOrderDetailRules.CodeNoProject);
            }

            var now = DateTime.UtcNow;

            var unit = OgUow.For<PROJECT_STATUS>(source);
            unit.PrimaryKey = nameof(PROJECT_STATUS.PROJECT_ID);

            unit.Add(new PROJECT_STATUS
            {
                PROJECT_ID = projectId,
                STATUS_ID = Guid.NewGuid().ToString("N"),
                STATUS_TYPE = "WO_STATE",
                STATUS = normalized,
                EFFECTIVE_DATE = now,
                REMARK = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
                ACTIVE_IND = "Y",
                SOURCE = "BeepOilGasDesktop",
                PPDM_GUID = Guid.NewGuid().ToString("N"),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgWorkOrderDetail>.Failed(
                    OgUow.Message(committed, "The transition was not saved."), WorkOrderDetailRules.CodeWriteFailed);
            }

            return await ReadAsync(source, detail.Id, asOf, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "recording a transition on " + orderId, "the transition was not saved", FailureSeverity.Error);

            return OgResult<OgWorkOrderDetail>.Failed(
                "The transition could not be saved" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? WorkOrderDetailRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgWorkOrderDetail>> AddLabourAsync(
        string orderId,
        string person,
        string duty,
        DateTime fromUtc,
        DateTime toUtc,
        decimal quantity,
        string unit,
        decimal unitCost,
        string currency,
        string actor,
        DateTime asOf,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor) || string.IsNullOrWhiteSpace(person))
        {
            return OgResult<OgWorkOrderDetail>.Failed(
                "A labour line records the person and who recorded it.", WorkOrderDetailRules.CodeLabourInvalid);
        }

        if (quantity <= 0m || unitCost < 0m || string.IsNullOrWhiteSpace(currency) || toUtc < fromUtc)
        {
            return OgResult<OgWorkOrderDetail>.Failed(
                "A labour line needs a positive quantity, a non-negative rate with its currency, and an end after its start.",
                WorkOrderDetailRules.CodeLabourInvalid);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWorkOrderDetail>.Failed(
                "No connection is open, so the labour line cannot be saved.", WorkOrderDetailRules.CodeNoConnection);
        }

        try
        {
            var read = await ReadAsync(source, orderId.Trim(), asOf, cancellationToken).ConfigureAwait(false);

            if (!read.Success || read.Value is null)
            {
                return read;
            }

            var detail = read.Value;
            var now = DateTime.UtcNow;

            var unitOfWork = OgUow.For<WORK_ORDER_LABOR_USAGE>(source);
            unitOfWork.PrimaryKey = nameof(WORK_ORDER_LABOR_USAGE.LABOR_ID);

            unitOfWork.Add(new WORK_ORDER_LABOR_USAGE
            {
                LABOR_ID = Guid.NewGuid().ToString("N"),
                OWNER_TENANT_ID = "APP",
                OWNER_COMPANY_BA_ID = "APP",
                FIELD_ID = detail.FieldId ?? string.Empty,
                WORK_ORDER_ID = detail.Id,
                // The owner marks these required, and the desktop has no process instance or assignment id: the
                // placeholder says so rather than leaving the field blank.
                PROCESS_INSTANCE_ID = "NO-PROCESS",
                START_ID = "NO-START",
                ASSIGNMENT_ID = "NO-ASSIGNMENT",
                EMPLOYEE_BA_ID = person.Trim(),
                EXECUTOR_USER_ID = actor.Trim(),
                DUTY_CODE = string.IsNullOrWhiteSpace(duty) ? "GENERAL" : duty.Trim(),
                LABOR_FROM_UTC = fromUtc,
                LABOR_TO_UTC = toUtc,
                QUANTITY = quantity,
                QUANTITY_UNIT = string.IsNullOrWhiteSpace(unit) ? "HR" : unit.Trim(),
                UNIT_COST = unitCost,
                CURRENCY_CODE = currency.Trim().ToUpperInvariant(),
                COMMAND_JSON = "{}",
                RECORDED_REVISION = now.ToString("O", CultureInfo.InvariantCulture),
                ACTIVE_IND = "Y",
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unitOfWork.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgWorkOrderDetail>.Failed(
                    OgUow.Message(committed, "The labour line was not saved."), WorkOrderDetailRules.CodeWriteFailed);
            }

            return await ReadAsync(source, detail.Id, asOf, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "recording a labour line on " + orderId, "the labour line was not saved", FailureSeverity.Error);

            return OgResult<OgWorkOrderDetail>.Failed(
                "The labour line could not be saved" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? WorkOrderDetailRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgWorkOrderDetail>> ReserveMaterialAsync(
        string orderId,
        string itemId,
        decimal quantity,
        string unit,
        string warehouseId,
        string actor,
        DateTime asOf,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgWorkOrderDetail>.Failed(
                "A reservation must record who made it.", WorkOrderDetailRules.CodeActorRequired);
        }

        if (quantity <= 0m)
        {
            return OgResult<OgWorkOrderDetail>.Failed(
                "A reservation needs a positive quantity.", WorkOrderDetailRules.CodeQuantityInvalid);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgWorkOrderDetail>.Failed(
                "No connection is open, so the material cannot be reserved.", WorkOrderDetailRules.CodeNoConnection);
        }

        try
        {
            var read = await ReadAsync(source, orderId.Trim(), asOf, cancellationToken).ConfigureAwait(false);

            if (!read.Success || read.Value is null)
            {
                return read;
            }

            var detail = read.Value;

            var items = await OgUow.ReadAllAsync<INVENTORY_ITEM>(source).ConfigureAwait(false);
            var allocations = await OgUow.ReadAllAsync<WORK_ORDER_MATERIAL_ALLOCATION>(source).ConfigureAwait(false);

            var item = items.FirstOrDefault(row =>
                string.Equals(row.INVENTORY_ITEM_ID, itemId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (item is null)
            {
                return OgResult<OgWorkOrderDetail>.Failed(
                    "Item '" + itemId + "' is not in the inventory register, so it cannot be reserved.",
                    WorkOrderDetailRules.CodeItemUnknown);
            }

            var allocated = allocations
                .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                .Where(row => string.Equals(row.INVENTORY_ITEM_ID, item.INVENTORY_ITEM_ID, StringComparison.OrdinalIgnoreCase))
                .Select(row => row.QUANTITY);

            var available = WorkOrderDetailRules.Available(item.QUANTITY_ON_HAND ?? 0m, allocated);

            // SG-13-3: a reservation above availability is refused, naming the short quantity.
            if (quantity > available)
            {
                return OgResult<OgWorkOrderDetail>.Failed(
                    "Only " + available.ToString("0.####", CultureInfo.InvariantCulture) + " " +
                    (item.UNIT_OF_MEASURE ?? unit) + " of " + (item.ITEM_NAME ?? item.INVENTORY_ITEM_ID) +
                    " is available, so " + quantity.ToString("0.####", CultureInfo.InvariantCulture) +
                    " cannot be reserved; the short quantity is " +
                    (quantity - available).ToString("0.####", CultureInfo.InvariantCulture) + ".",
                    WorkOrderDetailRules.CodeStockShort);
            }

            var now = DateTime.UtcNow;

            var unitOfWork = OgUow.For<WORK_ORDER_MATERIAL_ALLOCATION>(source);
            unitOfWork.PrimaryKey = nameof(WORK_ORDER_MATERIAL_ALLOCATION.ALLOCATION_ID);

            unitOfWork.Add(new WORK_ORDER_MATERIAL_ALLOCATION
            {
                ALLOCATION_ID = Guid.NewGuid().ToString("N"),
                OWNER_TENANT_ID = "APP",
                OWNER_COMPANY_BA_ID = "APP",
                INVENTORY_ITEM_ID = item.INVENTORY_ITEM_ID,
                FIELD_ID = detail.FieldId ?? string.Empty,
                WORK_ORDER_ID = detail.Id,
                PROCESS_INSTANCE_ID = "NO-PROCESS",
                QUANTITY = quantity,
                QUANTITY_UNIT = string.IsNullOrWhiteSpace(unit) ? item.UNIT_OF_MEASURE ?? "EA" : unit.Trim(),
                WAREHOUSE_ID = warehouseId ?? string.Empty,
                LOCATION_ID = string.IsNullOrWhiteSpace(warehouseId) ? "UNSPECIFIED" : warehouseId,
                VALID_FROM_UTC = now,
                VALID_TO_UTC = now.AddDays(30),
                COMMAND_JSON = "{}",
                RECORDED_REVISION = now.ToString("O", CultureInfo.InvariantCulture),
                ACTIVE_IND = "Y",
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unitOfWork.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgWorkOrderDetail>.Failed(
                    OgUow.Message(committed, "The reservation was not saved."), WorkOrderDetailRules.CodeWriteFailed);
            }

            return await ReadAsync(source, detail.Id, asOf, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "reserving material for " + orderId, "the material was not reserved", FailureSeverity.Error);

            return OgResult<OgWorkOrderDetail>.Failed(
                "The material could not be reserved" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? WorkOrderDetailRules.CodeWriteFailed);
        }
    }

    // ------------------------------------------------------------------ the read model

    private async Task<OgResult<OgWorkOrderDetail>> ReadAsync(
        IDataSource source,
        string orderId,
        DateTime asOf,
        CancellationToken cancellationToken)
    {
        var headers = await OgUow.ReadAllAsync<WORK_ORDER>(source).ConfigureAwait(false);
        var components = await OgUow.ReadAllAsync<WORK_ORDER_COMPONENT>(source).ConfigureAwait(false);
        var types = await OgUow.ReadAllAsync<R_WO_TYPE>(source).ConfigureAwait(false);
        var statuses = await OgUow.ReadAllAsync<PROJECT_STATUS>(source).ConfigureAwait(false);
        var labour = await OgUow.ReadAllAsync<WORK_ORDER_LABOR_USAGE>(source).ConfigureAwait(false);
        var usage = await OgUow.ReadAllAsync<WORK_ORDER_MATERIAL_USAGE>(source).ConfigureAwait(false);
        var items = await OgUow.ReadAllAsync<INVENTORY_ITEM>(source).ConfigureAwait(false);
        var conditions = await OgUow.ReadAllAsync<WORK_ORDER_CONDITION>(source).ConfigureAwait(false);
        var permits = await OgUow.ReadAllAsync<WORK_ORDER_EXECUTION_PERMIT>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var header = headers.FirstOrDefault(row =>
            string.Equals(row.WORK_ORDER_ID, orderId, StringComparison.OrdinalIgnoreCase));

        if (header is null)
        {
            return OgResult<OgWorkOrderDetail>.Failed(
                "Work order '" + orderId + "' is not recorded.", WorkOrderDetailRules.CodeUnknownOrder);
        }

        var mine = components
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => string.Equals(row.WORK_ORDER_ID, orderId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var project = mine
            .Select(row => row.PROJECT_ID)
            .FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));

        var latest = project is null
            ? null
            : statuses
                .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                .Where(row => string.Equals(row.PROJECT_ID, project, StringComparison.OrdinalIgnoreCase))
                .Where(row => string.Equals(row.STATUS_TYPE?.Trim(), "WO_STATE", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(row => row.EFFECTIVE_DATE)
                .ThenByDescending(row => row.ROW_CREATED_DATE)
                .FirstOrDefault();

        var kind = types
            .Where(row => string.Equals(row.WORK_ORDER_TYPE?.Trim(), header.WORK_ORDER_TYPE?.Trim(), StringComparison.OrdinalIgnoreCase))
            .Select(row => row.LONG_NAME)
            .FirstOrDefault(name => !string.IsNullOrWhiteSpace(name)) ?? header.WORK_ORDER_TYPE ?? string.Empty;

        var labourLines = labour
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => string.Equals(row.WORK_ORDER_ID, orderId, StringComparison.OrdinalIgnoreCase))
            .OrderBy(row => row.LABOR_FROM_UTC)
            .Select(row => new OgLabourLine(
                row.LABOR_ID,
                row.EMPLOYEE_BA_ID,
                row.DUTY_CODE,
                row.LABOR_FROM_UTC,
                row.LABOR_TO_UTC,
                row.QUANTITY,
                row.QUANTITY_UNIT,
                string.IsNullOrWhiteSpace(row.EXECUTOR_USER_ID) ? row.ROW_CREATED_BY ?? string.Empty : row.EXECUTOR_USER_ID,
                row.UNIT_COST,
                row.CURRENCY_CODE ?? string.Empty))
            .ToList();

        var materialLines = usage
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => string.Equals(row.WORK_ORDER_ID, orderId, StringComparison.OrdinalIgnoreCase))
            .OrderBy(row => row.USED_AT_UTC)
            .Select(row =>
            {
                var item = items.FirstOrDefault(candidate =>
                    string.Equals(candidate.INVENTORY_ITEM_ID, row.INVENTORY_ITEM_ID, StringComparison.OrdinalIgnoreCase));

                return new OgMaterialLine(
                    row.USAGE_ID,
                    row.INVENTORY_ITEM_ID,
                    item?.ITEM_NAME ?? row.INVENTORY_ITEM_ID,
                    row.QUANTITY,
                    item?.UNIT_OF_MEASURE ?? string.Empty,
                    item?.CURRENCY_CODE ?? string.Empty,
                    item?.UNIT_COST,
                    row.USED_AT_UTC,
                    string.IsNullOrWhiteSpace(row.ALLOCATION_ID) ? null : row.ALLOCATION_ID);
            })
            .ToList();

        var permitsOfOrder = permits
            .Where(row => string.Equals(row.WORK_ORDER_ID, orderId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(row => row.VALID_TO_UTC)
            .ToList();

        var permit = permitsOfOrder.Count == 0
            ? WorkOrderDetailRules.PermitOf(null, null, null, asOf.ToUniversalTime())
            : WorkOrderDetailRules.PermitOf(
                permitsOfOrder[0].STATUS, permitsOfOrder[0].VALID_FROM_UTC, permitsOfOrder[0].VALID_TO_UTC,
                asOf.ToUniversalTime());

        var evidence = WorkOrderDetailRules.EvidenceOf(conditions.Where(row =>
            string.Equals(row.WORK_ORDER_ID, orderId, StringComparison.OrdinalIgnoreCase)));

        var state = latest?.STATUS ?? string.Empty;
        var subtype = header.WORK_ORDER_TYPE ?? string.Empty;
        var transitions = WorkOrderDetailRules.Transitions(state, subtype);
        var costs = WorkOrderDetailRules.CostsOf(labourLines, materialLines);

        var blocks = new List<string>();

        if (transitions.Contains(WorkOrderState.InProgress, StringComparer.Ordinal) && !permit.IsValid)
        {
            blocks.Add(permit.Sentence);
        }

        if (transitions.Contains(WorkOrderState.Completed, StringComparer.Ordinal) && !evidence.IsRecorded)
        {
            blocks.Add(evidence.Sentence);
        }

        if (costs.Any(cost => !cost.Complete))
        {
            blocks.Add(costs.Where(cost => !cost.Complete).Sum(cost => cost.UncostedLines)
                .ToString(CultureInfo.InvariantCulture) +
                " line(s) record no cost, so the actual cost is only the costed lines.");
        }

        return OgResult<OgWorkOrderDetail>.Ok(new OgWorkOrderDetail(
            header.WORK_ORDER_ID,
            string.IsNullOrWhiteSpace(header.WORK_ORDER_NUMBER) ? header.WORK_ORDER_ID : header.WORK_ORDER_NUMBER!,
            kind,
            subtype,
            mine.Select(row => row.FIELD_ID).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)),
            mine.Select(row => row.UWI ?? row.FACILITY_ID ?? row.EQUIPMENT_ID)
                .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)),
            mine.Select(row => row.EMPLOYEE_BA_ID ?? row.BUSINESS_ASSOCIATE_ID)
                .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)),
            state,
            latest?.ROW_CREATED_BY,
            header.REQUEST_DATE,
            header.DUE_DATE,
            header.COMPLETE_DATE,
            labourLines,
            materialLines,
            costs,
            permit,
            evidence,
            transitions,
            blocks,
            DateTime.UtcNow));
    }

    private static async Task<string?> ProjectOfAsync(
        IDataSource source,
        string orderId,
        CancellationToken cancellationToken)
    {
        var components = await OgUow.ReadAllAsync<WORK_ORDER_COMPONENT>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return components
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => string.Equals(row.WORK_ORDER_ID, orderId, StringComparison.OrdinalIgnoreCase))
            .Select(row => row.PROJECT_ID)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }
}
