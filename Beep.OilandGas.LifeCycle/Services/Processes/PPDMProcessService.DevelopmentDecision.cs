using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.LifeCycle.Models;
using Beep.OilandGas.DevelopmentPlanning.Models;
using System.Data;
using System.Reflection;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.Core;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

public partial class PPDMProcessService
{
    private const string DevelopmentDecisionsKey="DevelopmentReviewDecisions";
    // Original commands and readback evidence live in the existing process payload; approvals/history/tasks remain canonical rows.
    private sealed record DevelopmentDecisionProof(DevelopmentReviewDecision Command,DateTime At,string AfterBusinessRevision,
        PROCESS_APPROVAL Approval,PROCESS_HISTORY History,List<CROSS_PERSONA_TASK> Closed,List<CROSS_PERSONA_TASK> Next,string? NextStep);
    private static List<DevelopmentDecisionProof> DevelopmentProofs(ProcessInstance instance)
    {
        if(!instance.ProcessData.Data.TryGetValue(DevelopmentDecisionsKey,out var value))return [];
        var proofs=JsonSerializer.Deserialize<List<DevelopmentDecisionProof>>(JsonSerializer.Serialize(value))??throw new InvalidOperationException("The retained development decisions are unavailable.");
        if(proofs.Count is <1 or >64 || proofs.Any(proof=>proof is null))throw new InvalidOperationException("The retained development decision sequence is invalid.");
        return proofs;
    }
    private static string DevelopmentBusinessRevision(ProcessInstance instance)
    {
        var copy=JsonSerializer.Deserialize<ProcessInstance>(JsonSerializer.Serialize(instance))!;copy.ProcessData.Data.Remove(DevelopmentDecisionsKey);
        return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));
    }
    private async Task<string> ConfirmDevelopmentGraphAsync(ProcessInstance instance,DevelopmentReviewStart original,IReadOnlyList<ProcessStepDefinition> templates)
    {
        var proofs=DevelopmentProofs(instance);var id=original.InstanceId();var context=original.Source.Context;
        var liveDefinition=await GetProcessDefinitionAsync(original.Workflow.ProcessId)??throw new InvalidOperationException("The original development definition is unavailable.");
        DevelopmentReviewDefinition.Require(liveDefinition);
        if(ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(liveDefinition))!=original.Workflow.DefinitionRevision)throw new InvalidOperationException("The original development review definition changed.");
        var processRows=(await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).ToArray();
        var steps=(await GetProcessStepInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).ToArray();
        var history=(await GetProcessHistoryRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).ToArray();
        var approvals=(await GetProcessApprovalRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).ToArray();
        var tasks=(await new CrossPersonaTaskRouter(_editor,_commonColumnHandler,_defaults,_metadata,()=>Task.FromResult(_connectionName),personas:_personaReader)
            .GetAllTasksForDevelopmentEvidenceAsync(id,_connectionName)).ToArray();
        if(processRows.Length!=1 || processRows[0] is not PROCESS_INSTANCE process || process.ACTIVE_IND!="Y" || process.ROW_CREATED_BY!=context.ActorUserId ||
            instance.InstanceId!=id || instance.ProcessId!=original.Workflow.ProcessId || instance.EntityType!="FIELD_DEVELOPMENT_PLAN" || instance.EntityId!=original.Source.PlanId ||
            instance.FieldId!=context.FieldId || instance.StartedBy!=context.ActorUserId || instance.ProcessData.InstanceId!=id || instance.StepInstances.Count!=templates.Count ||
            steps.Length!=templates.Count || steps.Any(row=>row is not PROCESS_STEP_INSTANCE) || history.Any(row=>row is not PROCESS_HISTORY) || approvals.Any(row=>row is not PROCESS_APPROVAL) ||
            instance.ProcessData.Data.Keys.Any(key=>key is not (DevelopmentReviewStart.PayloadKey or DevelopmentDecisionsKey)))
            throw new InvalidOperationException("The complete development process ownership or native graph is ambiguous.");
        if(proofs.Count==0)
        {
            if(approvals.Length!=0)throw new InvalidOperationException("An unrecorded approval changed the initial development review.");
            RequireDevelopmentStart(instance,original,templates);await ConfirmStartedGraphAsync(instance);
        }
        else
        {
            if(proofs.Count>templates.Count || proofs.Select(proof=>proof.Command.IntentId).Distinct().Count()!=proofs.Count ||
                DevelopmentBusinessRevision(instance)!=proofs[^1].AfterBusinessRevision || approvals.Length!=proofs.Count || history.Length!=proofs.Count+1 ||
                instance.History.Count!=proofs.Count+1 || !instance.History.Any(entry=>entry.HistoryId=="development-start-"+ProcessDefinitionRevision.FromJson(id) && entry.Action=="PROCESS_STARTED" && entry.PerformedBy==context.ActorUserId && entry.Timestamp==instance.StartDate))
                throw new InvalidOperationException("The recorded development decisions or complete audit graph changed.");
            for(var index=0;index<proofs.Count;index++)
            {
                var proof=proofs[index];var command=proof.Command;command.Require();var review=command.Review;var template=templates[index];
                if(review.InstanceId!=id || review.Scope.Company!=context.Company || review.Scope.FieldId!=context.FieldId || review.Scope.CoreDatabase!=context.Database ||
                    review.Scope.PlanningDatabase!=original.Source.PlanningDatabase || review.Scope.LifecycleDatabase!=original.LifecycleDatabase || review.SourceRevision!=original.Source.Revision ||
                    review.DefinitionRevision!=original.Workflow.DefinitionRevision || review.StepId!=template.StepId || review.StepInstanceId!=DevelopmentStepId(id,template.StepId) ||
                    review.AssignedRole!=template.RequiredRoles[0] || !original.ReceivingSteps[index].Recipients.Any(row=>row.UserId==review.Scope.ActorUserId && row.PersonaCode==review.PersonaCode) ||
                    index<proofs.Count-1 && command.Decision!="APPROVE" || proof.At.Kind==DateTimeKind.Local || proof.At<instance.StartDate || index>0 && proof.At<proofs[index-1].At ||
                    proof.Approval.PROCESS_APPROVAL_ID!="development-approval-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Instance=id,command.IntentId})) ||
                    proof.Approval.PROCESS_INSTANCE_ID!=id || proof.Approval.PROCESS_STEP_INSTANCE_ID!=review.StepInstanceId || proof.Approval.APPROVAL_TYPE!="DEVELOPMENT_REVIEW" ||
                    proof.Approval.APPROVAL_STATUS!=(command.Decision=="APPROVE"?"APPROVED":"REJECTED") || proof.Approval.REQUIRED_ACTION!=command.Decision ||
                    proof.Approval.APPROVED_BY!=review.Scope.ActorUserId || proof.Approval.APPROVER_USER_ID!=review.Scope.ActorUserId || proof.Approval.APPROVAL_DATE!=proof.At ||
                    proof.Approval.APPROVAL_NOTES!=command.Notes || proof.Approval.REQUESTED_BY!=context.ActorUserId || proof.Approval.REQUESTED_DATE!=instance.StartDate ||
                    proof.History.PROCESS_HISTORY_ID!="development-decision-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Instance=id,command.IntentId})) ||
                    proof.History.PROCESS_INSTANCE_ID!=id || proof.History.PROCESS_STEP_INSTANCE_ID!=review.StepInstanceId || proof.History.USER_ID!=review.Scope.ActorUserId ||
                    proof.History.EVENT_TYPE!=(command.Decision=="APPROVE"?"DEVELOPMENT_REVIEW_APPROVED":"DEVELOPMENT_REVIEW_REJECTED") || proof.History.EVENT_DATE!=proof.At || proof.History.DETAILS!=command.Notes)
                    throw new InvalidOperationException("A retained development decision differs from its original independent source, stage or audit.");
                static void RequireRow<T>(IEnumerable<object> rows,T expected,Func<T,bool> key) where T:ModelEntityBase
                {
                    var found=rows.OfType<T>().Where(key).ToArray();
                    if(found.Length!=1 || expected.ACTIVE_IND!="Y" || JsonSerializer.Serialize(found[0])!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("A canonical development decision audit row changed.");
                }
                RequireRow(approvals,proof.Approval,row=>row.PROCESS_APPROVAL_ID==proof.Approval.PROCESS_APPROVAL_ID);
                RequireRow(history,proof.History,row=>row.PROCESS_HISTORY_ID==proof.History.PROCESS_HISTORY_ID);
                foreach(var task in proof.Closed)
                    if(task.TASK_STATUS!="COMPLETED" || task.PROCESS_INSTANCE_ID!=id || task.PROCESS_STEP_INSTANCE_ID!=review.StepInstanceId || task.ROW_CHANGED_BY!=review.Scope.ActorUserId ||
                        tasks.Count(row=>row.CROSS_TASK_ID==task.CROSS_TASK_ID && JsonSerializer.Serialize(row)==JsonSerializer.Serialize(task))!=1)
                        throw new InvalidOperationException("The complete closed development task set changed.");
                if(proof.Closed.Count!=original.Workflow.Steps[index].PersonaCodes.Count || !proof.Closed.Select(row=>row.TARGET_PERSONA_CODE).Order(StringComparer.Ordinal).SequenceEqual(original.Workflow.Steps[index].PersonaCodes.Order(StringComparer.Ordinal)))
                    throw new InvalidOperationException("A recorded development decision lost its complete persona task set.");
            }
        }
        var latest=proofs.LastOrDefault();var rejected=latest?.Command.Decision=="REJECT";var terminal=rejected || proofs.Count==templates.Count;
        var currentIndex=proofs.Count==0?0:terminal?proofs.Count-1:proofs.Count;
        for(var index=0;index<templates.Count;index++)
        {
            var template=templates[index];var model=instance.StepInstances.Single(row=>row.StepId==template.StepId);var row=steps.Cast<PROCESS_STEP_INSTANCE>().Single(step=>step.STEP_ID==template.StepId);
            var decided=index<proofs.Count;var proof=decided?proofs[index]:null;
            var status=decided?(proof!.Command.Decision=="REJECT"?StepStatus.FAILED:StepStatus.COMPLETED):index==currentIndex && !terminal?StepStatus.PENDING:StepStatus.BLOCKED;
            if(model.InstanceId!=id || model.StepInstanceId!=DevelopmentStepId(id,template.StepId) || model.StepName!=template.StepName || model.SequenceNumber!=template.SequenceNumber ||
                model.RequiredRole!=template.RequiredRoles[0] || !model.ApprovalRequired || model.SlaHours!=template.SlaHours || model.Status!=status ||
                model.CompletionDate!=(decided?proof!.At:null) || model.CompletedBy!=(decided?proof!.Command.Review.Scope.ActorUserId:null) ||
                model.Outcome!=(decided?(proof!.Command.Decision=="REJECT"?"REJECTED":"APPROVED"):"") || model.Notes!=(decided?proof!.Command.Notes:"") ||
                row.ACTIVE_IND!="Y" || row.ROW_CREATED_BY!=context.ActorUserId || row.ROW_CREATED_DATE is null || !string.IsNullOrEmpty(model.AssignedTo))
                throw new InvalidOperationException("The ordered development steps differ from the recorded decisions.");
        }
        if(instance.CurrentStepId!=templates[currentIndex].StepId || instance.Status!=(terminal?(rejected?ProcessStatus.FAILED:ProcessStatus.COMPLETED):ProcessStatus.IN_PROGRESS) ||
            instance.CurrentState!=(proofs.Count==0?"INITIAL":terminal?(rejected?"REVIEW_REJECTED":"REVIEW_APPROVED"):templates[currentIndex].StepId) ||
            instance.CompletionDate!=(terminal?latest!.At:null) || instance.CompletedBy!=(terminal?latest!.Command.Review.Scope.ActorUserId:null))
            throw new InvalidOperationException("The development review outcome differs from its recorded stage decisions.");
        var expectedTasks=proofs.SelectMany(proof=>proof.Closed).Select(row=>row.CROSS_TASK_ID).ToList();
        if(!terminal)
        {
            var active=instance.StepInstances.Single(row=>row.StepId==instance.CurrentStepId);
            var pending=tasks.Where(row=>row.PROCESS_STEP_INSTANCE_ID==active.StepInstanceId).ToArray();
            var personas=original.Workflow.Steps[currentIndex].PersonaCodes;
            if(pending.Length!=personas.Count || !pending.Select(row=>row.TARGET_PERSONA_CODE).Order(StringComparer.Ordinal).SequenceEqual(personas.Order(StringComparer.Ordinal)))
                throw new InvalidOperationException("The complete pending development recipient set changed.");
            var creator=latest?.Command.Review.Scope.ActorUserId??context.ActorUserId;
            foreach(var task in pending)
            {
                var taskId="task-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Process=id,Step=active.StepInstanceId,Persona=task.TARGET_PERSONA_CODE}));
                var taskContext=JsonSerializer.Serialize(new{assignedRole=active.RequiredRole,entityType=instance.EntityType,entityId=instance.EntityId,processInstanceId=id,stepInstanceId=active.StepInstanceId,scopeKind="FIELD"});
                var due=active.SlaHours.HasValue && active.StartDate.HasValue?active.StartDate.Value.AddHours(active.SlaHours.Value):(DateTime?)null;
                if(task.CROSS_TASK_ID!=taskId || task.TASK_STATUS!="PENDING" || task.ACTIVE_IND!="Y" || task.ENTITY_TYPE!=instance.EntityType || task.ENTITY_ID!=instance.EntityId ||
                    task.ASSIGNED_ROLE!=active.RequiredRole || task.TASK_TYPE!="APPROVAL" || task.PRIORITY is <1 or >4 || task.DUE_DATE!=due || task.ENTITY_DESCRIPTION is not null ||
                    task.ROUTE!=WorkflowTaskRoute.For("FIELD",id,instance.EntityType) || task.TASK_CONTEXT_JSON!=taskContext || task.ROW_CREATED_BY!=creator || task.ROW_CREATED_DATE is null ||
                    task.ROW_CHANGED_DATE is not null || !string.IsNullOrEmpty(task.ROW_CHANGED_BY))throw new InvalidOperationException("The pending original development task changed.");
            }
            expectedTasks.AddRange(pending.Select(row=>row.CROSS_TASK_ID));
            if(latest is not null && (!latest.Next.OrderBy(row=>row.CROSS_TASK_ID).Select(row=>JsonSerializer.Serialize(row)).SequenceEqual(pending.OrderBy(row=>row.CROSS_TASK_ID).Select(row=>JsonSerializer.Serialize(row))) || latest.NextStep!=active.StepInstanceId))
                throw new InvalidOperationException("The recorded next development assignment changed.");
        }
        else if(latest is not null && (latest.Next.Count!=0 || latest.NextStep is not null))throw new InvalidOperationException("A closed development review cannot route more work.");
        if(expectedTasks.Distinct(StringComparer.Ordinal).Count()!=expectedTasks.Count || tasks.Length!=expectedTasks.Count || tasks.Any(row=>!expectedTasks.Contains(row.CROSS_TASK_ID,StringComparer.Ordinal)))
            throw new InvalidOperationException("The complete development task population changed.");
        return JsonSerializer.Serialize(new{Process=process,Steps=steps.Cast<PROCESS_STEP_INSTANCE>().OrderBy(row=>row.PROCESS_STEP_INSTANCE_ID,StringComparer.Ordinal),History=history.Cast<PROCESS_HISTORY>().OrderBy(row=>row.PROCESS_HISTORY_ID,StringComparer.Ordinal),Approvals=approvals.Cast<PROCESS_APPROVAL>().OrderBy(row=>row.PROCESS_APPROVAL_ID,StringComparer.Ordinal),Tasks=tasks.OrderBy(row=>row.CROSS_TASK_ID,StringComparer.Ordinal)});
    }

    public Task<DevelopmentReviewDecisionReceipt> DecideDevelopmentReviewAsync(DevelopmentReviewDecision command,Func<Task> requireCurrentReview,Func<Task<DevelopmentPlanSourceReview>> readSource)
        =>ReadOrDecideDevelopmentAsync(command,requireCurrentReview,readSource,false);
    public Task<DevelopmentReviewDecisionReceipt> RecoverDevelopmentDecisionAsync(DevelopmentReviewDecision command,Func<Task> requireCurrentReview,Func<Task<DevelopmentPlanSourceReview>> readSource)
        =>ReadOrDecideDevelopmentAsync(command,requireCurrentReview,readSource,true);
    private async Task<DevelopmentReviewDecisionReceipt> ReadOrDecideDevelopmentAsync(DevelopmentReviewDecision command,Func<Task> requireCurrentReview,Func<Task<DevelopmentPlanSourceReview>> readSource,bool readOnly)
    {
        command.Require();var frozen=JsonSerializer.Deserialize<DevelopmentReviewDecision>(JsonSerializer.Serialize(command))!;var review=frozen.Review;var actor=review.Scope.ActorUserId;
        if(_connectionName!=review.Scope.LifecycleDatabase.ConnectionName)throw new InvalidOperationException("The original lifecycle decision target is required.");
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("The development decision source is unavailable.");
        async Task Guard(bool owned)
        {
            if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("The original development decision target changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            await requireCurrentReview();
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
        }
        await Guard(false);var firstSource=await readSource();
        if(firstSource.Revision!=review.SourceRevision || firstSource.Plan is null)throw new InvalidOperationException("The reviewed immutable source changed before the decision.");
        DevelopmentDecisionProof? recorded=null;string? graph=null;ProcessInstance? instance=null;DevelopmentReviewStart? original=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);instance=await LoadProcessInstanceAsync(review.InstanceId)??throw new InvalidOperationException("The original development review is unavailable.");
            original=DevelopmentReviewStart.FromPayload(JsonSerializer.Serialize(instance.ProcessData),instance.InstanceId,instance.FieldId,instance.EntityId);original.RequireSource(firstSource);
            var definition=await GetProcessDefinitionAsync(original.Workflow.ProcessId)??throw new InvalidOperationException("The original review definition is unavailable.");var templates=DevelopmentReviewDefinition.Require(definition);
            if(ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition))!=review.DefinitionRevision || review.DefinitionRevision!=original.Workflow.DefinitionRevision)
                throw new InvalidOperationException("The original reviewed definition changed.");
            await ConfirmDevelopmentGraphAsync(instance,original,templates);var proofs=DevelopmentProofs(instance);recorded=proofs.SingleOrDefault(proof=>proof.Command.IntentId==frozen.IntentId);
            if(recorded is not null)
            {
                if(recorded.Command.Revision()!=frozen.Revision())throw new InvalidOperationException("The original decision intent was changed.");
                graph=await ConfirmDevelopmentGraphAsync(instance,original,templates);await Guard(true);return;
            }
            if(ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(instance))!=review.ProcessRevision || instance.Status!=ProcessStatus.IN_PROGRESS ||
                instance.CurrentStepId!=review.StepId || review.Scope.Company!=original.Source.Context.Company || review.Scope.FieldId!=instance.FieldId ||
                review.Scope.CoreDatabase!=original.Source.Context.Database || review.Scope.PlanningDatabase!=original.Source.PlanningDatabase || review.Scope.LifecycleDatabase!=original.LifecycleDatabase ||
                !original.ReceivingSteps.Single(step=>step.StepId==review.StepId).Recipients.Any(row=>row.UserId==actor && row.PersonaCode==review.PersonaCode) ||
                actor==instance.StartedBy || actor==original.Source.Plan!.ROW_CREATED_BY)
                throw new InvalidOperationException("The original independent task or reviewed process changed before this decision.");
            var current=instance.StepInstances.Single(step=>step.StepInstanceId==review.StepInstanceId && step.StepId==review.StepId);
            if(current.RequiredRole!=review.AssignedRole || !current.ApprovalRequired || current.Status!=StepStatus.PENDING)throw new InvalidOperationException("The original pending approval stage is required.");
            var router=new CrossPersonaTaskRouter(_editor,_commonColumnHandler,_defaults,_metadata,()=>Task.FromResult(_connectionName),personas:new DevelopmentStartPersonas(original,()=>Guard(true)));
            var candidates=await router.ReadTaskSetInOwnedTransactionAsync(instance.InstanceId,current.StepInstanceId,current.RequiredRole!,instance.EntityType,instance.EntityId,proofs.LastOrDefault()?.Command.Review.Scope.ActorUserId??instance.StartedBy);
            if(candidates.Count(task=>task.CROSS_TASK_ID==review.TaskId && task.TARGET_PERSONA_CODE==review.PersonaCode && ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(task))==review.TaskRevision)!=1)
                throw new InvalidOperationException("The exact displayed independent task changed.");
            if(readOnly){graph=await ConfirmDevelopmentGraphAsync(instance,original,templates);await Guard(true);return;}
            var applied=await ApplyOwnedReviewStageAsync(instance,current,templates,router,actor,frozen.Decision,frozen.Notes,frozen.IntentId,"development","DEVELOPMENT_REVIEW",()=>Guard(true));
            instance=applied.Process;var processRow=applied.Row;var now=applied.At;var approval=applied.Approval;var history=applied.History;var closed=applied.Closed;var routed=applied.Routed;
            recorded=new(frozen,now,DevelopmentBusinessRevision(instance),approval,history,closed,routed,applied.NextStep);proofs.Add(recorded);instance.ProcessData.Data[DevelopmentDecisionsKey]=proofs;
            processRow.INSTANCE_DATA_JSON=JsonSerializer.Serialize(instance.ProcessData);if(processRow.INSTANCE_DATA_JSON.Length>1048576)throw new InvalidOperationException("The bounded original review audit exceeds its supported payload.");
            await GetProcessInstanceRepository().UpdateAsync(processRow,actor);await Guard(true);
            instance=await LoadProcessInstanceAsync(instance.InstanceId)??throw new InvalidOperationException("The retained development decision could not be confirmed.");
            graph=await ConfirmDevelopmentGraphAsync(instance,original,templates);await Guard(true);
        });
        await Guard(false);
        var closing=await LoadProcessInstanceAsync(review.InstanceId)??throw new InvalidOperationException("The original decision commit is unconfirmed.");
        var closingDefinition=await GetProcessDefinitionAsync(original!.Workflow.ProcessId)??throw new InvalidOperationException("The original decision definition is unavailable.");
        if(graph!=await ConfirmDevelopmentGraphAsync(closing,original,DevelopmentReviewDefinition.Require(closingDefinition)))throw new InvalidOperationException("The decision commit or complete task/approval graph is unconfirmed. Recover its retained original intent.");
        original.RequireSource(await readSource());await Guard(false);
        var receipt=new DevelopmentReviewDecisionReceipt(frozen.Revision(),review.InstanceId,recorded is not null,recorded?.Approval.PROCESS_APPROVAL_ID,recorded?.History.PROCESS_HISTORY_ID,recorded?.NextStep,closing.CurrentState,ProcessDefinitionRevision.FromJson(graph!),DateTimeOffset.UtcNow);
        receipt.Require(frozen);return receipt;
    }
}
