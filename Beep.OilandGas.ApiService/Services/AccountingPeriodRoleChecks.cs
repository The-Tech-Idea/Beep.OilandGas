using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.Security.Claims;
namespace Beep.OilandGas.ApiService.Services;
/// <summary>Standard ASP.NET roles must agree with freshly read app-owned selected-company roles.</summary>
public static class AccountingPeriodRoleChecks
{
    public static bool CanAct(ClaimsPrincipal user,CompanyScope expected,RepositoryCompanyAccess? current) =>
        user.Identity?.IsAuthenticated==true && current is not null && current.Scope==expected && current.Roles is not null &&
        current.Roles.Any(role=>role is "Accountant" or "Administrator" && user.IsInRole(role));
}
