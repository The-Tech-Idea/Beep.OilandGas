using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;

namespace Beep.OilandGas.Accounting.Services
{
    /// <summary>
    /// AP Payment Service
    /// Records payments against vendor bills
    /// GL Posting: Debit AP (2000), Credit Cash (1000)
    /// </summary>
    public class APPaymentService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<APPaymentService> _logger;
        private const string ConnectionName = "PPDM39";

        public APPaymentService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            AccountingBasisPostingService basisPosting,
            ILogger<APPaymentService> logger,
            IAccountMappingService? accountMapping = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _basisPosting = basisPosting ?? throw new ArgumentNullException(nameof(basisPosting));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accountMapping = accountMapping;
        }

        /// <summary>
        /// Record a payment against a vendor invoice
        /// Posts GL entry: Debit AP (2000), Credit Cash (1000)
        /// </summary>
        public async Task<AP_PAYMENT> RecordPaymentAsync(
            string apInvoiceId,
            decimal paymentAmount,
            DateTime paymentDate,
            string paymentMethod = "CASH",
            string? checkNumber = null,
            string? referenceNumber = null,
            string userId = "SYSTEM")
        {
            if (string.IsNullOrWhiteSpace(apInvoiceId))
                throw new ArgumentNullException(nameof(apInvoiceId));
            if (paymentAmount <= 0)
                throw new ArgumentException("Payment amount must be greater than zero", nameof(paymentAmount));

            _logger?.LogInformation("Recording AP payment for invoice {InvoiceId}: {Amount:C}",
                apInvoiceId, paymentAmount);

            try
            {
                // Create GL journal entry: Debit AP, Credit Cash
                var lineItems = new List<JOURNAL_ENTRY_LINE>
                {
                    new JOURNAL_ENTRY_LINE
                    {
                        GL_ACCOUNT_ID = GetAccountId(AccountMappingKeys.AccountsPayable, DefaultGlAccounts.AccountsPayable),
                        DEBIT_AMOUNT = paymentAmount,
                        CREDIT_AMOUNT = 0m,
                        DESCRIPTION = $"Vendor payment - Invoice {apInvoiceId}"
                    },
                    new JOURNAL_ENTRY_LINE
                    {
                        GL_ACCOUNT_ID = GetAccountId(AccountMappingKeys.Cash, DefaultGlAccounts.Cash),
                        DEBIT_AMOUNT = 0m,
                        CREDIT_AMOUNT = paymentAmount,
                        DESCRIPTION = $"Payment via {paymentMethod}"
                    }
                };

                await _basisPosting.PostEntryAsync(
                    paymentDate,
                    $"Vendor payment for invoice {apInvoiceId}",
                    lineItems,
                    userId,
                    referenceNumber ?? $"AP-PAY-{apInvoiceId}",
                    "AP");

                // Create AP_PAYMENT record
                var payment = new AP_PAYMENT
                {
                    AP_PAYMENT_ID = Guid.NewGuid().ToString(),
                    AP_INVOICE_ID = apInvoiceId,
                    PAYMENT_NUMBER = await GeneratePaymentNumberAsync(),
                    PAYMENT_DATE = paymentDate,
                    PAYMENT_AMOUNT = paymentAmount,
                    PAYMENT_METHOD = paymentMethod,
                    CHECK_NUMBER = checkNumber,
                    ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                    PPDM_GUID = Guid.NewGuid().ToString(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                var repo = await GetRepoAsync<AP_PAYMENT>("AP_PAYMENT");

                await repo.InsertAsync(payment, userId);

                _logger?.LogInformation("AP payment {PaymentNumber} recorded for invoice {InvoiceId}: {Amount:C}",
                    payment.PAYMENT_NUMBER, apInvoiceId, paymentAmount);

                return payment;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error recording AP payment for invoice {InvoiceId}: {Message}",
                    apInvoiceId, ex.Message);
                throw;
            }
        }

        private string GetAccountId(string key, string fallback)
        {
            return _accountMapping?.GetAccountId(key) ?? fallback;
        }

        /// <summary>
        /// Get all payments for a specific invoice
        /// </summary>
        public async Task<List<AP_PAYMENT>> GetInvoicePaymentsAsync(string apInvoiceId)
        {
            if (string.IsNullOrWhiteSpace(apInvoiceId))
                return new List<AP_PAYMENT>();

            try
            {
                var repo = await GetRepoAsync<AP_PAYMENT>("AP_PAYMENT");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "AP_INVOICE_ID", Operator = "=", FilterValue = apInvoiceId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var payments = await repo.GetAsync(filters);
                return payments?.Cast<AP_PAYMENT>().ToList() ?? new List<AP_PAYMENT>();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting payments for invoice {InvoiceId}", apInvoiceId);
                return new List<AP_PAYMENT>();
            }
        }

        /// <summary>
        /// Get total amount paid for an invoice
        /// </summary>
        public async Task<decimal> GetTotalPaidAsync(string apInvoiceId)
        {
            try
            {
                var payments = await GetInvoicePaymentsAsync(apInvoiceId);
                return payments.Sum(x => x.PAYMENT_AMOUNT ?? 0m);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error calculating total paid for invoice {InvoiceId}", apInvoiceId);
                throw;
            }
        }

        /// <summary>
        /// Get payment by ID
        /// </summary>
        public async Task<AP_PAYMENT?> GetPaymentByIdAsync(string paymentId)
        {
            if (string.IsNullOrWhiteSpace(paymentId))
                return null;

            try
            {
                var repo = await GetRepoAsync<AP_PAYMENT>("AP_PAYMENT");

                var payment = await repo.GetByIdAsync(paymentId);
                return payment as AP_PAYMENT;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting payment {PaymentId}", paymentId);
                return null;
            }
        }

        /// <summary>
        /// Get all payments within a date range
        /// </summary>
        public async Task<List<AP_PAYMENT>> GetPaymentsByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var repo = await GetRepoAsync<AP_PAYMENT>("AP_PAYMENT");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "PAYMENT_DATE", Operator = ">=", FilterValue = fromDate.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "PAYMENT_DATE", Operator = "<=", FilterValue = toDate.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var payments = await repo.GetAsync(filters);
                return payments?.Cast<AP_PAYMENT>().ToList() ?? new List<AP_PAYMENT>();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting payments for date range {FromDate} to {ToDate}",
                    fromDate, toDate);
                return new List<AP_PAYMENT>();
            }
        }

        /// <summary>
        /// Generate next payment number
        /// </summary>
        private async Task<string> GeneratePaymentNumberAsync()
        {
            try
            {
                return $"APPAY-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString().Substring(0, 8)}";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating payment number");
                throw;
            }
        }
        private async Task<PPDMGenericRepository> GetRepoAsync<T>(string tableName, string? connectionName = null)
        {
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            
            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(T), connectionName ?? ConnectionName, tableName);
        }
    }
}


