using System;
using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.Client.App.Services.DataManagement
{
    internal partial class DataManagementService
    {
        #region Setup & Configuration

        public async Task<object> InitializeDatabaseAsync(object setupOptions, CancellationToken cancellationToken = default)
        {
            if (setupOptions == null) throw new ArgumentNullException(nameof(setupOptions));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<object, object>("/api/ppdm39setup/initialize", setupOptions, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> GetDatabaseStatusAsync(CancellationToken cancellationToken = default)
        {
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<object>("/api/ppdm39setup/status", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> RunMigrationsAsync(CancellationToken cancellationToken = default)
        {
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<object, object>("/api/ppdm39setup/migrations", new { }, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> SeedDataAsync(string seedType, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(seedType)) throw new ArgumentException("Seed type is required", nameof(seedType));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<object, object>($"/api/ppdm39setup/seed/{Uri.EscapeDataString(seedType)}", new { }, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion

        #region Demo Database

        public async Task<object> CreateDemoDatabaseAsync(object options, CancellationToken cancellationToken = default)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<object, object>("/api/ppdm39demo/create", options, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> CleanupDemoDatabaseAsync(string demoId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(demoId)) throw new ArgumentException("Demo ID is required", nameof(demoId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await DeleteAsync<object>($"/api/ppdm39demo/{Uri.EscapeDataString(demoId)}", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> GetDemoDatabaseStatusAsync(string demoId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(demoId)) throw new ArgumentException("Demo ID is required", nameof(demoId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<object>($"/api/ppdm39demo/{Uri.EscapeDataString(demoId)}/status", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}

