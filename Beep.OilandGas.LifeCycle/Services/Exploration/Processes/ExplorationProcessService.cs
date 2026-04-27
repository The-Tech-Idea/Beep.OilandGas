using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.Models.Data.ProspectIdentification;
using Beep.OilandGas.ProspectIdentification;

namespace Beep.OilandGas.LifeCycle.Services.Exploration.Processes
{
    /// <summary>
    /// Service for exploration process orchestration: Lead to Prospect, Prospect to Discovery, and Discovery to Development.
    /// Step-level persistence is delegated to <see cref="IProcessService"/>; HTTP entry and field guards live on the field
    /// <c>ExplorationController</c>. Data-slice ownership, route-to-method handoffs for risk/volume/economics/discovery, and
    /// <see cref="ProcessInstance"/> anchor rules are documented in
    /// <c>Beep.OilandGas.ProspectIdentification/.plans/12_Phase4_Data_Ownership_And_Handoffs.md</c>.
    /// For <see cref="ExplorationReferenceCodes.ProcessIdProspectToDiscovery"/> instances, maturation step methods enforce a linear prerequisite chain
    /// using <see cref="ProcessInstance.StepInstances"/> (prior step must not be <see cref="StepStatus.PENDING"/> — matches execute-only vs completed steps).
    /// </summary>
    public partial class ExplorationProcessService
    {
        private readonly IProcessService _processService;
        private readonly ILeadExplorationService? _leadExplorationService;
        private readonly ILogger<ExplorationProcessService>? _logger;

        public ExplorationProcessService(
            IProcessService processService,
            ILogger<ExplorationProcessService>? logger = null,
            ILeadExplorationService? leadExplorationService = null)
        {
            _processService = processService ?? throw new ArgumentNullException(nameof(processService));
            _logger = logger;
            _leadExplorationService = leadExplorationService;
        }

        #region Lead to Prospect Process

        /// <summary>
        /// Start Lead to Prospect workflow
        /// </summary>
        public async Task<ProcessInstance> StartLeadToProspectProcessAsync(
            string leadId,
            string fieldId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync(ExplorationReferenceCodes.ProcessTypeExploration);
                cancellationToken.ThrowIfCancellationRequested();
                var leadToProspectProcess = processDef.FirstOrDefault(p => p.ProcessName == ExplorationReferenceCodes.ProcessNameLeadToProspect);
                
                if (leadToProspectProcess == null)
                {
                    throw new InvalidOperationException($"{ExplorationReferenceCodes.ProcessNameLeadToProspect} process definition not found");
                }

                cancellationToken.ThrowIfCancellationRequested();
                return await _processService.StartProcessAsync(
                    leadToProspectProcess.ProcessId,
                    leadId,
                    ExplorationReferenceCodes.EntityTypeLead,
                    fieldId,
                    userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting Lead to Prospect process for lead: {leadId}");
                throw;
            }
        }

        /// <summary>
        /// Evaluate lead
        /// </summary>
        public async Task<bool> EvaluateLeadAsync(
            string instanceId,
            PROCESS_STEP_DATA evaluationData,
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var instance = await _processService.GetProcessInstanceAsync(instanceId);
                if (instance == null)
                {
                    return false;
                }

                cancellationToken.ThrowIfCancellationRequested();
                // Execute evaluation step
                return await _processService.ExecuteStepAsync(instanceId, ExplorationReferenceCodes.StepLeadEvaluation, evaluationData, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error evaluating lead for process instance: {instanceId}");
                throw;
            }
        }

        /// <summary>
        /// Approve lead
        /// </summary>
        public async Task<bool> ApproveLeadAsync(
            string instanceId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var instance = await _processService.GetProcessInstanceAsync(instanceId);
                if (instance == null)
                {
                    return false;
                }

                cancellationToken.ThrowIfCancellationRequested();
                // Complete approval step
                return await _processService.CompleteStepAsync(instanceId, ExplorationReferenceCodes.StepLeadApproval, ExplorationReferenceCodes.OutcomeApproved, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error approving lead for process instance: {instanceId}");
                throw;
            }
        }

