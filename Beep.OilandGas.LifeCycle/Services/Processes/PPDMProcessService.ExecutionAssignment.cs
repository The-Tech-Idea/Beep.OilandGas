using System.Data;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

public partial class PPDMProcessService
{
    private sealed class ExecutionAssignmentPersonas(DrillingExecutionAssignment original, Func<Task> guard) : IRolePersonaReader
    {
        public async Task<List<string>> GetActivePersonasAsync(string role)
        {
            await guard();
            if (role != original.Workflow.Step.AssignedRole) throw new InvalidOperationException("The original execution receiving role changed.");
            return original.Workflow.Step.PersonaCodes.Order(StringComparer.Ordinal).ToList();
        }
    }

    private static bool ExecutionAssignmentMarker(PROCESS_INSTANCE row) => row.ENTITY_TYPE == "DRILLING_OPERATION" ||
        row.PROCESS_INSTANCE_ID?.StartsWith(DrillingExecutionAssignment.InstancePrefix, StringComparison.Ordinal) == true ||
        DrillingExecutionAssignment.HasPayload(row.INSTANCE_DATA_JSON);
    private static string ExecutionAssignmentHistoryId(string id) => "execution-assignment-start-" + ProcessDefinitionRevision.FromJson(id);

    private async Task<PROCESS_INSTANCE?> FindExecutionAssignmentAsync(CompanyScope company, string field, string operation, CrossPersonaTaskRouter router)
    {
        var rows = (await GetProcessInstanceRepository().GetAsync(Key("ENTITY_ID", operation))).ToArray();
        if (rows.Length > 256 || rows.Any(row => row is not PROCESS_INSTANCE))
            throw new InvalidOperationException("The complete execution assignment population is unavailable or exceeds its bound.");
        await router.RequireExecutionSourceTaskParentsAsync(operation, rows.Cast<PROCESS_INSTANCE>().ToArray(), _connectionName);
        var found = new List<PROCESS_INSTANCE>();
        foreach (var row in rows.Cast<PROCESS_INSTANCE>())
        {
            if (row.ENTITY_ID != operation) throw new InvalidOperationException("Execution assignment lookup returned another source.");
            if (!ExecutionAssignmentMarker(row)) continue;
            if (DrillingExecutionAssignment.OwnerCompany(row.INSTANCE_DATA_JSON!) != company) continue;
            if (row.ENTITY_TYPE != "DRILLING_OPERATION" || row.FIELD_ID != field || row.ACTIVE_IND != "Y")
                throw new InvalidOperationException("The original company execution assignment is retagged or inactive and needs reconciliation.");
            var original = DrillingExecutionAssignment.FromPayload(row.INSTANCE_DATA_JSON!, row.PROCESS_INSTANCE_ID!, field, operation);
            if (original.Scope().LifecycleDatabase.ConnectionName != _connectionName)
                throw new InvalidOperationException("The original execution assignment lifecycle target changed.");
            found.Add(row);
        }
        if (found.Count > 1) throw new InvalidOperationException("This company execution has multiple reporting assignments.");
        return found.SingleOrDefault();
    }

