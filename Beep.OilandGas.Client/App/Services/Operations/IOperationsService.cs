using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.Client.App.Services.Operations
{
    /// <summary>
    /// Service interface for Operations (Drilling, Production, Enhanced Recovery)
    /// </summary>
    public interface IOperationsService
    {
        Task<object> CreateDrillingOperationAsync(object request, CancellationToken cancellationToken = default);
        Task<object> GetDrillingOperationAsync(string operationId, CancellationToken cancellationToken = default);
        Task<object> CreateProductionOperationAsync(object request, CancellationToken cancellationToken = default);
        Task<object> AnalyzeEnhancedRecoveryAsync(object request, CancellationToken cancellationToken = default);
    }
}

