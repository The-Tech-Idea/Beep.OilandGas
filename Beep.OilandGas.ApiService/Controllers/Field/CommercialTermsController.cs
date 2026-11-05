using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Refusals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Beep.OilandGas.ApiService.Controllers.Field;
[ApiController][Authorize][Route("api/fields/{fieldId}/accounting/commercial-terms")]
public sealed class CommercialTermsController(CompanyFieldRevenueReads reads):ControllerBase
{
    [HttpGet]
    public Task<ActionResult<SalesContractTermsReadReview>> Read(string fieldId,DateTime from,DateTime through,string transactionId)=>Run(()=>reads.ReadTermsAsync(User,ExactField(),from,through,transactionId));
    [HttpPost("prepare")]
    public Task<ActionResult<SalesContractTermsRecord>> Prepare(string fieldId,[FromBody]SalesContractTermsPreparation preparation)=>Run(()=>reads.PrepareTermsAsync(User,ExactField(),preparation));
    [HttpPost("decide")]
    public Task<ActionResult<SalesContractTermsRecord>> Decide(string fieldId,[FromBody]SalesContractTermsDecisionSubmission submission)=>Run(()=>reads.DecideTermsAsync(User,ExactField(),submission));
    private string ExactField()=>RecordedSourceRouteIds.Read(Request,"fieldId");
    private async Task<ActionResult<T>> Run<T>(Func<Task<T>> action)
    {
        try{return Ok(await action());}
        catch(ArgumentException){return BadRequest();}catch(FormatException){return BadRequest();}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(RefusalException refusal) when(refusal.Kind==RefusalKind.Forbidden){return Forbid();}
        catch(Exception invalid) when(invalid is InvalidOperationException or System.Text.Json.JsonException)
        {return Conflict(new{error="Original commercial source or recorded outcome could not be confirmed. Retain and reconcile the original intent."});}
    }
}
