using System.Net;
using System.Text.Json;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public class FieldDashboardStateTests
{
    [Fact]
    public async Task NoSelectedFieldDoesNotRequestOrInventData()
    {
        using var state = new FieldDashboardState(() => throw new Exception("must not call"));
        await state.LoadAsync(() => Task.FromResult<string?>(null));
        Assert.True(state.NeedsField); Assert.Null(state.Dashboard); Assert.Null(state.Error); Assert.False(state.Loading);
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden, "do not have access")]
    [InlineData(HttpStatusCode.Unauthorized, "session has expired")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "Retry")]
    public async Task FailedReloadClearsPriorFigures(HttpStatusCode status, string message)
    {
        var fail = false;
        using var state = new FieldDashboardState(() => fail ? Task.FromException<FieldDashboard>(new HttpRequestException("secret", null, status)) : Task.FromResult(new FieldDashboard { FieldId = "a" }));
        await state.LoadAsync(() => Task.FromResult<string?>("a")); Assert.NotNull(state.Dashboard);
        fail = true;
        await state.LoadAsync(() => Task.FromResult<string?>("a"));
        Assert.Null(state.Dashboard); Assert.Contains(message, state.Error); Assert.DoesNotContain("secret", state.Error); Assert.False(state.Loading);
    }

    [Fact]
    public async Task LateFieldResponseCannotReplaceNewField()
    {
        var pending = new TaskCompletionSource<FieldDashboard>(TaskCreationOptions.RunContinuationsAsynchronously);
        var calls = 0;
        using var state = new FieldDashboardState(() => ++calls == 1 ? pending.Task : Task.FromResult(new FieldDashboard { FieldId = "b" }));
        var first = state.LoadAsync(() => Task.FromResult<string?>("a"));
        await state.LoadAsync(() => Task.FromResult<string?>("b"));
        pending.SetResult(new FieldDashboard { FieldId = "a" }); await first;
        Assert.Equal("b", state.Dashboard!.FieldId);
    }

    [Fact]
    public async Task WrongFieldPayloadIsUnavailable()
    {
        using var state = new FieldDashboardState(() => Task.FromResult(new FieldDashboard { FieldId = "other" }));
        await state.LoadAsync(() => Task.FromResult<string?>("selected"));
        Assert.Null(state.Dashboard); Assert.NotNull(state.Error);
    }

    [Theory]
    [InlineData("0", true)]
    [InlineData("12.5", true)]
    [InlineData("null", false)]
    [InlineData("{}", false)]
    public void JsonMetricsKeepRealZeroAndRejectMissingOrComplexValues(string json, bool available)
    {
        var value = JsonSerializer.Deserialize<JsonElement>(json);
        var formatted = FieldDashboardState.MetricValue(value);
        Assert.Equal(!available, formatted == "Not available");
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, "null")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "unavailable")]
    public async Task LifecycleClientDoesNotReturnEmptyDashboardOnFailure(HttpStatusCode code, string body)
    {
        using var http = new HttpClient(new Handler(code, body)) { BaseAddress = new("https://api.example") };
        var client = new LifeCycleService(new ApiClient(http, NullLogger<ApiClient>.Instance), NullLogger<LifeCycleService>.Instance);
        await Assert.ThrowsAnyAsync<Exception>(() => client.GetFieldDashboardAsync());
    }
    [Fact]
    public async Task ClearedFieldCannotBeReplacedByPendingDashboard()
    {
        var pending = new TaskCompletionSource<FieldDashboard>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var state = new FieldDashboardState(() => pending.Task);
        var oldLoad = state.LoadAsync(() => Task.FromResult<string?>("old"));
        await state.LoadAsync(() => Task.FromResult<string?>(null));
        pending.SetResult(new FieldDashboard { FieldId = "old" });
        await oldLoad;
        Assert.True(state.NeedsField); Assert.Null(state.Dashboard); Assert.False(state.Loading);
    }

    [Fact]
    public async Task LateCurrentFieldLookupCannotStartObsoleteDashboardRequest()
    {
        var pending = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var calls = 0;
        using var state = new FieldDashboardState(() => { calls++; return Task.FromResult(new FieldDashboard { FieldId = "new" }); });
        var oldLoad = state.LoadAsync(() => pending.Task);
        await state.LoadAsync(() => Task.FromResult<string?>("new"));
        pending.SetResult("old"); await oldLoad;
        Assert.Equal(1, calls); Assert.Equal("new", state.Dashboard!.FieldId);
    }

    [Fact]
    public async Task DisposedPageDoesNotAcceptPendingDashboard()
    {
        var pending = new TaskCompletionSource<FieldDashboard>(TaskCreationOptions.RunContinuationsAsynchronously);
        var state = new FieldDashboardState(() => pending.Task);
        var load = state.LoadAsync(() => Task.FromResult<string?>("a"));
        state.Dispose();
        pending.SetResult(new FieldDashboard { FieldId = "a" }); await load;
        Assert.Null(state.Dashboard);
    }

    [Fact]
    public void LifecycleOverviewRequiresAuthenticatedUser()
    {
        var page = Assert.Single(typeof(ApiClient).Assembly.GetTypes(), type =>
            type.GetCustomAttributes(typeof(Microsoft.AspNetCore.Components.RouteAttribute), true)
                .Cast<Microsoft.AspNetCore.Components.RouteAttribute>().Any(route => route.Template == "/ppdm39/field/dashboard"));
        Assert.NotEmpty(page.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true));
        Assert.Empty(page.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute), true));
    }

    private sealed class Handler(HttpStatusCode status, string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
            => Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(body) });
    }
}
