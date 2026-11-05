using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
using System.Security.Claims;
namespace Beep.OilandGas.ApiService.Services;

public sealed partial class CompanyMaintenanceHandoffService
{
    public async Task<CompanyWorkOrderLaborUsageReview> ReviewLaborUsageAsync(ClaimsPrincipal user,CompanyWorkOrderExecutionStartReference reference)
    {
        reference.Require();var task=reference.Permit.Task;var boundary=await Boundary(user,task.WorkOrderId,task.FieldId);
        var context=await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"Maintenance.ExecuteWork");
        async Task Guard()
        {
            if(await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"Maintenance.ExecuteWork")!=context)
                throw new UnauthorizedAccessException("Current executor/company/FIELD/order authority changed around labor review.");
        }
        if(executionPermits is null)throw new InvalidOperationException("The native original work-start and labor owner is unavailable.");
        var result=await executionPermits.ReviewLaborAsync(context,reference,Guard,owned=>ReadNativeExecutionRequirementsCurrentAsync(context,reference.Permit,owned,Guard),()=>ReviewAssignedStageAsync(user,task));
        if(!user.IsInRole(result.Stage.AssignedRole))throw new UnauthorizedAccessException("The current original configured execution role is required.");
        result.Require();await Guard();return result;
    }
    public async Task<CompanyWorkOrderLaborUsageRecovery> RecordLaborUsageAsync(ClaimsPrincipal user,CompanyWorkOrderLaborUsageCommand original)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var context=command.Review.Execution.Current.Context;var reference=command.Reference();var task=reference.Start.Permit.Task;
        var boundary=await Boundary(user,task.WorkOrderId,task.FieldId);
        async Task Guard()
        {
            if(await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"Maintenance.ExecuteWork")!=context || !user.IsInRole(command.Review.Stage.AssignedRole))
                throw new UnauthorizedAccessException("Original executor/company/FIELD/order and configured execution role are required for labor reporting.");
        }
        await Guard();if(executionPermits is null)throw new InvalidOperationException("The native labor owner is unavailable.");
        return await executionPermits.RecordLaborAsync(command,Guard,owned=>ReadNativeExecutionRequirementsCurrentAsync(context,reference.Start.Permit,owned,Guard),()=>ReviewAssignedStageAsync(user,task));
    }
    public async Task<CompanyWorkOrderLaborUsageRecovery> RecoverLaborUsageAsync(ClaimsPrincipal user,CompanyWorkOrderLaborUsageReference reference)
    {
        reference.Require();var task=reference.Start.Permit.Task;var boundary=await Boundary(user,task.WorkOrderId,task.FieldId);
        var context=await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"Maintenance.ExecuteWork");
        async Task Guard()
        {
            if(await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"Maintenance.ExecuteWork")!=context)
                throw new UnauthorizedAccessException("Current original labor recovery company/account/FIELD/order authority changed.");
        }
        if(executionPermits is null)throw new InvalidOperationException("The native labor recovery owner is unavailable.");
        var result=await executionPermits.RecoverLaborAsync(context,reference,Guard);
        if(!user.IsInRole(result.Command.Review.Stage.AssignedRole))throw new UnauthorizedAccessException("The original configured execution role is required for private labor recovery.");
        result.Require(context.Company,context.ActorUserId,reference);await Guard();return result;
    }
}
