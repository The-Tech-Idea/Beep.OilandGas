using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheTechIdea.Data.OilGas;

/// <summary>Read-only proof that this original is absent and a different exact start owns the approved program.</summary>
public sealed record DrillingExecutionConflictReview(
    [property: JsonRequired] string IntentRevision,
    [property: JsonRequired] string OriginalOperationId,
    [property: JsonRequired] bool OriginalConfirmedAbsent,
    [property: JsonRequired] DRILLING_OPERATION Competing,
    [property: JsonRequired] string Revision,
    [property: JsonRequired] DateTimeOffset CheckedAtUtc)
{
    public string ContentRevision() => ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new {
        IntentRevision, OriginalOperationId, OriginalConfirmedAbsent, Competing }));
    public void Require(DrillingExecutionStart original)
    {
        original.Require();
        if (Competing is null) throw new InvalidOperationException("An exact different recorded execution is required for reconciliation.");
        var recorded = DrillingExecutionStart.FromRecord(Competing);
        original.Basis.RequireSource(recorded.Basis, false, false);
        if (IntentRevision != original.Revision() || OriginalOperationId != original.OperationId() || !OriginalConfirmedAbsent ||
            Competing.DRILLING_OPERATION_ID == OriginalOperationId || recorded.IntentId == original.IntentId ||
            Revision != ContentRevision() || CheckedAtUtc == default || CheckedAtUtc.Offset != TimeSpan.Zero)
            throw new InvalidOperationException("The absent original and exact different recorded execution could not be confirmed.");
    }
}

public sealed record DrillingExecutionConflictAcknowledgement(
    [property: JsonRequired] DrillingExecutionStart Original,
    [property: JsonRequired] DrillingExecutionConflictReview Review)
{
    public void Require()
    {
        if (Original is null || Review is null) throw new InvalidOperationException("The retained original and reviewed competing execution are required.");
        Review.Require(Original);
    }
}
