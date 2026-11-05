using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.HSE;
namespace Beep.OilandGas.Models.Data.HSE;

/// <summary>Read-only closure prerequisites. This evidence is not a lifecycle approval or closure receipt.</summary>
public sealed record IncidentClosureReview(CAVerificationReview Actions, IReadOnlyList<CAVerificationDecision> Decisions)
{
    public bool VerificationSatisfied => Actions.Actions.Count > 0 && Actions.Actions.All(action => action.Status == "COMPLETED") &&
        Decisions.Count(decision => decision.Review.Revision == Actions.Revision) == 1 &&
        Decisions.Any(decision => decision.Review.Revision == Actions.Revision && decision.Decision == "VERIFIED");

    public void Require()
    {
        if (Actions is null || Decisions is null || Decisions.Count > 256) throw new InvalidOperationException("Closure evidence is unavailable.");
        Actions.Require();
        foreach (var decision in Decisions)
        {
            if (decision is null) throw new InvalidOperationException("Closure verification is unavailable.");
            decision.Require(); var recorded = decision.Review;
            if (recorded.Context.Company != Actions.Context.Company || recorded.Context.Database != Actions.Context.Database ||
                recorded.Context.FieldId != Actions.Context.FieldId || recorded.IncidentId != Actions.IncidentId || recorded.PlanId != Actions.PlanId ||
                recorded.Revision == Actions.Revision && !recorded.Actions.SequenceEqual(Actions.Actions))
                throw new InvalidOperationException("Closure verification belongs to another incident, plan or action population.");
        }
        if (Decisions.Select(x => x.IntentId).Distinct().Count() != Decisions.Count || Decisions.Select(x => x.Review.Revision).Distinct(StringComparer.Ordinal).Count() != Decisions.Count)
            throw new InvalidOperationException("Closure verification is ambiguous.");
    }
}
