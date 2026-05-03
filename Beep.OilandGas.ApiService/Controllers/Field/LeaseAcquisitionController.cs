using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Field;

/// <summary>
/// Field-scoped mirror of <see cref="Operations.LeaseAcquisitionController"/> under the current field context.
/// Filtering by orchestrator field id can be merged into <c>GetAvailableLeases</c> filters in a follow-up.
/// </summary>
[ApiController]
[Authorize]
[Route("api/field/current/lease-acquisition")]
[RequireCurrentFieldAccess]
public class LeaseAcquisitionController : ControllerBase
{
    private readonly ILeaseAcquisitionService _service;
    private readonly ILogger<LeaseAcquisitionController> _logger;

    public LeaseAcquisitionController(
        ILeaseAcquisitionService service,
        ILogger<LeaseAcquisitionController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("evaluate/{leaseId}")]
    public async Task<ActionResult<LeaseSummary>> EvaluateLease(string leaseId)
    {
        if (string.IsNullOrWhiteSpace(leaseId)) return BadRequest(new { error = "Lease ID is required." });
        try
        {
            var result = await _service.EvaluateLeaseAsync(leaseId);
            return Ok(result);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Lease not found: {LeaseId}", leaseId);
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating lease {LeaseId}", leaseId);
            return StatusCode(500, new { error = "An internal error occurred." });
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
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available leases");
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

    [HttpPost]
    public async Task<ActionResult<string>> CreateLeaseAcquisition([FromBody] CreateLeaseAcquisition? leaseRequest, [FromQuery] string? userId = null)
    {
        if (leaseRequest is null) return BadRequest(new { error = "Request body is required." });
        try
        {
            var id = await _service.CreateLeaseAcquisitionAsync(leaseRequest, userId ?? GetUserId());
            return Ok(new { message = "Lease acquisition created successfully", leaseId = id });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating lease acquisition");
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

    [HttpPut("{leaseId}/status")]
    public async Task<ActionResult> UpdateLeaseStatus(string leaseId, [FromBody] UpdateLeaseStatusRequest? request, [FromQuery] string? userId = null)
    {
        if (string.IsNullOrWhiteSpace(leaseId)) return BadRequest(new { error = "Lease ID is required." });
        if (request is null) return BadRequest(new { error = "Request body is required." });
        try
        {
            await _service.UpdateLeaseStatusAsync(leaseId, request.Status, userId ?? GetUserId());
            return Ok(new { message = "Lease status updated successfully" });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lease status for {LeaseId}", leaseId);
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

    private string GetUserId() => User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";
}
