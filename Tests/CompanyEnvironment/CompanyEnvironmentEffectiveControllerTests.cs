using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CompanyEnvironment.Tests;

public sealed class CompanyEnvironmentEffectiveControllerTests
{
    private sealed class Access : IRepositoryAccessService
    {
        public RepositoryCompanyAccess? Value;
        public Task<RepositoryCompanyAccess?> GetCompanyAccessAsync(string userId, CompanyScope scope, CancellationToken token = default) => Task.FromResult(Value);
    }
    private sealed class Store(CompanyEnvironmentConfiguration configuration) : ICompanyEnvironmentStore
    {
        public Action? OnRead;
        public async Task<CompanyEnvironmentConfiguration?> ReadAsync(CompanyScope company, Func<Task> authority) { await authority(); OnRead?.Invoke(); return configuration; }
        public Task<CompanyEnvironmentConfiguration> SaveAsync(CompanyScope company, string actor, CompanyEnvironmentReviewedSaveRequest request, Func<Task> authority) => throw new NotSupportedException();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CompanyMemberCanReadButRevokedMembershipCannotPublish(bool revoke)
    {
        var company = new CompanyScope("tenant", "company"); var access = new Access { Value = new(company, ["Engineer"], ["read"]) };
        var resolver = new RepositoryCompanyContextResolver(access); var tenant = new RepositoryTenantContext();
        tenant.SelectCompany((await resolver.ResolveAsync("actor", company))!);
        var saved = CompanyEnvironmentIdentity.Prepare(company, new("KW", "{}", "", Guid.NewGuid().ToString("N")), CompanyEnvironmentPreviewer.ToolsVersion, null, new string('A', 64), null);
        var store = new Store(saved) { OnRead = () => { if (revoke) access.Value = null; } };
        var controller = new CompanyEnvironmentEffectiveController(tenant, resolver, new CompanyEnvironmentEffectiveResolver(store, new TestSystemDefaults()), NullLogger<CompanyEnvironmentEffectiveController>.Instance)
        { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new("party_id", "actor")], "test")) } } };
        var result = await controller.Get(default);
        if (revoke) Assert.IsType<ForbidResult>(result);
        else Assert.Equal(company, Assert.IsType<CompanyEnvironmentEffectiveReview>(Assert.IsType<OkObjectResult>(result).Value).Company);
    }
}
