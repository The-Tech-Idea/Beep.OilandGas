using System.ComponentModel.DataAnnotations;

namespace TheTechIdea.Data.OilGas;

public sealed record RepositoryRoleRequest(
    [Required, MaxLength(256), RegularExpression(@"[^,]+") ] string RoleName,
    [MaxLength(1000)] string? Description);
public sealed record RepositoryRoleSummary(string RoleId, string RoleName, string? Description);
