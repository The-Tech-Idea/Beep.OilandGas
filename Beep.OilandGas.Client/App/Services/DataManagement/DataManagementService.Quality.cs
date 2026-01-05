using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.Client.App.Services.DataManagement
{
    internal partial class DataManagementService
    {
        #region Data Quality

        public async Task<object> CalculateQualityScoreAsync(string tableName, object entity, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("Table name is required", nameof(tableName));
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<object, object>($"/api/ppdm39quality/{Uri.EscapeDataString(tableName)}/score", entity, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> CalculateTableQualityMetricsAsync(string tableName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("Table name is required", nameof(tableName));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<object>($"/api/ppdm39quality/{Uri.EscapeDataString(tableName)}/metrics", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<object>> FindQualityIssuesAsync(string tableName, List<string>? fieldNames = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("Table name is required", nameof(tableName));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var request = new { FieldNames = fieldNames };
                return await PostAsync<object, List<object>>($"/api/ppdm39quality/{Uri.EscapeDataString(tableName)}/issues", request, null, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> GetQualityDashboardAsync(string? tableName = null, CancellationToken cancellationToken = default)
        {
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var endpoint = string.IsNullOrEmpty(tableName)
                    ? "/api/ppdm39quality/dashboard"
                    : $"/api/ppdm39quality/dashboard/{Uri.EscapeDataString(tableName)}";
                return await GetAsync<object>(endpoint, null, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<object>> GetQualityTrendsAsync(string tableName, int days = 30, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("Table name is required", nameof(tableName));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string> { ["days"] = days.ToString() };
                var endpoint = BuildRequestUriWithParams($"/api/ppdm39quality/{Uri.EscapeDataString(tableName)}/trends", queryParams);
                return await GetAsync<List<object>>(endpoint, null, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}

