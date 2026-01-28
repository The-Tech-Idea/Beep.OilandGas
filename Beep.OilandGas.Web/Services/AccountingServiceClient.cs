using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Web.Services
{
    /// <summary>
    /// Royalty calculation request
    /// </summary>
    public class RoyaltyCalculationRequest
    {
        public string FieldId { get; set; } = string.Empty;
        public string? WellId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? ProductType { get; set; }
    }

    /// <summary>
    /// Royalty calculation response
    /// </summary>
    public class RoyaltyCalculationResponse
    {
        public bool Success { get; set; }
        public decimal TotalRoyalty { get; set; }
        public List<RoyaltyDetail> Details { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Royalty detail
    /// </summary>
    public class RoyaltyDetail
    {
        public string WellId { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public decimal Volume { get; set; }
        public decimal RoyaltyRate { get; set; }
        public decimal RoyaltyAmount { get; set; }
    }

    /// <summary>
    /// Cost allocation request
    /// </summary>
    public class CostAllocationRequest
    {
        public string FieldId { get; set; } = string.Empty;
        public decimal TotalCost { get; set; }
        public string AllocationMethod { get; set; } = string.Empty;
        public DateTime AllocationDate { get; set; }
        public string? CostCategory { get; set; }
    }

    /// <summary>
    /// Cost allocation response
    /// </summary>
    public class CostAllocationResponse
    {
        public bool Success { get; set; }
        public List<ALLOCATION_DETAIL> Allocations { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Allocation detail
    /// </summary>
    public class ALLOCATION_DETAIL
    {
        public string EntityId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public decimal AllocatedAmount { get; set; }
        public decimal AllocationPercentage { get; set; }
    }

    /// <summary>
    /// Volume reconciliation request
    /// </summary>
    public class VolumeReconciliationRequest
    {
        public string FieldId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? ProductType { get; set; }
    }

    /// <summary>
    /// Volume reconciliation response
    /// </summary>
    public class VolumeReconciliationResponse
    {
        public bool Success { get; set; }
        public Dictionary<string, decimal> Volumes { get; set; } = new();
        public Dictionary<string, decimal> Discrepancies { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Service interface for accounting operations
    /// </summary>
    public interface IAccountingServiceClient
    {
        // Royalty Operations
        Task<RoyaltyCalculationResponse> CalculateRoyaltiesAsync(RoyaltyCalculationRequest request);
        Task<RoyaltyCalculationResponse> GetRoyaltyCalculationsAsync(string fieldId, DateTime? startDate = null, DateTime? endDate = null);

        // Cost Allocation Operations
        Task<CostAllocationResponse> AllocateCostsAsync(CostAllocationRequest request);
        Task<CostAllocationResponse> GetCostAllocationsAsync(string fieldId, DateTime? startDate = null, DateTime? endDate = null);

        // Volume Reconciliation Operations
        Task<VolumeReconciliationResponse> ReconcileVolumesAsync(VolumeReconciliationRequest request);
    }

    /// <summary>
    /// Client service for accounting operations
    /// </summary>
    public class AccountingServiceClient : IAccountingServiceClient
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<AccountingServiceClient> _logger;

        public AccountingServiceClient(
            ApiClient apiClient,
            ILogger<AccountingServiceClient> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Calculate royalties
        /// </summary>
        public async Task<RoyaltyCalculationResponse> CalculateRoyaltiesAsync(RoyaltyCalculationRequest request)
        {
            try
            {
                var response = await _apiClient.PostAsync<RoyaltyCalculationRequest, RoyaltyCalculationResponse>(
                    "/api/accounting/royalty/calculate", request);
                return response ?? new RoyaltyCalculationResponse
                {
                    Success = false,
                    ErrorMessage = "Failed to calculate royalties"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating royalties");
                return new RoyaltyCalculationResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Get royalty calculations
        /// </summary>
        public async Task<RoyaltyCalculationResponse> GetRoyaltyCalculationsAsync(
            string fieldId, 
            DateTime? startDate = null, 
            DateTime? endDate = null)
        {
            try
            {
                var endpoint = $"/api/accounting/royalty/calculations?fieldId={Uri.EscapeDataString(fieldId)}";
                if (startDate.HasValue)
                {
                    endpoint += $"&startDate={startDate.Value:yyyy-MM-dd}";
                }
                if (endDate.HasValue)
                {
                    endpoint += $"&endDate={endDate.Value:yyyy-MM-dd}";
                }
                var response = await _apiClient.GetAsync<RoyaltyCalculationResponse>(endpoint);
                return response ?? new RoyaltyCalculationResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting royalty calculations for field {FieldId}", fieldId);
                return new RoyaltyCalculationResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Allocate costs
        /// </summary>
        public async Task<CostAllocationResponse> AllocateCostsAsync(CostAllocationRequest request)
        {
            try
            {
                var response = await _apiClient.PostAsync<CostAllocationRequest, CostAllocationResponse>(
                    "/api/accounting/cost-allocation/allocate", request);
                return response ?? new CostAllocationResponse
                {
                    Success = false,
                    ErrorMessage = "Failed to allocate costs"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error allocating costs");
                return new CostAllocationResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Get cost allocations
        /// </summary>
        public async Task<CostAllocationResponse> GetCostAllocationsAsync(
            string fieldId, 
            DateTime? startDate = null, 
            DateTime? endDate = null)
        {
            try
            {
                var endpoint = $"/api/accounting/cost-allocation/allocations?fieldId={Uri.EscapeDataString(fieldId)}";
                if (startDate.HasValue)
                {
                    endpoint += $"&startDate={startDate.Value:yyyy-MM-dd}";
                }
                if (endDate.HasValue)
                {
                    endpoint += $"&endDate={endDate.Value:yyyy-MM-dd}";
                }
                var response = await _apiClient.GetAsync<CostAllocationResponse>(endpoint);
                return response ?? new CostAllocationResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cost allocations for field {FieldId}", fieldId);
                return new CostAllocationResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Reconcile volumes
        /// </summary>
        public async Task<VolumeReconciliationResponse> ReconcileVolumesAsync(VolumeReconciliationRequest request)
        {
            try
            {
                var response = await _apiClient.PostAsync<VolumeReconciliationRequest, VolumeReconciliationResponse>(
                    "/api/accounting/volume-reconciliation/reconcile", request);
                return response ?? new VolumeReconciliationResponse
                {
                    Success = false,
                    ErrorMessage = "Failed to reconcile volumes"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reconciling volumes");
                return new VolumeReconciliationResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}

