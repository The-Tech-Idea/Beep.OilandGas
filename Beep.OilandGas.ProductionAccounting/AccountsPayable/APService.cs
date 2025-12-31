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

namespace Beep.OilandGas.ProductionAccounting.AccountsPayable
{
    /// <summary>
    /// Service for managing accounts payable invoices.
    /// Uses IDataSource directly for database operations.
    /// </summary>
    public class APService : IAPService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly ILogger<APService>? _logger;
        private readonly string _connectionName;

        private const string AP_INVOICE_TABLE = "AP_INVOICE";
        private const string AP_PAYMENT_TABLE = "AP_PAYMENT";
        private const string AP_CREDIT_MEMO_TABLE = "AP_CREDIT_MEMO";

        public APService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            ILoggerFactory? loggerFactory,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _logger = loggerFactory?.CreateLogger<APService>();
            _connectionName = connectionName ?? "PPDM39";
        }

        /// <summary>
        /// Creates a new AP invoice.
        /// </summary>
        public async Task<AP_INVOICE> CreateInvoiceAsync(CreateAPInvoiceRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var invoice = new AP_INVOICE
            {
                AP_INVOICE_ID = Guid.NewGuid().ToString(),
                INVOICE_NUMBER = request.InvoiceNumber,
                VENDOR_BA_ID = request.VendorBaId,
                INVOICE_DATE = request.InvoiceDate,
                DUE_DATE = request.DueDate,
                TOTAL_AMOUNT = request.TotalAmount,
                BALANCE_DUE = request.TotalAmount,
                PAID_AMOUNT = 0m,
                STATUS = "Open",
                ACTIVE_IND = "Y"
            };

            if (invoice is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
            }

            var result = dataSource.InsertEntity(AP_INVOICE_TABLE, invoice);
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to create AP invoice {InvoiceNumber}: {Error}", request.InvoiceNumber, errorMessage);
                throw new InvalidOperationException($"Failed to save AP invoice: {errorMessage}");
            }

            _logger?.LogDebug("Created AP invoice {InvoiceNumber}", request.InvoiceNumber);
            return invoice;
        }

        /// <summary>
        /// Gets an AP invoice by ID.
        /// </summary>
        public async Task<AP_INVOICE?> GetInvoiceAsync(string invoiceId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(invoiceId))
                return null;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "AP_INVOICE_ID", Operator = "=", FilterValue = invoiceId }
            };

            var results = await dataSource.GetEntityAsync(AP_INVOICE_TABLE, filters);
            return results?.Cast<AP_INVOICE>().FirstOrDefault();
        }

        /// <summary>
        /// Gets invoices by vendor.
        /// </summary>
        public async Task<List<AP_INVOICE>> GetInvoicesByVendorAsync(string vendorId, DateTime? startDate, DateTime? endDate, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(vendorId))
                return new List<AP_INVOICE>();

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "VENDOR_BA_ID", Operator = "=", FilterValue = vendorId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (startDate.HasValue)
                filters.Add(new AppFilter { FieldName = "INVOICE_DATE", Operator = ">=", FilterValue = startDate.Value.ToString("yyyy-MM-dd") });
            if (endDate.HasValue)
                filters.Add(new AppFilter { FieldName = "INVOICE_DATE", Operator = "<=", FilterValue = endDate.Value.ToString("yyyy-MM-dd") });

            var results = await dataSource.GetEntityAsync(AP_INVOICE_TABLE, filters);
            if (results == null)
                return new List<AP_INVOICE>();

            return results.Cast<AP_INVOICE>().OrderByDescending(i => i.INVOICE_DATE).ToList();
        }

        /// <summary>
        /// Updates an AP invoice.
        /// </summary>
        public async Task<AP_INVOICE> UpdateInvoiceAsync(UpdateAPInvoiceRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.ApInvoiceId))
                throw new ArgumentException("AP Invoice ID is required.", nameof(request));

            var connName = connectionName ?? _connectionName;
            var invoice = await GetInvoiceAsync(request.ApInvoiceId, connName);
            if (invoice == null)
                throw new InvalidOperationException($"AP invoice {request.ApInvoiceId} not found.");

            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            // Update properties
            invoice.INVOICE_NUMBER = request.InvoiceNumber;
            invoice.VENDOR_BA_ID = request.VendorBaId;
            invoice.INVOICE_DATE = request.InvoiceDate;
            invoice.DUE_DATE = request.DueDate;
            invoice.TOTAL_AMOUNT = request.TotalAmount;
            invoice.STATUS = request.Status;

            // Recalculate balance due
            invoice.BALANCE_DUE = (invoice.TOTAL_AMOUNT ?? 0m) - (invoice.PAID_AMOUNT ?? 0m);

            if (invoice is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForUpdate(ppdmEntity, userId);
            }

            var result = dataSource.UpdateEntity(AP_INVOICE_TABLE, invoice);
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to update AP invoice {InvoiceId}: {Error}", request.ApInvoiceId, errorMessage);
                throw new InvalidOperationException($"Failed to update AP invoice: {errorMessage}");
            }

            _logger?.LogDebug("Updated AP invoice {InvoiceId}", request.ApInvoiceId);
            return invoice;
        }

        /// <summary>
        /// Deletes an AP invoice (soft delete by setting ACTIVE_IND = 'N').
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

            var result = dataSource.UpdateEntity(AP_INVOICE_TABLE, invoice);
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                _logger?.LogError("Failed to delete AP invoice {InvoiceId}", invoiceId);
                return false;
            }

            _logger?.LogDebug("Deleted AP invoice {InvoiceId}", invoiceId);
            return true;
        }

        /// <summary>
        /// Creates a payment against an AP invoice.
        /// </summary>
        public async Task<AP_PAYMENT> CreatePaymentAsync(CreateAPPaymentRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.ApInvoiceId))
                throw new ArgumentException("AP Invoice ID is required.", nameof(request));

            var connName = connectionName ?? _connectionName;
            var invoice = await GetInvoiceAsync(request.ApInvoiceId, connName);
            if (invoice == null)
                throw new InvalidOperationException($"AP invoice {request.ApInvoiceId} not found.");

            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var payment = new AP_PAYMENT
            {
                AP_PAYMENT_ID = Guid.NewGuid().ToString(),
                AP_INVOICE_ID = request.ApInvoiceId,
                PAYMENT_NUMBER = GeneratePaymentNumber(),
                PAYMENT_DATE = request.PaymentDate,
                PAYMENT_AMOUNT = request.PaymentAmount,
                PAYMENT_METHOD = request.PaymentMethod,
                CHECK_NUMBER = request.CheckNumber,
                ACTIVE_IND = "Y"
            };

            if (payment is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
            }

            var insertResult = dataSource.InsertEntity(AP_PAYMENT_TABLE, payment);
            if (insertResult != null && insertResult.Errors != null && insertResult.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", insertResult.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to create payment for AP invoice {InvoiceId}: {Error}", request.ApInvoiceId, errorMessage);
                throw new InvalidOperationException($"Failed to save payment: {errorMessage}");
            }

            // Update invoice payment amounts
            var payments = await GetPaymentsByInvoiceAsync(request.ApInvoiceId, connName);
            decimal totalPaid = payments.Sum(p => p.PAYMENT_AMOUNT ?? 0m);

            invoice.PAID_AMOUNT = totalPaid;
            invoice.BALANCE_DUE = (invoice.TOTAL_AMOUNT ?? 0m) - totalPaid;

            // Update invoice status
            if (invoice.BALANCE_DUE <= 0.01m)
                invoice.STATUS = "Paid";
            else if (totalPaid > 0)
                invoice.STATUS = "Partial";
            else
                invoice.STATUS = "Open";

            if (invoice is IPPDMEntity invoicePpdmEntity)
            {
                _commonColumnHandler.PrepareForUpdate(invoicePpdmEntity, userId);
            }

            dataSource.UpdateEntity(AP_INVOICE_TABLE, invoice);

            _logger?.LogDebug("Created payment {Amount} for AP invoice {InvoiceId}", request.PaymentAmount, request.ApInvoiceId);
            return payment;
        }

        /// <summary>
        /// Gets all payments for an AP invoice.
        /// </summary>
        public async Task<List<AP_PAYMENT>> GetPaymentsByInvoiceAsync(string invoiceId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(invoiceId))
                return new List<AP_PAYMENT>();

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "AP_INVOICE_ID", Operator = "=", FilterValue = invoiceId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await dataSource.GetEntityAsync(AP_PAYMENT_TABLE, filters);
            if (results == null)
                return new List<AP_PAYMENT>();

            return results.Cast<AP_PAYMENT>().OrderBy(p => p.PAYMENT_DATE).ToList();
        }

        /// <summary>
        /// Processes a payment (marks as processed).
        /// </summary>
        public async Task<AP_PAYMENT> ProcessPaymentAsync(string paymentId, string userId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(paymentId))
                throw new ArgumentException("Payment ID is required.", nameof(paymentId));

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "AP_PAYMENT_ID", Operator = "=", FilterValue = paymentId }
            };

            var results = await dataSource.GetEntityAsync(AP_PAYMENT_TABLE, filters);
            var payment = results?.Cast<AP_PAYMENT>().FirstOrDefault();

            if (payment == null)
                throw new InvalidOperationException($"Payment {paymentId} not found.");

            // Update payment status to processed
            payment.STATUS = "Processed";

            if (payment is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForUpdate(ppdmEntity, userId);
            }

            var result = dataSource.UpdateEntity(AP_PAYMENT_TABLE, payment);
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to process payment {PaymentId}: {Error}", paymentId, errorMessage);
                throw new InvalidOperationException($"Failed to process payment: {errorMessage}");
            }

            _logger?.LogDebug("Processed payment {PaymentId}", paymentId);
            return payment;
        }

        /// <summary>
        /// Creates a credit memo.
        /// </summary>
        public async Task<AP_CREDIT_MEMO> CreateCreditMemoAsync(CreateAPCreditMemoRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.ApInvoiceId))
                throw new ArgumentException("AP Invoice ID is required.", nameof(request));

            var connName = connectionName ?? _connectionName;
            var invoice = await GetInvoiceAsync(request.ApInvoiceId, connName);
            if (invoice == null)
                throw new InvalidOperationException($"AP invoice {request.ApInvoiceId} not found.");

            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var creditMemo = new AP_CREDIT_MEMO
            {
                AP_CREDIT_MEMO_ID = Guid.NewGuid().ToString(),
                AP_INVOICE_ID = request.ApInvoiceId,
                CREDIT_MEMO_NUMBER = request.CreditMemoNumber,
                CREDIT_MEMO_DATE = request.CreditMemoDate,
                CREDIT_AMOUNT = request.CreditAmount,
                REASON = request.Reason,
                DESCRIPTION = request.Description,
                STATUS = "Active",
                ACTIVE_IND = "Y"
            };

            if (creditMemo is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
            }

            var insertResult = dataSource.InsertEntity(AP_CREDIT_MEMO_TABLE, creditMemo);
            if (insertResult != null && insertResult.Errors != null && insertResult.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", insertResult.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to create credit memo for AP invoice {InvoiceId}: {Error}", request.ApInvoiceId, errorMessage);
                throw new InvalidOperationException($"Failed to save credit memo: {errorMessage}");
            }

            // Adjust invoice balance
            invoice.BALANCE_DUE = (invoice.BALANCE_DUE ?? 0m) - request.CreditAmount;
            if (invoice.BALANCE_DUE < 0)
                invoice.BALANCE_DUE = 0;

            if (invoice is IPPDMEntity invoicePpdmEntity)
            {
                _commonColumnHandler.PrepareForUpdate(invoicePpdmEntity, userId);
            }

            dataSource.UpdateEntity(AP_INVOICE_TABLE, invoice);

            _logger?.LogDebug("Created credit memo {Amount} for AP invoice {InvoiceId}", request.CreditAmount, request.ApInvoiceId);
            return creditMemo;
        }

        /// <summary>
        /// Approves an AP invoice.
        /// </summary>
        public async Task<APApprovalResult> ApproveInvoiceAsync(string invoiceId, string approverId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(invoiceId))
                throw new ArgumentException("Invoice ID is required.", nameof(invoiceId));

            var connName = connectionName ?? _connectionName;
            var invoice = await GetInvoiceAsync(invoiceId, connName);
            if (invoice == null)
                throw new InvalidOperationException($"AP invoice {invoiceId} not found.");

            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            invoice.STATUS = "Approved";

            if (invoice is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForUpdate(ppdmEntity, approverId);
            }

            dataSource.UpdateEntity(AP_INVOICE_TABLE, invoice);

            return new APApprovalResult
            {
                ApInvoiceId = invoiceId,
                IsApproved = true,
                ApproverId = approverId,
                ApprovalDate = DateTime.UtcNow,
                Status = "Approved"
            };
        }

        /// <summary>
        /// Gets AP aging summary.
        /// </summary>
        public async Task<List<APAgingSummary>> GetAPAgingAsync(string? vendorId, string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
                new AppFilter { FieldName = "STATUS", Operator = "IN", FilterValue = "Open,Partial" }
            };

            if (!string.IsNullOrEmpty(vendorId))
            {
                filters.Add(new AppFilter { FieldName = "VENDOR_BA_ID", Operator = "=", FilterValue = vendorId });
            }

            var results = await dataSource.GetEntityAsync(AP_INVOICE_TABLE, filters);
            var invoiceList = results?.Cast<AP_INVOICE>().ToList() ?? new List<AP_INVOICE>();

            var aging = new List<APAgingSummary>();
            var today = DateTime.UtcNow.Date;

            foreach (var invoice in invoiceList)
            {
                if (invoice.BALANCE_DUE <= 0.01m)
                    continue;

                var dueDate = invoice.DUE_DATE ?? invoice.INVOICE_DATE ?? today;
                int daysPastDue = (today - dueDate).Days;
                string agingBucket = GetAgingBucket(daysPastDue);

                aging.Add(new APAgingSummary
                {
                    ApInvoiceId = invoice.AP_INVOICE_ID ?? string.Empty,
                    InvoiceNumber = invoice.INVOICE_NUMBER ?? string.Empty,
                    VendorBaId = invoice.VENDOR_BA_ID ?? string.Empty,
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
        /// Gets vendor summary.
        /// </summary>
        public async Task<VendorSummary> GetVendorSummaryAsync(string vendorId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(vendorId))
                throw new ArgumentException("Vendor ID is required.", nameof(vendorId));

            var connName = connectionName ?? _connectionName;
            var invoices = await GetInvoicesByVendorAsync(vendorId, null, null, connName);

            decimal totalInvoices = invoices.Sum(i => i.TOTAL_AMOUNT ?? 0m);
            decimal totalPaid = invoices.Sum(i => i.PAID_AMOUNT ?? 0m);
            decimal totalOutstanding = invoices.Sum(i => i.BALANCE_DUE ?? 0m);
            int numberOfInvoices = invoices.Count;

            int numberOfOverdueInvoices = invoices.Count(i =>
            {
                var dueDate = i.DUE_DATE ?? i.INVOICE_DATE ?? DateTime.UtcNow;
                return DateTime.UtcNow.Date > dueDate.Date && (i.BALANCE_DUE ?? 0m) > 0.01m;
            });

            // Calculate average days to pay
            var paidInvoices = invoices.Where(i => (i.PAID_AMOUNT ?? 0m) > 0 && i.STATUS == "Paid").ToList();
            decimal averageDaysToPay = 0m;

            if (paidInvoices.Any())
            {
                var daysToPay = paidInvoices.Select(i =>
                {
                    var payments = GetPaymentsByInvoiceAsync(i.AP_INVOICE_ID, connName).GetAwaiter().GetResult();
                    var firstPayment = payments.OrderBy(p => p.PAYMENT_DATE).FirstOrDefault();

                    if (firstPayment != null && i.INVOICE_DATE.HasValue)
                    {
                        return (firstPayment.PAYMENT_DATE ?? DateTime.UtcNow) - i.INVOICE_DATE.Value;
                    }

                    return TimeSpan.Zero;
                }).Where(ts => ts.TotalDays > 0).Select(ts => (decimal)ts.TotalDays).ToList();

                if (daysToPay.Any())
                    averageDaysToPay = daysToPay.Average();
            }

            return new VendorSummary
            {
                VendorBaId = vendorId,
                TotalInvoices = totalInvoices,
                TotalPaid = totalPaid,
                TotalOutstanding = totalOutstanding,
                NumberOfInvoices = numberOfInvoices,
                NumberOfOverdueInvoices = numberOfOverdueInvoices,
                AverageDaysToPay = averageDaysToPay
            };
        }

        /// <summary>
        /// Processes a batch of payments.
        /// </summary>
        public async Task<PaymentBatchResult> ProcessPaymentBatchAsync(PaymentBatchRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (request.PaymentIds == null || !request.PaymentIds.Any())
                throw new ArgumentException("Payment IDs are required.", nameof(request));

            var connName = connectionName ?? _connectionName;

            var result = new PaymentBatchResult
            {
                BatchDate = request.BatchDate,
                TotalPayments = request.PaymentIds.Count
            };

            foreach (var paymentId in request.PaymentIds)
            {
                try
                {
                    var payment = await ProcessPaymentAsync(paymentId, userId, connName);
                    result.SuccessfulPayments++;
                    result.TotalAmount += payment.PAYMENT_AMOUNT ?? 0m;
                    result.ProcessedPaymentIds.Add(paymentId);
                }
                catch (Exception ex)
                {
                    result.FailedPayments++;
                    result.FailedPaymentIds.Add(paymentId);
                    _logger?.LogError(ex, "Failed to process payment {PaymentId}", paymentId);
                }
            }

            _logger?.LogDebug("Processed payment batch: {Successful} successful, {Failed} failed",
                result.SuccessfulPayments, result.FailedPayments);

            return result;
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
        /// Generates a payment number.
        /// </summary>
        private string GeneratePaymentNumber()
        {
            return $"PAY-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
    }
}
