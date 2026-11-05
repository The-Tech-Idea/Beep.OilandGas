using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Models.Data;

/// <summary>Immutable original draft input. Authoritative company/FIELD access is independently checked by the API.</summary>
public sealed record CreateDevelopmentPlan
{
    public required FieldSelectionReview SelectionContext { get; init; }
    public required AssetDatabaseScope PlanningDatabase { get; init; }
    public required Guid IntentId { get; init; }
    public required string PlanName { get; init; }
    public required string FieldId { get; init; }
    public string? Description { get; init; }
    public DateOnly? PlannedFirstProductionDate { get; init; }
    public DateOnly? PlannedFieldLifeEndDate { get; init; }
    /// <summary>Millions of the explicit CapexCurrency, matching TOTAL_CAPEX_MM. No currency is assumed.</summary>
    public decimal? EstimatedCapexMillions { get; init; }
    public string? CapexCurrency { get; init; }
}
