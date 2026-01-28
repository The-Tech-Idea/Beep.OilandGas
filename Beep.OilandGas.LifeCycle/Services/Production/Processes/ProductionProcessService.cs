using System;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.LifeCycle.Models.Processes;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.LifeCycle.Services.Production;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Beep.OilandGas.Models.Data.LifeCycle;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.ProductionAccounting;

// Alias to resolve type conflicts
using DataModels = Beep.OilandGas.Models.Data;
using WellTestRequestData = Beep.OilandGas.Models.Data.WellTestRequest;

namespace Beep.OilandGas.LifeCycle.Services.Production.Processes
{
    /// <summary>
    /// Service for Production process orchestration
    /// Handles Well Startup, Production Operations, Decline Management, and Workover workflows
    /// </summary>
    public class ProductionProcessService
    {
        private readonly IProcessService _processService;
        private readonly PPDMProductionService _productionService;
        private readonly ILogger<ProductionProcessService>? _logger;

        public ProductionProcessService(
            IProcessService processService,
            PPDMProductionService productionService,
            ILogger<ProductionProcessService>? logger = null)
        {
            _processService = processService ?? throw new ArgumentNullException(nameof(processService));
            _productionService = productionService ?? throw new ArgumentNullException(nameof(productionService));
            _logger = logger;
        }

        #region Well Production Startup Process

        public async Task<ProcessInstance> StartWellProductionStartupProcessAsync(string wellId, string fieldId, string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("PRODUCTION");
                var startupProcess = processDef.FirstOrDefault(p => p.ProcessName == "WellStartup");
                
                if (startupProcess == null)
                {
                    throw new InvalidOperationException("WellStartup process definition not found");
                }

                return await _processService.StartProcessAsync(
                    startupProcess.ProcessId,
                    wellId,
                    "WELL",
                    fieldId,
                    userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting Well Production Startup process for well: {wellId}");
                throw;
            }
        }

        public async Task<bool> PerformProductionTestAsync(string instanceId, WellTestRequestData testData, string userId)
        {
            var stepData = new PROCESS_STEP_DATA
            {
                StepInstanceId = instanceId,
                StepType = "PRODUCTION_TESTING",
                WellTest = testData,
                LastUpdated = DateTime.UtcNow
            };
            return await _processService.ExecuteStepAsync(instanceId, "PRODUCTION_TESTING", stepData, userId);
        }

        public async Task<bool> ApproveProductionAsync(string instanceId, string userId)
        {
            return await _processService.CompleteStepAsync(instanceId, "PRODUCTION_APPROVAL", "APPROVED", userId);
        }

        public async Task<bool> StartProductionAsync(string instanceId, PRODUCTION_FORECAST productionData, string userId)
        {
            var stepData = new PROCESS_STEP_DATA
            {
                StepInstanceId = instanceId,
                StepType = "PRODUCTION_START",
                PRODUCTION_FORECAST = productionData, // Assuming PROCESS_STEP_DATA uses ProductionForecasting.PRODUCTION_FORECAST
                LastUpdated = DateTime.UtcNow
            };
            var result = await _processService.ExecuteStepAsync(instanceId, "PRODUCTION_START", stepData, userId);
            if (result)
            {
                await _processService.CompleteStepAsync(instanceId, "PRODUCTION_START", "SUCCESS", userId);
            }
            return result;
        }

        public async Task<bool> ConfirmProducingAsync(string instanceId, string userId)
        {
            return await _processService.CompleteStepAsync(instanceId, "PRODUCING_CONFIRMATION", "PRODUCING", userId);
        }

        #endregion

        #region Production Operations Process

        public async Task<ProcessInstance> StartProductionOperationsProcessAsync(string wellId, string fieldId, string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("PRODUCTION");
                var operationsProcess = processDef.FirstOrDefault(p => p.ProcessName == "ProductionOperations");
                
                if (operationsProcess == null)
                {
                    throw new InvalidOperationException("ProductionOperations process definition not found");
                }

                return await _processService.StartProcessAsync(
                    operationsProcess.ProcessId,
                    wellId,
                    "WELL",
                    fieldId,
                    userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting Production Operations process for well: {wellId}");
                throw;
            }
        }

