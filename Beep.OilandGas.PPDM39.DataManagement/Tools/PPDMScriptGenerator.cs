using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.PPDM39.DataManagement.Tools
{
    /// <summary>
    /// Generates SQL scripts (TAB, PK, FK) from entity classes using reflection
    /// </summary>
    public class PPDMScriptGenerator
    {
        private readonly IPPDMMetadataRepository? _metadata;
        private readonly ILogger<PPDMScriptGenerator>? _logger;
        private readonly DatabaseTypeMapper.DatabaseType _databaseType;

        // Standard PPDM columns that are always included
        private static readonly HashSet<string> StandardPPDMColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ROW_ID", "PPDM_GUID", "ACTIVE_IND", "REMARK", "SOURCE", "ROW_QUALITY",
            "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE",
            "ROW_EFFECTIVE_DATE", "ROW_EXPIRY_DATE"
        };

        public PPDMScriptGenerator(
            DatabaseTypeMapper.DatabaseType databaseType,
            IPPDMMetadataRepository? metadata = null,
            ILogger<PPDMScriptGenerator>? logger = null)
        {
            _databaseType = databaseType;
            _metadata = metadata;
            _logger = logger;
        }

        /// <summary>
        /// Generates table creation script (TAB) for an entity type
        /// </summary>
        public async Task<string> GenerateTableScriptAsync(Type entityType)
        {
            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));

            var tableName = GetTableName(entityType);
            var properties = GetEntityProperties(entityType);

            var sb = new StringBuilder();
            var commentPrefix = DatabaseTypeMapper.GetCommentSyntax(_databaseType);
            var terminator = DatabaseTypeMapper.GetStatementTerminator(_databaseType);
            var quoteOpen = DatabaseTypeMapper.GetIdentifierQuote(_databaseType);
            var quoteClose = DatabaseTypeMapper.GetIdentifierQuoteClose(_databaseType);

            // Add header comment
            sb.AppendLine($"{commentPrefix} {tableName} Table Creation Script");
            sb.AppendLine($"{commentPrefix} Generated for {_databaseType}");
            sb.AppendLine($"{commentPrefix} Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine();

            // SQL Server specific: raiserror statement
            if (_databaseType == DatabaseTypeMapper.DatabaseType.SqlServer)
            {
                sb.AppendLine($"raiserror ('CREATING TABLE {tableName}', 10, 1) with nowait");
            }

            // CREATE TABLE statement
            sb.AppendLine($"CREATE TABLE {quoteOpen}{tableName}{quoteClose}");
            sb.AppendLine("(");

            var columnDefinitions = new List<string>();

            foreach (var prop in properties)
            {
                var columnDef = await GenerateColumnDefinitionAsync(prop, tableName);
                if (!string.IsNullOrEmpty(columnDef))
                {
                    columnDefinitions.Add(columnDef);
                }
            }

            // Join columns with proper indentation
            for (int i = 0; i < columnDefinitions.Count; i++)
            {
                var isLast = i == columnDefinitions.Count - 1;
                var indent = _databaseType == DatabaseTypeMapper.DatabaseType.Oracle ? "\t" : "\t";
                sb.Append($"{indent}{columnDefinitions[i]}");
                if (!isLast)
                {
                    sb.Append(",");
                }
                sb.AppendLine();
            }

            sb.AppendLine($"){terminator}");
            sb.AppendLine();

            return sb.ToString();
        }

        /// <summary>
        /// Generates primary key script (PK) for an entity type
        /// </summary>
        public async Task<string> GeneratePrimaryKeyScriptAsync(Type entityType)
        {
            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));

            var tableName = GetTableName(entityType);
            var primaryKeyProperty = GetPrimaryKeyProperty(entityType);

            if (primaryKeyProperty == null)
            {
                _logger?.LogWarning($"No primary key found for {tableName}, skipping PK script");
                return string.Empty;
            }

            var sb = new StringBuilder();
            var commentPrefix = DatabaseTypeMapper.GetCommentSyntax(_databaseType);
            var terminator = DatabaseTypeMapper.GetStatementTerminator(_databaseType);
            var quoteOpen = DatabaseTypeMapper.GetIdentifierQuote(_databaseType);
            var quoteClose = DatabaseTypeMapper.GetIdentifierQuoteClose(_databaseType);

            var pkColumnName = primaryKeyProperty.Name;
            var pkConstraintName = $"{tableName}_PK";

            // Add header comment
            sb.AppendLine($"{commentPrefix} {tableName} Primary Key");
            sb.AppendLine($"{commentPrefix} Generated for {_databaseType}");
            sb.AppendLine();

            // SQL Server specific: raiserror statement
            if (_databaseType == DatabaseTypeMapper.DatabaseType.SqlServer)
            {
                sb.AppendLine($"raiserror ('CREATING PRIMARY KEY {pkConstraintName}', 10, 1) with nowait");
            }

            // ALTER TABLE ADD CONSTRAINT
            sb.AppendLine($"ALTER TABLE {quoteOpen}{tableName}{quoteClose} ADD CONSTRAINT {quoteOpen}{pkConstraintName}{quoteClose} PRIMARY KEY");
            sb.AppendLine("(");
            sb.AppendLine($"\t{quoteOpen}{pkColumnName}{quoteClose}");
            sb.AppendLine($"){terminator}");
            sb.AppendLine();

            return sb.ToString();
        }

        /// <summary>
        /// Generates foreign key scripts (FK) for an entity type
        /// </summary>
        public async Task<string> GenerateForeignKeyScriptsAsync(Type entityType)
        {
            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));

            var tableName = GetTableName(entityType);
            var foreignKeyProperties = GetForeignKeyProperties(entityType);

            if (foreignKeyProperties.Count == 0)
            {
                _logger?.LogInformation($"No foreign keys found for {tableName}");
                return string.Empty;
            }

            var sb = new StringBuilder();
            var commentPrefix = DatabaseTypeMapper.GetCommentSyntax(_databaseType);
            var terminator = DatabaseTypeMapper.GetStatementTerminator(_databaseType);
            var quoteOpen = DatabaseTypeMapper.GetIdentifierQuote(_databaseType);
            var quoteClose = DatabaseTypeMapper.GetIdentifierQuoteClose(_databaseType);

            // Add header comment
            sb.AppendLine($"{commentPrefix} {tableName} Foreign Keys");
            sb.AppendLine($"{commentPrefix} Generated for {_databaseType}");
            sb.AppendLine();

            foreach (var fkProp in foreignKeyProperties)
            {
                var referencedTable = await GetReferencedTableAsync(fkProp);
                if (string.IsNullOrEmpty(referencedTable))
                {
                    _logger?.LogWarning($"Could not determine referenced table for foreign key {fkProp.Name} in {tableName}");
                    continue;
                }

                var fkColumnName = fkProp.Name;
                var referencedColumn = await GetReferencedColumnAsync(fkProp, referencedTable);
                var fkConstraintName = $"{tableName}_{referencedTable}_FK";

                // SQL Server specific: raiserror statement
                if (_databaseType == DatabaseTypeMapper.DatabaseType.SqlServer)
                {
                    sb.AppendLine($"raiserror ('CREATING FOREIGN KEY CONSTRAINT {fkConstraintName}', 10, 1) with nowait");
                }

                // ALTER TABLE ADD CONSTRAINT
                sb.AppendLine($"ALTER TABLE {quoteOpen}{tableName}{quoteClose} ADD CONSTRAINT {quoteOpen}{fkConstraintName}{quoteClose} FOREIGN KEY");
                sb.AppendLine("(");
                sb.AppendLine($"\t{quoteOpen}{fkColumnName}{quoteClose}");
                sb.AppendLine(")");
                sb.AppendLine($"REFERENCES {quoteOpen}{referencedTable}{quoteClose}");
                sb.AppendLine("(");
                sb.AppendLine($"\t{quoteOpen}{referencedColumn}{quoteClose}");
                sb.AppendLine($"){terminator}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generates index scripts for foreign keys
        /// </summary>
        public async Task<string> GenerateIndexScriptsAsync(Type entityType)
        {
            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));

            var tableName = GetTableName(entityType);
            var foreignKeyProperties = GetForeignKeyProperties(entityType);

            if (foreignKeyProperties.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            var commentPrefix = DatabaseTypeMapper.GetCommentSyntax(_databaseType);
            var terminator = DatabaseTypeMapper.GetStatementTerminator(_databaseType);
            var quoteOpen = DatabaseTypeMapper.GetIdentifierQuote(_databaseType);
            var quoteClose = DatabaseTypeMapper.GetIdentifierQuoteClose(_databaseType);

            // Add header comment
            sb.AppendLine($"{commentPrefix} {tableName} Indexes for Foreign Keys");
            sb.AppendLine($"{commentPrefix} Generated for {_databaseType}");
            sb.AppendLine();

            foreach (var fkProp in foreignKeyProperties)
            {
                var indexName = $"IX_{tableName}_{fkProp.Name}";
                var columnName = fkProp.Name;

                // CREATE INDEX statement
                sb.AppendLine($"CREATE INDEX {quoteOpen}{indexName}{quoteClose} ON {quoteOpen}{tableName}{quoteClose}");
                sb.AppendLine("(");
                sb.AppendLine($"\t{quoteOpen}{columnName}{quoteClose}");
                sb.AppendLine($"){terminator}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generates all scripts (TAB, PK, FK, IX) for an entity type
        /// </summary>
        public async Task<Dictionary<ScriptType, string>> GenerateAllScriptsAsync(Type entityType)
        {
            var scripts = new Dictionary<ScriptType, string>();

            scripts[ScriptType.TAB] = await GenerateTableScriptAsync(entityType);
            scripts[ScriptType.PK] = await GeneratePrimaryKeyScriptAsync(entityType);
            scripts[ScriptType.FK] = await GenerateForeignKeyScriptsAsync(entityType);
            scripts[ScriptType.IX] = await GenerateIndexScriptsAsync(entityType);

            return scripts;
        }

        #region Helper Methods

        private string GetTableName(Type entityType)
        {
            return entityType.Name;
        }

        private List<PropertyInfo> GetEntityProperties(Type entityType)
        {
            return entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite)
                .OrderBy(p => p.Name == GetPrimaryKeyProperty(entityType)?.Name ? 0 : 1) // Primary key first
                .ThenBy(p => StandardPPDMColumns.Contains(p.Name) ? 2 : 1) // Standard columns last
                .ThenBy(p => p.Name)
                .ToList();
        }

        private PropertyInfo? GetPrimaryKeyProperty(Type entityType)
        {
            var tableName = GetTableName(entityType);
            var expectedPkName = $"{tableName}_ID";

            // Try exact match first
            var pkProp = entityType.GetProperty(expectedPkName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (pkProp != null)
                return pkProp;

            // Try ROW_ID
            pkProp = entityType.GetProperty("ROW_ID", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (pkProp != null)
                return pkProp;

            // Try any property ending with _ID that's not a foreign key
            var allProps = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            pkProp = allProps.FirstOrDefault(p => 
                p.Name.EndsWith("_ID", StringComparison.OrdinalIgnoreCase) &&
                p.Name.Equals(expectedPkName, StringComparison.OrdinalIgnoreCase));

            return pkProp;
        }

        private List<PropertyInfo> GetForeignKeyProperties(Type entityType)
        {
            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var tableName = GetTableName(entityType);
            var primaryKeyName = GetPrimaryKeyProperty(entityType)?.Name;

            return properties
                .Where(p => 
                    p.Name.EndsWith("_ID", StringComparison.OrdinalIgnoreCase) &&
                    !p.Name.Equals(primaryKeyName, StringComparison.OrdinalIgnoreCase) &&
                    !StandardPPDMColumns.Contains(p.Name))
                .ToList();
        }

        private async Task<string> GenerateColumnDefinitionAsync(PropertyInfo property, string tableName)
        {
            var columnName = property.Name;
            var propertyType = property.PropertyType;
            var isNullable = IsNullable(propertyType);
            var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            // Get SQL type
            var sqlType = DatabaseTypeMapper.MapType(underlyingType, _databaseType, GetMaxLength(columnName));

            // Determine if column should be NOT NULL
            var isPrimaryKey = columnName.Equals($"{tableName}_ID", StringComparison.OrdinalIgnoreCase) ||
                              columnName.Equals("ROW_ID", StringComparison.OrdinalIgnoreCase);

            var notNull = isPrimaryKey ? " NOT NULL" : (isNullable ? "" : " NOT NULL");

            var quoteOpen = DatabaseTypeMapper.GetIdentifierQuote(_databaseType);
            var quoteClose = DatabaseTypeMapper.GetIdentifierQuoteClose(_databaseType);

            // Handle default values for standard columns
            var defaultValue = GetDefaultValue(columnName, _databaseType);

            return $"{quoteOpen}{columnName}{quoteClose} {sqlType}{notNull}{defaultValue}";
        }

        private bool IsNullable(Type type)
        {
            if (type.IsValueType)
            {
                return Nullable.GetUnderlyingType(type) != null;
            }
            return true; // Reference types are nullable
        }

        private int? GetMaxLength(string columnName)
        {
            // Common length patterns
            if (columnName.EndsWith("_ID", StringComparison.OrdinalIgnoreCase))
                return 40;
            if (columnName.Contains("GUID", StringComparison.OrdinalIgnoreCase))
                return 38;
            if (columnName.Contains("DESCRIPTION", StringComparison.OrdinalIgnoreCase) ||
                columnName.Contains("REMARK", StringComparison.OrdinalIgnoreCase))
                return 4000;
            if (columnName.Contains("NAME", StringComparison.OrdinalIgnoreCase))
                return 200;
            if (columnName.Equals("ACTIVE_IND", StringComparison.OrdinalIgnoreCase))
                return 1;
            if (columnName.Contains("STATUS", StringComparison.OrdinalIgnoreCase) ||
                columnName.Contains("TYPE", StringComparison.OrdinalIgnoreCase))
                return 50;

            return null; // Use default
        }

        private string GetDefaultValue(string columnName, DatabaseTypeMapper.DatabaseType databaseType)
        {
            if (columnName.Equals("ACTIVE_IND", StringComparison.OrdinalIgnoreCase))
            {
                return databaseType == DatabaseTypeMapper.DatabaseType.Oracle 
                    ? " DEFAULT 'Y'" 
                    : " DEFAULT 'Y'";
            }

            return string.Empty;
        }

        private async Task<string?> GetReferencedTableAsync(PropertyInfo foreignKeyProperty)
        {
            var fkName = foreignKeyProperty.Name;
            
            // Remove _ID suffix to get table name
            if (fkName.EndsWith("_ID", StringComparison.OrdinalIgnoreCase))
            {
                var tableName = fkName.Substring(0, fkName.Length - 3);
                
                // Try to find the entity type in the same assembly
                var entityAssembly = foreignKeyProperty.DeclaringType?.Assembly;
                if (entityAssembly != null)
                {
                    var entityType = entityAssembly.GetTypes()
                        .FirstOrDefault(t => t.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));
                    
                    if (entityType != null)
                    {
                        return entityType.Name;
                    }
                }

                // Try metadata lookup
                if (_metadata != null)
                {
                    try
                    {
                        var metadata = await _metadata.GetTableMetadataAsync(tableName);
                        if (metadata != null)
                        {
                            return tableName;
                        }
                    }
                    catch
                    {
                        // Ignore metadata lookup errors
                    }
                }

                // Return the inferred table name
                return tableName;
            }

            return null;
        }

        private async Task<string> GetReferencedColumnAsync(PropertyInfo foreignKeyProperty, string referencedTable)
        {
            // Usually the referenced column is {TABLE}_ID
            var expectedColumn = $"{referencedTable}_ID";

            // Try metadata lookup
            if (_metadata != null)
            {
                try
                {
                    var metadata = await _metadata.GetTableMetadataAsync(referencedTable);
                    if (metadata != null)
                    {
                        // Try to find primary key column from metadata
                        // For now, assume it's {TABLE}_ID
                        return expectedColumn;
                    }
                }
                catch
                {
                    // Ignore metadata lookup errors
                }
            }

            return expectedColumn;
        }

        #endregion
    }
}

