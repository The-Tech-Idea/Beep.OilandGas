using System;
using System.Collections.Generic;
using TheTechIdea.Beep.ConfigUtil;
using Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation;

namespace Beep.OilandGas.ApiService.Models
{
    public class ConnectionConfig
    {
        public string DatabaseType { get; set; } = string.Empty;
        public string ConnectionName { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Database { get; set; } = string.Empty;
        public string? Schema { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public ConnectionProperties ToConnectionProperties()
        {
            return new ConnectionProperties();
        }
    }

    public class ConnectionTestResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorDetails { get; set; }
    }

    public class DriverInfo
    {
        public bool IsInstalled { get; set; }
        public string? NuGetPackage { get; set; }
        public string? StatusMessage { get; set; }
    }

    public class CheckDriverRequest
    {
        public string DatabaseType { get; set; } = string.Empty;
    }

    public class ScriptExecutionRequest
    {
        public ConnectionConfig? Connection { get; set; }
        public string ScriptName { get; set; } = string.Empty;
    }

    public class AllScriptsExecutionResult
    {
        public bool AllSucceeded { get; set; }
        public int TotalScripts { get; set; }
        public int SuccessfulScripts { get; set; }
        public int FailedScripts { get; set; }
        public List<ScriptExecutionResult> Results { get; internal set; }
    }

    public class SaveConnectionRequest
    {
        public ConnectionConfig Connection { get; set; } = new ConnectionConfig();
        public bool TestAfterSave { get; set; }
        public bool OpenAfterSave { get; set; }
    }

    public class SaveConnectionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ErrorDetails { get; internal set; }
    }

    public class DatabaseConnectionListItem
    {
        public string ConnectionName { get; set; } = string.Empty;
        public string DatabaseType { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Database { get; set; } = string.Empty;
        public string? Username { get; set; }
        public bool IsCurrent { get; set; }
    }

    public class SetCurrentDatabaseRequest { public string ConnectionName { get; set; } = string.Empty; }
    public class SetCurrentDatabaseResult { public bool Success { get; set; } public string Message { get; set; } = string.Empty; public bool RequiresLogout { get; set; }
        public string ErrorDetails { get; internal set; }
    }

    public class UpdateConnectionRequest { public ConnectionConfig Connection { get; set; } = new ConnectionConfig();
        public string OriginalConnectionName { get; internal set; }
    }

    public class SchemaPrivilegeCheckRequest { public ConnectionConfig Connection { get; set; } = new ConnectionConfig(); public string? SchemaName { get; set; } }
    public class SchemaPrivilegeCheckResult { public bool HasCreatePrivilege { get; set; } public string Message { get; set; } = string.Empty; public List<string> ExistingSchemas { get; set; } = new();
        public string ErrorDetails { get; internal set; }
    }

    public class CreateSchemaRequest { public ConnectionConfig Connection { get; set; } = new ConnectionConfig(); public string SchemaName { get; set; } = string.Empty; }
    public class CreateSchemaResult { public bool Success { get; set; } public string ErrorMessage { get; set; } = string.Empty;
        public string Message { get; internal set; }
        public string ErrorDetails { get; internal set; }
    }

    public class DropDatabaseRequest { public ConnectionConfig Connection { get; set; } = new ConnectionConfig(); public string? SchemaName { get; set; } }
    public class DropDatabaseResult { public bool Success { get; set; } public string? ErrorMessage { get; set; } }

    public class RecreateDatabaseRequest { public ConnectionConfig Connection { get; set; } = new ConnectionConfig(); public string? SchemaName { get; set; } }
    public class RecreateDatabaseResult { public bool Success { get; set; } public string? ErrorMessage { get; set; } }

    public class CopyDatabaseRequest { public string SourceConnection { get; set; } = string.Empty; public string TargetConnection { get; set; } = string.Empty; public string? SourceSchema { get; set; } public string? TargetSchema { get; set; } public bool CopyData { get; set; } = true; }
    public class CopyDatabaseResult { public bool Success { get; set; } public string? ErrorMessage { get; set; } }

    public class DeleteConnectionResult { public bool Success { get; set; } public string Message { get; set; } = string.Empty; }

    public class InstallDriverRequest { public string DatabaseType { get; set; } = string.Empty; }
    public class InstallDriverResult { public bool Success { get; set; } public string Message { get; set; } = string.Empty; }

    public class OperationStartResponse { public string OperationId { get; set; } = string.Empty; public string Message { get; set; } = string.Empty; }

    public class ExecuteScriptRequest { public ConnectionConfig Connection { get; set; } = new ConnectionConfig(); public string ScriptName { get; set; } = string.Empty; }
}
