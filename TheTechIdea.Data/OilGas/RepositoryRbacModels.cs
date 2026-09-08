namespace TheTechIdea.Data.OilGas;

public sealed record AssignRoleRequest(string RoleId, string? Reason);
public sealed record GrantPermissionRequest(string PermissionId);

public sealed class RepositoryRoleDetails
{
    public string RoleId { get; init; } = "";
    public string RoleName { get; init; } = "";
    public string? Description { get; init; }
    public string? RoleType { get; init; }
    public string? RoleCategory { get; init; }
    public string SystemRoleInd { get; init; } = "N";
    public string SensitiveRoleInd { get; init; } = "N";
    public string SodFlag { get; init; } = "N";
    public int? DisplaySortOrder { get; init; }
    public DateTime CreatedUtc { get; init; }
    public string? ValidFieldScope { get; init; }
}

public sealed class RepositoryPermission
{
    public string PermissionId { get; init; } = "";
    public string PermissionKey { get; init; } = "";
    public string? ResourceKey { get; init; }
    public string? ActionKey { get; init; }
    public string? ScopeKey { get; init; }
    public string? DomainKey { get; init; }
    public string? PolicyMappingKey { get; init; }
    public string? Description { get; init; }
    public string? RiskLevel { get; init; }
}

public sealed class RepositoryUserRole
{
    public string UserRoleId { get; init; } = "";
    public string UserId { get; init; } = "";
    public string RoleId { get; init; } = "";
    public string? GrantedByUserId { get; init; }
    public string? AssignmentReason { get; init; }
    public DateTime EffectiveFromUtc { get; init; }
    public DateTime? EffectiveToUtc { get; init; }
    public string? ApprovalStatus { get; init; }
    public string? ApprovalReference { get; init; }
}

public sealed class RepositoryRolePermission
{
    public string RolePermissionId { get; init; } = "";
    public string RoleId { get; init; } = "";
    public string PermissionId { get; init; } = "";
    public string? SourceSystem { get; init; }
    public DateTime EffectiveFromUtc { get; init; }
    public DateTime? EffectiveToUtc { get; init; }
    public string? ApprovedByUserId { get; init; }
    public DateTime? ApprovedAtUtc { get; init; }
}
