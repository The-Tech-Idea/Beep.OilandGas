using System.Reflection;
using System.Security.Claims;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace Beep.OilandGas.ApiService.Services;

// Identity owns grants; APP_* records describe their application metadata and history.
public sealed class RepositoryRoleAssignmentService(
    RepositoryDbContext db, UserManager<OilGasUser> users, RoleManager<IdentityRole> roles,
    ILogger<RepositoryRoleAssignmentService> logger)
{
    public async Task<RepositoryUserRole> AssignRoleAsync(string userId, string roleId, string grantedByUserId, string? reason = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(grantedByUserId);
        if (reason?.Length > 1000) throw new ArgumentException("Assignment reason exceeds 1000 characters.");
        var user = await users.FindByIdAsync(userId) ?? throw new ArgumentException("User not found.");
        if (!user.IsActive) throw new InvalidOperationException("Cannot assign a role to a disabled user.");
        var role = await roles.FindByIdAsync(roleId) ?? throw new ArgumentException("Role not found.");
        await using var transaction = await db.Database.BeginTransactionAsync();
        Require(await roles.UpdateAsync(role));
        if (!await users.IsInRoleAsync(user, role.Name!)) Require(await users.AddToRoleAsync(user, role.Name!));
        var extension = await db.Set<AppUserRoleExtension>().SingleOrDefaultAsync(x =>
            x.UserId == userId && x.RoleId == roleId && x.EffectiveToUtc == null);
        if (extension is null)
        {
            extension = new AppUserRoleExtension { UserId = userId, RoleId = roleId,
                GrantedByUserId = grantedByUserId, AssignmentReason = reason, EffectiveFromUtc = DateTime.UtcNow };
            db.Add(extension);
            await db.SaveChangesAsync();
        }
        await transaction.CommitAsync();
        logger.LogInformation("Role assigned: Actor={Actor} User={User} Role={Role} Reason={Reason}",
            grantedByUserId, userId, roleId, reason);
        return Assignment(extension);
    }

    public async Task<bool> RevokeRoleAsync(string userRoleId, string revokedByUserId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(revokedByUserId);
        AppUserRoleExtension? extension;
        string userId, roleId;
        if (userRoleId.StartsWith("identity:", StringComparison.Ordinal))
        {
            string[]? keys;
            try { keys = JsonSerializer.Deserialize<string[]>(WebEncoders.Base64UrlDecode(userRoleId[9..])); }
            catch (Exception ex) when (ex is FormatException or JsonException) { return false; }
            if (keys is not { Length: 2 } || keys.Any(string.IsNullOrWhiteSpace)) return false;
            userId = keys[0];
            roleId = keys[1];
            extension = await db.Set<AppUserRoleExtension>().SingleOrDefaultAsync(x =>
                x.UserId == userId && x.RoleId == roleId && x.EffectiveToUtc == null);
        }
        else
        {
            extension = await db.Set<AppUserRoleExtension>().SingleOrDefaultAsync(x => x.UserRoleId == userRoleId);
            if (extension is null || extension.EffectiveToUtc is not null) return false;
            userId = extension.UserId;
            roleId = extension.RoleId;
        }
        var user = await users.FindByIdAsync(userId);
        var role = await roles.FindByIdAsync(roleId);
        if (user is null || role is null) return false;
        await using var transaction = await db.Database.BeginTransactionAsync();
        // Updating the shared role row prevents two concurrent last-admin removals
        // from independently passing the count check, including on Oracle.
        Require(await roles.UpdateAsync(role));
        if (!await users.IsInRoleAsync(user, role.Name!)) return false;
        if (roles.NormalizeKey(role.Name) == roles.NormalizeKey(RepositoryBootstrapService.AdministratorRole) && user.IsActive)
        {
            var activeAdmins = await (from membership in db.UserRoles
                                      join account in db.Users on membership.UserId equals account.Id
                                      where membership.RoleId == role.Id && account.IsActive
                                      select account.Id).CountAsync();
            if (activeAdmins <= 1) throw new InvalidOperationException("The last active administrator cannot be removed.");
        }
        Require(await users.RemoveFromRoleAsync(user, role.Name!));
        if (extension is null)
        {
            extension = new AppUserRoleExtension { UserId = userId, RoleId = roleId,
                EffectiveFromUtc = DateTime.UtcNow,
                AssignmentReason = "Identity membership observed at revocation; original grant details are unknown." };
            db.Add(extension);
        }
        extension.EffectiveToUtc = DateTime.UtcNow;
        extension.ApprovalStatus = "Revoked";
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        logger.LogInformation("Role revoked: Actor={Actor} User={User} Role={Role}", revokedByUserId, user.Id, role.Id);
        return true;
    }

    public async Task<List<RepositoryUserRole>> GetUserRoleAssignmentsAsync(string userId)
    {
        var memberships = await db.UserRoles.AsNoTracking().Where(x => x.UserId == userId).ToListAsync();
        var metadata = await db.Set<AppUserRoleExtension>().AsNoTracking()
            .Where(x => x.UserId == userId && x.EffectiveToUtc == null).ToDictionaryAsync(x => x.RoleId);
        return memberships.Select(membership => metadata.TryGetValue(membership.RoleId, out var extension)
            ? Assignment(extension)
            : new RepositoryUserRole
            {
                UserRoleId = "identity:" + WebEncoders.Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new[] { userId, membership.RoleId })),
                UserId = userId, RoleId = membership.RoleId, EffectiveFromUtc = DateTime.MinValue,
                AssignmentReason = "Identity membership; original grant details are unknown."
            }).ToList();
    }

    public async Task<RepositoryRolePermission> GrantPermissionToRoleAsync(string roleId, string permissionId, string approvedByUserId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(approvedByUserId);
        var metadata = await db.Set<AppPermissionExtension>().FindAsync(permissionId);
        metadata ??= await db.Set<AppPermissionExtension>().SingleOrDefaultAsync(x => x.PermissionKey == permissionId);
        if (metadata is null && !PermissionCodes().Contains(permissionId) &&
            !await db.RoleClaims.AnyAsync(x => x.ClaimType == "permission" && x.ClaimValue == permissionId))
            throw new ArgumentException("Unknown permission code.");
        var code = metadata?.PermissionKey ?? permissionId;
        permissionId = metadata?.PermissionId ?? permissionId;
        var role = await roles.FindByIdAsync(roleId) ?? throw new ArgumentException("Role not found.");
        await using var transaction = await db.Database.BeginTransactionAsync();
        Require(await roles.UpdateAsync(role));
        var existing = await db.RoleClaims.Where(x => x.RoleId == roleId &&
            x.ClaimType == "permission" && x.ClaimValue == code).OrderBy(x => x.Id).FirstOrDefaultAsync();
        if (existing is null)
        {
            Require(await roles.AddClaimAsync(role, new Claim("permission", code)));
            existing = await db.RoleClaims.SingleAsync(x => x.RoleId == roleId &&
                x.ClaimType == "permission" && x.ClaimValue == code);
        }
        if (metadata is null) db.Add(new AppPermissionExtension { PermissionId = permissionId, PermissionKey = code });
        var extension = await db.Set<AppRolePermissionExtension>().SingleOrDefaultAsync(x =>
            x.RoleId == roleId && x.PermissionId == permissionId && x.EffectiveToUtc == null);
        if (extension is not null && !await db.RoleClaims.AnyAsync(x => x.Id == extension.RoleClaimId &&
            x.RoleId == roleId && x.ClaimType == "permission" && x.ClaimValue == code))
        {
            extension.EffectiveToUtc = DateTime.UtcNow;
            extension.RoleClaimId = null;
            extension = null;
        }
        if (extension is null)
        {
            extension = new AppRolePermissionExtension { RoleId = roleId, PermissionId = permissionId,
                RoleClaimId = existing.Id, EffectiveFromUtc = DateTime.UtcNow, SourceSystem = "Manual",
                ApprovedByUserId = approvedByUserId, ApprovedAtUtc = DateTime.UtcNow };
            db.Add(extension);
        }
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        logger.LogInformation("Permission granted: Actor={Actor} Role={Role} Permission={Permission}",
            approvedByUserId, roleId, permissionId);
        return Permission(extension);
    }

    public async Task<bool> RevokePermissionFromRoleAsync(string rolePermissionId, string revokedByUserId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(revokedByUserId);
        if (rolePermissionId.StartsWith("claim:", StringComparison.Ordinal))
        {
            if (!int.TryParse(rolePermissionId[6..], out var claimId)) return false;
            var claim = await db.RoleClaims.SingleOrDefaultAsync(x => x.Id == claimId && x.ClaimType == "permission");
            if (claim is null) return false;
            var identityRole = await roles.FindByIdAsync(claim.RoleId) ?? throw new ArgumentException("Role not found.");
            await using var claimTransaction = await db.Database.BeginTransactionAsync();
            Require(await roles.UpdateAsync(identityRole));
            var linked = await db.Set<AppRolePermissionExtension>().Where(x => x.RoleClaimId == claimId).ToListAsync();
            foreach (var history in linked)
            {
                history.RoleClaimId = null;
                history.EffectiveToUtc ??= DateTime.UtcNow;
            }
            await db.SaveChangesAsync();
            db.RoleClaims.Remove(claim);
            await db.SaveChangesAsync();
            await claimTransaction.CommitAsync();
            logger.LogInformation("Identity permission claim revoked: Actor={Actor} Role={Role} Claim={Claim}",
                revokedByUserId, claim.RoleId, claimId);
            return true;
        }
        var extension = await db.Set<AppRolePermissionExtension>().SingleOrDefaultAsync(x => x.RolePermissionId == rolePermissionId);
        if (extension is null || extension.EffectiveToUtc is not null) return false;
        var permission = await db.Set<AppPermissionExtension>().FindAsync(extension.PermissionId);
        if (permission is null) return false;
        var grant = await db.RoleClaims.SingleOrDefaultAsync(x => x.Id == extension.RoleClaimId &&
            x.RoleId == extension.RoleId && x.ClaimType == "permission" && x.ClaimValue == permission.PermissionKey);
        if (grant is null) return false;
        var role = await roles.FindByIdAsync(grant.RoleId) ?? throw new ArgumentException("Role not found.");
        await using var transaction = await db.Database.BeginTransactionAsync();
        Require(await roles.UpdateAsync(role));
        // Preserve every history row linked to this exact claim before deleting it.
        var linkedHistory = await db.Set<AppRolePermissionExtension>().Where(x => x.RoleClaimId == grant.Id).ToListAsync();
        foreach (var history in linkedHistory)
        {
            history.RoleClaimId = null;
            history.EffectiveToUtc ??= DateTime.UtcNow;
        }
        await db.SaveChangesAsync();
        db.RoleClaims.Remove(grant);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        logger.LogInformation("Permission revoked: Actor={Actor} Role={Role} Permission={Permission}",
            revokedByUserId, role.Id, grant.ClaimValue);
        return true;
    }

    public async Task<List<RepositoryRolePermission>> GetRolePermissionsAsync(string roleId)
    {
        var claims = await db.RoleClaims.AsNoTracking().Where(x => x.RoleId == roleId &&
            x.ClaimType == "permission" && x.ClaimValue != null).ToListAsync();
        var matching = await (from extension in db.Set<AppRolePermissionExtension>().AsNoTracking()
                join claim in db.RoleClaims on extension.RoleClaimId equals (int?)claim.Id
                join permission in db.Set<AppPermissionExtension>() on extension.PermissionId equals permission.PermissionId
                where extension.RoleId == roleId && extension.EffectiveToUtc == null &&
                    claim.RoleId == roleId && claim.ClaimType == "permission" &&
                    claim.ClaimValue == permission.PermissionKey
                select extension).ToListAsync();
        return claims.Select(claim =>
        {
            var extension = matching.SingleOrDefault(x => x.RoleClaimId == claim.Id);
            return extension is not null ? Permission(extension) : new RepositoryRolePermission
            {
                RolePermissionId = "claim:" + claim.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
                RoleId = roleId, PermissionId = claim.ClaimValue!, SourceSystem = "Identity",
                EffectiveFromUtc = DateTime.MinValue
            };
        }).ToList();
    }

    public async Task<List<RepositoryRoleDetails>> GetRoleCatalogAsync()
    {
        var metadata = await db.Set<AppRoleExtension>().AsNoTracking().ToDictionaryAsync(x => x.RoleId);
        return (await db.Roles.AsNoTracking().OrderBy(x => x.Name).ToListAsync()).Select(role =>
        {
            metadata.TryGetValue(role.Id, out var extension);
            return new RepositoryRoleDetails { RoleId = role.Id, RoleName = role.Name!, Description = extension?.Description,
                RoleType = extension?.RoleType, RoleCategory = extension?.RoleCategory,
                SystemRoleInd = extension?.SystemRoleInd ?? "N", SensitiveRoleInd = extension?.SensitiveRoleInd ?? "N",
                SodFlag = extension?.SodFlag ?? "N", DisplaySortOrder = extension?.DisplaySortOrder,
                CreatedUtc = extension?.CreatedUtc ?? DateTime.MinValue, ValidFieldScope = extension?.ValidFieldScope };
        }).ToList();
    }

    public async Task<List<RepositoryPermission>> GetPermissionCatalogAsync()
    {
        var metadata = await db.Set<AppPermissionExtension>().AsNoTracking().ToListAsync();
        var result = metadata.Select(x => new RepositoryPermission { PermissionId = x.PermissionId,
            PermissionKey = x.PermissionKey, ResourceKey = x.ResourceKey, ActionKey = x.ActionKey,
            ScopeKey = x.ScopeKey, DomainKey = x.DomainKey, PolicyMappingKey = x.PolicyMappingKey,
            Description = x.Description, RiskLevel = x.RiskLevel }).ToList();
        var identityCodes = await db.RoleClaims.AsNoTracking()
            .Where(x => x.ClaimType == "permission" && x.ClaimValue != null && x.ClaimValue != "")
            .Select(x => x.ClaimValue!).Distinct().ToListAsync();
        result.AddRange(PermissionCodes().Concat(identityCodes).Distinct(StringComparer.Ordinal)
            .Except(metadata.Select(x => x.PermissionKey), StringComparer.Ordinal)
            .Select(code => new RepositoryPermission { PermissionId = code, PermissionKey = code }));
        return result.OrderBy(x => x.PermissionKey, StringComparer.Ordinal).ToList();
    }

    private static string[] PermissionCodes() => typeof(PermissionConstants).GetNestedTypes(BindingFlags.Public)
        .SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.Static))
        .Where(field => field.IsLiteral && field.FieldType == typeof(string))
        .Select(field => (string)field.GetRawConstantValue()!).Distinct().Order().ToArray();

    private static RepositoryUserRole Assignment(AppUserRoleExtension extension) => new()
    {
        UserRoleId = extension.UserRoleId, UserId = extension.UserId, RoleId = extension.RoleId,
        ApprovalStatus = extension.ApprovalStatus, ApprovalReference = extension.ApprovalReference,
        GrantedByUserId = extension.GrantedByUserId, AssignmentReason = extension.AssignmentReason,
        EffectiveFromUtc = extension.EffectiveFromUtc, EffectiveToUtc = extension.EffectiveToUtc
    };

    private static RepositoryRolePermission Permission(AppRolePermissionExtension extension) => new()
    {
        RolePermissionId = extension.RolePermissionId, RoleId = extension.RoleId,
        PermissionId = extension.PermissionId, SourceSystem = extension.SourceSystem,
        EffectiveFromUtc = extension.EffectiveFromUtc, EffectiveToUtc = extension.EffectiveToUtc,
        ApprovedByUserId = extension.ApprovedByUserId, ApprovedAtUtc = extension.ApprovedAtUtc
    };

    private static void Require(IdentityResult result)
    {
        if (!result.Succeeded) throw new InvalidOperationException(string.Join(", ", result.Errors.Select(x => x.Code)));
    }
}
