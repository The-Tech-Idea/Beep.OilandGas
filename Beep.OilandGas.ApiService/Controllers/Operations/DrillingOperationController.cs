using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Drilling;
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
        public async Task<ActionResult<List<DRILLING_OPERATION>>> GetDrillingOperations([FromQuery] string? wellUWI = null)
        {
            try
            {
                var result = await _service.GetDrillingOperationsAsync(wellUWI);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drilling operations");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpGet("operations/{operationId}")]
        public async Task<ActionResult<DRILLING_OPERATION>> GetDrillingOperation(string operationId)
        {
            if (string.IsNullOrWhiteSpace(operationId)) return BadRequest(new { error = "Operation ID is required." });
            try
            {
                var result = await _service.GetDrillingOperationAsync(operationId);
                if (result == null)
                        return NotFound(new { error = $"Drilling operation {operationId} not found." });
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drilling operation {OperationId}", operationId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpPost("operations")]
        public async Task<ActionResult<DRILLING_OPERATION>> CreateDrillingOperation([FromBody] CREATE_DRILLING_OPERATION createDto)
        {
            try
            {
                var result = await _service.CreateDrillingOperationAsync(createDto, userId: GetUserId());
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating drilling operation");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpPut("operations/{operationId}")]
        public async Task<ActionResult<DRILLING_OPERATION>> UpdateDrillingOperation(string operationId, [FromBody] UpdateDrillingOperation updateDto)
        {
            if (string.IsNullOrWhiteSpace(operationId)) return BadRequest(new { error = "Operation ID is required." });
            try
            {
                var result = await _service.UpdateDrillingOperationAsync(operationId, updateDto, userId: GetUserId());
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating drilling operation {OperationId}", operationId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpGet("operations/{operationId}/reports")]
        public async Task<ActionResult<List<DRILLING_REPORT>>> GetDrillingReports(string operationId)
        {
            if (string.IsNullOrWhiteSpace(operationId)) return BadRequest(new { error = "Operation ID is required." });
            try
            {
                var result = await _service.GetDrillingReportsAsync(operationId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drilling reports for operation {OperationId}", operationId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpPost("operations/{operationId}/reports")]
        public async Task<ActionResult<DRILLING_REPORT>> CreateDrillingReport(string operationId, [FromBody] CreateDrillingReport createDto)
        {
            if (string.IsNullOrWhiteSpace(operationId)) return BadRequest(new { error = "Operation ID is required." });
            try
            {
                var result = await _service.CreateDrillingReportAsync(operationId, createDto, userId: GetUserId());
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating drilling report for operation {OperationId}", operationId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        private string GetUserId() => User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";
    }
}

