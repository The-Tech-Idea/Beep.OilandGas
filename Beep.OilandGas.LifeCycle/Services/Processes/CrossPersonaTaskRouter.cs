using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.Models.Core.Interfaces;
using System.Text.Json;
using System.Data;
using TheTechIdea.Data.OilGas;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

/// <summary>
/// Routes workflow tasks to persona inboxes represented by active members of the assigned role.
/// When a step is assigned to a role, this service determines which persona(s) should see it
/// and creates CROSS_PERSONA_TASK records for each.
/// Part of Phase 3 cross-role orchestration.
/// </summary>
public class CrossPersonaTaskRouter : ICrossPersonaTaskRouter
{
    private readonly IDMEEditor _editor;
    private readonly ICommonColumnHandler _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly Func<Task<string>>? _resolveConnection;
    private readonly IRolePersonaReader? _personas;
    private readonly ILogger<CrossPersonaTaskRouter> _logger;

    public CrossPersonaTaskRouter(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        Func<Task<string>>? resolveConnection = null,
        ILogger<CrossPersonaTaskRouter>? logger = null,
        IRolePersonaReader? personas = null)
    {
        _editor = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults = defaults;
        _metadata = metadata;
        _resolveConnection = resolveConnection;
        _personas = personas;
        _logger = logger;
    }

    public Task<List<CROSS_PERSONA_TASK>> RouteTaskAsync(string processInstanceId, string stepInstanceId, string assignedRole,
        string entityType, string entityId, string? entityDescription, string userId)
        => RouteAsync(processInstanceId, stepInstanceId, assignedRole, entityType, entityId, entityDescription, userId, true);

    // This composition boundary requires the transaction helper's actual owned command; it cannot join a borrowed transaction.
    internal Task<List<CROSS_PERSONA_TASK>> RouteTaskInOwnedTransactionAsync(string processInstanceId, string stepInstanceId, string assignedRole,
        string entityType, string entityId, string? entityDescription, string userId, DrillingReportingReassignment? reportingTransfer = null)
        => RouteAsync(processInstanceId, stepInstanceId, assignedRole, entityType, entityId, entityDescription, userId, false, false, reportingTransfer);

    internal Task<List<CROSS_PERSONA_TASK>> ReadTaskSetInOwnedTransactionAsync(string process, string step, string role,
        string type, string entity, string actor,string? entityDescription=null)
        => RouteAsync(process, step, role, type, entity, entityDescription, actor, false, true);
    internal async Task<IReadOnlyList<CROSS_PERSONA_TASK>> GetAllTasksForDevelopmentEvidenceAsync(string process,string expectedConnection)
    {
        if(await ResolveConnectionAsync()!=expectedConnection)throw new InvalidOperationException("The original development task binding changed.");
        var rows=(await GetTaskRepo(expectedConnection).GetAsync([new(){FieldName="PROCESS_INSTANCE_ID",Operator = "=", FilterValue=process}])).ToArray();
        if(rows.Any(row=>row is not CROSS_PERSONA_TASK) || rows.Cast<CROSS_PERSONA_TASK>().Any(row=>row.PROCESS_INSTANCE_ID!=process) ||
            rows.Cast<CROSS_PERSONA_TASK>().Select(row=>row.CROSS_TASK_ID).Distinct(StringComparer.Ordinal).Count()!=rows.Length)
            throw new InvalidOperationException("The complete development task evidence is ambiguous.");
        return rows.Cast<CROSS_PERSONA_TASK>().ToArray();
    }

