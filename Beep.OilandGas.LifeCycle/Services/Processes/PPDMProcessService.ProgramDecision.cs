using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
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
    private const string ProgramDecisionsKey="DrillingProgramDecisions";
    private sealed record ProgramDecisionProof(DrillingProgramReviewDecision Command,DateTime At,string AfterBusinessRevision,
        PROCESS_APPROVAL Approval,PROCESS_HISTORY History,List<CROSS_PERSONA_TASK> Closed,List<CROSS_PERSONA_TASK> Next,string? NextStep);
    private static List<ProgramDecisionProof> ProgramProofs(ProcessInstance process)
    {
        if(!process.ProcessData.Data.TryGetValue(ProgramDecisionsKey,out var value))return [];
        var result=JsonSerializer.Deserialize<List<ProgramDecisionProof>>(JsonSerializer.Serialize(value))??throw new InvalidOperationException("The original program decision sequence is unavailable.");
        if(result.Count is <1 or >64 || result.Any(proof=>proof is null))throw new InvalidOperationException("The complete program decision sequence is invalid.");return result;
    }
    private static string ProgramBusinessRevision(ProcessInstance process)
    {
        var copy=CompanyInventorySourceReview.Copy(process);copy.ProcessData.Data.Remove(ProgramDecisionsKey);return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));
    }
    private static ProcessStepDefinition[] ProgramTemplates(DrillingProgramReviewStart original)=>original.Workflow.Stages.Select(stage=>new ProcessStepDefinition{
        StepId=stage.Definition.StepId,StepName=stage.Definition.StepName,SequenceNumber=stage.Definition.Sequence,StepType="APPROVAL",RequiresApproval=true,RequiredRoles=[stage.Definition.AssignedRole],SlaHours=stage.Definition.SlaHours}).ToArray();
    private static void RequireProgramStage(DrillingProgramReviewDecision command,DrillingProgramReviewStart original,int index)
    {
        command.Require();if(command.Review.Original.Revision()!=original.Revision() || command.Review.StageNumber!=index+1)throw new UnauthorizedAccessException("The independent decision belongs to another original program or ordered stage.");
    }
    private async Task<string> ConfirmProgramReviewGraphAsync(ProcessInstance process,DrillingProgramReviewStart original)
    {
        var id=original.InstanceId();var owner=original.Scope();var proofs=ProgramProofs(process);var templates=ProgramTemplates(original);
        var processRows=(await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).ToArray();
        var steps=(await GetProcessStepInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).ToArray();
        var history=(await GetProcessHistoryRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).ToArray();
        var approvals=(await GetProcessApprovalRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).ToArray();
        var tasks=(await new CrossPersonaTaskRouter(_editor,_commonColumnHandler,_defaults,_metadata,()=>Task.FromResult(_connectionName)).GetAllTasksForDevelopmentEvidenceAsync(id,_connectionName)).ToArray();
        if(processRows.Length!=1 || processRows[0] is not PROCESS_INSTANCE row || row.ACTIVE_IND!="Y" || row.ROW_CREATED_BY!=owner.ActorUserId ||
            process.InstanceId!=id || process.EntityType!="DRILLING_PROGRAM" || process.EntityId!=original.Source.Program.PROGRAM_ID || process.FieldId!=owner.FieldId || process.ProcessId!=original.Workflow.ProcessId ||
            process.StartedBy!=owner.ActorUserId || DrillingProgramReviewStart.FromPayload(JsonSerializer.Serialize(process.ProcessData),id,owner.FieldId,process.EntityId).Revision()!=original.Revision() ||
            process.StepInstances.Count!=templates.Length || steps.Length!=templates.Length || steps.Any(item=>item is not PROCESS_STEP_INSTANCE) || history.Any(item=>item is not PROCESS_HISTORY) || approvals.Any(item=>item is not PROCESS_APPROVAL) ||
            process.ProcessData.Data.Keys.Any(key=>key is not (DrillingProgramReviewStart.PayloadKey or ProgramDecisionsKey)))
            throw new InvalidOperationException("The complete original program process ownership or graph is ambiguous.");
        if(proofs.Count==0)await ConfirmProgramReviewInitialAsync(process,original,new CrossPersonaTaskRouter(_editor,_commonColumnHandler,_defaults,_metadata,()=>Task.FromResult(_connectionName)));
        else if(proofs.Count>templates.Length || proofs.Select(proof=>proof.Command.IntentId).Distinct().Count()!=proofs.Count ||
            ProgramBusinessRevision(process)!=proofs[^1].AfterBusinessRevision || approvals.Length!=proofs.Count || history.Length!=proofs.Count+1 || process.History.Count!=proofs.Count+1 ||
            !process.History.Any(item=>item.HistoryId==ProgramReviewHistoryId(id) && item.Action=="PROCESS_STARTED" && item.PerformedBy==owner.ActorUserId && item.Timestamp==process.StartDate))
            throw new InvalidOperationException("The recorded program decisions or complete audit graph changed.");
        static void RequireRow<T>(IEnumerable<object> rows,T expected,Func<T,bool> key) where T:ModelEntityBase
        {
            var found=rows.OfType<T>().Where(key).ToArray();
            if(found.Length!=1 || expected.ACTIVE_IND!="Y" || JsonSerializer.Serialize(found[0])!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("A canonical program decision audit row changed.");
        }
        for(var index=0;index<proofs.Count;index++)
        {
            var proof=proofs[index];var command=proof.Command;RequireProgramStage(command,original,index);var actor=command.Scope().ActorUserId;
            var digest=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Instance=id,command.IntentId}));
            if(index<proofs.Count-1 && command.Decision!="APPROVE" || proof.At.Kind==DateTimeKind.Local ||
                proof.At<process.StartDate || index>0 && proof.At<proofs[index-1].At ||
                proof.Approval.PROCESS_APPROVAL_ID!="program-stage-approval-"+digest || proof.Approval.PROCESS_INSTANCE_ID!=id || proof.Approval.PROCESS_STEP_INSTANCE_ID!=command.Review.StepInstanceId ||
                proof.Approval.APPROVAL_TYPE!="PROGRAM_REVIEW" || proof.Approval.APPROVAL_STATUS!=(command.Decision=="REJECT"?"REJECTED":"APPROVED") || proof.Approval.REQUIRED_ACTION!=command.Decision ||
                proof.Approval.APPROVED_BY!=actor || proof.Approval.APPROVER_USER_ID!=actor || proof.Approval.APPROVAL_DATE!=proof.At || proof.Approval.APPROVAL_NOTES!=command.Rationale ||
                proof.Approval.REQUESTED_BY!=owner.ActorUserId || proof.Approval.REQUESTED_DATE!=process.StartDate || proof.Approval.APPROVER_SEQUENCE!=templates[index].SequenceNumber ||
                proof.History.PROCESS_HISTORY_ID!="program-stage-decision-"+digest || proof.History.PROCESS_INSTANCE_ID!=id || proof.History.PROCESS_STEP_INSTANCE_ID!=command.Review.StepInstanceId ||
                proof.History.USER_ID!=actor || proof.History.EVENT_DATE!=proof.At || proof.History.DETAILS!=command.Rationale || proof.History.EVENT_TYPE!=(command.Decision=="REJECT"?"PROGRAM_REVIEW_REJECTED":"PROGRAM_REVIEW_APPROVED"))throw new InvalidOperationException("A program decision differs from its exact retained stage, independent actor or audit.");
            RequireRow(approvals,proof.Approval,item=>item.PROCESS_APPROVAL_ID==proof.Approval.PROCESS_APPROVAL_ID);
            RequireRow(history,proof.History,item=>item.PROCESS_HISTORY_ID==proof.History.PROCESS_HISTORY_ID);
            if(proof.Closed.Count!=original.Workflow.Stages[index].Definition.PersonaCodes.Count ||
                !proof.Closed.Select(task=>task.TARGET_PERSONA_CODE).Order(StringComparer.Ordinal).SequenceEqual(original.Workflow.Stages[index].Definition.PersonaCodes.Order(StringComparer.Ordinal)))
                throw new InvalidOperationException("The complete closed program persona task set changed.");
            foreach(var task in proof.Closed)
                if(task.PROCESS_INSTANCE_ID!=id || task.PROCESS_STEP_INSTANCE_ID!=command.Review.StepInstanceId || task.TASK_STATUS!="COMPLETED" || task.ROW_CHANGED_BY!=actor || task.ROW_CHANGED_DATE is null ||
                    tasks.Count(item=>item.CROSS_TASK_ID==task.CROSS_TASK_ID && JsonSerializer.Serialize(item)==JsonSerializer.Serialize(task))!=1)
                    throw new InvalidOperationException("A closed program task differs from its recorded stage proof.");
        }
        var last=proofs.LastOrDefault();var rejected=last?.Command.Decision=="REJECT";var terminal=rejected || proofs.Count==templates.Length;
        var activeIndex=proofs.Count==0?0:terminal?proofs.Count-1:proofs.Count;
        for(var index=0;index<templates.Length;index++)
        {
            var expected=templates[index];var stage=process.StepInstances.Single(item=>item.StepId==expected.StepId);var native=steps.Cast<PROCESS_STEP_INSTANCE>().Single(item=>item.STEP_ID==expected.StepId);
            var decided=index<proofs.Count;var proof=decided?proofs[index]:null;
            var status=decided?(proof!.Command.Decision=="REJECT"?StepStatus.FAILED:StepStatus.COMPLETED):index==activeIndex && !terminal?StepStatus.PENDING:StepStatus.BLOCKED;
            if(stage.StepInstanceId!=DrillingProgramReviewStartReceipt.StepId(id,expected.StepId) || stage.InstanceId!=id || stage.StepName!=expected.StepName || stage.SequenceNumber!=expected.SequenceNumber ||
                stage.RequiredRole!=expected.RequiredRoles[0] || !stage.ApprovalRequired || stage.SlaHours!=expected.SlaHours || stage.Status!=status || !string.IsNullOrEmpty(stage.AssignedTo) ||
                stage.CompletionDate!=(decided?proof!.At:null) || stage.CompletedBy!=(decided?proof!.Command.Scope().ActorUserId:null) ||
                stage.Outcome!=(decided?(proof!.Command.Decision=="REJECT"?"REJECTED":"APPROVED"):"") || stage.Notes!=(decided?proof!.Command.Rationale:"") || native.ACTIVE_IND!="Y" || native.ROW_CREATED_BY!=owner.ActorUserId || native.ROW_CREATED_DATE is null)
                throw new InvalidOperationException("The ordered native program stages differ from their recorded decisions.");
        }
        if(process.CurrentStepId!=templates[activeIndex].StepId || process.Status!=(terminal?(rejected?ProcessStatus.FAILED:ProcessStatus.COMPLETED):ProcessStatus.IN_PROGRESS) ||
            process.CurrentState!=(proofs.Count==0?"INITIAL":terminal?(rejected?"REVIEW_REJECTED":"REVIEW_APPROVED"):templates[activeIndex].StepId) ||
            process.CompletionDate!=(terminal?last!.At:null) || process.CompletedBy!=(terminal?last!.Command.Scope().ActorUserId:null))
            throw new InvalidOperationException("The program workflow outcome differs from its complete ordered stage decisions.");
        var expectedTasks=proofs.SelectMany(proof=>proof.Closed).Select(task=>task.CROSS_TASK_ID).ToList();
        if(!terminal)
        {
            var stage=process.StepInstances.Single(item=>item.StepId==process.CurrentStepId);var active=tasks.Where(task=>task.PROCESS_STEP_INSTANCE_ID==stage.StepInstanceId).ToArray();
            var personas=original.Workflow.Stages[activeIndex].Definition.PersonaCodes;var creator=last?.Command.Scope().ActorUserId??owner.ActorUserId;
            if(active.Length!=personas.Count || !active.Select(task=>task.TARGET_PERSONA_CODE).Order(StringComparer.Ordinal).SequenceEqual(personas.Order(StringComparer.Ordinal)))
                throw new InvalidOperationException("The complete current program receiving set changed.");
            foreach(var task in active)
            {
                var identity="task-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Process=id,Step=stage.StepInstanceId,Persona=task.TARGET_PERSONA_CODE}));
                var context=JsonSerializer.Serialize(new{assignedRole=stage.RequiredRole,entityType="DRILLING_PROGRAM",entityId=process.EntityId,processInstanceId=id,stepInstanceId=stage.StepInstanceId,scopeKind="FIELD"});
                if(task.CROSS_TASK_ID!=identity || task.ACTIVE_IND!="Y" || task.TASK_STATUS!="PENDING" || task.TASK_TYPE!="APPROVAL" || task.ASSIGNED_ROLE!=stage.RequiredRole || task.ENTITY_TYPE!="DRILLING_PROGRAM" || task.ENTITY_ID!=process.EntityId ||
                    task.PRIORITY is <1 or >4 || task.ROUTE!=WorkflowTaskRoute.For("FIELD",id,"DRILLING_PROGRAM") || task.TASK_CONTEXT_JSON!=context || task.ROW_CREATED_BY!=creator || task.ROW_CREATED_DATE is null ||
                    task.ROW_CHANGED_DATE is not null || !string.IsNullOrEmpty(task.ROW_CHANGED_BY))throw new InvalidOperationException("The original pending program task changed.");
            }
            if(last is not null && (last.NextStep!=stage.StepInstanceId || !last.Next.OrderBy(task=>task.CROSS_TASK_ID).Select(item=>JsonSerializer.Serialize(item)).SequenceEqual(active.OrderBy(task=>task.CROSS_TASK_ID).Select(item=>JsonSerializer.Serialize(item)))))
                throw new InvalidOperationException("The recorded next program assignment changed.");
            expectedTasks.AddRange(active.Select(task=>task.CROSS_TASK_ID));
        }
        else if(last is not null && (last.Next.Count!=0 || last.NextStep is not null))throw new InvalidOperationException("A completed program decision cannot route another stage.");
        if(expectedTasks.Distinct(StringComparer.Ordinal).Count()!=expectedTasks.Count || tasks.Length!=expectedTasks.Count || tasks.Any(task=>!expectedTasks.Contains(task.CROSS_TASK_ID,StringComparer.Ordinal)))
            throw new InvalidOperationException("The complete program task population changed.");
        return JsonSerializer.Serialize(new{Process=row,Steps=steps.Cast<PROCESS_STEP_INSTANCE>().OrderBy(item=>item.PROCESS_STEP_INSTANCE_ID,StringComparer.Ordinal),History=history.Cast<PROCESS_HISTORY>().OrderBy(item=>item.PROCESS_HISTORY_ID,StringComparer.Ordinal),Approvals=approvals.Cast<PROCESS_APPROVAL>().OrderBy(item=>item.PROCESS_APPROVAL_ID,StringComparer.Ordinal),Tasks=tasks.OrderBy(item=>item.CROSS_TASK_ID,StringComparer.Ordinal)});
    }
    /// <summary>Owns one native decision with canonical approval/history/tasks. Recovery never writes.</summary>
    public async Task<DrillingProgramReviewDecisionReceipt> RecordProgramReviewDecisionAsync(DrillingProgramReviewDecision input,Func<Task> authority,Func<Task<DrillingProgramReviewSource>> readSource,bool recover=false)
    {
        input.Require();var command=CompanyInventorySourceReview.Copy(input);var review=command.Review;var original=review.Original;var scope=command.Scope();var id=review.InstanceId;
        if(scope.LifecycleDatabase.ConnectionName!=_connectionName)throw new UnauthorizedAccessException("The original program lifecycle target is required.");
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("The original program decision source is unavailable.");
        async Task Guard(bool owned)
        {
            if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("The original program decision target changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);await authority();
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
        }
        await Guard(false);var before=await readSource();original.RequireSource(before,false,false);
        if(before.Basis.Scope().ActorUserId!=scope.ActorUserId || !recover && before.Revision!=review.Source.Revision)throw new InvalidOperationException("The displayed original program source or current reader changed.");
        ProgramDecisionProof? recorded=null;string? graph=null;ProcessInstance? process=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);process=await LoadProcessInstanceAsync(id)??throw new InvalidOperationException("The original program assignment is unavailable.");
            var payload=JsonSerializer.Serialize(process.ProcessData);if(DrillingProgramReviewStart.OwnerCompany(payload)!=scope.Company)throw new UnauthorizedAccessException("The program belongs to another original company.");
            if(DrillingProgramReviewStart.FromPayload(payload,id,scope.FieldId,original.Source.Program.PROGRAM_ID).Revision()!=original.Revision())throw new InvalidOperationException("The original program assignment changed.");
            graph=await ConfirmProgramReviewGraphAsync(process,original);var proofs=ProgramProofs(process);recorded=proofs.SingleOrDefault(proof=>proof.Command.IntentId==command.IntentId);
            if(recorded is not null){if(recorded.Command.Revision()!=command.Revision())throw new InvalidOperationException("The recorded program decision intent changed.");await Guard(true);return;}
            if(proofs.Any(proof=>proof.Command.Review.StepInstanceId==review.StepInstanceId))throw new InvalidOperationException("Another decision already owns this program stage.");
            RequireProgramStage(command,original,proofs.Count);
            if(process.Status!=ProcessStatus.IN_PROGRESS || process.CurrentStepId!=review.Stage().Definition.StepId || ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(process))!=review.ProcessRevision || ProcessDefinitionRevision.FromJson(graph)!=review.NativeRevision)
                throw new InvalidOperationException("The displayed pending program stage or complete native revision changed.");
            var current=process.StepInstances.Single(step=>step.StepInstanceId==review.StepInstanceId);var router=new CrossPersonaTaskRouter(_editor,_commonColumnHandler,_defaults,_metadata,()=>Task.FromResult(_connectionName),personas:new ProgramReviewPersonas(original,()=>Guard(true)));
            var tasks=await router.ReadTaskSetInOwnedTransactionAsync(id,current.StepInstanceId,current.RequiredRole!,"DRILLING_PROGRAM",process.EntityId,proofs.LastOrDefault()?.Command.Scope().ActorUserId??process.StartedBy,original.Source.Program.PROGRAM_NAME);
            if(tasks.Count(task=>task.CROSS_TASK_ID==review.TaskId && task.TARGET_PERSONA_CODE==review.PersonaCode && ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(task))==review.TaskRevision)!=1)throw new InvalidOperationException("The exact displayed program task changed.");
            if(recover){await Guard(true);return;}
            var definition=await GetProcessDefinitionAsync(original.Workflow.ProcessId)??throw new InvalidOperationException("The original configured program workflow is unavailable.");DevelopmentReviewDefinition.RequireProgram(definition);
            if(ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition))!=original.Workflow.DefinitionRevision)throw new InvalidOperationException("The configured program definition changed before this decision.");
            var applied=await ApplyOwnedReviewStageAsync(process,current,ProgramTemplates(original),router,scope.ActorUserId,command.Decision,command.Rationale,command.IntentId,"program-stage","PROGRAM_REVIEW",()=>Guard(true),original.Source.Program.PROGRAM_NAME);
            process=applied.Process;recorded=new(command,applied.At,ProgramBusinessRevision(process),applied.Approval,applied.History,applied.Closed,applied.Routed,applied.NextStep);proofs.Add(recorded);process.ProcessData.Data[ProgramDecisionsKey]=proofs;
            var row=applied.Row;row.INSTANCE_DATA_JSON=JsonSerializer.Serialize(process.ProcessData);if(row.INSTANCE_DATA_JSON.Length>DrillingProgramReviewStart.MaximumBodyLength+65536)throw new InvalidOperationException("The complete program decision audit exceeds its supported payload bound.");
            await GetProcessInstanceRepository().UpdateAsync(row,scope.ActorUserId);await Guard(true);process=await LoadProcessInstanceAsync(id)??throw new InvalidOperationException("The recorded program decision is unavailable.");graph=await ConfirmProgramReviewGraphAsync(process,original);await Guard(true);
        });
        await Guard(false);var closing=await LoadProcessInstanceAsync(id)??throw new InvalidOperationException("The original program decision commit is unconfirmed.");
        if(graph!=await ConfirmProgramReviewGraphAsync(closing,original))throw new InvalidOperationException("The program decision commit or whole original graph is unconfirmed. Recover the retained original.");
        var after=await readSource();original.RequireSource(after,false,false);if(after.Basis.Scope().ActorUserId!=scope.ActorUserId || !recover && before.Revision!=after.Revision)throw new InvalidOperationException("The original saved program source changed during acknowledgement.");await Guard(false);
        var receipt=new DrillingProgramReviewDecisionReceipt(command.Revision(),id,recorded is not null,recorded?.Approval.PROCESS_APPROVAL_ID,recorded?.History.PROCESS_HISTORY_ID,recorded?.NextStep,closing.CurrentState,ProcessDefinitionRevision.FromJson(graph!),DateTimeOffset.UtcNow);receipt.Require(command);return receipt;
    }
}
