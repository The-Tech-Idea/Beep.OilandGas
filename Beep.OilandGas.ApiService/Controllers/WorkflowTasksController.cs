using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;

namespace Beep.OilandGas.ApiService.Controllers;

[ApiController]
[Authorize]
[Route("api/workflow/tasks")]
public sealed class WorkflowTasksController(RepositoryDbContext repository, ICrossPersonaTaskRouter tasks, RepositoryTenantContext tenant, IRepositoryAccessService access,
    RepositoryPersonaService personas, IAccessControlService resources, IProcessService processes, CompanyAccountingProcessReads accountingProcesses,
    IDevelopmentAssignedReviewReader developmentReviews, IDevelopmentInvestmentAssignedReader investmentReviews, IDrillingProgramAssignedReviewReader programReviews, IFailureReporter failures) : ControllerBase
{
    private string _reviewedPersona = "";
    private bool _readConflict;
    private IActionResult ReadFailure() => _readConflict
        ? Conflict(new { error = "Assigned work ownership, assignment or source evidence changed or is unavailable. Reload company access and the inbox." })
        : Forbid();

    [HttpGet("inbox")]
    public async Task<IActionResult> Inbox(string? personaCode, CancellationToken token)
    {
        var visible = await ReviewedTasksAsync(personaCode, token);
        if (visible is null) return ReadFailure();
        return Ok(new UnifiedInbox { Company = tenant.Company!.Scope, ActorUserId = tenant.Company.UserId, PersonaCode = _reviewedPersona, CriticalTasks = visible.Where(x => x.Priority <= 1).ToList(),
            HighPriorityTasks = visible.Where(x => x.Priority == 2).ToList(),
            NormalTasks = visible.Where(x => x.Priority > 2).ToList(), Counts = Count(visible) });
    }

    [HttpGet("counts")]
    public async Task<IActionResult> Counts(string? personaCode, CancellationToken token)
    {
        var visible = await ReviewedTasksAsync(personaCode, token);
        return visible is null ? ReadFailure() : Ok(Count(visible));
    }

    [HttpGet]
    public async Task<IActionResult> List(string? personaCode, string? taskType, int? minPriority,
        DateTime? dueBefore, string? workflow, string? sortBy, int page = 0, int pageSize = 20, CancellationToken token = default)
    {
        if (page < 0 || page > 100000 || pageSize is < 1 or > 100 || minPriority is < 1 or > 4 ||
            dueBefore is { Kind: not DateTimeKind.Utc } ||
            (sortBy is not null && sortBy != "priority" && sortBy != "dueDate" && sortBy != "createdDate")) return BadRequest();
        var visible = await ReviewedTasksAsync(personaCode, token);
        if (visible is null) return ReadFailure();
        IEnumerable<UnifiedTask> filtered = visible;
        if (!string.IsNullOrWhiteSpace(taskType)) filtered = filtered.Where(x => x.TaskType == taskType);
        if (minPriority.HasValue) filtered = filtered.Where(x => x.Priority <= minPriority.Value);
        if (dueBefore.HasValue) filtered = filtered.Where(x => x.DueDate is { Kind: DateTimeKind.Utc } due && due <= dueBefore.Value);
        if (!string.IsNullOrWhiteSpace(workflow)) filtered = filtered.Where(x => x.WorkflowName == workflow);
        filtered = sortBy switch {
            "dueDate" => filtered.OrderBy(x => x.DueDate is { Kind: DateTimeKind.Utc } due ? due.Ticks : long.MaxValue).ThenBy(x => x.TaskId, StringComparer.Ordinal),
            "createdDate" => filtered.OrderByDescending(x => x.CreatedDate.Kind == DateTimeKind.Utc ? x.CreatedDate.Ticks : -1).ThenBy(x => x.TaskId, StringComparer.Ordinal),
            _ => filtered.OrderBy(x => x.Priority).ThenBy(x => x.TaskId, StringComparer.Ordinal) };
        return Ok(filtered.Skip(page * pageSize).Take(pageSize).ToList());
    }

    private async Task<string> CurrentPersonaAsync(CancellationToken token)
    {
        var actor = User.ActingUserId(); var company = tenant.Company;
        if (company is null || company.UserId != actor) return "";
        return (await repository.Set<AppUserPersona>().AsNoTracking().SingleOrDefaultAsync(row => row.UserId == actor &&
            row.TenantId == company.Scope.TenantId && row.BusinessAssociateId == company.Scope.BusinessAssociateId, token))?.PersonaCode ?? "";
    }

    private async Task<List<UnifiedTask>?> ReviewedTasksAsync(string? persona, CancellationToken token)
    {
        try { return await VisibleTasksAsync(persona, token); }
        catch (UnauthorizedAccessException denied)
        {
            failures.ReportHandled(denied, "reading assigned company work", "workspace access is refused", FailureSeverity.Degraded);
            return null;
        }
        catch (InvalidOperationException changed)
        {
            failures.ReportHandled(changed, "reading assigned company work", "assigned work evidence changed or is unavailable", FailureSeverity.Degraded);
            _readConflict = true;
            return null;
        }
    }

    private async Task<List<UnifiedTask>?> VisibleTasksAsync(string? requestedPersona, CancellationToken token)
    {
        var userId = User.ActingUserId();
        if (tenant.Company is not { } company || company.UserId != userId) return null;
        var persona = await CurrentPersonaAsync(token);
        if (string.IsNullOrWhiteSpace(persona) || (!string.IsNullOrWhiteSpace(requestedPersona) && requestedPersona != persona)) return null;
        CompanyPersonaWorkspaceReview review;
        try { review = await personas.ReviewWorkspacesAsync(userId, company.Scope, access, token); }
        catch (UnauthorizedAccessException denied) { failures.ReportHandled(denied, "reading assigned company work", "workspace access is refused", FailureSeverity.Degraded); return null; }
        if (!review.Personas.Any(row => row.Code == persona)) return null;
        var roles = review.Roles.ToHashSet(StringComparer.Ordinal);
        var visible = new List<UnifiedTask>(); var now = DateTime.UtcNow;
        var originalTasks = new Dictionary<string, string>(StringComparer.Ordinal);
        var executionProcesses = new Dictionary<string,string>(StringComparer.Ordinal);
        var completionProcesses = new Dictionary<string,string>(StringComparer.Ordinal);
        var foreignProcesses = new Dictionary<string, (string Task,string Process,string Instance)>(StringComparer.Ordinal);
        var initialTasks=await tasks.GetTasksForPersonaAsync(persona);
        bool Candidate(CROSS_PERSONA_TASK task)=>task.TARGET_PERSONA_CODE==persona && task.TASK_STATUS=="PENDING" && roles.Contains(task.ASSIGNED_ROLE) && User.IsInRole(task.ASSIGNED_ROLE) && !string.IsNullOrWhiteSpace(task.PROCESS_INSTANCE_ID) && !string.IsNullOrWhiteSpace(task.ENTITY_ID) && !string.IsNullOrWhiteSpace(task.ENTITY_TYPE);
        string Candidates(IReadOnlyList<CROSS_PERSONA_TASK> rows)=>System.Text.Json.JsonSerializer.Serialize(rows.Where(Candidate).OrderBy(task=>task.CROSS_TASK_ID,StringComparer.Ordinal));
        var candidateSnapshot=Candidates(initialTasks);
        foreach (var task in initialTasks)
        {
            token.ThrowIfCancellationRequested();
            if (task.TARGET_PERSONA_CODE != persona || task.TASK_STATUS != "PENDING" || !roles.Contains(task.ASSIGNED_ROLE) || !User.IsInRole(task.ASSIGNED_ROLE) ||
                string.IsNullOrWhiteSpace(task.PROCESS_INSTANCE_ID) || string.IsNullOrWhiteSpace(task.ENTITY_ID) || string.IsNullOrWhiteSpace(task.ENTITY_TYPE)) continue;
            var process = await processes.GetProcessInstanceAsync(task.PROCESS_INSTANCE_ID);
            if (process is null) continue;
            var investment=process.EntityType=="AFE" || process.InstanceId.StartsWith(DevelopmentInvestmentHandoff.InstancePrefix,StringComparison.Ordinal) || process.ProcessData?.Data?.ContainsKey(DevelopmentInvestmentHandoff.PayloadKey)==true;
            if(investment && (task.ENTITY_TYPE!="AFE" || process.EntityType!="AFE"))continue;
            var program=process.EntityType=="DRILLING_PROGRAM" || process.InstanceId.StartsWith(DrillingProgramReviewStart.InstancePrefix,StringComparison.Ordinal) || process.ProcessData?.Data?.ContainsKey(DrillingProgramReviewStart.PayloadKey)==true;
            var execution=process.EntityType=="DRILLING_OPERATION" || process.InstanceId.StartsWith(DrillingExecutionAssignment.InstancePrefix,StringComparison.Ordinal) || process.ProcessData?.Data?.ContainsKey(DrillingExecutionAssignment.PayloadKey)==true;
            var completion=process.EntityType=="DRILLING_DAILY_REPORT" || process.InstanceId.StartsWith(DrillingCompletionReviewStart.InstancePrefix,StringComparison.Ordinal) || process.ProcessData?.Data?.ContainsKey(DrillingCompletionReviewStart.PayloadKey)==true;
            if(!WorkflowTaskProcessAdmission.CompanyMatches(process,company.Scope))
            {
                if(!foreignProcesses.TryAdd(task.CROSS_TASK_ID,(System.Text.Json.JsonSerializer.Serialize(task),System.Text.Json.JsonSerializer.Serialize(process),task.PROCESS_INSTANCE_ID)))
                    throw new InvalidOperationException("The recorded foreign maintenance task identity is ambiguous.");
                continue;
            }
            if(completion && (task.ENTITY_TYPE!="DRILLING_DAILY_REPORT" || process.EntityType!="DRILLING_DAILY_REPORT"))throw new InvalidOperationException("The completion review task was retagged and requires reconciliation.");
            if(program && (task.ENTITY_TYPE!="DRILLING_PROGRAM" || process.EntityType!="DRILLING_PROGRAM"))throw new InvalidOperationException("The recorded program task source was retagged and requires reconciliation.");
            if(execution && (task.ENTITY_TYPE!="DRILLING_OPERATION" || process.EntityType!="DRILLING_OPERATION"))throw new InvalidOperationException("The recorded execution reporting task was retagged and requires reconciliation.");
            var scopeKind = string.IsNullOrWhiteSpace(process.FieldId) ? "COMPANY_ACCOUNTING" : "FIELD";
            if (scopeKind == "COMPANY_ACCOUNTING" && task.ENTITY_TYPE is not ("AP_INVOICE" or "PURCHASE_ORDER" or "SALES_CONTRACT" or "RUN_TICKET" or "INVOICE" or "AR_INVOICE" or "JOURNAL_ENTRY" or "AR_PAYMENT")) continue;
            if(scopeKind=="COMPANY_ACCOUNTING")
            {
                if(process.InstanceId!=task.PROCESS_INSTANCE_ID || process.EntityType!=task.ENTITY_TYPE || process.EntityId!=task.ENTITY_ID)
                    throw new InvalidOperationException("The recorded task and company process source disagree.");
                var owner=CompanyAccountingProcessBinding.FromPayloadJson(System.Text.Json.JsonSerializer.Serialize(process.ProcessData),task.PROCESS_INSTANCE_ID,task.ENTITY_TYPE,task.ENTITY_ID);
                if(owner.Company!=company.Scope)
                {
                    if(!foreignProcesses.TryAdd(task.CROSS_TASK_ID,(System.Text.Json.JsonSerializer.Serialize(task),System.Text.Json.JsonSerializer.Serialize(process),task.PROCESS_INSTANCE_ID)))
                        throw new InvalidOperationException("The recorded foreign task identity is ambiguous.");
                    continue;
                }
            }
            var row = new UnifiedTask { TaskId = task.CROSS_TASK_ID, TaskType = task.TASK_TYPE,
                WorkflowName = task.PROCESS_INSTANCE_ID, StepName = task.PROCESS_STEP_INSTANCE_ID,
                ScopeKind = scopeKind, AssignedRole = task.ASSIGNED_ROLE,
                EntityType = task.ENTITY_TYPE, EntityId = task.ENTITY_ID, EntityDescription = task.ENTITY_DESCRIPTION ?? "",
                Priority = task.PRIORITY, DueDate = task.ENTITY_TYPE == "DRILLING_OPERATION" ? null : task.DUE_DATE, CreatedDate = task.ROW_CREATED_DATE ?? DateTime.MinValue,
                CreatedByUserId = string.IsNullOrWhiteSpace(task.ROW_CREATED_BY) ? null : task.ROW_CREATED_BY,
                SlaStatus = WorkflowTaskSchedule.Classify(task.ENTITY_TYPE, task.DUE_DATE, now), Status = task.TASK_STATUS,
                Route = WorkflowTaskRoute.For(scopeKind, task.PROCESS_INSTANCE_ID, task.ENTITY_TYPE) };
            if (scopeKind == "COMPANY_ACCOUNTING")
            {
                try {
                    if(row.EntityType is "JOURNAL_ENTRY" or "AR_PAYMENT"){var captured=row.EntityType=="AR_PAYMENT"?await accountingProcesses.ReadAssignedPaymentTaskAsync(User,row,company.Scope,userId):await accountingProcesses.ReadAssignedJournalTaskAsync(User,row,company.Scope,userId);process=captured.Process;row.AccountingStepReview=captured.Review;}
                    else process = await accountingProcesses.ReadAsync(User, row.WorkflowName, company.Scope, userId);
                }
                catch (UnauthorizedAccessException) { continue; }
            }
            else if(row.EntityType=="AFE")
            {
                try
                {
                    var assigned=await investmentReviews.ReadAssignedAsync(User,row.WorkflowName);assigned.Require(company.Scope,userId,row.WorkflowName);
                    if(assigned.ProcessRevision!=ProcessDefinitionRevision.FromJson(System.Text.Json.JsonSerializer.Serialize(process)) ||
                        assigned.TaskRevision!=ProcessDefinitionRevision.FromJson(System.Text.Json.JsonSerializer.Serialize(task)) || assigned.TaskId!=row.TaskId ||
                        assigned.StepInstanceId!=row.StepName || assigned.AssignedRole!=row.AssignedRole || assigned.Source.AfeId!=row.EntityId || assigned.PersonaCode!=persona)
                        throw new InvalidOperationException("The qualified investment source or assignment differs from its inbox candidate.");
                    row.InvestmentReviewRevision=assigned.Revision;row.EntityDescription=assigned.Source.Afe().Name;
                }
                catch(UnauthorizedAccessException){continue;}
            }
            else if(row.EntityType=="DRILLING_DAILY_REPORT")
            {
                var original=DrillingCompletionReviewStart.FromPayload(System.Text.Json.JsonSerializer.Serialize(process.ProcessData),row.WorkflowName,process.FieldId,row.EntityId);
                var matches=original.Workflow.Stages.Where(stage=>DrillingCompletionReviewStartReceipt.StepId(original.InstanceId(),stage.Definition.StepId)==row.StepName).ToArray();
                if(matches.Length!=1)throw new InvalidOperationException("The original completion task stage is unqualified.");
                var first=matches[0];
                if(first.Definition.AssignedRole!=row.AssignedRole || !first.Authority.Recipients.Any(recipient=>recipient.UserId==userId && recipient.PersonaCode==persona))continue;
                if(!completionProcesses.TryAdd(row.TaskId,System.Text.Json.JsonSerializer.Serialize(process)))throw new InvalidOperationException("The completion task candidate identity is ambiguous.");
            }
            else if(row.EntityType=="DRILLING_OPERATION")
            {
                // Candidate filtering only. Complete native/source admission runs once in the closing pass,
                // against these exact process/task snapshots, before any row or count is returned.
                var responsibility=Beep.OilandGas.LifeCycle.Services.Processes.PPDMProcessService.CurrentReportingGeneration(process);
                if(responsibility.Workflow.Step.AssignedRole!=row.AssignedRole || responsibility.StepInstanceId!=row.StepName ||
                    !responsibility.Workflow.Authority.Recipients.Any(recipient=>recipient.UserId==userId && recipient.PersonaCode==persona))continue;
                if(!executionProcesses.TryAdd(row.TaskId,System.Text.Json.JsonSerializer.Serialize(process)))
                    throw new InvalidOperationException("The original execution assignment candidate identity is ambiguous.");
            }
            else if(row.EntityType=="DRILLING_PROGRAM")
            {
                try
                {
                    var assigned=await programReviews.ReadAssignedProgramAsync(User,row.WorkflowName);assigned.Require(company.Scope,userId,row.WorkflowName);
                    if(assigned.ProcessRevision!=ProcessDefinitionRevision.FromJson(System.Text.Json.JsonSerializer.Serialize(process)) || assigned.TaskRevision!=ProcessDefinitionRevision.FromJson(System.Text.Json.JsonSerializer.Serialize(task)) || assigned.TaskId!=row.TaskId || assigned.StepInstanceId!=row.StepName || assigned.AssignedRole!=row.AssignedRole || assigned.Source.Program.PROGRAM_ID!=row.EntityId || assigned.PersonaCode!=persona)throw new InvalidOperationException("The qualified program source or assignment differs from its inbox candidate.");
                    row.ProgramReviewRevision=assigned.Revision;row.EntityDescription=assigned.Source.Program.PROGRAM_NAME;
                }
                catch(UnauthorizedAccessException){continue;}
            }
            else if(row.EntityType=="FIELD_DEVELOPMENT_PLAN")
            {
                try
                {
                    var assigned=await developmentReviews.ReadAsync(User,row.WorkflowName);assigned.Require(company.Scope,userId,row.WorkflowName);
                    if(assigned.ProcessRevision!=ProcessDefinitionRevision.FromJson(System.Text.Json.JsonSerializer.Serialize(process)) ||
                        assigned.TaskRevision!=ProcessDefinitionRevision.FromJson(System.Text.Json.JsonSerializer.Serialize(task)) || assigned.TaskId!=row.TaskId ||
                        assigned.StepInstanceId!=row.StepName || assigned.AssignedRole!=row.AssignedRole || assigned.Plan.FDP_ID!=row.EntityId || assigned.PersonaCode!=persona)
                        throw new InvalidOperationException("The qualified development source or assignment differs from its inbox candidate.");
                    row.DevelopmentReviewRevision=assigned.Revision;
                    row.EntityDescription=assigned.Plan.FDP_NAME;
                }
                catch(UnauthorizedAccessException){continue;}
            }
            else if (!(await resources.CheckAssetAccessAsync(userId, process.FieldId, "FIELD", "READ")).HasAccess ||
                !(await resources.CheckAssetAccessAsync(userId, process.EntityId, process.EntityType, "READ")).HasAccess) continue;
            if (!WorkflowTaskProcessAdmission.Matches(row, process, userId)) continue;
            var stepName=process!.StepInstances.Single(step=>step.StepInstanceId==row.StepName).StepName;
            row.StepDisplayName=string.IsNullOrWhiteSpace(stepName)?null:stepName;
            if(scopeKind=="COMPANY_ACCOUNTING" && row.EntityType is not ("JOURNAL_ENTRY" or "AR_PAYMENT"))row.AccountingStepReview=await accountingProcesses.ReviewAssignedStepEvidenceAsync(User,row,process,company.Scope,userId);
            if(row.EntityType is not ("DRILLING_OPERATION" or "DRILLING_DAILY_REPORT"))WorkflowInboxReview.RequireTask(row,roles);
            if (!originalTasks.TryAdd(row.TaskId, System.Text.Json.JsonSerializer.Serialize(task)))
                throw new InvalidOperationException("Assigned task identities are ambiguous.");
            visible.Add(row);
        }
        var closing = await personas.ReviewWorkspacesAsync(userId, company.Scope, access, token);
        if (closing.Revision != review.Revision || await CurrentPersonaAsync(token) != persona || tenant.Company is not { } currentCompany || currentCompany.Scope != company.Scope || currentCompany.UserId != userId)
            throw new InvalidOperationException("Company workspace access changed while loading assigned work. Reload company access.");
        var closingTasks = await tasks.GetTasksForPersonaAsync(persona);
        if(Candidates(closingTasks)!=candidateSnapshot)throw new InvalidOperationException("The complete assigned-role task candidate set changed during inbox review.");
        foreach(var skipped in foreignProcesses)
        {
            var records=closingTasks.Where(task=>task.CROSS_TASK_ID==skipped.Key).ToArray();
            if(records.Length!=1 || System.Text.Json.JsonSerializer.Serialize(records[0])!=skipped.Value.Task ||
                System.Text.Json.JsonSerializer.Serialize(await processes.GetProcessInstanceAsync(skipped.Value.Instance))!=skipped.Value.Process)
                throw new InvalidOperationException("Skipped company task ownership changed during inbox review.");
        }
        foreach (var row in visible)
        {
            var records = closingTasks.Where(task => task.CROSS_TASK_ID == row.TaskId).ToArray();
            if (records.Length != 1 || System.Text.Json.JsonSerializer.Serialize(records[0]) != originalTasks[row.TaskId])
                throw new InvalidOperationException("Assigned work changed while loading. Reload the inbox.");
        }
        foreach (var row in visible)
        {
            token.ThrowIfCancellationRequested();
            if (!closing.Roles.Contains(row.AssignedRole, StringComparer.Ordinal) || !User.IsInRole(row.AssignedRole)) return null;
            Beep.OilandGas.Models.Processes.ProcessInstance? process;
            CompanyAccountingStepReview? journalReview=null;
            if (row.ScopeKind == "COMPANY_ACCOUNTING")
            {
                try {
                    if(row.EntityType is "JOURNAL_ENTRY" or "AR_PAYMENT"){var captured=row.EntityType=="AR_PAYMENT"?await accountingProcesses.ReadAssignedPaymentTaskAsync(User,row,company.Scope,userId):await accountingProcesses.ReadAssignedJournalTaskAsync(User,row,company.Scope,userId);process=captured.Process;journalReview=captured.Review;}
                    else process = await accountingProcesses.ReadAsync(User, row.WorkflowName, company.Scope, userId);
                }
                catch (UnauthorizedAccessException) { return null; }
            }
            else if(row.EntityType=="AFE")
            {
                try
                {
                    var assigned=await investmentReviews.ReadAssignedAsync(User,row.WorkflowName);assigned.Require(company.Scope,userId,row.WorkflowName);
                    process=await processes.GetProcessInstanceAsync(row.WorkflowName);
                    if(assigned.Revision!=row.InvestmentReviewRevision || assigned.ProcessRevision!=ProcessDefinitionRevision.FromJson(System.Text.Json.JsonSerializer.Serialize(process)))
                        throw new InvalidOperationException("The qualified investment source changed during inbox readback.");
                }
                catch(UnauthorizedAccessException){return null;}
            }
            else if(row.EntityType=="DRILLING_DAILY_REPORT")
            {
                try
                {
                    var assigned=await programReviews.ReadAssignedCompletionAsync(User,row.WorkflowName);assigned.Require(company.Scope,userId,row.WorkflowName);
                    process=await processes.GetProcessInstanceAsync(row.WorkflowName);var record=closingTasks.Single(task=>task.CROSS_TASK_ID==row.TaskId);
                    if(!completionProcesses.TryGetValue(row.TaskId,out var initialProcess) || initialProcess!=System.Text.Json.JsonSerializer.Serialize(process) ||
                        assigned.ProcessRevision!=ProcessDefinitionRevision.FromJson(initialProcess) || assigned.TaskRevision!=ProcessDefinitionRevision.FromJson(System.Text.Json.JsonSerializer.Serialize(record)) ||
                        assigned.TaskId!=row.TaskId || assigned.StepInstanceId!=row.StepName || assigned.AssignedRole!=row.AssignedRole || assigned.Original.ClaimId!=row.EntityId || assigned.PersonaCode!=persona)
                        throw new InvalidOperationException("The qualified completion source or original process/task candidate changed during inbox readback.");
                    row.CompletionReviewRevision=assigned.Revision;row.EntityDescription=assigned.Original.Description;WorkflowInboxReview.RequireTask(row,closing.Roles);
                }
                catch(UnauthorizedAccessException){return null;}
            }
            else if(row.EntityType=="DRILLING_OPERATION")
            {
                try
                {
                    var assigned=await programReviews.ReadAssignedExecutionAsync(User,row.WorkflowName);assigned.Require(company.Scope,userId,row.WorkflowName);
                    process=await processes.GetProcessInstanceAsync(row.WorkflowName);
                    var record=closingTasks.Single(task=>task.CROSS_TASK_ID==row.TaskId);
                    if(!executionProcesses.TryGetValue(row.TaskId,out var initialProcess) || initialProcess!=System.Text.Json.JsonSerializer.Serialize(process) ||
                        assigned.ProcessRevision!=ProcessDefinitionRevision.FromJson(initialProcess) || assigned.TaskRevision!=ProcessDefinitionRevision.FromJson(System.Text.Json.JsonSerializer.Serialize(record)) ||
                        assigned.TaskId!=row.TaskId || assigned.StepInstanceId!=row.StepName || assigned.AssignedRole!=row.AssignedRole ||
                        assigned.Source.Recorded!.DRILLING_OPERATION_ID!=row.EntityId || assigned.PersonaCode!=persona)
                        throw new InvalidOperationException("The qualified execution reporting source or original process/task candidate changed during inbox readback.");
                    row.ExecutionAssignmentRevision=assigned.Revision;row.EntityDescription=assigned.Source.Basis.Source.Program.PROGRAM_NAME;
                    WorkflowInboxReview.RequireTask(row,closing.Roles);
                }
                catch(UnauthorizedAccessException){return null;}
            }
            else if(row.EntityType=="DRILLING_PROGRAM")
            {
                try
                {
                    var assigned=await programReviews.ReadAssignedProgramAsync(User,row.WorkflowName);assigned.Require(company.Scope,userId,row.WorkflowName);process=await processes.GetProcessInstanceAsync(row.WorkflowName);
                    if(assigned.Revision!=row.ProgramReviewRevision || assigned.ProcessRevision!=ProcessDefinitionRevision.FromJson(System.Text.Json.JsonSerializer.Serialize(process)))throw new InvalidOperationException("The qualified program source changed during inbox readback.");
                }
                catch(UnauthorizedAccessException){return null;}
            }
            else if(row.EntityType=="FIELD_DEVELOPMENT_PLAN")
            {
                try
                {
                    var assigned=await developmentReviews.ReadAsync(User,row.WorkflowName);assigned.Require(company.Scope,userId,row.WorkflowName);
                    process=await processes.GetProcessInstanceAsync(row.WorkflowName);
                    if(assigned.Revision!=row.DevelopmentReviewRevision || assigned.ProcessRevision!=ProcessDefinitionRevision.FromJson(System.Text.Json.JsonSerializer.Serialize(process)))
                        throw new InvalidOperationException("The qualified development review changed during inbox readback.");
                }
                catch(UnauthorizedAccessException){return null;}
            }
            else
            {
                process = await processes.GetProcessInstanceAsync(row.WorkflowName);
                if (process is null || string.IsNullOrWhiteSpace(process.FieldId) ||
                    !(await resources.CheckAssetAccessAsync(userId, process.FieldId, "FIELD", "READ")).HasAccess ||
                    !(await resources.CheckAssetAccessAsync(userId, process.EntityId, process.EntityType, "READ")).HasAccess) return null;
            }
            if (!WorkflowTaskProcessAdmission.Matches(row, process, userId)) return null;
            if(!WorkflowTaskProcessAdmission.CompanyMatches(process!,company.Scope))throw new InvalidOperationException("The original maintenance company ownership changed during inbox review.");
            var stepName=process!.StepInstances.Single(step=>step.StepInstanceId==row.StepName).StepName;
            if(row.StepDisplayName!=(string.IsNullOrWhiteSpace(stepName)?null:stepName))
                throw new InvalidOperationException("The assigned step description changed during inbox review.");
            if(row.ScopeKind=="COMPANY_ACCOUNTING" && row.AccountingStepReview!=(row.EntityType is "JOURNAL_ENTRY" or "AR_PAYMENT"?journalReview:await accountingProcesses.ReviewAssignedStepEvidenceAsync(User,row,process,company.Scope,userId)))
                throw new InvalidOperationException("The recorded accounting disposition or current step eligibility changed during inbox review.");
        }
        if(executionProcesses.Count>0)
        {
            var finalTasks=await tasks.GetTasksForPersonaAsync(persona);
            var finalWorkspace=await personas.ReviewWorkspacesAsync(userId,company.Scope,access,token);
            if(Candidates(finalTasks)!=candidateSnapshot || finalWorkspace.Revision!=review.Revision || await CurrentPersonaAsync(token)!=persona ||
                tenant.Company is not {} finalCompany || finalCompany.Scope!=company.Scope || finalCompany.UserId!=userId)
                throw new InvalidOperationException("The complete reporting candidate set or current company workspace changed around source admission.");
        }
        _reviewedPersona = persona;
        return visible;
    }

    private static InboxCounts Count(List<UnifiedTask> tasks) => new() { TotalPending = tasks.Count,
        Critical = tasks.Count(x => x.Priority <= 1), Overdue = tasks.Count(x => x.SlaStatus == "BREACHED"),
        Approvals = tasks.Count(x => x.TaskType == "APPROVAL"), Reviews = tasks.Count(x => x.TaskType == "REVIEW"),
        DataEntry = tasks.Count(x => x.TaskType == "DATA_ENTRY") };
}
