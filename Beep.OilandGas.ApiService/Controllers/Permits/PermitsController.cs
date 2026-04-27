using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.PermitsAndApplications.Constants;
using Beep.OilandGas.PermitsAndApplications.Validation;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Beep.OilandGas.ApiService.Controllers.Permits;

/// <summary>
/// API controller for permit and application management.
/// Provides REST endpoints for the full permit lifecycle: create, submit, review, approve/reject.
/// </summary>
[ApiController]
[Route("api/field/current/permits")]
[Authorize]
public class PermitsController : ControllerBase
{
    private readonly IPermitApplicationLifecycleService _lifecycle;
    private readonly IPermitApplicationWorkflowService _workflow;
    private readonly IPermitComplianceCheckService _compliance;
    private readonly IPermitStatusHistoryService _statusHistory;
    private readonly IFieldOrchestrator _fieldOrchestrator;

    public PermitsController(
        IPermitApplicationLifecycleService lifecycle,
        IPermitApplicationWorkflowService workflow,
        IPermitComplianceCheckService compliance,
        IPermitStatusHistoryService statusHistory,
        IFieldOrchestrator fieldOrchestrator)
    {
        _lifecycle = lifecycle;
        _workflow = workflow;
        _compliance = compliance;
        _statusHistory = statusHistory;
        _fieldOrchestrator = fieldOrchestrator;
    }

    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";

    // ── Application CRUD ──────────────────────────────────────────────────────

