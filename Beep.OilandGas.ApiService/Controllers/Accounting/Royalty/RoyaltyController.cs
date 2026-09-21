using System.Security.Claims;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Accounting.Royalty;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.ProductionAccounting.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Royalty;

[ApiController]
[Authorize]
[Route("api/accounting/royalty")]
public sealed class RoyaltyController(IRoyaltyService royalties, IAccessControlService access,
    ILogger<RoyaltyController> logger) : ControllerBase
{
    [HttpPost("calculations/{royaltyId}/payments")]
    public Task<IActionResult> RecordPayment(string royaltyId, [FromBody] CreateRoyaltyPaymentRequest request)
        => WithCalculation(royaltyId, true, async actor => Ok(await royalties.RecordPaymentAsync(
            royaltyId, request.RequestId, request.Amount, actor)));

    [HttpGet("calculations/{royaltyId}/payments")]
    public Task<IActionResult> GetPayments(string royaltyId)
        => WithCalculation(royaltyId, false, async _ => Ok(await royalties.GetPaymentsAsync(royaltyId)));

    [HttpGet("service/calculations/{royaltyId}")]
    public Task<IActionResult> GetCalculation(string royaltyId)
        => WithCalculation(royaltyId, false, async _ => Ok(await royalties.GetAsync(royaltyId)));

    [HttpGet("calculations/{royaltyId}/posting-review")]
    public Task<IActionResult> ReviewPostings(string royaltyId)
        => WithCalculation(royaltyId, true, async _ => Ok(await royalties.ReviewPostingsAsync(royaltyId)));

    private async Task<IActionResult> WithCalculation(string royaltyId, bool write, Func<string, Task<IActionResult>> action)
    {
        var actor = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (User.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(actor)) return Unauthorized();
        try
        {
            if (write && !await access.HasPermissionAsync(actor, "Accounting.PostJournal", null)) return Forbid();
            var calculation = await royalties.GetAsync(royaltyId);
            if (calculation is null) return NotFound();
            var field = await royalties.GetAllocationFieldAsync(calculation.ALLOCATION_DETAIL_ID);
            if (!(await access.CheckAssetAccessAsync(actor, field, "FIELD", null)).HasAccess) return Forbid();
            return await action(actor);
        }
        catch (RoyaltyException ex) { return Conflict(new { error = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
        catch (Exception ex)
        {
            logger.LogError(ex, "Royalty operation failed for {RoyaltyId}", royaltyId);
            return StatusCode(500, new { error = "The royalty operation could not be confirmed. Check the recorded status before retrying." });
        }
    }
}
