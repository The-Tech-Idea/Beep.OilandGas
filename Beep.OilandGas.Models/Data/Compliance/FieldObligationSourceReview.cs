using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
namespace Beep.OilandGas.Models.Data.Compliance;

/// <summary>Recorded PPDM component facts; no deadline, status or legal jurisdiction is inferred.</summary>
public sealed record RecordedObligationComponent(decimal Observation, string? ComponentType, DateTime? CriticalDate, string? FulfilledIndicator, DateTime? FulfilledDate, string? Description);

/// <summary>A recorded obligation and its explicitly linked components for one reviewed FIELD.</summary>
public sealed record RecordedFieldObligation(string ObligationId, decimal ObligationSequence, string? ObligationType, string? Description, DateTime? CriticalDate, string? FulfilledIndicator, DateTime? FulfilledDate, IReadOnlyList<RecordedObligationComponent> Components);

public sealed record FieldObligationSourceReview(FieldSelectionReview Context, IReadOnlyList<RecordedFieldObligation> Obligations);
