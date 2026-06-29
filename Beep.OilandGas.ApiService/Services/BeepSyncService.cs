using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.BeepSync;
using TheTechIdea.Beep.Editor.BeepSync.Models;
using TheTechIdea.Beep.Editor.Importing;

namespace Beep.OilandGas.ApiService.Services
{
    /// <summary>
    /// Wraps BeepDM's BeepSyncManager for PPDM 3.9 entity synchronization.
    /// Supports full sync, CDC incremental sync, conflict resolution, data quality
    /// gates, reconciliation reports, and SLO monitoring.
    ///
    /// Phase 5A of BeepDM framework integration.
    ///
    /// Start with full-sync mode (DestinationWins) for WELL and FIELD entities.
    /// Add CDC incremental sync and bidirectional conflict resolution in Phase 5B.
    /// </summary>
    public class BeepSyncService : IDisposable
    {
        private readonly BeepSyncManager _syncManager;
        private readonly ILogger<BeepSyncService> _logger;
        private readonly IDMEEditor _editor;
        private bool _schemasLoaded;

        public BeepSyncService(IDMEEditor editor, ILogger<BeepSyncService> logger)
        {
            _editor = editor;
            _logger = logger;
            _syncManager = new BeepSyncManager(editor);
        }

        /// <summary>
        /// Creates or updates a sync schema for a PPDM entity.
        /// A schema defines what to sync (source → destination), how to map fields,
        /// and what quality rules to apply.
        /// </summary>
        public DataSyncSchema CreateEntitySyncSchema(
            string entityName,
            string sourceDataSourceName,
            string destDataSourceName,
            SyncMode mode = SyncMode.Full,
            SyncDirection direction = SyncDirection.SourceToDestination,
            List<SyncFieldMapping>? fieldMappings = null)
        {
            var schema = new DataSyncSchema
            {
                SchemaId = $"ppdm39-{entityName.ToLowerInvariant()}",
                SchemaName = $"PPDM39 {entityName} Sync",
                SourceEntityName = entityName,
                DestEntityName = entityName,
                SourceDataSourceName = sourceDataSourceName,
                DestDataSourceName = destDataSourceName,
                SyncMode = mode,
                SyncDirection = direction,
                ConflictResolutionStrategy = "DestinationWins",
                IsEnabled = true
            };

            if (fieldMappings?.Count > 0)
            {
                foreach (var mapping in fieldMappings)
                    schema.FieldMappings.Add(mapping);
            }

            _syncManager.AddSyncSchema(schema);
            _logger.LogInformation("Created sync schema: {SchemaId} ({Entity})", schema.SchemaId, entityName);

            return schema;
        }

