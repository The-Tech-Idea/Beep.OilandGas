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
    private const string MaintenanceAcceptanceKey="CompanyMaintenanceAcceptance";
    private async Task<(CompanyMaintenanceRecoveryQuery Assignment,CompanyMaintenanceAcceptanceCommand? Command)> ReadReceivingTask(FieldSelectionReview current,CompanyMaintenanceReceivingQuery task,Guid? acceptance,Func<string?,Task> authority)
    {
        ArgumentNullException.ThrowIfNull(current);ArgumentNullException.ThrowIfNull(task);ArgumentNullException.ThrowIfNull(authority);task.Require();FieldSelectionReviewValidator.RequireReview(current,current.Company,current.ActorUserId);
        if(current.FieldId!=task.FieldId || current.Database.ConnectionName!=_connectionName || _personaReader is null)throw new UnauthorizedAccessException("The original receiving FIELD/lifecycle/persona owner is required.");
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("The receiving datasource is unavailable.");
        async Task Guard(bool owned,string? role)
        {
            if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("The receiving datasource changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            await authority(role);
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
        }
        await Guard(false,null);CompanyMaintenanceRecoveryQuery? assignment=null;CompanyMaintenanceAcceptanceCommand? command=null;string? role=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true,null);var rows=(await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",task.InstanceId))).ToArray();
            if(rows.Length!=1 || rows[0] is not PROCESS_INSTANCE row || row.PROCESS_INSTANCE_ID!=task.InstanceId || row.FIELD_ID!=task.FieldId || row.ENTITY_ID!=task.WorkOrderId || row.ENTITY_TYPE!="WORK_ORDER" || row.ACTIVE_IND!="Y")
                throw new InvalidOperationException("The original receiving task has no unique company/FIELD/order process.");
            var snapshot=JsonSerializer.Serialize(row);var start=CompanyMaintenanceProcessStart.FromPayload(row.INSTANCE_DATA_JSON!,task.InstanceId,task.FieldId,task.WorkOrderId);
            if(start.Context.Company!=current.Company || start.Context.Database!=current.Database || row.ROW_CREATED_BY!=start.Context.ActorUserId || current.ActorUserId==start.Context.ActorUserId || current.ActorUserId==start.Source.Reviewer)
                throw new UnauthorizedAccessException("The original receiving task requires its independent company/account/database owner.");
            assignment=new(start.IntentId,start.Source.DecisionId,task.FieldId,start.Source.WellId,task.WorkOrderId);
            var owned=await ReadReceivingOwner(current,assignment);role=owned.Definition.Steps.OrderBy(step=>step.SequenceNumber).First().RequiredRoles[0];await Guard(true,role);
            if(acceptance is {} id)
            {
                if(!owned.Instance.ProcessData.Data.TryGetValue(MaintenanceAcceptanceKey,out var retained))throw new InvalidOperationException("The original acceptance command is not recorded. Retain its reference.");
                var proof=JsonSerializer.Deserialize<MaintenanceAcceptanceEvidence>(JsonSerializer.Serialize(retained))??throw new InvalidOperationException("The original acceptance evidence is unavailable.");
                proof.Command.Require();
                if(proof.Command.IntentId!=id || proof.Command.Review.Context!=current || proof.Command.Review.InstanceId!=task.InstanceId || proof.Command.Review.Assignment!=assignment || proof.Command.Review.AssignedRole!=role)
                    throw new UnauthorizedAccessException("The private original acceptance command belongs to another reference/account/authority.");
                command=JsonSerializer.Deserialize<CompanyMaintenanceAcceptanceCommand>(JsonSerializer.Serialize(proof.Command))!;
            }
            var closing=(await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",task.InstanceId))).ToArray();
            if(closing.Length!=1 || JsonSerializer.Serialize(closing[0])!=snapshot)throw new InvalidOperationException("The original receiving task changed during discovery.");
            await Guard(true,role);
        });
        await Guard(false,role);return(assignment??throw new InvalidOperationException("The original receiving assignment is unconfirmed."),command);
    }
    public async Task<CompanyMaintenanceRecoveryQuery> DiscoverMaintenanceReceivingAssignmentAsync(FieldSelectionReview current,CompanyMaintenanceReceivingQuery task,Func<string?,Task> authority)
        =>(await ReadReceivingTask(current,task,null,authority)).Assignment;
    public async Task<CompanyMaintenanceAcceptanceCommand> ReadOriginalMaintenanceAcceptanceAsync(FieldSelectionReview current,CompanyMaintenanceAcceptanceReferenceQuery query,Func<string?,Task> authority)
    {
        ArgumentNullException.ThrowIfNull(query);query.Require();return(await ReadReceivingTask(current,query.Task,query.IntentId,authority)).Command??throw new InvalidOperationException("The original acceptance command is unavailable.");
    }
    private sealed record MaintenanceAcceptanceEvidence(CompanyMaintenanceAcceptanceCommand Command,string ProcessDigest,
        PROCESS_APPROVAL Approval,PROCESS_HISTORY History,List<CROSS_PERSONA_TASK> Closed,List<CROSS_PERSONA_TASK> Next,string? NextStep);
    private static string MaintenanceAcceptanceBusinessDigest(ProcessInstance instance)
    {
        var copy=JsonSerializer.Deserialize<ProcessInstance>(JsonSerializer.Serialize(instance))!;
        copy.ProcessData.Data.Remove(MaintenanceAcceptanceKey);return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));
    }
    private static void RequireReceivingSource(CompanyMaintenanceProcessStart start,FieldSelectionReview current,CompanyMaintenanceRecoveryQuery query,ProductionInterventionMaintenanceLink link)
    {
        start.Require();FieldSelectionReviewValidator.RequireReview(current,current.Company,current.ActorUserId);query.Require();
        link.Require(current.Company,current.ActorUserId,query.FieldId,query.WorkOrderId,query.WellId,query.DecisionId);
        if(start.Context.Company!=current.Company || start.Context.Database!=current.Database || start.Context.FieldId!=current.FieldId || current.FieldId!=query.FieldId || start.IntentId!=query.IntentId ||
            start.Source with{Actor=current.ActorUserId,RetrievedAtUtc=link.RetrievedAtUtc}!=link)
            throw new InvalidOperationException("The original maintenance assignment and approved FIELD/order/component source changed.");
    }
    private async Task<(ProcessInstance Instance,CompanyMaintenanceProcessStart Start,ProcessDefinition Definition)> ReadReceivingOwner(FieldSelectionReview current,CompanyMaintenanceRecoveryQuery query)
    {
        var id=CompanyMaintenanceProcessStart.InstanceIdFor(current.Company,query.IntentId);
        var rows=(await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).ToArray();
        if(rows.Length!=1 || rows[0] is not PROCESS_INSTANCE row || row.PROCESS_INSTANCE_ID!=id || row.ACTIVE_IND!="Y" || row.FIELD_ID!=query.FieldId || row.ENTITY_TYPE!="WORK_ORDER" || row.ENTITY_ID!=query.WorkOrderId)
            throw new InvalidOperationException("The original company/FIELD/order receiving assignment is unavailable.");
        var start=CompanyMaintenanceProcessStart.FromPayload(row.INSTANCE_DATA_JSON!,id,query.FieldId,query.WorkOrderId);
        if(start.Context.Company!=current.Company || start.Context.Database!=current.Database || current.FieldId!=query.FieldId || start.Source.WellId!=query.WellId || start.Source.DecisionId!=query.DecisionId)
            throw new UnauthorizedAccessException("The receiving assignment belongs to another original company/FIELD/database/source.");
        var instance=await LoadProcessInstanceAsync(id)??throw new InvalidOperationException("The complete receiving assignment is unavailable.");
        if(instance.StartedBy!=start.Context.ActorUserId || row.ROW_CREATED_BY!=start.Context.ActorUserId)throw new InvalidOperationException("The original receiving assignment starter audit changed.");
        var definition=await GetProcessDefinitionAsync(start.ProcessId)??throw new InvalidOperationException("The original configured receiving definition is unavailable.");
        if(!definition.IsActive || definition.ProcessType!="WORK_ORDER" || definition.EntityType!="WORK_ORDER" || ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition))!=start.DefinitionRevision ||
            definition.Steps is not {Count:>0 and <=64} || definition.Steps.Any(step=>step is null) || definition.Transitions is null || definition.Transitions.Count!=0)
            throw new InvalidOperationException("The original receiving definition changed or has unsupported state transitions.");
        var steps=definition.Steps.OrderBy(step=>step.SequenceNumber).ToArray();
        for(var position=0;position<steps.Length;position++)
        {
            var step=steps[position];
            if(!LeaseSummaryValidator.Identifier(step.StepId) || step.RequiredRoles is not {Count:1} || !LeaseSummaryValidator.Identifier(step.RequiredRoles[0]) || step.SequenceNumber<0 || step.SlaHours<0 ||
                step.ConditionalNextSteps is {Count:>0} || step.ValidationRules is {Count:>0} || step.StepConfiguration is {Count:>0} ||
                !string.IsNullOrEmpty(step.NextStepId) && (position+1==steps.Length || step.NextStepId!=steps[position+1].StepId))
                throw new InvalidOperationException("The receiving workflow needs explicit support for its configured branching or validation policy.");
        }
        if(steps.Select(step=>step.StepId).Distinct(StringComparer.Ordinal).Count()!=steps.Length || steps.Select(step=>step.SequenceNumber).Distinct().Count()!=steps.Length || !steps[0].RequiresApproval || steps[0].StepType!="APPROVAL")
            throw new InvalidOperationException("An explicit unique initial receiving approval role is required.");
        return(instance,start,definition);
    }
    private async Task<string> MaintenanceAcceptanceNativeGraph(string id,string definitionId)
    {
        async Task<object[]> Rows(PPDMGenericRepository repository,string table,string ownerKey,string owner,string key)
        {
            var values=(await repository.GetAsync(Key(ownerKey,owner))).ToArray();
            if(values.Any(row=>row.GetType().Name!=table || row.GetType().GetProperty(ownerKey)?.GetValue(row) as string!=owner) ||
                values.Select(row=>row.GetType().GetProperty(key)?.GetValue(row) as string).Any(value=>!LeaseSummaryValidator.Identifier(value)) ||
                values.Select(row=>row.GetType().GetProperty(key)?.GetValue(row) as string).Distinct(StringComparer.Ordinal).Count()!=values.Length)
                throw new InvalidOperationException("The receiving native graph has conflicting source ownership or identities.");
            return values.OrderBy(row=>row.GetType().GetProperty(key)!.GetValue(row) as string,StringComparer.Ordinal).ToArray();
        }
        var definition=await Rows(GetProcessDefinitionRepository(),"PROCESS_DEFINITION","PROCESS_DEFINITION_ID",definitionId,"PROCESS_DEFINITION_ID");
        var process=await Rows(GetProcessInstanceRepository(),"PROCESS_INSTANCE","PROCESS_INSTANCE_ID",id,"PROCESS_INSTANCE_ID");
        var steps=await Rows(GetProcessStepInstanceRepository(),"PROCESS_STEP_INSTANCE","PROCESS_INSTANCE_ID",id,"PROCESS_STEP_INSTANCE_ID");
        var history=await Rows(GetProcessHistoryRepository(),"PROCESS_HISTORY","PROCESS_INSTANCE_ID",id,"PROCESS_HISTORY_ID");
        var approvals=await Rows(GetProcessApprovalRepository(),"PROCESS_APPROVAL","PROCESS_INSTANCE_ID",id,"PROCESS_APPROVAL_ID");
        var tasks=await Rows(new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(CROSS_PERSONA_TASK),_connectionName,"CROSS_PERSONA_TASK"),"CROSS_PERSONA_TASK","PROCESS_INSTANCE_ID",id,"CROSS_TASK_ID");
        if(definition.Length!=1 || process.Length!=1 || steps.Length==0 || history.Length==0)throw new InvalidOperationException("The complete receiving graph is unavailable.");
        return JsonSerializer.Serialize(new{Definition=definition[0],Process=process[0],Steps=steps,History=history,Approvals=approvals,Tasks=tasks});
    }
    private async Task<CompanyMaintenanceAcceptanceReview> BuildReceivingReview(FieldSelectionReview current,CompanyMaintenanceRecoveryQuery query,ProductionInterventionMaintenanceLink link,CrossPersonaTaskRouter router,Func<string?,Task> authority)
    {
        var owned=await ReadReceivingOwner(current,query);RequireReceivingSource(owned.Start,current,query,link);
        var steps=owned.Definition.Steps.OrderBy(step=>step.SequenceNumber).ToArray();RequireMaintenanceStart(owned.Instance,owned.Start,steps);
        var first=owned.Instance.StepInstances.Single(step=>step.StepId==steps[0].StepId);var role=first.RequiredRole!;
        if(current.ActorUserId==owned.Start.Context.ActorUserId || current.ActorUserId==link.Reviewer)throw new UnauthorizedAccessException("Receiving acceptance requires an independent actor.");
        await authority(role);
        await router.ReadTaskSetInOwnedTransactionAsync(owned.Instance.InstanceId,first.StepInstanceId,role,"WORK_ORDER",query.WorkOrderId,owned.Start.Context.ActorUserId);
        if((await GetProcessApprovalRepository().GetAsync(Key("PROCESS_INSTANCE_ID",owned.Instance.InstanceId))).Any())throw new InvalidOperationException("Existing receiving approvals require reconciliation.");
        var graph=await MaintenanceAcceptanceNativeGraph(owned.Instance.InstanceId,owned.Start.ProcessId);
        var review=new CompanyMaintenanceAcceptanceReview(current,query,owned.Instance.InstanceId,owned.Start.ProcessId,owned.Start.DefinitionRevision,
            ProcessDefinitionRevision.FromJson(graph),first.StepInstanceId,role,owned.Start.Context.ActorUserId,link);review.Require();return review;
    }
    public async Task<CompanyMaintenanceAcceptanceReview> ReviewMaintenanceAcceptanceAsync(FieldSelectionReview current,CompanyMaintenanceRecoveryQuery query,ProductionInterventionMaintenanceLink link,Func<string?,Task> authority)
    {
        ArgumentNullException.ThrowIfNull(current);ArgumentNullException.ThrowIfNull(query);ArgumentNullException.ThrowIfNull(link);ArgumentNullException.ThrowIfNull(authority);
        FieldSelectionReviewValidator.RequireReview(current,current.Company,current.ActorUserId);query.Require();
        if(current.FieldId!=query.FieldId || current.Database.ConnectionName!=_connectionName || _personaReader is null)throw new UnauthorizedAccessException("The original receiving FIELD/lifecycle/persona owner is required.");
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("The receiving datasource is unavailable.");
        PPDMDataSourceTransaction.RequireIdle(source);await authority(null);PPDMDataSourceTransaction.RequireIdle(source);
        var router=new CrossPersonaTaskRouter(_editor,_commonColumnHandler,_defaults,_metadata,()=>Task.FromResult(_connectionName),personas:_personaReader);
        CompanyMaintenanceAcceptanceReview? review=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);await authority(null);
            review=await BuildReceivingReview(current,query,link,router,authority);
            await authority(review.AssignedRole);
            if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)) || await MaintenanceAcceptanceNativeGraph(review.InstanceId,review.ProcessId) is var graph && ProcessDefinitionRevision.FromJson(graph)!=review.ProcessRevision)
                throw new InvalidOperationException("The receiving review graph or database binding changed.");
            PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);
        });
        await authority(review!.AssignedRole);PPDMDataSourceTransaction.RequireIdle(source);
        if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)) || ProcessDefinitionRevision.FromJson(await MaintenanceAcceptanceNativeGraph(review.InstanceId,review.ProcessId))!=review.ProcessRevision)
            throw new InvalidOperationException("The receiving review changed after confirmation.");
        return review;
    }
    public Task<CompanyMaintenanceAcceptanceReceipt> AcceptMaintenanceAsync(CompanyMaintenanceAcceptanceCommand command,Func<string?,Task> authority,Func<Task<ProductionInterventionMaintenanceLink>> readSource)
        => ReadOrAcceptMaintenanceAsync(command,authority,readSource,false);
    public Task<CompanyMaintenanceAcceptanceReceipt> RecoverMaintenanceAcceptanceAsync(CompanyMaintenanceAcceptanceCommand command,Func<string?,Task> authority,Func<Task<ProductionInterventionMaintenanceLink>> readSource)
        => ReadOrAcceptMaintenanceAsync(command,authority,readSource,true);
    private async Task<CompanyMaintenanceAcceptanceReceipt> ReadOrAcceptMaintenanceAsync(CompanyMaintenanceAcceptanceCommand command,Func<string?,Task> authority,Func<Task<ProductionInterventionMaintenanceLink>> readSource,bool readOnly)
    {
        ArgumentNullException.ThrowIfNull(command);command.Require();ArgumentNullException.ThrowIfNull(authority);ArgumentNullException.ThrowIfNull(readSource);
        var frozen=JsonSerializer.Deserialize<CompanyMaintenanceAcceptanceCommand>(JsonSerializer.Serialize(command))!;var review=frozen.Review;
        if(_personaReader is null || _connectionName!=review.Context.Database.ConnectionName)throw new InvalidOperationException("The original receiving lifecycle/persona owner is required.");
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("The receiving datasource is unavailable.");
        var router=new CrossPersonaTaskRouter(_editor,_commonColumnHandler,_defaults,_metadata,()=>Task.FromResult(_connectionName),personas:_personaReader);
        async Task Guard(bool owned)
        {
            if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("The receiving database binding changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            await authority(review.AssignedRole);
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
        }
        void RequireSource(ProductionInterventionMaintenanceLink link)
        {
            link.Require(review.Context.Company,review.Context.ActorUserId,review.Assignment.FieldId,review.Assignment.WorkOrderId,review.Assignment.WellId,review.Assignment.DecisionId);
            if(review.Source with{RetrievedAtUtc=link.RetrievedAtUtc}!=link)throw new InvalidOperationException("The original approved receiving source changed.");
        }
        async Task Confirm(MaintenanceAcceptanceEvidence evidence)
        {
            if(JsonSerializer.Serialize(evidence.Command)!=JsonSerializer.Serialize(frozen))throw new InvalidOperationException("The original receiving acceptance intent changed.");
            var identity=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{review.InstanceId,frozen.IntentId}));
            if(evidence.Approval.PROCESS_APPROVAL_ID!="maintenance-accept-approval-"+identity || evidence.Approval.PROCESS_INSTANCE_ID!=review.InstanceId || evidence.Approval.PROCESS_STEP_INSTANCE_ID!=review.StepInstanceId ||
                evidence.Approval.APPROVAL_TYPE!="MAINTENANCE_RECEIVING" || evidence.Approval.APPROVAL_STATUS!="ACCEPTED" || evidence.Approval.REQUIRED_ACTION!="ACCEPT_RECEIVING" || evidence.Approval.APPROVER_USER_ID!=review.Context.ActorUserId ||
                evidence.Approval.APPROVED_BY!=review.Context.ActorUserId || evidence.Approval.REQUESTED_BY!=review.WorkflowStarter || evidence.Approval.APPROVAL_NOTES!=frozen.Notes || evidence.Approval.ACTIVE_IND!="Y" || evidence.Approval.ROW_CREATED_BY!=review.Context.ActorUserId || evidence.Approval.APPROVAL_DATE is null || evidence.Approval.ROW_CREATED_DATE is null ||
                evidence.History.PROCESS_HISTORY_ID!="maintenance-accept-history-"+identity || evidence.History.PROCESS_INSTANCE_ID!=review.InstanceId || evidence.History.PROCESS_STEP_INSTANCE_ID!=review.StepInstanceId || evidence.History.EVENT_TYPE!="RECEIVING_ACCEPTED" ||
                evidence.History.USER_ID!=review.Context.ActorUserId || evidence.History.ACTIVE_IND!="Y" || evidence.History.ROW_CREATED_BY!=review.Context.ActorUserId || evidence.History.DETAILS!=frozen.Notes || evidence.Closed.Count==0 ||
                evidence.Closed.Any(task=>task.TASK_STATUS!="COMPLETED" || task.ACTIVE_IND!="Y" || task.ASSIGNED_ROLE!=review.AssignedRole || task.ROW_CREATED_BY!=review.WorkflowStarter || task.ROW_CHANGED_BY!=review.Context.ActorUserId || task.PROCESS_INSTANCE_ID!=review.InstanceId || task.PROCESS_STEP_INSTANCE_ID!=review.StepInstanceId))
                throw new InvalidOperationException("The original receiving approval/history/task actor audit differs from its retained command.");
            var current=await LoadProcessInstanceAsync(review.InstanceId)??throw new InvalidOperationException("The accepted process is unavailable.");
            if(MaintenanceAcceptanceBusinessDigest(current)!=evidence.ProcessDigest)throw new InvalidOperationException("The accepted receiving process progressed or changed; reconcile its history.");
            await router.ConfirmRecordedTaskSetAsync(evidence.Closed,_connectionName);if(evidence.Next.Count>0)await router.ConfirmRecordedTaskSetAsync(evidence.Next,_connectionName);
            if(evidence.Next.Count>0)
            {
                var next=current.StepInstances.SingleOrDefault(step=>step.StepInstanceId==evidence.NextStep)??throw new InvalidOperationException("The recorded next receiving role step is unavailable.");
                if(current.CurrentStepId!=next.StepId || evidence.Next.Any(task=>task.ASSIGNED_ROLE!=next.RequiredRole || task.TASK_STATUS!="PENDING" || task.ROW_CREATED_BY!=review.Context.ActorUserId) ||
                    !evidence.Next.Select(task=>task.TARGET_PERSONA_CODE).Order(StringComparer.Ordinal).SequenceEqual((await _personaReader!.GetActivePersonasAsync(next.RequiredRole!)).Order(StringComparer.Ordinal),StringComparer.Ordinal))
                    throw new InvalidOperationException("The recorded next-role persona assignment changed after receiving acceptance.");
            }
            var approvals=(await GetProcessApprovalRepository().GetAsync(Key("PROCESS_STEP_INSTANCE_ID",review.StepInstanceId))).ToArray();
            var history=(await GetProcessHistoryRepository().GetAsync(Key("PROCESS_HISTORY_ID",evidence.History.PROCESS_HISTORY_ID))).ToArray();
            if(approvals.Length!=1 || JsonSerializer.Serialize(approvals[0])!=JsonSerializer.Serialize(evidence.Approval) || history.Length!=1 || JsonSerializer.Serialize(history[0])!=JsonSerializer.Serialize(evidence.History))
                throw new InvalidOperationException("The original receiving approval/history audit is unconfirmed.");
        }
        static void CopyBusiness<T>(T target,T prepared) where T:ModelEntityBase
        {foreach(var property in typeof(T).GetProperties(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly))if(property.CanWrite)property.SetValue(target,property.GetValue(prepared));}
        async Task Update(ModelEntityBase row,PPDMGenericRepository repository,string key,string id,string creator,DateTime? created)
        {
            await repository.UpdateAsync(row,review.Context.ActorUserId);var recorded=(await repository.GetAsync(Key(key,id))).ToArray();
            if(row.ROW_CREATED_BY!=creator || row.ROW_CREATED_DATE!=created || row.ROW_CHANGED_BY!=review.Context.ActorUserId || row.ROW_CHANGED_DATE is null || recorded.Length!=1 || JsonSerializer.Serialize(recorded[0])!=JsonSerializer.Serialize((object)row))
                throw new InvalidOperationException("The receiving business/audit row update is unconfirmed.");
        }
        await Guard(false);RequireSource(await readSource());await Guard(false);
        MaintenanceAcceptanceEvidence? proof=null;string? graph=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);var owned=await ReadReceivingOwner(review.Context,review.Assignment);RequireReceivingSource(owned.Start,review.Context,review.Assignment,review.Source);
            var instance=owned.Instance;
            var first=owned.Definition.Steps.OrderBy(step=>step.SequenceNumber).First();
            if(review.ProcessId!=owned.Start.ProcessId || review.DefinitionRevision!=owned.Start.DefinitionRevision || review.WorkflowStarter!=owned.Start.Context.ActorUserId || review.AssignedRole!=first.RequiredRoles[0] ||
                review.StepInstanceId!="maintenance-step-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Instance=review.InstanceId,Step=first.StepId})))
                throw new InvalidOperationException("The receiving command differs from the original configured role/starter/step.");
            if(instance.ProcessData.Data.TryGetValue(MaintenanceAcceptanceKey,out var value))
            {
                proof=JsonSerializer.Deserialize<MaintenanceAcceptanceEvidence>(JsonSerializer.Serialize(value))??throw new InvalidOperationException("The original receiving acceptance evidence is unavailable.");
                await Confirm(proof);graph=await MaintenanceAcceptanceNativeGraph(review.InstanceId,review.ProcessId);await Guard(true);return;
            }
            if(readOnly)throw new InvalidOperationException("The original receiving acceptance is not recorded. Read-only recovery cannot create it.");
            var currentReview=await BuildReceivingReview(review.Context,review.Assignment,review.Source,router,authority);
            if(currentReview!=review)throw new InvalidOperationException("The original receiving assignment review changed before acceptance.");
            var definitionSnapshot=JsonSerializer.Serialize(owned.Definition);var steps=owned.Definition.Steps.OrderBy(step=>step.SequenceNumber).ToArray();
            var current=instance.StepInstances.Single(step=>step.StepInstanceId==review.StepInstanceId);
            var closed=await router.CompleteTaskSetInOwnedTransactionAsync(review.InstanceId,review.StepInstanceId,review.AssignedRole,"WORK_ORDER",review.Assignment.WorkOrderId,review.Context.ActorUserId,_connectionName);
            await Guard(true);var now=DateTime.UtcNow;current.Status=StepStatus.COMPLETED;current.StartDate??=now;current.CompletionDate=now;current.CompletedBy=review.Context.ActorUserId;current.Outcome="RECEIVING_ACCEPTED";current.Notes=frozen.Notes;
            var identity=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{review.InstanceId,frozen.IntentId}));
            var approval=new PROCESS_APPROVAL{PROCESS_APPROVAL_ID="maintenance-accept-approval-"+identity,PROCESS_INSTANCE_ID=review.InstanceId,PROCESS_STEP_INSTANCE_ID=review.StepInstanceId,
                APPROVAL_TYPE="MAINTENANCE_RECEIVING",APPROVAL_STATUS="ACCEPTED",REQUIRED_ACTION="ACCEPT_RECEIVING",APPROVER_USER_ID=review.Context.ActorUserId,APPROVED_BY=review.Context.ActorUserId,
                APPROVAL_DATE=now,APPROVAL_NOTES=frozen.Notes,REQUESTED_BY=review.WorkflowStarter,REQUESTED_DATE=instance.StartDate,ROW_CREATED_BY=review.Context.ActorUserId,ROW_CREATED_DATE=now};
            if((await GetProcessApprovalRepository().GetAsync(Key("PROCESS_APPROVAL_ID",approval.PROCESS_APPROVAL_ID))).Any())throw new InvalidOperationException("The receiving approval identity is already owned.");
            await GetProcessApprovalRepository().InsertAsync(approval,review.Context.ActorUserId);
            ProcessStepInstance? next=null;
            if(steps.Length>1){next=instance.StepInstances.Single(step=>step.StepId==steps[1].StepId);next.Status=StepStatus.PENDING;next.StartDate=now;instance.CurrentStepId=next.StepId;instance.CurrentState=next.StepId;}
            else{instance.Status=ProcessStatus.COMPLETED;instance.CurrentState="RECEIVING_ACCEPTED";instance.CompletionDate=now;instance.CompletedBy=review.Context.ActorUserId;}
            instance.ProcessData.LastUpdated=now;
            var history=new ProcessHistoryEntry{HistoryId="maintenance-accept-history-"+identity,InstanceId=review.InstanceId,StepInstanceId=review.StepInstanceId,Action="RECEIVING_ACCEPTED",PreviousState="INITIAL",NewState=instance.CurrentState,Timestamp=now,PerformedBy=review.Context.ActorUserId,Notes=frozen.Notes};
            var historyRow=ConvertToProcessHistoryEntity(history);historyRow.ROW_CREATED_BY=review.Context.ActorUserId;historyRow.ROW_CREATED_DATE=now;
            await GetProcessHistoryRepository().InsertAsync(historyRow,review.Context.ActorUserId);instance.History.Add(history);
            var processRow=(PROCESS_INSTANCE)(await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",review.InstanceId))).Single();
            var creator=processRow.ROW_CREATED_BY;var created=processRow.ROW_CREATED_DATE;CopyBusiness(processRow,ConvertToProcessInstanceEntity(instance));processRow.ROW_CHANGED_BY=review.Context.ActorUserId;processRow.ROW_CHANGED_DATE=now;
            await Update(processRow,GetProcessInstanceRepository(),"PROCESS_INSTANCE_ID",review.InstanceId,creator,created);
            foreach(var changed in new[]{current,next}.OfType<ProcessStepInstance>())
            {
                var row=(PROCESS_STEP_INSTANCE)(await GetProcessStepInstanceRepository().GetAsync(Key("PROCESS_STEP_INSTANCE_ID",changed.StepInstanceId))).Single();var originalActor=row.ROW_CREATED_BY;var originalDate=row.ROW_CREATED_DATE;
                CopyBusiness(row,ConvertToProcessStepInstanceEntity(changed));row.ROW_CHANGED_BY=review.Context.ActorUserId;row.ROW_CHANGED_DATE=now;
                await Update(row,GetProcessStepInstanceRepository(),"PROCESS_STEP_INSTANCE_ID",changed.StepInstanceId,originalActor,originalDate);
            }
            var routed=next is null?new List<CROSS_PERSONA_TASK>():await router.RouteTaskInOwnedTransactionAsync(review.InstanceId,next.StepInstanceId,next.RequiredRole!,"WORK_ORDER",review.Assignment.WorkOrderId,null,review.Context.ActorUserId);
            proof=new(frozen,MaintenanceAcceptanceBusinessDigest(instance),approval,historyRow,closed,routed,next?.StepInstanceId);
            instance.ProcessData.Data[MaintenanceAcceptanceKey]=proof;processRow.INSTANCE_DATA_JSON=JsonSerializer.Serialize(instance.ProcessData);processRow.ROW_CHANGED_BY=review.Context.ActorUserId;processRow.ROW_CHANGED_DATE=now;
            await Update(processRow,GetProcessInstanceRepository(),"PROCESS_INSTANCE_ID",review.InstanceId,creator,created);
            await Confirm(proof);graph=await MaintenanceAcceptanceNativeGraph(review.InstanceId,review.ProcessId);await Guard(true);
            if(definitionSnapshot!=JsonSerializer.Serialize(await GetProcessDefinitionAsync(review.ProcessId)) || graph!=await MaintenanceAcceptanceNativeGraph(review.InstanceId,review.ProcessId))throw new InvalidOperationException("The complete receiving acceptance changed before commit.");
        });
        if(proof is null || !ReferenceEquals(source,_editor.GetDataSource(_connectionName)) || graph!=await MaintenanceAcceptanceNativeGraph(review.InstanceId,review.ProcessId))throw new InvalidOperationException("Receiving acceptance commit is unconfirmed. Retain its original intent.");
        await Confirm(proof);await Guard(false);RequireSource(await readSource());await Guard(false);await Confirm(proof);
        if(graph!=await MaintenanceAcceptanceNativeGraph(review.InstanceId,review.ProcessId))throw new InvalidOperationException("The receiving acceptance outcome changed after authority/source confirmation.");
        var receipt=new CompanyMaintenanceAcceptanceReceipt(review.Context.Company,review.Context.ActorUserId,frozen.IntentId,review.InstanceId,review.StepInstanceId,proof.History.PROCESS_HISTORY_ID,proof.Approval.PROCESS_APPROVAL_ID,proof.NextStep,
            proof.Closed.Select(task=>task.CROSS_TASK_ID).Order(StringComparer.Ordinal).ToArray(),proof.Next.Select(task=>task.CROSS_TASK_ID).Order(StringComparer.Ordinal).ToArray(),ProcessDefinitionRevision.FromJson(graph));
        receipt.Require(frozen);return receipt;
    }
}
