using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.ApiService.Services;
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
        private readonly Beep.OilandGas.DrillingAndConstruction.Services.DrillingOperationService _service;
        private readonly ILogger<DrillingOperationController> _logger;

        public DrillingOperationController(Beep.OilandGas.DrillingAndConstruction.Services.DrillingOperationService service, ILogger<DrillingOperationController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("operations")]
        public async Task<ActionResult<List<DRILLING_OPERATION>>> GetDrillingOperations([FromQuery] string? wellUWI = null)
        {
            var result = await _service.GetDrillingOperationsAsync(wellUWI);
            return Ok(result);
        }

        [HttpGet("operations/{operationId}")]
        public async Task<ActionResult<DRILLING_OPERATION>> GetDrillingOperation(string operationId)
        {
            if (string.IsNullOrWhiteSpace(operationId)) return BadRequest(new { error = "Operation ID is required." });
            var result = await _service.GetDrillingOperationAsync(operationId);
            if (result == null)
                    return NotFound(new { error = $"Drilling operation {operationId} not found." });
            return Ok(result);
        }

        [HttpPost("operations")]
        public async Task<ActionResult<DRILLING_OPERATION>> CreateDrillingOperation([FromBody] CREATE_DRILLING_OPERATION createDto)
        {
            var userId = User.ActingUserId();
            var result = await _service.CreateDrillingOperationAsync(createDto, userId: userId);
            return Ok(result);
        }

        [HttpPut("operations/{operationId}")]
        public async Task<ActionResult<DRILLING_OPERATION>> UpdateDrillingOperation(string operationId, [FromBody] UpdateDrillingOperation updateDto)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(operationId)) return BadRequest(new { error = "Operation ID is required." });
            var result = await _service.UpdateDrillingOperationAsync(operationId, updateDto, userId: userId);
            return Ok(result);
        }

        [HttpGet("operations/{operationId}/reports")]
        public async Task<ActionResult<List<DRILLING_REPORT>>> GetDrillingReports(string operationId)
        {
            if (string.IsNullOrWhiteSpace(operationId)) return BadRequest(new { error = "Operation ID is required." });
            var result = await _service.GetDrillingReportsAsync(operationId);
            return Ok(result);
        }

        [HttpPost("operations/{operationId}/reports")]
        public async Task<ActionResult<DRILLING_REPORT>> CreateDrillingReport(string operationId, [FromBody] CreateDrillingReport createDto)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(operationId)) return BadRequest(new { error = "Operation ID is required." });
            var result = await _service.CreateDrillingReportAsync(operationId, createDto, userId: userId);
            return Ok(result);
        }
    }
}
