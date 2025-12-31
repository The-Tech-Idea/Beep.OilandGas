using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Models;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Web.Services
{
    /// <summary>
    /// Service interface for demo database operations
    /// </summary>
    public interface IDemoDatabaseService
    {
        Task<CreateDemoDatabaseResponse> CreateDemoDatabaseAsync(string userId, string seedDataOption, string? connectionName = null);
        Task<List<DemoDatabaseMetadata>> GetMyDemoDatabasesAsync(string userId);
        Task<bool> DeleteDemoDatabaseAsync(string connectionName);
    }

    /// <summary>
    /// Client service for demo database operations
    /// </summary>
    public class DemoDatabaseService : IDemoDatabaseService
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<DemoDatabaseService> _logger;

        public DemoDatabaseService(
            ApiClient apiClient,
            ILogger<DemoDatabaseService> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Create a demo database for a user
        /// </summary>
        public async Task<CreateDemoDatabaseResponse> CreateDemoDatabaseAsync(
            string userId, 
            string seedDataOption, 
            string? connectionName = null)
        {
            try
            {
                var request = new CreateDemoDatabaseRequest
                {
                    UserId = userId,
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating demo database for user {UserId}", userId);
                return new CreateDemoDatabaseResponse
                {
                    Success = false,
                    Message = "Failed to create demo database",
                    ErrorDetails = ex.Message
                };
            }
        }

        /// <summary>
        /// Get demo databases for a user
        /// </summary>
        public async Task<List<DemoDatabaseMetadata>> GetMyDemoDatabasesAsync(string userId)
        {
            try
            {
                var databases = await _apiClient.GetAsync<List<DemoDatabaseMetadata>>(
                    $"/api/demo/my-databases?userId={Uri.EscapeDataString(userId)}");

                return databases ?? new List<DemoDatabaseMetadata>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting demo databases for user {UserId}", userId);
                return new List<DemoDatabaseMetadata>();
            }
        }

        /// <summary>
        /// Delete a demo database
        /// </summary>
        public async Task<bool> DeleteDemoDatabaseAsync(string connectionName)
        {
            try
            {
                var response = await _apiClient.DeleteAsync<DeleteDemoDatabaseResponse>(
                    $"/api/demo/{Uri.EscapeDataString(connectionName)}");

                return response?.Success ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting demo database {ConnectionName}", connectionName);
                return false;
            }
        }
    }
}

