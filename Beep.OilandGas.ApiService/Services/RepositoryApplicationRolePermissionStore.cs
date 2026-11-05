using Beep.OilandGas.Repository.Models;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.LifeCycle.Services.AccessControl;
using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;

namespace Beep.OilandGas.ApiService.Services;

public sealed class RepositoryApplicationRolePermissionStore(RepositoryRoleAssignmentService assignments,
    RepositoryDbContext repository, RepositoryRequestAccount account) : IApplicationRolePermissionStore
{
    public async Task<List<string>> GetPermissionsAsync(string roleId)
    {
        await account.RequireAdministratorAsync();
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);
        return (await assignments.GetRolePermissionsAsync(roleId)).Select(x => x.PermissionId)
            .Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToList();
    }

    public async Task<bool> GrantAsync(string roleId, string permissionId)
    {
        var actor = await account.RequireAdministratorAsync();
        await assignments.GrantPermissionToRoleAsync(roleId, permissionId, actor);
        return true;
    }

    public async Task<bool> RevokeAsync(string roleId, string permissionId)
    {
        var actor = await account.RequireAdministratorAsync();
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);
        ArgumentException.ThrowIfNullOrWhiteSpace(permissionId);
        var permission = await repository.Set<AppPermissionExtension>().AsNoTracking()
            .SingleOrDefaultAsync(x => x.PermissionId == permissionId || x.PermissionKey == permissionId);
        var grants = (await assignments.GetRolePermissionsAsync(roleId)).Where(x =>
            x.PermissionId == permissionId || x.PermissionId == permission?.PermissionId || x.PermissionId == permission?.PermissionKey).ToList();
        if (grants.Count == 0) return false;
        if (grants.Count != 1) throw RefusalException.Conflict("Multiple permission grants match. Reconcile the role grants before revoking by permission.");
        return await assignments.RevokePermissionFromRoleAsync(grants[0].RolePermissionId, actor);
    }

}
