using System;
using System.Collections.Generic;

namespace Beep.OilandGas.PPDM39.DataManagement.Tools
{
    /// <summary>
    /// Maps C# types to SQL types for different database systems
    /// </summary>
    public static class DatabaseTypeMapper
    {
        /// <summary>
        /// Database types supported
        /// </summary>
        public enum DatabaseType
        {
            SqlServer,
            Oracle,
            PostgreSQL,
            MySQL,
            MariaDB,
            SQLite
        }

        /// <summary>
        /// Maps C# type to SQL type for a specific database
        /// </summary>
        public static string MapType(Type csharpType, DatabaseType databaseType, int? maxLength = null, int? precision = null, int? scale = null)
        {
            if (csharpType == null)
                throw new ArgumentNullException(nameof(csharpType));

            // Handle nullable types
            var underlyingType = Nullable.GetUnderlyingType(csharpType) ?? csharpType;

            // Handle string types
            if (underlyingType == typeof(string))
            {
                return MapStringType(databaseType, maxLength);
            }

            // Handle numeric types
            if (underlyingType == typeof(decimal) || underlyingType == typeof(Decimal))
            {
                return MapDecimalType(databaseType, precision, scale);
            }

            if (underlyingType == typeof(int) || underlyingType == typeof(Int32))
            {
                return MapIntegerType(databaseType);
            }

            if (underlyingType == typeof(long) || underlyingType == typeof(Int64))
            {
                return MapBigIntegerType(databaseType);
            }

            if (underlyingType == typeof(short) || underlyingType == typeof(Int16))
            {
                return MapSmallIntegerType(databaseType);
            }

            if (underlyingType == typeof(byte))
            {
                return MapTinyIntegerType(databaseType);
            }

            if (underlyingType == typeof(float) || underlyingType == typeof(Single))
            {
                return MapFloatType(databaseType);
            }

            if (underlyingType == typeof(double) || underlyingType == typeof(Double))
            {
                return MapDoubleType(databaseType);
            }

            // Handle date/time types
            if (underlyingType == typeof(DateTime))
            {
                return MapDateTimeType(databaseType);
            }

            if (underlyingType == typeof(TimeSpan))
            {
                return MapTimeType(databaseType);
            }

            // Handle GUID
            if (underlyingType == typeof(Guid))
            {
                return MapGuidType(databaseType);
            }

            // Handle boolean
            if (underlyingType == typeof(bool) || underlyingType == typeof(Boolean))
            {
                return MapBooleanType(databaseType);
            }

            // Handle byte array
            if (underlyingType == typeof(byte[]))
            {
                return MapBinaryType(databaseType, maxLength);
            }

            // Default to string for unknown types
            return MapStringType(databaseType, maxLength);
        }

        private static string MapStringType(DatabaseType databaseType, int? maxLength)
        {
            var length = maxLength ?? 50;
            if (length > 4000) length = 4000; // Max for NVARCHAR in SQL Server

            return databaseType switch
            {
                DatabaseType.SqlServer => length <= 4000 ? $"NVARCHAR({length})" : "NVARCHAR(MAX)",
                DatabaseType.Oracle => length <= 4000 ? $"VARCHAR2({length})" : "CLOB",
                DatabaseType.PostgreSQL => length <= 10485760 ? $"VARCHAR({length})" : "TEXT",
                DatabaseType.MySQL => length <= 65535 ? $"VARCHAR({length})" : "TEXT",
                DatabaseType.MariaDB => length <= 65535 ? $"VARCHAR({length})" : "TEXT",
                DatabaseType.SQLite => "TEXT",
                _ => $"VARCHAR({length})"
            };
        }

