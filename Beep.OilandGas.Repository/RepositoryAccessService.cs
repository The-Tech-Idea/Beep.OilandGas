using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Repository;

/// <summary>An OilGas account's state, roles and permissions, by the account's own id.</summary>
public interface IRepositoryAccessService
{
    /// <summary>The account's access, or null when no account has <paramref name="userId"/>.</summary>
    Task<RepositoryUserAccess?> GetAccessAsync(string userId, CancellationToken cancellationToken = default);
}

/// <remarks>
/// Keyed by the account's id, which the identity server's client library resolves for each caller (the application's own
/// key). It was keyed by the token's issuer and subject, a second copy of that resolution without its reseed repair.
/// </remarks>
public sealed class RepositoryAccessService(RepositoryDbContext context) : IRepositoryAccessService
{
    public async Task<RepositoryUserAccess?> GetAccessAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await context.Users.AsNoTracking().SingleOrDefaultAsync(account => account.Id == userId, cancellationToken);
        if (user is null) return null;
        if (!user.IsActive) return new(user.Id, false, [], []);

        var roles = await (from membership in context.UserRoles.AsNoTracking()
                           join role in context.Roles.AsNoTracking() on membership.RoleId equals role.Id
                           where membership.UserId == user.Id && role.Name != null
                           select role.Name!).Distinct().ToArrayAsync(cancellationToken);
        var permissions = await (from membership in context.UserRoles.AsNoTracking()
                                 join claim in context.RoleClaims.AsNoTracking() on membership.RoleId equals claim.RoleId
                                 where membership.UserId == user.Id && claim.ClaimType == "permission" && claim.ClaimValue != null
                                 select claim.ClaimValue!).Distinct().ToArrayAsync(cancellationToken);
        return new(user.Id, true, roles, permissions);
    }
}
