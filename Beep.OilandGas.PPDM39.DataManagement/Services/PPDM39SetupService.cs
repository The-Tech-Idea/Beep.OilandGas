using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation;
using Beep.OilandGas.Models.DTOs.DataManagement;
using Beep.OilandGas.DataManager.Core.Interfaces;
using Beep.OilandGas.DataManager.Core.Models;
using Beep.OilandGas.DataManager.Core.Registry;
using Beep.OilandGas.DataManager.Core.Utilities;
using Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation;

namespace Beep.OilandGas.PPDM39.DataManagement.Services
{
    /// <summary>
    /// Service for PPDM39 database setup operations
    /// </summary>
    public class PPDM39SetupService : IPPDM39SetupService
    {
        private readonly IDMEEditor _editor;
        private readonly ILogger<PPDM39SetupService> _logger;
        private readonly IDataManager _dataManager;
        private IProgressTrackingService? _progressTracking;
        private static string? _currentConnectionName; // In-memory storage for current connection name

        public PPDM39SetupService(
            IDMEEditor editor, 
            ILogger<PPDM39SetupService> logger,
            IDataManager dataManager)
        {
            _editor = editor;
            _logger = logger;
            _dataManager = dataManager ?? throw new ArgumentNullException(nameof(dataManager));
        }

        /// <summary>
        /// Set progress tracking service (called via controller to avoid circular dependency)
        /// </summary>
        public void SetProgressTracking(IProgressTrackingService progressTracking)
        {
            _progressTracking = progressTracking;
        }

        /// <summary>
        /// Helper method to convert string to DataSourceType enum
        /// </summary>
        private static DataSourceType ParseDataSourceType(string dataSourceTypeString)
        {
            if (string.IsNullOrEmpty(dataSourceTypeString))
                return DataSourceType.SqlServer; // Default

            // Try parsing the enum directly
            if (Enum.TryParse<DataSourceType>(dataSourceTypeString, true, out var result))
                return result;

            // Map string values to enum values (case-insensitive)
            return dataSourceTypeString.ToLowerInvariant() switch
            {
                "sqlserver" => DataSourceType.SqlServer,
                "postgre" or "postgresql" => DataSourceType.Postgre,
                "mysql" or "mariadb" => DataSourceType.Mysql,
                "oracle" => DataSourceType.Oracle,
                "sqlite" => DataSourceType.SqlServer, // Sqlite may not be in enum, use SqlServer as fallback
                _ => DataSourceType.SqlServer // Default fallback
            };
        }

        // Database type to driver mapping
        private static readonly Dictionary<string, DatabaseDriverInfo> DatabaseDrivers = new()
        {
            { "SqlServer", new DatabaseDriverInfo 
                { 
                    NuGetPackage = "TheTechIdea.Beep.SqlServerDataSourceCore",
                    DataSourceType = "SqlServer",
                    DefaultPort = 1433,
                    ScriptPath = "Sqlserver",
                    DisplayName = "SQL Server"
                }
            },
            { "PostgreSQL", new DatabaseDriverInfo 
                { 
                    NuGetPackage = "TheTechIdea.Beep.PostgreDataSourceCore",
                    DataSourceType = "Postgre",
                    DefaultPort = 5432,
                    ScriptPath = "PostgreSQL",
                    DisplayName = "PostgreSQL"
                }
            },
            { "MySQL", new DatabaseDriverInfo 
                { 
                    NuGetPackage = "TheTechIdea.Beep.MySqlDataSourceCore",
                    DataSourceType = "Mysql",
                    DefaultPort = 3306,
                    ScriptPath = "MariaDB", // Use MariaDB scripts for MySQL
                    DisplayName = "MySQL"
                }
            },
            { "MariaDB", new DatabaseDriverInfo 
                { 
                    NuGetPackage = "TheTechIdea.Beep.MySqlDataSourceCore",
                    DataSourceType = "Mysql",
                    DefaultPort = 3306,
                    ScriptPath = "MariaDB",
                    DisplayName = "MariaDB"
                }
            },
            { "Oracle", new DatabaseDriverInfo 
                { 
                    NuGetPackage = "TheTechIdea.Beep.OracleDataSourceCore",
                    DataSourceType = "Oracle",
                    DefaultPort = 1521,
                    ScriptPath = "Oracle",
                    DisplayName = "Oracle"
                }
            },
            { "SQLite", new DatabaseDriverInfo 
                { 
                    NuGetPackage = "TheTechIdea.Beep.SqliteDatasourceCore",
                    DataSourceType = "Sqlite",
                    DefaultPort = 0, // SQLite doesn't use ports
                    ScriptPath = "SQLite",
                    DisplayName = "SQLite"
                }
            }
        };

        // Script execution order and metadata (legacy - not currently used, kept for reference)
        // Script metadata is now handled by ModuleScriptInfo from DataManager

        /// <summary>
        /// Get database driver information
        /// </summary>
        public DatabaseDriverInfo? GetDriverInfo(string databaseType)
        {
            if (DatabaseDrivers.TryGetValue(databaseType, out var driverInfo))
            {
                return driverInfo;
            }
            return null;
        }

        /// <summary>
        /// Get all available database types
        /// </summary>
        public List<string> GetAvailableDatabaseTypes()
        {
            return DatabaseDrivers.Keys.OrderBy(k => k).ToList();
        }

        /// <summary>
        /// Check if RemoveDataConnection method exists (extension method check)
        /// </summary>
        private bool RemoveDataConnection(string connectionName)
        {
            // Try to remove from the collection directly
            var connections = _editor.ConfigEditor?.DataConnections;
            if (connections == null)
                return false;

            var connectionToRemove = connections.FirstOrDefault(c => c.ConnectionName == connectionName);
            if (connectionToRemove == null)
                return false;

            connections.Remove(connectionToRemove);
            return true;
        }

        /// <summary>
        /// Check if driver is available
        /// </summary>
        public DriverInfo CheckDriver(string databaseType)
        {
            var driverInfo = GetDriverInfo(databaseType);
            if (driverInfo == null)
            {
                return new DriverInfo
                {
                    DatabaseType = databaseType,
                    IsAvailable = false,
                    IsInstalled = false,
                    StatusMessage = $"Unknown database type: {databaseType}"
                };
            }

            var driver = _editor.ConfigEditor?.DataDriversClasses?
                .FirstOrDefault(d => d.DatasourceType.ToString() == driverInfo.DataSourceType);

            bool isInstalled = driver != null && !driver.NuggetMissing;

            return new DriverInfo
            {
                DatabaseType = databaseType,
                NuGetPackage = driverInfo.NuGetPackage,
                IsAvailable = driver != null,
                IsInstalled = isInstalled,
                StatusMessage = isInstalled 
                    ? "Driver is installed and available" 
                    : driver != null 
                        ? "Driver configuration found but NuGet package is missing"
                        : "Driver configuration not found"
            };
        }