        private static string MapDecimalType(DatabaseType databaseType, int? precision, int? scale)
        {
            var p = precision ?? 18;
            var s = scale ?? 2;

            return databaseType switch
            {
                DatabaseType.SqlServer => $"NUMERIC({p},{s})",
                DatabaseType.Oracle => $"NUMBER({p},{s})",
                DatabaseType.PostgreSQL => $"DECIMAL({p},{s})",
                DatabaseType.MySQL => $"DECIMAL({p},{s})",
                DatabaseType.MariaDB => $"DECIMAL({p},{s})",
                DatabaseType.SQLite => "REAL",
                _ => $"DECIMAL({p},{s})"
            };
        }

        private static string MapIntegerType(DatabaseType databaseType)
        {
            return databaseType switch
            {
                DatabaseType.SqlServer => "INT",
                DatabaseType.Oracle => "NUMBER(10)",
                DatabaseType.PostgreSQL => "INTEGER",
                DatabaseType.MySQL => "INT",
                DatabaseType.MariaDB => "INT",
                DatabaseType.SQLite => "INTEGER",
                _ => "INT"
            };
        }

        private static string MapBigIntegerType(DatabaseType databaseType)
        {
            return databaseType switch
            {
                DatabaseType.SqlServer => "BIGINT",
                DatabaseType.Oracle => "NUMBER(19)",
                DatabaseType.PostgreSQL => "BIGINT",
                DatabaseType.MySQL => "BIGINT",
                DatabaseType.MariaDB => "BIGINT",
                DatabaseType.SQLite => "INTEGER",
                _ => "BIGINT"
            };
        }

        private static string MapSmallIntegerType(DatabaseType databaseType)
        {
            return databaseType switch
            {
                DatabaseType.SqlServer => "SMALLINT",
                DatabaseType.Oracle => "NUMBER(5)",
                DatabaseType.PostgreSQL => "SMALLINT",
                DatabaseType.MySQL => "SMALLINT",
                DatabaseType.MariaDB => "SMALLINT",
                DatabaseType.SQLite => "INTEGER",
                _ => "SMALLINT"
            };
        }

        private static string MapTinyIntegerType(DatabaseType databaseType)
        {
            return databaseType switch
            {
                DatabaseType.SqlServer => "TINYINT",
                DatabaseType.Oracle => "NUMBER(3)",
                DatabaseType.PostgreSQL => "SMALLINT",
                DatabaseType.MySQL => "TINYINT",
                DatabaseType.MariaDB => "TINYINT",
                DatabaseType.SQLite => "INTEGER",
                _ => "TINYINT"
            };
        }

        private static string MapFloatType(DatabaseType databaseType)
        {
            return databaseType switch
            {
                DatabaseType.SqlServer => "REAL",
                DatabaseType.Oracle => "BINARY_FLOAT",
                DatabaseType.PostgreSQL => "REAL",
                DatabaseType.MySQL => "FLOAT",
                DatabaseType.MariaDB => "FLOAT",
                DatabaseType.SQLite => "REAL",
                _ => "REAL"
            };
        }

        private static string MapDoubleType(DatabaseType databaseType)
        {
            return databaseType switch
            {
                DatabaseType.SqlServer => "FLOAT",
                DatabaseType.Oracle => "BINARY_DOUBLE",
                DatabaseType.PostgreSQL => "DOUBLE PRECISION",
                DatabaseType.MySQL => "DOUBLE",
                DatabaseType.MariaDB => "DOUBLE",
                DatabaseType.SQLite => "REAL",
                _ => "DOUBLE"
            };
        }

        private static string MapDateTimeType(DatabaseType databaseType)
        {
            return databaseType switch
            {
                DatabaseType.SqlServer => "DATETIME2",
                DatabaseType.Oracle => "TIMESTAMP",
                DatabaseType.PostgreSQL => "TIMESTAMP",
                DatabaseType.MySQL => "DATETIME",
                DatabaseType.MariaDB => "DATETIME",
                DatabaseType.SQLite => "DATETIME",
                _ => "DATETIME"
            };
        }

