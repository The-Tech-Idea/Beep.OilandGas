using System.Text.Json;
using Beep.OilandGas.Models.Data.WorkOrder;
namespace TheTechIdea.Data.OilGas;
public sealed record CompanyWorkOrderMaterialAcceptanceFacts(Guid IntentId,Guid PermitIntentId,string AllocationId,string ReservationRevision,string InventoryRevision,string Reviewer,DateTime AcceptedAtUtc,string Revision)
{
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)] public Guid AllocationIntentId {get;init;}
}
public sealed record CompanyWorkOrderMaterialAcceptanceCommand(CompanyWorkOrderMaterialReadinessReview Review,string InspectionId,Guid IntentId,string Reason)
{
    public const int MaximumBodyLength=4*1024*1024;
    [System.Text.Json.Serialization.JsonIgnore] public InspectionCondition OriginalInspection=>Review.PermitReview.CurrentSource.Inspections.Single(row=>row.InspectionId==InspectionId);
    public void Require()
    {
        if(Review is null || IntentId==Guid.Empty || string.IsNullOrWhiteSpace(InspectionId) || string.IsNullOrWhiteSpace(Reason) || Reason.Length>4096 || Reason.Any(char.IsControl))throw new ArgumentException("Retain the original material review, exact inspection, intent and explicit rationale.");
        var permit=Review.PermitReview;Review.Require(permit.Context.Company,permit.Context.ActorUserId,Review.Query);
        if(Review.Blockers.Count!=0 || Review.ComparedAtUtc<permit.Proposal.ValidFromUtc || Review.ComparedAtUtc>=permit.Proposal.ValidToUtc)throw new InvalidOperationException("Current complete material evidence and permit validity are required before acceptance.");
        var rows=permit.CurrentSource.Inspections.Where(row=>row.InspectionId==InspectionId).ToArray();if(rows.Length!=1)throw new InvalidOperationException("Original MATERIAL requirement is not unique.");var inspection=rows[0];
        if(inspection.Status!="UNRECORDED" || inspection.PermitPrerequisiteKind!="MATERIAL" || inspection.RegulatoryRef!=Review.Reservation.AllocationId || inspection.Jurisdiction!=permit.Proposal.Jurisdiction || inspection.RequirementActor==permit.Context.ActorUserId || permit.Proposal.Context.ActorUserId==permit.Context.ActorUserId || inspection.MaterialAcceptance is not null)throw new InvalidOperationException("An original unrecorded matching MATERIAL requirement and independent reviewer are required.");
        if(JsonSerializer.Serialize(this).Length>MaximumBodyLength)throw new InvalidOperationException("Retained material acceptance proof exceeds its bounded native contract.");
    }
    private string ProofRevision(CompanyWorkOrderMaterialReadinessReview current,WORK_ORDER_INSPECTION? own)
    {
        var selected=current.PermitReview.CurrentSource.Inspections.Where(row=>row.InspectionId==InspectionId).ToArray();if(selected.Length!=1)throw new InvalidOperationException("The exact current material inspection is unavailable.");
        if(own is null){if(JsonSerializer.Serialize(selected[0])!=JsonSerializer.Serialize(OriginalInspection))throw new InvalidOperationException("Original pending material requirement changed.");}
        else{RequireRecorded(own);if(JsonSerializer.Serialize(selected[0])!=JsonSerializer.Serialize(WorkOrderInspectionRules.ProjectForRequirement(own,OriginalInspection)))throw new InvalidOperationException("Only the exact original accepted inspection may close the proof.");}
        var inspection=current.PermitReview.CurrentSource;var rows=inspection.Inspections.Select(row=>row.InspectionId==InspectionId?OriginalInspection:row).ToArray();var normalized=CompanyMaintenanceInspectionReview.Revision(inspection.Stage,inspection.InspectionDatabase,inspection.Order,rows);var material=current.CurrentMaterial;
        return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{current.Query,current.Reservation,PermitContext=current.PermitReview.Context,current.PermitReview.Proposal,OriginalPermit=current.PermitReview.OriginalSource.SnapshotRevision,current.PermitReview.RecordedRevision,current.PermitReview.PrerequisiteEvidenceVersion,Inspection=normalized,MaterialContext=material.Source.Context,MaterialQuery=material.Source.Query,Inventory=material.Source.Inventory.Revision,material.Allocations,material.Releases,material.ReservedQuantity,material.AvailableQuantity}));
    }
    public void RequireCurrent(CompanyWorkOrderMaterialReadinessReview current,WORK_ORDER_INSPECTION? own=null)
    {
        Require();current.Require(Review.PermitReview.Context.Company,Review.PermitReview.Context.ActorUserId,Review.Query);
        if(current.Blockers.Count!=0 || current.ComparedAtUtc<Review.ComparedAtUtc || current.ComparedAtUtc<current.PermitReview.Proposal.ValidFromUtc || current.ComparedAtUtc>=current.PermitReview.Proposal.ValidToUtc || ProofRevision(current,own)!=ProofRevision(Review,null))throw new InvalidOperationException("Original permit/reservation/stock/order/inspection/authority evidence changed during acceptance.");
    }
    public void RequireCurrentCore(CompanyMaintenanceInspectionReview current,WORK_ORDER_INSPECTION? own)
    {
        Require();var original=Review.PermitReview.CurrentSource;current.Require(Review.PermitReview.Context.Company,Review.PermitReview.Context.ActorUserId,Review.Query.Permit.Task);var selected=current.Inspections.Where(row=>row.InspectionId==InspectionId).ToArray();if(selected.Length!=1)throw new InvalidOperationException("Exact local core material inspection changed.");
        if(own is null){if(JsonSerializer.Serialize(selected[0])!=JsonSerializer.Serialize(OriginalInspection))throw new InvalidOperationException("Original pending core inspection changed.");}else{RequireRecorded(own);if(JsonSerializer.Serialize(selected[0])!=JsonSerializer.Serialize(WorkOrderInspectionRules.ProjectForRequirement(own,OriginalInspection)))throw new InvalidOperationException("Original accepted core inspection changed.");}
        var rows=current.Inspections.Select(row=>row.InspectionId==InspectionId?OriginalInspection:row).ToArray();if(CompanyMaintenanceInspectionReview.Revision(current.Stage,current.InspectionDatabase,current.Order,rows)!=original.SnapshotRevision)throw new InvalidOperationException("Original complete local core order/inspection source changed during acceptance.");
    }
    public WORK_ORDER_INSPECTION Prepare(WORK_ORDER_INSPECTION pending,DateTime utc)
    {
        Require();if(utc<Review.ComparedAtUtc || utc.Kind!=DateTimeKind.Utc || utc>=Review.PermitReview.Proposal.ValidToUtc || JsonSerializer.Serialize(WorkOrderInspectionRules.ProjectForRequirement(pending,OriginalInspection))!=JsonSerializer.Serialize(OriginalInspection))throw new InvalidOperationException("Exact original pending inspection and current acceptance UTC are required.");
        var row=WorkOrderInspectionRules.PrepareResult(pending,new(){FieldId=pending.FIELD_ID,InspectionId=pending.INSPECTION_ID,ExpectedSourceRevision=OriginalInspection.SourceRevision,Result="PASS",Notes=Reason},Review.PermitReview.Context.ActorUserId,utc);row.MATERIAL_ACCEPTANCE_DATA_JSON=JsonSerializer.Serialize(this);row.MATERIAL_ACCEPTANCE_REVISION=Digest(row);return row;
    }
    public static string Digest(WORK_ORDER_INSPECTION row){var copy=CompanyInventorySourceReview.Copy(row);copy.MATERIAL_ACCEPTANCE_REVISION=null;return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public static CompanyWorkOrderMaterialAcceptanceCommand Read(WORK_ORDER_INSPECTION row)
    {
        if(row.MATERIAL_ACCEPTANCE_DATA_JSON is not {Length:>0} body || body.Length>MaximumBodyLength)throw new InvalidOperationException("Original retained material acceptance is unavailable.");var command=JsonSerializer.Deserialize<CompanyWorkOrderMaterialAcceptanceCommand>(body)??throw new InvalidOperationException("Original acceptance body is unavailable.");command.RequireRecorded(row);return command;
    }
    public void RequireRecorded(WORK_ORDER_INSPECTION row)
    {
        Require();WorkOrderInspectionRules.Require(row.FIELD_ID,row.WORK_ORDER_ID,[row]);var original=OriginalInspection;if(original.RequirementRevision is not null && WorkOrderInspectionRules.RequirementRevision(row)!=original.RequirementRevision)throw new InvalidOperationException("Original immutable material requirement creation audit changed.");var actor=Review.PermitReview.Context.ActorUserId;
        if(row.INSPECTION_ID!=InspectionId || row.FIELD_ID!=original.FieldId || row.WORK_ORDER_ID!=original.WorkOrderId || row.COND_SEQ!=original.CondSeq || row.APPLICABLE_STAGE!=original.ApplicableStage || row.COND_TYPE!=original.CondType || row.REQUIRED_TEXT!=original.RequiredText || row.JURISDICTION!=original.Jurisdiction || row.REGULATORY_REF!=original.RegulatoryRef || row.PERMIT_PREREQUISITE_KIND!="MATERIAL" || row.REQUIREMENT_ACTOR!=original.RequirementActor || row.RESULT_STATUS!="PASS" || row.RESULT_NOTES!=Reason || row.INSPECTOR!=actor || row.INSPECTED_AT_UTC is not {} utc || utc.Kind!=DateTimeKind.Utc || utc<Review.ComparedAtUtc || utc>=Review.PermitReview.Proposal.ValidToUtc || row.ROW_CHANGED_BY!=actor || row.ROW_CHANGED_DATE!=utc || row.MATERIAL_ACCEPTANCE_DATA_JSON!=JsonSerializer.Serialize(this) || row.MATERIAL_ACCEPTANCE_REVISION!=Digest(row))throw new InvalidOperationException("Original accepted material proof, result or complete native audit changed.");
    }
    public CompanyWorkOrderMaterialAcceptanceFacts Facts(WORK_ORDER_INSPECTION row){RequireRecorded(row);return new(IntentId,Review.Query.Permit.IntentId,Review.Reservation.AllocationId,Review.Reservation.Revision,Review.CurrentMaterial.Source.Inventory.Revision,Review.PermitReview.Context.ActorUserId,row.INSPECTED_AT_UTC!.Value,row.MATERIAL_ACCEPTANCE_REVISION!){AllocationIntentId=Review.Query.AllocationIntentId};}
}
public sealed record CompanyWorkOrderMaterialAcceptanceReference(CompanyWorkOrderExecutionPermitReference Permit,string InspectionId,Guid IntentId)
{
    public void Require(){if(Permit is null || !LeaseSummaryValidator.Identifier(InspectionId) || IntentId==Guid.Empty)throw new ArgumentException("Retain the original permit/inspection/material acceptance intent reference.");Permit.Require();}
}
public sealed record CompanyWorkOrderMaterialAcceptanceRecovery(CompanyWorkOrderMaterialAcceptanceCommand Command,WORK_ORDER_INSPECTION Recorded)
{
    public void Require(CompanyScope company,string actor,CompanyWorkOrderMaterialAcceptanceReference reference)
    {
        reference.Require();if(Command is null || Recorded is null || Command.Review.PermitReview.Context.Company!=company || Command.Review.PermitReview.Context.ActorUserId!=actor || Command.Review.Query.Permit!=reference.Permit || Command.InspectionId!=reference.InspectionId || Command.IntentId!=reference.IntentId)throw new UnauthorizedAccessException("Original private material acceptance company/reviewer/reference differs.");Command.RequireRecorded(Recorded);
    }
}
public interface ICompanyWorkOrderMaterialAcceptanceOwner
{
    Task<CompanyWorkOrderMaterialAcceptanceRecovery> RecoverMaterialAcceptanceReferenceAsync(FieldSelectionReview context,CompanyWorkOrderMaterialAcceptanceReference reference,Func<Task> requireAuthority);
    Task<WORK_ORDER_INSPECTION> RecordMaterialAcceptanceAsync(CompanyWorkOrderMaterialAcceptanceCommand command,Func<Task> requireAuthority,Func<WORK_ORDER_INSPECTION?,Task> requireCurrentProof,bool readOnly=false,Func<WORK_ORDER_INSPECTION?,Task>? requireCoreProof=null);
}
