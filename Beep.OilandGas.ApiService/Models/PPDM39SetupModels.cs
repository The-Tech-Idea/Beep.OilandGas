using TheTechIdea.Beep.DataBase;

namespace Beep.OilandGas.ApiService.Models
{
    /// <summary>
    /// Connection configuration for PPDM39 database setup
    /// </summary>
    public class ConnectionConfig
    {
        public string DatabaseType { get; set; } = string.Empty;
        public string ConnectionName { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Database { get; set; } = string.Empty;
        public string? Schema { get; set; } // Schema name (for PostgreSQL, SQL Server, etc.)
        public bool CreateSchemaIfNotExists { get; set; } = false;
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? ConnectionString { get; set; }
    }

    /// <summary>
    /// Response for connection test
    /// </summary>
    public class ConnectionTestResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorDetails { get; set; }
    }

    /// <summary>
    /// Driver information
    /// </summary>
    public class DriverInfo
    {
        public string DatabaseType { get; set; } = string.Empty;
        public string NuGetPackage { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public bool IsInstalled { get; set; }
        public string? StatusMessage { get; set; }
    }

    /// <summary>
    /// Request to install a driver
    /// </summary>
    public class InstallDriverRequest
    {
        public string DatabaseType { get; set; } = string.Empty;
        public string NuGetPackage { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response from driver installation
    /// </summary>
    public class InstallDriverResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorDetails { get; set; }
    }

    /// <summary>
    /// Available script information
    /// </summary>
    public class ScriptInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsMandatory { get; set; }
        public int ExecutionOrder { get; set; }
        public bool IsSelected { get; set; } = true;
    }

    /// <summary>
    /// Script execution request
    /// </summary>
    public class ScriptExecutionRequest
    {
        public ConnectionConfig Connection { get; set; } = null!;
        public List<string> ScriptNames { get; set; } = new();
        public bool ExecuteAll { get; set; } = true;
    }

    /// <summary>
    /// Script execution result
    /// </summary>
    public class ScriptExecutionResult
    {
        public string ScriptName { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorDetails { get; set; }
        public TimeSpan ExecutionTime { get; set; }
    }

    /// <summary>
    /// All scripts execution result
    /// </summary>
    public class AllScriptsExecutionResult
    {
        public List<ScriptExecutionResult> Results { get; set; } = new();
        public int TotalScripts { get; set; }
        public int SuccessfulScripts { get; set; }
        public int FailedScripts { get; set; }
        public bool AllSucceeded { get; set; }
        public TimeSpan TotalExecutionTime { get; set; }
    }

    /// <summary>
    /// Save connection request
    /// </summary>
    public class SaveConnectionRequest
    {
        public ConnectionConfig Connection { get; set; } = null!;
        public bool TestAfterSave { get; set; } = true;
        public bool OpenAfterSave { get; set; } = false;
    }

    /// <summary>
    /// Save connection result
    /// </summary>
    public class SaveConnectionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorDetails { get; set; }
        public ConnectionTestResult? TestResult { get; set; }
    }

    /// <summary>
    /// Database driver configuration
    /// </summary>
    public class DatabaseDriverInfo
    {
        public string NuGetPackage { get; set; } = string.Empty;
        public string DataSourceType { get; set; } = string.Empty; // Stored as string representation
        public int DefaultPort { get; set; }
        public string ScriptPath { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
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
    /// Set current database request
    /// </summary>
    public class SetCurrentDatabaseRequest
    {
        public string ConnectionName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Set current database result
    /// </summary>
    public class SetCurrentDatabaseResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorDetails { get; set; }
        public bool RequiresLogout { get; set; }
    }

    /// <summary>
    /// Update connection request
    /// </summary>
    public class UpdateConnectionRequest
    {
        public string OriginalConnectionName { get; set; } = string.Empty;
        public ConnectionConfig Connection { get; set; } = null!;
    }

    /// <summary>
    /// Delete connection request
    /// </summary>
    public class DeleteConnectionRequest
    {
        public string ConnectionName { get; set; } = string.Empty;
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

    /// <summary>
    /// Schema privilege check request
    /// </summary>
    public class SchemaPrivilegeCheckRequest
    {
        public ConnectionConfig Connection { get; set; } = null!;
        public string? SchemaName { get; set; }
    }

    /// <summary>
    /// Schema privilege check result
    /// </summary>
    public class SchemaPrivilegeCheckResult
    {
        public bool HasCreatePrivilege { get; set; }
        public bool SchemaExists { get; set; }
        public List<string> ExistingSchemas { get; set; } = new();
        public string Message { get; set; } = string.Empty;
        public string? ErrorDetails { get; set; }
    }

    /// <summary>
    /// Create schema request
    /// </summary>
    public class CreateSchemaRequest
    {
        public ConnectionConfig Connection { get; set; } = null!;
        public string SchemaName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Create schema result
    /// </summary>
    public class CreateSchemaResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorDetails { get; set; }
    }

    /// <summary>
    /// Drop database request
    /// </summary>
    public class DropDatabaseRequest
    {
        public string ConnectionName { get; set; } = string.Empty;
        public string? SchemaName { get; set; }
        public bool DropIfExists { get; set; } = true;
    }

    /// <summary>
    /// Drop database result
    /// </summary>
    public class DropDatabaseResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorDetails { get; set; }
    }

    /// <summary>
    /// Recreate database request
    /// </summary>
    public class RecreateDatabaseRequest
    {
        public string ConnectionName { get; set; } = string.Empty;
        public string? SchemaName { get; set; }
        public bool BackupFirst { get; set; } = false;
        public string? BackupPath { get; set; }
    }

    /// <summary>
    /// Recreate database result
    /// </summary>
    public class RecreateDatabaseResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorDetails { get; set; }
        public bool BackupCreated { get; set; }
        public string? BackupPath { get; set; }
    }

    /// <summary>
    /// Copy database (ETL) request
    /// </summary>
    public class CopyDatabaseRequest
    {
        public string SourceConnectionName { get; set; } = string.Empty;
        public string TargetConnectionName { get; set; } = string.Empty;
        public string? SourceSchema { get; set; }
        public string? TargetSchema { get; set; }
        public bool CopyStructureOnly { get; set; } = false;
        public bool CopyData { get; set; } = true;
        public bool CopyIndexes { get; set; } = true;
        public bool CopyConstraints { get; set; } = true;
        public List<string>? TablesToCopy { get; set; } // If null, copy all tables
        public bool TruncateTargetTables { get; set; } = false;
    }

    /// <summary>
    /// Copy database (ETL) progress update
    /// </summary>
    public class CopyDatabaseProgress
    {
        public string CurrentTable { get; set; } = string.Empty;
        public int TotalTables { get; set; }
        public int CompletedTables { get; set; }
        public long TotalRows { get; set; }
        public long CopiedRows { get; set; }
        public bool IsComplete { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Operation start response (returns operation ID for progress tracking)
    /// </summary>
    public class OperationStartResponse
    {
        public string OperationId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Copy database (ETL) result
    /// </summary>
    public class CopyDatabaseResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorDetails { get; set; }
        public int TablesCopied { get; set; }
        public long RowsCopied { get; set; }
        public TimeSpan Duration { get; set; }
        public List<string>? Errors { get; set; }
    }
}
