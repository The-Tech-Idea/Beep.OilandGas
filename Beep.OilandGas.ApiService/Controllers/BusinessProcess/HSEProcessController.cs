using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.ApiService.Services;

namespace Beep.OilandGas.ApiService.Controllers.BusinessProcess
{
    /// <summary>
    /// HSE incident and safety PROCESS WORKFLOW endpoints (IProcessService layer).
    /// Separate from the domain HSEController at api/field/current/hse.
    /// Incident workflow changes require company HSE management permission and FIELD write access.
    /// </summary>
    [ApiController]
    [Route("api/field/current/process/hse")]
    [Authorize]
    [RequireCurrentFieldAccess]
    public class HSEProcessController : ControllerBase
    {
        private const string HseIncidentEntityType = "HSE_INCIDENT";
        private readonly IFieldOrchestrator _fieldOrchestrator;
        private readonly ICompanyHseWorkflowReader _workflowReader;
        private readonly ILogger<HSEProcessController> _logger;

        public HSEProcessController(
            IFieldOrchestrator fieldOrchestrator,
            ICompanyHseWorkflowReader workflowReader,
            ILogger<HSEProcessController> logger)
        {
            _fieldOrchestrator = fieldOrchestrator ?? throw new ArgumentNullException(nameof(fieldOrchestrator));
            _workflowReader = workflowReader ?? throw new ArgumentNullException(nameof(workflowReader));
            _logger = logger;
        }

        private IFieldHSEService Hse => _fieldOrchestrator.GetHSEService();

        /// <summary>List HSE incidents for the current field, optionally filtered by severity or status.</summary>
        [HttpGet("incidents")]
        [ProducesResponseType(typeof(List<ProcessInstanceSummary>), 200)]
        public async Task<ActionResult<List<ProcessInstanceSummary>>> GetIncidentsAsync(
            [FromQuery] string? severity = null,
            [FromQuery] string? status = null)
        {
            if(!string.IsNullOrWhiteSpace(severity) && severity is not ("TIER1" or "TIER2" or "TIER3" or "TIER4"))
                return BadRequest(new { error = "Choose an explicit incident tier." });
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrEmpty(fieldId))
                    return BadRequest(new { error = "No active field selected." });

            var incidents = await Hse.GetIncidentsAsync(null) ?? throw new InvalidOperationException("The incident population was unavailable.");
            if (_fieldOrchestrator.CurrentFieldId != fieldId || incidents.Any(incident => incident is null || string.IsNullOrWhiteSpace(incident.IncidentId) || incident.FieldId != fieldId) ||
                incidents.Select(incident => incident.IncidentId).Distinct(StringComparer.Ordinal).Count() != incidents.Count)
                throw new InvalidOperationException("The incident population or FIELD could not be confirmed.");
            var summaries = new List<ProcessInstanceSummary>();

            foreach (var incident in incidents)
            {
                if (!MatchesSeverity(incident, severity))
                {
                    continue;
                }

                var inst = await GetUniqueIncidentProcessAsync(incident.IncidentId);
                if (!string.IsNullOrWhiteSpace(status) &&
                    (inst == null || !string.Equals(inst.Status.ToString(), status, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                summaries.Add(new ProcessInstanceSummary
                {
                    InstanceId = inst?.InstanceId ?? string.Empty,
                    ProcessId = inst?.ProcessId ?? string.Empty,
                    EntityId = incident.IncidentId,
                    EntityType = "HSE_INCIDENT",
                    CurrentStepId = inst?.CurrentStepId ?? string.Empty,
                    Status = inst?.Status.ToString() ?? incident.CurrentState,
                    StartedAt = inst?.StartDate ?? incident.IncidentDate,
                    StartedBy = inst?.StartedBy ?? string.Empty,
                    CompletedAt = inst?.CompletionDate
                });
            }

            if (_fieldOrchestrator.CurrentFieldId != fieldId) throw new InvalidOperationException("The FIELD changed during incident process review.");
            return Ok(summaries);
        }

        /// <summary>Get a specific incident process instance.</summary>
        [HttpGet("incident")]
        [ProducesResponseType(typeof(ProcessInstance), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ProcessInstance>> GetIncidentAsync([FromQuery] string incidentId)
        {
            if (string.IsNullOrWhiteSpace(incidentId))
                    return BadRequest(new { error = "Incident ID is required." });

            var instance = await GetUniqueIncidentProcessAsync(incidentId);
            if (instance == null)
                return NotFound(new { error = $"Incident '{incidentId}' not found." });
            return Ok(instance);
        }

        private async Task<ProcessInstance?> GetUniqueIncidentProcessAsync(string incidentId)
        {
            return await _workflowReader.ReadAsync(User, incidentId);
        }

        private static int MapSeverityToTier(string severity) => severity switch
        {
            "TIER1" => 1, "TIER2" => 2, "TIER3" => 3, "TIER4" => 4,
            _ => throw new ArgumentException("An explicit incident tier is required.", nameof(severity))
        };

        private static bool MatchesSeverity(HSEIncidentRecord incident, string? severity)
        {
            if (string.IsNullOrWhiteSpace(severity))
            {
                return true;
            }

            return incident.Tier == MapSeverityToTier(severity);
        }
    }
}
