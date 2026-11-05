using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Asset hierarchy service interface
    /// </summary>
    public interface IAssetHierarchyService
    {
        /// <summary>
        /// Gets the asset hierarchy for an organization
        /// </summary>
        Task<AssetHierarchyNode?> GetAssetHierarchyAsync(string organizationId, string? rootAssetId = null, string? rootAssetType = null, string? rootFacilityType = null);

        /// <summary>
        /// Gets the asset hierarchy filtered by user access
        /// </summary>
        Task<AssetHierarchyNode?> GetAssetHierarchyForUserAsync(string userId, string? organizationId = null, string? rootAssetId = null, string? rootAssetType = null, string? rootFacilityType = null);

        /// <summary>
        /// Gets child assets of a given asset
        /// </summary>
        Task<List<AssetHierarchyNode>> GetAssetChildrenAsync(string assetId, string assetType, string? organizationId = null, string? facilityType = null);

        /// <summary>
        /// Gets the visible stored path to a given asset
        /// </summary>
        Task<List<AssetHierarchyNode>> GetAssetPathAsync(string assetId, string assetType, string? organizationId = null, string? facilityType = null);

        /// <summary>
        /// Validates if a user has access to an asset path
        /// </summary>
        Task<bool> ValidateAccessAsync(string userId, List<AssetHierarchyNode> assetPath);

        /// <summary>
        /// Gets the hierarchy configuration for an organization
        /// </summary>
        Task<List<HierarchyConfig>> GetHierarchyConfigAsync(string organizationId);

        /// <summary>
        /// Updates the hierarchy configuration for an organization
        /// </summary>
        Task<bool> UpdateHierarchyConfigAsync(string organizationId, List<HierarchyConfig> config);
    }

}
