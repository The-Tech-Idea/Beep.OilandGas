using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.AspNetCore.Identity;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Repository;

/// <summary>An OilGas account's state, roles and permissions, by the account's own id.</summary>
public interface IRepositoryAccessService
{
    /// <summary>Fresh app-account existence/active state only; grants no role or permission authority.</summary>
    Task<bool> IsAccountActiveAsync(string userId, CancellationToken cancellationToken = default);
    /// <summary>The account's access, or null when no account has <paramref name="userId"/>.</summary>
    Task<RepositoryUserAccess?> GetAccessAsync(string userId, CancellationToken cancellationToken = default);
    Task<CompanyScope[]> GetCompaniesAsync(string userId, CancellationToken cancellationToken = default);
    Task<RepositoryCompanyAccess?> GetCompanyAccessAsync(string userId, CompanyScope scope,
        CancellationToken cancellationToken = default);
    Task<RepositoryCompanyRoleRecipient[]> GetCompanyRoleRecipientsAsync(CompanyScope scope, string roleName,
        CancellationToken cancellationToken = default);
}

/// <remarks>
/// Keyed by the account's id, which the identity server's client library resolves for each caller (the application's own
/// key). It was keyed by the token's issuer and subject, a second copy of that resolution without its reseed repair.
/// </remarks>
public sealed class RepositoryAccessService(RepositoryDbContext context, TimeProvider? clock = null) : IRepositoryAccessService
{
    public Task<bool> IsAccountActiveAsync(string userId,CancellationToken cancellationToken=default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        return context.Users.AsNoTracking().AnyAsync(user=>user.Id==userId && user.IsActive,cancellationToken);
    }
    private readonly TimeProvider time = clock ?? TimeProvider.System;
    // Reuse translation plans per actual EF model; every invocation still queries current app RBAC rows.
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Microsoft.EntityFrameworkCore.Metadata.IModel,CompanyAccessQueries> CompanyQueries=new();
    private sealed class CompanyAccessRow
    {
        public int Kind {get;init;}
        public string? Role {get;init;}
        public string? Permission {get;init;}
    }
    private sealed class CompanyAccessQueries(string? roleCollation,string? permissionCollation)
    {
        // One current app-RBAC statement; separate value columns retain each original database DISTINCT/collation.
        internal readonly Func<RepositoryDbContext,string,string,string,DateTime,IAsyncEnumerable<CompanyAccessRow>> Snapshot=EF.CompileAsyncQuery(
            (RepositoryDbContext db,string userId,string tenantId,string companyId,DateTime now)=>
                (from member in db.Set<AppCompanyMembership>().AsNoTracking()
                 join company in db.Set<AppCompany>().AsNoTracking() on new{member.TenantId,member.BusinessAssociateId} equals new{company.TenantId,company.BusinessAssociateId}
                 join tenant in db.Set<AppTenant>().AsNoTracking() on member.TenantId equals tenant.TenantId
                 join user in db.Users.AsNoTracking() on member.UserId equals user.Id
                 where member.UserId==userId && member.TenantId==tenantId && member.BusinessAssociateId==companyId && user.IsActive && company.IsActive && tenant.IsActive &&
                     member.EffectiveFromUtc<=now && (member.EffectiveToUtc==null || member.EffectiveToUtc>now)
                 select new CompanyAccessRow{Kind=0,Role=null,Permission=null}).Take(1)
                .Concat(
                (from member in db.UserRoles.AsNoTracking()
                 join assignment in db.Set<AppUserRoleExtension>().AsNoTracking() on new{member.UserId,member.RoleId} equals new{assignment.UserId,assignment.RoleId}
                 join role in db.Roles.AsNoTracking() on member.RoleId equals role.Id
                 where member.UserId==userId && assignment.TenantId==tenantId && assignment.BusinessAssociateId==companyId && assignment.ApprovalStatus=="Approved" &&
                     assignment.EffectiveFromUtc<=now && (assignment.EffectiveToUtc==null || assignment.EffectiveToUtc>now) && role.Name!=null
                 select role.Name!).Distinct().Select(name=>new CompanyAccessRow{Kind=1,Role=name,Permission=null}))
                .Concat(
                (from member in db.UserRoles.AsNoTracking()
                 join assignment in db.Set<AppUserRoleExtension>().AsNoTracking() on new{member.UserId,member.RoleId} equals new{assignment.UserId,assignment.RoleId}
                 join claim in db.RoleClaims.AsNoTracking() on member.RoleId equals claim.RoleId
                 where member.UserId==userId && assignment.TenantId==tenantId && assignment.BusinessAssociateId==companyId && assignment.ApprovalStatus=="Approved" &&
                     assignment.EffectiveFromUtc<=now && (assignment.EffectiveToUtc==null || assignment.EffectiveToUtc>now) && claim.ClaimType=="permission" && claim.ClaimValue!=null
                 select claim.ClaimValue!).Distinct().Select(value=>new CompanyAccessRow{Kind=2,Role=null,Permission=value}))
                .OrderBy(row=>row.Kind)
                .ThenBy(row=>roleCollation==null?row.Role:EF.Functions.Collate(row.Role,roleCollation))
                .ThenBy(row=>permissionCollation==null?row.Permission:EF.Functions.Collate(row.Permission,permissionCollation)).AsQueryable());
    }

