using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.DTOs.Accounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for purchase order operations.
    /// </summary>
    public interface IPurchaseOrderService
    {
        /// <summary>
        /// Creates a new purchase order.
        /// </summary>
        Task<PURCHASE_ORDER> CreatePurchaseOrderAsync(CreatePurchaseOrderRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets a purchase order by ID.
        /// </summary>
        Task<PURCHASE_ORDER?> GetPurchaseOrderAsync(string poId, string? connectionName = null);
        
        /// <summary>
        /// Gets purchase orders by vendor.
        /// </summary>
        Task<List<PURCHASE_ORDER>> GetPurchaseOrdersByVendorAsync(string vendorId, DateTime? startDate, DateTime? endDate, string? connectionName = null);
        
        /// <summary>
        /// Updates a purchase order.
        /// </summary>
        Task<PURCHASE_ORDER> UpdatePurchaseOrderAsync(UpdatePurchaseOrderRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Deletes a purchase order (soft delete by setting ACTIVE_IND = 'N').
        /// </summary>
        Task<bool> DeletePurchaseOrderAsync(string poId, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets purchase order line items.
        /// </summary>
        Task<List<PO_LINE_ITEM>> GetPOLineItemsAsync(string poId, string? connectionName = null);
        
        /// <summary>
        /// Creates a PO receipt.
        /// </summary>
        Task<PO_RECEIPT> CreateReceiptAsync(CreatePOReceiptRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets receipts for a purchase order.
        /// </summary>
        Task<List<PO_RECEIPT>> GetReceiptsByPOAsync(string poId, string? connectionName = null);
        
        /// <summary>
        /// Approves a purchase order.
        /// </summary>
        Task<POApprovalResult> ApprovePurchaseOrderAsync(string poId, string approverId, string? connectionName = null);
        
        /// <summary>
        /// Gets purchase order status summary.
        /// </summary>
        Task<POStatusSummary> GetPOStatusAsync(string poId, string? connectionName = null);
        
        /// <summary>
        /// Gets purchase orders requiring approval.
        /// </summary>
        Task<List<PURCHASE_ORDER>> GetPOsRequiringApprovalAsync(string? connectionName = null);
    }
}

