using Beep.OilandGas.Models.Data.WorkOrder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.ApiService.Services;
namespace Beep.OilandGas.ApiService.Controllers.BusinessProcess;
[ApiController][Authorize][Route("api/field/current/maintenance-handoffs/recorded-work")][RequireCurrentFieldAccess("READ")]
public sealed class CompanyMaintenanceRecordedWorkController(CompanyMaintenanceHandoffService service):ControllerBase
{
    [HttpPost("pages/{page:int}/{size:int}")]
    public async Task<ActionResult<CompanyMaintenanceRecordedWorkPage>> Read(CompanyMaintenanceRecordedWorkWindow window,int page,int size)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.ReadRecordedWorkPageAsync(User,window,page,size));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
}
