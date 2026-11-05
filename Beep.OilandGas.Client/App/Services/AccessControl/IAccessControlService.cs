using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Company;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Client.App.Services.AccessControl
{
    /// <summary>
    /// Service interface for Access Control operations
    /// </summary>
    public interface IAccessControlService
    {
        Task<bool> CheckAssetAccessAsync(string userId, string assetId, string assetType, CancellationToken cancellationToken = default, string? facilityType = null, string? requiredPermission = null);
        Task<List<AssetAccess>> GetUserAccessibleAssetsAsync(string userId, string? assetType = null, CancellationToken cancellationToken = default, bool includeInherited = true, string? organizationId = null);
        Task<List<string>> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default);
        Task<bool> HasPermissionAsync(string userId, string permission, CancellationToken cancellationToken = default);
        Task<UserProfile> GetUserProfileAsync(string userId, CancellationToken cancellationToken = default);
        Task<Beep.OilandGas.Models.Data.AccessControl.AssetGrantReviewPage> ReviewStoredGrantsAsync(string userId, int offset = 0, int limit = 50, CancellationToken cancellationToken = default);
        Task<Beep.OilandGas.Models.Data.AccessControl.AssetGrantRetirement> RetireStoredGrantAsync(string grantId, Beep.OilandGas.Models.Data.AccessControl.RetireAssetGrantRequest request, CancellationToken cancellationToken = default);
        Task<Beep.OilandGas.Models.Data.AccessControl.AssetGrantRetirementPage> GetGrantRetirementsAsync(string grantId, int offset = 0, int limit = 50, CancellationToken cancellationToken = default);
        Task<Beep.OilandGas.Models.Data.Lease.FieldInterestReview> GetFieldInterestReviewAsync(string fieldId, CancellationToken cancellationToken = default);
    }
}

