using Beep.OilandGas.Models.Data.WorkOrder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.ApiService.Services;
namespace Beep.OilandGas.ApiService.Controllers.BusinessProcess;

[ApiController][Authorize][Route("api/field/current/maintenance-handoffs/execution-permit/asset-return/status")][RequireCurrentFieldAccess("READ")]
public sealed class CompanyMaintenanceAssetReturnStatusController(CompanyMaintenanceHandoffService service,TheTechIdeaWeb.Diagnostics.IFailureReporter failures):ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CompanyMaintenanceAssetReturnStatus>> Read(CompanyWorkOrderCompletionQuery query)
    {
        Response.Headers.CacheControl="no-store";
        try{return Ok(await service.ReadMaintenanceAssetReturnStatusAsync(User,query));}
        catch(UnauthorizedAccessException denied){failures.ReportHandled(denied,"authorizing company workflow request","request forbidden");return Forbid();}
        catch(ArgumentException error){var failureReference=failures.ReportHandled(error,"validating company workflow request","request rejected");return BadRequest(new{error="The workflow request is invalid.",reference=failureReference});}
        catch(InvalidOperationException error){var failureReference=failures.ReportHandled(error,"reviewing company workflow evidence","original evidence or outcome unconfirmed; reconcile before retrying");return Conflict(new{error="The workflow evidence or outcome could not be confirmed. Reconcile the original record before retrying.",reference=failureReference});}
    }
}
