using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Controllers.HSE;

/// <summary>Reviews and starts the original incident workflow under the current company and FIELD.</summary>
[ApiController]
[Route("api/field/current/hse/workflow")]
[Authorize(Policy = PermissionConstants.HSE.ManageIncidents)]
[RequireCurrentFieldAccess(PermissionConstants.HSE.ManageIncidents, true)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class IncidentWorkflowController(CompanyHseWorkflowService workflow) : ControllerBase
{
    [HttpGet("closure/resolution-review")]
    [Authorize(Policy = PermissionConstants.Workflow.Approve)]
    public async Task<ActionResult<IncidentClosureResolutionReview>> ReviewClosureResolutionAsync([FromQuery] string incidentId)
    {
        if (!LeaseSummaryValidator.Identifier(incidentId)) throw RefusalException.Invalid("An exact incident is required.");
        try { return Ok(await workflow.ReviewClosureResolutionAsync(User, incidentId)); } catch (UnauthorizedAccessException) { return Forbid(); }
    }
    [HttpPost("closure/resolve-native-history")]
    [Authorize(Policy = PermissionConstants.Workflow.Approve)]
    public async Task<ActionResult<IncidentClosedReceipt>> ResolveClosureAsync([FromBody] IncidentClosureResolution original)
    {
        if (original is null) throw RefusalException.Invalid("The original native resolution is required.");
        try { original.Require(); } catch (InvalidOperationException) { throw RefusalException.Invalid("The original native resolution is invalid."); }
        try { return Ok(await workflow.ResolveClosureAsync(User, original)); } catch (UnauthorizedAccessException) { return Forbid(); }
    }
    [HttpGet("closure/review")]
    [Authorize(Policy = PermissionConstants.Workflow.Approve)]
    public async Task<ActionResult<CompanyHseClosureReview>> ReviewClosureStageAsync([FromQuery] string incidentId)
    {
        try { return Ok(await workflow.ReviewClosureStageAsync(User, incidentId)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }
    [HttpPost("closure/complete-stage")]
    [Authorize(Policy = PermissionConstants.Workflow.Approve)]
    public async Task<ActionResult<CompanyHseClosureReceipt>> CompleteClosureStageAsync([FromBody] CompanyHseClosureCommand original)
    {
        if (original is null) throw RefusalException.Invalid("The original reviewed closure command is required.");
        try { original.Require(); } catch (InvalidOperationException) { throw RefusalException.Invalid("The original closure command is invalid."); }
        try { return Ok(await workflow.CompleteClosureStageAsync(User, original)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }
    [HttpPost("closure/record-native")]
    [Authorize(Policy = PermissionConstants.Workflow.Approve)]
    public async Task<ActionResult<CompanyHseClosureOutcome>> RecordApprovedIncidentClosedAsync([FromBody] CompanyHseClosureRecovery approved)
    {
        if (approved is null) throw RefusalException.Invalid("The recorded lifecycle approval is required.");
        try { approved.Require(); } catch (InvalidOperationException) { throw RefusalException.Invalid("The recorded lifecycle approval is invalid."); }
        try { return Ok(await workflow.RecordApprovedIncidentClosedAsync(User, approved)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }
    [HttpGet("closure/correction-review")]
    [Authorize(Policy = PermissionConstants.Workflow.Approve)]
    public async Task<ActionResult<CompanyHseClosureReview>> ReviewCorrectionReturnAsync([FromQuery] string incidentId)
    {
        try { return Ok(await workflow.ReviewCorrectionReturnAsync(User, incidentId)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }
    [HttpPost("closure/return-for-correction")]
    [Authorize(Policy = PermissionConstants.Workflow.Approve)]
    public async Task<ActionResult<CompanyHseCorrectionReceipt>> ReturnForCorrectionAsync([FromBody] CompanyHseCorrectionCommand original)
    {
        if (original is null) throw RefusalException.Invalid("The original reviewed rejection return is required.");
        try { original.Require(); } catch (InvalidOperationException) { throw RefusalException.Invalid("The correction return command is invalid."); }
        try { return Ok(await workflow.ReturnForCorrectionAsync(User, original)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }
    [HttpGet("closure/recovery")]
    [Authorize(Policy = PermissionConstants.Workflow.Approve)]
    public async Task<ActionResult<CompanyHseClosureRecovery>> RecoverClosureStageAsync([FromQuery] string incidentId)
    {
        try { var recovered = await workflow.RecoverClosureStageAsync(User, incidentId); return recovered is null ? NotFound() : Ok(recovered); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }
    [HttpGet("review")]
    public async Task<ActionResult<CompanyHseWorkflowReview>> ReviewAsync([FromQuery] string incidentId)
    {
        if (!LeaseSummaryValidator.Identifier(incidentId)) throw RefusalException.Invalid("An exact incident is required.");
        try { return Ok(await workflow.ReviewAsync(User, incidentId)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPost("start")]
    public async Task<ActionResult<CompanyHseWorkflowReceipt>> StartAsync([FromBody] CompanyHseWorkflowStart original)
    {
        if (original is null) throw RefusalException.Invalid("The reviewed workflow request is required.");
        try { original.Require(); }
        catch (InvalidOperationException) { throw RefusalException.Invalid("The reviewed workflow request is invalid."); }
        try { return Ok(await workflow.StartAsync(User, original)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpGet("rca/review")]
    public async Task<ActionResult<IncidentRcaSourceReview>> ReviewRcaAsync([FromQuery] string incidentId)
    {
        if (!LeaseSummaryValidator.Identifier(incidentId)) throw RefusalException.Invalid("An exact incident is required.");
        try { return Ok(await workflow.ReviewRcaAsync(User, incidentId)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPost("rca/record-cause")]
    public async Task<ActionResult<IncidentRcaReceipt>> RecordRcaAsync([FromBody] IncidentRcaIntent original)
    {
        if (original is null) throw RefusalException.Invalid("The original reviewed cause is required.");
        try { original.Require(); }
        catch (InvalidOperationException) { throw RefusalException.Invalid("The original reviewed cause is invalid."); }
        try { return Ok(await workflow.RecordRcaAsync(User, original)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpGet("rca/recorded")]
    public async Task<ActionResult<IReadOnlyList<IncidentRcaIntent>>> ReadRecordedRcaAsync([FromQuery] string incidentId)
    {
        if (!LeaseSummaryValidator.Identifier(incidentId)) throw RefusalException.Invalid("An exact incident is required.");
        try { return Ok(await workflow.ReadRecordedRcaAsync(User, incidentId)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpGet("rca/shared")]
    public async Task<ActionResult<IReadOnlyList<IncidentRcaRecordedCause>>> ReadSharedRcaAsync([FromQuery] string incidentId)
    {
        if (!LeaseSummaryValidator.Identifier(incidentId)) throw RefusalException.Invalid("An exact incident is required.");
        try { return Ok(await workflow.ReadSharedRcaAsync(User, incidentId)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpGet("rca/handoff-review")]
    public async Task<ActionResult<CompanyHseRcaHandoffReview>> ReviewRcaHandoffAsync([FromQuery] string incidentId, [FromQuery] Guid intentId)
    {
        if (!LeaseSummaryValidator.Identifier(incidentId) || intentId == Guid.Empty) throw RefusalException.Invalid("An exact incident and recorded cause are required.");
        try { return Ok(await workflow.ReviewRcaHandoffAsync(User, incidentId, intentId)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpPost("rca/handoff")]
    public async Task<ActionResult<CompanyHseRcaHandoffReceipt>> CompleteRcaHandoffAsync([FromBody] CompanyHseRcaHandoffCommand original)
    {
        if (original is null) throw RefusalException.Invalid("The original RCA handoff is required.");
        try { original.Require(); }
        catch (InvalidOperationException) { throw RefusalException.Invalid("The original RCA handoff is invalid."); }
        try { return Ok(await workflow.CompleteRcaHandoffAsync(User, original)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    [HttpGet("rca/handoff-recovery")]
    public async Task<ActionResult<CompanyHseRcaRecoveredHandoff>> RecoverRcaHandoffAsync([FromQuery] string incidentId)
    {
        if (!LeaseSummaryValidator.Identifier(incidentId)) throw RefusalException.Invalid("An exact incident is required.");
        try { var result = await workflow.RecoverRcaHandoffAsync(User, incidentId); return result is null ? NotFound() : Ok(result); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }
}
