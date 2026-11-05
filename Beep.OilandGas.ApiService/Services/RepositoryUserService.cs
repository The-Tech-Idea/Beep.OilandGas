using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Repository.Models;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Beep.OilandGas.ApiService.Services;

public sealed class RepositoryUserService(RepositoryDbContext db, UserManager<OilGasUser> users,
    RoleManager<IdentityRole> roles, RepositoryRoleAssignmentService assignments, RepositoryRequestAccount account,
    TimeProvider? clock = null)
{
    private readonly TimeProvider time = clock ?? TimeProvider.System;
    public async Task<RepositoryUserSummary?> GetByIdAsync(string id)
    {
        await account.RequireSelfOrAdministratorAsync(id);
        var user = await users.FindByIdAsync(id);
        return user is null ? null : Map(user, await db.Set<AppUserExtension>().FindAsync(id));
    }

    public async Task<RepositoryUserSummary?> GetByUsernameAsync(string username)
    {
        await account.RequireAdministratorAsync();
        var user = await users.FindByNameAsync(username);
        return user is null ? null : Map(user, await db.Set<AppUserExtension>().FindAsync(user.Id));
    }

    public async Task<IEnumerable<RepositoryUserSummary>> GetAllAsync()
    {
        await account.RequireAdministratorAsync();
        var metadata = await db.Set<AppUserExtension>().AsNoTracking().ToDictionaryAsync(x => x.UserId);
        return (await db.Users.AsNoTracking().OrderBy(x => x.UserName).ToListAsync())
            .Select(user => Map(user, metadata.GetValueOrDefault(user.Id))).ToList();
    }

    /// <summary>
    /// Whether this account is active, an administrator, and the only active one — the rule <see cref="UpdateAsync"/>
    /// applies to switching one off, asked before a person deletes their own account: the identity server deletes first,
    /// and OilGas could afterwards only refuse to switch its account off.
    /// </summary>
    public async Task<bool> IsLastActiveAdministratorAsync(string id)
    {
        await account.RequireSelfOrAdministratorAsync(id);
        var user = await users.FindByIdAsync(id);
        if (user is not { IsActive: true }) return false;
        var role = await roles.FindByNameAsync("Administrator");
        if (role is null || !await RepositoryRoleScope.Memberships(db, time.GetUtcNow().UtcDateTime)
            .AnyAsync(x => x.UserId == id && x.RoleId == role.Id)) return false;
        return await ActiveAdministratorCountAsync(role.Id) <= 1;
    }

    private Task<int> ActiveAdministratorCountAsync(string administratorRoleId) =>
        (from membership in RepositoryRoleScope.Memberships(db, time.GetUtcNow().UtcDateTime)
         join account in db.Users on membership.UserId equals account.Id
         where membership.RoleId == administratorRoleId && account.IsActive select account.Id).CountAsync();

    public async Task<RepositoryUserSummary?> UpdateAsync(string id, RepositoryUserUpdate input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var actor = await account.RequireSelfOrAdministratorAsync(id);
        if (input.IsActive.HasValue) await account.RequireAdministratorAsync();
        return await UpdateCoreAsync(id, input, actor);
    }

    private async Task<RepositoryUserSummary?> UpdateCoreAsync(string id, RepositoryUserUpdate input, string actor)
    {
        if (input.FullName?.Length > 1000) throw RefusalException.Invalid("Full name exceeds 1000 characters.");
        var user = await users.FindByIdAsync(id);
        if (user is null) return null;
        // The version the caller read: a change made since is theirs to reload, not EF's concurrency failure.
        if (string.IsNullOrWhiteSpace(input.ConcurrencyStamp) || input.ConcurrencyStamp != user.ConcurrencyStamp)
            throw RefusalException.Conflict("The user changed. Reload before saving.");
        await using var transaction = await db.Database.BeginTransactionAsync();
        var active = input.IsActive ?? user.IsActive;
        var administrator = await roles.FindByNameAsync(RepositoryBootstrapService.AdministratorRole);
        if (!active && user.IsActive && administrator is not null &&
            await RepositoryRoleScope.Memberships(db, time.GetUtcNow().UtcDateTime).AnyAsync(value => value.UserId == id && value.RoleId == administrator.Id))
        {
            Require(await roles.UpdateAsync(administrator));
            if (await ActiveAdministratorCountAsync(administrator.Id) <= 1)
                throw RefusalException.Conflict("The last active administrator cannot be disabled.");
        }
        user.IsActive = active;
        Require(await users.UpdateAsync(user));
        var metadata = await db.Set<AppUserExtension>().FindAsync(user.Id);
        if (metadata is null)
        {
            metadata = new AppUserExtension { UserId = user.Id, CreatedUtc = time.GetUtcNow().UtcDateTime };
            db.Add(metadata);
        }
        metadata.FullName = input.FullName;
        metadata.ChangedBy = actor;
        metadata.ChangedUtc = time.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return Map(user, metadata);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var actor = await account.RequireSelfOrAdministratorAsync(id);
        var user = await users.FindByIdAsync(id);
        if (user is null) return false;
        var metadata = await db.Set<AppUserExtension>().FindAsync(id);
        return await UpdateCoreAsync(id, new(metadata?.FullName, false, user.ConcurrencyStamp ?? ""), actor) is not null;
    }

    public async Task<bool> DeactivateSelfAsync()
    {
        var actor = await account.RequireAsync();
        await account.RequireSelfAsync(actor);
        return await DeleteAsync(actor);
    }

    public async Task<bool> AddToRoleAsync(string userId, string roleName)
    {
        var actor = await account.RequireAdministratorAsync();
        var role = await roles.FindByNameAsync(roleName);
        if (role is null) return false;
        await assignments.AssignRoleAsync(userId, role.Id, actor);
        return true;
    }

    public async Task<bool> RemoveFromRoleAsync(string userId, string roleName)
    {
        var actor = await account.RequireAdministratorAsync();
        var role = await roles.FindByNameAsync(roleName);
        if (role is null) return false;
        var assignment = (await assignments.GetUserRoleAssignmentsAsync(userId)).SingleOrDefault(x => x.RoleId == role.Id);
        return assignment is not null && await assignments.RevokeRoleAsync(assignment.UserRoleId, actor);
    }

    public async Task<IEnumerable<string>> GetRolesAsync(string userId)
    {
        await account.RequireSelfOrAdministratorAsync(userId);
        return await (from membership in RepositoryRoleScope.Memberships(db, time.GetUtcNow().UtcDateTime)
                      join role in db.Roles.AsNoTracking() on membership.RoleId equals role.Id
                      where membership.UserId == userId && role.Name != null
                      select role.Name!).ToArrayAsync();
    }

    private static RepositoryUserSummary Map(OilGasUser user, AppUserExtension? metadata) =>
        new(user.Id, user.UserName ?? "", user.Email, metadata?.FullName, user.IsActive, user.ConcurrencyStamp ?? "");

    private static void Require(IdentityResult result)
    {
        if (result.Errors.Any(x => x.Code == nameof(IdentityErrorDescriber.ConcurrencyFailure)))
            throw RefusalException.Conflict("The account or role changed. Reload before retrying.");
        if (!result.Succeeded) throw new InvalidOperationException(string.Join(", ", result.Errors.Select(x => x.Code)));
    }
}
