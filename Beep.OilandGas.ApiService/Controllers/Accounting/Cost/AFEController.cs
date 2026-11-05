using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Refusals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheTechIdeaWeb.Diagnostics;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Cost;

[ApiController,Authorize]
[Route("api/accounting/cost/afe")]
[ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
public sealed class AFEController(CompanyDevelopmentAfeService service,IFailureReporter failures):ControllerBase
{
    [HttpPost("development/assigned/decisions")]
    public Task<ActionResult<DevelopmentInvestmentStageReceipt>> AssignedStageDecisionAsync([FromBody]DevelopmentInvestmentStageDecision original,
        [FromServices]CompanyDevelopmentInvestmentRoutingService routing)=>Answer(()=>routing.DecideAssignedStageAsync(User,original));
    [HttpPost("development/assigned/decisions/recover")]
    public Task<ActionResult<DevelopmentInvestmentStageReceipt>> RecoverAssignedStageDecisionAsync([FromBody]DevelopmentInvestmentStageDecision original,
        [FromServices]CompanyDevelopmentInvestmentRoutingService routing)=>Answer(()=>routing.DecideAssignedStageAsync(User,original,true));
    [HttpGet("development/assigned/{instanceId}/original-decision")]
    public Task<ActionResult<DevelopmentInvestmentStageRecoveryInput>> OwnStageOriginalAsync(string instanceId,
        [FromServices]CompanyDevelopmentInvestmentRoutingService routing)=>Answer(()=>routing.ReadOwnStageOriginalAsync(User,instanceId));
    [HttpGet("development/assigned/{instanceId}")]
    public Task<ActionResult<DevelopmentInvestmentAssignedReview>> AssignedInvestmentAsync(string instanceId,
        [FromServices]IDevelopmentInvestmentAssignedReader assigned)=>Answer(()=>assigned.ReadAssignedAsync(User,instanceId));
    [HttpGet("development/{afeId}/handoff")]
    public Task<ActionResult<DevelopmentInvestmentHandoffStatus>> HandoffStatusAsync(string afeId,[FromQuery]string processId,
        [FromServices]CompanyDevelopmentInvestmentRoutingService routing)=>Answer(()=>routing.ReadHandoffStatusAsync(User,processId,afeId));
    [HttpPost("development/{afeId}/handoff/prepare")]
    public Task<ActionResult<DevelopmentInvestmentHandoff>> PrepareHandoffAsync(string afeId,[FromQuery]string processId,[FromBody]DevelopmentInvestmentHandoffSelection selection,
        [FromServices]CompanyDevelopmentInvestmentRoutingService routing)=>Answer(()=>routing.PrepareHandoffAsync(User,processId,afeId,selection));
    [HttpPost("development/handoffs")]
    public Task<ActionResult<DevelopmentInvestmentHandoffReceipt>> HandoffAsync([FromBody]DevelopmentInvestmentHandoff original,
        [FromServices]CompanyDevelopmentInvestmentRoutingService routing)=>Answer(()=>routing.SubmitHandoffAsync(User,original));
    [HttpPost("development/handoffs/recover")]
    public Task<ActionResult<DevelopmentInvestmentHandoffReceipt>> RecoverHandoffAsync([FromBody]DevelopmentInvestmentHandoff original,
        [FromServices]CompanyDevelopmentInvestmentRoutingService routing)=>Answer(()=>routing.SubmitHandoffAsync(User,original,true));
    [HttpGet("development/queue")]
    public Task<ActionResult<DevelopmentInvestmentQueue>> QueueAsync()=>Answer(()=>service.QueueAsync(User));
    [HttpGet("development")]
    public Task<ActionResult<DevelopmentAfeReview>> ReviewAsync([FromQuery]string processId)=>Answer(()=>service.ReviewAsync(User,processId));
    [HttpPost("development")]
    public Task<ActionResult<DevelopmentAfeReceipt>> RecordAsync([FromBody]DevelopmentAfeCommand original)=>Answer(()=>service.RecordAsync(User,original));
    [HttpPost("development/recover")]
    public Task<ActionResult<DevelopmentAfeReceipt>> RecoverAsync([FromBody]DevelopmentAfeCommand original)=>Answer(()=>service.RecordAsync(User,original,true));
    [HttpGet("development/{afeId}/investment")]
    public Task<ActionResult<DevelopmentInvestmentReview>> InvestmentAsync(string afeId,[FromQuery]string processId)=>Answer(()=>service.ReviewInvestmentAsync(User,processId,afeId));
    [HttpGet("development/{afeId}/routing")]
    public Task<ActionResult<DevelopmentInvestmentRoutingReview>> RoutingAsync(string afeId,[FromQuery]string processId,
        [FromServices]CompanyDevelopmentInvestmentRoutingService routing)=>Answer(()=>routing.ReadAsync(User,processId,afeId));
    private async Task<ActionResult<T>> Answer<T>(Func<Task<T>> action)
    {
        try{return Ok(await action());}
        catch(UnauthorizedAccessException){throw RefusalException.Forbidden("Current company financial authority and access to the original approved FDP FIELD are required.");}
        catch(InvalidOperationException error)
        {
            failures.ReportHandled(error,"reviewing or recording development AFE request","the original financial request could not be confirmed",FailureSeverity.Degraded);
            throw RefusalException.Conflict("The approved FDP, configured book or original AFE request could not be confirmed. Preserve any retained request and recover its original outcome.");
        }
    }
}
