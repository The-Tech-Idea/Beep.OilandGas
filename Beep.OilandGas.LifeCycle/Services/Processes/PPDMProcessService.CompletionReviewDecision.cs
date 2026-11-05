using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.LifeCycle.Models;
using Beep.OilandGas.DrillingAndConstruction.Models;
using System.Data;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.Core;
namespace Beep.OilandGas.LifeCycle.Services.Processes;
public partial class PPDMProcessService
{
    private static void RequireCompletionAudit(ModelEntityBase row,string creator,string changer,DateTime createdAfter,DateTime changedAfter)
    {
        var now=DateTime.UtcNow;
        if(row.ACTIVE_IND!="Y" || row.ROW_CREATED_BY!=creator || row.ROW_CHANGED_BY!=changer || row.ROW_CREATED_DATE is not {} created || row.ROW_CHANGED_DATE is not {} changed ||
            created.Kind==DateTimeKind.Local || changed.Kind==DateTimeKind.Local || created<createdAfter || changed<created || changed<changedAfter || created>now || changed>now)
            throw new InvalidOperationException("The completion decision graph differs from its actual common-column creation/change audit.");
    }
    private const string CompletionDecisionsKey="DrillingCompletionDecisions";
    private sealed record CompletionDecisionProof(DrillingCompletionReviewDecision Command,DateTime At,string AfterBusinessRevision,
        PROCESS_APPROVAL Approval,PROCESS_HISTORY History,List<CROSS_PERSONA_TASK> Before,List<CROSS_PERSONA_TASK> Closed,List<CROSS_PERSONA_TASK> Next,string? NextStep);
    private static List<CompletionDecisionProof> CompletionProofs(ProcessInstance process)
    {
        if(!process.ProcessData.Data.TryGetValue(CompletionDecisionsKey,out var value))return [];
        var result=JsonSerializer.Deserialize<List<CompletionDecisionProof>>(JsonSerializer.Serialize(value))??throw new InvalidOperationException("The original completion review decision sequence is unavailable.");
        if(result.Count is <1 or >64 || result.Any(proof=>proof is null))throw new InvalidOperationException("The complete completion review decision sequence is invalid.");return result;
    }
    private static string CompletionBusinessRevision(ProcessInstance process)
    {
        var copy=CompanyInventorySourceReview.Copy(process);copy.ProcessData.Data.Remove(CompletionDecisionsKey);return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));
    }
    private static Dictionary<string,object> CompletionAcceptanceAudit(DrillingCompletionReviewDecision command)=>command.Acceptance is null?new():new(){{"IntentRevision",command.Revision()},{"PhysicalCompletionAccepted",true}};
    private static ProcessStepDefinition[] CompletionTemplates(DrillingCompletionReviewStart original)=>original.Workflow.Stages.Select(stage=>new ProcessStepDefinition{
        StepId=stage.Definition.StepId,StepName=stage.Definition.StepName,SequenceNumber=stage.Definition.Sequence,StepType="APPROVAL",RequiresApproval=true,RequiredRoles=[stage.Definition.AssignedRole],SlaHours=stage.Definition.SlaHours}).ToArray();
    private static void RequireCompletionStage(DrillingCompletionReviewDecision command,DrillingCompletionReviewStart original,int index)
    {
        command.Require();if(command.Review.Original.Revision()!=original.Revision() || command.Review.StageNumber!=index+1)throw new UnauthorizedAccessException("The independent decision belongs to another original completion review or ordered stage.");
    }
    private async Task<string> ConfirmCompletionReviewGraphAsync(ProcessInstance process,DrillingCompletionReviewStart original)
    {
        var id=original.InstanceId();var owner=original.Scope();var proofs=CompletionProofs(process);var templates=CompletionTemplates(original);
        var processRows=(await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).ToArray();
        var steps=(await GetProcessStepInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).ToArray();
        var history=(await GetProcessHistoryRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).ToArray();
        var approvals=(await GetProcessApprovalRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).ToArray();
        var tasks=(await new CrossPersonaTaskRouter(_editor,_commonColumnHandler,_defaults,_metadata,()=>Task.FromResult(_connectionName)).GetAllTasksForDevelopmentEvidenceAsync(id,_connectionName)).ToArray();
        if(processRows.Length!=1 || processRows[0] is not PROCESS_INSTANCE row || row.ACTIVE_IND!="Y" || row.ROW_CREATED_BY!=owner.ActorUserId ||
            process.InstanceId!=id || process.EntityType!="DRILLING_DAILY_REPORT" || process.EntityId!=original.ClaimId || process.FieldId!=owner.FieldId || process.ProcessId!=original.Workflow.ProcessId ||
            process.StartedBy!=owner.ActorUserId || DrillingCompletionReviewStart.FromPayload(JsonSerializer.Serialize(process.ProcessData),id,owner.FieldId,process.EntityId).Revision()!=original.Revision() ||
            process.StepInstances.Count!=templates.Length || steps.Length!=templates.Length || steps.Any(item=>item is not PROCESS_STEP_INSTANCE) || history.Any(item=>item is not PROCESS_HISTORY) || approvals.Any(item=>item is not PROCESS_APPROVAL) ||
            process.ProcessData.Data.Keys.Any(key=>key is not (DrillingCompletionReviewStart.PayloadKey or CompletionDecisionsKey)))
            throw new InvalidOperationException("The complete original completion review process ownership or graph is ambiguous.");
        if(proofs.Count==0)await ConfirmCompletionReviewInitialGraphAsync(process,original,new CrossPersonaTaskRouter(_editor,_commonColumnHandler,_defaults,_metadata,()=>Task.FromResult(_connectionName)));
        else if(proofs.Count>templates.Length || proofs.Select(proof=>proof.Command.IntentId).Distinct().Count()!=proofs.Count ||
            CompletionBusinessRevision(process)!=proofs[^1].AfterBusinessRevision || approvals.Length!=proofs.Count || history.Length!=proofs.Count+1 || process.History.Count!=proofs.Count+1 ||
            !process.History.Any(item=>item.HistoryId==CompletionReviewHistoryId(id) && item.Action=="PROCESS_STARTED" && item.PerformedBy==owner.ActorUserId && item.Timestamp==process.StartDate))
            throw new InvalidOperationException("The recorded completion review decisions or complete audit graph changed.");
        static void RequireRow<T>(IEnumerable<object> rows,T expected,Func<T,bool> key) where T:ModelEntityBase
        {
            var found=rows.OfType<T>().Where(key).ToArray();
            if(found.Length!=1 || expected.ACTIVE_IND!="Y" || JsonSerializer.Serialize(found[0])!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("A canonical completion review decision audit row changed.");
        }
        RequireCompletionAudit(row,owner.ActorUserId,proofs.LastOrDefault()?.Command.Scope().ActorUserId??owner.ActorUserId,process.StartDate,proofs.LastOrDefault()?.At??process.StartDate);
        var starts=history.OfType<PROCESS_HISTORY>().Where(item=>item.PROCESS_HISTORY_ID==CompletionReviewHistoryId(id)).ToArray();
        if(starts.Length!=1 || starts[0].PROCESS_INSTANCE_ID!=id || starts[0].EVENT_TYPE!="PROCESS_STARTED" || starts[0].EVENT_DATE!=process.StartDate || starts[0].USER_ID!=owner.ActorUserId ||
            !string.IsNullOrEmpty(starts[0].PROCESS_STEP_INSTANCE_ID) || !string.IsNullOrEmpty(starts[0].FROM_STATUS) || starts[0].TO_STATUS!="INITIAL" || starts[0].DETAILS!=CompletionReviewStartNotes(original))
            throw new InvalidOperationException("The original completion start audit changed.");
        RequireCompletionAudit(starts[0],owner.ActorUserId,owner.ActorUserId,process.StartDate,process.StartDate);
        if(process.ProcessData.LastUpdated!=(proofs.LastOrDefault()?.At??process.StartDate))throw new InvalidOperationException("The completion decision audit clock changed.");
        for(var index=0;index<proofs.Count;index++)
        {
            var proof=proofs[index];var command=proof.Command;RequireCompletionStage(command,original,index);var actor=command.Scope().ActorUserId;
            var digest=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Instance=id,command.IntentId}));
            if(index<proofs.Count-1 && command.Decision!="APPROVE" || proof.At.Kind==DateTimeKind.Local ||
                proof.At<process.StartDate || index>0 && proof.At<proofs[index-1].At ||
                proof.Approval.PROCESS_APPROVAL_ID!="completion-stage-approval-"+digest || proof.Approval.PROCESS_INSTANCE_ID!=id || proof.Approval.PROCESS_STEP_INSTANCE_ID!=command.Review.StepInstanceId ||
                proof.Approval.APPROVAL_TYPE!=(command.Acceptance is null?"COMPLETION_REVIEW":"COMPLETION_ACCEPTANCE") || proof.Approval.APPROVAL_STATUS!=(command.Decision=="REJECT"?"REJECTED":"APPROVED") || proof.Approval.REQUIRED_ACTION!=command.Decision ||
                proof.Approval.APPROVED_BY!=actor || proof.Approval.APPROVER_USER_ID!=actor || proof.Approval.APPROVAL_DATE!=proof.At || proof.Approval.APPROVAL_NOTES!=command.Rationale ||
                proof.Approval.REQUESTED_BY!=owner.ActorUserId || proof.Approval.REQUESTED_DATE!=process.StartDate || proof.Approval.APPROVER_SEQUENCE!=templates[index].SequenceNumber ||
                proof.History.PROCESS_HISTORY_ID!="completion-stage-decision-"+digest || proof.History.PROCESS_INSTANCE_ID!=id || proof.History.PROCESS_STEP_INSTANCE_ID!=command.Review.StepInstanceId ||
                proof.History.USER_ID!=actor || proof.History.EVENT_DATE!=proof.At || proof.History.DETAILS!=command.Rationale || proof.History.EVENT_TYPE!=(command.Acceptance is not null?"COMPLETION_ACCEPTANCE_APPROVED":command.Decision=="REJECT"?"COMPLETION_REVIEW_REJECTED":"COMPLETION_REVIEW_APPROVED") ||
                proof.History.EVENT_DATA_JSON!=JsonSerializer.Serialize(CompletionAcceptanceAudit(command)) ||
                command.Acceptance is {} acceptance && proof.At<acceptance.AcceptedAtUtc.UtcDateTime)throw new InvalidOperationException("A completion review decision differs from its exact retained stage, independent actor or audit.");
            RequireCompletionAudit(proof.Approval,actor,actor,proof.At,proof.At);RequireCompletionAudit(proof.History,actor,actor,proof.At,proof.At);
            RequireRow(approvals,proof.Approval,item=>item.PROCESS_APPROVAL_ID==proof.Approval.PROCESS_APPROVAL_ID);
            RequireRow(history,proof.History,item=>item.PROCESS_HISTORY_ID==proof.History.PROCESS_HISTORY_ID);
            var configured=original.Workflow.Stages[index].Definition;var taskCreator=index==0?owner.ActorUserId:proofs[index-1].Command.Scope().ActorUserId;var createdAfter=index==0?process.StartDate:proofs[index-1].At;
            if(proof.Before is null || proof.Before.Count!=configured.PersonaCodes.Count || proof.Before.Select(task=>task.CROSS_TASK_ID).Distinct(StringComparer.Ordinal).Count()!=proof.Before.Count ||
                !proof.Before.Select(task=>task.TARGET_PERSONA_CODE).Order(StringComparer.Ordinal).SequenceEqual(configured.PersonaCodes.Order(StringComparer.Ordinal)) ||
                proof.Before.Count(task=>task.CROSS_TASK_ID==command.Review.TaskId && ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(task))==command.Review.TaskRevision)!=1)
                throw new InvalidOperationException("The displayed original complete completion task set changed.");
            if(index>0 && !proofs[index-1].Next.OrderBy(task=>task.CROSS_TASK_ID).Select(task=>JsonSerializer.Serialize(task)).SequenceEqual(proof.Before.OrderBy(task=>task.CROSS_TASK_ID).Select(task=>JsonSerializer.Serialize(task))))
                throw new InvalidOperationException("The completion decision no longer follows its exact previous receiving set.");
            foreach(var pending in proof.Before)
            {
                var context=JsonSerializer.Serialize(new{assignedRole=configured.AssignedRole,entityType="DRILLING_DAILY_REPORT",entityId=original.ClaimId,processInstanceId=id,stepInstanceId=command.Review.StepInstanceId,scopeKind="FIELD"});
                if(pending.CROSS_TASK_ID!=DrillingCompletionReviewStartReceipt.TaskId(id,command.Review.StepInstanceId,pending.TARGET_PERSONA_CODE) || pending.PROCESS_INSTANCE_ID!=id || pending.PROCESS_STEP_INSTANCE_ID!=command.Review.StepInstanceId ||
                    pending.ASSIGNED_ROLE!=configured.AssignedRole || pending.ENTITY_TYPE!="DRILLING_DAILY_REPORT" || pending.ENTITY_ID!=original.ClaimId || pending.ENTITY_DESCRIPTION!=original.Description || pending.TASK_STATUS!="PENDING" ||
                    pending.TASK_TYPE!="APPROVAL" || pending.TASK_CONTEXT_JSON!=context || pending.ROUTE!=WorkflowTaskRoute.For("FIELD",id,"DRILLING_DAILY_REPORT") || pending.PRIORITY is <1 or >4 || pending.DUE_DATE!=(index==0 || configured.SlaHours is null?null:createdAfter.AddHours(configured.SlaHours.Value)))
                    throw new InvalidOperationException("The original reviewed completion task identity or routing changed.");
                RequireCompletionAudit(pending,taskCreator,taskCreator,createdAfter,createdAfter);
            }
            if(proof.Closed.Count!=original.Workflow.Stages[index].Definition.PersonaCodes.Count ||
                !proof.Closed.Select(task=>task.TARGET_PERSONA_CODE).Order(StringComparer.Ordinal).SequenceEqual(original.Workflow.Stages[index].Definition.PersonaCodes.Order(StringComparer.Ordinal)))
                throw new InvalidOperationException("The complete closed completion review persona task set changed.");
            foreach(var task in proof.Closed)
            {
                RequireCompletionAudit(task,index==0?owner.ActorUserId:proofs[index-1].Command.Scope().ActorUserId,actor,index==0?process.StartDate:proofs[index-1].At,index==0?process.StartDate:proofs[index-1].At);
                var pending=proof.Before.SingleOrDefault(row=>row.CROSS_TASK_ID==task.CROSS_TASK_ID)??throw new InvalidOperationException("A closed completion task has no original pending snapshot.");
                var restored=CompanyInventorySourceReview.Copy(task);restored.TASK_STATUS="PENDING";restored.ROW_CHANGED_BY=pending.ROW_CHANGED_BY;restored.ROW_CHANGED_DATE=pending.ROW_CHANGED_DATE;
                if(JsonSerializer.Serialize(restored)!=JsonSerializer.Serialize(pending))throw new InvalidOperationException("The closed completion task differs from its exact original pending snapshot.");
                if(task.ROW_CHANGED_DATE>proof.At)throw new InvalidOperationException("A completion task closure occurs after its decision audit.");
                if(task.PROCESS_INSTANCE_ID!=id || task.PROCESS_STEP_INSTANCE_ID!=command.Review.StepInstanceId || task.TASK_STATUS!="COMPLETED" || task.ROW_CHANGED_BY!=actor || task.ROW_CHANGED_DATE is null ||
                    tasks.Count(item=>item.CROSS_TASK_ID==task.CROSS_TASK_ID && JsonSerializer.Serialize(item)==JsonSerializer.Serialize(task))!=1)
                    throw new InvalidOperationException("A closed completion review task differs from its recorded stage proof.");
            }
        }
        var last=proofs.LastOrDefault();var rejected=last?.Command.Decision=="REJECT";var terminal=rejected || proofs.Count==templates.Length;
        var activeIndex=proofs.Count==0?0:terminal?proofs.Count-1:proofs.Count;
        for(var index=0;index<templates.Length;index++)
        {
            var expected=templates[index];var stage=process.StepInstances.Single(item=>item.StepId==expected.StepId);var native=steps.Cast<PROCESS_STEP_INSTANCE>().Single(item=>item.STEP_ID==expected.StepId);
            var decided=index<proofs.Count;var proof=decided?proofs[index]:null;
            var status=decided?(proof!.Command.Decision=="REJECT"?StepStatus.FAILED:StepStatus.COMPLETED):index==activeIndex && !terminal?StepStatus.PENDING:StepStatus.BLOCKED;
            if(stage.StepInstanceId!=DrillingCompletionReviewStartReceipt.StepId(id,expected.StepId) || stage.InstanceId!=id || stage.StepName!=expected.StepName || stage.SequenceNumber!=expected.SequenceNumber ||
                stage.RequiredRole!=expected.RequiredRoles[0] || !stage.ApprovalRequired || stage.SlaHours!=expected.SlaHours || stage.Status!=status || !string.IsNullOrEmpty(stage.AssignedTo) ||
                stage.CompletionDate!=(decided?proof!.At:null) || stage.CompletedBy!=(decided?proof!.Command.Scope().ActorUserId:null) ||
                stage.Outcome!=(decided?(proof!.Command.Decision=="REJECT"?"REJECTED":"APPROVED"):"") || stage.Notes!=(decided?proof!.Command.Rationale:"") || native.ACTIVE_IND!="Y" || native.ROW_CREATED_BY!=owner.ActorUserId || native.ROW_CREATED_DATE is null)
                throw new InvalidOperationException("The ordered native completion review stages differ from their recorded decisions.");
            var expectedStart=decided?(index==0?proof!.At:proofs[index-1].At):!terminal && index==activeIndex && index>0?last!.At:(DateTime?)null;
            if(stage.StartDate!=expectedStart || stage.Approvals.Count!=0 || approvals.OfType<PROCESS_APPROVAL>().Count(approval=>approval.PROCESS_STEP_INSTANCE_ID==stage.StepInstanceId)!=(decided?1:0))throw new InvalidOperationException("The completion stage start or approval population differs from its ordered decision.");
            RequireCompletionAudit(native,owner.ActorUserId,decided?proof!.Command.Scope().ActorUserId:expectedStart is not null?proofs[index-1].Command.Scope().ActorUserId:owner.ActorUserId,process.StartDate,decided?proof!.At:expectedStart??process.StartDate);
        }
        if(process.CurrentStepId!=templates[activeIndex].StepId || process.Status!=(terminal?(rejected?ProcessStatus.FAILED:ProcessStatus.COMPLETED):ProcessStatus.IN_PROGRESS) ||
            process.CurrentState!=(proofs.Count==0?"INITIAL":terminal?(rejected?"REVIEW_REJECTED":"REVIEW_APPROVED"):templates[activeIndex].StepId) ||
            process.CompletionDate!=(terminal?last!.At:null) || process.CompletedBy!=(terminal?last!.Command.Scope().ActorUserId:null))
            throw new InvalidOperationException("The completion review workflow outcome differs from its complete ordered stage decisions.");
        var expectedTasks=proofs.SelectMany(proof=>proof.Closed).Select(task=>task.CROSS_TASK_ID).ToList();
        if(!terminal)
        {
            var stage=process.StepInstances.Single(item=>item.StepId==process.CurrentStepId);var active=tasks.Where(task=>task.PROCESS_STEP_INSTANCE_ID==stage.StepInstanceId).ToArray();
            var personas=original.Workflow.Stages[activeIndex].Definition.PersonaCodes;var creator=last?.Command.Scope().ActorUserId??owner.ActorUserId;
            if(active.Length!=personas.Count || !active.Select(task=>task.TARGET_PERSONA_CODE).Order(StringComparer.Ordinal).SequenceEqual(personas.Order(StringComparer.Ordinal)))
                throw new InvalidOperationException("The complete current completion review receiving set changed.");
            foreach(var task in active)
            {
                var identity="task-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Process=id,Step=stage.StepInstanceId,Persona=task.TARGET_PERSONA_CODE}));
                var context=JsonSerializer.Serialize(new{assignedRole=stage.RequiredRole,entityType="DRILLING_DAILY_REPORT",entityId=process.EntityId,processInstanceId=id,stepInstanceId=stage.StepInstanceId,scopeKind="FIELD"});
                RequireCompletionAudit(task,creator,creator,stage.StartDate??process.StartDate,stage.StartDate??process.StartDate);
                if(task.CROSS_TASK_ID!=identity || task.ACTIVE_IND!="Y" || task.TASK_STATUS!="PENDING" || task.TASK_TYPE!="APPROVAL" || task.ASSIGNED_ROLE!=stage.RequiredRole || task.ENTITY_TYPE!="DRILLING_DAILY_REPORT" || task.ENTITY_ID!=process.EntityId ||
                    task.PRIORITY is <1 or >4 || task.ROUTE!=WorkflowTaskRoute.For("FIELD",id,"DRILLING_DAILY_REPORT") || task.TASK_CONTEXT_JSON!=context || task.ROW_CREATED_BY!=creator || task.ROW_CREATED_DATE is null ||
                    task.ROW_CHANGED_BY!=creator || task.ROW_CHANGED_DATE is null || task.ROW_CHANGED_DATE<task.ROW_CREATED_DATE)throw new InvalidOperationException("The original pending completion review task changed.");
            }
            if(last is not null && (last.NextStep!=stage.StepInstanceId || !last.Next.OrderBy(task=>task.CROSS_TASK_ID).Select(item=>JsonSerializer.Serialize(item)).SequenceEqual(active.OrderBy(task=>task.CROSS_TASK_ID).Select(item=>JsonSerializer.Serialize(item)))))
                throw new InvalidOperationException("The recorded next completion review assignment changed.");
            expectedTasks.AddRange(active.Select(task=>task.CROSS_TASK_ID));
        }
        else if(last is not null && (last.Next.Count!=0 || last.NextStep is not null))throw new InvalidOperationException("A completed completion review decision cannot route another stage.");
        if(expectedTasks.Distinct(StringComparer.Ordinal).Count()!=expectedTasks.Count || tasks.Length!=expectedTasks.Count || tasks.Any(task=>!expectedTasks.Contains(task.CROSS_TASK_ID,StringComparer.Ordinal)))
            throw new InvalidOperationException("The complete completion review task population changed.");
        for(int index=0;index<proofs.Count;index++)
        {
            var proof=proofs[index];var expectedNext=proof.Command.Terminal()?null:DrillingCompletionReviewStartReceipt.StepId(id,original.Workflow.Stages[index+1].Definition.StepId);
            if(proof.NextStep!=expectedNext || proof.Next.Count!=(expectedNext is null?0:original.Workflow.Stages[index+1].Definition.PersonaCodes.Count))throw new InvalidOperationException("The recorded completion decision destination changed.");
            foreach(var pending in proof.Next)
            {
                var actual=tasks.SingleOrDefault(task=>task.CROSS_TASK_ID==pending.CROSS_TASK_ID)??throw new InvalidOperationException("The completion decision destination task is missing.");
                var restored=CompanyInventorySourceReview.Copy(actual);restored.TASK_STATUS=pending.TASK_STATUS;restored.ROW_CHANGED_BY=pending.ROW_CHANGED_BY;restored.ROW_CHANGED_DATE=pending.ROW_CHANGED_DATE;
                if(pending.TASK_STATUS!="PENDING" || pending.PROCESS_STEP_INSTANCE_ID!=expectedNext || JsonSerializer.Serialize(restored)!=JsonSerializer.Serialize(pending))throw new InvalidOperationException("The original completion destination task changed after routing.");
            }
        }
        return JsonSerializer.Serialize(new{Process=row,Steps=steps.Cast<PROCESS_STEP_INSTANCE>().OrderBy(item=>item.PROCESS_STEP_INSTANCE_ID,StringComparer.Ordinal),History=history.Cast<PROCESS_HISTORY>().OrderBy(item=>item.PROCESS_HISTORY_ID,StringComparer.Ordinal),Approvals=approvals.Cast<PROCESS_APPROVAL>().OrderBy(item=>item.PROCESS_APPROVAL_ID,StringComparer.Ordinal),Tasks=tasks.OrderBy(item=>item.CROSS_TASK_ID,StringComparer.Ordinal)});
    }
    /// <summary>Owns one native decision with canonical approval/history/tasks. Recovery never writes.</summary>
    public async Task<DrillingCompletionReviewDecisionReceipt> RecordCompletionReviewDecisionAsync(DrillingCompletionReviewDecision input,Func<Task> authority,Func<Task<DrillingDailyReportPage>> readSource,bool recover=false,Func<DrillingCompletionAcceptance,Task>? confirmAcceptance=null)
    {
        input.Require();var command=CompanyInventorySourceReview.Copy(input);var review=command.Review;var original=review.Original;var scope=command.Scope();var id=review.InstanceId;
        if(scope.LifecycleDatabase.ConnectionName!=_connectionName)throw new UnauthorizedAccessException("The original completion review lifecycle target is required.");
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("The original completion review decision source is unavailable.");
        async Task Guard(bool owned)
        {
            if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("The original completion review decision target changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);await authority();
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
        }
        async Task ConfirmTests()
        {
            if(recover || command.Acceptance is null)return;
            if(confirmAcceptance is null)throw new InvalidOperationException("The canonical native test source reader is required before physical completion acceptance.");
            await Guard(false);await confirmAcceptance(command.Acceptance);await Guard(false);
        }
        await Guard(false);var before=await readSource();original.RequireSource(before,false,false);await ConfirmTests();
        if(!recover && command.Decision=="APPROVE" && before.History.ReportedCompletion?.ReportId!=original.ClaimId)throw new InvalidOperationException("The completion claim is no longer current; reload the original review before deciding.");
        if(before.History.Execution.Basis.Scope().ActorUserId!=scope.ActorUserId || !recover && before.Revision!=review.Source.Revision)throw new InvalidOperationException("The displayed original completion review source or current reader changed.");
        CompletionDecisionProof? recorded=null;string? graph=null;ProcessInstance? process=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);process=await LoadProcessInstanceAsync(id)??throw new InvalidOperationException("The original completion review assignment is unavailable.");
            var payload=JsonSerializer.Serialize(process.ProcessData);if(DrillingCompletionReviewStart.OwnerCompany(payload)!=scope.Company)throw new UnauthorizedAccessException("The completion review belongs to another original company.");
            if(DrillingCompletionReviewStart.FromPayload(payload,id,scope.FieldId,original.ClaimId).Revision()!=original.Revision())throw new InvalidOperationException("The original completion review assignment changed.");
            if((await FindCompletionReviewAsync(scope.Company,scope.FieldId,original.ClaimId,new CrossPersonaTaskRouter(_editor,_commonColumnHandler,_defaults,_metadata,()=>Task.FromResult(_connectionName))))?.PROCESS_INSTANCE_ID!=id)throw new InvalidOperationException("The original completion review source population changed.");
            graph=await ConfirmCompletionReviewGraphAsync(process,original);var proofs=CompletionProofs(process);recorded=proofs.SingleOrDefault(proof=>proof.Command.IntentId==command.IntentId);
            if(recorded is not null){if(recorded.Command.Revision()!=command.Revision())throw new InvalidOperationException("The recorded completion review decision intent changed.");await Guard(true);return;}
            if(proofs.Any(proof=>proof.Command.Review.StepInstanceId==review.StepInstanceId))throw new InvalidOperationException("Another decision already owns this completion review stage.");
            RequireCompletionStage(command,original,proofs.Count);
            if((await GetProcessApprovalRepository().GetAsync(Key("PROCESS_APPROVAL_ID",command.AuditId("approval")))).Any() ||
                (await GetProcessHistoryRepository().GetAsync(Key("PROCESS_HISTORY_ID",command.AuditId("decision")))).Any())
                throw new InvalidOperationException("A partial or retagged completion decision key cannot become a new decision or confirmed absence.");
            if(process.Status!=ProcessStatus.IN_PROGRESS || process.CurrentStepId!=review.Stage().Definition.StepId || ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(process))!=review.ProcessRevision || ProcessDefinitionRevision.FromJson(graph)!=review.NativeRevision)
                throw new InvalidOperationException("The displayed pending completion review stage or complete native revision changed.");
            var current=process.StepInstances.Single(step=>step.StepInstanceId==review.StepInstanceId);var router=new CrossPersonaTaskRouter(_editor,_commonColumnHandler,_defaults,_metadata,()=>Task.FromResult(_connectionName),personas:new CompletionReviewPersonas(original,()=>Guard(true)));
            var tasks=await router.ReadTaskSetInOwnedTransactionAsync(id,current.StepInstanceId,current.RequiredRole!,"DRILLING_DAILY_REPORT",process.EntityId,proofs.LastOrDefault()?.Command.Scope().ActorUserId??process.StartedBy,original.Description);
            if(tasks.Count(task=>task.CROSS_TASK_ID==review.TaskId && task.TARGET_PERSONA_CODE==review.PersonaCode && ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(task))==review.TaskRevision)!=1)throw new InvalidOperationException("The exact displayed completion review task changed.");
            if(recover){await Guard(true);return;}
            var definition=await GetProcessDefinitionAsync(original.Workflow.ProcessId)??throw new InvalidOperationException("The original configured completion review workflow is unavailable.");DevelopmentReviewDefinition.RequireCompletion(definition);
            if(ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition))!=original.Workflow.DefinitionRevision)throw new InvalidOperationException("The configured completion review definition changed before this decision.");
            var applied=await ApplyOwnedReviewStageAsync(process,current,CompletionTemplates(original),router,scope.ActorUserId,command.Decision,command.Rationale,command.IntentId,"completion-stage",command.Acceptance is null?"COMPLETION_REVIEW":"COMPLETION_ACCEPTANCE",()=>Guard(true),original.Description,CompletionAcceptanceAudit(command));
            process=applied.Process;recorded=new(command,applied.At,CompletionBusinessRevision(process),applied.Approval,applied.History,tasks,applied.Closed,applied.Routed,applied.NextStep);proofs.Add(recorded);process.ProcessData.Data[CompletionDecisionsKey]=proofs;
            var row=applied.Row;row.INSTANCE_DATA_JSON=JsonSerializer.Serialize(process.ProcessData);if(row.INSTANCE_DATA_JSON.Length>DrillingCompletionReviewStart.MaximumBodyLength+65536)throw new InvalidOperationException("The complete completion review decision audit exceeds its supported payload bound.");
            await GetProcessInstanceRepository().UpdateAsync(row,scope.ActorUserId);await Guard(true);process=await LoadProcessInstanceAsync(id)??throw new InvalidOperationException("The recorded completion review decision is unavailable.");graph=await ConfirmCompletionReviewGraphAsync(process,original);await Guard(true);
        });
        await Guard(false);var closing=await LoadProcessInstanceAsync(id)??throw new InvalidOperationException("The original completion review decision commit is unconfirmed.");
        if(graph!=await ConfirmCompletionReviewGraphAsync(closing,original))throw new InvalidOperationException("The completion review decision commit or whole original graph is unconfirmed. Recover the retained original.");
        var after=await readSource();original.RequireSource(after,false,false);if(after.History.Execution.Basis.Scope().ActorUserId!=scope.ActorUserId || !recover && before.Revision!=after.Revision)throw new InvalidOperationException("The original saved completion review source changed during acknowledgement.");await ConfirmTests();await Guard(false);
        closing=await LoadProcessInstanceAsync(id)??throw new InvalidOperationException("The completion decision disappeared around closing source proof.");
        if(graph!=await ConfirmCompletionReviewGraphAsync(closing,original) || (await FindCompletionReviewAsync(scope.Company,scope.FieldId,original.ClaimId,new CrossPersonaTaskRouter(_editor,_commonColumnHandler,_defaults,_metadata,()=>Task.FromResult(_connectionName))))?.PROCESS_INSTANCE_ID!=id)
            throw new InvalidOperationException("The original completion decision graph or source population changed after closing source proof.");
        await Guard(false);
        var receipt=new DrillingCompletionReviewDecisionReceipt(command.Revision(),id,recorded is not null,recorded?.Approval.PROCESS_APPROVAL_ID,recorded?.History.PROCESS_HISTORY_ID,recorded?.NextStep,closing.CurrentState,ProcessDefinitionRevision.FromJson(graph!),DateTimeOffset.UtcNow){CompletionAccepted=recorded?.Command.Acceptance is not null};receipt.Require(command);return receipt;
    }
}
