using System.Text.Json;
using Beep.OilandGas.Models.Data.WorkOrder;
namespace TheTechIdea.Data.OilGas;

public sealed record CompanyWorkOrderDocumentAcceptanceFacts(Guid IntentId,Guid PermitIntentId,string Kind,string ComponentId,string DocumentId,string Version,string ContentRevision,string SourceProof,string Reviewer,DateTime AcceptedAtUtc,string Revision)
{
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)] public int ContributorEvidenceVersion {get;init;}
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] public string? ContributorProof {get;init;}
}
public sealed record CompanyWorkOrderDocumentAcceptanceCommand(CompanyWorkOrderPermitDocumentReview Review,string InspectionId,Guid IntentId,string Reason,bool ContentReviewed,bool RequirementVerified)
{
    public const int MaximumBodyLength=8*1024*1024;
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] public CompanyWorkOrderDocumentContributorReview? ContributorReview {get;init;}
    private sealed record StoredProof(CompanyWorkOrderDocumentAcceptanceCommand Command,string PendingRowJson);
    [System.Text.Json.Serialization.JsonIgnore] public InspectionCondition OriginalInspection=>Review.PermitReview.CurrentSource.Inspections.Single(row=>row.InspectionId==InspectionId);
    public void Require()
    {
        if(Review is null || !LeaseSummaryValidator.Identifier(InspectionId) || IntentId==Guid.Empty || string.IsNullOrWhiteSpace(Reason) || Reason.Length>4096 || Reason.Any(char.IsControl) || !ContentReviewed || !RequirementVerified)throw new ArgumentException("Retain the exact document/requirement, intent and rationale; explicitly confirm content review and verification of the original requirement.");
        Review.Require();var permit=Review.PermitReview;var actor=permit.Context.ActorUserId;Review.Require(permit.Context.Company,actor,Review.Source.Query);
        if(Review.Blockers.Count!=1 || Review.Blockers[0]!=CompanyWorkOrderPermitDocumentReview.PendingAcceptanceBlocker || Review.Source.ComparedAtUtc<permit.Proposal.ValidFromUtc || Review.Source.ComparedAtUtc>=permit.Proposal.ValidToUtc)throw new InvalidOperationException("Complete unrestricted current document evidence within the permit window is required.");
        if(actor==permit.Proposal.Context.ActorUserId || actor==Review.Source.Document!.RecordedBy)throw new UnauthorizedAccessException("Document reviewer must differ from the original permit issuer and document recording account.");
        var rows=permit.CurrentSource.Inspections.Where(row=>row.InspectionId==InspectionId).ToArray();
        if(rows.Length!=1 || rows[0].Status!="UNRECORDED" || rows[0].PermitPrerequisiteKind!=Review.Source.Query.Kind || rows[0].RegulatoryRef!=Review.Source.ExpectedReference || rows[0].Jurisdiction!=permit.Proposal.Jurisdiction || rows[0].RequirementActor==actor || rows[0].DocumentAcceptance is not null)throw new InvalidOperationException("Select the exact unrecorded isolation/risk requirement, original reference/jurisdiction and independent requirement author.");
        if(ContributorReview is {} contributors)
        {
            contributors.Require();if(contributors.Blockers.Count!=1 || contributors.Blockers[0]!=CompanyWorkOrderPermitDocumentReview.PendingAcceptanceBlocker || contributors.Document.PermitReview.Context!=permit.Context || contributors.Document.Source.Query!=Review.Source.Query || contributors.Document.PermitReview.RecordedRevision!=permit.RecordedRevision || contributors.Document.PermitReview.CurrentSource.SnapshotRevision!=permit.CurrentSource.SnapshotRevision || CompanyWorkOrderPermitDocumentFacts.CurrentProof(contributors.Document.Source)!=CompanyWorkOrderPermitDocumentFacts.CurrentProof(Review.Source) || contributors.Contributors.ComparedAtUtc<Review.Source.ComparedAtUtc)throw new InvalidOperationException("Exact original current contributor/reviewer identity and complete document proof are required.");
        }
        if(JsonSerializer.Serialize(this).Length>MaximumBodyLength)throw new InvalidOperationException("Retained document acceptance exceeds its bounded native capacity.");
    }
    public void RequireCurrentAcceptance(){Require();if(ContributorReview is null)throw new InvalidOperationException("Current contributor/reviewer identity review is required for new document acceptance; historical proof is recovery-only.");}
    public static string ContributorProof(CompanyWorkOrderDocumentContributorReview review)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Contributors=CompanyWorkOrderDocumentContributors.EvidenceProof(review.Contributors),review.Identities.Context.Company,review.Identities.Context.Database,Versions=review.Identities.Versions.OrderBy(row=>row.RecordId,StringComparer.Ordinal).ToArray(),review.ReviewerPerson}));
    public void RequireCurrentContributors(CompanyWorkOrderDocumentContributorReview current,WORK_ORDER_INSPECTION? own=null)
    {
        RequireCurrentAcceptance();current.Require();RequireCurrent(current.Document,own);if(current.Blockers.Count!=1 || current.Blockers[0]!=CompanyWorkOrderPermitDocumentReview.PendingAcceptanceBlocker || current.ComparedAtUtc<ContributorReview!.ComparedAtUtc || ContributorProof(current)!=ContributorProof(ContributorReview))throw new InvalidOperationException("Complete original document/contributor/reviewer mapping/person proof changed.");
    }
    public void RequireCurrentCore(CompanyMaintenanceInspectionReview current,WORK_ORDER_INSPECTION? own=null)
    {
        Require();var permit=Review.PermitReview;current.Require(permit.Context.Company,permit.Context.ActorUserId,Review.Source.Query.Permit.Task);
        var selected=current.Inspections.Where(row=>row.InspectionId==InspectionId).ToArray();if(selected.Length!=1)throw new InvalidOperationException("Exact current document requirement is unavailable.");
        if(own is null){if(JsonSerializer.Serialize(selected[0])!=JsonSerializer.Serialize(OriginalInspection))throw new InvalidOperationException("Original pending document requirement changed.");}
        else{RequireRecorded(own);if(JsonSerializer.Serialize(selected[0])!=JsonSerializer.Serialize(WorkOrderInspectionRules.ProjectForRequirement(own,OriginalInspection)))throw new InvalidOperationException("Only the original accepted document row may close proof.");}
        var normalized=CompanyMaintenanceInspectionReview.Revision(current.Stage,current.InspectionDatabase,current.Order,current.Inspections.Select(row=>row.InspectionId==InspectionId?OriginalInspection:row).ToArray());
        if(normalized!=permit.CurrentSource.SnapshotRevision)throw new InvalidOperationException("Complete original local order/inspection/stage proof changed.");
    }
    public void RequireCurrentPermit(CompanyWorkOrderExecutionPermitApprovalReview current,WORK_ORDER_INSPECTION? own=null)
    {
        Require();var permit=Review.PermitReview;current.Require(permit.Context.Company,permit.Context.ActorUserId,Review.Source.Query.Permit);RequireCurrentCore(current.CurrentSource,own);if(current.Context!=permit.Context || current.RecordedRevision!=permit.RecordedRevision || current.OriginalSource.SnapshotRevision!=permit.OriginalSource.SnapshotRevision || current.PrerequisiteEvidenceVersion!=permit.PrerequisiteEvidenceVersion || JsonSerializer.Serialize(current.Proposal)!=JsonSerializer.Serialize(permit.Proposal))throw new InvalidOperationException("Complete original permit/lifecycle/context/native source proposal changed.");
    }
    public void RequireCurrent(CompanyWorkOrderPermitDocumentReview current,WORK_ORDER_INSPECTION? own=null)
    {
        Require();var permit=Review.PermitReview;current.Require(permit.Context.Company,permit.Context.ActorUserId,Review.Source.Query);RequireCurrentCore(current.PermitReview.CurrentSource,own);
        if(current.Blockers.Count!=1 || current.Blockers[0]!=CompanyWorkOrderPermitDocumentReview.PendingAcceptanceBlocker || current.Source.ComparedAtUtc<Review.Source.ComparedAtUtc || current.Source.ComparedAtUtc<permit.Proposal.ValidFromUtc || current.Source.ComparedAtUtc>=permit.Proposal.ValidToUtc || CompanyWorkOrderPermitDocumentFacts.CurrentProof(current.Source)!=CompanyWorkOrderPermitDocumentFacts.CurrentProof(Review.Source) || JsonSerializer.Serialize(current.PermitReview.Proposal)!=JsonSerializer.Serialize(permit.Proposal) || current.PermitReview.RecordedRevision!=permit.RecordedRevision || current.PermitReview.OriginalSource.SnapshotRevision!=permit.OriginalSource.SnapshotRevision || current.PermitReview.PrerequisiteEvidenceVersion!=permit.PrerequisiteEvidenceVersion)throw new InvalidOperationException("Complete original permit/document membership/version/content/window proof changed.");
    }
    public WORK_ORDER_INSPECTION Prepare(WORK_ORDER_INSPECTION pending,DateTime utc)
    {
        Require();if(utc.Kind!=DateTimeKind.Utc || utc<Review.Source.ComparedAtUtc || ContributorReview is not null && utc<ContributorReview.ComparedAtUtc || utc>=Review.PermitReview.Proposal.ValidToUtc || JsonSerializer.Serialize(WorkOrderInspectionRules.ProjectForRequirement(pending,OriginalInspection))!=JsonSerializer.Serialize(OriginalInspection))throw new InvalidOperationException("Original pending row and explicit current acceptance UTC are required.");
        var row=WorkOrderInspectionRules.PrepareResult(pending,new(){FieldId=pending.FIELD_ID,InspectionId=InspectionId,ExpectedSourceRevision=OriginalInspection.SourceRevision,Result="PASS",Notes=Reason},Review.PermitReview.Context.ActorUserId,utc);
        row.DOCUMENT_ACCEPTANCE_DATA_JSON=JsonSerializer.Serialize(new StoredProof(this,JsonSerializer.Serialize(pending)));if(row.DOCUMENT_ACCEPTANCE_DATA_JSON.Length>MaximumBodyLength)throw new InvalidOperationException("Complete original document proof exceeds native capacity.");row.DOCUMENT_ACCEPTANCE_REVISION=Digest(row);return row;
    }
    public static string Digest(WORK_ORDER_INSPECTION row){var copy=CompanyInventorySourceReview.Copy(row);copy.DOCUMENT_ACCEPTANCE_REVISION=null;return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public static CompanyWorkOrderDocumentAcceptanceCommand Read(WORK_ORDER_INSPECTION row)
    {
        if(row.DOCUMENT_ACCEPTANCE_DATA_JSON is not {Length:>0} body || body.Length>MaximumBodyLength)throw new InvalidOperationException("Original retained document acceptance is unavailable.");
        CompanyWorkOrderDocumentAcceptanceCommand command;try{command=JsonSerializer.Deserialize<StoredProof>(body)?.Command??throw new InvalidOperationException("Original document command is unavailable.");}catch(JsonException){throw new InvalidOperationException("Original document command is malformed.");}command.RequireRecorded(row);return command;
    }
    public void RequireRecorded(WORK_ORDER_INSPECTION row)
    {
        Require();WorkOrderInspectionRules.Require(row.FIELD_ID,row.WORK_ORDER_ID,[row]);
        if(row.DOCUMENT_ACCEPTANCE_DATA_JSON is not {Length:>0} body || body.Length>MaximumBodyLength || row.DOCUMENT_ACCEPTANCE_REVISION!=Digest(row) || row.INSPECTED_AT_UTC is not {} utc || utc.Kind!=DateTimeKind.Utc)throw new InvalidOperationException("Complete original document acceptance/result proof is unavailable or changed.");
        StoredProof stored;WORK_ORDER_INSPECTION pending;try{stored=JsonSerializer.Deserialize<StoredProof>(body)??throw new InvalidOperationException("Original document proof is unavailable.");pending=JsonSerializer.Deserialize<WORK_ORDER_INSPECTION>(stored.PendingRowJson)??throw new InvalidOperationException("Original pending row is unavailable.");}catch(JsonException){throw new InvalidOperationException("Original document proof is malformed.");}
        if(JsonSerializer.Serialize(stored.Command)!=JsonSerializer.Serialize(this) || JsonSerializer.Serialize(WorkOrderInspectionRules.ProjectForRequirement(pending,OriginalInspection))!=JsonSerializer.Serialize(OriginalInspection) || JsonSerializer.Serialize(Prepare(pending,utc))!=JsonSerializer.Serialize(row))throw new InvalidOperationException("Complete original document acceptance/requirement/native audit changed.");
    }
    public CompanyWorkOrderDocumentAcceptanceFacts Facts(WORK_ORDER_INSPECTION row){RequireRecorded(row);return new(IntentId,Review.Source.Query.Permit.IntentId,Review.Source.Query.Kind,Review.Source.Query.ComponentId,Review.Source.ExpectedReference,Review.Source.InformationItem!.Version!,Review.Source.File!.ContentRevision!,CompanyWorkOrderPermitDocumentFacts.EvidenceProof(Review.Source),Review.PermitReview.Context.ActorUserId,row.INSPECTED_AT_UTC!.Value,row.DOCUMENT_ACCEPTANCE_REVISION!){ContributorEvidenceVersion=ContributorReview is null?0:1,ContributorProof=ContributorReview is null?null:ContributorProof(ContributorReview)};}
}
public sealed record CompanyWorkOrderDocumentAcceptanceReference(CompanyWorkOrderExecutionPermitReference Permit,string InspectionId,Guid IntentId)
{
    public void Require(){if(Permit is null || !LeaseSummaryValidator.Identifier(InspectionId) || IntentId==Guid.Empty)throw new ArgumentException("Retain the original permit/document requirement/acceptance references.");Permit.Require();}
}
public sealed record CompanyWorkOrderDocumentAcceptanceRecovery(CompanyWorkOrderDocumentAcceptanceCommand Command,WORK_ORDER_INSPECTION Recorded)
{
    public void Require(CompanyScope company,string actor,CompanyWorkOrderDocumentAcceptanceReference reference){reference.Require();if(Command is null || Recorded is null || Command.Review.PermitReview.Context.Company!=company || Command.Review.PermitReview.Context.ActorUserId!=actor || Command.Review.Source.Query.Permit!=reference.Permit || Command.InspectionId!=reference.InspectionId || Command.IntentId!=reference.IntentId)throw new UnauthorizedAccessException("Original document acceptance company/reviewer/reference differs.");Command.RequireRecorded(Recorded);}
}
public interface ICompanyWorkOrderDocumentAcceptanceOwner
{
    Task<WORK_ORDER_INSPECTION> RecordDocumentAcceptanceAsync(CompanyWorkOrderDocumentAcceptanceCommand command,Func<Task> requireAuthority,Func<WORK_ORDER_INSPECTION?,Task> requireCurrentProof,Func<WORK_ORDER_INSPECTION?,Task> requireCoreProof,bool readOnly=false);
    Task<CompanyWorkOrderDocumentAcceptanceRecovery> RecoverDocumentAcceptanceReferenceAsync(FieldSelectionReview context,CompanyWorkOrderDocumentAcceptanceReference reference,Func<Task> requireAuthority);
}
