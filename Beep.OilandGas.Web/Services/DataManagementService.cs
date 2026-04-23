using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.SeedData;
using Beep.OilandGas.Web.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Web.Services
{
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
        /// Test a PPDM setup connection configuration.
        /// </summary>
        Task<ConnectionTestResult> TestConnectionAsync(ConnectionConfig connectionConfig);

        /// <summary>
        /// Save a PPDM setup connection.
        /// </summary>
        Task<SaveConnectionResult> SaveConnectionAsync(SaveConnectionRequest request);

        /// <summary>
        /// Get available PPDM setup database types.
        /// </summary>
        Task<List<string>> GetAvailableDatabaseTypesAsync();

        /// <summary>
        /// Discover available PPDM setup scripts for a database type.
        /// </summary>
        Task<List<ScriptInfo>> DiscoverScriptsAsync(string databaseType);

        /// <summary>
        /// Start PPDM database creation.
        /// </summary>
        Task<DatabaseCreationResult> CreateDatabaseAsync(CreateDatabaseRequest request);

        /// <summary>
        /// Get PPDM database creation progress.
        /// </summary>
        Task<ScriptExecutionProgressInfo?> GetCreationProgressAsync(string executionId);

        /// <summary>
        /// Delete a saved PPDM setup connection.
        /// </summary>
        Task<DeleteConnectionResult> DeleteConnectionAsync(string connectionName);

        /// <summary>
        /// Drop a PPDM database or schema.
        /// </summary>
        Task<DropDatabaseResult> DropDatabaseAsync(DropDatabaseRequest request);

        /// <summary>
        /// Recreate a PPDM database or schema.
        /// </summary>
        Task<RecreateDatabaseResult> RecreateDatabaseAsync(RecreateDatabaseRequest request);

        /// <summary>
        /// Start a PPDM database copy operation.
        /// </summary>
        Task<OperationStartResponse> CopyDatabaseAsync(CopyDatabaseRequest request);

        /// <summary>
        /// Get the current well-status facet seed status.
        /// </summary>
        Task<FacetSeedStatus?> GetWellStatusFacetSeedStatusAsync();

        /// <summary>
        /// Seed well-status facet reference data.
        /// </summary>
        Task<SeedingOperationResult> SeedWellStatusFacetsAsync();

        /// <summary>
        /// Seed enum-backed reference data.
        /// </summary>
        Task<SeedingOperationResult> SeedEnumReferenceDataAsync();

        /// <summary>
        /// Seed all reference data.
        /// </summary>
        Task<SeedingOperationResult> SeedAllReferenceDataAsync();

        /// <summary>
        /// Generate PPDM setup dummy data.
        /// </summary>
        Task<GenerateDummyDataResponse> GenerateDummyDataAsync(GenerateDummyDataRequest request);

        /// <summary>
        /// Get PPDM audit statistics.
        /// </summary>
        Task<AccessStatistics?> GetAuditStatisticsAsync(DateTime? from = null, DateTime? to = null, string? tableName = null);

        /// <summary>
        /// Get recent PPDM audit events.
        /// </summary>
        Task<List<DataAccessEvent>> GetRecentAuditEventsAsync(DateTime? from = null, DateTime? to = null);

        /// <summary>
        /// Create a SQLite PPDM setup database.
        /// </summary>
        Task<CreateSqliteResult> CreateSqliteAsync(CreateSqliteRequest request);

        /// <summary>
        /// Create schema from the migration-based setup path.
        /// </summary>
        Task<CreateSchemaResult> CreateSchemaFromMigrationAsync(CreateSchemaRequest request);

        /// <summary>
        /// Build a schema migration plan and review artifacts.
        /// </summary>
        Task<SchemaMigrationPlanResult> PlanSchemaMigrationAsync(SchemaMigrationPlanRequest request);

        /// <summary>
        /// Record approval for a schema migration plan.
        /// </summary>
        Task<SchemaMigrationApprovalResult> ApproveSchemaMigrationAsync(SchemaMigrationApprovalRequest request);

        /// <summary>
        /// Execute an approved schema migration plan.
        /// </summary>
        Task<SchemaMigrationExecuteResult> ExecuteSchemaMigrationAsync(SchemaMigrationExecuteRequest request);

        /// <summary>
        /// Start an approved schema migration plan in the background and return an execution token.
        /// </summary>
        Task<OperationStartResponse> StartSchemaMigrationExecutionAsync(SchemaMigrationExecuteRequest request);

        /// <summary>
        /// Get checkpointed progress for a schema migration execution.
        /// </summary>
        Task<SchemaMigrationProgressResult> GetSchemaMigrationProgressAsync(string executionToken);

        /// <summary>
        /// Get stored evidence artifacts for a schema migration plan.
        /// </summary>
        Task<SchemaMigrationArtifactsResult> GetSchemaMigrationArtifactsAsync(string planId);

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
        Task<Stream?> ExportToCsvAsync(string tableName, List<AppFilter>? filters = null, string? connectionName = null, Action<ProgressUpdate>? onProgress = null);

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
        
        // Retry configuration
        private readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(1);

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

        public async Task<ConnectionTestResult> TestConnectionAsync(ConnectionConfig connectionConfig)
        {
            ArgumentNullException.ThrowIfNull(connectionConfig);

            try
            {
                var result = await _apiClient.PostAsync<ConnectionConfig, ConnectionTestResult>(
                    "/api/ppdm39/setup/test-connection",
                    connectionConfig);

                return result ?? new ConnectionTestResult
                {
                    Success = false,
                    Message = "Connection test failed"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing setup connection {ConnectionName}", connectionConfig.ConnectionName);
                return new ConnectionTestResult
                {
                    Success = false,
                    Message = "Connection test failed",
                    ErrorDetails = ex.Message
                };
            }
        }

        public async Task<SaveConnectionResult> SaveConnectionAsync(SaveConnectionRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                var result = await _apiClient.PostAsync<SaveConnectionRequest, SaveConnectionResult>(
                    "/api/ppdm39/setup/save-connection",
                    request);

                if (result?.Success == true)
                {
                    await RefreshConnectionsAsync();
                }

                return result ?? new SaveConnectionResult
                {
                    Success = false,
                    Message = "Failed to save connection"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving setup connection {ConnectionName}", request.Connection?.ConnectionName);
                return new SaveConnectionResult
                {
                    Success = false,
                    Message = "Failed to save connection",
                    ErrorDetails = ex.Message
                };
            }
        }

        public async Task<List<string>> GetAvailableDatabaseTypesAsync()
        {
            try
            {
                return await _apiClient.GetAsync<List<string>>("/api/ppdm39/setup/database-types")
                    ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available database types");
                return new List<string>();
            }
        }

        public async Task<List<ScriptInfo>> DiscoverScriptsAsync(string databaseType)
        {
            if (string.IsNullOrWhiteSpace(databaseType))
            {
                return new List<ScriptInfo>();
            }

            try
            {
                return await _apiClient.GetAsync<List<ScriptInfo>>(
                    $"/api/ppdm39/setup/discover-scripts/{Uri.EscapeDataString(databaseType)}")
                    ?? new List<ScriptInfo>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error discovering setup scripts for database type {DatabaseType}", databaseType);
                return new List<ScriptInfo>();
            }
        }

        public async Task<DatabaseCreationResult> CreateDatabaseAsync(CreateDatabaseRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                var result = await _apiClient.PostAsync<CreateDatabaseRequest, DatabaseCreationResult>(
                    "/api/ppdm39/setup/create-database",
                    request);

                return result ?? new DatabaseCreationResult
                {
                    Success = false,
                    ErrorMessage = "Database creation failed"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PPDM database for connection {ConnectionName}", request.Connection?.ConnectionName);
                return new DatabaseCreationResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ScriptExecutionProgressInfo?> GetCreationProgressAsync(string executionId)
        {
            if (string.IsNullOrWhiteSpace(executionId))
            {
                return null;
            }

            try
            {
                return await _apiClient.GetAsync<ScriptExecutionProgressInfo>(
                    $"/api/ppdm39/setup/creation-progress/{Uri.EscapeDataString(executionId)}");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error getting database creation progress for execution {ExecutionId}", executionId);
                return null;
            }
        }

        public async Task<DeleteConnectionResult> DeleteConnectionAsync(string connectionName)
        {
            if (string.IsNullOrWhiteSpace(connectionName))
            {
                return new DeleteConnectionResult
                {
                    Success = false,
                    Message = "Connection name is required"
                };
            }

            try
            {
                var result = await _apiClient.DeleteAsync<DeleteConnectionResult>(
                    $"/api/ppdm39/setup/connection/{Uri.EscapeDataString(connectionName)}");

                if (result?.Success == true)
                {
                    await RefreshConnectionsAsync();
                }

                return result ?? new DeleteConnectionResult
                {
                    Success = false,
                    Message = "Failed to delete connection"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting setup connection {ConnectionName}", connectionName);
                return new DeleteConnectionResult
                {
                    Success = false,
                    Message = "Failed to delete connection",
                    ErrorDetails = ex.Message
                };
            }
        }

        public async Task<DropDatabaseResult> DropDatabaseAsync(DropDatabaseRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                var result = await _apiClient.PostAsync<DropDatabaseRequest, DropDatabaseResult>(
                    "/api/ppdm39/setup/drop-database",
                    request);

                return result ?? new DropDatabaseResult
                {
                    Success = false,
                    Message = "Failed to drop database"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dropping setup database for connection {ConnectionName}", request.ConnectionName);
                return new DropDatabaseResult
                {
                    Success = false,
                    Message = "Failed to drop database",
                    ErrorDetails = ex.Message
                };
            }
        }

        public async Task<RecreateDatabaseResult> RecreateDatabaseAsync(RecreateDatabaseRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                var result = await _apiClient.PostAsync<RecreateDatabaseRequest, RecreateDatabaseResult>(
                    "/api/ppdm39/setup/recreate-database",
                    request);

                return result ?? new RecreateDatabaseResult
                {
                    Success = false,
                    Message = "Failed to recreate database"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recreating setup database for connection {ConnectionName}", request.ConnectionName);
                return new RecreateDatabaseResult
                {
                    Success = false,
                    Message = "Failed to recreate database",
                    ErrorDetails = ex.Message
                };
            }
        }

        public async Task<OperationStartResponse> CopyDatabaseAsync(CopyDatabaseRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                var result = await _apiClient.PostAsync<CopyDatabaseRequest, OperationStartResponse>(
                    "/api/ppdm39/setup/copy-database",
                    request);

                return result ?? new OperationStartResponse
                {
                    Success = false,
                    OperationId = string.Empty,
                    Message = "Failed to start database copy"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error copying setup database from {SourceConnection} to {TargetConnection}", request.SourceConnectionName, request.TargetConnectionName);
                return new OperationStartResponse
                {
                    Success = false,
                    OperationId = string.Empty,
                    Message = ex.Message
                };
            }
        }

        public async Task<FacetSeedStatus?> GetWellStatusFacetSeedStatusAsync()
        {
            try
            {
                return await _apiClient.GetAsync<FacetSeedStatus>(
                    "/api/ppdm39/setup/seed/well-status-facets/status");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error getting well-status facet seed status");
                return null;
            }
        }

        public async Task<SeedingOperationResult> SeedWellStatusFacetsAsync()
        {
            try
            {
                var rawResult = await _apiClient.PostAsync<object, FacetSeedResult>(
                    "/api/ppdm39/setup/seed/well-status-facets",
                    new { });

                return rawResult == null
                    ? new SeedingOperationResult
                    {
                        Success = false,
                        Message = "Facet seeding failed."
                    }
                    : new SeedingOperationResult
                    {
                        Success = rawResult.Success,
                        Message = rawResult.Message,
                        TotalInserted = rawResult.TotalInserted,
                        Details = new List<string>
                        {
                            $"R_WELL_STATUS_TYPE:       {rawResult.FacetTypeRows} rows",
                            $"R_WELL_STATUS:            {rawResult.FacetValueRows} rows",
                            $"R_WELL_STATUS_QUAL:       {rawResult.FacetQualifierRows} rows",
                            $"R_WELL_STATUS_QUAL_VALUE: {rawResult.FacetQualValueRows} rows"
                        },
                        Errors = rawResult.Errors ?? new List<string>()
                    };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding well-status facets");
                return new SeedingOperationResult
                {
                    Success = false,
                    Message = "Facet seeding failed.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<SeedingOperationResult> SeedEnumReferenceDataAsync()
        {
            try
            {
                return await _apiClient.PostAsync<object, SeedingOperationResult>(
                           "/api/ppdm39/setup/seed/enum-reference-data",
                           new { })
                       ?? new SeedingOperationResult
                       {
                           Success = false,
                           Message = "Enum reference data seeding failed."
                       };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding enum reference data");
                return new SeedingOperationResult
                {
                    Success = false,
                    Message = "Enum reference data seeding failed.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<SeedingOperationResult> SeedAllReferenceDataAsync()
        {
            try
            {
                return await _apiClient.PostAsync<object, SeedingOperationResult>(
                           "/api/ppdm39/setup/seed/all-reference-data",
                           new { })
                       ?? new SeedingOperationResult
                       {
                           Success = false,
                           Message = "Reference data seeding failed."
                       };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding all reference data");
                return new SeedingOperationResult
                {
                    Success = false,
                    Message = "Reference data seeding failed.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<GenerateDummyDataResponse> GenerateDummyDataAsync(GenerateDummyDataRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                return await _apiClient.PostAsync<GenerateDummyDataRequest, GenerateDummyDataResponse>(
                           "/api/ppdm39/setup/generate-dummy-data",
                           request)
                       ?? new GenerateDummyDataResponse
                       {
                           Success = false,
                           Message = "Generation failed."
                       };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating dummy setup data for seed option {SeedOption}", request.SeedOption);
                return new GenerateDummyDataResponse
                {
                    Success = false,
                    Message = "An error occurred generating demo data.",
                    ErrorDetails = ex.Message,
                    SeedOption = request.SeedOption
                };
            }
        }

        public async Task<AccessStatistics?> GetAuditStatisticsAsync(DateTime? from = null, DateTime? to = null, string? tableName = null)
        {
            try
            {
                var queryParts = new List<string>();
                if (from.HasValue)
                {
                    queryParts.Add($"from={Uri.EscapeDataString(from.Value.ToString("o"))}");
                }

                if (to.HasValue)
                {
                    queryParts.Add($"to={Uri.EscapeDataString(to.Value.ToString("o"))}");
                }

                if (!string.IsNullOrWhiteSpace(tableName))
                {
                    queryParts.Add($"tableName={Uri.EscapeDataString(tableName)}");
                }

                var url = "/api/ppdm39/audit/statistics";
                if (queryParts.Count > 0)
                {
                    url += "?" + string.Join("&", queryParts);
                }

                return await _apiClient.GetAsync<AccessStatistics>(url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting PPDM audit statistics");
                return null;
            }
        }

        public async Task<List<DataAccessEvent>> GetRecentAuditEventsAsync(DateTime? from = null, DateTime? to = null)
        {
            try
            {
                var queryParts = new List<string>();
                if (from.HasValue)
                {
                    queryParts.Add($"from={Uri.EscapeDataString(from.Value.ToString("o"))}");
                }

                if (to.HasValue)
                {
                    queryParts.Add($"to={Uri.EscapeDataString(to.Value.ToString("o"))}");
                }

                var url = "/api/ppdm39/audit/recent";
                if (queryParts.Count > 0)
                {
                    url += "?" + string.Join("&", queryParts);
                }

                return await _apiClient.GetAsync<List<DataAccessEvent>>(url)
                    ?? new List<DataAccessEvent>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent PPDM audit events");
                return new List<DataAccessEvent>();
            }
        }

        public async Task<CreateSqliteResult> CreateSqliteAsync(CreateSqliteRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                return await _apiClient.PostAsync<CreateSqliteRequest, CreateSqliteResult>(
                           "/api/ppdm39/setup/create-sqlite",
                           request)
                       ?? new CreateSqliteResult
                       {
                           Success = false,
                           Message = "Failed to create database."
                       };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating SQLite setup database for connection {ConnectionName}", request.ConnectionName);
                return new CreateSqliteResult
                {
                    Success = false,
                    Message = "An error occurred.",
                    ErrorDetails = ex.Message,
                    ConnectionName = request.ConnectionName
                };
            }
        }

        public async Task<CreateSchemaResult> CreateSchemaFromMigrationAsync(CreateSchemaRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                return await _apiClient.PostAsync<CreateSchemaRequest, CreateSchemaResult>(
                           "/api/ppdm39/setup/create-schema-from-migration",
                           request)
                       ?? new CreateSchemaResult
                       {
                           Success = false,
                           Message = "Schema migration failed."
                       };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating schema from migration for connection {ConnectionName}", request.ConnectionName);
                return new CreateSchemaResult
                {
                    Success = false,
                    Message = "Schema migration failed.",
                    ErrorDetails = ex.Message
                };
            }
        }

        public async Task<SchemaMigrationPlanResult> PlanSchemaMigrationAsync(SchemaMigrationPlanRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                return await _apiClient.PostAsync<SchemaMigrationPlanRequest, SchemaMigrationPlanResult>(
                           "/api/ppdm39/setup/schema/plan",
                           request)
                       ?? new SchemaMigrationPlanResult
                       {
                           Success = false,
                           Message = "Schema migration planning failed."
                       };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error planning schema migration for connection {ConnectionName}", request.ConnectionName);
                return new SchemaMigrationPlanResult
                {
                    Success = false,
                    ConnectionName = request.ConnectionName,
                    Message = "Schema migration planning failed.",
                    DryRunDiagnostics = new List<string> { ex.Message }
                };
            }
        }

        public async Task<SchemaMigrationApprovalResult> ApproveSchemaMigrationAsync(SchemaMigrationApprovalRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                return await _apiClient.PostAsync<SchemaMigrationApprovalRequest, SchemaMigrationApprovalResult>(
                           "/api/ppdm39/setup/schema/approve",
                           request)
                       ?? new SchemaMigrationApprovalResult
                       {
                           Success = false,
                           Message = "Schema migration approval failed."
                       };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving schema migration plan {PlanId}", request.PlanId);
                return new SchemaMigrationApprovalResult
                {
                    Success = false,
                    PlanId = request.PlanId,
                    Message = "Schema migration approval failed."
                };
            }
        }

        public async Task<SchemaMigrationExecuteResult> ExecuteSchemaMigrationAsync(SchemaMigrationExecuteRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                return await _apiClient.PostAsync<SchemaMigrationExecuteRequest, SchemaMigrationExecuteResult>(
                           "/api/ppdm39/setup/schema/execute",
                           request)
                       ?? new SchemaMigrationExecuteResult
                       {
                           Success = false,
                           Message = "Schema migration execution failed."
                       };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing schema migration plan {PlanId}", request.PlanId);
                return new SchemaMigrationExecuteResult
                {
                    Success = false,
                    PlanId = request.PlanId,
                    Message = "Schema migration execution failed.",
                    CompensationOutcome = ex.Message
                };
            }
        }

        public async Task<OperationStartResponse> StartSchemaMigrationExecutionAsync(SchemaMigrationExecuteRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                return await _apiClient.PostAsync<SchemaMigrationExecuteRequest, OperationStartResponse>(
                           "/api/ppdm39/setup/schema/start",
                           request)
                       ?? new OperationStartResponse
                       {
                           Success = false,
                           Message = "Schema migration could not be started."
                       };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting schema migration plan {PlanId}", request.PlanId);
                return new OperationStartResponse
                {
                    Success = false,
                    Message = "Schema migration could not be started."
                };
            }
        }

        public async Task<SchemaMigrationProgressResult> GetSchemaMigrationProgressAsync(string executionToken)
        {
            if (string.IsNullOrWhiteSpace(executionToken))
            {
                return new SchemaMigrationProgressResult
                {
                    Success = false,
                    Message = "Execution token is required."
                };
            }

            try
            {
                return await _apiClient.GetAsync<SchemaMigrationProgressResult>(
                           $"/api/ppdm39/setup/schema/progress/{Uri.EscapeDataString(executionToken)}")
                       ?? new SchemaMigrationProgressResult
                       {
                           Success = false,
                           ExecutionToken = executionToken,
                           Message = "Schema migration progress was not found."
                       };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schema migration progress for {ExecutionToken}", executionToken);
                return new SchemaMigrationProgressResult
                {
                    Success = false,
                    ExecutionToken = executionToken,
                    Message = "Could not read schema migration progress.",
                    FailureReason = ex.Message
                };
            }
        }

        public async Task<SchemaMigrationArtifactsResult> GetSchemaMigrationArtifactsAsync(string planId)
        {
            if (string.IsNullOrWhiteSpace(planId))
            {
                return new SchemaMigrationArtifactsResult
                {
                    Success = false,
                    Message = "Plan ID is required."
                };
            }

            try
            {
                return await _apiClient.GetAsync<SchemaMigrationArtifactsResult>(
                           $"/api/ppdm39/setup/schema/artifacts/{Uri.EscapeDataString(planId)}")
                       ?? new SchemaMigrationArtifactsResult
                       {
                           Success = false,
                           PlanId = planId,
                           Message = "Schema migration artifacts were not found."
                       };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schema migration artifacts for {PlanId}", planId);
                return new SchemaMigrationArtifactsResult
                {
                    Success = false,
                    PlanId = planId,
                    Message = "Could not load schema migration artifacts."
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

        /// <summary>
        /// Retry helper method with exponential backoff
        /// </summary>
        private async Task<T> ExecuteWithRetryAsync<T>(
            Func<Task<T>> operation,
            string operationName,
            int maxRetries = 3)
        {
            int attempt = 0;
            Exception? lastException = null;

            while (attempt < maxRetries)
            {
                try
                {
                    return await operation();
                }
                catch (HttpRequestException ex) when (attempt < maxRetries - 1)
                {
                    lastException = ex;
                    attempt++;
                    var delay = TimeSpan.FromMilliseconds(_retryDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));
                    _logger.LogWarning(ex, "Attempt {Attempt} failed for {OperationName}, retrying in {Delay}ms", 
                        attempt, operationName, delay.TotalMilliseconds);
                    await Task.Delay(delay);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Non-retryable error in {OperationName}", operationName);
                    throw;
                }
            }

            _logger.LogError(lastException, "All retry attempts failed for {OperationName}", operationName);
            throw lastException ?? new InvalidOperationException($"Operation {operationName} failed after {maxRetries} attempts");
        }

        public async Task<GetEntitiesResponse> GetEntitiesAsync(string tableName, List<AppFilter>? filters = null, string? connectionName = null)
        {
            try
            {
                return await ExecuteWithRetryAsync(async () =>
                {
                    var request = new GetEntitiesRequest
                    {
                        TableName = tableName,
                        Filters = filters ?? new List<AppFilter>(),
                        ConnectionName = connectionName
                    };
                    return await _apiClient.PostAsync<GetEntitiesRequest, GetEntitiesResponse>(
                        $"/api/ppdm39/data/{tableName}", request) ?? new GetEntitiesResponse { Success = false };
                }, $"GetEntitiesAsync({tableName})");
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
                return await ExecuteWithRetryAsync(async () =>
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
                }, $"InsertEntityAsync({tableName})");
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
                return await ExecuteWithRetryAsync(async () =>
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
                }, $"UpdateEntityAsync({tableName}, {entityId})");
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

        public async Task<Stream?> ExportToCsvAsync(string tableName, List<AppFilter>? filters = null, string? connectionName = null, Action<ProgressUpdate>? onProgress = null)
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
