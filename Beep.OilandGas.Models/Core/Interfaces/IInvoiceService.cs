using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.DTOs.Accounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for invoice operations.
    /// </summary>
    public interface IInvoiceService
    {
        /// <summary>
        /// Creates a new invoice.
        /// </summary>
        Task<INVOICE> CreateInvoiceAsync(CreateInvoiceRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets an invoice by ID.
        /// </summary>
        Task<INVOICE?> GetInvoiceAsync(string invoiceId, string? connectionName = null);
        
        /// <summary>
        /// Gets invoices by customer.
        /// </summary>
        Task<List<INVOICE>> GetInvoicesByCustomerAsync(string customerId, DateTime? startDate, DateTime? endDate, string? connectionName = null);
        
        /// <summary>
        /// Updates an invoice.
        /// </summary>
        Task<INVOICE> UpdateInvoiceAsync(UpdateInvoiceRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Deletes an invoice (soft delete by setting ACTIVE_IND = 'N').
        /// </summary>
        Task<bool> DeleteInvoiceAsync(string invoiceId, string userId, string? connectionName = null);
        
        /// <summary>
        /// Records a payment against an invoice.
        /// </summary>
        Task<INVOICE_PAYMENT> RecordPaymentAsync(CreateInvoicePaymentRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets all payments for an invoice.
        /// </summary>
        Task<List<INVOICE_PAYMENT>> GetInvoicePaymentsAsync(string invoiceId, string? connectionName = null);
        
        /// <summary>
        /// Gets invoice line items.
        /// </summary>
        Task<List<INVOICE_LINE_ITEM>> GetInvoiceLineItemsAsync(string invoiceId, string? connectionName = null);
        
        /// <summary>
        /// Approves an invoice.
        /// </summary>
        Task<InvoiceApprovalResult> ApproveInvoiceAsync(string invoiceId, string approverId, string? connectionName = null);
        
        /// <summary>
        /// Gets invoice aging summary.
        /// </summary>
        Task<List<InvoiceAgingSummary>> GetInvoiceAgingAsync(string? customerId, string? connectionName = null);
        
        /// <summary>
        /// Gets invoice payment status.
        /// </summary>
        Task<InvoicePaymentStatus> GetInvoicePaymentStatusAsync(string invoiceId, string? connectionName = null);
    }
}




