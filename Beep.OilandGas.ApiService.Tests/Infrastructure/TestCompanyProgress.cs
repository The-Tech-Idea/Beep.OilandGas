using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Repository;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Beep.OilandGas.ApiService.Tests.Infrastructure;

internal static class TestCompanyProgress
{
    internal static readonly CompanyScope Scope = new("test-tenant", "test-company");
    internal static IRepositoryAccessService Access()
    {
        var access = new Mock<IRepositoryAccessService>();
        access.Setup(service => service.GetCompanyAccessAsync("test-account", Scope, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RepositoryCompanyAccess(Scope, ["Admin"], []));
        return access.Object;
    }
    internal static RepositoryCompanyContext Company() => new RepositoryCompanyContextResolver(Access())
        .ResolveAsync("test-account", Scope).GetAwaiter().GetResult()!;
    internal static RepositoryTenantContext Tenant()
    {
        var tenant = new RepositoryTenantContext(); tenant.SelectCompany(Company()); return tenant;
    }
    internal static IServiceCollection Services() => new ServiceCollection().AddSingleton(Access())
        .AddScoped<RepositoryTenantContext>().AddScoped<RepositoryCompanyContextResolver>();
}
