using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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
                return await PostAsync<DrillingOperation, DrillingOperation>("/api/DRILLING_OPERATION/create", request, cancellationToken);
            var localService = GetLocalService<IOperationsLocalService>();
            if (localService == null) throw new InvalidOperationException("IOperationsLocalService not available");
            return await localService.CreateDrillingOperationAsync(request);
        }

        public async Task<DrillingOperation> GetDrillingOperationAsync(string operationId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(operationId)) throw new ArgumentException("Operation ID is required", nameof(operationId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<DrillingOperation>($"/api/DRILLING_OPERATION/{Uri.EscapeDataString(operationId)}", cancellationToken);
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
    }

    public interface IOperationsLocalService
    {
        Task<DrillingOperation> CreateDrillingOperationAsync(DrillingOperation request);
        Task<DrillingOperation> GetDrillingOperationAsync(string operationId);
        Task<ProductionOperation> CreateProductionOperationAsync(ProductionOperation request);
        Task<EnhancedRecoveryAnalysis> AnalyzeEnhancedRecoveryAsync(EnhancedRecoveryAnalysis request);
    }
}

