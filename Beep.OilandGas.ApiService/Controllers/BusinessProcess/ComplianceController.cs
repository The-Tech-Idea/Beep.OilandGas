using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.ApiService.Attributes;

namespace Beep.OilandGas.ApiService.Controllers.BusinessProcess
{
    /// <summary>
    /// Compliance PROCESS WORKFLOW endpoints (IProcessService layer).
    /// Separate from the domain ComplianceController at api/field/current/compliance.
    /// Remediation workflow initiation requires the Compliance role.
    /// </summary>
    [ApiController]
    [Route("api/field/current/process/compliance")]
    [Authorize]
    [RequireCurrentFieldAccess]
    public class ComplianceController : ControllerBase
    {
        private readonly IFieldOrchestrator _fieldOrchestrator;
        private readonly IProcessService _processService;
        private readonly ILogger<ComplianceController> _logger;

        public ComplianceController(
            IFieldOrchestrator fieldOrchestrator,
            IProcessService processService,
            ILogger<ComplianceController> logger)
        {
            _fieldOrchestrator = fieldOrchestrator ?? throw new ArgumentNullException(nameof(fieldOrchestrator));
            _processService = processService ?? throw new ArgumentNullException(nameof(processService));
            _logger = logger;
        }

        /// <summary>List all compliance obligations for the current field.</summary>
        [HttpGet("obligations")]
        [ProducesResponseType(typeof(List<ComplianceObligationSummary>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<List<ComplianceObligationSummary>>> GetObligationsAsync(
            [FromQuery] string? jurisdiction = null,
            [FromQuery] string? obligationType = null)
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId))
                    return BadRequest(new { error = "No active field selected." });

            try
            {
                var instances = await _processService.GetProcessInstancesForEntityAsync(fieldId, "COMPLIANCE");
                var summaries = new List<ComplianceObligationSummary>();

                if (instances != null)
                {
                    foreach (var inst in instances)
                    {
                        var summary = new ComplianceObligationSummary
                        {
                            ObligationId = inst.InstanceId,
                            Status = inst.Status.ToString()
                        };

                        summaries.Add(summary);
                    }
                }

                return Ok(summaries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving compliance obligations for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "Error retrieving obligations." });
            }
        }

        /// <summary>List compliance obligations due within the specified number of days (default 30).</summary>
        [HttpGet("obligations/due")]
        [ProducesResponseType(typeof(List<ComplianceObligationSummary>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<List<ComplianceObligationSummary>>> GetDueObligationsAsync(
            [FromQuery] int daysAhead = 30)
        {
            if (daysAhead < 0)
                    return BadRequest(new { error = "daysAhead must be a non-negative integer." });

            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId))
                    return BadRequest(new { error = "No active field selected." });

            try
            {
                var instances = await _processService.GetProcessInstancesForEntityAsync(fieldId, "COMPLIANCE");
                var summaries = new List<ComplianceObligationSummary>();

                if (instances != null)
                {
                    foreach (var inst in instances)
                    {
                        summaries.Add(new ComplianceObligationSummary
                        {
                            ObligationId = inst.InstanceId,
                            Status = inst.Status.ToString()
                        });
                    }
                }

                summaries.Sort((a, b) => a.DueDate.CompareTo(b.DueDate));
                return Ok(summaries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving due obligations for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "Error retrieving due obligations." });
            }
        }

