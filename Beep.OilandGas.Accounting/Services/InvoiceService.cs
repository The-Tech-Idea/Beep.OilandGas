using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.Accounting.Services
{
    /// <summary>
    /// Invoice service for revenue recognition, invoicing, and collections.
    /// Uses INVOICE/INVOICE_LINE_ITEM/INVOICE_PAYMENT entities.
    /// Usage: Use for general billing workflows tied to INVOICE tables.
    /// </summary>
    public class InvoiceService : IInvoiceService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<InvoiceService> _logger;
        private const string ConnectionName = "PPDM39";

        public InvoiceService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            AccountingBasisPostingService basisPosting,
            ILogger<InvoiceService> logger = null,
            IAccountMappingService? accountMapping = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _basisPosting = basisPosting ?? throw new ArgumentNullException(nameof(basisPosting));
            _logger = logger;
            _accountMapping = accountMapping;
        }

        public async Task<INVOICE> CreateInvoiceAsync(CreateInvoiceRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.CustomerBaId))
                throw new InvalidOperationException("Customer BA ID is required");
            if (request.Subtotal <= 0m)
                throw new InvalidOperationException("Invoice subtotal must be positive");
            if (request.DueDate.Date < request.InvoiceDate.Date)
                throw new InvalidOperationException("Due date cannot be earlier than invoice date");

            var invoice = new INVOICE
            {
                INVOICE_ID = Guid.NewGuid().ToString(),
                INVOICE_NUMBER = string.IsNullOrWhiteSpace(request.InvoiceNumber)
                    ? await GenerateInvoiceNumberAsync(connectionName)
                    : request.InvoiceNumber,
                CUSTOMER_BA_ID = request.CustomerBaId,
                INVOICE_DATE = request.InvoiceDate,
                DUE_DATE = request.DueDate,
                SUBTOTAL = request.Subtotal,
                TAX_AMOUNT = request.TaxAmount ?? 0m,
                TOTAL_AMOUNT = request.Subtotal + (request.TaxAmount ?? 0m),
                PAID_AMOUNT = 0m,
                BALANCE_DUE = request.Subtotal + (request.TaxAmount ?? 0m),
                CURRENCY_CODE = string.IsNullOrWhiteSpace(request.CurrencyCode) ? "USD" : request.CurrencyCode,
                DESCRIPTION = request.Description,
                STATUS = InvoiceStatuses.Draft,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<INVOICE>("INVOICE", connectionName);
            await repo.InsertAsync(invoice, userId);
            return invoice;
        }

        public async Task<INVOICE?> GetInvoiceAsync(string invoiceId, string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(invoiceId))
                throw new ArgumentNullException(nameof(invoiceId));

            var repo = await GetRepoAsync<INVOICE>("INVOICE", connectionName);
            var invoice = await repo.GetByIdAsync(invoiceId);
            return invoice as INVOICE;
        }

        public async Task<List<INVOICE>> GetInvoicesByCustomerAsync(string customerId, DateTime? startDate, DateTime? endDate, string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                throw new ArgumentNullException(nameof(customerId));

            var repo = await GetRepoAsync<INVOICE>("INVOICE", connectionName);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "CUSTOMER_BA_ID", Operator = "=", FilterValue = customerId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (startDate.HasValue)
                filters.Add(new AppFilter { FieldName = "INVOICE_DATE", Operator = ">=", FilterValue = startDate.Value.ToString("yyyy-MM-dd") });
            if (endDate.HasValue)
                filters.Add(new AppFilter { FieldName = "INVOICE_DATE", Operator = "<=", FilterValue = endDate.Value.ToString("yyyy-MM-dd") });

            var results = await repo.GetAsync(filters);
            return results?.Cast<INVOICE>().ToList() ?? new List<INVOICE>();
        }

        public async Task<INVOICE> UpdateInvoiceAsync(UpdateInvoiceRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.InvoiceId))
                throw new ArgumentNullException(nameof(request.InvoiceId));

            var invoice = await GetInvoiceAsync(request.InvoiceId, connectionName);
            if (invoice == null)
                throw new InvalidOperationException($"Invoice not found: {request.InvoiceId}");

            invoice.INVOICE_NUMBER = request.InvoiceNumber ?? invoice.INVOICE_NUMBER;
            invoice.CUSTOMER_BA_ID = request.CustomerBaId ?? invoice.CUSTOMER_BA_ID;
            invoice.INVOICE_DATE = request.InvoiceDate ?? invoice.INVOICE_DATE;
            var updatedDueDate = request.DueDate ?? invoice.DUE_DATE;
            var updatedInvoiceDate = request.InvoiceDate ?? invoice.INVOICE_DATE;
            if (updatedDueDate.HasValue && updatedInvoiceDate.HasValue
                && updatedDueDate.Value.Date < updatedInvoiceDate.Value.Date)
                throw new InvalidOperationException("Due date cannot be earlier than invoice date");
            invoice.DUE_DATE = updatedDueDate;
            invoice.SUBTOTAL = request.Subtotal ?? invoice.SUBTOTAL;
            invoice.TAX_AMOUNT = request.TaxAmount ?? invoice.TAX_AMOUNT;
            if (request.Subtotal.HasValue || request.TaxAmount.HasValue)
            {
                // Safely handle TOTAL_AMOUNT/TAX_AMOUNT/PAID_AMOUNT which may be nullable or non-nullable
                var subtotalVal = invoice.SUBTOTAL is decimal s ? s : 0m;
                var taxVal = invoice.TAX_AMOUNT is decimal t ? t : 0m;
                invoice.TOTAL_AMOUNT = subtotalVal + taxVal;
                var paid = invoice.PAID_AMOUNT is decimal p ? p : 0m;
                invoice.BALANCE_DUE = Math.Max(0m, (invoice.TOTAL_AMOUNT is decimal ta ? ta : 0m) - paid);
            }
            invoice.CURRENCY_CODE = request.CurrencyCode ?? invoice.CURRENCY_CODE;
            invoice.DESCRIPTION = request.Description ?? invoice.DESCRIPTION;
            invoice.STATUS = request.Status ?? invoice.STATUS;
            invoice.ROW_CHANGED_BY = userId;
            invoice.ROW_CHANGED_DATE = DateTime.UtcNow;

            var repo = await GetRepoAsync<INVOICE>("INVOICE", connectionName);
            await repo.UpdateAsync(invoice, userId);
            return invoice;
        }

        public async Task<bool> DeleteInvoiceAsync(string invoiceId, string userId, string? connectionName = null)
        {
            var invoice = await GetInvoiceAsync(invoiceId, connectionName);
            if (invoice == null)
                return false;

            invoice.ACTIVE_IND = _defaults.GetActiveIndicatorNo();
            invoice.ROW_CHANGED_BY = userId;
            invoice.ROW_CHANGED_DATE = DateTime.UtcNow;

            var repo = await GetRepoAsync<INVOICE>("INVOICE", connectionName);
            await repo.UpdateAsync(invoice, userId);
            return true;
        }

        public async Task<INVOICE_PAYMENT> RecordPaymentAsync(CreateInvoicePaymentRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.InvoiceId))
                throw new ArgumentNullException(nameof(request.InvoiceId));
            if (request.PaymentAmount <= 0m)
                throw new InvalidOperationException("Payment amount must be positive");

            var invoice = await GetInvoiceAsync(request.InvoiceId, connectionName);
            if (invoice == null)
                throw new InvalidOperationException($"Invoice not found: {request.InvoiceId}");
            var currentBalance = (invoice.TOTAL_AMOUNT is decimal ta2 ? ta2 : 0m) - (invoice.PAID_AMOUNT is decimal pa2 ? pa2 : 0m);
            if (request.PaymentAmount > currentBalance + 0.01m)
                throw new InvalidOperationException("Payment amount exceeds invoice balance");

            var payment = new INVOICE_PAYMENT
            {
                INVOICE_PAYMENT_ID = Guid.NewGuid().ToString(),
                INVOICE_ID = request.InvoiceId,
                PAYMENT_DATE = request.PaymentDate,
                PAYMENT_AMOUNT = request.PaymentAmount,
                PAYMENT_METHOD = request.PaymentMethod,
                REFERENCE_NUMBER = request.ReferenceNumber,
                DESCRIPTION = request.Description,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var paymentRepo = await GetRepoAsync<INVOICE_PAYMENT>("INVOICE_PAYMENT", connectionName);
            await paymentRepo.InsertAsync(payment, userId);

            invoice.PAID_AMOUNT = (invoice.PAID_AMOUNT is decimal paidSoFar ? paidSoFar : 0m) + request.PaymentAmount;
            invoice.BALANCE_DUE = Math.Max(0m, (invoice.TOTAL_AMOUNT is decimal totalAmt ? totalAmt : 0m) - (invoice.PAID_AMOUNT is decimal paidNow ? paidNow : 0m));
            invoice.STATUS = invoice.BALANCE_DUE == 0m ? InvoiceStatuses.Paid : InvoiceStatuses.PartiallyPaid;
            invoice.ROW_CHANGED_BY = userId;
            invoice.ROW_CHANGED_DATE = DateTime.UtcNow;

            var invoiceRepo = await GetRepoAsync<INVOICE>("INVOICE", connectionName);
            await invoiceRepo.UpdateAsync(invoice, userId);

            await _basisPosting.PostBalancedEntryByAccountAsync(
                GetAccountId(AccountMappingKeys.Cash, DefaultGlAccounts.Cash),
                GetAccountId(AccountMappingKeys.AccountsReceivable, DefaultGlAccounts.AccountsReceivable),
                request.PaymentAmount,
                $"Invoice payment {invoice.INVOICE_NUMBER}",
                userId,
                connectionName ?? ConnectionName);

            return payment;
        }

        public async Task<List<INVOICE_PAYMENT>> GetInvoicePaymentsAsync(string invoiceId, string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(invoiceId))
                throw new ArgumentNullException(nameof(invoiceId));

            var repo = await GetRepoAsync<INVOICE_PAYMENT>("INVOICE_PAYMENT", connectionName);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "INVOICE_ID", Operator = "=", FilterValue = invoiceId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results?.Cast<INVOICE_PAYMENT>().ToList() ?? new List<INVOICE_PAYMENT>();
        }

        public async Task<List<INVOICE_LINE_ITEM>> GetInvoiceLineItemsAsync(string invoiceId, string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(invoiceId))
                throw new ArgumentNullException(nameof(invoiceId));

            var repo = await GetRepoAsync<INVOICE_LINE_ITEM>("INVOICE_LINE_ITEM", connectionName);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "INVOICE_ID", Operator = "=", FilterValue = invoiceId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results?.Cast<INVOICE_LINE_ITEM>().ToList() ?? new List<INVOICE_LINE_ITEM>();
        }

        public async Task<InvoiceApprovalResult> ApproveInvoiceAsync(string invoiceId, string approverId, string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(invoiceId))
                throw new ArgumentNullException(nameof(invoiceId));
            if (string.IsNullOrWhiteSpace(approverId))
                throw new ArgumentNullException(nameof(approverId));

            var invoice = await GetInvoiceAsync(invoiceId, connectionName);
            if (invoice == null)
                throw new InvalidOperationException($"Invoice not found: {invoiceId}");

            if (!string.Equals(invoice.STATUS, InvoiceStatuses.Draft, StringComparison.OrdinalIgnoreCase))
            {
                return new InvoiceApprovalResult
                {
                    InvoiceId = invoice.INVOICE_ID,
                    IsApproved = false,
                    ApproverId = approverId,
                    Status = invoice.STATUS
                };
            }

            await _basisPosting.PostBalancedEntryByAccountAsync(
                GetAccountId(AccountMappingKeys.AccountsReceivable, DefaultGlAccounts.AccountsReceivable),
                GetAccountId(AccountMappingKeys.Revenue, DefaultGlAccounts.Revenue),
                (invoice.TOTAL_AMOUNT is decimal totalForPosting ? totalForPosting : 0m),
                $"Invoice approved {invoice.INVOICE_NUMBER}",
                approverId,
                connectionName ?? ConnectionName);

            invoice.STATUS = InvoiceStatuses.Issued;
            invoice.ROW_CHANGED_BY = approverId;
            invoice.ROW_CHANGED_DATE = DateTime.UtcNow;

            var repo = await GetRepoAsync<INVOICE>("INVOICE", connectionName);
            await repo.UpdateAsync(invoice, approverId);

            return new InvoiceApprovalResult
            {
                InvoiceId = invoice.INVOICE_ID,
                IsApproved = true,
                ApproverId = approverId,
                Status = invoice.STATUS
            };
        }

        public async Task<List<InvoiceAgingSummary>> GetInvoiceAgingAsync(string? customerId, string? connectionName = null)
        {
            var repo = await GetRepoAsync<INVOICE>("INVOICE", connectionName);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            if (!string.IsNullOrWhiteSpace(customerId))
                filters.Add(new AppFilter { FieldName = "CUSTOMER_BA_ID", Operator = "=", FilterValue = customerId });

            var results = await repo.GetAsync(filters);
            var invoices = results?.Cast<INVOICE>().ToList() ?? new List<INVOICE>();
            var today = DateTime.UtcNow.Date;

            return invoices.Select(invoice =>
            {
                var dueDate = invoice.DUE_DATE?.Date ?? invoice.INVOICE_DATE?.Date ?? today;
                var daysPastDue = Math.Max(0, (today - dueDate).Days);
                var bucket = daysPastDue switch
                {
                    <= 0 => "Current",
                    <= 30 => "1-30",
                    <= 60 => "31-60",
                    <= 90 => "61-90",
                    _ => "90+"
                };

                return new InvoiceAgingSummary
                {
                    InvoiceId = invoice.INVOICE_ID,
                    InvoiceNumber = invoice.INVOICE_NUMBER,
                    CustomerBaId = invoice.CUSTOMER_BA_ID,
                    InvoiceDate = invoice.INVOICE_DATE,
                    DueDate = invoice.DUE_DATE,
                    TotalAmount = invoice.TOTAL_AMOUNT,
                    PaidAmount = invoice.PAID_AMOUNT,
                    BalanceDue = invoice.BALANCE_DUE,
                    DaysPastDue = daysPastDue,
                    AgingBucket = bucket
                };
            }).ToList();
        }

        public async Task<InvoicePaymentStatus> GetInvoicePaymentStatusAsync(string invoiceId, string? connectionName = null)
        {
            var invoice = await GetInvoiceAsync(invoiceId, connectionName);
            if (invoice == null)
                throw new InvalidOperationException($"Invoice not found: {invoiceId}");

            var payments = await GetInvoicePaymentsAsync(invoiceId, connectionName);
            var totalPaid = payments.Sum(p => p.PAYMENT_AMOUNT is decimal pa3 ? pa3 : 0m);
            var totalAmount = invoice.TOTAL_AMOUNT is decimal ta3 ? ta3 : 0m;
            var balance = totalAmount - totalPaid;
            var paymentStatus = balance switch
            {
                < 0m => "OVERPAID",
                0m => InvoiceStatuses.Paid,
                _ when totalPaid > 0m => InvoiceStatuses.PartiallyPaid,
                _ => "UNPAID"
            };

            var dueDate = invoice.DUE_DATE ?? invoice.INVOICE_DATE;
            var daysPastDue = 0;
            var isOverdue = false;
            if (dueDate.HasValue && balance > 0m)
            {
                daysPastDue = Math.Max(0, (DateTime.UtcNow.Date - dueDate.Value.Date).Days);
                isOverdue = daysPastDue > 0;
            }

            return new InvoicePaymentStatus
            {
                InvoiceId = invoice.INVOICE_ID,
                InvoiceNumber = invoice.INVOICE_NUMBER,
                TotalAmount = totalAmount,
                PaidAmount = totalPaid,
                BalanceDue = balance,
                PaymentStatus = paymentStatus,
                NumberOfPayments = payments.Count,
                LastPaymentDate = payments.OrderByDescending(p => p.PAYMENT_DATE).FirstOrDefault()?.PAYMENT_DATE,
                DueDate = invoice.DUE_DATE,
                IsOverdue = isOverdue,
                DaysPastDue = daysPastDue
            };
        }

        private async Task<string> GenerateInvoiceNumberAsync(string? connectionName)
        {
            var repo = await GetRepoAsync<INVOICE>("INVOICE", connectionName);
            var existing = await repo.GetAsync(new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            });

            var count = existing?.Cast<INVOICE>().Count() ?? 0;
            return $"INV-{DateTime.UtcNow:yyyyMMdd}-{(count + 1):D4}";
        }

        private async Task<PPDMGenericRepository> GetRepoAsync<T>(string tableName, string? connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(T);

            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName ?? ConnectionName, tableName);
        }

        private string GetAccountId(string key, string fallback)
        {
            return _accountMapping?.GetAccountId(key) ?? fallback;
        }
    }
}



