using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Data.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Controllers.Compliance;

[ApiController]
[Route("api/field/current/compliance/ghg/production-sources")]
[Authorize]
[RequireCurrentFieldAccess(PermissionConstants.Regulatory.ViewCompliance)]
public sealed class GhgProductionSourcesController(CompanyFieldGhgProductionReader reader, CompanyGhgArithmeticService checks) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<FieldGhgProductionSources>> GetAsync() => Ok(await reader.ReadAsync(User.ActingUserId()));
    [HttpPost("dispositions")]
    public async Task<ActionResult<FieldGhgDispositions>> DispositionsAsync([FromBody] GhgDispositionQuery query) => Ok(await reader.ReadDispositionsAsync(User.ActingUserId(), query));
    [HttpGet("calculation-basis")]
    public async Task<ActionResult<GhgCalculationBasis>> BasisAsync() => Ok(await checks.BasisAsync(User.ActingUserId()));
    [HttpPost("calculation-check")]
    public async Task<ActionResult<FieldGhgArithmeticCheck>> CheckAsync([FromBody] GhgArithmeticRequest request) => Ok(await checks.CheckAsync(User.ActingUserId(), request));
}
