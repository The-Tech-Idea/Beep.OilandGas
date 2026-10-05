using System.Net;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public class FieldSelectionTests
{
    [Fact]
    public async Task NoActiveFieldStillAllowsFirstSelection()
    {
        using var fixture = new Fixture { CurrentStatus = HttpStatusCode.NotFound };
        await fixture.State.LoadAsync();
        Assert.True(fixture.State.Loaded); Assert.Null(fixture.State.SelectedId); Assert.Null(fixture.State.Error);
        await fixture.State.SelectAsync("b");
        Assert.Equal("b", fixture.State.SelectedId);
    }

    [Theory]
    [InlineData("[]", true)]
    [InlineData("null", false)]
    [InlineData("[{\"fieldId\":\"a\"},{\"fieldId\":\"a\"}]", false)]
    public async Task EmptyCatalogIsDistinctFromInvalidPayload(string catalog, bool valid)
    {
        using var fixture = new Fixture { Catalog = catalog, CurrentStatus = HttpStatusCode.NotFound };
        await fixture.State.LoadAsync();
        Assert.Equal(valid, fixture.State.Loaded); Assert.Equal(valid, fixture.State.Error is null);
    }

    [Fact]
    public async Task SelectionWaitsForConfirmationAndIgnoresConcurrentInput()
    {
        using var fixture = new Fixture();
        await fixture.State.LoadAsync();
        var pending = new TaskCompletionSource<HttpResponseMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        fixture.Post = () => pending.Task;
        var save = fixture.State.SelectAsync("b");
        Assert.True(fixture.State.Saving); Assert.Equal("a", fixture.State.SelectedId);
        await fixture.State.SelectAsync("a"); Assert.Equal(1, fixture.PostCount);
        pending.SetResult(Reply(HttpStatusCode.OK, "{\"success\":true,\"fieldId\":\"b\"}"));
        await save;
        Assert.Equal("b", fixture.State.SelectedId); Assert.False(fixture.State.Saving); Assert.NotNull(fixture.State.Confirmation);
    }

    [Fact]
    public async Task RejectedSelectionPreservesConfirmedField()
    {
        using var fixture = new Fixture { Post = () => Task.FromResult(Reply(HttpStatusCode.OK, "{\"success\":false}")) };
        var events = new List<string>(); fixture.Data.CurrentFieldChanged += events.Add;
        await fixture.State.LoadAsync(); await fixture.State.SelectAsync("b");
        Assert.Equal("a", fixture.State.SelectedId); Assert.True(fixture.State.Loaded); Assert.NotNull(fixture.State.Error); Assert.Empty(events);
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden, "{}", "do not have access")]
    [InlineData(HttpStatusCode.Unauthorized, "{}", "session has expired")]
    [InlineData(HttpStatusCode.OK, "{\"success\":true,\"fieldId\":\"wrong\"}", "could not be confirmed")]
    public async Task UnconfirmedSelectionInvalidatesDisplayedContext(HttpStatusCode status, string body, string message)
    {
        using var fixture = new Fixture { Post = () => Task.FromResult(Reply(status, body)) };
        var events = new List<string>(); fixture.Data.CurrentFieldChanged += events.Add;
        await fixture.State.LoadAsync(); await fixture.State.SelectAsync("b");
        Assert.Null(fixture.State.SelectedId); Assert.False(fixture.State.Loaded); Assert.Contains(message, fixture.State.Error);
        Assert.Equal(string.Empty, Assert.Single(events));
    }

    [Fact]
    public async Task FailedSubscriberDoesNotTurnSavedSelectionIntoFailure()
    {
        using var fixture = new Fixture();
        fixture.Data.CurrentFieldChanged += _ => throw new InvalidOperationException();
        var events = new List<string>(); fixture.Data.CurrentFieldChanged += events.Add;
        Assert.True(await fixture.Data.SetCurrentFieldAsync("b")); Assert.Equal("b", Assert.Single(events));
    }

    [Fact]
    public async Task CurrentFieldIsRefreshedAndServiceFailureIsNotNoSelection()
    {
        using var fixture = new Fixture();
        Assert.Equal("a", await fixture.Data.GetCurrentFieldIdAsync());
        fixture.Current = "b";
        Assert.Equal("b", await fixture.Data.GetCurrentFieldIdAsync());
        fixture.CurrentStatus = HttpStatusCode.ServiceUnavailable;
        var failure = await Assert.ThrowsAsync<OilGasApiException>(() => fixture.Data.GetCurrentFieldIdAsync());
        Assert.Equal(HttpStatusCode.ServiceUnavailable, failure.StatusCode);
    }

    private static HttpResponseMessage Reply(HttpStatusCode status, string body) => new(status) { Content = new StringContent(body) };
    private sealed class Fixture : HttpMessageHandler
    {
        public string Catalog = "[{\"fieldId\":\"a\",\"fieldName\":\"Alpha\"},{\"fieldId\":\"b\",\"fieldName\":\"Beta\"}]";
        public string Current = "a";
        public HttpStatusCode CurrentStatus = HttpStatusCode.OK;
        public Func<Task<HttpResponseMessage>> Post = () => Task.FromResult(Reply(HttpStatusCode.OK, "{\"success\":true,\"fieldId\":\"b\"}"));
        public int PostCount;
        public DataManagementService Data { get; }
        public FieldSelectionState State { get; }
        private readonly HttpClient _http;
        public Fixture()
        {
            _http = new HttpClient(this, false) { BaseAddress = new Uri("https://api.example") };
            var reporter = new RecordingFailureReporter();
            var api = new ApiClient(_http, reporter);
            var calls = TestFailures.Calls(reporter);
            Data = new DataManagementService(api, NullLogger<DataManagementService>.Instance, calls, reporter);
            State = new FieldSelectionState(api, Data, calls);
        }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Post) { PostCount++; return Post(); }
            return Task.FromResult(request.RequestUri!.AbsolutePath.EndsWith("/fields")
                ? Reply(HttpStatusCode.OK, Catalog)
                : Reply(CurrentStatus, "{\"fieldId\":\"" + Current + "\"}"));
        }
        protected override void Dispose(bool disposing) { if (disposing) { State.Dispose(); _http.Dispose(); } base.Dispose(disposing); }
    }
}
