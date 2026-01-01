namespace Beep.OilandGas.DataManager.Core.Utilities
{
    /// <summary>
    /// Utility class to normalize database type names to match script folder names
    /// </summary>
    public static class DatabaseTypeNormalizer
    {
        private static readonly Dictionary<string, string> _normalizationMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // SQL Server variations
            { "SqlServer", "Sqlserver" },
            { "SQL Server", "Sqlserver" },
            { "sqlserver", "Sqlserver" },
            { "SQLSERVER", "Sqlserver" },
            { "MSSQL", "Sqlserver" },
            { "Microsoft SQL Server", "Sqlserver" },
            
            // SQLite variations
            { "SQLite", "SQLite" },
            { "sqlite", "SQLite" },
            { "SQLITE", "SQLite" },
            { "Sqlite", "SQLite" },
            
            // PostgreSQL variations
            { "PostgreSQL", "PostgreSQL" },
            { "postgresql", "PostgreSQL" },
            { "POSTGRESQL", "PostgreSQL" },
            { "Postgre", "PostgreSQL" },
            { "postgre", "PostgreSQL" },
            { "Postgres", "PostgreSQL" },
            { "postgres", "PostgreSQL" },
            
            // Oracle variations
            { "Oracle", "Oracle" },
            { "oracle", "Oracle" },
            { "ORACLE", "Oracle" },
            
            // MySQL variations (scripts are in MariaDB folder)
            { "MySQL", "MariaDB" },
            { "mysql", "MariaDB" },
            { "MYSQL", "MariaDB" },
            { "Mysql", "MariaDB" },
            
            // MariaDB variations
            { "MariaDB", "MariaDB" },
            { "mariadb", "MariaDB" },
            { "MARIADB", "MariaDB" },
            { "Maria", "MariaDB" },
            { "maria", "MariaDB" }
        };

        /// <summary>
        /// Normalizes a database type string to match the script folder name
        /// </summary>
        /// <param name="databaseType">The database type string (case-insensitive)</param>
        /// <returns>Normalized database type folder name</returns>
        public static string Normalize(string databaseType)
        {
            if (string.IsNullOrWhiteSpace(databaseType))
            {
                throw new ArgumentException("Database type cannot be null or empty", nameof(databaseType));
            }

            // Trim whitespace
            var trimmed = databaseType.Trim();

            // Check if we have a direct mapping
            if (_normalizationMap.TryGetValue(trimmed, out var normalized))
            {
                return normalized;
            }

            // If no mapping found, try to match by case-insensitive comparison
            var matchingKey = _normalizationMap.Keys.FirstOrDefault(k => 
                string.Equals(k, trimmed, StringComparison.OrdinalIgnoreCase));

            if (matchingKey != null)
            {
                return _normalizationMap[matchingKey];
            }

            // Default: return as-is (might be already normalized)
            // Common folder names: Sqlserver, SQLite, PostgreSQL, Oracle, MySQL, MariaDB
            return trimmed;
        }

        /// <summary>
        /// Gets all supported database type folder names
        /// </summary>
        public static IEnumerable<string> GetSupportedDatabaseTypes()
        {
            return new[] { "Sqlserver", "SQLite", "PostgreSQL", "Oracle", "MySQL", "MariaDB" };
        }

        /// <summary>
        /// Checks if a database type is supported
        /// </summary>
        public static bool IsSupported(string databaseType)
        {
            if (string.IsNullOrWhiteSpace(databaseType))
            {
                return false;
            }

            var normalized = Normalize(databaseType);
            return GetSupportedDatabaseTypes().Contains(normalized, StringComparer.OrdinalIgnoreCase);
        }
    }
}
