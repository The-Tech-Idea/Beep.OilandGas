using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.LifeCycle.Models;
using Beep.OilandGas.DevelopmentPlanning.Models;
using System.Data;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.Core;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

public partial class PPDMProcessService
{
    private const string InvestmentStageProofsKey="InvestmentStageDecisions";
    private const string InvestmentReservationKey="InvestmentStageReservation";
    private sealed record InvestmentReservation(DevelopmentInvestmentStageDecision Command,DateTime At);
    private sealed record InvestmentStageProof(DevelopmentInvestmentStageDecision Command,DateTime ReservedAt,DateTime At,string AfterBusinessRevision,
        PROCESS_APPROVAL Approval,PROCESS_HISTORY History,List<CROSS_PERSONA_TASK> Closed,List<CROSS_PERSONA_TASK> Next,string? NextStep,DevelopmentInvestmentFacts? Financial);
    private static List<InvestmentStageProof> InvestmentProofs(ProcessInstance process)
    {
        if(!process.ProcessData.Data.TryGetValue(InvestmentStageProofsKey,out var value))return [];
        var result=JsonSerializer.Deserialize<List<InvestmentStageProof>>(JsonSerializer.Serialize(value))??throw new InvalidOperationException("The recorded investment stage sequence is unavailable.");
        if(result.Count is <1 or >64 || result.Any(item=>item is null))throw new InvalidOperationException("The complete investment decision sequence is invalid.");return result;
    }
    private static InvestmentReservation? InvestmentPending(ProcessInstance process)
        =>process.ProcessData.Data.TryGetValue(InvestmentReservationKey,out var value)?JsonSerializer.Deserialize<InvestmentReservation>(JsonSerializer.Serialize(value))
            ??throw new InvalidOperationException("The original investment reservation is unavailable."):null;
    public static (string? Revision,string? Actor) InvestmentReservationStatus(ProcessInstance process)
    {var pending=InvestmentPending(process);return(pending?.Command.Revision(),pending?.Command.Scope().ActorUserId);}
    // Callers must validate the complete native graph and current authority before returning this candidate.
    public static DevelopmentInvestmentStageDecision? OwnInvestmentStageOriginal(ProcessInstance process,string actor)
    {
        var pending=InvestmentPending(process)?.Command;
        var original=pending?.Scope().ActorUserId==actor?pending:InvestmentProofs(process).LastOrDefault(proof=>proof.Command.Scope().ActorUserId==actor)?.Command;
        return original is null?null:CompanyInventorySourceReview.Copy(original);
    }
    private static string InvestmentBusinessRevision(ProcessInstance process,bool keepProofs=false)
    {
        var copy=CompanyInventorySourceReview.Copy(process);copy.ProcessData.Data.Remove(InvestmentReservationKey);
        if(!keepProofs)copy.ProcessData.Data.Remove(InvestmentStageProofsKey);
        return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));
    }
    private static ProcessStepDefinition[] InvestmentTemplates(DevelopmentInvestmentHandoff original)=>original.Workflow.Steps.Select(step=>new ProcessStepDefinition{
        StepId=step.StepId,StepName=step.StepName,SequenceNumber=step.Sequence,StepType="APPROVAL",RequiresApproval=true,RequiredRoles=[step.AssignedRole],SlaHours=step.SlaHours}).ToArray();
    private static void RequireInvestmentStage(DevelopmentInvestmentStageDecision command,DevelopmentInvestmentHandoff original,int index)
    {
        command.Require();var review=command.Review;var scope=command.Scope();var owner=original.Scope();
        if(index<0 || index>=original.Workflow.Steps.Count)throw new InvalidOperationException("The configured investment stage is unavailable.");
        var stage=original.Workflow.Steps[index];original.RequireSource(review.Source,false);
        if(review.InstanceId!=original.InstanceId() || review.StageNumber!=index+1 || review.StageCount!=original.Workflow.Steps.Count || review.StepId!=stage.StepId || review.StepName!=stage.StepName ||
            review.StepInstanceId!=InvestmentHandoffStepId(review.InstanceId,stage.StepId) || review.AssignedRole!=stage.AssignedRole || review.Sender!=owner.ActorUserId ||
            review.WorkflowName!=original.Workflow.ProcessName || review.WorkflowVersion!=original.Workflow.Version || review.DefinitionRevision!=original.Workflow.DefinitionRevision ||
            scope.ActorUserId==owner.ActorUserId || scope.ActorUserId==original.Review.Source.Afe().CreatedBy ||
            !original.Option().Stages[index].Recipients.Any(recipient=>recipient.UserId==scope.ActorUserId && recipient.PersonaCode==review.PersonaCode))
            throw new UnauthorizedAccessException("The retained investment decision belongs to another original independent stage, recipient or source.");
    }
    private async Task<string> ConfirmInvestmentGraph(ProcessInstance process,DevelopmentInvestmentHandoff original)
    {
        var id=original.InstanceId();var owner=original.Scope();var proofs=InvestmentProofs(process);var pending=InvestmentPending(process);var templates=InvestmentTemplates(original);
        var processRows=(await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).ToArray();
        var steps=(await GetProcessStepInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).ToArray();
        var history=(await GetProcessHistoryRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).ToArray();
        var approvals=(await GetProcessApprovalRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).ToArray();
        var tasks=(await new CrossPersonaTaskRouter(_editor,_commonColumnHandler,_defaults,_metadata,()=>Task.FromResult(_connectionName)).GetAllTasksForDevelopmentEvidenceAsync(id,_connectionName)).ToArray();
        if(processRows.Length!=1 || processRows[0] is not PROCESS_INSTANCE row || row.ACTIVE_IND!="Y" || row.ROW_CREATED_BY!=owner.ActorUserId ||
            process.InstanceId!=id || process.EntityType!="AFE" || process.EntityId!=original.Review.Source.AfeId || process.FieldId!=owner.FieldId || process.ProcessId!=original.Workflow.ProcessId ||
            process.StartedBy!=owner.ActorUserId || DevelopmentInvestmentHandoff.FromPayload(JsonSerializer.Serialize(process.ProcessData),id,owner.FieldId,process.EntityId).Revision()!=original.Revision() ||
            process.StepInstances.Count!=templates.Length || steps.Length!=templates.Length || steps.Any(item=>item is not PROCESS_STEP_INSTANCE) || history.Any(item=>item is not PROCESS_HISTORY) || approvals.Any(item=>item is not PROCESS_APPROVAL) ||
            process.ProcessData.Data.Keys.Any(key=>key is not (DevelopmentInvestmentHandoff.PayloadKey or InvestmentStageProofsKey or InvestmentReservationKey)))
            throw new InvalidOperationException("The complete original investment process ownership or graph is ambiguous.");
        if(proofs.Count==0)await ConfirmInvestmentInitialGraph(process,original);
        else if(proofs.Count>templates.Length || proofs.Select(proof=>proof.Command.IntentId).Distinct().Count()!=proofs.Count ||
            InvestmentBusinessRevision(process)!=proofs[^1].AfterBusinessRevision || approvals.Length!=proofs.Count || history.Length!=proofs.Count+1 || process.History.Count!=proofs.Count+1 ||
            !process.History.Any(item=>item.HistoryId==InvestmentHandoffHistoryId(id) && item.Action=="PROCESS_STARTED" && item.PerformedBy==owner.ActorUserId && item.Timestamp==process.StartDate))
            throw new InvalidOperationException("The recorded investment decisions or complete audit graph changed.");
        static void RequireRow<T>(IEnumerable<object> rows,T expected,Func<T,bool> key) where T:ModelEntityBase
        {
            var found=rows.OfType<T>().Where(key).ToArray();
            if(found.Length!=1 || expected.ACTIVE_IND!="Y" || JsonSerializer.Serialize(found[0])!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("A canonical investment decision audit row changed.");
        }
        for(var index=0;index<proofs.Count;index++)
        {
            var proof=proofs[index];var command=proof.Command;RequireInvestmentStage(command,original,index);var actor=command.Scope().ActorUserId;
            var digest=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Instance=id,command.IntentId}));
            if(index<proofs.Count-1 && command.Decision!="APPROVE" || proof.At.Kind==DateTimeKind.Local || proof.ReservedAt.Kind==DateTimeKind.Local ||
                proof.ReservedAt<process.StartDate || proof.At<proof.ReservedAt || index>0 && proof.ReservedAt<proofs[index-1].At ||
                proof.Approval.PROCESS_APPROVAL_ID!="investment-stage-approval-"+digest || proof.Approval.PROCESS_INSTANCE_ID!=id || proof.Approval.PROCESS_STEP_INSTANCE_ID!=command.Review.StepInstanceId ||
                proof.Approval.APPROVAL_TYPE!="INVESTMENT_REVIEW" || proof.Approval.APPROVAL_STATUS!=(command.Decision=="REJECT"?"REJECTED":"APPROVED") || proof.Approval.REQUIRED_ACTION!=command.Decision ||
                proof.Approval.APPROVED_BY!=actor || proof.Approval.APPROVER_USER_ID!=actor || proof.Approval.APPROVAL_DATE!=proof.At || proof.Approval.APPROVAL_NOTES!=command.Rationale ||
                proof.Approval.REQUESTED_BY!=owner.ActorUserId || proof.Approval.REQUESTED_DATE!=process.StartDate || proof.Approval.APPROVER_SEQUENCE!=templates[index].SequenceNumber ||
                proof.History.PROCESS_HISTORY_ID!="investment-stage-decision-"+digest || proof.History.PROCESS_INSTANCE_ID!=id || proof.History.PROCESS_STEP_INSTANCE_ID!=command.Review.StepInstanceId ||
                proof.History.USER_ID!=actor || proof.History.EVENT_DATE!=proof.At || proof.History.DETAILS!=command.Rationale || proof.History.EVENT_TYPE!=(command.Decision=="REJECT"?"INVESTMENT_REVIEW_REJECTED":"INVESTMENT_REVIEW_APPROVED") ||
                command.Terminal()!=(proof.Financial is not null))throw new InvalidOperationException("An investment decision differs from its exact retained stage, independent actor or audit.");
            if(proof.Financial is not null){command.RequireFinancial(proof.Financial);if(proof.Financial.AtUtc.UtcDateTime<proof.ReservedAt || proof.Financial.AtUtc.UtcDateTime>proof.At)throw new InvalidOperationException("The final financial outcome does not follow its original lifecycle reservation.");}
            RequireRow(approvals,proof.Approval,item=>item.PROCESS_APPROVAL_ID==proof.Approval.PROCESS_APPROVAL_ID);
            RequireRow(history,proof.History,item=>item.PROCESS_HISTORY_ID==proof.History.PROCESS_HISTORY_ID);
            if(proof.Closed.Count!=original.Workflow.Steps[index].PersonaCodes.Count ||
                !proof.Closed.Select(task=>task.TARGET_PERSONA_CODE).Order(StringComparer.Ordinal).SequenceEqual(original.Workflow.Steps[index].PersonaCodes.Order(StringComparer.Ordinal)))
                throw new InvalidOperationException("The complete closed investment persona task set changed.");
            foreach(var task in proof.Closed)
                if(task.PROCESS_INSTANCE_ID!=id || task.PROCESS_STEP_INSTANCE_ID!=command.Review.StepInstanceId || task.TASK_STATUS!="COMPLETED" || task.ROW_CHANGED_BY!=actor || task.ROW_CHANGED_DATE is null ||
                    tasks.Count(item=>item.CROSS_TASK_ID==task.CROSS_TASK_ID && JsonSerializer.Serialize(item)==JsonSerializer.Serialize(task))!=1)
                    throw new InvalidOperationException("A closed investment task differs from its recorded stage proof.");
        }
        var last=proofs.LastOrDefault();var rejected=last?.Command.Decision=="REJECT";var terminal=rejected || proofs.Count==templates.Length;
        var activeIndex=proofs.Count==0?0:terminal?proofs.Count-1:proofs.Count;
        for(var index=0;index<templates.Length;index++)
        {
            var expected=templates[index];var stage=process.StepInstances.Single(item=>item.StepId==expected.StepId);var native=steps.Cast<PROCESS_STEP_INSTANCE>().Single(item=>item.STEP_ID==expected.StepId);
            var decided=index<proofs.Count;var proof=decided?proofs[index]:null;
            var status=decided?(proof!.Command.Decision=="REJECT"?StepStatus.FAILED:StepStatus.COMPLETED):index==activeIndex && !terminal?StepStatus.PENDING:StepStatus.BLOCKED;
            if(stage.StepInstanceId!=InvestmentHandoffStepId(id,expected.StepId) || stage.InstanceId!=id || stage.StepName!=expected.StepName || stage.SequenceNumber!=expected.SequenceNumber ||
                stage.RequiredRole!=expected.RequiredRoles[0] || !stage.ApprovalRequired || stage.SlaHours!=expected.SlaHours || stage.Status!=status || !string.IsNullOrEmpty(stage.AssignedTo) ||
                stage.CompletionDate!=(decided?proof!.At:null) || stage.CompletedBy!=(decided?proof!.Command.Scope().ActorUserId:null) ||
                stage.Outcome!=(decided?(proof!.Command.Decision=="REJECT"?"REJECTED":"APPROVED"):"") || stage.Notes!=(decided?proof!.Command.Rationale:"") || native.ACTIVE_IND!="Y" || native.ROW_CREATED_BY!=owner.ActorUserId || native.ROW_CREATED_DATE is null)
                throw new InvalidOperationException("The ordered native investment stages differ from their recorded decisions.");
        }
        if(process.CurrentStepId!=templates[activeIndex].StepId || process.Status!=(terminal?(rejected?ProcessStatus.FAILED:ProcessStatus.COMPLETED):ProcessStatus.IN_PROGRESS) ||
            process.CurrentState!=(proofs.Count==0?"INITIAL":terminal?(rejected?"REVIEW_REJECTED":"REVIEW_APPROVED"):templates[activeIndex].StepId) ||
            process.CompletionDate!=(terminal?last!.At:null) || process.CompletedBy!=(terminal?last!.Command.Scope().ActorUserId:null))
            throw new InvalidOperationException("The investment workflow outcome differs from its complete ordered stage decisions.");
        var expectedTasks=proofs.SelectMany(proof=>proof.Closed).Select(task=>task.CROSS_TASK_ID).ToList();
        if(!terminal)
        {
            var stage=process.StepInstances.Single(item=>item.StepId==process.CurrentStepId);var active=tasks.Where(task=>task.PROCESS_STEP_INSTANCE_ID==stage.StepInstanceId).ToArray();
            var personas=original.Workflow.Steps[activeIndex].PersonaCodes;var creator=last?.Command.Scope().ActorUserId??owner.ActorUserId;
            if(active.Length!=personas.Count || !active.Select(task=>task.TARGET_PERSONA_CODE).Order(StringComparer.Ordinal).SequenceEqual(personas.Order(StringComparer.Ordinal)))
                throw new InvalidOperationException("The complete current investment receiving set changed.");
            foreach(var task in active)
            {
                var identity="task-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Process=id,Step=stage.StepInstanceId,Persona=task.TARGET_PERSONA_CODE}));
                var context=JsonSerializer.Serialize(new{assignedRole=stage.RequiredRole,entityType="AFE",entityId=process.EntityId,processInstanceId=id,stepInstanceId=stage.StepInstanceId,scopeKind="FIELD"});
                if(task.CROSS_TASK_ID!=identity || task.ACTIVE_IND!="Y" || task.TASK_STATUS!="PENDING" || task.TASK_TYPE!="APPROVAL" || task.ASSIGNED_ROLE!=stage.RequiredRole || task.ENTITY_TYPE!="AFE" || task.ENTITY_ID!=process.EntityId ||
                    task.PRIORITY is <1 or >4 || task.ROUTE!=WorkflowTaskRoute.For("FIELD",id,"AFE") || task.TASK_CONTEXT_JSON!=context || task.ROW_CREATED_BY!=creator || task.ROW_CREATED_DATE is null ||
                    task.ROW_CHANGED_DATE is not null || !string.IsNullOrEmpty(task.ROW_CHANGED_BY))throw new InvalidOperationException("The original pending investment task changed.");
            }
            if(last is not null && (last.NextStep!=stage.StepInstanceId || !last.Next.OrderBy(task=>task.CROSS_TASK_ID).Select(item=>JsonSerializer.Serialize(item)).SequenceEqual(active.OrderBy(task=>task.CROSS_TASK_ID).Select(item=>JsonSerializer.Serialize(item)))))
                throw new InvalidOperationException("The recorded next investment assignment changed.");
            expectedTasks.AddRange(active.Select(task=>task.CROSS_TASK_ID));
        }
        else if(last is not null && (last.Next.Count!=0 || last.NextStep is not null))throw new InvalidOperationException("A completed investment decision cannot route another stage.");
        if(expectedTasks.Distinct(StringComparer.Ordinal).Count()!=expectedTasks.Count || tasks.Length!=expectedTasks.Count || tasks.Any(task=>!expectedTasks.Contains(task.CROSS_TASK_ID,StringComparer.Ordinal)))
            throw new InvalidOperationException("The complete investment task population changed.");
        if(pending is not null)
        {
            RequireInvestmentStage(pending.Command,original,proofs.Count);
            if(terminal || pending.At.Kind==DateTimeKind.Local || pending.At<process.StartDate || last is not null && pending.At<last.At ||
                pending.Command.Review.ProcessRevision!=InvestmentBusinessRevision(process,true) ||
                tasks.Count(task=>task.CROSS_TASK_ID==pending.Command.Review.TaskId && ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(task))==pending.Command.Review.TaskRevision)!=1 || row.ROW_CHANGED_BY!=pending.Command.Scope().ActorUserId || row.ROW_CHANGED_DATE!=pending.At)
                throw new InvalidOperationException("The exact original lifecycle reservation or its unchanged stage graph is unavailable.");
        }
        return JsonSerializer.Serialize(new{Process=row,Steps=steps.Cast<PROCESS_STEP_INSTANCE>().OrderBy(item=>item.PROCESS_STEP_INSTANCE_ID,StringComparer.Ordinal),History=history.Cast<PROCESS_HISTORY>().OrderBy(item=>item.PROCESS_HISTORY_ID,StringComparer.Ordinal),Approvals=approvals.Cast<PROCESS_APPROVAL>().OrderBy(item=>item.PROCESS_APPROVAL_ID,StringComparer.Ordinal),Tasks=tasks.OrderBy(item=>item.CROSS_TASK_ID,StringComparer.Ordinal)});
    }
    public Task<DevelopmentInvestmentStageReceipt> ReserveInvestmentStageAsync(DevelopmentInvestmentStageDecision command,Func<Task> authority,Func<Task<DevelopmentInvestmentReview>> readSource)
        =>InvestmentStageOperation(command,authority,readSource,0);
    public Task<DevelopmentInvestmentStageReceipt> CompleteInvestmentStageAsync(DevelopmentInvestmentStageDecision command,Func<Task> authority,Func<Task<DevelopmentInvestmentReview>> readSource)
        =>InvestmentStageOperation(command,authority,readSource,1);
    public Task<DevelopmentInvestmentStageReceipt> RecoverInvestmentStageAsync(DevelopmentInvestmentStageDecision command,Func<Task> authority,Func<Task<DevelopmentInvestmentReview>> readSource)
        =>InvestmentStageOperation(command,authority,readSource,2);
    private async Task<DevelopmentInvestmentStageReceipt> InvestmentStageOperation(DevelopmentInvestmentStageDecision input,Func<Task> authority,Func<Task<DevelopmentInvestmentReview>> readSource,int operation)
    {
        input.Require();var command=CompanyInventorySourceReview.Copy(input);var review=command.Review;var scope=command.Scope();
        if(scope.LifecycleDatabase.ConnectionName!=_connectionName)throw new UnauthorizedAccessException("The original investment lifecycle target is required.");
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("The original investment lifecycle datasource is unavailable.");
        async Task Guard(bool owned)
        {
            if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("The original investment stage source changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            await authority();if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
        }
        await Guard(false);var before=await readSource();before.Require();DevelopmentInvestmentHandoff? original=null;ProcessInstance? process=null;string? graph=null;
        InvestmentStageProof? proof=null;InvestmentReservation? pending=null;
        async Task PersistPayload(ProcessInstance instance,string actor,DateTime at)
        {
            instance.ProcessData=CompanyInventorySourceReview.Copy(instance.ProcessData);
            var row=(PROCESS_INSTANCE)(await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",instance.InstanceId))).Single();
            row.INSTANCE_DATA_JSON=JsonSerializer.Serialize(instance.ProcessData);
            if(row.INSTANCE_DATA_JSON.Length>DevelopmentInvestmentHandoff.MaximumBodyLength+65536)throw new InvalidOperationException("The complete bounded investment stage payload exceeds its supported size.");
            row.ROW_CHANGED_BY=actor;row.ROW_CHANGED_DATE=at;await GetProcessInstanceRepository().UpdateAsync(row,actor);
        }
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);process=await LoadProcessInstanceAsync(review.InstanceId)??throw new InvalidOperationException("The original investment assignment is unavailable.");
            var payload=JsonSerializer.Serialize(process.ProcessData);
            if(DevelopmentInvestmentHandoff.OwnerCompany(payload)!=scope.Company)throw new UnauthorizedAccessException("The investment assignment belongs to another original company.");
            original=DevelopmentInvestmentHandoff.FromPayload(payload,review.InstanceId,scope.FieldId,review.Source.AfeId);RequireInvestmentStage(command,original,review.StageNumber-1);original.RequireSource(before,false);
            graph=await ConfirmInvestmentGraph(process,original);var proofs=InvestmentProofs(process);pending=InvestmentPending(process);proof=proofs.SingleOrDefault(item=>item.Command.IntentId==command.IntentId);
            if(proof is not null){if(proof.Command.Revision()!=command.Revision())throw new InvalidOperationException("The recorded original investment decision intent changed.");return;}
            if(proofs.Any(item=>item.Command.Review.StepId==review.StepId))throw new InvalidOperationException("Another original decision already owns this configured stage.");
            if(pending is not null && pending.Command.Revision()!=command.Revision())throw new InvalidOperationException("Another retained original decision reserved this stage. Its outcome must be reconciled first.");
            if(process.Status!=ProcessStatus.IN_PROGRESS || process.CurrentStepId!=review.StepId || proofs.Count!=review.StageNumber-1)
                throw new InvalidOperationException("The exact original pending investment stage is no longer current.");
            if(operation!=2)
            {
                var definition=await GetProcessDefinitionAsync(original.Workflow.ProcessId)??throw new InvalidOperationException("The original configured investment definition is unavailable.");DevelopmentReviewDefinition.RequireInvestment(definition);
                if(ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition))!=original.Workflow.DefinitionRevision)throw new InvalidOperationException("The configured investment workflow changed before the stage decision.");
            }
            if(pending is null)
            {
                if(ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(process))!=review.ProcessRevision || operation!=2 && (before.Current.Revision!=review.Source.Current.Revision || before.Recorded is not null || before.BlocksRouting()))
                    throw new InvalidOperationException("The exact displayed investment stage or financial source changed before reservation.");
                var current=process.StepInstances.Single(item=>item.StepInstanceId==review.StepInstanceId);var router=new CrossPersonaTaskRouter(_editor,_commonColumnHandler,_defaults,_metadata,()=>Task.FromResult(_connectionName),personas:new InvestmentHandoffPersonas(original,()=>Guard(true)));
                var tasks=await router.ReadTaskSetInOwnedTransactionAsync(process.InstanceId,current.StepInstanceId,current.RequiredRole!,"AFE",process.EntityId,proofs.LastOrDefault()?.Command.Scope().ActorUserId??process.StartedBy);
                if(tasks.Count(task=>task.CROSS_TASK_ID==review.TaskId && task.TARGET_PERSONA_CODE==review.PersonaCode && ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(task))==review.TaskRevision)!=1)
                    throw new InvalidOperationException("The original displayed investment task changed before reservation.");
                if(operation==1)throw new InvalidOperationException("A durable original lifecycle reservation must precede financial or stage completion.");
                if(operation==0){pending=new(command,DateTime.UtcNow);process.ProcessData.Data[InvestmentReservationKey]=pending;await PersistPayload(process,scope.ActorUserId,pending.At);}
            }
            else if(operation==1)
            {
                DevelopmentInvestmentFacts? financial=null;
                if(command.Terminal()){financial=before.Recorded??throw new InvalidOperationException("The exact original final financial outcome must be recorded before lifecycle completion.");command.RequireFinancial(financial);}
                else if(before.Recorded is not null)throw new InvalidOperationException("An intermediate approval cannot adopt a final financial outcome.");
                var current=process.StepInstances.Single(item=>item.StepInstanceId==review.StepInstanceId);var router=new CrossPersonaTaskRouter(_editor,_commonColumnHandler,_defaults,_metadata,()=>Task.FromResult(_connectionName),personas:new InvestmentHandoffPersonas(original,()=>Guard(true)));
                var applied=await ApplyOwnedReviewStageAsync(process,current,InvestmentTemplates(original),router,scope.ActorUserId,command.Decision,command.Rationale,command.IntentId,"investment-stage","INVESTMENT_REVIEW",()=>Guard(true));
                process=applied.Process;proof=new(command,pending.At,applied.At,InvestmentBusinessRevision(process),applied.Approval,applied.History,applied.Closed,applied.Routed,applied.NextStep,financial);
                proofs.Add(proof);process.ProcessData.Data.Remove(InvestmentReservationKey);process.ProcessData.Data[InvestmentStageProofsKey]=proofs;await PersistPayload(process,scope.ActorUserId,applied.At);pending=null;
            }
            process=await LoadProcessInstanceAsync(review.InstanceId)??throw new InvalidOperationException("The recorded investment stage requires original readback.");graph=await ConfirmInvestmentGraph(process,original);await Guard(true);
        });
        await Guard(false);var closing=await LoadProcessInstanceAsync(review.InstanceId)??throw new InvalidOperationException("The original investment stage outcome is unconfirmed.");
        if(graph!=await ConfirmInvestmentGraph(closing,original!))throw new InvalidOperationException("The investment stage commit or complete original graph is unconfirmed.");
        var after=await readSource();original!.RequireSource(after,false);await Guard(false);
        DevelopmentInvestmentFacts? observed=proof?.Financial;
        if(proof is null && pending is not null && command.Terminal() && after.Recorded is not null)observed=command.RequireFinancial(after.Recorded);
        if(proof?.Financial is not null && after.Recorded!=proof.Financial || proof is not null && !command.Terminal() && operation!=2 && after.Recorded is not null)
            throw new InvalidOperationException("The final financial source changed during native stage acknowledgement.");
        if(proof is null && pending is null && after.Recorded is not null)throw new InvalidOperationException("Financial evidence without the original lifecycle reservation requires reconciliation; absence was not confirmed.");
        var status=proof is not null?"RECORDED":pending is not null?(observed is null?"RESERVED":"FINANCIAL_RECORDED"):"ABSENT";
        var receipt=new DevelopmentInvestmentStageReceipt(command.Revision(),review.InstanceId,status,closing.CurrentState,proof?.Approval.PROCESS_APPROVAL_ID,proof?.History.PROCESS_HISTORY_ID,proof?.NextStep,observed,ProcessDefinitionRevision.FromJson(graph!),DateTimeOffset.UtcNow);receipt.Require(command);return receipt;
    }
}