    private async Task<string> ConfirmExecutionAssignmentInitialAsync(ProcessInstance process, DrillingExecutionAssignment original, CrossPersonaTaskRouter router,ReportingInitialSnapshot? initial=null)
    {
        original.Require();
        var id = original.InstanceId(); var scope = original.Scope(); var expected = original.Workflow.Step;
        if (process.InstanceId != id || process.EntityType != "DRILLING_OPERATION" || process.EntityId != original.OperationId ||
            process.FieldId != scope.FieldId || process.ProcessId != original.Workflow.ProcessId || process.StartedBy != scope.ActorUserId ||
            process.Status != ProcessStatus.IN_PROGRESS || process.CurrentState != "INITIAL" || process.CurrentStepId != expected.StepId ||
            process.CompletionDate is not null || !string.IsNullOrEmpty(process.CompletedBy) || process.StartDate.Kind == DateTimeKind.Local || process.StartDate < original.Source.CheckedAtUtc.UtcDateTime ||
            process.ProcessData is not { Data.Count: 1 } || !process.ProcessData.Data.ContainsKey(DrillingExecutionAssignment.PayloadKey) ||
            process.ProcessData.DataJson is not null || process.ProcessData.LastUpdated != process.StartDate ||
            DrillingExecutionAssignment.FromPayload(JsonSerializer.Serialize(process.ProcessData), id, scope.FieldId, original.OperationId).Revision() != original.Revision() ||
            process.StepInstances.Count != 1 || process.History.Count != 1)
            throw new InvalidOperationException("The execution reporting responsibility differs from its exact original source and initial graph.");
        var step = process.StepInstances[0]; var history = process.History[0];
        if (step.StepId != expected.StepId || step.StepInstanceId != DrillingExecutionAssignmentReceipt.StepId(id, expected.StepId) ||
            step.InstanceId != id || step.StepName != expected.StepName || step.RequiredRole != expected.AssignedRole ||
            step.SequenceNumber != expected.Sequence || step.ApprovalRequired || step.SlaHours != expected.SlaHours || step.Status != StepStatus.PENDING ||
            !string.IsNullOrEmpty(step.AssignedTo) || step.StartDate is not null || step.CompletionDate is not null ||
            !string.IsNullOrEmpty(step.CompletedBy) || !string.IsNullOrEmpty(step.Outcome) || !string.IsNullOrEmpty(step.Notes) || step.Approvals.Count != 0 ||
            JsonSerializer.Serialize(step.StepData) != JsonSerializer.Serialize(new PROCESS_STEP_DATA()) ||
            history.HistoryId != ExecutionAssignmentHistoryId(id) || history.InstanceId != id || history.Action != "PROCESS_STARTED" ||
            history.Timestamp != process.StartDate || history.PerformedBy != scope.ActorUserId || history.NewState != "INITIAL" ||
            !string.IsNullOrEmpty(history.StepInstanceId) || !string.IsNullOrEmpty(history.PreviousState) || history.ActionData is not { Count: 0 } ||
            history.Notes != "Recorded execution assigned for configured ongoing reporting")
            throw new InvalidOperationException("The original ongoing reporting step or assignment audit changed.");
        if (initial is null && (await GetProcessApprovalRepository().GetAsync(Key("PROCESS_INSTANCE_ID", id))).Any())
            throw new InvalidOperationException("A reporting responsibility cannot adopt unqualified approval evidence.");
        IReadOnlyList<CROSS_PERSONA_TASK> tasks = initial?.Tasks ?? await router.GetAllTasksForDevelopmentEvidenceAsync(id, _connectionName);
        if (tasks.Count != expected.PersonaCodes.Count ||
            !tasks.Select(task => task.TARGET_PERSONA_CODE).Order(StringComparer.Ordinal).SequenceEqual(expected.PersonaCodes.Order(StringComparer.Ordinal)))
            throw new InvalidOperationException("The complete original execution destination set is unavailable or ambiguous.");
        foreach (var task in tasks)
        {
            var stepId = step.StepInstanceId;
            var identity = DrillingExecutionAssignmentReceipt.TaskId(id,stepId,task.TARGET_PERSONA_CODE);
            var context = JsonSerializer.Serialize(new { assignedRole = expected.AssignedRole, entityType = "DRILLING_OPERATION", entityId = original.OperationId, processInstanceId = id, stepInstanceId = stepId, scopeKind = "FIELD" });
            if (task.CROSS_TASK_ID != identity || task.PROCESS_INSTANCE_ID != id || task.PROCESS_STEP_INSTANCE_ID != stepId ||
                task.ENTITY_TYPE != "DRILLING_OPERATION" || task.ENTITY_ID != original.OperationId ||
                task.ENTITY_DESCRIPTION != original.Source.Basis.Source.Program.PROGRAM_NAME || task.ASSIGNED_ROLE != expected.AssignedRole ||
                task.TASK_STATUS != "PENDING" || task.TASK_TYPE != "DATA_ENTRY" || task.ACTIVE_IND != "Y" ||
                task.ROUTE != WorkflowTaskRoute.For("FIELD", id, "DRILLING_OPERATION") || task.TASK_CONTEXT_JSON != context ||
                task.DUE_DATE is not null || task.PRIORITY != 3 || task.ROW_CREATED_BY != scope.ActorUserId ||
                task.ROW_CREATED_DATE is not {} created || created.Kind == DateTimeKind.Local || created < process.StartDate ||
                task.ROW_CHANGED_DATE is not {} changed || changed.Kind == DateTimeKind.Local || changed < created || task.ROW_CHANGED_BY != scope.ActorUserId)
                throw new InvalidOperationException("The exact execution reporting task, route, context or audit changed.");
        }
        if(initial is null)await router.ConfirmRecordedTaskSetAsync(tasks, _connectionName);
        var graph = initial is null?await ConfirmStartedGraphAsync(process):ConfirmStartedGraphSnapshot(process,[initial.Row],initial.Steps.Cast<object>().ToArray(),[initial.Start]);
        return JsonSerializer.Serialize(new { Graph = graph, Tasks = tasks.OrderBy(task => task.CROSS_TASK_ID, StringComparer.Ordinal) });
    }

