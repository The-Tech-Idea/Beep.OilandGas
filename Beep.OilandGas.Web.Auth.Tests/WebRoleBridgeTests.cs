using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Beep.Foundation.IdentityServer.Shared.Authentication;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public class WebRoleBridgeTests
{
    [Fact]
    public async Task UsesApiRolesAndRechecksOnNextRequest()
    {
        using var handler = new Handler(HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://api.example/") };
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        var tokens = new TokenProvider();
        tokens.SetUserToken("subject", "test-token");
        var bridge = new OilGasClaimsTransformation(new RepositoryAccountClient(http), tokens, accessor,
            NullLogger<OilGasClaimsTransformation>.Instance);
        var source = Principal();
        var result = await bridge.TransformAsync(source);
        Assert.True(result.IsInRole("Viewer"));
        Assert.False(result.IsInRole("Administrator"));
        Assert.Null(result.FindFirst("permissions"));
        Assert.Null(result.FindFirst("elevated_permissions"));
        Assert.Equal("local-user", result.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.Same(result, await bridge.TransformAsync(result));
        Assert.Equal(1, handler.Calls);
        accessor.HttpContext = new DefaultHttpContext();
        await bridge.TransformAsync(result);
        Assert.Equal(2, handler.Calls);
        Assert.Equal("Bearer test-token", handler.Authorization);
        Assert.Equal("/api/auth/repository/me", handler.Path);
    }

    [Theory]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    public async Task FailedLookupCannotPreserveCookieAdminClaim(HttpStatusCode status)
    {
        using var handler = new Handler(status);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://api.example/") };
        var tokens = new TokenProvider();
        tokens.SetUserToken("subject", "test-token");
        var bridge = new OilGasClaimsTransformation(new RepositoryAccountClient(http), tokens,
            new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
            NullLogger<OilGasClaimsTransformation>.Instance);
        var result = await bridge.TransformAsync(Principal());
        Assert.False(result.Identity!.IsAuthenticated);
        Assert.False(result.IsInRole("Administrator"));
    }

    [Fact]
    public async Task RegistrationUsesExplicitBearerPost()
    {
        using var handler = new Handler(HttpStatusCode.OK);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://api.example/") };
        await new RepositoryAccountClient(http).RegisterAsync("registration-token");
        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal("/api/setup/repository/register", handler.Path);
        Assert.Equal("Bearer registration-token", handler.Authorization);
    }

    private static ClaimsPrincipal Principal() => new(new ClaimsIdentity(new[]
    {
        new Claim("sub", "subject"), new Claim("role", "Administrator"),
        new Claim(ClaimTypes.Role, "Administrator"), new Claim("oilgas:roles-resolved", "true"),
        new Claim("permissions", "Admin.ManageUsers"),
        new Claim("elevated_permissions", "Admin.AssignRoles")
    }, "Cookies"));

    private sealed class Handler(HttpStatusCode status) : HttpMessageHandler
    {
        public int Calls { get; private set; }
        public string? Path { get; private set; }
        public HttpMethod? Method { get; private set; }
        public string? Authorization { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            Path = request.RequestUri!.AbsolutePath;
            Method = request.Method;
            Authorization = request.Headers.Authorization?.ToString();
            return Task.FromResult(new HttpResponseMessage(status)
            {
                Content = JsonContent.Create(new RepositoryUserAccess("local-user", true, ["Viewer"], []))
            });
        }
    }
}
