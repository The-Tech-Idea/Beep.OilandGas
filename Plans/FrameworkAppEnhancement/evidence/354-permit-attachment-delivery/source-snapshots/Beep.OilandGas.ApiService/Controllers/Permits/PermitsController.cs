using Beep.OilandGas.PermitsAndApplications.Exceptions;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.PermitsAndApplications.Constants;
using Beep.OilandGas.PermitsAndApplications.Validation;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.ApiService.Attributes;
using TheTechIdea.Data.OilGas;
using PermitApplicationSummary = TheTechIdea.Data.OilGas.PermitApplicationSummary;
using PermitApplicationDetail = TheTechIdea.Data.OilGas.PermitApplicationDetail;
using Beep.OilandGas.Models.Data.Security;

namespace Beep.OilandGas.ApiService.Controllers.Permits;

/// <summary>
/// API controller for permit and application management.
/// Provides REST endpoints for the full permit lifecycle: create, submit, review, approve/reject.
/// </summary>
[ApiController]
[Route("api/field/current/permits")]
[Authorize]
[RequireCurrentFieldAccess(PermissionConstants.HSE.ViewPermits)]
public class PermitsController : ControllerBase
{
    private readonly IPermitApplicationLifecycleService _lifecycle;
    private readonly IPermitApplicationWorkflowService _workflow;
    private readonly IPermitComplianceCheckService _compliance;
    private readonly IPermitStatusHistoryService _statusHistory;
    private readonly IFieldOrchestrator _fieldOrchestrator;
    private readonly ICompanyPermitApplicationDataAccess _permitAccess;

    public PermitsController(
        IPermitApplicationLifecycleService lifecycle,
        IPermitApplicationWorkflowService workflow,
        IPermitComplianceCheckService compliance,
        IPermitStatusHistoryService statusHistory,
        IFieldOrchestrator fieldOrchestrator,
        ICompanyPermitApplicationDataAccess permitAccess)
    {
        _lifecycle = lifecycle;
        _workflow = workflow;
        _compliance = compliance;
        _statusHistory = statusHistory;
        _fieldOrchestrator = fieldOrchestrator;
        _permitAccess = permitAccess;
    }

    // ── Application CRUD ──────────────────────────────────────────────────────

    [HttpGet("application-catalog")]
    public ActionResult<PermitApplicationCatalog> GetApplicationCatalog() => Ok(new PermitApplicationCatalog(
        Enum.GetNames<PermitApplicationType>(), Enum.GetNames<RegulatoryAuthority>(),
        Enum.GetNames<Country>().Where(country => country != nameof(Country.Other)).ToArray(), Enum.GetNames<StateProvince>()));

