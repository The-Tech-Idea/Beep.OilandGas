using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.UserManagement.Models.Scope;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.UserManagement.Services;

/// <summary>
/// Resolves field-level access scoping for a user.
/// Used at JWT issuance time to populate the "field_scope" claim.
/// </summary>
public interface IFieldAccessService
{
    /// <summary>
    /// Get the list of field IDs the user has access to.
    /// Returns ["*"] for users with global scope.
    /// </summary>
    Task<List<string>> GetUserFieldsAsync(string userId);

    /// <summary>
    /// Check if a user has access to a specific field.
    /// </summary>
    Task<bool> HasFieldAccessAsync(string userId, string fieldId);

    /// <summary>
    /// Get all fields accessible to a user, optionally filtered by a specific asset type.
    /// </summary>
    Task<List<string>> GetUserFieldsByAssetTypeAsync(string userId, string assetType);
}

public class FieldAccessService : IFieldAccessService
{
    private readonly IDMEEditor _editor;
    private readonly ICommonColumnHandler _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly ILogger<FieldAccessService> _logger;
    private readonly string _connectionName;

    public FieldAccessService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName = "PPDM39",
        ILogger<FieldAccessService>? logger = null)
    {
        _editor = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults = defaults;
        _metadata = metadata;
        _connectionName = connectionName;
        _logger = logger;
    }

    public async Task<List<string>> GetUserFieldsAsync(string userId)
    {
        var fields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(userId))
            return new();
        var now = DateTime.UtcNow;

        try
        {
            // 1. Check USER_ASSET_ACCESS for field-level assignments
            var assetRepo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(UserAssetAccess),
                _connectionName,
                "USER_ASSET_ACCESS",
                null);

            var assetFilters = new List<AppFilter>
            {
                new() { FieldName = "USER_ID", Operator = "=", FilterValue = userId },
                new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
            };

            var assets = (await assetRepo.GetAsync(assetFilters))
                .OfType<UserAssetAccess>()
                .Where(a => a.USER_ID == userId && a.ACTIVE_IND == "Y" &&
                    string.Equals(a.ASSET_TYPE, "FIELD", StringComparison.OrdinalIgnoreCase) &&
                    (!a.ACCESS_EXPIRES_UTC.HasValue || a.ACCESS_EXPIRES_UTC > now))
                .ToList();

            var denied = assets.Where(a => string.Equals(a.DENY_OVERRIDE_IND, "Y", StringComparison.OrdinalIgnoreCase))
                .Select(a => a.ASSET_ID).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var global = false;
            foreach (var asset in assets)
            {
                if (!string.IsNullOrWhiteSpace(asset.ASSET_ID) && !denied.Contains(asset.ASSET_ID))
                {
                    if (asset.ASSET_ID == "*" || asset.ASSET_ID.Equals("GLOBAL", StringComparison.OrdinalIgnoreCase))
                        global = true;
                    else if (!asset.ASSET_ID.Contains(','))
                        fields.Add(asset.ASSET_ID);
                }
            }

            // 2. Check USER_SCOPE_ASSIGNMENT for GLOBAL scope
            var scopeRepo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(UserScopeAssignment),
                _connectionName,
                "USER_SCOPE_ASSIGNMENT",
                null);

            var scopeFilters = new List<AppFilter>
            {
                new() { FieldName = "USER_ID", Operator = "=", FilterValue = userId },
                new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
            };

            var scopes = (await scopeRepo.GetAsync(scopeFilters))
                .OfType<UserScopeAssignment>()
                .Where(s => s.USER_ID == userId && s.ACTIVE_IND == "Y" && s.EFFECTIVE_FROM_UTC <= now &&
                    (!s.EFFECTIVE_TO_UTC.HasValue || s.EFFECTIVE_TO_UTC > now))
                .ToList();

            foreach (var scope in scopes)
            {
                if (scope.SCOPE_TYPE?.Equals("GLOBAL", StringComparison.OrdinalIgnoreCase) == true)
                {
                    global = true;
                }
                else if (string.Equals(scope.SCOPE_TYPE, "FIELD", StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(scope.SCOPE_VALUE) && !scope.SCOPE_VALUE.Contains(','))
                {
                    if (scope.SCOPE_VALUE == "*" || scope.SCOPE_VALUE.Equals("GLOBAL", StringComparison.OrdinalIgnoreCase))
                        global = true;
                    else
                        fields.Add(scope.SCOPE_VALUE);
                }
            }

            if (denied.Contains("*") || denied.Contains("GLOBAL"))
                return new();
            fields.ExceptWith(denied);
            // The existing claim format cannot represent wildcard access with exclusions.
            if (global && denied.Count == 0)
                return new() { "*" };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to resolve field access for user {UserId}", userId);
            return new();
        }

        return fields.Any() ? fields.ToList() : new List<string>();
    }

    public async Task<bool> HasFieldAccessAsync(string userId, string fieldId)
    {
        if (string.IsNullOrWhiteSpace(fieldId) || fieldId.Contains(','))
            return false;
        var fields = await GetUserFieldsAsync(userId);
        return fields.Contains("*") || fields.Contains(fieldId, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<List<string>> GetUserFieldsByAssetTypeAsync(string userId, string assetType)
    {
        // No authoritative well/facility-to-field mapping is available in this resolver.
        if (!string.Equals(assetType, "FIELD", StringComparison.OrdinalIgnoreCase))
            return new();
        return await GetUserFieldsAsync(userId);
    }
}
