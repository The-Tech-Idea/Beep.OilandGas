using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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

        public async Task<object> GetProductionDataAsync(string wellId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(wellId)) throw new ArgumentException("Well ID is required", nameof(wellId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<object>($"/api/accountingproduction/well/{Uri.EscapeDataString(wellId)}", cancellationToken);
            var localService = GetLocalService<IAccountingLocalService>();
            if (localService == null) throw new InvalidOperationException("IAccountingLocalService not available");
            return await localService.GetProductionDataAsync(wellId);
        }

        public async Task<object> SaveProductionDataAsync(object productionData, CancellationToken cancellationToken = default)
        {
            if (productionData == null) throw new ArgumentNullException(nameof(productionData));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<object, object>("/api/accountingproduction/save", productionData, cancellationToken);
            var localService = GetLocalService<IAccountingLocalService>();
            if (localService == null) throw new InvalidOperationException("IAccountingLocalService not available");
            return await localService.SaveProductionDataAsync(productionData);
        }

        public async Task<object> CalculateRoyaltyAsync(object request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<object, object>("/api/accountingroyalty/calculate", request, cancellationToken);
            var localService = GetLocalService<IAccountingLocalService>();
            if (localService == null) throw new InvalidOperationException("IAccountingLocalService not available");
            return await localService.CalculateRoyaltyAsync(request);
        }

        public async Task<object> GetCostSummaryAsync(string entityId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(entityId)) throw new ArgumentException("Entity ID is required", nameof(entityId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<object>($"/api/accountingcost/summary/{Uri.EscapeDataString(entityId)}", cancellationToken);
            var localService = GetLocalService<IAccountingLocalService>();
            if (localService == null) throw new InvalidOperationException("IAccountingLocalService not available");
            return await localService.GetCostSummaryAsync(entityId);
        }

        public async Task<object> GetRevenueSummaryAsync(string entityId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(entityId)) throw new ArgumentException("Entity ID is required", nameof(entityId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<object>($"/api/accountingrevenue/summary/{Uri.EscapeDataString(entityId)}", cancellationToken);
            var localService = GetLocalService<IAccountingLocalService>();
            if (localService == null) throw new InvalidOperationException("IAccountingLocalService not available");
            return await localService.GetRevenueSummaryAsync(entityId);
        }

        public async Task<object> AllocateProductionAsync(object allocationRequest, CancellationToken cancellationToken = default)
        {
            if (allocationRequest == null) throw new ArgumentNullException(nameof(allocationRequest));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<object, object>("/api/accountingallocation/allocate", allocationRequest, cancellationToken);
            var localService = GetLocalService<IAccountingLocalService>();
            if (localService == null) throw new InvalidOperationException("IAccountingLocalService not available");
            return await localService.AllocateProductionAsync(allocationRequest);
        }
    }

    public interface IAccountingLocalService
    {
        Task<object> GetProductionDataAsync(string wellId);
        Task<object> SaveProductionDataAsync(object productionData);
        Task<object> CalculateRoyaltyAsync(object request);
        Task<object> GetCostSummaryAsync(string entityId);
        Task<object> GetRevenueSummaryAsync(string entityId);
        Task<object> AllocateProductionAsync(object allocationRequest);
    }
}

