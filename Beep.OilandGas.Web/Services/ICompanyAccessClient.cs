using Beep.OilandGas.Models.Data.AccessControl;

namespace Beep.OilandGas.Web.Services;

public interface ICompanyAccessClient
{
    Task<CompanyScope[]> ListAsync(CancellationToken token = default);
    Task<RepositoryCompanyAccess> GetAccessAsync(CompanyScope scope, CancellationToken token = default);
}
