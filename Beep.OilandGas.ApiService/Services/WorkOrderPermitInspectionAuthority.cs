using System.Security.Claims;
using Beep.OilandGas.Models.Core.Refusals;
namespace Beep.OilandGas.ApiService.Services;
public static class WorkOrderPermitInspectionAuthority
{
    public static async Task RequireAsync(CompanyModuleConnection gateway,ClaimsPrincipal user,string actor)
    {
        gateway.RequireActor(actor);var current=await gateway.RefreshCompanyAsync();
        if(user.Identity?.IsAuthenticated!=true || user.FindActingUserId()!=actor || !current.Roles.Any(user.IsInRole) || !current.Permissions.Contains("HSE.ApprovePermit",StringComparer.Ordinal))throw RefusalException.Forbidden("Current standard app roles and HSE.ApprovePermit are required for explicit permit-prerequisite inspections.");gateway.RequireActor(actor);
    }
}
