using Beep.OilandGas.Models.Core.Refusals;
using TheTechIdea.Data.OilGas;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.ApiService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.Models.Data.Security;

namespace Beep.OilandGas.ApiService.Controllers.HSE;

[ApiController]
[Route("api/field/current/hse")]
[Authorize]
[RequireCurrentFieldAccess]
public class HSEController : ControllerBase
{
    private readonly IFieldOrchestrator _fieldOrchestrator;
    private readonly ILogger<HSEController> _logger;
    private readonly RepositoryRequestAccount _accounts;

    public HSEController(IFieldOrchestrator fieldOrchestrator, RepositoryRequestAccount accounts, ILogger<HSEController> logger)
    {
        _fieldOrchestrator = fieldOrchestrator;
        _accounts = accounts ?? throw new ArgumentNullException(nameof(accounts));
        _logger = logger;
    }

    private IFieldHSEService Hse => _fieldOrchestrator.GetHSEService();

    // ── Incidents ──────────────────────────────────────────────────────────────

    [HttpGet("incidents")]
    public async Task<ActionResult<List<HSEIncidentRecord>>> GetIncidentsAsync(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        DateRangeFilter? range = (from.HasValue && to.HasValue)
            ? new DateRangeFilter(from.Value, to.Value)
            : null;

        if (string.IsNullOrWhiteSpace(_fieldOrchestrator.CurrentFieldId)) return BadRequest(new { error = "Current field is required." });
        return Ok(await Hse.GetIncidentsAsync(range));
    }

    [HttpGet("incident")]
    public async Task<ActionResult<HSEIncidentRecord>> GetIncidentAsync([FromQuery] string incidentId)
    {
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        var result = await Hse.GetIncidentAsync(incidentId);
        if (result is null) return NotFound(new { error = $"Incident {incidentId} not found." });
        return Ok(result);
    }

    [HttpPost("incidents/{incidentId}/transition")]
    public async Task<ActionResult> TransitionAsync(
        string incidentId, [FromBody] TransitionIncidentRequest request)
    {
        if (request?.Trigger is "close" or "ca_done")
            throw RefusalException.Forbidden("Incident verification and closure require reviewed lifecycle commands.");
        var userId = await _accounts.RequireAsync(HttpContext.RequestAborted);
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        if (request is null) return BadRequest(new { error = "Request body is required." });
        var ok = await Hse.TransitionAsync(incidentId, request.Trigger, request.Reason, userId);
        return ok ? NoContent() : BadRequest(new { error = "Invalid transition." });
    }

    [HttpPut("incidents/{incidentId}/tier")]
    public async Task<ActionResult> UpdateTierAsync(
        string incidentId, [FromQuery] int tier)
    {
        var userId = await _accounts.RequireAsync(HttpContext.RequestAborted);
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        await Hse.UpdateTierAsync(incidentId, tier, userId);
        return NoContent();
    }

    [HttpGet("incident/investigator-review")]
    public async Task<ActionResult<InvestigatorAssignmentReview>> ReviewInvestigatorAssignmentAsync([FromQuery] string incidentId)
    {
        _ = await _accounts.RequireAsync(HttpContext.RequestAborted);
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        return Ok(await Hse.ReviewInvestigatorAssignmentAsync(incidentId));
    }

    [HttpPut("incident/investigator")]
    public async Task<ActionResult> AssignInvestigatorAsync(
        [FromQuery] string incidentId, [FromBody] AssignIncidentInvestigatorRequest request)
    {
        var userId = await _accounts.RequireAsync(HttpContext.RequestAborted);
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        if (request is null) return BadRequest(new { error = "A reviewed assignment body is required." });
        await Hse.AssignInvestigatorAsync(incidentId, request, userId);
        return NoContent();
    }

    // ── RCA ────────────────────────────────────────────────────────────────────

    [HttpGet("incident/causes")]
    public async Task<ActionResult<List<CauseFinding>>> GetCausesAsync([FromQuery] string incidentId)
    {
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        return Ok(await Hse.GetCausesAsync(incidentId));
    }

