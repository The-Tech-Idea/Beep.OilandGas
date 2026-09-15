using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Beep.OilandGas.ApiService.Services;

public sealed class RepositoryRolePermissionReader(RepositoryDbContext repository, ILookupNormalizer normalizer)
    : ISodRolePermissionReader
{
    public async Task<HashSet<string>> GetPermissionsAsync(string roleName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleName);
        var normalizedName = normalizer.NormalizeName(roleName);
        var role = await repository.Roles.AsNoTracking().SingleOrDefaultAsync(x => x.NormalizedName == normalizedName)
            ?? throw new InvalidOperationException("The requested role does not exist in the default repository.");
        var permissions = await repository.RoleClaims.AsNoTracking()
            .Where(x => x.RoleId == role.Id && x.ClaimType == "permission" && x.ClaimValue != null)
            .Select(x => x.ClaimValue!).ToListAsync();
        return permissions.Where(x => !string.IsNullOrWhiteSpace(x)).ToHashSet(StringComparer.Ordinal);
    }
}
