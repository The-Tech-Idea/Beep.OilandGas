using System.Text.Json;
using System.Text.Json.Serialization;
namespace TheTechIdea.Data.OilGas;

public sealed record DrillingReportingClosureReview(
    [property:JsonRequired]DrillingReportingGeneration Responsibility,[property:JsonRequired]DrillingAcceptedCompletionTests Evidence,
    [property:JsonRequired]string ProcessRevision,[property:JsonRequired]string NativeRevision,
    [property:JsonRequired]string ManagerRole,[property:JsonRequired]string ManagerPersona,[property:JsonRequired]string AuthorityRevision,
    [property:JsonRequired]string Revision,[property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    public DevelopmentAssignedReviewScope Scope()=>Evidence.Acceptance.Source.History.Execution.Basis.Scope();
    public string ContentRevision()=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Responsibility=Responsibility.ContentRevision(),Evidence=Evidence.Revision,ProcessRevision,NativeRevision,ManagerRole,ManagerPersona,AuthorityRevision}));
    public void Require()
    {
        if(Responsibility is null || Evidence is null)throw new InvalidOperationException("The current reporting responsibility and accepted-test evidence are required.");
        Responsibility.Require();var root=Responsibility.Root;
        var scope=Scope();Evidence.Require(scope.Company,scope.ActorUserId,Evidence.Acceptance.InstanceId);root.RequireSource(Evidence.Acceptance.Source.History.Execution,false);
        if(scope.ActorUserId!=Responsibility.ManagerUserId || scope.Company!=root.Scope().Company || scope.CoreDatabase!=root.Scope().CoreDatabase || scope.LifecycleDatabase!=root.Scope().LifecycleDatabase ||
            ManagerRole!=Responsibility.ManagerRole || !LeaseSummaryValidator.Identifier(ManagerPersona) || ManagerPersona.Length>64 || !ProcessDefinitionRevision.IsValid(AuthorityRevision) ||
            !ProcessDefinitionRevision.IsValid(ProcessRevision) || !ProcessDefinitionRevision.IsValid(NativeRevision) ||
            Revision!=ContentRevision() || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero || CheckedAtUtc<Evidence.CheckedAtUtc)
            throw new InvalidOperationException("Closure requires the original accountable sender, current management role/workspace, original bindings and complete pending responsibility/test proof.");
    }
}
public sealed record DrillingReportingClosure([property:JsonRequired]DrillingReportingClosureReview Review,[property:JsonRequired]Guid IntentId,
    [property:JsonRequired]DateTimeOffset ClosedAtUtc,[property:JsonRequired]string Reason,[property:JsonRequired]bool Confirmed)
{
    public const string PayloadKey="DrillingReportingClosure";
    public DevelopmentAssignedReviewScope Scope()=>Review.Scope();
    public void Require()
    {
        if(Review is null)throw new InvalidOperationException("The exact reviewed reporting responsibility is required.");Review.Require();
        if(IntentId==Guid.Empty || !Confirmed || ClosedAtUtc==default || ClosedAtUtc.Offset!=TimeSpan.Zero || ClosedAtUtc<Review.CheckedAtUtc || ClosedAtUtc>DateTimeOffset.UtcNow ||
            !DrillingDailyReportInput.Text(Reason,true) || JsonSerializer.SerializeToUtf8Bytes(this).Length>DrillingExecutionAssignment.MaximumBodyLength)
            throw new InvalidOperationException("Retain the exact current closure review, explicit actual UTC, rationale and confirmation before closing reporting.");
    }
    public string Revision(){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(this));}
    public string HistoryId()=>"reporting-closure-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Instance=Review.Responsibility.Root.InstanceId(),IntentId}));
}
public sealed record DrillingReportingClosureReceipt([property:JsonRequired]string IntentRevision,[property:JsonRequired]string AssignmentId,[property:JsonRequired]bool Recorded,
    [property:JsonRequired]string? HistoryId,[property:JsonRequired]DateTimeOffset? ClosedAtUtc,[property:JsonRequired]IReadOnlyList<string> TaskIds,
    [property:JsonRequired]string NativeRevision,[property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    public void Require(DrillingReportingClosure original)
    {
        original.Require();if(IntentRevision!=original.Revision() || AssignmentId!=original.Review.Responsibility.Root.InstanceId() || TaskIds is null ||
                !TaskIds.Order(StringComparer.Ordinal).SequenceEqual(original.Review.Responsibility.TaskIds()) ||
            (Recorded?HistoryId!=original.HistoryId() || ClosedAtUtc!=original.ClosedAtUtc:HistoryId is not null || ClosedAtUtc is not null) ||
            !ProcessDefinitionRevision.IsValid(NativeRevision) || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero)
            throw new InvalidOperationException("The exact original reporting closure and complete task outcome were not confirmed.");
    }
}
