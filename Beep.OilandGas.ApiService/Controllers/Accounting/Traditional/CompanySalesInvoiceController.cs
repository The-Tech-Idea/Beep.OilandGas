using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.ApiService.Services;
namespace Beep.OilandGas.ApiService.Controllers.Accounting.Traditional;

[ApiController]
[Authorize]
[Route("api/accounting/traditional/company-invoices")]
public sealed class CompanySalesInvoiceController(CompanySalesInvoiceReads reads,TheTechIdeaWeb.Diagnostics.IFailureReporter failures):ControllerBase
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
        catch(ArgumentException invalid){failures.ReportHandled(invalid,"validating company accounting request","request rejected");return BadRequest();}
        catch(FormatException invalid){failures.ReportHandled(invalid,"reading company accounting route","request rejected");return BadRequest();}
        catch(UnauthorizedAccessException denied){failures.ReportHandled(denied,"authorizing company accounting request","request forbidden");return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){failures.ReportHandled(denied,"authorizing company accounting request","request forbidden");return Forbid();}
        catch(Exception changed) when(changed is InvalidOperationException or System.Text.Json.JsonException or AggregateException)
        {failures.ReportHandled(changed,"The original company invoice source or draft outcome is unconfirmed.","original accounting evidence or outcome unconfirmed; request refused");return Conflict(new{error="Invoice evidence or outcome is unconfirmed. Retain and reconcile the original command; do not create another invoice or infer financial approval."});}
    }
}
