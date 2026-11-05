using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Accounting.Services;
namespace Beep.OilandGas.ApiService.Controllers.Accounting.Traditional;
public sealed partial class CompanySalesReceivableController
{
    [HttpPost("journal-preparations")]
    [RequestSizeLimit(16777216)]
    public Task<ActionResult> PrepareJournal([FromBody]CompanySalesJournalPreparation command,[FromServices]JournalEntryService journal,[FromServices]AccountMappingService mappings,[FromServices]PeriodClosingService periods)
        =>command?.Source is null?Task.FromResult<ActionResult>(BadRequest()):Execute(async()=>Ok(await reads.PrepareJournalAsync(User,command,journal,mappings,periods,HttpContext.RequestAborted)));
    [HttpGet("journal-preparations/{id}/recovery")]
    public Task<ActionResult> RecoverJournal(string id,[FromQuery]string book,[FromQuery]string currency,[FromServices]JournalEntryService journal)=>Execute(async()=>{
        var result=await reads.RecoverJournalPreparationAsync(User,id,book,currency,journal,HttpContext.RequestAborted);return result is null?NotFound():Ok(result);
    });
}
