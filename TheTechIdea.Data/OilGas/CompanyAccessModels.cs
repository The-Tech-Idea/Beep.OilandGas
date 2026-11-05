using System.ComponentModel.DataAnnotations;

namespace TheTechIdea.Data.OilGas;

/// <summary>A customer security boundary, independent of PPDM business-associate identity.</summary>
public sealed class AppTenant
{
    [MaxLength(64)] public string TenantId { get; set; } = "";
    [MaxLength(256)] public string Name { get; set; } = "";
    public bool IsActive { get; set; } = true;
    [MaxLength(36)] public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
}

/// <summary>Links a tenant to an existing PPDM business associate; does not copy its organization records.</summary>
public sealed class AppCompany
{
    [MaxLength(64)] public string TenantId { get; set; } = "";
    [MaxLength(128)] public string BusinessAssociateId { get; set; } = "";
    public bool IsActive { get; set; } = true;
    [MaxLength(36)] public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
}

public sealed class AppCompanyMembership
{
    [MaxLength(64)] public string TenantId { get; set; } = "";
    [MaxLength(128)] public string BusinessAssociateId { get; set; } = "";
    [MaxLength(128)] public string UserId { get; set; } = "";
    public DateTime EffectiveFromUtc { get; set; }
    public DateTime? EffectiveToUtc { get; set; }
    [MaxLength(128)] public string GrantedByUserId { get; set; } = "";
    public DateTime ChangedUtc { get; set; }
    [MaxLength(36)] public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
}

/// <summary>Selected identifiers are a request, never proof of access.</summary>
public sealed record CompanyScope(string TenantId, string BusinessAssociateId);

public sealed record RepositoryCompanyAccess(CompanyScope Scope, string[] Roles, string[] Permissions);

/// <summary>A current company role holder with an active persona; does not grant task or resource access.</summary>
public sealed record RepositoryCompanyRoleRecipient(string UserId, string PersonaCode);
