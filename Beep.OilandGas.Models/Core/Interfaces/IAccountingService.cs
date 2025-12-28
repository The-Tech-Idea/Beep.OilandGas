using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.DTOs;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Interface for Production Accounting operations
    /// All data is stored in PPDM database using PPDMGenericRepository
    /// </summary>
    public interface IAccountingService
    {
        /// <summary>
        /// Reconcile production volumes between different sources
        /// </summary>
        Task<VolumeReconciliationResult> ReconcileVolumesAsync(string fieldId, DateTime startDate, DateTime endDate, List<AppFilter>? additionalFilters = null);

        /// <summary>
        /// Calculate royalties for production
        /// </summary>
        Task<RoyaltyCalculationResult> CalculateRoyaltiesAsync(string fieldId, DateTime startDate, DateTime endDate, string? poolId = null, List<AppFilter>? additionalFilters = null);

        /// <summary>
        /// Allocate costs to production entities (wells, pools, facilities)
        /// </summary>
        Task<CostAllocationResult> AllocateCostsAsync(string fieldId, DateTime startDate, DateTime endDate, CostAllocationMethod method, List<AppFilter>? additionalFilters = null);

        /// <summary>
        /// Get accounting allocation records
        /// </summary>
        Task<List<ACCOUNTING_ALLOCATION>> GetAccountingAllocationsAsync(string? fieldId = null, string? poolId = null, string? wellId = null, DateTime? startDate = null, DateTime? endDate = null, List<AppFilter>? additionalFilters = null);

        /// <summary>
        /// Get royalty calculation records
        /// </summary>
        Task<List<DTOs.ROYALTY_CALCULATION>> GetRoyaltyCalculationsAsync(string? fieldId = null, string? poolId = null, DateTime? startDate = null, DateTime? endDate = null, List<AppFilter>? additionalFilters = null);

        /// <summary>
        /// Get cost allocation records
        /// </summary>
        Task<List<COST_ALLOCATION>> GetCostAllocationsAsync(string? fieldId = null, DateTime? startDate = null, DateTime? endDate = null, List<AppFilter>? additionalFilters = null);

        /// <summary>
        /// Create or update accounting allocation record
        /// </summary>
        Task<object> SaveAccountingAllocationAsync(object allocationData, string userId);

        /// <summary>
        /// Create or update royalty calculation record
        /// </summary>
        Task<object> SaveRoyaltyCalculationAsync(object royaltyData, string userId);

        /// <summary>
        /// Create or update cost allocation record
        /// </summary>
        Task<object> SaveCostAllocationAsync(object costAllocationData, string userId);
    }
}

