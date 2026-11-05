using System.Data;
using System.Reflection;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.LifeCycle.Services.Processes;

public partial class PPDMProcessService
{
    private const string StepCommandKey="CompanyAccountingStepCommand";
    private static string? JournalEvidenceAction(ProcessStepDefinition step)
    {
        if(step.StepConfiguration is not {Count:1} || !step.StepConfiguration.TryGetValue(CompanyAccountingStepEvidence.ConfigurationKey,out var configured))return null;return configured switch{string text=>text,JsonElement {ValueKind:JsonValueKind.String} json=>json.GetString(),_=>null};
    }
    private static bool SupportedAccountingEvidence(ProcessStepDefinition step,string entityType)
    {
        if(step.StepConfiguration is null)return false;
        if(CompanyAccountingSourceTypes.IsNativeDecision(entityType))
        {
            if(step.StepConfiguration.Count!=1 || !step.StepConfiguration.TryGetValue(CompanyAccountingStepEvidence.ConfigurationKey,out var commercial))return false;
            var commercialAction=commercial switch{string value=>value,JsonElement {ValueKind:JsonValueKind.String} json=>json.GetString(),_=>null};
            if(entityType=="JOURNAL_ENTRY" && commercialAction==CompanyAccountingSourceTypes.JournalPosting)return !step.RequiresApproval && step.StepType=="ACTION";
            return commercialAction==CompanyAccountingSourceTypes.Approval(entityType)?step.RequiresApproval && step.StepType=="APPROVAL":
                commercialAction==CompanyAccountingSourceTypes.RejectionReview(entityType) && !step.RequiresApproval && step.StepType=="ACTION";
        }
        if(step.StepConfiguration.Count==0)return true;
        if(entityType!="AP_INVOICE" || step.StepConfiguration.Count!=1 ||
            !step.StepConfiguration.TryGetValue(CompanyAccountingStepEvidence.ConfigurationKey,out var configured))return false;
        var action=configured switch{string value=>value,JsonElement value when value.ValueKind==JsonValueKind.String=>value.GetString(),_=>null};
        return action==CompanyAccountingStepEvidence.InvoiceCorrectionAcknowledgement?step.RequiresApproval && step.StepType=="APPROVAL":action==CompanyAccountingStepEvidence.InvoiceVarianceCorrectionWork
            ? !step.RequiresApproval && step.StepType=="ACTION"
            : step.RequiresApproval && action is CompanyAccountingStepEvidence.InvoiceVarianceDisposition or CompanyAccountingStepEvidence.InvoiceVarianceCorrectionRequest or CompanyAccountingStepEvidence.InvoiceCorrectionAcknowledgement;
    }
    public async Task<CompanyAccountingStepCommand?> GetRecordedCompanyAccountingStepCommandAsync(string instanceId)
    {
        var instance=await LoadProcessInstanceAsync(instanceId) ?? throw new InvalidOperationException("The recorded process is unavailable.");
        if(!instance.ProcessData.Data.TryGetValue(StepCommandKey,out var value))return null;
        try
        {
            var evidence=JsonSerializer.Deserialize<StepEvidence>(JsonSerializer.Serialize(value)) ?? throw new InvalidOperationException("The retained command is unavailable.");
            evidence.Command.Require();if(evidence.Command.Review.InstanceId!=instanceId)throw new InvalidOperationException("The retained command belongs to another process.");
            return JsonSerializer.Deserialize<CompanyAccountingStepCommand>(JsonSerializer.Serialize(evidence.Command))!;
        }
        catch(Exception invalid) when(invalid is JsonException or ArgumentException){throw new InvalidOperationException("The retained command is invalid.",invalid);}
    }
    private sealed record StepEvidence(CompanyAccountingStepCommand Command,string ProcessDigest,string HistoryId,
        string? NextStep,List<CROSS_PERSONA_TASK> Closed,List<CROSS_PERSONA_TASK> Next,PROCESS_APPROVAL? Approval,
        [property:System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] StepEvidence? Previous=null);
    private static string StepBusinessDigest(ProcessInstance instance)
    {
        var copy=JsonSerializer.Deserialize<ProcessInstance>(JsonSerializer.Serialize(instance))!;
        copy.ProcessData.Data.Remove(StepCommandKey);
        return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));
    }
    public async Task<CompanyAccountingStepReceipt> CompleteCompanyAccountingStepAsync(CompanyAccountingStepCommand command,Func<Task> requireCurrentReview)
    {
        ArgumentNullException.ThrowIfNull(command);command.Require();ArgumentNullException.ThrowIfNull(requireCurrentReview);
        if(_personaReader is null)throw new InvalidOperationException("Current company persona assignments are required.");
        var frozen=JsonSerializer.Deserialize<CompanyAccountingStepCommand>(JsonSerializer.Serialize(command))!;var review=frozen.Review;var returning=frozen.Action=="RETURN_CORRECTION_WORK";
        var nativeRejection=CompanyAccountingSourceTypes.IsNativeDecision(review.EntityType) && frozen.Action==CompanyAccountingSourceTypes.RejectionRoute(review.EntityType);
        var source=_editor.GetDataSource(_connectionName) ?? throw new InvalidOperationException("The company process datasource is unavailable.");
        var router=new CrossPersonaTaskRouter(_editor,_commonColumnHandler,_defaults,_metadata,()=>Task.FromResult(_connectionName),personas:_personaReader);
        async Task Guard(){if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("The command binding changed.");PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);await requireCurrentReview();PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);}
        async Task<string> NativeGraph()
        {
            var processes=(await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",review.InstanceId))).ToList();
            var steps=(await GetProcessStepInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",review.InstanceId))).ToList();
            var history=(await GetProcessHistoryRepository().GetAsync(Key("PROCESS_INSTANCE_ID",review.InstanceId))).ToList();
            var definitions=(await GetProcessDefinitionRepository().GetAsync(Key("PROCESS_DEFINITION_ID",review.ProcessId))).ToList();
            var approvals=(await GetProcessApprovalRepository().GetAsync(Key("PROCESS_INSTANCE_ID",review.InstanceId))).ToList();
            if(processes.Count!=1 || processes[0] is not PROCESS_INSTANCE || definitions.Count!=1 || definitions[0] is not PROCESS_DEFINITION || steps.Any(row=>row is not PROCESS_STEP_INSTANCE) || history.Any(row=>row is not PROCESS_HISTORY) || approvals.Any(row=>row is not PROCESS_APPROVAL))throw new InvalidOperationException("The native command graph is ambiguous.");
            return JsonSerializer.Serialize(new{Definition=definitions[0],Process=processes[0],Steps=steps.Cast<PROCESS_STEP_INSTANCE>().OrderBy(row=>row.PROCESS_STEP_INSTANCE_ID,StringComparer.Ordinal),History=history.Cast<PROCESS_HISTORY>().OrderBy(row=>row.PROCESS_HISTORY_ID,StringComparer.Ordinal),Approvals=approvals.Cast<PROCESS_APPROVAL>().OrderBy(row=>row.PROCESS_APPROVAL_ID,StringComparer.Ordinal)});
        }
        async Task Confirm(StepEvidence evidence)
        {
            var current=await LoadProcessInstanceAsync(review.InstanceId) ?? throw new InvalidOperationException("The recorded command process is unavailable.");
            if(StepBusinessDigest(current)!=evidence.ProcessDigest)throw new InvalidOperationException("The recorded command graph progressed or changed.");
            await router.ConfirmRecordedTaskSetAsync(evidence.Closed,_connectionName);
            if(evidence.Next.Count>0)await router.ConfirmRecordedTaskSetAsync(evidence.Next,_connectionName);
            var approvals=(await GetProcessApprovalRepository().GetAsync(Key("PROCESS_STEP_INSTANCE_ID",evidence.Command.Review.StepInstanceId))).ToList();
            if(evidence.Approval is {} approval)
            {if(approvals.Count!=1 || JsonSerializer.Serialize(approvals[0])!=JsonSerializer.Serialize(approval))throw new InvalidOperationException("The recorded workflow approval is unconfirmed.");}
            else if(approvals.Count!=0)throw new InvalidOperationException("An unreviewed approval changed the recorded action outcome.");
        }
        static void CopyBusiness<T>(T target,T converted) where T:ModelEntityBase
        {foreach(var property in typeof(T).GetProperties(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly))if(property.CanWrite)property.SetValue(target,property.GetValue(converted));}
        async Task ConfirmUpdatedRow(ModelEntityBase prepared,string table,string key,string id,string creator,DateTime? created)
        {
            var repository=table=="PROCESS_INSTANCE"?GetProcessInstanceRepository():GetProcessStepInstanceRepository();
            var recorded=(await repository.GetAsync(Key(key,id))).ToList();
            if(prepared.ROW_CREATED_BY!=creator || prepared.ROW_CREATED_DATE!=created || prepared.ROW_CHANGED_BY!=review.ActorUserId || prepared.ROW_CHANGED_DATE is null ||
                recorded.Count!=1 || JsonSerializer.Serialize(recorded[0])!=JsonSerializer.Serialize((object)prepared))throw new InvalidOperationException("The updated process/step business and audit read-back is unconfirmed.");
        }
        StepEvidence? proof=null;string? graph=null;
        await requireCurrentReview();
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>{
            await Guard();
            var instance=await LoadProcessInstanceAsync(review.InstanceId) ?? throw new InvalidOperationException("The original company process is unavailable.");
            CompanyAccountingProcessBinding.FromPayloadJson(JsonSerializer.Serialize(instance.ProcessData),review.InstanceId,review.EntityType,review.EntityId).Require(review.Company,review.InstanceId,review.EntityType,review.EntityId);
            var definition=await GetProcessDefinitionAsync(review.ProcessId) ?? throw new InvalidOperationException("The reviewed definition is unavailable.");
            if(!definition.IsActive || definition.ProcessType!="FINANCIAL" || definition.EntityType!=review.EntityType || instance.ProcessId!=review.ProcessId ||
                ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition))!=review.DefinitionRevision)throw new InvalidOperationException("The reviewed definition changed.");
            var nativeDefinition=JsonSerializer.Serialize((await GetProcessDefinitionRepository().GetAsync(Key("PROCESS_DEFINITION_ID",review.ProcessId))).Single());
            if(instance.ProcessData.Data.TryGetValue(StepCommandKey,out var retained))
            {
                try{proof=JsonSerializer.Deserialize<StepEvidence>(JsonSerializer.Serialize(retained));}
                catch(JsonException invalid){throw new InvalidOperationException("The retained command evidence is malformed.",invalid);}
                if(proof?.Command.IntentId==frozen.IntentId)
                {
                    if(JsonSerializer.Serialize(proof.Command)!=JsonSerializer.Serialize(frozen))throw new InvalidOperationException("The retained intent changed.");
                    await Confirm(proof);graph=await NativeGraph();await Guard();return;
                }
                if(proof is null)throw new InvalidOperationException("The retained command evidence is unavailable.");
                await Confirm(proof);
            }
            if(ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(instance))!=review.ProcessRevision || instance.Status!=ProcessStatus.IN_PROGRESS)
                throw new InvalidOperationException("The displayed process changed before completion.");
            var start=JsonSerializer.Deserialize<CompanyAccountingProcessStart>(JsonSerializer.Serialize(instance.ProcessData.Data[CompanyAccountingProcessStart.PayloadKey]))!;start.Require();
            if(start.Company!=review.Company || start.InstanceId()!=review.InstanceId || start.ActorUserId!=review.WorkflowStarterUserId || instance.StartedBy!=review.WorkflowStarterUserId ||
                start.SourcePreparerUserId!=review.SourceMakerUserId || start.SourceRevision!=review.SourceRevision || start.BindingRevision!=review.BindingRevision ||
                start.DefinitionRevision!=review.DefinitionRevision)throw new InvalidOperationException("The original source/maker handoff differs from the review.");
            var steps=definition.Steps.OrderBy(step=>step.SequenceNumber).ToArray();
            if(steps.Length!=instance.StepInstances.Count || steps.Select(step=>step.StepId).Distinct(StringComparer.Ordinal).Count()!=steps.Length ||
                steps.Select(step=>step.SequenceNumber).Distinct().Count()!=steps.Length || steps.Any(step=>step.RequiredRoles is null || step.RequiredRoles.Count!=1 ||
                    string.IsNullOrWhiteSpace(step.StepId) || step.StepId.Length>128 || step.StepId.Any(char.IsControl) || step.SequenceNumber<0 || step.SlaHours<0 ||
                    step.RequiredRoles.Any(role=>string.IsNullOrWhiteSpace(role) || role.Length>128 || role.Any(char.IsControl)) ||
                    step.ConditionalNextSteps?.Count>0 || step.ValidationRules?.Count>0 || !SupportedAccountingEvidence(step,review.EntityType)))
                throw new InvalidOperationException("This recorded branching or validation policy needs explicit command support.");
            var index=Array.FindIndex(steps,step=>step.StepId==review.StepId);
            static string? EvidenceAction(ProcessStepDefinition step)=>step.StepConfiguration.TryGetValue(CompanyAccountingStepEvidence.ConfigurationKey,out var value)
                ?value switch{string text=>text,JsonElement json when json.ValueKind==JsonValueKind.String=>json.GetString(),_=>null}:null;
            for(var position=0;position<steps.Length;position++)
            {
                var action=EvidenceAction(steps[position]);
                if(action==CompanyAccountingStepEvidence.InvoiceVarianceCorrectionRequest && (position+1>=steps.Length || EvidenceAction(steps[position+1])!=CompanyAccountingStepEvidence.InvoiceVarianceCorrectionWork) ||
                    action==CompanyAccountingStepEvidence.InvoiceVarianceCorrectionWork && (position==0 || EvidenceAction(steps[position-1])!=CompanyAccountingStepEvidence.InvoiceVarianceCorrectionRequest))
                    throw new InvalidOperationException("A correction request requires its immediately configured accountant correction step.");
            }
            if(index<0 || instance.CurrentStepId!=review.StepId)throw new InvalidOperationException("The reviewed current step changed.");
            var correctionAction=EvidenceAction(steps[index]);
            var nativeRejectionReview=CompanyAccountingSourceTypes.IsNativeDecision(review.EntityType) && correctionAction==CompanyAccountingSourceTypes.RejectionReview(review.EntityType);
            if(CompanyAccountingSourceTypes.IsNativeDecision(review.EntityType) && review.EntityType!="JOURNAL_ENTRY")
            {
                if(steps.Length is <1 or >2 || EvidenceAction(steps[0])!=CompanyAccountingSourceTypes.Approval(review.EntityType) ||
                    steps.Length==2 && EvidenceAction(steps[1])!=CompanyAccountingSourceTypes.RejectionReview(review.EntityType) ||
                    nativeRejection && (index!=0 || steps.Length!=2) ||
                    index==1 && (review.ActorUserId!=start.SourcePreparerUserId || proof?.Command.Action!=CompanyAccountingSourceTypes.RejectionRoute(review.EntityType) || proof.Command.Review.AccountingEvidenceRevision!=review.AccountingEvidenceRevision))
                    throw new InvalidOperationException("Native rejection requires its configured original-preparer review and exact recorded routing evidence.");
            }
            if(review.EntityType=="JOURNAL_ENTRY")
            {
                if(review.AccountingEvidenceAction!=EvidenceAction(steps[index]) || steps.Length!=3 || EvidenceAction(steps[0])!=CompanyAccountingSourceTypes.Approval("JOURNAL_ENTRY") || EvidenceAction(steps[1])!=CompanyAccountingSourceTypes.JournalPosting || EvidenceAction(steps[2])!=CompanyAccountingSourceTypes.RejectionReview("JOURNAL_ENTRY") || nativeRejection && index!=0 || index==2 && (review.ActorUserId!=start.SourcePreparerUserId || proof?.Command.Action!=CompanyAccountingSourceTypes.RejectionRoute("JOURNAL_ENTRY")) || index==1 && (proof?.Command.Review.StepId!=steps[0].StepId || proof.Command.Action is not null))throw new InvalidOperationException("Journal handoff requires its exact native approval, separate posting and original-maker rejection branch.");
            }
            if(correctionAction is CompanyAccountingStepEvidence.InvoiceVarianceCorrectionWork or CompanyAccountingStepEvidence.InvoiceCorrectionAcknowledgement)
            {
                var evidence=review.CorrectionEvidence??throw new InvalidOperationException("Recorded corrected invoice command evidence is required before completing accountant correction work.");
                evidence.Require(review.Company,review.ActorUserId,review.EntityId);
                if(evidence.Action!=(returning?CompanyAccountingStepEvidence.InvoiceCorrectionReturn:correctionAction) || evidence.Revision!=review.AccountingEvidenceRevision || evidence.SourceMakerUserId!=review.SourceMakerUserId ||
                    correctionAction==CompanyAccountingStepEvidence.InvoiceVarianceCorrectionWork && (index+1>=steps.Length || EvidenceAction(steps[index+1])!=CompanyAccountingStepEvidence.InvoiceCorrectionAcknowledgement) ||
                    correctionAction==CompanyAccountingStepEvidence.InvoiceCorrectionAcknowledgement && (index==0 || EvidenceAction(steps[index-1])!=CompanyAccountingStepEvidence.InvoiceVarianceCorrectionWork))
                    throw new InvalidOperationException("Correction work requires its immediately configured independent version-bound acknowledgement step.");
                if(correctionAction==CompanyAccountingStepEvidence.InvoiceCorrectionAcknowledgement)
                {
                    var work=proof?.Command.Review.CorrectionEvidence;
                    if(work?.Action!=CompanyAccountingStepEvidence.InvoiceVarianceCorrectionWork || proof!.Command.IntentId!=evidence.CorrectionWorkIntentId ||
                        proof.Command.Review.StepId!=steps[index-1].StepId ||
                        (returning?work.CorrectionVersion>=evidence.CorrectionVersion:work.CorrectionVersion!=evidence.CorrectionVersion || work.CorrectionIntentId!=evidence.CorrectionIntentId || work.HistoryRevision!=evidence.HistoryRevision || work.SourceRevision!=evidence.SourceRevision))
                        throw new InvalidOperationException("Independent acknowledgement requires the exact completed correction-work intent and version.");
                }
            }
            var current=instance.StepInstances.Single(step=>step.StepInstanceId==review.StepInstanceId && step.StepId==review.StepId);var template=steps[index];
            if((template.StepConfiguration?.Count>0)!=(review.AccountingEvidenceRevision is not null))
                throw new InvalidOperationException("The configured accounting evidence differs from the reviewed command.");
            var independent=current.ApprovalRequired || template.RequiresApproval || template.StepType is "APPROVAL" or "REVIEW";
            if(current.Status is not (StepStatus.PENDING or StepStatus.IN_PROGRESS) || current.RequiredRole!=review.AssignedRole || template.RequiredRoles[0]!=review.AssignedRole ||
                current.ApprovalRequired!=template.RequiresApproval || independent!=review.RequiresIndependentActor ||
                independent && (review.ActorUserId==instance.StartedBy || review.ActorUserId==start.SourcePreparerUserId) ||
                review.RequiredPermission!=(current.ApprovalRequired || template.StepType=="APPROVAL"?"Workflow.Approve":"Workflow.Execute"))throw new InvalidOperationException("The current assignment or independent actor evidence changed.");
            for(var position=0;position<steps.Length;position++)
            {
                var row=instance.StepInstances.Single(step=>step.StepId==steps[position].StepId);
                if(row.RequiredRole!=steps[position].RequiredRoles[0] || row.SequenceNumber!=steps[position].SequenceNumber ||
                    position<index && row.Status!=(review.EntityType=="JOURNAL_ENTRY" && index==2 && position==1?StepStatus.SKIPPED:StepStatus.COMPLETED) || position>index && row.Status!=StepStatus.BLOCKED)
                    throw new InvalidOperationException("The complete reviewed step graph is inconsistent.");
                var nextId=position+1<steps.Length?steps[position+1].StepId:"";
                if(!string.IsNullOrEmpty(steps[position].NextStepId) && steps[position].NextStepId!=nextId)throw new InvalidOperationException("The recorded routing is not a reviewed sequential handoff.");
            }
            var historyId="company-action-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{review.InstanceId,frozen.IntentId}));
            if((await GetProcessHistoryRepository().GetAsync(Key("PROCESS_HISTORY_ID",historyId))).Any())throw new InvalidOperationException("The action audit identity is already owned.");
            if((await GetProcessApprovalRepository().GetAsync(Key("PROCESS_STEP_INSTANCE_ID",review.StepInstanceId))).Any())throw new InvalidOperationException("An existing approval or delegation requires reconciliation before completion.");
            var closed=await router.CompleteTaskSetInOwnedTransactionAsync(review.InstanceId,review.StepInstanceId,review.AssignedRole,review.EntityType,review.EntityId,review.ActorUserId,_connectionName);
            await Guard();var now=DateTime.UtcNow;current.Status=StepStatus.COMPLETED;current.StartDate??=now;current.CompletionDate=now;current.CompletedBy=review.ActorUserId;current.Outcome=nativeRejection?"REJECTED":nativeRejectionReview?"REJECTION_REVIEWED":"SUCCESS";current.Notes=frozen.Notes;
            PROCESS_APPROVAL? approval=null;
            if(!returning && review.RequiredPermission=="Workflow.Approve")
            {
                approval=new(){PROCESS_APPROVAL_ID="company-approval-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{review.InstanceId,frozen.IntentId})),
                    PROCESS_INSTANCE_ID=review.InstanceId,PROCESS_STEP_INSTANCE_ID=review.StepInstanceId,APPROVAL_TYPE=nativeRejection?CompanyAccountingSourceTypes.DecisionApprovalType(review.EntityType):correctionAction==CompanyAccountingStepEvidence.InvoiceCorrectionAcknowledgement?"CORRECTION_ACKNOWLEDGEMENT":"STEP_APPROVAL",APPROVAL_STATUS=nativeRejection?"REJECTED":correctionAction==CompanyAccountingStepEvidence.InvoiceCorrectionAcknowledgement?"ACKNOWLEDGED":"APPROVED",REQUIRED_ACTION=nativeRejection?"REJECT":correctionAction==CompanyAccountingStepEvidence.InvoiceCorrectionAcknowledgement?"ACKNOWLEDGE_CORRECTION":"APPROVE",
                    APPROVER_USER_ID=review.ActorUserId,APPROVED_BY=review.ActorUserId,APPROVAL_DATE=now,APPROVAL_NOTES=frozen.Notes,REQUESTED_BY=review.WorkflowStarterUserId,REQUESTED_DATE=instance.StartDate,
                    ROW_CREATED_BY=review.ActorUserId,ROW_CREATED_DATE=now};
                if((await GetProcessApprovalRepository().GetAsync(Key("PROCESS_APPROVAL_ID",approval.PROCESS_APPROVAL_ID))).Any())throw new InvalidOperationException("The approval audit identity is already owned.");
                await GetProcessApprovalRepository().InsertAsync(approval,review.ActorUserId);
            }
            var skipped=new List<ProcessStepInstance>();
            ProcessStepInstance? next=null;
            if(returning)
            {
                if(correctionAction!=CompanyAccountingStepEvidence.InvoiceCorrectionAcknowledgement || index==0)throw new InvalidOperationException("Only the current independent acknowledgement can return correction work.");
                next=instance.StepInstances.Single(step=>step.StepId==steps[index-1].StepId);
                if(next.Status!=StepStatus.COMPLETED || next.CompletedBy!=review.CorrectionEvidence!.CorrectionPreparerUserId)throw new InvalidOperationException("The original preparer's completed correction work is required.");
                var cycle=new CompanyAccountingCorrectionCycle(review.Company,review.InstanceId,next.StepInstanceId,current.StepInstanceId,frozen.IntentId,
                    review.CorrectionEvidence.CorrectionVersion,review.CorrectionEvidence.CorrectionIntentId,review.CorrectionEvidence.CorrectionPreparerUserId);
                cycle.Require(new(review.Company,review.InstanceId,review.EntityType,review.EntityId));instance.ProcessData.Data[CompanyAccountingCorrectionCycle.PayloadKey]=cycle;
                current.Status=StepStatus.BLOCKED;current.StartDate=null;current.CompletionDate=null;current.CompletedBy=null;current.Outcome=string.Empty;current.Notes=string.Empty;
                next.Status=StepStatus.PENDING;next.StartDate=now;next.CompletionDate=null;next.CompletedBy=null;next.Outcome=string.Empty;next.Notes=string.Empty;
                instance.CurrentStepId=next.StepId;instance.CurrentState=next.StepId;
            }
            else if(review.EntityType=="JOURNAL_ENTRY")
            {
                if(index==0){var target=nativeRejection?2:1;next=instance.StepInstances.Single(step=>step.StepId==steps[target].StepId);next.Status=StepStatus.PENDING;next.StartDate=now;instance.CurrentStepId=next.StepId;instance.CurrentState=next.StepId;if(nativeRejection){var unused=instance.StepInstances.Single(step=>step.StepId==steps[1].StepId);unused.Status=StepStatus.SKIPPED;unused.CompletionDate=now;unused.CompletedBy=review.ActorUserId;unused.Outcome="REJECTED_NO_POSTING";skipped.Add(unused);}}
                else{instance.Status=ProcessStatus.COMPLETED;instance.CurrentState=index==2?"REJECTION_REVIEWED":"POSTED_CONFIRMED";instance.CompletionDate=now;instance.CompletedBy=review.ActorUserId;if(index==1){var unused=instance.StepInstances.Single(step=>step.StepId==steps[2].StepId);unused.Status=StepStatus.SKIPPED;unused.CompletionDate=now;unused.CompletedBy=review.ActorUserId;unused.Outcome="POSTED_NO_REJECTION_REVIEW";skipped.Add(unused);}}
            }
            else if(index+1<steps.Length && !(CompanyAccountingSourceTypes.IsNativeDecision(review.EntityType) && !nativeRejection)){next=instance.StepInstances.Single(step=>step.StepId==steps[index+1].StepId);next.Status=StepStatus.PENDING;next.StartDate=now;instance.CurrentStepId=next.StepId;instance.CurrentState=next.StepId;}
            else{instance.Status=ProcessStatus.COMPLETED;instance.CurrentState=nativeRejectionReview?"REJECTION_REVIEWED":"COMPLETED";instance.CompletionDate=now;instance.CompletedBy=review.ActorUserId;}
            instance.ProcessData.LastUpdated=now;
            if(CompanyAccountingSourceTypes.IsNativeDecision(review.EntityType) && review.EntityType!="JOURNAL_ENTRY" && index==0 && !nativeRejection && steps.Length==2)
            {
                var unused=instance.StepInstances.Single(step=>step.StepId==steps[1].StepId);unused.Status=StepStatus.SKIPPED;unused.CompletionDate=now;unused.CompletedBy=review.ActorUserId;unused.Outcome="APPROVED_NO_REJECTION_REVIEW";unused.Notes="The original terms were approved; rejection review is not required.";skipped.Add(unused);
            }
            var history=new ProcessHistoryEntry{HistoryId=historyId,InstanceId=review.InstanceId,StepInstanceId=review.StepInstanceId,Action=nativeRejection?CompanyAccountingSourceTypes.RejectionHistory(review.EntityType):nativeRejectionReview?CompanyAccountingSourceTypes.RejectionReviewHistory(review.EntityType):returning?"CORRECTION_RETURNED":"STEP_COMPLETED",PreviousState=review.StepId,NewState=instance.CurrentState,Timestamp=now,PerformedBy=review.ActorUserId,Notes=frozen.Notes};
            var nativeHistory=ConvertToProcessHistoryEntity(history);nativeHistory.ROW_CREATED_BY=review.ActorUserId;nativeHistory.ROW_CREATED_DATE=now;
            await GetProcessHistoryRepository().InsertAsync(nativeHistory,review.ActorUserId);instance.History.Add(history);
            var processRow=(PROCESS_INSTANCE)(await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",review.InstanceId))).Single();
            var processCreator=processRow.ROW_CREATED_BY;var processCreated=processRow.ROW_CREATED_DATE;
            CopyBusiness(processRow,ConvertToProcessInstanceEntity(instance));processRow.ROW_CHANGED_BY=review.ActorUserId;processRow.ROW_CHANGED_DATE=now;
            await GetProcessInstanceRepository().UpdateAsync(processRow,review.ActorUserId);
            await ConfirmUpdatedRow(processRow,"PROCESS_INSTANCE","PROCESS_INSTANCE_ID",review.InstanceId,processCreator,processCreated);
            foreach(var changed in new[]{current,next}.OfType<ProcessStepInstance>().Concat(skipped))
            {
                var row=(PROCESS_STEP_INSTANCE)(await GetProcessStepInstanceRepository().GetAsync(Key("PROCESS_STEP_INSTANCE_ID",changed.StepInstanceId))).Single();
                var creator=row.ROW_CREATED_BY;var created=row.ROW_CREATED_DATE;
                CopyBusiness(row,ConvertToProcessStepInstanceEntity(changed));row.ROW_CHANGED_BY=review.ActorUserId;row.ROW_CHANGED_DATE=now;await GetProcessStepInstanceRepository().UpdateAsync(row,review.ActorUserId);
                await ConfirmUpdatedRow(row,"PROCESS_STEP_INSTANCE","PROCESS_STEP_INSTANCE_ID",changed.StepInstanceId,creator,created);
            }
            var routed=next is null?new List<CROSS_PERSONA_TASK>():await router.RouteTaskInOwnedTransactionAsync(review.InstanceId,next.StepInstanceId,next.RequiredRole!,review.EntityType,review.EntityId,null,review.ActorUserId);
            proof=new(frozen,StepBusinessDigest(instance),historyId,next?.StepInstanceId,closed,routed,approval,CompanyAccountingSourceTypes.IsNativeDecision(review.EntityType) || returning || instance.ProcessData.Data.ContainsKey(CompanyAccountingCorrectionCycle.PayloadKey)?proof:null);
            instance.ProcessData.Data[StepCommandKey]=proof;processRow.INSTANCE_DATA_JSON=JsonSerializer.Serialize(instance.ProcessData);
            await GetProcessInstanceRepository().UpdateAsync(processRow,review.ActorUserId);
            await ConfirmUpdatedRow(processRow,"PROCESS_INSTANCE","PROCESS_INSTANCE_ID",review.InstanceId,processCreator,processCreated);
            await Confirm(proof);graph=await NativeGraph();await Guard();
            if(nativeDefinition!=JsonSerializer.Serialize((await GetProcessDefinitionRepository().GetAsync(Key("PROCESS_DEFINITION_ID",review.ProcessId))).Single()) || graph!=await NativeGraph())throw new InvalidOperationException("The complete handoff changed before commit.");
        });
        if(proof is null || !ReferenceEquals(source,_editor.GetDataSource(_connectionName)) || graph!=await NativeGraph())throw new InvalidOperationException("The step commit is unconfirmed. Retain its original intent.");
        await Confirm(proof);await requireCurrentReview();await Confirm(proof);
        if(graph!=await NativeGraph())throw new InvalidOperationException("The complete recorded command outcome changed after authority confirmation.");
        var receipt=new CompanyAccountingStepReceipt(review.Company,review.ActorUserId,frozen.IntentId,review.InstanceId,review.StepInstanceId,proof.NextStep,proof.HistoryId,
            proof.Closed.Select(row=>row.CROSS_TASK_ID).Order(StringComparer.Ordinal).ToArray(),proof.Next.Select(row=>row.CROSS_TASK_ID).Order(StringComparer.Ordinal).ToArray(),ProcessDefinitionRevision.FromJson(graph!));
        receipt.Require(frozen);return receipt;
    }
}
