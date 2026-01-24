using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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

        public async Task<object> CreateDrillingOperationAsync(object request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<object, object>("/api/drillingoperation/create", request, cancellationToken);
            var localService = GetLocalService<IOperationsLocalService>();
            if (localService == null) throw new InvalidOperationException("IOperationsLocalService not available");
            return await localService.CreateDrillingOperationAsync(request);
        }

        public async Task<object> GetDrillingOperationAsync(string operationId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(operationId)) throw new ArgumentException("Operation ID is required", nameof(operationId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<object>($"/api/drillingoperation/{Uri.EscapeDataString(operationId)}", cancellationToken);
            var localService = GetLocalService<IOperationsLocalService>();
            if (localService == null) throw new InvalidOperationException("IOperationsLocalService not available");
            return await localService.GetDrillingOperationAsync(operationId);
        }

        public async Task<object> CreateProductionOperationAsync(object request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<object, object>("/api/productionoperations/create", request, cancellationToken);
            var localService = GetLocalService<IOperationsLocalService>();
            if (localService == null) throw new InvalidOperationException("IOperationsLocalService not available");
            return await localService.CreateProductionOperationAsync(request);
        }

        public async Task<object> AnalyzeEnhancedRecoveryAsync(object request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<object, object>("/api/enhancedrecovery/analyze", request, cancellationToken);
            var localService = GetLocalService<IOperationsLocalService>();
            if (localService == null) throw new InvalidOperationException("IOperationsLocalService not available");
            return await localService.AnalyzeEnhancedRecoveryAsync(request);
        }
    }

    public interface IOperationsLocalService
    {
        Task<object> CreateDrillingOperationAsync(object request);
        Task<object> GetDrillingOperationAsync(string operationId);
        Task<object> CreateProductionOperationAsync(object request);
        Task<object> AnalyzeEnhancedRecoveryAsync(object request);
    }
}

