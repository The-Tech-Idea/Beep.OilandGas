using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.ApiService.Services;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Controllers.BusinessProcess;
[ApiController][Authorize(Roles="Administrator")][Route("api/company/personnel-policies")]
public sealed class CompanyPersonnelPoliciesController(CompanyPersonnelPolicyService service):ControllerBase
{
    [HttpGet("review")]
    public async Task<ActionResult<CompanyPersonnelPolicyCatalog>> Review()
    {
        try{return Ok(await service.ReviewAsync(User));}catch(UnauthorizedAccessException){return Forbid();}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("record")] public Task<ActionResult<WORK_ORDER_PERSONNEL_POLICY>> Record(CompanyPersonnelPolicyCommand command)=>Save(command,false);
    [HttpPost("recovery")] public Task<ActionResult<WORK_ORDER_PERSONNEL_POLICY>> Recover(CompanyPersonnelPolicyCommand command)=>Save(command,true);
    private async Task<ActionResult<WORK_ORDER_PERSONNEL_POLICY>> Save(CompanyPersonnelPolicyCommand command,bool readOnly)
    {
        try{return Ok(await service.RecordAsync(User,command,readOnly));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("recovery/reference")]
    public async Task<ActionResult<CompanyPersonnelPolicyRecovery>> RecoverReference(CompanyPersonnelPolicyReference reference)
    {
        try{return Ok(await service.RecoverReferenceAsync(User,reference));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
}
