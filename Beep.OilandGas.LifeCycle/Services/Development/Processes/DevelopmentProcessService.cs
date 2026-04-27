using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.LifeCycle.Services.Development;
using Beep.OilandGas.Models.Data.Process;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.Models.Data.PipelineAnalysis;

namespace Beep.OilandGas.LifeCycle.Services.Development.Processes
{
    /// <summary>
    /// Service for Development process orchestration
    /// Handles Pool Definition, Facility Development, Well Development, and Pipeline Development workflows
    /// </summary>
    public class DevelopmentProcessService
    {
        private readonly IProcessService _processService;
        private readonly PPDMDevelopmentService _developmentService;
        private readonly ILogger<DevelopmentProcessService>? _logger;

        public DevelopmentProcessService(
            IProcessService processService,
            PPDMDevelopmentService developmentService,
            ILogger<DevelopmentProcessService>? logger = null)
        {
            _processService = processService ?? throw new ArgumentNullException(nameof(processService));
            _developmentService = developmentService ?? throw new ArgumentNullException(nameof(developmentService));
            _logger = logger;
        }

        #region Pool Definition Process

        public async Task<ProcessInstance> StartPoolDefinitionProcessAsync(string poolId, string fieldId, string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("DEVELOPMENT");
                var poolProcess = processDef.FirstOrDefault(p => p.ProcessName == "PoolDefinition");
                
                if (poolProcess == null)
                {
                    throw new InvalidOperationException("PoolDefinition process definition not found");
                }

                return await _processService.StartProcessAsync(
                    poolProcess.ProcessId,
                    poolId,
                    "POOL",
                    fieldId,
                    userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting Pool Definition process for pool: {poolId}");
                throw;
            }
        }

        public async Task<bool> DelineatePoolAsync(string instanceId, PROCESS_STEP_DATA stepData, string userId)
        {
            stepData.StepType = "POOL_DELINEATION";
            stepData.LastUpdated = DateTime.UtcNow;
            return await _processService.ExecuteStepAsync(instanceId, "POOL_DELINEATION", stepData, userId);
        }

        public async Task<bool> AssignReservesAsync(string instanceId, PROCESS_STEP_DATA stepData, string userId)
        {
            stepData.StepType = "RESERVE_ASSIGNMENT";
            stepData.LastUpdated = DateTime.UtcNow;
            return await _processService.ExecuteStepAsync(instanceId, "RESERVE_ASSIGNMENT", stepData, userId);
        }

        public async Task<bool> ApprovePoolAsync(string instanceId, string userId)
        {
            return await _processService.CompleteStepAsync(instanceId, "POOL_APPROVAL", "APPROVED", userId);
        }

        public async Task<bool> ActivatePoolAsync(string instanceId, string userId)
        {
            return await _processService.CompleteStepAsync(instanceId, "POOL_ACTIVATION", "ACTIVE", userId);
        }

        #endregion

        #region Facility Development Process

        public async Task<ProcessInstance> StartFacilityDevelopmentProcessAsync(string facilityId, string fieldId, string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("DEVELOPMENT");
                var facilityProcess = processDef.FirstOrDefault(p => p.ProcessName == "FacilityDevelopment");
                
                if (facilityProcess == null)
                {
                    throw new InvalidOperationException("FacilityDevelopment process definition not found");
                }

                return await _processService.StartProcessAsync(
                    facilityProcess.ProcessId,
                    facilityId,
                    "FACILITY",
                    fieldId,
                    userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting Facility Development process for facility: {facilityId}");
                throw;
            }
        }

        public async Task<bool> DesignFacilityAsync(string instanceId, PROCESS_STEP_DATA stepData, string userId)
        {
            stepData.StepType = "FACILITY_DESIGN";
            stepData.LastUpdated = DateTime.UtcNow;
            return await _processService.ExecuteStepAsync(instanceId, "FACILITY_DESIGN", stepData, userId);
        }

        public async Task<bool> ObtainFacilityPermitsAsync(string instanceId, string userId)
        {
            var stepData = new PROCESS_STEP_DATA
            {
                StepType = "FACILITY_PERMITTING",
                Status = "PENDING",
                LastUpdated = DateTime.UtcNow
            };
            return await _processService.ExecuteStepAsync(instanceId, "FACILITY_PERMITTING", stepData, userId);
        }

