using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ApiService.Controllers.PPDM39
{
    /// <summary>
    /// API controller for PPDM39 data operations.
    /// Uses typed entity classes only - no Dictionary or untyped object.
    /// </summary>
    [ApiController]
    [Route("api/ppdm39/data")]
    public class PPDM39DataController : ControllerBase
    {
        private readonly IPPDM39DataService _dataService;
        private readonly ILogger<PPDM39DataController> _logger;
        private readonly IProgressTrackingService? _progressTracking;

        public PPDM39DataController(
            IPPDM39DataService dataService,
            ILogger<PPDM39DataController> logger,
            IProgressTrackingService progressTracking)
        {
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _progressTracking = progressTracking;
        }

        /// <summary>
        /// Get entities from a table with optional filters
        /// </summary>
        [HttpPost("{tableName}")]
        public async Task<ActionResult<EntityListResult<object>>> GetEntities(string tableName, [FromBody] GetEntitiesRequest request)
        {
            if (string.IsNullOrWhiteSpace(tableName)) return BadRequest(new EntityListResult<object> { Success = false, ErrorMessage = "Table name is required." });
            try
            {
                _logger.LogInformation("GET entities from table {TableName} on connection {ConnectionName}",
                    tableName, request?.ConnectionName ?? "default");

                var filters = request?.Filters ?? new List<AppFilter>();
                var result = await _dataService.GetEntitiesAsync(tableName, filters, request?.ConnectionName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entities from table {TableName}", tableName);
                return StatusCode(500, new EntityListResult<object>
                {
                    Success = false,
                    ErrorMessage = "An internal error occurred."
                });
            }
        }

        /// <summary>
        /// Get a single entity by ID
        /// </summary>
        [HttpGet("{tableName}/{id}")]
        public async Task<ActionResult<EntityResult<object>>> GetEntity(string tableName, string id, [FromQuery] string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(tableName)) return BadRequest(new EntityResult<object> { Success = false, ErrorMessage = "Table name is required." });
            if (string.IsNullOrWhiteSpace(id)) return BadRequest(new EntityResult<object> { Success = false, ErrorMessage = "Entity ID is required." });
            try
            {
                _logger.LogInformation("GET entity by ID from table {TableName}, ID: {Id}", tableName, id);
                var result = await _dataService.GetEntityByIdAsync(tableName, id, connectionName);

                if (!result.Success)
                    return NotFound(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entity {Id} from table {TableName}", id, tableName);
                return StatusCode(500, new EntityResult<object>
                {
                    Success = false,
                    ErrorMessage = "An internal error occurred."
                });
            }
        }

        /// <summary>
        /// Insert a new typed entity. Request body is the entity JSON (e.g. WELL, AREA, etc.).
        /// </summary>
        [HttpPost("{tableName}/insert")]
        public async Task<ActionResult<EntityResult<object>>> InsertEntity(string tableName, [FromBody] JsonElement body, [FromQuery] string userId = "SYSTEM", [FromQuery] string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(tableName)) return BadRequest(new EntityResult<object> { Success = false, ErrorMessage = "Table name is required." });
            try
            {
                var entity = JsonSerializer.Deserialize<Dictionary<string, object>>(body.GetRawText());
                if (entity == null)
                    return BadRequest(new EntityResult<object> { Success = false, ErrorMessage = "Failed to deserialize entity" });

                _logger.LogInformation("INSERT entity into table {TableName} on connection {ConnectionName}",
                    tableName, connectionName ?? "default");

                var result = await _dataService.InsertEntityAsync(tableName, entity, userId, connectionName);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Invalid JSON for insert into table {TableName}", tableName);
                return BadRequest(new EntityResult<object> { Success = false, ErrorMessage = "Invalid JSON format." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting entity into table {TableName}", tableName);
                return StatusCode(500, new EntityResult<object>
                {
                    Success = false,
                    ErrorMessage = "An internal error occurred."
                });
            }
        }

        /// <summary>
        /// Update an existing typed entity. Request body is the entity JSON.
        /// </summary>
        [HttpPut("{tableName}/{id}")]
        public async Task<ActionResult<EntityResult<object>>> UpdateEntity(string tableName, string id, [FromBody] JsonElement body, [FromQuery] string userId = "SYSTEM", [FromQuery] string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(tableName)) return BadRequest(new EntityResult<object> { Success = false, ErrorMessage = "Table name is required." });
            if (string.IsNullOrWhiteSpace(id)) return BadRequest(new EntityResult<object> { Success = false, ErrorMessage = "Entity ID is required." });
            try
            {
                var entity = JsonSerializer.Deserialize<Dictionary<string, object>>(body.GetRawText());
                if (entity == null)
                    return BadRequest(new EntityResult<object> { Success = false, ErrorMessage = "Failed to deserialize entity" });

                _logger.LogInformation("UPDATE entity {Id} in table {TableName} on connection {ConnectionName}",
                    id, tableName, connectionName ?? "default");

                var result = await _dataService.UpdateEntityAsync(tableName, id, entity, userId, connectionName);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Invalid JSON for update in table {TableName}", tableName);
                return BadRequest(new EntityResult<object> { Success = false, ErrorMessage = "Invalid JSON format." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating entity {Id} in table {TableName}", id, tableName);
                return StatusCode(500, new EntityResult<object>
                {
                    Success = false,
                    ErrorMessage = "An internal error occurred."
                });
            }
        }

        /// <summary>
        /// Delete an entity
        /// </summary>
        [HttpDelete("{tableName}/{id}")]
        public async Task<ActionResult<OperationResult>> DeleteEntity(string tableName, string id, [FromQuery] string userId = "SYSTEM", [FromQuery] string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(tableName)) return BadRequest(new OperationResult { Success = false, ErrorMessage = "Table name is required." });
            if (string.IsNullOrWhiteSpace(id)) return BadRequest(new OperationResult { Success = false, ErrorMessage = "Entity ID is required." });
            try
            {
                _logger.LogInformation("DELETE entity {Id} from table {TableName} on connection {ConnectionName}",
                    id, tableName, connectionName ?? "default");

                var result = await _dataService.DeleteEntityAsync(tableName, id, userId, connectionName);

                if (!result.Success)
                    return NotFound(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting entity {Id} from table {TableName}", id, tableName);
                return StatusCode(500, new OperationResult
                {
                    Success = false,
                    ErrorMessage = "An internal error occurred."
                });
            }
        }

        /// <summary>
        /// Export all rows from a table as CSV
        /// </summary>
        [HttpGet("{tableName}/export")]
        public async Task<IActionResult> ExportCsv(
            string tableName,
            [FromQuery] string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(tableName)) return BadRequest(new { success = false, message = "Table name is required." });
            try
            {
                _logger.LogInformation("EXPORT CSV from table {TableName}", tableName);

                var result = await _dataService.GetEntitiesAsync(tableName, new List<AppFilter>(), connectionName);
                if (!result.Success)
                    return BadRequest(new { success = false, message = result.ErrorMessage });

                var rows = (result.Entities ?? new List<Dictionary<string, object>>())
                    .ToList();

                if (rows.Count == 0)
                {
                    var empty = Encoding.UTF8.GetBytes(string.Empty);
                    return File(empty, "text/csv", $"{tableName}.csv");
                }

                // Build CSV
                var columns = rows[0].Keys.ToList();
                var sb = new StringBuilder();
                sb.AppendLine(string.Join(",", columns.Select(CsvEscape)));
                foreach (var row in rows)
                {
                    sb.AppendLine(string.Join(",", columns.Select(c =>
                        CsvEscape(row.TryGetValue(c, out var v) ? v?.ToString() : string.Empty))));
                }

                var bytes = Encoding.UTF8.GetBytes(sb.ToString());
                return File(bytes, "text/csv", $"{tableName}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting CSV for table {TableName}", tableName);
                return StatusCode(500, new { success = false, message = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Import rows into a table from a CSV file upload
        /// </summary>
        [HttpPost("{tableName}/import")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportCsv(
            string tableName,
            IFormFile file,
            [FromQuery] string userId = "SYSTEM",
            [FromQuery] string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(tableName)) return BadRequest(new { success = false, message = "Table name is required." });
            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = "No file provided." });

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { success = false, message = "Only CSV files are supported." });

            try
            {
                _logger.LogInformation("IMPORT CSV into table {TableName}, file={File}, size={Size}",
                    tableName, file.FileName, file.Length);

                using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8);
                var headerLine = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(headerLine))
                    return BadRequest(new { success = false, message = "CSV file has no header row." });

                var columns = ParseCsvLine(headerLine);
                int inserted = 0, failed = 0;
                var errors = new List<string>();

                string? line;
                int rowNum = 1;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    rowNum++;
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var values = ParseCsvLine(line);
                    var entity = new Dictionary<string, object>();
                    for (int i = 0; i < columns.Count && i < values.Count; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(columns[i]))
                            entity[columns[i]] = values[i];
                    }

                    if (entity.Count == 0) continue;

                    try
                    {
                        var result = await _dataService.InsertEntityAsync(tableName, entity, userId, connectionName);
                        if (result.Success) inserted++;
                        else { failed++; errors.Add($"Row {rowNum}: {result.ErrorMessage}"); }
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        errors.Add($"Row {rowNum}: Insert failed.");
                    }
                }

                return Ok(new
                {
                    success = true,
                    inserted,
                    failed,
                    errors = errors.Take(20).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing CSV for table {TableName}", tableName);
                return StatusCode(500, new { success = false, message = "An internal error occurred." });
            }
        }

        // ── CSV helpers ───────────────────────────────────────────────────────

        private static Dictionary<string, object?> ToDictionary(object entity)
        {
            if (entity is JsonElement je)
            {
                var d = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                foreach (var p in je.EnumerateObject())
                {
                    d[p.Name] = p.Value.ValueKind switch
                    {
                        JsonValueKind.String  => p.Value.GetString(),
                        JsonValueKind.Number  => p.Value.TryGetDecimal(out var dec) ? dec : (object?)p.Value.GetDouble(),
                        JsonValueKind.True    => true,
                        JsonValueKind.False   => false,
                        JsonValueKind.Null    => null,
                        _                    => p.Value.ToString()
                    };
                }
                return d;
            }
            if (entity is Dictionary<string, object?> dict) return dict;
            var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in entity.GetType().GetProperties())
                result[prop.Name] = prop.GetValue(entity);
            return result;
        }

        private static string CsvEscape(string? value)
        {
            if (value == null) return string.Empty;
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }

        private static List<string> ParseCsvLine(string line)
        {
            var fields = new List<string>();
            var sb = new StringBuilder();
            bool inQuotes = false;
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    { sb.Append('"'); i++; }
                    else inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                { fields.Add(sb.ToString()); sb.Clear(); }
                else sb.Append(c);
            }
            fields.Add(sb.ToString());
            return fields;
        }
    }
}
