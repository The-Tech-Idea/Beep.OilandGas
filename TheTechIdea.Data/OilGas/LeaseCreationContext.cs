using System.Text.Json.Serialization;

namespace TheTechIdea.Data.OilGas;

// A reviewed intent assertion. Authority is always resolved again by the API.
public sealed record LeaseCreationContext([property: JsonRequired] CompanyScope Company,
    [property: JsonRequired] string DatabaseScopeFingerprint, [property: JsonRequired] string ActorUserId,
    [property: JsonRequired] string FieldId, [property: JsonRequired] string LandRightSubtype,
    [property: JsonRequired] string CompanyComponentType, [property: JsonRequired] string StatusType, [property: JsonRequired] string DateZoneId);

// Historical registration evidence only; this is not a lease approval or current legal status.
public sealed record LeaseCreationReceipt([property: JsonRequired] Guid CommandId, [property: JsonRequired] string State,
    [property: JsonRequired] string? LeaseId, [property: JsonRequired] LeaseCreationContext Context);
