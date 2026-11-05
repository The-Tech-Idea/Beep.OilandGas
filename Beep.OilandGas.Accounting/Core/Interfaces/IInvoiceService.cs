using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
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
        Task<Beep.OilandGas.Models.Data.Accounting.SALES_INVOICE_INTENT?> GetCompanyInvoiceDraftIntentAsync(Beep.OilandGas.Models.Data.AccessControl.CompanyScope company,string actor,string intentId,Func<Task> requireAuthority);
        Task<Beep.OilandGas.Models.Data.Accounting.SALES_INVOICE_INTENT?> GetCompanyInvoiceDraftIntentForDeliveryAsync(Beep.OilandGas.Models.Data.AccessControl.CompanyScope company,string actor,string field,string ticket,Func<Task> requireAuthority);
        Task<Beep.OilandGas.Models.Data.Accounting.SALES_INVOICE_INTENT> SaveCompanyInvoiceDraftIntentAsync(Beep.OilandGas.Models.Data.Accounting.CompanySalesInvoiceDraftCommand command,Func<Task> requireAuthority,Func<Task<Beep.OilandGas.Models.Data.Accounting.CompanySalesInvoiceDraftReview>> readCurrent);
        Task<IReadOnlyList<INVOICE>> GetCompanySalesInvoicesForDeliveryAsync(Beep.OilandGas.Models.Data.AccessControl.CompanyScope company,string field,string ticket,Func<Task> requireAuthority);
        Task<IReadOnlyList<Beep.OilandGas.Models.Data.Accounting.CompanySalesInvoiceDraftOutcome>> GetCompanySalesInvoiceQueueAsync(Beep.OilandGas.Models.Data.AccessControl.CompanyScope company,string field,Func<Task> requireAuthority);
        Task<Beep.OilandGas.Models.Data.Accounting.CompanySalesInvoiceDraftOutcome?> GetCompanyInvoiceDraftOutcomeAsync(Beep.OilandGas.Models.Data.AccessControl.CompanyScope company,string actor,string intentId,Func<Task> requireAuthority);
        Task<Beep.OilandGas.Models.Data.Accounting.CompanySalesInvoiceDraftOutcome> GetCompanyInvoiceDraftForReviewAsync(Beep.OilandGas.Models.Data.AccessControl.CompanyScope company,string invoiceId,Func<Task> requireAuthority,Func<string,Task> requireFieldAccess);
        Task<Beep.OilandGas.Models.Data.Accounting.SALES_INVOICE_REVIEW_INTENT?> GetCompanyInvoiceReviewIntentAsync(Beep.OilandGas.Models.Data.AccessControl.CompanyScope company,string actor,string invoiceId,Func<Task> requireAuthority,Func<string,Task> requireFieldAccess);
        Task<Beep.OilandGas.Models.Data.Accounting.CompanySalesInvoiceDraftOutcome> RecordCompanyInvoiceDecisionAsync(Beep.OilandGas.Models.Data.Accounting.CompanySalesInvoiceDecisionCommand command,Func<Task> requireAuthority,Func<Task<Beep.OilandGas.Models.Data.Accounting.CompanySalesInvoiceReviewSource>> readCurrent);
        Task<Beep.OilandGas.Models.Data.Accounting.CompanySalesInvoiceDraftOutcome> ExecuteCompanyInvoiceDraftAsync(Beep.OilandGas.Models.Data.Accounting.CompanySalesInvoiceDraftCommand command,Func<Task> requireAuthority,Func<Task<Beep.OilandGas.Models.Data.Accounting.CompanySalesInvoiceDraftReview>> readCurrent);
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