        /// <summary>Submit a regulatory compliance report.</summary>
        [HttpPost("reports")]
        [ProducesResponseType(typeof(ProcessInstance), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ProcessInstance>> SubmitReportAsync(
            [FromBody] ComplianceReportRequest request)
        {
            if (request == null)
                 return BadRequest(new { error = "Request body is required." });
            if (string.IsNullOrWhiteSpace(request.ObligationType))
                 return BadRequest(new { error = "ObligationType is required." });
            if (string.IsNullOrWhiteSpace(request.JurisdictionTag))
                 return BadRequest(new { error = "JurisdictionTag is required." });
            if (request.PeriodEnd <= request.PeriodStart)
                 return BadRequest(new { error = "PeriodEnd must be after PeriodStart." });

            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId))
                 return BadRequest(new { error = "No active field selected." });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";

            try
            {
                // Map to the matching compliance process definition
                var processId = $"COMPLIANCE_{request.ObligationType.ToUpperInvariant().Replace(" ", "_")}";
                var reportEntityId = Guid.NewGuid().ToString();

                var instance = await _processService.StartProcessAsync(
                    processId,
                    reportEntityId,
                    "COMPLIANCE_REPORT",
                    fieldId,
                    userId);

                // Record submission details
                await _processService.AddHistoryEntryAsync(instance.InstanceId, new ProcessHistoryEntry
                {
                    HistoryId = Guid.NewGuid().ToString(),
                    InstanceId = instance.InstanceId,
                    Action = "REPORT_SUBMITTED",
                    Notes = $"Jurisdiction: {request.JurisdictionTag} | Type: {request.ObligationType} | Period: {request.PeriodStart:yyyy-MM-dd} – {request.PeriodEnd:yyyy-MM-dd}",
                    PerformedBy = userId,
                    Timestamp = DateTime.UtcNow,
                    ActionData = new Dictionary<string, object>
                    {
                        ["JurisdictionTag"] = request.JurisdictionTag,
                        ["ObligationType"] = request.ObligationType,
                        ["PeriodStart"] = request.PeriodStart,
                        ["PeriodEnd"] = request.PeriodEnd,
                        ["SubmissionReference"] = request.SubmissionReference
                    }
                });

                return CreatedAtAction(nameof(GetReportStatusAsync), new { reportId = instance.InstanceId }, instance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting compliance report for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "Error submitting compliance report." });
            }
        }

        /// <summary>Check the submission status of a compliance report.</summary>
        [HttpGet("reports/{reportId}/status")]
        [ProducesResponseType(typeof(ProcessInstanceSummary), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ProcessInstanceSummary>> GetReportStatusAsync(string reportId)
        {
            if (string.IsNullOrWhiteSpace(reportId))
                    return BadRequest(new { error = "Report ID is required." });

            try
            {
                var instance = await _processService.GetProcessInstanceAsync(reportId);
                if (instance == null)
                    return NotFound(new { error = $"Report '{reportId}' not found." });

                var summary = new ProcessInstanceSummary
                {
                    InstanceId = instance.InstanceId,
                    ProcessId = instance.ProcessId,
                    EntityId = instance.EntityId,
                    EntityType = instance.EntityType,
                    CurrentStepId = instance.CurrentStepId,
                    Status = instance.Status.ToString(),
                    StartedAt = instance.StartDate,
                    StartedBy = instance.StartedBy,
                    CompletedAt = instance.CompletionDate
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving status for report {ReportId}", reportId);
                return StatusCode(500, new { error = "Error retrieving report status." });
            }
        }

        /// <summary>Start a remediation workflow for a compliance report finding. Requires Compliance role.</summary>
        [HttpPost("reports/{reportId}/remediate")]
        [Authorize(Roles = "Compliance")]
        [ProducesResponseType(typeof(ProcessInstance), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ProcessInstance>> StartRemediationAsync(
            string reportId,
            [FromBody] RemediationRequest request)
        {
            if (string.IsNullOrWhiteSpace(reportId))
                    return BadRequest(new { error = "Report ID is required." });

            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId))
                    return BadRequest(new { error = "No active field selected." });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";

            try
            {
                var sourceReport = await _processService.GetProcessInstanceAsync(reportId);
                if (sourceReport == null)
                    return NotFound(new { error = $"Report '{reportId}' not found." });

                // Start a remediation process linked to the source report
                var remediationInstance = await _processService.StartProcessAsync(
                    "COMPLIANCE_OBLIGATION_MGMT",
                    reportId,
                    "COMPLIANCE_REMEDIATION",
                    fieldId,
                    userId);

                await _processService.AddHistoryEntryAsync(remediationInstance.InstanceId, new ProcessHistoryEntry
                {
                    HistoryId = Guid.NewGuid().ToString(),
                    InstanceId = remediationInstance.InstanceId,
                    Action = "REMEDIATION_STARTED",
                    Notes = request?.Notes ?? "Remediation initiated.",
                    PerformedBy = userId,
                    Timestamp = DateTime.UtcNow,
                    ActionData = new Dictionary<string, object> { ["SourceReportId"] = reportId }
                });

                return CreatedAtAction(nameof(GetReportStatusAsync), new { reportId = remediationInstance.InstanceId }, remediationInstance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting remediation for report {ReportId}", reportId);
                return StatusCode(500, new { error = "Error starting remediation." });
            }
        }
    }

    public class RemediationRequest
    {
        public string Notes { get; set; } = string.Empty;
    }
}
