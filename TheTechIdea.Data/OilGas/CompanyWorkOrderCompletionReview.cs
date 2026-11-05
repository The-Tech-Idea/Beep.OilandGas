using System.Text.Json;
using Beep.OilandGas.Models.Data.WorkOrder;
namespace TheTechIdea.Data.OilGas;

public sealed record CompanyWorkOrderCompletionQuery(CompanyMaintenanceReceivingQuery Task,string SubmissionId)
{
    public void Require(){if(Task is null || !LeaseSummaryValidator.Identifier(SubmissionId) || SubmissionId.Length>128)throw new ArgumentException("Original company work-order task and recorded submission identity are required.");Task.Require();}
}
/// <summary>Authorized reviewer work-report facts; retained command, rationale and recovery intent remain private.</summary>
public sealed record CompanyWorkOrderSubmissionFacts(CompanyScope Company,AssetDatabaseScope Database,CompanyWorkOrderCompletionQuery Query,
    string StartId,string ExecutionStepId,string ExecutionRole,string ExecutorUserId,string EmployeeBaId,DateTime CompletedAtUtc,string WorkPerformed,IReadOnlyList<string> OutstandingDefects,
    int RequirementEvidenceVersion,string SubmittedInspectionRevision,string OriginalRequirementPopulation,string OriginalOrderRevision,DateTime RecordedAtUtc,string Revision)
{
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public CompanyMaintenanceQualifiedAssets? QualifiedWorkAssets { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? WorkStartedAtUtc { get; init; }
    public void Require()
    {
        PurchaseOrderOwnership.RequireScope(Company);Query.Require();
        if(QualifiedWorkAssets is {} assets)
        {
            if(WorkStartedAtUtc is not {} started || started>CompletedAtUtc)
                throw new InvalidOperationException("Original qualified report start must precede completion.");
            CompanyMaintenanceQualifiedWorkScope.RequireRecorded(assets,Company,Query.Task,ExecutorUserId,started);
            if(assets.Context.Database!=Database || assets.OrderRevision!=OriginalOrderRevision)
                throw new InvalidOperationException("Original qualified report scope must bind its recorded CORE/order.");
        }
        else if(WorkStartedAtUtc is not null)throw new InvalidOperationException("Qualified original start UTC cannot extend an earlier report without its original snapshot.");
        if(Database is null || !LeaseSummaryValidator.Identifier(Database.ConnectionName) || !LeaseSummaryValidator.Identifier(Database.Fingerprint) ||
            new[]{StartId,ExecutionStepId,ExecutionRole,ExecutorUserId,EmployeeBaId}.Any(value=>!LeaseSummaryValidator.Identifier(value) || value.Length>128) || CompletedAtUtc.Kind!=DateTimeKind.Utc || CompletedAtUtc==default ||
            RecordedAtUtc.Kind!=DateTimeKind.Utc || RecordedAtUtc<CompletedAtUtc || !Text(WorkPerformed,16384) || OutstandingDefects is null || OutstandingDefects.Count>64 ||
            OutstandingDefects.Any(value=>!Text(value,2048)) || OutstandingDefects.Distinct(StringComparer.Ordinal).Count()!=OutstandingDefects.Count || RequirementEvidenceVersion is <0 or >2 ||
            !ProcessDefinitionRevision.IsValid(SubmittedInspectionRevision) || !ProcessDefinitionRevision.IsValid(OriginalRequirementPopulation) || !ProcessDefinitionRevision.IsValid(OriginalOrderRevision) || !ProcessDefinitionRevision.IsValid(Revision))
            throw new InvalidOperationException("Original recorded report, explicit work/defects, source scope and UTC audit are required.");
    }
    private static bool Text(string? value,int limit)=>!string.IsNullOrWhiteSpace(value) && value.Length<=limit && !value.Any(ch=>char.IsControl(ch) && ch is not ('\r' or '\n' or '\t'));
    public static string SubmissionInspections(IReadOnlyList<InspectionCondition> rows)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(rows.Where(row=>row.ApplicableStage=="WORK_SUBMISSION").OrderBy(row=>row.CondSeq).ToArray()));
    public static CompanyWorkOrderSubmissionFacts From(WORK_ORDER_EXECUTION_SUBMISSION row)
    {
        var command=CompanyWorkOrderSubmissionCommand.Read(row);var source=command.Review.Inspections;var context=source.Current.Context;
        var result=new CompanyWorkOrderSubmissionFacts(context.Company,context.Database,new(source.Start.Permit.Task,row.SUBMISSION_ID),row.START_ID,source.Start.StepId,source.Start.AssignedRole,row.EXECUTOR_USER_ID,row.EMPLOYEE_BA_ID,
            row.COMPLETED_AT_UTC,command.WorkPerformed,command.OutstandingDefects.ToArray(),source.Current.Approval.RequirementEvidenceVersion,SubmissionInspections(source.Current.Requirements),source.Current.OriginalRequirementPopulation,
            source.Current.OriginalOrderRevision,row.ROW_CREATED_DATE!.Value,row.RECORDED_REVISION);
        if(source.Start.QualifiedWorkAssets is {} assets)result=result with{QualifiedWorkAssets=assets,WorkStartedAtUtc=source.Start.StartedAtUtc};
        result.Require();return result;
    }
}
public sealed record CompanyWorkOrderCompletionReview(FieldSelectionReview Context,CompanyWorkOrderSubmissionFacts Report,WorkOrderDetailModel Order,
    IReadOnlyList<InspectionCondition> Requirements,CompanyPersonnelIdentityCatalog Identities,CompanyNativePersonFacts? ReviewerPerson,DateTime ComparedAtUtc,IReadOnlyList<string> Blockers)
{
    public const string PendingVerification="This evidence comparison does not establish a recorded independent verification decision.";
    public static string[] GetBlockers(FieldSelectionReview context,CompanyWorkOrderSubmissionFacts report,WorkOrderDetailModel order,IReadOnlyList<InspectionCondition> requirements,
        CompanyPersonnelIdentityCatalog identities,CompanyNativePersonFacts? reviewer,DateTime utc)
    {
        report.Require();FieldSelectionReviewValidator.RequireReview(context,context.Company,context.ActorUserId);identities.Require();var task=report.Query.Task;
        if(report.Company!=context.Company || report.Database!=context.Database || task.FieldId!=context.FieldId || order.Source!="WORK_ORDER" || order.InstanceId!=task.WorkOrderId || order.FieldId!=task.FieldId ||
            identities.Context.Company!=context.Company || identities.Context.Database!=context.Database || identities.Context.ActorUserId!=context.ActorUserId || utc.Kind!=DateTimeKind.Utc || utc<report.RecordedAtUtc || utc<identities.ComparedAtUtc)
            throw new InvalidOperationException("Original company/account/order/CORE identity and current UTC comparison differ.");
        WorkOrderInspectionRules.RequireProjected(task.FieldId,task.WorkOrderId,requirements);var result=new List<string>();
        if(context.ActorUserId==report.ExecutorUserId)result.Add("The executor cannot independently verify their own reported work.");
        if(report.OriginalOrderRevision!=CompanyWorkOrderExecutionRequirements.OrderRevision(order))result.Add("Original submitted work scope or component population changed.");
        if(report.OriginalRequirementPopulation!=CompanyWorkOrderExecutionRequirements.Population(task.FieldId,task.WorkOrderId,requirements))result.Add("Original approved requirement population or applicability changed.");
        if(report.SubmittedInspectionRevision!=CompanyWorkOrderSubmissionFacts.SubmissionInspections(requirements))result.Add("Original work-submission inspection evidence changed after the report.");
        if(report.RequirementEvidenceVersion!=2)result.Add("Original report has no explicit approved completion-verification applicability.");
        else WorkOrderInspectionApplicability.RequireComplete(requirements);
        var checks=requirements.Where(row=>row.ApplicableStage=="COMPLETION_VERIFICATION").ToArray();
        if(checks.Length==0)result.Add("No explicitly approved completion-verification inspections are configured.");
        if(checks.Any(row=>row.Status!="PASS"))result.Add("Current completion-verification inspections are not accepted.");
        if(checks.Any(row=>row.Status=="PASS" && (row.InspectDate is not {} at || at.Kind!=DateTimeKind.Utc || at<report.CompletedAtUtc || at>utc)))result.Add("Completion inspection UTC must follow reported completion and precede the current comparison.");
        if(checks.Any(row=>row.Status=="PASS" && row.Inspector==report.ExecutorUserId))result.Add("Completion inspection was recorded by the original executor.");
        foreach(var check in checks.Where(row=>row.Status=="PASS" && row.InspectDate?.Kind==DateTimeKind.Utc))
        {
            var at=check.InspectDate!.Value;var inspectorLinks=identities.Versions.Where(row=>row.UserId==check.Inspector && row.EffectiveFromUtc<=at && row.EffectiveToUtc>at).ToArray();
            if(inspectorLinks.Length!=1 || inspectorLinks[0].RecordedBy==check.Inspector)result.Add($"Completion inspector identity for {check.InspectionId} is unconfirmed at the recorded inspection UTC.");
            else if(inspectorLinks[0].PersonBaId==report.EmployeeBaId)result.Add($"Completion inspector for {check.InspectionId} is the original executor's native person.");
        }
        if(report.OutstandingDefects.Count>0)result.Add("The reported work contains unresolved outstanding defects.");
        var links=identities.Versions.Where(row=>row.UserId==context.ActorUserId && row.EffectiveFromUtc<=utc && row.EffectiveToUtc>utc).ToArray();
        if(links.Length!=1 || reviewer is null)result.Add("One authoritative current reviewer account/native person identity is required.");
        else
        {
            var link=links[0];reviewer.Require(context.Company,link.PersonBaId);
            if(link.RecordedBy==context.ActorUserId || link.PersonType!=reviewer.Type || !Covers(reviewer.EffectiveFromUtc,reviewer.EffectiveToUtc) || !Covers(reviewer.Employment.EffectiveDate,reviewer.Employment.ExpiryDate))
                result.Add("Current independently recorded reviewer identity/person/employment evidence is not established.");
            if(reviewer.PersonBaId==report.EmployeeBaId)result.Add("Reviewer and original executor are the same native person.");
        }
        result.Add(PendingVerification);return result.ToArray();
        bool Covers(DateTime? start,DateTime? end)=>start is {} from && end is {} to && from.Kind==DateTimeKind.Utc && to.Kind==DateTimeKind.Utc && from<=utc && to>utc;
    }
    public void Require()
    {
        if(Context is null || Report is null || Order is null || Requirements is null || Identities is null || Blockers is null ||
            !GetBlockers(Context,Report,Order,Requirements,Identities,ReviewerPerson,ComparedAtUtc).SequenceEqual(Blockers,StringComparer.Ordinal))throw new InvalidOperationException("Complete recorded report/current completion/independence evidence differs.");
    }
    public string SourceRevision(){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Context,Report,Order=CompanyWorkOrderExecutionRequirements.OrderRevision(Order),
        Requirements=Requirements.OrderBy(row=>row.CondSeq),Identities=Identities.Revision,ReviewerPerson,Blockers}));}
    public string WorkSubjectRevision()
    {
        Require();var checks=Requirements.Where(row=>row.ApplicableStage=="COMPLETION_VERIFICATION" && row.Status=="PASS").OrderBy(row=>row.CondSeq).ToArray();
        return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Report,Order=CompanyWorkOrderExecutionRequirements.OrderRevision(Order),
            Population=CompanyWorkOrderExecutionRequirements.Population(Report.Query.Task.FieldId,Report.Query.Task.WorkOrderId,Requirements),
            Results=Requirements.Where(row=>row.ApplicableStage!="RETURN_TO_SERVICE").OrderBy(row=>row.CondSeq),
            InspectorIdentities=checks.Select(row=>new{row.InspectionId,Versions=Identities.Versions.Where(link=>link.UserId==row.Inspector && row.InspectDate is {} at && link.EffectiveFromUtc<=at && link.EffectiveToUtc>at).OrderBy(link=>link.RecordId,StringComparer.Ordinal)})}));
    }
}