        public async Task<bool> StartConstructionAsync(string instanceId, PROCESS_STEP_DATA stepData, string userId)
        {
            stepData.StepType = "CONSTRUCTION";
            stepData.LastUpdated = DateTime.UtcNow;
            return await _processService.ExecuteStepAsync(instanceId, "CONSTRUCTION", stepData, userId);
        }

        public async Task<bool> TestFacilityAsync(string instanceId, PROCESS_STEP_DATA stepData, string userId)
        {
            stepData.StepType = "FACILITY_TESTING";
            stepData.LastUpdated = DateTime.UtcNow;
            return await _processService.ExecuteStepAsync(instanceId, "FACILITY_TESTING", stepData, userId);
        }

        public async Task<bool> CommissionFacilityAsync(string instanceId, PROCESS_STEP_DATA stepData, string userId)
        {
            stepData.StepType = "COMMISSIONING";
            stepData.LastUpdated = DateTime.UtcNow;
            return await _processService.ExecuteStepAsync(instanceId, "COMMISSIONING", stepData, userId);
        }

        public async Task<bool> ActivateFacilityAsync(string instanceId, string userId)
        {
            return await _processService.CompleteStepAsync(instanceId, "FACILITY_ACTIVATION", "ACTIVE", userId);
        }

        #endregion

        #region Well Development Process

        public async Task<ProcessInstance> StartWellDevelopmentProcessAsync(string wellId, string fieldId, string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("DEVELOPMENT");
                var wellProcess = processDef.FirstOrDefault(p => p.ProcessName == "WellDevelopment");
                
                if (wellProcess == null)
                {
                    throw new InvalidOperationException("WellDevelopment process definition not found");
                }

                return await _processService.StartProcessAsync(
                    wellProcess.ProcessId,
                    wellId,
                    "WELL",
                    fieldId,
                    userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting Well Development process for well: {wellId}");
                throw;
            }
        }

        public async Task<bool> ObtainDrillingPermitAsync(string instanceId, string userId)
        {
            var stepData = new PROCESS_STEP_DATA
            {
                StepType = "DRILLING_PERMIT",
                Status = "PENDING",
                LastUpdated = DateTime.UtcNow
            };
            return await _processService.ExecuteStepAsync(instanceId, "DRILLING_PERMIT", stepData, userId);
        }

        public async Task<bool> StartDrillingAsync(string instanceId, PROCESS_STEP_DATA stepData, string userId)
        {
            stepData.StepType = "DRILLING";
            stepData.LastUpdated = DateTime.UtcNow;
            return await _processService.ExecuteStepAsync(instanceId, "DRILLING", stepData, userId);
        }

        public async Task<bool> CompleteWellAsync(string instanceId, PROCESS_STEP_DATA stepData, string userId)
        {
            stepData.StepType = "COMPLETION";
            stepData.LastUpdated = DateTime.UtcNow;
            return await _processService.ExecuteStepAsync(instanceId, "COMPLETION", stepData, userId);
        }

        public async Task<bool> TestWellProductionAsync(string instanceId, PROCESS_STEP_DATA stepData, string userId)
        {
            stepData.StepType = "PRODUCTION_TESTING";
            stepData.LastUpdated = DateTime.UtcNow;
            return await _processService.ExecuteStepAsync(instanceId, "PRODUCTION_TESTING", stepData, userId);
        }

        public async Task<bool> HandoverToProductionAsync(string instanceId, string userId)
        {
            return await _processService.CompleteStepAsync(instanceId, "PRODUCTION_HANDOVER", "HANDED_OVER", userId);
        }

        #endregion

        #region Pipeline Development Process

        public async Task<ProcessInstance> StartPipelineDevelopmentProcessAsync(string pipelineId, string fieldId, string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("DEVELOPMENT");
                var pipelineProcess = processDef.FirstOrDefault(p => p.ProcessName == "PipelineDevelopment");
                
                if (pipelineProcess == null)
                {
                    throw new InvalidOperationException("PipelineDevelopment process definition not found");
                }

                return await _processService.StartProcessAsync(
                    pipelineProcess.ProcessId,
                    pipelineId,
                    "PIPELINE",
                    fieldId,
                    userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting Pipeline Development process for pipeline: {pipelineId}");
                throw;
            }
        }

