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
    /// Service interface for accounts receivable operations.
    /// </summary>
    public interface IARService
    {
        Task<Beep.OilandGas.Models.Data.Accounting.CompanySalesReceivableOutcome> GetCompanySalesReceivableForReviewAsync(Beep.OilandGas.Models.Data.AccessControl.CompanyScope company,string receivableId,Func<Task> requireAuthority,Func<string,Task> requireFieldAccess);
        Task<Beep.OilandGas.Models.Data.Accounting.SALES_RECEIVABLE_REVIEW_INTENT?> GetCompanySalesReceivableReviewIntentAsync(Beep.OilandGas.Models.Data.AccessControl.CompanyScope company,string actor,string receivableId,Func<Task> requireAuthority,Func<string,Task> requireFieldAccess);
        Task<Beep.OilandGas.Models.Data.Accounting.CompanySalesReceivableOutcome> RecordCompanySalesReceivableDecisionAsync(Beep.OilandGas.Models.Data.Accounting.CompanySalesReceivableDecisionCommand command,Func<Task> requireAuthority,Func<Task<Beep.OilandGas.Models.Data.Accounting.CompanySalesReceivableReviewSource>> readCurrent);
        Task<IReadOnlyList<AR_INVOICE>> GetCompanySalesReceivablesAsync(Beep.OilandGas.Models.Data.AccessControl.CompanyScope company,string field,string invoiceId,Func<Task> requireAuthority);
        Task<Beep.OilandGas.Models.Data.Accounting.SALES_RECEIVABLE_INTENT?> GetCompanySalesReceivableIntentAsync(Beep.OilandGas.Models.Data.AccessControl.CompanyScope company,string actor,string invoiceId,Func<Task> requireAuthority);
        Task<Beep.OilandGas.Models.Data.Accounting.CompanySalesReceivableOutcome?> GetCompanySalesReceivableOutcomeAsync(Beep.OilandGas.Models.Data.AccessControl.CompanyScope company,string actor,string invoiceId,Func<Task> requireAuthority);
        Task<Beep.OilandGas.Models.Data.Accounting.CompanySalesReceivableOutcome> PrepareCompanySalesReceivableAsync(Beep.OilandGas.Models.Data.Accounting.CompanySalesReceivableCommand command,Func<Task> requireAuthority,Func<Task<Beep.OilandGas.Models.Data.Accounting.CompanySalesReceivableSource>> readCurrent);
        /// <summary>
        /// Creates a new AR invoice.
        /// </summary>
        Task<AR_INVOICE> CreateInvoiceAsync(CreateARInvoiceRequest request, string userId, string cn = "PPDM39");
        
        /// <summary>
        /// Gets an AR invoice by ID.
        /// </summary>
        Task<AR_INVOICE?> GetInvoiceAsync(string invoiceId, string cn = "PPDM39");
        
        /// <summary>
        /// Gets invoices by customer.
        /// </summary>
        Task<List<AR_INVOICE>> GetInvoicesByCustomerAsync(string customerId, DateTime? startDate, DateTime? endDate, string cn = "PPDM39");
        
        /// <summary>
        /// Updates an AR invoice.
        /// </summary>
        Task<AR_INVOICE> UpdateInvoiceAsync(UpdateARInvoiceRequest request, string userId, string cn = "PPDM39");
        
        /// <summary>
        /// Deletes an AR invoice (soft delete by setting ACTIVE_IND = 'N').
        /// </summary>
        Task<bool> DeleteInvoiceAsync(string invoiceId, string userId, string cn = "PPDM39");
        
        /// <summary>
        /// Creates a payment from a customer.
        /// </summary>
        Task<AR_PAYMENT> CreatePaymentAsync(CreateARPaymentRequest request, string userId, string cn = "PPDM39");
        
        /// <summary>
        /// Applies a payment to an invoice.
        /// </summary>
        Task<AR_PAYMENT> ApplyPaymentAsync(string paymentId, string invoiceId, decimal amount, string userId, string cn = "PPDM39");
        
        /// <summary>
        /// Gets all payments for an AR invoice.
        /// </summary>
        Task<List<AR_PAYMENT>> GetPaymentsByInvoiceAsync(string invoiceId, string cn = "PPDM39");
        
        /// <summary>
        /// Creates a credit memo.
        /// </summary>
        Task<AR_CREDIT_MEMO> CreateCreditMemoAsync(CreateARCreditMemoRequest request, string userId, string cn = "PPDM39");
        
        /// <summary>
        /// Approves an AR invoice.
        /// </summary>
        Task<ARApprovalResult> ApproveInvoiceAsync(string invoiceId, string approverId, string cn = "PPDM39");
        
        /// <summary>
        /// Gets AR aging summary.
        /// </summary>
        Task<List<ARAgingSummary>> GetARAgingAsync(string? customerId, string cn = "PPDM39");
        
        /// <summary>
        /// Gets customer summary.
        /// </summary>
        Task<CustomerSummary> GetCustomerSummaryAsync(string customerId, string cn = "PPDM39");
        
        /// <summary>
        /// Applies a payment to multiple invoices.
        /// </summary>
        Task<PaymentApplicationResult> ApplyPaymentToInvoicesAsync(PaymentApplicationRequest request, string userId, string cn = "PPDM39");
    }
}




