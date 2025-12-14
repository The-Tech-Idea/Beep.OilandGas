using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.DTOs;
using Beep.OilandGas.PPDM39.Core.Interfaces;
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

        // In-memory cache for audit events (can be persisted to database)
        private readonly List<DataAccessEvent> _auditCache = new List<DataAccessEvent>();
        private readonly object _auditLock = new object();

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

            // Create repository for audit table (could use a dedicated AUDIT_LOG table)
            // For now, using in-memory storage
        }

        /// <summary>
        /// Records data access event
        /// </summary>
        public async Task RecordAccessAsync(DataAccessEvent accessEvent)
        {
            if (accessEvent == null)
                throw new ArgumentNullException(nameof(accessEvent));

            // Generate event ID if not provided
            if (string.IsNullOrWhiteSpace(accessEvent.EventId))
            {
                accessEvent.EventId = Guid.NewGuid().ToString();
            }

            // Set access date if not provided
            if (accessEvent.AccessDate == default(DateTime))
            {
                accessEvent.AccessDate = DateTime.UtcNow;
            }

            // Store in cache (in production, persist to database)
            lock (_auditLock)
            {
                _auditCache.Add(accessEvent);
            }

            // Optionally persist to database here
            // await PersistAuditEventAsync(accessEvent);

            await Task.CompletedTask;
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

            lock (_auditLock)
            {
                var history = _auditCache
                    .Where(e => e.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase) &&
                                e.EntityId?.ToString() == entityId.ToString())
                    .OrderByDescending(e => e.AccessDate)
                    .ToList();

                return Task.FromResult(history).Result;
            }
        }

        /// <summary>
        /// Gets access history for a user
        /// </summary>
        public async Task<List<DataAccessEvent>> GetUserAccessHistoryAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            lock (_auditLock)
            {
                var query = _auditCache
                    .Where(e => e.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase));

                if (fromDate.HasValue)
                {
                    query = query.Where(e => e.AccessDate >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(e => e.AccessDate <= toDate.Value);
                }

                var history = query
                    .OrderByDescending(e => e.AccessDate)
                    .ToList();

                return Task.FromResult(history).Result;
            }
        }

        /// <summary>
        /// Gets access statistics for compliance reporting
        /// </summary>
        public async Task<AccessStatistics> GetAccessStatisticsAsync(string tableName = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            lock (_auditLock)
            {
                var query = _auditCache.AsQueryable();

                if (!string.IsNullOrWhiteSpace(tableName))
                {
                    query = query.Where(e => e.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase));
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(e => e.AccessDate >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(e => e.AccessDate <= toDate.Value);
                }

                var events = query.ToList();

                var stats = new AccessStatistics
                {
                    TableName = tableName,
                    FromDate = fromDate ?? DateTime.MinValue,
                    ToDate = toDate ?? DateTime.MaxValue,
                    TotalAccessEvents = events.Count,
                    UniqueUsers = events.Select(e => e.UserId).Distinct().Count(),
                    ReadOperations = events.Count(e => e.AccessType == "Read"),
                    WriteOperations = events.Count(e => e.AccessType == "Write"),
                    DeleteOperations = events.Count(e => e.AccessType == "Delete"),
                    AccessByUser = events
                        .GroupBy(e => e.UserId)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    AccessByType = events
                        .GroupBy(e => e.AccessType)
                        .ToDictionary(g => g.Key, g => g.Count())
                };

                return Task.FromResult(stats).Result;
            }
        }
    }
}