    [HttpPost("incidents/{incidentId}/causes")]
    public async Task<ActionResult> AddCauseAsync(
        string incidentId, [FromBody] AddCauseRequest request)
    {
        var userId = await _accounts.RequireAsync(HttpContext.RequestAborted);
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        await Hse.AddCauseAsync(incidentId, request, userId);
        return NoContent();
    }

    [HttpGet("incidents/{incidentId}/rca-complete")]
    public async Task<ActionResult<bool>> IsRCACompleteAsync(string incidentId)
    {
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        return Ok(await Hse.IsRcaCompleteAsync(incidentId));
    }

    // ── Barriers ───────────────────────────────────────────────────────────────

    [HttpGet("incidents/{incidentId}/barriers")]
    public async Task<ActionResult<List<BarrierRecord>>> GetBarriersAsync(string incidentId)
    {
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        return Ok(await Hse.GetBarriersAsync(incidentId));
    }

    [HttpPost("incidents/{incidentId}/barriers")]
    public async Task<ActionResult> AddBarrierAsync(
        string incidentId, [FromBody] AddBarrierRequest request)
    {
        var userId = await _accounts.RequireAsync(HttpContext.RequestAborted);
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        await Hse.AddBarrierAsync(incidentId, request, userId);
        return NoContent();
    }

    [HttpPut("incidents/{incidentId}/barriers/{equipId}/status")]
    public async Task<ActionResult> SetBarrierStatusAsync(
        string incidentId, string equipId, [FromQuery] string status)
    {
        var userId = await _accounts.RequireAsync(HttpContext.RequestAborted);
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        if (string.IsNullOrWhiteSpace(equipId)) return BadRequest(new { error = "Equipment ID is required." });
        if (string.IsNullOrWhiteSpace(status)) return BadRequest(new { error = "Status is required." });
        await Hse.SetBarrierStatusAsync(incidentId, equipId, status, userId);
        return NoContent();
    }

    [HttpGet("incidents/{incidentId}/barriers/summary")]
    public async Task<ActionResult<BarrierSummary>> GetBarrierSummaryAsync(string incidentId)
    {
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        return Ok(await Hse.GetBarrierSummaryAsync(incidentId));
    }

    // ── Corrective Actions ─────────────────────────────────────────────────────

    [HttpGet("incident/cas")]
    public async Task<ActionResult<List<CAStatus>>> GetCAsAsync([FromQuery] string incidentId)
    {
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        return Ok(await Hse.GetCorrectiveActionsAsync(incidentId));
    }

    [HttpGet("incident/ca-plan-review")]
    public async Task<ActionResult<CAPlanCreationReview>> ReviewCaPlanCreationAsync([FromQuery] string incidentId)
    {
        await _accounts.RequireAsync();
        if (string.IsNullOrWhiteSpace(incidentId)) throw RefusalException.Invalid("incidentId is required.");
        return Ok(await Hse.ReviewCaPlanCreationAsync(incidentId));
    }

    [HttpGet("incident/ca-plan")]
    public async Task<ActionResult<CAPlanResolution>> GetCAPlanAsync([FromQuery] string incidentId)
    {
        await _accounts.RequireAsync(HttpContext.RequestAborted);
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        return Ok(await Hse.GetCaPlanAsync(incidentId));
    }

    [HttpPost("incident/ca-plan")]
    public async Task<ActionResult<CAPlanResolution>> CreateCAPlanAsync([FromQuery] string incidentId, [FromBody] CreateCAPlanSubmission submission)
    {
        var userId = await _accounts.RequireAsync(HttpContext.RequestAborted);
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        return Ok(await Hse.CreateCaPlanAsync(incidentId, submission, userId));
    }

    [HttpGet("incidents/{incidentId}/restrictions/review")]
    public async Task<ActionResult<HseRestrictionReview>> ReviewRestrictionsAsync(string incidentId)
    {
        await _accounts.RequireAsync(HttpContext.RequestAborted);
        if (string.IsNullOrWhiteSpace(incidentId)) return BadRequest(new { error = "Incident ID is required." });
        return Ok(await Hse.ReviewIncidentRestrictionsAsync(incidentId));
    }

