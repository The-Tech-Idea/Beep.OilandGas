using System.Data;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.LifeCycle.Services.Processes;
public partial class PPDMProcessService
{
    private const string ReturnHandoffKey="CompanyMaintenanceReturnHandoff";
    private sealed record ReturnHandoffEvidence(CompanyMaintenanceReturnHandoffCommand Command,PROCESS_HISTORY History,List<CROSS_PERSONA_TASK> Closed,List<CROSS_PERSONA_TASK> Next,MaintenanceGraphDelta Delta);
    private static T[] MaintenanceGraphRows<T>(JsonObject graph,string section)=>graph[section]!.Deserialize<T[]>()??throw new InvalidOperationException("Complete typed native graph rows are required.");
    private ProcessInstance MaintenanceGraphInstance(string graph)
    {
        var node=JsonNode.Parse(graph)!.AsObject();var result=ConvertToProcessInstance(node["Process"]!.Deserialize<PROCESS_INSTANCE>()!);
        result.StepInstances=MaintenanceGraphRows<PROCESS_STEP_INSTANCE>(node,"Steps").Select(ConvertToProcessStepInstance).OrderBy(row=>row.SequenceNumber).ToList();
        result.History=MaintenanceGraphRows<PROCESS_HISTORY>(node,"History").Select(ConvertToProcessHistoryEntry).OrderBy(row=>row.Timestamp).ToList();return result;
    }
    private static T MaintenancePreparedUpdate<T>(T original,T business,string actor,DateTime utc) where T:ModelEntityBase
    {
        var result=CompanyInventorySourceReview.Copy(original);foreach(var property in typeof(T).GetProperties(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly))if(property.CanWrite)property.SetValue(result,property.GetValue(business));result.ROW_CHANGED_BY=actor;result.ROW_CHANGED_DATE=utc;return result;
    }
    private PROCESS_HISTORY ReturnHandoffHistory(CompanyMaintenanceReturnHandoffCommand command,DateTime utc)
    {
        var stage=command.Review.Stage;var row=ConvertToProcessHistoryEntity(new ProcessHistoryEntry{HistoryId=command.HistoryId(),InstanceId=stage.Task.InstanceId,StepInstanceId=stage.StepInstanceId,Action="VERIFIED_WORK_ROUTED",PreviousState=stage.StepId,NewState=command.Review.NextStepId,Timestamp=utc,PerformedBy=stage.Context.ActorUserId,Notes=command.Reason});
        row.PPDM_GUID=MaintenanceAuditGuid("PROCESS_HISTORY",row.PROCESS_HISTORY_ID);row.ACTIVE_IND="Y";row.ROW_CREATED_BY=stage.Context.ActorUserId;row.ROW_CREATED_DATE=utc;row.ROW_CHANGED_BY=stage.Context.ActorUserId;row.ROW_CHANGED_DATE=utc;row.ROW_EFFECTIVE_DATE=utc;return row;
    }
    private static void ApplyReturnHandoff(ProcessInstance instance,CompanyMaintenanceReturnHandoffCommand command,DateTime utc)
    {
        var review=command.Review;var current=instance.StepInstances.Single(row=>row.StepInstanceId==review.Stage.StepInstanceId);var next=instance.StepInstances.Single(row=>row.StepInstanceId==review.NextStepInstanceId);
        current.Status=StepStatus.COMPLETED;current.StartDate??=utc;current.CompletionDate=utc;current.CompletedBy=review.Stage.Context.ActorUserId;current.Outcome="VERIFIED_WORK_ROUTED";current.Notes=command.Reason;
        next.Status=StepStatus.PENDING;next.StartDate=utc;instance.CurrentStepId=next.StepId;instance.CurrentState=next.StepId;instance.ProcessData.LastUpdated=utc;
    }
    // This projection restores an exact validated earlier view in memory; it never writes historical rows.
    private string MaintenanceBeforeReturnGraph(string graph,FieldSelectionReview context,CompanyWorkOrderCompletionQuery query)
    {
        graph=MaintenanceBeforeReturnAcceptanceGraph(graph,context,query);
        var instance=MaintenanceGraphInstance(graph);var node=JsonNode.Parse(graph)!.AsObject();
        if(!instance.ProcessData.Data.TryGetValue(ReturnHandoffKey,out var retained))
        {if(MaintenanceGraphRows<PROCESS_HISTORY>(node,"History").Any(row=>row.EVENT_TYPE=="VERIFIED_WORK_ROUTED"))throw new InvalidOperationException("Uncorrelated return handoff history cannot establish a prior graph.");return graph;}
        var proof=JsonSerializer.Deserialize<ReturnHandoffEvidence>(JsonSerializer.Serialize(retained))??throw new InvalidOperationException("Original return handoff proof is unavailable.");proof.Command.Require();var review=proof.Command.Review;var stage=review.Stage;
        if(stage.Context.Company!=context.Company || stage.Context.Database!=context.Database || review.Completion.Report.Query!=query || proof.Delta is null || proof.Delta.EvidenceKey!=ReturnHandoffKey || proof.Delta.BeforeRevision!=stage.ProcessRevision || proof.History.ROW_CREATED_DATE is not {} utc || utc.Kind!=DateTimeKind.Utc || utc<review.Completion.ComparedAtUtc ||
            JsonSerializer.Serialize(proof.History)!=JsonSerializer.Serialize(ReturnHandoffHistory(proof.Command,utc)) || proof.Closed is not {Count:>0 and <=128} || proof.Next is not {Count:>0 and <=128})throw new InvalidOperationException("Original return handoff company/source/history/task proof differs.");
        var before=RestoreMaintenanceDelta(graph,proof.Delta);var prior=JsonNode.Parse(before)!.AsObject();var expected=MaintenanceGraphInstance(before);
        if(expected.CurrentStepId!=stage.StepId || expected.Status!=ProcessStatus.IN_PROGRESS || expected.StepInstances.Single(row=>row.StepInstanceId==stage.StepInstanceId).Status!=StepStatus.PENDING || expected.StepInstances.Single(row=>row.StepInstanceId==review.NextStepInstanceId).Status!=StepStatus.BLOCKED)
            throw new InvalidOperationException("Original reviewer and blocked return assignment differ from the reviewed source.");
        ApplyReturnHandoff(expected,proof.Command,utc);
        var process=MaintenancePreparedUpdate(prior["Process"]!.Deserialize<PROCESS_INSTANCE>()!,ConvertToProcessInstanceEntity(expected),stage.Context.ActorUserId,utc);
        var projected=JsonNode.Parse(graph)!.AsObject();projected["Process"]=JsonSerializer.SerializeToNode(process);
        if(!JsonNode.DeepEquals(MaintenanceDeltaGraph(graph,ReturnHandoffKey)["Process"],MaintenanceDeltaGraph(projected.ToJsonString(),ReturnHandoffKey)["Process"]))throw new InvalidOperationException("Recorded return process business/audit fields differ from the declared transition.");
        foreach(var step in expected.StepInstances.Where(row=>row.StepInstanceId==stage.StepInstanceId || row.StepInstanceId==review.NextStepInstanceId))
        {
            var original=MaintenanceGraphRows<PROCESS_STEP_INSTANCE>(prior,"Steps").Single(row=>row.PROCESS_STEP_INSTANCE_ID==step.StepInstanceId);var prepared=MaintenancePreparedUpdate(original,ConvertToProcessStepInstanceEntity(step),stage.Context.ActorUserId,utc);
            if(JsonSerializer.Serialize(prepared)!=JsonSerializer.Serialize(MaintenanceGraphRows<PROCESS_STEP_INSTANCE>(node,"Steps").Single(row=>row.PROCESS_STEP_INSTANCE_ID==step.StepInstanceId)))throw new InvalidOperationException("Recorded reviewer/return step differs from its exact declared transition.");
        }
        var changes=proof.Delta.Changes;var declared=new HashSet<(string,string)>{("Process",stage.Task.InstanceId),("Steps",stage.StepInstanceId),("Steps",review.NextStepInstanceId),("History",proof.Command.HistoryId())};
        foreach(var task in proof.Closed.Concat(proof.Next))if(!declared.Add(("Tasks",task.CROSS_TASK_ID)))throw new InvalidOperationException("Unique complete return task identities are required.");
        if(changes.Count!=declared.Count || changes.Any(change=>!declared.Contains((change.Section,change.Id)) || change.Section=="History" && change.Before is not null || change.Section is "Steps" or "Process" && change.Before is null || change.Section=="Tasks" && (change.Before is null)!=proof.Next.Any(row=>row.CROSS_TASK_ID==change.Id)))throw new InvalidOperationException("Return handoff delta contains undeclared writes or replacements.");
        if(!JsonNode.DeepEquals(JsonSerializer.SerializeToNode(proof.History),node["History"]!.AsArray().Single(row=>row!["PROCESS_HISTORY_ID"]!.GetValue<string>()==proof.Command.HistoryId())))throw new InvalidOperationException("Original return history row changed.");
        RequireGraphTasks(node,proof.Closed);RequireGraphTasks(node,proof.Next);
        if(proof.Closed.Any(row=>row.PROCESS_STEP_INSTANCE_ID!=stage.StepInstanceId || row.ASSIGNED_ROLE!=stage.AssignedRole || row.TASK_STATUS!="COMPLETED" || row.ROW_CHANGED_BY!=stage.Context.ActorUserId || row.ROW_CHANGED_DATE is not {} at || at.Kind!=DateTimeKind.Utc || at<review.Decision.RecordedAtUtc || at>utc) ||
            proof.Next.Any(row=>row.PROCESS_STEP_INSTANCE_ID!=review.NextStepInstanceId || row.ASSIGNED_ROLE!=review.NextRole || row.TASK_STATUS!="PENDING" || row.ROW_CREATED_BY!=stage.Context.ActorUserId || row.ROW_CREATED_DATE is not {} at || at.Kind!=DateTimeKind.Utc || at<utc) ||
            proof.Closed.Concat(proof.Next).Any(row=>row.PROCESS_INSTANCE_ID!=stage.Task.InstanceId || row.ENTITY_TYPE!="WORK_ORDER" || row.ENTITY_ID!=stage.Task.WorkOrderId || row.ACTIVE_IND!="Y"))throw new InvalidOperationException("Original complete reviewer/return task population or audit differs.");return before;
    }
    private static void RequireGraphTasks(JsonObject graph,IReadOnlyList<CROSS_PERSONA_TASK> expected)
    {
        if(expected.Count==0)throw new InvalidOperationException("Complete original task set is required.");var actual=MaintenanceGraphRows<CROSS_PERSONA_TASK>(graph,"Tasks").Where(row=>row.PROCESS_STEP_INSTANCE_ID==expected[0].PROCESS_STEP_INSTANCE_ID).OrderBy(row=>row.CROSS_TASK_ID,StringComparer.Ordinal).ToArray();
        if(JsonSerializer.Serialize(actual)!=JsonSerializer.Serialize(expected.OrderBy(row=>row.CROSS_TASK_ID,StringComparer.Ordinal).ToArray()))throw new InvalidOperationException("Complete original native task set differs.");
    }
    private async Task<ReturnHandoffEvidence> ReadReturnHandoffEvidence(FieldSelectionReview context,CompanyWorkOrderCompletionQuery query,bool privateRead,Func<string?,Task> authority)
    {
        var owner=await ReadExecutionHandoffOwner(context,query.Task);var graph=await MaintenanceAcceptanceNativeGraph(query.Task.InstanceId,owner.Start.ProcessId);var instance=MaintenanceGraphInstance(graph);
        if(!instance.ProcessData.Data.TryGetValue(ReturnHandoffKey,out var retained))throw new InvalidOperationException("Original return handoff remains unconfirmed; recovery cannot record it.");
        if(privateRead && instance.StepInstances.SingleOrDefault(row=>row.Outcome=="VERIFIED_WORK_ROUTED")?.CompletedBy!=context.ActorUserId)throw new UnauthorizedAccessException("Original return handoff belongs to another reviewer account.");
        MaintenanceBeforeReturnGraph(graph,context,query);var proof=JsonSerializer.Deserialize<ReturnHandoffEvidence>(JsonSerializer.Serialize(retained))!;
        await authority(privateRead?proof.Command.Review.Stage.AssignedRole:proof.Command.Review.NextRole);
        var decision=await ReadCompletionDecisionEvidence(context,query)??throw new InvalidOperationException("Original recorded independent decision is unavailable.");
        if(JsonSerializer.Serialize(CompanyWorkOrderCompletionDecisionFacts.From(CompletionDecisionResult(decision)))!=JsonSerializer.Serialize(proof.Command.Review.Decision) ||
            !proof.Next.Select(row=>row.TARGET_PERSONA_CODE).Order(StringComparer.Ordinal).SequenceEqual((await _personaReader!.GetActivePersonasAsync(proof.Command.Review.NextRole)).Order(StringComparer.Ordinal)))throw new InvalidOperationException("Original verification or complete configured return persona population differs.");return proof;
    }
    private async Task<CompanyMaintenanceReturnHandoffRecovery> ReturnHandoffReceipt(FieldSelectionReview context,CompanyMaintenanceReturnHandoffReference reference,Func<string?,Task> authority)
    {
        var proof=await ReadReturnHandoffEvidence(context,reference.Query,true,authority);if(proof.Command.Reference()!=reference || proof.Command.Review.Stage.Context.ActorUserId!=context.ActorUserId)throw new UnauthorizedAccessException("Original return handoff account/reference differs.");
        var closing=await ReadReturnHandoffEvidence(context,reference.Query,true,authority);if(JsonSerializer.Serialize(proof)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Original return handoff changed during recovery.");var review=proof.Command.Review;
        var result=new CompanyMaintenanceReturnHandoffRecovery(proof.Command,new(context.Company,context.ActorUserId,reference.IntentId,reference.Query.Task.InstanceId,reference.Query.SubmissionId,review.Stage.StepInstanceId,review.NextStepInstanceId,review.NextRole,proof.History.PROCESS_HISTORY_ID,proof.Closed.Select(row=>row.CROSS_TASK_ID).Order(StringComparer.Ordinal).ToArray(),proof.Next.Select(row=>row.CROSS_TASK_ID).Order(StringComparer.Ordinal).ToArray(),proof.History.ROW_CREATED_DATE!.Value,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(proof))));result.Require(context.Company,context.ActorUserId,reference);return result;
    }
    public async Task<CompanyMaintenanceReturnHandoffRecovery> RecoverReturnHandoffAsync(FieldSelectionReview context,CompanyMaintenanceReturnHandoffReference reference,Func<string?,Task> authority)
    {
        reference.Require();var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("Original return lifecycle source is unavailable.");PPDMDataSourceTransaction.RequireIdle(source);await authority(null);var result=await ReturnHandoffReceipt(context,reference,authority);await authority(result.Command.Review.Stage.AssignedRole);
        if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("Original return lifecycle binding changed.");PPDMDataSourceTransaction.RequireIdle(source);return result;
    }
    private async Task<CompanyMaintenanceAssignedStageReview> ReviewReturnAssignedStageAsync(FieldSelectionReview context,CompanyMaintenanceReceivingQuery task,Func<string?,Task> authority)
    {
        var owner=await ReadExecutionHandoffOwner(context,task);var closed=owner.Instance.StepInstances.SingleOrDefault(row=>row.Outcome=="VERIFIED_WORK_ROUTED")??throw new InvalidOperationException("Original verified-work handoff is unavailable.");
        if(context.ActorUserId==closed.CompletedBy)throw new UnauthorizedAccessException("The reviewer cannot accept their own return handoff.");
        var retained=JsonSerializer.Deserialize<ReturnHandoffEvidence>(JsonSerializer.Serialize(owner.Instance.ProcessData.Data[ReturnHandoffKey]))??throw new InvalidOperationException("Original return handoff is unavailable.");
        if(context.ActorUserId==retained.Command.Review.Completion.Report.ExecutorUserId)throw new UnauthorizedAccessException("The original executor cannot accept their own return assignment.");
        var first=await ReadReturnHandoffEvidence(context,retained.Command.Reference().Query,false,authority);var closing=await ReadReturnHandoffEvidence(context,retained.Command.Reference().Query,false,authority);if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Original configured return assignment changed.");
        var stage=first.Command.Review;var next=owner.Instance.StepInstances.Single(row=>row.StepInstanceId==stage.NextStepInstanceId);if(!string.IsNullOrEmpty(next.AssignedTo) && next.AssignedTo!=context.ActorUserId && next.AssignedTo!=next.RequiredRole)throw new UnauthorizedAccessException("Current configured return assignment belongs to another account.");
        var graph=await MaintenanceAcceptanceNativeGraph(task.InstanceId,owner.Start.ProcessId);await authority(stage.NextRole);if(graph!=await MaintenanceAcceptanceNativeGraph(task.InstanceId,owner.Start.ProcessId))throw new InvalidOperationException("Original configured return graph changed.");
        var result=new CompanyMaintenanceAssignedStageReview(context,task,next.StepInstanceId,next.StepId,next.StepName,stage.NextRole,stage.Stage.ReceivingActor,stage.Stage.ReceivingHistoryId,stage.Stage.ReceivingApprovalId,owner.Start.DefinitionRevision,ProcessDefinitionRevision.FromJson(graph));result.Require(context.Company,context.ActorUserId,task);return result;
    }
    public async Task<CompanyMaintenanceReturnHandoffRecovery> RecordReturnHandoffAsync(CompanyMaintenanceReturnHandoffCommand original,Func<string?,Task> authority,Func<bool,Task<CompanyWorkOrderCompletionReview?>> readCompletion)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var review=command.Review;var context=review.Stage.Context;var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("Original return lifecycle source is unavailable.");
        async Task Guard(bool owned){if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);await authority(review.Stage.AssignedRole);if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("Original return lifecycle binding changed.");if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);}
        await Guard(false);var existing=await ReadExecutionHandoffOwner(context,review.Completion.Report.Query.Task);if(existing.Instance.ProcessData.Data.ContainsKey(ReturnHandoffKey)){var proof=await ReadReturnHandoffEvidence(context,command.Reference().Query,true,authority);if(JsonSerializer.Serialize(proof.Command)!=JsonSerializer.Serialize(command))throw new InvalidOperationException("Original return handoff body/intent differs.");return await RecoverReturnHandoffAsync(context,proof.Command.Reference(),authority);}
        async Task<CompanyWorkOrderCompletionReview> Source(bool owned){var current=await readCompletion(owned);if(!owned && current is null || current is not null && current.SourceRevision()!=review.Completion.SourceRevision())throw new InvalidOperationException("Original verified work/current completion source changed before return handoff.");return current??review.Completion;}
        async Task Stage(bool owned){await Guard(owned);var current=await ReviewReturnHandoffCoreAsync(context,review.Completion.Report.Query,await Source(owned),authority,owned);if(current.SourceRevision()!=review.SourceRevision())throw new InvalidOperationException("Original configured return review changed.");}
        await Stage(false);
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Stage(true);var owner=await ReadExecutionHandoffOwner(context,review.Stage.Task);var beforeGraph=await MaintenanceAcceptanceNativeGraph(review.Stage.Task.InstanceId,owner.Start.ProcessId);if(ProcessDefinitionRevision.FromJson(beforeGraph)!=review.Stage.ProcessRevision)throw new InvalidOperationException("Original full reviewer graph changed before return handoff.");
            var instance=owner.Instance;var router=new CrossPersonaTaskRouter(_editor,_commonColumnHandler,_defaults,_metadata,()=>Task.FromResult(_connectionName),personas:_personaReader);var closed=await router.CompleteTaskSetInOwnedTransactionAsync(instance.InstanceId,review.Stage.StepInstanceId,review.Stage.AssignedRole,"WORK_ORDER",review.Stage.Task.WorkOrderId,context.ActorUserId,_connectionName);
            var utc=DateTime.UtcNow;if(utc<review.Completion.ComparedAtUtc)throw new InvalidOperationException("Return handoff UTC precedes original verified evidence.");ApplyReturnHandoff(instance,command,utc);var history=ReturnHandoffHistory(command,utc);await MaintenanceDecisionWriter(history,null,"PROCESS_HISTORY","PROCESS_HISTORY_ID",context.ActorUserId,PPDMRowMutation.Insert,()=>Guard(true)).InsertAsync(history,context.ActorUserId);instance.History.Add(ConvertToProcessHistoryEntry(history));
            foreach(var step in instance.StepInstances.Where(row=>row.StepInstanceId==review.Stage.StepInstanceId || row.StepInstanceId==review.NextStepInstanceId))
            {var previous=CompanyInventorySourceReview.Copy((PROCESS_STEP_INSTANCE)(await GetProcessStepInstanceRepository().GetAsync(Key("PROCESS_STEP_INSTANCE_ID",step.StepInstanceId))).Single());var prepared=MaintenancePreparedUpdate(previous,ConvertToProcessStepInstanceEntity(step),context.ActorUserId,utc);await MaintenanceDecisionWriter(prepared,previous,"PROCESS_STEP_INSTANCE","PROCESS_STEP_INSTANCE_ID",context.ActorUserId,PPDMRowMutation.Update,()=>Guard(true)).UpdateAsync(prepared,context.ActorUserId);}
            var previousProcess=CompanyInventorySourceReview.Copy((PROCESS_INSTANCE)(await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",instance.InstanceId))).Single());var process=MaintenancePreparedUpdate(previousProcess,ConvertToProcessInstanceEntity(instance),context.ActorUserId,utc);await MaintenanceDecisionWriter(process,previousProcess,"PROCESS_INSTANCE","PROCESS_INSTANCE_ID",context.ActorUserId,PPDMRowMutation.Update,()=>Guard(true)).UpdateAsync(process,context.ActorUserId);
            var next=await router.RouteTaskInOwnedTransactionAsync(instance.InstanceId,review.NextStepInstanceId,review.NextRole,"WORK_ORDER",review.Stage.Task.WorkOrderId,null,context.ActorUserId);var afterGraph=await MaintenanceAcceptanceNativeGraph(instance.InstanceId,owner.Start.ProcessId);var delta=CaptureMaintenanceDelta(ReturnHandoffKey,beforeGraph,afterGraph);instance.ProcessData.Data[ReturnHandoffKey]=new ReturnHandoffEvidence(command,history,closed,next,delta);
            var closingProcess=MaintenancePreparedUpdate(process,ConvertToProcessInstanceEntity(instance),context.ActorUserId,utc);await MaintenanceDecisionWriter(closingProcess,process,"PROCESS_INSTANCE","PROCESS_INSTANCE_ID",context.ActorUserId,PPDMRowMutation.Update,()=>Guard(true)).UpdateAsync(closingProcess,context.ActorUserId);await Guard(true);await Source(true);await ReturnHandoffReceipt(context,command.Reference(),authority);
        });await Guard(false);await Source(false);return await RecoverReturnHandoffAsync(context,command.Reference(),authority);
    }
}
