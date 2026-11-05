using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;
using System.Data;
using Beep.OilandGas.PPDM39.Core;
namespace Beep.OilandGas.LifeCycle.Services.Processes;
public partial class PPDMProcessService
{
    public async Task<DrillingExecutionAssignmentConflictReview> ReadExecutionAssignmentConflictAsync(DrillingExecutionAssignment input,
        Func<Task> authority,Func<Task<DrillingExecutionStatus>> readSource)
    {
        input.Require();ArgumentNullException.ThrowIfNull(authority);ArgumentNullException.ThrowIfNull(readSource);
        var original=CompanyInventorySourceReview.Copy(input);var scope=original.Scope();
        if(scope.LifecycleDatabase.ConnectionName!=_connectionName)throw new InvalidOperationException("The original reporting lifecycle target is required.");
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("The original reporting source is unavailable.");
        var router=new CrossPersonaTaskRouter(_editor,_commonColumnHandler,_defaults,_metadata,()=>Task.FromResult(_connectionName));
        async Task Guard(bool owned)
        {
            if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("The original reporting source changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
            await authority();
            if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("The original reporting source changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
        }
        async Task Source(){await Guard(false);original.RequireSource(await readSource());await Guard(false);}
        async Task<DrillingExecutionAssignmentConflictReview> Read()
        {
            await RequireAbsentExecutionAssignmentGraphAsync(original,router);
            var row=await FindExecutionAssignmentAsync(scope.Company,scope.FieldId,original.OperationId,router)
                ??throw new InvalidOperationException("No different recorded reporting responsibility is confirmed. Recover the original outcome instead.");
            var competing=DrillingExecutionAssignment.FromPayload(row.INSTANCE_DATA_JSON!,row.PROCESS_INSTANCE_ID!,scope.FieldId,original.OperationId);
            original.RequireSource(competing.Source,false);
            var process=await LoadProcessInstanceAsync(competing.InstanceId())??throw new InvalidOperationException("The exact competing responsibility is unavailable.");
            var graph=await ConfirmExecutionAssignmentAsync(process,competing,router);
            var tasks=await router.GetAllTasksForDevelopmentEvidenceAsync(competing.InstanceId(),_connectionName);var at=DateTimeOffset.UtcNow;
            var outcome=new DrillingExecutionAssignmentReceipt(competing.Revision(),competing.InstanceId(),true,
                DrillingExecutionAssignmentReceipt.StepId(competing.InstanceId(),competing.Workflow.Step.StepId),
                tasks.Select(task=>task.CROSS_TASK_ID).Order(StringComparer.Ordinal).ToArray(),ProcessDefinitionRevision.FromJson(graph),at);
            var result=new DrillingExecutionAssignmentConflictReview(original.Revision(),original.InstanceId(),true,competing,outcome,"",at);
            result=result with{Revision=result.ContentRevision()};result.Require(original);return result;
        }
        await Source();DrillingExecutionAssignmentConflictReview? result=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>{
            await Guard(true);result=await Read();await Guard(true);
            if(result.Revision!=(await Read()).Revision)throw new InvalidOperationException("The competing responsibility or original absence changed during inspection.");
            await Guard(true);
        });
        await Guard(false);
        if(result!.Revision!=(await Read()).Revision)throw new InvalidOperationException("The competing responsibility changed after inspection.");
        await Source();var closing=await Read();
        if(result.Revision!=closing.Revision)throw new InvalidOperationException("The original absence or competing responsibility changed around closing source review.");
        await Guard(false);return CompanyInventorySourceReview.Copy(closing);
    }
}
