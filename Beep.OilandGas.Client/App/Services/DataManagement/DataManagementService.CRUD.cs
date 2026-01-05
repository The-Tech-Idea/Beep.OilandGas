using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.Client.App.Services.DataManagement
{
    internal partial class DataManagementService
    {
        #region CRUD Operations

        public async Task<object> GetEntitiesAsync(string tableName, object request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("Table name is required", nameof(tableName));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<object, object>($"/api/ppdm39data/{Uri.EscapeDataString(tableName)}/entities", request ?? new { }, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> GetEntityAsync(string tableName, string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("Table name is required", nameof(tableName));
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("ID is required", nameof(id));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<object>($"/api/ppdm39data/{Uri.EscapeDataString(tableName)}/entity/{Uri.EscapeDataString(id)}", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> InsertEntityAsync(string tableName, object request, string userId = "SYSTEM", CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("Table name is required", nameof(tableName));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string> { ["userId"] = userId };
                var endpoint = BuildRequestUriWithParams($"/api/ppdm39data/{Uri.EscapeDataString(tableName)}/entity", queryParams);
                return await PostAsync<object, object>(endpoint, request, null, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> UpdateEntityAsync(string tableName, string id, object request, string userId = "SYSTEM", CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("Table name is required", nameof(tableName));
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("ID is required", nameof(id));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string> { ["userId"] = userId };
                var endpoint = BuildRequestUriWithParams($"/api/ppdm39data/{Uri.EscapeDataString(tableName)}/entity/{Uri.EscapeDataString(id)}", queryParams);
                return await PutAsync<object, object>(endpoint, request, null, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> DeleteEntityAsync(string tableName, string id, string userId = "SYSTEM", CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("Table name is required", nameof(tableName));
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("ID is required", nameof(id));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string> { ["userId"] = userId };
                var endpoint = BuildRequestUriWithParams($"/api/ppdm39data/{Uri.EscapeDataString(tableName)}/entity/{Uri.EscapeDataString(id)}", queryParams);
                return await DeleteAsync<object>(endpoint, null, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}

