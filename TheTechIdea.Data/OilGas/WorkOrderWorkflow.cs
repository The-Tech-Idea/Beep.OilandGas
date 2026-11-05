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
    string?  RegulatoryRef)
{
    public string InspectionId {get;init;}=string.Empty;
    public string FieldId {get;init;}=string.Empty;
    public string WorkOrderId {get;init;}=string.Empty;
    public string Jurisdiction {get;init;}=string.Empty;
    public string RequirementActor {get;init;}=string.Empty;
    public string Inspector {get;init;}=string.Empty;
    public string SourceRevision {get;init;}=string.Empty;
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? ApplicableStage {get;init;}
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? RequirementRevision {get;init;}
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? PermitPrerequisiteKind {get;init;}
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public TheTechIdea.Data.OilGas.CompanyWorkOrderMaterialAcceptanceFacts? MaterialAcceptance {get;init;}
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public TheTechIdea.Data.OilGas.CompanyWorkOrderPersonnelAcceptanceFacts? PersonnelAcceptance {get;init;}
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public TheTechIdea.Data.OilGas.CompanyWorkOrderDocumentAcceptanceFacts? DocumentAcceptance {get;init;}
}


/// <summary>One graph for advertised and executed transitions; not an authorization policy.</summary>
public static class WorkOrderWorkflow
{
    public static List<string> Available(string state,string subtype)
    {
        if(subtype is not (WorkOrderSubType.Preventive or WorkOrderSubType.Corrective or WorkOrderSubType.Safety or WorkOrderSubType.Environmental or WorkOrderSubType.RegulatoryInspect or WorkOrderSubType.Turnaround))return [];
        return state switch
        {
            WorkOrderState.Draft=>[WorkOrderState.Scoped,WorkOrderState.Planned],
            WorkOrderState.Scoped=>[WorkOrderState.Planned,WorkOrderState.Cancelled],
            WorkOrderState.Planned=>[WorkOrderState.InProgress,WorkOrderState.Cancelled],
            WorkOrderState.InProgress when subtype==WorkOrderSubType.Safety=>[WorkOrderState.UnderReview,WorkOrderState.Cancelled],
            WorkOrderState.InProgress=>[WorkOrderState.Completed,WorkOrderState.Cancelled],
            WorkOrderState.UnderReview when subtype==WorkOrderSubType.Safety=>[WorkOrderState.Completed,WorkOrderState.InProgress],
            _=>[]
        };
    }
    public static bool RecordedConditionsPassed(IReadOnlyList<InspectionCondition>? rows,IEnumerable<string>? requiredTypes)
    {
        if(rows is null || requiredTypes is null || rows.Count==0)return false;
        var types=requiredTypes.ToArray();
        if(types.Length==0 || types.Any(string.IsNullOrWhiteSpace) || types.Distinct(StringComparer.Ordinal).Count()!=types.Length ||
            rows.Any(x=>x is null || x.CondSeq<=0 || string.IsNullOrWhiteSpace(x.CondType) || string.IsNullOrWhiteSpace(x.RequiredText)) ||
            rows.Select(x=>x.CondSeq).Distinct().Count()!=rows.Count)return false;
        return types.All(type=>rows.Any(row=>row.CondType==type)) && rows.Where(row=>types.Contains(row.CondType,StringComparer.Ordinal))
            .All(row=>row.Status=="PASS" && row.InspectDate.HasValue && row.InspectDate.Value!=default);
    }
    public static bool CompletionEvidence(IReadOnlyList<InspectionCondition>? rows) =>
        rows is not null && rows.Any(row=>row is not null && row.CondType==ConditionType.CloseOut) &&
        RecordedConditionsPassed(rows,rows.Where(row=>row is not null).Select(row=>row.CondType).Distinct(StringComparer.Ordinal));
}
