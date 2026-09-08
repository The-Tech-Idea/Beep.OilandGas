using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.BeepSync;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor.Importing.Factories;
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
        private SyncReconciliationReport? _lastReport;

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
            SyncMode mode = SyncMode.FullRefresh,
            string direction = "OneWay",
            List<SyncFieldMapping>? fieldMappings = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(entityName);
            ArgumentException.ThrowIfNullOrWhiteSpace(sourceDataSourceName);
            ArgumentException.ThrowIfNullOrWhiteSpace(destDataSourceName);
            if (!Enum.IsDefined(mode) || direction != "OneWay")
                throw new ArgumentException("Only defined one-way sync modes are supported by this adapter.");
            var schema = new DataSyncSchema
            {
                Id = $"ppdm39-{entityName.ToLowerInvariant()}",
                EntityName = entityName,
                SourceEntityName = entityName,
                DestinationEntityName = entityName,
                SourceDataSourceName = sourceDataSourceName,
                DestinationDataSourceName = destDataSourceName,
                SyncType = mode == SyncMode.FullRefresh ? "Full" : mode.ToString(),
                SyncDirection = direction,
                ConflictResolutionStrategy = "DestinationWins",
                BatchSize = 500
            };

            if (fieldMappings?.Count > 0)
            {
                if (fieldMappings.Any(m => string.IsNullOrWhiteSpace(m.SourceField) || string.IsNullOrWhiteSpace(m.DestField)))
                    throw new ArgumentException("Field mappings require source and destination names.");
                foreach (var mapping in fieldMappings)
                    schema.MappedFields.Add(new FieldSyncData { SourceField = mapping.SourceField, DestinationField = mapping.DestField });
                var keys = fieldMappings.Where(m => m.IsKey).ToList();
                var watermarks = fieldMappings.Where(m => m.IsWatermark).ToList();
                if (keys.Count > 1 || watermarks.Count > 1)
                    throw new ArgumentException("The sync schema supports only one key and one watermark field.");
                if (mode == SyncMode.Incremental && watermarks.Count != 1)
                    throw new ArgumentException("Incremental sync requires an explicit watermark mapping.");
                schema.SourceKeyField = keys.FirstOrDefault()?.SourceField;
                schema.DestinationKeyField = keys.FirstOrDefault()?.DestField;
                schema.SourceSyncDataField = watermarks.FirstOrDefault()?.SourceField ?? schema.SourceKeyField;
                schema.DestinationSyncDataField = mode == SyncMode.Upsert ? schema.DestinationKeyField : watermarks.FirstOrDefault()?.DestField ?? schema.DestinationKeyField;
            }

            if (_syncManager.SyncSchemas.Any(s => s.Id == schema.Id))
                _syncManager.UpdateSyncSchema(schema);
            else
                _syncManager.AddSyncSchema(schema);
            _logger.LogInformation("Created sync schema: {SchemaId} ({Entity})", schema.Id, entityName);

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
            IProgress<PassedArgs>? progress = null,
            CancellationToken token = default)
        {
            _logger.LogInformation("Starting entity sync: {SchemaId}", schemaId);

            try
            {
                token.ThrowIfCancellationRequested();
                _lastReport = null;
                // Use file-based stores for error/history tracking
                var errorStore = LocalStoreFactory.CreateErrorStore(_editor);
                var historyStore = LocalStoreFactory.CreateHistoryStore(_editor);

                token.ThrowIfCancellationRequested();
                var schema = _syncManager.SyncSchemas.SingleOrDefault(s => s.Id == schemaId)
                    ?? throw new InvalidOperationException("Sync schema was not found.");
                if (string.IsNullOrWhiteSpace(schema.SourceKeyField) || string.IsNullOrWhiteSpace(schema.DestinationKeyField) ||
                    string.IsNullOrWhiteSpace(schema.SourceSyncDataField) || string.IsNullOrWhiteSpace(schema.DestinationSyncDataField))
                    throw new InvalidOperationException("Configure the schema's key and sync field mappings before execution.");
                // Early validation failures in the engine may leave the previous report in place.
                schema.LastReconciliationReport = null;
                var timer = System.Diagnostics.Stopwatch.StartNew();
                var outcome = await _syncManager.SyncDataAsync(schema, token, progress, errorStore, historyStore);
                token.ThrowIfCancellationRequested();
                var report = schema.LastReconciliationReport;
                _lastReport = report;
                var success = outcome != null && outcome.Flag == Errors.Ok &&
                    !(outcome.Errors?.Any() ?? false) && schema.SyncStatus == "Success" &&
                    report?.RunAbortedByThreshold != true;

                return new SyncResult
                {
                    Success = success,
                    ErrorMessage = success ? null : outcome?.Message ?? schema.SyncStatusMessage ?? "Sync did not report success.",
                    SchemaId = schemaId,
                    SchemaName = schema.EntityName,
                    RecordsRead = report?.SourceRowsScanned ?? 0,
                    RecordsInserted = report?.DestRowsInserted ?? 0,
                    RecordsUpdated = report?.DestRowsUpdated ?? 0,
                    RecordsFailed = report?.RejectCount ?? 0,
                    Duration = timer.Elapsed,
                    Reconciliation = report != null ? new SyncReconciliation
                    {
                        SourceRows = report.SourceRowsScanned,
                        DestRows = report.DestRowsWritten,
                        Rejects = report.RejectCount,
                        Conflicts = report.ConflictCount,
                        MappingQualityBand = report.MappingQualityBand
                    } : null
                };
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                throw;
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
            IProgress<PassedArgs>? progress = null,
            CancellationToken token = default)
        {
            var results = new List<SyncResult>();

            var schemas = _syncManager.SyncSchemas.ToList();
            foreach (var schema in schemas)
            {
                token.ThrowIfCancellationRequested();
                results.Add(await SyncEntityAsync(schema.Id, progress, token));
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
                mode: SyncMode.FullRefresh);

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
            return _syncManager.SyncSchemas.ToList();
        }

        /// <summary>
        /// Returns the reconciliation report from the last sync run.
        /// </summary>
        public SyncReconciliationReport? GetLastReconciliation()
        {
            return _lastReport;
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
