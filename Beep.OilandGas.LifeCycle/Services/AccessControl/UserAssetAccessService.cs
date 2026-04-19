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

                    // Check specific permission via role-permission table
                    bool hasPermission = await HasPermissionAsync(userId, requiredPermission);
                    if (!hasPermission)
                    {
                        // Fallback: WRITE/DELETE access level implies full permissions
                        hasPermission = accessLevel == "WRITE" || accessLevel == "DELETE";
                    }
                    return new AccessCheckResponse
                    {
                        HasAccess = hasPermission,
                        AccessLevel = accessLevel,
                        Reason = hasPermission ? "Permission granted via role assignment" : "Insufficient permission for requested operation"
                    };
                }

                // Check for inherited access: look for wildcard or type-level grants with INHERIT_IND = Y
                var inheritRepo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(object), _connectionName, "USER_ASSET_ACCESS");

                var inheritFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "USER_ID", Operator = "=", FilterValue = userId },
                    new AppFilter { FieldName = "ASSET_TYPE", Operator = "=", FilterValue = assetType },
                    new AppFilter { FieldName = "INHERIT_IND", Operator = "=", FilterValue = "Y" },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var inheritedResults = await inheritRepo.GetAsync(inheritFilters);
                if (inheritedResults != null && inheritedResults.Any())
                {
                    var inherited = inheritedResults.First();
                    var inheritedLevel = GetPropertyValue(inherited, "ACCESS_LEVEL")?.ToString() ?? "READ";
                    return new AccessCheckResponse
                    {
                        HasAccess = true,
                        AccessLevel = inheritedLevel,
                        Reason = "Inherited access granted via type-level permission"
                    };
                }

                return new AccessCheckResponse
                {
                    HasAccess = false,
                    Reason = "No direct or inherited access found"
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

        public async Task<List<AssetAccess>> GetUserAccessibleAssetsAsync(string userId, string? assetType = null, string? organizationId = null, bool includeInherited = true)
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
            var assetAccessList = new List<AssetAccess>();

            foreach (var result in results ?? Enumerable.Empty<object>())
            {
                assetAccessList.Add(new AssetAccess
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

            // If includeInherited, add child assets based on hierarchy
            if (includeInherited)
            {
                var inheritedAssets = new List<AssetAccess>();

                foreach (var assetAccess in assetAccessList.Where(a => a.Inherit && a.Active))
                {
                    // FIELD → WELL, POOL, FACILITY
                    if (assetAccess.AssetType?.ToUpper() == "FIELD")
                    {
                        // Get wells for this field
                        var wellRepo = new PPDMGenericRepository(
                            _editor, _commonColumnHandler, _defaults, _metadata,
                            typeof(object), _connectionName, "WELL");
                        var wellFilters = new List<AppFilter>
                        {
                            new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = assetAccess.AssetId }
                        };
                        var wells = await wellRepo.GetAsync(wellFilters);
                        foreach (var well in wells ?? Enumerable.Empty<object>())
                        {
                            var wellId = GetPropertyValue(well, "WELL_ID")?.ToString();
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
                            typeof(object), _connectionName, "POOL");
                        var poolFilters = new List<AppFilter>
                        {
                            new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = assetAccess.AssetId }
                        };
                        var pools = await poolRepo.GetAsync(poolFilters);
                        foreach (var pool in pools ?? Enumerable.Empty<object>())
                        {
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
                            typeof(object), _connectionName, "FACILITY");
                        var facilityFilters = new List<AppFilter>
                        {
                            new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = assetAccess.AssetId }
                        };
                        var facilities = await facilityRepo.GetAsync(facilityFilters);
                        foreach (var facility in facilities ?? Enumerable.Empty<object>())
                        {
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

                    // POOL → WELL
                    if (assetAccess.AssetType?.ToUpper() == "POOL")
                    {
                        var wellRepo = new PPDMGenericRepository(
                            _editor, _commonColumnHandler, _defaults, _metadata,
                            typeof(object), _connectionName, "WELL");
                        var wellFilters = new List<AppFilter>
                        {
                            new AppFilter { FieldName = "POOL_ID", Operator = "=", FilterValue = assetAccess.AssetId }
                        };
                        var wells = await wellRepo.GetAsync(wellFilters);
                        foreach (var well in wells ?? Enumerable.Empty<object>())
                        {
                            var wellId = GetPropertyValue(well, "WELL_ID")?.ToString();
                            if (!string.IsNullOrEmpty(wellId) && !assetAccessList.Any(a => a.AssetId == wellId && a.AssetType == "WELL"))
                            {
                                inheritedAssets.Add(new AssetAccess
                                {
                                    UserId = userId,
                                    AssetType = "WELL",
                                    AssetId = wellId,
                                    AccessLevel = assetAccess.AccessLevel,
                                    Inherit = false,
                                    OrganizationId = assetAccess.OrganizationId,
                                    Active = true
                                });
                            }
                        }
                    }
                }

                assetAccessList.AddRange(inheritedAssets);
            }

            return assetAccessList;
        }

        public async Task<List<string>> GetUserRolesAsync(string userId, string? organizationId = null)
        {
            try
            {
                // Query USER_ROLE or BA_AUTHORITY table for user roles
                var userRoleRepo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(object), _connectionName, "USER_ROLE");
                
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "USER_ID", Operator = "=", FilterValue = userId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                if (!string.IsNullOrEmpty(organizationId))
                {
                    filters.Add(new AppFilter { FieldName = "ORGANIZATION_ID", Operator = "=", FilterValue = organizationId });
                }

                var results = await userRoleRepo.GetAsync(filters);
                var roles = new List<string>();

                foreach (var result in results ?? Enumerable.Empty<object>())
                {
                    var roleId = GetPropertyValue(result, "ROLE_ID")?.ToString();
                    if (!string.IsNullOrEmpty(roleId))
                    {
                        roles.Add(roleId);
                    }
                }

                // If USER_ROLE table doesn't exist, try BA_AUTHORITY
                if (!roles.Any())
                {
                    var authorityRepo = new PPDMGenericRepository(
                        _editor, _commonColumnHandler, _defaults, _metadata,
                        typeof(object), _connectionName, "BA_AUTHORITY");
                    
                    var authorityFilters = new List<AppFilter>
                    {
                        new AppFilter { FieldName = "USER_ID", Operator = "=", FilterValue = userId }
                    };

                    var authorityResults = await authorityRepo.GetAsync(authorityFilters);
                    foreach (var result in authorityResults ?? Enumerable.Empty<object>())
                    {
                        var roleId = GetPropertyValue(result, "ROLE_ID")?.ToString() ?? 
                                    GetPropertyValue(result, "AUTHORITY_ID")?.ToString();
                        if (!string.IsNullOrEmpty(roleId))
                        {
                            roles.Add(roleId);
                        }
                    }
                }

                return roles;
            }
            catch (Exception)
            {
                // If tables don't exist, return empty list
                return new List<string>();
            }
        }

        public async Task<bool> HasPermissionAsync(string userId, string permissionId, string? organizationId = null)
        {
            try
            {
                // Get user's roles
                var roles = await GetUserRolesAsync(userId, organizationId);
                if (!roles.Any())
                {
                    return false;
                }

                // Query ROLE_PERMISSION table to check if any role has the permission
                var rolePermissionRepo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(object), _connectionName, "ROLE_PERMISSION");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "PERMISSION_ID", Operator = "=", FilterValue = permissionId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                if (!string.IsNullOrEmpty(organizationId))
                {
                    filters.Add(new AppFilter { FieldName = "ORGANIZATION_ID", Operator = "=", FilterValue = organizationId });
                }

                var results = await rolePermissionRepo.GetAsync(filters);

                foreach (var result in results ?? Enumerable.Empty<object>())
                {
                    var roleId = GetPropertyValue(result, "ROLE_ID")?.ToString();
                    if (!string.IsNullOrEmpty(roleId) && roles.Contains(roleId))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
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

                // Use dynamic insert via anonymous object
                var insertObj = new
                {
                    USER_ID = (object)userId,
                    ASSET_TYPE = (object)assetType,
                    ASSET_ID = (object)assetId,
                    ACCESS_LEVEL = (object)accessLevel,
                    INHERIT_IND = (object)(inherit ? "Y" : "N"),
                    ACTIVE_IND = (object)"Y",
                    ORGANIZATION_ID = (object)(organizationId ?? string.Empty),
                    ROW_CREATED_BY = (object)userId,
                    ROW_CREATED_DATE = (object)DateTime.UtcNow,
                    ROW_CHANGED_BY = (object)userId,
                    ROW_CHANGED_DATE = (object)DateTime.UtcNow
                };
                await repo.InsertAsync(insertObj, userId);
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
                
                // Soft delete by setting ACTIVE_IND = 'N'
                foreach (var result in results ?? Enumerable.Empty<object>())
                {
                    if (result is IDictionary<string, object> entity)
                    {
                        entity["ACTIVE_IND"] = "N";
                        entity["UPDATE_DATE"] = DateTime.UtcNow;
                        // UPDATE_USER would be set if userId parameter is available
                        await repo.UpdateAsync(entity, userId);
                    }
                    else
                    {
                        // Use reflection to update properties
                        var activeIndProp = result.GetType().GetProperty("ACTIVE_IND");
                        var updateDateProp = result.GetType().GetProperty("UPDATE_DATE");
                        var updateUserProp = result.GetType().GetProperty("UPDATE_USER");

                        if (activeIndProp != null) activeIndProp.SetValue(result, "N");
                        if (updateDateProp != null) updateDateProp.SetValue(result, DateTime.UtcNow);
                        if (updateUserProp != null) updateUserProp.SetValue(result, userId);

                        await repo.UpdateAsync(result, userId);
                    }
                }

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

                var insertObj = new
                {
                    ROLE_ID = (object)roleId,
                    PERMISSION_ID = (object)permissionId,
                    ACTIVE_IND = (object)"Y",
                    ORGANIZATION_ID = (object)(organizationId ?? string.Empty),
                    ROW_CREATED_BY = (object)(organizationId ?? "SYSTEM"),
                    ROW_CREATED_DATE = (object)DateTime.UtcNow,
                    ROW_CHANGED_BY = (object)(organizationId ?? "SYSTEM"),
                    ROW_CHANGED_DATE = (object)DateTime.UtcNow
                };
                await repo.InsertAsync(insertObj, organizationId ?? "SYSTEM");
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
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ROLE_ID", Operator = "=", FilterValue = roleId },
                    new AppFilter { FieldName = "PERMISSION_ID", Operator = "=", FilterValue = permissionId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                if (!string.IsNullOrEmpty(organizationId))
                    filters.Add(new AppFilter { FieldName = "ORGANIZATION_ID", Operator = "=", FilterValue = organizationId });

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(object), _connectionName, "ROLE_PERMISSION");

                var results = await repo.GetAsync(filters);

                foreach (var result in results ?? Enumerable.Empty<object>())
                {
                    if (result is IDictionary<string, object> entity)
                    {
                        entity["ACTIVE_IND"] = "N";
                        entity["ROW_CHANGED_DATE"] = DateTime.UtcNow;
                        await repo.UpdateAsync(entity, organizationId ?? "SYSTEM");
                    }
                    else
                    {
                        var activeIndProp = result.GetType().GetProperty("ACTIVE_IND");
                        var changedDateProp = result.GetType().GetProperty("ROW_CHANGED_DATE");
                        if (activeIndProp != null) activeIndProp.SetValue(result, "N");
                        if (changedDateProp != null) changedDateProp.SetValue(result, DateTime.UtcNow);
                        await repo.UpdateAsync(result, organizationId ?? "SYSTEM");
                    }
                }

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
