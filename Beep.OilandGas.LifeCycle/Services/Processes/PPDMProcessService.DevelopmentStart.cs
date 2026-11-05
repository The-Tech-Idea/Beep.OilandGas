using Beep.OilandGas.DevelopmentPlanning.Models;
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
    public Task<DevelopmentReviewStartReceipt> StartDevelopmentReviewAsync(DevelopmentReviewStart original,Func<Task> requireCurrentReview,Func<Task<DevelopmentPlanSourceReview>> readSource)
        =>ReadOrStartDevelopmentReviewAsync(original,requireCurrentReview,readSource,false);
    public Task<DevelopmentReviewStartReceipt> RecoverDevelopmentReviewAsync(DevelopmentReviewStart original,Func<Task> requireCurrentReview,Func<Task<DevelopmentPlanSourceReview>> readSource)
        =>ReadOrStartDevelopmentReviewAsync(original,requireCurrentReview,readSource,true);

    private sealed class DevelopmentStartPersonas(DevelopmentReviewStart original,Func<Task> guard) : IRolePersonaReader
    {
        public async Task<List<string>> GetActivePersonasAsync(string roleName)
        {
            await guard();
            return original.ReceivingSteps.FirstOrDefault(step=>step.AssignedRole==roleName)?.Recipients.Select(row=>row.PersonaCode).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList()
                ?? throw new InvalidOperationException("The original independent development recipients are required.");
        }
    }
    private async Task<DevelopmentReviewStartReceipt> ReadOrStartDevelopmentReviewAsync(DevelopmentReviewStart original,Func<Task> requireCurrentReview,Func<Task<DevelopmentPlanSourceReview>> readSource,bool readOnly)
    {
        ArgumentNullException.ThrowIfNull(original);original.Require();ArgumentNullException.ThrowIfNull(requireCurrentReview);ArgumentNullException.ThrowIfNull(readSource);
        var frozen=JsonSerializer.Deserialize<DevelopmentReviewStart>(JsonSerializer.Serialize(original))!;var id=frozen.InstanceId();var context=frozen.Source.Context;
        if(_connectionName!=frozen.LifecycleDatabase.ConnectionName)throw new InvalidOperationException("The original reviewed development lifecycle target is required.");
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("The original lifecycle datasource is unavailable.");
        async Task Guard(bool owned)
        {
            if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("The development lifecycle datasource changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            await requireCurrentReview();
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
        }
        // Original planning and FIELD reads bracket this transaction; no cross-source native read is enlisted inside it.
        await Guard(false);frozen.RequireSource(await readSource());await Guard(false);
        var router=new CrossPersonaTaskRouter(_editor,_commonColumnHandler,_defaults,_metadata,()=>Task.FromResult(_connectionName),
            personas:new DevelopmentStartPersonas(frozen,()=>Guard(true)));
        ProcessInstance? instance=null;List<CROSS_PERSONA_TASK> tasks=[];string? graph=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);
            var definitions=(await GetProcessDefinitionRepository().GetAsync(Key("PROCESS_DEFINITION_ID",frozen.Workflow.ProcessId))).ToArray();
            if(definitions.Length!=1 || definitions[0] is not PROCESS_DEFINITION native || native.PROCESS_DEFINITION_ID!=frozen.Workflow.ProcessId || native.ACTIVE_IND!="Y" || native.IS_ACTIVE!="Y")
                throw new InvalidOperationException("One active recorded development review definition is required.");
            var definitionJson=JsonSerializer.Serialize(native);var definition=ConvertToProcessDefinition(native);var steps=DevelopmentReviewDefinition.Require(definition);
            if(ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition))!=frozen.Workflow.DefinitionRevision || definition.ProcessName!=frozen.Workflow.ProcessName || definition.Version!=frozen.Workflow.Version ||
                steps.Count!=frozen.Workflow.Steps.Count || steps.Where((step,index)=>step.StepId!=frozen.Workflow.Steps[index].StepId || step.StepName!=frozen.Workflow.Steps[index].StepName ||
                    step.SequenceNumber!=frozen.Workflow.Steps[index].Sequence || step.RequiredRoles[0]!=frozen.Workflow.Steps[index].AssignedRole || step.SlaHours!=frozen.Workflow.Steps[index].SlaHours).Any())
                throw new InvalidOperationException("The original reviewed development workflow or ordered stages changed.");
            var existing=(await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).ToArray();
            if(existing.Length>1 || existing.Any(row=>row is not PROCESS_INSTANCE))throw new InvalidOperationException("The original development handoff identity is ambiguous.");
            if(existing.Length==1)
            {
                if(((PROCESS_INSTANCE)existing[0]).ACTIVE_IND!="Y")throw new InvalidOperationException("The original development handoff is inactive and needs reconciliation.");
                instance=await LoadProcessInstanceAsync(id)??throw new InvalidOperationException("The recorded development handoff is unavailable.");
                if(DevelopmentReviewStart.FromPayload(JsonSerializer.Serialize(instance.ProcessData),id,instance.FieldId,instance.EntityId).Revision()!=frozen.Revision())
                    throw new InvalidOperationException("The original development submission intent changed.");
                await ConfirmDevelopmentGraphAsync(instance,frozen,steps);
            }
            else if(!readOnly)
            {
                var others=await GetProcessInstanceRepository().GetAsync([new(){FieldName="ENTITY_ID",FilterValue=frozen.Source.PlanId},new(){FieldName="ENTITY_TYPE",FilterValue="FIELD_DEVELOPMENT_PLAN"}]);
                foreach(var value in others)
                {
                    if(value is not PROCESS_INSTANCE row)throw new InvalidOperationException("The development source process ownership is ambiguous.");
                    if(row.ACTIVE_IND!="Y" || row.FIELD_ID!=context.FieldId)continue;
                    var owner=DevelopmentReviewStart.FromPayload(row.INSTANCE_DATA_JSON!,row.PROCESS_INSTANCE_ID!,row.FIELD_ID!,row.ENTITY_ID!);
                    if(owner.Source.Context.Company==context.Company)
                        throw new InvalidOperationException("This recorded draft already has a review handoff. Open its recorded process before requesting more work.");
                }
                var now=DateTime.UtcNow;
                var payload=JsonSerializer.Deserialize<PROCESS_DATA>(JsonSerializer.Serialize(new PROCESS_DATA{InstanceId=id,LastUpdated=now,Data=new(){[DevelopmentReviewStart.PayloadKey]=frozen}}))!;
                instance=CreateInitialProcess(steps,frozen.Workflow.ProcessId,frozen.Source.PlanId,"FIELD_DEVELOPMENT_PLAN",context.FieldId!,context.ActorUserId,id,now,
                    step=>DevelopmentStepId(id,step.StepId),payload);
                await PersistStartedProcessAsync(instance,new ProcessHistoryEntry{HistoryId="development-start-"+ProcessDefinitionRevision.FromJson(id),InstanceId=id,Action="PROCESS_STARTED",NewState="INITIAL",Timestamp=now,PerformedBy=context.ActorUserId,Notes="Recorded development draft submitted for independent configured review"});
            }
            if(instance is not null)
            {
                var initial=instance.StepInstances.Single(step=>step.StepId==steps[0].StepId);
                // Existing submissions may already have independent decisions. Recover the complete current graph,
                // while the submission receipt still identifies its original first-stage assignment.
                tasks=existing.Length==1
                    ?(await router.GetAllTasksForDevelopmentEvidenceAsync(id,_connectionName)).Where(task=>task.PROCESS_STEP_INSTANCE_ID==initial.StepInstanceId).ToList()
                    :await router.RouteTaskInOwnedTransactionAsync(id,initial.StepInstanceId,initial.RequiredRole!,"FIELD_DEVELOPMENT_PLAN",frozen.Source.PlanId,null,context.ActorUserId);
                graph=await ConfirmDevelopmentGraphAsync(instance,frozen,steps);await router.ConfirmRecordedTaskSetAsync(tasks,_connectionName);
            }
            await Guard(true);var closing=(await GetProcessDefinitionRepository().GetAsync(Key("PROCESS_DEFINITION_ID",frozen.Workflow.ProcessId))).ToArray();
            if(closing.Length!=1 || JsonSerializer.Serialize(closing[0])!=definitionJson || instance is not null && await ConfirmDevelopmentGraphAsync(instance,frozen,steps)!=graph)
                throw new InvalidOperationException("The original development definition or initial handoff graph changed.");
            await Guard(true);
        });
        await Guard(false);
        if(instance is not null)
        {
            if(await ConfirmDevelopmentGraphAsync(instance,frozen,DevelopmentReviewDefinition.Require(await GetProcessDefinitionAsync(frozen.Workflow.ProcessId)??throw new InvalidOperationException("The original development definition is unavailable.")))!=graph)throw new InvalidOperationException("The development handoff commit is unconfirmed. Recover its retained original intent.");
            await router.ConfirmRecordedTaskSetAsync(tasks,_connectionName);
        }
        else if((await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).Any())throw new InvalidOperationException("Original development handoff absence changed during recovery.");
        frozen.RequireSource(await readSource());await Guard(false);
        var receipt=new DevelopmentReviewStartReceipt(frozen.Revision(),id,instance is not null,instance?.StepInstances.Single(step=>step.StepId==frozen.Workflow.Steps[0].StepId).StepInstanceId,
            tasks.Select(task=>task.CROSS_TASK_ID).Order(StringComparer.Ordinal).ToArray(),ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Intent=frozen.Revision(),Graph=graph,Tasks=tasks})),DateTimeOffset.UtcNow);
        receipt.Require(frozen);return receipt;
    }
    private static string DevelopmentStepId(string instance,string step)=>"development-step-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Instance=instance,Step=step}));
    private static void RequireDevelopmentStart(ProcessInstance instance,DevelopmentReviewStart original,IReadOnlyList<ProcessStepDefinition> steps)
    {
        var context=original.Source.Context;var id=original.InstanceId();
        if(instance.InstanceId!=id || instance.ProcessId!=original.Workflow.ProcessId || instance.EntityType!="FIELD_DEVELOPMENT_PLAN" || instance.EntityId!=original.Source.PlanId ||
            instance.FieldId!=context.FieldId || instance.StartedBy!=context.ActorUserId || instance.Status!=ProcessStatus.IN_PROGRESS || instance.CurrentState!="INITIAL" ||
            instance.ProcessData?.InstanceId!=id || DevelopmentReviewStart.FromPayload(JsonSerializer.Serialize(instance.ProcessData),id,context.FieldId!,instance.EntityId).Revision()!=original.Revision() ||
            instance.History.Count!=1 || instance.History[0].Action!="PROCESS_STARTED" || instance.History[0].PerformedBy!=context.ActorUserId ||
            instance.History[0].HistoryId!="development-start-"+ProcessDefinitionRevision.FromJson(id) || instance.History[0].Timestamp!=instance.StartDate ||
            instance.StepInstances.Count!=steps.Count || instance.CurrentStepId!=steps[0].StepId)
            throw new InvalidOperationException("The original development handoff differs from its retained initial submission or has progressed.");
        foreach(var template in steps)
        {
            var matches=instance.StepInstances.Where(step=>step.StepId==template.StepId).ToArray();
            if(matches.Length!=1 || matches[0].StepInstanceId!=DevelopmentStepId(id,template.StepId) || matches[0].InstanceId!=id || matches[0].StepName!=template.StepName ||
                matches[0].SequenceNumber!=template.SequenceNumber || matches[0].RequiredRole!=template.RequiredRoles[0] || matches[0].ApprovalRequired!=template.RequiresApproval ||
                matches[0].SlaHours!=template.SlaHours || matches[0].Status!=(template.StepId==instance.CurrentStepId?StepStatus.PENDING:StepStatus.BLOCKED) ||
                !string.IsNullOrEmpty(matches[0].AssignedTo) || matches[0].StartDate is not null || matches[0].CompletionDate is not null)
                throw new InvalidOperationException("The original development handoff steps changed.");
        }
    }
}
