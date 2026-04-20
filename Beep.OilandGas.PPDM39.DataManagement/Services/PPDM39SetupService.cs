using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.SeedData;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using BeepDataSourceType = TheTechIdea.Beep.Utilities.DataSourceType;

namespace Beep.OilandGas.PPDM39.DataManagement.Services
{
    public class PPDM39SetupService : IPPDM39SetupService
    {
        private readonly IDMEEditor _editor;
        private readonly ILogger<PPDM39SetupService> _logger;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly LOVManagementService? _lovService;

        private IProgressTrackingService? _progressTracking;
        private string? _currentConnectionName;

        // Static fallback driver map — used when BeepDM has not yet loaded the driver assembly
        private static readonly Dictionary<string, DatabaseDriverInfo> _staticDriverMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["SqlServer"]  = new DatabaseDriverInfo { NuGetPackage = "TheTechIdea.Beep.Winform.Net8.SQLServerDataSource", DataSourceType = "SqlServer", DefaultPort = 1433, ScriptPath = "Scripts/SqlServer" },
                ["Postgre"]    = new DatabaseDriverInfo { NuGetPackage = "TheTechIdea.Beep.Winform.Net8.PostgreDataSource",  DataSourceType = "Postgre",   DefaultPort = 5432, ScriptPath = "Scripts/PostgreSQL" },
                ["PostgreSQL"] = new DatabaseDriverInfo { NuGetPackage = "TheTechIdea.Beep.Winform.Net8.PostgreDataSource",  DataSourceType = "Postgre",   DefaultPort = 5432, ScriptPath = "Scripts/PostgreSQL" },
                ["Mysql"]      = new DatabaseDriverInfo { NuGetPackage = "TheTechIdea.Beep.Winform.Net8.MySqlDataSource",    DataSourceType = "Mysql",     DefaultPort = 3306, ScriptPath = "Scripts/MySQL" },
                ["MariaDB"]    = new DatabaseDriverInfo { NuGetPackage = "TheTechIdea.Beep.Winform.Net8.MySqlDataSource",    DataSourceType = "Mysql",     DefaultPort = 3306, ScriptPath = "Scripts/MySQL" },
                ["Oracle"]     = new DatabaseDriverInfo { NuGetPackage = "TheTechIdea.Beep.Winform.Net8.OracleDataSource",   DataSourceType = "Oracle",    DefaultPort = 1521, ScriptPath = "Scripts/Oracle" },
                ["SqlLite"]    = new DatabaseDriverInfo { NuGetPackage = "TheTechIdea.Beep.Winform.Net8.SQLiteDataSource",   DataSourceType = "SqlLite",   DefaultPort = 0,    ScriptPath = "Scripts/SQLite" },
                ["SQLite"]     = new DatabaseDriverInfo { NuGetPackage = "TheTechIdea.Beep.Winform.Net8.SQLiteDataSource",   DataSourceType = "SqlLite",   DefaultPort = 0,    ScriptPath = "Scripts/SQLite" },
            };

        public PPDM39SetupService(
            IDMEEditor editor,
            ILogger<PPDM39SetupService> logger,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            LOVManagementService? lovService = null)
        {
            _editor              = editor              ?? throw new ArgumentNullException(nameof(editor));
            _logger              = logger              ?? throw new ArgumentNullException(nameof(logger));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults            = defaults            ?? throw new ArgumentNullException(nameof(defaults));
            _metadata            = metadata            ?? throw new ArgumentNullException(nameof(metadata));
            _lovService          = lovService;
        }

        // ── PROGRESS TRACKING ─────────────────────────────────────────────────

        public void SetProgressTracking(IProgressTrackingService progressTracking)
            => _progressTracking = progressTracking;

        // ── DRIVER INFO ───────────────────────────────────────────────────────

