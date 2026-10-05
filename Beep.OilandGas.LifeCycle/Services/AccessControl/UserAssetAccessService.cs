using Beep.OilandGas.PPDM39.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;

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
        private readonly IUserAssetAccessStore _assetStore;
        private readonly Func<Task<AssetDatabaseScope>> _resolveScope;
        private readonly IApplicationAuthorizationReader _authorization;
        private readonly IApplicationRolePermissionStore _rolePermissions;

        public UserAssetAccessService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            PPDMMappingService mappingService,
            IApplicationAuthorizationReader authorization,
            IApplicationRolePermissionStore rolePermissions,
            IUserAssetAccessStore assetStore,
            Func<Task<AssetDatabaseScope>> resolveScope)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _mappingService = mappingService ?? throw new ArgumentNullException(nameof(mappingService));
            _assetStore = assetStore ?? throw new ArgumentNullException(nameof(assetStore));
            _resolveScope = resolveScope ?? throw new ArgumentNullException(nameof(resolveScope));
            _authorization = authorization ?? throw new ArgumentNullException(nameof(authorization));
            _rolePermissions = rolePermissions ?? throw new ArgumentNullException(nameof(rolePermissions));
        }

        /// <remarks>
        /// OILGAS-CATCH-01: a check that cannot be made reaches the caller (an authorization filter or the API's handler
        /// reports it with its reference, and the request is still refused). It had been answered as access denied
        /// ("could not be verified"), and the failure itself was told to no one.
        /// </remarks>
        public async Task<AccessCheckResponse> CheckAssetAccessAsync(string userId, string assetId, string assetType, string? requiredPermission = null)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(assetId) || string.IsNullOrWhiteSpace(assetType))
                return new AccessCheckResponse { HasAccess = false, Reason = "User and asset identifiers are required." };
            var assets = await GetUserAccessibleAssetsAsync(userId, includeInherited: true);
            var access = assets.FirstOrDefault(candidate => candidate.Active && candidate.UserId == userId
                && candidate.AssetId == assetId && string.Equals(candidate.AssetType, assetType, StringComparison.OrdinalIgnoreCase));
            if (access is null)
                return new AccessCheckResponse { HasAccess = false, Reason = "No direct or inherited access found." };

            var permitted = string.IsNullOrWhiteSpace(requiredPermission)
                || await HasPermissionAsync(userId, requiredPermission);
            return new AccessCheckResponse
            {
                HasAccess = permitted,
                AccessLevel = access.AccessLevel,
                Reason = permitted ? "Access granted for the requested asset." : "The required application permission is not assigned."
            };
        }

        public async Task<List<AssetAccess>> GetUserAccessibleAssetsAsync(string userId, string? assetType = null, string? organizationId = null, bool includeInherited = true)
        {
            var scope = await ResolveScopeAsync();
            var grants = await _assetStore.ReadAsync(userId, scope.Fingerprint, organizationId);
            var assetAccessList = grants.Select(grant => new AssetAccess
            {
                UserId = grant.UserId,
                AssetType = grant.AssetType,
                AssetId = grant.AssetId,
                AccessLevel = grant.AccessLevel,
                Inherit = grant.Inherit,
                OrganizationId = grant.OrganizationId,
                Active = grant.IsActive
            }).ToList();

            // If includeInherited, add child assets based on hierarchy
            if (includeInherited)
            {
                var inheritedAssets = new List<AssetAccess>();

                foreach (var assetAccess in assetAccessList.Where(a => a.UserId == userId && a.Inherit && a.Active))
                {
                    // FIELD → WELL, POOL, FACILITY
                    if (assetAccess.AssetType?.ToUpper() == "FIELD")
                    {
                        var source = _editor.GetDataSource(scope.ConnectionName)
                            ?? throw new InvalidOperationException("The asset hierarchy datasource is unavailable.");
                        Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup.MigrationConnectionTarget.Validate(
                            _editor, source, scope.ConnectionName);
                        // Get wells for this field
                        var wellRepo = new PPDMGenericRepository(
                            _editor, _commonColumnHandler, _defaults, _metadata,
                            typeof(Beep.OilandGas.PPDM39.Models.WELL), scope.ConnectionName, "WELL");
                        var wellFilters = new List<AppFilter>
                        {
                            new AppFilter { FieldName = "ASSIGNED_FIELD", Operator = "=", FilterValue = assetAccess.AssetId }
                        };
                        var wells = await wellRepo.GetAsync(wellFilters);
                        foreach (var well in wells ?? Enumerable.Empty<object>())
                        {
                            if (GetPropertyValue(well, "ASSIGNED_FIELD")?.ToString() != assetAccess.AssetId) continue;
                            var wellId = GetPropertyValue(well, "UWI")?.ToString();
                            if (!string.IsNullOrEmpty(wellId) && !assetAccessList.Any(a => a.AssetId == wellId && a.AssetType == "WELL"))
                            {
                                inheritedAssets.Add(new AssetAccess
                                {
                                    UserId = userId,
                                    AssetType = "WELL",
                                    AssetId = wellId,
                                    AccessLevel = assetAccess.AccessLevel,
                                    Inherit = false, // Don't inherit from inherited assets
                                    OrganizationId = assetAccess.OrganizationId,
                                    Active = true
                                });
                            }
                        }

                        // Get pools for this field
                        var poolRepo = new PPDMGenericRepository(
                            _editor, _commonColumnHandler, _defaults, _metadata,
                            typeof(Beep.OilandGas.PPDM39.Models.POOL), scope.ConnectionName, "POOL");
                        var poolFilters = new List<AppFilter>
                        {
                            new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = assetAccess.AssetId }
                        };
                        var pools = await poolRepo.GetAsync(poolFilters);
                        foreach (var pool in pools ?? Enumerable.Empty<object>())
                        {
                            if (GetPropertyValue(pool, "FIELD_ID")?.ToString() != assetAccess.AssetId) continue;
                            var poolId = GetPropertyValue(pool, "POOL_ID")?.ToString();
                            if (!string.IsNullOrEmpty(poolId) && !assetAccessList.Any(a => a.AssetId == poolId && a.AssetType == "POOL"))
                            {
                                inheritedAssets.Add(new AssetAccess
                                {
                                    UserId = userId,
                                    AssetType = "POOL",
                                    AssetId = poolId,
                                    AccessLevel = assetAccess.AccessLevel,
                                    Inherit = false,
                                    OrganizationId = assetAccess.OrganizationId,
                                    Active = true
                                });
                            }
                        }

                        // Get facilities for this field
                        var facilityRepo = new PPDMGenericRepository(
                            _editor, _commonColumnHandler, _defaults, _metadata,
                            typeof(Beep.OilandGas.PPDM39.Models.FACILITY), scope.ConnectionName, "FACILITY");
                        var facilityFilters = new List<AppFilter>
                        {
                            new AppFilter { FieldName = "PRIMARY_FIELD_ID", Operator = "=", FilterValue = assetAccess.AssetId }
                        };
                        var facilities = await facilityRepo.GetAsync(facilityFilters);
                        foreach (var facility in facilities ?? Enumerable.Empty<object>())
                        {
                            if (GetPropertyValue(facility, "PRIMARY_FIELD_ID")?.ToString() != assetAccess.AssetId) continue;
                            var facilityId = GetPropertyValue(facility, "FACILITY_ID")?.ToString();
                            if (!string.IsNullOrEmpty(facilityId) && !assetAccessList.Any(a => a.AssetId == facilityId && a.AssetType == "FACILITY"))
                            {
                                inheritedAssets.Add(new AssetAccess
                                {
                                    UserId = userId,
                                    AssetType = "FACILITY",
                                    AssetId = facilityId,
                                    AccessLevel = assetAccess.AccessLevel,
                                    Inherit = false,
                                    OrganizationId = assetAccess.OrganizationId,
                                    Active = true
                                });
                            }
                        }
                    }

                    // Pool-to-well inheritance requires a validated relationship, not WELL.POOL_ID.
                }

                assetAccessList.AddRange(inheritedAssets);
            }

            if (scope != await ResolveScopeAsync())
                throw new InvalidOperationException("The asset database binding changed during this operation. Retry with the current binding.");
            return assetAccessList.Where(a => a.UserId == userId && a.Active
                && (string.IsNullOrEmpty(assetType) || string.Equals(a.AssetType, assetType, StringComparison.OrdinalIgnoreCase))).ToList();
        }

        public Task<List<string>> GetUserRolesAsync(string userId, string? organizationId = null)
        {
            RejectOrganizationRoleScope(organizationId);
            return _authorization.GetRolesAsync(userId);
        }

        public Task<bool> HasPermissionAsync(string userId, string permissionId, string? organizationId = null)
        {
            RejectOrganizationRoleScope(organizationId);
            return _authorization.HasPermissionAsync(userId, permissionId);
        }

        private static void RejectOrganizationRoleScope(string? organizationId)
        {
            if (!string.IsNullOrWhiteSpace(organizationId))
                throw new NotSupportedException("Application roles are not organization-scoped. Use the separate organization and asset access checks.");
        }

        public async Task<bool> GrantAssetAccessAsync(string userId, string assetId, string assetType, string accessLevel = "READ", bool inherit = true, string? organizationId = null)
        {
            var scope = await ResolveScopeAsync();
            return await _assetStore.GrantAsync(userId, scope.Fingerprint, assetType, assetId, accessLevel, inherit, organizationId);
        }

        public async Task<bool> RevokeAssetAccessAsync(string userId, string assetId, string assetType)
        {
            var scope = await ResolveScopeAsync();
            return await _assetStore.RevokeAsync(userId, scope.Fingerprint, assetType, assetId);
        }

        private async Task<AssetDatabaseScope> ResolveScopeAsync()
        {
            var scope = await _resolveScope();
            if (scope is null || string.IsNullOrWhiteSpace(scope.ConnectionName)
                || scope.Fingerprint is null || scope.Fingerprint.Length != 64
                || scope.Fingerprint.Any(c => !char.IsAsciiHexDigit(c))
                || scope.Fingerprint != scope.Fingerprint.ToUpperInvariant())
                throw new InvalidOperationException("A reviewed module database scope is required for asset access.");
            return scope;
        }

        public Task<List<string>> GetRolePermissionsAsync(string roleId, string? organizationId = null)
        {
            RejectOrganizationRoleScope(organizationId);
            return _rolePermissions.GetPermissionsAsync(roleId);
        }

        public Task<bool> AssignPermissionToRoleAsync(string roleId, string permissionId, string? organizationId = null)
        {
            RejectOrganizationRoleScope(organizationId);
            return _rolePermissions.GrantAsync(roleId, permissionId);
        }

        public Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionId, string? organizationId = null)
        {
            RejectOrganizationRoleScope(organizationId);
            return _rolePermissions.RevokeAsync(roleId, permissionId);
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
