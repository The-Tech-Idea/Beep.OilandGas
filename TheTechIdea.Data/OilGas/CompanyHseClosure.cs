using System.Text.Json;
namespace TheTechIdea.Data.OilGas;

public sealed record CompanyHseClosureReview(IncidentClosureReview Evidence, AssetDatabaseScope LifecycleDatabase,
    string InstanceId, string StepInstanceId, string Stage, string AssignedRole, string GraphRevision, string NativeRevision)
{
    public FieldSelectionReview Context => Evidence.Actions.Context;
    public IncidentRcaRecordedCause? RecordedCause { get; init; }
    public Guid? CorrectionRound { get; init; }
    public void Require() => RequireCore(false);
    public void RequireCorrectionReview() => RequireCore(true);
    private void RequireCore(bool correction)
    {
        if (Evidence is null || LifecycleDatabase is null) throw new InvalidOperationException("Recorded closure evidence is required.");
        Evidence.Require();
        if (RecordedCause is { } cause)
        {
            cause.Require();
            if (cause.Company != Context.Company || cause.Database != Context.Database || cause.FieldId != Context.FieldId || cause.IncidentId != Evidence.Actions.IncidentId) throw new InvalidOperationException("The closure cause belongs to another company, FIELD or incident.");
        }
        if (Stage is not ("INC_CORRECTIVE_ACTIONS" or "INC_VERIFICATION" or "INC_CLOSURE") ||
            !LeaseSummaryValidator.Identifier(InstanceId) || !LeaseSummaryValidator.Identifier(StepInstanceId) || !LeaseSummaryValidator.Identifier(AssignedRole) ||
            !LeaseSummaryValidator.Identifier(LifecycleDatabase.ConnectionName) || !LeaseSummaryValidator.Identifier(LifecycleDatabase.Fingerprint) ||
            !ProcessDefinitionRevision.IsValid(GraphRevision) || !ProcessDefinitionRevision.IsValid(NativeRevision) ||
            Evidence.Actions.Actions.Count == 0 || Evidence.Actions.Actions.Any(a => a.Status != "COMPLETED") ||
            CorrectionRound == Guid.Empty ||
            (correction ? Stage != "INC_VERIFICATION" || !Evidence.Actions.CanReviewIndependently ||
                Evidence.Decisions.Count(d => d.Review.Revision == Evidence.Actions.Revision && d.Decision == "REJECTED") != 1 :
                Stage != "INC_CORRECTIVE_ACTIONS" && (!Evidence.VerificationSatisfied || !Evidence.Actions.CanReviewIndependently)))
            throw new InvalidOperationException("The current assigned closure stage and independent verified evidence are required.");
    }
}
public sealed record CompanyHseClosureCommand(Guid IntentId, CompanyHseClosureReview Review, string Reason)
{
    public void Require()
    {
        if (Review is null) throw new InvalidOperationException("An original closure review is required."); Review.Require();
        if (IntentId == Guid.Empty || string.IsNullOrWhiteSpace(Reason) || Reason != Reason.Trim() || Reason.Length is < 20 or > 2000 || Reason.Any(c => char.IsControl(c) && c is not ('\r' or '\n')) || JsonSerializer.Serialize(this).Length > 32768)
            throw new InvalidOperationException("An explicit bounded approval reason and original identity are required.");
    }
}
public sealed record CompanyHseClosureReceipt(Guid IntentId, CompanyScope Company, string Actor, string InstanceId, string StepInstanceId,
    string Stage, string HistoryId, string? ApprovalId, string? NextStepInstanceId, IReadOnlyList<string> CompletedTasks, IReadOnlyList<string> NextTasks, string GraphRevision)
{
    public void Require(CompanyHseClosureCommand original)
    {
        original.Require(); var r = original.Review;
        if (IntentId != original.IntentId || Company != r.Context.Company || Actor != r.Context.ActorUserId || InstanceId != r.InstanceId || StepInstanceId != r.StepInstanceId || Stage != r.Stage ||
            !LeaseSummaryValidator.Identifier(HistoryId) || (Stage == "INC_CORRECTIVE_ACTIONS" ? ApprovalId is not null : !LeaseSummaryValidator.Identifier(ApprovalId)) ||
            (Stage == "INC_CLOSURE" ? NextStepInstanceId is not null || NextTasks is not { Count: 0 } : !LeaseSummaryValidator.Identifier(NextStepInstanceId) || NextTasks is not { Count: > 0 }) ||
            CompletedTasks is not { Count: > 0 and <= 128 } || NextTasks is null || NextTasks.Count > 128 ||
            CompletedTasks.Concat(NextTasks).Any(id => !LeaseSummaryValidator.Identifier(id)) || CompletedTasks.Concat(NextTasks).Distinct(StringComparer.Ordinal).Count() != CompletedTasks.Count + NextTasks.Count || !ProcessDefinitionRevision.IsValid(GraphRevision))
            throw new InvalidOperationException("The lifecycle closure receipt differs from the original review.");
    }
}
public sealed record CompanyHseClosureRecovery(CompanyHseClosureCommand Original, CompanyHseClosureReceipt Receipt)
{
    public void Require() { if (Original is null || Receipt is null) throw new InvalidOperationException("Recorded closure recovery is unavailable."); Receipt.Require(Original); }
}
public sealed record IncidentClosedReceipt(CompanyHseClosureCommand Original, CompanyHseClosureReceipt Lifecycle, long StatusObservation)
{
    public IncidentClosureResolution? Resolution { get; init; }
    public void Require() { if (Original is null || Lifecycle is null || Original.Review.Stage != "INC_CLOSURE" || StatusObservation <= 0) throw new InvalidOperationException("A confirmed closed incident is required."); Lifecycle.Require(Original); if (Resolution is { } resolution) { resolution.Require(); resolution.Review.RequireAgainst(Original, Lifecycle); } }
}
public sealed record CompanyHseClosureOutcome(CompanyHseClosureCommand Original, CompanyHseClosureReceipt Lifecycle, IncidentClosedReceipt? Native)
{
    public void Require()
    {
        Lifecycle.Require(Original);
        if (Original.Review.Stage == "INC_CLOSURE")
        {
            if (Native is null) throw new InvalidOperationException("The native incident CLOSED status has not been confirmed."); Native.Require();
            if (JsonSerializer.Serialize(Native.Original) != JsonSerializer.Serialize(Original) || JsonSerializer.Serialize(Native.Lifecycle) != JsonSerializer.Serialize(Lifecycle)) throw new InvalidOperationException("The native incident closure differs from the lifecycle approval.");
        }
        else if (Native is not null) throw new InvalidOperationException("Only the terminal lifecycle stage can close the native incident.");
    }
}
