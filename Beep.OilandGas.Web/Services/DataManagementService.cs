using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.SeedData;
using Beep.OilandGas.Web.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using TheTechIdea.Beep.Report;
using TheTechIdeaWeb.Diagnostics;

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
        Task<GetEntitiesResponse> GetEntitiesAsync(string tableName, List<AppFilter>? filters = null, string connectionName = "PPDM39");

        /// <summary>
        /// Get a single entity by ID
        /// </summary>
        Task<GenericEntityResponse> GetEntityByIdAsync(string tableName, object id, string connectionName = "PPDM39");

        /// <summary>
        /// Insert an entity
        /// </summary>
        Task<GenericEntityResponse> InsertEntityAsync(string tableName, Dictionary<string, object> entityData, string connectionName = "PPDM39");

        /// <summary>
        /// Update an entity
        /// </summary>
        Task<GenericEntityResponse> UpdateEntityAsync(string tableName, string entityId, Dictionary<string, object> entityData, string connectionName = "PPDM39");

        /// <summary>
        /// Delete an entity
        /// </summary>
        Task<GenericEntityResponse> DeleteEntityAsync(string tableName, object id, string connectionName = "PPDM39");

        // ============================================
        // Import/Export Operations
        // ============================================

        /// <summary>
        /// Import data from CSV file
        /// </summary>
        Task<OperationStartResponse> ImportFromCsvAsync(string tableName, Stream csvStream, string fileName, Dictionary<string, string>? columnMapping = null, bool validateForeignKeys = true, string connectionName = "PPDM39", Action<ProgressUpdate>? onProgress = null);

        /// <summary>
        /// Export data to CSV file
        /// </summary>
        Task<Stream?> ExportToCsvAsync(string tableName, List<AppFilter>? filters = null, string connectionName = "PPDM39", Action<ProgressUpdate>? onProgress = null);

        // ============================================
        // Validation Operations
        // ============================================

        /// <summary>
        /// Validate an entity
        /// </summary>
        Task<ValidationResult> ValidateEntityAsync(string tableName, Dictionary<string, object> entityData, string connectionName = "PPDM39");

        /// <summary>
        /// Validate multiple entities in batch
        /// </summary>
        Task<List<ValidationResult>> ValidateBatchAsync(string tableName, List<Dictionary<string, object>> entities, string connectionName = "PPDM39");

        /// <summary>
        /// Get validation rules for a table
        /// </summary>
        Task<object> GetValidationRulesAsync(string tableName, string connectionName = "PPDM39");

        // ============================================
        // Quality Operations
        // ============================================

        /// <summary>
        /// Get data quality metrics for a table
        /// </summary>
        Task<DataQualityResult> GetTableQualityMetricsAsync(string tableName, string connectionName = "PPDM39");

        /// <summary>
        /// Get data quality dashboard
        /// </summary>
        Task<DataQualityDashboardResult> GetQualityDashboardAsync(string connectionName = "PPDM39");

        // ============================================
        // Versioning Operations
        // ============================================

        /// <summary>
        /// Create a version snapshot of an entity
        /// </summary>
        Task<VersioningResult> CreateVersionAsync(string tableName, string entityId, Dictionary<string, object>? entityData, string? versionLabel = null, string connectionName = "PPDM39");

        /// <summary>
        /// Get version history for an entity
        /// </summary>
        Task<List<VersionInfo>> GetVersionHistoryAsync(string tableName, string entityId, string connectionName = "PPDM39");

        /// <summary>
        /// Restore an entity to a specific version
        /// </summary>
        Task<VersioningResult> RestoreVersionAsync(string tableName, string entityId, string versionId, string connectionName = "PPDM39");

        // ============================================
        // Defaults Operations
        // ============================================

        /// <summary>
        /// Get default values for an entity type
        /// </summary>
        Task<Dictionary<string, object>> GetDefaultsAsync(string entityType, string connectionName = "PPDM39");

        /// <summary>
        /// Get well status facets
        /// </summary>
        Task<object> GetWellStatusFacetsAsync(string statusId, string connectionName = "PPDM39");
    }

    /// <summary>
    /// Implementation of DataManagementService
    /// </summary>
    public class DataManagementService : IDataManagementService
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<DataManagementService> _logger;
        private readonly OilGasCallFailures _calls;
        private readonly IFailureReporter _failures;
        private readonly IProgressTrackingClient? _progressTrackingClient;
        
        private string? _currentConnectionName;
        private readonly SemaphoreSlim _fieldLock = new(1, 1);
        private List<DatabaseConnectionListItem> _connections = new();
        private readonly SemaphoreSlim _refreshLock = new(1, 1);
        private DateTime _lastRefreshTime = DateTime.MinValue;
        private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(5);
        
        // Retry configuration
        private readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(1);

        public DataManagementService(
            ApiClient apiClient,
            ILogger<DataManagementService> logger,
            OilGasCallFailures calls,
            IFailureReporter failures,
            IProgressTrackingClient? progressTrackingClient = null)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _calls = calls ?? throw new ArgumentNullException(nameof(calls));
            _failures = failures ?? throw new ArgumentNullException(nameof(failures));
            _progressTrackingClient = progressTrackingClient;
        }

        public string? CurrentConnectionName => _currentConnectionName;

        public ReadOnlyCollection<DatabaseConnectionListItem> Connections => _connections.AsReadOnly();

        public event EventHandler<string?>? CurrentConnectionChanged;

        public async Task<string?> GetCurrentConnectionNameAsync()
        {
            var responseModel = await _apiClient.GetAsync<CurrentConnectionResponse>("/api/ppdm39/setup/current-connection");
            _currentConnectionName = responseModel?.ConnectionName;
            return _currentConnectionName;
        }

        public async Task<SetCurrentDatabaseResult> SetCurrentConnectionAsync(string connectionName)
        {
            SetCurrentDatabaseResult? result;
            try
            {
                var request = new SetCurrentDatabaseRequest { ConnectionName = connectionName };
                result = await _apiClient.PostAsync<SetCurrentDatabaseRequest, SetCurrentDatabaseResult>(
                    "/api/ppdm39/setup/set-current-connection", request);
            }
            // A failed call is answered as a failed result the page shows (its contract); the store keeps the failure.
            catch (Exception failure) when (OilGasCallFailures.IsCallFailure(failure))
            {
                return new SetCurrentDatabaseResult
                {
                    Success = false,
                    Message = _calls.Explain(failure, "setting the current database connection", "The current connection was not changed")
                };
            }

            if (result?.Success == true)
            {
                var oldConnection = _currentConnectionName;
                _currentConnectionName = connectionName;

                // Update connections list to reflect current status
                await RefreshAfterChangeAsync("reading the connection list again after the current connection changed");

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
            // A failed call is answered as a failed result the page shows (its contract); the store keeps the failure.
            catch (Exception failure) when (OilGasCallFailures.IsCallFailure(failure))
            {
                return new ConnectionTestResult
                {
                    Success = false,
                    Message = _calls.Explain(failure, "testing a database connection", "The connection could not be tested")
                };
            }
        }

        public async Task<SaveConnectionResult> SaveConnectionAsync(SaveConnectionRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            SaveConnectionResult? result;
            try
            {
                result = await _apiClient.PostAsync<SaveConnectionRequest, SaveConnectionResult>(
                    "/api/ppdm39/setup/save-connection",
                    request);
            }
            // A failed call is answered as a failed result the page shows (its contract); the store keeps the failure.
            catch (Exception failure) when (OilGasCallFailures.IsCallFailure(failure))
            {
                return new SaveConnectionResult
                {
                    Success = false,
                    Message = _calls.Explain(failure, "saving a database connection", "The connection was not saved")
                };
            }

            if (result?.Success == true)
            {
                await RefreshAfterChangeAsync("reading the connection list again after a connection was saved");
            }

            return result ?? new SaveConnectionResult
            {
                Success = false,
                Message = "Failed to save connection"
            };
        }

        public async Task<List<string>> GetAvailableDatabaseTypesAsync()
        {
            return await _apiClient.GetAsync<List<string>>("/api/ppdm39/setup/database-types")
                ?? new List<string>();
        }

        public async Task<List<ScriptInfo>> DiscoverScriptsAsync(string databaseType)
        {
            if (string.IsNullOrWhiteSpace(databaseType))
            {
                return new List<ScriptInfo>();
            }

            return await _apiClient.GetAsync<List<ScriptInfo>>(
                $"/api/ppdm39/setup/discover-scripts/{Uri.EscapeDataString(databaseType)}")
                ?? new List<ScriptInfo>();
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
            // A failed call is answered as a failed result the page shows (its contract); the store keeps the failure.
            catch (Exception failure) when (OilGasCallFailures.IsCallFailure(failure))
            {
                return new DatabaseCreationResult
                {
                    Success = false,
                    ErrorMessage = _calls.Explain(failure, "creating a PPDM database", "The database was not created")
                };
            }
        }

        public async Task<ScriptExecutionProgressInfo?> GetCreationProgressAsync(string executionId)
        {
            if (string.IsNullOrWhiteSpace(executionId))
            {
                return null;
            }

            return await _apiClient.GetAsync<ScriptExecutionProgressInfo>(
                $"/api/ppdm39/setup/creation-progress/{Uri.EscapeDataString(executionId)}");
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

            DeleteConnectionResult? result;
            try
            {
                result = await _apiClient.DeleteAsync<DeleteConnectionResult>(
                    $"/api/ppdm39/setup/connection/{Uri.EscapeDataString(connectionName)}");
            }
            // A failed call is answered as a failed result the page shows (its contract); the store keeps the failure.
            catch (Exception failure) when (OilGasCallFailures.IsCallFailure(failure))
            {
                return new DeleteConnectionResult
                {
                    Success = false,
                    Message = _calls.Explain(failure, "deleting a database connection", "The connection was not deleted")
                };
            }

            if (result?.Success == true)
            {
                await RefreshAfterChangeAsync("reading the connection list again after a connection was deleted");
            }

            return result ?? new DeleteConnectionResult
            {
                Success = false,
                Message = "Failed to delete connection"
            };
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
            // A failed call is answered as a failed result the page shows (its contract); the store keeps the failure.
            catch (Exception failure) when (OilGasCallFailures.IsCallFailure(failure))
            {
                return new DropDatabaseResult
                {
                    Success = false,
                    Message = _calls.Explain(failure, "dropping a PPDM database", "The database was not dropped")
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
            // A failed call is answered as a failed result the page shows (its contract); the store keeps the failure.
            catch (Exception failure) when (OilGasCallFailures.IsCallFailure(failure))
            {
                return new RecreateDatabaseResult
                {
                    Success = false,
                    Message = _calls.Explain(failure, "recreating a PPDM database", "The database was not recreated")
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
            // A failed call is answered as a failed result the page shows (its contract); the store keeps the failure.
            catch (Exception failure) when (OilGasCallFailures.IsCallFailure(failure))
            {
                return new OperationStartResponse
                {
                    Success = false,
                    OperationId = string.Empty,
                    Message = _calls.Explain(failure, "starting a PPDM database copy", "The database copy was not started")
                };
            }
        }

        public async Task<FacetSeedStatus?> GetWellStatusFacetSeedStatusAsync()
        {
            return await _apiClient.GetAsync<FacetSeedStatus>(
                "/api/ppdm39/setup/seed/well-status-facets/status");
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
            // A failed call is answered as a failed result the page shows (its contract); the store keeps the failure.
            catch (Exception failure) when (OilGasCallFailures.IsCallFailure(failure))
            {
                return new SeedingOperationResult
                {
                    Success = false,
                    Message = _calls.Explain(failure, "seeding the well-status facets", "The well-status facets were not seeded")
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
            // A failed call is answered as a failed result the page shows (its contract); the store keeps the failure.
            catch (Exception failure) when (OilGasCallFailures.IsCallFailure(failure))
            {
                return new SeedingOperationResult
                {
                    Success = false,
                    Message = _calls.Explain(failure, "seeding the enum reference data", "The enum reference data was not seeded")
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
            // A failed call is answered as a failed result the page shows (its contract); the store keeps the failure.
            catch (Exception failure) when (OilGasCallFailures.IsCallFailure(failure))
            {
                return new SeedingOperationResult
                {
                    Success = false,
                    Message = _calls.Explain(failure, "seeding all reference data", "The reference data was not seeded")
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
            // A failed call is answered as a failed result the page shows (its contract); the store keeps the failure.
            catch (Exception failure) when (OilGasCallFailures.IsCallFailure(failure))
            {
                return new GenerateDummyDataResponse
                {
                    Success = false,
                    Message = _calls.Explain(failure, "generating demo data", "The demo data was not generated"),
                    SeedOption = request.SeedOption
                };
            }
        }

        public async Task<AccessStatistics?> GetAuditStatisticsAsync(DateTime? from = null, DateTime? to = null, string? tableName = null)
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

        public async Task<List<DataAccessEvent>> GetRecentAuditEventsAsync(DateTime? from = null, DateTime? to = null)
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
            // A failed call is answered as a failed result the page shows (its contract); the store keeps the failure.
            catch (Exception failure) when (OilGasCallFailures.IsCallFailure(failure))
            {
                return new CreateSqliteResult
                {
                    Success = false,
                    Message = _calls.Explain(failure, "creating a SQLite PPDM database", "The SQLite database was not created"),
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
            // A failed call is answered as a failed result the page shows (its contract); the store keeps the failure.
            catch (Exception failure) when (OilGasCallFailures.IsCallFailure(failure))
            {
                return new CreateSchemaResult
                {
                    Success = false,
                    Message = _calls.Explain(failure, "creating the schema from its migrations", "The schema was not created")
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
            // A failed call is answered as a failed result the page shows (its contract); the store keeps the failure.
            catch (Exception failure) when (OilGasCallFailures.IsCallFailure(failure))
            {
                return new SchemaMigrationPlanResult
                {
                    Success = false,
                    ConnectionName = request.ConnectionName,
                    Message = _calls.Explain(failure, "planning a schema migration", "The schema migration was not planned")
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
            // A failed call is answered as a failed result the page shows (its contract); the store keeps the failure.
            catch (Exception failure) when (OilGasCallFailures.IsCallFailure(failure))
            {
                return new SchemaMigrationApprovalResult
                {
                    Success = false,
                    PlanId = request.PlanId,
                    Message = _calls.Explain(failure, "approving a schema migration plan", "The schema migration plan was not approved")
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
            // A failed call is answered as a failed result the page shows (its contract); the store keeps the failure.
            catch (Exception failure) when (OilGasCallFailures.IsCallFailure(failure))
            {
                return new SchemaMigrationExecuteResult
                {
                    Success = false,
                    PlanId = request.PlanId,
                    Message = _calls.Explain(failure, "executing a schema migration plan", "The schema migration was not executed")
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
            // A failed call is answered as a failed result the page shows (its contract); the store keeps the failure.
            catch (Exception failure) when (OilGasCallFailures.IsCallFailure(failure))
            {
                return new OperationStartResponse
                {
                    Success = false,
                    Message = _calls.Explain(failure, "starting a schema migration", "The schema migration was not started")
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
            // A failed call is answered as a failed result the page shows (its contract); the store keeps the failure.
            catch (Exception failure) when (OilGasCallFailures.IsCallFailure(failure))
            {
                return new SchemaMigrationProgressResult
                {
                    Success = false,
                    ExecutionToken = executionToken,
                    Message = _calls.Explain(failure, "reading a schema migration's progress", "The schema migration's progress could not be read")
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
            // A failed call is answered as a failed result the page shows (its contract); the store keeps the failure.
            catch (Exception failure) when (OilGasCallFailures.IsCallFailure(failure))
            {
                return new SchemaMigrationArtifactsResult
                {
                    Success = false,
                    PlanId = planId,
                    Message = _calls.Explain(failure, "loading a schema migration plan's artifacts", "The schema migration artifacts could not be loaded")
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
            return await _apiClient.GetAsync<ConnectionConfig>($"/api/ppdm39/setup/connection/{Uri.EscapeDataString(connectionName)}");
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
            finally
            {
                _refreshLock.Release();
            }
        }

        /// <summary>
        /// Reads the connection list again after a change the API confirmed. A failure to read it does not undo the change:
        /// it is reported, the cached list is marked stale so its next use asks the API again, and the change answers as done.
        /// </summary>
        private async Task RefreshAfterChangeAsync(string operation)
        {
            try
            {
                await RefreshConnectionsAsync();
            }
            catch (Exception failure) when (OilGasCallFailures.IsCallFailure(failure))
            {
                _lastRefreshTime = DateTime.MinValue;
                _failures.ReportHandled(
                    failure,
                    operation,
                    consequence: "the change was made; the cached connection list is stale and is read again on its next use",
                    FailureSeverity.Degraded);
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
                // A request that did not complete is tried again; the API's own answers (OilGasApiException) are not.
                catch (HttpRequestException ex) when (attempt < maxRetries - 1)
                {
                    lastException = ex;
                    attempt++;
                    var delay = TimeSpan.FromMilliseconds(_retryDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));
                    _failures.ReportHandled(
                        ex,
                        operationName,
                        consequence: $"attempt {attempt} of {maxRetries} did not complete; the request is sent again after {delay.TotalMilliseconds} ms",
                        FailureSeverity.Degraded);
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

        public async Task<GetEntitiesResponse> GetEntitiesAsync(string tableName, List<AppFilter>? filters = null, string connectionName = "PPDM39")
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
                }, $"reading the rows of {tableName}");
            }
            // A failed call is answered as a failed result the page shows (its contract); the store keeps the failure.
            catch (Exception failure) when (OilGasCallFailures.IsCallFailure(failure))
            {
                return new GetEntitiesResponse
                {
                    Success = false,
                    ErrorMessage = _calls.Explain(failure, $"reading the rows of {tableName}", $"The {tableName} rows could not be loaded")
                };
            }
        }

        public async Task<GenericEntityResponse> GetEntityByIdAsync(string tableName, object id, string connectionName = "PPDM39")
        {
            try
            {
                var url = $"/api/ppdm39/data/{tableName}/{id}";
                if (!string.IsNullOrEmpty(connectionName))
                    url += $"?connectionName={Uri.EscapeDataString(connectionName)}";
                
                return await _apiClient.GetAsync<GenericEntityResponse>(url) ?? new GenericEntityResponse { Success = false };
            }
            // A failed call is answered as a failed result the page shows (its contract); the store keeps the failure.
            catch (Exception failure) when (OilGasCallFailures.IsCallFailure(failure))
            {
                return new GenericEntityResponse
                {
                    Success = false,
                    ErrorMessage = _calls.Explain(failure, $"reading a {tableName} row", "The record could not be loaded")
                };
            }
        }

        public async Task<GenericEntityResponse> InsertEntityAsync(string tableName, Dictionary<string, object> entityData, string connectionName = "PPDM39")
        {
            // Not sent again when it does not complete (ExecuteWithRetryAsync): the request may have reached the API, and
            // sending it again would insert the row twice.
            try
            {
                var request = new GenericEntityRequest
                {
                    TableName = tableName,
                    EntityData = entityData,
                    ConnectionName = connectionName
                };
                var url = $"/api/ppdm39/data/{tableName}/insert";
                return await _apiClient.PostAsync<GenericEntityRequest, GenericEntityResponse>(url, request)
                    ?? new GenericEntityResponse { Success = false };
            }
            // A failed call is answered as a failed result the page shows (its contract); the store keeps the failure.
            catch (Exception failure) when (OilGasCallFailures.IsCallFailure(failure))
            {
                return new GenericEntityResponse
                {
                    Success = false,
                    ErrorMessage = _calls.Explain(failure, $"inserting a {tableName} row", "The record was not added")
                };
            }
        }

        public async Task<GenericEntityResponse> UpdateEntityAsync(string tableName, string entityId, Dictionary<string, object> entityData, string connectionName = "PPDM39")
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
                    var url = $"/api/ppdm39/data/{tableName}/{entityId}";
                    return await _apiClient.PutAsync<GenericEntityRequest, GenericEntityResponse>(url, request)
                        ?? new GenericEntityResponse { Success = false };
                }, $"updating a {tableName} row");
            }
            // A failed call is answered as a failed result the page shows (its contract); the store keeps the failure.
            catch (Exception failure) when (OilGasCallFailures.IsCallFailure(failure))
            {
                return new GenericEntityResponse
                {
                    Success = false,
                    ErrorMessage = _calls.Explain(failure, $"updating a {tableName} row", "The record was not updated")
                };
            }
        }

        public async Task<GenericEntityResponse> DeleteEntityAsync(string tableName, object id, string connectionName = "PPDM39")
        {
            try
            {
                var url = $"/api/ppdm39/data/{tableName}/{id}";
                if (!string.IsNullOrEmpty(connectionName))
                    url += $"?connectionName={Uri.EscapeDataString(connectionName)}";
                
                return await _apiClient.DeleteAsync<GenericEntityResponse>(url) ?? new GenericEntityResponse { Success = false };
            }
            // A failed call is answered as a failed result the page shows (its contract); the store keeps the failure.
            catch (Exception failure) when (OilGasCallFailures.IsCallFailure(failure))
            {
                return new GenericEntityResponse
                {
                    Success = false,
                    ErrorMessage = _calls.Explain(failure, $"deleting a {tableName} row", "The record was not deleted")
                };
            }
        }

        // ============================================
        // Import/Export Operations Implementation
        // ============================================

        public async Task<OperationStartResponse> ImportFromCsvAsync(string tableName, Stream csvStream, string fileName, Dictionary<string, string>? columnMapping = null, bool validateForeignKeys = true, string connectionName = "PPDM39", Action<ProgressUpdate>? onProgress = null)
        {
            try
            {
                // Convert stream to multipart form data
                using var content = new MultipartFormDataContent();
                using var streamContent = new StreamContent(csvStream);
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
                content.Add(streamContent, "file", fileName);

                var url = $"/api/ppdm39/import-export/csv/{tableName}?validateForeignKeys={validateForeignKeys}";
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
            // A failed call is answered as a failed result the page shows (its contract); the store keeps the failure.
            catch (Exception failure) when (OilGasCallFailures.IsCallFailure(failure))
            {
                return new OperationStartResponse
                {
                    OperationId = "",
                    Message = _calls.Explain(failure, $"starting a CSV import into {tableName}", "The CSV import was not started")
                };
            }
        }

        public async Task<Stream?> ExportToCsvAsync(string tableName, List<AppFilter>? filters = null, string connectionName = "PPDM39", Action<ProgressUpdate>? onProgress = null)
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

        // A validation the API could not run is not a validation that failed: the failure reaches the caller, which says so.
        public async Task<ValidationResult> ValidateEntityAsync(string tableName, Dictionary<string, object> entityData, string connectionName = "PPDM39")
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

        public async Task<List<ValidationResult>> ValidateBatchAsync(string tableName, List<Dictionary<string, object>> entities, string connectionName = "PPDM39")
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

        public async Task<object> GetValidationRulesAsync(string tableName, string connectionName = "PPDM39")
        {
            var url = $"/api/ppdm39/validation/{tableName}/rules";
            if (!string.IsNullOrEmpty(connectionName))
                url += $"?connectionName={Uri.EscapeDataString(connectionName)}";

            return await _apiClient.GetAsync<object>(url) ?? new List<object>();
        }

        // ============================================
        // Quality Operations Implementation
        // ============================================

        public async Task<DataQualityResult> GetTableQualityMetricsAsync(string tableName, string connectionName = "PPDM39")
        {
            var url = $"/api/datamanagement/quality/{tableName}/metrics";
            if (!string.IsNullOrEmpty(connectionName))
                url += $"?connectionName={Uri.EscapeDataString(connectionName)}";

            return await _apiClient.GetAsync<DataQualityResult>(url)
                ?? new DataQualityResult { TableName = tableName };
        }

        public async Task<DataQualityDashboardResult> GetQualityDashboardAsync(string connectionName = "PPDM39")
        {
            var url = "/api/datamanagement/quality/alerts";
            if (!string.IsNullOrEmpty(connectionName))
                url += $"?connectionName={Uri.EscapeDataString(connectionName)}";

            return await _apiClient.GetAsync<DataQualityDashboardResult>(url)
                ?? new DataQualityDashboardResult();
        }

        // ============================================
        // Versioning Operations Implementation
        // ============================================

        public async Task<VersioningResult> CreateVersionAsync(string tableName, string entityId, Dictionary<string, object>? entityData, string? versionLabel = null, string connectionName = "PPDM39")
        {
            try
            {
                var request = new VersioningRequest
                {
                    TableName = tableName,
                    EntityId = entityId,
                    ConnectionName = connectionName
                };
                return await _apiClient.PostAsync<VersioningRequest, VersioningResult>(
                    $"/api/ppdm39/versioning/{tableName}/{entityId}/create-version", request)
                    ?? new VersioningResult { Success = false };
            }
            // A failed call is answered as a failed result the page shows (its contract); the store keeps the failure.
            catch (Exception failure) when (OilGasCallFailures.IsCallFailure(failure))
            {
                return new VersioningResult
                {
                    Success = false,
                    ErrorMessage = _calls.Explain(failure, $"creating a version of a {tableName} row", "The version was not created")
                };
            }
        }

        public async Task<List<VersionInfo>> GetVersionHistoryAsync(string tableName, string entityId, string connectionName = "PPDM39")
        {
            var url = $"/api/ppdm39/versioning/{tableName}/{entityId}/versions";
            if (!string.IsNullOrEmpty(connectionName))
                url += $"?connectionName={Uri.EscapeDataString(connectionName)}";

            return await _apiClient.GetAsync<List<VersionInfo>>(url) ?? new List<VersionInfo>();
        }

        public async Task<VersioningResult> RestoreVersionAsync(string tableName, string entityId, string versionId, string connectionName = "PPDM39")
        {
            try
            {
                var request = new RestoreVersionRequest
                {
                    TableName = tableName,
                    EntityId = entityId,
                    VersionId = versionId,
                    ConnectionName = connectionName
                };
                return await _apiClient.PostAsync<RestoreVersionRequest, VersioningResult>(
                    $"/api/ppdm39/versioning/{tableName}/{entityId}/restore", request)
                    ?? new VersioningResult { Success = false };
            }
            // A failed call is answered as a failed result the page shows (its contract); the store keeps the failure.
            catch (Exception failure) when (OilGasCallFailures.IsCallFailure(failure))
            {
                return new VersioningResult
                {
                    Success = false,
                    ErrorMessage = _calls.Explain(failure, $"restoring a version of a {tableName} row", "The version was not restored")
                };
            }
        }

        // ============================================
        // Defaults Operations Implementation
        // ============================================

        public async Task<Dictionary<string, object>> GetDefaultsAsync(string entityType, string connectionName = "PPDM39")
        {
            var url = $"/api/ppdm39/defaults/{entityType}";
            if (!string.IsNullOrEmpty(connectionName))
                url += $"?connectionName={Uri.EscapeDataString(connectionName)}";

            return await _apiClient.GetAsync<Dictionary<string, object>>(url) ?? new Dictionary<string, object>();
        }

        public async Task<object> GetWellStatusFacetsAsync(string statusId, string connectionName = "PPDM39")
        {
            var url = $"/api/ppdm39/defaults/well-status/{statusId}/facets";
            if (!string.IsNullOrEmpty(connectionName))
                url += $"?connectionName={Uri.EscapeDataString(connectionName)}";

            return await _apiClient.GetAsync<object>(url) ?? new List<object>();
        }

        // ============================================
        // Field Management Implementation
        // ============================================

        public event Action<string>? CurrentFieldChanged;

        public async Task<string?> GetCurrentFieldIdAsync()
        {
            await _fieldLock.WaitAsync();
            try
            {
                var response = await _apiClient.GetAsync<FieldResponse>("/api/field/current")
                    ?? throw new InvalidOperationException("Current field response was empty.");
                return string.IsNullOrWhiteSpace(response.FieldId) ? null : response.FieldId;
            }
            // This endpoint defines 404 as no active field, not a failure: answered as no selection, and recorded.
            catch (OilGasApiException noField) when (noField.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _failures.ReportHandled(
                    noField,
                    "reading the active field",
                    consequence: "the API holds no active field for this person; the caller was answered with no selection",
                    FailureSeverity.Degraded);
                return null;
            }
            finally { _fieldLock.Release(); }
        }

        public async Task<bool> SetCurrentFieldAsync(string fieldId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fieldId);
            bool confirmed = false;
            bool uncertain = false;
            await _fieldLock.WaitAsync();
            try
            {
                var response = await _apiClient.PostAsync<SetActiveFieldRequest, SetActiveFieldResponse>(
                    "/api/field/set-active", new SetActiveFieldRequest { FieldId = fieldId })
                    ?? throw new InvalidOperationException("Field selection was not confirmed.");
                if (!response.Success) return false;
                if (response.FieldId != fieldId) throw new InvalidOperationException("The confirmed field differs from the requested field.");
                confirmed = true;
                return true;
            }
            catch { uncertain = true; throw; }
            finally
            {
                _fieldLock.Release();
                if (confirmed || uncertain) PublishFieldChange(confirmed ? fieldId : string.Empty);
            }
        }

        private void PublishFieldChange(string fieldId)
        {
            foreach (var handler in CurrentFieldChanged?.GetInvocationList() ?? Array.Empty<Delegate>())
            {
                try { ((Action<string>)handler)(fieldId); }
                // One subscriber's failure must not keep the others from hearing the change, so each is reported in turn.
                catch (Exception ex)
                {
                    _failures.ReportHandled(
                        ex,
                        "telling a subscriber that the active field changed",
                        consequence: "that subscriber shows the previous field until it is refreshed; the change itself stands");
                }
            }
        }
    }
}
