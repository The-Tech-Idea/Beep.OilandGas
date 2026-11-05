using Beep.OilandGas.Models.Data.AccessControl;

namespace Beep.OilandGas.Web.Services;

public sealed class CompanyAccessClient(ApiClient api) : ICompanyAccessClient
{
    public async Task<CompanyScope[]> ListAsync(CancellationToken token = default) =>
        await api.GetAsync<CompanyScope[]>("/api/auth/repository/me/companies", token) ??
        throw new InvalidOperationException("The company membership response was empty.");

    public async Task<RepositoryCompanyAccess> GetAccessAsync(CompanyScope scope, CancellationToken token = default) =>
        await api.GetAsync<RepositoryCompanyAccess>($"/api/auth/repository/me/company-access?tenantId={Uri.EscapeDataString(scope.TenantId)}&businessAssociateId={Uri.EscapeDataString(scope.BusinessAssociateId)}", token) ??
        throw new InvalidOperationException("The company access response was empty.");
}
