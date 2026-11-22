using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// FND-06: the failure path. A reference is always well formed, the record is durable, a secret never
/// survives, and a broken store degrades instead of throwing back into the catch.
/// </summary>
public sealed class DiagnosticsPipelineTests
{
    private static OgDiagnosticsPipeline Build(IOgFailureStore? store = null)
    {
        var context = new OgRuntimeContext { UserId = "test-user", CompanyId = "test-company" };
        return new OgDiagnosticsPipeline(
            new FailureStoreAccessor(store ?? new SqliteFailureStore()),
            new SqliteAuditStore(),
            context,
            NullLogger<OgDiagnosticsPipeline>.Instance);
    }

    [Fact]
    public void ReportHandled_returns_a_well_formed_reference()
    {
        var pipeline = Build();

        var reference = pipeline.ReportHandled(
            new InvalidOperationException("boom"),
            "loading a page",
            "nothing was shown");

        Assert.True(FailureReferenceFormat.IsWellFormed(reference), reference);
    }

    [Fact]
    public void ReportHandled_stores_the_record_under_its_reference()
    {
        var store = new SqliteFailureStore();
        var pipeline = Build(store);

        var reference = pipeline.ReportHandled(
            new InvalidOperationException("boom"),
            "loading a page",
            "nothing was shown",
            FailureSeverity.Error);

        var stored = store.Find(reference);

        Assert.NotNull(stored);
        Assert.Equal(FailureSeverity.Error, stored!.Severity);
        Assert.Equal("loading a page", stored.Operation);
        Assert.Equal("nothing was shown", stored.Consequence);
    }

    [Fact]
    public void ReportHandled_redacts_a_secret_from_the_message()
    {
        var store = new SqliteFailureStore();
        var pipeline = Build(store);

        var reference = pipeline.ReportHandled(
            new InvalidOperationException("connect failed: password=hunter2 token=abcdef"),
            "connecting",
            "the connection was not made");

        var stored = store.Find(reference);

        Assert.NotNull(stored);
        Assert.DoesNotContain("hunter2", stored!.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("abcdef", stored.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ReportHandled_never_throws_when_the_store_fails()
    {
        var pipeline = Build(new ThrowingFailureStore());

        var reference = pipeline.ReportHandled(
            new InvalidOperationException("boom"),
            "loading a page",
            "nothing was shown");

        Assert.True(FailureReferenceFormat.IsWellFormed(reference));
        Assert.True(pipeline.IsDegraded);
        Assert.False(string.IsNullOrWhiteSpace(pipeline.DegradedReason));
    }

    [Fact]
    public void Add_writes_exactly_one_audit_row()
    {
        // A private file: this asserts an exact count, so a parallel test writing to the shared store would
        // make it flaky rather than wrong.
        var audit = new SqliteAuditStore(PrivateAuditPath());
        var before = audit.Count();
        var pipeline = new OgDiagnosticsPipeline(
            new FailureStoreAccessor(new SqliteFailureStore()),
            audit,
            new OgRuntimeContext { UserId = "auditor", CompanyId = "c1" },
            NullLogger<OgDiagnosticsPipeline>.Instance);

        pipeline.Add("well.status.change", "well", "W-1", "ok", "status set to producing");

        Assert.Equal(before + 1, audit.Count());
        var row = audit.Recent(1)[0];
        Assert.Equal("well.status.change", row.Verb);
        Assert.Equal("W-1", row.SubjectId);
        Assert.Equal("auditor", row.UserId);
    }

    [Fact]
    public void Add_records_a_refusal_as_an_audit_row()
    {
        var audit = new SqliteAuditStore(PrivateAuditPath());
        var pipeline = new OgDiagnosticsPipeline(
            new FailureStoreAccessor(new SqliteFailureStore()),
            audit,
            new OgRuntimeContext(),
            NullLogger<OgDiagnosticsPipeline>.Instance);

        pipeline.Add("well.status.change", "well", "W-2", "refused", "period is closed");

        var row = audit.Recent(1)[0];
        Assert.Equal("refused", row.Outcome);
        Assert.Equal("period is closed", row.Detail);
    }

    [Fact]
    public void A_reported_failure_is_in_the_store_and_in_the_file_log()
    {
        var logDirectory = Path.Combine(OgPaths.DataRoot, "log-" + Guid.NewGuid().ToString("n")[..8]);
        using var provider = new OgFileLoggerProvider(logDirectory);

        var store = new SqliteFailureStore();
        var pipeline = new OgDiagnosticsPipeline(
            new FailureStoreAccessor(store),
            new SqliteAuditStore(PrivateAuditPath()),
            new OgRuntimeContext { UserId = "operator" },
            new ForwardingLogger<OgDiagnosticsPipeline>(provider.CreateLogger("probe")));

        var reference = pipeline.ReportHandled(
            new InvalidOperationException("boom"),
            "loading a well",
            "nothing was shown");

        // AC-6: the record, and a log line that can be joined to it by the reference.
        Assert.NotNull(store.Find(reference));
        Assert.True(OgPaths.IsWritableLocation(logDirectory), "The log must live under the data root.");

        var file = Assert.Single(Directory.GetFiles(logDirectory, "*.log"));
        var text = File.ReadAllText(file);
        Assert.Contains(reference, text, StringComparison.Ordinal);
        Assert.Contains("loading a well", text, StringComparison.Ordinal);
    }

    private static string PrivateAuditPath() =>
        Path.Combine(OgPaths.DataRoot, "audit-" + Guid.NewGuid().ToString("n")[..8], "failures.db");

    /// <summary>Adapts a non-generic logger so a test can route the pipeline's lines to a chosen provider.</summary>
    private sealed class ForwardingLogger<T>(ILogger inner) : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull =>
            inner.BeginScope(state);

        public bool IsEnabled(LogLevel logLevel) => inner.IsEnabled(logLevel);

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter) =>
            inner.Log(logLevel, eventId, state, exception, formatter);
    }

    private sealed class ThrowingFailureStore : IOgFailureStore
    {
        public void Write(OgFailureRecord record) => throw new IOException("store unavailable");

        public OgFailureRecord? Find(string reference) => null;

        public IReadOnlyList<OgFailureRecord> Recent(int take) => [];

        public int Count() => 0;

        public int Purge(DateTimeOffset olderThanUtc) => 0;

        public int MarkForwarded(IEnumerable<string> references) => 0;
    }
}
