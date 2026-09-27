using System.Security.Claims;
using Beep.Foundation.IdentityServer.Shared.Identity;
using Beep.OilandGas.ApiService.Controllers;
using Beep.OilandGas.ApiService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

/// <summary>
/// <see cref="RepositoryAuthorization"/>: every endpoint admits an active OilGas account — both of its claims issued by
/// this API — and <c>/api/auth/repository/me</c> admits anybody signed in, so it can say what state their account is in.
/// </summary>
public class RepositoryAuthorizationTests
{
    private const string Issuer = "https://idp.oilgas.test/";

    [Theory]
    [InlineData("active", true)]
    [InlineData("inactive", false)]
    [InlineData("unresolved", false)]
    [InlineData("claims-from-a-token", false)]
    [InlineData("inactive-with-a-token-s-marker", false)]
    [InlineData("anonymous", false)]
    public async Task Business_endpoints_admit_only_an_active_account_this_API_resolved(string caller, bool allowed)
    {
        using var services = CreateServices();
        var policies = services.GetRequiredService<IAuthorizationPolicyProvider>();
        var authorization = services.GetRequiredService<IAuthorizationService>();

        var defaultPolicy = await AuthorizationPolicy.CombineAsync(policies, [new AuthorizeAttribute()]);
        var fallbackPolicy = await policies.GetFallbackPolicyAsync();

        Assert.Equal(allowed, (await authorization.AuthorizeAsync(Caller(caller), null, defaultPolicy!)).Succeeded);
        Assert.Equal(allowed, (await authorization.AuthorizeAsync(Caller(caller), null, fallbackPolicy!)).Succeeded);
    }

    [Theory]
    [InlineData("active", true)]
    [InlineData("inactive", true)]
    [InlineData("unresolved", true)]
    [InlineData("anonymous", false)]
    public async Task The_account_lookup_admits_anybody_signed_in(string caller, bool allowed)
    {
        using var services = CreateServices();
        var attributes = typeof(RepositoryAccountController).GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>();
        var policy = await AuthorizationPolicy.CombineAsync(services.GetRequiredService<IAuthorizationPolicyProvider>(), attributes);

        Assert.Equal(allowed, (await services.GetRequiredService<IAuthorizationService>()
            .AuthorizeAsync(Caller(caller), null, policy!)).Succeeded);
    }

    [Theory]
    [InlineData("Admin.ManageUsers")]
    [InlineData("Admin.AssignRoles")]
    public async Task Administration_needs_the_repository_s_Administrator_role_on_an_active_account(string name)
    {
        using var services = CreateServices();
        var authorization = services.GetRequiredService<IAuthorizationService>();

        Assert.True((await authorization.AuthorizeAsync(Caller("active", "Administrator"), null, name)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(Caller("active"), null, name)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(Caller("inactive", "Administrator"), null, name)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(Caller("claims-from-a-token", "Administrator"), null, name)).Succeeded);
    }

    private static ServiceProvider CreateServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization(RepositoryAuthorization.Configure);
        return services.BuildServiceProvider();
    }

    /// <summary>
    /// A caller after the API's transformations: <c>active</c> carries the account and the active marker issued here;
    /// <c>inactive</c> the account only; <c>unresolved</c> neither; <c>claims-from-a-token</c> both, issued by the identity
    /// server rather than this API; <c>inactive-with-a-token-s-marker</c> the account this API resolved and an active marker
    /// the token carried.
    /// </summary>
    private static ClaimsPrincipal Caller(string kind, params string[] roles)
    {
        if (kind == "anonymous")
            return new ClaimsPrincipal(new ClaimsIdentity());

        var claims = new List<Claim> { new("sub", "subject", ClaimValueTypes.String, Issuer) };
        switch (kind)
        {
            case "active":
                claims.Add(new Claim(PartyIdClaimsTransformation<string>.ClaimType, "local-id"));
                claims.Add(new Claim(RepositoryRolesClaimsTransformation.ActiveAccount, "true"));
                break;
            case "inactive":
                claims.Add(new Claim(PartyIdClaimsTransformation<string>.ClaimType, "local-id"));
                break;
            case "inactive-with-a-token-s-marker":
                claims.Add(new Claim(PartyIdClaimsTransformation<string>.ClaimType, "local-id"));
                claims.Add(new Claim(RepositoryRolesClaimsTransformation.ActiveAccount, "true", ClaimValueTypes.String, Issuer));
                break;
            case "claims-from-a-token":
                claims.Add(new Claim(PartyIdClaimsTransformation<string>.ClaimType, "local-id", ClaimValueTypes.String, Issuer));
                claims.Add(new Claim(RepositoryRolesClaimsTransformation.ActiveAccount, "true", ClaimValueTypes.String, Issuer));
                break;
        }

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer", "name", ClaimTypes.Role));
    }
}
