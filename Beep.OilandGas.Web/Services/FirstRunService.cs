using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Blazored.LocalStorage;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Web.Services
{
    public interface IFirstRunService
    {
        Task<bool> IsSetupRequiredAsync();
        Task MarkSetupCompleteAsync(string connectionName);
        Task ClearSetupFlagAsync();
        Task<string?> GetSavedConnectionNameAsync();
    }

    public class FirstRunService : IFirstRunService
    {
        private const string SetupCompletedKey = "ppdm39:setup:completed";
        private const string ConnectionNameKey = "ppdm39:setup:connectionName";

        private readonly ILocalStorageService _localStorage;
        private readonly ApiClient _apiClient;
        private readonly ILogger<FirstRunService> _logger;

        public FirstRunService(
            ILocalStorageService localStorage,
            ApiClient apiClient,
            ILogger<FirstRunService> logger)
        {
            _localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> IsSetupRequiredAsync()
        {
            var canPersistSetupState = true;

            try
            {
                var completed = await _localStorage.GetItemAsync<bool>(SetupCompletedKey);
                if (completed)
                    return false;
            }
            catch (InvalidOperationException ex)
            {
                canPersistSetupState = false;
                _logger.LogDebug(ex, "Local storage is unavailable during prerender; falling back to API setup status.");
            }
            catch (Exception ex)
            {
                canPersistSetupState = false;
                _logger.LogWarning(ex, "Could not read setup flag from local storage; falling back to API setup status.");
            }

            try
            {
                // Double-check with the API — connection might have been set up outside the wizard
                var status = await _apiClient.GetAsync<SetupStatusResult>("api/ppdm39/setup/status");
                if (status?.HasConnection == true && status.IsSchemaReady)
                {
                    if (canPersistSetupState)
                    {
                        // Auto-mark as complete so we don't prompt again when local storage is available.
                        await MarkSetupCompleteAsync(status.ConnectionName ?? string.Empty);
                    }

                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not determine setup status from the API; assuming setup required");
                return true;
            }
        }

        public async Task MarkSetupCompleteAsync(string connectionName)
        {
            await _localStorage.SetItemAsync(SetupCompletedKey, true);
            if (!string.IsNullOrWhiteSpace(connectionName))
                await _localStorage.SetItemAsync(ConnectionNameKey, connectionName);
        }

        public async Task ClearSetupFlagAsync()
        {
            await _localStorage.RemoveItemAsync(SetupCompletedKey);
            await _localStorage.RemoveItemAsync(ConnectionNameKey);
        }

        public async Task<string?> GetSavedConnectionNameAsync()
        {
            return await _localStorage.GetItemAsync<string>(ConnectionNameKey);
        }
    }
}
