using System;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers
{
    /// <summary>
    /// Data synchronization API powered by BeepDM BeepSyncManager.
    /// Supports full/incremental sync with conflict resolution, reconciliation,
    /// and SLO monitoring for PPDM 3.9 entities.
    ///
    /// Phase 5A of BeepDM framework integration.
    /// </summary>
    [ApiController]
    [Route("api/sync")]
    [Authorize]
    public class SyncController : ControllerBase
    {
        private readonly BeepSyncService _syncService;
        private readonly ILogger<SyncController> _logger;

        public SyncController(BeepSyncService syncService, ILogger<SyncController> logger)
        {
            _syncService = syncService;
            _logger = logger;
        }

        /// <summary>Lists all configured sync schemas.</summary>
        [HttpGet("schemas")]
        public IActionResult GetSchemas()
        {
            var schemas = _syncService.GetSchemas();
            return Ok(new { count = schemas.Count, schemas });
        }

        /// <summary>Synchronizes a single entity schema.</summary>
        [HttpPost("run/{schemaId}")]
        public async Task<IActionResult> SyncEntity(
            string schemaId,
            CancellationToken token)
        {
            _logger.LogInformation("Sync requested: {SchemaId}", schemaId);

            var result = await _syncService.SyncEntityAsync(schemaId, token: token);

            if (result.Success)
            {
                return Ok(new
                {
                    message = "Sync completed",
                    schemaId = result.SchemaId,
                    recordsRead = result.RecordsRead,
                    recordsInserted = result.RecordsInserted,
                    recordsUpdated = result.RecordsUpdated,
                    recordsFailed = result.RecordsFailed,
                    conflictsResolved = result.ConflictsResolved,
                    duration = result.Duration.ToString(),
                    sloTier = result.SloTier,
                    reconciliation = result.Reconciliation
                });
            }

            return StatusCode(500, new
            {
                error = result.ErrorMessage ?? "Sync failed",
                schemaId = result.SchemaId
            });
        }

        /// <summary>Synchronizes all enabled schemas sequentially.</summary>
        [HttpPost("run-all")]
        public async Task<IActionResult> SyncAll(CancellationToken token)
        {
            _logger.LogInformation("Sync-all requested");

            var results = await _syncService.SyncAllAsync(token: token);

            return Ok(new
            {
                message = "Sync-all completed",
                totalSchemas = results.Count,
                succeeded = results.FindAll(r => r.Success).Count,
                failed = results.FindAll(r => !r.Success).Count,
                results
            });
        }

        /// <summary>Returns the reconciliation report from the last sync run.</summary>
        [HttpGet("reconciliation")]
        public IActionResult GetReconciliation()
        {
            var report = _syncService.GetLastReconciliation();

            if (report == null)
                return NotFound(new { message = "No reconciliation report available. Run a sync first." });

            return Ok(report);
        }

        /// <summary>Initializes the standard PPDM39 sync schemas.</summary>
        [HttpPost("init-schemas")]
        public IActionResult InitializeSchemas(
            [FromQuery] string sourceDataSource = "PPDM39_SOURCE",
            [FromQuery] string destDataSource = "PPDM39")
        {
            _syncService.InitializePpdmSyncSchemas(sourceDataSource, destDataSource);
            var schemas = _syncService.GetSchemas();
            return Ok(new
            {
                message = "PPDM39 sync schemas initialized",
                count = schemas.Count,
                schemas = schemas
            });
        }

        /// <summary>Saves sync schemas to persistent storage.</summary>
        [HttpPost("save-schemas")]
        public async Task<IActionResult> SaveSchemas()
        {
            await _syncService.SaveSchemasAsync();
            return Ok(new { message = "Sync schemas saved" });
        }

        /// <summary>Loads sync schemas from persistent storage.</summary>
        [HttpPost("load-schemas")]
        public async Task<IActionResult> LoadSchemas()
        {
            await _syncService.LoadSchemasAsync();
            var schemas = _syncService.GetSchemas();
            return Ok(new { message = "Sync schemas loaded", count = schemas.Count });
        }
    }
}
