using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Repositories;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.DataManagement;

namespace Beep.OilandGas.ApiService.Services
{
    /// <summary>
    /// Generic data service implementing IPPDM39DataService via PPDMGenericRepository.
    /// </summary>
    public class PPDM39GenericDataService : IPPDM39DataService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly ILogger<PPDM39GenericDataService> _logger;

        public PPDM39GenericDataService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName,
            ILogger<PPDM39GenericDataService> logger)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName ?? "PPDM39";
            _logger = logger;
        }

        public async Task<GetEntitiesResponse> GetEntitiesAsync(string tableName, List<AppFilter> filters, string? connectionName = null)
        {
            var response = new GetEntitiesResponse();
            try
            {
                var tableMetadata = await _metadata.GetTableMetadataAsync(tableName);
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{tableMetadata.EntityTypeName}") ?? typeof(object);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, entityType, connectionName ?? _connectionName, tableName);
                var entities = await repo.GetAsync(filters ?? new List<AppFilter>());
                response.Entities = entities.Select(e => ObjectToDictionary(e)).ToList();
                response.Count = response.Entities.Count;
                response.Success = true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting entities from {TableName}", tableName);
                response.Success = false;
                response.ErrorMessage = ex.Message;
            }
            return response;
        }

        public async Task<GenericEntityResponse> GetEntityByIdAsync(string tableName, object id, string? connectionName = null)
        {
            var response = new GenericEntityResponse();
            try
            {
                var tableMetadata = await _metadata.GetTableMetadataAsync(tableName);
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{tableMetadata.EntityTypeName}") ?? typeof(object);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, entityType, connectionName ?? _connectionName, tableName);
                var entity = await repo.GetByIdAsync(id);
                response.EntityData = entity != null ? ObjectToDictionary(entity) : null;
                response.Success = entity != null;
                response.Message = entity != null ? "Found" : "Not found";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting entity by id from {TableName}", tableName);
                response.Success = false;
                response.ErrorMessage = ex.Message;
            }
            return response;
        }

        public async Task<GenericEntityResponse> InsertEntityAsync(string tableName, Dictionary<string, object> entityData, string userId, string? connectionName = null)
        {
            var response = new GenericEntityResponse();
            try
            {
                var tableMetadata = await _metadata.GetTableMetadataAsync(tableName);
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{tableMetadata.EntityTypeName}") ?? typeof(object);
                var entity = DictionaryToObject(entityType, entityData);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, entityType, connectionName ?? _connectionName, tableName);
                var result = await repo.InsertAsync(entity, userId);
                response.Success = result != null;
                response.Message = string.Empty;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error inserting entity into {TableName}", tableName);
                response.Success = false;
                response.ErrorMessage = ex.Message;
            }
            return response;
        }

        public async Task<GenericEntityResponse> UpdateEntityAsync(string tableName, string entityId, Dictionary<string, object> entityData, string userId, string? connectionName = null)
        {
            var response = new GenericEntityResponse();
            try
            {
                var tableMetadata = await _metadata.GetTableMetadataAsync(tableName);
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{tableMetadata.EntityTypeName}") ?? typeof(object);
                var entity = DictionaryToObject(entityType, entityData);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, entityType, connectionName ?? _connectionName, tableName);
                var result = await repo.UpdateAsync(entity, userId);
                response.Success = result != null;
                response.Message = string.Empty;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating entity in {TableName}", tableName);
                response.Success = false;
                response.ErrorMessage = ex.Message;
            }
            return response;
        }

        public async Task<GenericEntityResponse> DeleteEntityAsync(string tableName, object id, string userId, string? connectionName = null)
        {
            var response = new GenericEntityResponse();
            try
            {
                var tableMetadata = await _metadata.GetTableMetadataAsync(tableName);
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{tableMetadata.EntityTypeName}") ?? typeof(object);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, entityType, connectionName ?? _connectionName, tableName);
                var result = await repo.SoftDeleteAsync(id, userId);
                response.Success = result;
                response.Message = string.Empty;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting entity from {TableName}", tableName);
                response.Success = false;
                response.ErrorMessage = ex.Message;
            }
            return response;
        }

        private static Dictionary<string, object> ObjectToDictionary(object obj)
        {
            if (obj == null) return new Dictionary<string, object>();
            return obj.GetType().GetProperties()
                .Where(p => p.CanRead)
                .ToDictionary(p => p.Name, p => p.GetValue(obj) ?? (object)DBNull.Value);
        }

        private static object DictionaryToObject(Type type, Dictionary<string, object> data)
        {
            var instance = Activator.CreateInstance(type) ?? throw new InvalidOperationException($"Cannot create instance of {type.Name}");
            foreach (var kv in data)
            {
                var prop = type.GetProperty(kv.Key);
                if (prop != null && prop.CanWrite && kv.Value != null && kv.Value != (object)DBNull.Value)
                {
                    try
                    {
                        var value = Convert.ChangeType(kv.Value, prop.PropertyType);
                        prop.SetValue(instance, value);
                    }
                    catch { /* skip unconvertible properties */ }
                }
            }
            return instance;
        }
    }
}
