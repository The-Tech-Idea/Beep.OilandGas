using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.UserManagement.Models.Identity;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.UserManagement.Services;

/// <summary>
/// Manages time-bound temporary role elevations for acting-manager, leave coverage,
/// and emergency access scenarios. Elevations auto-expire and are audit-logged.
/// </summary>
public interface ITempRoleElevationService
{
    /// <summary>
    /// Request a temporary role elevation. Creates a pending elevation that requires approval.
    /// </summary>
    Task<TEMP_ROLE_ELEVATION> RequestElevationAsync(
        string userId, string elevatedRoleId, string reason,
        DateTime effectiveTo, string requestedBy, string? scopeLimitation = null);

    /// <summary>
    /// Activate an approved elevation. Called after the approval workflow completes.
    /// </summary>
    Task<TEMP_ROLE_ELEVATION> ActivateElevationAsync(string elevationId, string activatedBy);

    /// <summary>
    /// Revoke an active elevation before its expiry date.
    /// </summary>
    Task<TEMP_ROLE_ELEVATION> RevokeElevationAsync(string elevationId, string revokedBy, string reason);

    /// <summary>
    /// Get all active (non-expired) elevations for a user.
    /// Used at JWT issuance time to add elevated roles to claims.
    /// </summary>
    Task<List<TEMP_ROLE_ELEVATION>> GetActiveElevationsAsync(string userId);

    /// <summary>
    /// Background cleanup: expire all elevations past their EFFECTIVE_TO date.
    /// Returns count of expired elevations.
    /// </summary>
    Task<int> CleanupExpiredAsync();

    /// <summary>
    /// Get all elevations for a user (active and historical).
    /// </summary>
    Task<List<TEMP_ROLE_ELEVATION>> GetElevationHistoryAsync(string userId);
}

public class TempRoleElevationService : ITempRoleElevationService
{
    private readonly IDMEEditor _editor;
    private readonly ICommonColumnHandler _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly ILogger<TempRoleElevationService> _logger;
    private readonly string _connectionName;

