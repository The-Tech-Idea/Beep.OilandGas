using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.Client.App.Services.DataManagement
{
    internal partial class DataManagementService
    {
        #region Versioning

        public async Task<object> CreateVersionAsync(string tableName, object entity, string userId, string? versionLabel = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("Table name is required", nameof(tableName));
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("User ID is required", nameof(userId));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string> { ["userId"] = userId };
                if (!string.IsNullOrEmpty(versionLabel)) queryParams["versionLabel"] = versionLabel;
                var endpoint = BuildRequestUriWithParams($"/api/ppdm39versioning/{Uri.EscapeDataString(tableName)}/version", queryParams);
                return await PostAsync<object, object>(endpoint, entity, null, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<object>> GetVersionsAsync(string tableName, object entityId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("Table name is required", nameof(tableName));
            if (entityId == null) throw new ArgumentNullException(nameof(entityId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<object>>($"/api/ppdm39versioning/{Uri.EscapeDataString(tableName)}/entity/{Uri.EscapeDataString(entityId.ToString()!)}/versions", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> GetVersionAsync(string tableName, object entityId, int versionNumber, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("Table name is required", nameof(tableName));
            if (entityId == null) throw new ArgumentNullException(nameof(entityId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<object>($"/api/ppdm39versioning/{Uri.EscapeDataString(tableName)}/entity/{Uri.EscapeDataString(entityId.ToString()!)}/version/{versionNumber}", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> CompareVersionsAsync(string tableName, object entityId, int version1, int version2, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("Table name is required", nameof(tableName));
            if (entityId == null) throw new ArgumentNullException(nameof(entityId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<object>($"/api/ppdm39versioning/{Uri.EscapeDataString(tableName)}/entity/{Uri.EscapeDataString(entityId.ToString()!)}/compare/{version1}/{version2}", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> RollbackToVersionAsync(string tableName, object entityId, int versionNumber, string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("Table name is required", nameof(tableName));
            if (entityId == null) throw new ArgumentNullException(nameof(entityId));
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("User ID is required", nameof(userId));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string> { ["userId"] = userId };
                var endpoint = BuildRequestUriWithParams($"/api/ppdm39versioning/{Uri.EscapeDataString(tableName)}/entity/{Uri.EscapeDataString(entityId.ToString()!)}/rollback/{versionNumber}", queryParams);
                return await PostAsync<object, object>(endpoint, new { }, null, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}

