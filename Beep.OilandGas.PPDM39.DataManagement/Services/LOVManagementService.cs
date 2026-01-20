using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Common;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Services
{
    /// <summary>
    /// Type of reference table
    /// </summary>
    public enum ReferenceTableType
    {
        ListOfValue,
        RTable,
        RATable,
        Unknown
    }

    /// <summary>
    /// Service for managing List of Values (LOV) data and reference tables (R_*, RA_*)
    /// Provides CRUD operations and querying capabilities for LIST_OF_VALUE, R_*, and RA_* tables
    /// </summary>
    public class LOVManagementService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private PPDMGenericRepository _repository;

        public LOVManagementService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName;
            _repository = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(LIST_OF_VALUE), _connectionName, "LIST_OF_VALUE");
        }

        /// <summary>
        /// Gets repository for a specific connection (or default)
        /// </summary>
        private PPDMGenericRepository GetRepository(string? connectionName = null)
        {
            connectionName ??= _connectionName;
            if (connectionName == _connectionName && _repository != null)
            {
                return _repository;
            }
            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(LIST_OF_VALUE), connectionName, "LIST_OF_VALUE");
        }

        /// <summary>
        /// Gets repository for a specific entity type and table
        /// </summary>
        private PPDMGenericRepository GetRepository(Type entityType, string tableName, string? connectionName = null)
        {
            connectionName ??= _connectionName;
            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, tableName);
        }

        /// <summary>
        /// Detects the type of reference table based on table name
        /// </summary>
        public static ReferenceTableType GetTableType(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                return ReferenceTableType.Unknown;

            var upperName = tableName.ToUpperInvariant();
            if (upperName == "LIST_OF_VALUE")
                return ReferenceTableType.ListOfValue;
            if (upperName.StartsWith("RA_"))
                return ReferenceTableType.RATable;
            if (upperName.StartsWith("R_"))
                return ReferenceTableType.RTable;

            return ReferenceTableType.Unknown;
        }

        /// <summary>
        /// Resolves entity type for a given table name
        /// </summary>
        public async Task<Type?> GetEntityTypeAsync(string tableName)
        {
            var tableType = GetTableType(tableName);
            
            if (tableType == ReferenceTableType.ListOfValue)
            {
                return typeof(LIST_OF_VALUE);
            }

            // Try to get entity type from metadata
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            if (metadata != null && !string.IsNullOrEmpty(metadata.EntityTypeName))
            {
                // Try PPDM39.Models namespace first (for R_* and RA_* tables)
                var entityType = Type.GetType($"Beep.OilandGas.PPDM.Models.39.{metadata.EntityTypeName}") ??
                                Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}") ??
                                Type.GetType($"Beep.OilandGas.Models.Data.{metadata.EntityTypeName}");

                if (entityType != null)
                    return entityType;
            }

            // Fallback: try convention-based type resolution
            var typeName = tableName.Replace("_", "");
            var ppdmType = Type.GetType($"Beep.OilandGas.PPDM.Models.39.{typeName}") ??
                          Type.GetType($"Beep.OilandGas.PPDM39.Models.{typeName}");

            return ppdmType;
        }

        /// <summary>
        /// Detects key properties for a given entity type based on table type and conventions
        /// </summary>
        private List<string> DetectKeyProperties(Type entityType, ReferenceTableType tableType)
        {
            var keyProperties = new List<string>();
            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            if (tableType == ReferenceTableType.ListOfValue)
            {
                // LIST_OF_VALUE: VALUE_TYPE + VALUE_CODE, or LIST_OF_VALUE_ID
                if (properties.Any(p => p.Name == "VALUE_TYPE") && properties.Any(p => p.Name == "VALUE_CODE"))
                {
                    keyProperties.Add("VALUE_TYPE");
                    keyProperties.Add("VALUE_CODE");
                }
                else if (properties.Any(p => p.Name == "LIST_OF_VALUE_ID"))
                {
                    keyProperties.Add("LIST_OF_VALUE_ID");
                }
            }
            else if (tableType == ReferenceTableType.RATable)
            {
                // RA_* tables: typically use composite keys with ALIAS_ID
                // Common patterns: STATUS_TYPE + STATUS + ALIAS_ID, or TYPE + ALIAS_ID
                if (properties.Any(p => p.Name == "STATUS_TYPE") && properties.Any(p => p.Name == "STATUS"))
                {
                    keyProperties.Add("STATUS_TYPE");
                    keyProperties.Add("STATUS");
                    if (properties.Any(p => p.Name == "ALIAS_ID"))
                    {
                        keyProperties.Add("ALIAS_ID");
                    }
                }
                else if (properties.Any(p => p.Name == "ALIAS_ID"))
                {
                    // Find the main type field (usually ends with _TYPE or is a single identifier)
                    var typeProperty = properties.FirstOrDefault(p => 
                        p.Name.EndsWith("_TYPE", StringComparison.OrdinalIgnoreCase) ||
                        (p.Name != "ALIAS_ID" && p.PropertyType == typeof(string) && 
                         !p.Name.Contains("_ID") && !p.Name.Contains("DATE") && !p.Name.Contains("BY")));
                    
                    if (typeProperty != null)
                    {
                        keyProperties.Add(typeProperty.Name);
                    }
                    keyProperties.Add("ALIAS_ID");
                }
            }
            else if (tableType == ReferenceTableType.RTable)
            {
                // R_* tables: typically use composite keys without ALIAS_ID
                // Common patterns: STATUS_TYPE + STATUS, or TYPE + VALUE
                if (properties.Any(p => p.Name == "STATUS_TYPE") && properties.Any(p => p.Name == "STATUS"))
                {
                    keyProperties.Add("STATUS_TYPE");
                    keyProperties.Add("STATUS");
                }
                else
                {
                    // Try to find TYPE and VALUE/STATUS pattern
                    var typeProperty = properties.FirstOrDefault(p => p.Name.EndsWith("_TYPE", StringComparison.OrdinalIgnoreCase));
                    var valueProperty = properties.FirstOrDefault(p => 
                        p.Name == "STATUS" || p.Name == "VALUE" || 
                        (p.Name != "TYPE" && !p.Name.Contains("_ID") && !p.Name.Contains("DATE") && 
                         !p.Name.Contains("BY") && p.PropertyType == typeof(string)));

                    if (typeProperty != null)
                        keyProperties.Add(typeProperty.Name);
                    if (valueProperty != null)
                        keyProperties.Add(valueProperty.Name);
                }
            }

            // Fallback: if no keys detected, use all non-null string properties (excluding common PPDM columns)
            if (keyProperties.Count == 0)
            {
                var excludedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE",
                    "ROW_EFFECTIVE_DATE", "ROW_EXPIRY_DATE", "ROW_QUALITY", "PPDM_GUID", "REMARK",
                    "ACTIVE_IND", "SOURCE", "DESCRIPTION", "LONG_NAME", "SHORT_NAME", "ABBREVIATION"
                };

                keyProperties = properties
                    .Where(p => p.PropertyType == typeof(string) && 
                                !excludedNames.Contains(p.Name) &&
                                !p.Name.Contains("_ID") && 
                                !p.Name.Contains("DATE"))
                    .Select(p => p.Name)
                    .Take(3) // Limit to first 3 to avoid too many keys
                    .ToList();
            }

            return keyProperties;
        }

        /// <summary>
        /// Builds filters for existence check based on detected key properties
        /// </summary>
        private List<AppFilter> BuildKeyFilters(object entity, List<string> keyProperties)
        {
            var filters = new List<AppFilter>();
            var entityType = entity.GetType();

            foreach (var keyProp in keyProperties)
            {
                var prop = entityType.GetProperty(keyProp, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop != null)
                {
                    var value = prop.GetValue(entity);
                    if (value != null)
                    {
                        filters.Add(new AppFilter
                        {
                            FieldName = keyProp,
                            Operator = "=",
                            FilterValue = value.ToString() ?? string.Empty
                        });
                    }
                }
            }

            return filters;
        }

        /// <summary>
        /// Generic method to check if a reference value exists
        /// </summary>
        public async Task<bool> CheckReferenceValueExistsAsync<T>(
            T entity,
            string? connectionName = null) where T : class, IPPDMEntity
        {
            connectionName ??= _connectionName;
            var entityType = typeof(T);
            var tableName = entityType.Name;
            var tableType = GetTableType(tableName);
            
            var keyProperties = DetectKeyProperties(entityType, tableType);
            if (keyProperties.Count == 0)
            {
                return false; // Cannot check existence without keys
            }

            var repository = GetRepository(entityType, tableName, connectionName);
            var filters = BuildKeyFilters(entity, keyProperties);
            
            if (filters.Count == 0)
            {
                return false;
            }

            var existing = await repository.GetAsync(filters);
            return existing.Any();
        }

        /// <summary>
        /// Generic method to add or update a reference value
        /// </summary>
        public async Task<(T? Entity, bool WasInserted, bool WasSkipped)> AddOrUpdateReferenceValueAsync<T>(
            T entity,
            string userId,
            bool skipExisting = true,
            string? connectionName = null) where T : class, IPPDMEntity
        {
            connectionName ??= _connectionName;
            var entityType = typeof(T);
            var tableName = entityType.Name;

            // Check if exists when skipExisting is true
            if (skipExisting)
            {
                var exists = await CheckReferenceValueExistsAsync(entity, connectionName);
                if (exists)
                {
                    return (null, false, true);
                }
            }

            // Add the entity
            var repository = GetRepository(entityType, tableName, connectionName);
            if (entity is IPPDMEntity ppdmEntity)
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
            
            var result = await repository.InsertAsync(entity, userId);
            return (result as T, true, false);
        }

        /// <summary>
        /// Generic method to bulk add reference values
        /// </summary>
        public async Task<BulkReferenceResult> BulkAddReferenceValuesAsync<T>(
            IEnumerable<T> entities,
            string userId,
            bool skipExisting = true,
            string? connectionName = null) where T : class, IPPDMEntity
        {
            connectionName ??= _connectionName;
            var result = new BulkReferenceResult
            {
                TotalProcessed = 0,
                TotalInserted = 0,
                TotalSkipped = 0,
                Errors = new List<string>()
            };

            foreach (var entity in entities)
            {
                try
                {
                    result.TotalProcessed++;
                    var (inserted, wasInserted, wasSkipped) = await AddOrUpdateReferenceValueAsync(entity, userId, skipExisting, connectionName);
                    
                    if (wasSkipped)
                    {
                        result.TotalSkipped++;
                    }
                    else if (wasInserted && inserted != null)
                    {
                        result.TotalInserted++;
                    }
                }
                catch (Exception ex)
                {
                    var entityType = typeof(T);
                    var tableName = entityType.Name;
                    result.Errors.Add($"Error processing {tableName} entity: {ex.Message}");
                }
            }

            result.Success = result.Errors.Count == 0;
            return result;
        }

        /// <summary>
        /// Checks if a LOV exists by VALUE_TYPE and VALUE_CODE
        /// </summary>
        public async Task<bool> CheckLOVExistsAsync(string valueType, string valueCode, string? connectionName = null)
        {
            connectionName ??= _connectionName;
            var repository = GetRepository(connectionName);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "VALUE_TYPE", Operator = "=", FilterValue = valueType },
                new AppFilter { FieldName = "VALUE_CODE", Operator = "=", FilterValue = valueCode }
            };

            var entities = await repository.GetAsync(filters);
            return entities.Any();
        }

        /// <summary>
        /// Gets LOVs by value type
        /// </summary>
        public async Task<List<ListOfValue>> GetLOVByTypeAsync(string valueType, string? category = null, string connectionName = null)
        {
            connectionName ??= _connectionName;
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "VALUE_TYPE", Operator = "=", FilterValue = valueType },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (!string.IsNullOrEmpty(category))
            {
                filters.Add(new AppFilter { FieldName = "CATEGORY", Operator = "=", FilterValue = category });
            }

            var repository = GetRepository(connectionName);
            var entities = await repository.GetAsync(filters);
            return entities.Cast<LIST_OF_VALUE>()
                .OrderBy(lov => lov.SORT_ORDER ?? int.MaxValue)
                .ThenBy(lov => lov.VALUE_NAME)
                .Select(MapToDto)
                .ToList();
        }

        /// <summary>
        /// Gets all LOVs in a category
        /// </summary>
        public async Task<List<ListOfValue>> GetLOVByCategoryAsync(string category, string connectionName = null)
        {
            connectionName ??= _connectionName;
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "CATEGORY", Operator = "=", FilterValue = category },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var repository = GetRepository(connectionName);
            var entities = await repository.GetAsync(filters);
            return entities.Cast<LIST_OF_VALUE>()
                .OrderBy(lov => lov.VALUE_TYPE)
                .ThenBy(lov => lov.SORT_ORDER ?? int.MaxValue)
                .ThenBy(lov => lov.VALUE_NAME)
                .Select(MapToDto)
                .ToList();
        }

        /// <summary>
        /// Gets all LOVs for a module
        /// </summary>
        public async Task<List<ListOfValue>> GetLOVByModuleAsync(string module, string connectionName = null)
        {
            connectionName ??= _connectionName;
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "MODULE", Operator = "=", FilterValue = module },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var repository = GetRepository(connectionName);
            var entities = await repository.GetAsync(filters);
            return entities.Cast<LIST_OF_VALUE>()
                .OrderBy(lov => lov.VALUE_TYPE)
                .ThenBy(lov => lov.SORT_ORDER ?? int.MaxValue)
                .ThenBy(lov => lov.VALUE_NAME)
                .Select(MapToDto)
                .ToList();
        }

        /// <summary>
        /// Gets LOVs by source (PPDM, IHS, Custom, API, ISO, etc.)
        /// </summary>
        public async Task<List<ListOfValue>> GetLOVBySourceAsync(string source, string connectionName = null)
        {
            connectionName ??= _connectionName;
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "SOURCE", Operator = "=", FilterValue = source },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var repository = GetRepository(connectionName);
            var entities = await repository.GetAsync(filters);
            return entities.Cast<LIST_OF_VALUE>()
                .OrderBy(lov => lov.VALUE_TYPE)
                .ThenBy(lov => lov.SORT_ORDER ?? int.MaxValue)
                .ThenBy(lov => lov.VALUE_NAME)
                .Select(MapToDto)
                .ToList();
        }

        /// <summary>
        /// Gets hierarchical LOVs (parent-child relationships)
        /// </summary>
        public async Task<List<ListOfValue>> GetHierarchicalLOVAsync(string valueType, string connectionName = null)
        {
            connectionName ??= _connectionName;
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "VALUE_TYPE", Operator = "=", FilterValue = valueType },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var repository = GetRepository(connectionName);
            var entities = await repository.GetAsync(filters);
            var allLOVs = entities.Cast<LIST_OF_VALUE>().ToList();

            // Build hierarchy: parents first, then children
            var parents = allLOVs.Where(lov => string.IsNullOrEmpty(lov.PARENT_VALUE_ID))
                .OrderBy(lov => lov.SORT_ORDER ?? int.MaxValue)
                .ThenBy(lov => lov.VALUE_NAME)
                .Select(MapToDto)
                .ToList();

            foreach (var parent in parents)
            {
                parent.Children = allLOVs
                    .Where(lov => lov.PARENT_VALUE_ID == parent.ListOfValueId)
                    .OrderBy(lov => lov.SORT_ORDER ?? int.MaxValue)
                    .ThenBy(lov => lov.VALUE_NAME)
                    .Select(MapToDto)
                    .ToList();
            }

            return parents;
        }

        /// <summary>
        /// Searches LOVs by search term
        /// </summary>
        public async Task<List<ListOfValue>> SearchLOVsAsync(string searchTerm, LOVRequest? filters = null, string connectionName = null)
        {
            connectionName ??= _connectionName;
            var appFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            // Add search term filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                appFilters.Add(new AppFilter { FieldName = "VALUE_NAME", Operator = "LIKE", FilterValue = $"%{searchTerm}%" });
            }

            // Add additional filters
            if (filters != null)
            {
                if (!string.IsNullOrEmpty(filters.ValueType))
                {
                    appFilters.Add(new AppFilter { FieldName = "VALUE_TYPE", Operator = "=", FilterValue = filters.ValueType });
                }
                if (!string.IsNullOrEmpty(filters.Category))
                {
                    appFilters.Add(new AppFilter { FieldName = "CATEGORY", Operator = "=", FilterValue = filters.Category });
                }
                if (!string.IsNullOrEmpty(filters.Module))
                {
                    appFilters.Add(new AppFilter { FieldName = "MODULE", Operator = "=", FilterValue = filters.Module });
                }
                if (!string.IsNullOrEmpty(filters.Source))
                {
                    appFilters.Add(new AppFilter { FieldName = "SOURCE", Operator = "=", FilterValue = filters.Source });
                }
            }

            var repository = GetRepository(connectionName);
            var entities = await repository.GetAsync(appFilters);
            return entities.Cast<LIST_OF_VALUE>()
                .OrderBy(lov => lov.VALUE_TYPE)
                .ThenBy(lov => lov.SORT_ORDER ?? int.MaxValue)
                .ThenBy(lov => lov.VALUE_NAME)
                .Select(MapToDto)
                .ToList();
        }

        /// <summary>
        /// Gets all distinct VALUE_TYPEs
        /// </summary>
        public async Task<List<string>> GetValueTypesAsync(string connectionName = null)
        {
            connectionName ??= _connectionName;
            var repository = GetRepository(connectionName);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var entities = await repository.GetAsync(filters);
            return entities.Cast<LIST_OF_VALUE>()
                .Select(lov => lov.VALUE_TYPE)
                .Distinct()
                .OrderBy(vt => vt)
                .ToList();
        }

        /// <summary>
        /// Adds a new LOV
        /// </summary>
        public async Task<LIST_OF_VALUE> AddLOVAsync(LIST_OF_VALUE lov, string userId, string connectionName = null)
        {
            connectionName ??= _connectionName;
            if (string.IsNullOrEmpty(lov.LIST_OF_VALUE_ID))
            {
                lov.LIST_OF_VALUE_ID = Guid.NewGuid().ToString();
            }

            var repository = GetRepository(connectionName);
            if (lov is IPPDMEntity entity)
                _commonColumnHandler.PrepareForInsert(entity, userId);
            var result = await repository.InsertAsync(lov, userId);
            return result as LIST_OF_VALUE ?? throw new InvalidOperationException("Failed to insert LOV");
        }

        /// <summary>
        /// Adds or updates a LOV (handles skipExisting logic internally)
        /// </summary>
        public async Task<(LIST_OF_VALUE? LOV, bool WasInserted, bool WasSkipped)> AddOrUpdateLOVAsync(
            LIST_OF_VALUE lov, 
            string userId, 
            bool skipExisting = true, 
            string? connectionName = null)
        {
            connectionName ??= _connectionName;

            // Check if exists when skipExisting is true
            if (skipExisting && !string.IsNullOrEmpty(lov.VALUE_TYPE) && !string.IsNullOrEmpty(lov.VALUE_CODE))
            {
                var exists = await CheckLOVExistsAsync(lov.VALUE_TYPE, lov.VALUE_CODE, connectionName);
                if (exists)
                {
                    return (null, false, true);
                }
            }

            // Add the LOV
            var inserted = await AddLOVAsync(lov, userId, connectionName);
            return (inserted, true, false);
        }

        /// <summary>
        /// Bulk adds LOVs efficiently
        /// </summary>
        public async Task<BulkLOVResult> BulkAddLOVsAsync(
            IEnumerable<LIST_OF_VALUE> lovs, 
            string userId, 
            bool skipExisting = true, 
            string? connectionName = null)
        {
            connectionName ??= _connectionName;
            var result = new BulkLOVResult
            {
                TotalProcessed = 0,
                TotalInserted = 0,
                TotalSkipped = 0,
                Errors = new List<string>()
            };

            foreach (var lov in lovs)
            {
                try
                {
                    result.TotalProcessed++;
                    var (inserted, wasInserted, wasSkipped) = await AddOrUpdateLOVAsync(lov, userId, skipExisting, connectionName);
                    
                    if (wasSkipped)
                    {
                        result.TotalSkipped++;
                    }
                    else if (wasInserted)
                    {
                        result.TotalInserted++;
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error processing LOV (VALUE_TYPE: {lov.VALUE_TYPE}, VALUE_CODE: {lov.VALUE_CODE}): {ex.Message}");
                }
            }

            result.Success = result.Errors.Count == 0;
            return result;
        }

        /// <summary>
        /// Updates an existing LOV
        /// </summary>
        public async Task<LIST_OF_VALUE> UpdateLOVAsync(LIST_OF_VALUE lov, string userId, string connectionName = null)
        {
            connectionName ??= _connectionName;
            var repository = GetRepository(connectionName);
            if (lov is IPPDMEntity entity)
                _commonColumnHandler.PrepareForUpdate(entity, userId);
            var result = await repository.UpdateAsync(lov, userId);
            return result as LIST_OF_VALUE ?? throw new InvalidOperationException("Failed to update LOV");
        }

        /// <summary>
        /// Deletes an LOV (soft delete by setting ACTIVE_IND = 'N')
        /// </summary>
        public async Task<bool> DeleteLOVAsync(string lovId, string userId, string connectionName = null)
        {
            connectionName ??= _connectionName;
            var repository = GetRepository(connectionName);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "LIST_OF_VALUE_ID", Operator = "=", FilterValue = lovId }
            };

            var entities = await repository.GetAsync(filters);
            var lov = entities.Cast<LIST_OF_VALUE>().FirstOrDefault();
            if (lov == null)
            {
                return false;
            }

            // Soft delete
            lov.ACTIVE_IND = "N";
            if (lov is IPPDMEntity entity)
                _commonColumnHandler.PrepareForUpdate(entity, userId);
            await repository.UpdateAsync(lov, userId);
            return true;
        }

        /// <summary>
        /// Maps LIST_OF_VALUE entity to DTO
        /// </summary>
        private ListOfValue MapToDto(LIST_OF_VALUE lov)
        {
            return new ListOfValue
            {
                ListOfValueId = lov.LIST_OF_VALUE_ID,
                ValueType = lov.VALUE_TYPE,
                ValueCode = lov.VALUE_CODE,
                ValueName = lov.VALUE_NAME,
                Description = lov.DESCRIPTION,
                Category = lov.CATEGORY,
                Module = lov.MODULE,
                SortOrder = lov.SORT_ORDER,
                ParentValueId = lov.PARENT_VALUE_ID,
                IsDefault = lov.IS_DEFAULT,
                ActiveInd = lov.ACTIVE_IND,
                Source = lov.SOURCE
            };
        }
    }

    /// <summary>
    /// Result of bulk LOV operations
    /// </summary>
    public class BulkLOVResult
    {
        public bool Success { get; set; }
        public int TotalProcessed { get; set; }
        public int TotalInserted { get; set; }
        public int TotalSkipped { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    /// <summary>
    /// Result of bulk reference table operations (for R_*, RA_*, and LIST_OF_VALUE)
    /// </summary>
    public class BulkReferenceResult
    {
        public bool Success { get; set; }
        public int TotalProcessed { get; set; }
        public int TotalInserted { get; set; }
        public int TotalSkipped { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}

