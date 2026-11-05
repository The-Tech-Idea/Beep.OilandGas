using Beep.OilandGas.Repository.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Beep.OilandGas.Repository;

/// <summary>The repository-administration subset of standard ASP.NET Identity memberships.</summary>
public static class RepositoryRoleScope
{
    public static IQueryable<IdentityUserRole<string>> Memberships(RepositoryDbContext db, DateTime now)
    {
        var metadata = db.Set<AppUserRoleExtension>().AsNoTracking();
        return db.UserRoles.AsNoTracking().Where(membership =>
            !metadata.Any(x => x.UserId == membership.UserId && x.RoleId == membership.RoleId) ||
            metadata.Any(x => x.UserId == membership.UserId && x.RoleId == membership.RoleId &&
                x.TenantId == null && x.BusinessAssociateId == null && x.ApprovalStatus == "Approved" &&
                x.EffectiveFromUtc <= now && (x.EffectiveToUtc == null || x.EffectiveToUtc > now)));
    }
}
