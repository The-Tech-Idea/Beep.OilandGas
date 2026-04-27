using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.ProspectIdentification;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Beep.OilandGas.LifeCycle.Services.Exploration
{
    /// <summary>
    /// Persists a field-scoped <see cref="Models.Data.ProspectIdentification.PROSPECT"/> after the process engine
    /// completes <c>PROSPECT_CREATION</c> on a <c>LEAD</c>-anchored instance.
    /// </summary>
    public sealed class LeadExplorationService : ILeadExplorationService
    {
        private readonly IProcessService _processService;
        private readonly IFieldExplorationService _explorationService;
        private readonly string _promotedLeadStatusCode;
        private readonly ILogger<LeadExplorationService>? _logger;

        public LeadExplorationService(
            IProcessService processService,
            IFieldExplorationService explorationService,
            IOptions<LeadExplorationWorkflowOptions> workflowOptions,
            ILogger<LeadExplorationService>? logger = null)
        {
            _processService = processService ?? throw new ArgumentNullException(nameof(processService));
            _explorationService = explorationService ?? throw new ArgumentNullException(nameof(explorationService));
            if (workflowOptions == null)
                throw new ArgumentNullException(nameof(workflowOptions));

            var code = workflowOptions.Value?.PromotedLeadStatusCode;
            _promotedLeadStatusCode = string.IsNullOrWhiteSpace(code)
                ? ExplorationReferenceCodes.LeadStatusPromotedToProspect
                : code.Trim();
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task AfterProspectCreationStepCompletedAsync(
            string processInstanceId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(processInstanceId))
                throw new ArgumentException("Process instance id is required.", nameof(processInstanceId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User id is required.", nameof(userId));

            var instance = await _processService.GetProcessInstanceAsync(processInstanceId).ConfigureAwait(false);
            if (instance == null)
            {
                _logger?.LogWarning("Lead exploration: process instance {InstanceId} not found", processInstanceId);
                return;
            }

            if (!string.Equals(instance.EntityType, ExplorationReferenceCodes.EntityTypeLead, StringComparison.OrdinalIgnoreCase))
            {
                _logger?.LogDebug(
                    "Lead exploration: instance {InstanceId} entity type is {EntityType}, not LEAD — skipping prospect create",
                    processInstanceId,
                    instance.EntityType);
                return;
            }

            if (string.IsNullOrWhiteSpace(instance.FieldId))
            {
                _logger?.LogWarning("Lead exploration: instance {InstanceId} has no FieldId", processInstanceId);
                return;
            }

            var leadId = instance.EntityId;
            if (string.IsNullOrWhiteSpace(leadId))
            {
                _logger?.LogWarning("Lead exploration: instance {InstanceId} has no lead EntityId", processInstanceId);
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();

            var existing = await _explorationService
                .GetProspectForFieldByLeadIdAsync(instance.FieldId, leadId)
                .ConfigureAwait(false);
            if (existing != null)
            {
                _logger?.LogInformation(
                    "Lead exploration: prospect already exists for lead {LeadId} in field {FieldId} (process {InstanceId}), skipping create",
                    leadId,
                    instance.FieldId,
                    processInstanceId);
                return;
            }

            var request = BuildProspectRequest(instance, leadId);
            await _explorationService
                .CreateProspectForFieldAsync(instance.FieldId, request, userId)
                .ConfigureAwait(false);

            await _explorationService
                .UpdateLeadStatusAsync(leadId, _promotedLeadStatusCode, userId)
                .ConfigureAwait(false);

            _logger?.LogInformation(
                "Lead exploration: created prospect for field {FieldId} from lead {LeadId} (process {InstanceId})",
                instance.FieldId,
                leadId,
                processInstanceId);
        }

        private static ProspectRequest BuildProspectRequest(ProcessInstance instance, string leadId)
        {
            var request = new ProspectRequest
            {
                ProspectName = $"Prospect from lead {leadId}",
                Status = "NEW",
                LeadId = leadId
            };

            var step = instance.StepInstances?
                .Where(s => string.Equals(s.StepId, ExplorationReferenceCodes.StepProspectCreation, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(s => s.SequenceNumber)
                .FirstOrDefault();

            var json = step?.StepData?.DataJson;
            if (string.IsNullOrWhiteSpace(json))
                return request;

            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.TryGetProperty("ProspectName", out var nameEl) && nameEl.ValueKind == JsonValueKind.String)
                {
                    var n = nameEl.GetString();
                    if (!string.IsNullOrWhiteSpace(n))
                        request.ProspectName = n!;
                }
                if (root.TryGetProperty("Description", out var descEl) && descEl.ValueKind == JsonValueKind.String)
                    request.Description = descEl.GetString();
                if (root.TryGetProperty("ProspectType", out var typeEl) && typeEl.ValueKind == JsonValueKind.String)
                    request.ProspectType = typeEl.GetString();
            }
            catch (JsonException)
            {
                // Keep defaults from lead id
            }

            return request;
        }
    }
}
