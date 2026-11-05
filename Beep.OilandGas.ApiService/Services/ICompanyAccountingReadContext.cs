using Beep.OilandGas.Models.Data.AccessControl;
namespace Beep.OilandGas.ApiService.Services;

/// <summary>Fresh app-owned company/account access and the current PRODUCTION binding for accounting source reads.</summary>
public interface ICompanyAccountingReadContext
{
    Task<(RepositoryCompanyAccess Access,string Binding,string Actor)> CaptureAsync(System.Security.Claims.ClaimsPrincipal user);
}
