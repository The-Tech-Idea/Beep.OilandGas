using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Repositories;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.DataManagement;

namespace Beep.OilandGas.ApiService.Services
{
    /// <summary>
    /// Generic data service implementing IPPDM39DataService via PPDMGenericRepository.
    /// </summary>
    /// <remarks>
    /// OILGAS-CATCH-01. Every operation caught every exception and answered <c>Success = false</c> with the exception's own
    /// text, which the controller handed to the caller; a table the metadata does not know reached a null dereference; an
    /// entity type the metadata named and this application lacks fell back to <see cref="object"/>; and a value that did not
    /// convert to its column was skipped and the row saved without it — which, for the JSON the controller passes, was every
    /// value, since a <see cref="JsonElement"/> converts to nothing. Now a table PPDM does not have is refused, a value its
    /// column cannot hold is answered <c>Success = false</c> naming the column, and anything else is a failure the API's
    /// handler reports.
    /// </remarks>
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

        public async Task<GetEntitiesResponse> GetEntitiesAsync(string tableName, List<AppFilter> filters, string connectionName = "PPDM39")
        {
            var (repo, _) = await RepositoryForAsync(tableName, connectionName);
            var entities = await repo.GetAsync(filters ?? new List<AppFilter>());
            var response = new GetEntitiesResponse { Entities = entities.Select(e => ObjectToDictionary(e)).ToList() };
            response.Count = response.Entities.Count;
            response.Success = true;
            return response;
        }

        public async Task<GenericEntityResponse> GetEntityByIdAsync(string tableName, object id, string connectionName = "PPDM39")
        {
            var (repo, _) = await RepositoryForAsync(tableName, connectionName);
            var entity = await repo.GetByIdAsync(id);
            return new GenericEntityResponse
            {
                EntityData = entity != null ? ObjectToDictionary(entity) : null,
                Success = entity != null,
                Message = entity != null ? "Found" : "Not found"
            };
        }

        public async Task<GenericEntityResponse> InsertEntityAsync(string tableName, Dictionary<string, object> entityData, string userId, string connectionName = "PPDM39")
        {
            var (repo, entityType) = await RepositoryForAsync(tableName, connectionName);
            var (entity, refusal) = DictionaryToObject(entityType, entityData);
            if (refusal != null)
                return new GenericEntityResponse { Success = false, ErrorMessage = refusal };
            var result = await repo.InsertAsync(entity!, userId);
            return new GenericEntityResponse { Success = result != null, Message = string.Empty };
        }

        public async Task<GenericEntityResponse> UpdateEntityAsync(string tableName, string entityId, Dictionary<string, object> entityData, string userId, string connectionName = "PPDM39")
        {
            var (repo, entityType) = await RepositoryForAsync(tableName, connectionName);
            var (entity, refusal) = DictionaryToObject(entityType, entityData);
            if (refusal != null)
                return new GenericEntityResponse { Success = false, ErrorMessage = refusal };
            var result = await repo.UpdateAsync(entity!, userId);
            return new GenericEntityResponse { Success = result != null, Message = string.Empty };
        }

        public async Task<GenericEntityResponse> DeleteEntityAsync(string tableName, object id, string userId, string connectionName = "PPDM39")
        {
            var (repo, _) = await RepositoryForAsync(tableName, connectionName);
            var result = await repo.SoftDeleteAsync(id, userId);
            return new GenericEntityResponse { Success = result, Message = string.Empty };
        }

        // A table the metadata does not know is the caller's to correct; an entity type the metadata names and this
        // application does not have is a fault in the metadata, not a reason to read rows as plain objects.
        private async Task<(PPDMGenericRepository Repository, Type EntityType)> RepositoryForAsync(string tableName, string? connectionName)
        {
            var tableMetadata = await _metadata.GetTableMetadataAsync(tableName)
                ?? throw RefusalException.NotFound($"Table '{tableName}' is not a PPDM table.");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{tableMetadata.EntityTypeName}")
                ?? throw new InvalidOperationException(
                    $"PPDM table '{tableName}' maps to entity type '{tableMetadata.EntityTypeName}', which this application does not have.");
            return (new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, entityType,
                connectionName ?? _connectionName, tableName), entityType);
        }

        private static Dictionary<string, object> ObjectToDictionary(object obj)
        {
            if (obj == null) return new Dictionary<string, object>();
            return obj.GetType().GetProperties()
                .Where(p => p.CanRead)
                .ToDictionary(p => p.Name, p => p.GetValue(obj) ?? (object)DBNull.Value);
        }

        // Each value is set only when its property's type can hold it, asked of the value itself: a JSON value by its kind,
        // text of the type's converter in the invariant culture. A value that cannot be held refuses the whole entity —
        // saving the rest would store a record that is not what was sent.
        private static (object? Entity, string? Refusal) DictionaryToObject(Type type, Dictionary<string, object> data)
        {
            var instance = Activator.CreateInstance(type) ?? throw new InvalidOperationException($"Cannot create instance of {type.Name}");
            foreach (var kv in data)
            {
                var prop = type.GetProperty(kv.Key);
                if (prop == null || !prop.CanWrite || kv.Value == null || kv.Value == (object)DBNull.Value)
                    continue;
                if (!TryConvert(kv.Value, prop.PropertyType, out var value))
                    return (null, $"'{kv.Key}' is not a valid {(Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType).Name} value.");
                if (value != null)
                    prop.SetValue(instance, value);
            }
            return (instance, null);
        }

        private static bool TryConvert(object raw, Type propertyType, out object? value)
        {
            var target = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            value = null;
            switch (raw)
            {
                case JsonElement element:
                    if (element.ValueKind == JsonValueKind.Null)
                        return true;
                    if (target == typeof(string))
                    {
                        value = element.ValueKind == JsonValueKind.String ? element.GetString() : element.GetRawText();
                        return true;
                    }
                    if (element.ValueKind == JsonValueKind.String)
                        return TryFromText(element.GetString()!, target, out value);
                    if (element.ValueKind is JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False)
                        return TryFromText(element.GetRawText(), target, out value);
                    return false;
                case string text:
                    if (target == typeof(string))
                    {
                        value = text;
                        return true;
                    }
                    // An empty value leaves a non-text column unset, as an empty CSV cell does.
                    return text.Length == 0 || TryFromText(text, target, out value);
                default:
                    if (target.IsInstanceOfType(raw))
                    {
                        value = raw;
                        return true;
                    }
                    return TryFromText(Convert.ToString(raw, CultureInfo.InvariantCulture) ?? string.Empty, target, out value);
            }
        }

        private static bool TryFromText(string text, Type target, out object? value)
        {
            var converter = TypeDescriptor.GetConverter(target);
            if (!converter.CanConvertFrom(typeof(string)) || !converter.IsValid(text))
            {
                value = null;
                return false;
            }
            value = converter.ConvertFromInvariantString(text);
            return true;
        }
    }
}
