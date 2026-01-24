using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Globalization;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.UOW;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.Models.Data.Calculations;

namespace Beep.OilandGas.PPDM39.DataManagement.Core
{
    /// <summary>
    /// Non-generic repository implementation for any PPDM entity
    /// Use this for simple CRUD operations - no need to create specific repositories for every table
    /// Uses metadata to automatically determine table names and primary keys
    /// Supports parent-child relationships through metadata
    /// Creates UnitOfWork instances internally using UnitOfWorkFactory
    /// Works with any entity type dynamically using Type objects
    /// </summary>
    public class PPDMGenericRepository
    {
        protected readonly ICommonColumnHandler _commonColumnHandler;
        protected readonly IPPDM39DefaultsRepository _defaults;
        protected readonly IPPDMMetadataRepository _metadata;
        protected readonly IDMEEditor _editor;
        protected readonly string _connectionName;
        protected readonly string _tableName;
        protected readonly Type _entityType;
        protected readonly ILogger<PPDMGenericRepository>? _logger;
        private readonly Dictionary<Type, IUnitOfWorkWrapper> _unitOfWorkCache = new Dictionary<Type, IUnitOfWorkWrapper>();
        private readonly object _unitOfWorkLock = new object();

        /// <summary>
        /// Gets the common column handler
        /// </summary>
        public ICommonColumnHandler CommonColumnHandler => _commonColumnHandler;

        /// <summary>
        /// Gets the PPDM39 defaults repository
        /// </summary>
        public IPPDM39DefaultsRepository Defaults => _defaults;

        /// <summary>
        /// Gets the metadata repository
        /// </summary>
        public IPPDMMetadataRepository Metadata => _metadata;

        /// <summary>
        /// Gets the entity type this repository works with
        /// </summary>
        public Type EntityType => _entityType;

        /// <summary>
        /// Gets the table name this repository works with
        /// </summary>
        public string TableName => _tableName;

        /// <summary>
        /// Gets the UnitOfWork instance (created internally via UnitOfWorkFactory)
        /// </summary>
        public IUnitofWork UnitOfWork => GetOrCreateUnitOfWork(_entityType, _tableName) as IUnitofWork 
            ?? throw new InvalidOperationException("UnitOfWork is not available");

        /// <summary>
        /// Creates a repository for a PPDM entity
        /// </summary>
        /// <param name="editor">IDMEEditor instance</param>
        /// <param name="commonColumnHandler">Common column handler</param>
        /// <param name="defaults">PPDM39 defaults repository</param>
        /// <param name="metadata">PPDM metadata repository</param>
        /// <param name="entityType">Entity type to work with</param>
        /// <param name="connectionName">Connection name (defaults to "PPDM39")</param>
        /// <param name="tableName">Table name (defaults to entity type name)</param>
        /// <param name="logger">Optional logger instance</param>
        public PPDMGenericRepository(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            Type entityType,
            string connectionName = "PPDM39",
            string tableName = null,
            ILogger<PPDMGenericRepository>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _entityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _tableName = tableName ?? entityType.Name;
            _logger = logger;

            // Validate entity type implements IPPDMEntity
            if (!typeof(IPPDMEntity).IsAssignableFrom(_entityType))
            {
                throw new ArgumentException($"Entity type {_entityType.Name} must implement IPPDMEntity", nameof(entityType));
            }
        }

        /// <summary>
        /// Gets or creates a UnitOfWork instance for the specified entity type
        /// Uses UnitOfWorkFactory to create UnitOfWork dynamically for any entity type
        /// </summary>
        protected IUnitOfWorkWrapper GetOrCreateUnitOfWork(Type entityType, string tableName = null, string primaryKey = null)
        {
            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));

            if (!_unitOfWorkCache.TryGetValue(entityType, out var unitOfWork))
            {
                lock (_unitOfWorkLock)
                {
                    if (!_unitOfWorkCache.TryGetValue(entityType, out unitOfWork))
                    {
                        // Get table name and primary key from metadata if not provided
                        if (string.IsNullOrWhiteSpace(tableName))
                        {
                            tableName = GetTableName(entityType);
                        }
                        
                        if (string.IsNullOrWhiteSpace(primaryKey))
                        {
                            primaryKey = GetPrimaryKeyName(tableName);
                        }
                        
                        // Create UnitOfWork internally for this entity type using UnitOfWorkFactory
                        unitOfWork = UnitOfWorkFactory.CreateUnitOfWork(
                            entityType, 
                            _editor, 
                            _connectionName, 
                            tableName, 
                            primaryKey);
                        
                        _unitOfWorkCache[entityType] = unitOfWork;
                    }
                }
            }
            