    [HttpPost("incidents/{incidentId}/cas")]
    [Authorize(Policy = PermissionConstants.HSE.ManageIncidents)]
    [RequireCurrentFieldAccess(PermissionConstants.HSE.ManageIncidents, true)]
    public async Task<ActionResult<AddCAReceipt>> AddCAAsync(
        string incidentId, [FromBody] AddCASubmission submission, [FromServices] ICompanyFieldSelectionService selections,
        [FromServices] ICompanyCorrectiveActionCommands commands)
    {
        var userId = await _accounts.RequireAsync(HttpContext.RequestAborted);
        if (string.IsNullOrWhiteSpace(incidentId) || submission?.Context is null || submission.Request is null ||
            !LeaseSummaryValidator.Identifier(submission.PlanId) || !ProcessDefinitionRevision.IsValid(submission.OptionsRevision)) return BadRequest(new { error = "Incident, plan and reviewed action choices are required." });
        async Task RequireSelection()
        {
            var current = await selections.GetCurrentFieldSelectionAsync();
            FieldSelectionReviewValidator.RequireCurrent(current, submission.Context.Company, userId);
            if (!current.SelectionAvailable || current.SelectionContext != submission.Context || _fieldOrchestrator.CurrentFieldId != submission.Context.FieldId)
                throw new UnauthorizedAccessException("The original company or FIELD selection changed.");
        }
        await RequireSelection();
        var plan = await Hse.GetCaPlanAsync(incidentId);
        if (plan.Context != submission.Context || plan.IncidentId != incidentId || plan.PlanId != submission.PlanId)
            throw new InvalidOperationException("The reviewed corrective plan changed.");
        var recorded = await commands.FindRecordedActionAsync(submission.Context, incidentId, plan.PlanId, submission.Request, userId);
        if (recorded is not null)
        {
            await RequireSelection();
            if (await Hse.GetCaPlanAsync(incidentId) != plan || recorded != submission.Request.ActionId.ToString("N"))
                throw new InvalidOperationException("The recorded corrective action could not be confirmed.");
            return Ok(new AddCAReceipt(submission.Context, incidentId, submission.Request.ActionId, recorded));
        }
        var options = await commands.ReviewActionOptionsAsync(submission.Context, incidentId, plan.PlanId, userId);
        options.Require();
        if (options.Context != submission.Context || options.IncidentId != incidentId || options.PlanId != plan.PlanId || options.Revision != submission.OptionsRevision ||
            !options.ActionTypes.Contains(submission.Request.CAType, StringComparer.Ordinal) || !options.Participants.Contains(submission.Request.ResponsibleBaId, StringComparer.Ordinal))
            throw new InvalidOperationException("The reviewed corrective-action choices changed.");
        await RequireSelection();
        var id = await commands.AddCorrectiveActionAsync(submission.Context, incidentId, plan.PlanId, submission.Request, userId);
        await RequireSelection();
        if (await Hse.GetCaPlanAsync(incidentId) != plan) throw new InvalidOperationException("The corrective plan changed while the action was recorded.");
        if (id != submission.Request.ActionId.ToString("N")) throw new InvalidOperationException("The recorded action does not match the original request.");
        return Ok(new AddCAReceipt(submission.Context, incidentId, submission.Request.ActionId, id));
    }