        public DatabaseDriverInfo? GetDriverInfo(string databaseType)
        {
            if (string.IsNullOrWhiteSpace(databaseType)) return null;

            // Try live BeepDM driver classes first
            if (Enum.TryParse<BeepDataSourceType>(databaseType, true, out var dsType))
            {
                var driverClass = _editor.ConfigEditor?.DataDriversClasses?
                    .FirstOrDefault(d => d.DatasourceType == dsType);
                if (driverClass != null)
                    return new DatabaseDriverInfo
                    {
                        NuGetPackage   = driverClass.PackageName,
                        DataSourceType = driverClass.DatasourceType.ToString(),
                        DefaultPort    = GetDefaultPort(dsType),
                        ScriptPath     = $"Scripts/{MapToScriptFolder(dsType)}"
                    };
            }

            // Fallback to static map
            return _staticDriverMap.TryGetValue(databaseType, out var info) ? info : null;
        }

        public List<string> GetAvailableDatabaseTypes()
        {
            var fromDrivers = _editor.ConfigEditor?.DataDriversClasses?
                .Select(d => d.DatasourceType.ToString())
                .Distinct()
                .ToList();

            return fromDrivers?.Count > 0
                ? fromDrivers
                : _staticDriverMap.Keys.Distinct().OrderBy(k => k).ToList();
        }

        public DriverInfo CheckDriver(string databaseType)
        {
            var info = GetDriverInfo(databaseType);
            if (info == null)
                return new DriverInfo { DatabaseType = databaseType, IsAvailable = false, IsInstalled = false };

            // Check if driver assembly is loaded in BeepDM
            var isInstalled = _editor.ConfigEditor?.DataDriversClasses?
                .Any(d => string.Equals(d.PackageName, info.NuGetPackage, StringComparison.OrdinalIgnoreCase)) ?? false;

            return new DriverInfo
            {
                DatabaseType = databaseType,
                NuGetPackage = info.NuGetPackage,
                IsAvailable  = true,
                IsInstalled  = isInstalled
            };
        }

        // ── CONNECTION TESTING ────────────────────────────────────────────────

        public async Task<ConnectionTestResult> TestConnectionAsync(ConnectionConfig config)
        {
            if (config == null)
                return new ConnectionTestResult { Success = false, Message = "Connection configuration is required" };

            _logger.LogInformation("Testing connection to {Host} ({DatabaseType})", config.Host, config.DatabaseType);
            try
            {
                // Register a temporary connection under its name (or a temp name)
                var connName = string.IsNullOrWhiteSpace(config.ConnectionName)
                    ? $"__test_{Guid.NewGuid():N}"
                    : config.ConnectionName;

                var props = BuildConnectionProperties(config, connName);
                var isTempConn = false;

                if (!_editor.ConfigEditor.DataConnectionExist(connName))
                {
                    _editor.ConfigEditor.AddDataConnection(props);
                    isTempConn = true;
                }

                var ds = _editor.GetDataSource(connName);
                if (ds == null)
                {
                    if (isTempConn) RemoveConnection(connName);
                    return new ConnectionTestResult { Success = false, Message = $"Could not create data source for type '{config.DatabaseType}'" };
                }

                var state = ds.Openconnection();
                ds.Closeconnection();

                if (isTempConn) RemoveConnection(connName);

                return state == ConnectionState.Open
                    ? new ConnectionTestResult { Success = true,  Message = "Connection successful" }
                    : new ConnectionTestResult { Success = false, Message = "Connection failed — could not open database" };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Connection test failed for {ConnectionName}", config.ConnectionName);
                return new ConnectionTestResult { Success = false, Message = "Connection test failed", ErrorDetails = ex.Message };
            }
        }

        // ── SAVE / UPDATE / DELETE CONNECTIONS ───────────────────────────────

