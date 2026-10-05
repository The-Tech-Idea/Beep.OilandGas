using TheTechIdeaWeb.Diagnostics;

namespace Beep.OilandGas.Web.Auth.Tests;

/// <summary>Records each report, and hands back a well-formed reference, for tests of code that reports.</summary>
internal sealed class RecordingFailureReporter : IFailureReporter
{
    public List<(Exception Exception, string Operation, FailureSeverity Severity)> Reports { get; } = [];

    public string ReportHandled(Exception exception, string operation, string consequence, FailureSeverity severity = FailureSeverity.Error)
    {
        lock (Reports)
        {
            Reports.Add((exception, operation, severity));
        }

        return FailureReferenceFormat.New();
    }
}
