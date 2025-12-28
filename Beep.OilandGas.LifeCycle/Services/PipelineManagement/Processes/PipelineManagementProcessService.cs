using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.LifeCycle.Models.Processes;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.LifeCycle.Services.PipelineManagement;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.LifeCycle.Services.PipelineManagement.Processes
{
    /// <summary>
    /// Service for Pipeline Management process orchestration
    /// </summary>
    public class PipelineManagementProcessService
    {
        private readonly IProcessService _processService;
        private readonly PipelineManagementService _pipelineManagementService;
        private readonly ILogger<PipelineManagementProcessService>? _logger;

        public PipelineManagementProcessService(
            IProcessService processService,
            PipelineManagementService pipelineManagementService,
            ILogger<PipelineManagementProcessService>? logger = null)
        {
            _processService = processService ?? throw new ArgumentNullException(nameof(processService));
            _pipelineManagementService = pipelineManagementService ?? throw new ArgumentNullException(nameof(pipelineManagementService));
            _logger = logger;
        }

        public async Task<ProcessInstance> StartPipelineCreationProcessAsync(string pipelineId, string fieldId, string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("PIPELINE_MANAGEMENT");
                var process = processDef.FirstOrDefault(p => p.ProcessName == "PipelineCreation");
                if (process == null) throw new InvalidOperationException("PipelineCreation process definition not found");
                return await _processService.StartProcessAsync(process.ProcessId, pipelineId, "PIPELINE", fieldId, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting Pipeline Creation process for pipeline: {pipelineId}");
                throw;
            }
        }

        public async Task<ProcessInstance> StartPipelineOperationsProcessAsync(string pipelineId, string fieldId, string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("PIPELINE_MANAGEMENT");
                var process = processDef.FirstOrDefault(p => p.ProcessName == "PipelineOperations");
                if (process == null) throw new InvalidOperationException("PipelineOperations process definition not found");
                return await _processService.StartProcessAsync(process.ProcessId, pipelineId, "PIPELINE", fieldId, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting Pipeline Operations process for pipeline: {pipelineId}");
                throw;
            }
        }

        public async Task<ProcessInstance> StartPipelineMaintenanceProcessAsync(string pipelineId, string fieldId, string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("PIPELINE_MANAGEMENT");
                var process = processDef.FirstOrDefault(p => p.ProcessName == "PipelineMaintenance");
                if (process == null) throw new InvalidOperationException("PipelineMaintenance process definition not found");
                return await _processService.StartProcessAsync(process.ProcessId, pipelineId, "PIPELINE", fieldId, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting Pipeline Maintenance process for pipeline: {pipelineId}");
                throw;
            }
        }

        public async Task<ProcessInstance> StartPipelineInspectionProcessAsync(string pipelineId, string fieldId, string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("PIPELINE_MANAGEMENT");
                var process = processDef.FirstOrDefault(p => p.ProcessName == "PipelineInspection");
                if (process == null) throw new InvalidOperationException("PipelineInspection process definition not found");
                return await _processService.StartProcessAsync(process.ProcessId, pipelineId, "PIPELINE", fieldId, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting Pipeline Inspection process for pipeline: {pipelineId}");
                throw;
            }
        }

        public async Task<ProcessInstance> StartPipelineIntegrityProcessAsync(string pipelineId, string fieldId, string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("PIPELINE_MANAGEMENT");
                var process = processDef.FirstOrDefault(p => p.ProcessName == "PipelineIntegrity");
                if (process == null) throw new InvalidOperationException("PipelineIntegrity process definition not found");
                return await _processService.StartProcessAsync(process.ProcessId, pipelineId, "PIPELINE", fieldId, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting Pipeline Integrity process for pipeline: {pipelineId}");
                throw;
            }
        }
    }
}

