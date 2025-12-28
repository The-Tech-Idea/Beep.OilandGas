using Beep.OilandGas.ApiService.Models;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.Web.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Web.Services
{
    /// <summary>
    /// Response model for current connection endpoint
    /// </summary>
    public class CurrentConnectionResponse
    {
        public string? ConnectionName { get; set; }
    }

    /// <summary>
    /// Service for managing data sources and connections in the web application.
    /// Provides centralized access to current data source, available connections, and related settings.
    /// </summary>
    public interface IDataManagementService
    {
        /// <summary>
        /// Get the current active data source connection name
        /// </summary>
        Task<string?> GetCurrentConnectionNameAsync();

        /// <summary>
        /// Set the current active data source connection
        /// </summary>
        Task<SetCurrentDatabaseResult> SetCurrentConnectionAsync(string connectionName);

        /// <summary>
        /// Get all available database connections
        /// </summary>
        Task<List<DatabaseConnectionListItem>> GetAllConnectionsAsync();

        /// <summary>
        /// Get connection details by name
        /// </summary>
        Task<ConnectionConfig?> GetConnectionByNameAsync(string connectionName);

        /// <summary>
        /// Check if a connection exists
        /// </summary>
        Task<bool> ConnectionExistsAsync(string connectionName);

        /// <summary>
        /// Get connection count
        /// </summary>
        Task<int> GetConnectionCountAsync();

        /// <summary>
        /// Refresh connections cache
        /// </summary>
        Task RefreshConnectionsAsync();

        /// <summary>
        /// Event fired when current connection changes
        /// </summary>
        event EventHandler<string?>? CurrentConnectionChanged;

        /// <summary>
        /// Current connection name (cached)
        /// </summary>
        string? CurrentConnectionName { get; }

        /// <summary>
        /// All connections (cached)
        /// </summary>
        ReadOnlyCollection<DatabaseConnectionListItem> Connections { get; }

        // ============================================
        // Field Management
        // ============================================

        /// <summary>
        /// Get the current active field ID
        /// </summary>
        Task<string?> GetCurrentFieldIdAsync();

        /// <summary>
        /// Set the current active field
        /// </summary>
        Task<bool> SetCurrentFieldAsync(string fieldId);

        /// <summary>
        /// Event fired when current field changes
        /// </summary>
        event Action<string>? CurrentFieldChanged;

        // ============================================
        // Entity Operations
        // ============================================

        /// <summary>
        /// Get entities from a table with optional filters
        /// </summary>
        Task<GetEntitiesResponse> GetEntitiesAsync(string tableName, List<AppFilter>? filters = null, string? connectionName = null);

        /// <summary>
        /// Get a single entity by ID
        /// </summary>
        Task<GenericEntityResponse> GetEntityByIdAsync(string tableName, object id, string? connectionName = null);

        /// <summary>
        /// Insert an entity
        /// </summary>
        Task<GenericEntityResponse> InsertEntityAsync(string tableName, Dictionary<string, object> entityData, string userId, string? connectionName = null);

        /// <summary>
        /// Update an entity
        /// </summary>
        Task<GenericEntityResponse> UpdateEntityAsync(string tableName, string entityId, Dictionary<string, object> entityData, string userId, string? connectionName = null);

        /// <summary>
        /// Delete an entity
        /// </summary>
        Task<GenericEntityResponse> DeleteEntityAsync(string tableName, object id, string userId, string? connectionName = null);

        // ============================================
        // Import/Export Operations
        // ============================================

        /// <summary>
        /// Import data from CSV file
        /// </summary>
        Task<OperationStartResponse> ImportFromCsvAsync(string tableName, Stream csvStream, string fileName, string userId, Dictionary<string, string>? columnMapping = null, bool validateForeignKeys = true, string? connectionName = null, Action<ProgressUpdate>? onProgress = null);

        /// <summary>
        /// Export data to CSV file
        /// </summary>
        Task<Stream> ExportToCsvAsync(string tableName, List<AppFilter>? filters = null, string? connectionName = null, Action<ProgressUpdate>? onProgress = null);

        // ============================================
        // Validation Operations
        // ============================================

        /// <summary>
        /// Validate an entity
        /// </summary>
        Task<ValidationResult> ValidateEntityAsync(string tableName, Dictionary<string, object> entityData, string? connectionName = null);

        /// <summary>
        /// Validate multiple entities in batch
        /// </summary>
        Task<List<ValidationResult>> ValidateBatchAsync(string tableName, List<Dictionary<string, object>> entities, string? connectionName = null);

        /// <summary>
        /// Get validation rules for a table
        /// </summary>
        Task<object> GetValidationRulesAsync(string tableName, string? connectionName = null);

        // ============================================
        // Quality Operations
        // ============================================

        /// <summary>
        /// Get data quality metrics for a table
        /// </summary>
        Task<DataQualityResult> GetTableQualityMetricsAsync(string tableName, string? connectionName = null);

        /// <summary>
        /// Get data quality dashboard
        /// </summary>
        Task<DataQualityDashboardResult> GetQualityDashboardAsync(string? connectionName = null);

        // ============================================
        // Versioning Operations
        // ============================================

        /// <summary>
        /// Create a version snapshot of an entity
        /// </summary>
        Task<VersioningResult> CreateVersionAsync(string tableName, string entityId, Dictionary<string, object>? entityData, string userId, string? versionLabel = null, string? connectionName = null);

        /// <summary>
        /// Get version history for an entity
        /// </summary>
        Task<List<VersionInfo>> GetVersionHistoryAsync(string tableName, string entityId, string? connectionName = null);

        /// <summary>
        /// Restore an entity to a specific version
        /// </summary>
        Task<VersioningResult> RestoreVersionAsync(string tableName, string entityId, string versionId, string userId, string? connectionName = null);

        // ============================================
        // Defaults Operations
        // ============================================

        /// <summary>
        /// Get default values for an entity type
        /// </summary>
        Task<Dictionary<string, object>> GetDefaultsAsync(string entityType, string? connectionName = null);

        /// <summary>
        /// Get well status facets
        /// </summary>
        Task<object> GetWellStatusFacetsAsync(string statusId, string? connectionName = null);
    }

    /// <summary>
    /// Implementation of DataManagementService
    /// </summary>
    public class DataManagementService : IDataManagementService
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<DataManagementService> _logger;
        private readonly IProgressTrackingClient? _progressTrackingClient;
        
        private string? _currentConnectionName;
        private string? _currentFieldId;
        private List<DatabaseConnectionListItem> _connections = new();
        private readonly SemaphoreSlim _refreshLock = new(1, 1);
        private DateTime _lastRefreshTime = DateTime.MinValue;
        private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(5);

        public DataManagementService(
            ApiClient apiClient,
            ILogger<DataManagementService> logger,
            IProgressTrackingClient? progressTrackingClient = null)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _progressTrackingClient = progressTrackingClient;
        }

        public string? CurrentConnectionName => _currentConnectionName;

        public ReadOnlyCollection<DatabaseConnectionListItem> Connections => _connections.AsReadOnly();

        public event EventHandler<string?>? CurrentConnectionChanged;

        public async Task<string?> GetCurrentConnectionNameAsync()
        {
            try
            {
                // Create a simple model class for the response
                var responseModel = await _apiClient.GetAsync<CurrentConnectionResponse>("/api/ppdm39/setup/current-connection");
                _currentConnectionName = responseModel?.ConnectionName;
                return _currentConnectionName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current connection name");
            }
            return null;
        }

        public async Task<SetCurrentDatabaseResult> SetCurrentConnectionAsync(string connectionName)
        {
            try
            {
                var request = new SetCurrentDatabaseRequest { ConnectionName = connectionName };
                var result = await _apiClient.PostAsync<SetCurrentDatabaseRequest, SetCurrentDatabaseResult>(
                    "/api/ppdm39/setup/set-current-connection", request);

                if (result?.Success == true)
                {
                    var oldConnection = _currentConnectionName;
                    _currentConnectionName = connectionName;
                    
                    // Update connections list to reflect current status
                    await RefreshConnectionsAsync();
                    
                    // Fire event
                    if (oldConnection != connectionName)
                    {
                        CurrentConnectionChanged?.Invoke(this, connectionName);
                    }
                }

                return result ?? new SetCurrentDatabaseResult 
                { 
                    Success = false, 
                    Message = "Failed to set current connection" 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting current connection {ConnectionName}", connectionName);
                return new SetCurrentDatabaseResult
                {
                    Success = false,
                    Message = "Error setting current connection",
                    ErrorDetails = ex.Message
                };
            }
        }

        public async Task<List<DatabaseConnectionListItem>> GetAllConnectionsAsync()
        {
            // Use cached data if recent
            if (_connections.Any() && DateTime.UtcNow - _lastRefreshTime < _cacheTimeout)
            {
                return _connections.ToList();
            }

            await RefreshConnectionsAsync();
            return _connections.ToList();
        }

        public async Task<ConnectionConfig?> GetConnectionByNameAsync(string connectionName)
        {
            try
            {
                return await _apiClient.GetAsync<ConnectionConfig>($"/api/ppdm39/setup/connection/{connectionName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting connection {ConnectionName}", connectionName);
                return null;
            }
        }

        public async Task<bool> ConnectionExistsAsync(string connectionName)
        {
            var connections = await GetAllConnectionsAsync();
            return connections.Any(c => c.ConnectionName == connectionName);
        }

        public async Task<int> GetConnectionCountAsync()
        {
            var connections = await GetAllConnectionsAsync();
            return connections.Count;
        }

        public async Task RefreshConnectionsAsync()
        {
            await _refreshLock.WaitAsync();
            try
            {
                var connections = await _apiClient.GetAsync<List<DatabaseConnectionListItem>>("/api/ppdm39/setup/connections") 
                    ?? new List<DatabaseConnectionListItem>();
                
                _connections = connections;
                _lastRefreshTime = DateTime.UtcNow;

                // Also refresh current connection name
                _currentConnectionName = await GetCurrentConnectionNameAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing connections");
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        // ============================================
        // Entity Operations Implementation
        // ============================================

        public async Task<GetEntitiesResponse> GetEntitiesAsync(string tableName, List<AppFilter>? filters = null, string? connectionName = null)
        {
            try
            {
                var request = new GetEntitiesRequest
                {
                    TableName = tableName,
                    Filters = filters ?? new List<AppFilter>(),
                    ConnectionName = connectionName
                };
                return await _apiClient.PostAsync<GetEntitiesRequest, GetEntitiesResponse>(
                    $"/api/ppdm39/data/{tableName}", request) ?? new GetEntitiesResponse { Success = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entities from table {TableName}", tableName);
                return new GetEntitiesResponse { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<GenericEntityResponse> GetEntityByIdAsync(string tableName, object id, string? connectionName = null)
        {
            try
            {
                var url = $"/api/ppdm39/data/{tableName}/{id}";
                if (!string.IsNullOrEmpty(connectionName))
                    url += $"?connectionName={Uri.EscapeDataString(connectionName)}";
                
                return await _apiClient.GetAsync<GenericEntityResponse>(url) ?? new GenericEntityResponse { Success = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entity {Id} from table {TableName}", id, tableName);
                return new GenericEntityResponse { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<GenericEntityResponse> InsertEntityAsync(string tableName, Dictionary<string, object> entityData, string userId, string? connectionName = null)
        {
            try
            {
                var request = new GenericEntityRequest
                {
                    TableName = tableName,
                    EntityData = entityData,
                    ConnectionName = connectionName
                };
                var url = $"/api/ppdm39/data/{tableName}/insert?userId={Uri.EscapeDataString(userId)}";
                return await _apiClient.PostAsync<GenericEntityRequest, GenericEntityResponse>(url, request) 
                    ?? new GenericEntityResponse { Success = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting entity into table {TableName}", tableName);
                return new GenericEntityResponse { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<GenericEntityResponse> UpdateEntityAsync(string tableName, string entityId, Dictionary<string, object> entityData, string userId, string? connectionName = null)
        {
            try
            {
                var request = new GenericEntityRequest
                {
                    TableName = tableName,
                    EntityData = entityData,
                    ConnectionName = connectionName
                };
                var url = $"/api/ppdm39/data/{tableName}/{entityId}?userId={Uri.EscapeDataString(userId)}";
                return await _apiClient.PutAsync<GenericEntityRequest, GenericEntityResponse>(url, request) 
                    ?? new GenericEntityResponse { Success = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating entity {EntityId} in table {TableName}", entityId, tableName);
                return new GenericEntityResponse { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<GenericEntityResponse> DeleteEntityAsync(string tableName, object id, string userId, string? connectionName = null)
        {
            try
            {
                var url = $"/api/ppdm39/data/{tableName}/{id}?userId={Uri.EscapeDataString(userId)}";
                if (!string.IsNullOrEmpty(connectionName))
                    url += $"&connectionName={Uri.EscapeDataString(connectionName)}";
                
                return await _apiClient.DeleteAsync<GenericEntityResponse>(url) ?? new GenericEntityResponse { Success = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting entity {Id} from table {TableName}", id, tableName);
                return new GenericEntityResponse { Success = false, ErrorMessage = ex.Message };
            }
        }

        // ============================================
        // Import/Export Operations Implementation
        // ============================================

        public async Task<OperationStartResponse> ImportFromCsvAsync(string tableName, Stream csvStream, string fileName, string userId, Dictionary<string, string>? columnMapping = null, bool validateForeignKeys = true, string? connectionName = null, Action<ProgressUpdate>? onProgress = null)
        {
            try
            {
                // Convert stream to multipart form data
                using var content = new MultipartFormDataContent();
                using var streamContent = new StreamContent(csvStream);
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
                content.Add(streamContent, "file", fileName);

                var url = $"/api/ppdm39/import-export/csv/{tableName}?userId={Uri.EscapeDataString(userId)}&validateForeignKeys={validateForeignKeys}";
                if (!string.IsNullOrEmpty(connectionName))
                    url += $"&connectionName={Uri.EscapeDataString(connectionName)}";

                var response = await _apiClient.PostAsync<OperationStartResponse>(url, content);
                
                // Subscribe to progress if callback provided
                if (onProgress != null && response?.OperationId != null && _progressTrackingClient != null)
                {
                    _progressTrackingClient.OnProgressUpdate += (progress) =>
                    {
                        if (progress.OperationId == response.OperationId)
                            onProgress(progress);
                    };
                    await _progressTrackingClient.JoinOperationAsync(response.OperationId);
                }

                return response ?? new OperationStartResponse { OperationId = "", Message = "Import failed" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting CSV import for table {TableName}", tableName);
                return new OperationStartResponse { OperationId = "", Message = ex.Message };
            }
        }

        public async Task<Stream> ExportToCsvAsync(string tableName, List<AppFilter>? filters = null, string? connectionName = null, Action<ProgressUpdate>? onProgress = null)
        {
            try
            {
                var request = new ExportRequest
                {
                    TableName = tableName,
                    Filters = filters,
                    Format = "csv",
                    IncludeHeaders = true,
                    ConnectionName = connectionName
                };

                var url = $"/api/ppdm39/import-export/csv/{tableName}/export";
                return await _apiClient.PostStreamAsync(url, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting CSV for table {TableName}", tableName);
                throw;
            }
        }

        // ============================================
        // Validation Operations Implementation
        // ============================================

        public async Task<ValidationResult> ValidateEntityAsync(string tableName, Dictionary<string, object> entityData, string? connectionName = null)
        {
            try
            {
                var request = new ValidationRequest
                {
                    TableName = tableName,
                    EntityData = entityData,
                    ConnectionName = connectionName
                };
                return await _apiClient.PostAsync<ValidationRequest, ValidationResult>(
                    $"/api/ppdm39/validation/{tableName}/validate", request) 
                    ?? new ValidationResult { IsValid = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating entity in table {TableName}", tableName);
                return new ValidationResult { IsValid = false, Errors = new List<ValidationError> { new ValidationError { ErrorMessage = ex.Message } } };
            }
        }

        public async Task<List<ValidationResult>> ValidateBatchAsync(string tableName, List<Dictionary<string, object>> entities, string? connectionName = null)
        {
            try
            {
                var request = new BatchValidationRequest
                {
                    TableName = tableName,
                    Entities = entities,
                    ConnectionName = connectionName
                };
                return await _apiClient.PostAsync<BatchValidationRequest, List<ValidationResult>>(
                    $"/api/ppdm39/validation/{tableName}/validate-batch", request) 
                    ?? new List<ValidationResult>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating batch entities in table {TableName}", tableName);
                return new List<ValidationResult>();
            }
        }

        public async Task<object> GetValidationRulesAsync(string tableName, string? connectionName = null)
        {
            try
            {
                var url = $"/api/ppdm39/validation/{tableName}/rules";
                if (!string.IsNullOrEmpty(connectionName))
                    url += $"?connectionName={Uri.EscapeDataString(connectionName)}";
                
                return await _apiClient.GetAsync<object>(url) ?? new List<object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting validation rules for table {TableName}", tableName);
                return new List<object>();
            }
        }

        // ============================================
        // Quality Operations Implementation
        // ============================================

        public async Task<DataQualityResult> GetTableQualityMetricsAsync(string tableName, string? connectionName = null)
        {
            try
            {
                var url = $"/api/datamanagement/quality/{tableName}/metrics";
                if (!string.IsNullOrEmpty(connectionName))
                    url += $"?connectionName={Uri.EscapeDataString(connectionName)}";
                
                return await _apiClient.GetAsync<DataQualityResult>(url) 
                    ?? new DataQualityResult { TableName = tableName };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quality metrics for table {TableName}", tableName);
                return new DataQualityResult { TableName = tableName };
            }
        }

        public async Task<DataQualityDashboardResult> GetQualityDashboardAsync(string? connectionName = null)
        {
            try
            {
                var url = "/api/datamanagement/quality/alerts";
                if (!string.IsNullOrEmpty(connectionName))
                    url += $"?connectionName={Uri.EscapeDataString(connectionName)}";
                
                return await _apiClient.GetAsync<DataQualityDashboardResult>(url) 
                    ?? new DataQualityDashboardResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quality dashboard");
                return new DataQualityDashboardResult();
            }
        }

        // ============================================
        // Versioning Operations Implementation
        // ============================================

        public async Task<VersioningResult> CreateVersionAsync(string tableName, string entityId, Dictionary<string, object>? entityData, string userId, string? versionLabel = null, string? connectionName = null)
        {
            try
            {
                var request = new VersioningRequest
                {
                    TableName = tableName,
                    EntityId = entityId,
                    UserId = userId,
                    ConnectionName = connectionName
                };
                return await _apiClient.PostAsync<VersioningRequest, VersioningResult>(
                    $"/api/ppdm39/versioning/{tableName}/{entityId}/create-version?userId={Uri.EscapeDataString(userId)}", request) 
                    ?? new VersioningResult { Success = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating version for entity {EntityId} in table {TableName}", entityId, tableName);
                return new VersioningResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<List<VersionInfo>> GetVersionHistoryAsync(string tableName, string entityId, string? connectionName = null)
        {
            try
            {
                var url = $"/api/ppdm39/versioning/{tableName}/{entityId}/versions";
                if (!string.IsNullOrEmpty(connectionName))
                    url += $"?connectionName={Uri.EscapeDataString(connectionName)}";
                
                return await _apiClient.GetAsync<List<VersionInfo>>(url) ?? new List<VersionInfo>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting version history for entity {EntityId} in table {TableName}", entityId, tableName);
                return new List<VersionInfo>();
            }
        }

        public async Task<VersioningResult> RestoreVersionAsync(string tableName, string entityId, string versionId, string userId, string? connectionName = null)
        {
            try
            {
                var request = new RestoreVersionRequest
                {
                    TableName = tableName,
                    EntityId = entityId,
                    VersionId = versionId,
                    UserId = userId,
                    ConnectionName = connectionName
                };
                return await _apiClient.PostAsync<RestoreVersionRequest, VersioningResult>(
                    $"/api/ppdm39/versioning/{tableName}/{entityId}/restore?userId={Uri.EscapeDataString(userId)}", request) 
                    ?? new VersioningResult { Success = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring version for entity {EntityId} in table {TableName}", entityId, tableName);
                return new VersioningResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        // ============================================
        // Defaults Operations Implementation
        // ============================================

        public async Task<Dictionary<string, object>> GetDefaultsAsync(string entityType, string? connectionName = null)
        {
            try
            {
                var url = $"/api/ppdm39/defaults/{entityType}";
                if (!string.IsNullOrEmpty(connectionName))
                    url += $"?connectionName={Uri.EscapeDataString(connectionName)}";
                
                return await _apiClient.GetAsync<Dictionary<string, object>>(url) ?? new Dictionary<string, object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting defaults for entity type {EntityType}", entityType);
                return new Dictionary<string, object>();
            }
        }

        public async Task<object> GetWellStatusFacetsAsync(string statusId, string? connectionName = null)
        {
            try
            {
                var url = $"/api/ppdm39/defaults/well-status/{statusId}/facets";
                if (!string.IsNullOrEmpty(connectionName))
                    url += $"?connectionName={Uri.EscapeDataString(connectionName)}";
                
                return await _apiClient.GetAsync<object>(url) ?? new List<object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting well status facets for status {StatusId}", statusId);
                return new List<object>();
            }
        }

        // ============================================
        // Field Management Implementation
        // ============================================

        public event Action<string>? CurrentFieldChanged;

        public async Task<string?> GetCurrentFieldIdAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(_currentFieldId))
                {
                    var response = await _apiClient.GetAsync<FieldResponse>("/api/field/current");
                    _currentFieldId = response?.FieldId;
                }
                return _currentFieldId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current field ID");
                return null;
            }
        }

        public async Task<bool> SetCurrentFieldAsync(string fieldId)
        {
            try
            {
                var request = new SetActiveFieldRequest { FieldId = fieldId };
                var response = await _apiClient.PostAsync<SetActiveFieldRequest, SetActiveFieldResponse>(
                    "/api/field/set-active", request);

                if (response?.Success == true)
                {
                    _currentFieldId = fieldId;
                    CurrentFieldChanged?.Invoke(fieldId);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting current field: {FieldId}", fieldId);
                return false;
            }
        }
    }
}
