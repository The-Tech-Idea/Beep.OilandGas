using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.LifeCycle.Models.Processes;
using Beep.OilandGas.LifeCycle.Services.Exploration.Processes;
using Beep.OilandGas.LifeCycle.Services.Development.Processes;
using Beep.OilandGas.LifeCycle.Services.Production.Processes;
using Beep.OilandGas.LifeCycle.Services.Decommissioning.Processes;
using Beep.OilandGas.LifeCycle.Services.WellLifecycle;
using Beep.OilandGas.LifeCycle.Services.FieldLifecycle;
using Beep.OilandGas.LifeCycle.Services.ReservoirLifecycle;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionForecasting;

namespace Beep.OilandGas.LifeCycle.Services.Processes
{
    /// <summary>
    /// Helper class to coordinate between process services and lifecycle services
    /// Provides high-level operations that combine process workflows with entity state management
    /// </summary>
    public class ProcessIntegrationHelper
    {
        private readonly IProcessService _processService;
        private readonly ExplorationProcessService _explorationProcessService;
        private readonly DevelopmentProcessService _developmentProcessService;
        private readonly ProductionProcessService _productionProcessService;
        private readonly DecommissioningProcessService _decommissioningProcessService;
        private readonly WellLifecycleService _wellLifecycleService;
        private readonly FieldLifecycleService _fieldLifecycleService;
        private readonly ReservoirLifecycleService _reservoirLifecycleService;
        private readonly ILogger<ProcessIntegrationHelper>? _logger;

        public ProcessIntegrationHelper(
            IProcessService processService,
            ExplorationProcessService explorationProcessService,
            DevelopmentProcessService developmentProcessService,
            ProductionProcessService productionProcessService,
            DecommissioningProcessService decommissioningProcessService,
            WellLifecycleService wellLifecycleService,
            FieldLifecycleService fieldLifecycleService,
            ReservoirLifecycleService reservoirLifecycleService,
            ILogger<ProcessIntegrationHelper>? logger = null)
        {
            _processService = processService ?? throw new ArgumentNullException(nameof(processService));
            _explorationProcessService = explorationProcessService ?? throw new ArgumentNullException(nameof(explorationProcessService));
            _developmentProcessService = developmentProcessService ?? throw new ArgumentNullException(nameof(developmentProcessService));
            _productionProcessService = productionProcessService ?? throw new ArgumentNullException(nameof(productionProcessService));
            _decommissioningProcessService = decommissioningProcessService ?? throw new ArgumentNullException(nameof(decommissioningProcessService));
            _wellLifecycleService = wellLifecycleService ?? throw new ArgumentNullException(nameof(wellLifecycleService));
            _fieldLifecycleService = fieldLifecycleService ?? throw new ArgumentNullException(nameof(fieldLifecycleService));
            _reservoirLifecycleService = reservoirLifecycleService ?? throw new ArgumentNullException(nameof(reservoirLifecycleService));
            _logger = logger;
        }

        #region Well Lifecycle Integration

        /// <summary>
        /// Start well development process and transition well to DRILLING state
        /// </summary>
        public async Task<ProcessInstance> StartWellDevelopmentWithStateTransitionAsync(
            string wellId,
            string fieldId,
            string userId)
        {
            try
            {
                // Start well development process
                var processInstance = await _developmentProcessService.StartWellDevelopmentProcessAsync(wellId, fieldId, userId);

                // Transition well state to DRILLING
                await _wellLifecycleService.TransitionWellStateAsync(wellId, "DRILLING", userId);

                _logger?.LogInformation($"Well development process started and well {wellId} transitioned to DRILLING state");
                return processInstance;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting well development with state transition for well {wellId}");
                throw;
            }
        }

