using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.ApiService.Services;
namespace Beep.OilandGas.ApiService.Controllers.Accounting.Traditional;
public sealed partial class CompanySalesReceivableController
{
    [HttpGet("posted-ledger/{receivableId}")]
    public Task<ActionResult> PostedLedger(string receivableId,[FromQuery]string book,[FromQuery]string currency,[FromServices]JournalEntryService journal)=>Execute(async()=>Ok(await reads.ReadPostedLedgerAsync(User,RecordedSourceRouteIds.Read(Request,nameof(receivableId)),book,currency,journal,HttpContext.RequestAborted)));
}
