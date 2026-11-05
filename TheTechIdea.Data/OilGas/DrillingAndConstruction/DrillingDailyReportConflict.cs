using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheTechIdea.Data.OilGas;

/// <summary>Exact original absent; a different immutable report occupies its reviewed sequence position.</summary>
public sealed record DrillingDailyReportConflictReview(
    [property: JsonRequired] string IntentRevision,
    [property: JsonRequired] string OriginalReportId,
    [property: JsonRequired] bool OriginalConfirmedAbsent,
    [property: JsonRequired] DRILLING_DAILY_REPORT Competing,
    [property: JsonRequired] string HistoryRevision,
    [property: JsonRequired] int ReportCount,
    [property: JsonRequired] string Revision,
    [property: JsonRequired] DateTimeOffset CheckedAtUtc)
{
    public string ContentRevision() => ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new
        { IntentRevision, OriginalReportId, OriginalConfirmedAbsent, Competing, HistoryRevision, ReportCount }));
    public void Require(DrillingDailyReportInput original)
    {
        original.Require();
        var recorded = DrillingDailyReportInput.FromRecord(Competing);
        original.Basis.RequireExecution(recorded.Basis.Execution, false);
        if (IntentRevision != original.Revision() || OriginalReportId != original.ReportId() || !OriginalConfirmedAbsent ||
            recorded.IntentId == original.IntentId || Competing.DDR_ID == OriginalReportId ||
            recorded.Basis.NextNumber != original.Basis.NextNumber ||
            recorded.Basis.PreviousReportId != original.Basis.PreviousReportId ||
            recorded.Basis.PreviousReportRevision != original.Basis.PreviousReportRevision ||
            recorded.Basis.PreviousPeriodEndUtc != original.Basis.PreviousPeriodEndUtc ||
            !ProcessDefinitionRevision.IsValid(HistoryRevision) || ReportCount < original.Basis.NextNumber ||
            Revision != ContentRevision() ||
            CheckedAtUtc == default || CheckedAtUtc.Offset != TimeSpan.Zero || Competing.ROW_CHANGED_DATE > CheckedAtUtc.UtcDateTime)
            throw new InvalidOperationException("The exact original absence, report predecessor and different recorded observations are unconfirmed.");
    }
}

// The retained original and reviewed digest suffice; do not duplicate two large source proofs in a request.
public sealed record DrillingDailyReportConflictAcknowledgement(
    [property: JsonRequired] DrillingDailyReportInput Original,
    [property: JsonRequired] string ReviewRevision)
{
    public void Require()
    {
        if (Original is null || !ProcessDefinitionRevision.IsValid(ReviewRevision))
            throw new InvalidOperationException("The retained original report and exact reviewed revision are required.");
        Original.Require();
    }
}
