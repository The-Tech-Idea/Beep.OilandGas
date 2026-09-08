using System.Security.Claims;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

public sealed class RepositoryUserService(RepositoryDbContext db, UserManager<OilGasUser> users,
    RoleManager<IdentityRole> roles, RepositoryRoleAssignmentService assignments, IHttpContextAccessor accessor)
{
    public async Task<RepositoryUserSummary?> GetByIdAsync(string id)
    {
        var user = await users.FindByIdAsync(id);
        return user is null ? null : Map(user, await db.Set<AppUserExtension>().FindAsync(id));
    }

    public async Task<RepositoryUserSummary?> GetByUsernameAsync(string username)
    {
        var user = await users.FindByNameAsync(username);
        return user is null ? null : Map(user, await db.Set<AppUserExtension>().FindAsync(user.Id));
    }

    public async Task<IEnumerable<RepositoryUserSummary>> GetAllAsync()
    {
        var metadata = await db.Set<AppUserExtension>().AsNoTracking().ToDictionaryAsync(x => x.UserId);
        return (await db.Users.AsNoTracking().OrderBy(x => x.UserName).ToListAsync())
            .Select(user => Map(user, metadata.GetValueOrDefault(user.Id))).ToList();
    }

    public async Task<RepositoryUserSummary?> UpdateAsync(string id, RepositoryUserUpdate input)
    {
        if (input.FullName?.Length > 1000) throw new ArgumentException("Full name exceeds 1000 characters.");
        var actor = Actor;
        var user = await users.FindByIdAsync(id);
        if (user is null) return null;
        if (string.IsNullOrWhiteSpace(input.ConcurrencyStamp) || input.ConcurrencyStamp != user.ConcurrencyStamp)
            throw new DbUpdateConcurrencyException("The user changed. Reload before saving.");
        await using var transaction = await db.Database.BeginTransactionAsync();
        var active = input.IsActive ?? user.IsActive;
        if (!active && user.IsActive && await users.IsInRoleAsync(user, "Administrator"))
        {
            var role = (await roles.FindByNameAsync("Administrator"))!;
            Require(await roles.UpdateAsync(role));
            var count = await (from membership in db.UserRoles join account in db.Users on membership.UserId equals account.Id
                               where membership.RoleId == role.Id && account.IsActive select account.Id).CountAsync();
            if (count <= 1) throw new InvalidOperationException("The last active administrator cannot be disabled.");
        }
        user.IsActive = active;
        Require(await users.UpdateAsync(user));
        var metadata = await db.Set<AppUserExtension>().FindAsync(user.Id);
        if (metadata is null)
        {
            metadata = new AppUserExtension { UserId = user.Id, CreatedUtc = DateTime.UtcNow };
            db.Add(metadata);
        }
        metadata.FullName = input.FullName;
        metadata.ChangedBy = actor;
        metadata.ChangedUtc = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return Map(user, metadata);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var user = await GetByIdAsync(id);
        if (user is null) return false;
        return await UpdateAsync(id, new(user.FullName, false, user.ConcurrencyStamp)) is not null;
    }

    public async Task<bool> AddToRoleAsync(string userId, string roleName)
    {
        var role = await roles.FindByNameAsync(roleName);
        if (role is null) return false;
        await assignments.AssignRoleAsync(userId, role.Id, Actor);
        return true;
    }

    public async Task<bool> RemoveFromRoleAsync(string userId, string roleName)
    {
        var role = await roles.FindByNameAsync(roleName);
        if (role is null) return false;
        var assignment = (await assignments.GetUserRoleAssignmentsAsync(userId)).SingleOrDefault(x => x.RoleId == role.Id);
        return assignment is not null && await assignments.RevokeRoleAsync(assignment.UserRoleId, Actor);
    }

    public async Task<IEnumerable<string>> GetRolesAsync(string userId)
    {
        var user = await users.FindByIdAsync(userId);
        return user is null || !user.IsActive ? [] : await users.GetRolesAsync(user);
    }

    private string Actor => accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("A local actor is required for user management.");

    private static RepositoryUserSummary Map(OilGasUser user, AppUserExtension? metadata) =>
        new(user.Id, user.UserName ?? "", user.Email, metadata?.FullName, user.IsActive, user.ConcurrencyStamp ?? "");

    private static void Require(IdentityResult result)
    {
        if (result.Errors.Any(x => x.Code == "ConcurrencyFailure"))
            throw new DbUpdateConcurrencyException("The account or role changed. Reload before retrying.");
        if (!result.Succeeded) throw new InvalidOperationException(string.Join(", ", result.Errors.Select(x => x.Code)));
    }
}
