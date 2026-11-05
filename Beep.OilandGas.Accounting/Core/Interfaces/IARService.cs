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
        Task<TheTechIdea.Data.OilGas.CompanySalesReceivableOutcome> GetCompanySalesReceivableForReviewAsync(TheTechIdea.Data.OilGas.CompanyScope company,string receivableId,Func<Task> requireAuthority,Func<string,Task> requireFieldAccess);
        Task<TheTechIdea.Data.OilGas.SALES_RECEIVABLE_REVIEW_INTENT?> GetCompanySalesReceivableReviewIntentAsync(TheTechIdea.Data.OilGas.CompanyScope company,string actor,string receivableId,Func<Task> requireAuthority,Func<string,Task> requireFieldAccess);
        Task<TheTechIdea.Data.OilGas.CompanySalesReceivableOutcome> RecordCompanySalesReceivableDecisionAsync(TheTechIdea.Data.OilGas.CompanySalesReceivableDecisionCommand command,Func<Task> requireAuthority,Func<Task<TheTechIdea.Data.OilGas.CompanySalesReceivableReviewSource>> readCurrent);
        Task<IReadOnlyList<AR_INVOICE>> GetCompanySalesReceivablesAsync(TheTechIdea.Data.OilGas.CompanyScope company,string field,string invoiceId,Func<Task> requireAuthority);
        Task<TheTechIdea.Data.OilGas.SALES_RECEIVABLE_INTENT?> GetCompanySalesReceivableIntentAsync(TheTechIdea.Data.OilGas.CompanyScope company,string actor,string invoiceId,Func<Task> requireAuthority);
        Task<TheTechIdea.Data.OilGas.CompanySalesReceivableOutcome?> GetCompanySalesReceivableOutcomeAsync(TheTechIdea.Data.OilGas.CompanyScope company,string actor,string invoiceId,Func<Task> requireAuthority);
        Task<TheTechIdea.Data.OilGas.CompanySalesReceivableOutcome> PrepareCompanySalesReceivableAsync(TheTechIdea.Data.OilGas.CompanySalesReceivableCommand command,Func<Task> requireAuthority,Func<Task<TheTechIdea.Data.OilGas.CompanySalesReceivableSource>> readCurrent);
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




