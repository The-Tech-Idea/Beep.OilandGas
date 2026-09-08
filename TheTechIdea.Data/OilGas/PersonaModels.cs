using System.ComponentModel.DataAnnotations;

namespace TheTechIdea.Data.OilGas;

public sealed record PersonaProfileResult(AppUserPersona? Profile);

public sealed class AppPersona
{
    [MaxLength(64)] public string Code { get; set; } = "";
    [MaxLength(128)] public string Name { get; set; } = "";
    [MaxLength(1000)] public string? Description { get; set; }
    [MaxLength(256)] public string? DefaultRoute { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
}

public sealed class AppUserPersona
{
    [MaxLength(128)] public string UserId { get; set; } = "";
    [MaxLength(64)] public string? PersonaCode { get; set; }
    [MaxLength(32)] public string? Locale { get; set; }
    [MaxLength(128)] public string? TimeZone { get; set; }
    [MaxLength(32)] public string? UnitSystem { get; set; }
    [MaxLength(128)] public string? DefaultFieldId { get; set; }
    [MaxLength(128)] public string ChangedBy { get; set; } = "";
    public DateTime ChangedUtc { get; set; }
    [MaxLength(36)] public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
}

public sealed class AppPersonaPreference
{
    [MaxLength(128)] public string UserId { get; set; } = "";
    [MaxLength(64)] public string PersonaCode { get; set; } = "";
    [MaxLength(128)] public string ViewKey { get; set; } = "";
    [MaxLength(4000)] public string? Value { get; set; }
    [MaxLength(128)] public string ChangedBy { get; set; } = "";
    public DateTime ChangedUtc { get; set; }
    [MaxLength(36)] public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
}

public sealed class AppPersonaAudit
{
    [MaxLength(36)] public string Id { get; set; } = Guid.NewGuid().ToString();
    [MaxLength(128)] public string UserId { get; set; } = "";
    [MaxLength(128)] public string ActorUserId { get; set; } = "";
    [MaxLength(32)] public string Action { get; set; } = "";
    public string? BeforeJson { get; set; }
    public string AfterJson { get; set; } = "";
    public DateTime ChangedUtc { get; set; }
}

public sealed record PersonaProfileUpdate(
    [property: MaxLength(64)] string? PersonaCode,
    [property: MaxLength(32)] string? Locale = null,
    [property: MaxLength(128)] string? TimeZone = null,
    [property: MaxLength(32)] string? UnitSystem = null,
    [property: MaxLength(128)] string? DefaultFieldId = null,
    [property: MaxLength(36)] string? ConcurrencyStamp = null);

public sealed record PersonaPreferenceUpdate(
    [property: MaxLength(4000)] string? Value,
    [property: MaxLength(36)] string? ConcurrencyStamp = null);

public sealed record PersonaCatalogUpdate(
    [property: Required, MaxLength(128)] string Name,
    [property: MaxLength(1000)] string? Description = null,
    [property: MaxLength(256)] string? DefaultRoute = null,
    bool IsActive = true, int DisplayOrder = 0);
