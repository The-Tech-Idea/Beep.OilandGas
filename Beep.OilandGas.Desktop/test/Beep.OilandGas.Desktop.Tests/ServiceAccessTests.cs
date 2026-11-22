using System.Net;
using System.Net.Http;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// FND-02: the access-mode rule, the audited switch, and a health check that tells "no answer" apart from
/// "refused this caller" and "the database is gone".
/// </summary>
public sealed class ServiceAccessTests
{
    private const string Api = "https://api.example.test/";

    [Theory]
    // AC-1: Auto chooses Remote only when base URL AND token are both usable.
    [InlineData(OgAccessPreference.Auto, Api, true, "conn", true, OgResolvedAccess.Remote)]
    [InlineData(OgAccessPreference.Auto, Api, false, "conn", true, OgResolvedAccess.Local)]
    [InlineData(OgAccessPreference.Auto, null, true, "conn", true, OgResolvedAccess.Local)]
    [InlineData(OgAccessPreference.Auto, Api, false, null, false, OgResolvedAccess.NotConfigured)]
    // AC-2: Auto chooses Local when no API is configured and a local database exists.
    [InlineData(OgAccessPreference.Auto, null, false, "conn", true, OgResolvedAccess.Local)]
    [InlineData(OgAccessPreference.Auto, null, false, "conn", false, OgResolvedAccess.NotConfigured)]
    // An explicit choice is honoured and does not fall back behind the operator's back.
    [InlineData(OgAccessPreference.Local, Api, true, "conn", true, OgResolvedAccess.Local)]
    [InlineData(OgAccessPreference.Local, null, false, "conn", false, OgResolvedAccess.NotConfigured)]
    [InlineData(OgAccessPreference.Remote, Api, false, "conn", true, OgResolvedAccess.NotConfigured)]
    [InlineData(OgAccessPreference.Remote, Api, true, null, false, OgResolvedAccess.Remote)]
    public void The_resolution_rule_covers_every_combination(
        OgAccessPreference preference,
        string? api,
        bool token,
        string? connection,
        bool database,
        OgResolvedAccess expected)
    {
        var resolution = OgAccessResolver.Resolve(
            new OgServiceAccessSettings(preference, api, token, connection, database));

        Assert.Equal(expected, resolution.Mode);
        Assert.False(string.IsNullOrWhiteSpace(resolution.Reason));
    }