            return unitOfWork;
        }

        /// <summary>
        /// Gets the table name for the entity type
        /// </summary>
        protected virtual string GetTableName(Type entityType)
        {
            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));
            
            // Default: Use entity type name as table name
            return entityType.Name;
        }

        /// <summary>
        /// Gets the primary key name for a table
        /// </summary>
        protected virtual string GetPrimaryKeyName(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty", nameof(tableName));
            
            // Get primary key from metadata synchronously
            var metadata = _metadata.GetTableMetadataAsync(tableName).GetAwaiter().GetResult();
            if (metadata == null)
            {
                throw new InvalidOperationException($"Table metadata not found for: {tableName}");
            }
            
            return metadata.PrimaryKeyColumn;
        }

        /// <summary>
        /// Gets the table metadata for a table name
        /// </summary>
        protected async Task<PPDMTableMetadata> GetTableMetadataAsync(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty", nameof(tableName));
            
            return await _metadata.GetTableMetadataAsync(tableName);
        }

        /// <summary>
        /// Helper method to get entities using AppFilter
        /// This method works with any data source (SQL, NoSQL, etc.)
        /// </summary>
        public virtual async Task<IEnumerable<object>> GetEntitiesWithFiltersAsync(
            Type entityType, 
            string tableName, 
            List<AppFilter> filters)
        {
            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty", nameof(tableName));

            // Use IDataSource.GetEntityAsync directly - preferred for simple single-table queries
            var dataSource = _editor.GetDataSource(_connectionName);
            if (dataSource == null)
            {
                throw new InvalidOperationException($"DataSource not found for connection: {_connectionName}");
            }

            // GetEntityAsync handles parameter delimiters automatically
            var result = await dataSource.GetEntityAsync(tableName, filters ?? new List<AppFilter>());
            return ConvertToTypedList(result, entityType);
        }

        /// <summary>
        /// Converts dynamic result to typed list
        /// </summary>
        protected virtual IEnumerable<object> ConvertToTypedList(dynamic result, Type targetType)
        {
            if (result == null)
                return Enumerable.Empty<object>();

            var list = new List<object>();
            if (result is System.Collections.IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item != null && targetType.IsAssignableFrom(item.GetType()))
                    {
                        list.Add(item);
                    }
                }
            }
            else if (result != null && targetType.IsAssignableFrom(result.GetType()))
            {
                list.Add(result);
            }

            return list;
        }

        /// <summary>
        /// Builds SQL query from AppFilter list (fallback for SQL data sources)
        /// Uses IDataSource.ParameterDelimiter and IDataSource.ColumnDelimiter for database compatibility
        /// </summary>
        protected virtual string BuildSqlFromFilters(string tableName, List<AppFilter> filters)
        {
            // Get IDataSource to access delimiters
            var dataSource = _editor.GetDataSource(_connectionName);
            if (dataSource == null)
            {
                throw new InvalidOperationException($"DataSource not found for connection: {_connectionName}");
            }

            var paramDelim = dataSource.ParameterDelimiter ?? "@";
            var columnDelim = dataSource.ColumnDelimiter ?? "";
            var columnDelimEnd = string.IsNullOrEmpty(columnDelim) ? "" : columnDelim;

            // Apply column delimiters to table name if needed
            var delimitedTableName = string.IsNullOrEmpty(columnDelim) 
                ? tableName 
                : $"{columnDelim}{tableName}{columnDelimEnd}";

            var sql = $"SELECT * FROM {delimitedTableName}";
            
            if (filters != null && filters.Count > 0)
            {
                var whereClauses = new List<string>();
                var paramIndex = 0;
                
                foreach (var filter in filters)
                {
                    if (string.IsNullOrWhiteSpace(filter.FieldName) || string.IsNullOrWhiteSpace(filter.Operator))
                        continue;

                    var operatorStr = filter.Operator.ToUpper();
                    var delimitedFieldName = string.IsNullOrEmpty(columnDelim) 
                        ? filter.FieldName 
                        : $"{columnDelim}{filter.FieldName}{columnDelimEnd}";

                    string clause;
                    string paramName = $"param{paramIndex++}";
                    
                    switch (operatorStr)
                    {
                        case "=":
                        case "!=":
                        case "<>":
                        case ">":
                        case ">=":
                        case "<":
                        case "<=":
                            clause = $"{delimitedFieldName} {operatorStr} {paramDelim}{paramName}";
                            break;
                        case "LIKE":
                            clause = $"{delimitedFieldName} LIKE {paramDelim}{paramName}";
                            break;
                        case "IN":
                            // For IN clause, handle multiple values
                            if (filter.FilterValue is string strValue && strValue.Contains(","))
                            {
                                var values = strValue.Split(',');
                                var paramNames = new List<string>();
                                for (int i = 0; i < values.Length; i++)
                                {
                                    var inParamName = $"{paramName}_{i}";
                                    paramNames.Add($"{paramDelim}{inParamName}");
                                }
                                clause = $"{delimitedFieldName} IN ({string.Join(", ", paramNames)})";
                            }
                            else
                            {
                                clause = $"{delimitedFieldName} IN ({paramDelim}{paramName})";
                            }
                            break;
                        default:
                            clause = $"{delimitedFieldName} {operatorStr} {paramDelim}{paramName}";
                            break;
                    }
                    whereClauses.Add(clause);
                }

                if (whereClauses.Count > 0)
                {
                    sql += " WHERE " + string.Join(" AND ", whereClauses);
                }
            }

            return sql;
        }

        /// <summary>
        /// Builds AppFilter list from SQL WHERE clause (reverse of BuildSqlFromFilters)
        /// Parses SQL WHERE conditions and converts them to AppFilter objects
        /// </summary>
        /// <param name="sql">SQL query string (can be full query or just WHERE clause)</param>
        /// <returns>List of AppFilter objects parsed from SQL</returns>
        public virtual List<AppFilter> BuildAppFiltersFromSql(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return new List<AppFilter>();

            var filters = new List<AppFilter>();
            
            // Extract WHERE clause from SQL
            string whereClause = ExtractWhereClause(sql);
            
            if (string.IsNullOrWhiteSpace(whereClause))
                return filters;

            // Split by AND/OR (but preserve them for future use if needed)
            // For now, we'll treat everything as AND conditions
            var conditions = SplitWhereConditions(whereClause);

            foreach (var condition in conditions)
            {
                var filter = ParseConditionToAppFilter(condition);
                if (filter != null)
                {
                    filters.Add(filter);
                }
            }

            return filters;
        }

        /// <summary>
        /// Extracts WHERE clause from SQL query
        /// </summary>
        private string ExtractWhereClause(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return string.Empty;

            // Normalize SQL - remove extra whitespace
            sql = Regex.Replace(sql, @"\s+", " ", RegexOptions.IgnoreCase).Trim();

            // Find WHERE clause (case insensitive)
            var whereMatch = Regex.Match(sql, @"\bWHERE\s+(.+)$", RegexOptions.IgnoreCase);
            if (whereMatch.Success)
            {
                return whereMatch.Groups[1].Value.Trim();
            }

            // If no WHERE keyword, assume the entire string is the WHERE clause
            // (useful when passing just the WHERE conditions)
            if (!sql.ToUpper().Contains("SELECT") && !sql.ToUpper().Contains("FROM"))
            {
                return sql;
            }

            return string.Empty;
        }

        /// <summary>
        /// Splits WHERE clause into individual conditions
        /// Handles AND/OR operators and parentheses
        /// </summary>
        private List<string> SplitWhereConditions(string whereClause)
        {
            var conditions = new List<string>();
            
            if (string.IsNullOrWhiteSpace(whereClause))
                return conditions;

            // Simple split by AND (case insensitive)
            // This is a basic implementation - for complex SQL with nested parentheses, 
            // a more sophisticated parser would be needed
            var andPattern = @"\s+AND\s+";
            var parts = Regex.Split(whereClause, andPattern, RegexOptions.IgnoreCase);
            
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed))
                {
                    conditions.Add(trimmed);
                }
            }

            return conditions;
        }

        /// <summary>
        /// Parses a single SQL condition into an AppFilter object
        /// Supports: =, !=, <>, >, >=, <, <=, LIKE, IN
        /// </summary>
        private AppFilter ParseConditionToAppFilter(string condition)
        {
            if (string.IsNullOrWhiteSpace(condition))
                return null;

            condition = condition.Trim();

            // Pattern for: column operator value
            // Matches: FIELD_NAME = 'value', FIELD_NAME > 123, FIELD_NAME LIKE '%text%', etc.
            
            // Try LIKE pattern first
            var likeMatch = Regex.Match(condition, @"^(\w+)\s+LIKE\s+['""]?(.+?)['""]?$", RegexOptions.IgnoreCase);
            if (likeMatch.Success)
            {
                var fieldName = likeMatch.Groups[1].Value;
                var likeValue = likeMatch.Groups[2].Value;
                
                // Remove wildcards for FilterValue (AppFilter will handle LIKE operator)
                var cleanValue = likeValue.Replace("%", "").Replace("*", "");
                
                return new AppFilter
                {
                    FieldName = fieldName,
                    Operator = "LIKE",
                    FilterValue = cleanValue
                };
            }

            // Try IN pattern
            var inMatch = Regex.Match(condition, @"^(\w+)\s+IN\s*\((.+?)\)$", RegexOptions.IgnoreCase);
            if (inMatch.Success)
            {
                var fieldName = inMatch.Groups[1].Value;
                var inValues = inMatch.Groups[2].Value;
                
                // Remove quotes from values
                inValues = Regex.Replace(inValues, @"['""]", "");
                
                return new AppFilter
                {
                    FieldName = fieldName,
                    Operator = "IN",
                    FilterValue = inValues
                };
            }

            // Try standard comparison operators: =, !=, <>, >, >=, <, <=
            // Pattern: field operator 'value' or field operator value
            var comparisonPattern = @"^(\w+)\s*(=|!=|<>|>=|<=|>|<)\s*(.+?)$";
            var comparisonMatch = Regex.Match(condition, comparisonPattern, RegexOptions.IgnoreCase);
            if (comparisonMatch.Success)
            {
                var fieldName = comparisonMatch.Groups[1].Value;
                var operatorStr = comparisonMatch.Groups[2].Value;
                var value = comparisonMatch.Groups[3].Value.Trim();

                // Remove quotes from value
                value = Regex.Replace(value, @"^['""](.+?)['""]$", "$1");

                // Normalize operator
                if (operatorStr == "<>")
                    operatorStr = "!=";

                return new AppFilter
                {
                    FieldName = fieldName,
                    Operator = operatorStr,
                    FilterValue = value
                };
            }

            // If no pattern matches, return null
            return null;
        }

        /// <summary>
        /// Gets child entities by parent key(s)
        /// Uses metadata to automatically determine foreign key columns
        /// </summary>
        /// <param name="parentTableName">Parent table name</param>
        /// <param name="parentKey">Parent primary key value (for single-column primary keys)</param>
        /// <returns>Child entities matching the parent key</returns>
        public virtual async Task<IEnumerable<object>> GetChildrenByParentKeyAsync(string parentTableName, object parentKey)
        {
            return await GetChildrenByParentKeysAsync(parentTableName, new Dictionary<string, object> { { await GetParentPrimaryKeyColumnAsync(parentTableName), parentKey } });
        }

        /// <summary>
        /// Gets child entities by parent entity object
        /// Extracts primary key values from the parent entity automatically
        /// </summary>
        /// <param name="parentEntity">Parent entity object</param>
        /// <param name="parentTableName">Parent table name (optional, defaults to entity type name)</param>
        /// <returns>Child entities matching the parent entity's primary key</returns>
        public virtual async Task<IEnumerable<object>> GetChildrenByParentEntityAsync(object parentEntity, string parentTableName = null)
        {
            if (parentEntity == null)
                throw new ArgumentNullException(nameof(parentEntity));

            var parentEntityType = parentEntity.GetType();
            parentTableName = parentTableName ?? parentEntityType.Name;

            // Get parent metadata to determine primary key columns
            var parentMetadata = await _metadata.GetTableMetadataAsync(parentTableName);
            if (parentMetadata == null)
            {
                throw new InvalidOperationException($"Metadata not found for parent table: {parentTableName}");
            }

            // Extract primary key values from parent entity
            var parentKeys = new Dictionary<string, object>();
            var parentPkColumns = parentMetadata.PrimaryKeyColumn.Split(',').Select(c => c.Trim()).ToList();

            foreach (var pkColumn in parentPkColumns)
            {
                var pkProperty = parentEntityType.GetProperty(pkColumn, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (pkProperty != null)
                {
                    var pkValue = pkProperty.GetValue(parentEntity);
                    parentKeys[pkColumn] = pkValue;
                }
                else
                {
                    throw new InvalidOperationException($"Primary key property '{pkColumn}' not found on parent entity type {parentEntityType.Name}");
                }
            }

            return await GetChildrenByParentKeysAsync(parentTableName, parentKeys);
        }

        /// <summary>
        /// Gets child entities by parent key(s) - supports composite primary keys
        /// Uses metadata to automatically determine foreign key columns
        /// </summary>
        /// <param name="parentTableName">Parent table name</param>
        /// <param name="parentKeys">Dictionary of parent primary key column names and values (for composite keys)</param>
        /// <returns>Child entities matching the parent keys</returns>
        public virtual async Task<IEnumerable<object>> GetChildrenByParentKeysAsync(string parentTableName, Dictionary<string, object> parentKeys)
        {
            if (string.IsNullOrWhiteSpace(parentTableName))
                throw new ArgumentException("Parent table name cannot be null or empty", nameof(parentTableName));

            if (parentKeys == null || parentKeys.Count == 0)
                throw new ArgumentException("Parent keys cannot be null or empty", nameof(parentKeys));

            var tableMetadata = await GetTableMetadataAsync(_tableName);
            
            // Find foreign keys that reference the parent table
            var foreignKeys = tableMetadata.ForeignKeys
                .Where(fk => string.Equals(fk.ReferencedTable, parentTableName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (foreignKeys.Count == 0)
            {
                throw new InvalidOperationException($"No foreign key relationship found from {_tableName} to {parentTableName}");
            }

            // Build filters for each foreign key
            var filters = new List<AppFilter>();
            
            // Get parent metadata to understand primary key structure
            var parentMetadata = await _metadata.GetTableMetadataAsync(parentTableName);
            if (parentMetadata == null)
            {
                throw new InvalidOperationException($"Metadata not found for parent table: {parentTableName}");
            }

            var parentPkColumns = parentMetadata.PrimaryKeyColumn.Split(',').Select(c => c.Trim()).ToList();
            
            // Match foreign keys to parent primary key columns
            // For composite keys, we need to match by position or by column name
            foreach (var fk in foreignKeys)
            {
                // Try to find matching parent key value
                // First, try direct match by column name
                if (parentKeys.ContainsKey(fk.ForeignKeyColumn))
                {
                    // Format ID according to table's ID type configuration (PPDM uses string IDs)
                    var formattedId = _defaults.FormatIdForTable(_tableName, parentKeys[fk.ForeignKeyColumn]);
                    filters.Add(new AppFilter
                    {
                        FieldName = fk.ForeignKeyColumn,
                        FilterValue = formattedId,
                        Operator = "="
                    });
                }
                // If not found, try matching by referenced primary key column name
                else if (parentPkColumns.Count == 1 && parentKeys.ContainsKey(parentPkColumns[0]))
                {
                    // Single column primary key - use the value
                    // Format ID according to table's ID type configuration (PPDM uses string IDs)
                    var formattedId = _defaults.FormatIdForTable(_tableName, parentKeys[parentPkColumns[0]]);
                    filters.Add(new AppFilter
                    {
                        FieldName = fk.ForeignKeyColumn,
                        FilterValue = formattedId,
                        Operator = "="
                    });
                }
                // For composite keys, try to match by position
                else
                {
                    // Find the index of this foreign key column in the referenced primary key
                    var referencedPkColumns = fk.ReferencedPrimaryKey.Split(',').Select(c => c.Trim()).ToList();
                    var fkIndex = referencedPkColumns.IndexOf(fk.ForeignKeyColumn);
                    
                    if (fkIndex >= 0 && fkIndex < parentPkColumns.Count && parentKeys.ContainsKey(parentPkColumns[fkIndex]))
                    {
                        // Format ID according to table's ID type configuration (PPDM uses string IDs)
                        var formattedId = _defaults.FormatIdForTable(_tableName, parentKeys[parentPkColumns[fkIndex]]);
                        filters.Add(new AppFilter
                        {
                            FieldName = fk.ForeignKeyColumn,
                            FilterValue = formattedId,
                            Operator = "="
                        });
                    }
                }
            }

            // Also filter by active indicator
            filters.Add(new AppFilter
            {
                FieldName = "ACTIVE_IND",
                FilterValue = _defaults.GetActiveIndicatorYes(),
                Operator = "="
            });

            return await GetEntitiesWithFiltersAsync(_entityType, _tableName, filters);
        }

        /// <summary>
        /// Gets the parent primary key column name for a given parent table
        /// </summary>
        private async Task<string> GetParentPrimaryKeyColumnAsync(string parentTableName)
        {
            var parentMetadata = await GetTableMetadataAsync(parentTableName);
            if (parentMetadata == null)
            {
                throw new InvalidOperationException($"Metadata not found for parent table: {parentTableName}");
            }

            // If composite key, return the first column (caller should use GetChildrenByParentKeysAsync for composite keys)
            var pkColumns = parentMetadata.PrimaryKeyColumn.Split(',');
            return pkColumns[0].Trim();
        }

        /// <summary>
        /// Gets all parent key information for this entity type from metadata
        /// Returns a dictionary mapping parent table names to their foreign key columns and referenced primary key columns
        /// Useful for understanding what parent keys are required when creating new entities
        /// </summary>
        /// <returns>Dictionary mapping parent table names to foreign key information</returns>
        public virtual async Task<Dictionary<string, ParentKeyInfo>> GetParentKeyInfoAsync()
        {
            var tableMetadata = await GetTableMetadataAsync(_tableName);
            var result = new Dictionary<string, ParentKeyInfo>(StringComparer.OrdinalIgnoreCase);

            foreach (var fk in tableMetadata.ForeignKeys)
            {
                if (!result.ContainsKey(fk.ReferencedTable))
                {
                    result[fk.ReferencedTable] = new ParentKeyInfo
                    {
                        ParentTableName = fk.ReferencedTable,
                        ForeignKeyColumns = new List<string>(),
                        ReferencedPrimaryKeyColumns = fk.ReferencedPrimaryKey.Split(',').Select(c => c.Trim()).ToList(),
                        RelationshipType = fk.RelationshipType
                    };
                }

                if (!result[fk.ReferencedTable].ForeignKeyColumns.Contains(fk.ForeignKeyColumn))
                {
                    result[fk.ReferencedTable].ForeignKeyColumns.Add(fk.ForeignKeyColumn);
                }
            }

            return result;
        }

        /// <summary>
        /// Information about parent key relationships for an entity
        /// </summary>
        public class ParentKeyInfo
        {
            /// <summary>
            /// Parent table name
            /// </summary>
            public string ParentTableName { get; set; }

            /// <summary>
            /// Foreign key column names in this entity that reference the parent
            /// </summary>
            public List<string> ForeignKeyColumns { get; set; } = new List<string>();

            /// <summary>
            /// Referenced primary key column names in the parent table
            /// </summary>
            public List<string> ReferencedPrimaryKeyColumns { get; set; } = new List<string>();

            /// <summary>
            /// Relationship type (OneToOne, OneToMany, ManyToMany)
            /// </summary>
            public string RelationshipType { get; set; }
        }

        /// <summary>
        /// Validates and sets parent keys when creating a new entity
        /// Uses metadata to determine required foreign keys
        /// </summary>
        /// <param name="entity">Entity to validate and set parent keys for</param>
        /// <param name="parentKeys">Dictionary of parent table names and their key values</param>
        protected virtual async Task ValidateAndSetParentKeysAsync(object entity, Dictionary<string, object> parentKeys)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (parentKeys == null || parentKeys.Count == 0)
                return; // No parent keys to set

            var tableMetadata = await GetTableMetadataAsync(_tableName);
            var entityType = entity.GetType();

            foreach (var parentKeyKvp in parentKeys)
            {
                var parentTableName = parentKeyKvp.Key;
                var parentKeyValue = parentKeyKvp.Value;

                // Find foreign keys that reference this parent table
                var foreignKeys = tableMetadata.ForeignKeys
                    .Where(fk => string.Equals(fk.ReferencedTable, parentTableName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (foreignKeys.Count == 0)
                {
                    throw new InvalidOperationException($"No foreign key relationship found from {_tableName} to {parentTableName}. Cannot set parent key.");
                }

                // Set the foreign key column(s) on the entity
                foreach (var fk in foreignKeys)
                {
                    var fkProperty = entityType.GetProperty(fk.ForeignKeyColumn, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (fkProperty != null && fkProperty.CanWrite)
                    {
                        // Convert parent key value to the property type
                        var convertedValue = Convert.ChangeType(parentKeyValue, fkProperty.PropertyType);
                        fkProperty.SetValue(entity, convertedValue);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Foreign key property '{fk.ForeignKeyColumn}' not found or not writable on entity type {entityType.Name}");
                    }
                }
            }
        }

        /// <summary>
        /// Inserts a new entity with parent key validation and automatic setting
        /// </summary>
        /// <param name="entity">Entity to insert</param>
        /// <param name="userId">User ID for audit columns</param>
        /// <param name="parentKeys">Optional dictionary of parent table names and their key values to set automatically</param>
        /// <returns>Inserted entity</returns>
        public virtual async Task<object> InsertWithParentKeysAsync(object entity, string userId, Dictionary<string, object> parentKeys = null)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // Validate and set parent keys if provided
            if (parentKeys != null && parentKeys.Count > 0)
            {
                await ValidateAndSetParentKeysAsync(entity, parentKeys);
            }

            // Use base insert method
            return await InsertAsync(entity, userId);
        }

        /// <summary>
        /// Inserts multiple entities with parent key validation and automatic setting
        /// </summary>
        /// <param name="entities">Entities to insert</param>
        /// <param name="userId">User ID for audit columns</param>
        /// <param name="parentKeys">Optional dictionary of parent table names and their key values to set automatically for all entities</param>
        /// <returns>Inserted entities</returns>
        public virtual async Task<IEnumerable<object>> InsertBatchWithParentKeysAsync(IEnumerable<object> entities, string userId, Dictionary<string, object> parentKeys = null)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            var entityList = entities.ToList();

            // Validate and set parent keys for each entity if provided
            if (parentKeys != null && parentKeys.Count > 0)
            {
                foreach (var entity in entityList)
                {
                    await ValidateAndSetParentKeysAsync(entity, parentKeys);
                }
            }

            // Use base batch insert method (existing InsertBatchAsync from parent class)
            var uow = GetOrCreateUnitOfWork(_entityType, _tableName);
            uow.EntityName = _tableName;
            
            foreach (var entity in entityList)
            {
                _commonColumnHandler.PrepareForInsert(entity as IPPDMEntity, userId);
                var result = await uow.InsertAsync(entity as Entity);
                if (result != null && !string.IsNullOrEmpty(result.Message))
                {
                    throw new Exception($"Batch insert failed: {result.Message}");
                }
            }

            return entityList;
        }

        /// <summary>
        /// Gets an entity by its primary key using AppFilter
        /// Uses metadata to determine primary key column
        /// Uses defaults configuration to format ID correctly (all PPDM tables use string IDs)
        /// </summary>
        public virtual async Task<object> GetByIdAsync(object id)
        {
            var metadata = await GetTableMetadataAsync(_tableName);
            var primaryKeyName = metadata.PrimaryKeyColumn;
            
            // Format ID according to table's ID type configuration (PPDM uses string IDs)
            var formattedId = _defaults.FormatIdForTable(_tableName, id);
            
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = primaryKeyName, FilterValue = formattedId, Operator = "=" }
            };

            var result = await GetEntitiesWithFiltersAsync(_entityType, _tableName, filters);
            return result?.FirstOrDefault();
        }

        /// <summary>
        /// Gets all active entities (ACTIVE_IND = 'Y') using AppFilter
        /// </summary>
        public virtual async Task<IEnumerable<object>> GetActiveAsync()
        {
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = _defaults.GetActiveIndicatorYes(), Operator = "=" }
            };

            return await GetEntitiesWithFiltersAsync(_entityType, _tableName, filters);
        }

        /// <summary>
        /// Gets entities matching filters
        /// </summary>
        public virtual async Task<IEnumerable<object>> GetAsync(List<AppFilter> filters)
        {
            _logger?.LogDebug("Getting entities from table {TableName} with {FilterCount} filters", 
                _tableName, filters?.Count ?? 0);
            var result = await GetEntitiesWithFiltersAsync(_entityType, _tableName, filters ?? new List<AppFilter>());
            var resultList = result.ToList();
            _logger?.LogDebug("Retrieved {Count} entities from table {TableName}", resultList.Count, _tableName);
            return resultList;
        }

        /// <summary>
        /// Inserts a new entity (automatically handles common columns)
        /// </summary>
        public virtual async Task<object> InsertAsync(object entity, string userId)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            _logger?.LogDebug("Inserting entity of type {EntityType} into table {TableName}", _entityType.Name, _tableName);

            // Validate entity type
            if (!_entityType.IsAssignableFrom(entity.GetType()))
            {
                throw new ArgumentException($"Entity type {entity.GetType().Name} does not match repository entity type {_entityType.Name}", nameof(entity));
            }

            // Prepare entity for insertion
            _commonColumnHandler.PrepareForInsert(entity as IPPDMEntity, userId);

            // Insert into database using IDataSource.InsertEntity directly
            var dataSource = _editor.GetDataSource(_connectionName);
            if (dataSource == null)
            {
                throw new InvalidOperationException($"DataSource not found for connection: {_connectionName}");
            }

            var result = dataSource.InsertEntity(_tableName, entity);
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Insert failed for table {TableName}: {Message}", _tableName, errorMessage);
                throw new Exception($"Insert failed: {errorMessage}");
            }

            _logger?.LogDebug("Successfully inserted entity into table {TableName}", _tableName);
            return entity;
        }

        /// <summary>
        /// Inserts multiple entities in a batch
        /// </summary>
        public virtual async Task<IEnumerable<object>> InsertBatchAsync(IEnumerable<object> entities, string userId)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            var entityList = entities.ToList();
            var dataSource = _editor.GetDataSource(_connectionName);
            if (dataSource == null)
            {
                throw new InvalidOperationException($"DataSource not found for connection: {_connectionName}");
            }
            
            foreach (var entity in entityList)
            {
                if (!_entityType.IsAssignableFrom(entity.GetType()))
                {
                    throw new ArgumentException($"Entity type {entity.GetType().Name} does not match repository entity type {_entityType.Name}", nameof(entities));
                }

                _commonColumnHandler.PrepareForInsert(entity as IPPDMEntity, userId);
                var result = dataSource.InsertEntity(_tableName, entity);
                if (result != null && result.Errors != null && result.Errors.Count > 0)
                {
                    var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                    throw new Exception($"Batch insert failed: {errorMessage}");
                }
            }

            return entityList;
        }

        /// <summary>
        /// Updates an existing entity (automatically handles common columns)
        /// </summary>
        public virtual async Task<object> UpdateAsync(object entity, string userId)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            _logger?.LogDebug("Updating entity of type {EntityType} in table {TableName}", _entityType.Name, _tableName);

            // Validate entity type
            if (!_entityType.IsAssignableFrom(entity.GetType()))
            {
                throw new ArgumentException($"Entity type {entity.GetType().Name} does not match repository entity type {_entityType.Name}", nameof(entity));
            }

            // Prepare entity for update
            _commonColumnHandler.PrepareForUpdate(entity as IPPDMEntity, userId);

            // Update in database using IDataSource.UpdateEntity directly
            var dataSource = _editor.GetDataSource(_connectionName);
            if (dataSource == null)
            {
                throw new InvalidOperationException($"DataSource not found for connection: {_connectionName}");
            }

            var result = dataSource.UpdateEntity(_tableName, entity);
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Update failed for table {TableName}: {Message}", _tableName, errorMessage);
                throw new Exception($"Update failed: {errorMessage}");
            }

            _logger?.LogDebug("Successfully updated entity in table {TableName}", _tableName);
            return entity;
        }

        /// <summary>
        /// Updates multiple entities in a batch
        /// </summary>
        public virtual async Task<IEnumerable<object>> UpdateBatchAsync(IEnumerable<object> entities, string userId)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            var entityList = entities.ToList();
            var dataSource = _editor.GetDataSource(_connectionName);
            if (dataSource == null)
            {
                throw new InvalidOperationException($"DataSource not found for connection: {_connectionName}");
            }
            
            foreach (var entity in entityList)
            {
                if (!_entityType.IsAssignableFrom(entity.GetType()))
                {
                    throw new ArgumentException($"Entity type {entity.GetType().Name} does not match repository entity type {_entityType.Name}", nameof(entities));
                }

                _commonColumnHandler.PrepareForUpdate(entity as IPPDMEntity, userId);
                var result = dataSource.UpdateEntity(_tableName, entity);
                if (result != null && result.Errors != null && result.Errors.Count > 0)
                {
                    var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                    throw new Exception($"Batch update failed: {errorMessage}");
                }
            }

            return entityList;
        }

        /// <summary>
        /// Soft deletes an entity (sets ACTIVE_IND = 'N') using defaults
        /// Uses defaults configuration to format ID correctly (all PPDM tables use string IDs)
        /// </summary>
        public virtual async Task<bool> SoftDeleteAsync(object id, string userId)
        {
            // Format ID according to table's ID type configuration (PPDM uses string IDs)
            var formattedId = _defaults.FormatIdForTable(_tableName, id);
            var entity = await GetByIdAsync(formattedId);
            if (entity == null)
                return false;

            _commonColumnHandler.SoftDelete(entity as IPPDMEntity, userId);
            await UpdateAsync(entity, userId);

            return true;
        }

        /// <summary>
        /// Hard deletes an entity from the database
        /// </summary>
        public virtual async Task<bool> DeleteAsync(object entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _logger?.LogDebug("Deleting entity of type {EntityType} from table {TableName}", _entityType.Name, _tableName);

            // Delete from database using IDataSource.DeleteEntity directly
            var dataSource = _editor.GetDataSource(_connectionName);
            if (dataSource == null)
            {
                throw new InvalidOperationException($"DataSource not found for connection: {_connectionName}");
            }

            var result = dataSource.DeleteEntity(_tableName, entity);
            bool success = result == null || (result.Errors == null || result.Errors.Count == 0);
            
            if (success)
            {
                _logger?.LogDebug("Successfully deleted entity from table {TableName}", _tableName);
            }
            else
            {
                var errorMessage = result?.Errors != null ? string.Join("; ", result.Errors.Select(e => e.Message)) : "Unknown error";
                _logger?.LogWarning("Delete operation failed: {Message}", errorMessage);
            }
            
            return success;
        }

        /// <summary>
        /// Hard deletes an entity by ID
        /// Uses defaults configuration to format ID correctly (all PPDM tables use string IDs)
        /// </summary>
        public virtual async Task<bool> DeleteByIdAsync(object id, string userId)
        {
            // Format ID according to table's ID type configuration (PPDM uses string IDs)
            var formattedId = _defaults.FormatIdForTable(_tableName, id);
            var entity = await GetByIdAsync(formattedId);
            if (entity == null)
            {
                _logger?.LogWarning("Entity not found for deletion, ID: {Id}", formattedId);
                return false;
            }

            return await DeleteAsync(entity);
        }

        /// <summary>
        /// Checks if an entity exists
        /// </summary>
        public virtual async Task<bool> ExistsAsync(object id)
        {
            var entity = await GetByIdAsync(id);
            return entity != null;
        }

        /// <summary>
        /// Gets the count of entities matching filters
        /// </summary>
        public virtual async Task<int> CountAsync(List<AppFilter> filters = null)
        {
            var entities = await GetEntitiesWithFiltersAsync(_entityType, _tableName, filters ?? new List<AppFilter>());
            return entities.Count();
        }

        /// <summary>
        /// Validates foreign key values in imported data (e.g., from CSV import)
        /// Checks if foreign key values exist in their referenced tables before import
        /// </summary>
        /// <param name="columnValues">Dictionary of column names and their values from imported row</param>
        /// <param name="rowNumber">Row number in the import file (for error reporting)</param>
        /// <returns>List of validation errors, empty if all foreign keys are valid</returns>
        public virtual async Task<List<ForeignKeyValidationError>> ValidateForeignKeyValuesAsync(
            Dictionary<string, string> columnValues, 
            int rowNumber = 0)
        {
            var errors = new List<ForeignKeyValidationError>();
            
            if (columnValues == null || columnValues.Count == 0)
                return errors;

            // Get table metadata to find foreign keys
            var tableMetadata = await GetTableMetadataAsync(_tableName);
            
            if (tableMetadata.ForeignKeys == null || tableMetadata.ForeignKeys.Count == 0)
                return errors; // No foreign keys to validate

            // Validate each foreign key
            foreach (var fk in tableMetadata.ForeignKeys)
            {
                // Check if this foreign key column has a value in the imported data
                if (!columnValues.ContainsKey(fk.ForeignKeyColumn) || 
                    string.IsNullOrWhiteSpace(columnValues[fk.ForeignKeyColumn]))
                {
                    // Foreign key is null/empty - this might be valid if it's nullable
                    // Skip validation for null values (can be enhanced to check if column is nullable)
                    continue;
                }

                var fkValue = columnValues[fk.ForeignKeyColumn];
                
                // Get metadata for the referenced table
                var referencedTableMetadata = await _metadata.GetTableMetadataAsync(fk.ReferencedTable);
                if (referencedTableMetadata == null)
                {
                    errors.Add(new ForeignKeyValidationError
                    {
                        RowNumber = rowNumber,
                        ForeignKeyColumn = fk.ForeignKeyColumn,
                        ReferencedTable = fk.ReferencedTable,
                        ForeignKeyValue = fkValue,
                        ErrorMessage = $"Referenced table '{fk.ReferencedTable}' not found in metadata"
                    });
                    continue;
                }

                // Check if the value exists in the referenced table
                var referencedPkColumn = referencedTableMetadata.PrimaryKeyColumn;
                var filters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = referencedPkColumn,
                        FilterValue = fkValue,
                        Operator = "="
                    }
                };

                try
                {
                    // Get entity type for referenced table
                    var referencedEntityType = GetEntityTypeForTable(fk.ReferencedTable);
                    if (referencedEntityType == null)
                    {
                        errors.Add(new ForeignKeyValidationError
                        {
                            RowNumber = rowNumber,
                            ForeignKeyColumn = fk.ForeignKeyColumn,
                            ReferencedTable = fk.ReferencedTable,
                            ForeignKeyValue = fkValue,
                            ErrorMessage = $"Entity type not found for referenced table '{fk.ReferencedTable}'"
                        });
                        continue;
                    }

                    // Query the referenced table to check if the value exists
                    var existingRecords = await GetEntitiesWithFiltersAsync(
                        referencedEntityType, 
                        fk.ReferencedTable, 
                        filters);

                    if (existingRecords == null || !existingRecords.Any())
                    {
                        errors.Add(new ForeignKeyValidationError
                        {
                            RowNumber = rowNumber,
                            ForeignKeyColumn = fk.ForeignKeyColumn,
                            ReferencedTable = fk.ReferencedTable,
                            ReferencedPrimaryKeyColumn = referencedPkColumn,
                            ForeignKeyValue = fkValue,
                            ErrorMessage = $"Foreign key value '{fkValue}' does not exist in referenced table '{fk.ReferencedTable}' (Primary Key: {referencedPkColumn})"
                        });
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(new ForeignKeyValidationError
                    {
                        RowNumber = rowNumber,
                        ForeignKeyColumn = fk.ForeignKeyColumn,
                        ReferencedTable = fk.ReferencedTable,
                        ForeignKeyValue = fkValue,
                        ErrorMessage = $"Error validating foreign key: {ex.Message}"
                    });
                }
            }

            return errors;
        }

        /// <summary>
        /// Validates foreign key values for multiple rows (batch validation for CSV import)
        /// </summary>
        /// <param name="rows">List of dictionaries, each representing a row with column names and values</param>
        /// <returns>List of all validation errors across all rows</returns>
        public virtual async Task<List<ForeignKeyValidationError>> ValidateForeignKeyValuesBatchAsync(
            List<Dictionary<string, string>> rows)
        {
            var allErrors = new List<ForeignKeyValidationError>();

            for (int i = 0; i < rows.Count; i++)
            {
                var rowErrors = await ValidateForeignKeyValuesAsync(rows[i], i + 1);
                allErrors.AddRange(rowErrors);
            }

            return allErrors;
        }

        /// <summary>
        /// Gets the entity type for a given table name
        /// </summary>
        private Type? GetEntityTypeForTable(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return null;

            // Try to find the entity type in the PPDM39.Models assembly
            var assembly = typeof(Beep.OilandGas.PPDM39.Models.STRAT_UNIT).Assembly;
            
            // Try exact match first
            var entityType = assembly.GetType($"Beep.OilandGas.PPDM39.Models.{tableName}");
            if (entityType != null)
                return entityType;

            // Try case-insensitive search
            entityType = assembly.GetTypes()
                .FirstOrDefault(t => t.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase) &&
                                    typeof(IPPDMEntity).IsAssignableFrom(t));

            return entityType;
        }

        #region Batch Operations

        /// <summary>
        /// Inserts multiple entities in a single batch operation with configurable batch size
        /// </summary>
        /// <param name="entities">Entities to insert</param>
        /// <param name="userId">User ID for audit columns</param>
        /// <param name="batchSize">Number of entities to insert per batch (default: 100)</param>
        /// <returns>List of inserted entities</returns>
        public virtual async Task<List<object>> InsertBatchWithSizeAsync(IEnumerable<object> entities, string userId, int batchSize = 100)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            var entityList = entities.ToList();
            if (entityList.Count == 0)
                return new List<object>();

            var results = new List<object>();
            var uow = GetOrCreateUnitOfWork(_entityType, _tableName);
            uow.EntityName = _tableName;

            // Process in batches
            for (int i = 0; i < entityList.Count; i += batchSize)
            {
                var batch = entityList.Skip(i).Take(batchSize);
                var batchList = batch.ToList();

                foreach (var entity in batchList)
                {
                    try
                    {
                        // Validate entity type
                        if (!_entityType.IsAssignableFrom(entity.GetType()))
                        {
                            throw new ArgumentException($"Entity type {entity.GetType().Name} does not match repository entity type {_entityType.Name}");
                        }

                        // Apply defaults and common columns
                        _commonColumnHandler.PrepareForInsert(entity as IPPDMEntity, userId);
                        
                        // Insert entity
                        var result = await uow.InsertAsync(entity as Entity);
                        if (result != null && !string.IsNullOrEmpty(result.Message))
                        {
                            throw new Exception($"Batch insert failed: {result.Message}");
                        }
                        results.Add(entity);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error inserting entity in batch at index {Index}", i);
                        throw;
                    }
                }
            }

            _logger?.LogInformation("Batch insert completed: {Count} entities inserted into {TableName}", results.Count, _tableName);
            return results;
        }

        /// <summary>
        /// Updates multiple entities in a single batch operation
        /// </summary>
        /// <param name="entities">Entities to update</param>
        /// <param name="userId">User ID for audit columns</param>
        /// <param name="batchSize">Number of entities to update per batch (default: 100)</param>
        /// <returns>List of updated entities</returns>
        public virtual async Task<List<object>> UpdateBatchAsync(IEnumerable<object> entities, string userId, int batchSize = 100)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            var entityList = entities.ToList();
            if (entityList.Count == 0)
                return new List<object>();

            var results = new List<object>();
            var uow = GetOrCreateUnitOfWork(_entityType, _tableName);
            uow.EntityName = _tableName;

            // Process in batches
            for (int i = 0; i < entityList.Count; i += batchSize)
            {
                var batch = entityList.Skip(i).Take(batchSize);
                var batchList = batch.ToList();

                foreach (var entity in batchList)
                {
                    try
                    {
                        // Validate entity type
                        if (!_entityType.IsAssignableFrom(entity.GetType()))
                        {
                            throw new ArgumentException($"Entity type {entity.GetType().Name} does not match repository entity type {_entityType.Name}");
                        }

                        // Apply common columns for update
                        _commonColumnHandler.PrepareForUpdate(entity as IPPDMEntity, userId);
                        
                        // Update entity
                        var result = await uow.UpdateAsync(entity as Entity);
                        if (result != null && !string.IsNullOrEmpty(result.Message))
                        {
                            throw new Exception($"Batch update failed: {result.Message}");
                        }
                        results.Add(entity);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error updating entity in batch at index {Index}", i);
                        throw;
                    }
                }
            }

            _logger?.LogInformation("Batch update completed: {Count} entities updated in {TableName}", results.Count, _tableName);
            return results;
        }

        /// <summary>
        /// Deletes multiple entities by their IDs in a single batch operation
        /// </summary>
        /// <param name="ids">IDs of entities to delete</param>
        /// <param name="userId">User ID for audit columns (if soft delete)</param>
        /// <param name="softDelete">Whether to perform soft delete (set ACTIVE_IND = 'N') instead of hard delete</param>
        /// <param name="batchSize">Number of entities to delete per batch (default: 100)</param>
        /// <returns>Number of entities deleted</returns>
        public virtual async Task<int> DeleteBatchAsync(IEnumerable<string> ids, string userId, bool softDelete = true, int batchSize = 100)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            var idList = ids.ToList();
            if (idList.Count == 0)
                return 0;

            var tableMetadata = await GetTableMetadataAsync(_tableName);
            var pkColumn = tableMetadata.PrimaryKeyColumn.Split(',').First().Trim();
            var deletedCount = 0;
            var uow = GetOrCreateUnitOfWork(_entityType, _tableName);
            uow.EntityName = _tableName;

            // Process in batches
            for (int i = 0; i < idList.Count; i += batchSize)
            {
                var batch = idList.Skip(i).Take(batchSize).ToList();

                foreach (var id in batch)
                {
                    try
                    {
                        var formattedId = _defaults.FormatIdForTable(_tableName, id);
                        var filters = new List<AppFilter>
                        {
                            new AppFilter { FieldName = pkColumn, FilterValue = formattedId, Operator = "=" }
                        };

                        var entities = await GetAsync(filters);
                        var entity = entities.FirstOrDefault();

                        if (entity != null)
                        {
                            if (softDelete)
                            {
                                // Perform soft delete
                                if (entity is IPPDMEntity ppdmEntity)
                                {
                                    _commonColumnHandler.SoftDelete(ppdmEntity, userId);
                                }
                                await uow.UpdateAsync(entity);
                            }
                            else
                            {
                                // Perform hard delete
                                await uow.DeleteAsync(entity);
                            }
                            deletedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error deleting entity with ID {Id} in batch", id);
                        throw;
                    }
                }

                // Commit batch
                await uow.Commit();
            }

            _logger?.LogInformation("Batch delete completed: {Count} entities deleted from {TableName} (SoftDelete: {SoftDelete})", 
                deletedCount, _tableName, softDelete);
            return deletedCount;
        }

        /// <summary>
        /// Performs bulk upsert (insert or update) operation
        /// Updates existing entities if they exist, otherwise inserts new ones
        /// </summary>
        /// <param name="entities">Entities to upsert</param>
        /// <param name="userId">User ID for audit columns</param>
        /// <param name="batchSize">Number of entities to process per batch (default: 100)</param>
        /// <returns>List of upserted entities</returns>
        public virtual async Task<List<object>> UpsertBatchAsync(IEnumerable<object> entities, string userId, int batchSize = 100)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            var entityList = entities.ToList();
            if (entityList.Count == 0)
                return new List<object>();

            var results = new List<object>();
            var tableMetadata = await GetTableMetadataAsync(_tableName);
            var pkColumn = tableMetadata.PrimaryKeyColumn.Split(',').First().Trim();
            var uow = GetOrCreateUnitOfWork(_entityType, _tableName);
            uow.EntityName = _tableName;

            // Process in batches
            for (int i = 0; i < entityList.Count; i += batchSize)
            {
                var batch = entityList.Skip(i).Take(batchSize).ToList();

                foreach (var entity in batch)
                {
                    try
                    {
                        var pkProperty = entity.GetType().GetProperty(pkColumn, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                        var pkValue = pkProperty?.GetValue(entity)?.ToString();

                        if (!string.IsNullOrEmpty(pkValue))
                        {
                            // Check if entity exists
                            var formattedId = _defaults.FormatIdForTable(_tableName, pkValue);
                            var filters = new List<AppFilter>
                            {
                                new AppFilter { FieldName = pkColumn, FilterValue = formattedId, Operator = "=" }
                            };

                            var existing = (await GetAsync(filters)).FirstOrDefault();

                            if (existing != null)
                            {
                                // Update existing entity
                                if (entity is IPPDMEntity ppdmEntity)
                                {
                                    _commonColumnHandler.PrepareForUpdate(ppdmEntity, userId);
                                }
                                var updated = await uow.UpdateAsync(entity);
                                if (updated != null)
                                    results.Add(updated);
                            }
                            else
                            {
                                // Insert new entity
                                if (entity is IPPDMEntity ppdmEntity)
                                {
                                    _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
                                }
                                var inserted = await uow.InsertAsync(entity);
                                if (inserted != null)
                                    results.Add(inserted);
                            }
                        }
                        else
                        {
                            // No primary key value - insert as new
                            if (entity is IPPDMEntity ppdmEntity)
                            {
                                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
                            }
                            var inserted = await uow.InsertAsync(entity);
                            if (inserted != null)
                                results.Add(inserted);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error upserting entity in batch at index {Index}", i);
                        throw;
                    }
                }

                // Commit batch
                await uow.Commit();
            }

            _logger?.LogInformation("Batch upsert completed: {Count} entities processed in {TableName}", results.Count, _tableName);
            return results;
        }

        #endregion

        #region Complex Queries and Aggregations

        /// <summary>
        /// Gets count of entities matching the filters
        /// </summary>
        /// <param name="filters">Optional filters to apply</param>
        /// <returns>Count of matching entities</returns>
        public virtual async Task<long> GetCountAsync(List<AppFilter>? filters = null)
        {
            var entities = await GetAsync(filters);
            return entities.LongCount();
        }

        /// <summary>
        /// Gets entities with pagination
        /// </summary>
        /// <param name="filters">Optional filters to apply</param>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortField">Field to sort by</param>
        /// <param name="sortDirection">Sort direction (ASC or DESC)</param>
        /// <returns>Paginated result with entities and total count</returns>
        public virtual async Task<PaginatedResult> GetPaginatedAsync(
            List<AppFilter>? filters = null,
            int pageNumber = 1,
            int pageSize = 50,
            string? sortField = null,
            string? sortDirection = "ASC")
        {
            var allEntities = await GetAsync(filters);
            var totalCount = allEntities.LongCount();

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(sortField))
            {
                var property = _entityType.GetProperty(sortField, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property != null)
                {
                    if (string.Equals(sortDirection, "DESC", StringComparison.OrdinalIgnoreCase))
                    {
                        allEntities = allEntities.OrderByDescending(e => property.GetValue(e)).ToList();
                    }
                    else
                    {
                        allEntities = allEntities.OrderBy(e => property.GetValue(e)).ToList();
                    }
                }
            }

            // Apply pagination
            var skip = (pageNumber - 1) * pageSize;
            var pagedEntities = allEntities.Skip(skip).Take(pageSize).ToList();

            return new PaginatedResult
            {
                Items = pagedEntities,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        /// <summary>
        /// Gets distinct values for a specific field
        /// </summary>
        /// <param name="fieldName">Field name to get distinct values for</param>
        /// <param name="filters">Optional filters to apply before getting distinct values</param>
        /// <returns>List of distinct values</returns>
        public virtual async Task<List<object?>> GetDistinctAsync(string fieldName, List<AppFilter>? filters = null)
        {
            var entities = await GetAsync(filters);
            var property = _entityType.GetProperty(fieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            
            if (property == null)
                throw new ArgumentException($"Field '{fieldName}' not found on entity type {_entityType.Name}", nameof(fieldName));

            return entities
                .Select(e => property.GetValue(e))
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Gets aggregated value for a numeric field
        /// </summary>
        /// <param name="fieldName">Numeric field name</param>
        /// <param name="aggregationType">Type of aggregation (SUM, AVG, MIN, MAX, COUNT)</param>
        /// <param name="filters">Optional filters to apply</param>
        /// <returns>Aggregated value</returns>
        public virtual async Task<decimal?> GetAggregateAsync(
            string fieldName,
            string aggregationType,
            List<AppFilter>? filters = null)
        {
            var entities = await GetAsync(filters);
            var property = _entityType.GetProperty(fieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (property == null)
                throw new ArgumentException($"Field '{fieldName}' not found on entity type {_entityType.Name}", nameof(fieldName));

            var values = entities
                .Select(e => property.GetValue(e))
                .Where(v => v != null)
                .Select(v =>
                {
                    if (v is decimal dec) return dec;
                    if (v is double dbl) return (decimal)dbl;
                    if (v is float flt) return (decimal)flt;
                    if (v is int i) return (decimal)i;
                    if (v is long lng) return (decimal)lng;
                    if (decimal.TryParse(v.ToString(), out var parsed)) return parsed;
                    return (decimal?)null;
                })
                .Where(v => v.HasValue)
                .Select(v => v!.Value)
                .ToList();

            if (values.Count == 0)
                return null;

            return aggregationType.ToUpperInvariant() switch
            {
                "SUM" => values.Sum(),
                "AVG" => values.Average(),
                "MIN" => values.Min(),
                "MAX" => values.Max(),
                "COUNT" => values.Count,
                _ => throw new ArgumentException($"Invalid aggregation type: {aggregationType}. Valid types are: SUM, AVG, MIN, MAX, COUNT", nameof(aggregationType))
            };
        }

        /// <summary>
        /// Gets grouped aggregated results
        /// </summary>
        /// <param name="groupByField">Field to group by</param>
        /// <param name="aggregateField">Numeric field to aggregate</param>
        /// <param name="aggregationType">Type of aggregation (SUM, AVG, MIN, MAX, COUNT)</param>
        /// <param name="filters">Optional filters to apply</param>
        /// <returns>Dictionary of group values to aggregated results</returns>
        public virtual async Task<Dictionary<string, decimal?>> GetGroupedAggregateAsync(
            string groupByField,
            string aggregateField,
            string aggregationType,
            List<AppFilter>? filters = null)
        {
            var entities = await GetAsync(filters);
            var groupByProperty = _entityType.GetProperty(groupByField, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            var aggregateProperty = _entityType.GetProperty(aggregateField, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (groupByProperty == null)
                throw new ArgumentException($"Group by field '{groupByField}' not found on entity type {_entityType.Name}", nameof(groupByField));
            
            if (aggregateProperty == null)
                throw new ArgumentException($"Aggregate field '{aggregateField}' not found on entity type {_entityType.Name}", nameof(aggregateField));

            var grouped = entities
                .GroupBy(e => groupByProperty.GetValue(e)?.ToString() ?? "NULL")
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var values = g
                            .Select(e => aggregateProperty.GetValue(e))
                            .Where(v => v != null)
                            .Select(v =>
                            {
                                if (v is decimal dec) return dec;
                                if (v is double dbl) return (decimal)dbl;
                                if (v is float flt) return (decimal)flt;
                                if (v is int i) return (decimal)i;
                                if (v is long lng) return (decimal)lng;
                                if (decimal.TryParse(v.ToString(), out var parsed)) return parsed;
                                return (decimal?)null;
                            })
                            .Where(v => v.HasValue)
                            .Select(v => v!.Value)
                            .ToList();

                        if (values.Count == 0)
                            return (decimal?)null;

                        return aggregationType.ToUpperInvariant() switch
                        {
                            "SUM" => values.Sum(),
                            "AVG" => values.Average(),
                            "MIN" => values.Min(),
                            "MAX" => values.Max(),
                            "COUNT" => values.Count,
                            _ => (decimal?)null
                        };
                    });

            return grouped;
        }

        /// <summary>
        /// Result of paginated query
        /// </summary>
        public class PaginatedResult
        {
            public List<object> Items { get; set; } = new List<object>();
            public long TotalCount { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
            public int TotalPages { get; set; }
        }

        #endregion

        #region Enhanced Relationship Navigation

        /// <summary>
        /// Gets related entities through a foreign key relationship
        /// </summary>
        /// <param name="entityId">ID of the source entity</param>
        /// <param name="relatedTableName">Name of the related table</param>
        /// <param name="foreignKeyColumn">Foreign key column name in the related table (if null, will be determined from metadata)</param>
        /// <returns>List of related entities</returns>
        public virtual async Task<List<object>> GetRelatedEntitiesAsync(
            string entityId,
            string relatedTableName,
            string? foreignKeyColumn = null)
        {
            if (string.IsNullOrWhiteSpace(entityId))
                throw new ArgumentException("Entity ID cannot be null or empty", nameof(entityId));

            if (string.IsNullOrWhiteSpace(relatedTableName))
                throw new ArgumentException("Related table name cannot be null or empty", nameof(relatedTableName));

            // Get metadata for related table
            var relatedMetadata = await _metadata.GetTableMetadataAsync(relatedTableName);
            if (relatedMetadata == null)
                throw new InvalidOperationException($"Metadata not found for related table: {relatedTableName}");

            // Find foreign key relationship
            if (string.IsNullOrWhiteSpace(foreignKeyColumn))
            {
                var fk = relatedMetadata.ForeignKeys
                    .FirstOrDefault(f => string.Equals(f.ReferencedTable, _tableName, StringComparison.OrdinalIgnoreCase));
                
                if (fk == null)
                    throw new InvalidOperationException($"No foreign key relationship found from {relatedTableName} to {_tableName}");

                foreignKeyColumn = fk.ForeignKeyColumn;
            }

            // Get related entity type
            var relatedEntityType = GetEntityTypeForTable(relatedTableName);
            if (relatedEntityType == null)
                throw new InvalidOperationException($"Entity type not found for table: {relatedTableName}");

            // Create repository for related table
            var relatedRepo = new PPDMGenericRepository(
                _editor,
                _commonColumnHandler,
                _defaults,
                _metadata,
                relatedEntityType,
                _connectionName,
                relatedTableName,
                _logger);

            // Build filter
            var formattedId = _defaults.FormatIdForTable(_tableName, entityId);
            var filters = new List<AppFilter>
            {
                new AppFilter
                {
                    FieldName = foreignKeyColumn,
                    FilterValue = formattedId,
                    Operator = "="
                }
            };

            return (await relatedRepo.GetAsync(filters)).ToList();
        }

        /// <summary>
        /// Gets parent entity through a foreign key relationship
        /// </summary>
        /// <param name="entityId">ID of the child entity</param>
        /// <param name="parentTableName">Name of the parent table</param>
        /// <param name="foreignKeyColumn">Foreign key column name in the current table (if null, will be determined from metadata)</param>
        /// <returns>Parent entity, or null if not found</returns>
        public virtual async Task<object?> GetParentEntityAsync(
            string entityId,
            string parentTableName,
            string? foreignKeyColumn = null)
        {
            if (string.IsNullOrWhiteSpace(entityId))
                throw new ArgumentException("Entity ID cannot be null or empty", nameof(entityId));

            if (string.IsNullOrWhiteSpace(parentTableName))
                throw new ArgumentException("Parent table name cannot be null or empty", nameof(parentTableName));

            // Get current entity to find foreign key value
            var tableMetadata = await GetTableMetadataAsync(_tableName);
            var pkColumn = tableMetadata.PrimaryKeyColumn.Split(',').First().Trim();
            var formattedId = _defaults.FormatIdForTable(_tableName, entityId);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = pkColumn, FilterValue = formattedId, Operator = "=" }
            };

            var entity = (await GetAsync(filters)).FirstOrDefault();
            if (entity == null)
                return null;

            // Find foreign key relationship
            if (string.IsNullOrWhiteSpace(foreignKeyColumn))
            {
                var fk = tableMetadata.ForeignKeys
                    .FirstOrDefault(f => string.Equals(f.ReferencedTable, parentTableName, StringComparison.OrdinalIgnoreCase));
                
                if (fk == null)
                    throw new InvalidOperationException($"No foreign key relationship found from {_tableName} to {parentTableName}");

                foreignKeyColumn = fk.ForeignKeyColumn;
            }

            // Get foreign key value from entity
            var fkProperty = _entityType.GetProperty(foreignKeyColumn, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (fkProperty == null)
                throw new InvalidOperationException($"Foreign key property '{foreignKeyColumn}' not found on entity type {_entityType.Name}");

            var fkValue = fkProperty.GetValue(entity)?.ToString();
            if (string.IsNullOrWhiteSpace(fkValue))
                return null;

            // Get parent entity type
            var parentEntityType = GetEntityTypeForTable(parentTableName);
            if (parentEntityType == null)
                throw new InvalidOperationException($"Entity type not found for table: {parentTableName}");

            // Create repository for parent table
            var parentRepo = new PPDMGenericRepository(
                _editor,
                _commonColumnHandler,
                _defaults,
                _metadata,
                parentEntityType,
                _connectionName,
                parentTableName,
                _logger);

            // Get parent metadata
            var parentMetadata = await _metadata.GetTableMetadataAsync(parentTableName);
            if (parentMetadata == null)
                throw new InvalidOperationException($"Metadata not found for parent table: {parentTableName}");

            var parentPkColumn = parentMetadata.PrimaryKeyColumn.Split(',').First().Trim();
            var formattedParentId = _defaults.FormatIdForTable(parentTableName, fkValue);

            var parentFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = parentPkColumn, FilterValue = formattedParentId, Operator = "=" }
            };

            return (await parentRepo.GetAsync(parentFilters)).FirstOrDefault();
        }

        /// <summary>
        /// Gets all relationships for an entity (both parent and child relationships)
        /// </summary>
        /// <param name="entityId">ID of the entity</param>
        /// <returns>Dictionary mapping relationship names to lists of related entities</returns>
        public virtual async Task<Dictionary<string, List<object>>> GetEntityRelationshipsAsync(string entityId)
        {
            if (string.IsNullOrWhiteSpace(entityId))
                throw new ArgumentException("Entity ID cannot be null or empty", nameof(entityId));

            var relationships = new Dictionary<string, List<object>>();
            var tableMetadata = await GetTableMetadataAsync(_tableName);

            // Get parent relationships (entities this entity references via foreign keys)
            foreach (var fk in tableMetadata.ForeignKeys)
            {
                try
                {
                    var parentEntity = await GetParentEntityAsync(entityId, fk.ReferencedTable, fk.ForeignKeyColumn);
                    if (parentEntity != null)
                    {
                        var relationshipName = $"Parent_{fk.ReferencedTable}";
                        relationships[relationshipName] = new List<object> { parentEntity };
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Error getting parent relationship {ReferencedTable} for entity {EntityId}", 
                        fk.ReferencedTable, entityId);
                }
            }

            // Get child relationships (entities that reference this entity via foreign keys)
            // This requires checking metadata for all tables to find those that reference this table
            // For now, we'll use a simplified approach - this could be enhanced with metadata queries
            // Note: Full implementation would require querying metadata for all tables

            return relationships;
        }

        #endregion

        #region File Import/Export with Mapping and Defaults

        /// <summary>
        /// Delegate for progress reporting
        /// </summary>
        public delegate void ProgressReportDelegate(string operationId, int percentage, string message, long? itemsProcessed = null, long? totalItems = null);

        /// <summary>
        /// Imports data from a CSV file with automatic mapping and default value application
        /// </summary>
        /// <param name="csvFilePath">Path to the CSV file</param>
        /// <param name="userId">User ID for audit columns</param>
        /// <param name="columnMapping">Optional column mapping (CSV column name -> Entity property name). If null, auto-mapping is used.</param>
        /// <param name="skipHeaderRow">Whether to skip the first row (header row)</param>
        /// <param name="validateForeignKeys">Whether to validate foreign keys before import</param>
        /// <param name="onProgress">Optional progress callback: (operationId, percentage, message, itemsProcessed, totalItems)</param>
        /// <param name="operationId">Optional operation ID for progress tracking</param>
        /// <returns>Import result with success count and errors</returns>
        public virtual async Task<FileImportResult> ImportFromCsvAsync(
            string csvFilePath,
            string userId,
            Dictionary<string, string> columnMapping = null,
            bool skipHeaderRow = true,
            bool validateForeignKeys = true,
            ProgressReportDelegate? onProgress = null,
            string? operationId = null)
        {
            if (string.IsNullOrWhiteSpace(csvFilePath))
                throw new ArgumentException("CSV file path cannot be null or empty", nameof(csvFilePath));

            if (!File.Exists(csvFilePath))
                throw new FileNotFoundException($"CSV file not found: {csvFilePath}");

            var result = new FileImportResult
            {
                FilePath = csvFilePath,
                TotalRows = 0,
                SuccessCount = 0,
                ErrorCount = 0,
                Errors = new List<FileImportError>()
            };

            _logger?.LogInformation("Starting CSV import for table {TableName} from file {FilePath} (OperationId: {OperationId})", 
                _tableName, csvFilePath, operationId ?? "none");

            try
            {
                if (onProgress != null && !string.IsNullOrEmpty(operationId))
                {
                    onProgress(operationId, 0, "Reading CSV file...");
                }

                // Read CSV file
                var csvLines = File.ReadAllLines(csvFilePath, Encoding.UTF8);
                if (csvLines.Length == 0)
                {
                    result.Errors.Add(new FileImportError
                    {
                        RowNumber = 0,
                        Message = "CSV file is empty"
                    });
                    return result;
                }

                if (onProgress != null && !string.IsNullOrEmpty(operationId))
                {
                    onProgress(operationId, 5, "Parsing CSV headers...");
                }

                // Parse header row
                var headerRow = ParseCsvLine(csvLines[0]);
                var csvColumnIndices = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < headerRow.Count; i++)
                {
                    var colName = headerRow[i].Trim('"').Trim();
                    if (!string.IsNullOrWhiteSpace(colName))
                    {
                        csvColumnIndices[colName] = i;
                    }
                }

                _logger?.LogDebug("Parsed {ColumnCount} columns from CSV header", csvColumnIndices.Count);

                // Build column mapping (use provided mapping or auto-map)
                var mapping = columnMapping ?? BuildAutoColumnMapping(csvColumnIndices.Keys);

                // Get table metadata
                var tableMetadata = await GetTableMetadataAsync(_tableName);

                // Prepare rows for validation
                var rowsForValidation = new List<Dictionary<string, string>>();
                var startRow = skipHeaderRow ? 1 : 0;
                result.TotalRows = csvLines.Length - startRow;

                if (onProgress != null && !string.IsNullOrEmpty(operationId))
                {
                    onProgress(operationId, 10, $"Parsing {result.TotalRows} rows for validation...", 0, result.TotalRows);
                }

                // First pass: Parse and validate
                int parsedRows = 0;
                for (int rowIndex = startRow; rowIndex < csvLines.Length; rowIndex++)
                {
                    var row = csvLines[rowIndex];
                    if (IsEmptyCsvRow(row))
                        continue;

                    try
                    {
                        var csvValues = ParseCsvLine(row);
                        var rowData = new Dictionary<string, string>();

                        // Map CSV columns to entity properties
                        foreach (var kvp in mapping)
                        {
                            var csvColumnName = kvp.Key;
                            var entityPropertyName = kvp.Value;

                            if (csvColumnIndices.ContainsKey(csvColumnName))
                            {
                                var colIndex = csvColumnIndices[csvColumnName];
                                if (colIndex < csvValues.Count)
                                {
                                    rowData[entityPropertyName] = csvValues[colIndex].Trim();
                                }
                            }
                        }

                        rowsForValidation.Add(rowData);
                        parsedRows++;
                        
                        // Update progress every 100 rows or at end
                        if (onProgress != null && !string.IsNullOrEmpty(operationId) && 
                            (parsedRows % 100 == 0 || rowIndex == csvLines.Length - 1))
                        {
                            var progress = 10 + (int)((parsedRows / (double)result.TotalRows) * 30); // 10-40% for parsing
                            onProgress(operationId, progress, 
                                $"Parsed {parsedRows}/{result.TotalRows} rows", parsedRows, result.TotalRows);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add(new FileImportError
                        {
                            RowNumber = rowIndex + 1,
                            Message = $"Error parsing row: {ex.Message}"
                        });
                        result.ErrorCount++;
                        _logger?.LogWarning("Error parsing CSV row {RowNumber}: {Error}", rowIndex + 1, ex.Message);
                    }
                }

                _logger?.LogInformation("Parsed {ParsedRows} rows, {ErrorCount} errors during parsing", parsedRows, result.ErrorCount);

                // Validate foreign keys if requested
                if (validateForeignKeys && rowsForValidation.Any())
                {
                    if (onProgress != null && !string.IsNullOrEmpty(operationId))
                    {
                        onProgress(operationId, 40, "Validating foreign keys...");
                    }
                    
                    var fkErrors = await ValidateForeignKeyValuesBatchAsync(rowsForValidation);
                    foreach (var fkError in fkErrors)
                    {
                        result.Errors.Add(new FileImportError
                        {
                            RowNumber = fkError.RowNumber,
                            Message = $"Foreign Key '{fkError.ForeignKeyColumn}' = '{fkError.ForeignKeyValue}': {fkError.ErrorMessage}"
                        });
                        result.ErrorCount++;
                    }
                    
                    _logger?.LogInformation("Validated foreign keys: {ErrorCount} errors found", fkErrors.Count);
                }

                // Second pass: Import valid rows
                if (onProgress != null && !string.IsNullOrEmpty(operationId))
                {
                    onProgress(operationId, 45, "Starting data import...", 0, result.TotalRows);
                }

                int importedRows = 0;
                int validRowsToImport = result.TotalRows - result.ErrorCount;
                
                for (int rowIndex = startRow; rowIndex < csvLines.Length; rowIndex++)
                {
                    var row = csvLines[rowIndex];
                    if (IsEmptyCsvRow(row))
                        continue;

                    var rowNumber = rowIndex + 1;
                    
                    // Skip if this row has errors
                    if (result.Errors.Any(e => e.RowNumber == rowNumber))
                        continue;

                    try
                    {
                        var csvValues = ParseCsvLine(row);
                        var entity = CreateEntityFromCsvRow(csvValues, csvColumnIndices, mapping, tableMetadata);

                        // Apply defaults
                        ApplyDefaultsToEntity(entity as IPPDMEntity);

                        // Insert using repository
                        await InsertAsync(entity, userId);
                        result.SuccessCount++;
                        importedRows++;
                        
                        // Update progress every 50 rows or at end
                        if (onProgress != null && !string.IsNullOrEmpty(operationId) && 
                            (importedRows % 50 == 0 || rowIndex == csvLines.Length - 1))
                        {
                            var progress = 45 + (int)((importedRows / (double)validRowsToImport) * 50); // 45-95% for import
                            onProgress(operationId, progress, 
                                $"Imported {importedRows}/{validRowsToImport} rows", importedRows, validRowsToImport);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add(new FileImportError
                        {
                            RowNumber = rowNumber,
                            Message = $"Error importing row: {ex.Message}"
                        });
                        result.ErrorCount++;
                        _logger?.LogWarning("Error importing row {RowNumber}: {Error}", rowNumber, ex.Message);
                    }
                }

                _logger?.LogInformation("Import completed: {SuccessCount} succeeded, {ErrorCount} errors", 
                    result.SuccessCount, result.ErrorCount);

                if (onProgress != null && !string.IsNullOrEmpty(operationId))
                {
                    onProgress(operationId, 100, 
                        $"Import completed: {result.SuccessCount} rows imported, {result.ErrorCount} errors", 
                        result.SuccessCount, result.TotalRows);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Fatal error during CSV import for table {TableName}", _tableName);
                result.Errors.Add(new FileImportError
                {
                    RowNumber = 0,
                    Message = $"Fatal error during import: {ex.Message}"
                });
                result.ErrorCount++;
                
                if (onProgress != null && !string.IsNullOrEmpty(operationId))
                {
                    onProgress(operationId, 100, $"Fatal error: {ex.Message}");
                }
            }

            return result;
        }

        /// <summary>
        /// Exports data to CSV file
        /// </summary>
        /// <param name="csvFilePath">Path where CSV file will be created</param>
        /// <param name="filters">Optional filters to apply to exported data</param>
        /// <param name="includeHeaders">Whether to include header row</param>
        /// <param name="onProgress">Optional progress callback: (operationId, percentage, message, itemsProcessed, totalItems)</param>
        /// <param name="operationId">Optional operation ID for progress tracking</param>
        /// <returns>Number of records exported</returns>
        public virtual async Task<int> ExportToCsvAsync(
            string csvFilePath,
            List<AppFilter> filters = null,
            bool includeHeaders = true,
            ProgressReportDelegate? onProgress = null,
            string? operationId = null)
        {
            if (string.IsNullOrWhiteSpace(csvFilePath))
                throw new ArgumentException("CSV file path cannot be null or empty", nameof(csvFilePath));

            _logger?.LogInformation("Starting CSV export for table {TableName} to file {FilePath} (OperationId: {OperationId})", 
                _tableName, csvFilePath, operationId ?? "none");

            if (onProgress != null && !string.IsNullOrEmpty(operationId))
            {
                onProgress(operationId, 0, "Querying data to export...");
            }

            // Get entities to export
            var entities = await GetAsync(filters ?? new List<AppFilter>());
            var entityList = entities.ToList();

            if (!entityList.Any())
            {
                _logger?.LogInformation("No entities found to export for table {TableName}", _tableName);
                if (onProgress != null && !string.IsNullOrEmpty(operationId))
                {
                    onProgress(operationId, 100, "Export completed: No data to export");
                }
                return 0;
            }

            _logger?.LogInformation("Found {EntityCount} entities to export", entityList.Count);

            // Get entity properties
            var properties = _entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead)
                .OrderBy(p => p.Name)
                .ToList();

            // Build CSV content
            var csvLines = new List<string>();

            // Add header row
            if (includeHeaders)
            {
                var headers = properties.Select(p => EscapeCsvValue(p.Name)).ToList();
                csvLines.Add(string.Join(",", headers));
            }

            // Add data rows
            int exportedCount = 0;
            foreach (var entity in entityList)
            {
                var values = properties.Select(p =>
                {
                    var value = p.GetValue(entity);
                    return EscapeCsvValue(value?.ToString() ?? string.Empty);
                }).ToList();

                csvLines.Add(string.Join(",", values));
                exportedCount++;
                
                        // Update progress every 100 entities or at end
                        if (onProgress != null && !string.IsNullOrEmpty(operationId) && 
                            (exportedCount % 100 == 0 || exportedCount == entityList.Count))
                        {
                            var progress = 20 + (int)((exportedCount / (double)entityList.Count) * 70); // 20-90% for export
                            onProgress(operationId, progress, 
                                $"Exported {exportedCount}/{entityList.Count} entities", exportedCount, entityList.Count);
                        }
            }

            if (onProgress != null && !string.IsNullOrEmpty(operationId))
            {
                onProgress(operationId, 95, "Writing CSV file...");
            }

            // Write to file
            await File.WriteAllLinesAsync(csvFilePath, csvLines, Encoding.UTF8);

            _logger?.LogInformation("CSV export completed: {ExportedCount} entities exported to {FilePath}", 
                exportedCount, csvFilePath);

            if (onProgress != null && !string.IsNullOrEmpty(operationId))
            {
                onProgress(operationId, 100, $"Export completed: {exportedCount} entities exported", exportedCount, entityList.Count);
            }

            return entityList.Count;
        }

        /// <summary>
        /// Builds automatic column mapping from CSV column names to entity property names
        /// </summary>
        private Dictionary<string, string> BuildAutoColumnMapping(IEnumerable<string> csvColumns)
        {
            var mapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var properties = _entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite)
                .ToList();

            // Common mappings
            var commonMappings = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                { "NAME1", new[] { "STATUS", "LONG_NAME", "NAME", "SHORT_NAME" } },
                { "DEFINITION", new[] { "DESCRIPTION", "REMARK", "COMMENT" } },
                { "VALUE_STATUS", new[] { "STATUS_GROUP", "STATUS_TYPE" } },
                { "SOURCE", new[] { "SOURCE", "SOURCE_NAME" } },
                { "RESOURCE", new[] { "RESOURCE", "URL", "LINK" } }
            };

            foreach (var csvColumn in csvColumns)
            {
                // Try direct match first
                var directMatch = properties.FirstOrDefault(p =>
                    p.Name.Equals(csvColumn, StringComparison.OrdinalIgnoreCase));
                if (directMatch != null)
                {
                    mapping[csvColumn] = directMatch.Name;
                    continue;
                }

                // Try common mappings
                var mapped = false;
                foreach (var commonMapping in commonMappings)
                {
                    if (csvColumn.Equals(commonMapping.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        var property = properties.FirstOrDefault(p =>
                            commonMapping.Value.Any(v => p.Name.Equals(v, StringComparison.OrdinalIgnoreCase)));
                        if (property != null)
                        {
                            mapping[csvColumn] = property.Name;
                            mapped = true;
                            break;
                        }
                    }
                }

                if (!mapped)
                {
                    // Try fuzzy match (remove underscores, case-insensitive)
                    var csvNormalized = csvColumn.Replace("_", "").Replace(" ", "");
                    var property = properties.FirstOrDefault(p =>
                    {
                        var propNormalized = p.Name.Replace("_", "").Replace(" ", "");
                        return propNormalized.Equals(csvNormalized, StringComparison.OrdinalIgnoreCase);
                    });

                    if (property != null)
                    {
                        mapping[csvColumn] = property.Name;
                    }
                }
            }

            return mapping;
        }

        /// <summary>
        /// Creates an entity instance from CSV row data
        /// </summary>
        private object CreateEntityFromCsvRow(
            List<string> csvValues,
            Dictionary<string, int> csvColumnIndices,
            Dictionary<string, string> columnMapping,
            PPDMTableMetadata tableMetadata)
        {
            var entity = Activator.CreateInstance(_entityType);
            if (entity == null)
                throw new InvalidOperationException($"Failed to create instance of {_entityType.Name}");

            var properties = _entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var kvp in columnMapping)
            {
                var csvColumnName = kvp.Key;
                var entityPropertyName = kvp.Value;

                if (!csvColumnIndices.ContainsKey(csvColumnName))
                    continue;

                var colIndex = csvColumnIndices[csvColumnName];
                if (colIndex >= csvValues.Count)
                    continue;

                var csvValue = csvValues[colIndex].Trim();
                if (string.IsNullOrWhiteSpace(csvValue))
                    continue;

                var property = properties.FirstOrDefault(p =>
                    p.Name.Equals(entityPropertyName, StringComparison.OrdinalIgnoreCase));

                if (property != null && property.CanWrite)
                {
                    try
                    {
                        var convertedValue = ConvertCsvValueToPropertyType(csvValue, property.PropertyType);
                        property.SetValue(entity, convertedValue);
                    }
                    catch (Exception ex)
                    {
                        // Log but continue - individual field errors are handled at row level
                        System.Diagnostics.Debug.WriteLine($"Error setting property {property.Name}: {ex.Message}");
                    }
                }
            }

            return entity;
        }

        /// <summary>
        /// Converts CSV string value to appropriate property type
        /// </summary>
        private object ConvertCsvValueToPropertyType(string value, Type targetType)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
                if (underlyingType.IsValueType && Nullable.GetUnderlyingType(targetType) == null)
                    return Activator.CreateInstance(underlyingType);
                return null;
            }

            var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (underlying == typeof(string))
                return value;
            if (underlying == typeof(int))
                return int.TryParse(value, out var i) ? i : (object)0;
            if (underlying == typeof(long))
                return long.TryParse(value, out var l) ? l : (object)0L;
            if (underlying == typeof(decimal))
                return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : (object)0m;
            if (underlying == typeof(double))
                return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var dbl) ? dbl : (object)0.0;
            if (underlying == typeof(DateTime))
            {
                if (DateTime.TryParse(value, out var dt))
                    return dt;
                // Try Excel date serial number
                if (double.TryParse(value, out var excelDate))
                    return DateTime.FromOADate(excelDate);
                return DateTime.MinValue;
            }
            if (underlying == typeof(bool))
            {
                return value.Equals("Y", StringComparison.OrdinalIgnoreCase) ||
                       value.Equals("Yes", StringComparison.OrdinalIgnoreCase) ||
                       value.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                       value.Equals("True", StringComparison.OrdinalIgnoreCase);
            }

            return Convert.ChangeType(value, underlying, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Applies default values to entity from PPDM39DefaultsRepository
        /// </summary>
        private void ApplyDefaultsToEntity(IPPDMEntity entity)
        {
            if (entity == null)
                return;

            var entityType = entity.GetType();
            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Apply common defaults
            foreach (var prop in properties)
            {
                if (!prop.CanWrite)
                    continue;

                var currentValue = prop.GetValue(entity);
                
                // Skip if value is already set
                if (currentValue != null && !string.IsNullOrWhiteSpace(currentValue.ToString()))
                    continue;

                // Apply defaults based on property name
                object defaultValue = null;

                if (prop.Name.Equals("ACTIVE_IND", StringComparison.OrdinalIgnoreCase))
                {
                    defaultValue = _defaults.GetActiveIndicatorYes();
                }
                else if (prop.Name.Equals("ROW_QUALITY", StringComparison.OrdinalIgnoreCase))
                {
                    defaultValue = _defaults.GetDefaultRowQuality();
                }
                else if (prop.Name.Equals("PREFERRED_IND", StringComparison.OrdinalIgnoreCase))
                {
                    defaultValue = _defaults.GetDefaultPreferredIndicator();
                }
                else if (prop.Name.Equals("CERTIFIED_IND", StringComparison.OrdinalIgnoreCase))
                {
                    defaultValue = _defaults.GetDefaultCertifiedIndicator();
                }
                // Add more default mappings as needed

                if (defaultValue != null)
                {
                    try
                    {
                        var convertedValue = Convert.ChangeType(defaultValue, prop.PropertyType);
                        prop.SetValue(entity, convertedValue);
                    }
                    catch
                    {
                        // Ignore conversion errors
                    }
                }
            }
        }

        /// <summary>
        /// Parses a CSV line handling quoted values
        /// </summary>
        private List<string> ParseCsvLine(string line)
        {
            var values = new List<string>();
            var currentValue = new StringBuilder();
            var inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                var currentChar = line[i];

                if (currentChar == '"')
                {
                    // Check if it's an escaped quote (two quotes in a row)
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        currentValue.Append('"');
                        i++; // Skip next quote
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (currentChar == ',' && !inQuotes)
                {
                    values.Add(currentValue.ToString().Trim('"'));
                    currentValue.Clear();
                }
                else
                {
                    currentValue.Append(currentChar);
                }
            }

            // Add last value
            values.Add(currentValue.ToString().Trim('"'));

            return values;
        }

        /// <summary>
        /// Checks if a CSV row is empty
        /// </summary>
        private bool IsEmptyCsvRow(string row)
        {
            if (string.IsNullOrWhiteSpace(row))
                return true;

            // Remove all commas and whitespace, check if anything remains
            var cleaned = row.Replace(",", "").Trim();
            return string.IsNullOrWhiteSpace(cleaned);
        }

        /// <summary>
        /// Escapes a value for CSV format
        /// </summary>
        private string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            // If value contains comma, quote, or newline, wrap in quotes and escape quotes
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }

            return value;
        }

        public async Task InsertAsync(NodalAnalysisResult result, object value)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    /// <summary>
    /// Result of file import operation
    /// </summary>
    public class FileImportResult
    {
        /// <summary>
        /// Path to the imported file
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Total number of rows processed
        /// </summary>
        public int TotalRows { get; set; }

        /// <summary>
        /// Number of successfully imported rows
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// Number of rows with errors
        /// </summary>
        public int ErrorCount { get; set; }

        /// <summary>
        /// List of import errors
        /// </summary>
        public List<FileImportError> Errors { get; set; } = new List<FileImportError>();

        /// <summary>
        /// Whether the import was successful (no errors)
        /// </summary>
        public bool IsSuccess => ErrorCount == 0;
    }

    /// <summary>
    /// Represents an error during file import
    /// </summary>
    public class FileImportError
    {
        /// <summary>
        /// Row number where error occurred (1-based)
        /// </summary>
        public int RowNumber { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Column name where error occurred (if applicable)
        /// </summary>
        public string ColumnName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a foreign key validation error
    /// </summary>
    public class ForeignKeyValidationError
    {
        /// <summary>
        /// Row number in the import file (1-based)
        /// </summary>
        public int RowNumber { get; set; }

        /// <summary>
        /// Foreign key column name in the current table
        /// </summary>
        public string ForeignKeyColumn { get; set; } = string.Empty;

        /// <summary>
        /// Referenced table name
        /// </summary>
        public string ReferencedTable { get; set; } = string.Empty;

        /// <summary>
        /// Referenced primary key column name
        /// </summary>
        public string ReferencedPrimaryKeyColumn { get; set; } = string.Empty;

        /// <summary>
        /// The foreign key value that was being validated
        /// </summary>
        public string ForeignKeyValue { get; set; } = string.Empty;

        /// <summary>
        /// Error message describing the validation failure
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// Factory for creating repositories for any PPDM entity
    /// </summary>
    public static class PPDMRepositoryFactory
    {
        /// <summary>
        /// Creates a repository for the specified entity type
        /// </summary>
        public static PPDMGenericRepository Create(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            Type entityType,
            string connectionName = "PPDM39",
            string tableName = null)
        {
            return new PPDMGenericRepository(editor, commonColumnHandler, defaults, metadata, entityType, connectionName, tableName);
        }

        /// <summary>
        /// Creates a repository for the specified generic entity type
        /// </summary>
        public static PPDMGenericRepository Create<T>(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            string tableName = null) where T : class, IPPDMEntity
        {
            return new PPDMGenericRepository(editor, commonColumnHandler, defaults, metadata, typeof(T), connectionName, tableName);
        }
    }
}
