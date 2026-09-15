namespace Beep.OilandGas.LifeCycle.Services.AccessControl;

public interface IApplicationRolePermissionStore
{
    Task<List<string>> GetPermissionsAsync(string roleId);
    Task<bool> GrantAsync(string roleId, string permissionId);
    Task<bool> RevokeAsync(string roleId, string permissionId);
}
