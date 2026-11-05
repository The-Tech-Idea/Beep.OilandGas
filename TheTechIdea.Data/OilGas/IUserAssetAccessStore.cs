namespace TheTechIdea.Data.OilGas;

public sealed record AssetDatabaseScope(string ConnectionName, string Fingerprint);

public interface IUserAssetAccessStore
{
    // The API implementation requires validated company authority and the current reviewed database fingerprint.
    // OrganizationId is an assertion against that company; null means the selected company, never global access.
    Task<List<AppUserAssetAccess>> ReadAsync(string userId, string databaseScope, string? organizationId = null);
    Task<bool> GrantAsync(string userId, string databaseScope, string assetType, string assetId,
        string accessLevel, bool inherit, string? organizationId = null, string? facilityType = null);
    Task<bool> RevokeAsync(string userId, string databaseScope, string assetType, string assetId, string? facilityType = null);
    Task<AssetGrantReviewPage> ReviewAsync(string userId, string databaseScope, int offset = 0, int limit = 50, CancellationToken token = default);
    Task<AssetGrantRetirement> RetireAsync(string grantId, string databaseScope, RetireAssetGrantRequest request, CancellationToken token = default);
    Task<AssetGrantRetirementPage> ReadRetirementsAsync(string grantId, string databaseScope, int offset = 0, int limit = 50, CancellationToken token = default);
}