        /// <summary>
        /// Reject lead
        /// </summary>
        public async Task<bool> RejectLeadAsync(
            string instanceId,
            string reason,
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var instance = await _processService.GetProcessInstanceAsync(instanceId);
                if (instance == null)
                {
                    return false;
                }

                cancellationToken.ThrowIfCancellationRequested();
                _logger?.LogDebug("Rejecting lead for instance {InstanceId}: {Reason}", instanceId, reason);
                // Complete approval step with rejection
                return await _processService.CompleteStepAsync(instanceId, ExplorationReferenceCodes.StepLeadApproval, ExplorationReferenceCodes.OutcomeRejected, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error rejecting lead for process instance: {instanceId}");
                throw;
            }
        }

        /// <summary>
        /// Promote lead to prospect
        /// </summary>
        public async Task<bool> PromoteLeadToProspectAsync(
            string instanceId,
            PROCESS_STEP_DATA prospectData,
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var instance = await _processService.GetProcessInstanceAsync(instanceId);
                if (instance == null)
                {
                    return false;
                }

                cancellationToken.ThrowIfCancellationRequested();
                // Execute prospect creation step
                var result = await _processService.ExecuteStepAsync(instanceId, ExplorationReferenceCodes.StepProspectCreation, prospectData, userId);
                
                if (result)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    // Complete the step
                    await _processService.CompleteStepAsync(instanceId, ExplorationReferenceCodes.StepProspectCreation, ExplorationReferenceCodes.OutcomeSuccess, userId);
                    if (_leadExplorationService != null)
                        await _leadExplorationService.AfterProspectCreationStepCompletedAsync(instanceId, userId, cancellationToken);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error promoting lead to prospect for process instance: {instanceId}");
                throw;
            }
        }

        #endregion

        #region Prospect to Discovery Process

        /// <summary>
        /// Start Prospect to Discovery workflow
        /// </summary>
        public async Task<ProcessInstance> StartProspectToDiscoveryProcessAsync(
            string prospectId,
            string fieldId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync(ExplorationReferenceCodes.ProcessTypeExploration);
                cancellationToken.ThrowIfCancellationRequested();
                var prospectToDiscoveryProcess = processDef.FirstOrDefault(p => p.ProcessName == ExplorationReferenceCodes.ProcessNameProspectToDiscovery);
                
                if (prospectToDiscoveryProcess == null)
                {
                    throw new InvalidOperationException($"{ExplorationReferenceCodes.ProcessNameProspectToDiscovery} process definition not found");
                }

                cancellationToken.ThrowIfCancellationRequested();
                return await _processService.StartProcessAsync(
                    prospectToDiscoveryProcess.ProcessId,
                    prospectId,
                    ExplorationReferenceCodes.EntityTypeProspect,
                    fieldId,
                    userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting Prospect to Discovery process for prospect: {prospectId}");
                throw;
            }
        }

        /// <summary>
        /// Runs and completes <see cref="ExplorationReferenceCodes.StepProspectCreation"/> for a
        /// <see cref="ExplorationReferenceCodes.ProcessIdProspectToDiscovery"/> instance anchored on
        /// <see cref="ExplorationReferenceCodes.EntityTypeProspect"/>. Does not invoke
        /// <see cref="ILeadExplorationService"/> — use <see cref="PromoteLeadToProspectAsync"/> for lead workflows.
        /// </summary>
        public async Task<bool> CompleteProspectReadinessStepAsync(
            string instanceId,
            PROCESS_STEP_DATA stepData,
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var instance = await _processService.GetProcessInstanceAsync(instanceId);
                if (instance == null)
                    return false;

                if (!string.Equals(
                        instance.ProcessId,
                        ExplorationReferenceCodes.ProcessIdProspectToDiscovery,
                        StringComparison.OrdinalIgnoreCase))
                {
                    _logger?.LogDebug(
                        "Prospect readiness: instance {InstanceId} process {ProcessId} is not {Expected}",
                        instanceId,
                        instance.ProcessId,
                        ExplorationReferenceCodes.ProcessIdProspectToDiscovery);
                    return false;
                }

                if (!string.Equals(
                        instance.EntityType,
                        ExplorationReferenceCodes.EntityTypeProspect,
                        StringComparison.OrdinalIgnoreCase))
                {
                    _logger?.LogDebug(
                        "Prospect readiness: instance {InstanceId} entity type {EntityType} is not PROSPECT",
                        instanceId,
                        instance.EntityType);
                    return false;
                }

                cancellationToken.ThrowIfCancellationRequested();
                var executed = await _processService.ExecuteStepAsync(
                    instanceId,
                    ExplorationReferenceCodes.StepProspectCreation,
                    stepData,
                    userId);
                if (!executed)
                    return false;

                cancellationToken.ThrowIfCancellationRequested();
                return await _processService.CompleteStepAsync(
                    instanceId,
                    ExplorationReferenceCodes.StepProspectCreation,
                    ExplorationReferenceCodes.OutcomeSuccess,
                    userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error completing prospect readiness step for process instance: {InstanceId}", instanceId);
                throw;
            }
        }

