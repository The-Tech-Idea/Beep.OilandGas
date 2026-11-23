using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.ApiService.Services;
namespace Beep.OilandGas.ApiService.Controllers.BusinessProcess;
[ApiController][Authorize(Roles="Administrator")][Route("api/company/personnel-policies")]
public sealed class CompanyPersonnelPoliciesController(CompanyPersonnelPolicyService service,TheTechIdeaWeb.Diagnostics.IFailureReporter failures):ControllerBase
{
    [HttpGet("review")]
    public async Task<ActionResult<CompanyPersonnelPolicyCatalog>> Review()
    {
        try{return Ok(await service.ReviewAsync(User));}catch(UnauthorizedAccessException denied){failures.ReportHandled(denied,"authorizing company personnel request","request forbidden");return Forbid();}catch(InvalidOperationException error){var failureReference=failures.ReportHandled(error,"processing company personnel request","original evidence or outcome unconfirmed; reconcile before retrying");return Conflict(new{error="The personnel evidence or outcome could not be confirmed. Reconcile the original record before retrying.",reference=failureReference});}
    }
    [HttpPost("record")] public Task<ActionResult<WORK_ORDER_PERSONNEL_POLICY>> Record(CompanyPersonnelPolicyCommand command)=>Save(command,false);
    [HttpPost("recovery")] public Task<ActionResult<WORK_ORDER_PERSONNEL_POLICY>> Recover(CompanyPersonnelPolicyCommand command)=>Save(command,true);
    private async Task<ActionResult<WORK_ORDER_PERSONNEL_POLICY>> Save(CompanyPersonnelPolicyCommand command,bool readOnly)
    {
        try{return Ok(await service.RecordAsync(User,command,readOnly));}catch(UnauthorizedAccessException denied){failures.ReportHandled(denied,"authorizing company personnel request","request forbidden");return Forbid();}catch(ArgumentException error){var failureReference=failures.ReportHandled(error,"validating company personnel request","request rejected");return BadRequest(new{error="The personnel request is invalid.",reference=failureReference});}catch(InvalidOperationException error){var failureReference=failures.ReportHandled(error,"processing company personnel request","original evidence or outcome unconfirmed; reconcile before retrying");return Conflict(new{error="The personnel evidence or outcome could not be confirmed. Reconcile the original record before retrying.",reference=failureReference});}
    }
    [HttpPost("recovery/reference")]
    public async Task<ActionResult<CompanyPersonnelPolicyRecovery>> RecoverReference(CompanyPersonnelPolicyReference reference)
    {
        try{return Ok(await service.RecoverReferenceAsync(User,reference));}catch(UnauthorizedAccessException denied){failures.ReportHandled(denied,"authorizing company personnel request","request forbidden");return Forbid();}catch(ArgumentException error){var failureReference=failures.ReportHandled(error,"validating company personnel request","request rejected");return BadRequest(new{error="The personnel request is invalid.",reference=failureReference});}catch(InvalidOperationException error){var failureReference=failures.ReportHandled(error,"processing company personnel request","original evidence or outcome unconfirmed; reconcile before retrying");return Conflict(new{error="The personnel evidence or outcome could not be confirmed. Reconcile the original record before retrying.",reference=failureReference});}
    }
}
