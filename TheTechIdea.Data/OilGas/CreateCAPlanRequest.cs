using System.Text.Json.Serialization;

namespace TheTechIdea.Data.OilGas;

// Stable caller intent with an explicit recorded incident participant; app-user identity is not a BA identity.
public sealed record CreateCAPlanRequest(
    [property: JsonRequired] Guid PlanId,
    [property: JsonRequired] string PlanName,
    [property: JsonRequired] string PlanNumber,
    [property: JsonRequired] string ResponseType,
    [property: JsonRequired] decimal PartyObservation,
    [property: JsonRequired] decimal RoleObservation,
    [property: JsonRequired] string ResponsibleBaId,
    [property: JsonRequired] string ExpectedRevision);
