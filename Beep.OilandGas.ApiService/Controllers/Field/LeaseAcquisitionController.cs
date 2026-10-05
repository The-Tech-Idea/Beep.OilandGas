using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.ApiService.Services;
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
        var result = await _service.EvaluateLeaseAsync(leaseId);
        return Ok(result);
    }

    [HttpGet("available")]
    public async Task<ActionResult<List<LeaseSummary>>> GetAvailableLeases([FromQuery] Dictionary<string, string>? filters = null)
    {
        var result = await _service.GetAvailableLeasesAsync(filters);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<string>> CreateLeaseAcquisition([FromBody] CreateLeaseAcquisition? leaseRequest)
    {
        var userId = User.ActingUserId();
        if (leaseRequest is null) return BadRequest(new { error = "Request body is required." });
        var id = await _service.CreateLeaseAcquisitionAsync(leaseRequest, userId);
        return Ok(new { message = "Lease acquisition created successfully", leaseId = id });
    }

    [HttpPut("{leaseId}/status")]
    public async Task<ActionResult> UpdateLeaseStatus(string leaseId, [FromBody] UpdateLeaseStatusRequest? request)
    {
        var userId = User.ActingUserId();
        if (string.IsNullOrWhiteSpace(leaseId)) return BadRequest(new { error = "Lease ID is required." });
        if (request is null) return BadRequest(new { error = "Request body is required." });
        await _service.UpdateLeaseStatusAsync(leaseId, request.Status, userId);
        return Ok(new { message = "Lease status updated successfully" });
    }
}
