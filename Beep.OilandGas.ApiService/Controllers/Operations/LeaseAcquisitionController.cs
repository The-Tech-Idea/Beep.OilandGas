using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Operations
{
    /// <summary>
    /// API controller for lease acquisition operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LeaseAcquisitionController : ControllerBase
    {
        private readonly ILeaseAcquisitionService _service;
        private readonly ILogger<LeaseAcquisitionController> _logger;

        public LeaseAcquisitionController(ILeaseAcquisitionService service, ILogger<LeaseAcquisitionController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("evaluate/{leaseId}")]
        public async Task<ActionResult<LeaseSummary>> EvaluateLease(string leaseId)
        {
            try
            {
                var result = await _service.EvaluateLeaseAsync(leaseId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating lease {LeaseId}", leaseId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("available")]
        public async Task<ActionResult<List<LeaseSummary>>> GetAvailableLeases([FromQuery] Dictionary<string, string>? filters = null)
        {
            try
            {
                var result = await _service.GetAvailableLeasesAsync(filters);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available leases");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<string>> CreateLeaseAcquisition([FromBody] CreateLeaseAcquisition leaseRequest, [FromQuery] string? userId = null)
        {
            try
            {
                var leaseId = await _service.CreateLeaseAcquisitionAsync(leaseRequest, userId ?? GetUserId());
                return Ok(new { message = "Lease acquisition created successfully", leaseId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating lease acquisition");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPut("{leaseId}/status")]
        public async Task<ActionResult> UpdateLeaseStatus(string leaseId, [FromBody] UpdateLeaseStatusRequest request, [FromQuery] string? userId = null)
        {
            try
            {
                await _service.UpdateLeaseStatusAsync(leaseId, request.Status, userId ?? GetUserId());
                return Ok(new { message = "Lease status updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating lease status for {LeaseId}", leaseId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private string GetUserId() => User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";
    }
}

