using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.LifeCycle.Models.Processes;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.LifeCycle.Services.FacilityManagement;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.LifeCycle.Services.FacilityManagement.Processes
{
    /// <summary>
    /// Service for Facility Management process orchestration
    /// </summary>
    public class FacilityManagementProcessService
    {
        private readonly IProcessService _processService;
        private readonly FacilityManagementService _facilityManagementService;
        private readonly ILogger<FacilityManagementProcessService>? _logger;

        public FacilityManagementProcessService(
            IProcessService processService,
            FacilityManagementService facilityManagementService,
            ILogger<FacilityManagementProcessService>? logger = null)
        {
            _processService = processService ?? throw new ArgumentNullException(nameof(processService));
            _facilityManagementService = facilityManagementService ?? throw new ArgumentNullException(nameof(facilityManagementService));
            _logger = logger;
        }

        public async Task<ProcessInstance> StartFacilityCreationProcessAsync(string facilityId, string fieldId, string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("FACILITY_MANAGEMENT");
                var process = processDef.FirstOrDefault(p => p.ProcessName == "FacilityCreation");
                if (process == null) throw new InvalidOperationException("FacilityCreation process definition not found");
                return await _processService.StartProcessAsync(process.ProcessId, facilityId, "FACILITY", fieldId, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting Facility Creation process for facility: {facilityId}");
                throw;
            }
        }

        public async Task<ProcessInstance> StartFacilityOperationsProcessAsync(string facilityId, string fieldId, string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("FACILITY_MANAGEMENT");
                var process = processDef.FirstOrDefault(p => p.ProcessName == "FacilityOperations");
                if (process == null) throw new InvalidOperationException("FacilityOperations process definition not found");
                return await _processService.StartProcessAsync(process.ProcessId, facilityId, "FACILITY", fieldId, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting Facility Operations process for facility: {facilityId}");
                throw;
            }
        }

        public async Task<ProcessInstance> StartFacilityMaintenanceProcessAsync(string facilityId, string fieldId, string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("FACILITY_MANAGEMENT");
                var process = processDef.FirstOrDefault(p => p.ProcessName == "FacilityMaintenance");
                if (process == null) throw new InvalidOperationException("FacilityMaintenance process definition not found");
                return await _processService.StartProcessAsync(process.ProcessId, facilityId, "FACILITY", fieldId, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting Facility Maintenance process for facility: {facilityId}");
                throw;
            }
        }

        public async Task<ProcessInstance> StartFacilityInspectionProcessAsync(string facilityId, string fieldId, string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("FACILITY_MANAGEMENT");
                var process = processDef.FirstOrDefault(p => p.ProcessName == "FacilityInspection");
                if (process == null) throw new InvalidOperationException("FacilityInspection process definition not found");
                return await _processService.StartProcessAsync(process.ProcessId, facilityId, "FACILITY", fieldId, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting Facility Inspection process for facility: {facilityId}");
                throw;
            }
        }

        public async Task<ProcessInstance> StartFacilityIntegrityProcessAsync(string facilityId, string fieldId, string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("FACILITY_MANAGEMENT");
                var process = processDef.FirstOrDefault(p => p.ProcessName == "FacilityIntegrity");
                if (process == null) throw new InvalidOperationException("FacilityIntegrity process definition not found");
                return await _processService.StartProcessAsync(process.ProcessId, facilityId, "FACILITY", fieldId, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting Facility Integrity process for facility: {facilityId}");
                throw;
            }
        }
    }
}

