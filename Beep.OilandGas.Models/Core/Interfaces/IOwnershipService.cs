using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Ownership;
using Beep.OilandGas.Models.DTOs.Ownership;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for ownership operations.
    /// </summary>
    public interface IOwnershipService
    {
        /// <summary>
        /// Creates a division order.
        /// </summary>
        Task<DIVISION_ORDER> CreateDivisionOrderAsync(CreateDivisionOrderRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets a division order by ID.
        /// </summary>
        Task<DIVISION_ORDER?> GetDivisionOrderAsync(string orderId, string? connectionName = null);
        
        /// <summary>
        /// Gets division orders by property.
        /// </summary>
        Task<List<DIVISION_ORDER>> GetDivisionOrdersByPropertyAsync(string propertyId, string? connectionName = null);
        
        /// <summary>
        /// Creates a transfer order.
        /// </summary>
        Task<TRANSFER_ORDER> CreateTransferOrderAsync(CreateTransferOrderRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets a transfer order by ID.
        /// </summary>
        Task<TRANSFER_ORDER?> GetTransferOrderAsync(string orderId, string? connectionName = null);
        
        /// <summary>
        /// Registers an ownership interest.
        /// </summary>
        Task<OWNERSHIP_INTEREST> RegisterOwnershipInterestAsync(CreateOwnershipInterestRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets ownership interests by property.
        /// </summary>
        Task<List<OWNERSHIP_INTEREST>> GetOwnershipInterestsByPropertyAsync(string propertyId, DateTime? asOfDate, string? connectionName = null);
        
        /// <summary>
        /// Gets ownership tree for a property.
        /// </summary>
        Task<OwnershipTree> GetOwnershipTreeAsync(string propertyId, DateTime? asOfDate, string? connectionName = null);
        
        /// <summary>
        /// Records an ownership change.
        /// </summary>
        Task<OwnershipChangeResult> RecordOwnershipChangeAsync(OwnershipChangeRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Approves an ownership change.
        /// </summary>
        Task<OwnershipApprovalResult> ApproveOwnershipChangeAsync(string changeId, string changeType, string approverId, string? connectionName = null);
        
        /// <summary>
        /// Gets ownership change history.
        /// </summary>
        Task<List<OwnershipChangeHistory>> GetOwnershipChangeHistoryAsync(string propertyId, string? connectionName = null);
    }
}

