using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.DevelopmentPlanning.Services;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Field;

[ApiController]
[Authorize]
[Route("api/field/current/development-planning")]
[RequireCurrentFieldAccess]
public class DevelopmentPlanningController : ControllerBase
{
    private readonly IDevelopmentPlanService _service;
    private readonly ILogger<DevelopmentPlanningController> _logger;

    public DevelopmentPlanningController(
        IDevelopmentPlanService service,
        ILogger<DevelopmentPlanningController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("plans")]
    public async Task<ActionResult<List<DevelopmentPlan>>> GetPlansAsync([FromQuery] string? fieldId = null)
    {
        var plans = await _service.GetDevelopmentPlansAsync(fieldId);
        return Ok(plans);
    }

    [HttpGet("plans/{planId}")]
    public async Task<ActionResult<DevelopmentPlan>> GetPlanAsync(string planId)
    {
        var plan = await _service.GetDevelopmentPlanAsync(planId);
        if (plan == null)
            return NotFound(new { error = $"Plan {planId} not found." });
        return Ok(plan);
    }

    [HttpPost("plans")]
    public async Task<ActionResult<DevelopmentPlan>> CreatePlanAsync([FromBody] CreateDevelopmentPlan request)
    {
        var created = await _service.CreateDevelopmentPlanAsync(request);
        return Ok(created);
    }

    [HttpPut("plans/{planId}")]
    public async Task<ActionResult<DevelopmentPlan>> UpdatePlanAsync(string planId, [FromBody] UpdateDevelopmentPlan request)
    {
        var updated = await _service.UpdateDevelopmentPlanAsync(planId, request);
        return Ok(updated);
    }

    [HttpPost("plans/{planId}/approve")]
    public async Task<ActionResult<DevelopmentPlan>> ApprovePlanAsync(string planId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? "system";
        var approved = await _service.ApproveDevelopmentPlanAsync(planId, userId);
        return Ok(approved);
    }

    [HttpGet("plans/{planId}/well-activities")]
    public async Task<ActionResult<List<WELL_ACTIVITY>>> GetWellActivitiesAsync(string planId, [FromQuery] string? wellUwi = null)
    {
        var activities = await _service.GetWellActivitiesAsync(planId, wellUwi);
        return Ok(activities);
    }

    [HttpPost("maintenance")]
    public async Task<ActionResult<WELL_MAINTENANCE_PLAN>> CreateMaintenanceAsync([FromBody] CreateWellMaintenancePlan request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? "system";
        var created = await _service.CreateWellMaintenancePlanAsync(request, userId);
        return Ok(created);
    }

    [HttpPost("service-jobs")]
    public async Task<ActionResult<WELL_SERVICE_JOB>> CreateServiceJobAsync([FromBody] CreateWellServiceJob request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? "system";
        var created = await _service.CreateWellServiceJobAsync(request, userId);
        return Ok(created);
    }
}
