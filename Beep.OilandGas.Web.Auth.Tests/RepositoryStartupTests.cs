using System.Net;
using Beep.OilandGas.Web.Services;
using Duende.AccessTokenManagement.OpenIdConnect;
using Microsoft.Extensions.Logging.Abstractions;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public class RepositoryStartupTests
{
    [Theory]
    [InlineData(200, "Ready", RepositoryReadiness.Ready)]
    [InlineData(503, "BootstrapRequired", RepositoryReadiness.BootstrapRequired)]
    [InlineData(503, "MigrationRequired", RepositoryReadiness.MigrationRequired)]
    [InlineData(503, "Unavailable", RepositoryReadiness.Unavailable)]
    [InlineData(503, "RecoveryRequired", RepositoryReadiness.RecoveryRequired)]
    [InlineData(200, "RecoveryRequired", RepositoryReadiness.Unavailable)]
    [InlineData(503, "Ready", RepositoryReadiness.Unavailable)]
    [InlineData(200, "BootstrapRequired", RepositoryReadiness.Unavailable)]
    [InlineData(401, "Ready", RepositoryReadiness.Unavailable)]
    [InlineData(200, "3", RepositoryReadiness.Unavailable)]
    [InlineData(200, "Unknown", RepositoryReadiness.Unavailable)]
    public async Task ReadinessUsesRepositoryStatusWithoutBusinessConnections(int code, string status, RepositoryReadiness expected)
    {
        using var handler = new Handler((HttpStatusCode)code, "{\"status\":\"" + status + "\"}");
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.example/") };
        Assert.Equal(expected, await Client(http).GetReadinessAsync());
        Assert.Equal("/health/repository", handler.Path);
        Assert.Null(handler.Authorization);
    }

    [Fact]
    public async Task InvalidResponseFailsClosed()
    {
        using var handler = new Handler(HttpStatusCode.OK, "not-json");
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.example/") };
        Assert.Equal(RepositoryReadiness.Unavailable, await Client(http).GetReadinessAsync());
    }

    // Readiness is asked before anybody signs in, so it never asks for a token: a manager that answers is a failure here.
    private static RepositoryAccountClient Client(HttpClient http) =>
        new(http, new NoTokens(), NullLogger<RepositoryAccountClient>.Instance);

    private sealed class NoTokens : IUserTokenManager
    {
        public Task<Duende.AccessTokenManagement.TokenResult<UserToken>> GetAccessTokenAsync(System.Security.Claims.ClaimsPrincipal user,
            UserTokenRequestParameters? parameters = null, CancellationToken ct = default) =>
            throw new InvalidOperationException("Readiness asked for a person's token.");

        public Task RevokeRefreshTokenAsync(System.Security.Claims.ClaimsPrincipal user, UserTokenRequestParameters? parameters = null,
            CancellationToken ct = default) => throw new InvalidOperationException("Readiness revoked a person's token.");
    }

    private sealed class Handler(HttpStatusCode status, string body) : HttpMessageHandler
    {
        public string? Path { get; private set; }
        public string? Authorization { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Path = request.RequestUri!.AbsolutePath;
            Authorization = request.Headers.Authorization?.ToString();
            return Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(body) });
        }
    }
}
