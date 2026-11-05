using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.Models.Core.Refusals;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Controllers.Accounting.Traditional;
[ApiController][Authorize][Route("api/accounting/traditional/inventory/company/work-order-reservations")][RequireCurrentFieldAccess("READ")]
public sealed class CompanyMaterialReservationsController(ICompanyMaterialReservationQueueReader reader):ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CompanyMaterialReservationQueue>> Get()
    {
        try{return Ok(await reader.ReadMaterialReservationQueueAsync(User));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(RefusalException denied) when(denied.Kind==RefusalKind.Forbidden){return Forbid();}
        catch(InvalidOperationException){return Conflict(new{error="The complete current FIELD reservation queue or current order rights changed. Reload original company/FIELD evidence."});}
    }
}