using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.DTOs.Accounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for accounts receivable operations.
    /// </summary>
    public interface IARService
    {
        /// <summary>
        /// Creates a new AR invoice.
        /// </summary>
        Task<AR_INVOICE> CreateInvoiceAsync(CreateARInvoiceRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets an AR invoice by ID.
        /// </summary>
        Task<AR_INVOICE?> GetInvoiceAsync(string invoiceId, string? connectionName = null);
        
        /// <summary>
        /// Gets invoices by customer.
        /// </summary>
        Task<List<AR_INVOICE>> GetInvoicesByCustomerAsync(string customerId, DateTime? startDate, DateTime? endDate, string? connectionName = null);
        
        /// <summary>
        /// Updates an AR invoice.
        /// </summary>
        Task<AR_INVOICE> UpdateInvoiceAsync(UpdateARInvoiceRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Deletes an AR invoice (soft delete by setting ACTIVE_IND = 'N').
        /// </summary>
        Task<bool> DeleteInvoiceAsync(string invoiceId, string userId, string? connectionName = null);
        
        /// <summary>
        /// Creates a payment from a customer.
        /// </summary>
        Task<AR_PAYMENT> CreatePaymentAsync(CreateARPaymentRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Applies a payment to an invoice.
        /// </summary>
        Task<AR_PAYMENT> ApplyPaymentAsync(string paymentId, string invoiceId, decimal amount, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets all payments for an AR invoice.
        /// </summary>
        Task<List<AR_PAYMENT>> GetPaymentsByInvoiceAsync(string invoiceId, string? connectionName = null);
        
        /// <summary>
        /// Creates a credit memo.
        /// </summary>
        Task<AR_CREDIT_MEMO> CreateCreditMemoAsync(CreateARCreditMemoRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Approves an AR invoice.
        /// </summary>
        Task<ARApprovalResult> ApproveInvoiceAsync(string invoiceId, string approverId, string? connectionName = null);
        
        /// <summary>
        /// Gets AR aging summary.
        /// </summary>
        Task<List<ARAgingSummary>> GetARAgingAsync(string? customerId, string? connectionName = null);
        
        /// <summary>
        /// Gets customer summary.
        /// </summary>
        Task<CustomerSummary> GetCustomerSummaryAsync(string customerId, string? connectionName = null);
        
        /// <summary>
        /// Applies a payment to multiple invoices.
        /// </summary>
        Task<PaymentApplicationResult> ApplyPaymentToInvoicesAsync(PaymentApplicationRequest request, string userId, string? connectionName = null);
    }
}

