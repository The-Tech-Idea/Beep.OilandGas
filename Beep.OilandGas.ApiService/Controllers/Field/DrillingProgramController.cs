using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Refusals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;

namespace Beep.OilandGas.ApiService.Controllers.Field;
[ApiController,Authorize,Route("api/drilling/programs")]
[ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
public sealed class DrillingProgramController(CompanyDrillingProgramService service,IFailureReporter failures):ControllerBase
{
    [HttpGet("execution/completion/review")]
    public Task<ActionResult<DrillingCompletionReviewReadiness>> CompletionReview([FromQuery]string reviewId)=>Answer(()=>service.ReadCompletionRoutingAsync(User,reviewId));
    [HttpPost("execution/completion/review")]
    public Task<ActionResult<DrillingCompletionReviewStartReceipt>> SubmitCompletionReview([FromBody]DrillingCompletionReviewStart original)=>Answer(()=>service.SubmitCompletionReviewAsync(User,original));
    [HttpPost("execution/completion/review/recover")]
    public Task<ActionResult<DrillingCompletionReviewStartReceipt>> RecoverCompletionReview([FromBody]DrillingCompletionReviewStart original)=>Answer(()=>service.SubmitCompletionReviewAsync(User,original,true));
    [HttpGet("execution/completion/assigned")]
    public Task<ActionResult<DrillingCompletionAssignedReview>> AssignedCompletion([FromQuery]string processId)=>Answer(()=>service.ReadAssignedCompletionAsync(User,processId));
    [HttpPost("execution/reports/conflict")]
    public Task<ActionResult<DrillingDailyReportConflictReview>> ReportConflict([FromBody]DrillingDailyReportInput original)=>Answer(()=>service.ReadDailyReportConflictAsync(User,original));
    [HttpPost("execution/reports/conflict/confirm")]
    public Task<ActionResult<DrillingDailyReportConflictReview>> ConfirmReportConflict([FromBody]DrillingDailyReportConflictAcknowledgement original)=>Answer(()=>service.ConfirmDailyReportConflictAsync(User,original));
    [HttpPost("execution/conflict")]
    public Task<ActionResult<DrillingExecutionConflictReview>> ExecutionConflict([FromBody]DrillingExecutionStart original)=>Answer(()=>service.ReadExecutionConflictAsync(User,original));
    [HttpPost("execution/conflict/confirm")]
    public Task<ActionResult<DrillingExecutionConflictReview>> ConfirmExecutionConflict([FromBody]DrillingExecutionConflictAcknowledgement original)=>Answer(()=>service.ConfirmExecutionConflictAsync(User,original));
    [HttpPost("execution/assignment/conflict")]
    public Task<ActionResult<DrillingExecutionAssignmentConflictReview>> AssignmentConflict([FromBody]DrillingExecutionAssignment original)=>Answer(()=>service.ReadExecutionAssignmentConflictAsync(User,original));
    [HttpPost("execution/assignment/conflict/confirm")]
    public Task<ActionResult<DrillingExecutionAssignmentConflictReview>> ConfirmAssignmentConflict([FromBody]DrillingExecutionAssignmentConflictAcknowledgement original)=>Answer(()=>service.ConfirmExecutionAssignmentConflictAsync(User,original));
    [HttpGet("execution/assignment")]
    public Task<ActionResult<DrillingExecutionAssignmentReview>> Assignment([FromQuery]string reviewId)=>Answer(()=>service.ReadExecutionAssignmentAsync(User,reviewId));
    [HttpPost("execution/assignment")]
    public Task<ActionResult<DrillingExecutionAssignmentReceipt>> Assign([FromBody]DrillingExecutionAssignment original)=>Answer(()=>service.AssignExecutionAsync(User,original));
    [HttpPost("execution/assignment/recover")]
    public Task<ActionResult<DrillingExecutionAssignmentReceipt>> RecoverAssignment([FromBody]DrillingExecutionAssignment original)=>Answer(()=>service.AssignExecutionAsync(User,original,true));
    [HttpGet("execution/assigned")]
    public Task<ActionResult<DrillingExecutionAssignedWork>> AssignedExecution([FromQuery]string processId)=>Answer(()=>service.ReadAssignedExecutionAsync(User,processId));
    [HttpGet("execution/reports")]
    public Task<ActionResult<DrillingDailyReportPage>> Reports([FromQuery]string reviewId,[FromQuery]int after=0,[FromQuery]int size=DrillingDailyReportPage.DefaultPageSize,[FromQuery]string? historyRevision=null)=>Answer(()=>service.ReadDailyReportPageAsync(User,reviewId,after,size,historyRevision));
    [HttpGet("execution/reports/basis")]
    public Task<ActionResult<DrillingDailyReportBasis>> ReportBasis([FromQuery]string reviewId)=>Answer(()=>service.ReadDailyReportBasisAsync(User,reviewId));
    [HttpPost("execution/reports")]
    public Task<ActionResult<DrillingDailyReportReceipt>> RecordReport([FromBody]DrillingDailyReportInput original)=>Answer(()=>service.RecordDailyReportAsync(User,original));
    [HttpPost("execution/reports/recover")]
    public Task<ActionResult<DrillingDailyReportReceipt>> RecoverReport([FromBody]DrillingDailyReportInput original)=>Answer(()=>service.RecordDailyReportAsync(User,original,true));
    [HttpGet("basis")]
    public Task<ActionResult<DrillingProgramBasis>> Basis([FromQuery]string processId,[FromQuery]string afeId,[FromQuery]string wellUwi)=>Answer(()=>service.ReviewAsync(User,processId,afeId,wellUwi));
    [HttpGet("drafts")]
    public Task<ActionResult<DrillingProgramDraftList>> List([FromQuery]string processId,[FromQuery]string afeId,[FromQuery]string wellUwi)=>Answer(()=>service.ListAsync(User,processId,afeId,wellUwi));
    [HttpGet("review")]
    public Task<ActionResult<DrillingProgramReviewReadiness>> Review([FromQuery]string processId,[FromQuery]string afeId,[FromQuery]string wellUwi,[FromQuery]string programId,[FromQuery]string programRevision)=>Answer(()=>service.ReviewRoutingAsync(User,processId,afeId,wellUwi,programId,programRevision));
    [HttpGet("execution/completion/progress")]
    public Task<ActionResult<DrillingCompletionReviewProgress>> CompletionProgress([FromQuery]string processId)=>Answer(()=>service.ReadCompletionProgressAsync(User,processId));
    [HttpGet("execution/reporting/status")]
    public Task<ActionResult<DrillingReportingClosureStatus>> ReportingStatus([FromQuery]string assignmentId)=>Answer(()=>service.ReadReportingResponsibilityAsync(User,assignmentId));
    [HttpGet("execution/reporting/transfer/catalog")]
    public Task<ActionResult<DrillingReportingReassignmentCatalog>> ReportingTransferCatalog([FromQuery]string assignmentId)=>Answer(()=>service.ReadReportingReassignmentCatalogAsync(User,assignmentId));
    [HttpPost("execution/reporting/transfer/review")]
    public Task<ActionResult<DrillingReportingReassignmentReview>> ReportingTransferReview([FromBody]DrillingReportingReassignmentSelection selection)=>Answer(()=>service.ReviewReportingReassignmentAsync(User,selection));
    [HttpPost("execution/reporting/transfer")]
    public Task<ActionResult<DrillingReportingReassignmentReceipt>> TransferReporting([FromBody]DrillingReportingReassignment original)=>Answer(()=>service.ReassignReportingAsync(User,original));
    [HttpPost("execution/reporting/transfer/recover")]
    public Task<ActionResult<DrillingReportingReassignmentReceipt>> RecoverReportingTransfer([FromBody]DrillingReportingReassignment original)=>Answer(()=>service.ReassignReportingAsync(User,original,true));
    [HttpGet("execution/reporting/closure/review")]
    public Task<ActionResult<DrillingReportingClosureReview>> ReportingClosureReview([FromQuery]string assignmentId,[FromQuery]string completionId)=>Answer(()=>service.ReviewReportingClosureAsync(User,assignmentId,completionId));
    [HttpPost("execution/reporting/closure")]
    public Task<ActionResult<DrillingReportingClosureReceipt>> CloseReporting([FromBody]DrillingReportingClosure original)=>Answer(()=>service.CloseReportingAsync(User,original));
    [HttpPost("execution/reporting/closure/recover")]
    public Task<ActionResult<DrillingReportingClosureReceipt>> RecoverReportingClosure([FromBody]DrillingReportingClosure original)=>Answer(()=>service.CloseReportingAsync(User,original,true));
    [HttpPost("execution/completion/test/inspect")]
    public Task<ActionResult<DrillingCompletionTestInspection>> InspectCompletionTest([FromBody]DrillingCompletionTestSelection selection)=>Answer(()=>service.InspectCompletionTestAsync(User,selection));
    [HttpPost("execution/completion/decision")]
    public Task<ActionResult<DrillingCompletionReviewDecisionReceipt>> CompletionDecision([FromBody]DrillingCompletionReviewDecision original)=>Answer(()=>service.DecideCompletionReviewAsync(User,original));
    [HttpPost("execution/completion/decision/recover")]
    public Task<ActionResult<DrillingCompletionReviewDecisionReceipt>> RecoverCompletionDecision([FromBody]DrillingCompletionReviewDecision original)=>Answer(()=>service.DecideCompletionReviewAsync(User,original,true));
    [HttpGet("execution/basis")]
    public Task<ActionResult<DrillingExecutionBasis>> ExecutionBasis([FromQuery]string reviewId)=>Answer(()=>service.ReadExecutionBasisAsync(User,reviewId));
    [HttpGet("execution")]
    public Task<ActionResult<DrillingExecutionStatus>> Execution([FromQuery]string reviewId)=>Answer(()=>service.ReadExecutionAsync(User,reviewId));
    [HttpPost("execution/start")]
    public Task<ActionResult<DrillingExecutionStartReceipt>> StartExecution([FromBody]DrillingExecutionStart original)=>Answer(()=>service.StartExecutionAsync(User,original));
    [HttpPost("execution/recover")]
    public Task<ActionResult<DrillingExecutionStartReceipt>> RecoverExecution([FromBody]DrillingExecutionStart original)=>Answer(()=>service.StartExecutionAsync(User,original,true));
    [HttpGet("review/progress")]
    public Task<ActionResult<DrillingProgramReviewProgress>> Progress([FromQuery]string processId)=>Answer(()=>service.ReadProgramProgressAsync(User,processId));
    [HttpGet("review/assigned")]
    public Task<ActionResult<DrillingProgramAssignedReview>> AssignedReview([FromQuery]string processId)=>Answer(()=>service.ReadAssignedProgramAsync(User,processId));
    [HttpPost("review/decision")]
    public Task<ActionResult<DrillingProgramReviewDecisionReceipt>> Decision([FromBody]DrillingProgramReviewDecision original)=>Answer(()=>service.DecideProgramAsync(User,original));
    [HttpPost("review/decision/recover")]
    public Task<ActionResult<DrillingProgramReviewDecisionReceipt>> RecoverDecision([FromBody]DrillingProgramReviewDecision original)=>Answer(()=>service.DecideProgramAsync(User,original,true));
    [HttpPost("review/start")]
    public Task<ActionResult<DrillingProgramReviewStartReceipt>> StartReview([FromBody]DrillingProgramReviewStart original)=>Answer(()=>service.SubmitReviewAsync(User,original));
    [HttpPost("review/recover")]
    public Task<ActionResult<DrillingProgramReviewStartReceipt>> RecoverReview([FromBody]DrillingProgramReviewStart original)=>Answer(()=>service.SubmitReviewAsync(User,original,true));
    [HttpPost("drafts")]
    public Task<ActionResult<DrillingProgramDraftReceipt>> Record([FromBody]CreateDrillingProgramDraft original)=>Answer(()=>service.RecordAsync(User,original));
    [HttpPost("drafts/recover")]
    public Task<ActionResult<DrillingProgramDraftReceipt>> Recover([FromBody]CreateDrillingProgramDraft original)=>Answer(()=>service.RecordAsync(User,original,true));
    private async Task<ActionResult<T>> Answer<T>(Func<Task<T>> action)
    {
        try{return Ok(await action());}
        catch(UnauthorizedAccessException){throw RefusalException.Forbidden("Current company drilling permission and original FIELD/well access are required.");}
        catch(InvalidOperationException error){failures.ReportHandled(error,"reviewing or recording drilling program","the exact original drilling program could not be confirmed",FailureSeverity.Degraded);throw RefusalException.Conflict("The approved funding, recorded well, schema or original program could not be confirmed. Preserve the retained input and recover its original outcome.");}
    }
}
