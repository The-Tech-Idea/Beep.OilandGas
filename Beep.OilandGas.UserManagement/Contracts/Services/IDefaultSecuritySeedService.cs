namespace Beep.OilandGas.UserManagement.Contracts.Services;

public interface IDefaultSecuritySeedService
{
    Task<DefaultSecuritySeedResult> SeedDefaultsAsync(string userId = "SYSTEM");
}

public sealed class DefaultSecuritySeedResult
{
    public bool Success { get; set; }
    public int BusinessAssociatesInserted { get; set; }
    public int BaOrganizationsInserted { get; set; }
    public int UsersInserted { get; set; }
    public int RolesInserted { get; set; }
    public int PermissionsInserted { get; set; }
    public int RolePermissionsInserted { get; set; }
    public int UserRolesInserted { get; set; }
    public List<string> Errors { get; set; } = new();
}
