using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.ProductionOperations.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Beep.OilandGas.ApiService.Controllers.Facility;

/// <summary>
/// PPDM39 facility master data, classification, components, status history, and rates.
/// </summary>
[ApiController]
[Route("api/facility")]
[Authorize]
public class FacilityController : ControllerBase
{
    private readonly IFacilityManagementService _facilities;
    private readonly IProductionManagementService _productionManagement;
    private readonly ILogger<FacilityController> _logger;

    public FacilityController(
        IFacilityManagementService facilities,
        IProductionManagementService productionManagement,
        ILogger<FacilityController> logger)
    {
        _facilities = facilities;
        _productionManagement = productionManagement;
        _logger = logger;
    }

    /// <summary>Lists active facilities, optionally filtered by primary field.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<FACILITY>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FACILITY>>> ListAsync(
        [FromQuery] string? primaryFieldId,
        CancellationToken cancellationToken)
    {
        var rows = await _facilities.ListFacilitiesAsync(primaryFieldId, cancellationToken).ConfigureAwait(false);
        return Ok(rows);
    }

    /// <summary>PDEN rows with subtype FACILITY (registry query).</summary>
    [HttpGet("pden/facility-subtype")]
    [ProducesResponseType(typeof(IReadOnlyList<PDEN>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PDEN>>> ListFacilityPdenAsync(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        var rows = await _productionManagement
            .ListFacilityPdenDeclarationsAsync(startDate, endDate, cancellationToken)
            .ConfigureAwait(false);
        return Ok(rows);
    }

    [HttpGet("{facilityId}")]
    [ProducesResponseType(typeof(FACILITY), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FACILITY>> GetAsync(
        string facilityId,
        [FromQuery] string? facilityType,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(facilityId))
            return BadRequest(new { error = "facilityId is required." });

        var row = await _facilities.GetFacilityAsync(facilityId, facilityType, cancellationToken).ConfigureAwait(false);
        if (row == null)
            return NotFound(new { error = $"Facility {facilityId} was not found." });
        return Ok(row);
    }

    [HttpPost]
    [ProducesResponseType(typeof(FACILITY), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FACILITY>> CreateAsync(
        [FromBody] FACILITY facility,
        CancellationToken cancellationToken)
    {
        var userId = User.ActingUserId();
        if (facility == null)
            return BadRequest(new { error = "Request body is required." });

        var created = await _facilities
            .CreateFacilityAsync(facility, userId, cancellationToken)
            .ConfigureAwait(false);
        return Ok(created);
    }

    [HttpPut("{facilityId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateAsync(
        string facilityId,
        [FromBody] FACILITY facility,
        CancellationToken cancellationToken)
    {
        var userId = User.ActingUserId();
        if (string.IsNullOrWhiteSpace(facilityId))
            return BadRequest(new { error = "facilityId is required." });
        if (facility == null)
            return BadRequest(new { error = "Request body is required." });

        if (!string.IsNullOrWhiteSpace(facility.FACILITY_ID) &&
            !string.Equals(facility.FACILITY_ID, facilityId, StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Route facilityId and body FACILITY_ID must match." });

        facility.FACILITY_ID = facilityId;
        await _facilities
            .UpdateFacilityAsync(facility, userId, cancellationToken)
            .ConfigureAwait(false);
        return NoContent();
    }

    [HttpGet("{facilityId}/classes")]
    public async Task<ActionResult<IReadOnlyList<FACILITY_CLASS>>> ListClassesAsync(
        string facilityId,
        [FromQuery] string? facilityType,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(facilityId))
            return BadRequest(new { error = "facilityId is required." });

        var rows = await _facilities.ListFacilityClassesAsync(facilityId, facilityType, cancellationToken).ConfigureAwait(false);
        return Ok(rows);
    }

    [HttpPost("{facilityId}/classes")]
    public async Task<ActionResult<FACILITY_CLASS>> AddClassAsync(
        string facilityId,
        [FromQuery] string facilityClassType,
        [FromQuery] string? facilityType,
        CancellationToken cancellationToken)
    {
        var userId = User.ActingUserId();
        if (string.IsNullOrWhiteSpace(facilityId))
            return BadRequest(new { error = "facilityId is required." });
        if (string.IsNullOrWhiteSpace(facilityClassType))
            return BadRequest(new { error = "facilityClassType query parameter is required." });

        var row = await _facilities
            .AddFacilityClassAsync(facilityId, facilityType, facilityClassType.Trim(), userId, cancellationToken)
            .ConfigureAwait(false);
        return Ok(row);
    }

    [HttpGet("{facilityId}/components")]
    public async Task<ActionResult<IReadOnlyList<FACILITY_COMPONENT>>> ListComponentsAsync(
        string facilityId,
        [FromQuery] string? facilityType,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(facilityId))
            return BadRequest(new { error = "facilityId is required." });

        var rows = await _facilities.ListFacilityComponentsAsync(facilityId, facilityType, cancellationToken).ConfigureAwait(false);
        return Ok(rows);
    }

    [HttpPost("{facilityId}/components")]
    public async Task<ActionResult<FACILITY_COMPONENT>> AddComponentAsync(
        string facilityId,
        [FromBody] FACILITY_COMPONENT component,
        CancellationToken cancellationToken)
    {
        var userId = User.ActingUserId();
        if (string.IsNullOrWhiteSpace(facilityId))
            return BadRequest(new { error = "facilityId is required." });
        if (component == null)
            return BadRequest(new { error = "Request body is required." });

        component.FACILITY_ID ??= facilityId;
        var row = await _facilities
            .AddFacilityComponentAsync(component, userId, cancellationToken)
            .ConfigureAwait(false);
        return Ok(row);
    }

    [HttpGet("{facilityId}/status")]
    public async Task<ActionResult<IReadOnlyList<FACILITY_STATUS>>> ListStatusAsync(
        string facilityId,
        [FromQuery] string? facilityType,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(facilityId))
            return BadRequest(new { error = "facilityId is required." });

        var rows = await _facilities.ListFacilityStatusHistoryAsync(facilityId, facilityType, cancellationToken).ConfigureAwait(false);
        return Ok(rows);
    }

    [HttpPost("{facilityId}/status")]
    public async Task<ActionResult<FACILITY_STATUS>> AddStatusAsync(
        string facilityId,
        [FromBody] FACILITY_STATUS status,
        [FromQuery] bool enforceActiveLicenseForOperationalStatus = true,
        CancellationToken cancellationToken = default)
    {
        var userId = User.ActingUserId();
        if (string.IsNullOrWhiteSpace(facilityId))
            return BadRequest(new { error = "facilityId is required." });
        if (status == null)
            return BadRequest(new { error = "Request body is required." });

        status.FACILITY_ID ??= facilityId;
        var row = await _facilities
            .AddFacilityStatusAsync(status, userId, enforceActiveLicenseForOperationalStatus, cancellationToken)
            .ConfigureAwait(false);
        return Ok(row);
    }

    [HttpGet("{facilityId}/rates")]
    public async Task<ActionResult<IReadOnlyList<FACILITY_RATE>>> ListRatesAsync(
        string facilityId,
        [FromQuery] string? facilityType,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(facilityId))
            return BadRequest(new { error = "facilityId is required." });

        var rows = await _facilities.ListFacilityRatesAsync(facilityId, facilityType, cancellationToken).ConfigureAwait(false);
        return Ok(rows);
    }
}
