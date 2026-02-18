using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Drilling;
using Beep.OilandGas.Models.Data.ProductionOperations;
using Beep.OilandGas.Models.Data.EnhancedRecovery;

namespace Beep.OilandGas.Client.App.Services.Operations
{
    /// <summary>
    /// Service interface for Operations (Drilling, Production, Enhanced Recovery)
    /// </summary>
    public interface IOperationsService
    {
        Task<DrillingOperation> CreateDrillingOperationAsync(DrillingOperation request, CancellationToken cancellationToken = default);
        Task<DrillingOperation> GetDrillingOperationAsync(string operationId, CancellationToken cancellationToken = default);
        Task<ProductionOperation> CreateProductionOperationAsync(ProductionOperation request, CancellationToken cancellationToken = default);
        Task<EnhancedRecoveryAnalysis> AnalyzeEnhancedRecoveryAsync(EnhancedRecoveryAnalysis request, CancellationToken cancellationToken = default);
    }
}

