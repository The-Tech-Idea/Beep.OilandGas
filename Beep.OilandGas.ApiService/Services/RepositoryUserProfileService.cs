using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Repository.Models;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Services.AccessControl;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Beep.OilandGas.ApiService.Services;

public sealed partial class RepositoryUserProfileService(RepositoryDbContext repository,
    IApplicationAuthorizationReader authorization, ILookupNormalizer normalizer, RepositoryRequestAccount account,
    CompanyModuleConnection company, IPPDM39DataService fields, TimeProvider? clock = null) : IUserProfileService, ICompanyFieldSelectionService, ICompanyFieldSelectionAuthority
{
    private readonly TimeProvider time = clock ?? TimeProvider.System;
    public async Task<UserProfile?> GetUserProfileAsync(string userId)
    {
        await RequireSubjectAsync(userId);
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

    public async Task<List<string>> GetUserRolesAsync(string userId, string? organizationId = null)
    {
        await RequireSubjectAsync(userId);
        if (organizationId is not null) throw RefusalException.Invalid("Select the company in validated request context; organization cannot override profile role scope.");
        return await authorization.GetRolesAsync(userId);
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
        return UpdateAsync(userId, metadata => metadata.PreferencesJson = PreserveFieldSelections(metadata.PreferencesJson, preferencesJson));
    }

    public async Task<bool> UpdateUserPrimaryRoleAsync(string userId, string primaryRole)
    {
        await RequireSubjectAsync(userId);
        if (string.IsNullOrWhiteSpace(primaryRole)) throw RefusalException.Invalid("A primary role is required.");
        var normalized = normalizer.NormalizeName(primaryRole);
        var role = await repository.Roles.SingleOrDefaultAsync(x => x.NormalizedName == normalized);
        if (role is null) throw RefusalException.Invalid("The primary role must be currently assigned in the selected company.");
        return await UpdateAsync(userId, metadata => metadata.PrimaryRoleId = role.Id, async () =>
        {
            var current = await authorization.GetRolesAsync(userId);
            if (role.Name is null || !current.Contains(role.Name, StringComparer.Ordinal))
                throw RefusalException.Invalid("The primary role must be currently assigned in the selected company.");
        });
    }

    public Task<bool> UpdateUserPreferredLayoutAsync(string userId, string preferredLayout)
    {
        if (string.IsNullOrWhiteSpace(preferredLayout)) throw RefusalException.Invalid("A layout is required.");
        if (preferredLayout.Length > 128) throw RefusalException.Invalid("Layout cannot exceed 128 characters.");
        return UpdateAsync(userId, metadata => metadata.PreferredLayout = preferredLayout);
    }

    public async Task RecordUserLoginAsync(string userId)
    {
        await account.RequireSelfAsync(userId);
        if (!await UpdateAsync(userId, metadata => metadata.LastLoginUtc = time.GetUtcNow().UtcDateTime))
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

    private async Task<string> RequireSubjectAsync(string subject)
    {
        var actor = await account.RequireSelfOrAdministratorAsync(subject);
        company.RequireActor(actor);
        await company.RefreshCompanyAsync();
        return actor;
    }

    private async Task<bool> UpdateAsync(string userId, Action<AppUserExtension> update, Func<Task>? validate = null)
    {
        var actor = await RequireSubjectAsync(userId);
        if (validate is not null) await validate();
        var user = await repository.Users.SingleOrDefaultAsync(x => x.Id == userId && x.IsActive);
        if (user is null) return false;
        var metadata = await repository.Set<AppUserExtension>().SingleOrDefaultAsync(x => x.UserId == userId);
        if (metadata is null)
        {
            metadata = new AppUserExtension { UserId = userId, CreatedUtc = time.GetUtcNow().UtcDateTime };
            repository.Add(metadata);
        }
        update(metadata);
        user.ConcurrencyStamp = Guid.NewGuid().ToString("N"); // Existing Identity CAS also protects shared preference updates.
        metadata.ChangedBy = actor;
        metadata.ChangedUtc = time.GetUtcNow().UtcDateTime;
        try { await repository.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException error) { throw new RefusalException(RefusalKind.Conflict, "The profile changed during this update; refresh before submitting again.", error); }
        return true;
    }
}
