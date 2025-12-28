using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.LifeCycle.Models.Processes;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.LifeCycle.Services.Operations;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.LifeCycle.Services.Operations.Processes
{
    /// <summary>
    /// Service for Operations process orchestration
    /// </summary>
    public class OperationsProcessService
    {
        private readonly IProcessService _processService;
        private readonly OperationsManagementService _operationsService;
        private readonly ILogger<OperationsProcessService>? _logger;

        public OperationsProcessService(
            IProcessService processService,
            OperationsManagementService operationsService,
            ILogger<OperationsProcessService>? logger = null)
        {
            _processService = processService ?? throw new ArgumentNullException(nameof(processService));
            _operationsService = operationsService ?? throw new ArgumentNullException(nameof(operationsService));
            _logger = logger;
        }

        public async Task<ProcessInstance> StartDailyOperationsProcessAsync(string entityId, string entityType, string fieldId, string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("OPERATIONS");
                var process = processDef.FirstOrDefault(p => p.ProcessName == "DailyOperations");
                if (process == null) throw new InvalidOperationException("DailyOperations process definition not found");
                return await _processService.StartProcessAsync(process.ProcessId, entityId, entityType, fieldId, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting Daily Operations process for {entityType}: {entityId}");
                throw;
            }
        }

        public async Task<ProcessInstance> StartIncidentManagementProcessAsync(string entityId, string entityType, string fieldId, string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("OPERATIONS");
                var process = processDef.FirstOrDefault(p => p.ProcessName == "IncidentManagement");
                if (process == null) throw new InvalidOperationException("IncidentManagement process definition not found");
                return await _processService.StartProcessAsync(process.ProcessId, entityId, entityType, fieldId, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting Incident Management process for {entityType}: {entityId}");
                throw;
            }
        }
    }
}

