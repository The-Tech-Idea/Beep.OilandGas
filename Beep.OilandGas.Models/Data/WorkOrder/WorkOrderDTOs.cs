using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.WorkOrder;

// ── WO Sub-Types ──────────────────────────────────────────────────────────────
public static class WorkOrderSubType
{
    public const string Preventive         = "WO-PREVENTIVE";
    public const string Corrective         = "WO-CORRECTIVE";
    public const string Safety             = "WO-SAFETY";
    public const string Environmental      = "WO-ENVIRONMENTAL";
    public const string RegulatoryInspect  = "WO-REGULATORY";
    public const string Turnaround         = "WO-TURNAROUND";
}

// ── WO States ─────────────────────────────────────────────────────────────────
public static class WorkOrderState
{
    public const string Draft        = "DRAFT";
    public const string Scoped       = "SCOPED";
    public const string Planned      = "PLANNED";
    public const string InProgress   = "IN_PROGRESS";
    public const string UnderReview  = "UNDER_REVIEW";
    public const string Completed    = "COMPLETED";
    public const string Cancelled    = "CANCELLED";
}

// ── Cost Codes ────────────────────────────────────────────────────────────────
public static class CostCode
{
    public const string Labor         = "01-LABOR";
    public const string Material      = "02-MATERIAL";
    public const string Contractor    = "03-CONTRACTOR";
    public const string Equipment     = "04-EQUIPMENT";
    public const string Environmental = "05-ENVIRONMENTAL";
    public const string Regulatory    = "06-REGULATORY";
    public const string Other         = "07-OTHER";
}

// ── Work Order DTOs ───────────────────────────────────────────────────────────
public class WorkOrderSummary : ModelEntityBase
{
    public string InstanceId   { get; set; } = string.Empty;
    public string InstanceName { get; set; } = string.Empty;
    public string WoSubType    { get; set; } = string.Empty;
    public string State        { get; set; } = WorkOrderState.Draft;
    public string FieldId      { get; set; } = string.Empty;
    public string EquipmentId  { get; set; } = string.Empty;
    public DateTime? PlannedStart  { get; set; }
    public DateTime? PlannedEnd    { get; set; }
    public DateTime? ActualStart   { get; set; }
    public DateTime? ActualEnd     { get; set; }
    public decimal BudgetAmt   { get; set; }
    public decimal ActualAmt   { get; set; }
    public bool   HasAFE       { get; set; }
    public int    StepCount    { get; set; }
    public int    ConditionCount { get; set; }
    public int    PassedConditions { get; set; }
    public string? LeadContractorName { get; set; }
}

public class WorkOrderDetailModel : WorkOrderSummary
{
    public List<WorkOrderStep>           Steps       { get; set; } = new();
    public List<ContractorAssignment>    Contractors { get; set; } = new();
    public List<InspectionCondition>     Checklist   { get; set; } = new();
    public List<CostVarianceLine>        CostLines   { get; set; } = new();
}

public class WorkOrderStep : ModelEntityBase
{
    public string InstanceId { get; set; } = string.Empty;
    public int    StepSeq    { get; set; }
    public string StepName   { get; set; } = string.Empty;
    public string StepType   { get; set; } = string.Empty;
    public string Status     { get; set; } = string.Empty;
    public DateTime? PlannedDate { get; set; }
    public DateTime? ActualDate  { get; set; }
}

public class CreateWorkOrderRequest
{
    public string FieldId      { get; set; } = string.Empty;
    public string WoSubType    { get; set; } = WorkOrderSubType.Preventive;
    public string InstanceName { get; set; } = string.Empty;
    public string EquipmentId  { get; set; } = string.Empty;
    public string Description  { get; set; } = string.Empty;
    public string Jurisdiction { get; set; } = "USA";
    public DateTime? ProposedStart { get; set; }
    public TimeSpan? Duration      { get; set; }
}

public class TransitionWorkOrderRequest
{
    public string ToState { get; set; } = string.Empty;
    public string? Notes  { get; set; }
}

// ── Scheduling DTOs ───────────────────────────────────────────────────────────
public record ScheduleResult(
    bool Success,
    DateTime ConfirmedStart,
    DateTime ConfirmedEnd,
    string? ConflictReason);

public record ScheduleConflict(
    string ConflictingInstanceId,
    DateTime OverlapStart,
    DateTime OverlapEnd);

public record CalendarSlot(
    string InstanceId,
    string InstanceName,
    string EquipmentId,
    DateTime Start,
    DateTime End,
    string State);

public class ScheduleWorkOrderRequest
{
    public string   EquipmentId    { get; set; } = string.Empty;
    public DateTime ProposedStart  { get; set; } = DateTime.UtcNow.AddDays(1);
    public double   DurationHours  { get; set; } = 8;
}

// ── Contractor DTOs ───────────────────────────────────────────────────────────
public record ContractorQualificationResult(
    bool IsQualified,
    string? FailureReason,
    DateTime? LicenseExpiry);

public record ContractorAssignment(
    string StepId,
    string BaId,
    string BaName,
    string RoleCode,
    DateTime AssignedDate);

public class AssignContractorRequest
{
    public string StepId   { get; set; } = string.Empty;
    public string BaId     { get; set; } = string.Empty;
    public string RoleCode { get; set; } = string.Empty;
}

// ── Cost Capture DTOs ─────────────────────────────────────────────────────────
public record CostVarianceLine(
    string CompCode,
    string Description,
    decimal BudgetAmt,
    decimal ActualAmt,
    decimal VariancePct);

public class UpsertAFERequest
{
    public decimal BudgetAmount { get; set; }
}

public class AddCostLineRequest
{
    public string  CompCode    { get; set; } = CostCode.Labor;
    public decimal BudgetAmt   { get; set; }
    public string  Description { get; set; } = string.Empty;
}

// ── Inspection DTOs ───────────────────────────────────────────────────────────
public static class ConditionType
{
    public const string PreStart    = "PRE_START";
    public const string Operational = "OPERATIONAL";
    public const string CloseOut    = "CLOSE_OUT";
    public const string Regulatory  = "REGULATORY";
    public const string SceVerify   = "SCE_VERIFY";
}

public record InspectionCondition(
    int      CondSeq,
    string   CondType,
    string   RequiredText,
    string   Status,
    string?  ResultText,
    DateTime? InspectDate,
    string?  RegulatoryRef);

public class RecordInspectionResultRequest
{
    public string  Result { get; set; } = "PASS";
    public string? Notes  { get; set; }
}
