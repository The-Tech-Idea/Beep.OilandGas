using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.ProductionOperations;

namespace Beep.OilandGas.Client.App.Services.Production
{
    internal partial class ProductionService
    {
        #region Operations

        public async Task<PRODUCTION_COSTS> CreateOperationAsync(PRODUCTION_COSTS request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<PRODUCTION_COSTS, PRODUCTION_COSTS>("/api/production/operations/create", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<PRODUCTION_COSTS> GetOperationStatusAsync(string operationId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(operationId)) throw new ArgumentNullException(nameof(operationId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<PRODUCTION_COSTS>($"/api/production/operations/{Uri.EscapeDataString(operationId)}", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<PRODUCTION_COSTS> UpdateOperationAsync(string operationId, PRODUCTION_COSTS request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(operationId)) throw new ArgumentNullException(nameof(operationId));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PutAsync<PRODUCTION_COSTS, PRODUCTION_COSTS>($"/api/production/operations/{Uri.EscapeDataString(operationId)}", request, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<PRODUCTION_ALLOCATION> GetProductionDataAsync(string wellId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(wellId)) throw new ArgumentNullException(nameof(wellId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<PRODUCTION_ALLOCATION>($"/api/production/data/{Uri.EscapeDataString(wellId)}", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<PRODUCTION_ALLOCATION>> GetProductionHistoryAsync(string wellId, DateRangeRequest dateRange, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(wellId)) throw new ArgumentNullException(nameof(wellId));
            if (dateRange == null) throw new ArgumentNullException(nameof(dateRange));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<DateRangeRequest, List<PRODUCTION_ALLOCATION>>($"/api/production/history/{Uri.EscapeDataString(wellId)}", dateRange, null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<PRODUCTION_ALLOCATION> RecordProductionAsync(PRODUCTION_ALLOCATION productionRecord, string? userId = null, CancellationToken cancellationToken = default)
        {
            if (productionRecord == null) throw new ArgumentNullException(nameof(productionRecord));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(userId)) queryParams["userId"] = userId;
                var endpoint = BuildRequestUriWithParams("/api/production/record", queryParams);
                return await PostAsync<PRODUCTION_ALLOCATION, PRODUCTION_ALLOCATION>(endpoint, productionRecord, null, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}
