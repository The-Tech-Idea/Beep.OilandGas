using System;
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
using TheTechIdeaWeb.Diagnostics;

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
        private readonly IFailureReporter _failures;

        public PPDM39ImportExportController(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<PPDM39ImportExportController> logger,
            ILoggerFactory loggerFactory,
            IProgressTrackingService progressTracking,
            IBackgroundOperationQueue queue,
            IFailureReporter failures)
        {
            _failures = failures ?? throw new ArgumentNullException(nameof(failures));
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
            [FromQuery] string connectionName = "PPDM39",
            [FromQuery] bool validateForeignKeys = true)
        {
            var actor = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(connectionName))
                return BadRequest(new { error = "Table and connection names are required." });
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
            try
            {
                var job = new CsvImportJob(operationId, entityType.Name, connectionName, actor, validateForeignKeys, content.ToArray());
                if (!_queue.TryEnqueue<CsvImportJobRunner, CsvImportJob>(CsvImportJob.QueueKey(operationId), job,
                    static (runner, request, token) => runner.RunAsync(request, token)))
                {
                    _progressTracking.CompleteOperation(operationId, false, errorMessage: "Import worker is full or stopping.");
                    return StatusCode(503, new OperationStartResponse { OperationId = operationId, Message = "Import worker is unavailable." });
                }
                return Ok(new OperationStartResponse { OperationId = operationId, Message = "Import queued" });
            }
            // Whatever stops the queueing, the tracked operation is closed as not started before the exception goes on to the
            // API's handler, which reports it and answers with its reference (OILGAS-CATCH-01: answered 500 with no reference
            // and "see server logs").
            catch (Exception)
            {
                _progressTracking.CompleteOperation(operationId, false, errorMessage: "Import could not be queued.");
                throw;
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
            connectionName ??= request?.ConnectionName ?? _editor.ConfigEditor?.DataConnections?.FirstOrDefault()?.ConnectionName ?? "PPDM39";

            // Get entity type — asked before an operation is started, so an unknown table leaves none open.
            var assembly = typeof(IPPDMEntity).Assembly;
            var entityType = assembly.GetTypes()
                .FirstOrDefault(t => typeof(IPPDMEntity).IsAssignableFrom(t) &&
                                    !t.IsInterface && !t.IsAbstract &&
                                    t.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));

            if (entityType == null)
            {
                return BadRequest(new { error = $"Entity type not found for table: {tableName}" });
            }

            operationId ??= _progressTracking?.StartOperation("ExportCsv", $"Exporting {tableName} to CSV");

            _logger.LogInformation("Starting CSV export for table {TableName} on connection {ConnectionName} (OperationId: {OperationId})",
                tableName, connectionName, operationId);

            var repository = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, tableName, _loggerFactory.CreateLogger<PPDMGenericRepository>());

            var filters = request?.Filters ?? new System.Collections.Generic.List<AppFilter>();
            // The file name is the entity type's own, never the route's text.
            var tempFilePath = Path.Combine(Path.GetTempPath(), $"export_{Guid.NewGuid():N}_{entityType.Name}.csv");

            try
            {
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
                return File(fileBytes, "text/csv", $"{tableName}_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
            }
            // Whatever ends the export, its tracked operation is closed as failed before the exception goes on to the API's
            // handler, which reports it and answers with its reference (OILGAS-CATCH-01: answered 500 with no reference).
            catch (Exception)
            {
                if (!string.IsNullOrEmpty(operationId))
                {
                    _progressTracking?.CompleteOperation(operationId, false, errorMessage: "Export failed.");
                }
                throw;
            }
            finally
            {
                DeleteTemporaryFile(tempFilePath);
            }
        }

        // The export has already answered; a temporary copy left behind costs disk, not the result.
        private void DeleteTemporaryFile(string tempFilePath)
        {
            try
            {
                if (System.IO.File.Exists(tempFilePath)) System.IO.File.Delete(tempFilePath);
            }
            catch (Exception cleanup) when (cleanup is IOException or UnauthorizedAccessException)
            {
                _failures.ReportHandled(cleanup, "deleting a CSV export's temporary file",
                    "the export's answer stands; the temporary file stays in the temp folder until removed", FailureSeverity.Degraded);
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
            var progress = _progressTracking?.GetProgress(operationId);
            if (progress == null)
            {
                    return NotFound(new { error = "Operation not found." });
            }
            return Ok(progress);
        }
    }
}