        private static string MapTimeType(DatabaseType databaseType)
        {
            return databaseType switch
            {
                DatabaseType.SqlServer => "TIME",
                DatabaseType.Oracle => "INTERVAL DAY TO SECOND",
                DatabaseType.PostgreSQL => "TIME",
                DatabaseType.MySQL => "TIME",
                DatabaseType.MariaDB => "TIME",
                DatabaseType.SQLite => "TEXT",
                _ => "TIME"
            };
        }

        private static string MapGuidType(DatabaseType databaseType)
        {
            return databaseType switch
            {
                DatabaseType.SqlServer => "UNIQUEIDENTIFIER",
                DatabaseType.Oracle => "RAW(16)",
                DatabaseType.PostgreSQL => "UUID",
                DatabaseType.MySQL => "CHAR(36)",
                DatabaseType.MariaDB => "CHAR(36)",
                DatabaseType.SQLite => "TEXT",
                _ => "CHAR(36)"
            };
        }

        private static string MapBooleanType(DatabaseType databaseType)
        {
            return databaseType switch
            {
                DatabaseType.SqlServer => "BIT",
                DatabaseType.Oracle => "NUMBER(1)",
                DatabaseType.PostgreSQL => "BOOLEAN",
                DatabaseType.MySQL => "TINYINT(1)",
                DatabaseType.MariaDB => "TINYINT(1)",
                DatabaseType.SQLite => "INTEGER",
                _ => "BIT"
            };
        }

        private static string MapBinaryType(DatabaseType databaseType, int? maxLength)
        {
            var length = maxLength ?? 8000;

            return databaseType switch
            {
                DatabaseType.SqlServer => length <= 8000 ? $"VARBINARY({length})" : "VARBINARY(MAX)",
                DatabaseType.Oracle => "BLOB",
                DatabaseType.PostgreSQL => "BYTEA",
                DatabaseType.MySQL => length <= 65535 ? $"VARBINARY({length})" : "BLOB",
                DatabaseType.MariaDB => length <= 65535 ? $"VARBINARY({length})" : "BLOB",
                DatabaseType.SQLite => "BLOB",
                _ => $"VARBINARY({length})"
            };
        }

        /// <summary>
        /// Gets the comment syntax for a database type
        /// </summary>
        public static string GetCommentSyntax(DatabaseType databaseType)
        {
            return databaseType switch
            {
                DatabaseType.SqlServer => "--",
                DatabaseType.Oracle => "--",
                DatabaseType.PostgreSQL => "--",
                DatabaseType.MySQL => "--",
                DatabaseType.MariaDB => "--",
                DatabaseType.SQLite => "--",
                _ => "--"
            };
        }

        /// <summary>
        /// Gets the statement terminator for a database type
        /// </summary>
        public static string GetStatementTerminator(DatabaseType databaseType)
        {
            return databaseType switch
            {
                DatabaseType.SqlServer => ";",
                DatabaseType.Oracle => ";",
                DatabaseType.PostgreSQL => ";",
                DatabaseType.MySQL => ";",
                DatabaseType.MariaDB => ";",
                DatabaseType.SQLite => ";",
                _ => ";"
            };
        }

        /// <summary>
        /// Gets the identifier quote character for a database type
        /// </summary>
        public static string GetIdentifierQuote(DatabaseType databaseType)
        {
            return databaseType switch
            {
                DatabaseType.SqlServer => "[",
                DatabaseType.Oracle => "\"",
                DatabaseType.PostgreSQL => "\"",
                DatabaseType.MySQL => "`",
                DatabaseType.MariaDB => "`",
                DatabaseType.SQLite => "\"",
                _ => ""
            };
        }

        /// <summary>
        /// Gets the closing identifier quote character for a database type
        /// </summary>
        public static string GetIdentifierQuoteClose(DatabaseType databaseType)
        {
            return databaseType switch
            {
                DatabaseType.SqlServer => "]",
                DatabaseType.Oracle => "\"",
                DatabaseType.PostgreSQL => "\"",
                DatabaseType.MySQL => "`",
                DatabaseType.MariaDB => "`",
                DatabaseType.SQLite => "\"",
                _ => ""
            };
        }
    }
}

