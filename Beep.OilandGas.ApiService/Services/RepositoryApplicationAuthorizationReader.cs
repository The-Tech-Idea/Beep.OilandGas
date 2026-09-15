using Beep.OilandGas.LifeCycle.Services.AccessControl;
using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;

namespace Beep.OilandGas.ApiService.Services;

public sealed class RepositoryApplicationAuthorizationReader(RepositoryDbContext repository) : IApplicationAuthorizationReader
{
    public async Task<List<string>> GetRolesAsync(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        return await (from user in repository.Users.AsNoTracking()
                      join membership in repository.UserRoles on user.Id equals membership.UserId
                      join role in repository.Roles on membership.RoleId equals role.Id
                      where user.Id == userId && user.IsActive && role.Name != null
                      select role.Name!).Distinct().OrderBy(name => name).ToListAsync();
    }

    public async Task<bool> HasPermissionAsync(string userId, string permission)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);
        var permissions = await (from user in repository.Users.AsNoTracking()
                                 join membership in repository.UserRoles on user.Id equals membership.UserId
                                 join claim in repository.RoleClaims on membership.RoleId equals claim.RoleId
                                 where user.Id == userId && user.IsActive && claim.ClaimType == "permission" && claim.ClaimValue != null
                                 select claim.ClaimValue!).ToListAsync();
        return permissions.Contains(permission, StringComparer.Ordinal);
    }
}
