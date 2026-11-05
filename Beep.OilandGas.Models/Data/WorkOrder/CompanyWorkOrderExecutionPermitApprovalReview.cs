using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
namespace Beep.OilandGas.Models.Data.WorkOrder;

/// <summary>An independent review of a pending proposal; it neither approves nor authorizes execution.</summary>
public sealed record CompanyWorkOrderExecutionPermitApprovalReview(
    FieldSelectionReview Context, CompanyWorkOrderExecutionPermitIntent Proposal,
    CompanyMaintenanceInspectionReview OriginalSource, CompanyMaintenanceInspectionReview CurrentSource,
    string RecordedRevision, DateTime ComparedAtUtc, IReadOnlyList<string> Blockers)
{
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    public int PrerequisiteEvidenceVersion {get;init;}
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] public CompanyWorkOrderAcceptedMaterialReview? CurrentMaterialAcceptance {get;init;}
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] public CompanyWorkOrderAcceptedPersonnelReview? CurrentPersonnelAcceptance {get;init;}
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] public IReadOnlyList<CompanyWorkOrderAcceptedDocumentReview>? CurrentDocumentAcceptance {get;init;}
    /// <summary>Validate complete retained proof again at the actual approval boundary.</summary>
    public void RequireCurrentPrerequisiteAcceptance(DateTime utc)
    {
        if(Context is null || Proposal is null)throw new InvalidOperationException("The original independent approval context is unavailable.");
        Require(Context.Company,Context.ActorUserId,new(Proposal.Task,Proposal.IntentId));
        if(!(PrerequisiteEvidenceVersion==6 && Proposal.RequirementEvidenceVersion==1 || PrerequisiteEvidenceVersion==7 && Proposal.RequirementEvidenceVersion==2) || utc.Kind!=DateTimeKind.Utc || utc<ComparedAtUtc)
            throw new InvalidOperationException("Approval requires current versioned native requirements and an explicit current UTC boundary.");
        if(CurrentMaterialAcceptance is null || CurrentPersonnelAcceptance is null || CurrentDocumentAcceptance is null || CurrentDocumentAcceptance.Count!=2)
            throw new InvalidOperationException("Approval requires complete current material, personnel, isolation and risk acceptance.");
        if(CurrentDocumentAcceptance.Any(row=>row.CurrentIdentities is null) || CurrentDocumentAcceptance.Select(row=>row.CurrentIdentities!.Revision).Distinct(StringComparer.Ordinal).Count()!=1)
            throw new InvalidOperationException("Complete current document acceptance must share one recorded-reviewer identity catalog population.");
        var current=GetBlockers(Proposal,OriginalSource,CurrentSource,utc,PrerequisiteEvidenceVersion,CurrentMaterialAcceptance,CurrentPersonnelAcceptance,CurrentDocumentAcceptance);
        if(Blockers.Count!=0 || current.Length!=0)throw new InvalidOperationException("Current prerequisite acceptance is incomplete or changed; review the original permit again.");
    }
    /// <summary>Compare complete source proof across fresh reads without treating observation time as a source change.</summary>
    public string PrerequisiteProofRevision()
    {
        if(Context is null || Proposal is null)throw new InvalidOperationException("The original prerequisite comparison context is unavailable.");
        Require(Context.Company,Context.ActorUserId,new(Proposal.Task,Proposal.IntentId));
        var material=CurrentMaterialAcceptance;var personnel=CurrentPersonnelAcceptance;
        var documents=CurrentDocumentAcceptance?.OrderBy(row=>row.Acceptance.Kind,StringComparer.Ordinal).Select(row=>new
        {
            row.Acceptance,row.Evidence.Source.Query,Source=CompanyWorkOrderPermitDocumentFacts.CurrentProof(row.Evidence.Source),
            Contributors=row.CurrentContributors is null?null:CompanyWorkOrderDocumentContributors.EvidenceProof(row.CurrentContributors),
            Identities=row.CurrentIdentities?.Revision,row.RecordedReviewerPerson,row.ContributorComparisonVersion
        }).ToArray();
        return ProcessDefinitionRevision.FromJson(System.Text.Json.JsonSerializer.Serialize(new
        {
            Context,Proposal,RecordedRevision,PrerequisiteEvidenceVersion,Original=OriginalSource.SnapshotRevision,Current=CurrentSource.SnapshotRevision,
            Material=material is null?null:new{material.Acceptance,material.Reservation,material.Material.Source.Context,Inventory=material.Material.Source.Inventory.Revision,material.Material.Allocations,material.Material.Releases},
            Personnel=personnel is null?null:new{personnel.Acceptance,personnel.Evidence.Assignment.Query,personnel.Evidence.Assignment.Roster.Context,personnel.Evidence.Assignment.Roster.Assignments,personnel.Evidence.Assignment.Roster.Releases,Policies=CompanyWorkOrderPersonnelAcceptanceCommand.PolicyProof(personnel.Evidence.Policies),Credentials=CompanyWorkOrderPersonnelAcceptanceCommand.CredentialProof(personnel.Evidence.Credentials),personnel.Evidence.EligibilityEvidenceVersion},
            Documents=documents
        }));
    }
    public static string[] GetBlockers(CompanyWorkOrderExecutionPermitIntent proposal,
        CompanyMaintenanceInspectionReview original,CompanyMaintenanceInspectionReview current,DateTime utc,int prerequisiteEvidenceVersion=0,CompanyWorkOrderAcceptedMaterialReview? materialAcceptance=null,CompanyWorkOrderAcceptedPersonnelReview? personnelAcceptance=null,IReadOnlyList<CompanyWorkOrderAcceptedDocumentReview>? documentAcceptance=null)
    {
        proposal.RequireSource(original);current.Require(proposal.Context.Company,current.Stage.Context.ActorUserId,proposal.Task);
        if(utc==default || utc.Kind!=DateTimeKind.Utc)throw new InvalidOperationException("An explicit recorded UTC comparison time is required.");
        var result=new List<string>();
        if(prerequisiteEvidenceVersion is 6 or 7?proposal.RequirementEvidenceVersion!=(prerequisiteEvidenceVersion==7?2:1) || original.RequirementFactsRevision()!=current.RequirementFactsRevision():original.FactsRevision()!=current.FactsRevision())result.Add("Original order, inspection or assigned-stage facts changed; independent re-review is required.");
        if(utc<proposal.ValidFromUtc)result.Add("The reported validity window has not started.");
        if(utc>=proposal.ValidToUtc)result.Add("The reported validity window has expired.");
        if(prerequisiteEvidenceVersion==7){global::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionApplicability.RequireComplete(original.Inspections);global::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionApplicability.RequireComplete(current.Inspections);if(!current.Inspections.Any(row=>row.ApplicableStage=="PRE_START"))result.Add("No explicitly applicable pre-start inspection establishes approval readiness.");}
        if(current.Inspections.Count==0)result.Add("No recorded inspection requirements establish prerequisite acceptance.");
        else if(current.Inspections.Any(row=>(prerequisiteEvidenceVersion!=7 || row.ApplicableStage=="PRE_START") && row.Status!="PASS"))result.Add("Recorded inspections include unaccepted results; PASS or independently justified exceptions are required.");
        if(prerequisiteEvidenceVersion==0)result.Add("Independent isolation, risk, personnel and material prerequisite acceptance is not yet recorded by this workflow.");
        else if(prerequisiteEvidenceVersion==1)
        {
            foreach(var evidence in CompanyWorkOrderPermitPrerequisiteEvidence.Read(proposal,current,utc))
                if(!evidence.Accepted)result.Add($"{evidence.Kind} inspection acceptance is not yet recorded: {evidence.State}.");
            result.Add("Independent material inventory quantity, unit, allocation and availability evidence is not yet recorded by this workflow.");
        }
        else if(prerequisiteEvidenceVersion==2)
        {
            foreach(var evidence in CompanyWorkOrderPermitPrerequisiteEvidence.Read(proposal,current,utc))if(!evidence.Accepted)result.Add($"{evidence.Kind} inspection acceptance is not yet recorded: {evidence.State}.");
            if(materialAcceptance is null)result.Add("Complete current recorded material acceptance and inventory/reservation evidence is unavailable.");
            else{materialAcceptance.Require(proposal,current);result.AddRange(materialAcceptance.Blockers);}
            result.Add("Verified personnel qualifications and referenced isolation/risk/source documents are still required before permit approval.");
        }
        else if(prerequisiteEvidenceVersion==3)
        {
            foreach(var evidence in CompanyWorkOrderPermitPrerequisiteEvidence.Read(proposal,current,utc).Where(row=>row.Kind!="PERSONNEL"))if(!evidence.Accepted)result.Add($"{evidence.Kind} inspection acceptance is not yet recorded: {evidence.State}.");
            if(materialAcceptance is null)result.Add("Complete current recorded material acceptance and inventory/reservation evidence is unavailable.");
            else{materialAcceptance.Require(proposal,current);result.AddRange(materialAcceptance.Blockers);}
            if(personnelAcceptance is null)result.Add("Complete current recorded personnel acceptance and duty/policy/credential evidence is unavailable.");
            else{personnelAcceptance.Require(proposal,current);result.AddRange(personnelAcceptance.Blockers);}
            result.Add("Referenced isolation/risk/source documents are still required before permit approval.");
        }
        else if(prerequisiteEvidenceVersion is 4 or 5 or 6 or 7)
        {
            foreach(var evidence in CompanyWorkOrderPermitPrerequisiteEvidence.Read(proposal,current,utc).Where(row=>row.Kind=="MATERIAL"))if(!evidence.Accepted)result.Add($"{evidence.Kind} inspection acceptance is not yet recorded: {evidence.State}.");
            if(materialAcceptance is null)result.Add("Complete current recorded material acceptance and inventory/reservation evidence is unavailable.");else{materialAcceptance.Require(proposal,current);result.AddRange(materialAcceptance.Blockers);}
            if(personnelAcceptance is null)result.Add("Complete current recorded personnel acceptance and duty/policy/credential evidence is unavailable.");else{personnelAcceptance.Require(proposal,current);result.AddRange(personnelAcceptance.Blockers);}
            if(documentAcceptance is not null && (documentAcceptance.Count>2 || documentAcceptance.Any(row=>row is null || row.Acceptance is null) || documentAcceptance.Select(row=>row.Acceptance.Kind).Distinct(StringComparer.Ordinal).Count()!=documentAcceptance.Count))throw new InvalidOperationException("Current document acceptance population is ambiguous.");
            foreach(var kind in new[]{"ISOLATION","RISK"}){var accepted=documentAcceptance?.SingleOrDefault(row=>row.Acceptance.Kind==kind);if(accepted is null)result.Add($"Complete current recorded {kind} document acceptance and exact source/content evidence is unavailable.");else{accepted.Require(proposal,current);result.AddRange(accepted.Blockers);if((prerequisiteEvidenceVersion is 5 or 6 or 7) && accepted.ContributorComparisonVersion!=1)result.Add($"Current {kind} contributor/recorded-reviewer comparison is unavailable.");}}
            if(prerequisiteEvidenceVersion==4)result.Add("Authoritative document contributor/reviewer identity mapping is still required before permit approval; a recording account alone does not establish preparer/approver independence.");
        }        else throw new InvalidOperationException("The retained prerequisite review policy version is unsupported.");
        return result.ToArray();
    }
    public void Require(CompanyScope company,string actor,CompanyWorkOrderExecutionPermitReference query)
    {
        query.Require();if(Context is null || Proposal is null || OriginalSource is null || CurrentSource is null || Blockers is null)throw new InvalidOperationException("The complete independent permit review is unavailable.");
        if(PrerequisiteEvidenceVersion is not (2 or 3 or 4 or 5 or 6 or 7) && CurrentMaterialAcceptance is not null || CurrentMaterialAcceptance is {} accepted && accepted.ComparedAtUtc>ComparedAtUtc || PrerequisiteEvidenceVersion is not (3 or 4 or 5 or 6 or 7) && CurrentPersonnelAcceptance is not null || CurrentPersonnelAcceptance is {} personnel && personnel.ComparedAtUtc>ComparedAtUtc)throw new InvalidOperationException("Current prerequisite proof is not part of this retained policy version/time.");
        if(PrerequisiteEvidenceVersion is not (4 or 5 or 6 or 7) && CurrentDocumentAcceptance is not null || CurrentDocumentAcceptance is not null && CurrentDocumentAcceptance.Any(row=>row is null || row.ComparedAtUtc>ComparedAtUtc))throw new InvalidOperationException("Current document comparison is not part of this retained policy version/time.");
        if(PrerequisiteEvidenceVersion==4 && CurrentDocumentAcceptance is not null && CurrentDocumentAcceptance.Any(row=>row.ContributorComparisonVersion!=0 || row.CurrentContributors is not null || row.CurrentIdentities is not null || row.RecordedReviewerPerson is not null))throw new InvalidOperationException("Historical policy 4 does not include contributor-aware current comparison.");
        FieldSelectionReviewValidator.RequireReview(Context,company,actor);Proposal.RequireSource(OriginalSource);CurrentSource.Require(company,actor,query.Task);
        if(Proposal.Context.Company!=company || Proposal.Task!=query.Task || Proposal.IntentId!=query.IntentId || Proposal.Context.ActorUserId==actor || Context.FieldId!=query.Task.FieldId || Context.Database!=Proposal.Context.Database || Context.Database!=CurrentSource.InspectionDatabase || !ProcessDefinitionRevision.IsValid(RecordedRevision) || !Blockers.SequenceEqual(GetBlockers(Proposal,OriginalSource,CurrentSource,ComparedAtUtc,PrerequisiteEvidenceVersion,CurrentMaterialAcceptance,CurrentPersonnelAcceptance,CurrentDocumentAcceptance),StringComparer.Ordinal))
            throw new InvalidOperationException("The independent reviewer/company/task/source or complete blockers are unconfirmed.");
    }
}
