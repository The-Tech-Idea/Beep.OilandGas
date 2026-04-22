using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Drilling;
using Beep.OilandGas.Models.Data.ProductionOperations;
using Beep.OilandGas.Models.Data.EnhancedRecovery;

namespace Beep.OilandGas.Client.App.Services.Operations
{
    /// <summary>
    /// Unified service for Operations
    /// </summary>
    internal class OperationsService : ServiceBase, IOperationsService
    {
        public OperationsService(BeepOilandGasApp app, ILogger<OperationsService>? logger = null)
            : base(app, logger)
        {
        }

        public async Task<DrillingOperation> CreateDrillingOperationAsync(DrillingOperation request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var createDto = new CREATE_DRILLING_OPERATION
                {
                    WellUWI = request.WellUWI,
                    WellName = string.IsNullOrWhiteSpace(request.WellName) ? null : request.WellName,
                    PlannedSpudDate = request.SpudDate,
                    TargetDepth = request.TargetDepth,
                    DrillingContractor = request.DrillingContractor,
                    RigName = request.RigName,
                    EstimatedDailyCost = request.DailyCost
                };

                var remoteResult = await PostAsync<CREATE_DRILLING_OPERATION, DRILLING_OPERATION>(
                    "/api/drillingoperation/operations",
                    createDto,
                    cancellationToken);

                return remoteResult == null
                    ? throw new InvalidOperationException("Failed to create drilling operation")
                    : MapDrillingOperation(remoteResult);
            }
            var localService = GetLocalService<IOperationsLocalService>();
            if (localService == null) throw new InvalidOperationException("IOperationsLocalService not available");
            return await localService.CreateDrillingOperationAsync(request);
        }

        public async Task<DrillingOperation> GetDrillingOperationAsync(string operationId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(operationId)) throw new ArgumentException("Operation ID is required", nameof(operationId));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var remoteResult = await GetAsync<DRILLING_OPERATION>(
                    $"/api/drillingoperation/operations/{Uri.EscapeDataString(operationId)}",
                    cancellationToken);

                return remoteResult == null
                    ? throw new InvalidOperationException($"Drilling operation {operationId} was not found")
                    : MapDrillingOperation(remoteResult);
            }
            var localService = GetLocalService<IOperationsLocalService>();
            if (localService == null) throw new InvalidOperationException("IOperationsLocalService not available");
            return await localService.GetDrillingOperationAsync(operationId);
        }

        public async Task<ProductionOperation> CreateProductionOperationAsync(ProductionOperation request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<ProductionOperation, ProductionOperation>("/api/productionoperations/create", request, cancellationToken);
            var localService = GetLocalService<IOperationsLocalService>();
            if (localService == null) throw new InvalidOperationException("IOperationsLocalService not available");
            return await localService.CreateProductionOperationAsync(request);
        }

        public async Task<EnhancedRecoveryAnalysis> AnalyzeEnhancedRecoveryAsync(EnhancedRecoveryAnalysis request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<EnhancedRecoveryAnalysis, EnhancedRecoveryAnalysis>("/api/enhancedrecovery/analyze", request, cancellationToken);
            var localService = GetLocalService<IOperationsLocalService>();
            if (localService == null) throw new InvalidOperationException("IOperationsLocalService not available");
            return await localService.AnalyzeEnhancedRecoveryAsync(request);
        }

        private static DrillingOperation MapDrillingOperation(DRILLING_OPERATION operation)
        {
            return new DrillingOperation
            {
                OperationId = operation.OPERATION_ID ?? string.Empty,
                WellUWI = operation.WELL_UWI ?? string.Empty,
                WellName = operation.WELL_NAME ?? string.Empty,
                SpudDate = operation.SPUD_DATE,
                CompletionDate = operation.COMPLETION_DATE,
                Status = operation.STATUS,
                CurrentDepth = operation.CURRENT_DEPTH,
                TargetDepth = operation.TARGET_DEPTH,
                DrillingContractor = operation.DRILLING_CONTRACTOR,
                RigName = operation.RIG_NAME,
                DailyCost = operation.DAILY_COST,
                TotalCost = operation.TOTAL_COST,
                Currency = operation.CURRENCY
            };
        }
    }

    public interface IOperationsLocalService
    {
        Task<DrillingOperation> CreateDrillingOperationAsync(DrillingOperation request);
        Task<DrillingOperation> GetDrillingOperationAsync(string operationId);
        Task<ProductionOperation> CreateProductionOperationAsync(ProductionOperation request);
        Task<EnhancedRecoveryAnalysis> AnalyzeEnhancedRecoveryAsync(EnhancedRecoveryAnalysis request);
    }
}

