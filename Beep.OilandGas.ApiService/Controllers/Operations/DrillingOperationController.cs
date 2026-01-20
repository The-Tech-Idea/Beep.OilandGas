using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Operations
{
    /// <summary>
    /// API controller for drilling operation management.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DrillingOperationController : ControllerBase
    {
        private readonly IDrillingOperationService _service;
        private readonly ILogger<DrillingOperationController> _logger;

        public DrillingOperationController(IDrillingOperationService service, ILogger<DrillingOperationController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("operations")]
        public async Task<ActionResult<List<DrillingOperation>>> GetDrillingOperations([FromQuery] string? wellUWI = null)
        {
            try
            {
                var result = await _service.GetDrillingOperationsAsync(wellUWI);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drilling operations");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("operations/{operationId}")]
        public async Task<ActionResult<DrillingOperation>> GetDrillingOperation(string operationId)
        {
            try
            {
                var result = await _service.GetDrillingOperationAsync(operationId);
                if (result == null)
                    return NotFound(new { error = $"Drilling operation {operationId} not found" });
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drilling operation {OperationId}", operationId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("operations")]
        public async Task<ActionResult<DrillingOperation>> CreateDrillingOperation([FromBody] CreateDrillingOperation createDto)
        {
            try
            {
                var result = await _service.CreateDrillingOperationAsync(createDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating drilling operation");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPut("operations/{operationId}")]
        public async Task<ActionResult<DrillingOperation>> UpdateDrillingOperation(string operationId, [FromBody] UpdateDrillingOperation updateDto)
        {
            try
            {
                var result = await _service.UpdateDrillingOperationAsync(operationId, updateDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating drilling operation {OperationId}", operationId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("operations/{operationId}/reports")]
        public async Task<ActionResult<List<DrillingReport>>> GetDrillingReports(string operationId)
        {
            try
            {
                var result = await _service.GetDrillingReportsAsync(operationId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drilling reports for operation {OperationId}", operationId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("operations/{operationId}/reports")]
        public async Task<ActionResult<DrillingReport>> CreateDrillingReport(string operationId, [FromBody] CreateDrillingReport createDto)
        {
            try
            {
                var result = await _service.CreateDrillingReportAsync(operationId, createDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating drilling report for operation {OperationId}", operationId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private string GetUserId() => User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";
    }
}

