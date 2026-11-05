using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.ApiService.Services;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Controllers.BusinessProcess;
[ApiController][Authorize(Roles="Administrator")][Route("api/company/current/personnel-identities")]
public sealed class CompanyPersonnelIdentityController(CompanyPersonnelIdentityService service):ControllerBase
{
    [HttpGet] public async Task<ActionResult<CompanyPersonnelIdentityCatalog>> Catalog(){try{return Ok(await service.CatalogAsync(User));}catch(UnauthorizedAccessException){return Forbid();}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("review")] public async Task<ActionResult<CompanyPersonnelIdentityReview>> Review(CompanyPersonnelIdentityQuery query){try{return Ok(await service.ReviewAsync(User,query));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost] public Task<ActionResult<COMPANY_PERSONNEL_IDENTITY>> Record(CompanyPersonnelIdentityCommand command)=>RecordCore(command,false);
    [HttpPost("recovery")] public Task<ActionResult<COMPANY_PERSONNEL_IDENTITY>> Recover(CompanyPersonnelIdentityCommand command)=>RecordCore(command,true);
    private async Task<ActionResult<COMPANY_PERSONNEL_IDENTITY>> RecordCore(CompanyPersonnelIdentityCommand command,bool readOnly){Response.Headers.CacheControl="no-store";try{return Ok(await service.RecordAsync(User,command,readOnly));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("recovery/reference")] public async Task<ActionResult<CompanyPersonnelIdentityRecovery>> RecoverReference(CompanyPersonnelIdentityReference reference){Response.Headers.CacheControl="no-store";try{return Ok(await service.RecoverReferenceAsync(User,reference));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
}