        /// <summary>
        /// Perform risk assessment.
        /// Prospect→discovery: requires <see cref="ExplorationReferenceCodes.StepProspectCreation"/> to have advanced past <see cref="StepStatus.PENDING"/>.
        /// Throws <see cref="ExplorationWorkflowPrerequisiteException"/> when blocked (HTTP 409 from <c>ExplorationController</c>).
        /// </summary>
        public async Task<bool> PerformRiskAssessmentAsync(
            string instanceId,
            PROCESS_STEP_DATA riskData,
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var instance = await _processService.GetProcessInstanceAsync(instanceId);
                RequireExplorationWorkflowStepPrerequisite(
                    instance,
                    IsProspectToDiscoveryProspectWorkflow,
                    ExplorationReferenceCodes.StepProspectCreation,
                    ExplorationReferenceCodes.StepRiskAssessment,
                    instanceId);

                cancellationToken.ThrowIfCancellationRequested();
                return await _processService.ExecuteStepAsync(instanceId, ExplorationReferenceCodes.StepRiskAssessment, riskData, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error performing risk assessment for process instance: {instanceId}");
                throw;
            }
        }

        /// <summary>
        /// Perform volume estimation.
        /// Prospect→discovery: requires <see cref="ExplorationReferenceCodes.StepRiskAssessment"/> advanced past <see cref="StepStatus.PENDING"/>.
        /// Throws <see cref="ExplorationWorkflowPrerequisiteException"/> when blocked (HTTP 409 from <c>ExplorationController</c>).
        /// </summary>
        public async Task<bool> PerformVolumeEstimationAsync(
            string instanceId,
            PROCESS_STEP_DATA volumeData,
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var instance = await _processService.GetProcessInstanceAsync(instanceId);
                RequireExplorationWorkflowStepPrerequisite(
                    instance,
                    IsProspectToDiscoveryProspectWorkflow,
                    ExplorationReferenceCodes.StepRiskAssessment,
                    ExplorationReferenceCodes.StepVolumeEstimation,
                    instanceId);

                cancellationToken.ThrowIfCancellationRequested();
                return await _processService.ExecuteStepAsync(instanceId, ExplorationReferenceCodes.StepVolumeEstimation, volumeData, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error performing volume estimation for process instance: {instanceId}");
                throw;
            }
        }

        /// <summary>
        /// Perform economic evaluation.
        /// Prospect→discovery: requires <see cref="ExplorationReferenceCodes.StepVolumeEstimation"/> advanced past <see cref="StepStatus.PENDING"/>.
        /// Throws <see cref="ExplorationWorkflowPrerequisiteException"/> when blocked (HTTP 409 from <c>ExplorationController</c>).
        /// </summary>
        public async Task<bool> PerformEconomicEvaluationAsync(
            string instanceId,
            PROCESS_STEP_DATA economicData,
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var instance = await _processService.GetProcessInstanceAsync(instanceId);
                RequireExplorationWorkflowStepPrerequisite(
                    instance,
                    IsProspectToDiscoveryProspectWorkflow,
                    ExplorationReferenceCodes.StepVolumeEstimation,
                    ExplorationReferenceCodes.StepEconomicEvaluation,
                    instanceId);

                cancellationToken.ThrowIfCancellationRequested();
                return await _processService.ExecuteStepAsync(instanceId, ExplorationReferenceCodes.StepEconomicEvaluation, economicData, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error performing economic evaluation for process instance: {instanceId}");
                throw;
            }
        }

