using TheTechIdeaWeb.Diagnostics;

namespace Beep.OilandGas.Repository.Tests;

/// <summary>
/// The failure reporter these tests register in place of the host's store (<c>AddOilGasDiagnostics</c>): it keeps what was
/// reported, so a test can ask, and answers with a well-formed reference.
/// </summary>
public sealed class RecordingFailureReporter : IFailureReporter
{
    private readonly List<(Exception Exception, string Operation, FailureSeverity Severity)> _reports = [];

    /// <summary>What was reported, oldest first.</summary>
    public IReadOnlyList<(Exception Exception, string Operation, FailureSeverity Severity)> Reports
    {
        get
        {
            lock (_reports)
            {
                return [.. _reports];
            }
        }
    }

    public string ReportHandled(Exception exception, string operation, string consequence, FailureSeverity severity = FailureSeverity.Error)
    {
        lock (_reports)
        {
            _reports.Add((exception, operation, severity));
        }

        return FailureReferenceFormat.New();
    }
}
