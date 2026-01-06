using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.Models.DTOs.DataManagement;
using TheTechIdea.Beep.ConfigUtil;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for PPDM39 database setup operations
    /// Platform-agnostic interface for database setup, script execution, and connection management
    /// </summary>
    public interface IPPDM39SetupService
    {
        void SetProgressTracking(IProgressTrackingService progressTracking);
        DatabaseDriverInfo? GetDriverInfo(string databaseType);
        List<string> GetAvailableDatabaseTypes();
        DriverInfo CheckDriver(string databaseType);
        Task<SchemaPrivilegeCheckResult> CheckSchemaPrivilegesAsync(ConnectionConfig config, string? schemaName = null);
        Task<CreateSchemaResult> CreateSchemaAsync(ConnectionConfig config, string schemaName);
        Task<ConnectionTestResult> TestConnectionAsync(ConnectionConfig config);
        Task<List<ScriptInfoDto>> GetAvailableScriptsAsync(string databaseType);
        List<ScriptInfoDto> GetAvailableScripts(string databaseType);
        Task<ScriptExecutionResultDto> ExecuteScriptAsync(ConnectionConfig config, string scriptName, string? operationId = null);
        Task<Beep.OilandGas.Models.DTOs.AllScriptsExecutionResult> ExecuteAllScriptsAsync(ConnectionConfig config, List<string> scriptNames, string? operationId = null);
        List<DatabaseConnectionListItem> GetAllConnections();
        string? GetCurrentConnectionName();
        SetCurrentDatabaseResult SetCurrentConnection(string connectionName);
        SaveConnectionResult UpdateConnection(string originalConnectionName, ConnectionConfig config, bool testAfterSave = true);
        DeleteConnectionResult DeleteConnection(string connectionName);
        ConnectionConfig? GetConnectionByName(string connectionName);
        SaveConnectionResult SaveConnection(ConnectionConfig config, bool testAfterSave = true, bool openAfterSave = false);
        Task<DropDatabaseResult> DropDatabaseAsync(string connectionName, string? schemaName, bool dropIfExists = true);
        Task<RecreateDatabaseResult> RecreateDatabaseAsync(string connectionName, string? schemaName, bool backupFirst = false, string? backupPath = null);
        Task<CopyDatabaseResult> CopyDatabaseAsync(CopyDatabaseRequest request, string? operationId = null);
    }

    /// <summary>
    /// Database driver information
    /// </summary>
    public class DatabaseDriverInfo
    {
        public string NuGetPackage { get; set; } = string.Empty;
        public string DataSourceType { get; set; } = string.Empty;
        public int DefaultPort { get; set; }
        public string ScriptPath { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Driver availability information
    /// </summary>
    public class DriverInfo
    {
        public string DatabaseType { get; set; } = string.Empty;
        public string? NuGetPackage { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsInstalled { get; set; }
        public string StatusMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// Schema privilege check result
    /// </summary>
    public class SchemaPrivilegeCheckResult
    {
        public bool HasCreatePrivilege { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorDetails { get; set; }
        public List<string> ExistingSchemas { get; set; } = new List<string>();
    }

    /// <summary>
    /// Create schema result
    /// </summary>
    public class CreateSchemaResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorDetails { get; set; }
        public string? SchemaName { get; set; }
    }

    /// <summary>
    /// Database connection list item
    /// </summary>
    public class DatabaseConnectionListItem
    {
        public string ConnectionName { get; set; } = string.Empty;
        public string DatabaseType { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Database { get; set; } = string.Empty;
        public string? Username { get; set; }
        public bool IsCurrent { get; set; }
        public string GuidId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Delete connection result
    /// </summary>
    public class DeleteConnectionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorDetails { get; set; }
    }
}



