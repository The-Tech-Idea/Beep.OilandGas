using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Services;
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
                    ErrorMessage = ex.Message
                });
            }
        }

        /// <summary>
        /// Get a single entity by ID
        /// </summary>
        [HttpGet("{tableName}/{id}")]
        public async Task<ActionResult<EntityResult<object>>> GetEntity(string tableName, string id, [FromQuery] string? connectionName = null)
        {
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
                    ErrorMessage = ex.Message
                });
            }
        }

        /// <summary>
        /// Insert a new typed entity. Request body is the entity JSON (e.g. WELL, AREA, etc.).
        /// </summary>
        [HttpPost("{tableName}/insert")]
        public async Task<ActionResult<EntityResult<object>>> InsertEntity(string tableName, [FromBody] JsonElement body, [FromQuery] string userId = "SYSTEM", [FromQuery] string? connectionName = null)
        {
            try
            {
                var entityType = _dataService.GetEntityTypeByTableName(tableName);
                if (entityType == null)
                    return BadRequest(new EntityResult<object> { Success = false, ErrorMessage = $"Entity type not found for table: {tableName}" });

                var entity = JsonSerializer.Deserialize(body.GetRawText(), entityType);
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
                return BadRequest(new EntityResult<object> { Success = false, ErrorMessage = $"Invalid JSON: {ex.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting entity into table {TableName}", tableName);
                return StatusCode(500, new EntityResult<object>
                {
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
        }

        /// <summary>
        /// Update an existing typed entity. Request body is the entity JSON.
        /// </summary>
        [HttpPut("{tableName}/{id}")]
        public async Task<ActionResult<EntityResult<object>>> UpdateEntity(string tableName, string id, [FromBody] JsonElement body, [FromQuery] string userId = "SYSTEM", [FromQuery] string? connectionName = null)
        {
            try
            {
                var entityType = _dataService.GetEntityTypeByTableName(tableName);
                if (entityType == null)
                    return BadRequest(new EntityResult<object> { Success = false, ErrorMessage = $"Entity type not found for table: {tableName}" });

                var entity = JsonSerializer.Deserialize(body.GetRawText(), entityType);
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
                return BadRequest(new EntityResult<object> { Success = false, ErrorMessage = $"Invalid JSON: {ex.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating entity {Id} in table {TableName}", id, tableName);
                return StatusCode(500, new EntityResult<object>
                {
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
        }

        /// <summary>
        /// Delete an entity
        /// </summary>
        [HttpDelete("{tableName}/{id}")]
        public async Task<ActionResult<OperationResult>> DeleteEntity(string tableName, string id, [FromQuery] string userId = "SYSTEM", [FromQuery] string? connectionName = null)
        {
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
                    ErrorMessage = ex.Message
                });
            }
        }
    }
}
