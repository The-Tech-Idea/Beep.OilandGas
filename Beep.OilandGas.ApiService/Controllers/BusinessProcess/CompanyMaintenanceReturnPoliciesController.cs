using Beep.OilandGas.Models.Data.WorkOrder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.ApiService.Services;
namespace Beep.OilandGas.ApiService.Controllers.BusinessProcess;

[ApiController][Authorize(Roles="Administrator")][Route("api/company/maintenance-return-policies")]
[ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)][RequestSizeLimit(3*1024*1024)]
public sealed class CompanyMaintenanceReturnPoliciesController(CompanyMaintenanceReturnPolicyService service):ControllerBase
{
    [HttpGet("review")]
    public async Task<ActionResult<CompanyMaintenanceReturnPolicyCatalog>> Review()
    {
        try{return Ok(await service.ReviewAsync(User));}catch(UnauthorizedAccessException){return Forbid();}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("record")]public Task<ActionResult<WORK_ORDER_MAINTENANCE_RETURN_POLICY>> Record(CompanyMaintenanceReturnPolicyCommand command)=>Save(command,false);
    [HttpPost("recovery")]public Task<ActionResult<WORK_ORDER_MAINTENANCE_RETURN_POLICY>> Recover(CompanyMaintenanceReturnPolicyCommand command)=>Save(command,true);
    private async Task<ActionResult<WORK_ORDER_MAINTENANCE_RETURN_POLICY>> Save(CompanyMaintenanceReturnPolicyCommand command,bool readOnly)
    {
        try{return Ok(await service.RecordAsync(User,command,readOnly));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("recovery/reference")]
    public async Task<ActionResult<CompanyMaintenanceReturnPolicyRecovery>> RecoverReference(CompanyMaintenanceReturnPolicyReference reference)
    {
        try{return Ok(await service.RecoverReferenceAsync(User,reference));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
}
