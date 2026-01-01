using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.DTOs.Accounting;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.DataBase;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.ProductionAccounting.Invoice
{
    /// <summary>
    /// Service for managing customer invoices.
    /// Uses IDataSource directly for database operations.
    /// </summary>
    public class InvoiceService : IInvoiceService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly ILogger<InvoiceService>? _logger;
        private readonly string _connectionName;

        private const string INVOICE_TABLE = "INVOICE";
        private const string INVOICE_LINE_ITEM_TABLE = "INVOICE_LINE_ITEM";
        private const string INVOICE_PAYMENT_TABLE = "INVOICE_PAYMENT";

        public InvoiceService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            ILoggerFactory? loggerFactory,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _logger = loggerFactory?.CreateLogger<InvoiceService>();
            _connectionName = connectionName ?? "PPDM39";
        }

        /// <summary>
        /// Creates a new invoice.
        /// </summary>
        public async Task<INVOICE> CreateInvoiceAsync(CreateInvoiceRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var invoice = new INVOICE
            {
                INVOICE_ID = Guid.NewGuid().ToString(),
                INVOICE_NUMBER = request.InvoiceNumber,
                CUSTOMER_BA_ID = request.CustomerBaId,
                INVOICE_DATE = request.InvoiceDate,
                DUE_DATE = request.DueDate,
                SUBTOTAL = request.Subtotal,
                TAX_AMOUNT = request.TaxAmount,
                TOTAL_AMOUNT = request.Subtotal + (request.TaxAmount ?? 0m),
                BALANCE_DUE = request.Subtotal + (request.TaxAmount ?? 0m),
                PAID_AMOUNT = 0m,
                STATUS = "Draft",
                CURRENCY_CODE = request.CurrencyCode,
                DESCRIPTION = request.Description,
                ACTIVE_IND = "Y"
            };

            if (invoice is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
            }

            var result = dataSource.InsertEntity(INVOICE_TABLE, invoice);
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to create invoice {InvoiceNumber}: {Error}", request.InvoiceNumber, errorMessage);
                throw new InvalidOperationException($"Failed to save invoice: {errorMessage}");
            }

            _logger?.LogDebug("Created invoice {InvoiceNumber}", request.InvoiceNumber);
            return invoice;
        }

        /// <summary>
        /// Gets an invoice by ID.
        /// </summary>
        public async Task<INVOICE?> GetInvoiceAsync(string invoiceId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(invoiceId))
                return null;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "INVOICE_ID", Operator = "=", FilterValue = invoiceId }
            };

            var results = await dataSource.GetEntityAsync(INVOICE_TABLE, filters);
            return results?.Cast<INVOICE>().FirstOrDefault();
        }

        /// <summary>
        /// Gets invoices by customer.
        /// </summary>
        public async Task<List<INVOICE>> GetInvoicesByCustomerAsync(string customerId, DateTime? startDate, DateTime? endDate, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(customerId))
                return new List<INVOICE>();

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "CUSTOMER_BA_ID", Operator = "=", FilterValue = customerId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (startDate.HasValue)
                filters.Add(new AppFilter { FieldName = "INVOICE_DATE", Operator = ">=", FilterValue = startDate.Value.ToString("yyyy-MM-dd") });
            if (endDate.HasValue)
                filters.Add(new AppFilter { FieldName = "INVOICE_DATE", Operator = "<=", FilterValue = endDate.Value.ToString("yyyy-MM-dd") });

            var results = await dataSource.GetEntityAsync(INVOICE_TABLE, filters);
            if (results == null)
                return new List<INVOICE>();

            return results.Cast<INVOICE>().OrderByDescending(i => i.INVOICE_DATE).ToList();
        }

        /// <summary>
        /// Updates an invoice.
        /// </summary>
        public async Task<INVOICE> UpdateInvoiceAsync(UpdateInvoiceRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.InvoiceId))
                throw new ArgumentException("Invoice ID is required.", nameof(request));

            var connName = connectionName ?? _connectionName;
            var invoice = await GetInvoiceAsync(request.InvoiceId, connName);

            if (invoice == null)
                throw new InvalidOperationException($"Invoice {request.InvoiceId} not found.");

            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            // Update properties
            invoice.INVOICE_NUMBER = request.InvoiceNumber;
            invoice.CUSTOMER_BA_ID = request.CustomerBaId;
            invoice.INVOICE_DATE = request.InvoiceDate;
            invoice.DUE_DATE = request.DueDate;
            invoice.SUBTOTAL = request.Subtotal;
            invoice.TAX_AMOUNT = request.TaxAmount;
            invoice.TOTAL_AMOUNT = (request.Subtotal ?? 0m) + (request.TaxAmount ?? 0m);
            invoice.CURRENCY_CODE = request.CurrencyCode;
            invoice.DESCRIPTION = request.Description;
            invoice.STATUS = request.Status;

            // Recalculate balance due
            invoice.BALANCE_DUE = invoice.TOTAL_AMOUNT - (invoice.PAID_AMOUNT ?? 0m);

            if (invoice is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForUpdate(ppdmEntity, userId);
            }

            var result = dataSource.UpdateEntity(INVOICE_TABLE, invoice);
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to update invoice {InvoiceId}: {Error}", request.InvoiceId, errorMessage);
                throw new InvalidOperationException($"Failed to update invoice: {errorMessage}");
            }

            _logger?.LogDebug("Updated invoice {InvoiceId}", request.InvoiceId);
            return invoice;
        }

        /// <summary>
        /// Deletes an invoice (soft delete by setting ACTIVE_IND = 'N').
        /// </summary>
        public async Task<bool> DeleteInvoiceAsync(string invoiceId, string userId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(invoiceId))
                return false;

            var connName = connectionName ?? _connectionName;
            var invoice = await GetInvoiceAsync(invoiceId, connName);

            if (invoice == null)
                return false;

            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            invoice.ACTIVE_IND = "N";

            if (invoice is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForUpdate(ppdmEntity, userId);
            }

            var result = dataSource.UpdateEntity(INVOICE_TABLE, invoice);
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                _logger?.LogError("Failed to delete invoice {InvoiceId}", invoiceId);
                return false;
            }

            _logger?.LogDebug("Deleted invoice {InvoiceId}", invoiceId);
            return true;
        }

        /// <summary>
        /// Records a payment against an invoice.
        /// </summary>
        public async Task<INVOICE_PAYMENT> RecordPaymentAsync(CreateInvoicePaymentRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.InvoiceId))
                throw new ArgumentException("Invoice ID is required.", nameof(request));

            var connName = connectionName ?? _connectionName;
            var invoice = await GetInvoiceAsync(request.InvoiceId, connName);

            if (invoice == null)
                throw new InvalidOperationException($"Invoice {request.InvoiceId} not found.");

            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var payment = new INVOICE_PAYMENT
            {
                INVOICE_PAYMENT_ID = Guid.NewGuid().ToString(),
                INVOICE_ID = request.InvoiceId,
                PAYMENT_DATE = request.PaymentDate,
                PAYMENT_AMOUNT = request.PaymentAmount,
                PAYMENT_METHOD = request.PaymentMethod,
                REFERENCE_NUMBER = request.ReferenceNumber,
                ACTIVE_IND = "Y"
            };

            if (payment is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
            }

            var insertResult = dataSource.InsertEntity(INVOICE_PAYMENT_TABLE, payment);
            if (insertResult != null && insertResult.Errors != null && insertResult.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", insertResult.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to record payment for invoice {InvoiceId}: {Error}", request.InvoiceId, errorMessage);
                throw new InvalidOperationException($"Failed to save payment: {errorMessage}");
            }

            // Update invoice payment amounts
            var payments = await GetInvoicePaymentsAsync(request.InvoiceId, connName);
            decimal totalPaid = payments.Sum(p => p.PAYMENT_AMOUNT ?? 0m);

            invoice.PAID_AMOUNT = totalPaid;
            invoice.BALANCE_DUE = (invoice.TOTAL_AMOUNT ?? 0m) - totalPaid;

            // Update invoice status
            if (invoice.BALANCE_DUE <= 0.01m)
                invoice.STATUS = "Paid";
            else if (totalPaid > 0)
                invoice.STATUS = "Partial";
            else
                invoice.STATUS = "Unpaid";

            if (invoice is IPPDMEntity invoicePpdmEntity)
            {
                _commonColumnHandler.PrepareForUpdate(invoicePpdmEntity, userId);
            }

            dataSource.UpdateEntity(INVOICE_TABLE, invoice);

            _logger?.LogDebug("Recorded payment {Amount} for invoice {InvoiceId}", request.PaymentAmount, request.InvoiceId);
            return payment;
        }

        /// <summary>
        /// Gets all payments for an invoice.
        /// </summary>
        public async Task<List<INVOICE_PAYMENT>> GetInvoicePaymentsAsync(string invoiceId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(invoiceId))
                return new List<INVOICE_PAYMENT>();

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "INVOICE_ID", Operator = "=", FilterValue = invoiceId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await dataSource.GetEntityAsync(INVOICE_PAYMENT_TABLE, filters);
            if (results == null)
                return new List<INVOICE_PAYMENT>();

            return results.Cast<INVOICE_PAYMENT>().OrderBy(p => p.PAYMENT_DATE).ToList();
        }

        /// <summary>
        /// Gets invoice line items.
        /// </summary>
        public async Task<List<INVOICE_LINE_ITEM>> GetInvoiceLineItemsAsync(string invoiceId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(invoiceId))
                return new List<INVOICE_LINE_ITEM>();

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "INVOICE_ID", Operator = "=", FilterValue = invoiceId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await dataSource.GetEntityAsync(INVOICE_LINE_ITEM_TABLE, filters);
            if (results == null)
                return new List<INVOICE_LINE_ITEM>();

            return results.Cast<INVOICE_LINE_ITEM>().OrderBy(l => l.LINE_NUMBER).ToList();
        }

        /// <summary>
        /// Approves an invoice.
        /// </summary>
        public async Task<InvoiceApprovalResult> ApproveInvoiceAsync(string invoiceId, string approverId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(invoiceId))
                throw new ArgumentException("Invoice ID is required.", nameof(invoiceId));

            var connName = connectionName ?? _connectionName;
            var invoice = await GetInvoiceAsync(invoiceId, connName);

            if (invoice == null)
                throw new InvalidOperationException($"Invoice {invoiceId} not found.");

            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            invoice.STATUS = "Approved";

            if (invoice is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForUpdate(ppdmEntity, approverId);
            }

            dataSource.UpdateEntity(INVOICE_TABLE, invoice);

            return new InvoiceApprovalResult
            {
                InvoiceId = invoiceId,
                IsApproved = true,
                ApproverId = approverId,
                ApprovalDate = DateTime.UtcNow,
                Status = "Approved"
            };
        }

        /// <summary>
        /// Gets invoice aging summary.
        /// </summary>
        public async Task<List<InvoiceAgingSummary>> GetInvoiceAgingAsync(string? customerId, string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
                new AppFilter { FieldName = "STATUS", Operator = "IN", FilterValue = "Unpaid,Partial" }
            };

            if (!string.IsNullOrEmpty(customerId))
            {
                filters.Add(new AppFilter { FieldName = "CUSTOMER_BA_ID", Operator = "=", FilterValue = customerId });
            }

            var results = await dataSource.GetEntityAsync(INVOICE_TABLE, filters);
            var invoiceList = results?.Cast<INVOICE>().ToList() ?? new List<INVOICE>();

            var aging = new List<InvoiceAgingSummary>();
            var today = DateTime.UtcNow.Date;

            foreach (var invoice in invoiceList)
            {
                if (invoice.BALANCE_DUE <= 0.01m)
                    continue;

                var dueDate = invoice.DUE_DATE ?? invoice.INVOICE_DATE ?? today;
                int daysPastDue = (today - dueDate).Days;
                string agingBucket = GetAgingBucket(daysPastDue);

                aging.Add(new InvoiceAgingSummary
                {
                    InvoiceId = invoice.INVOICE_ID ?? string.Empty,
                    InvoiceNumber = invoice.INVOICE_NUMBER ?? string.Empty,
                    CustomerBaId = invoice.CUSTOMER_BA_ID ?? string.Empty,
                    InvoiceDate = invoice.INVOICE_DATE,
                    DueDate = invoice.DUE_DATE,
                    TotalAmount = invoice.TOTAL_AMOUNT,
                    PaidAmount = invoice.PAID_AMOUNT,
                    BalanceDue = invoice.BALANCE_DUE,
                    DaysPastDue = daysPastDue,
                    AgingBucket = agingBucket
                });
            }

            return aging.OrderByDescending(a => a.DaysPastDue).ToList();
        }

        /// <summary>
        /// Gets invoice payment status.
        /// </summary>
        public async Task<InvoicePaymentStatus> GetInvoicePaymentStatusAsync(string invoiceId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(invoiceId))
                throw new ArgumentException("Invoice ID is required.", nameof(invoiceId));

            var connName = connectionName ?? _connectionName;
            var invoice = await GetInvoiceAsync(invoiceId, connName);

            if (invoice == null)
                throw new InvalidOperationException($"Invoice {invoiceId} not found.");

            var payments = await GetInvoicePaymentsAsync(invoiceId, connName);
            decimal totalPaid = payments.Sum(p => p.PAYMENT_AMOUNT ?? 0m);
            decimal balanceDue = (invoice.TOTAL_AMOUNT ?? 0m) - totalPaid;
            string paymentStatus = GetPaymentStatus(balanceDue, invoice.TOTAL_AMOUNT ?? 0m, totalPaid);

            var dueDate = invoice.DUE_DATE ?? invoice.INVOICE_DATE ?? DateTime.UtcNow;
            bool isOverdue = DateTime.UtcNow.Date > dueDate.Date && balanceDue > 0.01m;
            int daysPastDue = isOverdue ? (DateTime.UtcNow.Date - dueDate.Date).Days : 0;

            return new InvoicePaymentStatus
            {
                InvoiceId = invoiceId,
                InvoiceNumber = invoice.INVOICE_NUMBER ?? string.Empty,
                TotalAmount = invoice.TOTAL_AMOUNT,
                PaidAmount = invoice.PAID_AMOUNT,
                BalanceDue = balanceDue,
                PaymentStatus = paymentStatus,
                NumberOfPayments = payments.Count,
                LastPaymentDate = payments.OrderByDescending(p => p.PAYMENT_DATE).FirstOrDefault()?.PAYMENT_DATE,
                DueDate = invoice.DUE_DATE,
                IsOverdue = isOverdue,
                DaysPastDue = daysPastDue
            };
        }

        /// <summary>
        /// Gets aging bucket based on days past due.
        /// </summary>
        private string GetAgingBucket(int daysPastDue)
        {
            if (daysPastDue <= 0)
                return "Current";
            else if (daysPastDue <= 30)
                return "1-30";
            else if (daysPastDue <= 60)
                return "31-60";
            else if (daysPastDue <= 90)
                return "61-90";
            else
                return "90+";
        }

        /// <summary>
        /// Gets payment status based on amounts.
        /// </summary>
        private string GetPaymentStatus(decimal balanceDue, decimal totalAmount, decimal paidAmount)
        {
            if (balanceDue <= 0.01m)
                return "Paid";
            else if (paidAmount > 0 && balanceDue > 0.01m)
                return "Partial";
            else if (paidAmount == 0)
                return "Unpaid";
            else
                return "Overpaid";
        }
    }
}
