using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Data.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Controllers.HSE;

[ApiController, Authorize, Route("api/field/current/hse/assigned-overdue-actions")]
[RequireCurrentFieldAccess(PermissionConstants.HSE.View)]
public sealed class HseAssignedActionsController(CompanyHseAssignedActionReader reader) : ControllerBase
{
    [HttpGet, ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<ActionResult<CompanyHseAssignedActions>> Read(CancellationToken token) =>
        Ok(await reader.ReadAsync(User, token));
}
