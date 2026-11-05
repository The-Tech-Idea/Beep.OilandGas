using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheTechIdea.Data.OilGas;

/// <summary>Recorder-reported supporting reference, never an inferred physical test or independent acceptance.</summary>
public sealed record DrillingCompletionTestReference(
    [property: JsonRequired] string TestType,
    [property: JsonRequired] string Reference,
    [property: JsonRequired] DateTimeOffset TestedAtUtc,
    [property: JsonRequired] string ReportedResult,
    [property: JsonRequired] string Provenance,
    [property: JsonRequired] string ResultNotes)
{
    public void Require(DateTimeOffset? recordedAt)
    {
        if (!DrillingDailyReportInput.Text(TestType, true) || TestType.Length > 128 ||
            !DrillingDailyReportInput.Text(Reference, true) || !DrillingDailyReportInput.Text(Provenance, true) ||
            !DrillingDailyReportInput.Text(ResultNotes, true) ||
            ReportedResult is not ("REPORTED_PASS" or "REPORTED_FAIL" or "INCONCLUSIVE") ||
            TestedAtUtc == default || TestedAtUtc.Offset != TimeSpan.Zero ||
            recordedAt is {} at && TestedAtUtc > at)
            throw new InvalidOperationException("Provide each supporting test's type, exact reference, actual UTC, explicitly reported result, provenance and result notes. References are not independently accepted tests.");
    }
}

public sealed record DrillingCompletionReport(
    [property: JsonRequired] DateTimeOffset CompletedAtUtc,
    [property: JsonRequired] string WorkCompleted,
    [property: JsonRequired] IReadOnlyList<DrillingCompletionTestReference> Tests,
    [property: JsonRequired] IReadOnlyList<string> OutstandingItems,
    [property: JsonRequired] bool OutstandingItemsReviewed)
{
    public const int MaximumBodyLength = 256 * 1024;
    public void Require(DateTimeOffset spud, DateTimeOffset observedEnd, DateTimeOffset? recordedAt = null)
    {
        if (CompletedAtUtc == default || CompletedAtUtc.Offset != TimeSpan.Zero || CompletedAtUtc < observedEnd || CompletedAtUtc < spud ||
            recordedAt is {} at && CompletedAtUtc > at || !DrillingDailyReportInput.Text(WorkCompleted, true) ||
            !OutstandingItemsReviewed || Tests is null || Tests.Count is < 1 or > 64 || Tests.Any(row => row is null) ||
            OutstandingItems is null || OutstandingItems.Count > 64 || OutstandingItems.Any(row => !DrillingDailyReportInput.Text(row, true)) ||
            OutstandingItems.Distinct(StringComparer.Ordinal).Count() != OutstandingItems.Count ||
            JsonSerializer.SerializeToUtf8Bytes(this).Length > MaximumBodyLength)
            throw new InvalidOperationException("Explicit completion UTC after the latest observed period, work performed, supporting test references and review of all known outstanding work/defects are required. This report is pending independent acceptance.");
        foreach (var test in Tests) test.Require(recordedAt);
        if (Tests.Select(row => JsonSerializer.Serialize(new { row.TestType, row.Reference, row.TestedAtUtc })).Distinct(StringComparer.Ordinal).Count() != Tests.Count)
            throw new InvalidOperationException("Supporting test references must identify distinct reported tests.");
    }
}

/// <summary>The current latest observed period's completion report, not completion acceptance or production release.</summary>
public sealed record DrillingReportedCompletion(
    [property: JsonRequired] string ReportId,
    [property: JsonRequired] int Number,
    [property: JsonRequired] string NativeRevision,
    [property: JsonRequired] DrillingCompletionReport Report,
    [property: JsonRequired] string RecorderUserId,
    [property: JsonRequired] string RecorderRole,
    [property: JsonRequired] string RecorderPersonaCode,
    [property: JsonRequired] DateTimeOffset RecordedAtUtc)
{
    public void Require(DrillingDailyReportHistory history)
    {
        var latest = history.LatestObservedReport;
        if (latest is null || ReportId != latest.ReportId || Number != latest.Number || NativeRevision != latest.NativeRevision ||
            !LeaseSummaryValidator.Identifier(RecorderUserId) || !LeaseSummaryValidator.Identifier(RecorderRole) ||
            !LeaseSummaryValidator.Identifier(RecorderPersonaCode) || RecorderPersonaCode.Length > 64 || Report is null ||
            RecordedAtUtc == default || RecordedAtUtc.Offset != TimeSpan.Zero || RecordedAtUtc > history.CheckedAtUtc)
            throw new InvalidOperationException("The current reported completion, latest observation and accountable recorder are unconfirmed.");
        var spud = new DateTimeOffset(DateTime.SpecifyKind(history.Execution.Recorded!.SPUD_DATE!.Value, DateTimeKind.Utc));
        Report.Require(spud, latest.LatestPeriodEndUtc, RecordedAtUtc);
    }
}