        public async Task<bool> RecordDailyProductionAsync(string instanceId, DailyOperationsRequest productionData, string userId)
        {
            var stepData = new PROCESS_STEP_DATA
            {
                StepInstanceId = instanceId,
                StepType = "DAILY_PRODUCTION",
                DailyOperations = productionData,
                LastUpdated = DateTime.UtcNow
            };
            return await _processService.ExecuteStepAsync(instanceId, "DAILY_PRODUCTION", stepData, userId);
        }

        public async Task<bool> MonitorProductionAsync(string instanceId, string userId)
        {
            var stepData = new PROCESS_STEP_DATA
            {
                StepInstanceId = instanceId,
                StepType = "PRODUCTION_MONITORING",
                LastUpdated = DateTime.UtcNow
            };
            return await _processService.ExecuteStepAsync(instanceId, "PRODUCTION_MONITORING", stepData, userId);
        }

        public async Task<bool> AnalyzePerformanceAsync(string instanceId, WellTestRequestData analysisData, string userId)
        {
            var stepData = new PROCESS_STEP_DATA
            {
                StepInstanceId = instanceId,
                StepType = "PERFORMANCE_ANALYSIS",
                WellTest = analysisData,
                LastUpdated = DateTime.UtcNow
            };
            return await _processService.ExecuteStepAsync(instanceId, "PERFORMANCE_ANALYSIS", stepData, userId);
        }

        public async Task<bool> MakeOptimizationDecisionAsync(string instanceId, WorkOrderCreationRequest decisionData, string userId)
        {
            var stepData = new PROCESS_STEP_DATA
            {
                StepInstanceId = instanceId,
                StepType = "OPTIMIZATION_DECISION",
                WorkOrderCreation = decisionData,
                LastUpdated = DateTime.UtcNow
            };
            return await _processService.ExecuteStepAsync(instanceId, "OPTIMIZATION_DECISION", stepData, userId);
        }

        public async Task<bool> ExecuteOptimizationAsync(string instanceId, WorkOrderUpdateRequest optimizationData, string userId)
        {
            var stepData = new PROCESS_STEP_DATA
            {
                StepInstanceId = instanceId,
                StepType = "OPTIMIZATION_EXECUTION",
                WorkOrderUpdate = optimizationData,
                LastUpdated = DateTime.UtcNow
            };
            return await _processService.ExecuteStepAsync(instanceId, "OPTIMIZATION_EXECUTION", stepData, userId);
        }

        #endregion

        #region Production Decline Management Process

        public async Task<ProcessInstance> StartDeclineManagementProcessAsync(string wellId, string fieldId, string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("PRODUCTION");
                var declineProcess = processDef.FirstOrDefault(p => p.ProcessName == "DeclineManagement");
                
                if (declineProcess == null)
                {
                    throw new InvalidOperationException("DeclineManagement process definition not found");
                }

                return await _processService.StartProcessAsync(
                    declineProcess.ProcessId,
                    wellId,
                    "WELL",
                    fieldId,
                    userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting Decline Management process for well: {wellId}");
                throw;
            }
        }

        public async Task<bool> DetectDeclineAsync(string instanceId, string userId)
        {
            var stepData = new PROCESS_STEP_DATA
            {
                StepInstanceId = instanceId,
                StepType = "DECLINE_DETECTION",
                LastUpdated = DateTime.UtcNow
            };
            return await _processService.ExecuteStepAsync(instanceId, "DECLINE_DETECTION", stepData, userId);
        }

        public async Task<bool> PerformDCAAnalysisAsync(string instanceId, DCARequest dcaData, string userId)
        {
            var stepData = new PROCESS_STEP_DATA
            {
                StepInstanceId = instanceId,
                StepType = "DCA_ANALYSIS",
                DCA = dcaData,
                LastUpdated = DateTime.UtcNow
            };
            return await _processService.ExecuteStepAsync(instanceId, "DCA_ANALYSIS", stepData, userId);
        }

        public async Task<bool> ForecastProductionAsync(string instanceId, DeclineCurveAnalysisRequest forecastData, string userId)
        {
            var stepData = new PROCESS_STEP_DATA
            {
                StepInstanceId = instanceId,
                StepType = "PRODUCTION_FORECAST",
                DeclineCurveAnalysis = forecastData,
                LastUpdated = DateTime.UtcNow
            };
            return await _processService.ExecuteStepAsync(instanceId, "PRODUCTION_FORECAST", stepData, userId);
        }

