using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Company;
namespace Beep.OilandGas.Models.Data.WorkOrder;
/// <summary>Current public document comparison is separate from retained independent acceptance.</summary>
public sealed record CompanyWorkOrderAcceptedDocumentReview(CompanyWorkOrderDocumentAcceptanceFacts Acceptance,CompanyWorkOrderPermitDocumentReview Evidence,DateTime ComparedAtUtc,IReadOnlyList<string> Blockers)
{
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)] public int ContributorComparisonVersion {get;init;}
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] public CompanyWorkOrderDocumentContributors? CurrentContributors {get;init;}
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] public CompanyPersonnelIdentityCatalog? CurrentIdentities {get;init;}
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] public CompanyNativePersonFacts? RecordedReviewerPerson {get;init;}
    public static string[] GetBlockers(CompanyWorkOrderExecutionPermitIntent proposal,CompanyWorkOrderDocumentAcceptanceFacts acceptance,CompanyWorkOrderPermitDocumentReview evidence,DateTime utc,int contributorComparisonVersion=0,CompanyWorkOrderDocumentContributors? contributors=null,CompanyPersonnelIdentityCatalog? identities=null,CompanyNativePersonFacts? reviewer=null)
    {
        var result=evidence.Blockers.Where(row=>row!=CompanyWorkOrderPermitDocumentReview.PendingAcceptanceBlocker).ToList();var source=evidence.Source;
        if(acceptance.PermitIntentId!=proposal.IntentId || source.Query.Permit!=new CompanyWorkOrderExecutionPermitReference(proposal.Task,proposal.IntentId) || source.Query.Kind!=acceptance.Kind || source.Query.ComponentId!=acceptance.ComponentId || source.ExpectedReference!=acceptance.DocumentId)result.Add("Recorded document acceptance does not match the exact current permit/kind/component/reference.");
        if(acceptance.Reviewer==proposal.Context.ActorUserId || acceptance.Reviewer==source.Document?.RecordedBy || acceptance.AcceptedAtUtc.Kind!=DateTimeKind.Utc || acceptance.AcceptedAtUtc>utc || acceptance.AcceptedAtUtc<proposal.ValidFromUtc || acceptance.AcceptedAtUtc>=proposal.ValidToUtc)result.Add("Recorded document reviewer independence or acceptance window is unconfirmed.");
        if(acceptance.Version!=source.InformationItem?.Version || acceptance.ContentRevision!=source.File?.ContentRevision || acceptance.SourceProof!=CompanyWorkOrderPermitDocumentFacts.EvidenceProof(source))result.Add("Current complete document membership/header/version/content evidence changed since independent acceptance.");
        if(contributorComparisonVersion==1)
        {
            if(acceptance.ContributorEvidenceVersion!=1 || !ProcessDefinitionRevision.IsValid(acceptance.ContributorProof))result.Add("Recorded document has no contributor-aware acceptance; historical proof is recovery-only.");
            if(contributors is null || identities is null)result.Add("Complete current contributors and recorded-reviewer identity evidence are unavailable.");
            else
            {
                contributors.Require();identities.Require();var context=evidence.PermitReview.Context;
                if(identities.Context.Company!=context.Company || identities.Context.Database!=context.Database || identities.Context.ActorUserId!=context.ActorUserId || CompanyWorkOrderPermitDocumentFacts.CurrentProof(contributors.Source)!=CompanyWorkOrderPermitDocumentFacts.CurrentProof(source) || contributors.ComparedAtUtc>utc || identities.ComparedAtUtc>utc)throw new InvalidOperationException("Current recorded-reviewer comparison company/source/context/time differs.");
                result.AddRange(CompanyWorkOrderDocumentContributorReview.GetBlockers(evidence,contributors,identities,reviewer,utc,acceptance.Reviewer).Where(value=>value!=CompanyWorkOrderPermitDocumentReview.PendingAcceptanceBlocker));
                var proof=CompanyWorkOrderDocumentAcceptanceCommand.ContributorProof(new(evidence,contributors,identities,reviewer,utc,[]));
                if(proof!=acceptance.ContributorProof)result.Add("Current complete contributor/recorded-reviewer identity/person proof changed since acceptance.");
            }
        }
        else if(contributorComparisonVersion!=0 || contributors is not null || identities is not null || reviewer is not null)throw new InvalidOperationException("Retained contributor comparison policy is unsupported.");
        return result.ToArray();
    }
    public void Require(CompanyWorkOrderExecutionPermitIntent proposal,CompanyMaintenanceInspectionReview current)
    {
        if(Acceptance is null || Evidence is null || Blockers is null || ComparedAtUtc==default || ComparedAtUtc.Kind!=DateTimeKind.Utc || Acceptance.IntentId==Guid.Empty || Acceptance.Kind is not ("ISOLATION" or "RISK") || !LeaseSummaryValidator.Identifier(Acceptance.ComponentId) || !LeaseSummaryValidator.Identifier(Acceptance.DocumentId) || !ProcessDefinitionRevision.IsValid(Acceptance.Revision) || !ProcessDefinitionRevision.IsValid(Acceptance.ContentRevision) || !ProcessDefinitionRevision.IsValid(Acceptance.SourceProof))throw new InvalidOperationException("Complete current public document acceptance is unavailable.");
        Evidence.Require(proposal.Context.Company,current.Stage.Context.ActorUserId,Evidence.Source.Query);
        var rows=current.Inspections.Where(row=>row.PermitPrerequisiteKind==Acceptance.Kind && row.RegulatoryRef==Acceptance.DocumentId && row.Jurisdiction==proposal.Jurisdiction && row.DocumentAcceptance is not null).ToArray();
        if(rows.Length!=1 || rows[0].Status!="PASS" || rows[0].DocumentAcceptance!=Acceptance || rows[0].Inspector!=Acceptance.Reviewer || rows[0].RequirementActor==Acceptance.Reviewer || rows[0].InspectDate!=Acceptance.AcceptedAtUtc || Evidence.PermitReview.CurrentSource.SnapshotRevision!=current.SnapshotRevision || ComparedAtUtc<Evidence.Source.ComparedAtUtc || !Blockers.SequenceEqual(GetBlockers(proposal,Acceptance,Evidence,ComparedAtUtc,ContributorComparisonVersion,CurrentContributors,CurrentIdentities,RecordedReviewerPerson),StringComparer.Ordinal))throw new InvalidOperationException("Exact current accepted document requirement/source or blockers changed.");
    }
}