        /// <summary>
        /// Complete well development and transition to PRODUCING state
        /// </summary>
        public async Task<bool> CompleteWellDevelopmentWithStateTransitionAsync(
            string instanceId,
            string wellId,
            string userId)
        {
            try
            {
                // Complete production handover step
                var handoverComplete = await _developmentProcessService.HandoverToProductionAsync(instanceId, userId);

                if (handoverComplete)
                {
                    // Transition well state to PRODUCING
                    await _wellLifecycleService.TransitionWellStateAsync(wellId, "PRODUCING", userId);
                }

                return handoverComplete;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error completing well development with state transition for well {wellId}");
                throw;
            }
        }

        /// <summary>
        /// Start workover process and transition well to WORKOVER state
        /// </summary>
        public async Task<ProcessInstance> StartWorkoverWithStateTransitionAsync(
            string wellId,
            string fieldId,
            string userId)
        {
            try
            {
                // Start workover process
                var processInstance = await _productionProcessService.StartWorkoverProcessAsync(wellId, fieldId, userId);

                // Transition well state to WORKOVER
                await _wellLifecycleService.TransitionWellStateAsync(wellId, "WORKOVER", userId);

                _logger?.LogInformation($"Workover process started and well {wellId} transitioned to WORKOVER state");
                return processInstance;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting workover with state transition for well {wellId}");
                throw;
            }
        }

        /// <summary>
        /// Complete workover and transition back to PRODUCING state
        /// </summary>
        public async Task<bool> CompleteWorkoverWithStateTransitionAsync(
            string instanceId,
            string wellId,
            string userId)
        {
            try
            {
                // Complete production restart step
                var restartComplete = await _productionProcessService.RestartProductionAsync(instanceId, new PRODUCTION_FORECAST(), userId);

                if (restartComplete)
                {
                    // Transition well state back to PRODUCING
                    await _wellLifecycleService.TransitionWellStateAsync(wellId, "PRODUCING", userId);
                }

                return restartComplete;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error completing workover with state transition for well {wellId}");
                throw;
            }
        }

        /// <summary>
        /// Start well abandonment process and transition well to ABANDONED state
        /// </summary>
        public async Task<ProcessInstance> StartWellAbandonmentWithStateTransitionAsync(
            string wellId,
            string fieldId,
            string userId)
        {
            try
            {
                // Start abandonment process
                var processInstance = await _decommissioningProcessService.StartWellAbandonmentProcessAsync(wellId, fieldId, userId);

                // Transition well state to ABANDONED (or keep in current state until completion)
                // Note: You might want to transition only after completion
                // await _wellLifecycleService.TransitionWellStateAsync(wellId, "ABANDONED", userId);

                _logger?.LogInformation($"Well abandonment process started for well {wellId}");
                return processInstance;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting well abandonment with state transition for well {wellId}");
                throw;
            }
        }

        /// <summary>
        /// Complete well abandonment and transition to ABANDONED state
        /// </summary>
        public async Task<bool> CompleteWellAbandonmentWithStateTransitionAsync(
            string instanceId,
            string wellId,
            string userId)
        {
            try
            {
                // Complete abandonment
                var abandonmentComplete = await _decommissioningProcessService.CompleteAbandonmentAsync(instanceId, userId);

                if (abandonmentComplete)
                {
                    // Transition well state to ABANDONED
                    await _wellLifecycleService.TransitionWellStateAsync(wellId, "ABANDONED", userId);
                }

                return abandonmentComplete;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error completing well abandonment with state transition for well {wellId}");
                throw;
            }
        }

        #endregion

        #region Field Lifecycle Integration

