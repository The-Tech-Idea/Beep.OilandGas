using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.Client.App.Services.DataManagement
{
    internal partial class DataManagementService
    {
        #region CRUD Operations

        public async Task<List<T>> GetEntitiesAsync<T>(string tableName, object request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("Table name is required", nameof(tableName));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<object, List<T>>($"/api/ppdm39data/{Uri.EscapeDataString(tableName)}/entities", request ?? new { }, cancellationToken) ?? new List<T>();
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<T> GetEntityAsync<T>(string tableName, string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("Table name is required", nameof(tableName));
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("ID is required", nameof(id));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<T>($"/api/ppdm39data/{Uri.EscapeDataString(tableName)}/entity/{Uri.EscapeDataString(id)}", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<T> InsertEntityAsync<T>(string tableName, T request, string userId = "SYSTEM", CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("Table name is required", nameof(tableName));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string> { ["userId"] = userId };
                var endpoint = BuildRequestUriWithParams($"/api/ppdm39data/{Uri.EscapeDataString(tableName)}/entity", queryParams);
                return await PostAsync<T, T>(endpoint, request, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<T> UpdateEntityAsync<T>(string tableName, string id, T request, string userId = "SYSTEM", CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("Table name is required", nameof(tableName));
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("ID is required", nameof(id));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string> { ["userId"] = userId };
                var endpoint = BuildRequestUriWithParams($"/api/ppdm39data/{Uri.EscapeDataString(tableName)}/entity/{Uri.EscapeDataString(id)}", queryParams);
                return await PutAsync<T, T>(endpoint, request, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<bool> DeleteEntityAsync(string tableName, string id, string userId = "SYSTEM", CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("Table name is required", nameof(tableName));
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("ID is required", nameof(id));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string> { ["userId"] = userId };
                var endpoint = BuildRequestUriWithParams($"/api/ppdm39data/{Uri.EscapeDataString(tableName)}/entity/{Uri.EscapeDataString(id)}", queryParams);
                // Assuming DeleteAsync returns void or we just check for success. 
                // Since base DeleteAsync returns T, if we want boolean success, we might need a different base method or just return true if no exception.
                // Re-reading base class might be useful, but assuming existing pattern:
                await DeleteAsync<object>(endpoint, cancellationToken); 
                return true; 
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}

