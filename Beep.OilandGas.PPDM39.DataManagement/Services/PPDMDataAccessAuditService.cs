using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.PPDM39.DataManagement.Services
{
    /// <summary>
    /// Service for tracking data access for compliance and auditing
    /// Records who accessed what data and when
    /// </summary>
    public class PPDMDataAccessAuditService : IPPDMDataAccessAuditService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly PPDMGenericRepository _auditRepository;

        public PPDMDataAccessAuditService(
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

            // Create repository for audit table
            _auditRepository = new PPDMGenericRepository(
                _editor,
                _commonColumnHandler,
                _defaults,
                _metadata,
                typeof(DATA_ACCESS_AUDIT),
                _connectionName,
                "DATA_ACCESS_AUDIT");
        }

        /// <summary>
        /// Records data access event
        /// </summary>
        public async Task RecordAccessAsync(DataAccessEvent accessEvent)
        {
            if (accessEvent == null)
                throw new ArgumentNullException(nameof(accessEvent));

            // Convert DTO to entity
            var auditEntity = new DATA_ACCESS_AUDIT
            {
                ACCESS_AUDIT_ID = string.IsNullOrWhiteSpace(accessEvent.EventId) ? Guid.NewGuid().ToString() : accessEvent.EventId,
                USER_ID = accessEvent.UserId,
                TABLE_NAME = accessEvent.TableName,
                ENTITY_ID = accessEvent.EntityId?.ToString() ?? string.Empty,
                ACCESS_TYPE = accessEvent.AccessType,
                ACCESS_DATE = accessEvent.AccessDate == default(DateTime) ? DateTime.UtcNow : accessEvent.AccessDate,
                IP_ADDRESS = accessEvent.IpAddress,
                SESSION_ID = accessEvent.ApplicationName, // Map ApplicationName to SESSION_ID
                REMARK = accessEvent.Metadata != null && accessEvent.Metadata.Any() 
                    ? System.Text.Json.JsonSerializer.Serialize(accessEvent.Metadata) 
                    : null,
                ACTIVE_IND = _defaults?.GetActiveIndicatorYes() ?? "Y",
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_DATE = DateTime.UtcNow,
                ROW_CREATED_BY = accessEvent.UserId
            };

            // Persist to database
            await _auditRepository.InsertAsync(auditEntity, accessEvent.UserId);
        }

        /// <summary>
        /// Gets access history for an entity
        /// </summary>
        public async Task<List<DataAccessEvent>> GetAccessHistoryAsync(string tableName, object entityId)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty", nameof(tableName));
            if (entityId == null)
                throw new ArgumentNullException(nameof(entityId));

            // Query database for access history
            var filters = new List<TheTechIdea.Beep.Report.AppFilter>
            {
                new TheTechIdea.Beep.Report.AppFilter { FieldName = "TABLE_NAME", Operator = "=", FilterValue = tableName },
                new TheTechIdea.Beep.Report.AppFilter { FieldName = "ENTITY_ID", Operator = "=", FilterValue = entityId.ToString() },
                new TheTechIdea.Beep.Report.AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults?.GetActiveIndicatorYes() ?? "Y" }
            };

            var auditEntities = await _auditRepository.GetAsync(filters);
            
            return auditEntities
                .Cast<DATA_ACCESS_AUDIT>()
                .OrderByDescending(e => e.ACCESS_DATE)
                .Select(MapToDataAccessEvent)
                .ToList();
        }

        /// <summary>
        /// Gets access history for a user
        /// </summary>
        public async Task<List<DataAccessEvent>> GetUserAccessHistoryAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            // Query database for user access history
            var filters = new List<TheTechIdea.Beep.Report.AppFilter>
            {
                new TheTechIdea.Beep.Report.AppFilter { FieldName = "USER_ID", Operator = "=", FilterValue = userId },
                new TheTechIdea.Beep.Report.AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults?.GetActiveIndicatorYes() ?? "Y" }
            };

            if (fromDate.HasValue)
            {
                filters.Add(new TheTechIdea.Beep.Report.AppFilter { FieldName = "ACCESS_DATE", Operator = ">=", FilterValue = fromDate.Value.ToString() });
            }

            if (toDate.HasValue)
            {
                filters.Add(new TheTechIdea.Beep.Report.AppFilter { FieldName = "ACCESS_DATE", Operator = "<=", FilterValue = toDate.Value.ToString() });
            }

            var auditEntities = await _auditRepository.GetAsync(filters);
            
            return auditEntities
                .Cast<DATA_ACCESS_AUDIT>()
                .OrderByDescending(e => e.ACCESS_DATE)
                .Select(MapToDataAccessEvent)
                .ToList();
        }

        /// <summary>
        /// Gets access statistics for compliance reporting
        /// </summary>
        public async Task<AccessStatistics> GetAccessStatisticsAsync(string tableName = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            // Query database for access events
            var filters = new List<TheTechIdea.Beep.Report.AppFilter>
            {
                new TheTechIdea.Beep.Report.AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults?.GetActiveIndicatorYes() ?? "Y" }
            };

            if (!string.IsNullOrWhiteSpace(tableName))
            {
                filters.Add(new TheTechIdea.Beep.Report.AppFilter { FieldName = "TABLE_NAME", Operator = "=", FilterValue = tableName });
            }

            if (fromDate.HasValue)
            {
                filters.Add(new TheTechIdea.Beep.Report.AppFilter { FieldName = "ACCESS_DATE", Operator = ">=", FilterValue = fromDate.Value.ToString() });
            }

            if (toDate.HasValue)
            {
                filters.Add(new TheTechIdea.Beep.Report.AppFilter { FieldName = "ACCESS_DATE", Operator = "<=", FilterValue = toDate.Value.ToString() });
            }

            var auditEntities = await _auditRepository.GetAsync(filters);
            var events = auditEntities.Cast<DATA_ACCESS_AUDIT>().ToList();

            var stats = new AccessStatistics
            {
                TableName = tableName,
                FromDate = fromDate ?? DateTime.MinValue,
                ToDate = toDate ?? DateTime.MaxValue,
                TotalAccessEvents = events.Count,
                UniqueUsers = events.Select(e => e.USER_ID).Distinct().Count(),
                ReadOperations = events.Count(e => e.ACCESS_TYPE == "Read"),
                WriteOperations = events.Count(e => e.ACCESS_TYPE == "Write"),
                DeleteOperations = events.Count(e => e.ACCESS_TYPE == "Delete"),
                AccessByUser = events
                    .GroupBy(e => e.USER_ID)
                    .ToDictionary(g => g.Key, g => g.Count()),
                AccessByType = events
                    .GroupBy(e => e.ACCESS_TYPE)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return stats;
        }

        /// <summary>
        /// Maps DATA_ACCESS_AUDIT entity to DataAccessEvent DTO
        /// </summary>
        private DataAccessEvent MapToDataAccessEvent(DATA_ACCESS_AUDIT entity)
        {
            var dto = new DataAccessEvent
            {
                EventId = entity.ACCESS_AUDIT_ID,
                UserId = entity.USER_ID,
                TableName = entity.TABLE_NAME,
                EntityId = entity.ENTITY_ID,
                AccessType = entity.ACCESS_TYPE,
                AccessDate = entity.ACCESS_DATE ?? DateTime.UtcNow,
                IpAddress = entity.IP_ADDRESS,
                ApplicationName = entity.SESSION_ID
            };

            // Deserialize metadata from REMARK if it's JSON
            if (!string.IsNullOrWhiteSpace(entity.REMARK))
            {
                try
                {
                    dto.Metadata = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(entity.REMARK) 
                        ?? new Dictionary<string, object>();
                }
                catch
                {
                    dto.Metadata = new Dictionary<string, object>();
                }
            }

            return dto;
        }
    }
}

