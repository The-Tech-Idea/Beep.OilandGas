using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.LifeCycle.Models.Processes;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.LifeCycle.Services.Maintenance;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.LifeCycle.Services.Maintenance.Processes
{
    /// <summary>
    /// Service for Maintenance process orchestration
    /// </summary>
    public class MaintenanceProcessService
    {
        private readonly IProcessService _processService;
        private readonly MaintenanceManagementService _maintenanceService;
        private readonly ILogger<MaintenanceProcessService>? _logger;

        public MaintenanceProcessService(
            IProcessService processService,
            MaintenanceManagementService maintenanceService,
            ILogger<MaintenanceProcessService>? logger = null)
        {
            _processService = processService ?? throw new ArgumentNullException(nameof(processService));
            _maintenanceService = maintenanceService ?? throw new ArgumentNullException(nameof(maintenanceService));
            _logger = logger;
        }

        public async Task<ProcessInstance> StartPreventiveMaintenanceProcessAsync(string entityId, string entityType, string fieldId, string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("MAINTENANCE");
                var process = processDef.FirstOrDefault(p => p.ProcessName == "PreventiveMaintenance");
                if (process == null) throw new InvalidOperationException("PreventiveMaintenance process definition not found");
                return await _processService.StartProcessAsync(process.ProcessId, entityId, entityType, fieldId, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting Preventive Maintenance process for {entityType}: {entityId}");
                throw;
            }
        }

        public async Task<ProcessInstance> StartCorrectiveMaintenanceProcessAsync(string entityId, string entityType, string fieldId, string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("MAINTENANCE");
                var process = processDef.FirstOrDefault(p => p.ProcessName == "CorrectiveMaintenance");
                if (process == null) throw new InvalidOperationException("CorrectiveMaintenance process definition not found");
                return await _processService.StartProcessAsync(process.ProcessId, entityId, entityType, fieldId, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting Corrective Maintenance process for {entityType}: {entityId}");
                throw;
            }
        }
    }
}

