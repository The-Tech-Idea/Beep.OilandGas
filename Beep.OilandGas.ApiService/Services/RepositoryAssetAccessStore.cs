using Beep.OilandGas.Models.Core.Refusals;
using System.Security.Cryptography;
using System.Text.Json;
using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

public sealed class RepositoryAssetAccessStore(RepositoryDbContext repository, IHttpContextAccessor httpContext) : IUserAssetAccessStore
{
    public async Task<List<AppUserAssetAccess>> ReadAsync(string userId, string databaseScope, string? organizationId = null)
    {
        ValidateUserAndScope(userId, databaseScope);
        organizationId = NormalizeOrganization(organizationId);
        var grants = await (from grant in repository.Set<AppUserAssetAccess>().AsNoTracking()
                      join user in repository.Users on grant.UserId equals user.Id
                      where user.Id == userId && user.IsActive && grant.IsActive
                          && grant.DatabaseScope == databaseScope && grant.OrganizationId == organizationId
                      select grant).ToListAsync();
        return grants.Where(x => x.UserId == userId && x.OrganizationId == organizationId).ToList();
    }

    public async Task<bool> GrantAsync(string userId, string databaseScope, string assetType, string assetId,
        string accessLevel, bool inherit, string? organizationId = null)
    {
        var actor = await RequireAdministratorAsync();
        ValidateUserAndScope(userId, databaseScope);
        assetType = NormalizeAssetType(assetType);
        ValidateIdentifier(assetId, nameof(assetId));
        organizationId = NormalizeOrganization(organizationId);
        accessLevel = accessLevel?.ToUpperInvariant() ?? "";
        if (accessLevel is not ("READ" or "WRITE" or "DELETE"))
            throw RefusalException.Invalid("Access level must be READ, WRITE, or DELETE.");
        if (!await repository.Users.AnyAsync(x => x.Id == userId && x.IsActive)) return false;

        // The deterministic primary key arbitrates concurrent creation on every provider.
        var id = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(
            new[] { userId, databaseScope, assetType, assetId, organizationId })));
        var grant = await repository.Set<AppUserAssetAccess>().SingleOrDefaultAsync(x => x.Id == id);
        var now = DateTime.UtcNow;
        if (grant is null)
        {
            grant = new AppUserAssetAccess { Id = id, UserId = userId, DatabaseScope = databaseScope,
                AssetType = assetType, AssetId = assetId, OrganizationId = organizationId,
                CreatedBy = actor, CreatedUtc = now };
            repository.Add(grant);
        }
        grant.AccessLevel = accessLevel;
        grant.Inherit = inherit;
        grant.IsActive = true;
        grant.ChangedBy = actor;
        grant.ChangedUtc = now;
        grant.ConcurrencyStamp = Guid.NewGuid().ToString();
        await repository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RevokeAsync(string userId, string databaseScope, string assetType, string assetId)
    {
        var actor = await RequireAdministratorAsync();
        ValidateUserAndScope(userId, databaseScope);
        assetType = NormalizeAssetType(assetType);
        ValidateIdentifier(assetId, nameof(assetId));
        var grants = await repository.Set<AppUserAssetAccess>().Where(x => x.UserId == userId
            && x.DatabaseScope == databaseScope && x.AssetType == assetType && x.AssetId == assetId && x.IsActive).ToListAsync();
        // SQL Server collation must not widen an exact asset identifier match.
        grants = grants.Where(x => x.UserId == userId && x.AssetType == assetType && x.AssetId == assetId).ToList();
        foreach (var grant in grants)
        {
            grant.IsActive = false;
            grant.ChangedBy = actor;
            grant.ChangedUtc = DateTime.UtcNow;
            grant.ConcurrencyStamp = Guid.NewGuid().ToString();
        }
        await repository.SaveChangesAsync();
        return grants.Count != 0;
    }

    private async Task<string> RequireAdministratorAsync()
    {
        var principal = httpContext.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true || !principal.IsInRole("Administrator"))
            throw RefusalException.Forbidden("A local Administrator is required.");
        var actor = principal.ActingUserId();
        var allowed = await (from user in repository.Users
                             join membership in repository.UserRoles on user.Id equals membership.UserId
                             join role in repository.Roles on membership.RoleId equals role.Id
                             where user.Id == actor && user.IsActive && role.NormalizedName == "ADMINISTRATOR"
                             select user.Id).AnyAsync();
        if (!allowed) throw RefusalException.Forbidden("The local Administrator assignment is no longer active.");
        return actor;
    }

    private static void ValidateUserAndScope(string userId, string databaseScope)
    {
        ValidateIdentifier(userId, nameof(userId));
        if (databaseScope is null || databaseScope.Length != 64 || databaseScope.Any(c => !char.IsAsciiHexDigit(c))
            || databaseScope != databaseScope.ToUpperInvariant())
            throw new ArgumentException("A canonical database scope fingerprint is required.", nameof(databaseScope));
    }

    private static string NormalizeAssetType(string value)
    {
        value = value?.ToUpperInvariant() ?? "";
        return value is "FIELD" or "WELL" or "POOL" or "FACILITY" ? value
            : throw RefusalException.Invalid("Unsupported asset type.");
    }

    private static string? NormalizeOrganization(string? value)
    {
        if (string.IsNullOrEmpty(value)) return null;
        ValidateIdentifier(value, nameof(value));
        return value;
    }

    private static void ValidateIdentifier(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 128 || value != value.Trim())
            throw RefusalException.Invalid($"{name}: an identifier of up to 128 characters without surrounding whitespace is required.");
    }
}
