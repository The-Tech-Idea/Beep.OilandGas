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
            catch (Exception ex)
            {
                // Log error
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

            // Determine child type based on asset type
            string? childType = assetType switch
            {
                "FIELD" => "POOL",
                "POOL" => "FACILITY",
                "FACILITY" => "WELL",
                _ => null
            };

            if (string.IsNullOrEmpty(childType)) return children;

            // Query child assets based on parent relationship
            // This is simplified - actual implementation would query appropriate tables
            return children;
        }

        public async Task<List<AssetHierarchyNode>> GetAssetPathAsync(string assetId, string assetType, string? organizationId = null)
        {
            var path = new List<AssetHierarchyNode>();

            // Traverse up the hierarchy to build path
            // This would query parent relationships
            // Simplified for now
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
                // Update hierarchy configuration
                // This would update/insert records in ORGANIZATION_HIERARCHY_CONFIG
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<AssetHierarchyNode?> BuildDefaultHierarchyAsync(string organizationId, string? rootAssetId, string? rootAssetType)
        {
            // Build default PPDM hierarchy starting from fields
            if (string.IsNullOrEmpty(rootAssetId))
            {
                // Start from fields
                var fieldRepo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(FIELD), _connectionName, "FIELD");

                var fields = await fieldRepo.GetAsync(new List<AppFilter>());
                // Build tree structure
                return null; // Simplified
            }

            return null;
        }

        private async Task<AssetHierarchyNode?> BuildConfiguredHierarchyAsync(string organizationId, List<HierarchyConfig> config, string? rootAssetId, string? rootAssetType)
        {
            // Build hierarchy based on configuration
            // This would traverse the config and build the tree
            return null; // Simplified
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
