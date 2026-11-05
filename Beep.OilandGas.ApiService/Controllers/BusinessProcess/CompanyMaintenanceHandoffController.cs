using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.ProductionOperations.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.ApiService.Services;

namespace Beep.OilandGas.ApiService.Controllers.BusinessProcess;

[ApiController][Authorize][Route("api/field/current/maintenance-handoffs")][RequireCurrentFieldAccess("WRITE")]
public sealed class CompanyMaintenanceHandoffController(CompanyMaintenanceHandoffService service):ControllerBase
{
    [HttpPost("execution-permit/asset-return/review")]
    public async Task<ActionResult<CompanyMaintenanceAssetReturnReview>> MaintenanceAssetReturnReview(CompanyWorkOrderCompletionQuery query)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.ReviewMaintenanceAssetReturnAsync(User,query));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/asset-return")]
    public async Task<ActionResult<CompanyMaintenanceAssetReturnRecovery>> MaintenanceAssetReturn(CompanyMaintenanceAssetReturnCommand command)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.RecordMaintenanceAssetReturnAsync(User,command));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/asset-return/recovery/reference")]
    public async Task<ActionResult<CompanyMaintenanceAssetReturnRecovery>> MaintenanceAssetReturnRecovery(CompanyMaintenanceAssetReturnReference reference)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.RecoverMaintenanceAssetReturnAsync(User,reference));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/asset-return/evidence")]
    public async Task<ActionResult<CompanyMaintenanceAssetEvidence>> MaintenanceAssetEvidence(CompanyWorkOrderCompletionQuery query)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.ReadMaintenanceAssetEvidenceAsync(User,query));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/closure/review")]
    public async Task<ActionResult<CompanyMaintenanceClosureReview>> MaintenanceClosureReview(CompanyWorkOrderCompletionQuery query)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.ReviewMaintenanceClosureAsync(User,query));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/closure")]
    public async Task<ActionResult<CompanyMaintenanceClosureRecovery>> MaintenanceClosure(CompanyMaintenanceClosureCommand command)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.CloseMaintenanceAsync(User,command));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/closure/recovery/reference")]
    public async Task<ActionResult<CompanyMaintenanceClosureRecovery>> MaintenanceClosureRecovery(CompanyMaintenanceClosureReference reference)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.RecoverMaintenanceClosureAsync(User,reference));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/closure/status")]
    public async Task<ActionResult<CompanyMaintenanceClosureStatus>> MaintenanceClosureStatus(CompanyWorkOrderCompletionQuery query)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.ReadMaintenanceClosureStatusAsync(User,query));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/return-acceptance/report")]
    public async Task<ActionResult<CompanyWorkOrderCompletionQuery>> ReturnAcceptanceReport(CompanyMaintenanceReceivingQuery task)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.DiscoverReturnAcceptanceReportAsync(User,task));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/return-acceptance/review")]
    public async Task<ActionResult<CompanyWorkOrderReturnReview>> ReturnAcceptanceReview(CompanyWorkOrderCompletionQuery query)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.ReviewReturnAcceptanceAsync(User,query));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/return-acceptance")]
    public async Task<ActionResult<CompanyWorkOrderReturnRecovery>> ReturnAcceptance(CompanyWorkOrderReturnCommand command)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.RecordReturnAcceptanceAsync(User,command));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/return-acceptance/recovery/reference")]
    public async Task<ActionResult<CompanyWorkOrderReturnRecovery>> ReturnAcceptanceRecovery(CompanyWorkOrderReturnReference reference)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.RecoverReturnAcceptanceAsync(User,reference));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/return-acceptance/status")]
    public async Task<ActionResult<CompanyWorkOrderReturnStatus>> ReturnAcceptanceStatus(CompanyWorkOrderCompletionQuery query)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.ReadReturnAcceptanceStatusAsync(User,query));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/return-handoff")]
    public async Task<ActionResult<CompanyMaintenanceReturnHandoffRecovery>> ReturnHandoff(CompanyMaintenanceReturnHandoffCommand command)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.RecordReturnHandoffAsync(User,command));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/return-handoff/recovery/reference")]
    public async Task<ActionResult<CompanyMaintenanceReturnHandoffRecovery>> ReturnHandoffRecovery(CompanyMaintenanceReturnHandoffReference reference)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.RecoverReturnHandoffAsync(User,reference));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/return-handoff/review")]
    public async Task<ActionResult<CompanyMaintenanceReturnHandoffReview>> ReturnHandoffReview(CompanyWorkOrderCompletionQuery query)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.ReviewReturnHandoffAsync(User,query));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/completion-decision/status")]
    public async Task<ActionResult<CompanyWorkOrderCompletionDecisionStatus>> CompletionDecisionStatus(CompanyWorkOrderCompletionQuery query)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.ReadCompletionDecisionStatusAsync(User,query));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/completion-decision/review")]
    public async Task<ActionResult<CompanyWorkOrderCompletionDecisionReview>> CompletionDecisionReview(CompanyWorkOrderCompletionQuery query)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.ReviewCompletionDecisionAsync(User,query));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/completion-decision")]
    public async Task<ActionResult<CompanyWorkOrderCompletionDecisionRecovery>> CompletionDecision(CompanyWorkOrderCompletionDecisionCommand command)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.RecordCompletionDecisionAsync(User,command));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/completion-decision/recovery/reference")]
    public async Task<ActionResult<CompanyWorkOrderCompletionDecisionRecovery>> CompletionDecisionRecovery(CompanyWorkOrderCompletionDecisionReference reference)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.RecoverCompletionDecisionAsync(User,reference));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/completion-report")]
    public async Task<ActionResult<CompanyWorkOrderCompletionQuery>> CompletionReport(CompanyMaintenanceReceivingQuery task)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.DiscoverCompletionReportAsync(User,task));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/report-handoff/review")]
    public async Task<ActionResult<CompanyMaintenanceExecutionHandoffReview>> ExecutionHandoffReview(CompanyWorkOrderCompletionQuery query)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.ReviewExecutionHandoffAsync(User,query));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/report-handoff")]
    public async Task<ActionResult<CompanyMaintenanceExecutionHandoffRecovery>> ExecutionHandoff(CompanyMaintenanceExecutionHandoffCommand command)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.RecordExecutionHandoffAsync(User,command));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/report-handoff/recovery/reference")]
    public async Task<ActionResult<CompanyMaintenanceExecutionHandoffRecovery>> ExecutionHandoffRecovery(CompanyMaintenanceExecutionHandoffReference reference)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.RecoverExecutionHandoffAsync(User,reference));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/completion-review")]
    public async Task<ActionResult<CompanyWorkOrderCompletionReview>> CompletionReview(CompanyWorkOrderCompletionQuery query)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.ReviewCompletionAsync(User,query));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/labor-usage/review")]
    public async Task<ActionResult<CompanyWorkOrderLaborUsageReview>> LaborUsageReview(CompanyWorkOrderExecutionStartReference reference)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.ReviewLaborUsageAsync(User,reference));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/labor-usage")]
    public async Task<ActionResult<CompanyWorkOrderLaborUsageRecovery>> LaborUsage(CompanyWorkOrderLaborUsageCommand command)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.RecordLaborUsageAsync(User,command));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/labor-usage/recovery/reference")]
    public async Task<ActionResult<CompanyWorkOrderLaborUsageRecovery>> LaborUsageRecovery(CompanyWorkOrderLaborUsageReference reference)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.RecoverLaborUsageAsync(User,reference));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/material-usage/review")]
    public async Task<ActionResult<CompanyWorkOrderMaterialUsageReview>> MaterialUsageReview(CompanyWorkOrderExecutionStartReference reference)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.ReviewMaterialUsageAsync(User,reference));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/material-usage")]
    public async Task<ActionResult<CompanyWorkOrderMaterialUsageRecovery>> MaterialUsage(CompanyWorkOrderMaterialUsageCommand command)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.RecordMaterialUsageAsync(User,command));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/material-usage/recovery/reference")]
    public async Task<ActionResult<CompanyWorkOrderMaterialUsageRecovery>> MaterialUsageRecovery(CompanyWorkOrderMaterialUsageReference reference)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.RecoverMaterialUsageAsync(User,reference));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/work-submission/review")]
    public async Task<ActionResult<CompanyWorkOrderSubmissionReview>> SubmissionReview(CompanyWorkOrderExecutionStartReference reference)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.ReviewSubmissionAsync(User,reference));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/work-submission")]
    public async Task<ActionResult<CompanyWorkOrderSubmissionReceipt>> SubmitWork(CompanyWorkOrderSubmissionCommand command)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.SubmitWorkAsync(User,command));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/work-submission/recovery/reference")]
    public async Task<ActionResult<CompanyWorkOrderSubmissionRecovery>> SubmissionRecovery(CompanyWorkOrderSubmissionReference reference)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.RecoverSubmissionAsync(User,reference));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/work-submission/inspection-review")]
    public async Task<ActionResult<CompanyWorkOrderSubmissionInspectionReview>> SubmissionInspections(CompanyWorkOrderExecutionStartReference reference)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.ReviewSubmissionInspectionsAsync(User,reference));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/work-start/recorded")]
    public async Task<ActionResult<CompanyWorkOrderRecordedStart?>> RecordedExecutionStart(CompanyWorkOrderExecutionPermitReference query)
    {Response.Headers.CacheControl="no-store";try{return new JsonResult(await service.ReadRecordedExecutionStartAsync(User,query));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/work-start/review")]
    public async Task<ActionResult<CompanyWorkOrderExecutorIdentity>> ExecutionStartReview(CompanyWorkOrderExecutionPermitReference query)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.ReviewExecutionStartAsync(User,query));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/work-start")]
    public async Task<ActionResult<CompanyWorkOrderExecutionStartReceipt>> ExecutionStart(CompanyWorkOrderExecutionStartCommand command)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.StartExecutionAsync(User,command));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/work-start/recovery/reference")]
    public async Task<ActionResult<CompanyWorkOrderExecutionStartRecovery>> ExecutionStartRecovery(CompanyWorkOrderExecutionStartReference reference)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.RecoverExecutionStartAsync(User,reference));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/execution-prerequisites")]
    public async Task<ActionResult<CompanyWorkOrderExecutionPrerequisites>> ExecutionPrerequisites(CompanyWorkOrderExecutionPermitReference query)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.ReadExecutionPrerequisitesAsync(User,query));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/recorded-approval")]
    public async Task<ActionResult<CompanyWorkOrderRecordedApproval>> RecordedApproval(CompanyWorkOrderExecutionPermitReference query)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.ReadRecordedExecutionPermitApprovalAsync(User,query));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/execution-requirements")]
    public async Task<ActionResult<CompanyWorkOrderExecutionRequirements>> ExecutionRequirements(CompanyWorkOrderExecutionPermitReference query)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.ReadExecutionPermitRequirementsAsync(User,query));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/documents/contributors")]
    public async Task<ActionResult<CompanyWorkOrderDocumentContributorReview>> DocumentContributors(CompanyWorkOrderPermitDocumentQuery query)
    {Response.Headers.CacheControl="no-store";try{return Ok(await service.ReviewDocumentContributorsAsync(User,query));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}}
    [HttpPost("execution-permit/documents/content")]
    public async Task<ActionResult<CompanyWorkOrderPermitDocumentContent>> DocumentContent(CompanyWorkOrderPermitDocumentReview original)
    {
        Response.Headers.CacheControl="no-store";Response.Headers.Pragma="no-cache";
        try{return Ok(await service.ReadPermitDocumentContentAsync(User,original));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("execution-permit/documents/catalog")]
    public async Task<ActionResult<CompanyWorkOrderDocumentCatalog>> DocumentCatalog(CompanyWorkOrderExecutionPermitReference query)
    {
        try{return Ok(await service.ReviewPermitDocumentCatalogAsync(User,query));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("execution-permit/documents/source")]
    public async Task<ActionResult<CompanyWorkOrderPermitDocumentReview>> DocumentSource(CompanyWorkOrderPermitDocumentQuery query)
    {
        try{return Ok(await service.ReviewPermitDocumentSourceAsync(User,query));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("execution-permit/document-acceptance")]
    public Task<ActionResult<Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION>> RecordDocumentAcceptance(CompanyWorkOrderDocumentAcceptanceCommand command)=>DocumentAcceptance(command,false);
    [HttpPost("execution-permit/document-acceptance/recovery")]
    public Task<ActionResult<Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION>> RecoverDocumentAcceptance(CompanyWorkOrderDocumentAcceptanceCommand command)=>DocumentAcceptance(command,true);
    private async Task<ActionResult<Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION>> DocumentAcceptance(CompanyWorkOrderDocumentAcceptanceCommand command,bool readOnly)
    {
        Response.Headers.CacheControl="no-store";try{return Ok(await service.RecordDocumentAcceptanceAsync(User,command,readOnly));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("execution-permit/document-acceptance/recovery/reference")]
    public async Task<ActionResult<CompanyWorkOrderDocumentAcceptanceRecovery>> RecoverDocumentAcceptanceReference(CompanyWorkOrderDocumentAcceptanceReference reference)
    {
        Response.Headers.CacheControl="no-store";try{return Ok(await service.RecoverDocumentAcceptanceReferenceAsync(User,reference));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("execution-permit/personnel-acceptance")]
    public Task<ActionResult<Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION>> RecordPersonnelAcceptance(CompanyWorkOrderPersonnelAcceptanceCommand command)=>PersonnelAcceptance(command,false);
    [HttpPost("execution-permit/personnel-acceptance/recovery")]
    public Task<ActionResult<Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION>> RecoverPersonnelAcceptance(CompanyWorkOrderPersonnelAcceptanceCommand command)=>PersonnelAcceptance(command,true);
    private async Task<ActionResult<Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION>> PersonnelAcceptance(CompanyWorkOrderPersonnelAcceptanceCommand command,bool readOnly)
    {
        try{return Ok(await service.RecordPersonnelAcceptanceAsync(User,command,readOnly));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("execution-permit/personnel-acceptance/recovery/reference")]
    public async Task<ActionResult<CompanyWorkOrderPersonnelAcceptanceRecovery>> RecoverPersonnelAcceptanceReference(CompanyWorkOrderPersonnelAcceptanceReference reference)
    {
        try{return Ok(await service.RecoverPersonnelAcceptanceReferenceAsync(User,reference));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("execution-permit/personnel-assignments/policy-evidence")]
    public async Task<ActionResult<CompanyPersonnelPolicyEvidenceReview>> PersonnelPolicyEvidence(CompanyWorkOrderAssignedPersonnelQuery query)
    {
        try{return Ok(await service.ReviewPersonnelPolicyEvidenceAsync(User,query));}catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("execution-permit/personnel-releases")]
    public Task<ActionResult<WORK_ORDER_PERSONNEL_RELEASE>> RecordPersonnelRelease(CompanyWorkOrderPersonnelReleaseCommand command)=>PersonnelRelease(command,false);
    [HttpPost("execution-permit/personnel-releases/recovery")]
    public Task<ActionResult<WORK_ORDER_PERSONNEL_RELEASE>> RecoverPersonnelRelease(CompanyWorkOrderPersonnelReleaseCommand command)=>PersonnelRelease(command,true);
    private async Task<ActionResult<WORK_ORDER_PERSONNEL_RELEASE>> PersonnelRelease(CompanyWorkOrderPersonnelReleaseCommand command,bool readOnly)
    {
        try{return Ok(await service.RecordPersonnelReleaseAsync(User,command,readOnly));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("execution-permit/personnel-releases/recovery/reference")]
    public async Task<ActionResult<CompanyWorkOrderPersonnelReleaseRecovery>> RecoverPersonnelReleaseReference(CompanyWorkOrderPersonnelReleaseReference reference)
    {
        try{return Ok(await service.RecoverPersonnelReleaseReferenceAsync(User,reference));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("execution-permit/personnel-assignments/roster")]
    public async Task<ActionResult<CompanyWorkOrderPersonnelAssignmentRosterReview>> PersonnelAssignmentRoster(CompanyWorkOrderExecutionPermitReference query)
    {
        try{return Ok(await service.ReviewPersonnelAssignmentsAsync(User,query));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("execution-permit/personnel-assignments/current")]
    public async Task<ActionResult<CompanyWorkOrderAssignedPersonnelReview>> CurrentAssignedPersonnel(CompanyWorkOrderAssignedPersonnelQuery query)
    {
        try{return Ok(await service.ReviewAssignedPersonnelAsync(User,query));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("execution-permit/personnel-assignments/recovery/reference")]
    public async Task<ActionResult<CompanyWorkOrderPersonnelAssignmentRecovery>> RecoverPersonnelAssignmentReference(CompanyWorkOrderPersonnelAssignmentReference reference)
    {
        try{return Ok(await service.RecoverPersonnelAssignmentReferenceAsync(User,reference));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("execution-permit/personnel-assignments")]
    public Task<ActionResult<WORK_ORDER_PERSONNEL_ASSIGNMENT>> RecordPersonnelAssignment(CompanyWorkOrderPersonnelAssignmentCommand command)=>PersonnelAssignment(command,false);
    [HttpPost("execution-permit/personnel-assignments/recovery")]
    public Task<ActionResult<WORK_ORDER_PERSONNEL_ASSIGNMENT>> RecoverPersonnelAssignment(CompanyWorkOrderPersonnelAssignmentCommand command)=>PersonnelAssignment(command,true);
    private async Task<ActionResult<WORK_ORDER_PERSONNEL_ASSIGNMENT>> PersonnelAssignment(CompanyWorkOrderPersonnelAssignmentCommand command,bool readOnly)
    {
        try{return Ok(await service.RecordPersonnelAssignmentAsync(User,command,readOnly));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("execution-permit/personnel-source")]
    public async Task<ActionResult<CompanyWorkOrderPersonnelSourceReview>> ReviewPersonnelSource(CompanyWorkOrderPersonnelSourceQuery query)
    {
        try{return Ok(await service.ReviewPersonnelSourceAsync(User,query));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("execution-permit/material-acceptance/recovery/reference")]
    public async Task<ActionResult<CompanyWorkOrderMaterialAcceptanceRecovery>> RecoverMaterialAcceptanceReference(CompanyWorkOrderMaterialAcceptanceReference reference)
    {
        try{return Ok(await service.RecoverMaterialAcceptanceReferenceAsync(User,reference));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("execution-permit/material-acceptance")]
    public Task<ActionResult<Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION>> RecordMaterialAcceptance(CompanyWorkOrderMaterialAcceptanceCommand command)=>MaterialAcceptance(command,false);
    [HttpPost("execution-permit/material-acceptance/recovery")]
    public Task<ActionResult<Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION>> RecoverMaterialAcceptance(CompanyWorkOrderMaterialAcceptanceCommand command)=>MaterialAcceptance(command,true);
    private async Task<ActionResult<Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION>> MaterialAcceptance(CompanyWorkOrderMaterialAcceptanceCommand command,bool readOnly)
    {
        try{return Ok(await service.RecordMaterialAcceptanceAsync(User,command,readOnly));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("materials/catalog")]
    public async Task<ActionResult<CompanyWorkOrderMaterialCatalog>> ReviewMaterialCatalog(CompanyMaintenanceReceivingQuery query)
    {
        try{return Ok(await service.ReviewMaterialCatalogAsync(User,query));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("execution-permit/material-readiness")]
    public async Task<ActionResult<CompanyWorkOrderMaterialReadinessReview>> ReviewPermitMaterialReadiness(CompanyWorkOrderMaterialReadinessQuery query)
    {
        try{return Ok(await service.ReviewPermitMaterialReadinessAsync(User,query));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("materials/allocations/release/recovery/reference")]
    public async Task<ActionResult<CompanyWorkOrderMaterialReleaseRecovery>> RecoverMaterialReleaseReference(CompanyWorkOrderMaterialReleaseReferenceQuery query)
    {
        try{return Ok(await service.RecoverMaterialReleaseReferenceAsync(User,query));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("materials/allocations/release")]
    public Task<ActionResult<WORK_ORDER_MATERIAL_RELEASE>> ReleaseMaterialAllocation(CompanyWorkOrderMaterialReleaseCommand command)=>ReadOrReleaseMaterialAllocation(command,false);
    [HttpPost("materials/allocations/release/recovery")]
    public Task<ActionResult<WORK_ORDER_MATERIAL_RELEASE>> RecoverMaterialRelease(CompanyWorkOrderMaterialReleaseCommand command)=>ReadOrReleaseMaterialAllocation(command,true);
    private async Task<ActionResult<WORK_ORDER_MATERIAL_RELEASE>> ReadOrReleaseMaterialAllocation(CompanyWorkOrderMaterialReleaseCommand command,bool readOnly)
    {
        try{return Ok(await service.ReleaseMaterialAllocationAsync(User,command,readOnly));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("materials/allocations/recovery/reference")]
    public async Task<ActionResult<CompanyWorkOrderMaterialAllocationRecovery>> RecoverMaterialAllocationReference(CompanyWorkOrderMaterialAllocationReferenceQuery query)
    {
        try{return Ok(await service.RecoverMaterialAllocationReferenceAsync(User,query));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("materials/allocation-review")]
    public async Task<ActionResult<CompanyWorkOrderMaterialAllocationReview>> ReviewMaterialAllocation(CompanyWorkOrderMaterialSourceQuery query)
    {
        try{return Ok(await service.ReviewMaterialAllocationAsync(User,query));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("materials/allocations")]
    public Task<ActionResult<WORK_ORDER_MATERIAL_ALLOCATION>> RecordMaterialAllocation(CompanyWorkOrderMaterialAllocationCommand command)=>ReadOrRecordMaterialAllocation(command,false);
    [HttpPost("materials/allocations/recovery")]
    public Task<ActionResult<WORK_ORDER_MATERIAL_ALLOCATION>> RecoverMaterialAllocation(CompanyWorkOrderMaterialAllocationCommand command)=>ReadOrRecordMaterialAllocation(command,true);
    private async Task<ActionResult<WORK_ORDER_MATERIAL_ALLOCATION>> ReadOrRecordMaterialAllocation(CompanyWorkOrderMaterialAllocationCommand command,bool readOnly)
    {
        try{return Ok(await service.RecordMaterialAllocationAsync(User,command,readOnly));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("materials/source-review")]
    public async Task<ActionResult<CompanyWorkOrderMaterialSourceReview>> ReviewMaterialSource(CompanyWorkOrderMaterialSourceQuery query)
    {
        try{return Ok(await service.ReviewMaterialSourceAsync(User,query));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("execution-permit/decision")]
    public Task<ActionResult<CompanyWorkOrderExecutionPermitDecisionReceipt>> DecideExecutionPermit(CompanyWorkOrderExecutionPermitDecision decision)=>ReadOrDecidePermit(decision,false);
    [HttpPost("execution-permit/decision/recovery")]
    public Task<ActionResult<CompanyWorkOrderExecutionPermitDecisionReceipt>> RecoverExecutionPermitDecision(CompanyWorkOrderExecutionPermitDecision decision)=>ReadOrDecidePermit(decision,true);
    private async Task<ActionResult<CompanyWorkOrderExecutionPermitDecisionReceipt>> ReadOrDecidePermit(CompanyWorkOrderExecutionPermitDecision decision,bool readOnly)
    {
        try{return Ok(await service.DecideExecutionPermitAsync(User,decision,readOnly));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("execution-permit/decision/recovery/reference")]
    public async Task<ActionResult<CompanyWorkOrderExecutionPermitDecisionRecovery>> RecoverExecutionPermitDecisionReference(CompanyWorkOrderExecutionPermitDecisionReference query)
    {
        try{return Ok(await service.RecoverExecutionPermitDecisionReferenceAsync(User,query));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("execution-permit/approval/review")]
    public async Task<ActionResult<CompanyWorkOrderExecutionPermitApprovalReview>> ReviewExecutionPermitApproval(CompanyWorkOrderExecutionPermitReference query)
    {
        try{return Ok(await service.ReviewExecutionPermitApprovalAsync(User,query));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("execution-permit/recovery/reference")]
    public async Task<ActionResult<CompanyWorkOrderExecutionPermitRecovery>> RecoverExecutionPermitReference(CompanyWorkOrderExecutionPermitReference query)
    {
        try{return Ok(await service.RecoverExecutionPermitReferenceAsync(User,query));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("execution-permit/review")]
    public async Task<ActionResult<CompanyWorkOrderExecutionPermitIssueReview>> ReviewExecutionPermitIssue(CompanyMaintenanceReceivingQuery task)
    {
        try{return Ok(await service.ReviewExecutionPermitIssueAsync(User,task));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("execution-permit")]
    public Task<ActionResult<CompanyWorkOrderExecutionPermitReceipt>> IssueExecutionPermit(CompanyWorkOrderExecutionPermitIntent intent)=>ReadOrIssuePermit(intent,false);
    [HttpPost("execution-permit/recovery")]
    public Task<ActionResult<CompanyWorkOrderExecutionPermitReceipt>> RecoverExecutionPermit(CompanyWorkOrderExecutionPermitIntent intent)=>ReadOrIssuePermit(intent,true);
    private async Task<ActionResult<CompanyWorkOrderExecutionPermitReceipt>> ReadOrIssuePermit(CompanyWorkOrderExecutionPermitIntent intent,bool readOnly)
    {
        try{return Ok(await service.IssueExecutionPermitAsync(User,intent,readOnly));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("assigned-stage/inspections")]
    public async Task<ActionResult<CompanyMaintenanceInspectionReview>> ReviewAssignedInspections(CompanyMaintenanceReceivingQuery task)
    {
        try{return Ok(await service.ReviewAssignedInspectionsAsync(User,task));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("assigned-stage/review")]
    public async Task<ActionResult<CompanyMaintenanceAssignedStageReview>> ReviewAssignedStage(CompanyMaintenanceReceivingQuery task)
    {
        try{return Ok(await service.ReviewAssignedStageAsync(User,task));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("acceptance/task-review")]
    public async Task<ActionResult<CompanyMaintenanceAcceptanceReview>> ReviewReceivingTask(CompanyMaintenanceReceivingQuery task)
    {
        try{return Ok(await service.ReviewReceivingTaskAsync(User,task));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("acceptance/recovery/reference")]
    public async Task<ActionResult<CompanyMaintenanceAcceptanceRecovery>> RecoverAcceptanceReference(CompanyMaintenanceAcceptanceReferenceQuery query)
    {
        try{return Ok(await service.RecoverAcceptanceReferenceAsync(User,query));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("acceptance/review")]
    public async Task<ActionResult<CompanyMaintenanceAcceptanceReview>> ReviewAcceptance(CompanyMaintenanceRecoveryQuery query)
    {
        try{return Ok(await service.ReviewAcceptanceAsync(User,query));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("acceptance")]
    public async Task<ActionResult<CompanyMaintenanceAcceptanceReceipt>> Accept(CompanyMaintenanceAcceptanceCommand command)
    {
        try{return Ok(await service.AcceptAsync(User,command));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("acceptance/recovery")]
    public async Task<ActionResult<CompanyMaintenanceAcceptanceReceipt>> RecoverAcceptance(CompanyMaintenanceAcceptanceCommand command)
    {
        try{return Ok(await service.RecoverAcceptanceAsync(User,command));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("choices")]
    public async Task<ActionResult<CompanyMaintenanceStartCatalog>> Choices(CompanyMaintenanceSourceQuery query)
    {
        try{return Ok(await service.CatalogAsync(User,query));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("recovery/reference")]
    public async Task<ActionResult<CompanyMaintenanceRecovery>> RecoverReference(CompanyMaintenanceRecoveryQuery query)
    {
        try{return Ok(await service.RecoverReferenceAsync(User,query));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost("review")]
    public async Task<ActionResult<CompanyMaintenanceProcessStartReview>> Review(CompanyMaintenanceProcessStartQuery query)
    {
        try{return Ok(await service.ReviewAsync(User,query));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    [HttpPost]
    public async Task<ActionResult<CompanyMaintenanceProcessStartReceipt>> Start(CompanyMaintenanceProcessStart intent)
    {
        try{return Ok(await service.StartAsync(User,intent));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
    // POST carries the bounded retained intent; this endpoint performs no persistent mutation.
    [HttpPost("recovery")]
    public async Task<ActionResult<CompanyMaintenanceProcessStartReceipt>> Recover(CompanyMaintenanceProcessStart intent)
    {
        try{return Ok(await service.RecoverAsync(User,intent));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}
        catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
}
