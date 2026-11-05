using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
namespace Beep.OilandGas.Models.Data.HSE;

// ActionId identifies one caller intent. Responsibility is a recorded PPDM participant, not an app user.
public sealed record AddCARequest(Guid ActionId, string CADescription, string CAType, DateTime DueDate, string ResponsibleBaId);
public sealed record AddCASubmission(FieldSelectionReview Context, AddCARequest Request, string PlanId, string OptionsRevision);
public sealed record AddCAReceipt(FieldSelectionReview Context, string IncidentId, Guid ActionId, string StepId);
public sealed record CACompletionReview(FieldSelectionReview Context, string IncidentId, string PlanId, int StepSequence, string StepId,
    string Description, string ResponsiblePerson, DateTime DueDate, string Revision);
public sealed record CompleteCASubmission(CACompletionReview Review, string Notes);
public sealed record CompleteCAReceipt(FieldSelectionReview Context, string IncidentId, string PlanId, int StepSequence, string StepId, string Notes);
public sealed record CAVerificationAction(int StepSequence, string StepId, string Description, string Status, string ResponsiblePerson,
    DateTime DueDate, DateTime? CompletedAt, string? CompletedBy, string? CompletionEvidence);
public sealed record CAVerificationReview(FieldSelectionReview Context, string IncidentId, string PlanId, string Revision, IReadOnlyList<CAVerificationAction> Actions)
{
    public void Require()
    {
        if (Context is null) throw new InvalidOperationException("A current FIELD review is required.");
        FieldSelectionReviewValidator.RequireReview(Context, Context.Company, Context.ActorUserId);
        if (!LeaseSummaryValidator.Identifier(Context.FieldId) || !LeaseSummaryValidator.Identifier(IncidentId) || !LeaseSummaryValidator.Identifier(PlanId) ||
            !ProcessDefinitionRevision.IsValid(Revision) || Actions is null || Actions.Count > 256 ||
            Actions.Any(a => a is null || a.StepSequence <= 0 || !LeaseSummaryValidator.Identifier(a.StepId) || string.IsNullOrWhiteSpace(a.Description) ||
                !LeaseSummaryValidator.Identifier(a.ResponsiblePerson) || a.DueDate.Kind != DateTimeKind.Utc || a.Status is not ("OPEN" or "COMPLETED") ||
                (a.Status == "OPEN" && (a.CompletedAt is not null || a.CompletedBy is not null || a.CompletionEvidence is not null)) ||
                (a.Status == "COMPLETED" && (a.CompletedAt is not { Kind: DateTimeKind.Utc } || !LeaseSummaryValidator.Identifier(a.CompletedBy) || string.IsNullOrWhiteSpace(a.CompletionEvidence)))) ||
            Actions.Select(a => a.StepSequence).Distinct().Count() != Actions.Count || Actions.Select(a => a.StepId).Distinct(StringComparer.Ordinal).Count() != Actions.Count)
            throw new InvalidOperationException("The corrective-action verification evidence is incomplete or ambiguous.");
    }
    public bool CanReviewIndependently => Actions.Count > 0 && Actions.All(a => a.Status == "COMPLETED" && a.CompletedBy != Context.ActorUserId);
}
public sealed record CorrectiveActionOptions(FieldSelectionReview Context, string IncidentId, string PlanId, string Revision,
    IReadOnlyList<string> ActionTypes, IReadOnlyList<string> Participants)
{
    public void Require()
    {
        if (Context is null) throw new InvalidOperationException("A current FIELD review is required.");
        FieldSelectionReviewValidator.RequireReview(Context, Context.Company, Context.ActorUserId);
        if (!LeaseSummaryValidator.Identifier(Context.FieldId) || !LeaseSummaryValidator.Identifier(IncidentId) || !LeaseSummaryValidator.Identifier(PlanId) ||
            !ProcessDefinitionRevision.IsValid(Revision) || ActionTypes is null || Participants is null || ActionTypes.Count > 256 || Participants.Count > 256 ||
            ActionTypes.Any(value => !LeaseSummaryValidator.Identifier(value)) || Participants.Any(value => !LeaseSummaryValidator.Identifier(value)) ||
            ActionTypes.Distinct(StringComparer.Ordinal).Count() != ActionTypes.Count || Participants.Distinct(StringComparer.Ordinal).Count() != Participants.Count)
            throw new InvalidOperationException("The corrective-action choices are incomplete or ambiguous.");
    }
}