        public async Task<bool> AnalyzeEconomicsAsync(string instanceId, EconomicAnalysisRequest economicData, string userId)
        {
            var stepData = new PROCESS_STEP_DATA
            {
                StepInstanceId = instanceId,
                StepType = "ECONOMIC_ANALYSIS",
                EconomicAnalysis = economicData,
                LastUpdated = DateTime.UtcNow
            };
            return await _processService.ExecuteStepAsync(instanceId, "ECONOMIC_ANALYSIS", stepData, userId);
        }

        public async Task<bool> MakeWorkoverDecisionAsync(string instanceId, WorkOrderCreationRequest decisionData, string userId)
        {
            var stepData = new PROCESS_STEP_DATA
            {
                StepInstanceId = instanceId,
                StepType = "WORKOVER_DECISION",
                WorkOrderCreation = decisionData,
                LastUpdated = DateTime.UtcNow
            };
            return await _processService.ExecuteStepAsync(instanceId, "WORKOVER_DECISION", stepData, userId);
        }

        #endregion

        #region Workover Process

        public async Task<ProcessInstance> StartWorkoverProcessAsync(string wellId, string fieldId, string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("PRODUCTION");
                var workoverProcess = processDef.FirstOrDefault(p => p.ProcessName == "Workover");
                
                if (workoverProcess == null)
                {
                    throw new InvalidOperationException("Workover process definition not found");
                }

                return await _processService.StartProcessAsync(
                    workoverProcess.ProcessId,
                    wellId,
                    "WELL",
                    fieldId,
                    userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting Workover process for well: {wellId}");
                throw;
            }
        }

        public async Task<bool> PlanWorkoverAsync(string instanceId, WorkOrderCreationRequest planData, string userId)
        {
            var stepData = new PROCESS_STEP_DATA
            {
                StepInstanceId = instanceId,
                StepType = "WORKOVER_PLANNING",
                WorkOrderCreation = planData,
                LastUpdated = DateTime.UtcNow
            };
            return await _processService.ExecuteStepAsync(instanceId, "WORKOVER_PLANNING", stepData, userId);
        }

        public async Task<bool> ApproveWorkoverAsync(string instanceId, string userId)
        {
            return await _processService.CompleteStepAsync(instanceId, "WORKOVER_APPROVAL", "APPROVED", userId);
        }

        public async Task<bool> ExecuteWorkoverAsync(string instanceId, WorkOrderUpdateRequest executionData, string userId)
        {
            var stepData = new PROCESS_STEP_DATA
            {
                StepInstanceId = instanceId,
                StepType = "WORKOVER_EXECUTION",
                WorkOrderUpdate = executionData,
                LastUpdated = DateTime.UtcNow
            };
            return await _processService.ExecuteStepAsync(instanceId, "WORKOVER_EXECUTION", stepData, userId);
        }

        public async Task<bool> TestPostWorkoverAsync(string instanceId, WellTestRequestData testData, string userId)
        {
            var stepData = new PROCESS_STEP_DATA
            {
                StepInstanceId = instanceId,
                StepType = "POST_WORKOVER_TESTING",
                WellTest = testData,
                LastUpdated = DateTime.UtcNow
            };
            return await _processService.ExecuteStepAsync(instanceId, "POST_WORKOVER_TESTING", stepData, userId);
        }

        public async Task<bool> RestartProductionAsync(string instanceId, PRODUCTION_FORECAST productionData, string userId)
        {
            var stepData = new PROCESS_STEP_DATA
            {
                StepInstanceId = instanceId,
                StepType = "PRODUCTION_RESTART",
                PRODUCTION_FORECAST = productionData,
                LastUpdated = DateTime.UtcNow
            };
            var result = await _processService.ExecuteStepAsync(instanceId, "PRODUCTION_RESTART", stepData, userId);
            if (result)
            {
                await _processService.CompleteStepAsync(instanceId, "PRODUCTION_RESTART", "SUCCESS", userId);
            }
            return result;
        }

        #endregion
    }
}
