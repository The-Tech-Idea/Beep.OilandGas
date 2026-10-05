using System.Text.Json;
using Beep.OilandGas.LifeCycle.Services.AccessControl;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

public sealed class RepositoryUserProfileService(RepositoryDbContext repository,
    IApplicationAuthorizationReader authorization, ILookupNormalizer normalizer, IHttpContextAccessor accessor) : IUserProfileService
{
    public async Task<UserProfile?> GetUserProfileAsync(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        var active = await repository.Users.AsNoTracking().AnyAsync(x => x.Id == userId && x.IsActive);
        if (!active) return null;
        var metadata = await repository.Set<AppUserExtension>().AsNoTracking().SingleOrDefaultAsync(x => x.UserId == userId);
        var roles = await authorization.GetRolesAsync(userId);
        var primaryRole = metadata?.PrimaryRoleId is null ? null : await repository.Roles.AsNoTracking()
            .Where(x => x.Id == metadata.PrimaryRoleId).Select(x => x.Name).SingleOrDefaultAsync();
        return new UserProfile
        {
            UserId = userId, Active = true, Roles = roles,
            PrimaryRole = primaryRole is not null && roles.Contains(primaryRole, StringComparer.Ordinal) ? primaryRole : null,
            PreferredLayout = metadata?.PreferredLayout, UserPreferences = metadata?.PreferencesJson,
            LastLoginDate = metadata?.LastLoginUtc
        };
    }

    public Task<List<string>> GetUserRolesAsync(string userId, string? organizationId = null)
    {
        if (!string.IsNullOrWhiteSpace(organizationId)) throw RefusalException.Invalid("Application roles are not organization-scoped.");
        return authorization.GetRolesAsync(userId);
    }

    public async Task<string?> GetUserDefaultLayoutAsync(string userId)
    {
        var profile = await GetUserProfileAsync(userId);
        return profile is null ? null : profile.PreferredLayout ?? "DefaultLayout";
    }

    public Task<bool> UpdateUserPreferencesAsync(string userId, string preferencesJson)
    {
        if (string.IsNullOrWhiteSpace(preferencesJson)) throw RefusalException.Invalid("Preferences are required.");
        if (preferencesJson.Length > 4000) throw RefusalException.Invalid("Preferences cannot exceed 4000 characters.");
        EnsureJson(preferencesJson);
        return UpdateAsync(userId, metadata => metadata.PreferencesJson = preferencesJson);
    }

    public async Task<bool> UpdateUserPrimaryRoleAsync(string userId, string primaryRole)
    {
        if (string.IsNullOrWhiteSpace(primaryRole)) throw RefusalException.Invalid("A primary role is required.");
        var normalized = normalizer.NormalizeName(primaryRole);
        var role = await repository.Roles.SingleOrDefaultAsync(x => x.NormalizedName == normalized);
        if (role is null || !await repository.UserRoles.AnyAsync(x => x.UserId == userId && x.RoleId == role.Id))
            throw RefusalException.Invalid("The primary role must already be assigned to the user.");
        return await UpdateAsync(userId, metadata => metadata.PrimaryRoleId = role.Id);
    }

    public Task<bool> UpdateUserPreferredLayoutAsync(string userId, string preferredLayout)
    {
        if (string.IsNullOrWhiteSpace(preferredLayout)) throw RefusalException.Invalid("A layout is required.");
        if (preferredLayout.Length > 128) throw RefusalException.Invalid("Layout cannot exceed 128 characters.");
        return UpdateAsync(userId, metadata => metadata.PreferredLayout = preferredLayout);
    }

    public async Task RecordUserLoginAsync(string userId)
    {
        if (!await UpdateAsync(userId, metadata => metadata.LastLoginUtc = DateTime.UtcNow))
            throw RefusalException.Forbidden("Only an active user of this application has a sign-in recorded.");
    }

    // System.Text.Json has no question for "is this JSON": parsing is the check. What the person sent is refused in this
    // service's words; the reader's own text stays with the inner exception.
    private static void EnsureJson(string preferencesJson)
    {
        try
        {
            using var parsed = JsonDocument.Parse(preferencesJson);
        }
        catch (JsonException notJson)
        {
            throw new RefusalException(RefusalKind.Invalid, "Preferences must be valid JSON.", notJson);
        }
    }

    private async Task<bool> UpdateAsync(string userId, Action<AppUserExtension> update)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        var actor = (accessor.HttpContext?.User).ActingUserId();
        if (!await repository.Users.AnyAsync(x => x.Id == userId && x.IsActive)) return false;
        var metadata = await repository.Set<AppUserExtension>().SingleOrDefaultAsync(x => x.UserId == userId);
        if (metadata is null)
        {
            metadata = new AppUserExtension { UserId = userId, CreatedUtc = DateTime.UtcNow };
            repository.Add(metadata);
        }
        update(metadata);
        metadata.ChangedBy = actor;
        metadata.ChangedUtc = DateTime.UtcNow;
        await repository.SaveChangesAsync();
        return true;
    }
}