    [Fact]
    public void Auto_explains_which_step_of_the_rule_it_took()
    {
        var remote = OgAccessResolver.Resolve(new OgServiceAccessSettings(OgAccessPreference.Auto, Api, true, "conn", true));
        var local = OgAccessResolver.Resolve(new OgServiceAccessSettings(OgAccessPreference.Auto, null, false, "conn", true));
        var none = OgAccessResolver.Resolve(new OgServiceAccessSettings(OgAccessPreference.Auto, null, false, null, false));

        Assert.Contains("Remote", remote.Reason, StringComparison.Ordinal);
        Assert.Contains("Local", local.Reason, StringComparison.Ordinal);
        Assert.Contains("refuse", none.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Changing_the_mode_is_audited_with_the_previous_and_the_new_mode()
    {
        var audit = new RecordingAudit();
        var access = Build(
            new OgServiceAccessSettings(OgAccessPreference.Auto, null, false, "conn", true),
            audit);

        Assert.Equal(OgResolvedAccess.Local, access.Mode);

        var result = await access.ConfigureAsync(
            new OgServiceAccessSettings(OgAccessPreference.Remote, Api, true, "conn", true));

        Assert.True(result.Success);
        Assert.Equal(OgResolvedAccess.Remote, access.Mode);

        var row = Assert.Single(audit.Rows);
        Assert.Equal("access.switch", row.Verb);
        Assert.Equal("Remote", row.SubjectId);
        Assert.Contains("previous=Local", row.Detail, StringComparison.Ordinal);
        Assert.Contains("preference=Remote", row.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Health_of_a_missing_local_database_is_DatabaseMissing_not_Unreachable()
    {
        var access = Build(
            new OgServiceAccessSettings(OgAccessPreference.Local, null, false, "conn", true),
            new RecordingAudit(),
            present: false);

        var health = await access.CheckHealthAsync();

        Assert.True(health.Success);
        Assert.Equal(OgAccessHealth.DatabaseMissing, health.Value!.State);
        Assert.Contains("missing", health.Value.Detail, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Health_of_a_present_local_database_is_Ok()
    {
        var access = Build(
            new OgServiceAccessSettings(OgAccessPreference.Local, null, false, "conn", true),
            new RecordingAudit(),
            present: true);

        var health = await access.CheckHealthAsync();

        Assert.Equal(OgAccessHealth.Ok, health.Value!.State);
    }

    [Fact]
    public async Task Health_of_nothing_configured_says_so_instead_of_failing()
    {
        var access = Build(
            new OgServiceAccessSettings(OgAccessPreference.Auto, null, false, null, false),
            new RecordingAudit());

        var health = await access.CheckHealthAsync();

        Assert.True(health.Success);
        Assert.Equal(OgAccessHealth.NotConfigured, health.Value!.State);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, OgAccessHealth.Ok)]
    [InlineData(HttpStatusCode.NoContent, OgAccessHealth.Ok)]
    [InlineData(HttpStatusCode.Unauthorized, OgAccessHealth.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden, OgAccessHealth.Unauthorized)]
    [InlineData(HttpStatusCode.InternalServerError, OgAccessHealth.Unreachable)]
    [InlineData(HttpStatusCode.NotFound, OgAccessHealth.Unreachable)]
    public async Task Health_distinguishes_an_answer_from_a_refusal(
        HttpStatusCode status,
        OgAccessHealth expected)
    {
        var access = Build(
            new OgServiceAccessSettings(OgAccessPreference.Remote, Api, true, null, false),
            new RecordingAudit(),
            handler: new StubHandler(_ => new HttpResponseMessage(status)));

        var health = await access.CheckHealthAsync();

        Assert.Equal(expected, health.Value!.State);
    }

    [Fact]
    public async Task Health_calls_the_health_endpoint_of_the_configured_api()
    {
        Uri? requested = null;
        var access = Build(
            new OgServiceAccessSettings(OgAccessPreference.Remote, Api, true, null, false),
            new RecordingAudit(),
            handler: new StubHandler(request =>
            {
                requested = request.RequestUri;
                return new HttpResponseMessage(HttpStatusCode.OK);
            }));

        await access.CheckHealthAsync();

        Assert.Equal(new Uri(Api + "health"), requested);
    }

    [Fact]
    public async Task A_remote_health_probe_that_cannot_connect_is_reported_and_answered_as_Unreachable()
    {
        var failures = new RecordingFailures();
        var access = new OgServiceAccess(
            new OgServiceAccessSettings(OgAccessPreference.Remote, Api, true, null, false),
            new RecordingAudit(),
            failures,
            _ => false,
            () => new ThrowingHandler());

        var health = await access.CheckHealthAsync();

        Assert.True(health.Success);
        Assert.Equal(OgAccessHealth.Unreachable, health.Value!.State);
        Assert.Single(failures.Reported);
    }

    private static OgServiceAccess Build(
        OgServiceAccessSettings settings,
        RecordingAudit audit,
        bool present = true,
        HttpMessageHandler? handler = null) =>
        new(
            settings,
            audit,
            new RecordingFailures(),
            _ => present,
            handler is null ? null : () => handler);

    private sealed class RecordingAudit : IOgAuditService
    {
        public List<(string Verb, string SubjectId, string Outcome, string? Detail)> Rows { get; } = [];

        public void Add(
            string verb,
            string subjectKind,
            string subjectId,
            string outcome,
            string? detail = null,
            string? fieldId = null) =>
            Rows.Add((verb, subjectId, outcome, detail));
    }

    private sealed class RecordingFailures : IFailureReporter
    {
        public List<string> Reported { get; } = [];

        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error)
        {
            Reported.Add($"{severity}: {operation} — {consequence}");
            return FailureReferenceFormat.New();
        }
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _respond;

        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) => _respond = respond;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(_respond(request));
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            throw new HttpRequestException("probe: the host did not answer");
    }
}