        /// <summary>
        /// Check schema privileges and list existing schemas
        /// </summary>
        public async Task<SchemaPrivilegeCheckResult> CheckSchemaPrivilegesAsync(ConnectionConfig config, string? schemaName = null)
        {
            try
            {
                var driverInfo = GetDriverInfo(config.DatabaseType);
                if (driverInfo == null)
                {
                    return new SchemaPrivilegeCheckResult
                    {
                        HasCreatePrivilege = false,
                        Message = $"Unknown database type: {config.DatabaseType}"
                    };
                }

                // SQLite doesn't use schemas
                if (config.DatabaseType == "SQLite")
                {
                    return new SchemaPrivilegeCheckResult
                    {
                        HasCreatePrivilege = true,
                        SchemaExists = true,
                        Message = "SQLite doesn't use schemas - database is file-based"
                    };
                }

                // Create connection properties
                var connectionProperties = new ConnectionProperties
                {
                    ConnectionName = "TempPrivilegeCheck",
                    DatabaseType = ParseDataSourceType(driverInfo.DataSourceType),
                    DriverName = driverInfo.NuGetPackage,
                    Host = config.Host,
                    Port = config.Port > 0 ? config.Port : driverInfo.DefaultPort,
                    Database = config.Database,
                    UserID = config.Username ?? string.Empty,
                    Password = config.Password ?? string.Empty,
                    ConnectionString = config.ConnectionString ?? string.Empty
                };

                // First save the connection, then open it
                if (!_editor.ConfigEditor.DataConnectionExist(connectionProperties.ConnectionName))
                {
                    _editor.ConfigEditor.AddDataConnection(connectionProperties);
                    _editor.ConfigEditor.SaveDataconnectionsValues();
                }
                var dataSource = _editor.GetDataSource(connectionProperties.ConnectionName);
                if (dataSource == null)
                {
                    return new SchemaPrivilegeCheckResult
                    {
                        HasCreatePrivilege = false,
                        Message = "Failed to get data source for privilege check"
                    };
                }

                // Open connection
                if (_editor.OpenDataSource(connectionProperties.ConnectionName) != ConnectionState.Open)
                {
                    return new SchemaPrivilegeCheckResult
                    {
                        HasCreatePrivilege = false,
                        Message = "Failed to open connection for privilege check"
                    };
                }

                try
                {
                    var result = await Task.Run(() => CheckSchemaPrivilegesInternal(dataSource, config.DatabaseType, schemaName, config));
                    return result;
                }
                finally
                {
                    // Close connection
                    _editor.CloseDataSource(connectionProperties.ConnectionName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking schema privileges for {DatabaseType}", config.DatabaseType);
                return new SchemaPrivilegeCheckResult
                {
                    HasCreatePrivilege = false,
                    Message = "Error checking schema privileges",
                    ErrorDetails = ex.Message
                };
            }
        }

        /// <summary>
        /// Execute a query and return DataTable results if available
        /// </summary>
        /// <summary>
        /// Get scalar boolean value from query result using IDataSource.GetScalar
        /// </summary>
        private bool GetScalarBoolean(IDataSource dataSource, string sql, bool defaultValue = false)
        {
            try
            {
                // Use GetScalar for scalar queries - returns double
                var scalarValue = dataSource.GetScalar(sql);
                // Convert double to bool: 0 = false, non-zero = true
                return scalarValue != 0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting scalar boolean value from query: {Sql}", sql);
                return defaultValue;
            }
        }

        /// <summary>
        /// Get list of string values from first column of query result using IDataSource.RunQuery
        /// </summary>
        private List<string> GetStringList(IDataSource dataSource, string sql)
        {
            var results = new List<string>();
            try
            {
                // Use RunQuery for multi-row queries - returns IEnumerable<object>
                var queryResults = dataSource.RunQuery(sql);
                if (queryResults != null)
                {
                    foreach (var row in queryResults)
                    {
                        // Extract first property/value from the row object
                        if (row != null)
                        {
                            object? firstValue = null;

                            // Handle dictionaries (most common case for dynamic queries)
                            if (row is IDictionary<string, object> dict)
                            {
                                firstValue = dict.Values.FirstOrDefault();
                            }
                            else if (row is System.Dynamic.ExpandoObject expando)
                            {
                                var expandoDict = (IDictionary<string, object>)expando;
                                firstValue = expandoDict.Values.FirstOrDefault();
                            }
                            else
                            {
                                // Try to get first property using reflection
                                var properties = row.GetType().GetProperties();
                                if (properties.Length > 0)
                                {
                                    var firstProp = properties[0];
                                    firstValue = firstProp.GetValue(row);
                                }
                            }

                            if (firstValue != null && firstValue != DBNull.Value)
                            {
                                results.Add(firstValue.ToString() ?? string.Empty);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting string list from query: {Sql}", sql);
            }
            return results;
        }

        /// <summary>
        /// Internal method to check schema privileges for different database types
        /// </summary>
        private SchemaPrivilegeCheckResult CheckSchemaPrivilegesInternal(IDataSource dataSource, string databaseType, string? schemaName, ConnectionConfig? config)
        {
            var result = new SchemaPrivilegeCheckResult
            {
                ExistingSchemas = new List<string>()
            };

            try
            {
                if (dataSource == null)
                {
                    result.Message = "Data source is null";
                    return result;
                }

                switch (databaseType)
                {
                    case "PostgreSQL":
                        // Check if user can create schemas
                        try
                        {
                            // Check if user has CREATE privilege on database or is superuser
                            var createPrivilegeSql = @"
                                SELECT COALESCE(
                                    (SELECT true FROM pg_roles WHERE rolname = current_user AND (rolcreatedb = true OR rolsuper = true)),
                                    has_schema_privilege(current_user, 'public', 'CREATE'),
                                    has_database_privilege(current_database(), 'CREATE'),
                                    false
                                )";
                            result.HasCreatePrivilege = GetScalarBoolean(dataSource, createPrivilegeSql, false);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error checking PostgreSQL CREATE privilege");
                            result.HasCreatePrivilege = false;
                        }
                        
                        // List existing schemas
                        try
                        {
                            var schemasSql = @"
                                SELECT schema_name 
                                FROM information_schema.schemata 
                                WHERE schema_name NOT IN ('pg_catalog', 'information_schema', 'pg_toast', 'pg_toast_temp_1') 
                                ORDER BY schema_name";
                            var schemas = GetStringList(dataSource, schemasSql);
                            if (schemas.Any())
                            {
                                result.ExistingSchemas.AddRange(schemas);
                            }
                            else
                            {
                                // Fallback to default schema
                                result.ExistingSchemas.Add("public");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error listing PostgreSQL schemas");
                            result.ExistingSchemas.Add("public"); // Default schema fallback
                        }
                        
                        if (!string.IsNullOrEmpty(schemaName))
                        {
                            result.SchemaExists = result.ExistingSchemas.Contains(schemaName, StringComparer.OrdinalIgnoreCase);
                        }
                        
                        result.Message = result.HasCreatePrivilege 
                            ? $"PostgreSQL schema check completed - user can create schemas. Found {result.ExistingSchemas.Count} schema(s)."
                            : $"PostgreSQL schema check completed - user may not have CREATE privileges. Found {result.ExistingSchemas.Count} schema(s).";
                        break;

                    case "SqlServer":
                        // SQL Server uses schemas within databases, check for CREATE SCHEMA permission
                        try
                        {
                            var createPrivilegeSql = "SELECT CAST(HAS_PERMS_BY_NAME(DB_NAME(), 'DATABASE', 'CREATE SCHEMA') AS BIT)";
                            result.HasCreatePrivilege = GetScalarBoolean(dataSource, createPrivilegeSql, false);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error checking SQL Server CREATE SCHEMA privilege");
                            result.HasCreatePrivilege = false;
                        }
                        
                        // List existing schemas
                        try
                        {
                            var schemasSql = @"
                                SELECT name 
                                FROM sys.schemas 
                                WHERE principal_id = SCHEMA_ID('dbo') OR principal_id IN (
                                    SELECT principal_id FROM sys.database_principals WHERE type = 'S'
                                )
                                ORDER BY name";
                            var schemas = GetStringList(dataSource, schemasSql);
                            if (schemas.Any())
                            {
                                result.ExistingSchemas.AddRange(schemas);
                            }
                            else
                            {
                                // Fallback to default schemas
                                result.ExistingSchemas.Add("dbo");
                                result.ExistingSchemas.Add("guest");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error listing SQL Server schemas");
                            result.ExistingSchemas.Add("dbo"); // Default schema fallback
                        }
                        
                        if (!string.IsNullOrEmpty(schemaName))
                        {
                            result.SchemaExists = result.ExistingSchemas.Contains(schemaName, StringComparer.OrdinalIgnoreCase);
                        }
                        
                        result.Message = result.HasCreatePrivilege
                            ? $"SQL Server schema check completed - user can create schemas. Found {result.ExistingSchemas.Count} schema(s)."
                            : $"SQL Server schema check completed - user may not have CREATE SCHEMA privileges. Found {result.ExistingSchemas.Count} schema(s).";
                        break;

                    case "MySQL":
                    case "MariaDB":
                        // MySQL/MariaDB use databases (not schemas), but we check CREATE DATABASE privilege
                        try
                        {
                            var createPrivilegeSql = @"
                                SELECT CASE 
                                    WHEN EXISTS (
                                        SELECT 1 FROM information_schema.USER_PRIVILEGES 
                                        WHERE GRANTEE = CONCAT('''', USER(), '''@''', HOST(), '''') 
                                        AND PRIVILEGE_TYPE = 'CREATE'
                                    ) THEN 1 
                                    ELSE 0 
                                END";
                            result.HasCreatePrivilege = GetScalarBoolean(dataSource, createPrivilegeSql, false);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error checking MySQL/MariaDB CREATE privilege");
                            result.HasCreatePrivilege = false;
                        }
                        
                        // List existing databases
                        try
                        {
                            var databasesSql = @"
                                SELECT SCHEMA_NAME 
                                FROM information_schema.SCHEMATA 
                                WHERE SCHEMA_NAME NOT IN ('information_schema', 'performance_schema', 'mysql', 'sys') 
                                ORDER BY SCHEMA_NAME";
                            var databases = GetStringList(dataSource, databasesSql);
                            if (databases.Any())
                            {
                                result.ExistingSchemas.AddRange(databases);
                            }
                            
                            // Always add current database if specified
                            if (config != null && !string.IsNullOrEmpty(config.Database) && 
                                !result.ExistingSchemas.Contains(config.Database, StringComparer.OrdinalIgnoreCase))
                            {
                                result.ExistingSchemas.Add(config.Database);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error listing MySQL/MariaDB databases");
                            if (config != null && !string.IsNullOrEmpty(config.Database))
                            {
                                result.ExistingSchemas.Add(config.Database);
                            }
                        }
                        
                        result.Message = result.HasCreatePrivilege
                            ? $"MySQL/MariaDB database check completed - user can create databases. Found {result.ExistingSchemas.Count} database(s)."
                            : $"MySQL/MariaDB database check completed - user may not have CREATE privileges. Found {result.ExistingSchemas.Count} database(s).";
                        break;

                    case "Oracle":
                        // Oracle uses schemas (users), check for CREATE USER privilege or use existing schema
                        try
                        {
                            // Check if user has CREATE USER privilege (typically requires DBA role)
                            var createPrivilegeSql = @"
                                SELECT CASE 
                                    WHEN EXISTS (
                                        SELECT 1 FROM USER_ROLE_PRIVS 
                                        WHERE GRANTED_ROLE = 'DBA'
                                    ) OR EXISTS (
                                        SELECT 1 FROM USER_SYS_PRIVS 
                                        WHERE PRIVILEGE = 'CREATE USER'
                                    ) THEN 1 
                                    ELSE 0 
                                END FROM DUAL";
                            result.HasCreatePrivilege = GetScalarBoolean(dataSource, createPrivilegeSql, false);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error checking Oracle CREATE USER privilege");
                            result.HasCreatePrivilege = false;
                        }
                        
                        // List existing schemas (users) - limited to current user's accessible schemas
                        try
                        {
                            var schemasSql = @"
                                SELECT DISTINCT USERNAME 
                                FROM ALL_USERS 
                                WHERE USERNAME = USER() 
                                   OR USERNAME IN (
                                       SELECT DISTINCT OWNER FROM ALL_TABLES WHERE OWNER = USER()
                                   )
                                ORDER BY USERNAME";
                            var schemas = GetStringList(dataSource, schemasSql);
                            if (schemas.Any())
                            {
                                result.ExistingSchemas.AddRange(schemas);
                            }
                            else if (config != null && !string.IsNullOrEmpty(config.Username))
                            {
                                result.ExistingSchemas.Add(config.Username);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error listing Oracle schemas");
                            if (config != null && !string.IsNullOrEmpty(config.Username))
                            {
                                result.ExistingSchemas.Add(config.Username);
                            }
                        }
                        
                        result.Message = result.HasCreatePrivilege
                            ? "Oracle schema check completed - user has DBA privileges and can create schemas/users."
                            : "Oracle schema creation typically requires DBA privileges. Please use an existing schema/user.";
                        break;

                    default:
                        result.Message = $"Schema privilege checking not implemented for {databaseType}";
                        result.HasCreatePrivilege = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error in privilege check for {DatabaseType}", databaseType);
                result.Message = $"Privilege check encountered an error: {ex.Message}";
                result.ErrorDetails = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Create a new schema/database
        /// </summary>
        public async Task<CreateSchemaResult> CreateSchemaAsync(ConnectionConfig config, string schemaName)
        {
            try
            {
                var driverInfo = GetDriverInfo(config.DatabaseType);
                if (driverInfo == null)
                {
                    return new CreateSchemaResult
                    {
                        Success = false,
                        Message = $"Unknown database type: {config.DatabaseType}"
                    };
                }

                // SQLite doesn't use schemas - database is file-based
                if (config.DatabaseType == "SQLite")
                {
                    return new CreateSchemaResult
                    {
                        Success = true,
                        Message = "SQLite uses file-based databases - no schema creation needed"
                    };
                }

                // Create connection properties
                // For schema creation, connect to the target database (schema will be created within it)
                // For MySQL/MariaDB database creation, we connect to the server and create a new database
                string targetDb = config.Database;
                if (config.DatabaseType == "MySQL" || config.DatabaseType == "MariaDB")
                {
                    // For MySQL/MariaDB, when creating a database, we can connect to any existing database
                    // or leave it empty - the driver will handle it
                    targetDb = config.Database; // Use the database specified (may be empty for new DB creation)
                }
                
                var connectionProperties = new ConnectionProperties
                {
                    ConnectionName = "TempSchemaCreation",
                    DatabaseType = ParseDataSourceType(driverInfo.DataSourceType),
                    DriverName = driverInfo.NuGetPackage,
                    Host = config.Host,
                    Port = config.Port > 0 ? config.Port : driverInfo.DefaultPort,
                    Database = targetDb,
                    UserID = config.Username ?? string.Empty,
                    Password = config.Password ?? string.Empty,
                    ConnectionString = config.ConnectionString ?? string.Empty
                };

                // First save the connection, then open it
                if (!_editor.ConfigEditor.DataConnectionExist(connectionProperties.ConnectionName))
                {
                    _editor.ConfigEditor.AddDataConnection(connectionProperties);
                    _editor.ConfigEditor.SaveDataconnectionsValues();
                }
                var dataSource = _editor.GetDataSource(connectionProperties.ConnectionName);
                if (dataSource == null)
                {
                    return new CreateSchemaResult
                    {
                        Success = false,
                        Message = "Failed to get data source for schema creation"
                    };
                }

                // Open connection
                if (_editor.OpenDataSource(connectionProperties.ConnectionName) != ConnectionState.Open)
                {
                    return new CreateSchemaResult
                    {
                        Success = false,
                        Message = "Failed to open connection for schema creation"
                    };
                }

                try
                {
                    var result = await Task.Run(() => CreateSchemaInternal(dataSource, config.DatabaseType, schemaName));
                    return result;
                }
                finally
                {
                    // Close connection
                    _editor.CloseDataSource(connectionProperties.ConnectionName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating schema {SchemaName} for {DatabaseType}", schemaName, config.DatabaseType);
                return new CreateSchemaResult
                {
                    Success = false,
                    Message = "Error creating schema",
                    ErrorDetails = ex.Message
                };
            }
        }

        /// <summary>
        /// Internal method to create schema for different database types
        /// </summary>
        private CreateSchemaResult CreateSchemaInternal(IDataSource dataSource, string databaseType, string schemaName)
        {
            try
            {
                if (dataSource == null)
                {
                    return new CreateSchemaResult
                    {
                        Success = false,
                        Message = "Data source is null"
                    };
                }

                string createSql;

                switch (databaseType)
                {
                    case "PostgreSQL":
                        createSql = $"CREATE SCHEMA IF NOT EXISTS \"{schemaName}\";";
                        break;

                    case "SqlServer":
                        createSql = $"IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = '{schemaName}') EXEC('CREATE SCHEMA [{schemaName}]');";
                        break;

                    case "MySQL":
                    case "MariaDB":
                        // For MySQL/MariaDB, we create a new database (schema)
                        // Note: The user should have CREATE DATABASE privilege
                        createSql = $"CREATE DATABASE IF NOT EXISTS `{schemaName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;";
                        break;

                    case "Oracle":
                        // Oracle schema creation is complex and requires DBA privileges
                        return new CreateSchemaResult
                        {
                            Success = false,
                            Message = "Oracle schema creation requires DBA privileges. Please create the schema/user manually or use an existing one."
                        };

                    default:
                        return new CreateSchemaResult
                        {
                            Success = false,
                            Message = $"Schema creation not implemented for {databaseType}"
                        };
                }

                // Execute SQL
                dataSource.ExecuteSql(createSql);

                return new CreateSchemaResult
                {
                    Success = true,
                    Message = $"Schema '{schemaName}' created successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing schema creation SQL for {SchemaName}", schemaName);
                return new CreateSchemaResult
                {
                    Success = false,
                    Message = $"Failed to create schema: {ex.Message}",
                    ErrorDetails = ex.Message
                };
            }
        }

        /// <summary>
        /// Test database connection
        /// </summary>
        public async Task<ConnectionTestResult> TestConnectionAsync(ConnectionConfig config)
        {
            try
            {
                var driverInfo = GetDriverInfo(config.DatabaseType);
                if (driverInfo == null)
                {
                    return new ConnectionTestResult
                    {
                        Success = false,
                        Message = $"Unknown database type: {config.DatabaseType}"
                    };
                }

                // Create connection properties
                var connectionProperties = new ConnectionProperties
                {
                    ConnectionName = config.ConnectionName,
                    DatabaseType = ParseDataSourceType(driverInfo.DataSourceType),
                    DriverName = driverInfo.NuGetPackage,
                    Host = config.Host,
                    Port = config.Port > 0 ? config.Port : driverInfo.DefaultPort,
                    Database = config.Database,
                    UserID = config.Username ?? string.Empty,
                    Password = config.Password ?? string.Empty,
                    ConnectionString = config.ConnectionString ?? string.Empty,
                    GuidID = Guid.NewGuid().ToString()
                };

                // Create temporary data source to test connection
                // First save the connection, then open it
                if (!_editor.ConfigEditor.DataConnectionExist(connectionProperties.ConnectionName))
                {
                    _editor.ConfigEditor.AddDataConnection(connectionProperties);
                    _editor.ConfigEditor.SaveDataconnectionsValues();
                }
                var dataSource = _editor.GetDataSource(connectionProperties.ConnectionName);
                if (dataSource == null)
                {
                    return new ConnectionTestResult
                    {
                        Success = false,
                        Message = "Failed to get data source. Please ensure the driver is installed."
                    };
                }

                // Open connection
                var result = await Task.Run(() => _editor.OpenDataSource(connectionProperties.ConnectionName));

                if (result == ConnectionState.Open)
                {
                    // Close connection
                    _editor.CloseDataSource(connectionProperties.ConnectionName);
                    return new ConnectionTestResult
                    {
                        Success = true,
                        Message = "Connection successful"
                    };
                }
                else
                {
                    return new ConnectionTestResult
                    {
                        Success = false,
                        Message = "Failed to open connection",
                        ErrorDetails = "Connection state: " + result.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing connection for {DatabaseType}", config.DatabaseType);
                return new ConnectionTestResult
                {
                    Success = false,
                    Message = "Connection test failed",
                    ErrorDetails = ex.Message
                };
            }
        }

        /// <summary>
        /// Get available scripts for database type using DataManager
        /// </summary>
        public async Task<List<Beep.OilandGas.Models.DTOs.DataManagement.ScriptInfoDto>> GetAvailableScriptsAsync(string databaseType)
        {
            try
            {
                var allModules = ModuleDataRegistry.GetAllModules();
                var normalizedDbType = DatabaseTypeNormalizer.Normalize(databaseType);
                var allScripts = new List<Beep.OilandGas.Models.DTOs.DataManagement.ScriptInfoDto>();

                foreach (var module in allModules.OrderBy(m => m.ExecutionOrder))
                {
                    try
                    {
                        var moduleScripts = await module.GetScriptsAsync(normalizedDbType);
                        foreach (var script in moduleScripts)
                        {
                            allScripts.Add(new Beep.OilandGas.Models.DTOs.DataManagement.ScriptInfoDto
                            {
                                FileName = script.FileName,
                                FullPath = script.FullPath,
                                RelativePath = script.RelativePath,
                                ScriptType = script.ScriptType.ToString(),
                                TableName = script.TableName,
                                Module = module.ModuleName,
                                IsConsolidated = script.IsConsolidated,
                                IsMandatory = script.IsRequired,
                                IsOptional = script.IsOptional,
                                FileSize = script.FileSize,
                                LastModified = script.LastModified,
                                ExecutionOrder = script.ExecutionOrder,
                                Dependencies = script.Dependencies
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error getting scripts for module {ModuleName}", module.ModuleName);
                    }
                }

                return allScripts.OrderBy(s => s.ExecutionOrder).ThenBy(s => s.FileName).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available scripts for {DatabaseType}", databaseType);
                return new List<Beep.OilandGas.Models.DTOs.DataManagement.ScriptInfoDto>();
            }
        }

        /// <summary>
        /// Get available scripts for database type (synchronous wrapper for backward compatibility)
        /// </summary>
        public List<Beep.OilandGas.Models.DTOs.DataManagement.ScriptInfoDto> GetAvailableScripts(string databaseType)
        {
            try
            {
                return GetAvailableScriptsAsync(databaseType).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available scripts for {DatabaseType}", databaseType);
                return new List<Beep.OilandGas.Models.DTOs.DataManagement.ScriptInfoDto>();
            }
        }

        /// <summary>
        /// Execute a single SQL script using DataManager
        /// </summary>
        public async Task<Beep.OilandGas.Models.DTOs.DataManagement.ScriptExecutionResultDto> ExecuteScriptAsync(ConnectionConfig config, string scriptName, string? operationId = null)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                // Get data source from connection config
                var dataSource = GetDataSourceFromConnectionConfig(config);

                // Ensure connection is open
                if (dataSource.Openconnection() != ConnectionState.Open)
                {
                    return new Beep.OilandGas.Models.DTOs.DataManagement.ScriptExecutionResultDto
                    {
                        ScriptFileName = scriptName,
                        Success = false,
                        ErrorMessage = "Failed to open connection",
                        StartTime = DateTime.UtcNow,
                        EndTime = DateTime.UtcNow,
                        Duration = stopwatch.Elapsed
                    };
                }

                // Find module that contains this script
                var module = await FindModuleByScriptNameAsync(scriptName, config.DatabaseType);
                if (module == null)
                {
                    return new Beep.OilandGas.Models.DTOs.DataManagement.ScriptExecutionResultDto
                    {
                        ScriptFileName = scriptName,
                        Success = false,
                        ErrorMessage = $"Script '{scriptName}' not found in any module",
                        StartTime = DateTime.UtcNow,
                        EndTime = DateTime.UtcNow,
                        Duration = stopwatch.Elapsed
                    };
                }

                // Get scripts from module
                var normalizedDbType = DatabaseTypeNormalizer.Normalize(config.DatabaseType);
                var moduleScripts = await module.GetScriptsAsync(normalizedDbType);
                var scriptInfo = moduleScripts.FirstOrDefault(s => s.FileName.Equals(scriptName, StringComparison.OrdinalIgnoreCase));

                if (scriptInfo == null)
                {
                    return new Beep.OilandGas.Models.DTOs.DataManagement.ScriptExecutionResultDto
                    {
                        ScriptFileName = scriptName,
                        Success = false,
                        ErrorMessage = $"Script '{scriptName}' not found in module '{module.ModuleName}'",
                        StartTime = DateTime.UtcNow,
                        EndTime = DateTime.UtcNow,
                        Duration = stopwatch.Elapsed
                    };
                }

                // Update progress
                if (!string.IsNullOrEmpty(operationId) && _progressTracking != null)
                {
                    _progressTracking.UpdateProgress(operationId, 0, $"Executing {scriptName}");
                }

                // Execute script using DataManager
                var options = new ScriptExecutionOptions
                {
                    ContinueOnError = false,
                    ValidateBeforeExecution = false, // Skip validation for individual script execution
                    CheckErrorsAfterExecution = false,
                    Logger = _logger
                };

                var dataManagerResult = await _dataManager.ExecuteScriptAsync(scriptInfo, dataSource, options);

                // Update progress
                if (!string.IsNullOrEmpty(operationId) && _progressTracking != null)
                {
                    var progress = dataManagerResult.Success ? 100 : 50;
                    _progressTracking.UpdateProgress(operationId, progress,
                        dataManagerResult.Success ? $"Completed {scriptName}" : $"Failed {scriptName}: {dataManagerResult.ErrorMessage}");
                }

                stopwatch.Stop();

                // Map result to DTO
                return new Beep.OilandGas.Models.DTOs.DataManagement.ScriptExecutionResultDto
                {
                    ScriptFileName = scriptName,
                    Success = dataManagerResult.Success,
                    ErrorMessage = dataManagerResult.ErrorMessage,
                    StartTime = dataManagerResult.StartTime,
                    EndTime = dataManagerResult.EndTime,
                    Duration = stopwatch.Elapsed,
                    RowsAffected = dataManagerResult.RowsAffected,
                    ExecutionLog = dataManagerResult.ExecutionLog
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error executing script {ScriptName}", scriptName);

                if (!string.IsNullOrEmpty(operationId) && _progressTracking != null)
                {
                    _progressTracking.CompleteOperation(operationId, false, $"Script execution failed: {ex.Message}", ex.ToString());
                }

                return new Beep.OilandGas.Models.DTOs.DataManagement.ScriptExecutionResultDto
                {
                    ScriptFileName = scriptName,
                    Success = false,
                    ErrorMessage = $"Script execution failed: {ex.Message}",
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow,
                    Duration = stopwatch.Elapsed
                };
            }
        }

        // Legacy method - kept for reference but not used
        private async Task<ScriptExecutionResult> ExecuteScriptAsyncLegacy(ConnectionConfig config, string scriptName, string? operationId = null)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var driverInfo = GetDriverInfo(config.DatabaseType);
                if (driverInfo == null)
                {
                    return new ScriptExecutionResult
                    {
                        ScriptName = scriptName,
                        Success = false,
                        Message = $"Unknown database type: {config.DatabaseType}",
                        ExecutionTime = stopwatch.Elapsed
                    };
                }

                // Get script path
                var scriptsPath = GetScriptsPath(driverInfo.ScriptPath);
                var scriptPath = Path.Combine(scriptsPath, scriptName);

                if (!File.Exists(scriptPath))
                {
                    return new ScriptExecutionResult
                    {
                        ScriptName = scriptName,
                        Success = false,
                        Message = $"Script file not found: {scriptPath}",
                        ExecutionTime = stopwatch.Elapsed
                    };
                }

                // Create connection
                var connectionProperties = new ConnectionProperties
                {
                    ConnectionName = config.ConnectionName,
                    DatabaseType = ParseDataSourceType(driverInfo.DataSourceType),
                    DriverName = driverInfo.NuGetPackage,
                    Host = config.Host,
                    Port = config.Port > 0 ? config.Port : driverInfo.DefaultPort,
                    Database = config.Database,
                    UserID = config.Username ?? string.Empty,
                    Password = config.Password ?? string.Empty,
                    ConnectionString = config.ConnectionString ?? string.Empty
                };

                // First save the connection, then open it
                if (!_editor.ConfigEditor.DataConnectionExist(connectionProperties.ConnectionName))
                {
                    _editor.ConfigEditor.AddDataConnection(connectionProperties);
                    _editor.ConfigEditor.SaveDataconnectionsValues();
                }
                var dataSource = _editor.GetDataSource(connectionProperties.ConnectionName);
                if (dataSource == null)
                {
                    return new ScriptExecutionResult
                    {
                        ScriptName = scriptName,
                        Success = false,
                        Message = "Failed to get data source",
                        ExecutionTime = stopwatch.Elapsed
                    };
                }

                // Open connection
                if (dataSource.Openconnection() != ConnectionState.Open)
                {
                    return new ScriptExecutionResult
                    {
                        ScriptName = scriptName,
                        Success = false,
                        Message = "Failed to open connection",
                        ExecutionTime = stopwatch.Elapsed
                    };
                }

                try
                {
                    // Read script
                    var scriptContent = await File.ReadAllTextAsync(scriptPath);

                    // Split script into statements
                    var statements = SplitScriptIntoStatements(scriptContent, config.DatabaseType);
                    var totalStatements = statements.Count;

                    int successCount = 0;
                    int failCount = 0;
                    string lastError = string.Empty;

                    // Execute each statement with progress tracking
                    for (int i = 0; i < statements.Count; i++)
                    {
                        var statement = statements[i];
                        if (string.IsNullOrWhiteSpace(statement))
                            continue;

                        // Update progress every 100 statements or at start/end
                        if (!string.IsNullOrEmpty(operationId) && _progressTracking != null && 
                            (i == 0 || i % 100 == 0 || i == statements.Count - 1))
                        {
                            var statementProgress = (int)(((double)(i + 1) / totalStatements) * 100);
                            _progressTracking.UpdateProgress(operationId, statementProgress,
                                $"Executing {scriptName}: {i + 1}/{totalStatements} statements", i + 1, totalStatements);
                        }

                        try
                        {
                            // Execute SQL
                            dataSource.ExecuteSql(statement);
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            failCount++;
                            lastError = ex.Message;
                            _logger.LogWarning(ex, "Error executing statement {StatementIndex} in {ScriptName}", i, scriptName);
                        }
                    }

                    stopwatch.Stop();

                    return new ScriptExecutionResult
                    {
                        ScriptName = scriptName,
                        Success = failCount == 0,
                        Message = failCount == 0 
                            ? $"Successfully executed {successCount} statements"
                            : $"Executed {successCount} statements, {failCount} failed. Last error: {lastError}",
                        ErrorDetails = failCount > 0 ? lastError : null,
                        ExecutionTime = stopwatch.Elapsed
                    };
                }
                finally
                {
                    // Close connection
                    _editor.CloseDataSource(connectionProperties.ConnectionName);
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error executing script {ScriptName}", scriptName);
                return new ScriptExecutionResult
                {
                    ScriptName = scriptName,
                    Success = false,
                    Message = "Script execution failed",
                    ErrorDetails = ex.Message,
                    ExecutionTime = stopwatch.Elapsed
                };
            }
        }

        /// <summary>
        /// Execute all scripts in order using DataManager
        /// </summary>
        public async Task<AllScriptsExecutionResult> ExecuteAllScriptsAsync(ConnectionConfig config, List<string> scriptNames, string? operationId = null)
        {
            var overallStopwatch = Stopwatch.StartNew();
            try
            {
                // Get data source from connection config
                var dataSource = GetDataSourceFromConnectionConfig(config);

                // Ensure connection is open
                if (dataSource.Openconnection() != ConnectionState.Open)
                {
                    return new Beep.OilandGas.Models.DTOs.AllScriptsExecutionResult
                    {
                        Results = new List<Beep.OilandGas.Models.DTOs.DataManagement.ScriptExecutionResultDto>(),
                        TotalScripts = scriptNames.Count,
                        SuccessfulScripts = 0,
                        FailedScripts = scriptNames.Count,
                        AllSucceeded = false,
                        TotalDuration = overallStopwatch.Elapsed
                    };
                }

                // Group scripts by module
                var normalizedDbType = DatabaseTypeNormalizer.Normalize(config.DatabaseType);
                var modulesToExecute = new Dictionary<IModuleData, List<ModuleScriptInfo>>();

                foreach (var scriptName in scriptNames)
                {
                    var module = await FindModuleByScriptNameAsync(scriptName, normalizedDbType);
                    if (module != null)
                    {
                        if (!modulesToExecute.ContainsKey(module))
                        {
                            modulesToExecute[module] = new List<ModuleScriptInfo>();
                        }

                        var moduleScripts = await module.GetScriptsAsync(normalizedDbType);
                        var scriptInfo = moduleScripts.FirstOrDefault(s => s.FileName.Equals(scriptName, StringComparison.OrdinalIgnoreCase));
                        if (scriptInfo != null)
                        {
                            modulesToExecute[module].Add(scriptInfo);
                        }
                    }
                }

                // Start progress tracking
                if (!string.IsNullOrEmpty(operationId) && _progressTracking != null)
                {
                    _progressTracking.UpdateProgress(operationId, 0, $"Starting execution of {scriptNames.Count} script(s) across {modulesToExecute.Count} module(s)");
                }

                var allResults = new List<Beep.OilandGas.Models.DTOs.DataManagement.ScriptExecutionResultDto>();
                var totalScripts = scriptNames.Count;
                var completedScripts = 0;

                // Execute modules in dependency order
                var sortedModules = ModuleDataRegistry.GetModulesWithDependenciesResolved()
                    .Where(m => modulesToExecute.ContainsKey(m))
                    .ToList();

                foreach (var module in sortedModules)
                {
                    var moduleScripts = modulesToExecute[module];

                    foreach (var scriptInfo in moduleScripts.OrderBy(s => s.ExecutionOrder))
                    {
                        // Update progress
                        if (!string.IsNullOrEmpty(operationId) && _progressTracking != null)
                        {
                            var percentage = (int)((completedScripts / (double)totalScripts) * 100);
                            _progressTracking.UpdateProgress(operationId, percentage,
                                $"Executing {scriptInfo.FileName} ({module.ModuleName})");
                        }

                        // Execute script
                        var options = new ScriptExecutionOptions
                        {
                            ContinueOnError = false,
                            ValidateBeforeExecution = false,
                            CheckErrorsAfterExecution = false,
                            Logger = _logger
                        };

                        var dataManagerResult = await _dataManager.ExecuteScriptAsync(scriptInfo, dataSource, options);
                        var result = MapScriptExecutionResultToDto(dataManagerResult);
                        allResults.Add(result);
                        completedScripts++;

                        // Update progress
                        if (!string.IsNullOrEmpty(operationId) && _progressTracking != null)
                        {
                            var percentage = (int)((completedScripts / (double)totalScripts) * 100);
                            _progressTracking.UpdateProgress(operationId, percentage,
                                $"Completed {scriptInfo.FileName} - {(result.Success ? "Success" : "Failed")}");
                        }

                        // Stop on first failure if script is mandatory
                        if (!result.Success && scriptInfo.IsRequired)
                        {
                            _logger.LogWarning("Required script {ScriptName} failed, stopping execution", scriptInfo.FileName);
                            if (!string.IsNullOrEmpty(operationId) && _progressTracking != null)
                            {
                                _progressTracking.CompleteOperation(operationId, false,
                                    "Execution stopped due to required script failure", result.ErrorDetails);
                            }
                            break;
                        }
                    }
                }

                overallStopwatch.Stop();

                if (!string.IsNullOrEmpty(operationId) && _progressTracking != null)
                {
                    var allSucceeded = allResults.All(r => r.Success);
                    _progressTracking.CompleteOperation(operationId, allSucceeded,
                        allSucceeded ? "All scripts executed successfully" : "Some scripts failed",
                        allSucceeded ? null : $"Failed: {allResults.Count(r => !r.Success)}/{allResults.Count}");
                }

                return new Beep.OilandGas.Models.DTOs.AllScriptsExecutionResult
                {
                    Results = allResults,
                    TotalScripts = scriptNames.Count,
                    SuccessfulScripts = allResults.Count(r => r.Success),
                    FailedScripts = allResults.Count(r => !r.Success),
                    AllSucceeded = allResults.All(r => r.Success),
                    TotalDuration = overallStopwatch.Elapsed
                };
            }
            catch (Exception ex)
            {
                overallStopwatch.Stop();
                _logger.LogError(ex, "Error executing all scripts");

                if (!string.IsNullOrEmpty(operationId) && _progressTracking != null)
                {
                    _progressTracking.CompleteOperation(operationId, false, "Execution failed", ex.Message);
                }

                return new Beep.OilandGas.Models.DTOs.AllScriptsExecutionResult
                {
                    Results = new List<Beep.OilandGas.Models.DTOs.DataManagement.ScriptExecutionResultDto>(),
                    TotalScripts = scriptNames.Count,
                    SuccessfulScripts = 0,
                    FailedScripts = scriptNames.Count,
                    AllSucceeded = false,
                    TotalDuration = overallStopwatch.Elapsed
                };
            }
        }

        /// <summary>
        /// Get all database connections
        /// </summary>
        public List<DatabaseConnectionListItem> GetAllConnections()
        {
            try
            {
                var connections = _editor.ConfigEditor?.DataConnections ?? new List<ConnectionProperties>();
                var currentConnectionName = GetCurrentConnectionName(); // Use our stored current connection name

                return connections.Select(conn =>
                {
                    var connectionDbTypeString = conn.DatabaseType.ToString();
                    var dbType = DatabaseDrivers.FirstOrDefault(d => 
                        d.Value.DataSourceType.Equals(connectionDbTypeString, StringComparison.OrdinalIgnoreCase) ||
                        d.Key.Equals(connectionDbTypeString, StringComparison.OrdinalIgnoreCase)).Key ?? "Unknown";
                    return new DatabaseConnectionListItem
                    {
                        ConnectionName = conn.ConnectionName ?? string.Empty,
                        DatabaseType = dbType,
                        Host = conn.Host ?? string.Empty,
                        Port = conn.Port,
                        Database = conn.Database ?? string.Empty,
                        Username = conn.UserID,
                        IsCurrent = conn.ConnectionName == currentConnectionName,
                        GuidId = conn.GuidID ?? string.Empty
                    };
                }).OrderBy(c => c.ConnectionName).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all connections");
                return new List<DatabaseConnectionListItem>();
            }
        }

        /// <summary>
        /// Get current database connection name
        /// </summary>
        public string? GetCurrentConnectionName()
        {
            try
            {
                // Return stored current connection name
                // The current connection is set via SetCurrentConnection which calls OpenDataSource
                return _currentConnectionName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current connection name");
                return null;
            }
        }

        /// <summary>
        /// Set current database connection
        /// </summary>
        public SetCurrentDatabaseResult SetCurrentConnection(string connectionName)
        {
            try
            {
                var currentConnectionName = GetCurrentConnectionName();
                
                // If switching to a different connection, require logout
                bool requiresLogout = !string.IsNullOrEmpty(currentConnectionName) && 
                                     currentConnectionName != connectionName;

                var connection = _editor.ConfigEditor?.DataConnections?
                    .FirstOrDefault(c => c.ConnectionName == connectionName);

                if (connection == null)
                {
                    return new SetCurrentDatabaseResult
                    {
                        Success = false,
                        Message = $"Connection '{connectionName}' not found",
                        RequiresLogout = false
                    };
                }

                // Set as current connection using OpenDataSource
                _editor.OpenDataSource(connectionName);
                _currentConnectionName = connectionName; // Store the current connection name
                _editor.ConfigEditor?.SaveDataconnectionsValues();

                return new SetCurrentDatabaseResult
                {
                    Success = true,
                    Message = $"Connection '{connectionName}' set as current",
                    RequiresLogout = requiresLogout
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting current connection {ConnectionName}", connectionName);
                return new SetCurrentDatabaseResult
                {
                    Success = false,
                    Message = "Failed to set current connection",
                    ErrorDetails = ex.Message,
                    RequiresLogout = false
                };
            }
        }

        /// <summary>
        /// Update an existing connection
        /// </summary>
        public SaveConnectionResult UpdateConnection(string originalConnectionName, ConnectionConfig config, bool testAfterSave = true)
        {
            try
            {
                var driverInfo = GetDriverInfo(config.DatabaseType);
                if (driverInfo == null)
                {
                    return new SaveConnectionResult
                    {
                        Success = false,
                        Message = $"Unknown database type: {config.DatabaseType}"
                    };
                }

                // Find existing connection
                var existingConnection = _editor.ConfigEditor?.DataConnections?
                    .FirstOrDefault(c => c.ConnectionName == originalConnectionName);

                if (existingConnection == null)
                {
                    return new SaveConnectionResult
                    {
                        Success = false,
                        Message = $"Connection '{originalConnectionName}' not found"
                    };
                }

                // Update connection properties
                existingConnection.ConnectionName = config.ConnectionName;
                existingConnection.DatabaseType = ParseDataSourceType(driverInfo.DataSourceType);
                existingConnection.DriverName = driverInfo.NuGetPackage;
                existingConnection.Host = config.Host;
                existingConnection.Port = config.Port > 0 ? config.Port : driverInfo.DefaultPort;
                existingConnection.Database = config.Database;
                existingConnection.UserID = config.Username ?? string.Empty;
                existingConnection.Password = config.Password ?? string.Empty;
                existingConnection.ConnectionString = config.ConnectionString ?? string.Empty;

                // If name changed and new name already exists (and it's not the same connection), fail
                if (config.ConnectionName != originalConnectionName &&
                    _editor.ConfigEditor.DataConnectionExist(config.ConnectionName))
                {
                    return new SaveConnectionResult
                    {
                        Success = false,
                        Message = $"Connection name '{config.ConnectionName}' already exists"
                    };
                }

                // Save connections
                _editor.ConfigEditor.SaveDataconnectionsValues();

                ConnectionTestResult? testResult = null;
                if (testAfterSave)
                {
                    testResult = TestConnectionAsync(config).GetAwaiter().GetResult();
                }

                return new SaveConnectionResult
                {
                    Success = true,
                    Message = $"Connection '{config.ConnectionName}' updated successfully",
                    TestResult = testResult
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating connection {ConnectionName}", originalConnectionName);
                return new SaveConnectionResult
                {
                    Success = false,
                    Message = "Failed to update connection",
                    ErrorDetails = ex.Message
                };
            }
        }

        /// <summary>
        /// Delete a connection
        /// </summary>
        public DeleteConnectionResult DeleteConnection(string connectionName)
        {
            try
            {
                var connection = _editor.ConfigEditor?.DataConnections?
                    .FirstOrDefault(c => c.ConnectionName == connectionName);

                if (connection == null)
                {
                    return new DeleteConnectionResult
                    {
                        Success = false,
                        Message = $"Connection '{connectionName}' not found"
                    };
                }

                // Check if it's the current connection
                var currentConnectionName = GetCurrentConnectionName();
                if (connectionName == currentConnectionName)
                {
                    return new DeleteConnectionResult
                    {
                        Success = false,
                        Message = "Cannot delete the current connection. Please set a different connection as current first."
                    };
                }

                // Remove connection
                bool removed = RemoveDataConnection(connectionName);
                if (!removed)
                {
                    return new DeleteConnectionResult
                    {
                        Success = false,
                        Message = "Failed to remove connection"
                    };
                }

                // Save connections
                _editor.ConfigEditor.SaveDataconnectionsValues();

                return new DeleteConnectionResult
                {
                    Success = true,
                    Message = $"Connection '{connectionName}' deleted successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting connection {ConnectionName}", connectionName);
                return new DeleteConnectionResult
                {
                    Success = false,
                    Message = "Failed to delete connection",
                    ErrorDetails = ex.Message
                };
            }
        }

        /// <summary>
        /// Get connection details by name
        /// </summary>
        public ConnectionConfig? GetConnectionByName(string connectionName)
        {
            try
            {
                var connection = _editor.ConfigEditor?.DataConnections?
                    .FirstOrDefault(c => c.ConnectionName == connectionName);

                if (connection == null)
                    return null;

                // Convert DatabaseType enum to string for comparison
                var connectionDbTypeString = connection.DatabaseType.ToString();
                var dbType = DatabaseDrivers.FirstOrDefault(d => 
                    d.Value.DataSourceType.Equals(connectionDbTypeString, StringComparison.OrdinalIgnoreCase) ||
                    d.Key.Equals(connectionDbTypeString, StringComparison.OrdinalIgnoreCase)).Key ?? "Unknown";

                return new ConnectionConfig
                {
                    ConnectionName = connection.ConnectionName ?? string.Empty,
                    DatabaseType = dbType,
                    Host = connection.Host ?? string.Empty,
                    Port = connection.Port,
                    Database = connection.Database ?? string.Empty,
                    Schema = null, // Schema is not stored separately in ConnectionProperties - may need to extract from connection string
                    CreateSchemaIfNotExists = false,
                    Username = connection.UserID,
                    Password = connection.Password, // Note: password might be encrypted
                    ConnectionString = connection.ConnectionString
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting connection {ConnectionName}", connectionName);
                return null;
            }
        }

        /// <summary>
        /// Save connection to ConfigEditor
        /// </summary>
        public SaveConnectionResult SaveConnection(ConnectionConfig config, bool testAfterSave = true, bool openAfterSave = false)
        {
            try
            {
                var driverInfo = GetDriverInfo(config.DatabaseType);
                if (driverInfo == null)
                {
                    return new SaveConnectionResult
                    {
                        Success = false,
                        Message = $"Unknown database type: {config.DatabaseType}"
                    };
                }

                // Check if connection already exists
                if (_editor.ConfigEditor.DataConnectionExist(config.ConnectionName))
                {
                    return new SaveConnectionResult
                    {
                        Success = false,
                        Message = $"Connection '{config.ConnectionName}' already exists"
                    };
                }

                // Create connection properties
                var connectionProperties = new ConnectionProperties
                {
                    ConnectionName = config.ConnectionName,
                    DatabaseType = ParseDataSourceType(driverInfo.DataSourceType),
                    DriverName = driverInfo.NuGetPackage,
                    Host = config.Host,
                    Port = config.Port > 0 ? config.Port : driverInfo.DefaultPort,
                    Database = config.Database,
                    UserID = config.Username ?? string.Empty,
                    Password = config.Password ?? string.Empty,
                    ConnectionString = config.ConnectionString ?? string.Empty,
                    GuidID = Guid.NewGuid().ToString()
                };

                // Add connection
                bool added = _editor.ConfigEditor.AddDataConnection(connectionProperties);
                if (!added)
                {
                    return new SaveConnectionResult
                    {
                        Success = false,
                        Message = "Failed to add connection"
                    };
                }

                // Save connections
                _editor.ConfigEditor.SaveDataconnectionsValues();

                ConnectionTestResult? testResult = null;
                if (testAfterSave)
                {
                    testResult = TestConnectionAsync(config).GetAwaiter().GetResult();
                }

                // Optionally open the connection
                if (openAfterSave && testResult?.Success == true)
                {
                    _editor.OpenDataSource(config.ConnectionName);
                }

                return new SaveConnectionResult
                {
                    Success = true,
                    Message = $"Connection '{config.ConnectionName}' saved successfully",
                    TestResult = testResult
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving connection {ConnectionName}", config.ConnectionName);
                return new SaveConnectionResult
                {
                    Success = false,
                    Message = "Failed to save connection",
                    ErrorDetails = ex.Message
                };
            }
        }

        /// <summary>
        /// Get scripts directory path
        /// </summary>
        private string GetScriptsPath(string scriptPath)
        {
            // Try to find the Scripts directory relative to the API service
            var basePath = AppContext.BaseDirectory;
            
            // Get the solution root (go up from ApiService/bin/Debug/netX.X to solution root)
            var solutionRoot = Path.GetFullPath(Path.Combine(basePath, "..", "..", "..", "..", ".."));
            
            var possiblePaths = new[]
            {
                // Relative to solution root
                Path.Combine(solutionRoot, "Beep.OilandGas.PPDM39", "Scripts", scriptPath),
                // Relative to current directory
                Path.Combine(basePath, "Scripts", scriptPath),
                // Alternative relative paths
                Path.Combine(basePath, "..", "..", "..", "..", "Beep.OilandGas.PPDM39", "Scripts", scriptPath),
                Path.Combine(basePath, "..", "Beep.OilandGas.PPDM39", "Scripts", scriptPath),
                // If running from project directory
                Path.Combine(solutionRoot, "..", "Beep.OilandGas.PPDM39", "Scripts", scriptPath)
            };

            foreach (var path in possiblePaths)
            {
                var fullPath = Path.GetFullPath(path);
                if (Directory.Exists(fullPath))
                {
                    _logger.LogDebug("Found scripts path: {Path}", fullPath);
                    return fullPath;
                }
            }

            // Return the most likely path (solution root)
            var defaultPath = Path.Combine(solutionRoot, "Beep.OilandGas.PPDM39", "Scripts", scriptPath);
            _logger.LogWarning("Scripts directory not found, using default path: {Path}", defaultPath);
            return Path.GetFullPath(defaultPath);
        }

        /// <summary>
        /// Split SQL script into executable statements
        /// </summary>
        private List<string> SplitScriptIntoStatements(string scriptContent, string databaseType)
        {
            var statements = new List<string>();

            // Remove comments (lines starting with --)
            var lines = scriptContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var cleanedLines = new List<string>();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                // Skip comment lines
                if (trimmedLine.StartsWith("--") || string.IsNullOrWhiteSpace(trimmedLine))
                    continue;

                // Skip GO statements (SQL Server batch separator)
                if (databaseType == "SqlServer" && trimmedLine.Equals("GO", StringComparison.OrdinalIgnoreCase))
                    continue;

                cleanedLines.Add(line);
            }

            var cleanedScript = string.Join("\n", cleanedLines);

                    // Split by semicolon (standard SQL statement separator)
                    // Use regex to split on semicolons that are not inside string literals
                    var parts = Regex.Split(cleanedScript, @";\s*(?=\n|$)")
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrWhiteSpace(s) && s.Length > 5) // Filter out very short fragments
                        .ToList();

                    statements.AddRange(parts);

            return statements;
        }

        /// <summary>
        /// Drop a database or schema
        /// </summary>
        public async Task<DropDatabaseResult> DropDatabaseAsync(string connectionName, string? schemaName, bool dropIfExists = true)
        {
            try
            {
                var connection = GetConnectionByName(connectionName);
                if (connection == null)
                {
                    return new DropDatabaseResult
                    {
                        Success = false,
                        Message = $"Connection '{connectionName}' not found"
                    };
                }

                // TODO: Implement database/schema dropping logic
                // This is a placeholder implementation
                _logger.LogWarning("DropDatabaseAsync not yet fully implemented for connection {ConnectionName}", connectionName);
                
                return new DropDatabaseResult
                {
                    Success = false,
                    Message = "Database dropping functionality is not yet implemented"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dropping database for connection {ConnectionName}", connectionName);
                return new DropDatabaseResult
                {
                    Success = false,
                    Message = $"Failed to drop database: {ex.Message}",
                    ErrorDetails = ex.Message
                };
            }
        }

        /// <summary>
        /// Recreate a database or schema (drop and recreate)
        /// </summary>
        public async Task<RecreateDatabaseResult> RecreateDatabaseAsync(string connectionName, string? schemaName, bool backupFirst = false, string? backupPath = null)
        {
            try
            {
                var connection = GetConnectionByName(connectionName);
                if (connection == null)
                {
                    return new RecreateDatabaseResult
                    {
                        Success = false,
                        Message = $"Connection '{connectionName}' not found"
                    };
                }

                // TODO: Implement database recreation logic
                // This is a placeholder implementation
                _logger.LogWarning("RecreateDatabaseAsync not yet fully implemented for connection {ConnectionName}", connectionName);
                
                return new RecreateDatabaseResult
                {
                    Success = false,
                    Message = "Database recreation functionality is not yet implemented"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recreating database for connection {ConnectionName}", connectionName);
                return new RecreateDatabaseResult
                {
                    Success = false,
                    Message = $"Failed to recreate database: {ex.Message}",
                    ErrorDetails = ex.Message
                };
            }
        }

        /// <summary>
        /// Copy database from source to target (ETL operation)
        /// </summary>
        public async Task<CopyDatabaseResult> CopyDatabaseAsync(CopyDatabaseRequest request, string? operationId = null)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.SourceConnectionName) || string.IsNullOrEmpty(request.TargetConnectionName))
                {
                    return new CopyDatabaseResult
                    {
                        Success = false,
                        Message = "Source and target connection names are required"
                    };
                }

                var sourceConnection = GetConnectionByName(request.SourceConnectionName);
                var targetConnection = GetConnectionByName(request.TargetConnectionName);

                if (sourceConnection == null)
                {
                    return new CopyDatabaseResult
                    {
                        Success = false,
                        Message = $"Source connection '{request.SourceConnectionName}' not found"
                    };
                }

                if (targetConnection == null)
                {
                    return new CopyDatabaseResult
                    {
                        Success = false,
                        Message = $"Target connection '{request.TargetConnectionName}' not found"
                    };
                }

                // TODO: Implement database copying (ETL) logic
                // This is a placeholder implementation
                _logger.LogWarning("CopyDatabaseAsync not yet fully implemented from {Source} to {Target}", 
                    request.SourceConnectionName, request.TargetConnectionName);

                if (!string.IsNullOrEmpty(operationId) && _progressTracking != null)
                {
                    _progressTracking.UpdateProgress(operationId, 50, "Database copy operation not yet implemented");
                }

                return new CopyDatabaseResult
                {
                    Success = false,
                    Message = "Database copying (ETL) functionality is not yet implemented"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error copying database from {Source} to {Target}", 
                    request?.SourceConnectionName, request?.TargetConnectionName);
                
                if (!string.IsNullOrEmpty(operationId) && _progressTracking != null)
                {
                    _progressTracking.CompleteOperation(operationId, false, "Database copy failed", ex.Message);
                }

                return new CopyDatabaseResult
                {
                    Success = false,
                    Message = $"Failed to copy database: {ex.Message}",
                    ErrorDetails = ex.Message
                };
            }
        }

        #region DataManager Helper Methods

        /// <summary>
        /// Gets or creates IDataSource from ConnectionConfig
        /// </summary>
        private IDataSource GetDataSourceFromConnectionConfig(ConnectionConfig config)
        {
            var driverInfo = GetDriverInfo(config.DatabaseType);
            if (driverInfo == null)
            {
                throw new InvalidOperationException($"Unknown database type: {config.DatabaseType}");
            }

            // Check if connection already exists
            if (_editor.ConfigEditor.DataConnectionExist(config.ConnectionName))
            {
                var dataSource = _editor.GetDataSource(config.ConnectionName);
                if (dataSource != null)
                {
                    return dataSource;
                }
            }

            // Create new connection
            var connectionProperties = new ConnectionProperties
            {
                ConnectionName = config.ConnectionName,
                DatabaseType = ParseDataSourceType(driverInfo.DataSourceType),
                DriverName = driverInfo.NuGetPackage,
                Host = config.Host,
                Port = config.Port > 0 ? config.Port : driverInfo.DefaultPort,
                Database = config.Database,
                UserID = config.Username ?? string.Empty,
                Password = config.Password ?? string.Empty,
                ConnectionString = config.ConnectionString ?? string.Empty,
                GuidID = Guid.NewGuid().ToString()
            };

            if (!_editor.ConfigEditor.DataConnectionExist(config.ConnectionName))
            {
                _editor.ConfigEditor.AddDataConnection(connectionProperties);
                _editor.ConfigEditor.SaveDataconnectionsValues();
            }

            var newDataSource = _editor.GetDataSource(config.ConnectionName);
            if (newDataSource == null)
            {
                throw new InvalidOperationException($"Failed to create data source for connection: {config.ConnectionName}");
            }

            return newDataSource;
        }


        /// <summary>
        /// Gets script description based on file name and script type
        /// </summary>
        private string GetScriptDescription(string fileName, ScriptType scriptType)
        {
            // Try to get from existing ScriptDescriptions first
            if (ScriptDescriptions.TryGetValue(fileName, out var description))
            {
                return description;
            }

            // Generate description based on script type
            return scriptType switch
            {
                ScriptType.TAB => "Create Tables and Columns",
                ScriptType.PK => "Create Primary Keys",
                ScriptType.FK => "Create Foreign Key Constraints",
                ScriptType.IX => "Create Indexes",
                ScriptType.CK => "Create Check Constraints",
                ScriptType.OUOM => "Create Original Units of Measure Foreign Keys",
                ScriptType.UOM => "Create Units of Measure Foreign Keys",
                ScriptType.RQUAL => "Create ROW_QUALITY Foreign Keys",
                ScriptType.RSRC => "Create SOURCE Foreign Keys",
                ScriptType.TCM => "Create Table Comments",
                ScriptType.CCM => "Create Column Comments",
                ScriptType.SYN => "Create Synonyms",
                ScriptType.GUID => "Create GUID Constraints",
                _ => $"Execute {fileName}"
            };
        }

        /// <summary>
        /// Maps ModuleExecutionResult to AllScriptsExecutionResult
        /// </summary>
        private AllScriptsExecutionResult MapModuleExecutionResultToAllScriptsExecutionResult(ModuleExecutionResult moduleResult)
        {
            var results = moduleResult.ScriptResults.Select(sr => MapScriptExecutionResultToDto(sr)).ToList();

            return new AllScriptsExecutionResult
            {
                Results = results,
                TotalScripts = moduleResult.TotalScripts,
                SuccessfulScripts = moduleResult.SuccessfulScripts,
                FailedScripts = moduleResult.FailedScripts,
                AllSucceeded = moduleResult.Success,
                TotalExecutionTime = moduleResult.Duration
            };
        }

        /// <summary>
        /// Maps ScriptExecutionResult from DataManager to ScriptExecutionResultDto DTO
        /// DataManager returns ScriptExecutionResult from PPDM39.DataManagement namespace
        /// </summary>
        private Beep.OilandGas.Models.DTOs.DataManagement.ScriptExecutionResultDto MapScriptExecutionResultToDto(Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation.ScriptExecutionResult dataManagerResult)
        {
            return new Beep.OilandGas.Models.DTOs.DataManagement.ScriptExecutionResultDto
            {
                ScriptFileName = dataManagerResult.ScriptFileName,
                Success = dataManagerResult.Success,
                ErrorMessage = dataManagerResult.ErrorMessage,
                StartTime = dataManagerResult.StartTime,
                EndTime = dataManagerResult.EndTime,
                Duration = dataManagerResult.Duration,
                RowsAffected = dataManagerResult.RowsAffected,
                ExecutionLog = dataManagerResult.ExecutionLog
            };
        }

        /// <summary>
        /// Finds module that contains a specific script file
        /// </summary>
        private async Task<IModuleData?> FindModuleByScriptNameAsync(string scriptName, string databaseType)
        {
            var allModules = ModuleDataRegistry.GetAllModules();
            var normalizedDbType = DatabaseTypeNormalizer.Normalize(databaseType);

            foreach (var module in allModules)
            {
                var scripts = await module.GetScriptsAsync(normalizedDbType);
                if (scripts.Any(s => s.FileName.Equals(scriptName, StringComparison.OrdinalIgnoreCase)))
                {
                    return module;
                }
            }

            return null;
        }

        /// <summary>
        /// Determines module name from script name (for backward compatibility)
        /// </summary>
        private string? DetermineModuleFromScriptName(string scriptName)
        {
            // Consolidated PPDM39 scripts (TAB.sql, PK.sql, etc.)
            var consolidatedScripts = new[] { "TAB.sql", "PK.sql", "FK.sql", "CK.sql", "IX.sql", 
                "OUOM.sql", "UOM.sql", "RQUAL.sql", "RSRC.sql", "TCM.sql", "CCM.sql", "SYN.sql", "GUID.sql" };
            
            if (consolidatedScripts.Any(s => s.Equals(scriptName, StringComparison.OrdinalIgnoreCase)))
            {
                return "PPDM39";
            }

            // Individual table scripts (TABLE_TAB.sql, TABLE_PK.sql, etc.)
            // Extract table name and try to find module
            // This is a simplified approach - in practice, we'd scan all modules
            return null; // Will be resolved by FindModuleByScriptNameAsync
        }

        #endregion
    }
}
