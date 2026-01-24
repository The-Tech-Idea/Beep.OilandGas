using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.Client.App.Services.DataManagement
{
    internal partial class DataManagementService
    {
        #region Metadata

        public async Task<object> GetTableMetadataAsync(string tableName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("Table name is required", nameof(tableName));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<object>($"/api/ppdm39metadata/table/{Uri.EscapeDataString(tableName)}", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<object>> GetAllTablesMetadataAsync(CancellationToken cancellationToken = default)
        {
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<object>>("/api/ppdm39metadata/tables", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> GetColumnMetadataAsync(string tableName, string columnName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("Table name is required", nameof(tableName));
            if (string.IsNullOrEmpty(columnName)) throw new ArgumentException("Column name is required", nameof(columnName));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<object>($"/api/ppdm39metadata/table/{Uri.EscapeDataString(tableName)}/column/{Uri.EscapeDataString(columnName)}", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}

