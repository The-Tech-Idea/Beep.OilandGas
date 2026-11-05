using System.Text.Json;
using System.Text.Json.Serialization;
namespace TheTechIdea.Data.OilGas;

/// <summary>Original deterministic assignment graph absent; a different qualified responsibility owns the same execution.</summary>
public sealed record DrillingExecutionAssignmentConflictReview(
    [property:JsonRequired]string IntentRevision,[property:JsonRequired]string OriginalInstanceId,
    [property:JsonRequired]bool OriginalConfirmedAbsent,[property:JsonRequired]DrillingExecutionAssignment Competing,
    [property:JsonRequired]DrillingExecutionAssignmentReceipt Outcome,[property:JsonRequired]string Revision,
    [property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    public string ContentRevision()=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{
        IntentRevision,OriginalInstanceId,OriginalConfirmedAbsent,Competing,
        Outcome=new{Outcome.IntentRevision,Outcome.InstanceId,Outcome.Recorded,Outcome.StepInstanceId,Outcome.TaskIds,Outcome.RecordedRevision}}));
    public void Require(DrillingExecutionAssignment original)
    {
        original.Require();
        if(Competing is null || Outcome is null)throw new InvalidOperationException("An exact different recorded reporting responsibility is required.");
        Competing.Require();Outcome.Require(Competing);original.RequireSource(Competing.Source,false);
        if(IntentRevision!=original.Revision() || OriginalInstanceId!=original.InstanceId() || !OriginalConfirmedAbsent || !Outcome.Recorded ||
            Competing.InstanceId()==OriginalInstanceId || Competing.IntentId==original.IntentId || Revision!=ContentRevision() ||
            CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero || Outcome.CheckedAtUtc>CheckedAtUtc)
            throw new InvalidOperationException("The original graph absence and exact different reporting responsibility are unconfirmed.");
    }
}
// Submit only the reviewed digest alongside the retained original; do not duplicate two large originals in the request body.
public sealed record DrillingExecutionAssignmentConflictAcknowledgement(
    [property:JsonRequired]DrillingExecutionAssignment Original,[property:JsonRequired]string ReviewRevision)
{
    public void Require()
    {
        if(Original is null || !ProcessDefinitionRevision.IsValid(ReviewRevision))throw new InvalidOperationException("The retained original and exact reviewed responsibility revision are required.");
        Original.Require();
    }
}
