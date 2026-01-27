using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.LifeCycle.Services.Calculations
{
    public partial class PPDMCalculationService
    {
        /// <summary>
        /// Retrieval mode for time-series data
        /// </summary>
        public enum DataRetrievalMode
        {
            /// <summary>Get the most recent record (default)</summary>
            Latest,
            /// <summary>Get record at or nearest to a specific date</summary>
            ByDate,
            /// <summary>Get all historical records</summary>
            History
        }

        // Entity cache to avoid multiple database calls for the same entity
        private readonly Dictionary<string, object?> _entityCache = new();
        private readonly Dictionary<string, List<object>> _entityListCache = new();

        /// <summary>
        /// Gets a single PPDM entity from database with caching
        /// </summary>
        private async Task<object?> GetEntityAsync(string tableName, string entityId, string idFieldName)
        {
            var cacheKey = $"{tableName}:{entityId}";
            if (_entityCache.TryGetValue(cacheKey, out var cached))
                return cached;

            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            if (metadata == null)
                return null;

            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(object);
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, _connectionName, tableName, null);

            var filters = new List<AppFilter>
            {
                new AppFilter
                {
                    FieldName = idFieldName,
                    Operator = "=",
                    FilterValue = _defaults.FormatIdForTable(tableName, entityId)
                }
            };

            var entities = await repo.GetAsync(filters);
            var entity = entities.FirstOrDefault();
            _entityCache[cacheKey] = entity;
            return entity;
        }

        /// <summary>
        /// Gets multiple PPDM entities from database with caching and optional date ordering
        /// Used for time-series tables like WELL_TEST, PRODUCTION, etc.
        /// </summary>
        private async Task<List<object>> GetEntitiesAsync(
            string tableName, 
            List<AppFilter> filters,
            string dateFieldName = "EFFECTIVE_DATE",
            DataRetrievalMode mode = DataRetrievalMode.Latest,
            DateTime? asOfDate = null)
        {
            var cacheKey = $"{tableName}:{string.Join(",", filters.Select(f => $"{f.FieldName}={f.FilterValue}"))}:{mode}:{asOfDate}";
            if (_entityListCache.TryGetValue(cacheKey, out var cached))
                return cached;

            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            if (metadata == null)
                return new List<object>();

            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(object);
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, _connectionName, tableName, null);

            if (mode == DataRetrievalMode.ByDate && asOfDate.HasValue)
            {
                filters.Add(new AppFilter
                {
                    FieldName = dateFieldName,
                    Operator = "<=",
                    FilterValue = asOfDate.Value.ToString("yyyy-MM-dd")
                });
            }

            var entities = await repo.GetAsync(filters);
            var entityList = entities.ToList();

            if (!string.IsNullOrEmpty(dateFieldName))
            {
                entityList = entityList
                    .OrderByDescending(e => GetDateValue(e, dateFieldName))
                    .ToList();
            }

            if (mode == DataRetrievalMode.Latest || mode == DataRetrievalMode.ByDate)
            {
                entityList = entityList.Take(1).ToList();
            }

            _entityListCache[cacheKey] = entityList;
            return entityList;
        }

        private async Task<object?> GetLatestEntityForWellAsync(
            string tableName, 
            string wellId,
            string dateFieldName = "EFFECTIVE_DATE",
            DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "WELL_ID", Operator = "=", FilterValue = wellId }
            };

            var mode = asOfDate.HasValue ? DataRetrievalMode.ByDate : DataRetrievalMode.Latest;
            var entities = await GetEntitiesAsync(tableName, filters, dateFieldName, mode, asOfDate);
            return entities.FirstOrDefault();
        }

        private async Task<List<object>> GetHistoryForWellAsync(
            string tableName,
            string wellId,
            string dateFieldName = "EFFECTIVE_DATE",
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return new List<object>();

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "WELL_ID", Operator = "=", FilterValue = wellId }
            };

            if (startDate.HasValue)
                filters.Add(new AppFilter { FieldName = dateFieldName, Operator = ">=", FilterValue = startDate.Value.ToString("yyyy-MM-dd") });

            if (endDate.HasValue)
                filters.Add(new AppFilter { FieldName = dateFieldName, Operator = "<=", FilterValue = endDate.Value.ToString("yyyy-MM-dd") });

            return await GetEntitiesAsync(tableName, filters, dateFieldName, DataRetrievalMode.History);
        }

        public void ClearEntityCache()
        {
            _entityCache.Clear();
            _entityListCache.Clear();
        }
    }
}
