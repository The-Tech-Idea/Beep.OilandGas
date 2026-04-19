using TheTechIdea.Beep.ConfigUtil;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class OperationStartResponse : ModelEntityBase
    {
        public string OperationId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; } = true;
    }

    public class CreateSchemaRequest : ModelEntityBase
    {
        public string SchemaName { get; set; } = string.Empty;
        public string ConnectionName { get; set; } = string.Empty;

        private ConnectionProperties? ConnectionValue;

        public ConnectionProperties? Connection
        {
            get
            {
                return ConnectionValue ?? (string.IsNullOrWhiteSpace(ConnectionName)
                    ? null
                    : new ConnectionProperties { ConnectionName = ConnectionName });
            }
            set
            {
                ConnectionValue = value;
                if (value != null && string.IsNullOrWhiteSpace(ConnectionName))
                    ConnectionName = value.ConnectionName;
            }
        }
    }

    public class UpdateConnectionRequest : ModelEntityBase
    {
        public string ConnectionName { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
        public string? DatabaseType { get; set; }

        public string OriginalConnectionName { get; set; } = string.Empty;

        private ConnectionProperties? ConnectionValue;

        public ConnectionProperties? Connection
        {
            get
            {
                return ConnectionValue ?? new ConnectionProperties
                {
                    ConnectionName = ConnectionName,
                    ConnectionString = ConnectionString
                };
            }
            set
            {
                ConnectionValue = value;
                if (value == null)
                    return;

                ConnectionName = value.ConnectionName;
                ConnectionString = value.ConnectionString;
            }
        }
    }

    public class InstallDriverRequest : ModelEntityBase
    {
        public string DriverName { get; set; } = string.Empty;
        public string? Version { get; set; }

        public string DatabaseType
        {
            get { return DriverName; }
            set { DriverName = value; }
        }
    }

    public class InstallDriverResult : ModelEntityBase
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public string? ErrorDetails { get; set; }
    }

    public class ScriptExecutionRequest : ModelEntityBase
    {
        public ConnectionProperties? Connection { get; set; }
        public List<string> ScriptNames { get; set; } = new();
        public bool ExecuteAll { get; set; } = false;
    }

    public class SchemaPrivilegeCheckRequest : ModelEntityBase
    {
        public string SchemaName { get; set; } = string.Empty;
        public string? UserId { get; set; }

        private ConnectionProperties? ConnectionValue;

        public ConnectionProperties? Connection
        {
            get { return ConnectionValue; }
            set { ConnectionValue = value; }
        }
    }

    public enum DataSourceType
    {
        SqlServer = 0,
        PostgreSQL = 1,
        Postgre = PostgreSQL,
        Oracle = 2,
        SQLite = 3,
        MySQL = 4,
        Mysql = MySQL,
        MariaDB = 5
    }

    public static class DatabaseTypeMapper
    {
        public enum DatabaseType
        {
            SqlServer = 0,
            PostgreSQL = 1,
            Oracle = 2,
            SQLite = 3,
            MySQL = 4,
            MariaDB = 5
        }

        public static string GetDbType(DataSourceType type) => type.ToString();
        public static string GetDbType(string typeName) => typeName;
    }
}