        public SaveConnectionResult SaveConnection(ConnectionConfig config, bool testAfterSave = true, bool openAfterSave = false)
        {
            if (config == null)
                return new SaveConnectionResult { Success = false, Message = "Connection configuration is required" };

            if (string.IsNullOrWhiteSpace(config.ConnectionName))
                return new SaveConnectionResult { Success = false, Message = "Connection name is required" };

            _logger.LogInformation("Saving connection {ConnectionName}", config.ConnectionName);
            try
            {
                var props = BuildConnectionProperties(config, config.ConnectionName);

                if (_editor.ConfigEditor.DataConnectionExist(config.ConnectionName))
                    RemoveConnection(config.ConnectionName);

                _editor.ConfigEditor.AddDataConnection(props);
                _editor.ConfigEditor.SaveDataconnectionsValues();

                if (testAfterSave)
                {
                    var ds = _editor.GetDataSource(config.ConnectionName);
                    if (ds != null)
                    {
                        var state = ds.Openconnection();
                        if (!openAfterSave) ds.Closeconnection();
                        if (state != ConnectionState.Open)
                            return new SaveConnectionResult { Success = false, ConnectionName = config.ConnectionName, Message = "Connection saved but test failed — could not open database" };
                    }
                }

                _currentConnectionName = config.ConnectionName;
                return new SaveConnectionResult { Success = true, ConnectionName = config.ConnectionName, Message = "Connection saved successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save connection {ConnectionName}", config.ConnectionName);
                return new SaveConnectionResult { Success = false, Message = "Failed to save connection", ErrorDetails = ex.Message };
            }
        }

        public SaveConnectionResult UpdateConnection(string originalConnectionName, ConnectionConfig config, bool testAfterSave = true)
        {
            if (string.IsNullOrWhiteSpace(originalConnectionName) || config == null)
                return new SaveConnectionResult { Success = false, Message = "Original connection name and configuration are required" };

            _logger.LogInformation("Updating connection {OriginalName} → {NewName}", originalConnectionName, config.ConnectionName);
            try
            {
                if (_editor.ConfigEditor.DataConnectionExist(originalConnectionName))
                    RemoveConnection(originalConnectionName);

                return SaveConnection(config, testAfterSave);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update connection {ConnectionName}", originalConnectionName);
                return new SaveConnectionResult { Success = false, Message = "Failed to update connection", ErrorDetails = ex.Message };
            }
        }

