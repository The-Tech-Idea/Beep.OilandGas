using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
using System.Data;
using System.Text.Json;
using System.Text.Json.Nodes;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.Core;
namespace Beep.OilandGas.LifeCycle.Services.Processes;

public partial class PPDMProcessService : ICompanyMaintenanceClosureOwner
{
    private const string MaintenanceClosureKey="CompanyMaintenanceClosure";
    private sealed record MaintenanceClosureEvidence(CompanyMaintenanceClosureCommand Command,PROCESS_HISTORY History,List<CROSS_PERSONA_TASK> Closed,MaintenanceGraphDelta Delta);
    private static void RequireMaintenanceTerminalReturn(ProcessInstance instance,CompanyMaintenanceAssignedStageReview stage)
    {
        var step=instance.StepInstances.Single(row=>row.StepInstanceId==stage.StepInstanceId);
        if(instance.Status!=ProcessStatus.IN_PROGRESS || instance.CurrentStepId!=stage.StepId || step.Status!=StepStatus.PENDING || step.RequiredRole!=stage.AssignedRole ||
            instance.StepInstances.Any(row=>row.SequenceNumber>step.SequenceNumber || row.StepInstanceId!=step.StepInstanceId && (row.SequenceNumber>=step.SequenceNumber || row.Status!=StepStatus.COMPLETED)))
            throw new InvalidOperationException("Closure requires the actual terminal pending return step and every earlier duty completed.");
    }
    private static void ApplyMaintenanceClosure(ProcessInstance instance,CompanyMaintenanceClosureCommand command,DateTime utc)
    {
        var stage=command.Review.Stage;var step=instance.StepInstances.Single(row=>row.StepInstanceId==stage.StepInstanceId);
        step.Status=StepStatus.COMPLETED;step.StartDate??=utc;step.CompletionDate=utc;step.CompletedBy=stage.Context.ActorUserId;step.Outcome="MAINTENANCE_CLOSED";step.Notes=command.Reason;
        instance.Status=ProcessStatus.COMPLETED;instance.CurrentState="COMPLETED";instance.CompletionDate=utc;instance.CompletedBy=stage.Context.ActorUserId;instance.ProcessData.LastUpdated=utc;
    }
    private PROCESS_HISTORY MaintenanceClosureHistory(CompanyMaintenanceClosureCommand command,DateTime utc)
    {
        var stage=command.Review.Stage;var row=ConvertToProcessHistoryEntity(new ProcessHistoryEntry{HistoryId=command.HistoryId(),InstanceId=stage.Task.InstanceId,StepInstanceId=stage.StepInstanceId,Action="MAINTENANCE_CLOSED",PreviousState=stage.StepId,NewState="COMPLETED",Timestamp=utc,PerformedBy=stage.Context.ActorUserId,Notes=command.Reason});
        row.PPDM_GUID=MaintenanceAuditGuid("PROCESS_HISTORY",row.PROCESS_HISTORY_ID);row.ACTIVE_IND="Y";row.ROW_CREATED_BY=stage.Context.ActorUserId;row.ROW_CREATED_DATE=utc;row.ROW_CHANGED_BY=stage.Context.ActorUserId;row.ROW_CHANGED_DATE=utc;row.ROW_EFFECTIVE_DATE=utc;return row;
    }
    private static CompanyMaintenanceClosureRecovery MaintenanceClosureResult(MaintenanceClosureEvidence proof)
    {
        var command=proof.Command;var stage=command.Review.Stage;return new(command,new(stage.Context.Company,stage.Context.ActorUserId,command.IntentId,stage.Task.InstanceId,command.Reference().Query.SubmissionId,stage.StepInstanceId,command.HistoryId(),proof.Closed.Select(row=>row.CROSS_TASK_ID).Order(StringComparer.Ordinal).ToArray(),proof.History.ROW_CREATED_DATE!.Value,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(proof))));
    }
    // Peel only an exact, validated recorded transition in memory so original receipts remain usable.
    private string MaintenanceBeforeClosureGraph(string graph,FieldSelectionReview context,CompanyWorkOrderCompletionQuery query)
    {
        var node=JsonNode.Parse(graph)!.AsObject();var instance=MaintenanceGraphInstance(graph);var histories=MaintenanceGraphRows<PROCESS_HISTORY>(node,"History").Where(row=>row.EVENT_TYPE=="MAINTENANCE_CLOSED").ToArray();
        if(!instance.ProcessData.Data.TryGetValue(MaintenanceClosureKey,out var retained)){if(histories.Length!=0)throw new InvalidOperationException("Uncorrelated maintenance closure history cannot establish a prior graph.");return graph;}
        var proof=JsonSerializer.Deserialize<MaintenanceClosureEvidence>(JsonSerializer.Serialize(retained))??throw new InvalidOperationException("Original maintenance closure proof is unavailable.");proof.Command.Require();var command=proof.Command;var stage=command.Review.Stage;
        if(stage.Context.Company!=context.Company || stage.Context.Database!=context.Database || command.Reference().Query!=query || proof.Delta is null || proof.Delta.EvidenceKey!=MaintenanceClosureKey || proof.Delta.BeforeRevision!=stage.ProcessRevision || proof.History.ROW_CREATED_DATE is not {} utc || utc.Kind!=DateTimeKind.Utc || utc<command.Review.Current.Completion.ComparedAtUtc || histories.Length!=1 || proof.Closed is not {Count:>0 and <=128} ||
            JsonSerializer.Serialize(proof.History)!=JsonSerializer.Serialize(MaintenanceClosureHistory(command,utc)) || JsonSerializer.Serialize(histories[0])!=JsonSerializer.Serialize(proof.History))throw new InvalidOperationException("Original maintenance closure company/source/history/UTC proof differs.");
        var before=RestoreMaintenanceDelta(graph,proof.Delta);var original=JsonNode.Parse(before)!.AsObject();var expected=MaintenanceGraphInstance(before);RequireMaintenanceTerminalReturn(expected,stage);
        var acceptance=JsonSerializer.Deserialize<ReturnAcceptanceEvidence>(JsonSerializer.Serialize(expected.ProcessData.Data[ReturnAcceptanceKey]))??throw new InvalidOperationException("Original recorded physical acceptance is required.");
        if(JsonSerializer.Serialize(CompanyWorkOrderReturnFacts.From(ReturnAcceptanceResult(acceptance)))!=JsonSerializer.Serialize(command.Review.Acceptance))throw new InvalidOperationException("Closure does not bind the original recorded physical acceptance.");
        ApplyMaintenanceClosure(expected,command,utc);var process=MaintenancePreparedUpdate(original["Process"]!.Deserialize<PROCESS_INSTANCE>()!,ConvertToProcessInstanceEntity(expected),stage.Context.ActorUserId,utc);var projected=JsonNode.Parse(graph)!.AsObject();projected["Process"]=JsonSerializer.SerializeToNode(process);
        if(!JsonNode.DeepEquals(MaintenanceDeltaGraph(graph,MaintenanceClosureKey)["Process"],MaintenanceDeltaGraph(projected.ToJsonString(),MaintenanceClosureKey)["Process"]))throw new InvalidOperationException("Closure changed undeclared native process fields.");
        var step=expected.StepInstances.Single(row=>row.StepInstanceId==stage.StepInstanceId);var priorStep=MaintenanceGraphRows<PROCESS_STEP_INSTANCE>(original,"Steps").Single(row=>row.PROCESS_STEP_INSTANCE_ID==stage.StepInstanceId);var prepared=MaintenancePreparedUpdate(priorStep,ConvertToProcessStepInstanceEntity(step),stage.Context.ActorUserId,utc);
        if(JsonSerializer.Serialize(prepared)!=JsonSerializer.Serialize(MaintenanceGraphRows<PROCESS_STEP_INSTANCE>(node,"Steps").Single(row=>row.PROCESS_STEP_INSTANCE_ID==stage.StepInstanceId)))throw new InvalidOperationException("Closure changed undeclared final step fields.");
        RequireGraphTasks(node,proof.Closed);var priorTasks=MaintenanceGraphRows<CROSS_PERSONA_TASK>(original,"Tasks").Where(row=>row.PROCESS_STEP_INSTANCE_ID==stage.StepInstanceId).ToArray();if(priorTasks.Length!=proof.Closed.Count)throw new InvalidOperationException("Complete original terminal task population is required.");
        foreach(var task in proof.Closed)
        {
            var prior=priorTasks.Single(row=>row.CROSS_TASK_ID==task.CROSS_TASK_ID);var priorCopy=CompanyInventorySourceReview.Copy(prior);
            if(prior.PROCESS_INSTANCE_ID!=stage.Task.InstanceId || prior.ENTITY_TYPE!="WORK_ORDER" || prior.ENTITY_ID!=stage.Task.WorkOrderId || prior.ASSIGNED_ROLE!=stage.AssignedRole || prior.ACTIVE_IND!="Y" || prior.TASK_STATUS!="PENDING" || task.ROW_CHANGED_DATE is not {} at || at.Kind!=DateTimeKind.Utc || at<command.Review.Current.Completion.ComparedAtUtc || at>utc)throw new InvalidOperationException("Original terminal task ownership/status/audit differs.");
            priorCopy.TASK_STATUS="COMPLETED";priorCopy.ROW_CHANGED_BY=stage.Context.ActorUserId;priorCopy.ROW_CHANGED_DATE=at;
            if(JsonSerializer.Serialize(priorCopy)!=JsonSerializer.Serialize(task))throw new InvalidOperationException("Closure changed undeclared native task fields.");
        }
        var declared=new HashSet<(string,string)>{("Process",stage.Task.InstanceId),("Steps",stage.StepInstanceId),("History",command.HistoryId())};foreach(var task in proof.Closed)if(!declared.Add(("Tasks",task.CROSS_TASK_ID)))throw new InvalidOperationException("Unique closure task identities are required.");
        if(proof.Delta.Changes.Count!=declared.Count || proof.Delta.Changes.Any(change=>!declared.Contains((change.Section,change.Id)) || (change.Before is null)!=(change.Section=="History")))throw new InvalidOperationException("Closure delta contains undeclared updates or appends.");
        return before;
    }
    private async Task<(CompanyMaintenanceReturnAssignment Assignment,CompanyWorkOrderReturnFacts Acceptance)> ReviewMaintenanceClosureCore(FieldSelectionReview context,CompanyWorkOrderCompletionQuery query,Func<string?,Task> authority,bool owned)
    {
        query.Require();var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("Original maintenance lifecycle source is unavailable.");
        async Task Guard(string? role){if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);await authority(role);if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("Original maintenance closure binding changed.");}
        await Guard(null);var owner=await ReadExecutionHandoffOwner(context,query.Task);if(owner.Instance.ProcessData.Data.ContainsKey(MaintenanceClosureKey))throw new InvalidOperationException("Maintenance is already closed; recover its original reference or read recorded status.");
        var stage=await ReviewReturnAssignedStageAsync(context,query.Task,Guard);RequireMaintenanceTerminalReturn(owner.Instance,stage);var handoff=await ReadReturnHandoffEvidence(context,query,false,Guard);var acceptance=await ReadReturnAcceptanceEvidence(context,query,false)??throw new InvalidOperationException("Record independent physical return acceptance before closing maintenance.");
        var assignment=new CompanyMaintenanceReturnAssignment(stage,handoff.Command.Review.Decision,handoff.History.PROCESS_HISTORY_ID,handoff.History.ROW_CREATED_DATE!.Value,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(handoff)));assignment.Require();
        if(assignment.Stage.StepInstanceId!=acceptance.Command.Review.Assignment.Stage.StepInstanceId || stage.ProcessRevision!=ProcessDefinitionRevision.FromJson(await MaintenanceAcceptanceNativeGraph(query.Task.InstanceId,owner.Start.ProcessId)))throw new InvalidOperationException("Original terminal assignment/acceptance graph changed during closure review.");await Guard(stage.AssignedRole);return(assignment,CompanyWorkOrderReturnFacts.From(ReturnAcceptanceResult(acceptance)));
    }
    public Task<(CompanyMaintenanceReturnAssignment Assignment,CompanyWorkOrderReturnFacts Acceptance)> ReviewMaintenanceClosureStageAsync(FieldSelectionReview context,CompanyWorkOrderCompletionQuery query,Func<string?,Task> authority)=>ReviewMaintenanceClosureCore(context,query,authority,false);
    private async Task<MaintenanceClosureEvidence?> ReadMaintenanceClosureEvidence(FieldSelectionReview context,CompanyWorkOrderCompletionQuery query,bool privateRead)
    {
        var owner=await ReadExecutionHandoffOwner(context,query.Task);var graph=await MaintenanceAcceptanceNativeGraph(query.Task.InstanceId,owner.Start.ProcessId);var instance=MaintenanceGraphInstance(graph);
        if(privateRead && instance.ProcessData.Data.ContainsKey(MaintenanceClosureKey) && instance.CompletedBy!=context.ActorUserId)throw new UnauthorizedAccessException("Original private maintenance closure belongs to another account before decoding.");
        var before=MaintenanceBeforeClosureGraph(graph,context,query);if(before==graph)return null;MaintenanceBeforeReturnGraph(graph,context,query);return JsonSerializer.Deserialize<MaintenanceClosureEvidence>(JsonSerializer.Serialize(instance.ProcessData.Data[MaintenanceClosureKey]))!;
    }
    private async Task<CompanyMaintenanceClosureRecovery> MaintenanceClosureReceipt(FieldSelectionReview context,CompanyMaintenanceClosureReference reference,Func<string?,Task> authority)
    {
        var proof=await ReadMaintenanceClosureEvidence(context,reference.Query,true)??throw new InvalidOperationException("Original closure is unconfirmed; recovery cannot record it.");if(proof.Command.Reference()!=reference || proof.Command.Review.Stage.Context.ActorUserId!=context.ActorUserId)throw new UnauthorizedAccessException("Original private closure belongs to another account/reference.");await authority(proof.Command.Review.Stage.AssignedRole);var closing=await ReadMaintenanceClosureEvidence(context,reference.Query,true);if(JsonSerializer.Serialize(proof)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Original maintenance closure changed during recovery.");var result=MaintenanceClosureResult(proof);result.Require(context.Company,context.ActorUserId,reference);return result;
    }
    public async Task<CompanyMaintenanceClosureRecovery> RecoverMaintenanceClosureAsync(FieldSelectionReview context,CompanyMaintenanceClosureReference reference,Func<string?,Task> authority)
    {
        reference.Require();var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("Original lifecycle source is unavailable.");PPDMDataSourceTransaction.RequireIdle(source);await authority(null);var result=await MaintenanceClosureReceipt(context,reference,authority);await authority(result.Command.Review.Stage.AssignedRole);PPDMDataSourceTransaction.RequireIdle(source);if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("Original closure source changed.");return result;
    }
    public async Task<CompanyMaintenanceClosureFacts?> ReadMaintenanceClosureFactsAsync(FieldSelectionReview context,CompanyWorkOrderCompletionQuery query,Func<Task> authority)
    {
        query.Require();var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("Original lifecycle source is unavailable.");async Task Guard(){PPDMDataSourceTransaction.RequireIdle(source);await authority();if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("Original closure source changed.");}
        await Guard();var first=await ReadMaintenanceClosureEvidence(context,query,false);await Guard();var closing=await ReadMaintenanceClosureEvidence(context,query,false);if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Original public closure changed during observation.");await Guard();return closing is null?null:CompanyMaintenanceClosureFacts.From(MaintenanceClosureResult(closing));
    }
    public async Task<CompanyMaintenanceClosureRecovery> CloseMaintenanceAsync(CompanyMaintenanceClosureCommand original,Func<string?,Task> authority,Func<bool,Task<CompanyWorkOrderReturnReview?>> readEvidence)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var stage=command.Review.Stage;var context=stage.Context;var query=command.Reference().Query;var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("Original closure lifecycle source is unavailable.");
        async Task Guard(bool owned){if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);await authority(stage.AssignedRole);if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("Original closure lifecycle binding changed.");}
        await Guard(false);var existing=await ReadMaintenanceClosureEvidence(context,query,true);if(existing is not null){if(JsonSerializer.Serialize(existing.Command)!=JsonSerializer.Serialize(command))throw new InvalidOperationException("Original maintenance closure exists with another body/intent.");return await RecoverMaintenanceClosureAsync(context,command.Reference(),authority);}
        async Task Source(bool owned){var current=await readEvidence(owned);if(!owned && current is null || current is not null && current.SourceRevision()!=command.Review.Current.SourceRevision())throw new InvalidOperationException("Original current work/return evidence changed before closure.");}
        async Task Stage(bool owned){await Guard(owned);var current=await ReviewMaintenanceClosureCore(context,query,authority,owned);if(JsonSerializer.Serialize(current.Assignment)!=JsonSerializer.Serialize(command.Review.Current.Assignment) || current.Acceptance!=command.Review.Acceptance)throw new InvalidOperationException("Original terminal closure assignment/recorded physical acceptance changed.");}
        await Source(false);await Stage(false);await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Source(true);await Stage(true);var owner=await ReadExecutionHandoffOwner(context,query.Task);var before=await MaintenanceAcceptanceNativeGraph(query.Task.InstanceId,owner.Start.ProcessId);if(ProcessDefinitionRevision.FromJson(before)!=stage.ProcessRevision)throw new InvalidOperationException("Original reviewed closure graph changed.");
            var router=new CrossPersonaTaskRouter(_editor,_commonColumnHandler,_defaults,_metadata,()=>Task.FromResult(_connectionName),personas:_personaReader);var closed=await router.CompleteTaskSetInOwnedTransactionAsync(stage.Task.InstanceId,stage.StepInstanceId,stage.AssignedRole,"WORK_ORDER",stage.Task.WorkOrderId,context.ActorUserId,_connectionName);var utc=DateTime.UtcNow;if(utc<command.Review.Current.Completion.ComparedAtUtc)throw new InvalidOperationException("Closure UTC precedes current source comparison.");
            ApplyMaintenanceClosure(owner.Instance,command,utc);var history=MaintenanceClosureHistory(command,utc);await MaintenanceDecisionWriter(history,null,"PROCESS_HISTORY","PROCESS_HISTORY_ID",context.ActorUserId,PPDMRowMutation.Insert,()=>Guard(true)).InsertAsync(history,context.ActorUserId);
            var previousStep=CompanyInventorySourceReview.Copy((PROCESS_STEP_INSTANCE)(await GetProcessStepInstanceRepository().GetAsync(Key("PROCESS_STEP_INSTANCE_ID",stage.StepInstanceId))).Single());var step=MaintenancePreparedUpdate(previousStep,ConvertToProcessStepInstanceEntity(owner.Instance.StepInstances.Single(row=>row.StepInstanceId==stage.StepInstanceId)),context.ActorUserId,utc);await MaintenanceDecisionWriter(step,previousStep,"PROCESS_STEP_INSTANCE","PROCESS_STEP_INSTANCE_ID",context.ActorUserId,PPDMRowMutation.Update,()=>Guard(true)).UpdateAsync(step,context.ActorUserId);
            var previous=CompanyInventorySourceReview.Copy((PROCESS_INSTANCE)(await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",query.Task.InstanceId))).Single());var process=MaintenancePreparedUpdate(previous,ConvertToProcessInstanceEntity(owner.Instance),context.ActorUserId,utc);var after=JsonNode.Parse(await MaintenanceAcceptanceNativeGraph(query.Task.InstanceId,owner.Start.ProcessId))!.AsObject();after["Process"]=JsonSerializer.SerializeToNode(process);var delta=CaptureMaintenanceDelta(MaintenanceClosureKey,before,after.ToJsonString());owner.Instance.ProcessData.Data[MaintenanceClosureKey]=new MaintenanceClosureEvidence(command,history,closed,delta);process.INSTANCE_DATA_JSON=JsonSerializer.Serialize(owner.Instance.ProcessData);
            await MaintenanceDecisionWriter(process,previous,"PROCESS_INSTANCE","PROCESS_INSTANCE_ID",context.ActorUserId,PPDMRowMutation.Update,()=>Guard(true)).UpdateAsync(process,context.ActorUserId);await Guard(true);await Source(true);await MaintenanceClosureReceipt(context,command.Reference(),authority);
        });await Guard(false);await Source(false);return await RecoverMaintenanceClosureAsync(context,command.Reference(),authority);
    }
}
