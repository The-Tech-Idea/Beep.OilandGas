using TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// FND-06 AC-7: a task nobody awaited is the only failure with no other record, so the process-wide
/// subscription must turn it into one.
/// </summary>
public sealed class UnobservedFailureTests
{
    [Fact]
    public async Task An_unobserved_task_exception_reaches_the_reporter()
    {
        var reporter = new RecordingFailureReporter();
        using var subscription = ProcessFailureSubscription.Start(
            reporter,
            whenEnding: "the process ended before the failure could be shown");

        CreateOrphanTask();

        // The event fires from the task's finalizer, so the test has to ask for a collection and wait for
        // the finalizer thread rather than sleeping a fixed time.
        for (var attempt = 0; attempt < 100 && reporter.Count == 0; attempt++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            await Task.Delay(25);
        }

        Assert.True(
            reporter.Count > 0,
            "An unobserved task exception never reached the reporter within ~2.5 s of forced collections.");

        var report = Assert.Single(reporter.Reports);
        Assert.Equal("running a task nothing awaited", report.Operation);
        Assert.Contains("nothing observed", report.Consequence, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("probe: orphan", report.ExceptionMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_disposed_subscription_reports_nothing_more()
    {
        var reporter = new RecordingFailureReporter();

        var subscription = ProcessFailureSubscription.Start(reporter, "the process ended");
        subscription.Dispose();

        CreateOrphanTask();

        for (var attempt = 0; attempt < 40 && reporter.Count == 0; attempt++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            await Task.Delay(25);
        }

        // Unsubscribed, so nothing is written — the report a disposed host would have made must not arrive.
        Assert.Equal(0, reporter.Count);
    }

    /// <summary>
    /// Starts a task that throws and does not keep a reference to it. The compiler and the runtime must not
    /// observe it: that is what makes it an "unobserved task exception".
    /// </summary>
    private static void CreateOrphanTask()
    {
        var task = Task.Run(() => throw new InvalidOperationException("probe: orphan"));
        GC.KeepAlive(task);
    }

    private sealed class RecordingFailureReporter : IFailureReporter
    {
        private readonly object _gate = new();
        private readonly List<Report> _reports = [];

        public int Count
        {
            get
            {
                lock (_gate)
                {
                    return _reports.Count;
                }
            }
        }

        public IReadOnlyList<Report> Reports
        {
            get
            {
                lock (_gate)
                {
                    return _reports.ToList();
                }
            }
        }

        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error)
        {
            lock (_gate)
            {
                _reports.Add(new Report(operation, consequence, severity, exception.Message));
            }

            return FailureReferenceFormat.New();
        }

        public sealed record Report(
            string Operation,
            string Consequence,
            FailureSeverity Severity,
            string ExceptionMessage);
    }
}
