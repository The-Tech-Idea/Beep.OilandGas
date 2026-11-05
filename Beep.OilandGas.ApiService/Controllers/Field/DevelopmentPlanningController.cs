using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.DevelopmentPlanning.Services;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.Models.Data.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheTechIdeaWeb.Diagnostics;

namespace Beep.OilandGas.ApiService.Controllers.Field;

[ApiController, Authorize]
[Route("api/field/current/development-planning")]
[ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
public sealed class DevelopmentPlanningController(IDevelopmentPlanService service,IDevelopmentPlanRequestAuthority authority) : ControllerBase
{
    [HttpGet("plans/options")]
    public async Task<ActionResult<DevelopmentPlanDraftOptions>> GetDraftOptionsAsync()
    {
        var scope=await authority.CaptureAsync(User,PermissionConstants.DevelopmentPlanning.Create,true);
        var result=await service.GetDraftOptionsAsync(scope.Context,scope.PlanningDatabase,scope.CheckCurrentAsync);await scope.ConfirmAsync();return Ok(result);
    }
    [HttpGet("plans")]
    public async Task<ActionResult<DevelopmentPlanDraftList>> GetPlansAsync()
    {
        var scope=await authority.CaptureAsync(User,PermissionConstants.DevelopmentPlanning.View,false);
        var result=await service.GetDevelopmentPlansAsync(scope.Context,scope.PlanningDatabase,scope.CheckCurrentAsync);await scope.ConfirmAsync();return Ok(result);
    }
    [HttpGet("plans/{id}")]
    public async Task<ActionResult<DevelopmentPlanSourceReview>> GetPlanAsync(string id)
    {
        var scope=await authority.CaptureAsync(User,PermissionConstants.DevelopmentPlanning.View,false);
        var result=await service.GetDevelopmentPlanAsync(scope.Context,scope.PlanningDatabase,id,scope.CheckCurrentAsync);
        result.Require(scope.Context,id);await scope.ConfirmAsync();return Ok(result);
    }
    [HttpGet("plans/{id}/review-workflows")]
    public async Task<ActionResult<DevelopmentReviewCatalog>> GetReviewWorkflowsAsync(string id,[FromQuery]string expectedSourceRevision,
        [FromServices]DevelopmentReviewCatalogService workflows)=>Ok(await workflows.ReadAsync(User,id,expectedSourceRevision));
    [HttpPost("plans")]
    public async Task<ActionResult<FIELD_DEVELOPMENT_PLAN>> CreatePlanAsync([FromBody] CreateDevelopmentPlan request)
    {
        request=DevelopmentPlanDraftReview.Capture(request,request.SelectionContext);
        var scope=await authority.CaptureAsync(User,PermissionConstants.DevelopmentPlanning.Create,true,request.SelectionContext,true);
        if(request.PlanningDatabase!=scope.PlanningDatabase)throw RefusalException.Conflict("The reviewed development-planning target changed.");
        var result=await service.CreateDevelopmentPlanAsync(request,scope.CheckCurrentAsync);await scope.ConfirmAsync();return Ok(result);
    }
    [HttpPost("plans/{id}/review-handoffs/prepare")]
    public async Task<ActionResult<DevelopmentReviewStart>> PrepareReviewAsync(string id,[FromBody]DevelopmentReviewSelection selection,
        [FromServices]DevelopmentReviewHandoffService handoffs)=>Ok(await handoffs.PrepareAsync(User,id,selection));
    [HttpPost("review-handoffs")]
    public async Task<ActionResult<DevelopmentReviewStartReceipt>> SubmitReviewAsync([FromBody]DevelopmentReviewStart original,
        [FromServices]DevelopmentReviewHandoffService handoffs)=>Ok(await handoffs.SubmitAsync(User,original));
    [HttpPost("review-handoffs/recover")]
    public async Task<ActionResult<DevelopmentReviewStartReceipt>> RecoverReviewAsync([FromBody]DevelopmentReviewStart original,
        [FromServices]DevelopmentReviewHandoffService handoffs)=>Ok(await handoffs.SubmitAsync(User,original,true));
    [HttpGet("review-handoffs/{processId}/assigned")]
    public async Task<ActionResult<DevelopmentAssignedReview>> GetAssignedReviewAsync(string processId,
        [FromServices]IDevelopmentAssignedReviewReader assigned,[FromServices]IFailureReporter failures)
    {
        try{return Ok(await assigned.ReadAsync(User,processId));}
        catch(UnauthorizedAccessException){throw RefusalException.Forbidden("Current independent company reviewer, workspace and original FIELD access are required.");}
        catch(InvalidOperationException changed)
        {
            failures.ReportHandled(changed,"opening assigned development review","the original task and source could not be confirmed",FailureSeverity.Degraded);
            throw RefusalException.Conflict("The assigned development task, workflow or original source changed or is unavailable. Reload company assigned work.");
        }
    }
    [HttpPost("review-decisions")]
    public Task<ActionResult<DevelopmentReviewDecisionReceipt>> DecideReviewAsync([FromBody]DevelopmentReviewDecision command,
        [FromServices]IDevelopmentReviewDecisionService decisions,[FromServices]IFailureReporter failures)=>DecisionResult(()=>decisions.DecideAsync(User,command),failures);
    [HttpPost("review-decisions/recover")]
    public Task<ActionResult<DevelopmentReviewDecisionReceipt>> RecoverReviewDecisionAsync([FromBody]DevelopmentReviewDecision original,
        [FromServices]IDevelopmentReviewDecisionService decisions,[FromServices]IFailureReporter failures)=>DecisionResult(()=>decisions.DecideAsync(User,original,true),failures);
    [HttpGet("review-handoffs/{processId}/history")]
    public Task<ActionResult<DevelopmentReviewProgress>> GetReviewProgressAsync(string processId,
        [FromServices]IDevelopmentReviewProgressReader reviews,[FromServices]IFailureReporter failures)=>ProgressResult(()=>reviews.ReadProgressAsync(User,processId),failures);
    [HttpGet("plans/{id}/review-status")]
    public Task<ActionResult<DevelopmentPlanReviewStatus>> GetPlanReviewStatusAsync(string id,[FromQuery]string expectedSourceRevision,
        [FromServices]IDevelopmentReviewProgressReader reviews,[FromServices]IFailureReporter failures)=>ProgressResult(async()=>{
            if(!ProcessDefinitionRevision.IsValid(expectedSourceRevision))throw RefusalException.Invalid("Inspect the recorded draft before checking its review outcome.");
            var scope=await authority.CaptureAsync(User,PermissionConstants.DevelopmentPlanning.View,false);
            var source=await service.GetDevelopmentPlanAsync(scope.Context,scope.PlanningDatabase,id,scope.CheckCurrentAsync);source.Require(scope.Context,id);
            if(source.Plan is null || source.Revision!=expectedSourceRevision)throw RefusalException.Conflict("The displayed draft changed or is unavailable. Reload its recorded source.");
            await scope.ConfirmAsync();var progress=await reviews.ReadForSourceAsync(User,source);
            var closing=await service.GetDevelopmentPlanAsync(scope.Context,scope.PlanningDatabase,id,scope.CheckCurrentAsync);closing.Require(scope.Context,id);
            if(closing.Plan is null || closing.Revision!=source.Revision)throw RefusalException.Conflict("The draft changed during its recorded review lookup.");
            await scope.ConfirmAsync();var result=new DevelopmentPlanReviewStatus(closing,progress,DateTimeOffset.UtcNow);result.Require(source);return result;
        },failures);
    private async Task<ActionResult<T>> ProgressResult<T>(Func<Task<T>> read,IFailureReporter failures)
    {
        try{return Ok(await read());}
        catch(UnauthorizedAccessException){throw RefusalException.Forbidden("Current company review readership and original FIELD access are required.");}
        catch(InvalidOperationException changed)
        {
            failures.ReportHandled(changed,"reading recorded development review history","the recorded review outcome could not be confirmed",FailureSeverity.Degraded);
            throw RefusalException.Conflict("The recorded development review, audit or source changed or is unavailable. Reload its current evidence.");
        }
    }
    [HttpGet("review-handoffs/{processId}/baseline")]
    public Task<ActionResult<DevelopmentPlanBaselineReview>> GetBaselineAsync(string processId,[FromQuery]string expectedReviewRevision,
        [FromServices]IDevelopmentPlanBaselineService baselines,[FromServices]IFailureReporter failures)
        =>BaselineResult(()=>baselines.ReadBaselineAsync(User,processId,expectedReviewRevision),failures,false);
    [HttpPost("baselines")]
    public Task<ActionResult<DevelopmentPlanReleaseReceipt>> ApproveBaselineAsync([FromBody]DevelopmentPlanRelease original,
        [FromServices]IDevelopmentPlanBaselineService baselines,[FromServices]IFailureReporter failures)
        =>BaselineResult(()=>baselines.ApproveBaselineAsync(User,original),failures,true);
    [HttpPost("baselines/recover")]
    public Task<ActionResult<DevelopmentPlanReleaseReceipt>> RecoverBaselineAsync([FromBody]DevelopmentPlanRelease original,
        [FromServices]IDevelopmentPlanBaselineService baselines,[FromServices]IFailureReporter failures)
        =>BaselineResult(()=>baselines.ApproveBaselineAsync(User,original,true),failures,false);
    private async Task<ActionResult<T>> BaselineResult<T>(Func<Task<T>> action,IFailureReporter failures,bool write)
    {
        try{return Ok(await action());}
        catch(UnauthorizedAccessException){throw RefusalException.Forbidden("Current original FDP participant, final approval role and original FIELD authority are required for this baseline action.");}
        catch(InvalidOperationException changed)
        {
            failures.ReportHandled(changed,write?"approving FDP baseline":"reading original FDP baseline","the original baseline outcome could not be confirmed",FailureSeverity.Degraded);
            throw RefusalException.Conflict("The original FDP baseline, source or review could not be confirmed. Preserve any retained approval intent and recover its original outcome before another approval.");
        }
    }
    private async Task<ActionResult<DevelopmentReviewDecisionReceipt>> DecisionResult(Func<Task<DevelopmentReviewDecisionReceipt>> action,IFailureReporter failures)
    {
        try{return Ok(await action());}
        catch(UnauthorizedAccessException){throw RefusalException.Forbidden("Current original independent reviewer, approval permissions and FIELD authority are required.");}
        catch(InvalidOperationException changed)
        {
            failures.ReportHandled(changed,"recording independent development review","the original decision outcome is unconfirmed",FailureSeverity.Degraded);
            throw RefusalException.Conflict("The original review decision, assignment or source could not be confirmed. Preserve its intent and recover the original outcome before another decision.");
        }
    }
    [HttpPost("plans/recover")]
    public async Task<ActionResult<DevelopmentPlanDraftRecovery>> RecoverPlanAsync([FromBody] CreateDevelopmentPlan original)
    {
        original=DevelopmentPlanDraftReview.Capture(original,original.SelectionContext);
        var scope=await authority.CaptureAsync(User,PermissionConstants.DevelopmentPlanning.Create,false,original.SelectionContext,false);
        if(original.PlanningDatabase!=scope.PlanningDatabase)throw RefusalException.Conflict("The original development-planning target changed.");
        var result=await service.RecoverDevelopmentPlanAsync(original,scope.Context,scope.PlanningDatabase,scope.CheckCurrentAsync);await scope.ConfirmAsync();return Ok(result);
    }
}
