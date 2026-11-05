using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.WorkOrder;
using System.Data;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
namespace Beep.OilandGas.LifeCycle.Services.Processes;
public partial class PPDMProcessService:ICompanyMaintenanceClosedReturnOwner
{
    public async Task<CompanyMaintenanceClosedReturnSource?> ReadClosedReturnSourceAsync(FieldSelectionReview context,CompanyWorkOrderCompletionQuery query,string coreConnection,bool ownedCore,Func<string?,Task> authority)
    {
        query.Require();FieldSelectionReviewValidator.RequireReview(context,context.Company,context.ActorUserId);if(context.Database.ConnectionName!=_connectionName || context.FieldId!=query.Task.FieldId || !LeaseSummaryValidator.Identifier(coreConnection))throw new UnauthorizedAccessException("Original company/FIELD/lifecycle and CORE source bindings are required.");var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("Original closed lifecycle source is unavailable.");var core=_editor.GetDataSource(coreConnection)??throw new InvalidOperationException("Original asset CORE source is unavailable.");
        async Task Guard(string? role){if(ownedCore)PPDMDataSourceTransaction.RequireOwned(core,IsolationLevel.Serializable);else{PPDMDataSourceTransaction.RequireIdle(core);PPDMDataSourceTransaction.RequireIdle(source);}await authority(role);if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)) || !ReferenceEquals(core,_editor.GetDataSource(coreConnection)))throw new InvalidOperationException("Original closed lifecycle/CORE pointers changed.");}
        await Guard(null);if(ownedCore && !ReferenceEquals(source,core))return null;
        var proof=await ReadMaintenanceClosureEvidence(context,query,false)??throw new InvalidOperationException("Original recorded terminal maintenance closure is required before asset return.");var first=await ReadExecutionHandoffOwner(context,query.Task);var graph=await MaintenanceAcceptanceNativeGraph(query.Task.InstanceId,first.Start.ProcessId);var stage=proof.Command.Review.Stage;var step=first.Instance.StepInstances.Single(row=>row.StepInstanceId==stage.StepInstanceId);
        if(!string.IsNullOrWhiteSpace(step.AssignedTo) && step.AssignedTo!=context.ActorUserId && step.AssignedTo!=stage.AssignedRole)throw new UnauthorizedAccessException("The original explicitly assigned return duty belongs to another account.");await Guard(stage.AssignedRole);
        var closing=await ReadMaintenanceClosureEvidence(context,query,false);if(JsonSerializer.Serialize(proof)!=JsonSerializer.Serialize(closing) || graph!=await MaintenanceAcceptanceNativeGraph(query.Task.InstanceId,first.Start.ProcessId))throw new InvalidOperationException("Original recorded closure/configured return source changed during observation.");
        var assignment=proof.Command.Review.Current.Assignment with{Stage=stage with{Context=context,ProcessRevision=ProcessDefinitionRevision.FromJson(graph)}};var result=new CompanyMaintenanceClosedReturnSource(assignment,CompanyMaintenanceClosureFacts.From(MaintenanceClosureResult(proof)));result.Require();await Guard(stage.AssignedRole);return result;
    }
}
