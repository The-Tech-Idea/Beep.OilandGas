using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using System.Collections.Generic;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.ProductionOperations;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Client.App.Services.Accounting
{
    /// <summary>
    /// Unified service for Accounting operations
    /// </summary>
    internal class AccountingService : ServiceBase, IAccountingService
    {
        public AccountingService(BeepOilandGasApp app, ILogger<AccountingService>? logger = null)
            : base(app, logger)
        {
        }

        public async Task<List<ProductionData>> GetProductionDataAsync(string wellId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(wellId)) throw new ArgumentException("Well ID is required", nameof(wellId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<ProductionData>>($"/api/accountingproduction/well/{Uri.EscapeDataString(wellId)}", cancellationToken);
            var localService = GetLocalService<IAccountingLocalService>();
            if (localService == null) throw new InvalidOperationException("IAccountingLocalService not available");
            return await localService.GetProductionDataAsync(wellId);
        }

        public async Task<ProductionData> SaveProductionDataAsync(ProductionData productionData, CancellationToken cancellationToken = default)
        {
            if (productionData == null) throw new ArgumentNullException(nameof(productionData));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<ProductionData, ProductionData>("/api/accountingproduction/save", productionData, cancellationToken);
            var localService = GetLocalService<IAccountingLocalService>();
            if (localService == null) throw new InvalidOperationException("IAccountingLocalService not available");
            return await localService.SaveProductionDataAsync(productionData);
        }

        public async Task<RoyaltyCalculationResult> CalculateRoyaltyAsync(RoyaltyCalculationRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<RoyaltyCalculationRequest, RoyaltyCalculationResult>("/api/accountingroyalty/calculate", request, cancellationToken);
            var localService = GetLocalService<IAccountingLocalService>();
            if (localService == null) throw new InvalidOperationException("IAccountingLocalService not available");
            return await localService.CalculateRoyaltyAsync(request);
        }

        public async Task<CostSummary> GetCostSummaryAsync(string entityId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(entityId)) throw new ArgumentException("Entity ID is required", nameof(entityId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<CostSummary>($"/api/accountingcost/summary/{Uri.EscapeDataString(entityId)}", cancellationToken);
            var localService = GetLocalService<IAccountingLocalService>();
            if (localService == null) throw new InvalidOperationException("IAccountingLocalService not available");
            return await localService.GetCostSummaryAsync(entityId);
        }

        public async Task<RevenueSummary> GetRevenueSummaryAsync(string entityId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(entityId)) throw new ArgumentException("Entity ID is required", nameof(entityId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<RevenueSummary>($"/api/accountingrevenue/summary/{Uri.EscapeDataString(entityId)}", cancellationToken);
            var localService = GetLocalService<IAccountingLocalService>();
            if (localService == null) throw new InvalidOperationException("IAccountingLocalService not available");
            return await localService.GetRevenueSummaryAsync(entityId);
        }

        public async Task<AllocationResult> AllocateProductionAsync(AllocationRequest allocationRequest, CancellationToken cancellationToken = default)
        {
            if (allocationRequest == null) throw new ArgumentNullException(nameof(allocationRequest));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<AllocationRequest, AllocationResult>("/api/accountingallocation/allocate", allocationRequest, cancellationToken);
            var localService = GetLocalService<IAccountingLocalService>();
            if (localService == null) throw new InvalidOperationException("IAccountingLocalService not available");
            return await localService.AllocateProductionAsync(allocationRequest);
        }
    }

    public interface IAccountingLocalService
    {
        Task<List<ProductionData>> GetProductionDataAsync(string wellId);
        Task<ProductionData> SaveProductionDataAsync(ProductionData productionData);
        Task<RoyaltyCalculationResult> CalculateRoyaltyAsync(RoyaltyCalculationRequest request);
        Task<CostSummary> GetCostSummaryAsync(string entityId);
        Task<RevenueSummary> GetRevenueSummaryAsync(string entityId);
        Task<AllocationResult> AllocateProductionAsync(AllocationRequest allocationRequest);
    }
}