    private async Task RequireAbsentExecutionAssignmentGraphAsync(DrillingExecutionAssignment original, CrossPersonaTaskRouter router)
    {
        var id=original.InstanceId();var step=DrillingExecutionAssignmentReceipt.StepId(id,original.Workflow.Step.StepId);
        if ((await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID", id))).Any() ||
            (await GetProcessStepInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID", id))).Any() ||
            (await GetProcessHistoryRepository().GetAsync(Key("PROCESS_INSTANCE_ID", id))).Any() ||
            (await GetProcessApprovalRepository().GetAsync(Key("PROCESS_INSTANCE_ID", id))).Any() ||
            (await router.GetAllTasksForDevelopmentEvidenceAsync(id, _connectionName)).Count > 0)
            throw new InvalidOperationException("Partial original assignment evidence cannot become confirmed absence.");
        if((await GetProcessStepInstanceRepository().GetAsync(Key("PROCESS_STEP_INSTANCE_ID",step))).Any() ||
            (await GetProcessHistoryRepository().GetAsync(Key("PROCESS_HISTORY_ID",ExecutionAssignmentHistoryId(id)))).Any() ||
            (await GetProcessApprovalRepository().GetAsync(Key("PROCESS_STEP_INSTANCE_ID",step))).Any())
            throw new InvalidOperationException("Retagged original assignment child evidence cannot become confirmed absence.");
        await router.RequireAbsentExecutionAssignmentTaskKeysAsync(original,_connectionName);
    }

    public async Task<DrillingExecutionAssignmentReceipt> RecordExecutionAssignmentAsync(DrillingExecutionAssignment input,
        Func<Task> authority, Func<Task<DrillingExecutionStatus>> readSource, bool recover = false)
    {
        input.Require(); ArgumentNullException.ThrowIfNull(authority); ArgumentNullException.ThrowIfNull(readSource);
        var original = CompanyInventorySourceReview.Copy(input); var scope = original.Scope(); var id = original.InstanceId();
        if (scope.LifecycleDatabase.ConnectionName != _connectionName) throw new InvalidOperationException("The original assignment lifecycle target is required.");
        var source = _editor.GetDataSource(_connectionName) ?? throw new InvalidOperationException("The original execution assignment source is unavailable.");
        async Task Guard(bool owned)
        {
            if (!ReferenceEquals(source, _editor.GetDataSource(_connectionName))) throw new InvalidOperationException("The original execution assignment target changed.");
            if (owned) PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable); else PPDMDataSourceTransaction.RequireIdle(source);
            await authority();
            if (owned) PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable); else PPDMDataSourceTransaction.RequireIdle(source);
        }
        var router = new CrossPersonaTaskRouter(_editor, _commonColumnHandler, _defaults, _metadata, () => Task.FromResult(_connectionName),
            personas: new ExecutionAssignmentPersonas(original, () => Guard(true)));
        await Guard(false); original.RequireSource(await readSource()); await Guard(false);
        ProcessInstance? process = null; IReadOnlyList<CROSS_PERSONA_TASK> tasks = []; string? graph = null;
        await PPDMDataSourceTransaction.ExecuteAsync(source, async () =>
        {
            await Guard(true);
            var rows = (await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID", id))).ToArray();
            if (rows.Length > 1 || rows.Any(row => row is not PROCESS_INSTANCE)) throw new InvalidOperationException("The original execution assignment identity is ambiguous.");
            if (rows.Length == 0) await RequireAbsentExecutionAssignmentGraphAsync(original, router);
            string? definitionJson = null;
            if (!recover)
            {
                var definitions = (await GetProcessDefinitionRepository().GetAsync(Key("PROCESS_DEFINITION_ID", original.Workflow.ProcessId))).ToArray();
                if (definitions.Length != 1 || definitions[0] is not PROCESS_DEFINITION definition || definition.ACTIVE_IND != "Y" || definition.IS_ACTIVE != "Y")
                    throw new InvalidOperationException("One active original execution reporting workflow is required.");
                var configured = ConvertToProcessDefinition(definition); var stage = DevelopmentReviewDefinition.RequireExecutionReporting(configured); var expected = original.Workflow;
                if (ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(configured)) != expected.DefinitionRevision ||
                    configured.ProcessName != expected.ProcessName || configured.Version != expected.Version ||
                    stage.StepId != expected.Step.StepId || stage.StepName != expected.Step.StepName || stage.SequenceNumber != expected.Step.Sequence ||
                    stage.RequiredRoles[0] != expected.Step.AssignedRole || stage.SlaHours != expected.Step.SlaHours)
                    throw new InvalidOperationException("The original configured execution reporting workflow changed.");
                definitionJson = JsonSerializer.Serialize(definition);
            }
            var existing = await FindExecutionAssignmentAsync(scope.Company, scope.FieldId, original.OperationId, router);
            if (rows.Length == 1)
            {
                if (existing?.PROCESS_INSTANCE_ID != id ||
                    DrillingExecutionAssignment.FromPayload(((PROCESS_INSTANCE)rows[0]).INSTANCE_DATA_JSON!, id, scope.FieldId, original.OperationId).Revision() != original.Revision())
                    throw new InvalidOperationException("The original execution assignment intent or source population changed.");
                process = await LoadProcessInstanceAsync(id) ?? throw new InvalidOperationException("The original execution assignment is unavailable.");
            }
            else
            {
                // Another original is a conflict even during recovery, never confirmed absence of a safe replacement.
                if (existing is not null) throw new InvalidOperationException("This execution already has a different reporting assignment. Reconcile its original.");
                if (!recover)
                {
                    var now = DateTime.UtcNow; var stage = original.Workflow.Step;
                    if (now < original.Source.CheckedAtUtc.UtcDateTime) throw new InvalidOperationException("The execution source check is in the future.");
                    var definition = new ProcessStepDefinition { StepId = stage.StepId, StepName = stage.StepName, SequenceNumber = stage.Sequence,
                        RequiredRoles = [stage.AssignedRole], RequiresApproval = false, StepType = "DATA_ENTRY", SlaHours = stage.SlaHours };
                    process = CreateInitialProcess([definition], original.Workflow.ProcessId, original.OperationId, "DRILLING_OPERATION", scope.FieldId,
                        scope.ActorUserId, id, now, step => DrillingExecutionAssignmentReceipt.StepId(id, step.StepId),
                        CompanyInventorySourceReview.Copy(new PROCESS_DATA { InstanceId = id, LastUpdated = now, Data = new() { [DrillingExecutionAssignment.PayloadKey] = original } }));
                    await PersistReviewedStartAsync(process, new() { HistoryId = ExecutionAssignmentHistoryId(id), InstanceId = id, Action = "PROCESS_STARTED",
                        NewState = "INITIAL", Timestamp = now, PerformedBy = scope.ActorUserId, Notes = "Recorded execution assigned for configured ongoing reporting" });
                    await router.RouteTaskInOwnedTransactionAsync(id, process.StepInstances[0].StepInstanceId, stage.AssignedRole, "DRILLING_OPERATION",
                        original.OperationId, original.Source.Basis.Source.Program.PROGRAM_NAME, scope.ActorUserId);
                }
            }
            if (process is not null)
            {
                graph = await ConfirmExecutionAssignmentAsync(process, original, router);
                tasks = (await router.GetAllTasksForDevelopmentEvidenceAsync(id, _connectionName))
                    .Where(task=>task.PROCESS_STEP_INSTANCE_ID==DrillingExecutionAssignmentReceipt.StepId(id,original.Workflow.Step.StepId))
                    .Select(CompanyInventorySourceReview.Copy).ToArray();
            }
            if (definitionJson is not null)
            {
                var closing = (await GetProcessDefinitionRepository().GetAsync(Key("PROCESS_DEFINITION_ID", original.Workflow.ProcessId))).ToArray();
                if (closing.Length != 1 || JsonSerializer.Serialize(closing[0]) != definitionJson) throw new InvalidOperationException("The original reporting workflow changed before commit.");
            }
            await Guard(true);
            if (process is not null && graph != await ConfirmExecutionAssignmentAsync(process, original, router)) throw new InvalidOperationException("The original execution assignment graph changed before commit.");
            await Guard(true);
        });
        await Guard(false);
        if (process is not null)
        {
            if (graph != await ConfirmExecutionAssignmentAsync(process, original, router)) throw new InvalidOperationException("The execution assignment commit is unconfirmed. Recover the retained original.");
        }
        else
        {
            await RequireAbsentExecutionAssignmentGraphAsync(original, router);
            if (await FindExecutionAssignmentAsync(scope.Company, scope.FieldId, original.OperationId, router) is not null)
                throw new InvalidOperationException("The original execution assignment absence changed during recovery.");
        }
        original.RequireSource(await readSource()); await Guard(false);
        if (process is not null)
        {
            if (graph != await ConfirmExecutionAssignmentAsync(process, original, router))
                throw new InvalidOperationException("The exact original assignment changed around closing execution-source readback.");
        }
        else
        {
            await RequireAbsentExecutionAssignmentGraphAsync(original, router);
            if (await FindExecutionAssignmentAsync(scope.Company, scope.FieldId, original.OperationId, router) is not null)
                throw new InvalidOperationException("The confirmed assignment absence changed around closing source readback.");
        }
        await Guard(false);
        var receipt = new DrillingExecutionAssignmentReceipt(original.Revision(), id, process is not null, process?.StepInstances[0].StepInstanceId,
            tasks.Select(task => task.CROSS_TASK_ID).Order(StringComparer.Ordinal).ToArray(),
            ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new { Intent = original.Revision(), Graph = graph })), DateTimeOffset.UtcNow);
        receipt.Require(original);
        return receipt;
    }

    public async Task<string?> FindRecordedExecutionAssignmentAsync(CompanyScope company, string field, string operation, Func<Task> authority)
    {
        APInvoiceOwnership.RequireScope(company);
        if (!LeaseSummaryValidator.Identifier(field) || !LeaseSummaryValidator.Identifier(operation))
            throw new InvalidOperationException("The exact original FIELD and recorded execution identifiers are required.");
        var source = _editor.GetDataSource(_connectionName) ?? throw new InvalidOperationException("The execution assignment source is unavailable.");
        var router = new CrossPersonaTaskRouter(_editor, _commonColumnHandler, _defaults, _metadata, () => Task.FromResult(_connectionName));
        async Task Guard(bool owned)
        {
            if (!ReferenceEquals(source, _editor.GetDataSource(_connectionName))) throw new InvalidOperationException("The original execution assignment source changed.");
            if (owned) PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable); else PPDMDataSourceTransaction.RequireIdle(source);
            await authority();
            if (owned) PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable); else PPDMDataSourceTransaction.RequireIdle(source);
        }
        string? id = null, graph = null; ProcessInstance? process = null; DrillingExecutionAssignment? original = null;
        await Guard(false);
        await PPDMDataSourceTransaction.ExecuteAsync(source, async () =>
        {
            await Guard(true);
            var row = await FindExecutionAssignmentAsync(company, field, operation, router); id = row?.PROCESS_INSTANCE_ID;
            if (row is not null)
            {
                original = DrillingExecutionAssignment.FromPayload(row.INSTANCE_DATA_JSON!, id!, field, operation);
                process = await LoadProcessInstanceAsync(id!) ?? throw new InvalidOperationException("The saved execution reporting assignment is unavailable.");
                graph = await ConfirmExecutionAssignmentAsync(process, original, router);
            }
            await Guard(true);
            if (id != (await FindExecutionAssignmentAsync(company, field, operation, router))?.PROCESS_INSTANCE_ID)
                throw new InvalidOperationException("The original execution assignment identity changed during lookup.");
        });
        await Guard(false);
        if (id != (await FindExecutionAssignmentAsync(company, field, operation, router))?.PROCESS_INSTANCE_ID ||
            process is not null && graph != await ConfirmExecutionAssignmentAsync(process, original!, router))
            throw new InvalidOperationException("The original execution assignment or confirmed absence changed during lookup.");
        await Guard(false);
        return id;
    }

    public async Task<(DrillingExecutionAssignment Original, ProcessInstance Process, IReadOnlyList<CROSS_PERSONA_TASK> Tasks, string NativeRevision)>
        ReadRecordedExecutionAssignmentAsync(CompanyScope company, string id, Func<Task> authority)
    {
        APInvoiceOwnership.RequireScope(company);
        if (!LeaseSummaryValidator.Identifier(id) || !id.StartsWith(DrillingExecutionAssignment.InstancePrefix, StringComparison.Ordinal))
            throw new InvalidOperationException("Choose an exact execution reporting assignment from the company inbox.");
        var source = _editor.GetDataSource(_connectionName) ?? throw new InvalidOperationException("The original execution assignment source is unavailable.");
        var router = new CrossPersonaTaskRouter(_editor, _commonColumnHandler, _defaults, _metadata, () => Task.FromResult(_connectionName));
        async Task Guard(bool owned)
        {
            if (!ReferenceEquals(source, _editor.GetDataSource(_connectionName))) throw new InvalidOperationException("The original execution assignment target changed.");
            if (owned) PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable); else PPDMDataSourceTransaction.RequireIdle(source);
            await authority();
            if (owned) PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable); else PPDMDataSourceTransaction.RequireIdle(source);
        }
        DrillingExecutionAssignment? original = null; ProcessInstance? process = null; IReadOnlyList<CROSS_PERSONA_TASK> tasks = []; string? graph = null;
        await Guard(false);
        await PPDMDataSourceTransaction.ExecuteAsync(source, async () =>
        {
            await Guard(true);
            var rows = (await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID", id))).ToArray();
            if (rows.Length != 1 || rows[0] is not PROCESS_INSTANCE row) throw new InvalidOperationException("The original execution assignment is missing or ambiguous.");
            if (DrillingExecutionAssignment.OwnerCompany(row.INSTANCE_DATA_JSON!) != company) throw new UnauthorizedAccessException("The execution assignment belongs to another selected company.");
            if (row.ENTITY_TYPE != "DRILLING_OPERATION" || row.ACTIVE_IND != "Y") throw new InvalidOperationException("The original execution assignment ownership changed.");
            original = DrillingExecutionAssignment.FromPayload(row.INSTANCE_DATA_JSON!, id, row.FIELD_ID!, row.ENTITY_ID!);
            if (original.Scope().LifecycleDatabase.ConnectionName != _connectionName) throw new InvalidOperationException("The original assignment lifecycle binding changed.");
            process = await LoadProcessInstanceAsync(id) ?? throw new InvalidOperationException("The original execution assignment is unavailable.");
            graph = await ConfirmExecutionAssignmentAsync(process, original, router);
            tasks = (await router.GetAllTasksForDevelopmentEvidenceAsync(id, _connectionName)).Select(CompanyInventorySourceReview.Copy).ToArray();
            await Guard(true);
            if ((await FindExecutionAssignmentAsync(company, process.FieldId, process.EntityId, router))?.PROCESS_INSTANCE_ID != id ||
                graph != await ConfirmExecutionAssignmentAsync(process, original, router)) throw new InvalidOperationException("The complete original execution assignment changed during readback.");
        });
        await Guard(false);
        if ((await FindExecutionAssignmentAsync(company, process!.FieldId, process.EntityId, router))?.PROCESS_INSTANCE_ID != id ||
            graph != await ConfirmExecutionAssignmentAsync(process, original!, router)) throw new InvalidOperationException("The original execution assignment changed after its read.");
        await Guard(false);
        return (CompanyInventorySourceReview.Copy(original!), CompanyInventorySourceReview.Copy(process), CompanyInventorySourceReview.Copy(tasks), ProcessDefinitionRevision.FromJson(graph!));
    }
}
