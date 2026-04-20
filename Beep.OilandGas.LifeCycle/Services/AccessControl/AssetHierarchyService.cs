using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.LifeCycle.Services.AccessControl
{
    /// <summary>
    /// Service for managing asset hierarchies
    /// </summary>
    public class AssetHierarchyService : IAssetHierarchyService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly PPDMMappingService _mappingService;
        private readonly IAccessControlService _accessControlService;
        private readonly string _connectionName;

        public AssetHierarchyService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            PPDMMappingService mappingService,
            IAccessControlService accessControlService,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _mappingService = mappingService ?? throw new ArgumentNullException(nameof(mappingService));
            _accessControlService = accessControlService ?? throw new ArgumentNullException(nameof(accessControlService));
            _connectionName = connectionName;
        }

        public async Task<AssetHierarchyNode?> GetAssetHierarchyAsync(string organizationId, string? rootAssetId = null, string? rootAssetType = null)
        {
            try
            {
                // Get hierarchy configuration for organization
                var config = await GetHierarchyConfigAsync(organizationId);

                if (config == null || !config.Any())
                {
                    // Use default PPDM hierarchy: FIELD -> POOL -> FACILITY -> WELL
                    return await BuildDefaultHierarchyAsync(organizationId, rootAssetId, rootAssetType);
                }

                // Build hierarchy based on configuration
                return await BuildConfiguredHierarchyAsync(organizationId, config, rootAssetId, rootAssetType);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<AssetHierarchyNode?> GetAssetHierarchyForUserAsync(string userId, string? organizationId = null, string? rootAssetId = null, string? rootAssetType = null)
        {
            var hierarchy = await GetAssetHierarchyAsync(organizationId ?? string.Empty, rootAssetId, rootAssetType);
            if (hierarchy == null) return null;

            // Filter hierarchy based on user access
            return await FilterHierarchyByUserAccessAsync(hierarchy, userId);
        }

        public async Task<List<AssetHierarchyNode>> GetAssetChildrenAsync(string assetId, string assetType, string? organizationId = null)
        {
            var children = new List<AssetHierarchyNode>();

            try
            {
                switch (assetType.ToUpperInvariant())
                {
                    case "FIELD":
                    {
                        // Query POOLs that belong to this field
                        var poolMeta = await _metadata.GetTableMetadataAsync("POOL");
                        if (poolMeta != null)
                        {
                            var poolRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, typeof(POOL), _connectionName, "POOL");
                            var poolFilters = new List<AppFilter>
                            {
                                new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = assetId },
                                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                            };
                            var pools = await poolRepo.GetAsync(poolFilters);
                            foreach (var pool in pools?.OfType<POOL>() ?? Enumerable.Empty<POOL>())
                            {
                                children.Add(new AssetHierarchyNode
                                {
                                    AssetId = pool.POOL_ID ?? string.Empty,
                                    AssetType = "POOL",
                                    AssetName = pool.POOL_NAME,
                                    ParentAssetId = assetId,
                                    ParentAssetType = "FIELD"
                                });
                            }
                        }
                        break;
                    }
                    case "POOL":
                    {
                        // WELLs don't have a POOL_ID FK in PPDM39; return wells linked via field (best effort using POOL.FIELD_ID as bridge)
                        // PPDM connects wells and pools through stratigraphic units, not direct FKs.
                        // Return empty — callers should query via the FIELD level instead.
                        break;
                    }
                    case "FACILITY":
                    {
                        // WELLs don't have a FACILITY_ID FK in standard PPDM39.
                        // WELL_FACILITY junction table would be required for a proper traversal.
                        // Return empty — callers should query WELL_FACILITY if available.
                        break;
                    }
                    default:
                        break;
                }
            }
            catch (Exception)
            {
                // Return whatever we collected so far
            }

            return children;
        }

        public async Task<List<AssetHierarchyNode>> GetAssetPathAsync(string assetId, string assetType, string? organizationId = null)
        {
            var path = new List<AssetHierarchyNode>();

            try
            {
                // Add current node first
                string? currentName = null;
                string? parentId = null;
                string? parentType = null;

                switch (assetType.ToUpperInvariant())
                {
                    case "WELL":
                    {
                        var wellMeta = await _metadata.GetTableMetadataAsync("WELL");
                        if (wellMeta != null)
                        {
                            var wellRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, typeof(WELL), _connectionName, "WELL");
                            var well = await wellRepo.GetByIdAsync(_defaults.FormatIdForTable("WELL", assetId));
                            if (well is WELL w)
                            {
                                currentName = w.WELL_NAME;
                                parentId = w.ASSIGNED_FIELD;
                                parentType = string.IsNullOrEmpty(parentId) ? null : "FIELD";
                            }
                        }
                        break;
                    }
                    case "POOL":
                    {
                        var poolMeta = await _metadata.GetTableMetadataAsync("POOL");
                        if (poolMeta != null)
                        {
                            var poolRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, typeof(POOL), _connectionName, "POOL");
                            var pool = await poolRepo.GetByIdAsync(_defaults.FormatIdForTable("POOL", assetId));
                            if (pool is POOL p)
                            {
                                currentName = p.POOL_NAME;
                                parentId = p.FIELD_ID;
                                parentType = string.IsNullOrEmpty(parentId) ? null : "FIELD";
                            }
                        }
                        break;
                    }
                    case "FACILITY":
                    {
                        var facMeta = await _metadata.GetTableMetadataAsync("FACILITY");
                        if (facMeta != null)
                        {
                            var facRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, typeof(FACILITY), _connectionName, "FACILITY");
                            var facility = await facRepo.GetByIdAsync(_defaults.FormatIdForTable("FACILITY", assetId));
                            if (facility is FACILITY f)
                            {
                                currentName = f.FACILITY_LONG_NAME ?? f.FACILITY_SHORT_NAME;
                                parentId = f.PRIMARY_FIELD_ID;
                                parentType = string.IsNullOrEmpty(parentId) ? null : "FIELD";
                            }
                        }
                        break;
                    }
                    case "FIELD":
                    {
                        var fieldMeta = await _metadata.GetTableMetadataAsync("FIELD");
                        if (fieldMeta != null)
                        {
                            var fieldRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, typeof(FIELD), _connectionName, "FIELD");
                            var field = await fieldRepo.GetByIdAsync(_defaults.FormatIdForTable("FIELD", assetId));
                            if (field is FIELD fld)
                                currentName = fld.FIELD_NAME;
                        }
                        break;
                    }
                }

                path.Insert(0, new AssetHierarchyNode
                {
                    AssetId = assetId,
                    AssetType = assetType,
                    AssetName = currentName,
                    ParentAssetId = parentId,
                    ParentAssetType = parentType
                });

                // Walk up to parent FIELD if applicable
                if (!string.IsNullOrEmpty(parentId) && !string.IsNullOrEmpty(parentType))
                {
                    var parentPath = await GetAssetPathAsync(parentId, parentType, organizationId);
                    path.InsertRange(0, parentPath);
                }
            }
            catch (Exception)
            {
                // Return whatever we built
            }

            return path;
        }

        public async Task<bool> ValidateAccessAsync(string userId, List<AssetHierarchyNode> assetPath)
        {
            // Check access for each node in the path
            foreach (var node in assetPath)
            {
                var accessCheck = await _accessControlService.CheckAssetAccessAsync(userId, node.AssetId, node.AssetType);
                if (!accessCheck.HasAccess)
                {
                    return false;
                }
            }
            return true;
        }

        public async Task<List<HierarchyConfig>> GetHierarchyConfigAsync(string organizationId)
        {
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ORGANIZATION_ID", Operator = "=", FilterValue = organizationId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(object), _connectionName, "ORGANIZATION_HIERARCHY_CONFIG");

            var results = await repo.GetAsync(filters);
            var config = new List<HierarchyConfig>();

            foreach (var result in results ?? Enumerable.Empty<object>())
            {
                config.Add(new HierarchyConfig
                {
                    OrganizationId = GetPropertyValue(result, "ORGANIZATION_ID")?.ToString() ?? string.Empty,
                    HierarchyLevel = Convert.ToInt32(GetPropertyValue(result, "HIERARCHY_LEVEL") ?? 0),
                    LevelName = GetPropertyValue(result, "LEVEL_NAME")?.ToString(),
                    AssetType = GetPropertyValue(result, "ASSET_TYPE")?.ToString(),
                    ParentLevel = GetPropertyValue(result, "PARENT_LEVEL") != null ? Convert.ToInt32(GetPropertyValue(result, "PARENT_LEVEL")) : null,
                    Active = GetPropertyValue(result, "ACTIVE_IND")?.ToString() == "Y"
                });
            }

            return config.OrderBy(c => c.HierarchyLevel).ToList();
        }

        public async Task<bool> UpdateHierarchyConfigAsync(string organizationId, List<HierarchyConfig> config)
        {
            try
            {
                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(object), _connectionName, "ORGANIZATION_HIERARCHY_CONFIG");

                foreach (var item in config ?? Enumerable.Empty<HierarchyConfig>())
                {
                    // Try to find existing record
                    var filters = new List<AppFilter>
                    {
                        new AppFilter { FieldName = "ORGANIZATION_ID", Operator = "=", FilterValue = organizationId },
                        new AppFilter { FieldName = "HIERARCHY_LEVEL", Operator = "=", FilterValue = item.HierarchyLevel.ToString() }
                    };
                    var existing = await repo.GetAsync(filters);
                    var existingRecord = existing?.FirstOrDefault();

                    if (existingRecord != null)
                    {
                        // Update existing
                        if (existingRecord is IDictionary<string, object> dict)
                        {
                            dict["LEVEL_NAME"] = item.LevelName ?? string.Empty;
                            dict["ASSET_TYPE"] = item.AssetType ?? string.Empty;
                            dict["PARENT_LEVEL"] = item.ParentLevel.HasValue ? (object)item.ParentLevel.Value : DBNull.Value;
                            dict["ACTIVE_IND"] = item.Active ? "Y" : "N";
                            dict["ROW_CHANGED_DATE"] = DateTime.UtcNow;
                            await repo.UpdateAsync(existingRecord, organizationId);
                        }
                        else
                        {
                            var levelNameProp = existingRecord.GetType().GetProperty("LEVEL_NAME");
                            var assetTypeProp = existingRecord.GetType().GetProperty("ASSET_TYPE");
                            var activeIndProp = existingRecord.GetType().GetProperty("ACTIVE_IND");
                            if (levelNameProp != null) levelNameProp.SetValue(existingRecord, item.LevelName);
                            if (assetTypeProp != null) assetTypeProp.SetValue(existingRecord, item.AssetType);
                            if (activeIndProp != null) activeIndProp.SetValue(existingRecord, item.Active ? "Y" : "N");
                            await repo.UpdateAsync(existingRecord, organizationId);
                        }
                    }
                    else
                    {
                        // Insert new record
                        var insertObj = new
                        {
                            ORGANIZATION_ID = (object)organizationId,
                            HIERARCHY_LEVEL = (object)item.HierarchyLevel,
                            LEVEL_NAME = (object)(item.LevelName ?? string.Empty),
                            ASSET_TYPE = (object)(item.AssetType ?? string.Empty),
                            PARENT_LEVEL = item.ParentLevel.HasValue ? (object)item.ParentLevel.Value : (object)DBNull.Value,
                            ACTIVE_IND = (object)(item.Active ? "Y" : "N"),
                            ROW_CREATED_BY = (object)organizationId,
                            ROW_CREATED_DATE = (object)DateTime.UtcNow,
                            ROW_CHANGED_BY = (object)organizationId,
                            ROW_CHANGED_DATE = (object)DateTime.UtcNow
                        };
                        await repo.InsertAsync(insertObj, organizationId);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<AssetHierarchyNode?> BuildDefaultHierarchyAsync(string organizationId, string? rootAssetId, string? rootAssetType)
        {
            try
            {
                var fieldMeta = await _metadata.GetTableMetadataAsync("FIELD");
                if (fieldMeta == null) return null;

                var fieldRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, typeof(FIELD), _connectionName, "FIELD");

                if (!string.IsNullOrEmpty(rootAssetId) && !string.IsNullOrEmpty(rootAssetType))
                {
                    // Start from a specific root asset
                    var rootNode = new AssetHierarchyNode
                    {
                        AssetId = rootAssetId,
                        AssetType = rootAssetType.ToUpperInvariant()
                    };
                    rootNode.Children = await GetAssetChildrenAsync(rootAssetId, rootAssetType.ToUpperInvariant(), organizationId);
                    return rootNode;
                }

                // Build from all fields — return a synthetic root containing all fields as children
                var fieldFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };
                var fields = await fieldRepo.GetAsync(fieldFilters);

                var rootHierarchy = new AssetHierarchyNode
                {
                    AssetId = organizationId,
                    AssetType = "ORGANIZATION",
                    AssetName = organizationId
                };

                foreach (var field in fields?.OfType<FIELD>() ?? Enumerable.Empty<FIELD>())
                {
                    var fieldId = field.FIELD_ID ?? string.Empty;
                    if (string.IsNullOrEmpty(fieldId)) continue;

                    var fieldNode = new AssetHierarchyNode
                    {
                        AssetId = fieldId,
                        AssetType = "FIELD",
                        AssetName = field.FIELD_NAME,
                        ParentAssetId = organizationId,
                        ParentAssetType = "ORGANIZATION"
                    };
                    fieldNode.Children = await GetAssetChildrenAsync(fieldId, "FIELD", organizationId);
                    rootHierarchy.Children.Add(fieldNode);
                }

                return rootHierarchy;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<AssetHierarchyNode?> BuildConfiguredHierarchyAsync(string organizationId, List<HierarchyConfig> config, string? rootAssetId, string? rootAssetType)
        {
            try
            {
                // Use config to determine hierarchy root level
                var rootLevel = config.OrderBy(c => c.HierarchyLevel).FirstOrDefault();
                if (rootLevel == null) return null;

                var rootType = rootLevel.AssetType ?? "FIELD";

                if (!string.IsNullOrEmpty(rootAssetId) && !string.IsNullOrEmpty(rootAssetType))
                {
                    var rootNode = new AssetHierarchyNode
                    {
                        AssetId = rootAssetId,
                        AssetType = rootAssetType.ToUpperInvariant(),
                        AssetName = rootAssetId
                    };
                    rootNode.Children = await GetAssetChildrenAsync(rootAssetId, rootAssetType.ToUpperInvariant(), organizationId);
                    return rootNode;
                }

                // Fall back to default hierarchy using configured root type
                return await BuildDefaultHierarchyAsync(organizationId, rootAssetId, rootType);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<AssetHierarchyNode?> FilterHierarchyByUserAccessAsync(AssetHierarchyNode node, string userId)
        {
            // Check if user has access to this node
            var accessCheck = await _accessControlService.CheckAssetAccessAsync(userId, node.AssetId, node.AssetType);
            node.UserHasAccess = accessCheck.HasAccess;

            // Recursively filter children
            var filteredChildren = new List<AssetHierarchyNode>();
            foreach (var child in node.Children)
            {
                var filteredChild = await FilterHierarchyByUserAccessAsync(child, userId);
                if (filteredChild != null && filteredChild.UserHasAccess)
                {
                    filteredChildren.Add(filteredChild);
                }
            }

            node.Children = filteredChildren;
            return node.UserHasAccess || filteredChildren.Any() ? node : null;
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
