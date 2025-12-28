using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.LifeCycle.Models.Processes;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.LifeCycle.Services.Exploration;

using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.LifeCycle.Services.Exploration.Processes
{
    /// <summary>
    /// Service for Exploration process orchestration
    /// Handles Lead to Prospect, Prospect to Discovery, and Discovery to Development workflows
    /// </summary>
    public class ExplorationProcessService
    {
        private readonly IProcessService _processService;
        private readonly PPDMExplorationService _explorationService;
        private readonly ILogger<ExplorationProcessService>? _logger;

        public ExplorationProcessService(
            IProcessService processService,
            PPDMExplorationService explorationService,
            ILogger<ExplorationProcessService>? logger = null)
        {
            _processService = processService ?? throw new ArgumentNullException(nameof(processService));
            _explorationService = explorationService ?? throw new ArgumentNullException(nameof(explorationService));
            _logger = logger;
        }

        #region Lead to Prospect Process

        /// <summary>
        /// Start Lead to Prospect workflow
        /// </summary>
        public async Task<ProcessInstance> StartLeadToProspectProcessAsync(string leadId, string fieldId, string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("EXPLORATION");
                var leadToProspectProcess = processDef.FirstOrDefault(p => p.ProcessName == "LeadToProspect");
                
                if (leadToProspectProcess == null)
                {
                    throw new InvalidOperationException("LeadToProspect process definition not found");
                }

                return await _processService.StartProcessAsync(
                    leadToProspectProcess.ProcessId,
                    leadId,
                    "LEAD",
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
        public async Task<bool> EvaluateLeadAsync(string instanceId, Dictionary<string, object> evaluationData, string userId)
        {
            try
            {
                var instance = await _processService.GetProcessInstanceAsync(instanceId);
                if (instance == null)
                {
                    return false;
                }

                // Execute evaluation step
                return await _processService.ExecuteStepAsync(instanceId, "LEAD_EVALUATION", evaluationData, userId);
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
        public async Task<bool> ApproveLeadAsync(string instanceId, string userId)
        {
            try
            {
                var instance = await _processService.GetProcessInstanceAsync(instanceId);
                if (instance == null)
                {
                    return false;
                }

                // Complete approval step
                return await _processService.CompleteStepAsync(instanceId, "LEAD_APPROVAL", "APPROVED", userId);
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
        public async Task<bool> RejectLeadAsync(string instanceId, string reason, string userId)
        {
            try
            {
                var instance = await _processService.GetProcessInstanceAsync(instanceId);
                if (instance == null)
                {
                    return false;
                }

                // Complete approval step with rejection
                return await _processService.CompleteStepAsync(instanceId, "LEAD_APPROVAL", "REJECTED", userId);
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
        public async Task<bool> PromoteLeadToProspectAsync(string instanceId, Dictionary<string, object> prospectData, string userId)
        {
            try
            {
                var instance = await _processService.GetProcessInstanceAsync(instanceId);
                if (instance == null)
                {
                    return false;
                }

                // Execute prospect creation step
                var result = await _processService.ExecuteStepAsync(instanceId, "PROSPECT_CREATION", prospectData, userId);
                
                if (result)
                {
                    // Complete the step
                    await _processService.CompleteStepAsync(instanceId, "PROSPECT_CREATION", "SUCCESS", userId);
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
        public async Task<ProcessInstance> StartProspectToDiscoveryProcessAsync(string prospectId, string fieldId, string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("EXPLORATION");
                var prospectToDiscoveryProcess = processDef.FirstOrDefault(p => p.ProcessName == "ProspectToDiscovery");
                
                if (prospectToDiscoveryProcess == null)
                {
                    throw new InvalidOperationException("ProspectToDiscovery process definition not found");
                }

                return await _processService.StartProcessAsync(
                    prospectToDiscoveryProcess.ProcessId,
                    prospectId,
                    "PROSPECT",
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
        /// Perform risk assessment
        /// </summary>
        public async Task<bool> PerformRiskAssessmentAsync(string instanceId, Dictionary<string, object> riskData, string userId)
        {
            try
            {
                return await _processService.ExecuteStepAsync(instanceId, "RISK_ASSESSMENT", riskData, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error performing risk assessment for process instance: {instanceId}");
                throw;
            }
        }

        /// <summary>
        /// Perform volume estimation
        /// </summary>
        public async Task<bool> PerformVolumeEstimationAsync(string instanceId, Dictionary<string, object> volumeData, string userId)
        {
            try
            {
                return await _processService.ExecuteStepAsync(instanceId, "VOLUME_ESTIMATION", volumeData, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error performing volume estimation for process instance: {instanceId}");
                throw;
            }
        }

        /// <summary>
        /// Perform economic evaluation
        /// </summary>
        public async Task<bool> PerformEconomicEvaluationAsync(string instanceId, Dictionary<string, object> economicData, string userId)
        {
            try
            {
                return await _processService.ExecuteStepAsync(instanceId, "ECONOMIC_EVALUATION", economicData, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error performing economic evaluation for process instance: {instanceId}");
                throw;
            }
        }

        /// <summary>
        /// Make drilling decision
        /// </summary>
        public async Task<bool> MakeDrillingDecisionAsync(string instanceId, Dictionary<string, object> decisionData, string userId)
        {
            try
            {
                return await _processService.ExecuteStepAsync(instanceId, "DRILLING_DECISION", decisionData, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error making drilling decision for process instance: {instanceId}");
                throw;
            }
        }

        /// <summary>
        /// Record discovery
        /// </summary>
        public async Task<bool> RecordDiscoveryAsync(string instanceId, Dictionary<string, object> discoveryData, string userId)
        {
            try
            {
                var result = await _processService.ExecuteStepAsync(instanceId, "DISCOVERY_RECORDING", discoveryData, userId);
                
                if (result)
                {
                    await _processService.CompleteStepAsync(instanceId, "DISCOVERY_RECORDING", "SUCCESS", userId);
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
        public async Task<ProcessInstance> StartDiscoveryToDevelopmentProcessAsync(string discoveryId, string fieldId, string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("EXPLORATION");
                var discoveryToDevelopmentProcess = processDef.FirstOrDefault(p => p.ProcessName == "DiscoveryToDevelopment");
                
                if (discoveryToDevelopmentProcess == null)
                {
                    throw new InvalidOperationException("DiscoveryToDevelopment process definition not found");
                }

                return await _processService.StartProcessAsync(
                    discoveryToDevelopmentProcess.ProcessId,
                    discoveryId,
                    "DISCOVERY",
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
        /// Perform appraisal
        /// </summary>
        public async Task<bool> PerformAppraisalAsync(string instanceId, Dictionary<string, object> appraisalData, string userId)
        {
            try
            {
                return await _processService.ExecuteStepAsync(instanceId, "APPRAISAL", appraisalData, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error performing appraisal for process instance: {instanceId}");
                throw;
            }
        }

        /// <summary>
        /// Estimate reserves
        /// </summary>
        public async Task<bool> EstimateReservesAsync(string instanceId, Dictionary<string, object> reserveData, string userId)
        {
            try
            {
                return await _processService.ExecuteStepAsync(instanceId, "RESERVE_ESTIMATION", reserveData, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error estimating reserves for process instance: {instanceId}");
                throw;
            }
        }

        /// <summary>
        /// Perform development economic analysis
        /// </summary>
        public async Task<bool> PerformDevelopmentEconomicAnalysisAsync(string instanceId, Dictionary<string, object> economicData, string userId)
        {
            try
            {
                return await _processService.ExecuteStepAsync(instanceId, "DEVELOPMENT_ECONOMIC_ANALYSIS", economicData, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error performing development economic analysis for process instance: {instanceId}");
                throw;
            }
        }

        /// <summary>
        /// Approve development
        /// </summary>
        public async Task<bool> ApproveDevelopmentAsync(string instanceId, string userId)
        {
            try
            {
                return await _processService.CompleteStepAsync(instanceId, "DEVELOPMENT_APPROVAL", "APPROVED", userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error approving development for process instance: {instanceId}");
                throw;
            }
        }

        #endregion
    }
}

