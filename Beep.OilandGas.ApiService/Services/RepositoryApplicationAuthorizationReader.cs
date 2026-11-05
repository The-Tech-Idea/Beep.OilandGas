using Beep.OilandGas.LifeCycle.Services.AccessControl;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Application roles and permissions for the validated selected company, from the canonical access gateway.</summary>
public sealed class RepositoryApplicationAuthorizationReader(CompanyModuleConnection company) : IApplicationAuthorizationReader
{
    public async Task<List<string>> GetRolesAsync(string userId) =>
        (await company.ResolveSubjectAsync(userId))?.Roles.ToList() ?? [];

    public async Task<bool> HasPermissionAsync(string userId, string permission)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);
        return (await company.ResolveSubjectAsync(userId))?.Permissions.Contains(permission, StringComparer.Ordinal) == true;
    }
}
