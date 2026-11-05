using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using System.Data;
using System.Reflection;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

public partial class PPDMProcessService
{
    public override async Task<ProcessInstance> StartProcessAsync(string processId, string entityId, string entityType, string fieldId, string userId)
    {
        static void RequireKey(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > 128 || value.Any(char.IsControl))
                throw new ArgumentException("Recorded bounded process, source, FIELD and actor identities are required.");
        }
        RequireKey(processId); RequireKey(entityId); RequireKey(entityType); RequireKey(fieldId); RequireKey(userId);
        if(entityType=="FACILITY" || processId is "FACILITY_DEVELOPMENT" or "WELL_DEVELOPMENT")
            throw Beep.OilandGas.Models.Core.Refusals.RefusalException.Forbidden("Construction and production handover require a source-bound evidence owner.");
        if(entityType is "AFE" or "DRILLING_PROGRAM" or "DRILLING_OPERATION" or "DRILLING_DAILY_REPORT")throw Beep.OilandGas.Models.Core.Refusals.RefusalException.Forbidden("Source-bound work must start through its canonical reviewed company source owner.");
        if(entityType=="FIELD_DEVELOPMENT_PLAN")
            throw Beep.OilandGas.Models.Core.Refusals.RefusalException.Forbidden("Development review starts require their reviewed company draft handoff owner.");
        // Company starts need their explicit reviewed binding, rather than the generic FIELD-only entry point.
        var source = _editor.GetDataSource(_connectionName) ?? throw new InvalidOperationException("The native process datasource is unavailable.");
        void Guard()
        {
            if (!ReferenceEquals(source, _editor.GetDataSource(_connectionName)))
                throw new InvalidOperationException("The process command datasource changed.");
            PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable);
        }
        ProcessInstance? result = null;
        string? confirmed = null;
        await PPDMDataSourceTransaction.ExecuteAsync(source, async () =>
        {
            Guard();
            var definitionRows = await GetProcessDefinitionRepository().GetAsync(Key("PROCESS_DEFINITION_ID", processId));
            if (definitionRows.Count() != 1 || definitionRows.Single() is not PROCESS_DEFINITION definition ||
                definition.PROCESS_DEFINITION_ID != processId || definition.ACTIVE_IND != "Y" || definition.IS_ACTIVE != "Y" || definition.ENTITY_TYPE != entityType)
                throw new InvalidOperationException("One active source-matched workflow definition is required.");
            var definitionSnapshot = JsonSerializer.Serialize(definition);
            var parsed = ConvertToProcessDefinition(definition);
            if (parsed.Steps is null || parsed.Steps.Count == 0 || parsed.Steps.Any(step => step is null ||
                string.IsNullOrWhiteSpace(step.StepId) || step.StepId.Length > 128 || step.StepId.Any(char.IsControl) ||
                step.SequenceNumber < 0 || step.SlaHours < 0 || step.RequiredRoles is null || step.RequiredRoles.Count > 1 ||
                step.RequiredRoles.Any(role => string.IsNullOrWhiteSpace(role) || role.Length > 128 || role.Any(char.IsControl))) ||
                parsed.Steps.Select(step => step.StepId).Distinct(StringComparer.Ordinal).Count() != parsed.Steps.Count ||
                parsed.Steps.Select(step => step.SequenceNumber).Distinct().Count() != parsed.Steps.Count)
                throw new InvalidOperationException("The recorded workflow steps or assignments are ambiguous.");
            result = await base.StartProcessAsync(processId, entityId, entityType, fieldId, userId);
            Guard();
            confirmed = await ConfirmStartedGraphAsync(result);
            var closing = await GetProcessDefinitionRepository().GetAsync(Key("PROCESS_DEFINITION_ID", processId));
            if (closing.Count() != 1 || closing.Single() is not PROCESS_DEFINITION current || JsonSerializer.Serialize(current) != definitionSnapshot)
                throw new InvalidOperationException("The workflow definition changed during process creation.");
            if (await ConfirmStartedGraphAsync(result) != confirmed)
                throw new InvalidOperationException("The initial process graph changed before commit.");
            Guard();
        });
        if (result is null || !ReferenceEquals(source, _editor.GetDataSource(_connectionName)) || await ConfirmStartedGraphAsync(result) != confirmed)
            throw new InvalidOperationException("The process commit outcome is unconfirmed. Reconcile the recorded source before another start.");
        return JsonSerializer.Deserialize<ProcessInstance>(JsonSerializer.Serialize(result))!;
    }

    protected override async Task PersistStartedProcessAsync(ProcessInstance instance, ProcessHistoryEntry history)
    {
        await RequireAbsentStartGraphAsync(instance,history);
        await base.PersistStartedProcessAsync(instance, history);
    }
    private async Task PersistReviewedStartAsync(ProcessInstance instance,ProcessHistoryEntry history)
    {
        await RequireAbsentStartGraphAsync(instance,history);var actor=instance.StartedBy;var now=instance.StartDate;
        var process=ConvertToProcessInstanceEntity(instance);process.ROW_CREATED_BY=actor;process.ROW_CREATED_DATE=now;await GetProcessInstanceRepository().InsertAsync(process,actor);
        foreach(var stage in instance.StepInstances){var row=ConvertToProcessStepInstanceEntity(stage);row.ROW_CREATED_BY=actor;row.ROW_CREATED_DATE=now;await GetProcessStepInstanceRepository().InsertAsync(row,actor);}
        var audit=ConvertToProcessHistoryEntity(history);audit.ROW_CREATED_BY=actor;audit.ROW_CREATED_DATE=now;await GetProcessHistoryRepository().InsertAsync(audit,actor);instance.History.Add(history);
    }
    private async Task RequireAbsentStartGraphAsync(ProcessInstance instance,ProcessHistoryEntry history)
    {
        var source = _editor.GetDataSource(_connectionName) ?? throw new InvalidOperationException("The native process datasource is unavailable.");
        PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable);
        if ((await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID", instance.InstanceId))).Any() ||
            (await GetProcessStepInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID", instance.InstanceId))).Any() ||
            (await GetProcessHistoryRepository().GetAsync(Key("PROCESS_INSTANCE_ID", instance.InstanceId))).Any() ||
            (await GetProcessHistoryRepository().GetAsync(Key("PROCESS_HISTORY_ID", history.HistoryId))).Any())
            throw new InvalidOperationException("A process start cannot adopt an existing graph or audit identity.");
        foreach (var step in instance.StepInstances)
            if ((await GetProcessStepInstanceRepository().GetAsync(Key("PROCESS_STEP_INSTANCE_ID", step.StepInstanceId))).Any())
                throw new InvalidOperationException("A process start cannot adopt an existing step identity.");
    }

    private static List<AppFilter> Key(string name, string value) => [new() { FieldName = name, FilterValue = value, Operator = "=" }];

    private async Task<string> ConfirmStartedGraphAsync(ProcessInstance instance)
    {
        var processRows = (await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID", instance.InstanceId))).ToList();
        var stepRows = (await GetProcessStepInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID", instance.InstanceId))).ToList();
        var historyRows = (await GetProcessHistoryRepository().GetAsync(Key("PROCESS_INSTANCE_ID", instance.InstanceId))).ToList();
        return ConfirmStartedGraphSnapshot(instance,processRows,stepRows,historyRows);
    }
    private string ConfirmStartedGraphSnapshot(ProcessInstance instance,IReadOnlyList<object> processRows,IReadOnlyList<object> stepRows,IReadOnlyList<object> historyRows)
    {
        if (processRows.Count != 1 || processRows[0] is not PROCESS_INSTANCE process || stepRows.Count != instance.StepInstances.Count ||
            stepRows.Any(row => row is not PROCESS_STEP_INSTANCE) || historyRows.Count != 1 || historyRows[0] is not PROCESS_HISTORY history ||
            instance.History.Count != 1)
            throw new InvalidOperationException("The complete initial process, steps and audit graph could not be confirmed.");
        static void RequireRecorded<T>(T actual, T expected, string actor) where T : Beep.OilandGas.Models.Data.ModelEntityBase
        {
            if (actual.ACTIVE_IND != "Y" || actual.ROW_CREATED_BY != actor || actual.ROW_CREATED_DATE is null)
                throw new InvalidOperationException("Native process creation audit differs from the prepared graph: "+typeof(T).Name);
            var changed=typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .FirstOrDefault(property => !Equals(property.GetValue(actual), property.GetValue(expected)));
            if(changed is not null)throw new InvalidOperationException("Native process creation read-back differs from the prepared graph: "+typeof(T).Name+"."+changed.Name);
        }
        RequireRecorded(process, ConvertToProcessInstanceEntity(instance), instance.StartedBy);
        var steps = stepRows.Cast<PROCESS_STEP_INSTANCE>().OrderBy(step => step.STEP_SEQUENCE).ToList();
        if (steps.Select(step => step.PROCESS_STEP_INSTANCE_ID).Distinct(StringComparer.Ordinal).Count() != steps.Count)
            throw new InvalidOperationException("The recorded initial step identities are ambiguous.");
        foreach (var expected in instance.StepInstances)
        {
            var matches = steps.Where(step => step.PROCESS_STEP_INSTANCE_ID == expected.StepInstanceId).ToList();
            if (matches.Count != 1) throw new InvalidOperationException("An initial process step is missing or ambiguous.");
            RequireRecorded(matches[0], ConvertToProcessStepInstanceEntity(expected), instance.StartedBy);
        }
        RequireRecorded(history, ConvertToProcessHistoryEntity(instance.History[0]), instance.StartedBy);
        return JsonSerializer.Serialize(new { Process = process, Steps = steps, History = history });
    }
}
