using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Royalty;
using Beep.OilandGas.Models.DTOs.Royalty;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for royalty operations.
    /// </summary>
    public interface IRoyaltyService
    {
        /// <summary>
        /// Registers a royalty interest.
        /// </summary>
        Task<ROYALTY_INTEREST> RegisterRoyaltyInterestAsync(CreateRoyaltyInterestRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets a royalty interest by ID.
        /// </summary>
        Task<ROYALTY_INTEREST?> GetRoyaltyInterestAsync(string interestId, string? connectionName = null);
        
        /// <summary>
        /// Gets royalty interests by property.
        /// </summary>
        Task<List<ROYALTY_INTEREST>> GetRoyaltyInterestsByPropertyAsync(string propertyId, string? connectionName = null);
        
        /// <summary>
        /// Calculates and creates a royalty payment.
        /// </summary>
        Task<ROYALTY_PAYMENT> CalculateAndCreatePaymentAsync(
            string revenueTransactionId,
            string royaltyOwnerBaId,
            decimal royaltyInterest,
            DateTime paymentDate,
            string userId,
            string? connectionName = null);
        
        /// <summary>
        /// Gets a royalty payment by ID.
        /// </summary>
        Task<ROYALTY_PAYMENT?> GetRoyaltyPaymentAsync(string paymentId, string? connectionName = null);
        
        /// <summary>
        /// Gets royalty payments by owner.
        /// </summary>
        Task<List<ROYALTY_PAYMENT>> GetRoyaltyPaymentsByOwnerAsync(string ownerId, DateTime? startDate, DateTime? endDate, string? connectionName = null);
        
        /// <summary>
        /// Creates a royalty statement.
        /// </summary>
        Task<ROYALTY_STATEMENT> CreateStatementAsync(CreateRoyaltyStatementRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets a royalty statement by ID.
        /// </summary>
        Task<ROYALTY_STATEMENT?> GetStatementAsync(string statementId, string? connectionName = null);
        
        /// <summary>
        /// Gets royalty owner summary.
        /// </summary>
        Task<List<RoyaltyOwnerSummary>> GetRoyaltyOwnerSummaryAsync(string ownerId, string? connectionName = null);
        
        /// <summary>
        /// Approves a royalty payment.
        /// </summary>
        Task<RoyaltyPaymentApprovalResult> ApprovePaymentAsync(string paymentId, string approverId, string? connectionName = null);
        
        /// <summary>
        /// Gets royalty audit trail.
        /// </summary>
        Task<List<RoyaltyAuditTrail>> GetRoyaltyAuditTrailAsync(string interestId, string? connectionName = null);
    }
}

