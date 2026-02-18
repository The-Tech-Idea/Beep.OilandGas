using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Client.App.Services.DataManagement
{
    /// <summary>
    /// Defaults Management operations for PPDM39 Data Management
    /// Provides per-data-source default value management
    /// </summary>
    internal partial class DataManagementService
    {
        #region Defaults Management (Per Data Source)

        public async Task<string?> GetDefaultValueAsync(string key, string databaseId, string? userId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            if (string.IsNullOrWhiteSpace(databaseId))
                throw new ArgumentException("Database ID cannot be null or empty", nameof(databaseId));

            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string>
                {
                    { "key", key },
                    { "databaseId", databaseId }
                };
                if (!string.IsNullOrEmpty(userId))
                    queryParams["userId"] = userId;

                var endpoint = BuildRequestUriWithParams("/api/datamanagement/defaults/value", queryParams);
                return await GetAsync<string?>(endpoint, cancellationToken);
            }

            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task SetDefaultValueAsync(string key, string value, string databaseId, string? userId = null, string category = "System", string valueType = "String", string? description = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value cannot be null or empty", nameof(value));
            if (string.IsNullOrWhiteSpace(databaseId))
                throw new ArgumentException("Database ID cannot be null or empty", nameof(databaseId));

            if (AccessMode == ServiceAccessMode.Remote)
            {
                var request = new
                {
                    key,
                    value,
                    databaseId,
                    userId,
                    category,
                    valueType,
                    description
                };

                await PostAsync<object, object>("/api/datamanagement/defaults/value", request, cancellationToken);
            }
            else
            {
                throw new InvalidOperationException("Local mode not yet implemented");
            }
        }

        public async Task<Dictionary<string, string>> GetDefaultsByCategoryAsync(string category, string databaseId, string? userId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Category cannot be null or empty", nameof(category));
            if (string.IsNullOrWhiteSpace(databaseId))
                throw new ArgumentException("Database ID cannot be null or empty", nameof(databaseId));

            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string>
                {
                    { "category", category },
                    { "databaseId", databaseId }
                };
                if (!string.IsNullOrEmpty(userId))
                    queryParams["userId"] = userId;

                var endpoint = BuildRequestUriWithParams("/api/datamanagement/defaults/category", queryParams);
                return await GetAsync<Dictionary<string, string>>(endpoint, cancellationToken) 
                    ?? new Dictionary<string, string>();
            }

            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<Dictionary<string, string>> GetDefaultsForDatabaseAsync(string databaseId, string? userId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(databaseId))
                throw new ArgumentException("Database ID cannot be null or empty", nameof(databaseId));

            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string>
                {
                    { "databaseId", databaseId }
                };
                if (!string.IsNullOrEmpty(userId))
                    queryParams["userId"] = userId;

                var endpoint = BuildRequestUriWithParams("/api/datamanagement/defaults/database", queryParams);
                return await GetAsync<Dictionary<string, string>>(endpoint, cancellationToken) 
                    ?? new Dictionary<string, string>();
            }

            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task InitializeSystemDefaultsAsync(string databaseId, string userId = "SYSTEM", CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(databaseId))
                throw new ArgumentException("Database ID cannot be null or empty", nameof(databaseId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            if (AccessMode == ServiceAccessMode.Remote)
            {
                var request = new
                {
                    databaseId,
                    userId
                };

                await PostAsync<object, object>("/api/datamanagement/defaults/initialize", request, cancellationToken);
            }
            else
            {
                throw new InvalidOperationException("Local mode not yet implemented");
            }
        }

        public async Task ResetToSystemDefaultsAsync(string databaseId, string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(databaseId))
                throw new ArgumentException("Database ID cannot be null or empty", nameof(databaseId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            if (AccessMode == ServiceAccessMode.Remote)
            {
                var request = new
                {
                    databaseId,
                    userId
                };

                await PostAsync<object, object>("/api/datamanagement/defaults/reset", request, cancellationToken);
            }
            else
            {
                throw new InvalidOperationException("Local mode not yet implemented");
            }
        }

        public async Task<Dictionary<string, string>> GetStandardDefaultsAsync(CancellationToken cancellationToken = default)
        {
            if (AccessMode == ServiceAccessMode.Remote)
            {
                return await GetAsync<Dictionary<string, string>>("/api/datamanagement/defaults/standard", cancellationToken) 
                    ?? new Dictionary<string, string>();
            }

            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}

