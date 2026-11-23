using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.ApiService.Services;
namespace Beep.OilandGas.ApiService.Controllers.Accounting.Traditional;

[ApiController]
[Authorize]
[Route("api/accounting/traditional/company-receivables")]
public sealed partial class CompanySalesReceivableController(CompanySalesReceivableReads reads,TheTechIdeaWeb.Diagnostics.IFailureReporter failures):ControllerBase
{
    [HttpGet("status/{invoiceId}")]
    public Task<ActionResult> Status(string invoiceId)=>Execute(async()=>Ok(await reads.ReadStatusAsync(User,RecordedSourceRouteIds.Read(Request,nameof(invoiceId)))));
    [HttpGet("recovery/{invoiceId}")]
    public Task<ActionResult> Recovery(string invoiceId)=>Execute(async()=>Ok(await reads.ReadRecoveryAsync(User,RecordedSourceRouteIds.Read(Request,nameof(invoiceId)))));
    [HttpGet("source/{invoiceId}")]
    public Task<ActionResult> Source(string invoiceId)=>Execute(async()=>Ok(await reads.ReadSourceAsync(User,RecordedSourceRouteIds.Read(Request,nameof(invoiceId)))));
    [HttpPost("drafts")]
    [RequestSizeLimit(134217728)]
    public Task<ActionResult> Prepare([FromBody]CompanySalesReceivableCommand command)=>command?.Source is null?Task.FromResult<ActionResult>(BadRequest()):Execute(async()=>Ok(await reads.PrepareAsync(User,command)));
    [HttpGet("review/{receivableId}")]
    public Task<ActionResult> Review(string receivableId)=>Execute(async()=>Ok(await reads.ReadReviewAsync(User,RecordedSourceRouteIds.Read(Request,nameof(receivableId)))));
    [HttpGet("review-recovery/{receivableId}")]
    public Task<ActionResult> ReviewRecovery(string receivableId)=>Execute(async()=>Ok(await reads.ReadReviewRecoveryAsync(User,RecordedSourceRouteIds.Read(Request,nameof(receivableId)))));
    [HttpPost("decisions")]
    [RequestSizeLimit(134217728)]
    public Task<ActionResult> Decide([FromBody]CompanySalesReceivableDecisionCommand command)=>command?.Review is null?Task.FromResult<ActionResult>(BadRequest()):Execute(async()=>Ok(await reads.RecordDecisionAsync(User,command)));
    private async Task<ActionResult> Execute(Func<Task<ActionResult>> action)
    {
        try{return await action();}
        catch(ArgumentException invalid){failures.ReportHandled(invalid,"validating company accounting request","request rejected");return BadRequest();}
        catch(FormatException invalid){failures.ReportHandled(invalid,"reading company accounting route","request rejected");return BadRequest();}
        catch(UnauthorizedAccessException denied){failures.ReportHandled(denied,"authorizing company accounting request","request forbidden");return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){failures.ReportHandled(denied,"authorizing company accounting request","request forbidden");return Forbid();}
        catch(Exception changed) when(changed is InvalidOperationException or JsonException or AggregateException)
        {failures.ReportHandled(changed,"The original company receivable evidence or outcome is unconfirmed.","original accounting evidence or outcome unconfirmed; request refused");return Conflict(new{error="Receivable evidence or outcome is unconfirmed. Recover the original preparation or reviewer command before another request."});}
    }
}
