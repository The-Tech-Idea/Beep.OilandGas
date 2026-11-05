using System.Security.Claims;
using Beep.OilandGas.Models.Core.Interfaces;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;

public interface ICompanyAccountingProcessReadContext
{
    Task<(RepositoryCompanyAccess Access,string Binding,string Actor,IProcessService Process)> CaptureProcessAsync(ClaimsPrincipal user);
}