    /// <summary>
    /// Get all permit applications, optionally filtered by status.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<PermitApplicationSummary>>> GetAllAsync(
        [FromQuery] string? status,
        [FromQuery] string? authority)
    {
        var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(status))
        {
            var parsed = Enum.TryParse<PermitApplicationStatus>(status, true, out var s) ? s : PermitApplicationStatus.Draft;
            var apps = await _lifecycle.GetByStatusAsync(parsed);
            return Ok(apps.Select(a => ToSummary(a, fieldId)).ToList());
        }

        if (!string.IsNullOrWhiteSpace(authority))
        {
            var apps = await _workflow.GetPermitApplicationsByAuthorityAsync(authority);
            return Ok(apps.Select(a => ToSummary(a, fieldId)).ToList());
        }

        // Return all active applications
        var allStatuses = Enum.GetValues<PermitApplicationStatus>();
        var allApps = new List<PERMIT_APPLICATION>();
        foreach (var s in allStatuses)
        {
            var batch = await _lifecycle.GetByStatusAsync(s);
            allApps.AddRange(batch);
        }
        return Ok(allApps.Select(a => ToSummary(a, fieldId)).ToList());
    }

    /// <summary>
    /// Get a specific permit application by ID.
    /// </summary>
    [HttpGet("{applicationId}")]
    public async Task<ActionResult<PermitApplicationDetail>> GetByIdAsync(string applicationId)
    {
        if (string.IsNullOrWhiteSpace(applicationId))
            return BadRequest(new { error = "Application ID is required." });

        var app = await _lifecycle.GetByIdAsync(applicationId);
        if (app == null)
            return NotFound(new { error = $"Permit application {applicationId} not found." });

        var history = await _statusHistory.GetHistoryAsync(applicationId);
        var complianceResult = await _compliance.CheckComplianceAsync(applicationId);

        return Ok(new PermitApplicationDetail
        {
            Application = app,
            StatusHistory = history.ToList(),
            ComplianceResult = complianceResult
        });
    }

    /// <summary>
    /// Create a new permit application.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<string>> CreateAsync([FromBody] CreatePermitApplicationRequest request)
    {
        if (request == null)
            return BadRequest(new { error = "Request body is required." });

        var application = new PERMIT_APPLICATION
        {
            APPLICATION_TYPE = ParseApplicationType(request.ApplicationType),
            REGULATORY_AUTHORITY = ParseRegulatoryAuthority(request.RegulatoryAuthority),
            APPLICANT_ID = string.IsNullOrWhiteSpace(request.ApplicantName) ? "unknown" : request.ApplicantName.Trim(),
            WELL_ID = request.WellId,
            FACILITY_ID = request.FacilityId,
            REMARKS = request.Description,
            FIELD_ID = _fieldOrchestrator.CurrentFieldId ?? string.Empty,
            STATUS = PermitApplicationStatus.Draft,
            COUNTRY = Country.UnitedStates,
            STATE_PROVINCE = StateProvince.Other
        };

        var created = await _lifecycle.CreateAsync(application, UserId);
        return CreatedAtAction(nameof(GetByIdAsync), new { applicationId = created.PERMIT_APPLICATION_ID }, created.PERMIT_APPLICATION_ID);
    }

    /// <summary>
    /// Update an existing permit application.
    /// </summary>
    [HttpPut("{applicationId}")]
    public async Task<ActionResult> UpdateAsync(string applicationId, [FromBody] UpdatePermitApplicationRequest request)
    {
        if (string.IsNullOrWhiteSpace(applicationId))
            return BadRequest(new { error = "Application ID is required." });
        if (request == null)
            return BadRequest(new { error = "Request body is required." });

        var existing = await _lifecycle.GetByIdAsync(applicationId);
        if (existing == null)
            return NotFound(new { error = $"Permit application {applicationId} not found." });

        if (!string.IsNullOrWhiteSpace(request.ApplicationType))
            existing.APPLICATION_TYPE = ParseApplicationType(request.ApplicationType);
        if (!string.IsNullOrWhiteSpace(request.RegulatoryAuthority))
            existing.REGULATORY_AUTHORITY = ParseRegulatoryAuthority(request.RegulatoryAuthority);
        if (!string.IsNullOrWhiteSpace(request.ApplicantName))
            existing.APPLICANT_ID = request.ApplicantName.Trim();
        if (request.Description != null)
            existing.REMARKS = request.Description;

        await _lifecycle.UpdateAsync(applicationId, existing, UserId);
        return NoContent();
    }

    // ── Workflow Actions ──────────────────────────────────────────────────────

    /// <summary>
    /// Submit a permit application for regulatory review.
    /// </summary>
    [HttpPost("{applicationId}/submit")]
    public async Task<ActionResult> SubmitAsync(string applicationId)
    {
        if (string.IsNullOrWhiteSpace(applicationId))
            return BadRequest(new { error = "Application ID is required." });

        var existing = await _lifecycle.GetByIdAsync(applicationId);
        if (existing == null)
            return NotFound(new { error = $"Permit application {applicationId} not found." });

        var submitted = await _lifecycle.SubmitAsync(applicationId, UserId);
        return Ok(new { applicationId = submitted.PERMIT_APPLICATION_ID, status = submitted.STATUS.ToString() });
    }

    /// <summary>
    /// Approve or reject a permit application (regulator action).
    /// </summary>
    [HttpPost("{applicationId}/decision")]
    public async Task<ActionResult> ProcessDecisionAsync(
        string applicationId,
        [FromBody] PermitDecisionRequest request)
    {
        if (string.IsNullOrWhiteSpace(applicationId))
            return BadRequest(new { error = "Application ID is required." });
        if (request == null || string.IsNullOrWhiteSpace(request.Decision))
            return BadRequest(new { error = "Decision is required (Approved or Rejected)." });

        var result = await _lifecycle.ProcessDecisionAsync(
            applicationId,
            request.Decision,
            request.Remarks ?? string.Empty,
            UserId);

        return Ok(new { applicationId = result.PERMIT_APPLICATION_ID, status = result.STATUS.ToString(), decision = result.DECISION });
    }

    /// <summary>
    /// Run a compliance check on a permit application.
    /// </summary>
    [HttpGet("{applicationId}/compliance")]
    public async Task<ActionResult<PermitComplianceResult>> CheckComplianceAsync(string applicationId)
    {
        if (string.IsNullOrWhiteSpace(applicationId))
            return BadRequest(new { error = "Application ID is required." });

        var result = await _compliance.CheckComplianceAsync(applicationId);
        return Ok(result);
    }

    // ── Helper Methods ────────────────────────────────────────────────────────

    private static PermitApplicationType ParseApplicationType(string? value) =>
        Enum.TryParse<PermitApplicationType>(value, ignoreCase: true, out var parsed)
            ? parsed
            : PermitApplicationType.Other;

    private static RegulatoryAuthority ParseRegulatoryAuthority(string? value) =>
        Enum.TryParse<RegulatoryAuthority>(value, ignoreCase: true, out var parsed)
            ? parsed
            : RegulatoryAuthority.Other;

    private static PermitApplicationSummary ToSummary(PERMIT_APPLICATION app, string fieldId)
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
            FieldId = fieldId
        };
    }
}

// ── DTOs ──────────────────────────────────────────────────────────────────────

public class PermitApplicationSummary
{
    public string PermitApplicationId { get; set; } = string.Empty;
    public string ApplicationType { get; set; } = string.Empty;
    public string RegulatoryAuthority { get; set; } = string.Empty;
    public string ApplicantName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? CreatedDate { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public DateTime? DecisionDate { get; set; }
    public string? Decision { get; set; }
    public string FieldId { get; set; } = string.Empty;
}

public class PermitApplicationDetail
{
    public PERMIT_APPLICATION Application { get; set; } = default!;
    public List<PERMIT_STATUS_HISTORY> StatusHistory { get; set; } = new();
    public PermitComplianceResult ComplianceResult { get; set; } = default!;
}

public class CreatePermitApplicationRequest
{
    public string ApplicationType { get; set; } = string.Empty;
    public string RegulatoryAuthority { get; set; } = string.Empty;
    public string ApplicantName { get; set; } = string.Empty;
    public string? WellId { get; set; }
    public string? FacilityId { get; set; }
    public string? Description { get; set; }
}

public class UpdatePermitApplicationRequest
{
    public string? ApplicationType { get; set; }
    public string? RegulatoryAuthority { get; set; }
    public string? ApplicantName { get; set; }
    public string? Description { get; set; }
}

public class PermitDecisionRequest
{
    public string Decision { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}
