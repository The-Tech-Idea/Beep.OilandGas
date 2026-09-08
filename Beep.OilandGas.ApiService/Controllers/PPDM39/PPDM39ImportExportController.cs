using System;
using System.Security.Claims;
using Beep.OilandGas.ApiService.Services;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ApiService.Controllers.PPDM39
{
    /// <summary>
    /// API controller for PPDM39 import/export operations
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/ppdm39/import-export")]
    public class PPDM39ImportExportController : ControllerBase
    {
        private readonly IDMEEditor _editor;
        private readonly IBackgroundOperationQueue _queue;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<PPDM39ImportExportController> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IProgressTrackingService? _progressTracking;

        public PPDM39ImportExportController(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<PPDM39ImportExportController> logger,
            ILoggerFactory loggerFactory,
            IProgressTrackingService progressTracking,
            IBackgroundOperationQueue queue)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _progressTracking = progressTracking;
            _queue = queue;
        }

        /// <summary>
        /// Import data from CSV file
        /// </summary>
        [HttpPost("csv/{tableName}")]
        [RequestSizeLimit(CsvImportJob.MaxUploadBytes + 65536)]
        [RequestFormLimits(MultipartBodyLengthLimit = CsvImportJob.MaxUploadBytes)]
        public async Task<ActionResult<OperationStartResponse>> ImportCsv(
            string tableName,
            IFormFile file,
            [FromQuery] string? operationId = null,
            [FromQuery] string userId = "SYSTEM",
            [FromQuery] string connectionName = "PPDM39",
            [FromQuery] bool validateForeignKeys = true)
        {
            if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(connectionName))
                return BadRequest(new { error = "Table and connection names are required." });
            var actor = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrWhiteSpace(actor)) return Unauthorized();
            if (!string.IsNullOrEmpty(operationId))
                return BadRequest(new { error = "Import operation IDs are assigned by the server." });
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file uploaded." });
            if (file.Length > CsvImportJob.MaxUploadBytes)
                return StatusCode(413, new { error = "CSV uploads are limited to 2 MiB." });
            var entityType = typeof(IPPDMEntity).Assembly.GetTypes().FirstOrDefault(t =>
                typeof(IPPDMEntity).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract &&
                t.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));
            if (entityType == null) return BadRequest(new { error = "Unknown PPDM table." });
            try
            {
                using var input = file.OpenReadStream();
                using var content = new MemoryStream();
                var buffer = new byte[81920];
                int read;
                while ((read = await input.ReadAsync(buffer, HttpContext.RequestAborted)) > 0)
                {
                    if (content.Length + read > CsvImportJob.MaxUploadBytes)
                        return StatusCode(413, new { error = "CSV uploads are limited to 2 MiB." });
                    await content.WriteAsync(buffer.AsMemory(0, read), HttpContext.RequestAborted);
                }
                if (content.Length == 0) return BadRequest(new { error = "No file uploaded." });
                operationId = _progressTracking!.StartOperation("ImportCsv", $"Importing {entityType.Name} from CSV");
                var job = new CsvImportJob(operationId, entityType.Name, connectionName, actor, validateForeignKeys, content.ToArray());
                if (!_queue.TryEnqueue<CsvImportJobRunner, CsvImportJob>(CsvImportJob.QueueKey(operationId), job,
                    static (runner, request, token) => runner.RunAsync(request, token)))
                {
                    _progressTracking.CompleteOperation(operationId, false, errorMessage: "Import worker is full or stopping.");
                    return StatusCode(503, new OperationStartResponse { OperationId = operationId, Message = "Import worker is unavailable." });
                }
                return Ok(new OperationStartResponse { OperationId = operationId, Message = "Import queued" });
            }
            catch (OperationCanceledException) when (HttpContext.RequestAborted.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting CSV import for {TableName}", tableName);
                if (operationId != null) _progressTracking?.CompleteOperation(operationId, false, errorMessage: "Import could not be queued.");
                return StatusCode(500, new OperationStartResponse { Message = "Error starting import. See server logs." });
            }
        }

        /// <summary>
        /// Export data to CSV file
        /// </summary>
        [HttpPost("csv/{tableName}/export")]
        public async Task<IActionResult> ExportCsv(
            string tableName,
            [FromBody] ExportRequest? request = null,
            [FromQuery] string connectionName = "PPDM39",
            [FromQuery] string? operationId = null)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return BadRequest(new { error = "Table name is required." });
            try
            {
                connectionName ??= request?.ConnectionName ?? _editor.ConfigEditor?.DataConnections?.FirstOrDefault()?.ConnectionName ?? "PPDM39";
                operationId ??= _progressTracking?.StartOperation("ExportCsv", $"Exporting {tableName} to CSV");

                _logger.LogInformation("Starting CSV export for table {TableName} on connection {ConnectionName} (OperationId: {OperationId})", 
                    tableName, connectionName, operationId);

                // Get entity type
                var assembly = typeof(IPPDMEntity).Assembly;
                var entityType = assembly.GetTypes()
                    .FirstOrDefault(t => typeof(IPPDMEntity).IsAssignableFrom(t) && 
                                        !t.IsInterface && !t.IsAbstract &&
                                        t.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));

                if (entityType == null)
                {
                    return BadRequest(new { error = $"Entity type not found for table: {tableName}" });
                }

                var repository = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, connectionName, tableName, _loggerFactory.CreateLogger<PPDMGenericRepository>());

                var filters = request?.Filters ?? new System.Collections.Generic.List<AppFilter>();
                var tempFilePath = Path.Combine(Path.GetTempPath(), $"export_{Guid.NewGuid()}_{tableName}.csv");

                // Wrap progress tracking in delegate
                PPDMGenericRepository.ProgressReportDelegate? progressDelegate = null;
                if (_progressTracking != null && !string.IsNullOrEmpty(operationId))
                {
                    progressDelegate = (opId, percentage, message, itemsProcessed, totalItems) =>
                    {
                        _progressTracking.UpdateProgress(opId, percentage, message, itemsProcessed, totalItems);
                    };
                }

                // Export to temp file
                var exportedCount = await repository.ExportToCsvAsync(
                    tempFilePath,
                    filters,
                    request?.IncludeHeaders ?? true,
                    progressDelegate,
                    operationId);

                _progressTracking?.CompleteOperation(operationId!, true, 
                    $"Export completed: {exportedCount} entities exported");

                // Return file
                var fileBytes = await System.IO.File.ReadAllBytesAsync(tempFilePath);
                System.IO.File.Delete(tempFilePath); // Clean up

                return File(fileBytes, "text/csv", $"{tableName}_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting CSV for table {TableName}", tableName);
                if (!string.IsNullOrEmpty(operationId))
                {
                    _progressTracking?.CompleteOperation(operationId, false, errorMessage: "Export failed. See server logs for details.");
                }
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Get import/export operation progress
        /// </summary>
        [HttpGet("progress/{operationId}")]
        public ActionResult<ProgressUpdate> GetProgress(string operationId)
        {
            if (string.IsNullOrWhiteSpace(operationId))
                return BadRequest(new { error = "Operation ID is required." });
            try
            {
                var progress = _progressTracking?.GetProgress(operationId);
                if (progress == null)
                {
                        return NotFound(new { error = "Operation not found." });
                }
                return Ok(progress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting progress for operation {OperationId}", operationId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }
    }
}