        public async Task<bool> DesignPipelineAsync(string instanceId, PROCESS_STEP_DATA stepData, string userId)
        {
            stepData.StepType = "PIPELINE_DESIGN";
            stepData.LastUpdated = DateTime.UtcNow;
            return await _processService.ExecuteStepAsync(instanceId, "PIPELINE_DESIGN", stepData, userId);
        }

        public async Task<bool> ObtainPipelinePermitsAsync(string instanceId, string userId)
        {
            var stepData = new PROCESS_STEP_DATA
            {
                StepType = "PIPELINE_PERMITTING",
                Status = "PENDING",
                LastUpdated = DateTime.UtcNow
            };
            return await _processService.ExecuteStepAsync(instanceId, "PIPELINE_PERMITTING", stepData, userId);
        }

        public async Task<bool> StartPipelineConstructionAsync(string instanceId, PROCESS_STEP_DATA stepData, string userId)
        {
            stepData.StepType = "PIPELINE_CONSTRUCTION";
            stepData.LastUpdated = DateTime.UtcNow;
            return await _processService.ExecuteStepAsync(instanceId, "PIPELINE_CONSTRUCTION", stepData, userId);
        }

        public async Task<bool> TestPipelineAsync(string instanceId, PROCESS_STEP_DATA stepData, string userId)
        {
            stepData.StepType = "PIPELINE_TESTING";
            stepData.LastUpdated = DateTime.UtcNow;
            return await _processService.ExecuteStepAsync(instanceId, "PIPELINE_TESTING", stepData, userId);
        }

        public async Task<bool> CommissionPipelineAsync(string instanceId, PROCESS_STEP_DATA stepData, string userId)
        {
            stepData.StepType = "PIPELINE_COMMISSIONING";
            stepData.LastUpdated = DateTime.UtcNow;
            return await _processService.ExecuteStepAsync(instanceId, "PIPELINE_COMMISSIONING", stepData, userId);
        }

        public async Task<bool> ActivatePipelineAsync(string instanceId, string userId)
        {
            return await _processService.CompleteStepAsync(instanceId, "PIPELINE_ACTIVATION", "ACTIVE", userId);
        }

        #endregion

        #region FDP Gate Review Process

        /// <summary>
        /// Starts the GATE_FDP_REVIEW process for the current field.
        /// The field ID is used as the entity ID since FDP is a field-level document.
        /// </summary>
        public async Task<ProcessInstance> StartFdpGateProcessAsync(string fieldId, string userId)
        {
            try
            {
                var processDefs = await _processService.GetProcessDefinitionsByTypeAsync("DEVELOPMENT");
                var fdpProcess  = processDefs.FirstOrDefault(p => p.ProcessId == "GATE_FDP_REVIEW")
                               ?? processDefs.FirstOrDefault(p => p.ProcessName == "FDPGateReview");

                if (fdpProcess == null)
                    throw new InvalidOperationException("GATE_FDP_REVIEW process definition not found. Ensure process definitions have been initialised.");

                return await _processService.StartProcessAsync(
                    fdpProcess.ProcessId,
                    fieldId,
                    "FIELD",
                    fieldId,
                    userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error starting FDP Gate Review process for field {FieldId}", fieldId);
                throw;
            }
        }

        /// <summary>
        /// Returns the most recent GATE_FDP_REVIEW process instance for the given field, or null if none exists.
        /// Entity type stored by StartFdpGateProcessAsync is "FIELD".
        /// </summary>
        public async Task<ProcessInstance?> GetCurrentFdpStatusAsync(string fieldId)
        {
            try
            {
                var instances = await _processService.GetProcessInstancesForEntityAsync(fieldId, "FIELD");
                return instances
                    .Where(i => i.ProcessId == "GATE_FDP_REVIEW")
                    .OrderByDescending(i => i.StartDate)
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error fetching FDP status for field {FieldId}", fieldId);
                throw;
            }
        }

        #endregion
    }
}
