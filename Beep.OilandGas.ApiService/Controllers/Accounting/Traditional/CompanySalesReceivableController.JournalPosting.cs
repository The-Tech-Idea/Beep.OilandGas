using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Accounting.Services;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Controllers.Accounting.Traditional;
public sealed partial class CompanySalesReceivableController
{
    [HttpGet("journal-postings/{id}/source")]
    public Task<ActionResult> JournalPostingSource(string id,[FromQuery]string book,[FromQuery]string currency,[FromServices]JournalEntryService journal,[FromServices]AccountMappingService mappings,[FromServices]PeriodClosingService periods)=>Execute(async()=>Ok(await reads.ReadJournalPostingSourceAsync(User,id,book,currency,journal,mappings,periods,HttpContext.RequestAborted)));
    [HttpGet("journal-postings/{id}/recovery")]
    public Task<ActionResult> JournalPostingRecovery(string id,[FromQuery]string book,[FromQuery]string currency,[FromServices]JournalEntryService journal)=>Execute(async()=>Ok(await reads.RecoverJournalPostingAsync(User,id,book,currency,journal,HttpContext.RequestAborted)));
    [HttpPost("journal-postings")][RequestSizeLimit(67108864)]
    public Task<ActionResult> JournalPosting([FromBody]CompanySalesJournalPostingCommand command,[FromServices]JournalEntryService journal,[FromServices]AccountMappingService mappings,[FromServices]PeriodClosingService periods)=>command?.Source is null?Task.FromResult<ActionResult>(BadRequest()):Execute(async()=>Ok(await reads.PostJournalAsync(User,command,journal,mappings,periods,HttpContext.RequestAborted)));
}
