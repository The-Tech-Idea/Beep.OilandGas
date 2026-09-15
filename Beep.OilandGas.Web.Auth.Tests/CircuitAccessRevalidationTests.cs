using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Beep.Foundation.IdentityServer.Shared.Authentication;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging.Abstractions;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public class CircuitAccessRevalidationTests
{
    [Theory]
    [InlineData("disabled")]
    [InlineData("different-user")]
    [InlineData("role-revoked")]
    [InlineData("permission-revoked")]
    [InlineData("role-added")]
    [InlineData("unavailable")]
    [InlineData("missing-token")]
    public async Task ChangedOrUnavailableAccessInvalidatesCircuit(string change)
    {
        using var handler = new Handler();
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://api.example/") };
        var tokens = new TokenProvider();
        if (change != "missing-token") tokens.SetUserToken("subject", "circuit-token");
        handler.Access = change switch
        {
            "disabled" => new("local-user", false, ["Administrator"], ["ManageUsers"]),
            "different-user" => new("other-user", true, ["Administrator"], ["ManageUsers"]),
            "role-revoked" => new("local-user", true, [], ["ManageUsers"]),
            "permission-revoked" => new("local-user", true, ["Administrator"], []),
            "role-added" => new("local-user", true, ["Administrator", "Viewer"], ["ManageUsers"]),
            _ => handler.Access
        };
        if (change == "unavailable") handler.Status = HttpStatusCode.ServiceUnavailable;
        using var provider = new Probe(new RepositoryAccountClient(http), tokens);
        Assert.False(await provider.Validate(new AuthenticationState(Principal())));
        Assert.Equal(change == "missing-token" ? 0 : 1, handler.Calls);
    }

    [Fact]
    public async Task SameAccessRemainsValidAndUsesCapturedSubjectsToken()
    {
        using var handler = new Handler();
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://api.example/") };
        var tokens = new TokenProvider();
        tokens.SetUserToken("subject", "circuit-token");
        tokens.SetUserToken("other-subject", "other-token");
        using var provider = new Probe(new RepositoryAccountClient(http), tokens);
        Assert.True(await provider.Validate(new AuthenticationState(Principal())));
        Assert.Equal("Bearer circuit-token", handler.Authorization);
    }

    [Fact]
    public async Task RevalidationLoopPublishesAnonymousStateAfterRevocation()
    {
        using var handler = new Handler { Access = new("local-user", false, [], []) };
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://api.example/") };
        var tokens = new TokenProvider();
        tokens.SetUserToken("subject", "circuit-token");
        using var provider = new Probe(new RepositoryAccountClient(http), tokens);
        var revoked = new TaskCompletionSource<AuthenticationState>(TaskCreationOptions.RunContinuationsAsynchronously);
        provider.AuthenticationStateChanged += async stateTask =>
        {
            var state = await stateTask;
            if (state.User.Identity?.IsAuthenticated != true) revoked.TrySetResult(state);
        };
        provider.SetAuthenticationState(Task.FromResult(new AuthenticationState(Principal())));
        await revoked.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var current = await provider.GetAuthenticationStateAsync();
        Assert.False(current.User.Identity!.IsAuthenticated);
        Assert.False(current.User.IsInRole("Administrator"));
    }

    private static ClaimsPrincipal Principal() => new(new ClaimsIdentity([
        new Claim("sub", "subject"), new Claim(ClaimTypes.NameIdentifier, "local-user"),
        new Claim(ClaimTypes.Role, "Administrator"), new Claim("permission", "ManageUsers")], "Cookies"));

    private sealed class Probe(RepositoryAccountClient repository, TokenProvider tokens)
        : OilGasRevalidatingAuthenticationStateProvider(NullLoggerFactory.Instance, repository, tokens)
    {
        protected override TimeSpan RevalidationInterval => TimeSpan.FromMilliseconds(20);
        public Task<bool> Validate(AuthenticationState state) => ValidateAuthenticationStateAsync(state, default);
    }

    private sealed class Handler : HttpMessageHandler
    {
        public RepositoryUserAccess Access { get; set; } = new("local-user", true, ["Administrator"], ["ManageUsers"]);
        public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;
        public int Calls { get; private set; }
        public string? Authorization { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            Authorization = request.Headers.Authorization?.ToString();
            return Task.FromResult(new HttpResponseMessage(Status) { Content = JsonContent.Create(Access) });
        }
    }
}
