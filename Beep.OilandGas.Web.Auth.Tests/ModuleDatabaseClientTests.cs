using System.Net;
using System.Text.Json;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public class ModuleDatabaseClientTests
{
    [Fact]
    public async Task ExecutionCarriesReviewedHashesAndDoesNotChooseAnActor()
    {
        using var handler = new Handler(HttpStatusCode.OK, "{\"success\":true}");
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.example") };
        var client = new ModuleDatabaseClient(new ApiClient(http, NullLogger<ApiClient>.Instance));
        await client.ExecuteAsync(new() { PlanId = "reviewed-plan", PlanHash = "plan-hash", ManifestHash = "manifest-hash" }, true);
        Assert.Equal("/api/ppdm39/setup/schema/execute", handler.Path);
        using var body = JsonDocument.Parse(handler.Body!);
        Assert.Equal("reviewed-plan", body.RootElement.GetProperty("planId").GetString());
        Assert.Equal("plan-hash", body.RootElement.GetProperty("expectedPlanHash").GetString());
        Assert.Equal("manifest-hash", body.RootElement.GetProperty("expectedManifestHash").GetString());
        Assert.True(body.RootElement.GetProperty("acknowledgeHighRisk").GetBoolean());
        Assert.Equal("", body.RootElement.GetProperty("executedBy").GetString());
        Assert.False(body.RootElement.GetProperty("resumeIfCheckpointExists").GetBoolean());
    }

    [Fact]
    public async Task ApprovalUsesReviewedPlanWithoutClientActor()
    {
        using var handler = new Handler(HttpStatusCode.OK, "{\"success\":true}");
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.example") };
        var client = new ModuleDatabaseClient(new ApiClient(http, NullLogger<ApiClient>.Instance));
        await client.ApproveAsync("reviewed-plan");
        Assert.Equal("/api/ppdm39/setup/schema/approve", handler.Path);
        using var body = JsonDocument.Parse(handler.Body!);
        Assert.Equal("reviewed-plan", body.RootElement.GetProperty("planId").GetString());
        Assert.Equal("", body.RootElement.GetProperty("approvedBy").GetString());
    }

    [Fact]
    public async Task BindingSendsSelectedConnectionAndConcurrencyStamp()
    {
        using var handler = new Handler(HttpStatusCode.OK, "{\"moduleId\":\"GAS_LIFT\",\"connectionName\":\"gas-db\",\"concurrencyStamp\":\"new\"}");
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.example") };
        var client = new ModuleDatabaseClient(new ApiClient(http, NullLogger<ApiClient>.Instance));
        var result = await client.BindAsync("GAS_LIFT", new("gas-db", "old"));
        Assert.Equal("new", result.ConcurrencyStamp);
        Assert.Equal(HttpMethod.Put, handler.Method);
        Assert.Equal("/api/setup/modules/GAS_LIFT/connection", handler.Path);
        using var body = JsonDocument.Parse(handler.Body!);
        Assert.Equal("gas-db", body.RootElement.GetProperty("connectionName").GetString());
        Assert.Equal("old", body.RootElement.GetProperty("concurrencyStamp").GetString());
    }

    [Fact]
    public async Task PlanningSendsEvidenceButNoConnectionOverride()
    {
        using var handler = new Handler(HttpStatusCode.OK, "{\"success\":true,\"planId\":\"plan\"}");
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.example") };
        var client = new ModuleDatabaseClient(new ApiClient(http, NullLogger<ApiClient>.Instance));
        Assert.True((await client.PlanAsync("GAS_LIFT", new("Production", true, true, "restore-123", "binding-version"))).Success);
        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal("/api/setup/modules/GAS_LIFT/plan", handler.Path);
        using var body = JsonDocument.Parse(handler.Body!);
        Assert.Equal("Production", body.RootElement.GetProperty("environmentTier").GetString());
        Assert.Equal("restore-123", body.RootElement.GetProperty("restoreTestEvidence").GetString());
        Assert.Equal("binding-version", body.RootElement.GetProperty("concurrencyStamp").GetString());
        Assert.False(body.RootElement.TryGetProperty("connectionName", out _));
    }

    [Fact]
    public async Task StaleBindingIsNotReportedAsSaved()
    {
        using var handler = new Handler(HttpStatusCode.Conflict, "{}");
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.example") };
        var client = new ModuleDatabaseClient(new ApiClient(http, NullLogger<ApiClient>.Instance));
        await Assert.ThrowsAsync<HttpRequestException>(() => client.BindAsync("GAS_LIFT", new("gas-db", "stale")));
    }

    private sealed class Handler(HttpStatusCode status, string response) : HttpMessageHandler
    {
        public HttpMethod? Method { get; private set; }
        public string? Path { get; private set; }
        public string? Body { get; private set; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Method = request.Method;
            Path = request.RequestUri!.AbsolutePath;
            Body = request.Content is null ? null : await request.Content.ReadAsStringAsync(token);
            return new(status) { Content = new StringContent(response) };
        }
    }
}
