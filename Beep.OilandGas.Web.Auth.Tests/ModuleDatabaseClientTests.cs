using System.Net;
using System.Text.Json;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public class ModuleDatabaseClientTests
{
    [Fact]
    public async Task SeedingUsesSelectedModuleVersionWithoutActorOrConnectionOverride()
    {
        using var handler = new Handler(HttpStatusCode.OK,
            "{\"moduleId\":\"LIFECYCLE\",\"success\":true,\"recordsInserted\":3,\"tablesSeeded\":1,\"errors\":[]}");
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.example") };
        var client = new ModuleDatabaseClient(new ApiClient(http, new RecordingFailureReporter()));
        var result = await client.SeedAsync("LIFECYCLE", new("saved-version"));
        Assert.True(result.Success);
        Assert.Equal(3, result.RecordsInserted);
        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal("/api/setup/modules/LIFECYCLE/seed", handler.Path);
        using var body = JsonDocument.Parse(handler.Body!);
        Assert.Equal("saved-version", body.RootElement.GetProperty("concurrencyStamp").GetString());
        Assert.Single(body.RootElement.EnumerateObject());
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Forbidden)]
    public async Task FailedSeedingIsNotReportedAsSuccess(HttpStatusCode status)
    {
        using var handler = new Handler(status, "{}");
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.example") };
        var client = new ModuleDatabaseClient(new ApiClient(http, new RecordingFailureReporter()));
        await Assert.ThrowsAsync<HttpRequestException>(() => client.SeedAsync("LIFECYCLE", new("saved-version")));
    }

    [Fact]
    public async Task ExecutionCarriesReviewedHashesAndDoesNotChooseAnActor()
    {
        using var handler = new Handler(HttpStatusCode.OK, "{\"success\":true,\"planId\":\"reviewed-plan\",\"planHash\":\"plan-hash\",\"manifestHash\":\"manifest-hash\"}");
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.example") };
        var client = new ModuleDatabaseClient(new ApiClient(http, new RecordingFailureReporter()));
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
        using var handler = new Handler(HttpStatusCode.OK, "{\"success\":true,\"planId\":\"reviewed-plan\"}");
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.example") };
        var client = new ModuleDatabaseClient(new ApiClient(http, new RecordingFailureReporter()));
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
        var client = new ModuleDatabaseClient(new ApiClient(http, new RecordingFailureReporter()));
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
        var client = new ModuleDatabaseClient(new ApiClient(http, new RecordingFailureReporter()));
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
        var client = new ModuleDatabaseClient(new ApiClient(http, new RecordingFailureReporter()));
        await Assert.ThrowsAsync<HttpRequestException>(() => client.BindAsync("GAS_LIFT", new("gas-db", "stale")));
    }

    [Theory]
    [InlineData("planId", "different")]
    [InlineData("planId", "")]
    [InlineData("planHash", "different")]
    [InlineData("planHash", "")]
    [InlineData("manifestHash", "different")]
    [InlineData("manifestHash", "")]
    public async Task SuccessfulExecutionMustMatchReviewedIdentity(string key, string value)
    {
        var reply = new Dictionary<string, object>
        {
            ["success"] = true, ["planId"] = "reviewed-plan",
            ["planHash"] = "plan-hash", ["manifestHash"] = "manifest-hash"
        };
        reply[key] = value;
        using var handler = new Handler(HttpStatusCode.OK, JsonSerializer.Serialize(reply));
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.example") };
        var client = new ModuleDatabaseClient(new ApiClient(http, new RecordingFailureReporter()));
        await Assert.ThrowsAsync<InvalidOperationException>(() => client.ExecuteAsync(
            new() { PlanId = "reviewed-plan", PlanHash = "plan-hash", ManifestHash = "manifest-hash" }, false));
    }

    [Theory]
    [InlineData("")]
    [InlineData("different")]
    [InlineData("REVIEWED-PLAN")]
    public async Task SuccessfulApprovalMustIdentifyTheRequestedPlan(string id)
    {
        using var handler = new Handler(HttpStatusCode.OK, JsonSerializer.Serialize(new { success = true, planId = id }));
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.example") };
        var client = new ModuleDatabaseClient(new ApiClient(http, new RecordingFailureReporter()));
        await Assert.ThrowsAsync<InvalidOperationException>(() => client.ApproveAsync("reviewed-plan"));
    }

    [Fact]
    public async Task FailedExecutionPreservesTheServerDiagnosticWithoutClaimingSuccess()
    {
        using var handler = new Handler(HttpStatusCode.OK, "{\"success\":false,\"message\":\"Plan expired\"}");
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.example") };
        var client = new ModuleDatabaseClient(new ApiClient(http, new RecordingFailureReporter()));
        var result = await client.ExecuteAsync(new() { PlanId = "plan", PlanHash = "hash", ManifestHash = "manifest" }, false);
        Assert.False(result.Success);
        Assert.Equal("Plan expired", result.Message);
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