        public DeleteConnectionResult DeleteConnection(string connectionName)
        {
            if (string.IsNullOrWhiteSpace(connectionName))
                return new DeleteConnectionResult { Success = false, Message = "Connection name is required" };

            _logger.LogInformation("Deleting connection {ConnectionName}", connectionName);
            try
            {
                if (!_editor.ConfigEditor.DataConnectionExist(connectionName))
                    return new DeleteConnectionResult { Success = false, Message = $"Connection '{connectionName}' not found" };

                RemoveConnection(connectionName);
                _editor.ConfigEditor.SaveDataconnectionsValues();

                if (string.Equals(_currentConnectionName, connectionName, StringComparison.OrdinalIgnoreCase))
                    _currentConnectionName = null;

                return new DeleteConnectionResult { Success = true, Message = "Connection deleted" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete connection {ConnectionName}", connectionName);
                return new DeleteConnectionResult { Success = false, Message = "Failed to delete connection", ErrorDetails = ex.Message };
            }
        }

        // ── CONNECTION LISTING ────────────────────────────────────────────────

        public List<DatabaseConnectionListItem> GetAllConnections()
        {
            return (_editor.ConfigEditor?.DataConnections ?? Enumerable.Empty<ConnectionProperties>())
                .Select(dc => new DatabaseConnectionListItem
                {
                    ConnectionName = dc.ConnectionName,
                    DatabaseType   = dc.DatabaseType.ToString(),
                    Host           = dc.Host,
                    Database       = dc.Database,
                    IsCurrent      = string.Equals(dc.ConnectionName, _currentConnectionName, StringComparison.OrdinalIgnoreCase)
                }).ToList();
        }

        public ConnectionConfig? GetConnectionByName(string connectionName)
        {
            var dc = (_editor.ConfigEditor?.DataConnections ?? Enumerable.Empty<ConnectionProperties>())
                .FirstOrDefault(c => string.Equals(c.ConnectionName, connectionName, StringComparison.OrdinalIgnoreCase));

            if (dc == null) return null;

            return new ConnectionConfig
            {
                ConnectionName = dc.ConnectionName,
                DatabaseType   = dc.DatabaseType.ToString(),
                Host           = dc.Host,
                Port           = dc.Port,
                Database       = dc.Database,
                Username       = dc.UserID,
                ConnectionString = dc.ConnectionString
            };
        }

        public string? GetCurrentConnectionName() => _currentConnectionName;

        public SetCurrentDatabaseResult SetCurrentConnection(string connectionName)
        {
            if (string.IsNullOrWhiteSpace(connectionName))
                return new SetCurrentDatabaseResult { Success = false, Message = "Connection name is required" };

            if (!_editor.ConfigEditor.DataConnectionExist(connectionName))
                return new SetCurrentDatabaseResult { Success = false, Message = $"Connection '{connectionName}' not found" };

            _currentConnectionName = connectionName;
            return new SetCurrentDatabaseResult { Success = true, Message = $"Current connection set to '{connectionName}'" };
        }

        // ── SCHEMA CREATION ───────────────────────────────────────────────────

        public async Task<CreateSchemaResult> CreateSchemaAsync(ConnectionConfig config, string schemaName)
        {
            var connectionName = config?.ConnectionName ?? string.Empty;
            if (string.IsNullOrWhiteSpace(connectionName))
                return new CreateSchemaResult { Success = false, Message = "Connection name is required. Save the connection first." };

            _logger.LogInformation("Creating PPDM39 schema in {ConnectionName}", connectionName);
            try
            {
                var ds = _editor.GetDataSource(connectionName);
                if (ds == null)
                    return new CreateSchemaResult { Success = false, Message = $"Connection '{connectionName}' not found. Save the connection first." };

                var state = ds.Openconnection();
                if (state != ConnectionState.Open)
                    return new CreateSchemaResult { Success = false, Message = $"Could not open connection to '{connectionName}'" };

                var migration = new TheTechIdea.Beep.Editor.Migration.MigrationManager(_editor, ds);
                migration.RegisterAssembly(typeof(Beep.OilandGas.PPDM39.Models.WELL).Assembly);

                var plan = migration.BuildMigrationPlan(
                    namespaceName: "Beep.OilandGas.PPDM39.Models",
                    assembly: typeof(Beep.OilandGas.PPDM39.Models.WELL).Assembly,
                    detectRelationships: true);

                if (plan == null)
                    return new CreateSchemaResult { Success = false, Message = "Migration plan could not be built." };

                var execResult = migration.ExecuteMigrationPlan(plan);

                _logger.LogInformation(
                    "Schema migration for '{Connection}': success={Success}",
                    connectionName, execResult.Success);

                return new CreateSchemaResult
                {
                    Success      = execResult.Success,
                    Message      = execResult.Success
                        ? $"Schema created successfully in '{connectionName}'"
                        : "Schema creation partially failed: " + execResult.Message,
                    ErrorDetails = execResult.Success ? null : execResult.Message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Schema creation failed for {ConnectionName}", connectionName);
                return new CreateSchemaResult { Success = false, Message = "Schema creation error", ErrorDetails = ex.Message };
            }
        }

        public async Task<SchemaPrivilegeCheckResult> CheckSchemaPrivilegesAsync(ConnectionConfig config, string? schemaName = null)
        {
            var connectionName = config?.ConnectionName ?? string.Empty;
            _logger.LogInformation("Checking schema privileges for {ConnectionName}", connectionName);
            try
            {
                var ds = _editor.GetDataSource(connectionName);
                if (ds == null)
                    return new SchemaPrivilegeCheckResult { HasCreatePrivilege = false, Message = $"Connection '{connectionName}' not found" };

                var state = ds.Openconnection();
                if (state != ConnectionState.Open)
                    return new SchemaPrivilegeCheckResult { HasCreatePrivilege = false, Message = "Could not open connection" };

                // Attempt a harmless DDL to verify CREATE TABLE privilege
                try
                {
                    var tempTable = $"__PRIV_CHECK_{Guid.NewGuid():N8}";
                    ds.ExecuteSql($"CREATE TABLE {tempTable} (ID INT); DROP TABLE {tempTable};");
                    return new SchemaPrivilegeCheckResult { HasCreatePrivilege = true, Message = "Schema creation privileges confirmed" };
                }
                catch
                {
                    return new SchemaPrivilegeCheckResult { HasCreatePrivilege = false, Message = "Insufficient privileges to create tables" };
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Privilege check failed for {ConnectionName}", connectionName);
                return new SchemaPrivilegeCheckResult { HasCreatePrivilege = false, Message = "Privilege check failed", ErrorDetails = ex.Message };
            }
        }

        // ── SCRIPTS ───────────────────────────────────────────────────────────

        public List<ScriptInfo> GetAvailableScripts(string databaseType)
        {
            // Scripts are managed via ModuleDataRegistry in the DataManager project.
            // Return empty; use the discover-scripts endpoint on the setup controller for full listing.
            return new List<ScriptInfo>();
        }

        public Task<List<ScriptInfo>> GetAvailableScriptsAsync(string databaseType)
            => Task.FromResult(GetAvailableScripts(databaseType));

        public async Task<ScriptExecutionResult> ExecuteScriptAsync(ConnectionConfig config, string scriptName, string? operationId = null)
        {
            _logger.LogInformation("ExecuteScript '{Script}' on {ConnectionName}", scriptName, config?.ConnectionName);
            return new ScriptExecutionResult
            {
                ScriptFileName = scriptName,
                Success        = false,
                ErrorMessage   = "Script execution via setup service is not supported. Use the create-database endpoint with ModuleDataRegistry."
            };
        }

        public async Task<AllScriptsExecutionResult> ExecuteAllScriptsAsync(ConnectionConfig config, List<string> scriptNames, string? operationId = null)
        {
            var result = new AllScriptsExecutionResult
            {
                ExecutionId  = operationId ?? Guid.NewGuid().ToString(),
                StartTime    = DateTime.UtcNow,
                TotalScripts = scriptNames?.Count ?? 0,
                Success      = false,
                ErrorMessage = "Batch script execution is not supported via setup service. Use the create-database endpoint."
            };
            result.EndTime = DateTime.UtcNow;
            return result;
        }

        // ── DATABASE OPERATIONS ───────────────────────────────────────────────

        public async Task<DropDatabaseResult> DropDatabaseAsync(string connectionName, string? schemaName, bool dropIfExists = true)
        {
            _logger.LogWarning("DropDatabase requested for {ConnectionName} / schema={Schema}", connectionName, schemaName);
            try
            {
                var ds = _editor.GetDataSource(connectionName);
                if (ds == null)
                    return new DropDatabaseResult { Success = false, Message = $"Connection '{connectionName}' not found" };

                var state = ds.Openconnection();
                if (state != ConnectionState.Open)
                    return new DropDatabaseResult { Success = false, Message = "Could not open connection" };

                var ddl = string.IsNullOrWhiteSpace(schemaName)
                    ? (dropIfExists ? "DROP DATABASE IF EXISTS PPDM39" : "DROP DATABASE PPDM39")
                    : (dropIfExists ? $"DROP SCHEMA IF EXISTS {schemaName}" : $"DROP SCHEMA {schemaName}");

                ds.ExecuteSql(ddl);
                return new DropDatabaseResult { Success = true, Message = "Database dropped" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DropDatabase failed for {ConnectionName}", connectionName);
                return new DropDatabaseResult { Success = false, Message = "Drop failed", ErrorDetails = ex.Message };
            }
        }

        public async Task<RecreateDatabaseResult> RecreateDatabaseAsync(string connectionName, string? schemaName, bool backupFirst = false, string? backupPath = null)
        {
            _logger.LogInformation("RecreateDatabase for {ConnectionName}", connectionName);
            var dropResult = await DropDatabaseAsync(connectionName, schemaName, dropIfExists: true);
            if (!dropResult.Success)
                return new RecreateDatabaseResult { Success = false, Message = "Drop step failed: " + dropResult.Message };

            var connConfig = GetConnectionByName(connectionName);
            if (connConfig == null)
                return new RecreateDatabaseResult { Success = false, Message = $"Connection '{connectionName}' not found after drop" };

            var createResult = await CreateSchemaAsync(connConfig, schemaName ?? string.Empty);
            return new RecreateDatabaseResult
            {
                Success = createResult.Success,
                Message = createResult.Success ? "Database recreated successfully" : "Recreate failed: " + createResult.Message,
                ErrorDetails = createResult.ErrorDetails
            };
        }

        public async Task<CopyDatabaseResult> CopyDatabaseAsync(CopyDatabaseRequest request, string? operationId = null)
        {
            _logger.LogInformation("CopyDatabase from {Source} to {Target}", request?.SourceConnectionName, request?.TargetConnectionName);
            return new CopyDatabaseResult
            {
                Success = false,
                Message = "Database copy is not yet implemented via the setup service. Use the DataManagement ETL sync endpoint."
            };
        }

        // ── PRIVATE HELPERS ───────────────────────────────────────────────────

        private ConnectionProperties BuildConnectionProperties(ConnectionConfig config, string connectionName)
        {
            var dsType = ParseDataSourceType(config.DatabaseType);
            var driverClass = _editor.ConfigEditor?.DataDriversClasses?
                .FirstOrDefault(d => d.DatasourceType == dsType);

            return new ConnectionProperties
            {
                ConnectionName   = connectionName,
                DatabaseType     = dsType,
                DriverName       = driverClass?.PackageName ?? string.Empty,
                Host             = config.Host ?? string.Empty,
                Port             = config.Port,
                Database         = config.Database ?? string.Empty,
                UserID           = config.Username ?? string.Empty,
                Password         = config.Password ?? string.Empty,
                ConnectionString = config.ConnectionString ?? string.Empty,
                GuidID           = Guid.NewGuid().ToString()
            };
        }

        private void RemoveConnection(string connectionName)
        {
            var existing = _editor.ConfigEditor?.DataConnections?
                .FirstOrDefault(c => string.Equals(c.ConnectionName, connectionName, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
                _editor.ConfigEditor.DataConnections.Remove(existing);
        }

        private static BeepDataSourceType ParseDataSourceType(string? databaseType)
        {
            if (string.IsNullOrWhiteSpace(databaseType))
                return BeepDataSourceType.SqlServer;

            if (Enum.TryParse<BeepDataSourceType>(databaseType, true, out var result))
                return result;

            return databaseType.ToLowerInvariant() switch
            {
                "sqlserver"              => BeepDataSourceType.SqlServer,
                "postgre" or "postgresql" => BeepDataSourceType.Postgre,
                "mysql" or "mariadb"     => BeepDataSourceType.Mysql,
                "oracle"                 => BeepDataSourceType.Oracle,
                "sqlite" or "sqllite"    => BeepDataSourceType.SqlLite,
                _                        => BeepDataSourceType.SqlServer
            };
        }

        private static int GetDefaultPort(BeepDataSourceType dsType) => dsType switch
        {
            BeepDataSourceType.SqlServer => 1433,
            BeepDataSourceType.Postgre   => 5432,
            BeepDataSourceType.Mysql     => 3306,
            BeepDataSourceType.Oracle    => 1521,
            _                            => 0
        };

        private static string MapToScriptFolder(BeepDataSourceType dsType) => dsType switch
        {
            BeepDataSourceType.SqlServer => "SqlServer",
            BeepDataSourceType.Postgre   => "PostgreSQL",
            BeepDataSourceType.Mysql     => "MySQL",
            BeepDataSourceType.Oracle    => "Oracle",
            BeepDataSourceType.SqlLite   => "SQLite",
            _                            => "SqlServer"
        };

        // ── Reference Data Seeding ─────────────────────────────────────────────

        /// <inheritdoc />
        public async Task<SeedingOperationResult> SeedWellStatusFacetsAsync(
            string connectionName, string userId = "SYSTEM", string? operationId = null)
        {
            _logger.LogInformation("Seeding WSC v3 well-status facets into {Connection}", connectionName);
            try
            {
                var seeder = new WellStatusFacetSeeder(_editor, _commonColumnHandler, _defaults, _metadata, connectionName);
                var result = await seeder.SeedAllAsync(userId);
                _logger.LogInformation(
                    "Well-status facet seed complete: {Total} rows inserted into {Connection}",
                    result.TotalInserted, connectionName);

                return new SeedingOperationResult
                {
                    Success       = result.Success,
                    Message       = result.Message,
                    TotalInserted = result.TotalInserted,
                    Details = new List<string>
                    {
                        $"R_WELL_STATUS_TYPE:       {result.FacetTypeRows} rows",
                        $"R_WELL_STATUS:            {result.FacetValueRows} rows",
                        $"R_WELL_STATUS_QUAL:       {result.FacetQualifierRows} rows",
                        $"R_WELL_STATUS_QUAL_VALUE: {result.FacetQualValueRows} rows",
                        $"RA_WELL_STATUS_TYPE:      {result.RaFacetTypeRows} rows",
                        $"RA_WELL_STATUS:           {result.RaFacetValueRows} rows",
                    },
                    Errors = result.Errors
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Well-status facet seeding failed for {Connection}", connectionName);
                return new SeedingOperationResult
                {
                    Success = false,
                    Message = $"Seeding failed: {ex.Message}",
                    Errors  = new List<string> { ex.ToString() }
                };
            }
        }

        /// <inheritdoc />
        public async Task<SeedingOperationResult> SeedEnumReferenceDataAsync(
            string connectionName, string userId = "SYSTEM", string? operationId = null)
        {
            _logger.LogInformation("Seeding enum reference data into {Connection}", connectionName);
            if (_lovService == null)
            {
                return new SeedingOperationResult
                {
                    Success = false,
                    Message = "LOVManagementService is not available. Ensure it is registered in DI.",
                    Errors  = new List<string> { "LOVManagementService is null" }
                };
            }

            try
            {
                var seeder = new EnumReferenceDataSeeder(
                    _editor, _commonColumnHandler, _defaults, _metadata, _lovService, connectionName);
                var count = await seeder.SeedAllEnumsAsync(userId);

                _logger.LogInformation("Enum reference data seeded: {Count} rows into {Connection}", count, connectionName);
                return new SeedingOperationResult
                {
                    Success       = true,
                    Message       = $"Seeded {count} enum reference rows.",
                    TotalInserted = count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Enum reference data seeding failed for {Connection}", connectionName);
                return new SeedingOperationResult
                {
                    Success = false,
                    Message = $"Seeding failed: {ex.Message}",
                    Errors  = new List<string> { ex.ToString() }
                };
            }
        }

        /// <inheritdoc />
        public async Task<SeedingOperationResult> SeedAllReferenceDataAsync(
            string connectionName, string userId = "SYSTEM", string? operationId = null)
        {
            _logger.LogInformation("Seeding all reference data into {Connection}", connectionName);
            var aggregate = new SeedingOperationResult();

            // 1. WSC v3 facets
            var facetsResult = await SeedWellStatusFacetsAsync(connectionName, userId, operationId);
            aggregate.TotalInserted += facetsResult.TotalInserted;
            aggregate.Details.AddRange(facetsResult.Details);
            aggregate.Errors.AddRange(facetsResult.Errors);

            // 2. Enum reference data
            var enumsResult = await SeedEnumReferenceDataAsync(connectionName, userId, operationId);
            aggregate.TotalInserted += enumsResult.TotalInserted;
            aggregate.Details.AddRange(enumsResult.Details);
            aggregate.Errors.AddRange(enumsResult.Errors);

            aggregate.Success = facetsResult.Success && enumsResult.Success;
            aggregate.Message = aggregate.Success
                ? $"All reference data seeded: {aggregate.TotalInserted} total rows."
                : $"Partial failure. {aggregate.TotalInserted} rows inserted; {aggregate.Errors.Count} error(s).";

            _logger.LogInformation(
                "All reference data seeding complete: {Total} rows, success={Success}",
                aggregate.TotalInserted, aggregate.Success);

            return aggregate;
        }
    }
}
