using Beep.OilandGas.Models.Data.WorkOrder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.ApiService.Services;
namespace Beep.OilandGas.ApiService.Controllers.BusinessProcess;

[ApiController][Authorize(Roles="Administrator")][Route("api/company/maintenance-return-policies")]
[ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)][RequestSizeLimit(3*1024*1024)]
public sealed class CompanyMaintenanceReturnPoliciesController(CompanyMaintenanceReturnPolicyService service,TheTechIdeaWeb.Diagnostics.IFailureReporter failures):ControllerBase
{
    [HttpGet("review")]
    public async Task<ActionResult<CompanyMaintenanceReturnPolicyCatalog>> Review()
    {
        try{return Ok(await service.ReviewAsync(User));}catch(UnauthorizedAccessException denied){failures.ReportHandled(denied,"authorizing maintenance return policy request","request forbidden");return Forbid();}catch(InvalidOperationException error){var failureReference=failures.ReportHandled(error,"processing maintenance return policy","original evidence or outcome unconfirmed; reconcile before retrying");return Conflict(new{error="The policy evidence or outcome could not be confirmed. Reconcile the original record before retrying.",reference=failureReference});}
    }
    [HttpPost("record")]public Task<ActionResult<WORK_ORDER_MAINTENANCE_RETURN_POLICY>> Record(CompanyMaintenanceReturnPolicyCommand command)=>Save(command,false);
    [HttpPost("recovery")]public Task<ActionResult<WORK_ORDER_MAINTENANCE_RETURN_POLICY>> Recover(CompanyMaintenanceReturnPolicyCommand command)=>Save(command,true);
    private async Task<ActionResult<WORK_ORDER_MAINTENANCE_RETURN_POLICY>> Save(CompanyMaintenanceReturnPolicyCommand command,bool readOnly)
    {
        try{return Ok(await service.RecordAsync(User,command,readOnly));}catch(UnauthorizedAccessException denied){failures.ReportHandled(denied,"authorizing maintenance return policy request","request forbidden");return Forbid();}catch(ArgumentException error){var failureReference=failures.ReportHandled(error,"validating maintenance return policy","request rejected");return BadRequest(new{error="The maintenance return policy request is invalid.",reference=failureReference});}catch(InvalidOperationException error){var failureReference=failures.ReportHandled(error,"processing maintenance return policy","original evidence or outcome unconfirmed; reconcile before retrying");return Conflict(new{error="The policy evidence or outcome could not be confirmed. Reconcile the original record before retrying.",reference=failureReference});}
    }
    [HttpPost("recovery/reference")]
    public async Task<ActionResult<CompanyMaintenanceReturnPolicyRecovery>> RecoverReference(CompanyMaintenanceReturnPolicyReference reference)
    {
        try{return Ok(await service.RecoverReferenceAsync(User,reference));}catch(UnauthorizedAccessException denied){failures.ReportHandled(denied,"authorizing maintenance return policy request","request forbidden");return Forbid();}catch(ArgumentException error){var failureReference=failures.ReportHandled(error,"validating maintenance return policy","request rejected");return BadRequest(new{error="The maintenance return policy request is invalid.",reference=failureReference});}catch(InvalidOperationException error){var failureReference=failures.ReportHandled(error,"processing maintenance return policy","original evidence or outcome unconfirmed; reconcile before retrying");return Conflict(new{error="The policy evidence or outcome could not be confirmed. Reconcile the original record before retrying.",reference=failureReference});}
    }
}