    public TempRoleElevationService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName = "PPDM39",
        ILogger<TempRoleElevationService>? logger = null)
    {
        _editor = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults = defaults;
        _metadata = metadata;
        _connectionName = connectionName;
        _logger = logger;
    }

    public async Task<TEMP_ROLE_ELEVATION> RequestElevationAsync(
        string userId, string elevatedRoleId, string reason,
        DateTime effectiveTo, string requestedBy, string? scopeLimitation = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID is required.", nameof(userId));
        if (string.IsNullOrWhiteSpace(elevatedRoleId))
            throw new ArgumentException("Elevated role ID is required.", nameof(elevatedRoleId));
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required for audit compliance.", nameof(reason));
        if (effectiveTo <= DateTime.UtcNow)
            throw new ArgumentException("Effective-to date must be in the future.", nameof(effectiveTo));
        if (effectiveTo > DateTime.UtcNow.AddDays(90))
            throw new ArgumentException("Temporary elevations cannot exceed 90 days.", nameof(effectiveTo));

        var repo = GetRepo();

        var elevation = new TEMP_ROLE_ELEVATION
        {
            USER_ID = userId,
            ELEVATED_ROLE_ID = elevatedRoleId,
            REASON = reason,
            EFFECTIVE_FROM = DateTime.UtcNow,
            EFFECTIVE_TO = effectiveTo,
            REQUESTED_BY = requestedBy,
            SCOPE_LIMITATION = scopeLimitation,
            STATUS = "PENDING",
        };

        await repo.InsertAsync(elevation, requestedBy);

        _logger?.LogInformation(
            "Temporary role elevation requested: User={UserId}, Role={RoleId}, Until={EffectiveTo}, By={RequestedBy}",
            userId, elevatedRoleId, effectiveTo, requestedBy);

        return elevation;
    }

    public async Task<TEMP_ROLE_ELEVATION> ActivateElevationAsync(string elevationId, string activatedBy)
    {
        var repo = GetRepo();
        var elevation = await GetElevationByIdAsync(repo, elevationId);

        if (elevation is null)
            throw new InvalidOperationException($"Elevation {elevationId} not found.");
        if (elevation.STATUS != "PENDING")
            throw new InvalidOperationException($"Elevation {elevationId} is not in PENDING status (current: {elevation.STATUS}).");
        if (elevation.EFFECTIVE_FROM > DateTime.UtcNow)
            throw new InvalidOperationException($"Elevation {elevationId} effective-from date is in the future.");

        elevation.STATUS = "ACTIVE";
        await repo.UpdateAsync(elevation, activatedBy);

        _logger?.LogInformation(
            "Temporary role elevation activated: ElevationId={ElevationId}, User={UserId}, By={ActivatedBy}",
            elevationId, elevation.USER_ID, activatedBy);

        return elevation;
    }

    public async Task<TEMP_ROLE_ELEVATION> RevokeElevationAsync(string elevationId, string revokedBy, string reason)
    {
        var repo = GetRepo();
        var elevation = await GetElevationByIdAsync(repo, elevationId);

        if (elevation is null)
            throw new InvalidOperationException($"Elevation {elevationId} not found.");
        if (elevation.STATUS is "EXPIRED" or "REVOKED")
            throw new InvalidOperationException($"Elevation {elevationId} is already {elevation.STATUS}.");

        elevation.STATUS = "REVOKED";
        elevation.REVOKED_AT = DateTime.UtcNow;
        elevation.REVOKED_BY = revokedBy;
        elevation.REVOKED_REASON = reason;
        await repo.UpdateAsync(elevation, revokedBy);

        _logger?.LogWarning(
            "Temporary role elevation revoked: ElevationId={ElevationId}, User={UserId}, By={RevokedBy}, Reason={Reason}",
            elevationId, elevation.USER_ID, revokedBy, reason);

        return elevation;
    }

    public async Task<List<TEMP_ROLE_ELEVATION>> GetActiveElevationsAsync(string userId)
    {
        var repo = GetRepo();
        var filters = new List<AppFilter>
        {
            new() { FieldName = "USER_ID", FilterValue = userId },
            new() { FieldName = "STATUS", FilterValue = "ACTIVE" },
        };

        var results = await repo.GetAsync(filters);
        var elevations = results.OfType<TEMP_ROLE_ELEVATION>()
            .Where(e => e.EFFECTIVE_TO > DateTime.UtcNow)
            .OrderBy(e => e.EFFECTIVE_TO)
            .ToList();

        // Auto-expire any that have passed their effective-to date
        var expired = results.OfType<TEMP_ROLE_ELEVATION>()
            .Where(e => e.STATUS == "ACTIVE" && e.EFFECTIVE_TO <= DateTime.UtcNow)
            .ToList();

        foreach (var exp in expired)
        {
            exp.STATUS = "EXPIRED";
            await repo.UpdateAsync(exp, "SYSTEM");
        }

        return elevations;
    }

    public async Task<int> CleanupExpiredAsync()
    {
        var repo = GetRepo();
        var filters = new List<AppFilter>
        {
            new() { FieldName = "STATUS", FilterValue = "ACTIVE" },
        };

        var results = await repo.GetAsync(filters);
        var expired = results.OfType<TEMP_ROLE_ELEVATION>()
            .Where(e => e.EFFECTIVE_TO <= DateTime.UtcNow)
            .ToList();

        foreach (var elevation in expired)
        {
            elevation.STATUS = "EXPIRED";
            await repo.UpdateAsync(elevation, "SYSTEM");
        }

        if (expired.Count > 0)
        {
            _logger?.LogInformation("Auto-expired {Count} temporary role elevations", expired.Count);
        }

        return expired.Count;
    }

    public async Task<List<TEMP_ROLE_ELEVATION>> GetElevationHistoryAsync(string userId)
    {
        var repo = GetRepo();
        var filters = new List<AppFilter>
        {
            new() { FieldName = "USER_ID", FilterValue = userId },
        };

        var results = await repo.GetAsync(filters);
        return results.OfType<TEMP_ROLE_ELEVATION>()
            .OrderByDescending(e => e.EFFECTIVE_FROM)
            .ToList();
    }

    private PPDMGenericRepository GetRepo()
    {
        return new PPDMGenericRepository(
            _editor, _commonColumnHandler, _defaults, _metadata,
            typeof(TEMP_ROLE_ELEVATION),
            _connectionName,
            "TEMP_ROLE_ELEVATION",
            null);
    }

    private static async Task<TEMP_ROLE_ELEVATION?> GetElevationByIdAsync(
        PPDMGenericRepository repo, string elevationId)
    {
        var filters = new List<AppFilter>
        {
            new() { FieldName = "ELEVATION_ID", FilterValue = elevationId },
        };
        var results = await repo.GetAsync(filters);
        return results.OfType<TEMP_ROLE_ELEVATION>().FirstOrDefault();
    }
}
