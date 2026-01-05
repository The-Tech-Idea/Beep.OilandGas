using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.Client.App.Services.DataManagement
{
    internal partial class DataManagementService
    {
        #region Access Audit

        public async Task<object> RecordAccessAsync(object accessEvent, CancellationToken cancellationToken = default)
        {
            if (accessEvent == null) throw new ArgumentNullException(nameof(accessEvent));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<object, object>("/api/ppdm39audit/access", accessEvent, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<object>> GetAccessHistoryAsync(string tableName, object entityId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("Table name is required", nameof(tableName));
            if (entityId == null) throw new ArgumentNullException(nameof(entityId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<object>>($"/api/ppdm39audit/{Uri.EscapeDataString(tableName)}/entity/{Uri.EscapeDataString(entityId.ToString()!)}/history", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<object>> GetUserAccessHistoryAsync(string userId, int? limit = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("User ID is required", nameof(userId));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string>();
                if (limit.HasValue) queryParams["limit"] = limit.Value.ToString();
                var endpoint = BuildRequestUriWithParams($"/api/ppdm39audit/user/{Uri.EscapeDataString(userId)}/history", queryParams);
                return await GetAsync<List<object>>(endpoint, null, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> GetAccessStatisticsAsync(string tableName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("Table name is required", nameof(tableName));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<object>($"/api/ppdm39audit/{Uri.EscapeDataString(tableName)}/statistics", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}

