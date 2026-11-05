using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Company;
namespace Beep.OilandGas.Models.Data.WorkOrder;
/// <summary>Current public personnel evidence remains separate from retained independent acceptance.</summary>
public sealed record CompanyWorkOrderAcceptedPersonnelReview(CompanyWorkOrderPersonnelAcceptanceFacts Acceptance,CompanyPersonnelPolicyEvidenceReview Evidence,DateTime ComparedAtUtc,IReadOnlyList<string> Blockers)
{
    public static string[] GetBlockers(CompanyWorkOrderExecutionPermitIntent proposal,CompanyWorkOrderPersonnelAcceptanceFacts acceptance,CompanyPersonnelPolicyEvidenceReview evidence,DateTime utc)
    {
        var result=evidence.Blockers.Where(row=>row!=CompanyPersonnelPolicyEvidenceReview.PendingAcceptanceBlocker).ToList();var assignment=evidence.Assignment.Assignment;var policy=evidence.ApplicablePolicy;
        if(acceptance.PermitIntentId!=proposal.IntentId || evidence.Assignment.Query.Permit!=new CompanyWorkOrderExecutionPermitReference(proposal.Task,proposal.IntentId) || acceptance.AssignmentId!=assignment.AssignmentId || acceptance.AssignmentRevision!=assignment.RecordedRevision)result.Add("Recorded personnel acceptance does not match the exact current permit and duty assignment.");
        if(acceptance.Reviewer==proposal.Context.ActorUserId || acceptance.Reviewer==assignment.AssignedBy || policy is not null && acceptance.Reviewer==policy.RecordedBy || acceptance.AcceptedAtUtc.Kind!=DateTimeKind.Utc || acceptance.AcceptedAtUtc>utc || acceptance.AcceptedAtUtc<proposal.ValidFromUtc || acceptance.AcceptedAtUtc>=proposal.ValidToUtc)result.Add("Recorded personnel reviewer independence or acceptance window is unconfirmed.");
        if(policy is null || acceptance.PolicyRecordId!=policy.RecordId || acceptance.PolicyRevision!=policy.Revision || acceptance.PolicyCatalogRevision!=CompanyWorkOrderPersonnelAcceptanceCommand.PolicyProof(evidence.Policies))result.Add("Current explicit personnel policy changed since independent acceptance.");
        if(acceptance.CredentialRevision!=CompanyWorkOrderPersonnelAcceptanceCommand.CredentialProof(evidence.Credentials))result.Add("Current membership/licence/status/condition evidence changed since independent personnel acceptance.");
        return result.ToArray();
    }
    public void Require(CompanyWorkOrderExecutionPermitIntent proposal,CompanyMaintenanceInspectionReview current)
    {
        if(Acceptance is null || Evidence is null || Blockers is null || ComparedAtUtc==default || ComparedAtUtc.Kind!=DateTimeKind.Utc || Acceptance.IntentId==Guid.Empty || !ProcessDefinitionRevision.IsValid(Acceptance.Revision) || !ProcessDefinitionRevision.IsValid(Acceptance.CredentialRevision) || !ProcessDefinitionRevision.IsValid(Acceptance.PolicyCatalogRevision))throw new InvalidOperationException("Complete current public personnel acceptance is unavailable.");
        Evidence.Require(proposal.Context.Company,current.Stage.Context.ActorUserId,Evidence.Assignment.Query);
        var rows=current.Inspections.Where(row=>row.PermitPrerequisiteKind=="PERSONNEL" && row.RegulatoryRef==(Acceptance.RequirementReferenceVersion switch{0=>Acceptance.AssignmentId,1 when proposal.RequirementEvidenceVersion is 1 or 2=>proposal.PersonnelReference,_=>throw new InvalidOperationException("Recorded personnel requirement policy is unsupported.")}) && row.Jurisdiction==proposal.Jurisdiction).ToArray();
        if(rows.Length!=1 || rows[0].Status!="PASS" || rows[0].PersonnelAcceptance!=Acceptance || rows[0].Inspector!=Acceptance.Reviewer || rows[0].RequirementActor==Acceptance.Reviewer || rows[0].InspectDate!=Acceptance.AcceptedAtUtc || Evidence.Assignment.CurrentPersonnel.PermitReview.CurrentSource.SnapshotRevision!=current.SnapshotRevision || ComparedAtUtc<Evidence.Credentials.ComparedAtUtc || !Blockers.SequenceEqual(GetBlockers(proposal,Acceptance,Evidence,ComparedAtUtc),StringComparer.Ordinal))throw new InvalidOperationException("Exact current accepted personnel requirement/source or blockers changed.");
    }
}