    private async Task<List<CROSS_PERSONA_TASK>> RouteAsync(
        string processInstanceId,
        string stepInstanceId,
        string assignedRole,
        string entityType,
        string entityId,
        string? entityDescription,
        string userId, bool ownTransaction, bool readOnly = false, DrillingReportingReassignment? reportingTransfer = null)
    {
        foreach(var value in new[]{processInstanceId,stepInstanceId,assignedRole,entityType,entityId,userId})
            if(string.IsNullOrWhiteSpace(value) || value.Length>128 || value.Any(char.IsControl))throw new ArgumentException("Recorded task routing identities are required.");
        if(_personas is null)throw new InvalidOperationException("Task routing requires the repository persona reader.");
        if(entityType is "FIELD_DEVELOPMENT_PLAN" or "AFE" or "DRILLING_PROGRAM" or "DRILLING_OPERATION" or "DRILLING_DAILY_REPORT" or "FACILITY" && ownTransaction)throw new InvalidOperationException("Source-bound assignment requires its reviewed command owner.");
        if (ownTransaction && entityType == CompanyHseWorkflowStart.EntityType && processInstanceId.StartsWith(CompanyHseWorkflowStart.InstancePrefix, StringComparison.Ordinal))
            throw new InvalidOperationException("HSE task routing requires its reviewed workflow owner.");
        var connectionName=await ResolveConnectionAsync();
        var source=_editor.GetDataSource(connectionName) ?? throw new InvalidOperationException("The task datasource is unavailable.");
        var result=new List<CROSS_PERSONA_TASK>();
        async Task Guard()
        {
            if(connectionName!=await ResolveConnectionAsync() || !ReferenceEquals(source,_editor.GetDataSource(connectionName)))
                throw new InvalidOperationException("The original task database binding changed.");
            PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);
        }
        async Task RouteOwned() {
            await Guard();
            var process=await ReadUniqueAsync<PROCESS_INSTANCE>("PROCESS_INSTANCE","PROCESS_INSTANCE_ID",processInstanceId,connectionName)
                ?? throw new InvalidOperationException("The recorded task process is unavailable.");
            var step=await GetStepInstanceAsync(stepInstanceId,connectionName)
                ?? throw new InvalidOperationException("The recorded task step is unavailable.");
            if(process.PROCESS_INSTANCE_ID?.StartsWith(DrillingCompletionReviewStart.InstancePrefix,StringComparison.Ordinal)==true || DrillingCompletionReviewStart.HasPayload(process.INSTANCE_DATA_JSON))
            {
                if(ownTransaction || entityType!="DRILLING_DAILY_REPORT" || process.ENTITY_TYPE!="DRILLING_DAILY_REPORT")
                    throw new InvalidOperationException("Completion approval tasks require their canonical original review owner.");
            }
            if(process.PROCESS_INSTANCE_ID?.StartsWith(DrillingExecutionAssignment.InstancePrefix,StringComparison.Ordinal)==true || DrillingExecutionAssignment.HasPayload(process.INSTANCE_DATA_JSON))
            {
                if(ownTransaction || entityType!="DRILLING_OPERATION" || process.ENTITY_TYPE!="DRILLING_OPERATION")
                    throw new InvalidOperationException("Execution reporting tasks require their canonical original assignment owner.");
            }
            if(process.PROCESS_INSTANCE_ID!=processInstanceId || string.IsNullOrWhiteSpace(process.PROCESS_DEFINITION_ID) || process.ENTITY_ID!=entityId || process.ENTITY_TYPE!=entityType || process.STATUS!="IN_PROGRESS" || process.ACTIVE_IND!="Y" || step.ACTIVE_IND!="Y" ||
                step.PROCESS_INSTANCE_ID!=processInstanceId || step.REQUIRED_ROLE!=assignedRole || step.STEP_ID!=process.CURRENT_STEP_ID ||
                step.STATUS is not ("PENDING" or "IN_PROGRESS"))throw new InvalidOperationException("The task source, active step or assigned role does not match its process.");
            var scopeKind=string.IsNullOrWhiteSpace(process.FIELD_ID)?"COMPANY_ACCOUNTING":"FIELD";
            if(entityType=="AFE")
            {
                var original=DevelopmentInvestmentHandoff.FromPayload(process.INSTANCE_DATA_JSON!,processInstanceId,process.FIELD_ID!,entityId);
                if(original.Scope().ActorUserId!=userId && step.ROW_CHANGED_BY!=userId || original.Scope().LifecycleDatabase.ConnectionName!=connectionName || scopeKind!="FIELD")
                    throw new InvalidOperationException("The original investment assignment actor, FIELD or lifecycle target changed.");
            }
            if(entityType=="DRILLING_PROGRAM")
            {
                var original=DrillingProgramReviewStart.FromPayload(process.INSTANCE_DATA_JSON!,processInstanceId,process.FIELD_ID!,entityId);
                if(original.Scope().ActorUserId!=userId && step.ROW_CHANGED_BY!=userId || original.Scope().LifecycleDatabase.ConnectionName!=connectionName || scopeKind!="FIELD")throw new InvalidOperationException("The original program assignment actor, FIELD or lifecycle target changed.");
            }
            if(entityType=="DRILLING_DAILY_REPORT")
            {
                var original=DrillingCompletionReviewStart.FromPayload(process.INSTANCE_DATA_JSON!,processInstanceId,process.FIELD_ID!,entityId);
                if(original.Scope().ActorUserId!=userId && step.ROW_CHANGED_BY!=userId || original.Scope().LifecycleDatabase.ConnectionName!=connectionName || scopeKind!="FIELD")
                    throw new InvalidOperationException("The original completion review actor, FIELD or lifecycle target changed.");
            }
            if(entityType=="DRILLING_OPERATION")
            {
                var original=DrillingExecutionAssignment.FromPayload(process.INSTANCE_DATA_JSON!,processInstanceId,process.FIELD_ID!,entityId);
                var currentTransfer=PPDMProcessService.ReportingTransferForRouting(process.INSTANCE_DATA_JSON!,original);
                if((currentTransfer is null)!=(reportingTransfer is null) || currentTransfer is not null && JsonSerializer.Serialize(currentTransfer)!=JsonSerializer.Serialize(reportingTransfer))
                    throw new InvalidOperationException("Reporting tasks require their exact canonical current generation owner.");
                var workflow=currentTransfer?.Review.Destination??original.Workflow;
                var expectedStep=currentTransfer?.StepInstanceId()??DrillingExecutionAssignmentReceipt.StepId(processInstanceId,workflow.Step.StepId);
                if((currentTransfer?.Scope().ActorUserId??original.Scope().ActorUserId)!=userId || original.Scope().LifecycleDatabase.ConnectionName!=connectionName || scopeKind!="FIELD" ||
                    workflow.Step.StepId!=process.CURRENT_STEP_ID || workflow.Step.AssignedRole!=assignedRole || stepInstanceId!=expectedStep || step.APPROVAL_REQUIRED)
                    throw new InvalidOperationException("The original execution reporting assignment actor, role, FIELD or lifecycle target changed.");
            }
            if(entityType=="FIELD_DEVELOPMENT_PLAN")
            {
                var original=DevelopmentReviewStart.FromPayload(process.INSTANCE_DATA_JSON!,processInstanceId,process.FIELD_ID!,entityId);
                if(original.Source.Context.ActorUserId!=userId && step.ROW_CHANGED_BY!=userId || original.LifecycleDatabase.ConnectionName!=connectionName)
                    throw new InvalidOperationException("The original development assignment actor or lifecycle target changed.");
            }
            if (entityType == CompanyHseWorkflowStart.EntityType && processInstanceId.StartsWith(CompanyHseWorkflowStart.InstancePrefix, StringComparison.Ordinal))
            {
                var original = CompanyHseWorkflowStart.FromPayload(process.INSTANCE_DATA_JSON!, processInstanceId, process.FIELD_ID!, entityId);
                if ((original.Context.ActorUserId != userId && step.ROW_CHANGED_BY != userId) || original.LifecycleDatabase.ConnectionName != connectionName || scopeKind != "FIELD")
                    throw new InvalidOperationException("The original HSE assignment actor, FIELD or lifecycle target changed.");
            }
            if(processInstanceId.StartsWith(CompanyMaintenanceProcessStart.InstancePrefix,StringComparison.Ordinal) || CompanyMaintenanceProcessStart.HasPayload(process.INSTANCE_DATA_JSON))
            {
                if(entityType!="WORK_ORDER" || string.IsNullOrWhiteSpace(process.FIELD_ID))throw new InvalidOperationException("The maintenance task must retain its original FIELD/order ownership.");
                CompanyMaintenanceProcessStart.FromPayload(process.INSTANCE_DATA_JSON ?? "{}",processInstanceId,process.FIELD_ID,entityId);
            }
            if(scopeKind=="COMPANY_ACCOUNTING")
            {
                CompanyAccountingProcessBinding.FromPayloadJson(process.INSTANCE_DATA_JSON ?? "{}",processInstanceId,entityType,entityId);
            }
            var cycle=scopeKind=="COMPANY_ACCOUNTING"?CompanyAccountingCorrectionCycle.FromPayload(process.INSTANCE_DATA_JSON!,processInstanceId,entityType,entityId):null;
            var round=entityType == CompanyHseWorkflowStart.EntityType ? CompanyHseCorrectionRound.FromPayload(process.INSTANCE_DATA_JSON!, processInstanceId, process.FIELD_ID!, entityId)?.RoundFor(stepInstanceId) : cycle?.RoundFor(stepInstanceId);
            var route=WorkflowTaskRoute.For(scopeKind,processInstanceId,entityType);
            if(step.SLA_HOURS is {} hours && (!double.IsFinite(hours) || hours<0))throw new InvalidOperationException("The recorded task SLA is invalid.");
            var originalProcess=JsonSerializer.Serialize(process);var originalStep=JsonSerializer.Serialize(step);
            var personas=(await _personas.GetActivePersonasAsync(assignedRole)).Order(StringComparer.Ordinal).ToArray();
            if(personas.Length==0 || personas.Length>128 || personas.Distinct(StringComparer.Ordinal).Count()!=personas.Length ||
                personas.Any(value=>string.IsNullOrWhiteSpace(value) || value.Length>64 || value.Any(char.IsControl)))
                throw new InvalidOperationException("The assigned role has no confirmed unique active persona destinations.");
            var dueDate=step.SLA_HOURS.HasValue && step.STARTED_DATE.HasValue?step.STARTED_DATE.Value.AddHours(step.SLA_HOURS.Value):(DateTime?)null;
            var repo=GetTaskRepo(connectionName);var now=DateTime.UtcNow;
            var taskSetFilters=new List<AppFilter>{new(){FieldName="PROCESS_INSTANCE_ID",Operator = "=", FilterValue=processInstanceId},new(){FieldName="PROCESS_STEP_INSTANCE_ID",Operator = "=", FilterValue=stepInstanceId}};
            var existingSet=(await repo.GetAsync(taskSetFilters)).OfType<CROSS_PERSONA_TASK>().Where(row=>CompanyAccountingCorrectionCycle.TaskRound(row.TASK_CONTEXT_JSON)==round).ToArray();
            if(existingSet.Length>0 && (existingSet.Length!=personas.Length || !existingSet.Select(row=>row.TARGET_PERSONA_CODE).Order(StringComparer.Ordinal).SequenceEqual(personas,StringComparer.Ordinal)))
                throw new InvalidOperationException("The recorded task destination set differs from this handoff; reconcile before routing.");
            foreach(var persona in personas)
            {
                await Guard();
                var identity="task-"+Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(
                    round is null?JsonSerializer.Serialize(new{Process=processInstanceId,Step=stepInstanceId,Persona=persona}):JsonSerializer.Serialize(new{Process=processInstanceId,Step=stepInstanceId,Persona=persona,CorrectionCycle=round}))));
                var expected=new CROSS_PERSONA_TASK{CROSS_TASK_ID=identity,PROCESS_INSTANCE_ID=processInstanceId,PROCESS_STEP_INSTANCE_ID=stepInstanceId,
                    TARGET_PERSONA_CODE=persona,ASSIGNED_ROLE=assignedRole,TASK_TYPE=entityType=="DRILLING_OPERATION"?"DATA_ENTRY":step.APPROVAL_REQUIRED?"APPROVAL":"REVIEW",PRIORITY=DeterminePriority(step,dueDate),
                    TASK_STATUS="PENDING",DUE_DATE=dueDate,ENTITY_TYPE=entityType,ENTITY_ID=entityId,ENTITY_DESCRIPTION=entityDescription,ROUTE=route,
                    TASK_CONTEXT_JSON=round is null?JsonSerializer.Serialize(new{assignedRole,entityType,entityId,processInstanceId,stepInstanceId,scopeKind}):JsonSerializer.Serialize(new{assignedRole,entityType,entityId,processInstanceId,stepInstanceId,scopeKind,correctionCycle=round}),
                    ACTIVE_IND="Y",ROW_CREATED_BY=userId,ROW_CREATED_DATE=now};
                var existing=existingSet.Where(row=>row.TARGET_PERSONA_CODE==persona).ToArray();
                if(existing.Length==1)expected.PRIORITY=existing[0].PRIORITY; // Retry preserves recorded priority rather than reprioritizing a task.
                if(existing.Length>1 || existing.Length==1 && !SameTask(existing[0],expected))throw new InvalidOperationException("Recorded task routing conflicts with this handoff; reconcile the original task.");
                if(existing.Length==0){
                    if(readOnly)throw new InvalidOperationException("The original assignment task is missing. Read-only recovery cannot repair it.");
                    if(await ReadUniqueAsync<CROSS_PERSONA_TASK>("CROSS_PERSONA_TASK","CROSS_TASK_ID",identity,connectionName) is not null)
                        throw new InvalidOperationException("The routed task identity is already owned by another source.");
                    await repo.InsertAsync(expected,userId);
                }
                var recorded=await ReadUniqueAsync<CROSS_PERSONA_TASK>("CROSS_PERSONA_TASK","CROSS_TASK_ID",identity,connectionName);
                if(recorded is null || !SameTask(recorded,expected))throw new InvalidOperationException("The routed task read-back was not confirmed.");
                result.Add(JsonSerializer.Deserialize<CROSS_PERSONA_TASK>(JsonSerializer.Serialize(recorded))!);
            }
            await Guard();
            foreach(var recorded in result)
                if(JsonSerializer.Serialize(recorded)!=JsonSerializer.Serialize(await ReadUniqueAsync<CROSS_PERSONA_TASK>("CROSS_PERSONA_TASK","CROSS_TASK_ID",recorded.CROSS_TASK_ID,connectionName)))
                    throw new InvalidOperationException("A routed task changed before commit.");
                        var closingTasks=(await repo.GetAsync(taskSetFilters)).ToArray();
            if(closingTasks.Any(row=>row is not CROSS_PERSONA_TASK) || closingTasks.Cast<CROSS_PERSONA_TASK>().Any(row=>CompanyAccountingCorrectionCycle.TaskRound(row.TASK_CONTEXT_JSON)!=round && row.TASK_STATUS!="COMPLETED") ||
                closingTasks.Cast<CROSS_PERSONA_TASK>().Count(row=>CompanyAccountingCorrectionCycle.TaskRound(row.TASK_CONTEXT_JSON)==round)!=result.Count)
                throw new InvalidOperationException("The recorded task destination set changed before commit.");
            if(originalProcess!=JsonSerializer.Serialize(await ReadUniqueAsync<PROCESS_INSTANCE>("PROCESS_INSTANCE","PROCESS_INSTANCE_ID",processInstanceId,connectionName)) || originalStep!=JsonSerializer.Serialize(await GetStepInstanceAsync(stepInstanceId,connectionName)) ||
                !personas.SequenceEqual((await _personas.GetActivePersonasAsync(assignedRole)).Order(StringComparer.Ordinal),StringComparer.Ordinal))
                throw new InvalidOperationException("The task process, assignment or persona destinations changed before commit.");
            await Guard();
        }
        if (ownTransaction) await PPDMDataSourceTransaction.ExecuteAsync(source, RouteOwned);
        else { await Guard(); await RouteOwned(); }
        // Commit uncertainty is propagated by the existing transaction owner; no successful task set is returned.
        if(connectionName!=await ResolveConnectionAsync() || !ReferenceEquals(source,_editor.GetDataSource(connectionName)))
            throw new InvalidOperationException("The task commit requires read-back on its original database binding.");
        foreach(var recorded in result)
            if(JsonSerializer.Serialize(recorded)!=JsonSerializer.Serialize(await ReadUniqueAsync<CROSS_PERSONA_TASK>("CROSS_PERSONA_TASK","CROSS_TASK_ID",recorded.CROSS_TASK_ID,connectionName)))
                throw new InvalidOperationException("The committed task read-back was not confirmed. Reconcile the original handoff before retrying.");
        return result;
    }

    internal async Task RequireExistingTaskSetAsync(string process, string step, string expectedConnection)
    {
        if (await ResolveConnectionAsync() != expectedConnection || !(await GetTaskRepo(expectedConnection).GetAsync([
            new() { FieldName = "PROCESS_INSTANCE_ID", Operator = "=", FilterValue = process }, new() { FieldName = "PROCESS_STEP_INSTANCE_ID", Operator = "=", FilterValue = step }])).Any())
            throw new InvalidOperationException("An existing handoff without its recorded task set requires reconciliation.");
    }

    internal async Task<List<CROSS_PERSONA_TASK>> CompleteTaskSetInOwnedTransactionAsync(string process,string step,string role,string type,string entity,string actor,string connection,DrillingReportingReassignment? reportingTransfer=null)
    {
        if(await ResolveConnectionAsync()!=connection)throw new InvalidOperationException("The original task binding changed.");
        var source=_editor.GetDataSource(connection) ?? throw new InvalidOperationException("The task source is unavailable.");
        void Target(){if(!ReferenceEquals(source,_editor.GetDataSource(connection)))throw new InvalidOperationException("The owned task completion target changed.");PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);}
        Target();
        var rows=(await GetTaskRepo(connection).GetAsync([new(){FieldName="PROCESS_INSTANCE_ID",Operator = "=", FilterValue=process},new(){FieldName="PROCESS_STEP_INSTANCE_ID",Operator = "=", FilterValue=step}])).ToList();
        if(rows.Count==0 || rows.Any(row=>row is not CROSS_PERSONA_TASK))throw new InvalidOperationException("The original complete task set is required.");
        var processRow=await ReadUniqueAsync<PROCESS_INSTANCE>("PROCESS_INSTANCE","PROCESS_INSTANCE_ID",process,connection)??throw new InvalidOperationException("The task process is unavailable.");
        var round=type == CompanyHseWorkflowStart.EntityType ? CompanyHseCorrectionRound.FromPayload(processRow.INSTANCE_DATA_JSON!,process,processRow.FIELD_ID!,entity)?.RoundFor(step) : string.IsNullOrWhiteSpace(processRow.FIELD_ID)?CompanyAccountingCorrectionCycle.FromPayload(processRow.INSTANCE_DATA_JSON!,process,type,entity)?.RoundFor(step):null;
        var recorded=rows.Cast<CROSS_PERSONA_TASK>().Where(row=>CompanyAccountingCorrectionCycle.TaskRound(row.TASK_CONTEXT_JSON)==round).ToArray();
        if(recorded.Length==0)throw new InvalidOperationException("The current complete correction-cycle task set is required.");
        var creators=recorded.Select(row=>row.ROW_CREATED_BY).Distinct(StringComparer.Ordinal).ToArray();
        var descriptions=recorded.Select(row=>row.ENTITY_DESCRIPTION).Distinct(StringComparer.Ordinal).ToArray();
        if(creators.Length!=1 || descriptions.Length!=1)throw new InvalidOperationException("The recorded task handoff is ambiguous.");
        var tasks=await RouteTaskInOwnedTransactionAsync(process,step,role,type,entity,descriptions[0],creators[0],reportingTransfer);
        foreach(var task in tasks)
        {
            Target();
            task.TASK_STATUS="COMPLETED";task.ROW_CHANGED_BY=actor;task.ROW_CHANGED_DATE=DateTime.UtcNow;
            await GetTaskRepo(connection).UpdateAsync(task,actor);
            Target();
        }
        await ConfirmRecordedTaskSetAsync(tasks,connection);return tasks;
    }

    internal async Task RequireAbsentReportingTransferTasksAsync(DrillingReportingReassignment command,string connection)
    {
        if(await ResolveConnectionAsync()!=connection)throw new InvalidOperationException("The reporting transfer task binding changed.");
        foreach(var id in command.TaskIds())
            if((await GetTaskRepo(connection).GetAsync([new(){FieldName="CROSS_TASK_ID",Operator = "=", FilterValue=id}])).Any())
                throw new InvalidOperationException("Partial or retagged reporting transfer tasks cannot become absence.");
        if((await GetTaskRepo(connection).GetAsync([new(){FieldName="PROCESS_STEP_INSTANCE_ID",Operator = "=", FilterValue=command.StepInstanceId()}])).Any())
            throw new InvalidOperationException("Unlisted reporting transfer task evidence cannot become absence.");
    }

    internal async Task ConfirmRecordedTaskSetAsync(IReadOnlyList<CROSS_PERSONA_TASK> recorded, string expectedConnection)
    {
        if (recorded.Count == 0 || await ResolveConnectionAsync() != expectedConnection)
            throw new InvalidOperationException("The recorded task set or original database binding is unavailable.");
        foreach (var task in recorded)
            if (JsonSerializer.Serialize(task) != JsonSerializer.Serialize(await ReadUniqueAsync<CROSS_PERSONA_TASK>("CROSS_PERSONA_TASK", "CROSS_TASK_ID", task.CROSS_TASK_ID, expectedConnection)))
                throw new InvalidOperationException("The complete recorded handoff tasks could not be confirmed.");
        var first = recorded[0];
        var rows = await GetTaskRepo(expectedConnection).GetAsync([new() { FieldName = "PROCESS_INSTANCE_ID", Operator = "=", FilterValue = first.PROCESS_INSTANCE_ID },
            new() { FieldName = "PROCESS_STEP_INSTANCE_ID", Operator = "=", FilterValue = first.PROCESS_STEP_INSTANCE_ID }]);
        var round=CompanyAccountingCorrectionCycle.TaskRound(first.TASK_CONTEXT_JSON);
        if(recorded.Any(row=>CompanyAccountingCorrectionCycle.TaskRound(row.TASK_CONTEXT_JSON)!=round))throw new InvalidOperationException("Recorded tasks belong to different correction cycles.");
        if (rows.OfType<CROSS_PERSONA_TASK>().Count(row=>CompanyAccountingCorrectionCycle.TaskRound(row.TASK_CONTEXT_JSON)==round) != recorded.Count) throw new InvalidOperationException("The recorded handoff task set changed.");
    }

    internal async Task ConfirmProgressedTaskSetAsync(IReadOnlyList<CROSS_PERSONA_TASK> recorded, string expectedConnection)
    {
        if (recorded.Count == 0 || await ResolveConnectionAsync() != expectedConnection)
            throw new InvalidOperationException("The recorded task set or original database binding is unavailable.");
        var first = recorded[0];
        var rows = (await GetTaskRepo(expectedConnection).GetAsync([new() { FieldName = "PROCESS_INSTANCE_ID", Operator = "=", FilterValue = first.PROCESS_INSTANCE_ID },
            new() { FieldName = "PROCESS_STEP_INSTANCE_ID", Operator = "=", FilterValue = first.PROCESS_STEP_INSTANCE_ID }])).OfType<CROSS_PERSONA_TASK>().ToArray();
        var round = CompanyAccountingCorrectionCycle.TaskRound(first.TASK_CONTEXT_JSON);
        if (recorded.Any(row => CompanyAccountingCorrectionCycle.TaskRound(row.TASK_CONTEXT_JSON) != round)) throw new InvalidOperationException("Recorded tasks belong to different correction rounds.");
        rows = rows.Where(row => CompanyAccountingCorrectionCycle.TaskRound(row.TASK_CONTEXT_JSON) == round).ToArray();
        if (rows.Length != recorded.Count || recorded.Select(item => item.CROSS_TASK_ID).Distinct(StringComparer.Ordinal).Count() != recorded.Count)
            throw new InvalidOperationException("The recorded handoff task set changed.");
        foreach (var original in recorded)
        {
            var matches = rows.Where(item => item.CROSS_TASK_ID == original.CROSS_TASK_ID).ToArray();
            if (matches.Length != 1) throw new InvalidOperationException("A recorded handoff task is missing or ambiguous.");
            var current = matches[0];
            if (current.PROCESS_INSTANCE_ID != original.PROCESS_INSTANCE_ID || current.PROCESS_STEP_INSTANCE_ID != original.PROCESS_STEP_INSTANCE_ID ||
                current.TARGET_PERSONA_CODE != original.TARGET_PERSONA_CODE || current.ASSIGNED_ROLE != original.ASSIGNED_ROLE || current.TASK_TYPE != original.TASK_TYPE ||
                current.PRIORITY != original.PRIORITY || current.DUE_DATE != original.DUE_DATE || current.ENTITY_TYPE != original.ENTITY_TYPE || current.ENTITY_ID != original.ENTITY_ID ||
                current.ENTITY_DESCRIPTION != original.ENTITY_DESCRIPTION || current.ROUTE != original.ROUTE || current.TASK_CONTEXT_JSON != original.TASK_CONTEXT_JSON ||
                current.ACTIVE_IND != original.ACTIVE_IND || current.ROW_CREATED_BY != original.ROW_CREATED_BY || current.ROW_CREATED_DATE != original.ROW_CREATED_DATE ||
                current.TASK_STATUS is not ("PENDING" or "IN_PROGRESS" or "COMPLETED" or "DISMISSED") ||
                (current.TASK_STATUS == "PENDING" && JsonSerializer.Serialize(current) != JsonSerializer.Serialize(original)) ||
                (current.TASK_STATUS != "PENDING" && (!LeaseSummaryValidator.Identifier(current.ROW_CHANGED_BY) || current.ROW_CHANGED_DATE is not { Kind: DateTimeKind.Utc } changed ||
                    original.ROW_CREATED_DATE is not { Kind: DateTimeKind.Utc } created || changed < created)))
                throw new InvalidOperationException("A recorded handoff task was replaced or its progress is unconfirmed.");
        }
    }
    private static bool SameTask(CROSS_PERSONA_TASK recorded,CROSS_PERSONA_TASK expected)=>
        recorded.CROSS_TASK_ID==expected.CROSS_TASK_ID && recorded.PROCESS_INSTANCE_ID==expected.PROCESS_INSTANCE_ID && recorded.PROCESS_STEP_INSTANCE_ID==expected.PROCESS_STEP_INSTANCE_ID &&
        recorded.TARGET_PERSONA_CODE==expected.TARGET_PERSONA_CODE && recorded.ASSIGNED_ROLE==expected.ASSIGNED_ROLE && recorded.TASK_TYPE==expected.TASK_TYPE && recorded.TASK_STATUS=="PENDING" &&
        recorded.ENTITY_TYPE==expected.ENTITY_TYPE && recorded.ENTITY_ID==expected.ENTITY_ID && recorded.ENTITY_DESCRIPTION==expected.ENTITY_DESCRIPTION && recorded.DUE_DATE==expected.DUE_DATE &&
        recorded.ROUTE==expected.ROUTE && recorded.TASK_CONTEXT_JSON==expected.TASK_CONTEXT_JSON && recorded.PRIORITY==expected.PRIORITY && recorded.PRIORITY is >=1 and <=4 && recorded.ACTIVE_IND=="Y" && recorded.ROW_CREATED_BY==expected.ROW_CREATED_BY && recorded.ROW_CREATED_DATE.HasValue;
    public async Task<List<CROSS_PERSONA_TASK>> GetTasksForPersonaAsync(string personaCode)
    {
        var repo = GetTaskRepo(await ResolveConnectionAsync());
        var filters = new List<AppFilter>
        {
            new() { FieldName = "TARGET_PERSONA_CODE", Operator = "=", FilterValue = personaCode },
            new() { FieldName = "TASK_STATUS", Operator = "=", FilterValue = "PENDING" },
        };
        var results = await repo.GetAsync(filters);
        return results.OfType<CROSS_PERSONA_TASK>()
            .OrderBy(t => t.PRIORITY)
            .ThenBy(t => t.DUE_DATE ?? DateTime.MaxValue)
            .ToList();
    }

    public async Task<Dictionary<string, int>> GetTaskCountsByPersonaAsync()
    {
        var repo = GetTaskRepo(await ResolveConnectionAsync());
        var filters = new List<AppFilter>
        {
            new() { FieldName = "TASK_STATUS", Operator = "=", FilterValue = "PENDING" },
        };
        var results = await repo.GetAsync(filters);
        return results.OfType<CROSS_PERSONA_TASK>()
            .GroupBy(t => t.TARGET_PERSONA_CODE)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private async Task<string> ResolveConnectionAsync()
    {
        var connection = _resolveConnection is null ? null : await _resolveConnection();
        if (string.IsNullOrWhiteSpace(connection))
            throw new InvalidOperationException("Task routing requires a bound LIFECYCLE database.");
        return connection;
    }

    private Task<PROCESS_STEP_INSTANCE?> GetStepInstanceAsync(string id,string connectionName)=>
        ReadUniqueAsync<PROCESS_STEP_INSTANCE>("PROCESS_STEP_INSTANCE","PROCESS_STEP_INSTANCE_ID",id,connectionName);

    private async Task<T?> ReadUniqueAsync<T>(string table,string key,string id,string connectionName) where T:class
    {
        var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(T),connectionName,table,null);
        var rows=(await repo.GetAsync([new(){FieldName=key,Operator = "=", FilterValue=id}])).ToArray();
        if(rows.Length>1 || rows.Length==1 && rows[0] is not T)throw new InvalidOperationException("The recorded routing source identity is ambiguous or invalid.");
        return rows.Length==0?null:(T)rows[0];
    }
    private static int DeterminePriority(PROCESS_STEP_INSTANCE? step, DateTime? dueDate)
    {
        if (dueDate.HasValue && dueDate.Value < DateTime.UtcNow) return 1; // Overdue = Critical
        if (step?.SLA_HOURS is > 0 && step.STARTED_DATE.HasValue)
        {
            var elapsed = DateTime.UtcNow - step.STARTED_DATE.Value;
            var sla = TimeSpan.FromHours(step.SLA_HOURS.Value);
            if (elapsed > sla * 0.75) return 1;  // > 75% of SLA elapsed
            if (elapsed > sla * 0.5) return 2;   // > 50% of SLA elapsed
        }
        return 3; // Normal
    }

    internal Task RequireInvestmentSourceTaskParentsAsync(string entity,IReadOnlyList<PROCESS_INSTANCE> parents,string connection)
        =>RequireSourceTaskParentsAsync(entity,parents,connection,"AFE",DevelopmentInvestmentHandoff.InstancePrefix,parent=>parent.ENTITY_TYPE=="AFE" || parent.PROCESS_INSTANCE_ID?.StartsWith(DevelopmentInvestmentHandoff.InstancePrefix,StringComparison.Ordinal)==true || DevelopmentInvestmentHandoff.HasPayload(parent.INSTANCE_DATA_JSON));
    internal Task RequireProgramSourceTaskParentsAsync(string entity,IReadOnlyList<PROCESS_INSTANCE> parents,string connection)
        =>RequireSourceTaskParentsAsync(entity,parents,connection,"DRILLING_PROGRAM",DrillingProgramReviewStart.InstancePrefix,parent=>parent.ENTITY_TYPE=="DRILLING_PROGRAM" || parent.PROCESS_INSTANCE_ID?.StartsWith(DrillingProgramReviewStart.InstancePrefix,StringComparison.Ordinal)==true || DrillingProgramReviewStart.HasPayload(parent.INSTANCE_DATA_JSON));
    internal Task RequireCompletionSourceTaskParentsAsync(string entity,IReadOnlyList<PROCESS_INSTANCE> parents,string connection)
        =>RequireSourceTaskParentsAsync(entity,parents,connection,"DRILLING_DAILY_REPORT",DrillingCompletionReviewStart.InstancePrefix,parent=>parent.ENTITY_TYPE=="DRILLING_DAILY_REPORT" || parent.PROCESS_INSTANCE_ID?.StartsWith(DrillingCompletionReviewStart.InstancePrefix,StringComparison.Ordinal)==true || DrillingCompletionReviewStart.HasPayload(parent.INSTANCE_DATA_JSON));
    internal Task RequireExecutionSourceTaskParentsAsync(string entity,IReadOnlyList<PROCESS_INSTANCE> parents,string connection)
        =>RequireSourceTaskParentsAsync(entity,parents,connection,"DRILLING_OPERATION",DrillingExecutionAssignment.InstancePrefix,parent=>parent.ENTITY_TYPE=="DRILLING_OPERATION" || parent.PROCESS_INSTANCE_ID?.StartsWith(DrillingExecutionAssignment.InstancePrefix,StringComparison.Ordinal)==true || DrillingExecutionAssignment.HasPayload(parent.INSTANCE_DATA_JSON));
    private async Task RequireSourceTaskParentsAsync(string entity,IReadOnlyList<PROCESS_INSTANCE> parents,string connection,string type,string prefix,Func<PROCESS_INSTANCE,bool> marker)
    {
        if(await ResolveConnectionAsync()!=connection)throw new InvalidOperationException("The reviewed task source binding changed.");
        var rows=(await GetTaskRepo(connection).GetAsync([new(){FieldName="ENTITY_ID",Operator = "=", FilterValue=entity}])).ToArray();
        if(rows.Length>4096 || rows.Any(value=>value is not CROSS_PERSONA_TASK))throw new InvalidOperationException("The complete reviewed source task population is ambiguous or exceeds its bound.");
        foreach(var task in rows.Cast<CROSS_PERSONA_TASK>())
        {
            if(task.ENTITY_ID!=entity)throw new InvalidOperationException("The reviewed task lookup returned another source.");var owners=parents.Where(parent=>parent.PROCESS_INSTANCE_ID==task.PROCESS_INSTANCE_ID).ToArray();
            if(task.ENTITY_TYPE!=type && task.PROCESS_INSTANCE_ID?.StartsWith(prefix,StringComparison.Ordinal)!=true && !owners.Any(marker))continue;
            if(owners.Length!=1 || !marker(owners[0]))throw new InvalidOperationException("Reviewed source task evidence has no unique original assignment parent and requires reconciliation.");
        }
    }

    internal async Task RequireAbsentExecutionAssignmentTaskKeysAsync(DrillingExecutionAssignment original,string connection)
    {
        if(await ResolveConnectionAsync()!=connection)throw new InvalidOperationException("The original reporting task binding changed.");
        var id=original.InstanceId();var step=DrillingExecutionAssignmentReceipt.StepId(id,original.Workflow.Step.StepId);var repo=GetTaskRepo(connection);
        if((await repo.GetAsync([new(){FieldName="PROCESS_STEP_INSTANCE_ID",Operator = "=", FilterValue=step}])).Any())
            throw new InvalidOperationException("Original reporting step tasks cannot become confirmed absence.");
        foreach(var persona in original.Workflow.Step.PersonaCodes)
            if((await repo.GetAsync([new(){FieldName="CROSS_TASK_ID",Operator = "=", FilterValue=DrillingExecutionAssignmentReceipt.TaskId(id,step,persona)}])).Any())
                throw new InvalidOperationException("Retagged original reporting task keys cannot become confirmed absence.");
    }
    internal async Task RequireAbsentCompletionReviewTaskKeysAsync(DrillingCompletionReviewStart original,string connection)
    {
        if(await ResolveConnectionAsync()!=connection)throw new InvalidOperationException("The original completion task binding changed.");
        var id=original.InstanceId();var repo=GetTaskRepo(connection);
        foreach(var stage in original.Workflow.Stages)
        {
            var step=DrillingCompletionReviewStartReceipt.StepId(id,stage.Definition.StepId);
            if((await repo.GetAsync([new(){FieldName="PROCESS_STEP_INSTANCE_ID",Operator = "=", FilterValue=step}])).Any())
                throw new InvalidOperationException("Original completion step tasks cannot become confirmed absence.");
        }
        var first=original.Workflow.Stages[0].Definition;var firstStep=DrillingCompletionReviewStartReceipt.StepId(id,first.StepId);
        foreach(var persona in first.PersonaCodes)
            if((await repo.GetAsync([new(){FieldName="CROSS_TASK_ID",Operator = "=", FilterValue=DrillingCompletionReviewStartReceipt.TaskId(id,firstStep,persona)}])).Any())
                throw new InvalidOperationException("Retagged original completion task keys cannot become confirmed absence.");
    }
    private PPDMGenericRepository GetTaskRepo(string connectionName) =>
        new(_editor, _commonColumnHandler, _defaults, _metadata,
            typeof(CROSS_PERSONA_TASK), connectionName, "CROSS_PERSONA_TASK", null);
}
