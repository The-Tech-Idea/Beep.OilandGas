using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.LifeCycle.Models.Processes;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.LifeCycle.Services.Decommissioning;
using Beep.OilandGas.Models.Data.Process;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Beep.OilandGas.LifeCycle.Services.Decommissioning.Processes
{
    /// <summary>
    /// Service for Decommissioning process orchestration
    /// Handles Well Abandonment and Facility Decommissioning workflows
    /// </summary>
    public class DecommissioningProcessService
    {
        private readonly IProcessService _processService;
        private readonly PPDMDecommissioningService _decommissioningService;
        private readonly ILogger<DecommissioningProcessService>? _logger;

        public DecommissioningProcessService(
            IProcessService processService,
            PPDMDecommissioningService decommissioningService,
            ILogger<DecommissioningProcessService>? logger = null)
        {
            _processService = processService ?? throw new ArgumentNullException(nameof(processService));
            _decommissioningService = decommissioningService ?? throw new ArgumentNullException(nameof(decommissioningService));
            _logger = logger;
        }

        #region Well Abandonment Process

        public async Task<ProcessInstance> StartWellAbandonmentProcessAsync(string wellId, string fieldId, string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("DECOMMISSIONING");
                var abandonmentProcess = processDef.FirstOrDefault(p => p.ProcessName == "WellAbandonment");
                
                if (abandonmentProcess == null)
                {
                    throw new InvalidOperationException("WellAbandonment process definition not found");
                }

                return await _processService.StartProcessAsync(
                    abandonmentProcess.ProcessId,
                    wellId,
                    "WELL",
                    fieldId,
                    userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting Well Abandonment process for well: {wellId}");
                throw;
            }
        }

        public async Task<bool> PlanAbandonmentAsync(string instanceId, Dictionary<string, object> planData, string userId)
        {
            var stepData = new PROCESS_STEP_DATA
            {
                DataJson = JsonSerializer.Serialize(planData),
                LastUpdated = DateTime.UtcNow
            };
            return await _processService.ExecuteStepAsync(instanceId, "ABANDONMENT_PLANNING", stepData, userId);
        }

        public async Task<bool> ObtainRegulatoryApprovalAsync(string instanceId, string userId)
        {
            var stepData = new PROCESS_STEP_DATA
            {
                DataJson = JsonSerializer.Serialize(new Dictionary<string, object>()),
                LastUpdated = DateTime.UtcNow
            };
            return await _processService.ExecuteStepAsync(instanceId, "REGULATORY_APPROVAL", stepData, userId);
        }

        public async Task<bool> PlugWellAsync(string instanceId, Dictionary<string, object> pluggingData, string userId)
        {
            var stepData = new PROCESS_STEP_DATA
            {
                DataJson = JsonSerializer.Serialize(pluggingData),
                LastUpdated = DateTime.UtcNow
            };
            return await _processService.ExecuteStepAsync(instanceId, "WELL_PLUGGING", stepData, userId);
        }

        public async Task<bool> RestoreSiteAsync(string instanceId, Dictionary<string, object> restorationData, string userId)
        {
            var stepData = new PROCESS_STEP_DATA
            {
                DataJson = JsonSerializer.Serialize(restorationData),
                LastUpdated = DateTime.UtcNow
            };
            return await _processService.ExecuteStepAsync(instanceId, "SITE_RESTORATION", stepData, userId);
        }

        public async Task<bool> CompleteAbandonmentAsync(string instanceId, string userId)
        {
            return await _processService.CompleteStepAsync(instanceId, "ABANDONMENT_COMPLETION", "COMPLETED", userId);
        }

        #endregion

        #region Facility Decommissioning Process

        public async Task<ProcessInstance> StartFacilityDecommissioningProcessAsync(string facilityId, string fieldId, string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("DECOMMISSIONING");
                var decommissioningProcess = processDef.FirstOrDefault(p => p.ProcessName == "FacilityDecommissioning");
                
                if (decommissioningProcess == null)
                {
                    throw new InvalidOperationException("FacilityDecommissioning process definition not found");
                }

                return await _processService.StartProcessAsync(
                    decommissioningProcess.ProcessId,
                    facilityId,
                    "FACILITY",
                    fieldId,
                    userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting Facility Decommissioning process for facility: {facilityId}");
                throw;
            }
        }

        public async Task<bool> PlanDecommissioningAsync(string instanceId, Dictionary<string, object> planData, string userId)
        {
            var stepData = new PROCESS_STEP_DATA
            {
                DataJson = JsonSerializer.Serialize(planData),
                LastUpdated = DateTime.UtcNow
            };
            return await _processService.ExecuteStepAsync(instanceId, "DECOMMISSIONING_PLANNING", stepData, userId);
        }

        public async Task<bool> RemoveEquipmentAsync(string instanceId, Dictionary<string, object> removalData, string userId)
        {
            var stepData = new PROCESS_STEP_DATA
            {
                DataJson = JsonSerializer.Serialize(removalData),
                LastUpdated = DateTime.UtcNow
            };
            return await _processService.ExecuteStepAsync(instanceId, "EQUIPMENT_REMOVAL", stepData, userId);
        }

        public async Task<bool> CleanupSiteAsync(string instanceId, Dictionary<string, object> cleanupData, string userId)
        {
            var stepData = new PROCESS_STEP_DATA
            {
                DataJson = JsonSerializer.Serialize(cleanupData),
                LastUpdated = DateTime.UtcNow
            };
            return await _processService.ExecuteStepAsync(instanceId, "SITE_CLEANUP", stepData, userId);
        }

        public async Task<bool> ObtainRegulatoryClosureAsync(string instanceId, string userId)
        {
            var stepData = new PROCESS_STEP_DATA
            {
                DataJson = JsonSerializer.Serialize(new Dictionary<string, object>()),
                LastUpdated = DateTime.UtcNow
            };
            return await _processService.ExecuteStepAsync(instanceId, "REGULATORY_CLOSURE", stepData, userId);
        }

        public async Task<bool> CompleteDecommissioningAsync(string instanceId, string userId)
        {
            return await _processService.CompleteStepAsync(instanceId, "DECOMMISSIONING_COMPLETION", "COMPLETED", userId);
        }

        #endregion
    }
}

