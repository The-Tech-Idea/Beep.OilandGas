using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.Client.App.Services.Accounting
{
    /// <summary>
    /// Service interface for Accounting operations
    /// </summary>
    public interface IAccountingService
    {
        Task<object> GetProductionDataAsync(string wellId, CancellationToken cancellationToken = default);
        Task<object> SaveProductionDataAsync(object productionData, CancellationToken cancellationToken = default);
        Task<object> CalculateRoyaltyAsync(object request, CancellationToken cancellationToken = default);
        Task<object> GetCostSummaryAsync(string entityId, CancellationToken cancellationToken = default);
        Task<object> GetRevenueSummaryAsync(string entityId, CancellationToken cancellationToken = default);
        Task<object> AllocateProductionAsync(object allocationRequest, CancellationToken cancellationToken = default);
    }
}

