using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging.Abstractions;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;
using TheTechIdeaWeb.Diagnostics;
using TheTechIdeaWeb.Diagnostics.Device;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// FND-06: a failure is stored locally first, then forwarded under the same reference through the shared
/// device outbox; only a delivery the API accepted marks it forwarded.
/// </summary>
public sealed class FailureForwardingTests
{
    [Fact]
    public void A_disabled_forwarder_accepts_nothing_and_queues_nothing()
    {
        var forwarder = new DisabledFailureForwarder();
        var record = Record("a1b2c3d4e5f6");

        Assert.False(forwarder.IsEnabled);
        Assert.False(forwarder.Forward(record));
        Assert.Equal(0, forwarder.Pending);
    }

    [Fact]
    public void A_reported_failure_reaches_the_outbox_under_the_reference_the_person_saw()
    {
        using var harness = new Harness();

        var reference = harness.Pipeline.ReportHandled(
            new InvalidOperationException("probe"),
            "loading a well",
            "the well was not shown");

        var queued = Assert.Single(harness.Outbox.Oldest(10));
        Assert.Equal(reference, queued.Reference);
        Assert.Equal("loading a well", queued.Operation);
        Assert.Equal("the well was not shown", queued.Consequence);

        // Local first: the record the reference points at is already durable.
        Assert.NotNull(harness.Store.Find(reference));

        // Queued is not delivered.
        Assert.False(harness.Store.Find(reference)!.Forwarded);
    }

