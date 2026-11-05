using System.Security.Claims;
using Beep.Foundation.IdentityServer.Shared.Identity;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Data.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class HseDecisionPolicyTests
{
    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    public async Task CompanyPermissionPolicyRequiresAnActiveAccount(bool active, bool permission, bool allowed)
    {
        using var services = new ServiceCollection().AddLogging().AddAuthorization(RepositoryAuthorization.Configure).BuildServiceProvider();
        var claims = new List<Claim> { new(PartyIdClaimsTransformation<string>.ClaimType, "actor"), new(ClaimTypes.Role, "CompanySafetyLead") };
        if (active) claims.Add(new(RepositoryRolesClaimsTransformation.ActiveAccount, "true"));
        if (permission) claims.Add(new("permission", PermissionConstants.HSE.ManageIncidents));
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test", ClaimTypes.Name, ClaimTypes.Role));
        var result = await services.GetRequiredService<IAuthorizationService>().AuthorizeAsync(user, null, PermissionConstants.HSE.ManageIncidents);
        Assert.Equal(allowed, result.Succeeded);
    }

    [Theory]
    [InlineData(true, "HSE.ReportIncident", true)]
    [InlineData(true, "HSE.ManageIncidents", false)]
    [InlineData(false, "HSE.ReportIncident", false)]
    public async Task IncidentReportingRequiresItsOwnPermissionAndAnActiveAccount(bool active, string permission, bool allowed)
    {
        using var services = new ServiceCollection().AddLogging().AddAuthorization(RepositoryAuthorization.Configure).BuildServiceProvider();
        var claims = new List<Claim> { new(PartyIdClaimsTransformation<string>.ClaimType, "actor"), new("permission", permission) };
        if (active) claims.Add(new(RepositoryRolesClaimsTransformation.ActiveAccount, "true"));
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var result = await services.GetRequiredService<IAuthorizationService>().AuthorizeAsync(user, null, PermissionConstants.HSE.ReportIncident);
        Assert.Equal(allowed, result.Succeeded);
    }
}
