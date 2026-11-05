using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.ApiService.Services;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Controllers.Accounting.Traditional;

[ApiController]
[Authorize]
[Route("api/accounting/traditional/company-invoices")]
public sealed class CompanySalesInvoiceController(CompanySalesInvoiceReads reads,ILogger<CompanySalesInvoiceController> logger):ControllerBase
{
    [HttpGet("queue/{fieldId}")]
    public Task<ActionResult> Queue(string fieldId)=>Execute(async()=>Ok(await reads.ReadQueueAsync(User,RecordedSourceRouteIds.Read(Request,nameof(fieldId)))));
    [HttpGet("delivery/{ticketId}")]
    public Task<ActionResult> Read(string ticketId)=>Execute(async()=>Ok(await reads.ReadAsync(User,RecordedSourceRouteIds.Read(Request,nameof(ticketId)))));
    [HttpGet("intents/{intentId}")]
    public Task<ActionResult> Recover(string intentId)=>Execute(async()=>
    {
        var review=await reads.ReadIntentAsync(User,RecordedSourceRouteIds.Read(Request,nameof(intentId)));return review is null?NotFound():Ok(review);
    });
    [HttpGet("review/{invoiceId}")]
    public Task<ActionResult> Review(string invoiceId)=>Execute(async()=>Ok(await reads.ReadReviewAsync(User,RecordedSourceRouteIds.Read(Request,nameof(invoiceId)))));
    [HttpGet("review-intents/{invoiceId}")]
    public Task<ActionResult> ReviewIntent(string invoiceId)=>Execute(async()=>new JsonResult(await reads.ReadReviewIntentAsync(User,RecordedSourceRouteIds.Read(Request,nameof(invoiceId)))));
    [HttpPost("decisions")]
    [RequestSizeLimit(67108864)]
    public Task<ActionResult> Decision([FromBody]CompanySalesInvoiceDecisionCommand command)=>command?.Review is null?Task.FromResult<ActionResult>(BadRequest()):Execute(async()=>Ok(await reads.RecordDecisionAsync(User,command)));
    [HttpPost("drafts")]
    [RequestSizeLimit(8388608)]
    public Task<ActionResult> Prepare([FromBody]CompanySalesInvoiceDraftCommand command)=>command?.Review is null?Task.FromResult<ActionResult>(BadRequest()):
        Execute(async()=>Ok(await reads.PrepareAsync(User,command)));
    private async Task<ActionResult> Execute(Func<Task<ActionResult>> action)
    {
        try{return await action();}
        catch(ArgumentException){return BadRequest();}
        catch(FormatException){return BadRequest();}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){return Forbid();}
        catch(Exception changed) when(changed is InvalidOperationException or System.Text.Json.JsonException or AggregateException)
        {logger.LogWarning(changed,"The original company invoice source or draft outcome is unconfirmed.");return Conflict(new{error="Invoice evidence or outcome is unconfirmed. Retain and reconcile the original command; do not create another invoice or infer financial approval."});}
    }
}
