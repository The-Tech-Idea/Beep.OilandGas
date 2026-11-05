using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.Accounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for invoice operations.
    /// </summary>
    public interface IInvoiceService
    {
        Task<TheTechIdea.Data.OilGas.SALES_INVOICE_INTENT?> GetCompanyInvoiceDraftIntentAsync(TheTechIdea.Data.OilGas.CompanyScope company,string actor,string intentId,Func<Task> requireAuthority);
        Task<TheTechIdea.Data.OilGas.SALES_INVOICE_INTENT?> GetCompanyInvoiceDraftIntentForDeliveryAsync(TheTechIdea.Data.OilGas.CompanyScope company,string actor,string field,string ticket,Func<Task> requireAuthority);
        Task<TheTechIdea.Data.OilGas.SALES_INVOICE_INTENT> SaveCompanyInvoiceDraftIntentAsync(TheTechIdea.Data.OilGas.CompanySalesInvoiceDraftCommand command,Func<Task> requireAuthority,Func<Task<TheTechIdea.Data.OilGas.CompanySalesInvoiceDraftReview>> readCurrent);
        Task<IReadOnlyList<INVOICE>> GetCompanySalesInvoicesForDeliveryAsync(TheTechIdea.Data.OilGas.CompanyScope company,string field,string ticket,Func<Task> requireAuthority);
        Task<IReadOnlyList<TheTechIdea.Data.OilGas.CompanySalesInvoiceDraftOutcome>> GetCompanySalesInvoiceQueueAsync(TheTechIdea.Data.OilGas.CompanyScope company,string field,Func<Task> requireAuthority);
        Task<TheTechIdea.Data.OilGas.CompanySalesInvoiceDraftOutcome?> GetCompanyInvoiceDraftOutcomeAsync(TheTechIdea.Data.OilGas.CompanyScope company,string actor,string intentId,Func<Task> requireAuthority);
        Task<TheTechIdea.Data.OilGas.CompanySalesInvoiceDraftOutcome> GetCompanyInvoiceDraftForReviewAsync(TheTechIdea.Data.OilGas.CompanyScope company,string invoiceId,Func<Task> requireAuthority,Func<string,Task> requireFieldAccess);
        Task<TheTechIdea.Data.OilGas.SALES_INVOICE_REVIEW_INTENT?> GetCompanyInvoiceReviewIntentAsync(TheTechIdea.Data.OilGas.CompanyScope company,string actor,string invoiceId,Func<Task> requireAuthority,Func<string,Task> requireFieldAccess);
        Task<TheTechIdea.Data.OilGas.CompanySalesInvoiceDraftOutcome> RecordCompanyInvoiceDecisionAsync(TheTechIdea.Data.OilGas.CompanySalesInvoiceDecisionCommand command,Func<Task> requireAuthority,Func<Task<TheTechIdea.Data.OilGas.CompanySalesInvoiceReviewSource>> readCurrent);
        Task<TheTechIdea.Data.OilGas.CompanySalesInvoiceDraftOutcome> ExecuteCompanyInvoiceDraftAsync(TheTechIdea.Data.OilGas.CompanySalesInvoiceDraftCommand command,Func<Task> requireAuthority,Func<Task<TheTechIdea.Data.OilGas.CompanySalesInvoiceDraftReview>> readCurrent);
        /// <summary>
        /// Creates a new invoice.
        /// </summary>
        Task<INVOICE> CreateInvoiceAsync(CreateInvoiceRequest request, string userId, string cn = "PPDM39");
        
        /// <summary>
        /// Gets an invoice by ID.
        /// </summary>
        Task<INVOICE?> GetInvoiceAsync(string invoiceId, string cn = "PPDM39");
        
        /// <summary>
        /// Gets invoices by customer.
        /// </summary>
        Task<List<INVOICE>> GetInvoicesByCustomerAsync(string customerId, DateTime? startDate, DateTime? endDate, string cn = "PPDM39");
        
        /// <summary>
        /// Updates an invoice.
        /// </summary>
        Task<INVOICE> UpdateInvoiceAsync(UpdateInvoiceRequest request, string userId, string cn = "PPDM39");
        
        /// <summary>
        /// Deletes an invoice (soft delete by setting ACTIVE_IND = 'N').
        /// </summary>
        Task<bool> DeleteInvoiceAsync(string invoiceId, string userId, string cn = "PPDM39");
        
        /// <summary>
        /// Records a payment against an invoice.
        /// </summary>
        Task<INVOICE_PAYMENT> RecordPaymentAsync(CreateInvoicePaymentRequest request, string userId, string cn = "PPDM39");
        
        /// <summary>
        /// Gets all payments for an invoice.
        /// </summary>
        Task<List<INVOICE_PAYMENT>> GetInvoicePaymentsAsync(string invoiceId, string cn = "PPDM39");
        
        /// <summary>
        /// Gets invoice line items.
        /// </summary>
        Task<List<INVOICE_LINE_ITEM>> GetInvoiceLineItemsAsync(string invoiceId, string cn = "PPDM39");
        
        /// <summary>
        /// Approves an invoice.
        /// </summary>
        Task<InvoiceApprovalResult> ApproveInvoiceAsync(string invoiceId, string approverId, string cn = "PPDM39");
        
        /// <summary>
        /// Gets invoice aging summary.
        /// </summary>
        Task<List<InvoiceAgingSummary>> GetInvoiceAgingAsync(string? customerId, string cn = "PPDM39");
        
        /// <summary>
        /// Gets invoice payment status.
        /// </summary>
        Task<InvoicePaymentStatus> GetInvoicePaymentStatusAsync(string invoiceId, string cn = "PPDM39");
    }
}




