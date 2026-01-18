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
    /// AP (Accounts Payable) Invoice Service
    /// Manages vendor invoices, expense recognition, and payment tracking
    /// Uses: AP_INVOICE entity
    /// GL Posting: Debit Expense (6001), Credit AP (2000) on bill receipt
    ///            Debit AP (2000), Credit Cash (1000) on payment
    /// </summary>
    public class APInvoiceService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly JournalEntryService _journalEntryService;
        private readonly ILogger<APInvoiceService> _logger;
        private const string ConnectionName = "PPDM39";

        public APInvoiceService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            JournalEntryService journalEntryService,
            ILogger<APInvoiceService> logger)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _journalEntryService = journalEntryService ?? throw new ArgumentNullException(nameof(journalEntryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Create a new vendor bill (DRAFT status)
        /// </summary>
        public async Task<AP_INVOICE> CreateBillAsync(
            string vendorBaId,
            decimal invoiceAmount,
            DateTime invoiceDate,
            string? invoiceNumber = null,
            string? description = null,
            string userId = "SYSTEM")
        {
            if (string.IsNullOrWhiteSpace(vendorBaId))
                throw new ArgumentNullException(nameof(vendorBaId));
            if (invoiceAmount <= 0)
                throw new ArgumentException("Invoice amount must be greater than zero", nameof(invoiceAmount));

            _logger?.LogInformation("Creating AP invoice for vendor {VendorId}, amount {Amount:C}",
                vendorBaId, invoiceAmount);

            try
            {
                var bill = new AP_INVOICE
                {
                    AP_INVOICE_ID = Guid.NewGuid().ToString(),
                    INVOICE_NUMBER = invoiceNumber ?? await GenerateInvoiceNumberAsync(),
                    VENDOR_BA_ID = vendorBaId,
                    INVOICE_DATE = invoiceDate,
                    TOTAL_AMOUNT = invoiceAmount,
                    PAID_AMOUNT = 0m,
                    BALANCE_DUE = invoiceAmount,
                    STATUS = "DRAFT",
                    ACTIVE_IND = "Y",
                    PPDM_GUID = Guid.NewGuid().ToString(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow,
                    REMARK = description
                };

                var metadata = await _metadata.GetTableMetadataAsync("AP_INVOICE");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{metadata.EntityTypeName}")
                    ?? typeof(AP_INVOICE);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, ConnectionName, "AP_INVOICE");

                await repo.InsertAsync(bill, userId);
                _logger?.LogInformation("AP invoice {InvoiceNumber} created (DRAFT)", bill.INVOICE_NUMBER);
                return bill;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating bill: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Receive vendor bill (DRAFT -> RECEIVED)
        /// Posts GL entries: Debit Expense (6001), Credit AP (2000)
        /// </summary>
        public async Task<AP_INVOICE> ReceiveBillAsync(string billId, string userId = "SYSTEM")
        {
            if (string.IsNullOrWhiteSpace(billId))
                throw new ArgumentNullException(nameof(billId));

            _logger?.LogInformation("Receiving AP invoice {BillId}", billId);

            try
            {
                var bill = await GetBillByIdAsync(billId);
                if (bill == null)
                    throw new InvalidOperationException($"Bill {billId} not found");

                if (bill.STATUS != "DRAFT")
                    throw new InvalidOperationException($"Only DRAFT bills can be received (current: {bill.STATUS})");

                // Create GL journal entry: Debit Expense, Credit AP
                var lineItems = new List<JOURNAL_ENTRY_LINE>
                {
                    new JOURNAL_ENTRY_LINE
                    {
                        GL_ACCOUNT_ID = "6001", // Expense account (Lease Operating Costs)
                        DEBIT_AMOUNT = bill.TOTAL_AMOUNT,
                        CREDIT_AMOUNT = 0m,
                        DESCRIPTION = $"Bill {bill.INVOICE_NUMBER} - Vendor {bill.VENDOR_BA_ID}"
                    },
                    new JOURNAL_ENTRY_LINE
                    {
                        GL_ACCOUNT_ID = "2000", // AP account
                        DEBIT_AMOUNT = 0m,
                        CREDIT_AMOUNT = bill.TOTAL_AMOUNT,
                        DESCRIPTION = $"Liability for bill {bill.INVOICE_NUMBER}"
                    }
                };

                var glEntry = await _journalEntryService.CreateEntryAsync(
                    bill.INVOICE_DATE ?? DateTime.UtcNow,
                    $"Bill receipt: {bill.INVOICE_NUMBER}",
                    lineItems,
                    userId,
                    bill.INVOICE_NUMBER,
                    "AP");

                await _journalEntryService.PostEntryAsync(glEntry.JOURNAL_ENTRY_ID, userId);

                // Update bill status
                bill.STATUS = "RECEIVED";
                bill.ROW_CHANGED_BY = userId;
                bill.ROW_CHANGED_DATE = DateTime.UtcNow;

                var metadata = await _metadata.GetTableMetadataAsync("AP_INVOICE");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{metadata.EntityTypeName}")
                    ?? typeof(AP_INVOICE);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, ConnectionName, "AP_INVOICE");

                await repo.UpdateAsync(bill, userId);
                _logger?.LogInformation("AP invoice {InvoiceNumber} received and posted to GL", bill.INVOICE_NUMBER);
                return bill;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error receiving bill {BillId}: {Message}", billId, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Record a payment against a bill
        /// Posts GL entry: Debit AP (2000), Credit Cash (1000)
        /// </summary>
        public async Task<AP_INVOICE> RecordPaymentAsync(
            string billId,
            decimal paymentAmount,
            DateTime paymentDate,
            string userId = "SYSTEM")
        {
            if (string.IsNullOrWhiteSpace(billId))
                throw new ArgumentNullException(nameof(billId));
            if (paymentAmount <= 0)
                throw new ArgumentException("Payment amount must be greater than zero", nameof(paymentAmount));

            _logger?.LogInformation("Recording payment for bill {BillId}: {Amount:C}", billId, paymentAmount);

            try
            {
                var bill = await GetBillByIdAsync(billId);
                if (bill == null)
                    throw new InvalidOperationException($"Bill {billId} not found");

                if (bill.STATUS != "RECEIVED" && bill.STATUS != "PARTIALLY_PAID")
                    throw new InvalidOperationException($"Bill must be RECEIVED or PARTIALLY_PAID (current: {bill.STATUS})");

                decimal currentBalance = bill.BALANCE_DUE ?? 0m;
                if (paymentAmount > currentBalance)
                    throw new InvalidOperationException($"Payment {paymentAmount:C} exceeds balance {currentBalance:C}");

                // Create GL journal entry: Debit AP, Credit Cash
                var lineItems = new List<JOURNAL_ENTRY_LINE>
                {
                    new JOURNAL_ENTRY_LINE
                    {
                        GL_ACCOUNT_ID = "2000", // AP account
                        DEBIT_AMOUNT = paymentAmount,
                        CREDIT_AMOUNT = 0m,
                        DESCRIPTION = $"Payment for bill {bill.INVOICE_NUMBER}"
                    },
                    new JOURNAL_ENTRY_LINE
                    {
                        GL_ACCOUNT_ID = "1000", // Cash account
                        DEBIT_AMOUNT = 0m,
                        CREDIT_AMOUNT = paymentAmount,
                        DESCRIPTION = $"Bill {bill.INVOICE_NUMBER} payment"
                    }
                };

                var glEntry = await _journalEntryService.CreateEntryAsync(
                    paymentDate,
                    $"Payment for bill {bill.INVOICE_NUMBER}",
                    lineItems,
                    userId,
                    $"PAY-{bill.INVOICE_NUMBER}",
                    "AP");

                await _journalEntryService.PostEntryAsync(glEntry.JOURNAL_ENTRY_ID, userId);

                // Update bill balance and status
                decimal newBalance = currentBalance - paymentAmount;
                bill.PAID_AMOUNT = (bill.PAID_AMOUNT ?? 0m) + paymentAmount;
                bill.BALANCE_DUE = newBalance;

                if (newBalance <= 0.01m)
                {
                    bill.STATUS = "PAID";
                }
                else
                {
                    bill.STATUS = "PARTIALLY_PAID";
                }

                bill.ROW_CHANGED_BY = userId;
                bill.ROW_CHANGED_DATE = DateTime.UtcNow;

                var metadata = await _metadata.GetTableMetadataAsync("AP_INVOICE");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{metadata.EntityTypeName}")
                    ?? typeof(AP_INVOICE);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, ConnectionName, "AP_INVOICE");

                await repo.UpdateAsync(bill, userId);

                _logger?.LogInformation("Payment recorded for bill {InvoiceNumber}: {Amount:C}, New Status: {Status}",
                    bill.INVOICE_NUMBER, paymentAmount, bill.STATUS);

                return bill;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error recording payment for bill {BillId}: {Message}", billId, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get bill by ID
        /// </summary>
        public async Task<AP_INVOICE?> GetBillByIdAsync(string billId)
        {
            if (string.IsNullOrWhiteSpace(billId))
                return null;

            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("AP_INVOICE");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{metadata.EntityTypeName}")
                    ?? typeof(AP_INVOICE);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, ConnectionName, "AP_INVOICE");

                var bill = await repo.GetByIdAsync(billId);
                return bill as AP_INVOICE;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting bill {BillId}", billId);
                return null;
            }
        }

        /// <summary>
        /// Get all bills for a vendor
        /// </summary>
        public async Task<List<AP_INVOICE>> GetVendorBillsAsync(string vendorBaId)
        {
            if (string.IsNullOrWhiteSpace(vendorBaId))
                return new List<AP_INVOICE>();

            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("AP_INVOICE");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{metadata.EntityTypeName}")
                    ?? typeof(AP_INVOICE);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, ConnectionName, "AP_INVOICE");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "VENDOR_BA_ID", Operator = "=", FilterValue = vendorBaId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var bills = await repo.GetAsync(filters);
                return bills?.Cast<AP_INVOICE>().ToList() ?? new List<AP_INVOICE>();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting bills for vendor {VendorId}", vendorBaId);
                return new List<AP_INVOICE>();
            }
        }

        /// <summary>
        /// Get AP Aging Report
        /// Groups unpaid/partially paid bills by age categories
        /// </summary>
        public async Task<List<(string Category, decimal Total, List<AP_INVOICE> Bills)>> GetAPAgingAsync(DateTime? asOfDate = null)
        {
            asOfDate = asOfDate ?? DateTime.Today;
            _logger?.LogInformation("Generating AP aging report as of {AsOfDate}", asOfDate);

            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("AP_INVOICE");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{metadata.EntityTypeName}")
                    ?? typeof(AP_INVOICE);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, ConnectionName, "AP_INVOICE");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var bills = await repo.GetAsync(filters);
                var openBills = bills?
                    .Cast<AP_INVOICE>()
                    .Where(x => x.STATUS == "RECEIVED" || x.STATUS == "PARTIALLY_PAID")
                    .ToList() ?? new List<AP_INVOICE>();

                var current = new List<AP_INVOICE>();
                var days31_60 = new List<AP_INVOICE>();
                var days61_90 = new List<AP_INVOICE>();
                var days90Plus = new List<AP_INVOICE>();

                foreach (var bill in openBills)
                {
                    int daysOverdue = (int)(asOfDate.Value - (bill.DUE_DATE ?? bill.INVOICE_DATE ?? DateTime.Today)).TotalDays;

                    if (daysOverdue <= 0)
                        current.Add(bill);
                    else if (daysOverdue <= 30)
                        days31_60.Add(bill);
                    else if (daysOverdue <= 60)
                        days61_90.Add(bill);
                    else
                        days90Plus.Add(bill);
                }

                var result = new List<(string, decimal, List<AP_INVOICE>)>
                {
                    ("Current", current.Sum(x => x.BALANCE_DUE ?? 0m), current),
                    ("31-60 Days", days31_60.Sum(x => x.BALANCE_DUE ?? 0m), days31_60),
                    ("61-90 Days", days61_90.Sum(x => x.BALANCE_DUE ?? 0m), days61_90),
                    ("90+ Days", days90Plus.Sum(x => x.BALANCE_DUE ?? 0m), days90Plus)
                };

                _logger?.LogInformation("AP aging report generated. Total outstanding: {Total:C}",
                    result.Sum(x => x.Item2));

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating AP aging: {Message}", ex.Message);
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
                return $"BILL-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString().Substring(0, 8)}";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating invoice number");
                throw;
            }
        }
    }
}
