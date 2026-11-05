using System.Data;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

public partial class PPDMProcessService
{
    public async Task<CompanyAccountingProcessStartReceipt> StartCompanyAccountingAsync(CompanyAccountingProcessStart intent, Func<Task> requireCurrentReview)
    {
        ArgumentNullException.ThrowIfNull(intent); intent.Require(); ArgumentNullException.ThrowIfNull(requireCurrentReview);
        if (_personaReader is null) throw new InvalidOperationException("Company handoff requires the authoritative persona reader.");
        // Freeze the submission before awaiting authority or provider work.
        var frozen = JsonSerializer.Deserialize<CompanyAccountingProcessStart>(JsonSerializer.Serialize(intent))!;
        var id = frozen.InstanceId();
        var source = _editor.GetDataSource(_connectionName) ?? throw new InvalidOperationException("The native company process datasource is unavailable.");
        var router = new CrossPersonaTaskRouter(_editor, _commonColumnHandler, _defaults, _metadata, () => Task.FromResult(_connectionName), personas: _personaReader);
        async Task Guard()
        {
            if (!ReferenceEquals(source, _editor.GetDataSource(_connectionName))) throw new InvalidOperationException("The company handoff datasource changed.");
            PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable);
            await requireCurrentReview();
            PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable);
        }
        ProcessInstance? instance = null;
        List<CROSS_PERSONA_TASK> tasks = [];
        string? graph = null;
        await requireCurrentReview();
        await PPDMDataSourceTransaction.ExecuteAsync(source, async () =>
        {
            await Guard();
            var definitions = (await GetProcessDefinitionRepository().GetAsync(Key("PROCESS_DEFINITION_ID", frozen.ProcessId))).ToList();
            if (definitions.Count != 1 || definitions[0] is not PROCESS_DEFINITION row || row.ACTIVE_IND != "Y" || row.IS_ACTIVE != "Y" || row.ENTITY_TYPE != frozen.EntityType)
                throw new InvalidOperationException("One active source-matched company workflow definition is required.");
            var nativeDefinition = JsonSerializer.Serialize(row);
            var definition = ConvertToProcessDefinition(row);
            if (ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition)) != frozen.DefinitionRevision)
                throw new InvalidOperationException("The reviewed company workflow definition changed.");
            var steps = definition.Steps.OrderBy(step => step.SequenceNumber).ToList();
            if(CompanyAccountingSourceTypes.IsNativeDecision(frozen.EntityType) && frozen.EntityType!="JOURNAL_ENTRY" && (definition.ProcessType!="FINANCIAL" || steps.Count is <1 or >2 || !SupportedAccountingEvidence(steps[0],frozen.EntityType) || !steps[0].RequiresApproval || steps.Count==2 && (!SupportedAccountingEvidence(steps[1],frozen.EntityType) || steps[1].RequiresApproval)))
                throw new InvalidOperationException("Native decision handoff requires native approval and an optional configured rejection-review step.");
            if(frozen.EntityType=="JOURNAL_ENTRY" && (definition.ProcessType!="FINANCIAL" || steps.Count!=3 || !SupportedAccountingEvidence(steps[0],frozen.EntityType) || !steps[0].RequiresApproval || !SupportedAccountingEvidence(steps[1],frozen.EntityType) || steps[1].RequiresApproval || !SupportedAccountingEvidence(steps[2],frozen.EntityType) || steps[2].RequiresApproval || JournalEvidenceAction(steps[0])!=CompanyAccountingSourceTypes.Approval("JOURNAL_ENTRY") || JournalEvidenceAction(steps[1])!=CompanyAccountingSourceTypes.JournalPosting || JournalEvidenceAction(steps[2])!=CompanyAccountingSourceTypes.RejectionReview("JOURNAL_ENTRY")))throw new InvalidOperationException("Journal handoff requires exactly native approval, posting and maker rejection-review branches.");
            if (steps.Count == 0 || steps.Any(step => step is null || string.IsNullOrWhiteSpace(step.StepId) || step.StepId.Length > 128 || step.StepId.Any(char.IsControl) ||
                step.SequenceNumber < 0 || step.SlaHours < 0 || step.RequiredRoles is null || step.RequiredRoles.Count != 1 ||
                step.RequiredRoles.Any(role => string.IsNullOrWhiteSpace(role) || role.Length > 128 || role.Any(char.IsControl))) ||
                steps.Select(step => step.StepId).Distinct(StringComparer.Ordinal).Count() != steps.Count || steps.Select(step => step.SequenceNumber).Distinct().Count() != steps.Count)
                throw new InvalidOperationException("Explicit unique company workflow steps and role assignments are required.");
            var existing = (await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID", id))).ToList();
            if (existing.Count > 1 || existing.Any(value => value is not PROCESS_INSTANCE)) throw new InvalidOperationException("The company start identity is ambiguous.");
            if (existing.Count == 1)
            {
                instance = await LoadProcessInstanceAsync(id) ?? throw new InvalidOperationException("The recorded company handoff is unavailable.");
                RequireRecordedStart(instance, frozen, steps);
            }
            else
            {
                var otherRows = await GetProcessInstanceRepository().GetAsync([new() { FieldName = "ENTITY_ID", FilterValue = frozen.EntityId }, new() { FieldName = "ENTITY_TYPE", FilterValue = frozen.EntityType }]);
                foreach (var other in otherRows)
                {
                    if (other is not PROCESS_INSTANCE process) throw new InvalidOperationException("The source process ownership evidence is ambiguous.");
                    if (process.STATUS != "IN_PROGRESS" || process.ACTIVE_IND != "Y" || !string.IsNullOrEmpty(process.FIELD_ID)) continue;
                    var recorded = ConvertToProcessInstance(process);
                    if (!recorded.ProcessData.Data.TryGetValue(CompanyAccountingProcessBinding.PayloadKey, out var owner))
                        throw new InvalidOperationException("An active source process needs ownership reconciliation before another handoff.");
                    var binding = JsonSerializer.Deserialize<CompanyAccountingProcessBinding>(JsonSerializer.Serialize(owner)) ?? throw new InvalidOperationException("Source process ownership is unavailable.");
                    binding.Require(binding.Company, recorded.InstanceId, frozen.EntityType, frozen.EntityId);
                    if (binding.Company == frozen.Company) throw new InvalidOperationException("This company source already has an active handoff. Review the recorded process.");
                }
                var now = DateTime.UtcNow;
                var payload = new PROCESS_DATA { InstanceId = id, LastUpdated = now, Data = new Dictionary<string, object>
                {
                    [CompanyAccountingProcessBinding.PayloadKey] = new CompanyAccountingProcessBinding(frozen.Company, id, frozen.EntityType, frozen.EntityId),
                    [CompanyAccountingProcessStart.PayloadKey] = frozen
                } };
                instance = CreateInitialProcess(steps, frozen.ProcessId, frozen.EntityId, frozen.EntityType, "", frozen.ActorUserId, id, now,
                    step => "company-step-" + ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new { Instance = id, Step = step.StepId })), payload);
                await PersistStartedProcessAsync(instance, new ProcessHistoryEntry { HistoryId = "company-start-" + ProcessDefinitionRevision.FromJson(id),
                    InstanceId = id, Action = "PROCESS_STARTED", NewState = "INITIAL", Timestamp = now, PerformedBy = frozen.ActorUserId,
                    Notes = $"Process {definition.ProcessName} started for {frozen.EntityType} {frozen.EntityId}" });
            }
            var active = instance.StepInstances.Single(step => step.StepId == instance.CurrentStepId);
            if (existing.Count == 1) await router.RequireExistingTaskSetAsync(id, active.StepInstanceId, _connectionName);
            tasks = await router.RouteTaskInOwnedTransactionAsync(id, active.StepInstanceId, active.RequiredRole!, frozen.EntityType, frozen.EntityId, null, frozen.ActorUserId);
            await Guard();
            RequireRecordedStart(instance, frozen, steps);
            graph = await ConfirmStartedGraphAsync(instance);
            await router.ConfirmRecordedTaskSetAsync(tasks, _connectionName);
            var closing = (await GetProcessDefinitionRepository().GetAsync(Key("PROCESS_DEFINITION_ID", frozen.ProcessId))).ToList();
            if (closing.Count != 1 || JsonSerializer.Serialize(closing[0]) != nativeDefinition || await ConfirmStartedGraphAsync(instance) != graph)
                throw new InvalidOperationException("The reviewed company workflow graph or definition changed before commit.");
            await Guard();
        });
        if (instance is null || !ReferenceEquals(source, _editor.GetDataSource(_connectionName)) || await ConfirmStartedGraphAsync(instance) != graph)
            throw new InvalidOperationException("The company handoff commit is unconfirmed. Retain this intent and reconcile the original source.");
        await router.ConfirmRecordedTaskSetAsync(tasks, _connectionName);
        await requireCurrentReview();
        return new(frozen.Company, frozen.ActorUserId, frozen.IntentId, id, frozen.EntityType, frozen.EntityId,
            instance.StepInstances.Single(step => step.StepId == instance.CurrentStepId).StepInstanceId,
            Array.AsReadOnly(tasks.Select(task => task.CROSS_TASK_ID).Order(StringComparer.Ordinal).ToArray()),
            ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new { Graph = graph, Tasks = tasks })));
    }

    private static void RequireRecordedStart(ProcessInstance instance, CompanyAccountingProcessStart intent, IReadOnlyList<ProcessStepDefinition> steps)
    {
        var id = intent.InstanceId();
        if (instance.InstanceId != id || instance.ProcessId != intent.ProcessId || instance.EntityType != intent.EntityType || instance.EntityId != intent.EntityId ||
            !string.IsNullOrEmpty(instance.FieldId) || instance.StartedBy != intent.ActorUserId || instance.Status != ProcessStatus.IN_PROGRESS ||
            instance.CurrentState != "INITIAL" || instance.ProcessData?.InstanceId != id || instance.ProcessData.Data is null ||
            !instance.ProcessData.Data.TryGetValue(CompanyAccountingProcessStart.PayloadKey, out var recorded) ||
            JsonSerializer.Serialize(JsonSerializer.Deserialize<CompanyAccountingProcessStart>(JsonSerializer.Serialize(recorded))) != JsonSerializer.Serialize(intent) ||
            !instance.ProcessData.Data.TryGetValue(CompanyAccountingProcessBinding.PayloadKey, out var owner))
            throw new InvalidOperationException("The recorded handoff conflicts with the original reviewed intent.");
        CompanyAccountingProcessBinding.FromJson(JsonSerializer.Serialize(owner), intent.Company, id, intent.EntityType, intent.EntityId);
        if (instance.History.Count != 1 || instance.History[0].Action != "PROCESS_STARTED" || instance.History[0].PerformedBy != intent.ActorUserId ||
            instance.History[0].HistoryId != "company-start-" + ProcessDefinitionRevision.FromJson(id) || instance.History[0].Timestamp != instance.StartDate ||
            instance.StepInstances.Count != steps.Count || instance.CurrentStepId != steps[0].StepId || instance.StepInstances.Count(step => step.Status == StepStatus.PENDING) != 1 ||
            instance.StepInstances.Any(step => step.InstanceId != id || step.Status is not (StepStatus.PENDING or StepStatus.BLOCKED)))
            throw new InvalidOperationException("The original initial handoff has progressed or needs reconciliation.");
        foreach (var template in steps)
        {
            var matches = instance.StepInstances.Where(step => step.StepId == template.StepId).ToList();
            if (matches.Count != 1 || matches[0].StepInstanceId != "company-step-" + ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new { Instance = id, Step = template.StepId })) ||
                matches[0].StepName != template.StepName || matches[0].SequenceNumber != template.SequenceNumber || matches[0].RequiredRole != template.RequiredRoles[0] ||
                matches[0].ApprovalRequired != template.RequiresApproval || matches[0].SlaHours != template.SlaHours ||
                matches[0].Status != (template.StepId == instance.CurrentStepId ? StepStatus.PENDING : StepStatus.BLOCKED) ||
                !string.IsNullOrEmpty(matches[0].AssignedTo) || matches[0].StartDate is not null || matches[0].CompletionDate is not null)
                throw new InvalidOperationException("The recorded handoff steps differ from the reviewed definition.");
        }
    }
}