    [Fact]
    public void A_secret_never_reaches_the_wire_report()
    {
        using var harness = new Harness();

        harness.Pipeline.ReportHandled(
            new InvalidOperationException("connect failed: password=hunter2 token=abcdef"),
            "connecting",
            "the connection was not made");

        var queued = Assert.Single(harness.Outbox.Oldest(10));

        // Forwarding builds the wire report from the redacted local record, not from the exception.
        Assert.DoesNotContain("hunter2", queued.Detail, StringComparison.Ordinal);
        Assert.DoesNotContain("abcdef", queued.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_delivery_the_api_accepts_clears_the_outbox_and_marks_the_record_forwarded()
    {
        using var harness = new Harness(HttpStatusCode.NoContent);

        var reference = harness.Pipeline.ReportHandled(
            new InvalidOperationException("probe"),
            "loading a well",
            "the well was not shown");

        var before = await harness.Forwarder.DeliverOnceAsync();

        Assert.Equal(FailureDeliveryOutcome.Delivered, before.Outcome);
        Assert.Equal(0, harness.Outbox.Count);
        Assert.True(harness.Store.Find(reference)!.Forwarded);
    }

    [Fact]
    public async Task An_unreachable_api_keeps_the_report_and_does_not_mark_it_forwarded()
    {
        using var harness = new Harness(unreachable: true);

        var reference = harness.Pipeline.ReportHandled(
            new InvalidOperationException("probe"),
            "loading a well",
            "the well was not shown");

        var result = await harness.Forwarder.DeliverOnceAsync();

        Assert.Equal(FailureDeliveryOutcome.Deferred, result.Outcome);
        Assert.Equal(1, harness.Outbox.Count);
        Assert.False(harness.Store.Find(reference)!.Forwarded);
    }

    [Fact]
    public async Task A_report_the_api_refuses_is_not_marked_forwarded()
    {
        // 400 is "outside the contract": no retry changes it, so the library discards it, but the server
        // never stored it — claiming otherwise would make the administrator look for a record that is not there.
        using var harness = new Harness(HttpStatusCode.BadRequest);

        var reference = harness.Pipeline.ReportHandled(
            new InvalidOperationException("probe"),
            "loading a well",
            "the well was not shown");

        var result = await harness.Forwarder.DeliverOnceAsync();

        Assert.Equal(FailureDeliveryOutcome.Refused, result.Outcome);
        Assert.Equal(0, harness.Outbox.Count);
        Assert.False(harness.Store.Find(reference)!.Forwarded);
    }

    [Fact]
    public async Task A_queued_report_survives_a_restart_and_is_sent_by_the_next_session()
    {
        var directory = Path.Combine(OgPaths.DataRoot, "outbox-" + Guid.NewGuid().ToString("n")[..8]);
        var store = new SqliteFailureStore();

        string reference;

        // First session: reachable by nobody, so the report stays in the outbox.
        using (var first = new Harness(directory: directory, unreachable: true, store: store))
        {
            reference = first.Pipeline.ReportHandled(
                new InvalidOperationException("probe"),
                "loading a well",
                "the well was not shown");

            await first.Forwarder.DeliverOnceAsync();
            Assert.Equal(1, first.Outbox.Count);
        }

        // Second session: a fresh outbox over the same directory finds the report and delivers it.
        using (var second = new Harness(directory: directory, store: store))
        {
            var result = await second.Forwarder.DeliverOnceAsync();

            Assert.Equal(FailureDeliveryOutcome.Delivered, result.Outcome);
            Assert.Equal(0, second.Outbox.Count);
            Assert.True(store.Find(reference)!.Forwarded);
        }
    }

    [Fact]
    public void Forwarding_is_off_for_an_installation_with_no_api_configured()
    {
        using var application = new OgBootstrapper().Create();

        // The registration passes no API URL, so a fresh installation must not pretend to forward.
        Assert.False(application.FailureRelay.IsEnabled);
        Assert.Equal(0, application.FailureRelay.Pending);
    }

    private static OgFailureRecord Record(string reference) =>
        new(
            reference,
            DateTimeOffset.UtcNow,
            FailureSeverity.Error,
            "probe",
            "nothing shown",
            "System.InvalidOperationException",
            "probe",
            string.Empty,
            null,
            null,
            Forwarded: false);

    /// <summary>A pipeline, a real outbox and a forwarder wired to a stub API.</summary>
    private sealed class Harness : IDisposable
    {
        private static readonly Uri ApiRoot = new("https://api.example.test/");

        private readonly string _directory;
        private readonly StubApi? _api;

        public Harness(
            HttpStatusCode? accepts = null,
            bool unreachable = false,
            string? directory = null,
            IOgFailureStore? store = null)
        {
            _directory = directory ?? Path.Combine(OgPaths.DataRoot, "outbox-" + Guid.NewGuid().ToString("n")[..8]);
            Store = store ?? new SqliteFailureStore();
            Outbox = new FailureOutbox(_directory, NullLogger<FailureOutbox>.Instance);

            _api = unreachable ? null : new StubApi(accepts ?? HttpStatusCode.NoContent);

            var delivery = new FailureDelivery(
                Outbox,
                () => unreachable
                ? new HttpClient(new UnreachableHandler()) { BaseAddress = ApiRoot }
                : new HttpClient(_api!) { BaseAddress = ApiRoot },
                "Beep Oil & Gas Desktop test",
                NullLogger<FailureDelivery>.Instance);

            Forwarder = new OgFailureForwarder(Outbox, delivery, Store, NullLogger<OgFailureForwarder>.Instance);
            // No background loop here: the test drives DeliverOnceAsync itself, and a loop starting a
            // delivery at the same moment made the outcome depend on which one won.
            Forwarder.Enable(startDeliveryLoop: false);

            Pipeline = new OgDiagnosticsPipeline(
                new FailureStoreAccessor(Store),
                new SqliteAuditStore(),
                new OgRuntimeContext { UserId = "probe-user", CompanyId = "probe-company" },
                NullLogger<OgDiagnosticsPipeline>.Instance);

            Relay = new OgFailureRelay(Pipeline, Store, Forwarder);
        }

        public IOgFailureStore Store { get; }

        public FailureOutbox Outbox { get; }

        public OgFailureForwarder Forwarder { get; }

        public OgDiagnosticsPipeline Pipeline { get; }

        public OgFailureRelay Relay { get; }

        public void Dispose()
        {
            Relay.Dispose();
            Forwarder.Dispose();
            _api?.Dispose();
        }
    }

    private sealed class StubApi : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;

        public StubApi(HttpStatusCode status) => _status = status;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(_status));
    }

    private sealed class UnreachableHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            throw new HttpRequestException("probe: the api did not answer");
    }
}
