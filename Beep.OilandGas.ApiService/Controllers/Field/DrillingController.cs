using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Drilling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Field
{
    [ApiController]
    [Route("api/field/current/drilling")]
    [Authorize]
    public class DrillingController : ControllerBase
    {
        private readonly IFieldOrchestrator _fieldOrchestrator;
        private readonly IDrillingOperationService _service;
        private readonly ILogger<DrillingController> _logger;

        public DrillingController(
            IFieldOrchestrator fieldOrchestrator,
            IDrillingOperationService service,
            ILogger<DrillingController> logger)
        {
            _fieldOrchestrator = fieldOrchestrator ?? throw new ArgumentNullException(nameof(fieldOrchestrator));
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("operations")]
        public async Task<ActionResult<List<DRILLING_OPERATION>>> GetDrillingOperationsAsync([FromQuery] string? wellUWI = null)
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrWhiteSpace(fieldId))
                return BadRequest(new { error = "No active field selected." });

            try
            {
                var result = await _service.GetDrillingOperationsAsync(wellUWI, fieldId);
                return Ok(result ?? new List<DRILLING_OPERATION>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drilling operations for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpGet("operations/{operationId}")]
        public async Task<ActionResult<DRILLING_OPERATION>> GetDrillingOperationAsync(string operationId)
        {
            if (string.IsNullOrWhiteSpace(operationId))
                return BadRequest(new { error = "Operation ID is required." });

            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrWhiteSpace(fieldId))
                return BadRequest(new { error = "No active field selected." });

            try
            {
                var result = await _service.GetDrillingOperationAsync(operationId, fieldId);
                if (result == null)
                    return NotFound(new { error = $"Drilling operation {operationId} not found." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drilling operation {OperationId} for field {FieldId}", operationId, fieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpPost("operations")]
        public async Task<ActionResult<DRILLING_OPERATION>> CreateDrillingOperationAsync([FromBody] CREATE_DRILLING_OPERATION createDto)
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrWhiteSpace(fieldId))
                return BadRequest(new { error = "No active field selected." });

            try
            {
                var result = await _service.CreateDrillingOperationAsync(createDto, fieldId, GetUserId());
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating drilling operation for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpPut("operations/{operationId}")]
        public async Task<ActionResult<DRILLING_OPERATION>> UpdateDrillingOperationAsync(string operationId, [FromBody] UpdateDrillingOperation updateDto)
        {
            if (string.IsNullOrWhiteSpace(operationId))
                return BadRequest(new { error = "Operation ID is required." });

            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrWhiteSpace(fieldId))
                return BadRequest(new { error = "No active field selected." });

            try
            {
                var result = await _service.UpdateDrillingOperationAsync(operationId, updateDto, fieldId, GetUserId());
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = $"Drilling operation {operationId} not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating drilling operation {OperationId} for field {FieldId}", operationId, fieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpGet("operations/{operationId}/reports")]
        public async Task<ActionResult<List<DRILLING_REPORT>>> GetDrillingReportsAsync(string operationId)
        {
            if (string.IsNullOrWhiteSpace(operationId))
                return BadRequest(new { error = "Operation ID is required." });

            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrWhiteSpace(fieldId))
                return BadRequest(new { error = "No active field selected." });

            try
            {
                var result = await _service.GetDrillingReportsAsync(operationId, fieldId);
                return Ok(result ?? new List<DRILLING_REPORT>());
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = $"Drilling operation {operationId} not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drilling reports for operation {OperationId} in field {FieldId}", operationId, fieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        [HttpPost("operations/{operationId}/reports")]
        public async Task<ActionResult<DRILLING_REPORT>> CreateDrillingReportAsync(string operationId, [FromBody] CreateDrillingReport createDto)
        {
            if (string.IsNullOrWhiteSpace(operationId))
                return BadRequest(new { error = "Operation ID is required." });

            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrWhiteSpace(fieldId))
                return BadRequest(new { error = "No active field selected." });

            try
            {
                var result = await _service.CreateDrillingReportAsync(operationId, createDto, fieldId, GetUserId());
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = $"Drilling operation {operationId} not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating drilling report for operation {OperationId} in field {FieldId}", operationId, fieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? "SYSTEM";
    }
}