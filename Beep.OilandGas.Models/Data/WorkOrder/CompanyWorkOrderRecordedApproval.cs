using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
namespace Beep.OilandGas.Models.Data.WorkOrder;

/// <summary>Recorded approval facts for an authorized execution persona; not current permission or readiness.</summary>
public sealed record CompanyWorkOrderRecordedApproval(CompanyScope Company,CompanyWorkOrderExecutionPermitReference Permit,
    string PermitId,Guid DecisionId,string Issuer,string Approver,DateTime ApprovedAtUtc,DateTime ValidFromUtc,DateTime ValidToUtc,
    string PermitType,string Jurisdiction,string ScopeDescription,int RequirementEvidenceVersion,int PrerequisiteEvidenceVersion,
    string OriginalRequirementRevision,string RecordedRevision)
{
    public void Require(CompanyScope company,CompanyWorkOrderExecutionPermitReference query)
    {
        query.Require();
        if(Company!=company || Permit!=query || PermitId!=CompanyWorkOrderExecutionPermitIntent.PermitIdFor(company,query.IntentId) || DecisionId==Guid.Empty ||
            !LeaseSummaryValidator.Identifier(Issuer) || !LeaseSummaryValidator.Identifier(Approver) || Issuer==Approver ||
            ApprovedAtUtc.Kind!=DateTimeKind.Utc || ValidFromUtc.Kind!=DateTimeKind.Utc || ValidToUtc.Kind!=DateTimeKind.Utc || ApprovedAtUtc<ValidFromUtc || ApprovedAtUtc>=ValidToUtc ||
            string.IsNullOrWhiteSpace(PermitType) || string.IsNullOrWhiteSpace(Jurisdiction) || string.IsNullOrWhiteSpace(ScopeDescription) ||
            !(RequirementEvidenceVersion==1 && PrerequisiteEvidenceVersion==6 || RequirementEvidenceVersion==2 && PrerequisiteEvidenceVersion==7) ||
            !ProcessDefinitionRevision.IsValid(OriginalRequirementRevision) || !ProcessDefinitionRevision.IsValid(RecordedRevision))
            throw new InvalidOperationException("Complete original recorded permit approval facts are unavailable or differ from the authorized company/reference.");
    }
}
