using System.Text.Json;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;

namespace Beep.OilandGas.ApiService.Services;

public sealed class ComplianceIdentityReader(RepositoryDbContext repository) : IComplianceIdentityReader
{
    public async Task<(int Users, int Roles, int Permissions)> GetCountsAsync() =>
        (await repository.Users.CountAsync(), await repository.Roles.CountAsync(),
            await repository.RoleClaims.Where(x => x.ClaimType == "permission" && x.ClaimValue != null)
                .Select(x => x.ClaimValue).Distinct().CountAsync());

    public async Task<List<UserAccessEntry>> GetUsersAsync()
    {
        var users = await repository.Users.AsNoTracking().OrderBy(x => x.Id)
            .Select(x => new UserAccessEntry { UserId = x.Id, UserName = x.UserName }).ToListAsync();
        var roles = await (from membership in repository.UserRoles.AsNoTracking()
                           join role in repository.Roles.AsNoTracking() on membership.RoleId equals role.Id
                           select new { membership.UserId, role.Name }).ToListAsync();
        var permissions = await (from membership in repository.UserRoles.AsNoTracking()
                                 join claim in repository.RoleClaims.AsNoTracking() on membership.RoleId equals claim.RoleId
                                 where claim.ClaimType == "permission" && claim.ClaimValue != null
                                 select new { membership.UserId, claim.ClaimValue }).ToListAsync();
        var rolesByUser = roles.ToLookup(x => x.UserId);
        var permissionsByUser = permissions.ToLookup(x => x.UserId);
        foreach (var user in users)
        {
            user.Roles = rolesByUser[user.UserId].Select(x => x.Name).OfType<string>()
                .Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToList();
            user.Permissions = permissionsByUser[user.UserId].Select(x => x.ClaimValue).OfType<string>()
                .Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToList();
        }
        return users;
    }

    public async Task<string> GetRolePermissionMatrixJsonAsync()
    {
        var roles = await repository.Roles.AsNoTracking().OrderBy(x => x.Id).ToListAsync();
        var claims = await repository.RoleClaims.AsNoTracking()
            .Where(x => x.ClaimType == "permission" && x.ClaimValue != null).ToListAsync();
        var byRole = claims.ToLookup(x => x.RoleId);
        var matrix = roles.ToDictionary(role => role.Name ?? role.Id, role =>
        {
            var permissions = byRole[role.Id].Select(x => x.ClaimValue!).Distinct(StringComparer.Ordinal)
                .OrderBy(x => x, StringComparer.Ordinal).ToList();
            return new { roleId = role.Id, permissionCount = permissions.Count, permissions };
        }, StringComparer.Ordinal);
        return JsonSerializer.Serialize(matrix, new JsonSerializerOptions { WriteIndented = true });
    }
}
