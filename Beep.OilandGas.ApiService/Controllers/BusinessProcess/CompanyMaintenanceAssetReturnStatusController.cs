using Beep.OilandGas.Models.Data.WorkOrder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.ApiService.Services;
namespace Beep.OilandGas.ApiService.Controllers.BusinessProcess;

[ApiController][Authorize][Route("api/field/current/maintenance-handoffs/execution-permit/asset-return/status")][RequireCurrentFieldAccess("READ")]
public sealed class CompanyMaintenanceAssetReturnStatusController(CompanyMaintenanceHandoffService service):ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CompanyMaintenanceAssetReturnStatus>> Read(CompanyWorkOrderCompletionQuery query)
    {
        Response.Headers.CacheControl="no-store";
        try{return Ok(await service.ReadMaintenanceAssetReturnStatusAsync(User,query));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
}
