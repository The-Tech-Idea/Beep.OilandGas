namespace TheTechIdea.Data.OilGas;

/// <summary>Recorded RCA evidence and the exact current lifecycle step reviewed for handoff.</summary>
public sealed record CompanyHseRcaHandoffReview(FieldSelectionReview Context, AssetDatabaseScope LifecycleDatabase,
    IncidentRcaRecordedCause Cause, string InstanceId, string StepInstanceId, string AssignedRole, string GraphRevision)
{
    public void Require()
    {
        if (Context is null || LifecycleDatabase is null || Cause is null) throw new InvalidOperationException("The company, lifecycle and recorded cause are required.");
        FieldSelectionReviewValidator.RequireReview(Context, Context.Company, Context.ActorUserId); Cause.Require();
        if (Cause.Company != Context.Company || Cause.Database != Context.Database || Cause.FieldId != Context.FieldId || !LeaseSummaryValidator.Identifier(LifecycleDatabase.ConnectionName) ||
            !LeaseSummaryValidator.Identifier(LifecycleDatabase.Fingerprint) || !LeaseSummaryValidator.Identifier(InstanceId) ||
            !LeaseSummaryValidator.Identifier(StepInstanceId) || !LeaseSummaryValidator.Identifier(AssignedRole) || !ProcessDefinitionRevision.IsValid(GraphRevision))
            throw new InvalidOperationException("The reviewed cause and lifecycle ownership do not match.");
    }
}

public sealed record CompanyHseRcaHandoffCommand(Guid IntentId, CompanyHseRcaHandoffReview Review)
{
    public void Require()
    {
        if (IntentId == Guid.Empty || Review is null) throw new InvalidOperationException("The original handoff review is required.");
        Review.Require();
    }
}

public sealed record CompanyHseRcaHandoffReceipt(CompanyScope Company, string Actor, Guid IntentId, string InstanceId,
    string StepInstanceId, string HistoryId, string? NextStepInstanceId, IReadOnlyList<string> CompletedTasks, IReadOnlyList<string> NextTasks, string GraphRevision)
{
    public void Require(CompanyHseRcaHandoffCommand original)
    {
        ArgumentNullException.ThrowIfNull(original); original.Require(); var review = original.Review;
        if (Company != review.Context.Company || Actor != review.Context.ActorUserId || IntentId != original.IntentId || InstanceId != review.InstanceId ||
            StepInstanceId != review.StepInstanceId || !LeaseSummaryValidator.Identifier(HistoryId) ||
            NextStepInstanceId is not null && !LeaseSummaryValidator.Identifier(NextStepInstanceId) || CompletedTasks is not { Count: > 0 and <= 128 } ||
            NextTasks is null || NextTasks.Count > 128 || CompletedTasks.Concat(NextTasks).Any(id => !LeaseSummaryValidator.Identifier(id)) ||
            CompletedTasks.Concat(NextTasks).Distinct(StringComparer.Ordinal).Count() != CompletedTasks.Count + NextTasks.Count || !ProcessDefinitionRevision.IsValid(GraphRevision))
            throw new InvalidOperationException("The recorded RCA handoff does not match the original review.");
    }
}

public sealed record CompanyHseRcaRecoveredHandoff(CompanyHseRcaHandoffCommand Original, CompanyHseRcaHandoffReceipt Receipt)
{
    public void Require()
    {
        if (Original is null || Receipt is null) throw new InvalidOperationException("The recorded RCA handoff and receipt are required.");
        Original.Require(); Receipt.Require(Original);
    }
}
