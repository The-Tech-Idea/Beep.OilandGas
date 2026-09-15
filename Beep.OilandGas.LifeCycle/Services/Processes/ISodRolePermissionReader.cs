namespace Beep.OilandGas.LifeCycle.Services.Processes;

public interface ISodRolePermissionReader
{
    Task<HashSet<string>> GetPermissionsAsync(string roleName);
}
