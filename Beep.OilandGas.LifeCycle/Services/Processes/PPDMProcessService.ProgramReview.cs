using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.LifeCycle.Models;
using System.Data;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.PPDM39.Core;
namespace Beep.OilandGas.LifeCycle.Services.Processes;
public partial class PPDMProcessService
{
    private sealed class ProgramReviewPersonas(DrillingProgramReviewStart original,Func<Task> guard):IRolePersonaReader
    {
        public async Task<List<string>> GetActivePersonasAsync(string role)
        {await guard();return original.Workflow.Stages.FirstOrDefault(stage=>stage.Definition.AssignedRole==role)?.Definition.PersonaCodes.Order(StringComparer.Ordinal).ToList()??throw new InvalidOperationException("The frozen program receiving role is unavailable.");}
    }
    private static bool ProgramReviewMarker(PROCESS_INSTANCE row)=>row.ENTITY_TYPE=="DRILLING_PROGRAM" || row.PROCESS_INSTANCE_ID?.StartsWith(DrillingProgramReviewStart.InstancePrefix,StringComparison.Ordinal)==true || DrillingProgramReviewStart.HasPayload(row.INSTANCE_DATA_JSON);
    private static string ProgramReviewHistoryId(string id)=>"program-review-start-"+ProcessDefinitionRevision.FromJson(id);
    private async Task<PROCESS_INSTANCE?> FindProgramReviewAsync(CompanyScope company,string field,string program,CrossPersonaTaskRouter router)
    {
        var rows=(await GetProcessInstanceRepository().GetAsync(Key("ENTITY_ID",program))).ToArray();if(rows.Length>256 || rows.Any(row=>row is not PROCESS_INSTANCE))throw new InvalidOperationException("The complete program assignment population is ambiguous or exceeds its bound.");
        await router.RequireProgramSourceTaskParentsAsync(program,rows.Cast<PROCESS_INSTANCE>().ToArray(),_connectionName);var found=new List<PROCESS_INSTANCE>();
        foreach(var row in rows.Cast<PROCESS_INSTANCE>())
        {
            if(row.ENTITY_ID!=program)throw new InvalidOperationException("The program assignment lookup returned another source.");if(!ProgramReviewMarker(row))continue;if(DrillingProgramReviewStart.OwnerCompany(row.INSTANCE_DATA_JSON!)!=company)continue;
            if(row.ENTITY_TYPE!="DRILLING_PROGRAM" || row.FIELD_ID!=field || row.ACTIVE_IND!="Y")throw new InvalidOperationException("The company program assignment source is retagged or inactive and needs reconciliation.");var original=DrillingProgramReviewStart.FromPayload(row.INSTANCE_DATA_JSON!,row.PROCESS_INSTANCE_ID!,field,program);
            if(original.Scope().LifecycleDatabase.ConnectionName!=_connectionName)throw new InvalidOperationException("The original program review target changed.");found.Add(row);
        }
        if(found.Count>1)throw new InvalidOperationException("The saved program has multiple company review assignments.");return found.SingleOrDefault();
    }
    public async Task<(DrillingProgramReviewStart Original,ProcessInstance Process,IReadOnlyList<CROSS_PERSONA_TASK> Tasks,string NativeRevision)> ReadRecordedProgramReviewAsync(CompanyScope company,string id,Func<Task> authority)
    {
        APInvoiceOwnership.RequireScope(company);if(!LeaseSummaryValidator.Identifier(id) || !id.StartsWith(DrillingProgramReviewStart.InstancePrefix,StringComparison.Ordinal))throw new InvalidOperationException("Choose an exact recorded program review assignment.");
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("The original program review source is unavailable.");var router=new CrossPersonaTaskRouter(_editor,_commonColumnHandler,_defaults,_metadata,()=>Task.FromResult(_connectionName));
        async Task Guard(bool owned){if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("The original program review target changed.");if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);await authority();if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);}
        DrillingProgramReviewStart? original=null;ProcessInstance? process=null;IReadOnlyList<CROSS_PERSONA_TASK> tasks=[];string? graph=null;await Guard(false);
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);var rows=(await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).ToArray();if(rows.Length!=1 || rows[0] is not PROCESS_INSTANCE row)throw new InvalidOperationException("The recorded program assignment is missing or ambiguous.");
            if(DrillingProgramReviewStart.OwnerCompany(row.INSTANCE_DATA_JSON!)!=company)throw new UnauthorizedAccessException("The program assignment belongs to another selected company.");if(row.ENTITY_TYPE!="DRILLING_PROGRAM" || row.ACTIVE_IND!="Y")throw new InvalidOperationException("The original program assignment ownership changed.");original=DrillingProgramReviewStart.FromPayload(row.INSTANCE_DATA_JSON!,id,row.FIELD_ID!,row.ENTITY_ID!);
            if(original.Scope().LifecycleDatabase.ConnectionName!=_connectionName)throw new InvalidOperationException("The original program lifecycle binding changed.");process=await LoadProcessInstanceAsync(id)??throw new InvalidOperationException("The original program assignment is unavailable.");graph=await ConfirmProgramReviewGraphAsync(process,original);tasks=(await router.GetAllTasksForDevelopmentEvidenceAsync(id,_connectionName)).Select(CompanyInventorySourceReview.Copy).ToArray();await Guard(true);
            if((await FindProgramReviewAsync(company,process.FieldId,process.EntityId,router))?.PROCESS_INSTANCE_ID!=id || graph!=await ConfirmProgramReviewGraphAsync(process,original))throw new InvalidOperationException("The complete original program assignment changed during readback.");
        });
        await Guard(false);if(process is null || graph!=await ConfirmProgramReviewGraphAsync(process,original!))throw new InvalidOperationException("The original program assignment changed after its read.");await Guard(false);return(CompanyInventorySourceReview.Copy(original!),CompanyInventorySourceReview.Copy(process),CompanyInventorySourceReview.Copy(tasks),ProcessDefinitionRevision.FromJson(graph!));
    }
    public async Task<string?> FindRecordedProgramReviewAsync(CompanyScope company,string field,string program,Func<Task> authority)
    {
        APInvoiceOwnership.RequireScope(company);if(!LeaseSummaryValidator.Identifier(field) || !LeaseSummaryValidator.Identifier(program))throw new ArgumentException("The exact original FIELD and program identifiers are required.");var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("The program review source is unavailable.");var router=new CrossPersonaTaskRouter(_editor,_commonColumnHandler,_defaults,_metadata,()=>Task.FromResult(_connectionName));
        async Task Guard(bool owned){if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("The program review source changed.");if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);await authority();if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);}
        string? id=null,graph=null;ProcessInstance? process=null;DrillingProgramReviewStart? original=null;await Guard(false);
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>{await Guard(true);var row=await FindProgramReviewAsync(company,field,program,router);id=row?.PROCESS_INSTANCE_ID;if(row is not null){original=DrillingProgramReviewStart.FromPayload(row.INSTANCE_DATA_JSON!,id!,field,program);process=await LoadProcessInstanceAsync(id!)??throw new InvalidOperationException("The saved program review is unavailable.");graph=await ConfirmProgramReviewGraphAsync(process,original);}await Guard(true);if(id!=(await FindProgramReviewAsync(company,field,program,router))?.PROCESS_INSTANCE_ID)throw new InvalidOperationException("The program assignment identity changed during lookup.");});
        await Guard(false);if(id!=(await FindProgramReviewAsync(company,field,program,router))?.PROCESS_INSTANCE_ID || process is not null && graph!=await ConfirmProgramReviewGraphAsync(process,original!))throw new InvalidOperationException("The recorded program review or its confirmed absence changed.");await Guard(false);return id;
    }
    public async Task<DrillingProgramReviewStartReceipt> RecordProgramReviewStartAsync(DrillingProgramReviewStart input,Func<Task> authority,Func<Task<DrillingProgramReviewSource>> readSource,bool recover=false)
    {
        input.Require();ArgumentNullException.ThrowIfNull(authority);ArgumentNullException.ThrowIfNull(readSource);var original=CompanyInventorySourceReview.Copy(input);var id=original.InstanceId();var scope=original.Scope();if(scope.LifecycleDatabase.ConnectionName!=_connectionName)throw new InvalidOperationException("The original program lifecycle target is required.");
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("The original program review source is unavailable.");
        async Task Guard(bool owned){if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("The original program review source changed.");if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);await authority();if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);}
        var router=new CrossPersonaTaskRouter(_editor,_commonColumnHandler,_defaults,_metadata,()=>Task.FromResult(_connectionName),personas:new ProgramReviewPersonas(original,()=>Guard(true)));
        await Guard(false);original.RequireSource(await readSource(),!recover);await Guard(false);ProcessInstance? process=null;List<CROSS_PERSONA_TASK> tasks=[];string? graph=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);var rows=(await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).ToArray();if(rows.Length>1 || rows.Any(row=>row is not PROCESS_INSTANCE))throw new InvalidOperationException("The original program review identity is ambiguous.");
            if(rows.Length==0 && ((await GetProcessStepInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).Any() || (await GetProcessHistoryRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).Any() || (await GetProcessApprovalRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).Any() || (await router.GetAllTasksForDevelopmentEvidenceAsync(id,_connectionName)).Count>0))throw new InvalidOperationException("Partial program assignment evidence cannot become confirmed absence.");
            string? definitionJson=null;
            if(!recover)
            {
                var definitions=(await GetProcessDefinitionRepository().GetAsync(Key("PROCESS_DEFINITION_ID",original.Workflow.ProcessId))).ToArray();if(definitions.Length!=1 || definitions[0] is not PROCESS_DEFINITION definition || definition.ACTIVE_IND!="Y" || definition.IS_ACTIVE!="Y")throw new InvalidOperationException("One active original program workflow is required.");var configured=ConvertToProcessDefinition(definition);var stages=DevelopmentReviewDefinition.RequireProgram(configured);
                if(ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(configured))!=original.Workflow.DefinitionRevision || configured.ProcessName!=original.Workflow.ProcessName || configured.Version!=original.Workflow.Version || stages.Count!=original.Workflow.Stages.Count || stages.Where((stage,index)=>stage.StepId!=original.Workflow.Stages[index].Definition.StepId || stage.StepName!=original.Workflow.Stages[index].Definition.StepName || stage.SequenceNumber!=original.Workflow.Stages[index].Definition.Sequence || stage.RequiredRoles[0]!=original.Workflow.Stages[index].Definition.AssignedRole || stage.SlaHours!=original.Workflow.Stages[index].Definition.SlaHours).Any())throw new InvalidOperationException("The original program workflow definition changed.");definitionJson=JsonSerializer.Serialize(definition);
            }
            var existing=await FindProgramReviewAsync(scope.Company,scope.FieldId,original.Source.Program.PROGRAM_ID,router);
            if(rows.Length==1)
            {
                if(existing?.PROCESS_INSTANCE_ID!=id || DrillingProgramReviewStart.FromPayload(((PROCESS_INSTANCE)rows[0]).INSTANCE_DATA_JSON!,id,scope.FieldId,original.Source.Program.PROGRAM_ID).Revision()!=original.Revision())throw new InvalidOperationException("The original program assignment intent or source population changed.");process=await LoadProcessInstanceAsync(id)??throw new InvalidOperationException("The original program assignment is unavailable.");
            }
            else if(!recover)
            {
                if(existing is not null)throw new InvalidOperationException("This saved program already has a review assignment. Open its original recorded review.");var now=DateTime.UtcNow;var stages=original.Workflow.Stages.Select(stage=>new ProcessStepDefinition{StepId=stage.Definition.StepId,StepName=stage.Definition.StepName,SequenceNumber=stage.Definition.Sequence,RequiredRoles=[stage.Definition.AssignedRole],RequiresApproval=true,StepType="APPROVAL",SlaHours=stage.Definition.SlaHours}).ToArray();
                process=CreateInitialProcess(stages,original.Workflow.ProcessId,original.Source.Program.PROGRAM_ID,"DRILLING_PROGRAM",scope.FieldId,scope.ActorUserId,id,now,stage=>DrillingProgramReviewStartReceipt.StepId(id,stage.StepId),CompanyInventorySourceReview.Copy(new PROCESS_DATA{InstanceId=id,LastUpdated=now,Data=new(){[DrillingProgramReviewStart.PayloadKey]=original}}));
                await PersistReviewedStartAsync(process,new(){HistoryId=ProgramReviewHistoryId(id),InstanceId=id,Action="PROCESS_STARTED",NewState="INITIAL",Timestamp=now,PerformedBy=scope.ActorUserId,Notes="Saved drilling program assigned for independent configured review"});
            }
            if(process is not null)
            {
                var first=process.StepInstances.Single(step=>step.StepId==original.Workflow.Stages[0].Definition.StepId);if(rows.Length==0)await router.RouteTaskInOwnedTransactionAsync(id,first.StepInstanceId,first.RequiredRole!,"DRILLING_PROGRAM",original.Source.Program.PROGRAM_ID,original.Source.Program.PROGRAM_NAME,scope.ActorUserId);
                graph=await ConfirmProgramReviewGraphAsync(process,original);tasks=(await router.GetAllTasksForDevelopmentEvidenceAsync(id,_connectionName)).Select(CompanyInventorySourceReview.Copy).ToList();
            }
            if(definitionJson is not null){var closing=(await GetProcessDefinitionRepository().GetAsync(Key("PROCESS_DEFINITION_ID",original.Workflow.ProcessId))).ToArray();if(closing.Length!=1 || JsonSerializer.Serialize(closing[0])!=definitionJson)throw new InvalidOperationException("The original program definition changed before commit.");}
            await Guard(true);if(process is not null && graph!=await ConfirmProgramReviewGraphAsync(process,original))throw new InvalidOperationException("The complete original program assignment changed before commit.");await Guard(true);
        });
        await Guard(false);if(process is not null){if(graph!=await ConfirmProgramReviewGraphAsync(process,original))throw new InvalidOperationException("The program review commit is unconfirmed. Recover the retained original.");}else if((await GetProcessInstanceRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).Any())throw new InvalidOperationException("Original program review absence changed during recovery.");
        original.RequireSource(await readSource(),!recover);await Guard(false);var receipt=new DrillingProgramReviewStartReceipt(original.Revision(),id,process is not null,process?.StepInstances.Single(step=>step.StepId==original.Workflow.Stages[0].Definition.StepId).StepInstanceId,tasks.Where(task=>task.PROCESS_STEP_INSTANCE_ID==DrillingProgramReviewStartReceipt.StepId(id,original.Workflow.Stages[0].Definition.StepId)).Select(task=>task.CROSS_TASK_ID).Order(StringComparer.Ordinal).ToArray(),ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Intent=original.Revision(),Graph=graph,Tasks=tasks})),DateTimeOffset.UtcNow);receipt.Require(original);return receipt;
    }
    private async Task<string> ConfirmProgramReviewInitialAsync(ProcessInstance process,DrillingProgramReviewStart original,CrossPersonaTaskRouter router)
    {
        var id=original.InstanceId();var scope=original.Scope();var stages=original.Workflow.Stages;var first=stages[0].Definition;
        if(process.InstanceId!=id || process.EntityType!="DRILLING_PROGRAM" || process.EntityId!=original.Source.Program.PROGRAM_ID || process.FieldId!=scope.FieldId || process.ProcessId!=original.Workflow.ProcessId || process.StartedBy!=scope.ActorUserId || process.Status!=ProcessStatus.IN_PROGRESS || process.CurrentState!="INITIAL" || process.CurrentStepId!=first.StepId || process.CompletionDate is not null ||
            DrillingProgramReviewStart.FromPayload(JsonSerializer.Serialize(process.ProcessData),id,scope.FieldId,process.EntityId).Revision()!=original.Revision() || process.StepInstances.Count!=stages.Count || process.History.Count!=1 || process.History[0].HistoryId!=ProgramReviewHistoryId(id) || process.History[0].Action!="PROCESS_STARTED" || process.History[0].Timestamp!=process.StartDate || process.History[0].PerformedBy!=scope.ActorUserId || process.History[0].NewState!="INITIAL" || !string.IsNullOrEmpty(process.History[0].StepInstanceId))throw new InvalidOperationException("The original program assignment differs from its frozen initial graph or progressed.");
        foreach(var configured in stages)
        {
            var expected=configured.Definition;var matches=process.StepInstances.Where(step=>step.StepId==expected.StepId).ToArray();if(matches.Length!=1)throw new InvalidOperationException("The original program step identity is ambiguous.");var step=matches[0];
            if(step.StepInstanceId!=DrillingProgramReviewStartReceipt.StepId(id,expected.StepId) || step.InstanceId!=id || step.StepName!=expected.StepName || step.RequiredRole!=expected.AssignedRole || step.SequenceNumber!=expected.Sequence || !step.ApprovalRequired || step.SlaHours!=expected.SlaHours || step.Status!=(step.StepId==first.StepId?StepStatus.PENDING:StepStatus.BLOCKED) || !string.IsNullOrEmpty(step.AssignedTo) || step.StartDate is not null || step.CompletionDate is not null || !string.IsNullOrEmpty(step.CompletedBy) || step.Approvals.Count!=0)throw new InvalidOperationException("The original configured program steps changed.");
        }
        if((await GetProcessApprovalRepository().GetAsync(Key("PROCESS_INSTANCE_ID",id))).Any())throw new InvalidOperationException("An initial program review cannot contain unqualified approvals.");
        var tasks=await router.GetAllTasksForDevelopmentEvidenceAsync(id,_connectionName);if(tasks.Count!=first.PersonaCodes.Count || !tasks.Select(task=>task.TARGET_PERSONA_CODE).Order(StringComparer.Ordinal).SequenceEqual(first.PersonaCodes.Order(StringComparer.Ordinal)) || tasks.Any(task=>task.PROCESS_STEP_INSTANCE_ID!=DrillingProgramReviewStartReceipt.StepId(id,first.StepId) || task.ENTITY_TYPE!="DRILLING_PROGRAM" || task.ENTITY_ID!=process.EntityId || task.ENTITY_DESCRIPTION!=original.Source.Program.PROGRAM_NAME || task.ASSIGNED_ROLE!=first.AssignedRole || task.TASK_STATUS!="PENDING" || task.ROW_CREATED_BY!=scope.ActorUserId || task.ACTIVE_IND!="Y"))throw new InvalidOperationException("The original complete pending program task set changed.");
        foreach(var task in tasks)
        {
            var step=DrillingProgramReviewStartReceipt.StepId(id,first.StepId);var identity="task-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Process=id,Step=step,Persona=task.TARGET_PERSONA_CODE}));var context=JsonSerializer.Serialize(new{assignedRole=first.AssignedRole,entityType="DRILLING_PROGRAM",entityId=process.EntityId,processInstanceId=id,stepInstanceId=step,scopeKind="FIELD"});
            if(task.CROSS_TASK_ID!=identity || task.TASK_TYPE!="APPROVAL" || task.ROUTE!=WorkflowTaskRoute.For("FIELD",id,"DRILLING_PROGRAM") || task.TASK_CONTEXT_JSON!=context || task.DUE_DATE is not null || task.PRIORITY is <1 or >4 || task.ROW_CREATED_DATE is null || task.ROW_CHANGED_DATE is not null || !string.IsNullOrEmpty(task.ROW_CHANGED_BY))throw new InvalidOperationException("The exact pending program task identity, route, context or audit changed.");
        }
        await router.ConfirmRecordedTaskSetAsync(tasks,_connectionName);return await ConfirmStartedGraphAsync(process)+JsonSerializer.Serialize(tasks.OrderBy(task=>task.CROSS_TASK_ID,StringComparer.Ordinal));
    }
}
