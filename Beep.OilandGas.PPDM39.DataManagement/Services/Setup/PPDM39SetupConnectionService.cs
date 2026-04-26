using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.DataManagement;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using BeepDataSourceType = TheTechIdea.Beep.Utilities.DataSourceType;

namespace Beep.OilandGas.PPDM39.DataManagement.Services.Setup
{
    /// <summary>
    /// Focused implementation of <see cref="IPPDM39SetupConnectionService"/>.
    /// Owns all connection lifecycle operations: driver resolution, CRUD, test,
    /// enumeration, and provider capability reporting.
    /// <para>
    /// This service replaces inline controller-level connection logic and provides
    /// the single canonical path for setup paths to create, persist, and validate
    /// connections through BeepDM's <see cref="IDMEEditor"/>.
    /// </para>
    /// </summary>
    public class PPDM39SetupConnectionService : IPPDM39SetupConnectionService
    {
        private readonly IDMEEditor _editor;
        private readonly ILogger<PPDM39SetupConnectionService> _logger;
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

        public PPDM39SetupConnectionService(IDMEEditor editor, ILogger<PPDM39SetupConnectionService> logger)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ── DRIVER RESOLUTION ─────────────────────────────────────────────────

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

        public DatabaseDriverInfo? GetDriverInfo(string databaseType)
        {
            if (string.IsNullOrWhiteSpace(databaseType)) return null;

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

            return _staticDriverMap.TryGetValue(databaseType, out var info) ? info : null;
        }

        public DriverInfo CheckDriver(string databaseType)
        {
            var info = GetDriverInfo(databaseType);
            if (info == null)
                return new DriverInfo { DatabaseType = databaseType, IsAvailable = false, IsInstalled = false };

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

        public bool InstallDriver(string databaseType)
        {
            // Driver installation is handled by BeepDM's package manager at startup.
            // This method signals to the wizard whether the driver is present.
            var check = CheckDriver(databaseType);
            return check.IsInstalled;
        }

        // ── PROVIDER CAPABILITY ───────────────────────────────────────────────

        public ProviderCapabilityInfo GetProviderCapabilities(string databaseType)
        {
            var dsType = ParseDataSourceType(databaseType);
            var driverCheck = CheckDriver(databaseType);

            return new ProviderCapabilityInfo
            {
                ProviderType                  = dsType.ToString(),
                IsDriverInstalled             = driverCheck.IsInstalled,
                NuGetPackage                  = driverCheck.NuGetPackage,
                SupportsLocalFileCreate       = dsType == BeepDataSourceType.SqlLite,
                RequiresNetworkHost           = dsType != BeepDataSourceType.SqlLite,
                SupportsSchemaNamespace       = dsType == BeepDataSourceType.SqlServer || dsType == BeepDataSourceType.Oracle || dsType == BeepDataSourceType.Postgre,
                SupportsSafeAdditiveMigration = true,
                SupportsDestructiveOperations = true,
                SupportsCheckpointResume      = true,
                SupportsDryRunArtifacts       = true,
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

        // ── CONNECTION CRUD ───────────────────────────────────────────────────

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
                ConnectionName   = dc.ConnectionName,
                DatabaseType     = dc.DatabaseType.ToString(),
                Host             = dc.Host,
                Port             = dc.Port,
                Database         = dc.Database,
                Username         = dc.UserID,
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

        // ── PRIVATE HELPERS ───────────────────────────────────────────────────

        /// <summary>
        /// Build <see cref="ConnectionProperties"/> from a <see cref="ConnectionConfig"/>,
        /// resolving the best available driver via <c>ConnectionHelper.GetBestMatchingDriver</c>
        /// and falling back to the loaded driver-class list when reflection lookup fails.
        /// File paths for local providers are normalized using the environment-aware storage root.
        /// </summary>
        private ConnectionProperties BuildConnectionProperties(ConnectionConfig config, string connectionName)
        {
            var dsType = ParseDataSourceType(config.DatabaseType);
            var props = new ConnectionProperties
            {
                ConnectionName   = connectionName,
                DatabaseType     = dsType,
                Host             = config.Host ?? string.Empty,
                Port             = config.Port,
                Database         = config.Database ?? string.Empty,
                UserID           = config.Username ?? string.Empty,
                Password         = config.Password ?? string.Empty,
                ConnectionString = config.ConnectionString ?? string.Empty,
                GuidID           = Guid.NewGuid().ToString()
            };

            PopulateBestDriver(props);

            // Normalize local file paths using a deterministic storage root
            if (dsType == BeepDataSourceType.SqlLite && string.IsNullOrWhiteSpace(props.FilePath))
            {
                props.FilePath = GetLocalDatabaseFolder();
            }

            return props;
        }

        /// <summary>
        /// Returns the canonical folder for local database files.
        /// Uses an environment-aware path: prefers <c>BEEP_DB_ROOT</c> environment variable
        /// before falling back to <c>&lt;AppBaseDir&gt;/Databases</c>.
        /// The folder is created if it does not exist.
        /// </summary>
        internal static string GetLocalDatabaseFolder()
        {
            var root = Environment.GetEnvironmentVariable("BEEP_DB_ROOT");
            var folder = string.IsNullOrWhiteSpace(root)
                ? Path.Combine(AppContext.BaseDirectory, "Databases")
                : root;

            Directory.CreateDirectory(folder);
            return folder;
        }

        private void PopulateBestDriver(ConnectionProperties props)
        {
            try
            {
                var helperType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly =>
                    {
                        try { return assembly.GetTypes(); }
                        catch { return Array.Empty<Type>(); }
                    })
                    .FirstOrDefault(t => t.FullName == "TheTechIdea.Beep.Helpers.ConnectionHelper");

                var method = helperType?.GetMethod("GetBestMatchingDriver",
                    new[] { typeof(ConnectionProperties), _editor.ConfigEditor.GetType() });

                var driver = method?.Invoke(null, new object[] { props, _editor.ConfigEditor });
                if (driver == null)
                    goto Fallback;

                var packageName = driver.GetType().GetProperty("PackageName")?.GetValue(driver)?.ToString();
                var version = driver.GetType().GetProperty("version")?.GetValue(driver)?.ToString()
                    ?? driver.GetType().GetProperty("Version")?.GetValue(driver)?.ToString();

                if (!string.IsNullOrWhiteSpace(packageName))
                    props.DriverName = packageName;
                if (!string.IsNullOrWhiteSpace(version))
                    props.DriverVersion = version;
                return;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "ConnectionHelper.GetBestMatchingDriver lookup failed; using fallback for {ConnectionName}", props.ConnectionName);
            }

            Fallback:
            var driverClass = _editor.ConfigEditor?.DataDriversClasses?
                .FirstOrDefault(d => d.DatasourceType == props.DatabaseType);
            if (driverClass != null)
            {
                props.DriverName = driverClass.PackageName;
                props.DriverVersion = driverClass.version;
            }
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
                "sqlserver"               => BeepDataSourceType.SqlServer,
                "postgre" or "postgresql" => BeepDataSourceType.Postgre,
                "mysql"  or "mariadb"     => BeepDataSourceType.Mysql,
                "oracle"                  => BeepDataSourceType.Oracle,
                "sqlite" or "sqllite"     => BeepDataSourceType.SqlLite,
                _                         => BeepDataSourceType.SqlServer
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
    }
}
