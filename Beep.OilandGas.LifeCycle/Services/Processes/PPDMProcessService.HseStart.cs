using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.HSE.Models;
using System.Data;
using System.Reflection;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.Core;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

public partial class PPDMProcessService
{
    private const string RcaHandoffKey = "CompanyHseRcaHandoff";
    private sealed record HseRcaHandoffEvidence(CompanyHseRcaHandoffCommand Command, string BusinessDigest,
        ProcessInstance Snapshot, PROCESS_HISTORY History, List<CROSS_PERSONA_TASK> Closed, List<CROSS_PERSONA_TASK> Next, string NextStepInstanceId);
    private static string HseRcaBusinessDigest(ProcessInstance instance)
    {
        var copy = JsonSerializer.Deserialize<ProcessInstance>(JsonSerializer.Serialize(instance))!;
        copy.ProcessData.Data.Remove(RcaHandoffKey);
        return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));
    }

    public async Task<CompanyHseRcaHandoffReceipt> CompleteRcaHandoffAsync(CompanyHseRcaHandoffCommand original,
        Func<string, Task> requireRoleAuthority, Func<Task> requireRecordedCause)
        => await CompleteRcaHandoffCoreAsync(original, requireRoleAuthority, requireRecordedCause, false);

    private async Task<CompanyHseRcaHandoffReceipt> CompleteRcaHandoffCoreAsync(CompanyHseRcaHandoffCommand original,
        Func<string, Task> requireRoleAuthority, Func<Task> requireRecordedCause, bool readOnly)
    {
        ArgumentNullException.ThrowIfNull(original); original.Require(); ArgumentNullException.ThrowIfNull(requireRoleAuthority); ArgumentNullException.ThrowIfNull(requireRecordedCause);
        var frozen = JsonSerializer.Deserialize<CompanyHseRcaHandoffCommand>(JsonSerializer.Serialize(original))!; var review = frozen.Review;
        if (_personaReader is null || review.LifecycleDatabase.ConnectionName != _connectionName) throw new InvalidOperationException("The reviewed lifecycle owner is unavailable.");
        var source = _editor.GetDataSource(_connectionName) ?? throw new InvalidOperationException("The lifecycle datasource is unavailable.");
        var router = new CrossPersonaTaskRouter(_editor, _commonColumnHandler, _defaults, _metadata, () => Task.FromResult(_connectionName), personas: _personaReader);
        async Task Guard(bool owned)
        {
            if (!ReferenceEquals(source, _editor.GetDataSource(_connectionName))) throw new InvalidOperationException("The lifecycle datasource changed.");
            if (owned) PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable); else PPDMDataSourceTransaction.RequireIdle(source);
            await requireRoleAuthority(review.AssignedRole);
            if (owned) PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable); else PPDMDataSourceTransaction.RequireIdle(source);
        }
        async Task<string> Graph()
        {
            var instance = await LoadProcessInstanceAsync(review.InstanceId) ?? throw new InvalidOperationException("The RCA workflow is unavailable.");
            var definition = await GetProcessDefinitionAsync(CompanyHseWorkflowStart.ProcessId) ?? throw new InvalidOperationException("The RCA definition is unavailable.");
            var tasks = await router.GetAllTasksForDevelopmentEvidenceAsync(review.InstanceId, _connectionName);
            return JsonSerializer.Serialize(new { Instance = instance, Definition = definition, Tasks = tasks.OrderBy(task => task.CROSS_TASK_ID, StringComparer.Ordinal) });
        }
        async Task Confirm(HseRcaHandoffEvidence evidence, bool allowProgress)
        {
            if (JsonSerializer.Serialize(evidence.Command) != JsonSerializer.Serialize(frozen)) throw new InvalidOperationException("The recorded RCA handoff differs from the original request.");
            var instance = await LoadProcessInstanceAsync(review.InstanceId) ?? throw new InvalidOperationException("The recorded RCA workflow is unavailable.");
            var step = instance.StepInstances.SingleOrDefault(item => item.StepInstanceId == review.StepInstanceId);
            var next = instance.StepInstances.SingleOrDefault(item => item.StepInstanceId == evidence.NextStepInstanceId);
            var recorded = evidence.Snapshot ?? throw new InvalidOperationException("The recorded RCA handoff snapshot is unavailable.");
            var recordedStep = recorded.StepInstances.SingleOrDefault(item => item.StepInstanceId == review.StepInstanceId);
            var recordedNext = recorded.StepInstances.SingleOrDefault(item => item.StepInstanceId == evidence.NextStepInstanceId);
            var historyId = "hse-rca-" + ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new { review.InstanceId, frozen.IntentId }));
            if (HseRcaBusinessDigest(recorded) != evidence.BusinessDigest || recorded.InstanceId != review.InstanceId ||
                recorded.ProcessId != CompanyHseWorkflowStart.ProcessId || recorded.EntityId != review.Cause.IncidentId || recorded.EntityType != CompanyHseWorkflowStart.EntityType ||
                recorded.FieldId != review.Context.FieldId || recordedStep is null || recordedNext is null || recorded.CurrentStepId != recordedNext.StepId || recordedNext.Status != StepStatus.PENDING ||
                instance.ProcessId != recorded.ProcessId || instance.EntityId != recorded.EntityId || instance.EntityType != recorded.EntityType || instance.FieldId != recorded.FieldId ||
                step is null || JsonSerializer.Serialize(step) != JsonSerializer.Serialize(recordedStep) || step.StepId != "INC_RCA" || step.Status != StepStatus.COMPLETED ||
                step.CompletedBy != review.Context.ActorUserId || step.Outcome != "RCA_RECORDED" || instance.CurrentStepId == "INC_RCA" ||
                next is null || recordedNext.SequenceNumber <= recordedStep.SequenceNumber || next.StepId != recordedNext.StepId ||
                next.InstanceId != recordedNext.InstanceId || next.SequenceNumber != recordedNext.SequenceNumber || next.StepName != recordedNext.StepName ||
                next.RequiredRole != recordedNext.RequiredRole || next.SlaHours != recordedNext.SlaHours || next.ApprovalRequired != recordedNext.ApprovalRequired ||
                next.Status is not (StepStatus.PENDING or StepStatus.IN_PROGRESS or StepStatus.COMPLETED or StepStatus.SKIPPED or StepStatus.FAILED) ||
                (next.Status is StepStatus.PENDING or StepStatus.IN_PROGRESS && instance.CurrentStepId != next.StepId) || !LeaseSummaryValidator.Identifier(next.RequiredRole) ||
                evidence.History.PROCESS_HISTORY_ID != historyId || evidence.History.DETAILS != "Recorded cause " + review.Cause.IntentId.ToString("N") ||
                evidence.History.PROCESS_INSTANCE_ID != review.InstanceId || evidence.History.PROCESS_STEP_INSTANCE_ID != review.StepInstanceId ||
                evidence.History.EVENT_TYPE != "RCA_HANDED_OFF" || evidence.History.USER_ID != review.Context.ActorUserId || evidence.Closed.Count == 0 || evidence.Next.Count == 0 ||
                evidence.Closed.Any(task => task.TASK_STATUS != "COMPLETED" || task.ASSIGNED_ROLE != review.AssignedRole || task.ROW_CHANGED_BY != review.Context.ActorUserId) ||
                evidence.Next.Any(task => task.TASK_STATUS != "PENDING" || task.PROCESS_STEP_INSTANCE_ID != evidence.NextStepInstanceId || task.ASSIGNED_ROLE != next.RequiredRole || task.ROW_CREATED_BY != review.Context.ActorUserId))
                throw new InvalidOperationException("The RCA handoff graph or audit is unconfirmed.");
            if (!allowProgress && (HseRcaBusinessDigest(instance) != evidence.BusinessDigest || instance.CurrentStepId != next.StepId || next.Status != StepStatus.PENDING ||
                !evidence.Next.Select(task => task.TARGET_PERSONA_CODE).Order(StringComparer.Ordinal).SequenceEqual((await _personaReader!.GetActivePersonasAsync(next.RequiredRole!)).Order(StringComparer.Ordinal), StringComparer.Ordinal)))
                throw new InvalidOperationException("The initial RCA handoff graph changed.");
            await router.ConfirmRecordedTaskSetAsync(evidence.Closed, _connectionName);
            if (allowProgress) await router.ConfirmProgressedTaskSetAsync(evidence.Next, _connectionName);
            else await router.ConfirmRecordedTaskSetAsync(evidence.Next, _connectionName);
            var history = (await GetProcessHistoryRepository().GetAsync(Key("PROCESS_HISTORY_ID", evidence.History.PROCESS_HISTORY_ID))).ToArray();
            if (history.Length != 1 || JsonSerializer.Serialize(history[0]) != JsonSerializer.Serialize(evidence.History)) throw new InvalidOperationException("The RCA handoff history is unconfirmed.");
        }
        static void CopyBusiness<T>(T target, T converted) where T : ModelEntityBase
        { foreach (var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)) if (property.CanWrite) property.SetValue(target, property.GetValue(converted)); }
        async Task Update(ModelEntityBase row, PPDMGenericRepository repository, string key, string id, string creator, DateTime? created)
        {
            await repository.UpdateAsync(row, review.Context.ActorUserId); var recorded = (await repository.GetAsync(Key(key, id))).ToArray();
            if (row.ROW_CREATED_BY != creator || row.ROW_CREATED_DATE != created || row.ROW_CHANGED_BY != review.Context.ActorUserId || row.ROW_CHANGED_DATE is null ||
                recorded.Length != 1 || JsonSerializer.Serialize(recorded[0]) != JsonSerializer.Serialize((object)row))
                throw new InvalidOperationException("The RCA lifecycle update is unconfirmed.");
        }
        await Guard(false); await requireRecordedCause(); await Guard(false);
        HseRcaHandoffEvidence? proof = null; string? graph = null; var existingProof = false;
        await PPDMDataSourceTransaction.ExecuteAsync(source, async () =>
        {
            await Guard(true);
            var instance = await LoadProcessInstanceAsync(review.InstanceId) ?? throw new InvalidOperationException("The RCA workflow is unavailable.");
            if (instance.ProcessData.Data.TryGetValue(RcaHandoffKey, out var saved))
            {
                proof = JsonSerializer.Deserialize<HseRcaHandoffEvidence>(JsonSerializer.Serialize(saved)) ?? throw new InvalidOperationException("The recorded RCA handoff is unavailable.");
                existingProof = true; await Confirm(proof, true); graph = await Graph(); await Guard(true); return;
            }
            if (readOnly) throw new InvalidOperationException("The original RCA handoff is not recorded. Recovery cannot create it.");
            var currentReview = await ReviewRcaHandoffCoreAsync(review.Context, review.LifecycleDatabase, review.Cause, requireRoleAuthority, true);
            if (JsonSerializer.Serialize(currentReview) != JsonSerializer.Serialize(review)) throw new InvalidOperationException("The RCA handoff review changed before submission.");
            var definition = await GetProcessDefinitionAsync(CompanyHseWorkflowStart.ProcessId) ?? throw new InvalidOperationException("The incident definition is unavailable.");
            var ordered = definition.Steps.OrderBy(step => step.SequenceNumber).ToArray(); var position = Array.FindIndex(ordered, step => step.StepId == "INC_RCA");
            if (position < 0 || position + 1 >= ordered.Length || ordered[position].NextStepId != ordered[position + 1].StepId ||
                ordered[position + 1].RequiredRoles is not { Count: 1 } || !LeaseSummaryValidator.Identifier(ordered[position + 1].RequiredRoles[0]))
                throw new InvalidOperationException("The RCA workflow requires an explicit next step and assigned role.");
            var current = instance.StepInstances.Single(step => step.StepInstanceId == review.StepInstanceId);
            var next = instance.StepInstances.Single(step => step.StepId == ordered[position + 1].StepId);
            if (next.Status != StepStatus.BLOCKED || next.RequiredRole != ordered[position + 1].RequiredRoles[0]) throw new InvalidOperationException("The next RCA workflow assignment changed.");
            var closed = await router.CompleteTaskSetInOwnedTransactionAsync(review.InstanceId, review.StepInstanceId, review.AssignedRole,
                CompanyHseWorkflowStart.EntityType, review.Cause.IncidentId, review.Context.ActorUserId, _connectionName);
            await Guard(true); var now = DateTime.UtcNow;
            current.Status = StepStatus.COMPLETED; current.StartDate ??= now; current.CompletionDate = now; current.CompletedBy = review.Context.ActorUserId; current.Outcome = "RCA_RECORDED";
            next.Status = StepStatus.PENDING; instance.CurrentStepId = next.StepId; instance.CurrentState = next.StepId; instance.ProcessData.LastUpdated = now;
            var history = new ProcessHistoryEntry { HistoryId = "hse-rca-" + ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new { review.InstanceId, frozen.IntentId })),
                InstanceId = review.InstanceId, StepInstanceId = review.StepInstanceId, Action = "RCA_HANDED_OFF", PreviousState = "INC_RCA", NewState = next.StepId,
                Timestamp = now, PerformedBy = review.Context.ActorUserId, Notes = "Recorded cause " + review.Cause.IntentId.ToString("N") };
            var historyRow = ConvertToProcessHistoryEntity(history); historyRow.ROW_CREATED_BY = review.Context.ActorUserId; historyRow.ROW_CREATED_DATE = now;
            if ((await GetProcessHistoryRepository().GetAsync(Key("PROCESS_HISTORY_ID", historyRow.PROCESS_HISTORY_ID))).Any()) throw new InvalidOperationException("The RCA handoff history identity already exists.");
            await GetProcessHistoryRepository().InsertAsync(historyRow, review.Context.ActorUserId); instance.History.Add(history);
            var processRow = (PROCESS_INSTANCE)(await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID", review.InstanceId))).Single();
            var creator = processRow.ROW_CREATED_BY; var created = processRow.ROW_CREATED_DATE;
            CopyBusiness(processRow, ConvertToProcessInstanceEntity(instance)); processRow.ROW_CHANGED_BY = review.Context.ActorUserId; processRow.ROW_CHANGED_DATE = now;
            await Update(processRow, GetProcessInstanceRepository(), "PROCESS_INSTANCE_ID", review.InstanceId, creator, created);
            foreach (var changed in new[] { current, next })
            {
                var row = (PROCESS_STEP_INSTANCE)(await GetProcessStepInstanceRepository().GetAsync(Key("PROCESS_STEP_INSTANCE_ID", changed.StepInstanceId))).Single();
                var stepCreator = row.ROW_CREATED_BY; var stepCreated = row.ROW_CREATED_DATE;
                CopyBusiness(row, ConvertToProcessStepInstanceEntity(changed)); row.ROW_CHANGED_BY = review.Context.ActorUserId; row.ROW_CHANGED_DATE = now;
                await Update(row, GetProcessStepInstanceRepository(), "PROCESS_STEP_INSTANCE_ID", changed.StepInstanceId, stepCreator, stepCreated);
            }
            var routed = await router.RouteTaskInOwnedTransactionAsync(review.InstanceId, next.StepInstanceId, next.RequiredRole!, CompanyHseWorkflowStart.EntityType,
                review.Cause.IncidentId, null, review.Context.ActorUserId);
            var snapshot = JsonSerializer.Deserialize<ProcessInstance>(JsonSerializer.Serialize(instance))!;
            proof = new(frozen, HseRcaBusinessDigest(snapshot), snapshot, historyRow, closed, routed, next.StepInstanceId);
            instance.ProcessData.Data[RcaHandoffKey] = proof; processRow.INSTANCE_DATA_JSON = JsonSerializer.Serialize(instance.ProcessData);
            processRow.ROW_CHANGED_BY = review.Context.ActorUserId; processRow.ROW_CHANGED_DATE = now;
            await Update(processRow, GetProcessInstanceRepository(), "PROCESS_INSTANCE_ID", review.InstanceId, creator, created);
            await Confirm(proof, false); graph = await Graph(); await Guard(true);
            if (graph != await Graph()) throw new InvalidOperationException("The RCA lifecycle graph changed before commit.");
        });
        if (proof is null || graph is null || !ReferenceEquals(source, _editor.GetDataSource(_connectionName)) || graph != await Graph()) throw new InvalidOperationException("The RCA lifecycle commit is unconfirmed. Retain the original request.");
        await Confirm(proof, existingProof); await Guard(false); await requireRecordedCause(); await Guard(false); await Confirm(proof, existingProof);
        if (graph != await Graph()) throw new InvalidOperationException("The RCA handoff changed after source confirmation.");
        var receipt = new CompanyHseRcaHandoffReceipt(review.Context.Company, review.Context.ActorUserId, frozen.IntentId, review.InstanceId, review.StepInstanceId,
            proof.History.PROCESS_HISTORY_ID, proof.NextStepInstanceId, proof.Closed.Select(task => task.CROSS_TASK_ID).Order(StringComparer.Ordinal).ToArray(),
            proof.Next.Select(task => task.CROSS_TASK_ID).Order(StringComparer.Ordinal).ToArray(), proof.BusinessDigest);
        receipt.Require(frozen); return receipt;
    }

    /// <summary>Read a previously recorded handoff; this path cannot create a lifecycle transition.</summary>
    public async Task<CompanyHseRcaRecoveredHandoff?> RecoverRcaHandoffAsync(FieldSelectionReview context, AssetDatabaseScope lifecycle, string incidentId,
        Func<string, Task> requireRoleAuthority, Func<IncidentRcaRecordedCause, Task> requireRecordedCause)
    {
        ArgumentNullException.ThrowIfNull(context); ArgumentNullException.ThrowIfNull(lifecycle); ArgumentNullException.ThrowIfNull(requireRoleAuthority); ArgumentNullException.ThrowIfNull(requireRecordedCause);
        FieldSelectionReviewValidator.RequireReview(context, context.Company, context.ActorUserId);
        if (!LeaseSummaryValidator.Identifier(incidentId) || lifecycle.ConnectionName != _connectionName) throw new InvalidOperationException("The incident and lifecycle binding are required.");
        var source = _editor.GetDataSource(_connectionName) ?? throw new InvalidOperationException("The lifecycle datasource is unavailable.");
        PPDMDataSourceTransaction.RequireIdle(source);
        var candidates = await GetProcessInstancesForEntityAsync(incidentId, CompanyHseWorkflowStart.EntityType) ?? throw new InvalidOperationException("The incident workflow population is unavailable.");
        var owned = new List<ProcessInstance>();
        foreach (var instance in candidates)
        {
            if (instance is null || instance.ProcessData is null || instance.ProcessId != CompanyHseWorkflowStart.ProcessId || instance.EntityId != incidentId || instance.EntityType != CompanyHseWorkflowStart.EntityType)
                throw new InvalidOperationException("The incident workflow ownership is ambiguous.");
            var start = CompanyHseWorkflowStart.FromPayload(JsonSerializer.Serialize(instance.ProcessData), instance.InstanceId, instance.FieldId, incidentId);
            if (start.Context.Company != context.Company || start.Context.FieldId != context.FieldId) continue;
            if (start.Context.Database != context.Database || start.LifecycleDatabase != lifecycle) throw new InvalidOperationException("The recorded workflow belongs to another database binding.");
            owned.Add(instance);
        }
        if (owned.Count > 1) throw new InvalidOperationException("More than one incident workflow belongs to this FIELD.");
        if (owned.Count == 0 || !owned[0].ProcessData.Data.TryGetValue(RcaHandoffKey, out var saved))
        {
            var snapshot = JsonSerializer.Serialize(candidates);
            var closing = await GetProcessInstancesForEntityAsync(incidentId, CompanyHseWorkflowStart.EntityType);
            if (!ReferenceEquals(source, _editor.GetDataSource(_connectionName)) || closing is null || JsonSerializer.Serialize(closing) != snapshot)
                throw new InvalidOperationException("The incident workflow changed during RCA recovery.");
            PPDMDataSourceTransaction.RequireIdle(source); return null;
        }
        var evidence = JsonSerializer.Deserialize<HseRcaHandoffEvidence>(JsonSerializer.Serialize(saved)) ?? throw new InvalidOperationException("The recorded RCA handoff is unavailable.");
        var original = evidence.Command ?? throw new InvalidOperationException("The recorded RCA handoff original is unavailable."); original.Require();
        var prior = original.Review.Context;
        if (prior.Company != context.Company || prior.ActorUserId != context.ActorUserId || prior.Database != context.Database || prior.FieldId != context.FieldId ||
            original.Review.LifecycleDatabase != lifecycle || original.Review.InstanceId != owned[0].InstanceId || original.Review.Cause.IncidentId != incidentId)
            throw new UnauthorizedAccessException("The recorded RCA handoff belongs to another account, FIELD or binding.");
        await requireRoleAuthority(original.Review.AssignedRole);
        var receipt = await CompleteRcaHandoffCoreAsync(original, requireRoleAuthority, () => requireRecordedCause(original.Review.Cause), true);
        if (!ReferenceEquals(source, _editor.GetDataSource(_connectionName))) throw new InvalidOperationException("The lifecycle datasource changed during recovery.");
        var result = new CompanyHseRcaRecoveredHandoff(original, receipt); result.Require(); return result;
    }
    /// <summary>Review a recorded cause against the one currently assigned RCA step; never advances earlier steps.</summary>
    public async Task<CompanyHseRcaHandoffReview> ReviewRcaHandoffAsync(FieldSelectionReview context, AssetDatabaseScope lifecycle,
        IncidentRcaRecordedCause cause, Func<string, Task> requireRoleAuthority)
        => await ReviewRcaHandoffCoreAsync(context, lifecycle, cause, requireRoleAuthority, false);

    private async Task<CompanyHseRcaHandoffReview> ReviewRcaHandoffCoreAsync(FieldSelectionReview context, AssetDatabaseScope lifecycle,
        IncidentRcaRecordedCause cause, Func<string, Task> requireRoleAuthority, bool owned)
    {
        ArgumentNullException.ThrowIfNull(context); ArgumentNullException.ThrowIfNull(lifecycle); ArgumentNullException.ThrowIfNull(cause); ArgumentNullException.ThrowIfNull(requireRoleAuthority);
        cause.Require(); FieldSelectionReviewValidator.RequireReview(context, context.Company, context.ActorUserId);
        if (_personaReader is null || lifecycle.ConnectionName != _connectionName || cause.Company != context.Company ||
            cause.Database != context.Database || cause.FieldId != context.FieldId)
            throw new InvalidOperationException("The current company, recorded cause and lifecycle owner are required.");
        var source = _editor.GetDataSource(_connectionName) ?? throw new InvalidOperationException("The lifecycle datasource is unavailable.");
        var router = new CrossPersonaTaskRouter(_editor, _commonColumnHandler, _defaults, _metadata, () => Task.FromResult(_connectionName), personas: _personaReader);
        async Task Guard(string role)
        {
            if (!ReferenceEquals(source, _editor.GetDataSource(_connectionName))) throw new InvalidOperationException("The lifecycle datasource changed.");
            if (owned) PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable); else PPDMDataSourceTransaction.RequireIdle(source);
            await requireRoleAuthority(role);
            if (owned) PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable); else PPDMDataSourceTransaction.RequireIdle(source);
        }
        async Task<(string Graph, string Instance, string Step, string Role)> Capture()
        {
            var definition = await GetProcessDefinitionAsync(CompanyHseWorkflowStart.ProcessId) ?? throw new InvalidOperationException("The incident workflow definition is unavailable.");
            if (!definition.IsActive || definition.ProcessId != CompanyHseWorkflowStart.ProcessId || definition.ProcessType != "HSE" || definition.EntityType != CompanyHseWorkflowStart.EntityType ||
                definition.Steps is not { Count: > 0 and <= 64 } || definition.Steps.Any(step => step is null || !LeaseSummaryValidator.Identifier(step.StepId) || step.RequiredRoles is not { Count: 1 } ||
                    !LeaseSummaryValidator.Identifier(step.RequiredRoles[0]) || step.SequenceNumber < 0 || step.SlaHours < 0) ||
                definition.Steps.Select(step => step.StepId).Distinct(StringComparer.Ordinal).Count() != definition.Steps.Count ||
                definition.Steps.Select(step => step.SequenceNumber).Distinct().Count() != definition.Steps.Count)
                throw new InvalidOperationException("The incident workflow definition is incomplete or ambiguous.");
            var definitionJson = JsonSerializer.Serialize(definition);
            var candidates = await GetProcessInstancesForEntityAsync(cause.IncidentId, CompanyHseWorkflowStart.EntityType)
                ?? throw new InvalidOperationException("The incident workflow population is unavailable.");
            var owned = new List<ProcessInstance>();
            foreach (var instance in candidates)
            {
                if (instance is null || instance.ProcessData is null || instance.ProcessId != CompanyHseWorkflowStart.ProcessId || instance.EntityId != cause.IncidentId || instance.EntityType != CompanyHseWorkflowStart.EntityType)
                    throw new InvalidOperationException("The incident workflow ownership is ambiguous.");
                var start = CompanyHseWorkflowStart.FromPayload(JsonSerializer.Serialize(instance.ProcessData), instance.InstanceId, instance.FieldId, cause.IncidentId);
                if (start.Context.Company != context.Company || start.Context.FieldId != context.FieldId) continue;
                if (start.Context.Database != context.Database || start.LifecycleDatabase != lifecycle || start.DefinitionRevision != ProcessDefinitionRevision.FromJson(definitionJson))
                    throw new InvalidOperationException("The incident workflow belongs to another binding or definition.");
                owned.Add(instance);
            }
            if (owned.Count != 1) throw new InvalidOperationException("Exactly one current incident workflow is required.");
            var current = owned[0];
            if (current.Status != ProcessStatus.IN_PROGRESS || current.CurrentStepId != "INC_RCA") throw new InvalidOperationException("The RCA step is not currently assigned. Complete earlier steps before handing off a cause.");
            if (current.StepInstances.Count != definition.Steps.Count || current.StepInstances.Select(step => step.StepId).Distinct(StringComparer.Ordinal).Count() != current.StepInstances.Count ||
                definition.Steps.Any(template => current.StepInstances.Count(step => step.StepId == template.StepId && step.SequenceNumber == template.SequenceNumber) != 1))
                throw new InvalidOperationException("The incident workflow steps differ from the recorded definition.");
            var templates = definition.Steps.Where(step => step.StepId == "INC_RCA").ToArray();
            var steps = current.StepInstances.Where(step => step.StepId == "INC_RCA").ToArray();
            if (templates.Length != 1 || steps.Length != 1 || steps[0].Status is not (StepStatus.PENDING or StepStatus.IN_PROGRESS) ||
                steps[0].InstanceId != current.InstanceId || !LeaseSummaryValidator.Identifier(steps[0].StepInstanceId) ||
                steps[0].RequiredRole != templates[0].RequiredRoles[0] || (!string.IsNullOrEmpty(steps[0].AssignedTo) && steps[0].AssignedTo != context.ActorUserId) ||
                current.StepInstances.Any(step => step.SequenceNumber < steps[0].SequenceNumber && step.Status is not (StepStatus.COMPLETED or StepStatus.SKIPPED)) ||
                steps[0].ApprovalRequired || templates[0].RequiresApproval || templates[0].StepType != "ACTION" ||
                templates[0].ConditionalNextSteps is not { Count: 0 } || templates[0].ValidationRules is not { Count: 0 } || templates[0].StepConfiguration is not { Count: 0 })
                throw new InvalidOperationException("The current RCA step or its policy needs explicit review.");
            var ordered = definition.Steps.OrderBy(step => step.SequenceNumber).ToArray(); var position = Array.FindIndex(ordered, step => step.StepId == "INC_RCA");
            if (position < 0 || position + 1 >= ordered.Length || templates[0].NextStepId != ordered[position + 1].StepId)
                throw new InvalidOperationException("The RCA workflow requires an explicitly configured next step and role.");
            var next = current.StepInstances.Single(step => step.StepId == ordered[position + 1].StepId);
            if (next.Status != StepStatus.BLOCKED || next.RequiredRole != ordered[position + 1].RequiredRoles[0])
                throw new InvalidOperationException("The next RCA workflow assignment changed.");
            var role = steps[0].RequiredRole!; await Guard(role);
            var graph = JsonSerializer.Serialize(current);
            var tasks = (await router.GetAllTasksForDevelopmentEvidenceAsync(current.InstanceId, _connectionName)).Where(task => task.PROCESS_STEP_INSTANCE_ID == steps[0].StepInstanceId).ToArray();
            var personas = await _personaReader.GetActivePersonasAsync(role);
            var nextPersonas = await _personaReader.GetActivePersonasAsync(next.RequiredRole!);
            if (tasks.Length == 0 || tasks.Any(task => task.ACTIVE_IND != "Y" || task.TASK_STATUS is not ("PENDING" or "IN_PROGRESS") || task.ASSIGNED_ROLE != role ||
                task.ENTITY_TYPE != CompanyHseWorkflowStart.EntityType || task.ENTITY_ID != cause.IncidentId || !LeaseSummaryValidator.Identifier(task.TARGET_PERSONA_CODE)) ||
                tasks.Select(task => task.TARGET_PERSONA_CODE).Distinct(StringComparer.Ordinal).Count() != tasks.Length ||
                personas.Count == 0 || nextPersonas.Count == 0 || nextPersonas.Any(item => !LeaseSummaryValidator.Identifier(item)) ||
                nextPersonas.Distinct(StringComparer.Ordinal).Count() != nextPersonas.Count ||
                !tasks.Select(task => task.TARGET_PERSONA_CODE).Order(StringComparer.Ordinal).SequenceEqual(personas.Order(StringComparer.Ordinal), StringComparer.Ordinal))
                throw new InvalidOperationException("The current RCA role and persona tasks are unavailable or ambiguous.");
            var taskJson = JsonSerializer.Serialize(tasks.OrderBy(task => task.CROSS_TASK_ID, StringComparer.Ordinal));
            await Guard(role);
            return (ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new { Definition = definitionJson, Process = graph, Tasks = taskJson, NextPersonas = nextPersonas.Order(StringComparer.Ordinal) })), current.InstanceId, steps[0].StepInstanceId, role);
        }
        var first = await Capture(); var second = await Capture();
        if (first != second) throw new InvalidOperationException("The RCA workflow or assignment changed during review.");
        var result = new CompanyHseRcaHandoffReview(context, lifecycle, cause, second.Instance, second.Step, second.Role, second.Graph);
        result.Require(); return result;
    }

    public async Task<CompanyHseWorkflowReceipt> StartHseWorkflowAsync(CompanyHseWorkflowStart original,
        Func<Task> requireCurrentAuthority, Func<Task<string>> readIncidentRevision)
    {
        ArgumentNullException.ThrowIfNull(original); original.Require();
        ArgumentNullException.ThrowIfNull(requireCurrentAuthority); ArgumentNullException.ThrowIfNull(readIncidentRevision);
        var frozen = JsonSerializer.Deserialize<CompanyHseWorkflowStart>(JsonSerializer.Serialize(original))!;
        var id = frozen.InstanceId();
        if (_personaReader is null || _connectionName != frozen.LifecycleDatabase.ConnectionName)
            throw new InvalidOperationException("The reviewed lifecycle datasource and persona reader are required.");
        var source = _editor.GetDataSource(_connectionName) ?? throw new InvalidOperationException("The lifecycle datasource is unavailable.");
        var router = new CrossPersonaTaskRouter(_editor, _commonColumnHandler, _defaults, _metadata, () => Task.FromResult(_connectionName), personas: _personaReader);
        async Task Guard(bool owned)
        {
            if (!ReferenceEquals(source, _editor.GetDataSource(_connectionName))) throw new InvalidOperationException("The lifecycle datasource changed.");
            if (owned) PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable); else PPDMDataSourceTransaction.RequireIdle(source);
            await requireCurrentAuthority();
            if (owned) PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable); else PPDMDataSourceTransaction.RequireIdle(source);
        }
        async Task RequireIncident()
        {
            if (await readIncidentRevision() != frozen.IncidentRevision)
                throw new InvalidOperationException("The reviewed incident changed before workflow confirmation.");
        }
        await Guard(false); await RequireIncident(); await Guard(false);
        ProcessInstance? instance = null; List<CROSS_PERSONA_TASK> tasks = []; string? graph = null;
        await PPDMDataSourceTransaction.ExecuteAsync(source, async () =>
        {
            await Guard(true);
            var definitions = (await GetProcessDefinitionRepository().GetAsync(Key("PROCESS_DEFINITION_ID", CompanyHseWorkflowStart.ProcessId))).ToList();
            if (definitions.Count != 1 || definitions[0] is not PROCESS_DEFINITION native || native.PROCESS_DEFINITION_ID != CompanyHseWorkflowStart.ProcessId ||
                native.ACTIVE_IND != "Y" || native.IS_ACTIVE != "Y" || native.ENTITY_TYPE != CompanyHseWorkflowStart.EntityType || native.PROCESS_TYPE != "HSE")
                throw new InvalidOperationException("One active HSE incident workflow definition is required.");
            var snapshot = JsonSerializer.Serialize(native); var definition = ConvertToProcessDefinition(native);
            if (ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition)) != frozen.DefinitionRevision || string.IsNullOrWhiteSpace(definition.ProcessName))
                throw new InvalidOperationException("The reviewed HSE workflow definition changed.");
            var steps = definition.Steps?.OrderBy(step => step.SequenceNumber).ToList() ?? throw new InvalidOperationException("HSE steps are unavailable.");
            if (steps.Count is < 1 or > 64 || steps.Any(step => step is null || !LeaseSummaryValidator.Identifier(step.StepId) ||
                step.SequenceNumber < 0 || step.SlaHours < 0 || step.RequiredRoles is null || step.RequiredRoles.Count != 1 ||
                !LeaseSummaryValidator.Identifier(step.RequiredRoles[0])) || steps.Select(step => step.StepId).Distinct(StringComparer.Ordinal).Count() != steps.Count ||
                steps.Select(step => step.SequenceNumber).Distinct().Count() != steps.Count)
                throw new InvalidOperationException("HSE workflow steps must have unique identities and one recorded role each.");
            var existing = (await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID", id))).ToList();
            if (existing.Count > 1 || existing.Any(row => row is not PROCESS_INSTANCE)) throw new InvalidOperationException("The HSE process identity is ambiguous.");
            if (existing.Count == 1)
            {
                instance = await LoadProcessInstanceAsync(id) ?? throw new InvalidOperationException("The recorded HSE workflow is unavailable.");
                RequireHseStart(instance, frozen, steps);
            }
            else
            {
                var others = await GetProcessInstanceRepository().GetAsync([new() { FieldName = "ENTITY_ID", FilterValue = frozen.IncidentId }, new() { FieldName = "ENTITY_TYPE", FilterValue = CompanyHseWorkflowStart.EntityType }]);
                foreach (var row in others)
                {
                    if (row is not PROCESS_INSTANCE other) throw new InvalidOperationException("The incident workflow ownership is ambiguous.");
                    if (other.ACTIVE_IND != "Y" || other.FIELD_ID != frozen.Context.FieldId) continue;
                    var owner = CompanyHseWorkflowStart.FromPayload(other.INSTANCE_DATA_JSON!, other.PROCESS_INSTANCE_ID!, other.FIELD_ID!, frozen.IncidentId);
                    if (owner.Context.Company == frozen.Context.Company) throw new InvalidOperationException("The incident already has a workflow; review it before another start.");
                }
                var now = DateTime.UtcNow;
                var payload = JsonSerializer.Deserialize<PROCESS_DATA>(JsonSerializer.Serialize(new PROCESS_DATA { InstanceId = id, LastUpdated = now, Data = new() { [CompanyHseWorkflowStart.PayloadKey] = frozen } }))!;
                instance = CreateInitialProcess(steps, CompanyHseWorkflowStart.ProcessId, frozen.IncidentId, CompanyHseWorkflowStart.EntityType,
                    frozen.Context.FieldId!, frozen.Context.ActorUserId, id, now,
                    step => "hse-step-" + ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new { Instance = id, Step = step.StepId })), payload);
                instance.CurrentState = IncidentState.Reported;
                await PersistReviewedStartAsync(instance, new ProcessHistoryEntry { HistoryId = "hse-start-" + ProcessDefinitionRevision.FromJson(id),
                    InstanceId = id, Action = "WORKFLOW_ENROLLED", NewState = IncidentState.Reported, Timestamp = now,
                    PerformedBy = frozen.Context.ActorUserId, Notes = "Reviewed HSE incident workflow started" });
            }
            var active = instance.StepInstances.Single(step => step.StepId == instance.CurrentStepId);
            if (existing.Count == 1) await router.RequireExistingTaskSetAsync(id, active.StepInstanceId, _connectionName);
            tasks = await router.RouteTaskInOwnedTransactionAsync(id, active.StepInstanceId, active.RequiredRole!, CompanyHseWorkflowStart.EntityType, frozen.IncidentId, null, frozen.Context.ActorUserId);
            await Guard(true); RequireHseStart(instance, frozen, steps); graph = await ConfirmStartedGraphAsync(instance);
            await router.ConfirmRecordedTaskSetAsync(tasks, _connectionName);
            var closing = (await GetProcessDefinitionRepository().GetAsync(Key("PROCESS_DEFINITION_ID", CompanyHseWorkflowStart.ProcessId))).ToList();
            if (closing.Count != 1 || JsonSerializer.Serialize(closing[0]) != snapshot || await ConfirmStartedGraphAsync(instance) != graph)
                throw new InvalidOperationException("The reviewed HSE definition or workflow graph changed.");
            await Guard(true);
        });
        if (instance is null || !ReferenceEquals(source, _editor.GetDataSource(_connectionName)) || await ConfirmStartedGraphAsync(instance) != graph)
            throw new InvalidOperationException("The HSE workflow commit is unconfirmed. Retain the original review.");
        await router.ConfirmRecordedTaskSetAsync(tasks, _connectionName); await Guard(false); await RequireIncident(); await Guard(false);
        var receipt = new CompanyHseWorkflowReceipt(frozen.Context.Company, frozen.Context.ActorUserId, frozen.Context.FieldId!, frozen.IntentId,
            frozen.IncidentId, id, instance.StepInstances.Single(step => step.StepId == instance.CurrentStepId).StepInstanceId,
            Array.AsReadOnly(tasks.Select(task => task.CROSS_TASK_ID).Order(StringComparer.Ordinal).ToArray()),
            ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new { Graph = graph, Tasks = tasks })));
        receipt.Require(frozen); return receipt;
    }

    private static void RequireHseStart(ProcessInstance instance, CompanyHseWorkflowStart original, IReadOnlyList<ProcessStepDefinition> steps)
    {
        var id = original.InstanceId();
        if (instance.InstanceId != id || instance.ProcessId != CompanyHseWorkflowStart.ProcessId || instance.EntityType != CompanyHseWorkflowStart.EntityType ||
            instance.EntityId != original.IncidentId || instance.FieldId != original.Context.FieldId || instance.StartedBy != original.Context.ActorUserId ||
            instance.Status != ProcessStatus.IN_PROGRESS || instance.CurrentState != IncidentState.Reported || instance.ProcessData?.InstanceId != id ||
            CompanyHseWorkflowStart.FromPayload(JsonSerializer.Serialize(instance.ProcessData), id, instance.FieldId, instance.EntityId) != original ||
            instance.History.Count != 1 || instance.History[0].Action != "WORKFLOW_ENROLLED" || instance.History[0].PerformedBy != original.Context.ActorUserId ||
            instance.History[0].NewState != IncidentState.Reported || instance.History[0].Notes != "Reviewed HSE incident workflow started" ||
            instance.History[0].HistoryId != "hse-start-" + ProcessDefinitionRevision.FromJson(id) || instance.History[0].Timestamp != instance.StartDate ||
            instance.StepInstances.Count != steps.Count || instance.CurrentStepId != steps[0].StepId)
            throw new InvalidOperationException("The recorded HSE workflow differs from its original review or has progressed.");
        foreach (var template in steps)
        {
            var matches = instance.StepInstances.Where(step => step.StepId == template.StepId).ToList();
            if (matches.Count != 1 || matches[0].StepInstanceId != "hse-step-" + ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new { Instance = id, Step = template.StepId })) ||
                matches[0].InstanceId != id || matches[0].StepName != template.StepName || matches[0].SequenceNumber != template.SequenceNumber ||
                matches[0].RequiredRole != template.RequiredRoles[0] || matches[0].ApprovalRequired != template.RequiresApproval || matches[0].SlaHours != template.SlaHours ||
                matches[0].Status != (template.StepId == instance.CurrentStepId ? StepStatus.PENDING : StepStatus.BLOCKED) ||
                !string.IsNullOrEmpty(matches[0].AssignedTo) || matches[0].StartDate is not null || matches[0].CompletionDate is not null)
                throw new InvalidOperationException("The recorded HSE steps differ from the reviewed definition.");
        }
    }
}
