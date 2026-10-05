using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.ProductionOperations.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Beep.OilandGas.ApiService.Controllers.Facility;

/// <summary>
/// Facility-level production declaration (PDEN) and <see cref="PDEN_VOL_SUMMARY"/> volumes.
/// </summary>
[ApiController]
[Route("api/facility/{facilityId}/production")]
[Authorize]
public class FacilityProductionController : ControllerBase
{
    private readonly IFacilityManagementService _facilities;
    private readonly ILogger<FacilityProductionController> _logger;

    public FacilityProductionController(IFacilityManagementService facilities, ILogger<FacilityProductionController> logger)
    {
        _facilities = facilities;
        _logger = logger;
    }

    [HttpPost("pden/ensure")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> EnsurePdenAsync(
        string facilityId,
        [FromQuery] string? facilityType,
        CancellationToken cancellationToken)
    {
        var userId = User.ActingUserId();
        if (string.IsNullOrWhiteSpace(facilityId))
            return BadRequest(new { error = "facilityId is required." });

        var pdenId = await _facilities
            .EnsureFacilityPdenAsync(facilityId, facilityType, userId, cancellationToken)
            .ConfigureAwait(false);
        return Ok(new { pdenId });
    }

    [HttpGet("volumes")]
    public async Task<ActionResult<IReadOnlyList<PDEN_VOL_SUMMARY>>> ListVolumesAsync(
        string facilityId,
        [FromQuery] string? facilityType,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(facilityId))
            return BadRequest(new { error = "facilityId is required." });
        if (startDate > endDate)
            return BadRequest(new { error = "startDate must be on or before endDate." });

        var rows = await _facilities
            .ListFacilityProductionVolumesAsync(facilityId, facilityType, startDate, endDate, cancellationToken)
            .ConfigureAwait(false);
        return Ok(rows);
    }

    [HttpPost("volumes")]
    public async Task<ActionResult<PDEN_VOL_SUMMARY>> RecordVolumeAsync(
        string facilityId,
        [FromBody] PDEN_VOL_SUMMARY volume,
        [FromQuery] string? facilityType,
        CancellationToken cancellationToken)
    {
        var userId = User.ActingUserId();
        if (string.IsNullOrWhiteSpace(facilityId))
            return BadRequest(new { error = "facilityId is required." });
        if (volume == null)
            return BadRequest(new { error = "Request body is required." });

        if (string.IsNullOrWhiteSpace(volume.PDEN_ID))
        {
            var pdenId = await _facilities
                .EnsureFacilityPdenAsync(facilityId, facilityType, userId, cancellationToken)
                .ConfigureAwait(false);
            volume.PDEN_ID = pdenId;
        }

        var row = await _facilities
            .RecordFacilityProductionVolumeAsync(volume, userId, cancellationToken)
            .ConfigureAwait(false);
        return Ok(row);
    }

    [HttpGet("reliability")]
    public async Task<ActionResult<object>> ReliabilityAsync(
        string facilityId,
        [FromQuery] string? facilityType,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(facilityId))
            return BadRequest(new { error = "facilityId is required." });
        if (startDate > endDate)
            return BadRequest(new { error = "startDate must be on or before endDate." });

        var metrics = await _facilities
            .GetFacilityReliabilityMetricsAsync(facilityId, facilityType, startDate, endDate, cancellationToken)
            .ConfigureAwait(false);
        return Ok(new
        {
            metrics.MaintenanceEvents,
            metrics.WorkOrders,
            metrics.EstimatedAvailabilityPercent
        });
    }
}