        /// <summary>
        /// Synchronizes a single PPDM entity from source to destination.
        /// </summary>
        /// <param name="schemaId">The schema ID to execute (e.g., "ppdm39-well").</param>
        /// <param name="progress">Optional progress reporter.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Sync result with row counts and reconciliation data.</returns>
        public async Task<SyncResult> SyncEntityAsync(
            string schemaId,
            IProgress<TheTechIdea.Beep.PassedArgs>? progress = null,
            CancellationToken token = default)
        {
            _logger.LogInformation("Starting entity sync: {SchemaId}", schemaId);

            try
            {
                // Use file-based stores for error/history tracking
                var errorStore = LocalStoreFactory.CreateErrorStore(_editor);
                var historyStore = LocalStoreFactory.CreateHistoryStore(_editor);

                await _syncManager.SyncDataAsync(schemaId, token, progress, errorStore, historyStore);

                var report = _syncManager.LastRunReconciliationReport;
                var metrics = _syncManager.LastRunMetrics;

                return new SyncResult
                {
                    Success = metrics?.IsSuccessful ?? report?.TotalRecords > 0,
                    SchemaId = schemaId,
                    RecordsRead = report?.SourceRowCount ?? 0,
                    RecordsInserted = metrics?.RecordsInserted ?? 0,
                    RecordsUpdated = metrics?.RecordsUpdated ?? 0,
                    RecordsFailed = metrics?.FailedRecords ?? 0,
                    ConflictsResolved = report?.ConflictsResolved ?? 0,
                    Duration = metrics?.Duration ?? TimeSpan.Zero,
                    SloTier = metrics?.SloComplianceTier ?? "Green",
                    Reconciliation = report != null ? new SyncReconciliation
                    {
                        SourceRows = report.SourceRowCount,
                        DestRows = report.DestRowCount,
                        Rejects = report.RejectCount,
                        Conflicts = report.ConflictCount,
                        MappingQualityBand = report.MappingQualityBand
                    } : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sync failed: {SchemaId}", schemaId);
                return new SyncResult
                {
                    Success = false,
                    SchemaId = schemaId,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Synchronizes all enabled PPDM sync schemas sequentially.
        /// </summary>
        public async Task<List<SyncResult>> SyncAllAsync(
            IProgress<TheTechIdea.Beep.PassedArgs>? progress = null,
            CancellationToken token = default)
        {
            var results = new List<SyncResult>();

            await _syncManager.SyncAllDataAsync(token, progress);

            var schemas = _syncManager.GetSyncSchemas();
            foreach (var schema in schemas.Where(s => s.IsEnabled))
            {
                results.Add(new SyncResult
                {
                    SchemaId = schema.SchemaId,
                    SchemaName = schema.SchemaName,
                    Success = true
                });
            }

            return results;
        }

        /// <summary>
        /// Initializes the standard PPDM39 sync schemas for core entities.
        /// Call this during app startup or first-run setup.
        /// </summary>
        public void InitializePpdmSyncSchemas(
            string sourceDataSourceName,
            string destDataSourceName = "PPDM39")
        {
            if (_schemasLoaded)
            {
                _logger.LogDebug("Sync schemas already initialized — skipping");
                return;
            }

            // Core entities that benefit from multi-instance synchronization
            CreateEntitySyncSchema("WELL", sourceDataSourceName, destDataSourceName);
            CreateEntitySyncSchema("FIELD", sourceDataSourceName, destDataSourceName);
            CreateEntitySyncSchema("FACILITY", sourceDataSourceName, destDataSourceName);

            // Production data — full sync initially (CDC in Phase 5B)
            CreateEntitySyncSchema("PDEN_VOL_SUMMARY", sourceDataSourceName, destDataSourceName,
                mode: SyncMode.Full);

            _schemasLoaded = true;
            _logger.LogInformation("Initialized PPDM39 sync schemas for WELL, FIELD, FACILITY, PDEN_VOL_SUMMARY");
        }

        /// <summary>
        /// Saves sync schemas to persistent storage.
        /// </summary>
        public async Task SaveSchemasAsync()
        {
            await _syncManager.SaveSchemasAsync();
            _logger.LogInformation("Sync schemas saved");
        }

        /// <summary>
        /// Loads sync schemas from persistent storage.
        /// </summary>
        public async Task LoadSchemasAsync()
        {
            await _syncManager.LoadSchemasAsync();
            _schemasLoaded = true;
            _logger.LogInformation("Sync schemas loaded");
        }

        /// <summary>
        /// Returns all configured sync schemas.
        /// </summary>
        public IReadOnlyList<DataSyncSchema> GetSchemas()
        {
            return _syncManager.GetSyncSchemas().ToList();
        }

        /// <summary>
        /// Returns the reconciliation report from the last sync run.
        /// </summary>
        public SyncReconciliationReport? GetLastReconciliation()
        {
            return _syncManager.LastRunReconciliationReport;
        }

        public void Dispose()
        {
            _syncManager?.Dispose();
        }
    }

    /// <summary>Simplified sync field mapping DTO.</summary>
    public class SyncFieldMapping
    {
        public string SourceField { get; set; } = string.Empty;
        public string DestField { get; set; } = string.Empty;
        public bool IsKey { get; set; }
        public bool IsWatermark { get; set; }
    }

    /// <summary>Result of a sync operation.</summary>
    public class SyncResult
    {
        public bool Success { get; set; }
        public string SchemaId { get; set; } = string.Empty;
        public string? SchemaName { get; set; }
        public string? ErrorMessage { get; set; }
        public int RecordsRead { get; set; }
        public int RecordsInserted { get; set; }
        public int RecordsUpdated { get; set; }
        public int RecordsFailed { get; set; }
        public int ConflictsResolved { get; set; }
        public TimeSpan Duration { get; set; }
        public string? SloTier { get; set; }
        public SyncReconciliation? Reconciliation { get; set; }
    }

    /// <summary>Reconciliation summary from a sync run.</summary>
    public class SyncReconciliation
    {
        public int SourceRows { get; set; }
        public int DestRows { get; set; }
        public int Rejects { get; set; }
        public int Conflicts { get; set; }
        public string? MappingQualityBand { get; set; }
    }
}
