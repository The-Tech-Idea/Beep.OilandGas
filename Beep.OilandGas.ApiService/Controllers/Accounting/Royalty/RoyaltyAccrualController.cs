using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.ProductionAccounting.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Royalty;

[ApiController]
[Authorize]
[Route("api/accounting/royalty")]
public sealed class RoyaltyAccrualController(IRoyaltyService royalties, IAccessControlService access,
    ILogger<RoyaltyAccrualController> logger) : ControllerBase
{
    [HttpPost("allocations/{allocationDetailId}/accrue")]
    public async Task<ActionResult<ROYALTY_CALCULATION>> Accrue(string allocationDetailId)
    {
        var actor = User.ActingUserId();
        if (string.IsNullOrWhiteSpace(allocationDetailId)) return BadRequest();
        if (!await access.HasPermissionAsync(actor, "Accounting.PostJournal", null)) return Forbid();
        var field = await royalties.GetAllocationFieldAsync(allocationDetailId);
        if (!(await access.CheckAssetAccessAsync(actor, field, "FIELD", null)).HasAccess) return Forbid();
        return Ok(await royalties.CalculateAsync(allocationDetailId, actor));
    }
}
