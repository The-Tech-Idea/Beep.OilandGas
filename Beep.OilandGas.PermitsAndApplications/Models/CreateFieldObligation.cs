using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
namespace Beep.OilandGas.PermitsAndApplications.Models;

/// <summary>An explicit request to record PPDM facts; it does not establish a legal deadline or filing.</summary>
public sealed record CreateFieldObligation(FieldSelectionReview Context, Guid IntentId, string ObligationType,
    string ComponentType, string Description, DateTime? CriticalDate);

public sealed record CreatedFieldObligation(FieldSelectionReview Context, string ObligationId, decimal Sequence, decimal ComponentObservation);

public sealed record FieldObligationCreationOptions(FieldSelectionReview Context, IReadOnlyList<string> ObligationTypes, IReadOnlyList<string> ComponentTypes);

public static class FieldObligationIdentity
{
    public static bool SameTarget(FieldSelectionReview? original, FieldSelectionReview? current) =>
        original is not null && current is not null && original.Company == current.Company && original.ActorUserId == current.ActorUserId &&
        original.Database == current.Database && original.FieldId is not null && original.FieldId == current.FieldId;
}
