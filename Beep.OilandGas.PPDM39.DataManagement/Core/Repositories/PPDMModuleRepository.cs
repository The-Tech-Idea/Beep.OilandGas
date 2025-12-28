using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Core.Repositories
{
    /// <summary>
    /// Module-based repository for PPDM entities
    /// Handles all tables in a module (e.g., Stratigraphy, Well, Production)
    /// Uses metadata to understand table relationships automatically
    /// </summary>
    public class PPDMModuleRepository
    {
        protected readonly IUnitofWork _unitOfWork;
        protected readonly ICommonColumnHandler _commonColumnHandler;
        protected readonly IPPDM39DefaultsRepository _defaults;
        protected readonly IPPDMMetadataRepository _metadata;
        protected readonly string _module;

        public PPDMModuleRepository(
            IUnitofWork unitOfWork,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string module)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _module = module ?? throw new ArgumentNullException(nameof(module));
        }

        /// <summary>
        /// Gets an entity by ID from any table in this module
        /// </summary>
        public async Task<T> GetByIdAsync<T>(string tableName, object id) where T : class, IPPDMEntity
        {
            var tableMeta = await _metadata.GetTableMetadataAsync(tableName);
            if (tableMeta == null)
                throw new ArgumentException($"Table {tableName} not found in module {_module}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = tableMeta.PrimaryKeyColumn, FilterValue = id?.ToString() ?? string.Empty, Operator = "=" }
            };

            var entities = await GetEntitiesWithFiltersAsync<T>(tableName, filters);
            return entities?.FirstOrDefault();
        }

        /// <summary>
        /// Gets all active entities from a table in this module
        /// </summary>
        public async Task<IEnumerable<T>> GetActiveAsync<T>(string tableName) where T : class, IPPDMEntity
        {
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = _defaults.GetActiveIndicatorYes(), Operator = "=" }
            };

            return await GetEntitiesWithFiltersAsync<T>(tableName, filters);
        }

        /// <summary>
        /// Gets entities with filters from any table in this module
        /// </summary>
        public async Task<IEnumerable<T>> GetAsync<T>(string tableName, List<AppFilter> filters) where T : class, IPPDMEntity
        {
            return await GetEntitiesWithFiltersAsync<T>(tableName, filters);
        }

        /// <summary>
        /// Gets related entities using foreign key relationships from metadata
        /// </summary>
        public async Task<IEnumerable<T>> GetRelatedAsync<T>(string tableName, string foreignKeyColumn, object foreignKeyValue) where T : class, IPPDMEntity
        {
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = foreignKeyColumn, FilterValue = foreignKeyValue?.ToString() ?? string.Empty, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = _defaults.GetActiveIndicatorYes(), Operator = "=" }
            };

            return await GetEntitiesWithFiltersAsync<T>(tableName, filters);
        }

        /// <summary>
        /// Gets children of a parent entity using metadata to find foreign key relationships
        /// Handles both direct FK relationships and junction table relationships
        /// </summary>
        /// <typeparam name="T">Child entity type</typeparam>
        /// <param name="childTableName">Table name of the child entities</param>
        /// <param name="parentTableName">Table name of the parent entity</param>
        /// <param name="parentId">Primary key value of the parent entity</param>
        /// <param name="junctionTableName">Optional: junction table name for many-to-many relationships (e.g., STRAT_HIERARCHY)</param>
        /// <param name="parentFkColumnInJunction">Optional: foreign key column name in junction table pointing to parent (e.g., PARENT_STRAT_UNIT_ID)</param>
        /// <param name="childFkColumnInJunction">Optional: foreign key column name in junction table pointing to child (e.g., CHILD_STRAT_UNIT_ID)</param>
        /// <returns>List of child entities</returns>
        public async Task<IEnumerable<T>> GetChildrenAsync<T>(
            string childTableName, 
            string parentTableName, 
            object parentId,
            string junctionTableName = null,
            string parentFkColumnInJunction = null,
            string childFkColumnInJunction = null) where T : class, IPPDMEntity
        {
            // If junction table is specified, use it for many-to-many relationships
            if (!string.IsNullOrWhiteSpace(junctionTableName))
            {
                // Query junction table to find child IDs
                var junctionFilters = new List<AppFilter>
                {
                    new AppFilter 
                    { 
                        FieldName = parentFkColumnInJunction ?? $"PARENT_{parentTableName}_ID", 
                        FilterValue = parentId?.ToString() ?? string.Empty, 
                        Operator = "=" 
                    },
                    new AppFilter { FieldName = "ACTIVE_IND", FilterValue = _defaults.GetActiveIndicatorYes(), Operator = "=" }
                };

                var junctionEntities = await GetEntitiesWithFiltersAsync<IPPDMEntity>(junctionTableName, junctionFilters);
                
                // Extract child IDs from junction entities
                var childFkColumn = childFkColumnInJunction ?? $"CHILD_{childTableName}_ID";
                var childIds = new List<object>();
                
                foreach (var junctionEntity in junctionEntities)
                {
                    var fkProperty = junctionEntity.GetType().GetProperty(childFkColumn);
                    if (fkProperty != null)
                    {
                        var childId = fkProperty.GetValue(junctionEntity);
                        if (childId != null)
                        {
                            childIds.Add(childId);
                        }
                    }
                }

                // Get child entities
                var results = new List<T>();
                
                foreach (var childId in childIds.Distinct())
                {
                    var child = await GetByIdAsync<T>(childTableName, childId);
                    if (child != null)
                    {
                        results.Add(child);
                    }
                }

                return results;
            }

            // Direct FK relationship - find FK in child table
            var childTableMeta = await _metadata.GetTableMetadataAsync(childTableName);
            if (childTableMeta == null)
                throw new ArgumentException($"Table {childTableName} not found in metadata");

            // Find foreign key that references the parent table
            var foreignKey = childTableMeta.ForeignKeys
                .FirstOrDefault(fk => fk.ReferencedTable.Equals(parentTableName, StringComparison.OrdinalIgnoreCase));

            if (foreignKey == null)
                throw new InvalidOperationException($"No foreign key relationship found from {childTableName} to {parentTableName}. Consider using junctionTableName parameter for many-to-many relationships.");

            // Query children using the foreign key
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = foreignKey.ForeignKeyColumn, FilterValue = parentId?.ToString() ?? string.Empty, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = _defaults.GetActiveIndicatorYes(), Operator = "=" }
            };

            return await GetEntitiesWithFiltersAsync<T>(childTableName, filters);
        }

        /// <summary>
        /// Gets parents of a child entity using metadata to find foreign key relationships
        /// Handles both direct FK relationships and junction table relationships
        /// </summary>
        /// <typeparam name="T">Parent entity type</typeparam>
        /// <param name="parentTableName">Table name of the parent entities</param>
        /// <param name="childTableName">Table name of the child entity</param>
        /// <param name="childId">Primary key value of the child entity</param>
        /// <param name="junctionTableName">Optional: junction table name for many-to-many relationships (e.g., STRAT_HIERARCHY)</param>
        /// <param name="childFkColumnInJunction">Optional: foreign key column name in junction table pointing to child (e.g., CHILD_STRAT_UNIT_ID)</param>
        /// <param name="parentFkColumnInJunction">Optional: foreign key column name in junction table pointing to parent (e.g., PARENT_STRAT_UNIT_ID)</param>
        /// <returns>List of parent entities</returns>
        public async Task<IEnumerable<T>> GetParentsAsync<T>(
            string parentTableName, 
            string childTableName, 
            object childId,
            string junctionTableName = null,
            string childFkColumnInJunction = null,
            string parentFkColumnInJunction = null) where T : class, IPPDMEntity
        {
            // If junction table is specified, use it for many-to-many relationships
            if (!string.IsNullOrWhiteSpace(junctionTableName))
            {
                // Query junction table to find parent IDs
                var junctionFilters = new List<AppFilter>
                {
                    new AppFilter 
                    { 
                        FieldName = childFkColumnInJunction ?? $"CHILD_{childTableName}_ID", 
                        FilterValue = childId?.ToString() ?? string.Empty, 
                        Operator = "=" 
                    },
                    new AppFilter { FieldName = "ACTIVE_IND", FilterValue = _defaults.GetActiveIndicatorYes(), Operator = "=" }
                };

                var junctionEntities = await GetEntitiesWithFiltersAsync<IPPDMEntity>(junctionTableName, junctionFilters);
                
                // Extract parent IDs from junction entities
                var parentFkColumn = parentFkColumnInJunction ?? $"PARENT_{parentTableName}_ID";
                var parentIds = new List<object>();
                
                foreach (var junctionEntity in junctionEntities)
                {
                    var parentFkProp = junctionEntity.GetType().GetProperty(parentFkColumn);
                    if (parentFkProp != null)
                    {
                        var parentId = parentFkProp.GetValue(junctionEntity);
                        if (parentId != null)
                        {
                            parentIds.Add(parentId);
                        }
                    }
                }

                // Get parent entities
                var results = new List<T>();
                
                foreach (var parentId in parentIds.Distinct())
                {
                    var parent = await GetByIdAsync<T>(parentTableName, parentId);
                    if (parent != null)
                    {
                        results.Add(parent);
                    }
                }

                return results;
            }

            // Direct FK relationship - find FK in child table
            var childTableMeta = await _metadata.GetTableMetadataAsync(childTableName);
            if (childTableMeta == null)
                throw new ArgumentException($"Table {childTableName} not found in metadata");

            var foreignKey = childTableMeta.ForeignKeys
                .FirstOrDefault(fk => fk.ReferencedTable.Equals(parentTableName, StringComparison.OrdinalIgnoreCase));

            if (foreignKey == null)
                throw new InvalidOperationException($"No foreign key relationship found from {childTableName} to {parentTableName}. Consider using junctionTableName parameter for many-to-many relationships.");

            // Get the child entity to extract the foreign key value
            var childEntity = await GetByIdAsync<IPPDMEntity>(childTableName, childId);
            if (childEntity == null)
                return Enumerable.Empty<T>();

            // Get the foreign key value from the child entity using reflection
            var childFkProp = childEntity.GetType().GetProperty(foreignKey.ForeignKeyColumn);
            if (childFkProp == null)
                throw new InvalidOperationException($"Property {foreignKey.ForeignKeyColumn} not found on {childTableName} entity");

            var fkValue = childFkProp.GetValue(childEntity);
            if (fkValue == null)
                return Enumerable.Empty<T>();

            // Query parent using the foreign key value
            var parentMeta = await _metadata.GetTableMetadataAsync(parentTableName);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = parentMeta.PrimaryKeyColumn, FilterValue = fkValue.ToString(), Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = _defaults.GetActiveIndicatorYes(), Operator = "=" }
            };

            return await GetEntitiesWithFiltersAsync<T>(parentTableName, filters);
        }

        /// <summary>
        /// Gets all related entities through a specific foreign key relationship
        /// Uses metadata to automatically determine the relationship
        /// </summary>
        /// <typeparam name="T">Related entity type</typeparam>
        /// <param name="sourceTableName">Table name of the source entity</param>
        /// <param name="targetTableName">Table name of the target/related entities</param>
        /// <param name="sourceId">Primary key value of the source entity</param>
        /// <param name="foreignKeyColumn">Optional: specific foreign key column name. If null, uses metadata to find it</param>
        /// <returns>List of related entities</returns>
        public async Task<IEnumerable<T>> GetRelatedByForeignKeyAsync<T>(
            string sourceTableName, 
            string targetTableName, 
            object sourceId, 
            string foreignKeyColumn = null) where T : class, IPPDMEntity
        {
            // If foreign key column not specified, find it from metadata
            if (string.IsNullOrWhiteSpace(foreignKeyColumn))
            {
                var sourceTableMeta = await _metadata.GetTableMetadataAsync(sourceTableName);
                if (sourceTableMeta == null)
                    throw new ArgumentException($"Table {sourceTableName} not found in metadata");

                var foreignKey = sourceTableMeta.ForeignKeys
                    .FirstOrDefault(fk => fk.ReferencedTable.Equals(targetTableName, StringComparison.OrdinalIgnoreCase));

                if (foreignKey == null)
                {
                    // Try reverse: maybe target table has FK to source
                    var targetTableMeta = await _metadata.GetTableMetadataAsync(targetTableName);
                    if (targetTableMeta != null)
                    {
                        foreignKey = targetTableMeta.ForeignKeys
                            .FirstOrDefault(fk => fk.ReferencedTable.Equals(sourceTableName, StringComparison.OrdinalIgnoreCase));
                        
                        if (foreignKey != null)
                        {
                            // Target table has FK to source - get children
                            return await GetChildrenAsync<T>(targetTableName, sourceTableName, sourceId);
                        }
                    }
                    throw new InvalidOperationException($"No foreign key relationship found between {sourceTableName} and {targetTableName}");
                }

                foreignKeyColumn = foreignKey.ForeignKeyColumn;
            }

            // Query using the foreign key
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = foreignKeyColumn, FilterValue = sourceId?.ToString() ?? string.Empty, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = _defaults.GetActiveIndicatorYes(), Operator = "=" }
            };

            return await GetEntitiesWithFiltersAsync<T>(targetTableName, filters);
        }

        /// <summary>
        /// Gets all tables that reference a given table (reverse foreign key lookup)
        /// Uses metadata to find all tables with foreign keys pointing to the specified table
        /// </summary>
        /// <param name="referencedTableName">Table name that is referenced</param>
        /// <returns>List of table metadata that reference the specified table</returns>
        public async Task<IEnumerable<PPDMTableMetadata>> GetReferencingTablesAsync(string referencedTableName)
        {
            return await _metadata.GetReferencingTablesAsync(referencedTableName);
        }

        /// <summary>
        /// Gets all foreign key relationships for a table
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <returns>List of foreign key relationships</returns>
        public async Task<IEnumerable<PPDMForeignKey>> GetForeignKeysAsync(string tableName)
        {
            return await _metadata.GetForeignKeysAsync(tableName);
        }

        /// <summary>
        /// Inserts an entity into any table in this module
        /// </summary>
        public async Task<T> InsertAsync<T>(string tableName, T entity, string userId) where T : class, IPPDMEntity
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            // Prepare entity for insertion (common columns)
            _commonColumnHandler.PrepareForInsert(entity, userId);

            // Insert into database - set EntityName from metadata
            _unitOfWork.EntityName = tableName;
            var insertResult = await _unitOfWork.InsertAsync(entity as Entity);
            if (insertResult != null && !string.IsNullOrEmpty(insertResult.Message))
            {
                throw new Exception($"Insert failed: {insertResult.Message}");
            }

            return entity;
        }

        /// <summary>
        /// Updates an entity in any table in this module
        /// </summary>
        public async Task<T> UpdateAsync<T>(string tableName, T entity, string userId) where T : class, IPPDMEntity
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            // Prepare entity for update (common columns)
            _commonColumnHandler.PrepareForUpdate(entity, userId);

            // Get primary key from metadata
            var tableMeta = await _metadata.GetTableMetadataAsync(tableName);
            if (tableMeta == null)
                throw new ArgumentException($"Table {tableName} not found in module {_module}");

            // Update in database - set EntityName from metadata
            _unitOfWork.EntityName = tableName;
            var updateResult = await _unitOfWork.UpdateAsync(entity as Entity);
            if (updateResult != null && !string.IsNullOrEmpty(updateResult.Message))
            {
                throw new Exception($"Update failed: {updateResult.Message}");
            }

            return entity;
        }

        /// <summary>
        /// Soft deletes an entity from any table in this module
        /// </summary>
        public async Task<bool> SoftDeleteAsync<T>(string tableName, object id, string userId) where T : class, IPPDMEntity
        {
            var entity = await GetByIdAsync<T>(tableName, id);
            if (entity == null)
                return false;

            _commonColumnHandler.SoftDelete(entity, userId);
            await UpdateAsync(tableName, entity, userId);

            return true;
        }

        /// <summary>
        /// Gets all tables in this module
        /// </summary>
        public async Task<IEnumerable<PPDMTableMetadata>> GetModuleTablesAsync()
        {
            return await _metadata.GetTablesByModuleAsync(_module);
        }

        /// <summary>
        /// Helper method to get entities using AppFilter
        /// </summary>
        protected virtual async Task<IEnumerable<TResult>> GetEntitiesWithFiltersAsync<TResult>(string tableName, List<AppFilter> filters)
        {
            // Try to use GetEntityAsync if available (for IDataSource)
            try
            {
                var getEntityMethod = _unitOfWork.GetType().GetMethod("GetEntityAsync");
                if (getEntityMethod != null)
                {
                    var task = (Task<dynamic>)getEntityMethod.Invoke(_unitOfWork, new object[] { tableName, filters });
                    var result = await task;
                    return ConvertToTypedList<TResult>(result);
                }
            }
            catch
            {
                // Fall through to SQL-based approach
            }

            // Fallback: Build SQL from AppFilter
            var sql = BuildSqlFromFilters(tableName, filters);
            _unitOfWork.EntityName = tableName;
            var queryResult = await _unitOfWork.GetQuery(sql);
            return queryResult.Cast<TResult>();
        }

        /// <summary>
        /// Converts dynamic result to typed list
        /// </summary>
        protected virtual IEnumerable<TResult> ConvertToTypedList<TResult>(dynamic result)
        {
            if (result == null)
                return Enumerable.Empty<TResult>();

            var list = new List<TResult>();
            if (result is System.Collections.IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item is TResult typedItem)
                    {
                        list.Add(typedItem);
                    }
                    else if (typeof(TResult) == typeof(Dictionary<string, object>) && item is Dictionary<string, object> dictItem)
                    {
                        list.Add((TResult)(object)dictItem);
                    }
                    else if (item != null)
                    {
                        // Try explicit cast for Dictionary<string, object>
                        if (typeof(TResult) == typeof(Dictionary<string, object>))
                        {
                            var dict = item as Dictionary<string, object>;
                            if (dict != null)
                            {
                                list.Add((TResult)(object)dict);
                            }
                        }
                    }
                }
            }
            else if (result is TResult singleItem)
            {
                list.Add(singleItem);
            }
            else if (typeof(TResult) == typeof(Dictionary<string, object>) && result is Dictionary<string, object> dictResult)
            {
                list.Add((TResult)(object)dictResult);
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
    }
}

