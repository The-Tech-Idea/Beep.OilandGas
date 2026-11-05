using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.Core;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

public partial class PPDMProcessService : ICompanyMaintenanceAssignedStageReads
{
    public async Task<CompanyMaintenanceAssignedStageReview> ReviewMaintenanceAssignedStageAsync(FieldSelectionReview current,
        CompanyMaintenanceReceivingQuery task,Func<string?,Task> authority)
    {
        ArgumentNullException.ThrowIfNull(current);ArgumentNullException.ThrowIfNull(task);ArgumentNullException.ThrowIfNull(authority);
        FieldSelectionReviewValidator.RequireReview(current,current.Company,current.ActorUserId);task.Require();
        if(current.FieldId!=task.FieldId || current.Database.ConnectionName!=_connectionName || _personaReader is null)
            throw new UnauthorizedAccessException("The original next-role maintenance FIELD/lifecycle owner is required.");
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("The next-role maintenance datasource is unavailable.");
        PPDMDataSourceTransaction.RequireIdle(source);await authority(null);PPDMDataSourceTransaction.RequireIdle(source);
        var instance=await LoadProcessInstanceAsync(task.InstanceId)??throw new InvalidOperationException("The recorded maintenance process is unavailable.");
        var start=CompanyMaintenanceProcessStart.FromPayload(JsonSerializer.Serialize(instance.ProcessData),task.InstanceId,task.FieldId,task.WorkOrderId);
        if(start.Context.Company!=current.Company || start.Context.Database!=current.Database || instance.FieldId!=task.FieldId || instance.EntityId!=task.WorkOrderId || instance.EntityType!="WORK_ORDER")
            throw new UnauthorizedAccessException("The next-role maintenance assignment belongs to another company/source.");
        if(instance.ProcessData.Data.ContainsKey(MaintenanceClosureKey))throw new InvalidOperationException("Maintenance is closed; read the original recorded closure outcome.");
        if(instance.ProcessData.Data.ContainsKey(ReturnHandoffKey))return await ReviewReturnAssignedStageAsync(current,task,authority);
        if(instance.ProcessData.Data.ContainsKey(ExecutionHandoffKey))return await ReviewExecutionReviewerStageAsync(current,task,authority);
        if(!instance.ProcessData.Data.TryGetValue(MaintenanceAcceptanceKey,out var value))throw new InvalidOperationException("Independent receiving acceptance is not recorded.");
        MaintenanceAcceptanceEvidence evidence;
        try
        {
            evidence=JsonSerializer.Deserialize<MaintenanceAcceptanceEvidence>(JsonSerializer.Serialize(value))??throw new InvalidOperationException("The recorded receiving evidence is unavailable.");
            if(evidence.Command is null || evidence.Approval is null || evidence.History is null || evidence.Closed is null || evidence.Next is null || !ProcessDefinitionRevision.IsValid(evidence.ProcessDigest))throw new InvalidOperationException("The complete recorded receiving audit is unavailable.");
            evidence.Command.Require();
        }
        catch(JsonException malformed){throw new InvalidOperationException("The recorded receiving evidence is malformed.",malformed);}
        catch(ArgumentException malformed){throw new InvalidOperationException("The retained receiving command is malformed.",malformed);}
        var query=new CompanyMaintenanceRecoveryQuery(start.IntentId,start.Source.DecisionId,task.FieldId,start.Source.WellId,task.WorkOrderId);
        var accepted=evidence.Command.Review;
        if(accepted.InstanceId!=task.InstanceId || accepted.Context.Company!=current.Company || accepted.Assignment!=query || accepted.ProcessId!=start.ProcessId || accepted.DefinitionRevision!=start.DefinitionRevision || evidence.NextStep is null)
            throw new InvalidOperationException("The receiving evidence does not establish this current next-role assignment.");
        if(MaintenanceAcceptanceBusinessDigest(instance)!=evidence.ProcessDigest)throw new InvalidOperationException("The initial next-role process snapshot differs from its accepted graph.");
        var steps=instance.StepInstances.Where(step=>step.StepInstanceId==evidence.NextStep).ToArray();
        if(steps.Length!=1 || instance.Status!=ProcessStatus.IN_PROGRESS || instance.CurrentStepId!=steps[0].StepId || steps[0].Status is not (StepStatus.PENDING or StepStatus.IN_PROGRESS) ||
            !LeaseSummaryValidator.Identifier(steps[0].RequiredRole) || !string.IsNullOrWhiteSpace(steps[0].AssignedTo) && steps[0].AssignedTo!=current.ActorUserId && steps[0].AssignedTo!=steps[0].RequiredRole)
            throw new UnauthorizedAccessException("The recorded current step is not assigned to this next-role account.");
        var step=steps[0];var role=step.RequiredRole!;
        async Task Guard(string? _)
        {
            if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("The next-role lifecycle binding changed.");
            await authority(role);
        }
        // Reuse the complete read-only acceptance graph/audit/task verifier. Its source argument
        // is retained receiving evidence, not a claim that current production or permits are reviewed.
        // The original private command stays inside the native owner; only audit identifiers leave it.
        var receipt=await RecoverMaintenanceAcceptanceAsync(evidence.Command,Guard,()=>Task.FromResult(accepted.Source));
        await Guard(role);PPDMDataSourceTransaction.RequireIdle(source);
        var graph=await MaintenanceAcceptanceNativeGraph(task.InstanceId,start.ProcessId);
        if(receipt.NextStepInstanceId!=step.StepInstanceId || receipt.RecordedRevision!=ProcessDefinitionRevision.FromJson(graph))
            throw new InvalidOperationException("The complete next-role assignment changed during review.");
        await Guard(role);PPDMDataSourceTransaction.RequireIdle(source);
        if(graph!=await MaintenanceAcceptanceNativeGraph(task.InstanceId,start.ProcessId))throw new InvalidOperationException("The next-role graph changed after authority confirmation.");
        var result=new CompanyMaintenanceAssignedStageReview(current,task,step.StepInstanceId,step.StepId,step.StepName,role,
            accepted.Context.ActorUserId,receipt.HistoryId,receipt.ApprovalId,start.DefinitionRevision,receipt.RecordedRevision);
        result.Require(current.Company,current.ActorUserId,task);return result;
    }
}
