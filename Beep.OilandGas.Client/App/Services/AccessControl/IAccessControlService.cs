using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.Client.App.Services.AccessControl
{
    /// <summary>
    /// Service interface for Access Control operations
    /// </summary>
    public interface IAccessControlService
    {
        Task<bool> CheckAssetAccessAsync(string userId, string assetId, string assetType, CancellationToken cancellationToken = default);
        Task<List<object>> GetUserAccessibleAssetsAsync(string userId, string? assetType = null, CancellationToken cancellationToken = default);
        Task<List<string>> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default);
        Task<bool> HasPermissionAsync(string userId, string permission, CancellationToken cancellationToken = default);
        Task<object> GetUserProfileAsync(string userId, CancellationToken cancellationToken = default);
    }
}

