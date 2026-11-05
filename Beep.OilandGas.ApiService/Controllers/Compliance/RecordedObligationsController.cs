using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Data.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Controllers.Compliance;

[ApiController]
[Route("api/field/current/compliance/recorded-obligations")]
[Authorize]
[RequireCurrentFieldAccess(PermissionConstants.Regulatory.ViewCompliance)]
public sealed class RecordedObligationsController(CompanyFieldObligationReader reader) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<FieldObligationSourceReview>> GetAsync() => Ok(await reader.ReadAsync(User.ActingUserId()));
}
