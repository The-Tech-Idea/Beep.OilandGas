using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.DataManagement;

namespace Beep.OilandGas.Client.App.Services.DataManagement
{
    internal partial class DataManagementService
    {
        #region Access Audit

        public async Task<AuditRecord> RecordAccessAsync(AuditRecord accessEvent, CancellationToken cancellationToken = default)
        {
            if (accessEvent == null) throw new ArgumentNullException(nameof(accessEvent));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<AuditRecord, AuditRecord>("/api/ppdm39audit/access", accessEvent, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<AccessHistory>> GetAccessHistoryAsync(string tableName, object entityId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("Table name is required", nameof(tableName));
            if (entityId == null) throw new ArgumentNullException(nameof(entityId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<AccessHistory>>($"/api/ppdm39audit/{Uri.EscapeDataString(tableName)}/entity/{Uri.EscapeDataString(entityId.ToString()!)}/history", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<AccessHistory>> GetUserAccessHistoryAsync(string userId, int? limit = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("User ID is required", nameof(userId));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string>();
                if (limit.HasValue) queryParams["limit"] = limit.Value.ToString();
                var endpoint = BuildRequestUriWithParams($"/api/ppdm39audit/user/{Uri.EscapeDataString(userId)}/history", queryParams);
                return await GetAsync<List<AccessHistory>>(endpoint, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> GetAccessStatisticsAsync(string tableName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("Table name is required", nameof(tableName));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<object>($"/api/ppdm39audit/{Uri.EscapeDataString(tableName)}/statistics", cancellationToken);
            // Keeping as object for now as AccessStatistics DTO might be complex or variable
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}

