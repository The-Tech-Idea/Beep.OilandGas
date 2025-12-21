using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.DTOs;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.LifeCycle.Services.AccessControl
{
    /// <summary>
    /// Service for managing user asset access
    /// </summary>
    public class UserAssetAccessService : IAccessControlService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly PPDMMappingService _mappingService;
        private readonly string _connectionName;

        public UserAssetAccessService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            PPDMMappingService mappingService,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _mappingService = mappingService ?? throw new ArgumentNullException(nameof(mappingService));
            _connectionName = connectionName;
        }

        public async Task<AccessCheckResponse> CheckAssetAccessAsync(string userId, string assetId, string assetType, string? requiredPermission = null)
        {
            try
            {
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "USER_ID", Operator = "=", FilterValue = userId },
                    new AppFilter { FieldName = "ASSET_ID", Operator = "=", FilterValue = assetId },
                    new AppFilter { FieldName = "ASSET_TYPE", Operator = "=", FilterValue = assetType },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                // Note: USER_ASSET_ACCESS is a custom table, not a PPDM model
                // We'll use PPDMGenericRepository with object casting
                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(object), _connectionName, "USER_ASSET_ACCESS");

                var results = await repo.GetAsync(filters);

                if (results != null && results.Any())
                {
                    var access = results.First();
                    var accessLevel = GetPropertyValue(access, "ACCESS_LEVEL")?.ToString() ?? "READ";
                    var inherit = GetPropertyValue(access, "INHERIT_IND")?.ToString() == "Y";

                    // If no specific permission required, just check access level
                    if (string.IsNullOrEmpty(requiredPermission))
                    {
                        return new AccessCheckResponse
                        {
                            HasAccess = true,
                            AccessLevel = accessLevel,
                            Reason = "Direct access granted"
                        };
                    }

                    // If permission is required, check user's role permissions
                    // This would require integration with role permission service
                    // For now, return based on access level (WRITE/DELETE implies higher permissions)
                    bool hasPermission = accessLevel == "WRITE" || accessLevel == "DELETE";
                    return new AccessCheckResponse
                    {
                        HasAccess = hasPermission,
                        AccessLevel = accessLevel,
                        Reason = hasPermission ? "Permission granted via access level" : "Insufficient permission"
                    };
                }

                // Check for inherited access (parent assets)
                // This would require hierarchy traversal - simplified for now
                return new AccessCheckResponse
                {
                    HasAccess = false,
                    Reason = "No access found"
                };
            }
            catch (Exception ex)
            {
                return new AccessCheckResponse
                {
                    HasAccess = false,
                    Reason = $"Error checking access: {ex.Message}"
                };
            }
        }

        public async Task<List<AssetAccessDTO>> GetUserAccessibleAssetsAsync(string userId, string? assetType = null, string? organizationId = null, bool includeInherited = true)
        {
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "USER_ID", Operator = "=", FilterValue = userId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (!string.IsNullOrEmpty(assetType))
            {
                filters.Add(new AppFilter { FieldName = "ASSET_TYPE", Operator = "=", FilterValue = assetType });
            }

            if (!string.IsNullOrEmpty(organizationId))
            {
                filters.Add(new AppFilter { FieldName = "ORGANIZATION_ID", Operator = "=", FilterValue = organizationId });
            }

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(object), _connectionName, "USER_ASSET_ACCESS");

            var results = await repo.GetAsync(filters);
            var assetAccessList = new List<AssetAccessDTO>();

            foreach (var result in results ?? Enumerable.Empty<object>())
            {
                assetAccessList.Add(new AssetAccessDTO
                {
                    UserId = GetPropertyValue(result, "USER_ID")?.ToString() ?? string.Empty,
                    AssetType = GetPropertyValue(result, "ASSET_TYPE")?.ToString() ?? string.Empty,
                    AssetId = GetPropertyValue(result, "ASSET_ID")?.ToString() ?? string.Empty,
                    AccessLevel = GetPropertyValue(result, "ACCESS_LEVEL")?.ToString() ?? "READ",
                    Inherit = GetPropertyValue(result, "INHERIT_IND")?.ToString() == "Y",
                    OrganizationId = GetPropertyValue(result, "ORGANIZATION_ID")?.ToString(),
                    Active = GetPropertyValue(result, "ACTIVE_IND")?.ToString() == "Y"
                });
            }

            // TODO: If includeInherited, add child assets based on hierarchy
            return assetAccessList;
        }

        public async Task<List<string>> GetUserRolesAsync(string userId, string? organizationId = null)
        {
            // This would query BA_AUTHORITY or a user-role mapping table
            // For now, return empty list - will be implemented with role service integration
            return new List<string>();
        }

        public async Task<bool> HasPermissionAsync(string userId, string permissionId, string? organizationId = null)
        {
            // This requires integration with role permission service
            // Check user's roles, then check if any role has the permission
            var roles = await GetUserRolesAsync(userId, organizationId);
            // TODO: Check permissions for roles
            return false;
        }

        public async Task<bool> GrantAssetAccessAsync(string userId, string assetId, string assetType, string accessLevel = "READ", bool inherit = true, string? organizationId = null)
        {
            try
            {
                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(object), _connectionName, "USER_ASSET_ACCESS");

                var entity = new Dictionary<string, object>
                {
                    { "USER_ID", userId },
                    { "ASSET_TYPE", assetType },
                    { "ASSET_ID", assetId },
                    { "ACCESS_LEVEL", accessLevel },
                    { "INHERIT_IND", inherit ? "Y" : "N" },
                    { "ACTIVE_IND", "Y" }
                };

                if (!string.IsNullOrEmpty(organizationId))
                {
                    entity["ORGANIZATION_ID"] = organizationId;
                }

                // Use repository to insert/update
                // Note: This is simplified - actual implementation would need proper entity creation
                // await repo.CreateAsync(entity);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RevokeAssetAccessAsync(string userId, string assetId, string assetType)
        {
            try
            {
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "USER_ID", Operator = "=", FilterValue = userId },
                    new AppFilter { FieldName = "ASSET_ID", Operator = "=", FilterValue = assetId },
                    new AppFilter { FieldName = "ASSET_TYPE", Operator = "=", FilterValue = assetType }
                };

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(object), _connectionName, "USER_ASSET_ACCESS");

                var results = await repo.GetAsync(filters);
                // TODO: Soft delete by setting ACTIVE_IND = 'N'
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<string>> GetRolePermissionsAsync(string roleId, string? organizationId = null)
        {
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ROLE_ID", Operator = "=", FilterValue = roleId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (!string.IsNullOrEmpty(organizationId))
            {
                filters.Add(new AppFilter { FieldName = "ORGANIZATION_ID", Operator = "=", FilterValue = organizationId });
            }

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(object), _connectionName, "ROLE_PERMISSION");

            var results = await repo.GetAsync(filters);
            return results?
                .Select(r => GetPropertyValue(r, "PERMISSION_ID")?.ToString())
                .Where(p => !string.IsNullOrEmpty(p))
                .ToList() ?? new List<string>();
        }

        public async Task<bool> AssignPermissionToRoleAsync(string roleId, string permissionId, string? organizationId = null)
        {
            try
            {
                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(object), _connectionName, "ROLE_PERMISSION");

                var entity = new Dictionary<string, object>
                {
                    { "ROLE_ID", roleId },
                    { "PERMISSION_ID", permissionId },
                    { "ACTIVE_IND", "Y" }
                };

                if (!string.IsNullOrEmpty(organizationId))
                {
                    entity["ORGANIZATION_ID"] = organizationId;
                }

                // await repo.CreateAsync(entity);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionId, string? organizationId = null)
        {
            try
            {
                // Soft delete by setting ACTIVE_IND = 'N'
                return true;
            }
            catch
            {
                return false;
            }
        }

        private object? GetPropertyValue(object obj, string propertyName)
        {
            if (obj == null) return null;

            if (obj is Dictionary<string, object> dict)
            {
                return dict.TryGetValue(propertyName, out var value) ? value : null;
            }

            var prop = obj.GetType().GetProperty(propertyName);
            return prop?.GetValue(obj);
        }
    }
}
