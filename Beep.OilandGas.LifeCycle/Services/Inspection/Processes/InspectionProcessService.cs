using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.LifeCycle.Models.Processes;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.LifeCycle.Services.Inspection;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.LifeCycle.Services.Inspection.Processes
{
    /// <summary>
    /// Service for Inspection process orchestration
    /// </summary>
    public class InspectionProcessService
    {
        private readonly IProcessService _processService;
        private readonly InspectionManagementService _inspectionService;
        private readonly ILogger<InspectionProcessService>? _logger;

        public InspectionProcessService(
            IProcessService processService,
            InspectionManagementService inspectionService,
            ILogger<InspectionProcessService>? logger = null)
        {
            _processService = processService ?? throw new ArgumentNullException(nameof(processService));
            _inspectionService = inspectionService ?? throw new ArgumentNullException(nameof(inspectionService));
            _logger = logger;
        }

        public async Task<ProcessInstance> StartRegularInspectionProcessAsync(string entityId, string entityType, string fieldId, string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("INSPECTION");
                var process = processDef.FirstOrDefault(p => p.ProcessName == "RegularInspection");
                if (process == null) throw new InvalidOperationException("RegularInspection process definition not found");
                return await _processService.StartProcessAsync(process.ProcessId, entityId, entityType, fieldId, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting Regular Inspection process for {entityType}: {entityId}");
                throw;
            }
        }

        public async Task<ProcessInstance> StartComplianceInspectionProcessAsync(string entityId, string entityType, string fieldId, string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("INSPECTION");
                var process = processDef.FirstOrDefault(p => p.ProcessName == "ComplianceInspection");
                if (process == null) throw new InvalidOperationException("ComplianceInspection process definition not found");
                return await _processService.StartProcessAsync(process.ProcessId, entityId, entityType, fieldId, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting Compliance Inspection process for {entityType}: {entityId}");
                throw;
            }
        }

        public async Task<ProcessInstance> StartSafetyInspectionProcessAsync(string entityId, string entityType, string fieldId, string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("INSPECTION");
                var process = processDef.FirstOrDefault(p => p.ProcessName == "SafetyInspection");
                if (process == null) throw new InvalidOperationException("SafetyInspection process definition not found");
                return await _processService.StartProcessAsync(process.ProcessId, entityId, entityType, fieldId, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting Safety Inspection process for {entityType}: {entityId}");
                throw;
            }
        }

        public async Task<ProcessInstance> StartIntegrityInspectionProcessAsync(string entityId, string entityType, string fieldId, string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("INSPECTION");
                var process = processDef.FirstOrDefault(p => p.ProcessName == "IntegrityInspection");
                if (process == null) throw new InvalidOperationException("IntegrityInspection process definition not found");
                return await _processService.StartProcessAsync(process.ProcessId, entityId, entityType, fieldId, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting Integrity Inspection process for {entityType}: {entityId}");
                throw;
            }
        }
    }
}

