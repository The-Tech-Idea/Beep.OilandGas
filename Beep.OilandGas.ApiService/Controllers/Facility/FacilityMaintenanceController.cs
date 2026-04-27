using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.ProductionOperations.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Beep.OilandGas.ApiService.Controllers.Facility;

/// <summary>
/// Facility maintenance records (<see cref="FACILITY_MAINTAIN"/>).
/// </summary>
[ApiController]
[Route("api/facility/{facilityId}/maintenance")]
[Authorize]
public class FacilityMaintenanceController : ControllerBase
{
    private readonly IFacilityManagementService _facilities;
    private readonly ILogger<FacilityMaintenanceController> _logger;

    public FacilityMaintenanceController(IFacilityManagementService facilities, ILogger<FacilityMaintenanceController> logger)
    {
        _facilities = facilities;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FACILITY_MAINTAIN>>> ListAsync(
        string facilityId,
        [FromQuery] string? facilityType,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(facilityId))
            return BadRequest(new { error = "facilityId is required." });

        try
        {
            var rows = await _facilities
                .ListFacilityMaintenanceAsync(facilityId, facilityType, startDate, endDate, cancellationToken)
                .ConfigureAwait(false);
            return Ok(rows);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "List maintenance for {FacilityId} failed", facilityId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An internal error occurred." });
        }
    }

    [HttpPost]
    public async Task<ActionResult<FACILITY_MAINTAIN>> CreateAsync(
        string facilityId,
        [FromBody] FACILITY_MAINTAIN maintenance,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(facilityId))
            return BadRequest(new { error = "facilityId is required." });
        if (maintenance == null)
            return BadRequest(new { error = "Request body is required." });

        try
        {
            maintenance.FACILITY_ID ??= facilityId;
            var row = await _facilities
                .CreateFacilityMaintenanceAsync(maintenance, FacilityUserHelper.ResolveUserId(User), cancellationToken)
                .ConfigureAwait(false);
            return Ok(row);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Create maintenance for {FacilityId} failed", facilityId);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create maintenance for {FacilityId} failed", facilityId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An internal error occurred." });
        }
    }
}
