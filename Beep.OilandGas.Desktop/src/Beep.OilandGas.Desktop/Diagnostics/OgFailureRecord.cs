using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;

/// <summary>One stored failure. Never carries anything but the redacted exception facts.</summary>
public sealed record OgFailureRecord(
    string Reference,
    DateTimeOffset OccurredAtUtc,
    FailureSeverity Severity,
    string Operation,
    string Consequence,
    string ExceptionType,
    string Message,
    string Stack,
    string? UserId,
    string? CompanyId,
    bool Forwarded);

/// <summary>The durable failure store behind <see cref="IFailureReporter"/>.</summary>
public interface IOgFailureStore
{
    void Write(OgFailureRecord record);

    OgFailureRecord? Find(string reference);

    IReadOnlyList<OgFailureRecord> Recent(int take);

    int Count();

    /// <summary>Deletes records older than the retention window. Returns how many were removed.</summary>
    int Purge(DateTimeOffset olderThanUtc);

    /// <summary>
    /// Records that the reports are now in the application's store. Called only after the intake accepted
    /// them, so <c>Forwarded</c> never claims a delivery that did not happen.
    /// </summary>
    int MarkForwarded(IEnumerable<string> references);
}
