using System.Security.Claims;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Accounting.Royalty;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Royalty;

/// <summary>Read-only field royalty reporting and previews; all storage is server-bound.</summary>
[ApiController]
[Authorize]
[Route("api/accounting/royalty")]
public sealed class RoyaltyReportsController(IAccountingService accounting, ILogger<RoyaltyReportsController> logger) : ControllerBase
{
    /// <summary>Preview recorded oil royalties without persisting obligations or journal entries.</summary>
    [HttpPost("preview")]
    public async Task<ActionResult<List<ROYALTY_CALCULATION>>> PreviewRoyalties(
        [FromBody] PreviewRoyaltiesRequest request,
        [FromServices] IAccessControlService access)
    {
        var actor = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (User.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(actor)) return Unauthorized();
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (string.IsNullOrWhiteSpace(request.FieldId)) return BadRequest(new { error = "Field ID is required." });
        try
        {
            if (!(await access.CheckAssetAccessAsync(actor, request.FieldId, "FIELD", null)).HasAccess) return Forbid();
            return Ok(await accounting.PreviewRoyaltiesAsync(request.FieldId,
                request.ProductionDate.ToDateTime(TimeOnly.MinValue), request.ProductionDate.ToDateTime(TimeOnly.MinValue), actor));
        }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
        catch (Beep.OilandGas.ProductionAccounting.Exceptions.RoyaltyException ex) { return UnprocessableEntity(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return UnprocessableEntity(new { error = ex.Message }); }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error previewing royalties");
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }
    /// <summary>
    /// Get royalty calculation records.
    /// </summary>
    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpGet("calculations")]
    public async Task<ActionResult<List<ROYALTY_CALCULATION>>> GetRoyaltyCalculations(
        [FromServices] IAccessControlService access,
        [FromQuery] string fieldId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var actor = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (User.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(actor)) return Unauthorized();
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (string.IsNullOrWhiteSpace(fieldId)) return BadRequest(new { error = "Field ID is required." });
        try
        {
            if (!(await access.CheckAssetAccessAsync(actor, fieldId, "FIELD", null)).HasAccess) return Forbid();
            var results = await accounting.GetRoyaltyCalculationsAsync(
                fieldId, startDate, endDate);

            return Ok(results);
        }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return UnprocessableEntity(new { error = ex.Message }); }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting royalty calculations");
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

}
