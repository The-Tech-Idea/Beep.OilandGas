using System.Text.Json;
namespace TheTechIdea.Data.OilGas;

public sealed record CompanyPersonnelLicenseStatusFacts(string EmployeeBaId,string LicenseId,string StatusId,string? Status,string? StatusType,DateTime? EffectiveFrom,DateTime? EffectiveTo,string Revision);
public sealed record CompanyPersonnelLicenseConditionFacts(string EmployeeBaId,string LicenseId,string ConditionId,string? Code,string? Type,decimal? Value,string? Unit,string? ExemptIndicator,DateTime? EffectiveFrom,DateTime? EffectiveTo,string? EvidenceReference,string Revision);
public sealed record CompanyPersonnelCredentialFacts(CompanyPersonnelSourceFacts Personnel,IReadOnlyList<CompanyPersonnelLicenseStatusFacts> Statuses,
    IReadOnlyList<CompanyPersonnelLicenseConditionFacts> Conditions,DateTime ComparedAtUtc,string Revision)
{
    public void Require()
    {
        Personnel.Require();
        if(Statuses is null || Conditions is null || Statuses.Count>4096 || Conditions.Count>4096 || ComparedAtUtc==default || ComparedAtUtc.Kind!=DateTimeKind.Utc ||
            (Personnel.Employment is null || Personnel.License is null) && (Statuses.Count>0 || Conditions.Count>0) ||
            Statuses.Select(row=>row.StatusId).Distinct(StringComparer.Ordinal).Count()!=Statuses.Count || Conditions.Select(row=>row.ConditionId).Distinct(StringComparer.Ordinal).Count()!=Conditions.Count)
            throw new InvalidOperationException("Complete company-member licence status/condition populations are unavailable.");
        foreach(var row in Statuses)if(row.EmployeeBaId!=Personnel.Query.EmployeeBaId || row.LicenseId!=Personnel.Query.LicenseId || !LeaseSummaryValidator.Identifier(row.StatusId) || !ProcessDefinitionRevision.IsValid(row.Revision))throw new UnauthorizedAccessException("Licence status belongs to another employee/licence or lacks its original key/proof.");
        foreach(var row in Conditions)if(row.EmployeeBaId!=Personnel.Query.EmployeeBaId || row.LicenseId!=Personnel.Query.LicenseId || !LeaseSummaryValidator.Identifier(row.ConditionId) || !ProcessDefinitionRevision.IsValid(row.Revision))throw new UnauthorizedAccessException("Licence condition belongs to another employee/licence or lacks its original key/proof.");
        if(Revision!=Digest(Personnel,Statuses,Conditions,ComparedAtUtc))throw new InvalidOperationException("Complete original personnel credential source changed.");
    }
    private static string Digest(CompanyPersonnelSourceFacts source,IReadOnlyList<CompanyPersonnelLicenseStatusFacts> statuses,IReadOnlyList<CompanyPersonnelLicenseConditionFacts> conditions,DateTime utc)
        =>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Personnel=source,Statuses=statuses.OrderBy(row=>row.StatusId,StringComparer.Ordinal).ToArray(),Conditions=conditions.OrderBy(row=>row.ConditionId,StringComparer.Ordinal).ToArray(),ComparedAtUtc=utc}));
    public static CompanyPersonnelCredentialFacts Create(CompanyPersonnelSourceFacts source,IReadOnlyList<CompanyPersonnelLicenseStatusFacts> statuses,IReadOnlyList<CompanyPersonnelLicenseConditionFacts> conditions,DateTime utc){var result=new CompanyPersonnelCredentialFacts(source,statuses,conditions,utc,Digest(source,statuses,conditions,utc));result.Require();return result;}
}
public sealed record CompanyPersonnelPolicyEvidenceReview(CompanyWorkOrderAssignedPersonnelReview Assignment,CompanyPersonnelPolicyCatalog Policies,CompanyPersonnelCredentialFacts Credentials,IReadOnlyList<string> Blockers)
{
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    public int EligibilityEvidenceVersion {get;init;}
    public const string PendingAcceptanceBlocker="Independent PERSONNEL acceptance of the exact current policy/source proof is still required; this comparison does not approve a permit or authorize work.";
    [System.Text.Json.Serialization.JsonIgnore] public CompanyPersonnelPolicyVersion? ApplicablePolicy=>Select(Assignment,Policies);
    private static CompanyPersonnelPolicyVersion? Select(CompanyWorkOrderAssignedPersonnelReview assignment,CompanyPersonnelPolicyCatalog policies)
    {
        var task=assignment.CurrentPersonnel.PermitReview;var from=task.Proposal.ValidFromUtc;var to=task.Proposal.ValidToUtc;
        return policies.Versions.SingleOrDefault(row=>row.Policy.DutyCode==assignment.Assignment.DutyCode && row.Policy.WorkOrderType==task.CurrentSource.Order.WoSubType && row.Policy.Jurisdiction==task.Proposal.Jurisdiction && row.Policy.EffectiveFromUtc<=from && row.Policy.EffectiveToUtc>=to);
    }
    public static string[] GetBlockers(CompanyWorkOrderAssignedPersonnelReview assignment,CompanyPersonnelPolicyCatalog catalog,CompanyPersonnelCredentialFacts credentials,int eligibilityEvidenceVersion=0)
    {
        if(eligibilityEvidenceVersion is not (0 or 1))throw new InvalidOperationException("Unsupported retained personnel eligibility policy version.");
        catalog.Require();credentials.Require();var result=new List<string>();var permit=assignment.CurrentPersonnel.PermitReview.Proposal;var source=credentials.Personnel;
        if(assignment.Roster.Releases is null)result.Add("Complete current assignment cancellation evidence is unavailable.");
        else if(assignment.Roster.Releases.Any(row=>row.AssignmentId==assignment.Assignment.AssignmentId))result.Add("The original duty assignment is cancelled.");
        if(credentials.ComparedAtUtc<assignment.Assignment.ValidFromUtc || credentials.ComparedAtUtc>=assignment.Assignment.ValidToUtc || assignment.Assignment.ValidFromUtc>permit.ValidFromUtc || assignment.Assignment.ValidToUtc<permit.ValidToUtc)result.Add("The recorded duty does not cover the current complete permit window.");
        if(source.Employment is null)result.Add("Current exact company employment membership is unavailable.");
        if(source.License is null)result.Add("Current exact personnel licence is unavailable.");
        if(string.IsNullOrWhiteSpace(assignment.CurrentPersonnel.PermitReview.CurrentSource.Order.WoSubType))result.Add("Actual native work-order type is unconfirmed; no policy type is inferred.");
        var policy=Select(assignment,catalog);
        if(policy is null)result.Add("No single explicit company duty/work-order/jurisdiction policy covers the complete permit window.");
        else
        {
            var p=policy.Policy;
            if(eligibilityEvidenceVersion==1)
            {
                if(p.AcceptedEmploymentStatuses is null || p.AcceptedEmploymentPositions is null)result.Add("Explicit company employment eligibility configuration is unavailable; historical policy does not establish current eligibility.");
                else if(source.Employment is {} employee)
                {
                    if(!p.AcceptedEmploymentStatuses.Contains(employee.Status??"",StringComparer.Ordinal))result.Add("Recorded employment status is not explicitly accepted by the current company policy.");
                    if(p.AcceptedEmploymentPositions.Count>0 && !p.AcceptedEmploymentPositions.Contains(employee.Position??"",StringComparer.Ordinal))result.Add("Recorded employment position does not match the current company policy restrictions.");
                }
            }
            if(source.License is {} license)
            {
                if(license.Type!=p.LicenseType || license.GrantingAuthority!=p.GrantingAuthority)result.Add("Recorded licence type/granting authority does not match the explicit company policy.");
                if(license.GrantedDate is not {} granted || granted.Kind!=DateTimeKind.Utc || granted>credentials.ComparedAtUtc || !Covers(license.EffectiveDate,license.ExpiryDate))result.Add("Recorded licence grant/effective UTC evidence does not cover the full permit window.");
            }
            if(source.Employment is {} employment && (string.IsNullOrWhiteSpace(employment.Position) || string.IsNullOrWhiteSpace(employment.Status) || !Covers(employment.EffectiveDate,employment.ExpiryDate)))result.Add("Recorded employment position/status/effective UTC evidence is unconfirmed for the full permit window.");
            var statuses=credentials.Statuses.Where(row=>row.StatusType==p.StatusType && Covers(row.EffectiveFrom,row.EffectiveTo)).ToArray();
            if(statuses.Length!=1 || !p.AcceptedStatuses.Contains(statuses[0].Status??"",StringComparer.Ordinal))result.Add("One unambiguous current recorded licence status/type explicitly accepted by policy is required.");
            foreach(var requirement in p.Conditions)
            {
                var rows=credentials.Conditions.Where(row=>row.Code==requirement.Code && row.Type==requirement.Type && Covers(row.EffectiveFrom,row.EffectiveTo)).ToArray();
                if(rows.Length!=1 || rows[0].ExemptIndicator!="N" || rows[0].Unit!=requirement.Unit || rows[0].Value is not {} value || value<=0 || string.IsNullOrWhiteSpace(rows[0].EvidenceReference) ||
                    requirement.Comparison=="MINIMUM" && value<requirement.Value || requirement.Comparison=="EXACT" && value!=requirement.Value)
                    result.Add("Required recorded competency condition is missing, ambiguous, exempt, unconfirmed or does not match explicit value/unit policy: "+requirement.Code+".");
            }
        }
        result.Add(PendingAcceptanceBlocker);
        return result.ToArray();
        bool Covers(DateTime? start,DateTime? end)=>start is {} from && end is {} to && from.Kind==DateTimeKind.Utc && to.Kind==DateTimeKind.Utc && from<=permit.ValidFromUtc && to>=permit.ValidToUtc && credentials.ComparedAtUtc>=from && credentials.ComparedAtUtc<to;
    }
    public void Require(CompanyScope company,string actor,CompanyWorkOrderAssignedPersonnelQuery query)
    {
        Assignment.Require(company,actor,query);Policies.Require();Credentials.Require();var context=Assignment.CurrentPersonnel.Personnel.Context;
        if(Policies.Context.Company!=company || Policies.Context.ActorUserId!=actor || Policies.Context.Database!=context.Database || Credentials.Personnel.Context!=context || Credentials.Personnel.Query!=Assignment.CurrentPersonnel.Personnel.Query ||
            Credentials.Personnel.Employment!=Assignment.CurrentPersonnel.Personnel.Employment || Credentials.Personnel.License!=Assignment.CurrentPersonnel.Personnel.License ||
            !Blockers.SequenceEqual(GetBlockers(Assignment,Policies,Credentials,EligibilityEvidenceVersion),StringComparer.Ordinal))throw new InvalidOperationException("Exact current assignment/policy/member/credential source or blockers changed.");
    }
}
public interface ICompanyPersonnelCredentialOwner
{
    Task<CompanyPersonnelCredentialFacts> ReadPersonnelCredentialsAsync(FieldSelectionReview context,CompanyWorkOrderPersonnelSourceQuery query,Func<Task> requireAuthority);
}
