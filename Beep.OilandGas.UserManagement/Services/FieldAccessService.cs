using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.UserManagement.Models.Scope;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;

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
                new() { FieldName = "USER_ID", FilterValue = userId },
                new() { FieldName = "ACTIVE_IND", FilterValue = "Y" },
            };

            var assets = (await assetRepo.GetAsync(assetFilters))
                .OfType<UserAssetAccess>()
                .ToList();

            foreach (var asset in assets)
            {
                if (!string.IsNullOrWhiteSpace(asset.FIELD_ID))
                {
                    if (asset.FIELD_ID == "*" || asset.FIELD_ID.Equals("GLOBAL", StringComparison.OrdinalIgnoreCase))
                    {
                        return new List<string> { "*" };
                    }
                    fields.Add(asset.FIELD_ID);
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
                new() { FieldName = "USER_ID", FilterValue = userId },
            };

            var scopes = (await scopeRepo.GetAsync(scopeFilters))
                .OfType<UserScopeAssignment>()
                .ToList();

            foreach (var scope in scopes)
            {
                if (scope.SCOPE_TYPE?.Equals("GLOBAL", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return new List<string> { "*" };
                }
            }

            // 3. If SYSTEM user, grant global access
            if (userId.Equals("SYSTEM", StringComparison.OrdinalIgnoreCase))
            {
                return new List<string> { "*" };
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to resolve field access for user {UserId}", userId);
        }

        return fields.Any() ? fields.ToList() : new List<string>();
    }

    public async Task<bool> HasFieldAccessAsync(string userId, string fieldId)
    {
        var fields = await GetUserFieldsAsync(userId);
        return fields.Contains("*") || fields.Contains(fieldId, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<List<string>> GetUserFieldsByAssetTypeAsync(string userId, string assetType)
    {
        var allFields = await GetUserFieldsAsync(userId);
        if (allFields.Contains("*"))
            return allFields;

        // For specific asset type filtering, we'd query USER_ASSET_ACCESS with the asset type
        // For now, return the full list — asset-type filtering can be added when needed
        return allFields;
    }
}
