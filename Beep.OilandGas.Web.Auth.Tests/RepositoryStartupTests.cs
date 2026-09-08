using System.Net;
using Beep.OilandGas.Web.Services;
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
        Assert.Equal(expected, await new RepositoryAccountClient(http).GetReadinessAsync());
        Assert.Equal("/health/repository", handler.Path);
        Assert.Null(handler.Authorization);
    }

    [Fact]
    public async Task InvalidResponseFailsClosed()
    {
        using var handler = new Handler(HttpStatusCode.OK, "not-json");
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.example/") };
        Assert.Equal(RepositoryReadiness.Unavailable, await new RepositoryAccountClient(http).GetReadinessAsync());
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