    /// <summary>
    /// Get all permit applications, optionally filtered by status.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<PermitApplicationSummary>>> GetAllAsync(
        [FromQuery] string? status,
        [FromQuery] string? authority)
    {
        var fieldId = _fieldOrchestrator.CurrentFieldId;
        if (string.IsNullOrWhiteSpace(fieldId)) return BadRequest(new { error = "No active field selected." });
        ActionResult<List<PermitApplicationSummary>> Confirm(IEnumerable<PERMIT_APPLICATION>? rows)
        {
            if (rows is null || _fieldOrchestrator.CurrentFieldId != fieldId) throw new InvalidOperationException("The permit population or current FIELD changed.");
            var materialized = rows.ToArray();
            if (materialized.Any(row => row is null)) throw new InvalidOperationException("The permit population is incomplete.");
            var selected = materialized.Where(row => row.FIELD_ID == fieldId).ToArray();
            if (selected.Any(row => string.IsNullOrWhiteSpace(row.PERMIT_APPLICATION_ID)) || selected.Select(row => row.PERMIT_APPLICATION_ID).Distinct(StringComparer.Ordinal).Count() != selected.Length)
                throw new InvalidOperationException("The selected FIELD permit identities are incomplete or ambiguous.");
            return Ok(selected.Select(ToSummary).ToList());
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<PermitApplicationStatus>(status, true, out var parsed) || !Enum.IsDefined(parsed))
                return BadRequest(new { error = "The permit status is not recognized." });
            var apps = await _lifecycle.GetByStatusAsync(parsed);
            return Confirm(apps);
        }

        if (!string.IsNullOrWhiteSpace(authority))
        {
            var apps = await _workflow.GetPermitApplicationsByAuthorityAsync(authority);
            return Confirm(apps);
        }

        // Return all active applications
        var allStatuses = Enum.GetValues<PermitApplicationStatus>();
        var allApps = new List<PERMIT_APPLICATION>();
        foreach (var s in allStatuses)
        {
            var batch = await _lifecycle.GetByStatusAsync(s);
            if (batch is null) throw new InvalidOperationException("The permit population is unavailable.");
            allApps.AddRange(batch);
        }
        return Confirm(allApps);
    }

    /// <summary>
    /// Get a specific permit application by ID.
    /// </summary>
    [HttpGet("application")]
    public async Task<ActionResult<PermitApplicationDetail>> GetByIdAsync([FromQuery] string applicationId)
    {
        if (string.IsNullOrWhiteSpace(applicationId))
            return BadRequest(new { error = "Application ID is required." });

        var fieldId = _fieldOrchestrator.CurrentFieldId;
        if (string.IsNullOrWhiteSpace(fieldId)) return BadRequest(new { error = "No active field selected." });
        var app = await _lifecycle.GetByIdAsync(applicationId);
        if (_fieldOrchestrator.CurrentFieldId != fieldId) throw new InvalidOperationException("The current FIELD changed during the permit read.");
        if (app == null || app.FIELD_ID != fieldId || app.PERMIT_APPLICATION_ID != applicationId)
            return NotFound(new { error = $"Permit application {applicationId} not found." });

        var history = MapHistory(await _statusHistory.GetHistoryAsync(applicationId), applicationId);
        var complianceResult = await _compliance.CheckComplianceAsync(applicationId);
        if (_fieldOrchestrator.CurrentFieldId != fieldId) throw new InvalidOperationException("The current FIELD changed during the permit read.");

        var detail = new PermitApplicationDetail
        {
            Application = ToSummary(app),
            StatusHistory = history,
            ComplianceResult = MapCheck(complianceResult, applicationId, fieldId)
        };
        PermitApplicationReadValidator.Require(detail, applicationId, fieldId);
        return Ok(detail);
    }

    [HttpGet("application-inbox")]
    public async Task<ActionResult<PermitApplicationInboxReview>> ReadInboxAsync()
    {
        var context=await RequirePermitContextAsync();var authority=await _permitAccess.ReviewInboxAuthorityAsync();
        if(authority.Context!=context)throw RefusalException.Forbidden("The permit inbox authority belongs to another module/selection.");
        var rows=new List<PermitApplicationSummary>();
        var returned=await _lifecycle.GetApplicationsAsync()??throw new InvalidOperationException("The permit inbox population is unavailable.");
        foreach(var row in returned)
        {
            if(row is null || row.FIELD_ID!=context.Field.FieldId)throw RefusalException.Forbidden("The returned permit record escaped its selected FIELD.");
            rows.Add(ToSummary(row));
            if(rows.Count>4096)throw new InvalidOperationException("The returned permit population exceeds this inbox limit; refine server paging before using it.");
        }
        if(await _permitAccess.ReviewInboxAuthorityAsync()!=authority)throw RefusalException.Conflict("The permit inbox authority changed while reading records.");
        await RequirePermitContextAsync(context);
        return Ok(PermitApplicationInboxReview.Capture(authority,DateTimeOffset.UtcNow,rows.OrderBy(row=>row.PermitApplicationId,StringComparer.Ordinal).ToArray()));
    }

    [HttpPost("application-creation-review")]
    [RequireCurrentFieldAccess(PermissionConstants.HSE.IssuePermit, true)]
    public async Task<ActionResult<PermitApplicationCreationReview>> ReviewCreationAsync([FromBody] PermitApplicationCreationProposal proposal)
    {
        if(proposal is null)return BadRequest(new{error="An explicit creation proposal is required."});
        try{proposal.Require();}
        catch(InvalidOperationException){throw RefusalException.Invalid("The creation proposal requires valid draft values and complete linked resource identities.");}
        var context=await RequirePermitContextAsync();
        var result=await _permitAccess.ReviewCreationAsync(proposal)??throw new InvalidOperationException("The creation source review is unavailable.");
        result.RequireFor(proposal,context.Field);
        if(result.Context!=context)throw RefusalException.Forbidden("The creation review belongs to another permit module binding.");
        await RequirePermitContextAsync(context);return Ok(result);
    }

    [HttpGet("application-review")]
    public async Task<ActionResult<PermitApplicationSourceReview>> ReviewSourceAsync([FromQuery] string applicationId)
    {
        if (!LeaseSummaryValidator.Identifier(applicationId)) return BadRequest(new {error="An exact permit application identity is required."});
        var context = await RequirePermitContextAsync();
        PermitApplicationSourceReview review;
        try { review = await _lifecycle.ReviewSourceAsync(applicationId) ?? throw new InvalidOperationException("The permit source review is unavailable."); }
        catch (PermitNotFoundException)
        {
            throw RefusalException.NotFound("The permit application is unavailable in the current company FIELD.");
        }
        review.Require();
        if (review.Context != context || review.ReadApplication().PERMIT_APPLICATION_ID != applicationId)
            throw new InvalidOperationException("The permit source review belongs to another request context or application.");
        await RequirePermitContextAsync(context); return Ok(review);
    }

    [HttpPost("application-command")]
    public async Task<ActionResult<PermitApplicationCommandOutcome>> RecordCommandAsync([FromBody] PermitApplicationCommand original)
    {
        if (original is null) return BadRequest(new {error="A retained reviewed permit original is required."});
        RequireOriginal(original); await RequirePermitContextAsync(original.Review.Context);
        // The native owner independently enforces current IssuePermit/ApprovePermit and FIELD write authority
        // for a new effect. A confirmed same-original replay remains read-only.
        var result = await _lifecycle.RecordCommandAsync(original) ?? throw new InvalidOperationException("The permit outcome is unconfirmed. Retain the original for recovery.");
        result.Require();
        if (result.Original != original) throw new InvalidOperationException("The permit outcome belongs to another original.");
        await RequirePermitContextAsync(original.Review.Context); return Ok(result);
    }

    [HttpPost("application-command/recovery")]
    public async Task<ActionResult<PermitApplicationCommandRecovery>> RecoverCommandAsync([FromBody] PermitApplicationCommand original)
    {
        if (original is null) return BadRequest(new {error="The original permit command is required for read-only recovery."});
        RequireOriginal(original); await RequirePermitContextAsync(original.Review.Context);
        var result = new PermitApplicationCommandRecovery(original, await _lifecycle.RecoverCommandAsync(original)); result.Require(original);
        await RequirePermitContextAsync(original.Review.Context); return Ok(result);
    }

    [HttpPost("application-document-stage")]
    public async Task<ActionResult<PermitDocumentStageResponse>> StageDocumentAsync([FromBody] PermitDocumentStageRequest request)
    {
        if(request is null)return BadRequest(new{error="An exact original and matching bytes are required."});request.Require();
        await RequirePermitContextAsync(request.Original.Review.Context);
        var proof=await _lifecycle.StageDocumentAsync(request.Original,request.Content)??throw new InvalidOperationException("Document staging is unconfirmed; retain the original.");
        proof.Require(request.Original);
        var response=new PermitDocumentStageResponse(request.Original,PermitDocumentStageReceipt.From(proof));response.Require(request.Original);
        await RequirePermitContextAsync(request.Original.Review.Context);return Ok(response);
    }
    [HttpPost("application-document-stage/recovery")]
    public async Task<ActionResult<PermitDocumentStageResponse>> RecoverDocumentStageAsync([FromBody] PermitDocumentStageOriginal original)
    {
        if(original is null)return BadRequest(new{error="The retained document original is required."});original.Require();await RequirePermitContextAsync(original.Review.Context);
        var proof=await _lifecycle.RecoverDocumentStageAsync(original);proof?.Require(original);
        var response=new PermitDocumentStageResponse(original,proof is null?null:PermitDocumentStageReceipt.From(proof));response.Require(original);
        await RequirePermitContextAsync(original.Review.Context);return Ok(response);
    }
    [HttpPost("application-attachment-content")]
    public async Task<ActionResult<PermitAttachmentContent>> ReadAttachmentContentAsync([FromBody] PermitAttachmentContentQuery query)
    {
        if(query is null)return BadRequest(new{error="An exact reviewed attachment is required."});query.Require();await RequirePermitContextAsync(query.Review.Context);
        var result=await _lifecycle.ReadAttachmentContentAsync(query)??throw new InvalidOperationException("Attachment content is unconfirmed.");result.Require(query);
        await RequirePermitContextAsync(query.Review.Context);return Ok(result);
    }
    [HttpPost("application-attachment-download")]
    public async Task<IActionResult> DownloadAttachmentAsync([FromBody] PermitAttachmentContentQuery query)
    {
        if(query is null)return BadRequest(new{error="An exact reviewed attachment is required."});query.Require();await RequirePermitContextAsync(query.Review.Context);
        var result=await _lifecycle.ReadAttachmentContentAsync(query)??throw new InvalidOperationException("Attachment content is unconfirmed.");result.Require(query);
        await RequirePermitContextAsync(query.Review.Context);Response.Headers["X-Content-Type-Options"]="nosniff";
        return File(result.Content,"application/octet-stream",result.Receipt.Document.FileName);
    }

    private static void RequireOriginal(PermitApplicationCommand original)
    {
        try { original.Require(); }
        catch (Exception exception) when (exception is InvalidOperationException or System.Text.Json.JsonException)
        {
            throw RefusalException.Invalid("The retained permit original is invalid. Review the current source before preparing a new command.");
        }
    }

    private async Task<PermitApplicationDataScope> RequirePermitContextAsync(PermitApplicationDataScope? expected = null)
    {
        var current = await _permitAccess.RequireReadScopeAsync() ?? throw new InvalidOperationException("The current permit company context is unavailable.");
        current.Require();
        if (current.Field.ActorUserId != User.ActingUserId() || current.Field.FieldId != _fieldOrchestrator.CurrentFieldId || expected is not null && current != expected)
            throw RefusalException.Forbidden("The permit original, actor, company, FIELD selection or module binding differs from the current request.");
        return current;
    }

    /// <summary>
    /// Run a compliance check on a permit application.
    /// </summary>
    [HttpGet("application-check")]
    public async Task<ActionResult<PermitApplicationCheckResult>> CheckComplianceAsync([FromQuery] string applicationId)
    {
        if (string.IsNullOrWhiteSpace(applicationId))
            return BadRequest(new { error = "Application ID is required." });

        var application = await GetCurrentApplicationAsync(applicationId);
        if (application is null) return NotFound(new { error = "The permit application is unavailable in the current FIELD." });
        var fieldId = application.FIELD_ID;
        var result = await _compliance.CheckComplianceAsync(applicationId);
        if (_fieldOrchestrator.CurrentFieldId != fieldId) throw new InvalidOperationException("The current FIELD changed during the permit check.");
        return Ok(MapCheck(result, applicationId, fieldId));
    }

    // ── Helper Methods ────────────────────────────────────────────────────────

    private async Task<PERMIT_APPLICATION?> GetCurrentApplicationAsync(string applicationId)
    {
        var fieldId = _fieldOrchestrator.CurrentFieldId;
        if (string.IsNullOrWhiteSpace(fieldId)) throw new InvalidOperationException("No active FIELD is selected.");
        var application = await _lifecycle.GetByIdAsync(applicationId);
        if (_fieldOrchestrator.CurrentFieldId != fieldId) throw new InvalidOperationException("The current FIELD changed during the permit read.");
        return application is not null && application.PERMIT_APPLICATION_ID == applicationId && application.FIELD_ID == fieldId ? application : null;
    }

    private static List<PermitStatusHistoryEntry> MapHistory(IEnumerable<PERMIT_STATUS_HISTORY>? rows, string applicationId)
    {
        var history = rows?.ToArray() ?? throw new InvalidOperationException("The permit status history is unavailable.");
        if (history.Any(row => row is null || row.PERMIT_APPLICATION_ID != applicationId || !LeaseSummaryValidator.Identifier(row.PERMIT_STATUS_HISTORY_ID)) ||
            history.Select(row => row.PERMIT_STATUS_HISTORY_ID).Distinct(StringComparer.Ordinal).Count() != history.Length)
            throw new InvalidOperationException("The permit history belongs to another application or has ambiguous identities.");
        return history.Select(row => new PermitStatusHistoryEntry { HistoryId = row.PERMIT_STATUS_HISTORY_ID, ApplicationId = applicationId,
            Status = row.STATUS, StatusDate = row.STATUS_DATE == default ? null : row.STATUS_DATE, Remarks = row.STATUS_REMARKS, UpdatedBy = row.UPDATED_BY }).ToList();
    }
    private static PermitApplicationCheckResult MapCheck(Beep.OilandGas.Models.Data.PermitsAndApplications.PermitComplianceResult? result, string applicationId, string fieldId)
    {
        if (result is null || result.ComplianceScore is < 0 or > 100 || result.Violations is null || result.Warnings is null)
            throw new InvalidOperationException("The permit checker result is unavailable or invalid.");
        var check = new PermitApplicationCheckResult { ApplicationId = applicationId, FieldId = fieldId, CheckedAtUtc = DateTimeOffset.UtcNow,
            IsCompliant = result.IsCompliant, ComplianceScore = result.ComplianceScore, Violations = result.Violations.ToList(), Warnings = result.Warnings.ToList() };
        PermitApplicationReadValidator.RequireCheck(check, applicationId, fieldId);
        return check;
    }

    private static PermitApplicationSummary ToSummary(PERMIT_APPLICATION app)
    {
        return new PermitApplicationSummary
        {
            PermitApplicationId = app.PERMIT_APPLICATION_ID,
            ApplicationType = app.APPLICATION_TYPE.ToString(),
            RegulatoryAuthority = app.REGULATORY_AUTHORITY.ToString(),
            ApplicantName = app.APPLICANT_ID ?? string.Empty,
            Status = app.STATUS.ToString(),
            CreatedDate = app.CREATED_DATE,
            SubmittedDate = app.SUBMITTED_DATE,
            DecisionDate = app.DECISION_DATE,
            Decision = app.DECISION,
            FieldId = app.FIELD_ID,
            SubmittedByUserId=app.SUBMITTED_BY_USER_ID,Country=app.COUNTRY.ToString(),Region=app.STATE_PROVINCE.ToString()
        };
    }
}
