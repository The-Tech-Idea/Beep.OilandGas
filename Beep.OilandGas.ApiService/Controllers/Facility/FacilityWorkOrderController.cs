using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.ProductionOperations.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Beep.OilandGas.ApiService.Controllers.Facility;

/// <summary>
/// Work orders associated with a facility (via <see cref="WORK_ORDER_COMPONENT"/> linkage in the service layer).
/// </summary>
[ApiController]
[Route("api/facility/{facilityId}/work-orders")]
[Authorize]
public class FacilityWorkOrderController : ControllerBase
{
    private readonly IFacilityManagementService _facilities;
    private readonly ILogger<FacilityWorkOrderController> _logger;

    public FacilityWorkOrderController(IFacilityManagementService facilities, ILogger<FacilityWorkOrderController> logger)
    {
        _facilities = facilities;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WORK_ORDER>>> ListAsync(
        string facilityId,
        [FromQuery] string? facilityType,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(facilityId))
            return BadRequest(new { error = "facilityId is required." });

        var rows = await _facilities.ListFacilityWorkOrdersAsync(facilityId, facilityType, cancellationToken).ConfigureAwait(false);
        return Ok(rows);
    }

    [HttpPost]
    public async Task<ActionResult<WORK_ORDER>> CreateAsync(
        string facilityId,
        [FromBody] WORK_ORDER workOrder,
        [FromQuery] string facilityType,
        CancellationToken cancellationToken)
    {
        var userId = User.ActingUserId();
        if (string.IsNullOrWhiteSpace(facilityId))
            return BadRequest(new { error = "facilityId is required." });
        if (workOrder == null)
            return BadRequest(new { error = "Request body is required." });
        if (string.IsNullOrWhiteSpace(facilityType))
            return BadRequest(new { error = "facilityType query parameter is required (PPDM composite key)." });

        var row = await _facilities
            .CreateFacilityWorkOrderAsync(workOrder, facilityId, facilityType.Trim(), userId, cancellationToken)
            .ConfigureAwait(false);
        return Ok(row);
    }
}
