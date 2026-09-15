using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Repository;

public interface IRepositoryAccessService
{
    Task<RepositoryUserAccess?> GetAccessAsync(string issuer, string subject, CancellationToken cancellationToken = default);
}

public sealed class RepositoryAccessService(RepositoryDbContext context) : IRepositoryAccessService
{
    public async Task<RepositoryUserAccess?> GetAccessAsync(string issuer, string subject,
        CancellationToken cancellationToken = default)
    {
        var provider = RepositoryBootstrapService.ExternalLoginProvider(issuer);
        var match = await (from login in context.UserLogins.AsNoTracking()
                          join account in context.Users.AsNoTracking() on login.UserId equals account.Id
                          where login.LoginProvider == provider && login.ProviderKey == subject
                          select new { Account = account, login.LoginProvider, login.ProviderKey }).SingleOrDefaultAsync(cancellationToken);
        // External subjects are opaque identifiers; database collation and SQL padding must not alias them.
        if (match is null || !string.Equals(match.LoginProvider, provider, StringComparison.Ordinal)
            || !string.Equals(match.ProviderKey, subject, StringComparison.Ordinal)) return null;
        var user = match.Account;
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
