using System.Net;
using System.Net.Http;
using System.Text;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// FND-02 AC-3: a failed remote read is a failed state with a named cause, never an empty result — and a
/// successful read with no matching records is still a success.
/// </summary>
public sealed class RemoteReadTests
{
    private static readonly string[] Body = ["{\"a\":1}", "{\"a\":2}"];

    [Fact]
    public async Task A_not_configured_reader_sends_nothing_and_says_so()
    {
        var handler = new Recorder(_ => Json("[]"));

        var reader = new OgRemoteReader(Settings(baseUrl: null, token: false), new QuietFailures(), handlerFactory: () => handler);

        var result = await reader.ReadAsync<Dictionary<string, int>>("api/wells/comparison-fields");

        Assert.Equal(OgRemoteReadOutcome.NotConfigured, result.Outcome);
        Assert.False(result.Success);
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    public async Task A_successful_read_returns_the_records()
    {
        var reader = new OgRemoteReader(Settings(), new QuietFailures(), handlerFactory: () => new Recorder(_ => Json("[{\"a\":1},{\"a\":2}]")));

        var result = await reader.ReadAsync<Dictionary<string, int>>("api/wells/comparison-fields");

        Assert.Equal(OgRemoteReadOutcome.Ok, result.Outcome);
        Assert.True(result.Success);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task An_empty_list_from_a_healthy_api_is_a_success_with_no_rows()
    {
        var reader = new OgRemoteReader(Settings(), new QuietFailures(), handlerFactory: () => new Recorder(_ => Json("[]")));

        var result = await reader.ReadAsync<Dictionary<string, int>>("api/wells/comparison-fields");

        // "No records match" is not a failure; "the API refused" is. The outcome keeps them apart.
        Assert.Equal(OgRemoteReadOutcome.Ok, result.Outcome);
        Assert.Empty(result.Items);
        Assert.Contains("no records", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task A_server_error_is_a_failure_not_an_empty_list()
    {
        var reader = new OgRemoteReader(Settings(), new QuietFailures(), handlerFactory: () => new Recorder(_ => Status(HttpStatusCode.InternalServerError)));

        var result = await reader.ReadAsync<Dictionary<string, int>>("api/wells/comparison-fields");

        Assert.Equal(OgRemoteReadOutcome.ServerError, result.Outcome);
        Assert.False(result.Success);
        Assert.Empty(result.Items);
        Assert.Contains("500", result.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_refused_caller_is_reported_as_unauthorized_with_the_next_step()
    {
        var reader = new OgRemoteReader(Settings(), new QuietFailures(), handlerFactory: () => new Recorder(_ => Status(HttpStatusCode.Unauthorized)));

        var result = await reader.ReadAsync<Dictionary<string, int>>("api/wells/comparison-fields");

        Assert.Equal(OgRemoteReadOutcome.Unauthorized, result.Outcome);
        Assert.Contains("Sign in", result.Message, StringComparison.Ordinal);
        Assert.Contains("not retried", result.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task An_unreachable_api_is_a_failure_with_a_reference()
    {
        var failures = new QuietFailures();
        var reader = new OgRemoteReader(Settings(), failures, handlerFactory: () => new Recorder(_ => throw new HttpRequestException("probe: no answer")));

        var result = await reader.ReadAsync<Dictionary<string, int>>("api/wells/comparison-fields");

        Assert.Equal(OgRemoteReadOutcome.Unreachable, result.Outcome);
        Assert.False(result.Success);
        Assert.NotNull(result.Reference);
        Assert.Single(failures.Reported);
        Assert.Contains("did not answer", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task An_empty_body_or_malformed_json_is_a_failure_named_as_such()
    {
        var empty = new OgRemoteReader(Settings(), new QuietFailures(), handlerFactory: () => new Recorder(_ => Json(string.Empty)));
        var broken = new OgRemoteReader(Settings(), new QuietFailures(), handlerFactory: () => new Recorder(_ => Json("{not json")));

        Assert.Equal(OgRemoteReadOutcome.InvalidPayload, (await empty.ReadAsync<Dictionary<string, int>>("api/x")).Outcome);
        Assert.Equal(OgRemoteReadOutcome.InvalidPayload, (await broken.ReadAsync<Dictionary<string, int>>("api/x")).Outcome);
    }

    [Fact]
    public async Task The_token_is_sent_when_one_is_supplied()
    {
        string? authorization = null;

        var handler = new Recorder(request =>
        {
            authorization = request.Headers.Authorization?.ToString();
            return Json("[]");
        });

        var reader = new OgRemoteReader(Settings(), new QuietFailures(), accessToken: "probe-token", handlerFactory: () => handler);

        await reader.ReadAsync<Dictionary<string, int>>("api/wells/comparison-fields");

        Assert.Equal("Bearer probe-token", authorization);
    }

    private static OgServiceAccessSettings Settings(string? baseUrl = "https://api.example.test/", bool token = true) =>
        new(OgAccessPreference.Auto, baseUrl, token, "local", LocalDatabaseExists: false);

    private static HttpResponseMessage Json(string body) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };

    private static HttpResponseMessage Status(HttpStatusCode status) => new(status);

    private sealed class Recorder : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _respond;

        public Recorder(Func<HttpRequestMessage, HttpResponseMessage> respond) => _respond = respond;

        public int Calls { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult(_respond(request));
        }
    }

    private sealed class QuietFailures : IFailureReporter
    {
        public List<string> Reported { get; } = [];

        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error)
        {
            Reported.Add($"{operation} | {consequence}");
            return FailureReferenceFormat.New();
        }
    }
}
