using System;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs.DataManagement;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ApiService.Controllers.PPDM39
{
    /// <summary>
    /// API controller for generic PPDM39 data operations (CRUD)
    /// </summary>
    [ApiController]
    [Route("api/ppdm39/data")]
    public class PPDM39DataController : ControllerBase
    {
        private readonly PPDM39DataService _dataService;
        private readonly ILogger<PPDM39DataController> _logger;
        private readonly IProgressTrackingService? _progressTracking;

        public PPDM39DataController(
            PPDM39DataService dataService,
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
        public async Task<ActionResult<GetEntitiesResponse>> GetEntities(string tableName, [FromBody] GetEntitiesRequest request)
        {
            try
            {
                _logger.LogInformation("GET entities from table {TableName} on connection {ConnectionName}", 
                    tableName, request?.ConnectionName ?? "default");

                var filters = request?.Filters ?? new System.Collections.Generic.List<AppFilter>();
                var result = await _dataService.GetEntitiesAsync(tableName, filters, request?.ConnectionName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entities from table {TableName}", tableName);
                return StatusCode(500, new GetEntitiesResponse
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
        public async Task<ActionResult<GenericEntityResponse>> GetEntity(string tableName, string id, [FromQuery] string? connectionName = null)
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
                return StatusCode(500, new GenericEntityResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
        }

        /// <summary>
        /// Insert a new entity
        /// </summary>
        [HttpPost("{tableName}/insert")]
        public async Task<ActionResult<GenericEntityResponse>> InsertEntity(string tableName, [FromBody] GenericEntityRequest request, [FromQuery] string userId = "SYSTEM")
        {
            try
            {
                if (request == null || request.EntityData == null)
                {
                    return BadRequest(new GenericEntityResponse { Success = false, ErrorMessage = "Entity data is required" });
                }

                _logger.LogInformation("INSERT entity into table {TableName} on connection {ConnectionName}", 
                    tableName, request.ConnectionName ?? "default");

                var result = await _dataService.InsertEntityAsync(
                    tableName, 
                    request.EntityData, 
                    userId, 
                    request.ConnectionName);
                
                if (!result.Success)
                    return BadRequest(result);
                    
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting entity into table {TableName}", tableName);
                return StatusCode(500, new GenericEntityResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
        }

        /// <summary>
        /// Update an existing entity
        /// </summary>
        [HttpPut("{tableName}/{id}")]
        public async Task<ActionResult<GenericEntityResponse>> UpdateEntity(string tableName, string id, [FromBody] GenericEntityRequest request, [FromQuery] string userId = "SYSTEM")
        {
            try
            {
                if (request == null || request.EntityData == null)
                {
                    return BadRequest(new GenericEntityResponse { Success = false, ErrorMessage = "Entity data is required" });
                }

                _logger.LogInformation("UPDATE entity {Id} in table {TableName} on connection {ConnectionName}", 
                    id, tableName, request.ConnectionName ?? "default");

                var result = await _dataService.UpdateEntityAsync(
                    tableName, 
                    id, 
                    request.EntityData, 
                    userId, 
                    request.ConnectionName);
                
                if (!result.Success)
                    return BadRequest(result);
                    
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating entity {Id} in table {TableName}", id, tableName);
                return StatusCode(500, new GenericEntityResponse
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
        public async Task<ActionResult<GenericEntityResponse>> DeleteEntity(string tableName, string id, [FromQuery] string userId = "SYSTEM", [FromQuery] string? connectionName = null)
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
                return StatusCode(500, new GenericEntityResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
        }
    }
}
