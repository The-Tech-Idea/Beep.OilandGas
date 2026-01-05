using System;
using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.Client.App.Services.DataManagement
{
    internal partial class DataManagementService
    {
        #region Field Mapping

        public async Task<object> GetFieldMappingsAsync(string sourceTable, string targetTable, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(sourceTable)) throw new ArgumentException("Source table is required", nameof(sourceTable));
            if (string.IsNullOrEmpty(targetTable)) throw new ArgumentException("Target table is required", nameof(targetTable));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<object>($"/api/ppdm39mapping/{Uri.EscapeDataString(sourceTable)}/{Uri.EscapeDataString(targetTable)}", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> SaveFieldMappingAsync(object mapping, string userId = "SYSTEM", CancellationToken cancellationToken = default)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new System.Collections.Generic.Dictionary<string, string> { ["userId"] = userId };
                var endpoint = BuildRequestUriWithParams("/api/ppdm39mapping", queryParams);
                return await PostAsync<object, object>(endpoint, mapping, null, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> ApplyFieldMappingAsync(string mappingId, object sourceEntity, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(mappingId)) throw new ArgumentException("Mapping ID is required", nameof(mappingId));
            if (sourceEntity == null) throw new ArgumentNullException(nameof(sourceEntity));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<object, object>($"/api/ppdm39mapping/{Uri.EscapeDataString(mappingId)}/apply", sourceEntity, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}