    [HttpGet("incidents/{incidentId}/cas/options")]
    [Authorize(Policy = PermissionConstants.HSE.ManageIncidents)]
    [RequireCurrentFieldAccess(PermissionConstants.HSE.ManageIncidents, true)]
    public async Task<ActionResult<CorrectiveActionOptions>> ReviewCAOptionsAsync(string incidentId,
        [FromServices] ICompanyFieldSelectionService selections, [FromServices] ICompanyCorrectiveActionCommands commands)
    {
        var actor = await _accounts.RequireAsync(HttpContext.RequestAborted);
        if (!LeaseSummaryValidator.Identifier(incidentId)) return BadRequest(new { error = "An exact incident is required." });
        var current = await selections.GetCurrentFieldSelectionAsync();
        var context = current.SelectionContext ?? throw new InvalidOperationException("The company FIELD selection is unavailable.");
        FieldSelectionReviewValidator.RequireCurrent(current, context.Company, actor);
        if (!current.SelectionAvailable || _fieldOrchestrator.CurrentFieldId != context.FieldId) throw new UnauthorizedAccessException("The selected FIELD changed.");
        var plan = await Hse.GetCaPlanAsync(incidentId);
        if (plan.Context != context || plan.IncidentId != incidentId || !LeaseSummaryValidator.Identifier(plan.PlanId))
            throw new InvalidOperationException("The incident corrective plan could not be confirmed.");
        var result = await commands.ReviewActionOptionsAsync(context, incidentId, plan.PlanId, actor);
        result.Require();
        var confirmedPlan = await Hse.GetCaPlanAsync(incidentId);
        var closing = await selections.GetCurrentFieldSelectionAsync();
        FieldSelectionReviewValidator.RequireCurrent(closing, context.Company, actor);
        if (!closing.SelectionAvailable || closing.SelectionContext != context || _fieldOrchestrator.CurrentFieldId != context.FieldId ||
            result.Context != context || result.IncidentId != incidentId || result.PlanId != plan.PlanId || confirmedPlan != plan)
            throw new InvalidOperationException("The corrective-action choices changed during review.");
        return Ok(result);
    }

    [HttpPut("incidents/{incidentId}/cas/{stepSeq}/responsible")]
    public async Task<ActionResult> AssignCAResponsibleAsync(
        string incidentId, int stepSeq, [FromBody] AssignCorrectiveActionResponsibleRequest request)
    {
        var userId = await _accounts.RequireAsync(HttpContext.RequestAborted);
        if (string.IsNullOrWhiteSpace(incidentId) || stepSeq <= 0)
            return BadRequest(new { error = "An incident ID and positive action sequence are required." });
        if (request is null || string.IsNullOrWhiteSpace(request.BusinessAssociateId))
            return BadRequest(new { error = "A recorded business-associate ID is required." });
        await Hse.AssignCorrectiveActionResponsibleAsync(incidentId, stepSeq, request.BusinessAssociateId, userId);
        return NoContent();
    }

