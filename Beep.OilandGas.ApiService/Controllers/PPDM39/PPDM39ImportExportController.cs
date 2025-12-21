using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Models;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.PPDM39.Core.DTOs;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
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
    [ApiController]
    [Route("api/ppdm39/import-export")]
    public class PPDM39ImportExportController : ControllerBase
    {
        private readonly IDMEEditor _editor;
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
            IProgressTrackingService progressTracking)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _progressTracking = progressTracking;
        }

        /// <summary>
        /// Import data from CSV file
        /// </summary>
        [HttpPost("csv/{tableName}")]
        public async Task<ActionResult<OperationStartResponse>> ImportCsv(
            string tableName,
            IFormFile file,
            [FromQuery] string? operationId = null,
            [FromQuery] string userId = "SYSTEM",
            [FromQuery] string? connectionName = null,
            [FromQuery] bool validateForeignKeys = true)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new OperationStartResponse { OperationId = "", Message = "No file uploaded" });
                }

                operationId ??= _progressTracking?.StartOperation("ImportCsv", $"Importing {tableName} from CSV file: {file.FileName}");

                connectionName ??= _editor.ConfigEditor?.DataConnections?.FirstOrDefault()?.ConnectionName ?? "PPDM39";

                _logger.LogInformation("Starting CSV import for table {TableName} from file {FileName} (OperationId: {OperationId})", 
                    tableName, file.FileName, operationId);

                // Save uploaded file temporarily
                var tempFilePath = Path.Combine(Path.GetTempPath(), $"import_{Guid.NewGuid()}_{file.FileName}");
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Start async import operation
                _ = Task.Run(async () =>
                {
                    try
                    {
                        // Get entity type
                        var assembly = typeof(IPPDMEntity).Assembly;
                        var entityType = assembly.GetTypes()
                            .FirstOrDefault(t => typeof(IPPDMEntity).IsAssignableFrom(t) && 
                                                !t.IsInterface && !t.IsAbstract &&
                                                t.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));

                        if (entityType == null)
                        {
                            _progressTracking?.CompleteOperation(operationId!, false, errorMessage: $"Entity type not found for table: {tableName}");
                            return;
                        }

                        var repository = new PPDMGenericRepository(
                            _editor, _commonColumnHandler, _defaults, _metadata,
                            entityType, connectionName, tableName, _loggerFactory.CreateLogger<PPDMGenericRepository>());

                        // Wrap progress tracking in delegate
                        PPDMGenericRepository.ProgressReportDelegate? progressDelegate = null;
                        if (_progressTracking != null && !string.IsNullOrEmpty(operationId))
                        {
                            progressDelegate = (opId, percentage, message, itemsProcessed, totalItems) =>
                            {
                                _progressTracking.UpdateProgress(opId, percentage, message, itemsProcessed, totalItems);
                            };
                        }

                        var result = await repository.ImportFromCsvAsync(
                            tempFilePath,
                            userId,
                            columnMapping: null,
                            skipHeaderRow: true,
                            validateForeignKeys: validateForeignKeys,
                            progressDelegate,
                            operationId);

                        // Clean up temp file
                        if (System.IO.File.Exists(tempFilePath))
                        {
                            System.IO.File.Delete(tempFilePath);
                        }

                        if (result.ErrorCount == 0)
                        {
                            _progressTracking?.CompleteOperation(operationId!, true, 
                                $"Import completed: {result.SuccessCount} rows imported successfully");
                        }
                        else
                        {
                            _progressTracking?.CompleteOperation(operationId!, false,
                                errorMessage: $"Import completed with {result.ErrorCount} errors. {result.SuccessCount} rows imported.");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during CSV import for table {TableName}", tableName);
                        
                        // Clean up temp file
                        if (System.IO.File.Exists(tempFilePath))
                        {
                            try { System.IO.File.Delete(tempFilePath); } catch { }
                        }
                        
                        _progressTracking?.CompleteOperation(operationId!, false, errorMessage: ex.Message);
                    }
                });

                return Ok(new OperationStartResponse { OperationId = operationId, Message = "Import started" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting CSV import for table {TableName}", tableName);
                return StatusCode(500, new OperationStartResponse 
                { 
                    OperationId = "", 
                    Message = $"Error starting import: {ex.Message}" 
                });
            }
        }

        /// <summary>
        /// Export data to CSV file
        /// </summary>
        [HttpPost("csv/{tableName}/export")]
        public async Task<IActionResult> ExportCsv(
            string tableName,
            [FromBody] ExportRequest? request = null,
            [FromQuery] string? connectionName = null,
            [FromQuery] string? operationId = null)
        {
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
                    _progressTracking?.CompleteOperation(operationId, false, errorMessage: ex.Message);
                }
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get import/export operation progress
        /// </summary>
        [HttpGet("progress/{operationId}")]
        public ActionResult<ProgressUpdate> GetProgress(string operationId)
        {
            try
            {
                var progress = _progressTracking?.GetProgress(operationId);
                if (progress == null)
                {
                    return NotFound(new { error = "Operation not found" });
                }
                return Ok(progress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting progress for operation {OperationId}", operationId);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
