using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;

namespace Beep.OilandGas.Accounting.Services
{
    /// <summary>
    /// AR (Accounts Receivable) Invoice Service
    /// Manages customer invoicing, revenue recognition, and payment tracking
    /// Uses: AR_INVOICE, AR_PAYMENT entities
    /// GL Posting: Debit AR (1110), Credit Revenue (4001) on invoice issue
    ///            Debit Cash (1000), Credit AR (1110) on payment receipt
    /// </summary>
    public class ARInvoiceService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly JournalEntryService _journalEntryService;
        private readonly ILogger<ARInvoiceService> _logger;
        private const string ConnectionName = "PPDM39";

        public ARInvoiceService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            JournalEntryService journalEntryService,
            ILogger<ARInvoiceService> logger)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _journalEntryService = journalEntryService ?? throw new ArgumentNullException(nameof(journalEntryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Create a new customer invoice (DRAFT status)
        /// Does NOT post to GL yet - only creates invoice record
        /// </summary>
        public async Task<AR_INVOICE> CreateInvoiceAsync(
            string customerBaId,
            decimal totalAmount,
            DateTime invoiceDate,
            string? invoiceNumber = null,
            string? description = null,
            string? terms = null,
            string userId = "SYSTEM")
        {
            if (string.IsNullOrWhiteSpace(customerBaId))
                throw new ArgumentNullException(nameof(customerBaId));
            if (totalAmount <= 0)
                throw new ArgumentException("Invoice amount must be greater than zero", nameof(totalAmount));

            _logger?.LogInformation("Creating AR invoice for customer {CustomerId}, amount {Amount:C}",
                customerBaId, totalAmount);

            try
            {
                var invoice = new AR_INVOICE
                {
                    AR_INVOICE_ID = Guid.NewGuid().ToString(),
                    INVOICE_NUMBER = invoiceNumber ?? await GenerateInvoiceNumberAsync(),
                    CUSTOMER_BA_ID = customerBaId,
                    INVOICE_DATE = invoiceDate,
                    TOTAL_AMOUNT = totalAmount,
                    PAID_AMOUNT = 0m,
                    BALANCE_DUE = totalAmount,
                    STATUS = "DRAFT",
                    ACTIVE_IND = "Y",
                    PPDM_GUID = Guid.NewGuid().ToString(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow,
                    REMARK = description
                };

                var metadata = await _metadata.GetTableMetadataAsync("AR_INVOICE");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{metadata.EntityTypeName}")
                    ?? typeof(AR_INVOICE);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, ConnectionName, "AR_INVOICE");

                await repo.InsertAsync(invoice, userId);
                _logger?.LogInformation("AR invoice {InvoiceNumber} created (DRAFT)", invoice.INVOICE_NUMBER);
                return invoice;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating invoice: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Issue an invoice (DRAFT -> ISSUED)
        /// Posts GL entries: Debit AR (1110), Credit Revenue (4001)
        /// </summary>
        public async Task<AR_INVOICE> IssueInvoiceAsync(string invoiceId, string userId = "SYSTEM")
        {
            if (string.IsNullOrWhiteSpace(invoiceId))
                throw new ArgumentNullException(nameof(invoiceId));

            _logger?.LogInformation("Issuing AR invoice {InvoiceId}", invoiceId);

            try
            {
                var invoice = await GetInvoiceByIdAsync(invoiceId);
                if (invoice == null)
                    throw new InvalidOperationException($"Invoice {invoiceId} not found");

                if (invoice.STATUS != "DRAFT")
                    throw new InvalidOperationException($"Only DRAFT invoices can be issued (current: {invoice.STATUS})");

                // Create GL journal entry: Debit AR, Credit Revenue
                var lineItems = new List<JOURNAL_ENTRY_LINE>
                {
                    new JOURNAL_ENTRY_LINE
                    {
                        GL_ACCOUNT_ID = "1110", // AR account
                        DEBIT_AMOUNT = invoice.TOTAL_AMOUNT,
                        CREDIT_AMOUNT = 0m,
                        DESCRIPTION = $"Invoice {invoice.INVOICE_NUMBER} - Customer {invoice.CUSTOMER_BA_ID}"
                    },
                    new JOURNAL_ENTRY_LINE
                    {
                        GL_ACCOUNT_ID = "4001", // Revenue account
                        DEBIT_AMOUNT = 0m,
                        CREDIT_AMOUNT = invoice.TOTAL_AMOUNT,
                        DESCRIPTION = $"Revenue from invoice {invoice.INVOICE_NUMBER}"
                    }
                };

                var glEntry = await _journalEntryService.CreateEntryAsync(
                    invoice.INVOICE_DATE ?? DateTime.UtcNow,
                    $"Invoice issue: {invoice.INVOICE_NUMBER}",
                    lineItems,
                    userId,
                    invoice.INVOICE_NUMBER,
                    "AR");

                await _journalEntryService.PostEntryAsync(glEntry.JOURNAL_ENTRY_ID, userId);

                // Update invoice status
                invoice.STATUS = "ISSUED";
                invoice.ROW_CHANGED_BY = userId;
                invoice.ROW_CHANGED_DATE = DateTime.UtcNow;

                var metadata = await _metadata.GetTableMetadataAsync("AR_INVOICE");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{metadata.EntityTypeName}")
                    ?? typeof(AR_INVOICE);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, ConnectionName, "AR_INVOICE");

                await repo.UpdateAsync(invoice, userId);
                _logger?.LogInformation("AR invoice {InvoiceNumber} issued and posted to GL", invoice.INVOICE_NUMBER);
                return invoice;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error issuing invoice {InvoiceId}: {Message}", invoiceId, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Record a payment against an invoice
        /// Posts GL entry: Debit Cash (1000), Credit AR (1110)
        /// Updates invoice balance and status
        /// </summary>
        public async Task<AR_PAYMENT> RecordPaymentAsync(
            string invoiceId,
            decimal paymentAmount,
            DateTime paymentDate,
            string paymentMethod = "CASH",
            string? checkNumber = null,
            string userId = "SYSTEM")
        {
            if (string.IsNullOrWhiteSpace(invoiceId))
                throw new ArgumentNullException(nameof(invoiceId));
            if (paymentAmount <= 0)
                throw new ArgumentException("Payment amount must be greater than zero", nameof(paymentAmount));

            _logger?.LogInformation("Recording payment for invoice {InvoiceId}: {Amount:C}", invoiceId, paymentAmount);

            try
            {
                var invoice = await GetInvoiceByIdAsync(invoiceId);
                if (invoice == null)
                    throw new InvalidOperationException($"Invoice {invoiceId} not found");

                if (invoice.STATUS != "ISSUED" && invoice.STATUS != "PARTIALLY_PAID")
                    throw new InvalidOperationException($"Invoice must be ISSUED or PARTIALLY_PAID to record payment (current: {invoice.STATUS})");

                decimal currentBalance = invoice.BALANCE_DUE ?? 0m;
                if (paymentAmount > currentBalance)
                    throw new InvalidOperationException($"Payment amount {paymentAmount:C} exceeds invoice balance {currentBalance:C}");

                // Create GL journal entry: Debit Cash, Credit AR
                var lineItems = new List<JOURNAL_ENTRY_LINE>
                {
                    new JOURNAL_ENTRY_LINE
                    {
                        GL_ACCOUNT_ID = "1000", // Cash account
                        DEBIT_AMOUNT = paymentAmount,
                        CREDIT_AMOUNT = 0m,
                        DESCRIPTION = $"Payment received for invoice {invoice.INVOICE_NUMBER}"
                    },
                    new JOURNAL_ENTRY_LINE
                    {
                        GL_ACCOUNT_ID = "1110", // AR account
                        DEBIT_AMOUNT = 0m,
                        CREDIT_AMOUNT = paymentAmount,
                        DESCRIPTION = $"Invoice {invoice.INVOICE_NUMBER} payment - {paymentMethod}"
                    }
                };

                var glEntry = await _journalEntryService.CreateEntryAsync(
                    paymentDate,
                    $"Payment for invoice {invoice.INVOICE_NUMBER}",
                    lineItems,
                    userId,
                    $"PAY-{invoice.INVOICE_NUMBER}",
                    "AR");

                await _journalEntryService.PostEntryAsync(glEntry.JOURNAL_ENTRY_ID, userId);

                // Create AR_PAYMENT record
                var payment = new AR_PAYMENT
                {
                    AR_PAYMENT_ID = Guid.NewGuid().ToString(),
                    AR_INVOICE_ID = invoiceId,
                    PAYMENT_NUMBER = await GeneratePaymentNumberAsync(),
                    PAYMENT_DATE = paymentDate,
                    PAYMENT_AMOUNT = paymentAmount,
                    PAYMENT_METHOD = paymentMethod,
                    CHECK_NUMBER = checkNumber,
                    ACTIVE_IND = "Y",
                    PPDM_GUID = Guid.NewGuid().ToString(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                var paymentMetadata = await _metadata.GetTableMetadataAsync("AR_PAYMENT");
                var paymentEntityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{paymentMetadata.EntityTypeName}")
                    ?? typeof(AR_PAYMENT);

                var paymentRepo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    paymentEntityType, ConnectionName, "AR_PAYMENT");

                await paymentRepo.InsertAsync(payment, userId);

                // Update invoice balance and status
                decimal newBalance = currentBalance - paymentAmount;
                invoice.PAID_AMOUNT = (invoice.PAID_AMOUNT ?? 0m) + paymentAmount;
                invoice.BALANCE_DUE = newBalance;

                if (newBalance <= 0.01m) // Consider paid if balance is near zero (rounding)
                {
                    invoice.STATUS = "PAID";
                }
                else
                {
                    invoice.STATUS = "PARTIALLY_PAID";
                }

                invoice.ROW_CHANGED_BY = userId;
                invoice.ROW_CHANGED_DATE = DateTime.UtcNow;

                var invoiceMetadata = await _metadata.GetTableMetadataAsync("AR_INVOICE");
                var invoiceEntityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{invoiceMetadata.EntityTypeName}")
                    ?? typeof(AR_INVOICE);

                var invoiceRepo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    invoiceEntityType, ConnectionName, "AR_INVOICE");

                await invoiceRepo.UpdateAsync(invoice, userId);

                _logger?.LogInformation("Payment recorded for invoice {InvoiceNumber}: {Amount:C}, New Status: {Status}",
                    invoice.INVOICE_NUMBER, paymentAmount, invoice.STATUS);

                return payment;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error recording payment for invoice {InvoiceId}: {Message}", invoiceId, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get invoice by ID
        /// </summary>
        public async Task<AR_INVOICE?> GetInvoiceByIdAsync(string invoiceId)
        {
            if (string.IsNullOrWhiteSpace(invoiceId))
                return null;

            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("AR_INVOICE");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{metadata.EntityTypeName}")
                    ?? typeof(AR_INVOICE);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, ConnectionName, "AR_INVOICE");

                var invoice = await repo.GetByIdAsync(invoiceId);
                return invoice as AR_INVOICE;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting invoice {InvoiceId}", invoiceId);
                return null;
            }
        }

        /// <summary>
        /// Get all invoices for a customer
        /// </summary>
        public async Task<List<AR_INVOICE>> GetCustomerInvoicesAsync(string customerBaId)
        {
            if (string.IsNullOrWhiteSpace(customerBaId))
                return new List<AR_INVOICE>();

            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("AR_INVOICE");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{metadata.EntityTypeName}")
                    ?? typeof(AR_INVOICE);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, ConnectionName, "AR_INVOICE");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "CUSTOMER_BA_ID", Operator = "=", FilterValue = customerBaId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var invoices = await repo.GetAsync(filters);
                return invoices?.Cast<AR_INVOICE>().ToList() ?? new List<AR_INVOICE>();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting invoices for customer {CustomerId}", customerBaId);
                return new List<AR_INVOICE>();
            }
        }

        /// <summary>
        /// Get AR Aging Report
        /// Groups unpaid/partially paid invoices by age categories
        /// </summary>
        public async Task<List<(string Category, decimal Total, List<AR_INVOICE> Invoices)>> GetARAgingAsync(DateTime? asOfDate = null)
        {
            asOfDate = asOfDate ?? DateTime.Today;
            _logger?.LogInformation("Generating AR aging report as of {AsOfDate}", asOfDate);

            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("AR_INVOICE");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{metadata.EntityTypeName}")
                    ?? typeof(AR_INVOICE);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, ConnectionName, "AR_INVOICE");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var invoices = await repo.GetAsync(filters);
                var openInvoices = invoices?
                    .Cast<AR_INVOICE>()
                    .Where(x => x.STATUS == "ISSUED" || x.STATUS == "PARTIALLY_PAID")
                    .ToList() ?? new List<AR_INVOICE>();

                var current = new List<AR_INVOICE>();
                var days31_60 = new List<AR_INVOICE>();
                var days61_90 = new List<AR_INVOICE>();
                var days90Plus = new List<AR_INVOICE>();

                foreach (var invoice in openInvoices)
                {
                    int daysOverdue = (int)(asOfDate.Value - (invoice.DUE_DATE ?? invoice.INVOICE_DATE ?? DateTime.Today)).TotalDays;

                    if (daysOverdue <= 0)
                        current.Add(invoice);
                    else if (daysOverdue <= 30)
                        days31_60.Add(invoice);
                    else if (daysOverdue <= 60)
                        days61_90.Add(invoice);
                    else
                        days90Plus.Add(invoice);
                }

                var result = new List<(string, decimal, List<AR_INVOICE>)>
                {
                    ("Current", current.Sum(x => x.BALANCE_DUE ?? 0m), current),
                    ("31-60 Days", days31_60.Sum(x => x.BALANCE_DUE ?? 0m), days31_60),
                    ("61-90 Days", days61_90.Sum(x => x.BALANCE_DUE ?? 0m), days61_90),
                    ("90+ Days", days90Plus.Sum(x => x.BALANCE_DUE ?? 0m), days90Plus)
                };

                _logger?.LogInformation("AR aging report generated. Total outstanding: {Total:C}",
                    result.Sum(x => x.Item2));

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating AR aging: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Generate next invoice number
        /// </summary>
        private async Task<string> GenerateInvoiceNumberAsync()
        {
            try
            {
                return $"INV-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString().Substring(0, 8)}";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating invoice number");
                throw;
            }
        }

        /// <summary>
        /// Generate next payment number
        /// </summary>
        private async Task<string> GeneratePaymentNumberAsync()
        {
            try
            {
                return $"PAY-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString().Substring(0, 8)}";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating payment number");
                throw;
            }
        }
    }
}
