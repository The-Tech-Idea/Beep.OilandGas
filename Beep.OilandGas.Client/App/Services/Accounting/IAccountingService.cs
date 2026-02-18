using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.ProductionOperations;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Client.App.Services.Accounting
{
    /// <summary>
    /// Service interface for Accounting operations
    /// </summary>
    public interface IAccountingService
    {
        Task<List<ProductionData>> GetProductionDataAsync(string wellId, CancellationToken cancellationToken = default);
        Task<ProductionData> SaveProductionDataAsync(ProductionData productionData, CancellationToken cancellationToken = default);
        Task<RoyaltyCalculationResult> CalculateRoyaltyAsync(RoyaltyCalculationRequest request, CancellationToken cancellationToken = default);
        Task<CostSummary> GetCostSummaryAsync(string entityId, CancellationToken cancellationToken = default);
        Task<RevenueSummary> GetRevenueSummaryAsync(string entityId, CancellationToken cancellationToken = default);
        Task<AllocationResult> AllocateProductionAsync(AllocationRequest allocationRequest, CancellationToken cancellationToken = default);
    }
}

