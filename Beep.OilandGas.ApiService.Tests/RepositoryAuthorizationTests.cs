using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class RepositoryAuthorizationTests
{
    [Theory]
    [InlineData(false, true, false)]
    [InlineData(true, true, true)]
    [InlineData(true, false, false)]
    public async Task BusinessPoliciesRequireActiveRepositoryAccount(bool registered, bool active, bool allowed)
    {
        using var services = CreateServices();
        var principal = await Transform(registered, active);
        var policies = services.GetRequiredService<IAuthorizationPolicyProvider>();
        var authorization = services.GetRequiredService<IAuthorizationService>();

        var defaultPolicy = await AuthorizationPolicy.CombineAsync(policies, [new AuthorizeAttribute()]);
        var fallbackPolicy = await AuthorizationPolicy.CombineAsync(policies, []);
        Assert.NotNull(defaultPolicy);
        Assert.NotNull(fallbackPolicy);
        Assert.Equal(allowed, (await authorization.AuthorizeAsync(principal, null, defaultPolicy)).Succeeded);
        Assert.Equal(allowed, (await authorization.AuthorizeAsync(principal, null, fallbackPolicy)).Succeeded);
    }

    [Theory]
    [InlineData(typeof(RepositoryBootstrapController))]
    [InlineData(typeof(RepositoryAccountController))]
    public async Task RegistrationAndLookupAcceptUnregisteredExternalAccount(Type controller)
    {
        using var services = CreateServices();
        var attributes = controller.GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>();
        var policy = await AuthorizationPolicy.CombineAsync(
            services.GetRequiredService<IAuthorizationPolicyProvider>(), attributes);
        Assert.NotNull(policy);
        var authorization = services.GetRequiredService<IAuthorizationService>();
        Assert.True((await authorization.AuthorizeAsync(await Transform(false, true), null, policy)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(new ClaimsPrincipal(new ClaimsIdentity()), null, policy)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(await Transform(true, false), null, policy)).Succeeded);
    }

    private static ServiceProvider CreateServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization(RepositoryAuthorization.Configure);
        return services.BuildServiceProvider();
    }

    private static Task<ClaimsPrincipal> Transform(bool registered, bool active)
    {
        var access = new Mock<IRepositoryAccessService>();
        access.Setup(x => x.GetAccessAsync("https://issuer", "subject", default))
            .ReturnsAsync(registered ? new RepositoryUserAccess("local-id", active, [], []) : null);
        var transform = new RepositoryClaimsTransformation(access.Object, NullLogger<RepositoryClaimsTransformation>.Instance);
        return transform.TransformAsync(new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("iss", "https://issuer"), new Claim("sub", "subject"),
            new Claim(ClaimTypes.NameIdentifier, "forged-local-id")
        }, "Bearer")));
    }
}
