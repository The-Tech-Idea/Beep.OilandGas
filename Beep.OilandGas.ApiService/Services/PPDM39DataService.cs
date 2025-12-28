using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs.DataManagement;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ApiService.Services
{
    /// <summary>
    /// Service for generic PPDM39 data operations using PPDMGenericRepository
    /// </summary>
    public class PPDM39DataService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<PPDM39DataService> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IProgressTrackingService? _progressTracking;

        public PPDM39DataService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<PPDM39DataService> logger,
            ILoggerFactory loggerFactory,
            IProgressTrackingService? progressTracking = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _progressTracking = progressTracking;
        }

        /// <summary>
        /// Gets a repository instance for the specified entity type
        /// </summary>
        private PPDMGenericRepository GetRepository(Type entityType, string connectionName, string? tableName = null)
        {
            return new PPDMGenericRepository(
                _editor,
                _commonColumnHandler,
                _defaults,
                _metadata,
                entityType,
                connectionName,
                tableName,
                _loggerFactory.CreateLogger<PPDMGenericRepository>());
        }

        /// <summary>
        /// Gets entity type by table name from metadata
        /// </summary>
        private Type? GetEntityTypeByTableName(string tableName)
        {
            try
            {
                // Try to find entity type in PPDM39.Models namespace
                var assembly = typeof(IPPDMEntity).Assembly;
                var types = assembly.GetTypes()
                    .Where(t => typeof(IPPDMEntity).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    .ToList();

                // Try exact match first
                var entityType = types.FirstOrDefault(t => t.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));
                if (entityType != null)
                    return entityType;

                // Try to get from metadata if available
                var tableMetadata = _metadata.GetTableMetadataAsync(tableName).GetAwaiter().GetResult();
                if (tableMetadata != null)
                {
                    // Try to find by table metadata entity type name
                    entityType = types.FirstOrDefault(t => 
                        t.Name.Equals(tableMetadata.EntityTypeName ?? tableName, StringComparison.OrdinalIgnoreCase));
                }

                return entityType;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding entity type for table {TableName}", tableName);
                return null;
            }
        }

        /// <summary>
        /// Gets entities from a table with filters
        /// </summary>
        public async Task<GetEntitiesResponse> GetEntitiesAsync(string tableName, List<AppFilter> filters, string? connectionName = null)
        {
            try
            {
                connectionName ??= _editor.ConfigEditor?.DataConnections?.FirstOrDefault()?.ConnectionName ?? "PPDM39";
                _logger.LogInformation("Getting entities from table {TableName} on connection {ConnectionName}", tableName, connectionName);

                var entityType = GetEntityTypeByTableName(tableName);
                if (entityType == null)
                {
                    return new GetEntitiesResponse
                    {
                        Success = false,
                        ErrorMessage = $"Entity type not found for table: {tableName}"
                    };
                }

                var repository = GetRepository(entityType, connectionName, tableName);
                var entities = await repository.GetAsync(filters ?? new List<AppFilter>());

                // Convert entities to dictionaries
                var entityDicts = new List<Dictionary<string, object>>();
                foreach (var entity in entities)
                {
                    var dict = new Dictionary<string, object>();
                    var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    foreach (var prop in properties)
                    {
                        if (prop.CanRead)
                        {
                            dict[prop.Name] = prop.GetValue(entity) ?? DBNull.Value;
                        }
                    }
                    entityDicts.Add(dict);
                }

                return new GetEntitiesResponse
                {
                    Success = true,
                    Entities = entityDicts,
                    Count = entityDicts.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entities from table {TableName}", tableName);
                return new GetEntitiesResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Gets a single entity by ID
        /// </summary>
        public async Task<GenericEntityResponse> GetEntityByIdAsync(string tableName, object id, string? connectionName = null)
        {
            try
            {
                connectionName ??= _editor.ConfigEditor?.DataConnections?.FirstOrDefault()?.ConnectionName ?? "PPDM39";
                _logger.LogInformation("Getting entity by ID from table {TableName}, ID: {Id}", tableName, id);

                var entityType = GetEntityTypeByTableName(tableName);
                if (entityType == null)
                {
                    return new GenericEntityResponse
                    {
                        Success = false,
                        ErrorMessage = $"Entity type not found for table: {tableName}"
                    };
                }

                var repository = GetRepository(entityType, connectionName, tableName);
                var primaryKeyName = repository.TableName; // Use metadata to get primary key
                
                // Get primary key from metadata
                var tableMetadata = await _metadata.GetTableMetadataAsync(tableName);
                var pkName = tableMetadata?.PrimaryKeyColumn ?? $"{tableName}_ID";

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = pkName, FilterValue = id?.ToString() ?? string.Empty, Operator = "=" }
                };

                var entities = await repository.GetAsync(filters);
                var entity = entities.FirstOrDefault();

                if (entity == null)
                {
                    return new GenericEntityResponse
                    {
                        Success = false,
                        Message = "Entity not found",
                        ErrorMessage = $"No entity found with ID: {id}"
                    };
                }

                // Convert entity to dictionary
                var entityDict = new Dictionary<string, object>();
                var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var prop in properties)
                {
                    if (prop.CanRead)
                    {
                        entityDict[prop.Name] = prop.GetValue(entity) ?? DBNull.Value;
                    }
                }

                return new GenericEntityResponse
                {
                    Success = true,
                    Message = "Entity retrieved successfully",
                    EntityData = entityDict
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entity by ID from table {TableName}", tableName);
                return new GenericEntityResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Inserts an entity
        /// </summary>
        public async Task<GenericEntityResponse> InsertEntityAsync(string tableName, Dictionary<string, object> entityData, string userId, string? connectionName = null)
        {
            try
            {
                connectionName ??= _editor.ConfigEditor?.DataConnections?.FirstOrDefault()?.ConnectionName ?? "PPDM39";
                _logger.LogInformation("Inserting entity into table {TableName}", tableName);

                var entityType = GetEntityTypeByTableName(tableName);
                if (entityType == null)
                {
                    return new GenericEntityResponse
                    {
                        Success = false,
                        ErrorMessage = $"Entity type not found for table: {tableName}"
                    };
                }

                var repository = GetRepository(entityType, connectionName, tableName);
                
                // Create entity instance and populate from dictionary
                var entity = Activator.CreateInstance(entityType);
                if (entity == null)
                {
                    return new GenericEntityResponse
                    {
                        Success = false,
                        ErrorMessage = $"Failed to create instance of {entityType.Name}"
                    };
                }

                var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
                foreach (var kvp in entityData)
                {
                    var prop = properties.FirstOrDefault(p => p.Name.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase));
                    if (prop != null && prop.CanWrite)
                    {
                        try
                        {
                            var value = kvp.Value;
                            if (value != null && value != DBNull.Value && prop.PropertyType != typeof(object))
                            {
                                // Convert value to property type if needed
                                if (value.GetType() != prop.PropertyType)
                                {
                                    value = Convert.ChangeType(value, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                                }
                            }
                            prop.SetValue(entity, value == DBNull.Value ? null : value);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to set property {PropertyName} on {EntityType}", kvp.Key, entityType.Name);
                        }
                    }
                }

                var insertedEntity = await repository.InsertAsync(entity, userId);

                // Convert inserted entity back to dictionary
                var resultDict = new Dictionary<string, object>();
                foreach (var prop in properties.Where(p => p.CanRead))
                {
                    resultDict[prop.Name] = prop.GetValue(insertedEntity) ?? DBNull.Value;
                }

                return new GenericEntityResponse
                {
                    Success = true,
                    Message = "Entity inserted successfully",
                    EntityData = resultDict
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting entity into table {TableName}", tableName);
                return new GenericEntityResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Updates an entity
        /// </summary>
        public async Task<GenericEntityResponse> UpdateEntityAsync(string tableName, string entityId, Dictionary<string, object> entityData, string userId, string? connectionName = null)
        {
            try
            {
                connectionName ??= _editor.ConfigEditor?.DataConnections?.FirstOrDefault()?.ConnectionName ?? "PPDM39";
                _logger.LogInformation("Updating entity in table {TableName}, ID: {Id}", tableName, entityId);

                var entityType = GetEntityTypeByTableName(tableName);
                if (entityType == null)
                {
                    return new GenericEntityResponse
                    {
                        Success = false,
                        ErrorMessage = $"Entity type not found for table: {tableName}"
                    };
                }

                var repository = GetRepository(entityType, connectionName, tableName);
                
                // Get existing entity first
                var tableMetadata = await _metadata.GetTableMetadataAsync(tableName);
                var pkName = tableMetadata?.PrimaryKeyColumn ?? $"{tableName}_ID";

                var getFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = pkName, FilterValue = entityId, Operator = "=" }
                };

                var existingEntities = await repository.GetAsync(getFilters);
                var existingEntity = existingEntities.FirstOrDefault();

                if (existingEntity == null)
                {
                    return new GenericEntityResponse
                    {
                        Success = false,
                        ErrorMessage = $"Entity not found with ID: {entityId}"
                    };
                }

                // Update properties from dictionary
                var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
                foreach (var kvp in entityData)
                {
                    var prop = properties.FirstOrDefault(p => p.Name.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase));
                    if (prop != null && prop.CanWrite && prop.Name != pkName) // Don't update primary key
                    {
                        try
                        {
                            var value = kvp.Value;
                            if (value != null && value != DBNull.Value && prop.PropertyType != typeof(object))
                            {
                                if (value.GetType() != prop.PropertyType)
                                {
                                    value = Convert.ChangeType(value, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                                }
                            }
                            prop.SetValue(existingEntity, value == DBNull.Value ? null : value);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to set property {PropertyName} on {EntityType}", kvp.Key, entityType.Name);
                        }
                    }
                }

                var updatedEntity = await repository.UpdateAsync(existingEntity, userId);

                // Convert updated entity back to dictionary
                var resultDict = new Dictionary<string, object>();
                foreach (var prop in properties.Where(p => p.CanRead))
                {
                    resultDict[prop.Name] = prop.GetValue(updatedEntity) ?? DBNull.Value;
                }

                return new GenericEntityResponse
                {
                    Success = true,
                    Message = "Entity updated successfully",
                    EntityData = resultDict
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating entity in table {TableName}", tableName);
                return new GenericEntityResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Deletes an entity
        /// </summary>
        public async Task<GenericEntityResponse> DeleteEntityAsync(string tableName, object id, string userId, string? connectionName = null)
        {
            try
            {
                connectionName ??= _editor.ConfigEditor?.DataConnections?.FirstOrDefault()?.ConnectionName ?? "PPDM39";
                _logger.LogInformation("Deleting entity from table {TableName}, ID: {Id}", tableName, id);

                var entityType = GetEntityTypeByTableName(tableName);
                if (entityType == null)
                {
                    return new GenericEntityResponse
                    {
                        Success = false,
                        ErrorMessage = $"Entity type not found for table: {tableName}"
                    };
                }

                var repository = GetRepository(entityType, connectionName, tableName);
                
                // Get entity first to ensure it exists
                var tableMetadata = await _metadata.GetTableMetadataAsync(tableName);
                var pkName = tableMetadata?.PrimaryKeyColumn ?? $"{tableName}_ID";

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = pkName, FilterValue = id?.ToString() ?? string.Empty, Operator = "=" }
                };

                var entities = await repository.GetAsync(filters);
                var entity = entities.FirstOrDefault();

                if (entity == null)
                {
                    return new GenericEntityResponse
                    {
                        Success = false,
                        ErrorMessage = $"Entity not found with ID: {id}"
                    };
                }

                // Delete using entity object (hard delete)
                await repository.DeleteByIdAsync(id, userId);

                return new GenericEntityResponse
                {
                    Success = true,
                    Message = "Entity deleted successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting entity from table {TableName}", tableName);
                return new GenericEntityResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}
