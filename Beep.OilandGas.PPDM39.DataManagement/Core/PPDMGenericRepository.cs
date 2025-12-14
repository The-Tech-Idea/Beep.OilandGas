using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Globalization;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.UOW;
using TheTechIdea.Beep.Report;

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
        public PPDMGenericRepository(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            Type entityType,
            string connectionName = "PPDM39",
            string tableName = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _entityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _tableName = tableName ?? entityType.Name;

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

            // Get UnitOfWork for this entity type
            var uow = GetOrCreateUnitOfWork(entityType, tableName);
            uow.EntityName = tableName;

            // Try to use Get method with filters
            try
            {
                var result = await uow.Get(filters);
                return ConvertToTypedList(result, entityType);
            }
            catch
            {
                // Fallback: Build SQL from AppFilter (for SQL-based data sources)
                var sql = BuildSqlFromFilters(tableName, filters);
                var queryResult = await uow.GetQuery(sql);
                return ConvertToTypedList(queryResult, entityType);
            }
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
        /// </summary>
        protected virtual string BuildSqlFromFilters(string tableName, List<AppFilter> filters)
        {
            var sql = $"SELECT * FROM {tableName}";
            
            if (filters != null && filters.Count > 0)
            {
                var whereClauses = new List<string>();
                foreach (var filter in filters)
                {
                    if (string.IsNullOrWhiteSpace(filter.FieldName) || string.IsNullOrWhiteSpace(filter.Operator))
                        continue;

                    var value = filter.FilterValue ?? string.Empty;
                    var operatorStr = filter.Operator.ToUpper();

                    string clause;
                    switch (operatorStr)
                    {
                        case "=":
                        case "!=":
                        case "<>":
                        case ">":
                        case ">=":
                        case "<":
                        case "<=":
                            clause = $"{filter.FieldName} {operatorStr} '{value}'";
                            break;
                        case "LIKE":
                            clause = $"{filter.FieldName} LIKE '%{value}%'";
                            break;
                        case "IN":
                            clause = $"{filter.FieldName} IN ({value})";
                            break;
                        default:
                            clause = $"{filter.FieldName} {operatorStr} '{value}'";
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
                    filters.Add(new AppFilter
                    {
                        FieldName = fk.ForeignKeyColumn,
                        FilterValue = parentKeys[fk.ForeignKeyColumn]?.ToString() ?? string.Empty,
                        Operator = "="
                    });
                }
                // If not found, try matching by referenced primary key column name
                else if (parentPkColumns.Count == 1 && parentKeys.ContainsKey(parentPkColumns[0]))
                {
                    // Single column primary key - use the value
                    filters.Add(new AppFilter
                    {
                        FieldName = fk.ForeignKeyColumn,
                        FilterValue = parentKeys[parentPkColumns[0]]?.ToString() ?? string.Empty,
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
                        filters.Add(new AppFilter
                        {
                            FieldName = fk.ForeignKeyColumn,
                            FilterValue = parentKeys[parentPkColumns[fkIndex]]?.ToString() ?? string.Empty,
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

            // Use base batch insert method
            return await InsertBatchAsync(entityList, userId);
        }

        /// <summary>
        /// Gets an entity by its primary key using AppFilter
        /// Uses metadata to determine primary key column
        /// </summary>
        public virtual async Task<object> GetByIdAsync(object id)
        {
            var metadata = await GetTableMetadataAsync(_tableName);
            var primaryKeyName = metadata.PrimaryKeyColumn;
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = primaryKeyName, FilterValue = id?.ToString() ?? string.Empty, Operator = "=" }
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
            return await GetEntitiesWithFiltersAsync(_entityType, _tableName, filters ?? new List<AppFilter>());
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

            // Validate entity type
            if (!_entityType.IsAssignableFrom(entity.GetType()))
            {
                throw new ArgumentException($"Entity type {entity.GetType().Name} does not match repository entity type {_entityType.Name}", nameof(entity));
            }

            // Prepare entity for insertion
            _commonColumnHandler.PrepareForInsert(entity as IPPDMEntity, userId);

            // Insert into database - set EntityName from metadata
            var uow = GetOrCreateUnitOfWork(_entityType, _tableName);
            uow.EntityName = _tableName;
            var result = await uow.InsertAsync(entity as Entity);
            if (result != null && !string.IsNullOrEmpty(result.Message))
            {
                throw new Exception($"Insert failed: {result.Message}");
            }

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
            var uow = GetOrCreateUnitOfWork(_entityType, _tableName);
            uow.EntityName = _tableName;
            
            foreach (var entity in entityList)
            {
                if (!_entityType.IsAssignableFrom(entity.GetType()))
                {
                    throw new ArgumentException($"Entity type {entity.GetType().Name} does not match repository entity type {_entityType.Name}", nameof(entities));
                }

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
        /// Updates an existing entity (automatically handles common columns)
        /// </summary>
        public virtual async Task<object> UpdateAsync(object entity, string userId)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            // Validate entity type
            if (!_entityType.IsAssignableFrom(entity.GetType()))
            {
                throw new ArgumentException($"Entity type {entity.GetType().Name} does not match repository entity type {_entityType.Name}", nameof(entity));
            }

            // Prepare entity for update
            _commonColumnHandler.PrepareForUpdate(entity as IPPDMEntity, userId);

            // Update in database - set EntityName from metadata
            var uow = GetOrCreateUnitOfWork(_entityType, _tableName);
            uow.EntityName = _tableName;
            var result = await uow.UpdateAsync(entity as Entity);
            if (result != null && !string.IsNullOrEmpty(result.Message))
            {
                throw new Exception($"Update failed: {result.Message}");
            }

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
            var uow = GetOrCreateUnitOfWork(_entityType, _tableName);
            uow.EntityName = _tableName;
            
            foreach (var entity in entityList)
            {
                if (!_entityType.IsAssignableFrom(entity.GetType()))
                {
                    throw new ArgumentException($"Entity type {entity.GetType().Name} does not match repository entity type {_entityType.Name}", nameof(entities));
                }

                _commonColumnHandler.PrepareForUpdate(entity as IPPDMEntity, userId);
                var result = await uow.UpdateAsync(entity as Entity);
                if (result != null && !string.IsNullOrEmpty(result.Message))
                {
                    throw new Exception($"Batch update failed: {result.Message}");
                }
            }

            return entityList;
        }

        /// <summary>
        /// Soft deletes an entity (sets ACTIVE_IND = 'N') using defaults
        /// </summary>
        public virtual async Task<bool> SoftDeleteAsync(object id, string userId)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
                return false;

            _commonColumnHandler.SoftDelete(entity as IPPDMEntity, userId);
            await UpdateAsync(entity, userId);

            return true;
        }

        /// <summary>
        /// Hard deletes an entity from the database
        /// </summary>
        public virtual async Task<bool> DeleteAsync(object id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
                return false;

            var uow = GetOrCreateUnitOfWork(_entityType, _tableName);
            uow.EntityName = _tableName;
            var result = await uow.DeleteAsync(entity as Entity);
            return result == null || string.IsNullOrEmpty(result.Message);
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

        #region File Import/Export with Mapping and Defaults

        /// <summary>
        /// Imports data from a CSV file with automatic mapping and default value application
        /// </summary>
        /// <param name="csvFilePath">Path to the CSV file</param>
        /// <param name="userId">User ID for audit columns</param>
        /// <param name="columnMapping">Optional column mapping (CSV column name -> Entity property name). If null, auto-mapping is used.</param>
        /// <param name="skipHeaderRow">Whether to skip the first row (header row)</param>
        /// <param name="validateForeignKeys">Whether to validate foreign keys before import</param>
        /// <returns>Import result with success count and errors</returns>
        public virtual async Task<FileImportResult> ImportFromCsvAsync(
            string csvFilePath,
            string userId,
            Dictionary<string, string> columnMapping = null,
            bool skipHeaderRow = true,
            bool validateForeignKeys = true)
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

            try
            {
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

                // Build column mapping (use provided mapping or auto-map)
                var mapping = columnMapping ?? BuildAutoColumnMapping(csvColumnIndices.Keys);

                // Get table metadata
                var tableMetadata = await GetTableMetadataAsync(_tableName);

                // Prepare rows for validation
                var rowsForValidation = new List<Dictionary<string, string>>();
                var startRow = skipHeaderRow ? 1 : 0;
                result.TotalRows = csvLines.Length - startRow;

                // First pass: Parse and validate
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
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add(new FileImportError
                        {
                            RowNumber = rowIndex + 1,
                            Message = $"Error parsing row: {ex.Message}"
                        });
                        result.ErrorCount++;
                    }
                }

                // Validate foreign keys if requested
                if (validateForeignKeys && rowsForValidation.Any())
                {
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
                }

                // Second pass: Import valid rows
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
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add(new FileImportError
                        {
                            RowNumber = rowNumber,
                            Message = $"Error importing row: {ex.Message}"
                        });
                        result.ErrorCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(new FileImportError
                {
                    RowNumber = 0,
                    Message = $"Fatal error during import: {ex.Message}"
                });
                result.ErrorCount++;
            }

            return result;
        }

        /// <summary>
        /// Exports data to CSV file
        /// </summary>
        /// <param name="csvFilePath">Path where CSV file will be created</param>
        /// <param name="filters">Optional filters to apply to exported data</param>
        /// <param name="includeHeaders">Whether to include header row</param>
        /// <returns>Number of records exported</returns>
        public virtual async Task<int> ExportToCsvAsync(
            string csvFilePath,
            List<AppFilter> filters = null,
            bool includeHeaders = true)
        {
            if (string.IsNullOrWhiteSpace(csvFilePath))
                throw new ArgumentException("CSV file path cannot be null or empty", nameof(csvFilePath));

            // Get entities to export
            var entities = await GetAsync(filters ?? new List<AppFilter>());
            var entityList = entities.ToList();

            if (!entityList.Any())
                return 0;

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
            foreach (var entity in entityList)
            {
                var values = properties.Select(p =>
                {
                    var value = p.GetValue(entity);
                    return EscapeCsvValue(value?.ToString() ?? string.Empty);
                }).ToList();

                csvLines.Add(string.Join(",", values));
            }

            // Write to file
            await File.WriteAllLinesAsync(csvFilePath, csvLines, Encoding.UTF8);

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
