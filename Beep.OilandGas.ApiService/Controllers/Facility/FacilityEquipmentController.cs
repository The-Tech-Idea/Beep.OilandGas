using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.ProductionOperations.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Beep.OilandGas.ApiService.Controllers.Facility;

/// <summary>
/// Equipment linked to a facility (<see cref="FACILITY_EQUIPMENT"/>).
/// </summary>
[ApiController]
[Route("api/facility/{facilityId}/equipment")]
[Authorize]
public class FacilityEquipmentController : ControllerBase
{
    private readonly IFacilityManagementService _facilities;
    private readonly ILogger<FacilityEquipmentController> _logger;

    public FacilityEquipmentController(IFacilityManagementService facilities, ILogger<FacilityEquipmentController> logger)
    {
        _facilities = facilities;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FACILITY_EQUIPMENT>>> ListAsync(
        string facilityId,
        [FromQuery] string? facilityType,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(facilityId))
            return BadRequest(new { error = "facilityId is required." });

        var rows = await _facilities.ListFacilityEquipmentAsync(facilityId, facilityType, cancellationToken).ConfigureAwait(false);
        return Ok(rows);
    }

    [HttpPost("{equipmentId}")]
    public async Task<ActionResult<FACILITY_EQUIPMENT>> LinkAsync(
        string facilityId,
        string equipmentId,
        [FromQuery] string? facilityType,
        CancellationToken cancellationToken)
    {
        var userId = User.ActingUserId();
        if (string.IsNullOrWhiteSpace(facilityId))
            return BadRequest(new { error = "facilityId is required." });
        if (string.IsNullOrWhiteSpace(equipmentId))
            return BadRequest(new { error = "equipmentId is required." });

        var row = await _facilities
            .LinkEquipmentToFacilityAsync(facilityId, facilityType, equipmentId.Trim(), userId, cancellationToken)
            .ConfigureAwait(false);
        return Ok(row);
    }
}
