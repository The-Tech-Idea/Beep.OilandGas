using Beep.OilandGas.Models.Data.Production;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public class ProductionDashboardStateTests
{
    private static Task<string?> Field() => Task.FromResult<string?>("a");
    private static ProductionDashboardResponse Response(string field = "a") => new() { Summary = new() { FieldId = field } };
    [Fact]
    public async Task NoFieldDoesNotLoadData()
    {
        using var state = new ProductionDashboardState(_ => throw new Exception(), TestFailures.Calls(new RecordingFailureReporter()));
        await state.LoadAsync(() => Task.FromResult<string?>(null));
        Assert.True(state.NeedsField); Assert.Null(state.Error); Assert.Null(state.Summary);
    }
    [Fact]
    public async Task EmptyWellsAreValidWithConfirmedSummary()
    {
        using var state = new ProductionDashboardState(_ => Task.FromResult(Response()), TestFailures.Calls(new RecordingFailureReporter()));
        await state.LoadAsync(Field);
        Assert.NotNull(state.Summary); Assert.Empty(state.Wells); Assert.Null(state.Error);
    }
    [Fact]
    public async Task WrongSummaryCannotBeDisplayed()
    {
        using var state = new ProductionDashboardState(_ => Task.FromResult(Response()), TestFailures.Calls(new RecordingFailureReporter()));
        await state.LoadAsync(() => Task.FromResult<string?>("other"));
        Assert.Null(state.Summary); Assert.NotNull(state.Error);
    }
    [Fact]
    public async Task RequestIsBoundToTheSelectedFieldOnce()
    {
        var reads = 0;
        using var state = new ProductionDashboardState(field => { Assert.Equal("a", field); return Task.FromResult(Response(field)); }, TestFailures.Calls(new RecordingFailureReporter()));
        await state.LoadAsync(() => Task.FromResult<string?>(++reads == 1 ? "a" : "b"));
        Assert.NotNull(state.Summary); Assert.Equal(1, reads);
    }
    [Fact]
    public async Task FailedDashboardClearsPreviousResult()
    {
        var fail = false;
        using var state = new ProductionDashboardState(_ => fail
            ? Task.FromException<ProductionDashboardResponse>(new OilGasApiException(HttpStatusCode.Forbidden, "secret", null))
            : Task.FromResult(Response()), TestFailures.Calls(new RecordingFailureReporter()));
        await state.LoadAsync(Field); Assert.NotNull(state.Summary);
        fail = true; await state.LoadAsync(Field);
        Assert.Null(state.Summary); Assert.Empty(state.Wells); Assert.Contains("do not have access", state.Error);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ClearingFieldOrDisposalRejectsLateDashboard(bool dispose)
    {
        var pending = new TaskCompletionSource<ProductionDashboardResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var state = new ProductionDashboardState(_ => pending.Task, TestFailures.Calls(new RecordingFailureReporter()));
        var load = state.LoadAsync(Field);
        if (dispose) state.Dispose(); else await state.LoadAsync(() => Task.FromResult<string?>(null));
        pending.SetResult(Response()); await load;
        Assert.Null(state.Summary); Assert.Empty(state.Wells);
    }
    [Theory]
    [InlineData("null", false)]
    [InlineData("[]", true)]
    public async Task ClientDistinguishesMissingPayloadFromEmptyList(string body, bool valid)
    {
        using var http = new HttpClient(new Handler(body)) { BaseAddress = new Uri("https://api.example") };
        var client = new ProductionServiceClient(new ApiClient(http, new RecordingFailureReporter()), NullLogger<ProductionServiceClient>.Instance);
        if (valid) Assert.Empty(await client.GetDashboardWellsAsync());
        else await Assert.ThrowsAsync<InvalidOperationException>(() => client.GetDashboardWellsAsync());
    }
    [Fact]
    public async Task FieldBoundClientEscapesFieldAndRejectsNullPayload()
    {
        using var http = new HttpClient(new BoundHandler()) { BaseAddress = new Uri("https://api.example") };
        var client = new ProductionServiceClient(new ApiClient(http, new RecordingFailureReporter()), NullLogger<ProductionServiceClient>.Instance);
        await Assert.ThrowsAsync<InvalidOperationException>(() => client.GetDashboardAsync("field ?#"));
    }
    private sealed class BoundHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Assert.Equal("https://api.example/api/fields/field%20%3F%23/production/dashboard", request.RequestUri!.AbsoluteUri);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("null") });
        }
    }
    private sealed class Handler(string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body) });
    }
}
