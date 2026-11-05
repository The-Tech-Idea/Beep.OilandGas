using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.ProductionOperations.Models;
using System.Security.Claims;
using System.Text.Json;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanyMaintenanceHandoffService
{
    private async Task<(FieldSelectionReview Lifecycle,FieldSelectionReview Core,ICompanyMaintenanceExecutionHandoffOwner Owner,Func<string?,Task> Guard)> ExecutionHandoffAuthority(ClaimsPrincipal user,CompanyWorkOrderCompletionQuery query)
    {
        query.Require();var boundary=await Boundary(user,query.Task.WorkOrderId,query.Task.FieldId);var core=await PermitAuthority(user,boundary.Preference,query.Task.WorkOrderId,"Maintenance.ExecuteWork");
        if(boundary.Process is not ICompanyMaintenanceExecutionHandoffOwner owner || executionPermits is null)throw new InvalidOperationException("Existing report and configured lifecycle handoff owners are required.");
        async Task Guard(string? role)
        {
            var access=await company.RefreshCompanyAsync();
            if(!access.Permissions.Contains("Workflow.Execute",StringComparer.Ordinal) || !access.Permissions.Contains("Maintenance.ExecuteWork",StringComparer.Ordinal) || role is not null && (!user.IsInRole(role) || !access.Roles.Contains(role,StringComparer.Ordinal)) ||
                await Authority(user,boundary.Preference,query.Task.WorkOrderId)!=boundary.Context || await PermitAuthority(user,boundary.Preference,query.Task.WorkOrderId,"Maintenance.ExecuteWork")!=core)
                throw new UnauthorizedAccessException("Original executor/company/FIELD/order/role and explicit workflow handoff authority changed.");
        }
        await Guard(null);return(boundary.Context,core,owner,Guard);
    }
    public async Task<CompanyMaintenanceExecutionHandoffReview> ReviewExecutionHandoffAsync(ClaimsPrincipal user,CompanyWorkOrderCompletionQuery query)
    {
        var captured=await ExecutionHandoffAuthority(user,query);var report=await executionPermits!.ReadSubmissionFactsAsync(captured.Core,query,()=>captured.Guard(null));
        if(report.ExecutorUserId!=captured.Core.ActorUserId)throw new UnauthorizedAccessException("Only the original executor can hand off this report.");
        var stage=await ReviewAssignedStageAsync(user,query.Task);if(stage.Context!=captured.Lifecycle)throw new UnauthorizedAccessException("Original lifecycle authority differs.");
        var result=await captured.Owner.ReviewExecutionHandoffAsync(stage,report,captured.Guard);var closing=await executionPermits.ReadSubmissionFactsAsync(captured.Core,query,()=>captured.Guard(stage.AssignedRole));
        if(JsonSerializer.Serialize(report)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Original report changed around configured handoff review.");await captured.Guard(stage.AssignedRole);result.Require();return result;
    }
    public async Task<CompanyMaintenanceExecutionHandoffRecovery> RecordExecutionHandoffAsync(ClaimsPrincipal user,CompanyMaintenanceExecutionHandoffCommand original)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var captured=await ExecutionHandoffAuthority(user,command.Review.Report.Query);
        if(command.Review.Stage.Context!=captured.Lifecycle || command.Review.Report.Database!=captured.Core.Database || command.Review.Report.ExecutorUserId!=captured.Core.ActorUserId)throw new UnauthorizedAccessException("Original report handoff executor and native bindings are required.");
        var result=await captured.Owner.RecordExecutionHandoffAsync(command,captured.Guard,owned=>executionPermits!.ReadSubmissionHandoffProofAsync(captured.Core,command.Review.Report.Query,captured.Lifecycle.Database.ConnectionName,owned,()=>captured.Guard(command.Review.Stage.AssignedRole)));
        result.Require(captured.Lifecycle.Company,captured.Lifecycle.ActorUserId,command.Reference());await captured.Guard(command.Review.Stage.AssignedRole);return result;
    }
    public async Task<CompanyMaintenanceExecutionHandoffRecovery> RecoverExecutionHandoffAsync(ClaimsPrincipal user,CompanyMaintenanceExecutionHandoffReference reference)
    {
        reference.Require();var captured=await ExecutionHandoffAuthority(user,reference.Query);var result=await captured.Owner.RecoverExecutionHandoffAsync(captured.Lifecycle,reference,captured.Guard);result.Require(captured.Lifecycle.Company,captured.Lifecycle.ActorUserId,reference);await captured.Guard(result.Command.Review.Stage.AssignedRole);return result;
    }
}
