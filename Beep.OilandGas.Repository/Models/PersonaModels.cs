using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Repository.Models;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Repository.Models;

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
    [MaxLength(64)] public string TenantId { get; set; } = "";
    [MaxLength(128)] public string BusinessAssociateId { get; set; } = "";
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
    [MaxLength(64)] public string TenantId { get; set; } = "";
    [MaxLength(128)] public string BusinessAssociateId { get; set; } = "";
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
    [MaxLength(64)] string? PersonaCode,
    [MaxLength(32)] string? Locale = null,
    [MaxLength(128)] string? TimeZone = null,
    [MaxLength(32)] string? UnitSystem = null,
    [MaxLength(128)] string? DefaultFieldId = null,
    [MaxLength(36)] string? ConcurrencyStamp = null);

public sealed record PersonaPreferenceUpdate(
    [MaxLength(4000)] string? Value,
    [MaxLength(36)] string? ConcurrencyStamp = null);

public sealed record PersonaCatalogUpdate(
    [Required, MaxLength(128)] string Name,
    [MaxLength(1000)] string? Description = null,
    [MaxLength(256)] string? DefaultRoute = null,
    bool IsActive = true, int DisplayOrder = 0);

public sealed record CompanyPersonaWorkspaceReview(CompanyScope Company, string ActorUserId,
    IReadOnlyList<string> Roles, IReadOnlyList<AppPersona> Personas, string Revision);

public sealed record RoleWorkspaceMappingRequest([property: System.Text.Json.Serialization.JsonRequired] bool Enabled,
 [property: System.Text.Json.Serialization.JsonRequired] string ExpectedRevision);

public sealed record RoleWorkspaceChoice(AppPersona Persona, bool Enabled);
public sealed record RoleWorkspaceReview(string RoleId, string RoleName, IReadOnlyList<RoleWorkspaceChoice> Choices, string Revision);
