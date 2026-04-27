using Beep.OilandGas.Models.Data.ProductionOperations;
using Beep.OilandGas.ProductionOperations.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Beep.OilandGas.ApiService.Controllers.Facility;

/// <summary>
/// Vertical facility/equipment monitoring endpoints (measurements + install/uninstall/move history).
/// </summary>
[ApiController]
[Route("api/facility/{facilityId}/monitoring")]
[Authorize]
public class FacilityMonitoringController : ControllerBase
{
    private readonly IFacilityManagementService _facilities;
    private readonly ILogger<FacilityMonitoringController> _logger;

    public FacilityMonitoringController(IFacilityManagementService facilities, ILogger<FacilityMonitoringController> logger)
    {
        _facilities = facilities;
        _logger = logger;
    }

    [HttpGet("measurements")]
    public async Task<ActionResult<IReadOnlyList<FACILITY_MEASUREMENT>>> ListMeasurementsAsync(
        string facilityId,
        [FromQuery] string? facilityType,
        [FromQuery] string? equipmentId,
        [FromQuery] string? measurementType,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(facilityId))
            return BadRequest(new { error = "facilityId is required." });
        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            return BadRequest(new { error = "startDate must be on or before endDate." });

        try
        {
            var rows = await _facilities
                .ListFacilityMeasurementsAsync(facilityId, facilityType, equipmentId, measurementType, startDate, endDate, cancellationToken)
                .ConfigureAwait(false);
            return Ok(rows);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "List measurements for {FacilityId} failed", facilityId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An internal error occurred." });
        }
    }

    [HttpPost("measurements")]
    public async Task<ActionResult<FACILITY_MEASUREMENT>> RecordMeasurementAsync(
        string facilityId,
        [FromBody] FACILITY_MEASUREMENT measurement,
        [FromQuery] string? facilityType,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(facilityId))
            return BadRequest(new { error = "facilityId is required." });
        if (measurement == null)
            return BadRequest(new { error = "Request body is required." });

        try
        {
            measurement.FACILITY_ID = facilityId;
            if (!string.IsNullOrWhiteSpace(facilityType))
                measurement.FACILITY_TYPE = facilityType.Trim();

            var row = await _facilities
                .RecordFacilityMeasurementAsync(measurement, FacilityUserHelper.ResolveUserId(User), cancellationToken)
                .ConfigureAwait(false);
            return Ok(row);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Record measurement for {FacilityId} failed", facilityId);
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Record measurement for {FacilityId} failed", facilityId);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Record measurement for {FacilityId} failed", facilityId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An internal error occurred." });
        }
    }

    [HttpGet("equipment/{equipmentId}/activity")]
    public async Task<ActionResult<IReadOnlyList<FACILITY_EQUIPMENT_ACTIVITY>>> ListEquipmentActivityAsync(
        string facilityId,
        string equipmentId,
        [FromQuery] string? facilityType,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(facilityId))
            return BadRequest(new { error = "facilityId is required." });
        if (string.IsNullOrWhiteSpace(equipmentId))
            return BadRequest(new { error = "equipmentId is required." });
        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            return BadRequest(new { error = "startDate must be on or before endDate." });

        try
        {
            var rows = await _facilities
                .ListEquipmentActivityAsync(facilityId, facilityType, equipmentId, startDate, endDate, cancellationToken)
                .ConfigureAwait(false);
            return Ok(rows);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "List equipment activity for {FacilityId}/{EquipmentId} failed", facilityId, equipmentId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An internal error occurred." });
        }
    }

    [HttpPost("equipment/{equipmentId}/activity")]
    public async Task<ActionResult<FACILITY_EQUIPMENT_ACTIVITY>> RecordEquipmentActivityAsync(
        string facilityId,
        string equipmentId,
        [FromBody] FACILITY_EQUIPMENT_ACTIVITY activity,
        [FromQuery] string? facilityType,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(facilityId))
            return BadRequest(new { error = "facilityId is required." });
        if (string.IsNullOrWhiteSpace(equipmentId))
            return BadRequest(new { error = "equipmentId is required." });
        if (activity == null)
            return BadRequest(new { error = "Request body is required." });

        try
        {
            activity.FACILITY_ID = facilityId;
            activity.EQUIPMENT_ID = equipmentId;
            if (!string.IsNullOrWhiteSpace(facilityType))
                activity.FACILITY_TYPE = facilityType.Trim();

            var row = await _facilities
                .RecordEquipmentActivityAsync(activity, FacilityUserHelper.ResolveUserId(User), cancellationToken)
                .ConfigureAwait(false);
            return Ok(row);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Record equipment activity for {FacilityId}/{EquipmentId} failed", facilityId, equipmentId);
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Record equipment activity for {FacilityId}/{EquipmentId} failed", facilityId, equipmentId);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Record equipment activity for {FacilityId}/{EquipmentId} failed", facilityId, equipmentId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An internal error occurred." });
        }
    }
}

