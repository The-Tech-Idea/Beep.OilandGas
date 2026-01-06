using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Pricing;
using Beep.OilandGas.Models.DTOs.Pricing;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for pricing operations.
    /// </summary>
    public interface IPricingService
    {
        /// <summary>
        /// Values a run ticket.
        /// </summary>
        Task<RUN_TICKET_VALUATION> ValueRunTicketAsync(ValueRunTicketRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets a valuation by ID.
        /// </summary>
        Task<RUN_TICKET_VALUATION?> GetValuationAsync(string valuationId, string? connectionName = null);
        
        /// <summary>
        /// Gets valuations by run ticket.
        /// </summary>
        Task<List<RUN_TICKET_VALUATION>> GetValuationsByRunTicketAsync(string runTicketNumber, string? connectionName = null);
        
        /// <summary>
        /// Creates a price index.
        /// </summary>
        Task<PRICE_INDEX> CreatePriceIndexAsync(CreatePriceIndexRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets the latest price for an index.
        /// </summary>
        Task<PRICE_INDEX?> GetLatestPriceAsync(string indexName, string? connectionName = null);
        
        /// <summary>
        /// Gets price history for an index.
        /// </summary>
        Task<List<PRICE_INDEX>> GetPriceHistoryAsync(string indexName, DateTime? startDate, DateTime? endDate, string? connectionName = null);
        
        /// <summary>
        /// Reconciles pricing.
        /// </summary>
        Task<PricingReconciliationResult> ReconcilePricingAsync(PricingReconciliationRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets pricing approvals.
        /// </summary>
        Task<List<PricingApproval>> GetPricingApprovalsAsync(string? connectionName = null);
        
        /// <summary>
        /// Approves pricing.
        /// </summary>
        Task<PricingApprovalResult> ApprovePricingAsync(string valuationId, string approverId, string? connectionName = null);
    }
}




