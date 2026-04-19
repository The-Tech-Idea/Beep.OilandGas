using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.Accounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for accounts payable operations.
    /// </summary>
    public interface IAPService
    {
        /// <summary>
        /// Creates a new AP invoice.
        /// </summary>
        Task<AP_INVOICE> CreateInvoiceAsync(CreateAPInvoiceRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets an AP invoice by ID.
        /// </summary>
        Task<AP_INVOICE?> GetInvoiceAsync(string invoiceId, string? connectionName = null);
        
        /// <summary>
        /// Gets invoices by vendor.
        /// </summary>
        Task<List<AP_INVOICE>> GetInvoicesByVendorAsync(string vendorId, DateTime? startDate, DateTime? endDate, string? connectionName = null);
        
        /// <summary>
        /// Updates an AP invoice.
        /// </summary>
        Task<AP_INVOICE> UpdateInvoiceAsync(UpdateAPInvoiceRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Deletes an AP invoice (soft delete by setting ACTIVE_IND = 'N').
        /// </summary>
        Task<bool> DeleteInvoiceAsync(string invoiceId, string userId, string? connectionName = null);
        
        /// <summary>
        /// Creates a payment against an AP invoice.
        /// </summary>
        Task<AP_PAYMENT> CreatePaymentAsync(CreateAPPaymentRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets all payments for an AP invoice.
        /// </summary>
        Task<List<AP_PAYMENT>> GetPaymentsByInvoiceAsync(string invoiceId, string? connectionName = null);
        
        /// <summary>
        /// Processes a payment (marks as processed).
        /// </summary>
        Task<AP_PAYMENT> ProcessPaymentAsync(string paymentId, string userId, string? connectionName = null);
        
        /// <summary>
        /// Creates a credit memo.
        /// </summary>
        Task<AP_CREDIT_MEMO> CreateCreditMemoAsync(CreateAPCreditMemoRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Approves an AP invoice.
        /// </summary>
        Task<APApprovalResult> ApproveInvoiceAsync(string invoiceId, string approverId, string? connectionName = null);
        
        /// <summary>
        /// Gets AP aging summary.
        /// </summary>
        Task<List<APAgingSummary>> GetAPAgingAsync(string? vendorId, string? connectionName = null);
        
        /// <summary>
        /// Gets vendor summary.
        /// </summary>
        Task<VendorSummary> GetVendorSummaryAsync(string vendorId, string? connectionName = null);
        
        /// <summary>
        /// Processes a batch of payments.
        /// </summary>
        Task<PaymentBatchResult> ProcessPaymentBatchAsync(PaymentBatchRequest request, string userId, string? connectionName = null);
    }
}




