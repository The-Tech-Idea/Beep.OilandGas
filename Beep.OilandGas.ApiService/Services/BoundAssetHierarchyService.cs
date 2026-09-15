using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

public sealed class BoundAssetHierarchyService(
    Func<Task<AssetDatabaseScope>> resolveScope,
    Func<AssetDatabaseScope, IAssetHierarchyService> createService) : IAssetHierarchyService
{
    private async Task<T> ExecuteAsync<T>(Func<IAssetHierarchyService, Task<T>> action)
    {
        var scope = await resolveScope();
        if (scope is null || string.IsNullOrWhiteSpace(scope.ConnectionName) || string.IsNullOrWhiteSpace(scope.Fingerprint))
            throw new InvalidOperationException("A reviewed module database scope is required for the hierarchy.");
        var result = await action(createService(scope));
        if (scope != await resolveScope())
            throw new InvalidOperationException("The hierarchy database binding changed during this operation.");
        return result;
    }

    public Task<AssetHierarchyNode?> GetAssetHierarchyAsync(string organizationId, string? rootAssetId = null, string? rootAssetType = null) =>
        ExecuteAsync(service => service.GetAssetHierarchyAsync(organizationId, rootAssetId, rootAssetType));

    public Task<AssetHierarchyNode?> GetAssetHierarchyForUserAsync(string userId, string? organizationId = null, string? rootAssetId = null, string? rootAssetType = null) =>
        ExecuteAsync(service => service.GetAssetHierarchyForUserAsync(userId, organizationId, rootAssetId, rootAssetType));

    public Task<List<AssetHierarchyNode>> GetAssetChildrenAsync(string assetId, string assetType, string? organizationId = null) =>
        ExecuteAsync(service => service.GetAssetChildrenAsync(assetId, assetType, organizationId));

    public Task<List<AssetHierarchyNode>> GetAssetPathAsync(string assetId, string assetType, string? organizationId = null) =>
        ExecuteAsync(service => service.GetAssetPathAsync(assetId, assetType, organizationId));

    public Task<bool> ValidateAccessAsync(string userId, List<AssetHierarchyNode> assetPath) =>
        ExecuteAsync(service => service.ValidateAccessAsync(userId, assetPath));

    public Task<List<HierarchyConfig>> GetHierarchyConfigAsync(string organizationId) =>
        ExecuteAsync(service => service.GetHierarchyConfigAsync(organizationId));

    public Task<bool> UpdateHierarchyConfigAsync(string organizationId, List<HierarchyConfig> config) =>
        ExecuteAsync(service => service.UpdateHierarchyConfigAsync(organizationId, config));
}