    public async Task<CompanyScope[]> GetCompaniesAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        var now = time.GetUtcNow().UtcDateTime;
        return await ActiveCompanies(userId, now)
            .OrderBy(x => x.TenantId).ThenBy(x => x.BusinessAssociateId)
            .Select(x => new CompanyScope(x.TenantId, x.BusinessAssociateId)).ToArrayAsync(cancellationToken);
    }

    public async Task<RepositoryCompanyAccess?> GetCompanyAccessAsync(string userId, CompanyScope scope,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentException.ThrowIfNullOrWhiteSpace(scope.TenantId);
        ArgumentException.ThrowIfNullOrWhiteSpace(scope.BusinessAssociateId);
        cancellationToken.ThrowIfCancellationRequested();
        var now = time.GetUtcNow().UtcDateTime;
        var queries=CompanyQueries.GetValue(context.Model,_=>
        {
            // UNION's marker/null columns can lose column-specific ordering. Use only this model's trusted collations.
            var model=context.GetService<Microsoft.EntityFrameworkCore.Metadata.IDesignTimeModel>().Model;
            var fallback=model.GetCollation();
            return new CompanyAccessQueries(
                model.FindEntityType(typeof(IdentityRole))!.FindProperty(nameof(IdentityRole.Name))!.GetCollation()??fallback,
                model.FindEntityType(typeof(IdentityRoleClaim<string>))!.FindProperty(nameof(IdentityRoleClaim<string>.ClaimValue))!.GetCollation()??fallback);
        });
        // AspNetUserRoles remains the role authority. Scoped metadata narrows an existing grant; it cannot create one.
        var member=false;var roles=new List<string>();var permissions=new List<string>();
        await foreach(var row in queries.Snapshot(context,userId,scope.TenantId,scope.BusinessAssociateId,now).WithCancellation(cancellationToken))
        {
            if(row.Kind==0){member=true;continue;}
            // The membership marker sorts first. Never expose roles/claims from a non-current company membership.
            if(!member)return null;
            if(row.Kind==1 && row.Role is not null)roles.Add(row.Role);
            else if(row.Kind==2 && row.Permission is not null)permissions.Add(row.Permission);
            else throw new InvalidOperationException("The current company authority projection is invalid.");
        }
        return member?new(scope,roles.ToArray(),permissions.ToArray()):null;
    }

    private IQueryable<AppCompanyMembership> ActiveCompanies(string userId, DateTime now) =>
        from membership in context.Set<AppCompanyMembership>().AsNoTracking()
        join company in context.Set<AppCompany>().AsNoTracking()
            on new { membership.TenantId, membership.BusinessAssociateId } equals new { company.TenantId, company.BusinessAssociateId }
        join tenant in context.Set<AppTenant>().AsNoTracking() on membership.TenantId equals tenant.TenantId
        join user in context.Users.AsNoTracking() on membership.UserId equals user.Id
        where membership.UserId == userId && user.IsActive && company.IsActive && tenant.IsActive &&
            membership.EffectiveFromUtc <= now && (membership.EffectiveToUtc == null || membership.EffectiveToUtc > now)
        select membership;

    public async Task<RepositoryCompanyRoleRecipient[]> GetCompanyRoleRecipientsAsync(CompanyScope scope, string roleName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentException.ThrowIfNullOrWhiteSpace(scope.TenantId);
        ArgumentException.ThrowIfNullOrWhiteSpace(scope.BusinessAssociateId);
        ArgumentException.ThrowIfNullOrWhiteSpace(roleName);
        var now = time.GetUtcNow().UtcDateTime;
        // The same company membership and approved standard-role authority used by GetCompanyAccessAsync.
        // Neither scoped metadata alone nor an IdentityServer role claim can create a recipient.
        var candidates = await (from membership in context.Set<AppCompanyMembership>().AsNoTracking()
            join company in context.Set<AppCompany>().AsNoTracking()
                on new { membership.TenantId, membership.BusinessAssociateId } equals new { company.TenantId, company.BusinessAssociateId }
            join tenant in context.Set<AppTenant>().AsNoTracking() on membership.TenantId equals tenant.TenantId
            join user in context.Users.AsNoTracking() on membership.UserId equals user.Id
            join grant in context.UserRoles.AsNoTracking() on user.Id equals grant.UserId
            join assignment in context.Set<AppUserRoleExtension>().AsNoTracking()
                on new { grant.UserId, grant.RoleId } equals new { assignment.UserId, assignment.RoleId }
            join role in context.Roles.AsNoTracking() on grant.RoleId equals role.Id
            join profile in context.Set<AppUserPersona>().AsNoTracking() on user.Id equals profile.UserId
            join persona in context.Set<AppPersona>().AsNoTracking() on profile.PersonaCode equals persona.Code
            where membership.TenantId == scope.TenantId && membership.BusinessAssociateId == scope.BusinessAssociateId &&
                profile.TenantId == scope.TenantId && profile.BusinessAssociateId == scope.BusinessAssociateId &&
                user.IsActive && company.IsActive && tenant.IsActive && persona.IsActive &&
                membership.EffectiveFromUtc <= now && (membership.EffectiveToUtc == null || membership.EffectiveToUtc > now) &&
                assignment.TenantId == scope.TenantId && assignment.BusinessAssociateId == scope.BusinessAssociateId &&
                assignment.ApprovalStatus == "Approved" && assignment.EffectiveFromUtc <= now &&
                (assignment.EffectiveToUtc == null || assignment.EffectiveToUtc > now) && role.Name == roleName
            select new { user.Id, persona.Code, Role = role.Name }).Distinct().ToArrayAsync(cancellationToken);
        // Role claim values use ordinal equality in ClaimsPrincipal.IsInRole, even with case-insensitive DB collation.
        return candidates.Where(row => string.Equals(row.Role, roleName, StringComparison.Ordinal))
            .OrderBy(row => row.Id, StringComparer.Ordinal).ThenBy(row => row.Code, StringComparer.Ordinal)
            .Select(row => new RepositoryCompanyRoleRecipient(row.Id, row.Code)).ToArray();
    }

    public async Task<RepositoryUserAccess?> GetAccessAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await context.Users.AsNoTracking().SingleOrDefaultAsync(account => account.Id == userId, cancellationToken);
        if (user is null) return null;
        if (!user.IsActive) return new(user.Id, false, [], []);

        var now = time.GetUtcNow().UtcDateTime;
        var repositoryRoleIds = RepositoryRoleScope.Memberships(context, now).Where(x => x.UserId == user.Id)
            .Select(x => x.RoleId);
        var roles = await (from membership in context.UserRoles.AsNoTracking()
                           join role in context.Roles.AsNoTracking() on membership.RoleId equals role.Id
                           where membership.UserId == user.Id && repositoryRoleIds.Contains(role.Id) && role.Name != null
                           select role.Name!).Distinct().ToArrayAsync(cancellationToken);
        var permissions = await (from membership in context.UserRoles.AsNoTracking()
                                 join claim in context.RoleClaims.AsNoTracking() on membership.RoleId equals claim.RoleId
                                 where membership.UserId == user.Id && repositoryRoleIds.Contains(claim.RoleId) &&
                                     claim.ClaimType == "permission" && claim.ClaimValue != null
                                 select claim.ClaimValue!).Distinct().ToArrayAsync(cancellationToken);
        return new(user.Id, true, roles, permissions);
    }
}
