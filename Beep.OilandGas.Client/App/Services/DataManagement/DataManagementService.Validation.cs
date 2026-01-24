using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.Client.App.Services.DataManagement
{
    internal partial class DataManagementService
    {
        #region Validation

        public async Task<object> ValidateEntityAsync(string tableName, object entity, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("Table name is required", nameof(tableName));
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<object, object>($"/api/ppdm39validation/{Uri.EscapeDataString(tableName)}/validate", entity, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<object>> ValidateBatchAsync(string tableName, List<object> entities, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("Table name is required", nameof(tableName));
            if (entities == null || entities.Count == 0) throw new ArgumentException("Entities list cannot be empty", nameof(entities));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<List<object>, List<object>>($"/api/ppdm39validation/{Uri.EscapeDataString(tableName)}/validate-batch", entities, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<object>> GetValidationRulesAsync(string tableName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("Table name is required", nameof(tableName));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<object>>($"/api/ppdm39validation/{Uri.EscapeDataString(tableName)}/rules", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> SaveValidationRuleAsync(string tableName, object rule, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("Table name is required", nameof(tableName));
            if (rule == null) throw new ArgumentNullException(nameof(rule));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<object, object>($"/api/ppdm39validation/{Uri.EscapeDataString(tableName)}/rules", rule, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}

