namespace Beep.OilandGas.LifeCycle.Services.AccessControl;

public interface IApplicationAuthorizationReader
{
    Task<List<string>> GetRolesAsync(string userId);
    Task<bool> HasPermissionAsync(string userId, string permission);
}
