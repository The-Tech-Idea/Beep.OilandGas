using System.Text.Json;
using Beep.OilandGas.Models.Data.WorkOrder;
namespace TheTechIdea.Data.OilGas;

public sealed record CompanyWellReturnFacet(string WellId,string Source,string StatusId,string Facet,string Value,string? Qualifier,string? QualifierValue,string ActiveIndicator,DateTime? EffectiveAtUtc,DateTime? ExpiresAtUtc,DateTime? EndedAtUtc,string? RecordedTimeZone,bool TemporalEvidence,string Revision)
{
    public void Require(string well)
    {
        if(WellId!=well || new[]{WellId,Source,StatusId,Facet,Value}.Any(value=>!LeaseSummaryValidator.Identifier(value) || value.Length>128) || ActiveIndicator is not ("Y" or "N") || !ProcessDefinitionRevision.IsValid(Revision) || new[]{EffectiveAtUtc,ExpiresAtUtc,EndedAtUtc}.Any(value=>value is {} at && at.Kind!=DateTimeKind.Utc))throw new InvalidOperationException("Complete native well facet identity/value/UTC/source evidence is required.");
    }
    public bool Covers(DateTime utc)=>TemporalEvidence && EffectiveAtUtc is {} from && from<=utc && (ExpiresAtUtc is null || ExpiresAtUtc>utc) && (EndedAtUtc is null || EndedAtUtc>utc);
}
public sealed record CompanyWellReturnReference(string Facet,string Value,string ActiveIndicator,DateTime? EffectiveAtUtc,DateTime? ExpiresAtUtc,bool TemporalEvidence,string Revision)
{
    public void Require(){if(new[]{Facet,Value}.Any(value=>!LeaseSummaryValidator.Identifier(value) || value.Length>128) || ActiveIndicator is not ("Y" or "N") || !ProcessDefinitionRevision.IsValid(Revision) || new[]{EffectiveAtUtc,ExpiresAtUtc}.Any(value=>value is {} at && at.Kind!=DateTimeKind.Utc))throw new InvalidOperationException("Recorded native well-status reference and date evidence are required.");}
    public bool Covers(DateTime utc)=>TemporalEvidence && ActiveIndicator=="Y" && (EffectiveAtUtc is null || EffectiveAtUtc<=utc) && (ExpiresAtUtc is null || ExpiresAtUtc>utc);
}
public sealed record CompanyMaintenanceWellEvidence(FieldSelectionReview Context,string WellId,string FieldId,string OperatorBaId,string AccessLevel,string WellRevision,string? DateTimeZoneId,string TimeZoneReferencesRevision,IReadOnlyList<CompanyWellReturnFacet> Facets,IReadOnlyList<CompanyWellReturnReference> References,DateTime ComparedAtUtc,IReadOnlyList<string> Blockers)
{
    public static readonly IReadOnlyList<string> RequiredFacets=Array.AsReadOnly(new[]{"Life Cycle","Role","Condition"});
    public static string[] GetBlockers(FieldSelectionReview context,string well,string field,string operatorBa,string level,string wellRevision,string? zone,string timeZoneRevision,IReadOnlyList<CompanyWellReturnFacet> facets,IReadOnlyList<CompanyWellReturnReference> references,DateTime utc)
    {
        FieldSelectionReviewValidator.RequireReview(context,context.Company,context.ActorUserId);
        if(!LeaseSummaryValidator.Identifier(well) || field!=context.FieldId || operatorBa!=context.Company.BusinessAssociateId || level is not ("READ" or "WRITE" or "DELETE") || !ProcessDefinitionRevision.IsValid(wellRevision) || !ProcessDefinitionRevision.IsValid(timeZoneRevision) || facets is null || facets.Count>256 || references is null || references.Count>256 || utc==default || utc.Kind!=DateTimeKind.Utc)throw new InvalidOperationException("Original company/FIELD/operator/native well access and bounded source population are required.");
        if(zone is not null){try{_ = TimeZoneInfo.FindSystemTimeZoneById(zone);}catch(Exception error) when(error is TimeZoneNotFoundException or InvalidTimeZoneException){throw new InvalidOperationException("Configured evidence time zone is invalid.");}}
        foreach(var row in facets){row.Require(well);if(!RequiredFacets.Contains(row.Facet,StringComparer.Ordinal))throw new InvalidOperationException("Unexpected native well facet population.");}
        foreach(var row in references){row.Require();if(!RequiredFacets.Contains(row.Facet,StringComparer.Ordinal))throw new InvalidOperationException("Unexpected native well-status reference population.");}
        if(facets.Select(row=>(row.Source,row.StatusId)).Distinct().Count()!=facets.Count || references.Select(row=>(row.Facet,row.Value)).Distinct().Count()!=references.Count)throw new InvalidOperationException("Complete unique native compound status/reference keys are required.");
        var result=new List<string>();var current=new Dictionary<string,CompanyWellReturnFacet>(StringComparer.Ordinal);
        foreach(var facet in RequiredFacets)
        {
            var population=facets.Where(row=>row.Facet==facet && row.ActiveIndicator=="Y").ToArray();
            if(population.Any(row=>!row.TemporalEvidence || row.EffectiveAtUtc is null || row.ExpiresAtUtc<=row.EffectiveAtUtc || row.EndedAtUtc<=row.EffectiveAtUtc))result.Add($"{facet} has unresolved or inconsistent native UTC/time-zone boundaries.");
            if(population.Any(row=>row.TemporalEvidence && row.EffectiveAtUtc>utc))result.Add($"{facet} has a future recorded assignment; reconcile it before return.");
            var candidates=population.Where(row=>row.Covers(utc)).ToArray();if(candidates.Length!=1){result.Add($"One unambiguous current {facet} assignment is required.");continue;}var value=candidates[0];current.Add(facet,value);
            if(!string.IsNullOrEmpty(value.Qualifier) || !string.IsNullOrEmpty(value.QualifierValue))result.Add($"Current {facet} qualifiers require their recorded applicability review before return.");
            if(!references.Any(row=>row.Facet==facet && row.Value==value.Value && row.Covers(utc)))result.Add($"Current {facet} value has no effective recorded native reference.");
        }
        if(current.TryGetValue("Life Cycle",out var lifecycle) && lifecycle.Value!="Operating")result.Add("Maintenance return requires recorded Operating life cycle; it cannot create or reopen that phase.");
        if(current.TryGetValue("Role",out var role) && role.Value is not ("Produce" or "Inject" or "Produce/Inject" or "Service" or "Research"))result.Add("A recorded supported operating Role is required.");
        if(current.TryGetValue("Condition",out var condition) && (condition.Value is not ("Shut In" or "Idle") || !string.IsNullOrEmpty(condition.Qualifier) || !string.IsNullOrEmpty(condition.QualifierValue)))result.Add("Only an unqualified recorded Shut In or Idle condition can use this maintenance return branch.");
        if(!references.Any(row=>row.Facet=="Condition" && row.Value=="Active" && row.Covers(utc)))result.Add("The Active target condition has no effective recorded native reference.");
        if(level=="READ")result.Add("Current native well WRITE authority is required for return; this is a read-only evidence view.");return result.ToArray();
    }
    public void Require(){if(Context is null || Blockers is null || !GetBlockers(Context,WellId,FieldId,OperatorBaId,AccessLevel,WellRevision,DateTimeZoneId,TimeZoneReferencesRevision,Facets,References,ComparedAtUtc).SequenceEqual(Blockers,StringComparer.Ordinal))throw new InvalidOperationException("Current canonical well-return evidence differs.");}
    public string SourceRevision(){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Context,WellId,FieldId,OperatorBaId,AccessLevel,WellRevision,DateTimeZoneId,TimeZoneReferencesRevision,Facets=Facets.OrderBy(row=>row.Source,StringComparer.Ordinal).ThenBy(row=>row.StatusId,StringComparer.Ordinal),References=References.OrderBy(row=>row.Facet,StringComparer.Ordinal).ThenBy(row=>row.Value,StringComparer.Ordinal),Blockers}));}
}
public interface ICompanyMaintenanceWellEvidenceOwner
{
    Task<CompanyMaintenanceWellEvidence> ReadMaintenanceWellEvidenceAsync(FieldSelectionReview context,string wellId,Func<Task> authority);
}
/// <summary>Required existing app capabilities for the explicit original targets; installation parents do not add mutation scope.</summary>
public static class CompanyMaintenanceAssetReturnCapabilities
{
    public static readonly IReadOnlyList<string> Common=Array.AsReadOnly(new[]{"Maintenance.ReturnToService","Workflow.Execute"});
    public static string[] ForOrder(WorkOrderDetailModel order)
    {
        ArgumentNullException.ThrowIfNull(order);RecordedWorkOrderComponentChecks.Require(order);
        if(order.Components.Count is 0 or >128)throw new InvalidOperationException("A bounded original affected component population is required for asset-return capabilities.");
        var result=new List<string>(Common);
        if(order.Components.Any(row=>!string.IsNullOrWhiteSpace(row.WellId)))result.Add("WellManagement.UpdateWellStatus");
        if(order.Components.Any(row=>!string.IsNullOrWhiteSpace(row.FacilityId) || !string.IsNullOrWhiteSpace(row.FacilityType)))result.Add("Facilities.Edit");
        if(order.Components.Any(row=>!string.IsNullOrWhiteSpace(row.EquipmentId)))result.Add("Facilities.ManageEquipment");
        return result.ToArray();
    }
}
public sealed record CompanyMaintenanceAssetEvidence(FieldSelectionReview Context,CompanyWorkOrderSubmissionFacts Report,CompanyMaintenanceClosureFacts Closure,WorkOrderDetailModel Order,IReadOnlyList<CompanyMaintenanceWellEvidence> Wells,DateTime ComparedAtUtc,IReadOnlyList<string> Blockers)
{
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public CompanyMaintenanceQualifiedAssets? QualifiedAssets{get;init;}
    public const string PendingQualifiedCommand="The complete controlled mixed-asset return is not recorded.";
    public const string PendingCommand="Asset return is unconfirmed. This view provides evidence only; resume service only after the authorized asset return is recorded.";
    public static string[] GetBlockers(FieldSelectionReview context,CompanyWorkOrderSubmissionFacts report,CompanyMaintenanceClosureFacts closure,WorkOrderDetailModel order,IReadOnlyList<CompanyMaintenanceWellEvidence> wells,DateTime utc,CompanyMaintenanceQualifiedAssets? qualifiedAssets=null)
    {
        FieldSelectionReviewValidator.RequireReview(context,context.Company,context.ActorUserId);report.Require();closure.Require();RecordedWorkOrderComponentChecks.Require(order);
        if(report.Company!=context.Company || closure.Company!=context.Company || report.Database!=context.Database || closure.CoreDatabase!=context.Database || report.Query!=closure.Query || closure.ReportRevision!=report.Revision || order.Components.Count>128 || report.Query.Task.FieldId!=context.FieldId || order.FieldId!=context.FieldId || order.InstanceId!=report.Query.Task.WorkOrderId || order.Source!="WORK_ORDER" || wells is null || wells.Count>128 || utc.Kind!=DateTimeKind.Utc || utc<closure.RecordedAtUtc)throw new InvalidOperationException("Original recorded closure/report/company/CORE/order and complete affected asset source are required.");
        var ids=order.Components.Where(row=>!string.IsNullOrWhiteSpace(row.WellId)).Select(row=>row.WellId!).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        if(!ids.SequenceEqual(wells.Select(row=>row.WellId).Order(StringComparer.Ordinal),StringComparer.Ordinal))throw new InvalidOperationException("Every original component well requires its complete native evidence; assets cannot be omitted.");
        var result=new List<string>();if(CompanyWorkOrderExecutionRequirements.OrderRevision(order)!=report.OriginalOrderRevision)result.Add("The original closed work-order scope or component population changed.");
        if(qualifiedAssets is not null){qualifiedAssets.Require(order);result.AddRange(CompanyMaintenanceQualifiedWorkScope.GetReturnBlockers(qualifiedAssets,report,order));if(qualifiedAssets.Context!=context || qualifiedAssets.LatestComparisonAtUtc()>utc)throw new InvalidOperationException("Qualified asset identities belong to another original company/account/FIELD/source comparison.");if(qualifiedAssets.ReturnPolicy is {} policy)result.AddRange(policy.Blockers);else{if(qualifiedAssets.Operating is {} operating)result.AddRange(operating.Blockers);if(qualifiedAssets.Licensing is {} licensing)result.AddRange(licensing.Blockers);}}
        foreach(var row in wells){row.Require();if(row.Context!=context || row.FieldId!=context.FieldId || row.ComparedAtUtc>utc)throw new InvalidOperationException("Affected well evidence belongs to another original company/account/FIELD/source comparison.");result.AddRange(row.Blockers.Select(value=>$"Well {row.WellId}: {value}"));}
        foreach(var component in order.Components)
        {
            if(!ProcessDefinitionRevision.IsValid(component.NativeRevision))result.Add($"Component {component.ComponentId} has no complete recorded native relationship revision; reconcile its original scope.");
            if(!string.IsNullOrWhiteSpace(component.EquipmentId))result.Add(qualifiedAssets?.ReturnPolicy is {Blockers.Count:0}?PendingQualifiedCommand:qualifiedAssets is null?$"Component {component.ComponentId} equipment {component.EquipmentId} has no confirmed operating-state evidence and authorization; reconcile this affected asset before return.":$"Component {component.ComponentId} equipment {component.EquipmentId} has reviewed current ownership/installation; native status, licensing and explicit return-policy evidence remain required.");
            if(!string.IsNullOrWhiteSpace(component.FacilityId) || !string.IsNullOrWhiteSpace(component.FacilityType))result.Add(qualifiedAssets?.ReturnPolicy is {Blockers.Count:0}?PendingQualifiedCommand:qualifiedAssets is null?$"Component {component.ComponentId} facility {component.FacilityId} / {component.FacilityType} requires its complete qualified operating-state evidence and authorization.":$"Component {component.ComponentId} facility {component.FacilityId} / {component.FacilityType} has reviewed current identity/FIELD/operator authority; native status, licensing and explicit return-policy evidence remain required.");
            if(component.UnresolvedReferenceKinds is {Count:>0})result.Add($"Component {component.ComponentId} has additional recorded relationships that need return-scope reconciliation: {string.Join(", ",component.UnresolvedReferenceKinds)}.");
            if(string.IsNullOrWhiteSpace(component.WellId) && string.IsNullOrWhiteSpace(component.EquipmentId) && string.IsNullOrWhiteSpace(component.FacilityId) && !(component.InformationItemSubtype=="DOCUMENT" && !string.IsNullOrWhiteSpace(component.InformationItemId)))result.Add($"Component {component.ComponentId} has no confirmed asset or source-document purpose; reconcile its actual affected assets.");
        }
        if(result.Contains(PendingQualifiedCommand,StringComparer.Ordinal)){result.RemoveAll(value=>value==PendingQualifiedCommand);result.Add(PendingQualifiedCommand);}
        result.Add(PendingCommand);return result.ToArray();
    }
    public void Require(){if(Context is null || Blockers is null || !GetBlockers(Context,Report,Closure,Order,Wells,ComparedAtUtc,QualifiedAssets).SequenceEqual(Blockers,StringComparer.Ordinal))throw new InvalidOperationException("Original complete maintenance asset evidence differs.");}
    public string SourceRevision(){Require();var original=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Context,Report,Closure,Order,Wells=Wells.OrderBy(row=>row.WellId,StringComparer.Ordinal).Select(row=>row.SourceRevision()),Blockers}));return QualifiedAssets is null?original:ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Original=original,Qualified=QualifiedAssets.SourceRevision()}));}
}
