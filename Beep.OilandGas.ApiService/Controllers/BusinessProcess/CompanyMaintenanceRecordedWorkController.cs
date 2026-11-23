using Beep.OilandGas.Models.Data.WorkOrder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.ApiService.Services;
namespace Beep.OilandGas.ApiService.Controllers.BusinessProcess;
[ApiController][Authorize][Route("api/field/current/maintenance-handoffs/recorded-work")][RequireCurrentFieldAccess("READ")]
public sealed class CompanyMaintenanceRecordedWorkController(CompanyMaintenanceHandoffService service,TheTechIdeaWeb.Diagnostics.IFailureReporter failures):ControllerBase
{
    [HttpPost("pages/{page:int}/{size:int}")]
    public async Task<ActionResult<CompanyMaintenanceRecordedWorkPage>> Read(CompanyMaintenanceRecordedWorkWindow window,int page,int size)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.ReadRecordedWorkPageAsync(User,window,page,size));}catch(UnauthorizedAccessException denied){failures.ReportHandled(denied,"authorizing company workflow request","request forbidden");return Forbid();}catch(ArgumentException error){var failureReference=failures.ReportHandled(error,"validating company workflow request","request rejected");return BadRequest(new{error="The workflow request is invalid.",reference=failureReference});}catch(InvalidOperationException error){var failureReference=failures.ReportHandled(error,"reviewing company workflow evidence","original evidence or outcome unconfirmed; reconcile before retrying");return Conflict(new{error="The workflow evidence or outcome could not be confirmed. Reconcile the original record before retrying.",reference=failureReference});}}
}