        /// <summary>
        /// Make drilling decision.
        /// Prospect→discovery: requires <see cref="ExplorationReferenceCodes.StepEconomicEvaluation"/> advanced past <see cref="StepStatus.PENDING"/>.
        /// Throws <see cref="ExplorationWorkflowPrerequisiteException"/> when blocked (HTTP 409 from <c>ExplorationController</c>).
        /// </summary>
        public async Task<bool> MakeDrillingDecisionAsync(
            string instanceId,
            PROCESS_STEP_DATA decisionData,
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var instance = await _processService.GetProcessInstanceAsync(instanceId);
                RequireExplorationWorkflowStepPrerequisite(
                    instance,
                    IsProspectToDiscoveryProspectWorkflow,
                    ExplorationReferenceCodes.StepEconomicEvaluation,
                    ExplorationReferenceCodes.StepDrillingDecision,
                    instanceId);

                cancellationToken.ThrowIfCancellationRequested();
                return await _processService.ExecuteStepAsync(instanceId, ExplorationReferenceCodes.StepDrillingDecision, decisionData, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error making drilling decision for process instance: {instanceId}");
                throw;
            }
        }

        /// <summary>
        /// Record discovery.
        /// Prospect→discovery: requires <see cref="ExplorationReferenceCodes.StepDrillingDecision"/> advanced past <see cref="StepStatus.PENDING"/>.
        /// Throws <see cref="ExplorationWorkflowPrerequisiteException"/> when blocked (HTTP 409 from <c>ExplorationController</c>).
        /// </summary>
        public async Task<bool> RecordDiscoveryAsync(
            string instanceId,
            PROCESS_STEP_DATA discoveryData,
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var instance = await _processService.GetProcessInstanceAsync(instanceId);
                RequireExplorationWorkflowStepPrerequisite(
                    instance,
                    IsProspectToDiscoveryProspectWorkflow,
                    ExplorationReferenceCodes.StepDrillingDecision,
                    ExplorationReferenceCodes.StepDiscoveryRecording,
                    instanceId);

                cancellationToken.ThrowIfCancellationRequested();
                var result = await _processService.ExecuteStepAsync(instanceId, ExplorationReferenceCodes.StepDiscoveryRecording, discoveryData, userId);
                
                if (result)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await _processService.CompleteStepAsync(instanceId, ExplorationReferenceCodes.StepDiscoveryRecording, ExplorationReferenceCodes.OutcomeSuccess, userId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error recording discovery for process instance: {instanceId}");
                throw;
            }
        }

        #endregion

        #region Discovery to Development Process

        /// <summary>
        /// Start Discovery to Development workflow
        /// </summary>
        public async Task<ProcessInstance> StartDiscoveryToDevelopmentProcessAsync(
            string discoveryId,
            string fieldId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync(ExplorationReferenceCodes.ProcessTypeExploration);
                cancellationToken.ThrowIfCancellationRequested();
                var discoveryToDevelopmentProcess = processDef.FirstOrDefault(p => p.ProcessName == ExplorationReferenceCodes.ProcessNameDiscoveryToDevelopment);
                
                if (discoveryToDevelopmentProcess == null)
                {
                    throw new InvalidOperationException($"{ExplorationReferenceCodes.ProcessNameDiscoveryToDevelopment} process definition not found");
                }

                cancellationToken.ThrowIfCancellationRequested();
                return await _processService.StartProcessAsync(
                    discoveryToDevelopmentProcess.ProcessId,
                    discoveryId,
                    ExplorationReferenceCodes.EntityTypeDiscovery,
                    fieldId,
                    userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting Discovery to Development process for discovery: {discoveryId}");
                throw;
            }
        }

        /// <summary>
        /// Perform appraisal.
        /// Discovery→development: requires <see cref="ExplorationReferenceCodes.StepDiscoveryRecording"/> advanced past <see cref="StepStatus.PENDING"/>.
        /// Throws <see cref="ExplorationWorkflowPrerequisiteException"/> when blocked (HTTP 409 from <c>ExplorationController</c>).
        /// </summary>
        public async Task<bool> PerformAppraisalAsync(
            string instanceId,
            PROCESS_STEP_DATA appraisalData,
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var instance = await _processService.GetProcessInstanceAsync(instanceId);
                RequireExplorationWorkflowStepPrerequisite(
                    instance,
                    IsDiscoveryToDevelopmentDiscoveryWorkflow,
                    ExplorationReferenceCodes.StepDiscoveryRecording,
                    ExplorationReferenceCodes.StepAppraisal,
                    instanceId);

                cancellationToken.ThrowIfCancellationRequested();
                return await _processService.ExecuteStepAsync(instanceId, ExplorationReferenceCodes.StepAppraisal, appraisalData, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error performing appraisal for process instance: {instanceId}");
                throw;
            }
        }

