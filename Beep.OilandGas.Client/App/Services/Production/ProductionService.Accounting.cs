using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Client.App.Services.Production
{
    internal partial class ProductionService
    {
        #region Accounting

        public async Task<PRODUCTION_ALLOCATION> GetProductionVolumesAsync(string wellId, DateRangeRequest dateRange, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(wellId)) throw new ArgumentNullException(nameof(wellId));
            if (dateRange == null) throw new ArgumentNullException(nameof(dateRange));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<DateRangeRequest, PRODUCTION_ALLOCATION>($"/api/production/volumes/{Uri.EscapeDataString(wellId)}", dateRange, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<ROYALTY_CALCULATION> CalculateRoyaltiesAsync(ROYALTY_INTEREST request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<ROYALTY_INTEREST, ROYALTY_CALCULATION>("/api/production/royalties/calculate", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<COST_ALLOCATION> GetCostAllocationAsync(string entityId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(entityId)) throw new ArgumentNullException(nameof(entityId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<COST_ALLOCATION>($"/api/production/costs/{Uri.EscapeDataString(entityId)}", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<REVENUE_DISTRIBUTION> GetRevenueDistributionAsync(string entityId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(entityId)) throw new ArgumentNullException(nameof(entityId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<REVENUE_DISTRIBUTION>($"/api/production/revenue/{Uri.EscapeDataString(entityId)}", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<ALLOCATION_RESULT> GetProductionAllocationAsync(PRODUCTION_ALLOCATION request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<PRODUCTION_ALLOCATION, ALLOCATION_RESULT>("/api/production/allocation", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<ALLOCATION_RESULT> SaveProductionAllocationAsync(ALLOCATION_RESULT allocation, string? userId = null, CancellationToken cancellationToken = default)
        {
            if (allocation == null) throw new ArgumentNullException(nameof(allocation));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(userId)) queryParams["userId"] = userId;
                var endpoint = BuildRequestUriWithParams("/api/production/allocation/save", queryParams);
                return await PostAsync<ALLOCATION_RESULT, ALLOCATION_RESULT>(endpoint, allocation, null, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}
