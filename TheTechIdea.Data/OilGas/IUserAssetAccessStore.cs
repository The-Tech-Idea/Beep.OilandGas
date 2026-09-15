namespace TheTechIdea.Data.OilGas;

public sealed record AssetDatabaseScope(string ConnectionName, string Fingerprint);

public interface IUserAssetAccessStore
{
    Task<List<AppUserAssetAccess>> ReadAsync(string userId, string databaseScope, string? organizationId = null);
    Task<bool> GrantAsync(string userId, string databaseScope, string assetType, string assetId,
        string accessLevel, bool inherit, string? organizationId = null);
    Task<bool> RevokeAsync(string userId, string databaseScope, string assetType, string assetId);
}
