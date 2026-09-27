using Beep.OilandGas.LifeCycle.Services.AccessControl;
using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

public sealed class RepositoryApplicationRolePermissionStore(RepositoryRoleAssignmentService assignments,
    RepositoryDbContext repository, IHttpContextAccessor accessor) : IApplicationRolePermissionStore
{
    public async Task<List<string>> GetPermissionsAsync(string roleId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);
        return (await assignments.GetRolePermissionsAsync(roleId)).Select(x => x.PermissionId)
            .Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToList();
    }

    public async Task<bool> GrantAsync(string roleId, string permissionId)
    {
        var actor = GetActor();
        await assignments.GrantPermissionToRoleAsync(roleId, permissionId, actor);
        return true;
    }

    public async Task<bool> RevokeAsync(string roleId, string permissionId)
    {
        var actor = GetActor();
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);
        ArgumentException.ThrowIfNullOrWhiteSpace(permissionId);
        var permission = await repository.Set<AppPermissionExtension>().AsNoTracking()
            .SingleOrDefaultAsync(x => x.PermissionId == permissionId || x.PermissionKey == permissionId);
        var grants = (await assignments.GetRolePermissionsAsync(roleId)).Where(x =>
            x.PermissionId == permissionId || x.PermissionId == permission?.PermissionId || x.PermissionId == permission?.PermissionKey).ToList();
        if (grants.Count == 0) return false;
        if (grants.Count != 1) throw new InvalidOperationException("Multiple permission grants match. Reconcile the role grants before revoking by permission.");
        return await assignments.RevokePermissionFromRoleAsync(grants[0].RolePermissionId, actor);
    }

    private string GetActor()
    {
        var user = accessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true || !user.IsInRole("Administrator"))
            throw new UnauthorizedAccessException("A local administrator is required to change role permissions.");
        return user.ActingUserId();
    }
}
