using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Accounting.Services;
namespace Beep.OilandGas.ApiService.Controllers.Accounting.Traditional;
public sealed partial class CompanySalesReceivableController
{
    [HttpGet("payment-review/{id}")]
    public Task<ActionResult> PaymentReview(string id,[FromServices]JournalEntryService journal)=>Execute(async()=>Ok(await reads.ReadPaymentReviewAsync(User,RecordedSourceRouteIds.Read(Request,nameof(id)),journal,HttpContext.RequestAborted)));
    [HttpPost("payment-decisions")]
    [RequestSizeLimit(8388608)]
    public Task<ActionResult> PaymentDecision([FromBody]CompanySalesPaymentReviewCommand command,[FromServices]JournalEntryService journal)=>command?.Review is null?Task.FromResult<ActionResult>(BadRequest()):Execute(async()=>Ok(await reads.RecordPaymentReviewAsync(User,command,journal,HttpContext.RequestAborted)));
    [HttpGet("payment-review-recovery/{id}")]
    public Task<ActionResult> PaymentReviewRecovery(string id)=>Execute(async()=>Ok(await reads.RecoverPaymentReviewAsync(User,RecordedSourceRouteIds.Read(Request,nameof(id)),HttpContext.RequestAborted)));
    [HttpGet("payment-source/{receivableId}")]
    public Task<ActionResult> PaymentSource(string receivableId,[FromQuery]string book,[FromQuery]string currency,[FromServices]JournalEntryService journal)=>Execute(async()=>Ok(await reads.ReadPaymentSourceAsync(User,RecordedSourceRouteIds.Read(Request,nameof(receivableId)),book,currency,journal,HttpContext.RequestAborted)));
    [HttpPost("payment-drafts")]
    [RequestSizeLimit(2097152)]
    public Task<ActionResult> PaymentDraft([FromBody]CompanySalesPaymentCommand command,[FromServices]JournalEntryService journal)=>command?.Source is null?Task.FromResult<ActionResult>(BadRequest()):Execute(async()=>Ok(await reads.PreparePaymentAsync(User,command,journal,HttpContext.RequestAborted)));
    [HttpGet("payment-recovery/{id}")]
    public Task<ActionResult> PaymentRecovery(string id)=>Execute(async()=>{var row=await reads.RecoverPaymentAsync(User,RecordedSourceRouteIds.Read(Request,nameof(id)),HttpContext.RequestAborted);return row is null?NotFound():Ok(row);});
}
