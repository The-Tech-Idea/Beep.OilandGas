using Beep.OilandGas.ApiService.Middleware;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Beep.OilandGas.Repository.Tests;

internal static class CompanyRequestTestInvoker
{
    public static async Task InvokeAsync(CompanyRequestMiddleware middleware, HttpContext http, RepositoryTenantContext tenant,
        RepositoryCompanyContextResolver companies, IRepositoryAccessService access, ModuleBindingService bindings)
    {
        using var services = new ServiceCollection().AddSingleton(tenant).AddSingleton(companies)
            .AddSingleton(access).AddSingleton(bindings).BuildServiceProvider();
        var previous = http.RequestServices;
        try { http.RequestServices = services; await middleware.InvokeAsync(http); }
        finally { http.RequestServices = previous; }
    }
}
