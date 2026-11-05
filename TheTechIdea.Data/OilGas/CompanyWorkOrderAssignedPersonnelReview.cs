using System.Text.Json;
namespace TheTechIdea.Data.OilGas;

/// <summary>Public original assignment facts; the maker's command, rationale and recovery intent stay private.</summary>
public sealed record CompanyWorkOrderPersonnelAssignmentFacts(
    CompanyScope Company,AssetDatabaseScope Database,CompanyWorkOrderExecutionPermitReference Permit,
    string AssignmentId,string EmployeeBaId,string LicenseId,string DutyCode,DateTime ValidFromUtc,DateTime ValidToUtc,
    string AssignedBy,DateTime AssignedAtUtc,string RecordedRevision,string EmploymentRevision,string LicenseRevision,
    string PermitRevision,string OrderInspectionFactsRevision)
{
    public void Require()
    {
        Permit.Require();
        if(Company is null || Database is null || string.IsNullOrWhiteSpace(Company.TenantId) || string.IsNullOrWhiteSpace(Company.BusinessAssociateId) ||
            !LeaseSummaryValidator.Identifier(Database.ConnectionName) || !LeaseSummaryValidator.Identifier(Database.Fingerprint) ||
            string.IsNullOrWhiteSpace(AssignmentId) || !AssignmentId.StartsWith("wo-personnel-",StringComparison.Ordinal) || !ProcessDefinitionRevision.IsValid(AssignmentId["wo-personnel-".Length..]) ||
            !LeaseSummaryValidator.Identifier(EmployeeBaId) || !LeaseSummaryValidator.Identifier(LicenseId) || !LeaseSummaryValidator.Identifier(DutyCode) ||
            !LeaseSummaryValidator.Identifier(AssignedBy) || AssignedAtUtc==default || AssignedAtUtc.Kind!=DateTimeKind.Utc ||
            ValidFromUtc==default || ValidFromUtc.Kind!=DateTimeKind.Utc || ValidToUtc.Kind!=DateTimeKind.Utc || ValidToUtc<=ValidFromUtc || AssignedAtUtc>=ValidToUtc ||
            new[]{RecordedRevision,EmploymentRevision,LicenseRevision,PermitRevision,OrderInspectionFactsRevision}.Any(revision=>!ProcessDefinitionRevision.IsValid(revision)))
            throw new InvalidOperationException("Complete original public assignment identity, native binding, UTC window, maker/audit and source revisions are required.");
    }
    public static CompanyWorkOrderPersonnelAssignmentFacts From(WORK_ORDER_PERSONNEL_ASSIGNMENT row)
    {
        if(string.IsNullOrEmpty(row.COMMAND_JSON) || row.COMMAND_JSON.Length>2*1024*1024)throw new InvalidOperationException("Original bounded native assignment proof is unavailable.");
        CompanyWorkOrderPersonnelAssignmentCommand command;
        try{command=JsonSerializer.Deserialize<CompanyWorkOrderPersonnelAssignmentCommand>(row.COMMAND_JSON)??throw new InvalidOperationException("Original assignment proof is unavailable.");}
        catch(JsonException){throw new InvalidOperationException("Original assignment proof is malformed.");}
        command.RequireRecorded(row);
        var source=command.Review.Personnel;var permit=command.Review.PermitReview;
        var result=new CompanyWorkOrderPersonnelAssignmentFacts(source.Context.Company,source.Context.Database,source.Query.Permit,row.ASSIGNMENT_ID,row.EMPLOYEE_BA_ID,row.LICENSE_ID,row.DUTY_CODE,row.VALID_FROM_UTC,row.VALID_TO_UTC,row.ROW_CREATED_BY!,row.ROW_CREATED_DATE!.Value,row.RECORDED_REVISION,source.Employment!.Revision,source.License!.Revision,permit.RecordedRevision,permit.CurrentSource.FactsRevision());
        result.Require();return result;
    }
}
public sealed record CompanyWorkOrderPersonnelAssignmentRoster(FieldSelectionReview Context,CompanyWorkOrderExecutionPermitReference Permit,
    IReadOnlyList<CompanyWorkOrderPersonnelAssignmentFacts> Assignments,DateTime ComparedAtUtc,string Revision)
{
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<CompanyWorkOrderPersonnelReleaseFacts>? Releases {get;init;}
    public void Require(CompanyScope company,string actor,CompanyWorkOrderExecutionPermitReference permit)
    {
        permit.Require();FieldSelectionReviewValidator.RequireReview(Context,company,actor);
        if(Permit!=permit || Context.FieldId!=permit.Task.FieldId || Assignments is null || Assignments.Count>4096 || ComparedAtUtc==default || ComparedAtUtc.Kind!=DateTimeKind.Utc ||
            Assignments.Select(row=>row.AssignmentId).Distinct(StringComparer.Ordinal).Count()!=Assignments.Count)throw new InvalidOperationException("Complete bounded original permit personnel roster is unavailable.");
        foreach(var row in Assignments){row.Require();if(row.AssignedAtUtc>ComparedAtUtc)throw new InvalidOperationException("Public assignment audit is later than its current comparison.");if(row.Company!=company || row.Database!=Context.Database || row.Permit!=permit)throw new UnauthorizedAccessException("Public assignment belongs to another company/native binding/permit/task.");}
        if(Releases is not null)
        {
            if(Releases.Count>Assignments.Count || Releases.Select(row=>row.AssignmentId).Distinct(StringComparer.Ordinal).Count()!=Releases.Count)throw new InvalidOperationException("Complete public cancellation population is ambiguous.");
            foreach(var release in Releases){release.Require(Assignments.Single(row=>row.AssignmentId==release.AssignmentId));if(release.ReleasedAtUtc>ComparedAtUtc)throw new InvalidOperationException("Cancellation audit is later than the current comparison.");}
        }
        if(Revision!=Digest(Context,Permit,Assignments,ComparedAtUtc,Releases))throw new InvalidOperationException("Complete public assignment roster changed.");
    }
    private static string Digest(FieldSelectionReview context,CompanyWorkOrderExecutionPermitReference permit,IReadOnlyList<CompanyWorkOrderPersonnelAssignmentFacts> rows,DateTime utc,IReadOnlyList<CompanyWorkOrderPersonnelReleaseFacts>? releases)
        =>ProcessDefinitionRevision.FromJson(releases is null?JsonSerializer.Serialize(new{Context=context,Permit=permit,Assignments=rows.OrderBy(row=>row.AssignmentId,StringComparer.Ordinal).ToArray(),ComparedAtUtc=utc}):
            JsonSerializer.Serialize(new{Context=context,Permit=permit,Assignments=rows.OrderBy(row=>row.AssignmentId,StringComparer.Ordinal).ToArray(),ComparedAtUtc=utc,Releases=releases.OrderBy(row=>row.AssignmentId,StringComparer.Ordinal).ToArray()}));
    public static CompanyWorkOrderPersonnelAssignmentRoster Create(FieldSelectionReview context,CompanyWorkOrderExecutionPermitReference permit,IReadOnlyList<CompanyWorkOrderPersonnelAssignmentFacts> rows,DateTime utc,IReadOnlyList<CompanyWorkOrderPersonnelReleaseFacts>? releases=null)
    {
        var result=new CompanyWorkOrderPersonnelAssignmentRoster(context,permit,rows,utc,Digest(context,permit,rows,utc,releases)){Releases=releases};result.Require(context.Company,context.ActorUserId,permit);return result;
    }
}
public sealed record CompanyWorkOrderAssignedPersonnelQuery(CompanyWorkOrderExecutionPermitReference Permit,string AssignmentId)
{
    public void Require(){if(Permit is null || string.IsNullOrWhiteSpace(AssignmentId) || !AssignmentId.StartsWith("wo-personnel-",StringComparison.Ordinal) || !ProcessDefinitionRevision.IsValid(AssignmentId["wo-personnel-".Length..]))throw new ArgumentException("Select the exact original permit and native personnel assignment.");Permit.Require();}
}
public sealed record CompanyWorkOrderPersonnelAssignmentRosterReview(CompanyWorkOrderExecutionPermitApprovalReview PermitReview,CompanyWorkOrderPersonnelAssignmentRoster Roster)
{
    public void Require(CompanyScope company,string actor,CompanyWorkOrderExecutionPermitReference permit)
    {
        if(PermitReview is null || Roster is null)throw new InvalidOperationException("Complete current permit and public personnel roster are unavailable.");
        PermitReview.Require(company,actor,permit);Roster.Require(company,actor,permit);
        if(PermitReview.Context!=Roster.Context)throw new InvalidOperationException("Original personnel roster/permit authority or native binding differs.");
    }
}
public sealed record CompanyWorkOrderAssignedPersonnelReview(CompanyWorkOrderAssignedPersonnelQuery Query,CompanyWorkOrderPersonnelAssignmentRoster Roster,
    CompanyWorkOrderPersonnelSourceReview CurrentPersonnel,IReadOnlyList<string> Blockers)
{
    [System.Text.Json.Serialization.JsonIgnore] public CompanyWorkOrderPersonnelAssignmentFacts Assignment=>Roster.Assignments.Single(row=>row.AssignmentId==Query.AssignmentId);
    public static string[] GetBlockers(CompanyWorkOrderPersonnelAssignmentFacts assignment,CompanyWorkOrderPersonnelSourceReview current,IReadOnlyList<CompanyWorkOrderPersonnelReleaseFacts>? releases=null)
    {
        assignment.Require();var result=new List<string>(current.Blockers);var permit=current.PermitReview;var source=current.Personnel;
        if(releases is null)result.Add("Complete current personnel cancellation evidence is unavailable; active duty is unconfirmed.");
        else if(releases.SingleOrDefault(row=>row.AssignmentId==assignment.AssignmentId) is {} release){release.Require(assignment);result.Add("The original personnel duty assignment was cancelled; historical evidence does not reserve current duty.");}
        if(source.ComparedAtUtc<assignment.ValidFromUtc)result.Add("The original assignment window has not started.");
        if(source.ComparedAtUtc>=assignment.ValidToUtc)result.Add("The original assignment window has expired.");
        if(assignment.ValidFromUtc>permit.Proposal.ValidFromUtc || assignment.ValidToUtc<permit.Proposal.ValidToUtc)result.Add("The assignment does not cover the complete current permit window.");
        if(source.Employment?.Revision!=assignment.EmploymentRevision)result.Add("Recorded company employment evidence changed since assignment.");
        if(source.License?.Revision!=assignment.LicenseRevision)result.Add("Recorded personnel licence evidence changed since assignment.");
        if(permit.RecordedRevision!=assignment.PermitRevision || permit.CurrentSource.FactsRevision()!=assignment.OrderInspectionFactsRevision)result.Add("Original permit/order/inspection facts changed since assignment; independent re-review is required.");
        if(assignment.AssignedBy==source.Context.ActorUserId)result.Add("The current reviewer is the assignment maker; independent personnel acceptance requires another reviewer.");
        result.Add("Configured duty/competency/jurisdiction requirements and independent PERSONNEL acceptance are still required before qualification or permit approval.");
        return result.ToArray();
    }
    public void Require(CompanyScope company,string actor,CompanyWorkOrderAssignedPersonnelQuery query)
    {
        query.Require();if(Query!=query || Roster is null || CurrentPersonnel is null || Blockers is null)throw new InvalidOperationException("Complete current assigned-personnel comparison is unavailable.");
        Roster.Require(company,actor,query.Permit);var assignment=Assignment;
        CurrentPersonnel.Require(company,actor,new(query.Permit,assignment.EmployeeBaId,assignment.LicenseId));
        if(CurrentPersonnel.Personnel.Context!=Roster.Context || !Blockers.SequenceEqual(GetBlockers(assignment,CurrentPersonnel,Roster.Releases),StringComparer.Ordinal))throw new InvalidOperationException("Original assignment/current personnel/binding/authority or blockers changed.");
    }
}
public interface ICompanyWorkOrderPersonnelAssignmentReads
{
    Task<CompanyWorkOrderPersonnelAssignmentRoster> ReadPersonnelAssignmentsAsync(FieldSelectionReview context,CompanyWorkOrderExecutionPermitReference permit,Func<Task> requireAuthority);
}