    [HttpGet("incidents/{incidentId}/cas/verification-review")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [Authorize(Policy = PermissionConstants.HSE.ManageIncidents)]
    [RequireCurrentFieldAccess(PermissionConstants.HSE.ManageIncidents, true)]
    public async Task<ActionResult<CAVerificationReview>> ReviewVerificationAsync(string incidentId,
        [FromServices] ICompanyFieldSelectionService selections, [FromServices] ICompanyCorrectiveActionCommands commands)
    {
        var actor = await _accounts.RequireAsync(HttpContext.RequestAborted);
        if (!LeaseSummaryValidator.Identifier(incidentId)) return BadRequest();
        var current = await selections.GetCurrentFieldSelectionAsync();
        var context = current.SelectionContext ?? throw new UnauthorizedAccessException("A FIELD selection is required.");
        FieldSelectionReviewValidator.RequireCurrent(current, context.Company, actor);
        if (!current.SelectionAvailable || _fieldOrchestrator.CurrentFieldId != context.FieldId) throw new UnauthorizedAccessException();
        var plan = await Hse.GetCaPlanAsync(incidentId);
        if (plan.Context != context || plan.IncidentId != incidentId || !LeaseSummaryValidator.Identifier(plan.PlanId)) throw new InvalidOperationException("The corrective plan changed.");
        var result = await commands.ReviewVerificationAsync(context, incidentId, plan.PlanId, actor); result.Require();
        var closing = await selections.GetCurrentFieldSelectionAsync();
        FieldSelectionReviewValidator.RequireCurrent(closing, context.Company, actor);
        if (!closing.SelectionAvailable || closing.SelectionContext != context || _fieldOrchestrator.CurrentFieldId != context.FieldId ||
            result.Context != context || result.IncidentId != incidentId || result.PlanId != plan.PlanId || await Hse.GetCaPlanAsync(incidentId) != plan)
            throw new InvalidOperationException("The verification review changed.");
        return Ok(result);
    }

    [HttpPost("incidents/{incidentId}/cas/verification-decision")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [Authorize(Policy = PermissionConstants.HSE.ManageIncidents)]
    [RequireCurrentFieldAccess(PermissionConstants.HSE.ManageIncidents, true)]
    public async Task<ActionResult<CAVerificationDecision>> RecordVerificationAsync(string incidentId, [FromBody] CAVerificationDecision original,
        [FromServices] ICompanyFieldSelectionService selections, [FromServices] ICompanyCorrectiveActionCommands commands)
    {
        var actor = await _accounts.RequireAsync(HttpContext.RequestAborted);
        if (original?.Review?.Context is null || original.Review.IncidentId != incidentId) return BadRequest();
        original.Require(); var context = original.Review.Context;
        async Task Guard()
        {
            var current = await selections.GetCurrentFieldSelectionAsync();
            FieldSelectionReviewValidator.RequireCurrent(current, context.Company, actor);
            if (!current.SelectionAvailable || current.SelectionContext != context || _fieldOrchestrator.CurrentFieldId != context.FieldId)
                throw new UnauthorizedAccessException("The company or FIELD selection changed.");
        }
        await Guard(); var plan = await Hse.GetCaPlanAsync(incidentId);
        if (plan.Context != context || plan.IncidentId != incidentId || plan.PlanId != original.Review.PlanId) throw new InvalidOperationException("The corrective plan changed.");
        var result = await commands.RecordVerificationAsync(original);
        await Guard();
        if (await Hse.GetCaPlanAsync(incidentId) != plan || System.Text.Json.JsonSerializer.Serialize(result) != System.Text.Json.JsonSerializer.Serialize(original))
            throw new InvalidOperationException("The recorded verification could not be confirmed.");
        return Ok(result);
    }

    [HttpGet("incidents/{incidentId}/closure-review")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [Authorize(Policy = PermissionConstants.HSE.ManageIncidents)]
    [RequireCurrentFieldAccess(PermissionConstants.HSE.ManageIncidents, true)]
    public async Task<ActionResult<IncidentClosureReview>> ReviewClosureAsync(string incidentId,
        [FromServices] ICompanyFieldSelectionService selections, [FromServices] ICompanyCorrectiveActionCommands commands)
    {
        var actor = await _accounts.RequireAsync(HttpContext.RequestAborted);
        if (!LeaseSummaryValidator.Identifier(incidentId)) return BadRequest();
        var current = await selections.GetCurrentFieldSelectionAsync();
        var context = current.SelectionContext ?? throw new UnauthorizedAccessException("A FIELD selection is required.");
        FieldSelectionReviewValidator.RequireCurrent(current, context.Company, actor);
        if (!current.SelectionAvailable || _fieldOrchestrator.CurrentFieldId != context.FieldId) throw new UnauthorizedAccessException();
        var plan = await Hse.GetCaPlanAsync(incidentId);
        if (plan.Context != context || plan.IncidentId != incidentId || !LeaseSummaryValidator.Identifier(plan.PlanId)) throw new InvalidOperationException("The corrective plan changed.");
        var result = await commands.ReviewClosureAsync(context, incidentId, plan.PlanId, actor); result.Require();
        var closing = await selections.GetCurrentFieldSelectionAsync();
        FieldSelectionReviewValidator.RequireCurrent(closing, context.Company, actor);
        if (!closing.SelectionAvailable || closing.SelectionContext != context || _fieldOrchestrator.CurrentFieldId != context.FieldId ||
            result.Actions.Context != context || result.Actions.IncidentId != incidentId || result.Actions.PlanId != plan.PlanId || await Hse.GetCaPlanAsync(incidentId) != plan)
            throw new InvalidOperationException("The incident closure review changed.");
        return Ok(result);
    }

    [HttpGet("incidents/{incidentId}/cas/{stepSeq}/completion-review")]
    [Authorize(Policy = PermissionConstants.HSE.ManageIncidents)]
    [RequireCurrentFieldAccess(PermissionConstants.HSE.ManageIncidents, true)]
    public async Task<ActionResult<CACompletionReview>> ReviewCompletionAsync(string incidentId, int stepSeq,
        [FromServices] ICompanyFieldSelectionService selections, [FromServices] ICompanyCorrectiveActionCommands commands)
    {
        var actor = await _accounts.RequireAsync(HttpContext.RequestAborted);
        if (!LeaseSummaryValidator.Identifier(incidentId) || stepSeq <= 0) return BadRequest();
        var current = await selections.GetCurrentFieldSelectionAsync();
        var context = current.SelectionContext ?? throw new UnauthorizedAccessException("A FIELD selection is required.");
        FieldSelectionReviewValidator.RequireCurrent(current, context.Company, actor);
        if (!current.SelectionAvailable || _fieldOrchestrator.CurrentFieldId != context.FieldId) throw new UnauthorizedAccessException();
        var plan = await Hse.GetCaPlanAsync(incidentId);
        if (plan.Context != context || plan.IncidentId != incidentId || !LeaseSummaryValidator.Identifier(plan.PlanId)) throw new InvalidOperationException("The corrective plan changed.");
        var result = await commands.ReviewCompletionAsync(context, incidentId, plan.PlanId, stepSeq, actor);
        var closing = await selections.GetCurrentFieldSelectionAsync();
        FieldSelectionReviewValidator.RequireCurrent(closing, context.Company, actor);
        if (!closing.SelectionAvailable || closing.SelectionContext != context || _fieldOrchestrator.CurrentFieldId != context.FieldId ||
            result.Context != context || result.IncidentId != incidentId || result.PlanId != plan.PlanId || result.StepSequence != stepSeq ||
            !LeaseSummaryValidator.Identifier(result.StepId) || !ProcessDefinitionRevision.IsValid(result.Revision) || await Hse.GetCaPlanAsync(incidentId) != plan)
            throw new InvalidOperationException("The action review changed.");
        return Ok(result);
    }

    [HttpPost("incidents/{incidentId}/cas/{stepSeq}/complete")]
    [Authorize(Policy = PermissionConstants.HSE.ManageIncidents)]
    [RequireCurrentFieldAccess(PermissionConstants.HSE.ManageIncidents, true)]
    public async Task<ActionResult<CompleteCAReceipt>> CompleteCAAsync(
        string incidentId, int stepSeq, [FromBody] CompleteCASubmission submission,
        [FromServices] ICompanyFieldSelectionService selections, [FromServices] ICompanyCorrectiveActionCommands commands)
    {
        var userId = await _accounts.RequireAsync(HttpContext.RequestAborted);
        if (!LeaseSummaryValidator.Identifier(incidentId) || stepSeq <= 0 || submission?.Review?.Context is null ||
            submission.Review.IncidentId != incidentId || submission.Review.StepSequence != stepSeq || !LeaseSummaryValidator.Identifier(submission.Review.PlanId) ||
            !LeaseSummaryValidator.Identifier(submission.Review.StepId) || !ProcessDefinitionRevision.IsValid(submission.Review.Revision) || string.IsNullOrWhiteSpace(submission.Notes) ||
            submission.Notes.Length > 2000 || submission.Notes.Any(c => char.IsControl(c) && c is not '\r' and not '\n' and not '\t'))
            return BadRequest(new { error = "An incident, action, plan and completion evidence are required." });
        async Task RequireSelection()
        {
            var current = await selections.GetCurrentFieldSelectionAsync();
            FieldSelectionReviewValidator.RequireCurrent(current, submission.Review.Context.Company, userId);
            if (!current.SelectionAvailable || current.SelectionContext != submission.Review.Context || _fieldOrchestrator.CurrentFieldId != submission.Review.Context.FieldId)
                throw new UnauthorizedAccessException("The company or FIELD selection changed.");
        }
        await RequireSelection();
        var plan = await Hse.GetCaPlanAsync(incidentId);
        if (plan.Context != submission.Review.Context || plan.IncidentId != incidentId || plan.PlanId != submission.Review.PlanId)
            throw new InvalidOperationException("The corrective plan changed.");
        await commands.RecordReviewedCompletionAsync(submission.Review, submission.Notes, userId);
        await RequireSelection();
        if (await Hse.GetCaPlanAsync(incidentId) != plan) throw new InvalidOperationException("The corrective plan changed during completion.");
        return Ok(new CompleteCAReceipt(submission.Review.Context, incidentId, plan.PlanId, stepSeq, submission.Review.StepId, submission.Notes));
    }

    // ── HAZOP ──────────────────────────────────────────────────────────────────

    [HttpGet("hazop")]
    public async Task<ActionResult<List<HAZOPSummary>>> GetStudiesAsync()
    {
        var studies = await Hse.GetStudiesAsync();
        return Ok(studies);
    }

    [HttpPost("hazop")]
    public async Task<ActionResult<string>> CreateStudyAsync(
        [FromBody] CreateHAZOPStudyRequest request)
    {
        var userId = await _accounts.RequireAsync(HttpContext.RequestAborted);
        var id = await Hse.CreateStudyAsync(request, userId);
        return CreatedAtAction(nameof(GetStudySummaryAsync), new { studyId = id }, id);
    }

    [HttpGet("hazop/{studyId}")]
    public async Task<ActionResult<HAZOPSummary>> GetStudySummaryAsync(string studyId)
    {
        if (string.IsNullOrWhiteSpace(studyId)) return BadRequest(new { error = "Study ID is required." });
        return Ok(await Hse.GetStudySummaryAsync(studyId));
    }

    [HttpGet("hazop/{studyId}/nodes")]
    public async Task<ActionResult<List<HAZOPNode>>> GetNodesAsync(string studyId)
    {
        if (string.IsNullOrWhiteSpace(studyId)) return BadRequest(new { error = "Study ID is required." });
        return Ok(await Hse.GetNodesAsync(studyId));
    }

    [HttpPost("hazop/{studyId}/nodes")]
    public async Task<ActionResult<string>> AddNodeAsync(
        string studyId, [FromBody] AddNodeRequest request)
    {
        var userId = await _accounts.RequireAsync(HttpContext.RequestAborted);
        if (string.IsNullOrWhiteSpace(studyId)) return BadRequest(new { error = "Study ID is required." });
        var id = await Hse.AddNodeAsync(studyId, request, userId);
        return Ok(id);
    }

    [HttpPost("hazop/{studyId}/nodes/{nodeSeq}/deviations")]
    public async Task<ActionResult<string>> AddDeviationAsync(
        string studyId, int nodeSeq, [FromBody] AddDeviationRequest request)
    {
        var userId = await _accounts.RequireAsync(HttpContext.RequestAborted);
        if (string.IsNullOrWhiteSpace(studyId)) return BadRequest(new { error = "Study ID is required." });
        var id = await Hse.AddDeviationAsync(studyId, nodeSeq, request, userId);
        return Ok(id);
    }

    [HttpPut("hazop/{studyId}/nodes/{nodeSeq}/deviations/{condSeq}/status")]
    public async Task<ActionResult> UpdateDeviationStatusAsync(
        string studyId, int nodeSeq, int condSeq, [FromQuery] string status)
    {
        var userId = await _accounts.RequireAsync(HttpContext.RequestAborted);
        if (string.IsNullOrWhiteSpace(studyId)) return BadRequest(new { error = "Study ID is required." });
        if (string.IsNullOrWhiteSpace(status)) return BadRequest(new { error = "Status is required." });
        await Hse.UpdateDeviationStatusAsync(studyId, nodeSeq, condSeq, status, userId);
        return NoContent();
    }

    // ── KPIs ───────────────────────────────────────────────────────────────────

    [HttpGet("kpi")]
    public async Task<ActionResult<HSEKPISet>> GetKPIsAsync(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var range = new DateRangeFilter(
            from ?? DateTime.UtcNow.AddYears(-1),
            to   ?? DateTime.UtcNow);

        return Ok(await Hse.GetKpisAsync(range));
    }

    [HttpGet("kpi/trend")]
    public async Task<ActionResult<List<TierRateTrend>>> GetTrendAsync(
        [FromQuery] int months = 12)
    {
        return Ok(await Hse.GetTrendAsync(months));
    }
}
