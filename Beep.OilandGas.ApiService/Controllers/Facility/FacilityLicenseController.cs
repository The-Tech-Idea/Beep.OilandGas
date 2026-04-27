using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.ProductionOperations.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Beep.OilandGas.ApiService.Controllers.Facility;

/// <summary>
/// Facility operating licenses (<see cref="FACILITY_LICENSE"/>).
/// </summary>
[ApiController]
[Route("api/facility/{facilityId}/licenses")]
[Authorize]
public class FacilityLicenseController : ControllerBase
{
    private readonly IFacilityManagementService _facilities;
    private readonly ILogger<FacilityLicenseController> _logger;

    public FacilityLicenseController(IFacilityManagementService facilities, ILogger<FacilityLicenseController> logger)
    {
        _facilities = facilities;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FACILITY_LICENSE>>> ListAsync(
        string facilityId,
        [FromQuery] string? facilityType,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(facilityId))
            return BadRequest(new { error = "facilityId is required." });

        try
        {
            var rows = await _facilities.ListFacilityLicensesAsync(facilityId, facilityType, cancellationToken).ConfigureAwait(false);
            return Ok(rows);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "List licenses for {FacilityId} failed", facilityId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An internal error occurred." });
        }
    }

    [HttpPost]
    public async Task<ActionResult<FACILITY_LICENSE>> CreateAsync(
        string facilityId,
        [FromBody] FACILITY_LICENSE license,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(facilityId))
            return BadRequest(new { error = "facilityId is required." });
        if (license == null)
            return BadRequest(new { error = "Request body is required." });

        try
        {
            license.FACILITY_ID ??= facilityId;
            var row = await _facilities
                .CreateFacilityLicenseAsync(license, FacilityUserHelper.ResolveUserId(User), cancellationToken)
                .ConfigureAwait(false);
            return Ok(row);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Create license for {FacilityId} failed", facilityId);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create license for {FacilityId} failed", facilityId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An internal error occurred." });
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<object>> HasActiveAsync(
        string facilityId,
        [FromQuery] string? facilityType,
        [FromQuery] DateTime? asOf,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(facilityId))
            return BadRequest(new { error = "facilityId is required." });

        try
        {
            var ok = await _facilities
                .FacilityHasActiveLicenseAsync(facilityId, facilityType, asOf, cancellationToken)
                .ConfigureAwait(false);
            return Ok(new { hasActiveLicense = ok, asOf = asOf ?? DateTime.UtcNow.Date });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Active license check for {FacilityId} failed", facilityId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An internal error occurred." });
        }
    }
}
