using System.Text.Json;
namespace TheTechIdea.Data.OilGas;

/// <summary>One validation and snapshot owner for the recorded persona queue consumed by clients and view state.</summary>
public static class WorkflowInboxReview
{
    public static UnifiedInbox Capture(UnifiedInbox result, CompanyScope company, string actor, string persona, IReadOnlyCollection<string> roles)
    {
        ArgumentNullException.ThrowIfNull(result);
        var frozen = JsonSerializer.Deserialize<UnifiedInbox>(JsonSerializer.Serialize(result))!;
        if (frozen.Company != company || frozen.ActorUserId != actor || frozen.PersonaCode != persona ||
            frozen.CriticalTasks is null || frozen.HighPriorityTasks is null || frozen.NormalTasks is null || frozen.Counts is null)
            throw new InvalidOperationException("The assigned-work response does not match the current company workspace.");
        var rows = frozen.CriticalTasks.Concat(frozen.HighPriorityTasks).Concat(frozen.NormalTasks).ToArray();
        foreach (var row in rows) RequireTask(row, roles);
        foreach(var row in rows.Where(row=>row.AccountingStepReview is not null))
            row.AccountingStepReview!.Require(company,actor,row.WorkflowName);
        if (rows.Select(row => row.TaskId).Distinct(StringComparer.Ordinal).Count() != rows.Length ||
            frozen.CriticalTasks.Any(row => row.Priority != 1) || frozen.HighPriorityTasks.Any(row => row.Priority != 2) || frozen.NormalTasks.Any(row => row.Priority <= 2) ||
            frozen.Counts.TotalPending != rows.Length || frozen.Counts.Critical != frozen.CriticalTasks.Count || frozen.Counts.Overdue != rows.Count(row => row.SlaStatus == "BREACHED") ||
            frozen.Counts.Approvals != rows.Count(row => row.TaskType == "APPROVAL") || frozen.Counts.Reviews != rows.Count(row => row.TaskType == "REVIEW") || frozen.Counts.DataEntry != rows.Count(row => row.TaskType == "DATA_ENTRY"))
            throw new InvalidOperationException("Assigned work and its counts could not be confirmed. Reload the inbox.");
        return frozen;
    }
    public static void RequireTask(UnifiedTask? row, IReadOnlyCollection<string> roles)
    {
        if(row is not null && (IsCompletionReview(row)?!QualifiedCompletionTask(row):row.CompletionReviewRevision is not null))
            throw new InvalidOperationException("Completion review requires its qualified individual source and task reader before inbox projection.");
        if(row?.EntityType=="DRILLING_OPERATION" && (row.ScopeKind!="FIELD" || row.TaskType!="DATA_ENTRY" || !ProcessDefinitionRevision.IsValid(row.ExecutionAssignmentRevision)) ||
            row is not null && row.EntityType!="DRILLING_OPERATION" && row.ExecutionAssignmentRevision is not null)
            throw new InvalidOperationException("Assigned execution reporting requires its qualified original source and task revision.");
        if(row?.EntityType=="DRILLING_PROGRAM" && (row.ScopeKind!="FIELD" || !ProcessDefinitionRevision.IsValid(row.ProgramReviewRevision)) || row is not null && row.EntityType!="DRILLING_PROGRAM" && row.ProgramReviewRevision is not null)
            throw new InvalidOperationException("Assigned program work requires its qualified independent source and task revision.");
        if(row?.EntityType=="AFE" && (row.ScopeKind!="FIELD" || !ProcessDefinitionRevision.IsValid(row.InvestmentReviewRevision)) ||
            row is not null && row.EntityType!="AFE" && row.InvestmentReviewRevision is not null)
            throw new InvalidOperationException("Assigned investment work requires its qualified individual source and task revision.");
        if(row?.EntityType=="FIELD_DEVELOPMENT_PLAN" && (row.ScopeKind!="FIELD" || !ProcessDefinitionRevision.IsValid(row.DevelopmentReviewRevision)) ||
            row is not null && row.EntityType!="FIELD_DEVELOPMENT_PLAN" && row.DevelopmentReviewRevision is not null)
            throw new InvalidOperationException("Assigned development work requires its qualified source and task revision.");
        if(row?.AccountingStepReview is {} evidence)
        {
            evidence.Require(evidence.Company,evidence.ActorUserId,row.WorkflowName);
            if(row.ScopeKind!="COMPANY_ACCOUNTING" || row.EntityType is not ("AP_INVOICE" or "SALES_CONTRACT" or "RUN_TICKET" or "INVOICE" or "AR_INVOICE" or "JOURNAL_ENTRY" or "AR_PAYMENT") || evidence.AccountingEvidenceRevision is null ||
                (evidence.RequiredPermission=="Workflow.Approve")!=evidence.RequiresIndependentActor ||
                evidence.RequiredPermission=="Workflow.Execute" && evidence.Eligible && (!CompanyAccountingSourceTypes.IsNativeDecision(row.EntityType) || evidence.ActorUserId!=evidence.SourceMakerUserId && !(row.EntityType=="JOURNAL_ENTRY" && evidence.AccountingEvidenceAction==CompanyAccountingSourceTypes.JournalPosting)) ||
                evidence.EntityId!=row.EntityId || evidence.EntityType!=row.EntityType || evidence.StepInstanceId!=row.StepName || evidence.AssignedRole!=row.AssignedRole ||
                evidence.StepName!=row.StepDisplayName)
                throw new InvalidOperationException("The configured accounting task evidence differs from its recorded assignment.");
        }
        if (row is not null && (row.SlaStatus is not ("UNSCHEDULED" or "UNCONFIRMED" or "BREACHED" or "ON_TRACK") ||
            row.SlaStatus == "UNSCHEDULED" && row.DueDate is not null ||
            row.SlaStatus == "UNCONFIRMED" && (row.DueDate is null || row.DueDate.Value.Kind == DateTimeKind.Utc) ||
            row.SlaStatus is "BREACHED" or "ON_TRACK" && (row.DueDate is null || row.DueDate.Value.Kind != DateTimeKind.Utc) ||
            row.EntityType == "DRILLING_OPERATION" && row.SlaStatus != "UNSCHEDULED"))
            throw new InvalidOperationException("The task due time or schedule status is unconfirmed.");
        if (row is null || string.IsNullOrWhiteSpace(row.TaskId) || row.Status != "PENDING" || row.Priority is < 1 or > 4 ||
            string.IsNullOrWhiteSpace(row.WorkflowName) || string.IsNullOrWhiteSpace(row.StepName) || string.IsNullOrWhiteSpace(row.EntityId) || string.IsNullOrWhiteSpace(row.EntityType) ||
            string.IsNullOrWhiteSpace(row.AssignedRole) || !roles.Contains(row.AssignedRole, StringComparer.Ordinal) || !WorkflowTaskRoute.Matches(row) ||
            row.StepDisplayName is {} name && (string.IsNullOrWhiteSpace(name) || name.Length>512 || name.Any(char.IsControl)) ||
            row.CreatedByUserId is {} creator && (string.IsNullOrWhiteSpace(creator) || creator.Length>128 || creator.Any(char.IsControl)))
            throw new InvalidOperationException("Assigned work and its route or role could not be confirmed. Reload the inbox.");
    }
    private static bool IsCompletionReview(UnifiedTask row)=>row.EntityType=="DRILLING_DAILY_REPORT" || row.WorkflowName?.StartsWith(DrillingCompletionReviewStart.InstancePrefix,StringComparison.Ordinal)==true;
    private static bool QualifiedCompletionTask(UnifiedTask row)=>row.EntityType=="DRILLING_DAILY_REPORT" && row.ScopeKind=="FIELD" && row.TaskType=="APPROVAL" && row.WorkflowName?.StartsWith(DrillingCompletionReviewStart.InstancePrefix,StringComparison.Ordinal)==true && ProcessDefinitionRevision.IsValid(row.CompletionReviewRevision);
}
