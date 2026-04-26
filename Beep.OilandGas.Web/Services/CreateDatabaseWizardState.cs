using Beep.OilandGas.Models.Core.DTOs;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.DataManagement;

namespace Beep.OilandGas.Web.Services;

public sealed class CreateDatabaseWizardState
{
    public const string TargetNewConnection = "new";
    public const string TargetExistingConnection = "existing";
    public const string TargetDemoDatabase = "demo";

    public static readonly HashSet<string> FoundationModuleIds = new(StringComparer.OrdinalIgnoreCase)
    {
        "PPDM_CORE",
        "R_SHARED_REFERENCES",
        "WELL_STATUS_FACETS",
        "WELL_REFERENCES"
    };

    public List<string> AvailableDatabaseTypes { get; set; } = new() { "SqlServer", "PostgreSQL", "MySQL", "MariaDB", "Oracle", "SQLite" };
    public List<DatabaseConnectionListItem> Connections { get; set; } = new();
    public List<SelectableModule> OptionalModules { get; set; } = new();

    public string TargetMode { get; set; } = TargetNewConnection;
    public string UserId { get; set; } = "wizard";
    public string ActiveConnectionName { get; set; } = string.Empty;
    public string? SelectedExistingConnection { get; set; }
    public string SelectedDatabaseType { get; set; } = "SqlServer";
    public string ConnectionName { get; set; } = string.Empty;
    public string Host { get; set; } = "localhost";
    public int? Port { get; set; } = 1433;
    public string Database { get; set; } = string.Empty;
    public string? Schema { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? ConnectionString { get; set; }
    public string DemoConnectionName { get; set; } = string.Empty;

    public bool IsPreparingTarget { get; set; }
    public bool TargetReady { get; set; }
    public string TargetResultMessage { get; set; } = string.Empty;

    public bool IsInstallingFoundation { get; set; }
    public bool FoundationInstalled { get; set; }
    public CreateSchemaResult? SchemaResult { get; set; }

    public bool IsLoadingModules { get; set; }
    public bool IsInstallingModules { get; set; }
    public bool ModulesInstalled { get; set; }
    public ModuleSeedingResponse? ModuleSeedResult { get; set; }

    public bool CanPrepareTarget => TargetMode switch
    {
        TargetExistingConnection => !string.IsNullOrWhiteSpace(SelectedExistingConnection),
        TargetDemoDatabase => !string.IsNullOrWhiteSpace(UserId),
        _ => !string.IsNullOrWhiteSpace(SelectedDatabaseType) &&
             !string.IsNullOrWhiteSpace(ConnectionName) &&
             (!string.IsNullOrWhiteSpace(ConnectionString) || !string.IsNullOrWhiteSpace(Database))
    };

    public bool CanInstallSelectedModules => FoundationInstalled && OptionalModules.Any(module => module.IsSelected);

    public void ResetForTargetPreparation()
    {
        TargetReady = false;
        FoundationInstalled = false;
        ModulesInstalled = false;
        SchemaResult = null;
        ModuleSeedResult = null;
        TargetResultMessage = string.Empty;
        OptionalModules.Clear();
    }

    public ConnectionConfig BuildConnectionConfig()
    {
        return new ConnectionConfig
        {
            DatabaseType = SelectedDatabaseType,
            ConnectionName = ConnectionName,
            Host = Host,
            Port = Port ?? GetDefaultPort(SelectedDatabaseType),
            Database = Database,
            Schema = Schema,
            Username = Username,
            Password = Password,
            ConnectionString = ConnectionString
        };
    }

    private static int GetDefaultPort(string databaseType) => databaseType.ToLowerInvariant() switch
    {
        "sqlserver" => 1433,
        "postgresql" or "postgre" => 5432,
        "mysql" or "mariadb" => 3306,
        "oracle" => 1521,
        _ => 0
    };

    public sealed class SelectableModule
    {
        public SelectableModule(ModuleInfo module)
        {
            Module = module;
        }

        public ModuleInfo Module { get; }
        public bool IsSelected { get; set; }
    }
}
