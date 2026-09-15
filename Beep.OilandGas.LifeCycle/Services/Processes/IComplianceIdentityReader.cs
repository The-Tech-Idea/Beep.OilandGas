namespace Beep.OilandGas.LifeCycle.Services.Processes;

public interface IComplianceIdentityReader
{
    Task<(int Users, int Roles, int Permissions)> GetCountsAsync();
    Task<List<UserAccessEntry>> GetUsersAsync();
    Task<string> GetRolePermissionMatrixJsonAsync();
}
