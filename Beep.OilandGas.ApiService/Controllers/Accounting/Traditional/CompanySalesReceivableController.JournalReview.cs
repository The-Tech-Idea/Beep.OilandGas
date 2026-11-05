using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Accounting.Services;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Controllers.Accounting.Traditional;
public sealed partial class CompanySalesReceivableController
{
    [HttpGet("journal-reviews/{id}/source")]
    public Task<ActionResult> JournalReviewSource(string id,[FromQuery]string book,[FromQuery]string currency,[FromServices]JournalEntryService journal,[FromServices]AccountMappingService mappings,[FromServices]PeriodClosingService periods)=>Execute(async()=>Ok(await reads.ReadJournalReviewAsync(User,id,book,currency,journal,mappings,periods,HttpContext.RequestAborted)));
    [HttpGet("journal-reviews/{id}/recovery")]
    public Task<ActionResult> JournalReviewRecovery(string id,[FromQuery]string book,[FromQuery]string currency,[FromServices]JournalEntryService journal)=>Execute(async()=>Ok(await reads.RecoverJournalReviewAsync(User,id,book,currency,journal,HttpContext.RequestAborted)));
    [HttpPost("journal-reviews")][RequestSizeLimit(33554432)]
    public Task<ActionResult> JournalReview([FromBody]CompanySalesJournalReviewCommand command,[FromServices]JournalEntryService journal,[FromServices]AccountMappingService mappings,[FromServices]PeriodClosingService periods)=>command?.Review is null?Task.FromResult<ActionResult>(BadRequest()):Execute(async()=>Ok(await reads.RecordJournalReviewAsync(User,command,journal,mappings,periods,HttpContext.RequestAborted)));
}
