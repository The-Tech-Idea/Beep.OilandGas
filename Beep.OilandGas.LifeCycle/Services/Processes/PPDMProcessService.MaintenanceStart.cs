using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
using System.Data;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.Core;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

public partial class PPDMProcessService
{
    public async Task<CompanyMaintenanceProcessStart> ReadOriginalMaintenanceIntentAsync(FieldSelectionReview current,CompanyMaintenanceRecoveryQuery query,Func<Task> requireLifecycleAuthority)
    {
        ArgumentNullException.ThrowIfNull(current);ArgumentNullException.ThrowIfNull(query);ArgumentNullException.ThrowIfNull(requireLifecycleAuthority);query.Require();
        FieldSelectionReviewValidator.RequireReview(current,current.Company,current.ActorUserId);
        if(current.Database is null || current.FieldId!=query.FieldId || !LeaseSummaryValidator.Identifier(current.ActorUserId) || string.IsNullOrWhiteSpace(current.Revision) || string.IsNullOrWhiteSpace(current.Database.Fingerprint) || _connectionName!=current.Database.ConnectionName)
            throw new UnauthorizedAccessException("The original reviewed maintenance company/account/FIELD/lifecycle binding is required.");
        var id=CompanyMaintenanceProcessStart.InstanceIdFor(current.Company,query.IntentId);
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("The original lifecycle datasource is unavailable.");
        async Task Guard(bool owned)
        {
            if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("The original lifecycle datasource changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            await requireLifecycleAuthority();
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
        }
        await Guard(false);CompanyMaintenanceProcessStart? result=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);var rows=(await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).ToArray();
            if(rows.Length!=1 || rows[0] is not PROCESS_INSTANCE row || row.ACTIVE_IND!="Y" || row.ENTITY_TYPE!="WORK_ORDER" || row.FIELD_ID!=query.FieldId || row.ENTITY_ID!=query.WorkOrderId || row.ROW_CREATED_BY!=current.ActorUserId)
                throw new InvalidOperationException("The private original maintenance assignment is unavailable in this account/FIELD/order.");
            var original=JsonSerializer.Serialize(row);var intent=CompanyMaintenanceProcessStart.FromPayload(row.INSTANCE_DATA_JSON!,id,query.FieldId,query.WorkOrderId);
            if(intent.Context!=current || intent.Source.WellId!=query.WellId || intent.Source.DecisionId!=query.DecisionId || intent.IntentId!=query.IntentId)
                throw new UnauthorizedAccessException("The original assignment authority or source reference differs from the selected company/account.");
            await Guard(true);var closing=(await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).ToArray();
            if(closing.Length!=1 || JsonSerializer.Serialize(closing[0])!=original)throw new InvalidOperationException("The private original assignment changed during recovery.");
            result=JsonSerializer.Deserialize<CompanyMaintenanceProcessStart>(JsonSerializer.Serialize(intent))!;
        });
        await Guard(false);return result??throw new InvalidOperationException("The original assignment intent is unconfirmed.");
    }
    public async Task<CompanyMaintenanceProcessStartReceipt> StartMaintenanceProcessAsync(CompanyMaintenanceProcessStart intent,
        Func<Task> requireLifecycleAuthority,Func<Task<ProductionInterventionMaintenanceLink>> readOriginalSource)
        => await ReadOrStartMaintenanceProcessAsync(intent,requireLifecycleAuthority,readOriginalSource,false);

    public async Task<CompanyMaintenanceProcessStartReceipt> RecoverMaintenanceProcessAsync(CompanyMaintenanceProcessStart intent,
        Func<Task> requireLifecycleAuthority,Func<Task<ProductionInterventionMaintenanceLink>> readOriginalSource)
        => await ReadOrStartMaintenanceProcessAsync(intent,requireLifecycleAuthority,readOriginalSource,true);

    private async Task<CompanyMaintenanceProcessStartReceipt> ReadOrStartMaintenanceProcessAsync(CompanyMaintenanceProcessStart intent,
        Func<Task> requireLifecycleAuthority,Func<Task<ProductionInterventionMaintenanceLink>> readOriginalSource,bool readOnly)
    {
        ArgumentNullException.ThrowIfNull(intent);intent.Require();ArgumentNullException.ThrowIfNull(requireLifecycleAuthority);ArgumentNullException.ThrowIfNull(readOriginalSource);
        var frozen=JsonSerializer.Deserialize<CompanyMaintenanceProcessStart>(JsonSerializer.Serialize(intent))!;var id=frozen.InstanceId();
        if(_personaReader is null || _connectionName!=frozen.Context.Database.ConnectionName)throw new InvalidOperationException("The original lifecycle datasource and authoritative role/persona reader are required.");
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("The original lifecycle datasource is unavailable.");
        var router=new CrossPersonaTaskRouter(_editor,_commonColumnHandler,_defaults,_metadata,()=>Task.FromResult(_connectionName),personas:_personaReader);
        async Task Guard(bool owned)
        {
            if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("The original lifecycle datasource changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);
            else PPDMDataSourceTransaction.RequireIdle(source);
            await requireLifecycleAuthority();
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);
            else PPDMDataSourceTransaction.RequireIdle(source);
        }
        // No production owner reads occur inside the lifecycle transaction.
        await Guard(false);frozen.RequireSource(await readOriginalSource());await Guard(false);
        ProcessInstance? instance=null;List<CROSS_PERSONA_TASK> tasks=[];string? graph=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);
            var definitions=(await GetProcessDefinitionRepository().GetAsync(Key("PROCESS_DEFINITION_ID",frozen.ProcessId))).ToList();
            if(definitions.Count!=1 || definitions[0] is not PROCESS_DEFINITION native || native.PROCESS_DEFINITION_ID!=frozen.ProcessId || native.ACTIVE_IND!="Y" || native.IS_ACTIVE!="Y" || native.ENTITY_TYPE!="WORK_ORDER" || native.PROCESS_TYPE!="WORK_ORDER")
                throw new InvalidOperationException("One active configured WORK_ORDER maintenance definition is required.");
            var nativeDefinition=JsonSerializer.Serialize(native);var definition=ConvertToProcessDefinition(native);
            if(string.IsNullOrWhiteSpace(definition.ProcessName))throw new InvalidOperationException("The configured maintenance definition requires its recorded name.");
            if(ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition))!=frozen.DefinitionRevision)throw new InvalidOperationException("The reviewed maintenance definition changed.");
            if(definition.Steps is null || definition.Steps.Any(step=>step is null))throw new InvalidOperationException("The configured maintenance steps are unavailable.");
            var steps=definition.Steps.OrderBy(step=>step.SequenceNumber).ToList();
            if(steps.Count is <1 or >64 || steps.Any(step=>step is null || !LeaseSummaryValidator.Identifier(step.StepId) || step.SequenceNumber<0 ||
                step.RequiredRoles is null || step.RequiredRoles.Count!=1 || !LeaseSummaryValidator.Identifier(step.RequiredRoles[0]) || step.SlaHours<0) ||
                steps.Select(step=>step.StepId).Distinct(StringComparer.Ordinal).Count()!=steps.Count || steps.Select(step=>step.SequenceNumber).Distinct().Count()!=steps.Count ||
                !steps[0].RequiresApproval || steps[0].StepType!="APPROVAL")throw new InvalidOperationException("Explicit unique maintenance role steps and an initial receiving review are required.");
            var existing=(await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).ToList();
            if(existing.Count>1 || existing.Any(row=>row is not PROCESS_INSTANCE))throw new InvalidOperationException("The original maintenance process identity is ambiguous.");
            if(existing.Count==1)
            {
                instance=await LoadProcessInstanceAsync(id)??throw new InvalidOperationException("The recorded maintenance handoff is unavailable.");
                RequireMaintenanceStart(instance,frozen,steps);
            }
            else
            {
                if(readOnly)throw new InvalidOperationException("The original maintenance assignment is not recorded. Retain its intent; recovery cannot create it.");
                var others=await GetProcessInstanceRepository().GetAsync([new(){FieldName="ENTITY_ID",FilterValue=frozen.Source.WorkOrderId},new(){FieldName="ENTITY_TYPE",FilterValue="WORK_ORDER"}]);
                foreach(var row in others)
                {
                    if(row is not PROCESS_INSTANCE process)throw new InvalidOperationException("The maintenance source ownership is ambiguous.");
                    if(process.ACTIVE_IND!="Y" || process.STATUS!="IN_PROGRESS" || process.FIELD_ID!=frozen.Context.FieldId)continue;
                    var other=CompanyMaintenanceProcessStart.FromPayload(process.INSTANCE_DATA_JSON!,process.PROCESS_INSTANCE_ID!,process.FIELD_ID!,frozen.Source.WorkOrderId);
                    if(other.Context.Company==frozen.Context.Company)throw new InvalidOperationException("This original order already has an active maintenance handoff. Review it before another start.");
                }
                var now=DateTime.UtcNow;
                // Freeze dictionary values in their persisted JSON representation, including UTC offsets and opaque keys.
                var payload=JsonSerializer.Deserialize<PROCESS_DATA>(JsonSerializer.Serialize(new PROCESS_DATA{InstanceId=id,LastUpdated=now,Data=new(){[CompanyMaintenanceProcessStart.PayloadKey]=frozen}}))!;
                instance=CreateInitialProcess(steps,frozen.ProcessId,frozen.Source.WorkOrderId,"WORK_ORDER",frozen.Context.FieldId!,frozen.Context.ActorUserId,id,now,
                    step=>"maintenance-step-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Instance=id,Step=step.StepId})),payload);
                await PersistStartedProcessAsync(instance,new ProcessHistoryEntry{HistoryId="maintenance-start-"+ProcessDefinitionRevision.FromJson(id),InstanceId=id,Action="PROCESS_STARTED",NewState="INITIAL",Timestamp=now,PerformedBy=frozen.Context.ActorUserId,Notes="Original intervention maintenance receiving assignment"});
            }
            var active=instance.StepInstances.Single(step=>step.StepId==instance.CurrentStepId);
            if(existing.Count==1)await router.RequireExistingTaskSetAsync(id,active.StepInstanceId,_connectionName);
            tasks=readOnly
                ? await router.ReadTaskSetInOwnedTransactionAsync(id,active.StepInstanceId,active.RequiredRole!,"WORK_ORDER",frozen.Source.WorkOrderId,frozen.Context.ActorUserId)
                : await router.RouteTaskInOwnedTransactionAsync(id,active.StepInstanceId,active.RequiredRole!,"WORK_ORDER",frozen.Source.WorkOrderId,null,frozen.Context.ActorUserId);
            await Guard(true);RequireMaintenanceStart(instance,frozen,steps);graph=await ConfirmStartedGraphAsync(instance);await router.ConfirmRecordedTaskSetAsync(tasks,_connectionName);
            var closing=(await GetProcessDefinitionRepository().GetAsync(Key("PROCESS_DEFINITION_ID",frozen.ProcessId))).ToList();
            if(closing.Count!=1 || JsonSerializer.Serialize(closing[0])!=nativeDefinition || await ConfirmStartedGraphAsync(instance)!=graph)throw new InvalidOperationException("The original maintenance definition or complete initial graph changed.");
            await Guard(true);
        });
        if(instance is null || !ReferenceEquals(source,_editor.GetDataSource(_connectionName)) || await ConfirmStartedGraphAsync(instance)!=graph)throw new InvalidOperationException("Maintenance commit is unconfirmed. Retain the original intent for reconciliation.");
        await router.ConfirmRecordedTaskSetAsync(tasks,_connectionName);await Guard(false);frozen.RequireSource(await readOriginalSource());await Guard(false);
        var receipt=new CompanyMaintenanceProcessStartReceipt(frozen.Context.Company,frozen.Context.ActorUserId,frozen.Context.FieldId!,frozen.IntentId,id,
            frozen.Source.WorkOrderId,frozen.Source.DecisionId,instance.StepInstances.Single(step=>step.StepId==instance.CurrentStepId).StepInstanceId,
            Array.AsReadOnly(tasks.Select(task=>task.CROSS_TASK_ID).Order(StringComparer.Ordinal).ToArray()),ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Graph=graph,Tasks=tasks})));
        receipt.Require(frozen);return receipt;
    }
    private static void RequireMaintenanceStart(ProcessInstance instance,CompanyMaintenanceProcessStart intent,IReadOnlyList<ProcessStepDefinition> steps)
    {
        var id=intent.InstanceId();
        if(instance.InstanceId!=id || instance.ProcessId!=intent.ProcessId || instance.EntityType!="WORK_ORDER" || instance.EntityId!=intent.Source.WorkOrderId ||
            instance.FieldId!=intent.Context.FieldId || instance.StartedBy!=intent.Context.ActorUserId || instance.Status!=ProcessStatus.IN_PROGRESS || instance.CurrentState!="INITIAL" ||
            instance.ProcessData?.InstanceId!=id || CompanyMaintenanceProcessStart.FromPayload(JsonSerializer.Serialize(instance.ProcessData),id,instance.FieldId,instance.EntityId)!=intent ||
            instance.History.Count!=1 || instance.History[0].Action!="PROCESS_STARTED" || instance.History[0].PerformedBy!=intent.Context.ActorUserId ||
            instance.History[0].HistoryId!="maintenance-start-"+ProcessDefinitionRevision.FromJson(id) || instance.History[0].Timestamp!=instance.StartDate ||
            instance.StepInstances.Count!=steps.Count || instance.CurrentStepId!=steps[0].StepId)throw new InvalidOperationException("The recorded maintenance handoff differs from its original intent or has progressed.");
        foreach(var template in steps)
        {
            var matches=instance.StepInstances.Where(step=>step.StepId==template.StepId).ToList();
            if(matches.Count!=1 || matches[0].StepInstanceId!="maintenance-step-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Instance=id,Step=template.StepId})) ||
                matches[0].InstanceId!=id || matches[0].StepName!=template.StepName || matches[0].SequenceNumber!=template.SequenceNumber || matches[0].RequiredRole!=template.RequiredRoles[0] ||
                matches[0].ApprovalRequired!=template.RequiresApproval || matches[0].SlaHours!=template.SlaHours || matches[0].Status!=(template.StepId==instance.CurrentStepId?StepStatus.PENDING:StepStatus.BLOCKED) ||
                !string.IsNullOrEmpty(matches[0].AssignedTo) || matches[0].StartDate is not null || matches[0].CompletionDate is not null)throw new InvalidOperationException("The recorded maintenance steps differ from their original definition.");
        }
    }
}
