using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;

namespace TheTechIdea.Data.OilGas;

public sealed record DevelopmentPlanDraftOptions(
    [property: JsonRequired] FieldSelectionReview Context,
    [property: JsonRequired] AssetDatabaseScope PlanningDatabase,
    [property: JsonRequired] IReadOnlyList<string> Currencies);

public sealed record DevelopmentPlanDraftList(
    [property: JsonRequired] FieldSelectionReview Context,
    [property: JsonRequired] AssetDatabaseScope PlanningDatabase,
    [property: JsonRequired] IReadOnlyList<FIELD_DEVELOPMENT_PLAN> Plans,
    [property: JsonRequired] DateTimeOffset CheckedAtUtc);

public sealed record DevelopmentPlanDraftRecovery(
    [property: JsonRequired] FieldSelectionReview Context,
    [property: JsonRequired] AssetDatabaseScope PlanningDatabase,
    [property: JsonRequired] Guid IntentId,
    [property: JsonRequired] string RequestRevision,
    [property: JsonRequired] FIELD_DEVELOPMENT_PLAN? Plan,
    [property: JsonRequired] DateTimeOffset CheckedAtUtc);
