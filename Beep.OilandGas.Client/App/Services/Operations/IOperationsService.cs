using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionOperations;
using Beep.OilandGas.Models.Data.EnhancedRecovery;

namespace Beep.OilandGas.Client.App.Services.Operations
{
    /// <summary>
    /// Service interface for Operations (Production, Enhanced Recovery)
    /// </summary>
    public interface IOperationsService
    {
        Task<ProductionOperation> CreateProductionOperationAsync(ProductionOperation request, CancellationToken cancellationToken = default);
        Task<EnhancedRecoveryAnalysis> AnalyzeEnhancedRecoveryAsync(EnhancedRecoveryAnalysis request, CancellationToken cancellationToken = default);
    }
}

