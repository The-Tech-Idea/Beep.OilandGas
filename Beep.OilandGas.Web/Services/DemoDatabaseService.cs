using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.DataManagement;

namespace Beep.OilandGas.Web.Services
{
    /// <summary>
    /// Service interface for demo database operations
    /// </summary>
    public interface IDemoDatabaseService
    {
        Task<CreateDemoDatabaseResponse> CreateDemoDatabaseAsync(string seedDataOption, string connectionName = "PPDM39");
        Task<List<DemoDatabaseMetadata>> GetMyDemoDatabasesAsync();
        Task<bool> DeleteDemoDatabaseAsync(string connectionName);
    }

    /// <summary>
    /// Client service for demo database operations
    /// </summary>
    public class DemoDatabaseService : IDemoDatabaseService
    {
        private readonly ApiClient _apiClient;
        private readonly OilGasCallFailures _calls;

        public DemoDatabaseService(ApiClient apiClient, OilGasCallFailures calls)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _calls = calls ?? throw new ArgumentNullException(nameof(calls));
        }

        /// <summary>
        /// Create a demo database for the signed-in user (the API takes the owner from the caller's account)
        /// </summary>
        public async Task<CreateDemoDatabaseResponse> CreateDemoDatabaseAsync(
            string seedDataOption, 
            string connectionName = "PPDM39")
        {
            try
            {
                var request = new CreateDemoDatabaseRequest
                {
                    SeedDataOption = seedDataOption,
                    ConnectionName = connectionName
                };

                var response = await _apiClient.PostAsync<CreateDemoDatabaseRequest, CreateDemoDatabaseResponse>(
                    "/api/demo/create", request);

                return response ?? new CreateDemoDatabaseResponse
                {
                    Success = false,
                    Message = "Failed to create demo database"
                };
            }
            // A failed call is answered as a failed result the page shows (its contract); the store keeps the failure.
            catch (Exception failure) when (OilGasCallFailures.IsCallFailure(failure))
            {
                return new CreateDemoDatabaseResponse
                {
                    Success = false,
                    Message = _calls.Explain(failure, "creating a demo database", "The demo database was not created")
                };
            }
        }

        /// <summary>
        /// Get the signed-in user's demo databases
        /// </summary>
        public async Task<List<DemoDatabaseMetadata>> GetMyDemoDatabasesAsync()
        {
            var databases = await _apiClient.GetAsync<List<DemoDatabaseMetadata>>("/api/demo/my-databases");

            return databases ?? new List<DemoDatabaseMetadata>();
        }

        /// <summary>
        /// Delete a demo database
        /// </summary>
        public async Task<bool> DeleteDemoDatabaseAsync(string connectionName)
        {
            var response = await _apiClient.DeleteAsync<DeleteDemoDatabaseResponse>(
                $"/api/demo/{Uri.EscapeDataString(connectionName)}");

            return response?.Success ?? false;
        }
    }
}

