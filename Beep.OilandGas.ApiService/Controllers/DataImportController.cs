using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers
{
    /// <summary>
    /// Server-side ETL data import API powered by BeepDM DataImportManager.
    /// Supports CSV upload with quality rule validation, error replay, and profiling.
    ///
    /// Phase 4A of BeepDM framework integration.
    /// </summary>
    [ApiController]
    [Route("api/data-import")]
    [Authorize]
    public class DataImportController : ControllerBase
    {
        private readonly DataImportService _importService;
        private readonly ILogger<DataImportController> _logger;

        public DataImportController(DataImportService importService, ILogger<DataImportController> logger)
        {
            _importService = importService;
            _logger = logger;
        }

        /// <summary>
        /// Uploads a CSV file and imports it into the specified PPDM table.
        /// </summary>
        [HttpPost("csv/{tableName}")]
        [RequestSizeLimit(100_000_000)] // 100 MB max
        public async Task<IActionResult> ImportCsv(
            string tableName,
            IFormFile file,
            [FromQuery] int? batchSize = null,
            CancellationToken token = default)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file uploaded." });

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { error = "Only .csv files are accepted." });

            // Save uploaded file to a temp location
            var tempDir = Path.Combine(Path.GetTempPath(), "BeepDataImport");
            Directory.CreateDirectory(tempDir);
            var tempPath = Path.Combine(tempDir, $"{Guid.NewGuid()}_{file.FileName}");

            try
            {
                await using (var stream = new FileStream(tempPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream, token);
                }

                var options = new DataImportOptions();
                if (batchSize.HasValue) options.BatchSize = batchSize.Value;

                var progress = new Progress<TheTechIdea.Beep.PassedArgs>(p =>
                    _logger.LogDebug("Import progress: {Message}", p.Messege));

                var result = await _importService.ImportCsvAsync(tempPath, tableName, options, progress, token);

                if (result.Success)
                {
                    return Ok(new
                    {
                        message = "Import completed successfully",
                        contextKey = result.ContextKey,
                        recordsRead = result.RecordsRead,
                        recordsInserted = result.RecordsInserted,
                        recordsFailed = result.RecordsFailed,
                        recordsSkipped = result.RecordsSkipped,
                        duration = result.Duration.ToString(),
                        quality = result.QualityMetrics
                    });
                }

                return StatusCode(500, new
                {
                    error = result.ErrorMessage ?? "Import failed",
                    contextKey = result.ContextKey,
                    recordsFailed = result.RecordsFailed
                });
            }
            finally
            {
                // Clean up temp file
                try { if (System.IO.File.Exists(tempPath)) System.IO.File.Delete(tempPath); }
                catch (Exception ex) { _logger.LogWarning(ex, "Failed to clean up temp file: {Path}", tempPath); }
            }
        }

        /// <summary>
        /// Replays previously failed records for a given import context.
        /// </summary>
        [HttpPost("replay/{contextKey}")]
        public async Task<IActionResult> ReplayFailed(string contextKey, CancellationToken token)
        {
            var result = await _importService.ReplayFailedAsync(contextKey, token: token);

            if (result.Success)
                return Ok(new { message = "Replay completed", contextKey });

            return StatusCode(500, new { error = result.ErrorMessage ?? "Replay failed" });
        }

        /// <summary>
        /// Profiles a PPDM table, returning per-field statistics.
        /// </summary>
        [HttpGet("profile/{tableName}")]
        public async Task<IActionResult> ProfileTable(
            string tableName,
            [FromQuery] int sampleSize = 1000,
            CancellationToken token = default)
        {
            try
            {
                var profile = await _importService.ProfileTableAsync(tableName, sampleSize, token);
                return Ok(new
                {
                    tableName,
                    sampleSize = profile.SampleSize,
                    capturedAt = profile.CapturedAt,
                    fields = profile.Fields
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to profile table {TableName}", tableName);
                return StatusCode(500, new { error = $"Profiling failed: {ex.Message}" });
            }
        }

        /// <summary>
        /// Cancels an in-progress import.
        /// </summary>
        [HttpPost("cancel/{contextKey}")]
        public IActionResult CancelImport(string contextKey)
        {
            _importService.CancelImport(contextKey);
            return Ok(new { message = "Import cancelled", contextKey });
        }
    }
}
