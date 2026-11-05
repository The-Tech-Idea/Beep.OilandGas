using Beep.OilandGas.Models.Data.WorkOrder;
namespace TheTechIdea.Data.OilGas;

/// <summary>Public business facts for the retained transition; excludes private rationale, intent UUID and command/snapshot bodies.</summary>
public sealed record CompanyMaintenanceWellReturnFact(string WellId,string Source,string PreviousStatusId,string PreviousCondition,string RecordedStatusId,decimal ObservedCapabilityPercent)
{
    public void Require()
    {
        if(new[]{WellId,Source,PreviousStatusId,RecordedStatusId}.Any(value=>!LeaseSummaryValidator.Identifier(value) || value.Length>128) || PreviousCondition is not ("Shut In" or "Idle") || ObservedCapabilityPercent is <0 or >100 || PreviousStatusId==RecordedStatusId)
            throw new InvalidOperationException("Complete original native well transition keys, prior condition and recorded observation are required.");
    }
}
public sealed record CompanyMaintenanceQualifiedReturnFact(string AssetType,string AssetId,string? FacilityType,string StatusType,
    string PreviousStatusId,string PreviousValue,string PreviousSource,string RecordedStatusId,string RecordedValue,string RecordedSource,decimal ObservedCapabilityPercent)
{
    public void Require()
    {
        new CompanyMaintenanceQualifiedReturnObservation(AssetType,AssetId,FacilityType,ObservedCapabilityPercent).Require();
        if(new[]{StatusType,PreviousStatusId,PreviousValue,PreviousSource,RecordedStatusId,RecordedValue,RecordedSource}.Any(value=>!LeaseSummaryValidator.Identifier(value) || value.Length>128) || PreviousStatusId==RecordedStatusId)
            throw new InvalidOperationException("Complete qualified native transition keys, values, sources and observations are required.");
    }
}
public sealed record CompanyMaintenanceAssetReturnFacts(CompanyScope Company,AssetDatabaseScope CoreDatabase,CompanyWorkOrderCompletionQuery Query,string ReturnId,string ActorUserId,string Role,DateTime ActivatedAtUtc,DateTime RecordedAtUtc,string ReportRevision,string ClosureRevision,string PhysicalSubjectRevision,IReadOnlyList<CompanyMaintenanceWellReturnFact> Wells,string Revision)
{
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<CompanyMaintenanceQualifiedReturnFact>? QualifiedAssets { get; init; }
    public void Require()
    {
        PurchaseOrderOwnership.RequireScope(Company);Query.Require();
        if(CoreDatabase is null || !LeaseSummaryValidator.Identifier(CoreDatabase.ConnectionName) || string.IsNullOrWhiteSpace(CoreDatabase.Fingerprint) || ReturnId!=CompanyMaintenanceAssetReturnReference.SourceId(Company,Query) || !LeaseSummaryValidator.Identifier(ActorUserId) || !LeaseSummaryValidator.Identifier(Role) || ActivatedAtUtc.Kind!=DateTimeKind.Utc || RecordedAtUtc.Kind!=DateTimeKind.Utc || RecordedAtUtc<ActivatedAtUtc || new[]{ReportRevision,ClosureRevision,PhysicalSubjectRevision,Revision}.Any(value=>!ProcessDefinitionRevision.IsValid(value)) || Wells is null || Wells.Count>128 || Wells.Count==0 && QualifiedAssets is not {Count:>0} || !Wells.Select(well=>well.WellId).SequenceEqual(Wells.Select(well=>well.WellId).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal),StringComparer.Ordinal))
            throw new InvalidOperationException("Original recorded asset-return company/CORE/report/closure/UTC and complete native well facts are required.");
        foreach(var well in Wells)well.Require();
        if(QualifiedAssets is {} qualified)
        {
            if(qualified.Count is 0 or >4096 || qualified.Select(row=>(row.AssetType,row.AssetId,row.FacilityType,row.StatusType)).Distinct().Count()!=qualified.Count)
                throw new InvalidOperationException("Complete unique qualified transition facts are required.");
            foreach(var row in qualified)row.Require();
        }
    }
    public static CompanyMaintenanceAssetReturnFacts From(CompanyMaintenanceAssetReturnRecovery saved)
    {
        var command=saved.Command;var context=command.Review.Assets.Context;saved.Require(context.Company,context.ActorUserId,command.Reference());
        var wells=command.Wells.OrderBy(well=>well.WellId,StringComparer.Ordinal).Select(well=>
        {
            var original=command.Review.Assets.Wells.Single(row=>row.WellId==well.WellId);var prior=original.Facets.Single(row=>row.Facet=="Condition" && row.ActiveIndicator=="Y" && row.Covers(original.ComparedAtUtc));
            return new CompanyMaintenanceWellReturnFact(well.WellId,well.RecordingSource,prior.StatusId,prior.Value,command.StatusId(well.WellId),well.ObservedCapabilityPercent!.Value);
        }).ToArray();
        var result=new CompanyMaintenanceAssetReturnFacts(context.Company,context.Database,command.Reference().Query,saved.Receipt.ReturnId,context.ActorUserId,command.Review.Physical.Assignment.Stage.AssignedRole,saved.Receipt.ActivatedAtUtc,saved.Receipt.RecordedAtUtc,command.Review.Assets.Report.Revision,command.Review.Assets.Closure.Revision,command.Review.Assets.Closure.PhysicalSubjectRevision,wells,saved.Receipt.Revision);
        if(command.Review.Assets.QualifiedAssets is {} qualified)
        {
            var changes=CompanyMaintenanceQualifiedAssetReturn.Resolve(qualified,command.Review.Assets.Order,command.QualifiedAssets!,command.Review.Physical.Assignment.Stage.AssignedRole,command.ActivatedAtUtc);
            result=result with{QualifiedAssets=changes.Select(change=>new CompanyMaintenanceQualifiedReturnFact(change.AssetType,change.AssetId,change.FacilityType,change.StatusType,change.OriginalStatusId,
                qualified.Operating!.Statuses.Single(row=>row.AssetType==change.AssetType && row.AssetId==change.AssetId && row.FacilityType==change.FacilityType && row.StatusId==change.OriginalStatusId).Value,
                change.OriginalSource,change.StatusId(command.ReturnId()),change.TargetValue,change.NativeSource,change.ObservedCapabilityPercent)).ToArray()};
        }
        result.Require();return result;
    }
}
public sealed record CompanyMaintenanceAssetReturnStatus(FieldSelectionReview Context,CompanyWorkOrderCompletionQuery Query,CompanyMaintenanceAssetReturnFacts? Recorded,IReadOnlyList<CompanyMaintenanceWellEvidence> CurrentWells)
{
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<WorkOrderComponentReference>? OriginalComponents{get;init;}
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public CompanyMaintenanceQualifiedAssets? CurrentQualifiedAssets{get;init;}
    public void Require()
    {
        FieldSelectionReviewValidator.RequireReview(Context,Context.Company,Context.ActorUserId);Query.Require();Recorded?.Require();
        if(Context.FieldId!=Query.Task.FieldId || CurrentWells is null || CurrentWells.Count>128 || CurrentWells.Count==0 && CurrentQualifiedAssets is null || CurrentWells.Any(well=>well.Context!=Context || well.FieldId!=Context.FieldId) || !CurrentWells.Select(well=>well.WellId).SequenceEqual(CurrentWells.Select(well=>well.WellId).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal),StringComparer.Ordinal) || Recorded is {} facts && (facts.Company!=Context.Company || facts.CoreDatabase!=Context.Database || facts.Query!=Query || !facts.Wells.Select(well=>well.WellId).SequenceEqual(CurrentWells.Select(well=>well.WellId),StringComparer.Ordinal)))throw new InvalidOperationException("Original public return company/FIELD/CORE/report and complete current asset source differs.");
        foreach(var well in CurrentWells)well.Require();
        if(CurrentQualifiedAssets is {} qualified)
        {
            if(OriginalComponents is not {} components || qualified.Context!=Context || qualified.Facilities.Count+qualified.Equipment.Count==0)
                throw new InvalidOperationException("Complete original affected order and current qualified identity/operating READ evidence are required.");
            qualified.RequireCurrentStatus(Query.Task,components);
            var wellIds=components.Where(row=>!string.IsNullOrWhiteSpace(row.WellId)).Select(row=>row.WellId).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal);
            if(!wellIds.SequenceEqual(CurrentWells.Select(row=>row.WellId),StringComparer.Ordinal))throw new InvalidOperationException("Current public evidence must cover every original affected well together with the qualified targets.");
            var targets=qualified.Equipment.Select(row=>(Type:"EQUIPMENT",Id:row.EquipmentId,Qualifier:(string?)null))
                .Concat(qualified.Facilities.Where(row=>components.Any(component=>component.FacilityId==row.FacilityId && component.FacilityType==row.FacilityType)).Select(row=>(Type:"FACILITY",Id:row.FacilityId,Qualifier:(string?)row.FacilityType))).ToHashSet();
            if(targets.Count==0 || Recorded is {} recorded && (recorded.QualifiedAssets is null || !targets.SetEquals(recorded.QualifiedAssets.Select(row=>(row.AssetType,row.AssetId,row.FacilityType)))))
                throw new InvalidOperationException("Recorded qualified transitions must cover exactly the explicit original targets; installation parents are read only.");
        }
        else if(OriginalComponents is not null || Recorded?.QualifiedAssets is not null)throw new InvalidOperationException("Every recorded qualified target requires complete current qualified READ evidence.");
    }
}
