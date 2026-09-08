namespace TheTechIdea.Data.OilGas;

public sealed class AppUserExtension
{
    public string UserId { get; set; } = "";
    public string? FullName { get; set; }
    public string? TenantId { get; set; }
    public string? BusinessAssociateId { get; set; }
    public string? ChangedBy { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime ChangedUtc { get; set; }
}

// AspNetRoles owns role names and membership semantics; this table owns OilGas metadata.
public sealed class AppRoleExtension
{
    public string RoleId { get; set; } = "";
    public string? Description { get; set; }
    public string? RoleType { get; set; }
    public string? RoleCategory { get; set; }
    public string SystemRoleInd { get; set; } = "N";
    public string SensitiveRoleInd { get; set; } = "N";
    public string SodFlag { get; set; } = "N";
    public int? DisplaySortOrder { get; set; }
    public DateTime CreatedUtc { get; set; }
    public string? ValidFieldScope { get; set; }
}

public sealed class AppPermissionExtension
{
    public string PermissionId { get; set; } = "";
    public string PermissionKey { get; set; } = "";
    public string? ResourceKey { get; set; }
    public string? ActionKey { get; set; }
    public string? ScopeKey { get; set; }
    public string? DomainKey { get; set; }
    public string? PolicyMappingKey { get; set; }
    public string? Description { get; set; }
    public string? RiskLevel { get; set; }
}

// Assignment history survives removal from AspNetUserRoles.
public sealed class AppUserRoleExtension
{
    public string UserRoleId { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = "";
    public string RoleId { get; set; } = "";
    public string? GrantedByUserId { get; set; }
    public string? AssignmentReason { get; set; }
    public DateTime EffectiveFromUtc { get; set; }
    public DateTime? EffectiveToUtc { get; set; }
    public string ApprovalStatus { get; set; } = "Approved";
    public string? ApprovalReference { get; set; }
}

public sealed class AppRolePermissionExtension
{
    public string RolePermissionId { get; set; } = Guid.NewGuid().ToString();
    public string RoleId { get; set; } = "";
    public string PermissionId { get; set; } = "";
    // Cleared on revocation so the metadata history survives removal of the claim.
    public int? RoleClaimId { get; set; }
    public DateTime EffectiveFromUtc { get; set; }
    public DateTime? EffectiveToUtc { get; set; }
    public string? SourceSystem { get; set; }
    public string? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
}