        /// <summary>
        /// Transition field phase and start appropriate process
        /// </summary>
        public async Task<ProcessInstance> TransitionFieldPhaseWithProcessAsync(
            string fieldId,
            string targetPhase,
            string userId)
        {
            try
            {
                // Transition field phase
                var phaseTransitioned = await _fieldLifecycleService.TransitionFieldPhaseAsync(fieldId, targetPhase, userId);

                if (!phaseTransitioned)
                {
                    throw new InvalidOperationException($"Failed to transition field {fieldId} to phase {targetPhase}");
                }

                // Start appropriate process based on phase
                ProcessInstance processInstance = null;

                switch (targetPhase.ToUpper())
                {
                    case "DEVELOPMENT":
                        // Could start a field development planning process
                        _logger?.LogInformation($"Field {fieldId} transitioned to DEVELOPMENT phase");
                        break;
                    case "PRODUCTION":
                        // Could start a field production startup process
                        _logger?.LogInformation($"Field {fieldId} transitioned to PRODUCTION phase");
                        break;
                    case "DECOMMISSIONING":
                        // Could start a field decommissioning process
                        _logger?.LogInformation($"Field {fieldId} transitioned to DECOMMISSIONING phase");
                        break;
                }

                return processInstance;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error transitioning field phase with process for field {fieldId}");
                throw;
            }
        }

        #endregion

        #region Reservoir Lifecycle Integration

        /// <summary>
        /// Transition reservoir state and start appropriate process
        /// </summary>
        public async Task<ProcessInstance> TransitionReservoirStateWithProcessAsync(
            string reservoirId,
            string targetState,
            string fieldId,
            string userId)
        {
            try
            {
                // Transition reservoir state
                var stateTransitioned = await _reservoirLifecycleService.TransitionReservoirStateAsync(reservoirId, targetState, userId);

                if (!stateTransitioned)
                {
                    throw new InvalidOperationException($"Failed to transition reservoir {reservoirId} to state {targetState}");
                }

                // Start appropriate process based on state
                ProcessInstance processInstance = null;

                switch (targetState.ToUpper())
                {
                    case "DEVELOPED":
                        // Could start a reservoir development process
                        _logger?.LogInformation($"Reservoir {reservoirId} transitioned to DEVELOPED state");
                        break;
                    case "PRODUCING":
                        // Could start a reservoir production process
                        _logger?.LogInformation($"Reservoir {reservoirId} transitioned to PRODUCING state");
                        break;
                }

                return processInstance;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error transitioning reservoir state with process for reservoir {reservoirId}");
                throw;
            }
        }

        #endregion

        #region Process Status Queries

        /// <summary>
        /// Get comprehensive status for an entity including process and lifecycle state
        /// </summary>
        public async Task<EntityProcessStatus> GetEntityProcessStatusAsync(
            string entityId,
            string entityType)
        {
            try
            {
                var status = new EntityProcessStatus
                {
                    EntityId = entityId,
                    EntityType = entityType,
                    ActiveProcesses = new List<ProcessInstance>(),
                    LifecycleState = string.Empty,
                    AvailableTransitions = new List<string>()
                };

                // Get active processes
                status.ActiveProcesses = await _processService.GetProcessInstancesForEntityAsync(entityId, entityType);

                // Get lifecycle state based on entity type
                switch (entityType.ToUpper())
                {
                    case "WELL":
                        status.LifecycleState = await _wellLifecycleService.GetCurrentWellStateAsync(entityId);
                        status.AvailableTransitions = await _wellLifecycleService.GetAvailableTransitionsAsync(entityId);
                        break;
                    case "FIELD":
                        status.LifecycleState = await _fieldLifecycleService.GetCurrentFieldPhaseAsync(entityId);
                        status.AvailableTransitions = await _fieldLifecycleService.GetAvailablePhaseTransitionsAsync(entityId);
                        break;
                    case "RESERVOIR":
                        status.LifecycleState = await _reservoirLifecycleService.GetCurrentReservoirStateAsync(entityId);
                        status.AvailableTransitions = await _reservoirLifecycleService.GetAvailableTransitionsAsync(entityId);
                        break;
                }

                return status;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting entity process status for {entityType} {entityId}");
                throw;
            }
        }

        #endregion
    }

    /// <summary>
    /// Comprehensive status for an entity including process and lifecycle information
    /// </summary>
    public class EntityProcessStatus
    {
        public string EntityId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string LifecycleState { get; set; } = string.Empty;
        public List<ProcessInstance> ActiveProcesses { get; set; } = new List<ProcessInstance>();
        public List<string> AvailableTransitions { get; set; } = new List<string>();
    }
}

