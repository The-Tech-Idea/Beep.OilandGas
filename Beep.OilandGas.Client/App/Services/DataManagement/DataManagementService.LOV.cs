using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.Client.App.Services.DataManagement
{
    internal partial class DataManagementService
    {
        #region List of Values (LOV)

        public async Task<List<object>> GetLOVAsync(string lovType, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(lovType)) throw new ArgumentException("LOV type is required", nameof(lovType));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<object>>($"/api/ppdm39lov/{Uri.EscapeDataString(lovType)}", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<object>> GetLOVsByTypeAsync(string lovType, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(lovType)) throw new ArgumentException("LOV type is required", nameof(lovType));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<object>>($"/api/ppdm39lov/type/{Uri.EscapeDataString(lovType)}", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> GetLOVByCodeAsync(string lovType, string code, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(lovType)) throw new ArgumentException("LOV type is required", nameof(lovType));
            if (string.IsNullOrEmpty(code)) throw new ArgumentException("Code is required", nameof(code));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<object>($"/api/ppdm39lov/{Uri.EscapeDataString(lovType)}/{Uri.EscapeDataString(code)}", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> CreateLOVAsync(object lovEntry, string userId = "SYSTEM", CancellationToken cancellationToken = default)
        {
            if (lovEntry == null) throw new ArgumentNullException(nameof(lovEntry));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string> { ["userId"] = userId };
                var endpoint = BuildRequestUriWithParams("/api/ppdm39lov", queryParams);
                return await PostAsync<object, object>(endpoint, lovEntry, null, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> UpdateLOVAsync(string lovId, object lovEntry, string userId = "SYSTEM", CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(lovId)) throw new ArgumentException("LOV ID is required", nameof(lovId));
            if (lovEntry == null) throw new ArgumentNullException(nameof(lovEntry));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string> { ["userId"] = userId };
                var endpoint = BuildRequestUriWithParams($"/api/ppdm39lov/{Uri.EscapeDataString(lovId)}", queryParams);
                return await PutAsync<object, object>(endpoint, lovEntry, null, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> DeleteLOVAsync(string lovId, string userId = "SYSTEM", CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(lovId)) throw new ArgumentException("LOV ID is required", nameof(lovId));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string> { ["userId"] = userId };
                var endpoint = BuildRequestUriWithParams($"/api/ppdm39lov/{Uri.EscapeDataString(lovId)}", queryParams);
                return await DeleteAsync<object>(endpoint, null, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<object>> GetReferenceTableDataAsync(string tableName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("Table name is required", nameof(tableName));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<object>>($"/api/ppdm39lov/reference/{Uri.EscapeDataString(tableName)}", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}

