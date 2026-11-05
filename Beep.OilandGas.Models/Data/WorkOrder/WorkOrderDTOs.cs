using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.WorkOrder;

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
    [System.Text.Json.Serialization.JsonRequired]
    public string InstanceId   { get; set; } = string.Empty;
    // Distinct from inherited PPDM SOURCE audit provenance under web JSON naming.
    [System.Text.Json.Serialization.JsonPropertyName("recordSource")]
    public string Source { get; set; } = string.Empty;
    public string? EvidenceUnavailableReason { get; set; }
    public DateTime? RequestDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Instructions { get; set; }
    public string InstanceName { get; set; } = string.Empty;
    public string WoSubType    { get; set; } = string.Empty;
    [System.Text.Json.Serialization.JsonRequired]
    public string State        { get; set; } = string.Empty;
    [System.Text.Json.Serialization.JsonRequired]
    public string FieldId      { get; set; } = string.Empty;
    public string EquipmentId  { get; set; } = string.Empty;
    public List<WorkOrderComponentReference> Components { get; set; } = [];
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

public sealed record WorkOrderComponentReference(string WorkOrderId,string ComponentId,string FieldId,string WellId,string EquipmentId)
{
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] public string? FacilityId{get;init;}
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] public string? FacilityType{get;init;}
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] public string? InformationItemId{get;init;}
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] public string? InformationItemSubtype{get;init;}
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] public string? ComponentType{get;init;}
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] public string? NativeRevision{get;init;}
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] public IReadOnlyList<string>? UnresolvedReferenceKinds{get;init;}
}

public class CreateWorkOrderRequest
{
    public string WorkOrderNumber { get; set; } = string.Empty;
    public string FieldId      { get; set; } = string.Empty;
    public string WoSubType    { get; set; } = WorkOrderSubType.Preventive;
    public string InstanceName { get; set; } = string.Empty;
    public string EquipmentId  { get; set; } = string.Empty;
    public string WellId { get; set; } = string.Empty;
    public string Description  { get; set; } = string.Empty;
    public string Jurisdiction { get; set; } = string.Empty;
    [System.Text.Json.Serialization.JsonIgnore]
    public string RecordedInstructions => $"Title: {InstanceName}\nReported jurisdiction: {Jurisdiction}\n{Description}";
    public DateTime? ProposedStart { get; set; }
    public TimeSpan? Duration      { get; set; }
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

// ── Inspection DTOs ───────────────────────────────────────────────────────────
public class RecordInspectionResultRequest
{
    [System.Text.Json.Serialization.JsonRequired]
    public string FieldId {get;set;}=string.Empty;
    [System.Text.Json.Serialization.JsonRequired]
    public string InspectionId {get;set;}=string.Empty;
    [System.Text.Json.Serialization.JsonRequired]
    public string ExpectedSourceRevision {get;set;}=string.Empty;
    public string Result { get; set; } = string.Empty;
    public string? Notes  { get; set; }
}

public static class RecordedWorkOrderComponentChecks
{
    public static void Require(WorkOrderSummary row)
    {
        if(row.Components is null || row.Components.Any(c=>c is null || c.WorkOrderId!=row.InstanceId || c.FieldId!=row.FieldId || string.IsNullOrWhiteSpace(c.ComponentId)) ||
           row.Components.Select(c=>c.ComponentId).Distinct(StringComparer.Ordinal).Count()!=row.Components.Count)
            throw new InvalidOperationException("Recorded work-order components do not match their original FIELD and order.");
    }
    public static void RequireCreated(CreateWorkOrderRequest request,WorkOrderSummary row)
    {
        Require(row);
        if(row.Components.Count!=1 || row.Components[0].WellId!=request.WellId || row.Components[0].EquipmentId!=request.EquipmentId)
            throw new InvalidOperationException("The saved component does not confirm the submitted well and equipment references.");
    }
    public static void RequireWellLink(WorkOrderSummary row,string field,string id,string well)
    {
        Require(row);
        if(string.IsNullOrWhiteSpace(well) || row.InstanceId!=id || row.FieldId!=field || row.Source!="WORK_ORDER" || row.WoSubType!=WorkOrderSubType.Corrective ||
           !row.Components.Any(c=>c.WellId==well))
            throw new InvalidOperationException("The asserted order has no recorded corrective WORK_ORDER_COMPONENT relationship to this FIELD and well.");
    }
}
