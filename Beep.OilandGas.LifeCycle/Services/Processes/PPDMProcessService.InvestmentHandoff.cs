using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;
using System.Data;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.PPDM39.Core;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

public partial class PPDMProcessService
{
    private static bool InvestmentHandoffMarker(PROCESS_INSTANCE row)=>row.ENTITY_TYPE=="AFE" || row.PROCESS_INSTANCE_ID?.StartsWith(DevelopmentInvestmentHandoff.InstancePrefix,StringComparison.Ordinal)==true || DevelopmentInvestmentHandoff.HasPayload(row.INSTANCE_DATA_JSON);
    private static void RejectUnqualifiedInvestmentMutation(ProcessInstance? process)
    {
        RequireReviewedHseMutation(process);
        if(process is not null && (process.EntityType=="FACILITY" || process.ProcessId is "FACILITY_DEVELOPMENT" or "WELL_DEVELOPMENT"))
            throw new InvalidOperationException("Construction and production handover require a source-bound evidence owner.");
        if(process is not null && (process.InstanceId.StartsWith(DrillingCompletionReviewStart.InstancePrefix,StringComparison.Ordinal) || process.ProcessData?.Data?.ContainsKey(DrillingCompletionReviewStart.PayloadKey)==true))
            throw new InvalidOperationException("Completion review requires its canonical original source owner even after retagging.");
        if(process is not null && (process.EntityType is "AFE" or "DRILLING_PROGRAM" or "DRILLING_OPERATION" or "DRILLING_DAILY_REPORT" || process.InstanceId.StartsWith(DrillingExecutionAssignment.InstancePrefix,StringComparison.Ordinal) || process.ProcessData?.Data?.ContainsKey(DrillingExecutionAssignment.PayloadKey)==true || process.InstanceId.StartsWith(DrillingProgramReviewStart.InstancePrefix,StringComparison.Ordinal) || process.ProcessData?.Data?.ContainsKey(DrillingProgramReviewStart.PayloadKey)==true || process.InstanceId.StartsWith(DevelopmentInvestmentHandoff.InstancePrefix,StringComparison.Ordinal) ||
            process.ProcessData?.Data?.ContainsKey(DevelopmentInvestmentHandoff.PayloadKey)==true))
            throw new InvalidOperationException("Source-bound investment assignments require their canonical handoff and financial-decision owners.");
    }
    private async Task RejectUnqualifiedInvestmentMutationAsync(string? instance)
    {if(!string.IsNullOrWhiteSpace(instance))RejectUnqualifiedInvestmentMutation(await LoadProcessInstanceAsync(instance));}
    private sealed class InvestmentHandoffPersonas(DevelopmentInvestmentHandoff original,Func<Task> guard):IRolePersonaReader
    {
        public async Task<List<string>> GetActivePersonasAsync(string role)
        {await guard();return original.Option().Stages.FirstOrDefault(stage=>stage.AssignedRole==role)?.Recipients.Select(recipient=>recipient.PersonaCode).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList()??throw new InvalidOperationException("The original investment receiving role is unavailable.");}
    }
    public Task<DevelopmentInvestmentHandoffReceipt> StartInvestmentHandoffAsync(DevelopmentInvestmentHandoff original,Func<Task> authority,Func<Task<DevelopmentInvestmentReview>> readSource)
        =>ReadOrStartInvestmentHandoff(original,authority,readSource,false);
    public Task<DevelopmentInvestmentHandoffReceipt> RecoverInvestmentHandoffAsync(DevelopmentInvestmentHandoff original,Func<Task> authority,Func<Task<DevelopmentInvestmentReview>> readSource)
        =>ReadOrStartInvestmentHandoff(original,authority,readSource,true);
    public async Task<(DevelopmentInvestmentHandoff Original,DateTime Started,IReadOnlyList<CROSS_PERSONA_TASK> Tasks,ProcessInstance Process)?> ReadRecordedInvestmentHandoffAsync(CompanyScope company,string field,string afe,Func<Task> authority)
    {
        APInvoiceOwnership.RequireScope(company);if(!LeaseSummaryValidator.Identifier(field) || !LeaseSummaryValidator.Identifier(afe))throw new ArgumentException("The exact recorded investment FIELD and AFE are required.");
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("The original investment lifecycle source is unavailable.");
        async Task Guard(bool owned)
        {
            if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("The original investment lifecycle source changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);await authority();
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
        }
        async Task<string?> Find()
        {
            var rows=(await GetProcessInstanceRepository().GetAsync(Key("ENTITY_ID",afe))).ToArray();
            if(rows.Length>256 || rows.Any(value=>value is not PROCESS_INSTANCE))throw new InvalidOperationException("The complete investment assignment source population is ambiguous or exceeds its bound.");
            await new CrossPersonaTaskRouter(_editor,_commonColumnHandler,_defaults,_metadata,()=>Task.FromResult(_connectionName)).RequireInvestmentSourceTaskParentsAsync(afe,rows.Cast<PROCESS_INSTANCE>().ToArray(),_connectionName);
            var ids=new List<string>();foreach(var row in rows.Cast<PROCESS_INSTANCE>())
            {
                if(row.ENTITY_ID!=afe)throw new InvalidOperationException("The investment assignment lookup returned another source.");if(!InvestmentHandoffMarker(row))continue;
                if(DevelopmentInvestmentHandoff.OwnerCompany(row.INSTANCE_DATA_JSON!)!=company)continue;
                if(row.ENTITY_TYPE!="AFE" || row.FIELD_ID!=field)throw new InvalidOperationException("The original company investment assignment was retagged to another source kind or FIELD.");
                if(row.ACTIVE_IND!="Y")throw new InvalidOperationException("The recorded investment assignment is inactive and needs reconciliation.");
                ids.Add(row.PROCESS_INSTANCE_ID!);
            }
            if(ids.Count>1 || ids.Any(id=>!LeaseSummaryValidator.Identifier(id)))throw new InvalidOperationException("The original company AFE has multiple or invalid assignment identities.");return ids.SingleOrDefault();
        }
        await Guard(false);string? id=null,graph=null;ProcessInstance? instance=null;DevelopmentInvestmentHandoff? original=null;List<CROSS_PERSONA_TASK> tasks=[];CrossPersonaTaskRouter? router=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);id=await Find();
            if(id is not null)
            {
                instance=await LoadProcessInstanceAsync(id)??throw new InvalidOperationException("The recorded investment assignment is unavailable.");
                original=DevelopmentInvestmentHandoff.FromPayload(JsonSerializer.Serialize(instance.ProcessData),id,field,afe);
                if(original.Scope().Company!=company || original.Scope().LifecycleDatabase.ConnectionName!=_connectionName)throw new UnauthorizedAccessException("The investment assignment belongs to another original company or target.");
                graph=await ConfirmInvestmentGraph(instance,original);
                router=new CrossPersonaTaskRouter(_editor,_commonColumnHandler,_defaults,_metadata,()=>Task.FromResult(_connectionName),personas:new InvestmentHandoffPersonas(original,()=>Guard(true)));
                tasks=(await router.GetAllTasksForDevelopmentEvidenceAsync(id,_connectionName)).Select(CompanyInventorySourceReview.Copy).ToList();
                if(instance.Status==ProcessStatus.IN_PROGRESS)
                {
                    var active=instance.StepInstances.Single(stage=>stage.StepId==instance.CurrentStepId);var creator=InvestmentProofs(instance).LastOrDefault()?.Command.Scope().ActorUserId??original.Scope().ActorUserId;
                    await router.ReadTaskSetInOwnedTransactionAsync(id,active.StepInstanceId,active.RequiredRole!,"AFE",afe,creator);
                }
            }
            await Guard(true);if(id!=await Find() || instance is not null && graph!=await ConfirmInvestmentGraph(instance,original!))throw new InvalidOperationException("The investment assignment or confirmed absence changed during the read.");await Guard(true);
        });
        await Guard(false);if(id!=await Find())throw new InvalidOperationException("The original investment assignment or its absence changed after the read.");
        if(instance is null)return null;
        if(graph!=await ConfirmInvestmentGraph(instance,original!))throw new InvalidOperationException("The original investment assignment graph changed after the read.");
        var complete=await router.GetAllTasksForDevelopmentEvidenceAsync(id!,_connectionName);
        if(JsonSerializer.Serialize(complete.OrderBy(task=>task.CROSS_TASK_ID,StringComparer.Ordinal))!=JsonSerializer.Serialize(tasks.OrderBy(task=>task.CROSS_TASK_ID,StringComparer.Ordinal)))throw new InvalidOperationException("The complete recorded investment task set changed after the read.");
        await Guard(false);return(CompanyInventorySourceReview.Copy(original!),instance.StartDate,tasks.Select(CompanyInventorySourceReview.Copy).ToArray(),CompanyInventorySourceReview.Copy(instance));
    }
    private async Task<DevelopmentInvestmentHandoffReceipt> ReadOrStartInvestmentHandoff(DevelopmentInvestmentHandoff submission,Func<Task> authority,Func<Task<DevelopmentInvestmentReview>> readSource,bool readOnly)
    {
        submission.Require();ArgumentNullException.ThrowIfNull(authority);ArgumentNullException.ThrowIfNull(readSource);
        var original=CompanyInventorySourceReview.Copy(submission);var scope=original.Scope();var id=original.InstanceId();var afe=original.Review.Source.AfeId;
        if(_connectionName!=scope.LifecycleDatabase.ConnectionName)throw new InvalidOperationException("The original investment lifecycle target is required.");
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("The investment lifecycle source is unavailable.");
        async Task Guard(bool owned)
        {
            if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("The original investment lifecycle source changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            await authority();
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
        }
        await Guard(false);original.RequireSource(await readSource());await Guard(false);
        var router=new CrossPersonaTaskRouter(_editor,_commonColumnHandler,_defaults,_metadata,()=>Task.FromResult(_connectionName),personas:new InvestmentHandoffPersonas(original,()=>Guard(true)));
        ProcessInstance? instance=null;List<CROSS_PERSONA_TASK> tasks=[];string? graph=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);var rows=(await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).ToArray();
            if(rows.Length>1 || rows.Any(row=>row is not PROCESS_INSTANCE))throw new InvalidOperationException("The original investment assignment identity is ambiguous.");
            if(rows.Length==0 && ((await GetProcessStepInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).Any() ||
                (await GetProcessHistoryRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).Any() || (await GetProcessApprovalRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).Any() ||
                (await router.GetAllTasksForDevelopmentEvidenceAsync(id,_connectionName)).Count>0))throw new InvalidOperationException("Partial original investment assignment evidence requires reconciliation; absence was not confirmed.");
            string? definitionJson=null;
            if(!readOnly)
            {
                var definitions=(await GetProcessDefinitionRepository().GetAsync(Key("PROCESS_DEFINITION_ID",original.Workflow.ProcessId))).ToArray();
                if(definitions.Length!=1 || definitions[0] is not PROCESS_DEFINITION definition || definition.ACTIVE_IND!="Y" || definition.IS_ACTIVE!="Y")throw new InvalidOperationException("One active configured investment workflow is required.");
                var configured=ConvertToProcessDefinition(definition);var stages=DevelopmentReviewDefinition.RequireInvestment(configured);
                if(configured.ProcessId!=original.Workflow.ProcessId || configured.ProcessName!=original.Workflow.ProcessName || configured.Version!=original.Workflow.Version ||
                    ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(configured))!=original.Workflow.DefinitionRevision || stages.Count!=original.Workflow.Steps.Count ||
                    stages.Where((stage,index)=>stage.StepId!=original.Workflow.Steps[index].StepId || stage.StepName!=original.Workflow.Steps[index].StepName || stage.SequenceNumber!=original.Workflow.Steps[index].Sequence ||
                        stage.RequiredRoles[0]!=original.Workflow.Steps[index].AssignedRole || stage.SlaHours!=original.Workflow.Steps[index].SlaHours).Any())throw new InvalidOperationException("The original investment workflow definition changed.");
                definitionJson=JsonSerializer.Serialize(definition);
            }
            if(rows.Length==1)
            {
                if(DevelopmentInvestmentHandoff.OwnerCompany(((PROCESS_INSTANCE)rows[0]).INSTANCE_DATA_JSON!)!=scope.Company)throw new UnauthorizedAccessException("The original investment assignment belongs to another company.");
                if(((PROCESS_INSTANCE)rows[0]).ACTIVE_IND!="Y")throw new InvalidOperationException("The original investment assignment is inactive and needs reconciliation.");
                instance=await LoadProcessInstanceAsync(id)??throw new InvalidOperationException("The original investment assignment is unavailable.");
                graph=await ConfirmInvestmentGraph(instance,original);
            }
            else if(!readOnly)
            {
                var others=(await GetProcessInstanceRepository().GetAsync(Key("ENTITY_ID",afe))).ToArray();
                if(others.Length>256 || others.Any(value=>value is not PROCESS_INSTANCE))throw new InvalidOperationException("The complete investment assignment source population is ambiguous or exceeds its bound.");
                await router.RequireInvestmentSourceTaskParentsAsync(afe,others.Cast<PROCESS_INSTANCE>().ToArray(),_connectionName);
                foreach(var value in others)
                {
                    if(value is not PROCESS_INSTANCE row || row.ENTITY_ID!=afe)throw new InvalidOperationException("The complete investment source assignment ownership is ambiguous.");if(!InvestmentHandoffMarker(row))continue;
                    if(DevelopmentInvestmentHandoff.OwnerCompany(row.INSTANCE_DATA_JSON!)!=scope.Company)continue;
                    if(row.ENTITY_TYPE!="AFE" || row.FIELD_ID!=scope.FieldId)throw new InvalidOperationException("The original company investment assignment source was retagged and needs reconciliation.");
                    DevelopmentInvestmentHandoff.FromPayload(row.INSTANCE_DATA_JSON!,row.PROCESS_INSTANCE_ID!,scope.FieldId,afe);
                    throw new InvalidOperationException("This company AFE already has an original review assignment. Recover it before requesting another.");
                }
                var now=DateTime.UtcNow;var stages=original.Workflow.Steps.Select(stage=>new ProcessStepDefinition{StepId=stage.StepId,StepName=stage.StepName,SequenceNumber=stage.Sequence,
                    RequiredRoles=[stage.AssignedRole],RequiresApproval=true,StepType="APPROVAL",SlaHours=stage.SlaHours}).ToArray();
                var payload=CompanyInventorySourceReview.Copy(new PROCESS_DATA{InstanceId=id,LastUpdated=now,Data=new(){[DevelopmentInvestmentHandoff.PayloadKey]=original}});
                instance=CreateInitialProcess(stages,original.Workflow.ProcessId,afe,"AFE",scope.FieldId,scope.ActorUserId,id,now,stage=>InvestmentHandoffStepId(id,stage.StepId),payload);
                var history=new ProcessHistoryEntry{HistoryId=InvestmentHandoffHistoryId(id),InstanceId=id,Action="PROCESS_STARTED",NewState="INITIAL",Timestamp=now,PerformedBy=scope.ActorUserId,Notes="Recorded source-bound investment request assigned for independent review"};
                await PersistReviewedStartAsync(instance,history);
            }
            if(instance is not null)
            {
                var first=instance.StepInstances.Single(stage=>stage.StepId==original.Workflow.Steps[0].StepId);
                if(rows.Length==0){tasks=await router.RouteTaskInOwnedTransactionAsync(id,first.StepInstanceId,first.RequiredRole!,"AFE",afe,null,scope.ActorUserId);await router.ConfirmRecordedTaskSetAsync(tasks,_connectionName);}
                else tasks=(await router.GetAllTasksForDevelopmentEvidenceAsync(id,_connectionName)).Where(task=>task.PROCESS_STEP_INSTANCE_ID==first.StepInstanceId).Select(CompanyInventorySourceReview.Copy).ToList();
                if(tasks.Count!=original.Workflow.Steps[0].PersonaCodes.Count)throw new InvalidOperationException("The complete original first-stage investment destinations are unavailable.");
                graph=await ConfirmInvestmentGraph(instance,original);
            }
            if(definitionJson is not null)
            {
                var after=(await GetProcessDefinitionRepository().GetAsync(Key("PROCESS_DEFINITION_ID",original.Workflow.ProcessId))).ToArray();
                if(after.Length!=1 || JsonSerializer.Serialize(after[0])!=definitionJson)throw new InvalidOperationException("The original investment workflow changed before assignment commit.");
            }
            await Guard(true);if(instance is not null && graph!=await ConfirmInvestmentGraph(instance,original))throw new InvalidOperationException("The original investment assignment changed before commit.");
            await Guard(true);
        });
        await Guard(false);
        if(instance is not null)
        {
            if(graph!=await ConfirmInvestmentGraph(instance,original))throw new InvalidOperationException("The investment assignment commit is unconfirmed. Recover its retained original.");
        }
        else if((await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).Any())throw new InvalidOperationException("The original investment assignment absence changed during recovery.");
        original.RequireSource(await readSource());await Guard(false);
        var result=new DevelopmentInvestmentHandoffReceipt(original.Revision(),id,instance is not null,instance?.StepInstances.Single(stage=>stage.StepId==original.Workflow.Steps[0].StepId).StepInstanceId,
            tasks.Select(task=>task.CROSS_TASK_ID).Order(StringComparer.Ordinal).ToArray(),ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Intent=original.Revision(),Graph=graph,Tasks=tasks})),DateTimeOffset.UtcNow);result.Require(original);return result;
    }
    private static string InvestmentHandoffStepId(string instance,string step)=>"investment-step-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{instance,step}));
    private static string InvestmentHandoffHistoryId(string instance)=>"investment-start-"+ProcessDefinitionRevision.FromJson(instance);
    private async Task<string> ConfirmInvestmentInitialGraph(ProcessInstance process,DevelopmentInvestmentHandoff original)
    {
        var id=original.InstanceId();var scope=original.Scope();
        if(process.InstanceId!=id || process.EntityType!="AFE" || process.EntityId!=original.Review.Source.AfeId || process.FieldId!=scope.FieldId || process.ProcessId!=original.Workflow.ProcessId ||
            process.StartedBy!=scope.ActorUserId || process.Status!=ProcessStatus.IN_PROGRESS || process.CurrentState!="INITIAL" || process.CurrentStepId!=original.Workflow.Steps[0].StepId || process.CompletionDate is not null ||
            DevelopmentInvestmentHandoff.FromPayload(JsonSerializer.Serialize(process.ProcessData),id,scope.FieldId,process.EntityId).Revision()!=original.Revision() ||
            process.StepInstances.Count!=original.Workflow.Steps.Count || process.History.Count!=1 || process.History[0].HistoryId!=InvestmentHandoffHistoryId(id) || process.History[0].Action!="PROCESS_STARTED" ||
            process.History[0].PerformedBy!=scope.ActorUserId || process.History[0].Timestamp!=process.StartDate || process.History[0].NewState!="INITIAL" || !string.IsNullOrEmpty(process.History[0].StepInstanceId))
            throw new InvalidOperationException("The original investment handoff ownership, source or initial graph changed or progressed.");
        foreach(var expected in original.Workflow.Steps)
        {
            var matches=process.StepInstances.Where(stage=>stage.StepId==expected.StepId).ToArray();
            if(matches.Length!=1 || matches[0].StepInstanceId!=InvestmentHandoffStepId(id,expected.StepId) || matches[0].InstanceId!=id || matches[0].StepName!=expected.StepName ||
                matches[0].SequenceNumber!=expected.Sequence || matches[0].RequiredRole!=expected.AssignedRole || !matches[0].ApprovalRequired || matches[0].SlaHours!=expected.SlaHours ||
                matches[0].Status!=(expected.StepId==process.CurrentStepId?StepStatus.PENDING:StepStatus.BLOCKED) || !string.IsNullOrEmpty(matches[0].AssignedTo) ||
                matches[0].StartDate is not null || matches[0].CompletionDate is not null || !string.IsNullOrEmpty(matches[0].CompletedBy) || matches[0].Approvals.Count!=0)
                throw new InvalidOperationException("The original investment assignment step differs from its frozen workflow.");
        }
        if((await GetProcessApprovalRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).Any())throw new InvalidOperationException("An initial investment assignment cannot contain an unqualified lifecycle approval.");
        return await ConfirmStartedGraphAsync(process);
    }
}
