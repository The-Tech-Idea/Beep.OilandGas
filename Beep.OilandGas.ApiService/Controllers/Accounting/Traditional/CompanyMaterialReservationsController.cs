using Beep.OilandGas.ProductionOperations.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.Models.Core.Refusals;
namespace Beep.OilandGas.ApiService.Controllers.Accounting.Traditional;
[ApiController][Authorize][Route("api/accounting/traditional/inventory/company/work-order-reservations")][RequireCurrentFieldAccess("READ")]
public sealed class CompanyMaterialReservationsController(ICompanyMaterialReservationQueueReader reader,TheTechIdeaWeb.Diagnostics.IFailureReporter failures):ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CompanyMaterialReservationQueue>> Get()
    {
        try{return Ok(await reader.ReadMaterialReservationQueueAsync(User));}
        catch(UnauthorizedAccessException denied){failures.ReportHandled(denied,"authorizing company or setup request","request forbidden");return Forbid();}
        catch(RefusalException denied) when(denied.Kind==RefusalKind.Forbidden){failures.ReportHandled(denied,"authorizing company request","request forbidden");return Forbid();}
        catch(InvalidOperationException changed){failures.ReportHandled(changed,"reading company reservations","complete evidence unconfirmed; request refused");return Conflict(new{error="The complete current FIELD reservation queue or current order rights changed. Reload original company/FIELD evidence."});}
    }
}