        /// <summary>
        /// Estimate reserves.
        /// Discovery→development: requires <see cref="ExplorationReferenceCodes.StepAppraisal"/> advanced past <see cref="StepStatus.PENDING"/>.
        /// Throws <see cref="ExplorationWorkflowPrerequisiteException"/> when blocked (HTTP 409 from <c>ExplorationController</c>).
        /// </summary>
        public async Task<bool> EstimateReservesAsync(
            string instanceId,
            PROCESS_STEP_DATA reserveData,
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var instance = await _processService.GetProcessInstanceAsync(instanceId);
                RequireExplorationWorkflowStepPrerequisite(
                    instance,
                    IsDiscoveryToDevelopmentDiscoveryWorkflow,
                    ExplorationReferenceCodes.StepAppraisal,
                    ExplorationReferenceCodes.StepReserveEstimation,
                    instanceId);

                cancellationToken.ThrowIfCancellationRequested();
                return await _processService.ExecuteStepAsync(instanceId, ExplorationReferenceCodes.StepReserveEstimation, reserveData, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error estimating reserves for process instance: {instanceId}");
                throw;
            }
        }

        /// <summary>
        /// Perform development economic analysis.
        /// Discovery→development: requires <see cref="ExplorationReferenceCodes.StepReserveEstimation"/> advanced past <see cref="StepStatus.PENDING"/>.
        /// Throws <see cref="ExplorationWorkflowPrerequisiteException"/> when blocked (HTTP 409 from <c>ExplorationController</c>).
        /// </summary>
        public async Task<bool> PerformDevelopmentEconomicAnalysisAsync(
            string instanceId,
            PROCESS_STEP_DATA economicData,
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var instance = await _processService.GetProcessInstanceAsync(instanceId);
                RequireExplorationWorkflowStepPrerequisite(
                    instance,
                    IsDiscoveryToDevelopmentDiscoveryWorkflow,
                    ExplorationReferenceCodes.StepReserveEstimation,
                    ExplorationReferenceCodes.StepDevelopmentEconomicAnalysis,
                    instanceId);

                cancellationToken.ThrowIfCancellationRequested();
                return await _processService.ExecuteStepAsync(instanceId, ExplorationReferenceCodes.StepDevelopmentEconomicAnalysis, economicData, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error performing development economic analysis for process instance: {instanceId}");
                throw;
            }
        }

        /// <summary>
        /// Approve development.
        /// Discovery→development: requires <see cref="ExplorationReferenceCodes.StepDevelopmentEconomicAnalysis"/> advanced past <see cref="StepStatus.PENDING"/>.
        /// Throws <see cref="ExplorationWorkflowPrerequisiteException"/> when blocked (HTTP 409 from <c>ExplorationController</c>).
        /// </summary>
        public async Task<bool> ApproveDevelopmentAsync(
            string instanceId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var instance = await _processService.GetProcessInstanceAsync(instanceId);
                RequireExplorationWorkflowStepPrerequisite(
                    instance,
                    IsDiscoveryToDevelopmentDiscoveryWorkflow,
                    ExplorationReferenceCodes.StepDevelopmentEconomicAnalysis,
                    ExplorationReferenceCodes.StepDevelopmentApproval,
                    instanceId);

                cancellationToken.ThrowIfCancellationRequested();
                return await _processService.CompleteStepAsync(instanceId, ExplorationReferenceCodes.StepDevelopmentApproval, ExplorationReferenceCodes.OutcomeApproved, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error approving development for process instance: {instanceId}");
                throw;
            }
        }

        #endregion

        #region Process instance validation

        /// <summary>
        /// True when the instance exists and <see cref="ProcessInstance.FieldId"/> matches <paramref name="fieldId"/> (trimmed, ordinal case-insensitive).
        /// </summary>
        public async Task<bool> IsProcessInstanceInFieldAsync(
            string instanceId,
            string fieldId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(instanceId) || string.IsNullOrWhiteSpace(fieldId))
                return false;

            cancellationToken.ThrowIfCancellationRequested();
            var instance = await _processService.GetProcessInstanceAsync(instanceId).ConfigureAwait(false);
            if (instance == null)
                return false;

            if (string.IsNullOrWhiteSpace(instance.FieldId))
                return false;

            return string.Equals(instance.FieldId.Trim(), fieldId.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        #endregion
    }
}

