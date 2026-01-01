using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.DTOs.Accounting;
using Beep.OilandGas.Models.Enums;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.DataBase;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.ProductionAccounting.AccountsReceivable
{
    /// <summary>
    /// Service for managing accounts receivable invoices.
    /// Uses IDataSource directly for database operations.
    /// </summary>
    public class ARService : IARService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly ILogger<ARService>? _logger;
        private readonly string _connectionName;

        private const string AR_INVOICE_TABLE = "AR_INVOICE";
        private const string AR_PAYMENT_TABLE = "AR_PAYMENT";
        private const string AR_CREDIT_MEMO_TABLE = "AR_CREDIT_MEMO";

        public ARService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            ILoggerFactory? loggerFactory,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _logger = loggerFactory?.CreateLogger<ARService>();
            _connectionName = connectionName ?? "PPDM39";
        }

        /// <summary>
        /// Creates a new AR invoice.
        /// </summary>
        public async Task<AR_INVOICE> CreateInvoiceAsync(CreateARInvoiceRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var invoice = new AR_INVOICE
            {
                AR_INVOICE_ID = Guid.NewGuid().ToString(),
                INVOICE_NUMBER = request.InvoiceNumber,
                CUSTOMER_BA_ID = request.CustomerBaId,
                INVOICE_DATE = request.InvoiceDate,
                DUE_DATE = request.DueDate,
                TOTAL_AMOUNT = request.TotalAmount,
                BALANCE_DUE = request.TotalAmount,
                PAID_AMOUNT = 0m,
                STATUS = ReceivableStatus.Open.ToString(),
                ACTIVE_IND = "Y"
            };

            if (invoice is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
            }

            var result = dataSource.InsertEntity(AR_INVOICE_TABLE, invoice);
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to create AR invoice {InvoiceNumber}: {Error}", request.InvoiceNumber, errorMessage);
                throw new InvalidOperationException($"Failed to save AR invoice: {errorMessage}");
            }

            _logger?.LogDebug("Created AR invoice {InvoiceNumber}", request.InvoiceNumber);
            return invoice;
        }

        /// <summary>
        /// Gets an AR invoice by ID.
        /// </summary>
        public async Task<AR_INVOICE?> GetInvoiceAsync(string invoiceId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(invoiceId))
                return null;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "AR_INVOICE_ID", Operator = "=", FilterValue = invoiceId }
            };

            var results = await dataSource.GetEntityAsync(AR_INVOICE_TABLE, filters);
            return results?.Cast<AR_INVOICE>().FirstOrDefault();
        }

        /// <summary>
        /// Gets invoices by customer.
        /// </summary>
        public async Task<List<AR_INVOICE>> GetInvoicesByCustomerAsync(string customerId, DateTime? startDate, DateTime? endDate, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(customerId))
                return new List<AR_INVOICE>();

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

            var results = await dataSource.GetEntityAsync(AR_INVOICE_TABLE, filters);
            if (results == null)
                return new List<AR_INVOICE>();

            return results.Cast<AR_INVOICE>().OrderByDescending(i => i.INVOICE_DATE).ToList();
        }

        /// <summary>
        /// Updates an AR invoice.
        /// </summary>
        public async Task<AR_INVOICE> UpdateInvoiceAsync(UpdateARInvoiceRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.ArInvoiceId))
                throw new ArgumentException("AR Invoice ID is required.", nameof(request));

            var connName = connectionName ?? _connectionName;
            var invoice = await GetInvoiceAsync(request.ArInvoiceId, connName);
            if (invoice == null)
                throw new InvalidOperationException($"AR invoice {request.ArInvoiceId} not found.");

            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            // Update properties
            invoice.INVOICE_NUMBER = request.InvoiceNumber;
            invoice.CUSTOMER_BA_ID = request.CustomerBaId;
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

            var result = dataSource.UpdateEntity(AR_INVOICE_TABLE, invoice);
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to update AR invoice {InvoiceId}: {Error}", request.ArInvoiceId, errorMessage);
                throw new InvalidOperationException($"Failed to update AR invoice: {errorMessage}");
            }

            _logger?.LogDebug("Updated AR invoice {InvoiceId}", request.ArInvoiceId);
            return invoice;
        }

        /// <summary>
        /// Deletes an AR invoice (soft delete by setting ACTIVE_IND = 'N').
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

            var result = dataSource.UpdateEntity(AR_INVOICE_TABLE, invoice);
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                _logger?.LogError("Failed to delete AR invoice {InvoiceId}", invoiceId);
                return false;
            }

            _logger?.LogDebug("Deleted AR invoice {InvoiceId}", invoiceId);
            return true;
        }

        /// <summary>
        /// Creates a payment from a customer.
        /// </summary>
        public async Task<AR_PAYMENT> CreatePaymentAsync(CreateARPaymentRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var payment = new AR_PAYMENT
            {
                AR_PAYMENT_ID = Guid.NewGuid().ToString(),
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

            var insertResult = dataSource.InsertEntity(AR_PAYMENT_TABLE, payment);
            if (insertResult != null && insertResult.Errors != null && insertResult.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", insertResult.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to create AR payment: {Error}", errorMessage);
                throw new InvalidOperationException($"Failed to save AR payment: {errorMessage}");
            }

            _logger?.LogDebug("Created AR payment {Amount} from customer", request.PaymentAmount);
            return payment;
        }

        /// <summary>
        /// Applies a payment to an invoice.
        /// </summary>
        public async Task<AR_PAYMENT> ApplyPaymentAsync(string paymentId, string invoiceId, decimal amount, string userId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(paymentId))
                throw new ArgumentException("Payment ID is required.", nameof(paymentId));
            if (string.IsNullOrEmpty(invoiceId))
                throw new ArgumentException("Invoice ID is required.", nameof(invoiceId));
            if (amount <= 0)
                throw new ArgumentException("Payment amount must be greater than zero.", nameof(amount));

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            // Get payment
            var paymentFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "AR_PAYMENT_ID", Operator = "=", FilterValue = paymentId }
            };

            var paymentResults = await dataSource.GetEntityAsync(AR_PAYMENT_TABLE, paymentFilters);
            var payment = paymentResults?.Cast<AR_PAYMENT>().FirstOrDefault();

            if (payment == null)
                throw new InvalidOperationException($"Payment {paymentId} not found.");

            // Get invoice
            var invoice = await GetInvoiceAsync(invoiceId, connName);
            if (invoice == null)
                throw new InvalidOperationException($"AR invoice {invoiceId} not found.");

            // Link payment to invoice
            payment.AR_INVOICE_ID = invoiceId;

            if (payment is IPPDMEntity paymentPpdmEntity)
            {
                _commonColumnHandler.PrepareForUpdate(paymentPpdmEntity, userId);
            }

            dataSource.UpdateEntity(AR_PAYMENT_TABLE, payment);

            // Update invoice payment amounts
            var payments = await GetPaymentsByInvoiceAsync(invoiceId, connName);
            decimal totalPaid = payments.Sum(p => p.PAYMENT_AMOUNT ?? 0m);

            invoice.PAID_AMOUNT = totalPaid;
            invoice.BALANCE_DUE = (invoice.TOTAL_AMOUNT ?? 0m) - totalPaid;

            // Update invoice status
            if (invoice.BALANCE_DUE <= 0.01m)
                invoice.STATUS = ReceivableStatus.Paid.ToString();
            else if (totalPaid > 0)
                invoice.STATUS = ReceivableStatus.PartiallyPaid.ToString();
            else
                invoice.STATUS = ReceivableStatus.Open.ToString();

            // Check if overdue
            if (invoice.DUE_DATE.HasValue && DateTime.UtcNow.Date > invoice.DUE_DATE.Value.Date && invoice.BALANCE_DUE > 0.01m)
                invoice.STATUS = ReceivableStatus.Overdue.ToString();

            if (invoice is IPPDMEntity invoicePpdmEntity)
            {
                _commonColumnHandler.PrepareForUpdate(invoicePpdmEntity, userId);
            }

            dataSource.UpdateEntity(AR_INVOICE_TABLE, invoice);

            _logger?.LogDebug("Applied payment {Amount} to AR invoice {InvoiceId}", amount, invoiceId);
            return payment;
        }

        /// <summary>
        /// Gets all payments for an AR invoice.
        /// </summary>
        public async Task<List<AR_PAYMENT>> GetPaymentsByInvoiceAsync(string invoiceId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(invoiceId))
                return new List<AR_PAYMENT>();

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "AR_INVOICE_ID", Operator = "=", FilterValue = invoiceId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await dataSource.GetEntityAsync(AR_PAYMENT_TABLE, filters);
            if (results == null)
                return new List<AR_PAYMENT>();

            return results.Cast<AR_PAYMENT>().OrderBy(p => p.PAYMENT_DATE).ToList();
        }

        /// <summary>
        /// Creates a credit memo.
        /// </summary>
        public async Task<AR_CREDIT_MEMO> CreateCreditMemoAsync(CreateARCreditMemoRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.ArInvoiceId))
                throw new ArgumentException("AR Invoice ID is required.", nameof(request));

            var connName = connectionName ?? _connectionName;
            var invoice = await GetInvoiceAsync(request.ArInvoiceId, connName);
            if (invoice == null)
                throw new InvalidOperationException($"AR invoice {request.ArInvoiceId} not found.");

            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var creditMemo = new AR_CREDIT_MEMO
            {
                AR_CREDIT_MEMO_ID = Guid.NewGuid().ToString(),
                AR_INVOICE_ID = request.ArInvoiceId,
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

            var insertResult = dataSource.InsertEntity(AR_CREDIT_MEMO_TABLE, creditMemo);
            if (insertResult != null && insertResult.Errors != null && insertResult.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", insertResult.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to create credit memo for AR invoice {InvoiceId}: {Error}", request.ArInvoiceId, errorMessage);
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

            dataSource.UpdateEntity(AR_INVOICE_TABLE, invoice);

            _logger?.LogDebug("Created credit memo {Amount} for AR invoice {InvoiceId}", request.CreditAmount, request.ArInvoiceId);
            return creditMemo;
        }

        /// <summary>
        /// Approves an AR invoice.
        /// </summary>
        public async Task<ARApprovalResult> ApproveInvoiceAsync(string invoiceId, string approverId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(invoiceId))
                throw new ArgumentException("Invoice ID is required.", nameof(invoiceId));

            var connName = connectionName ?? _connectionName;
            var invoice = await GetInvoiceAsync(invoiceId, connName);
            if (invoice == null)
                throw new InvalidOperationException($"AR invoice {invoiceId} not found.");

            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            invoice.STATUS = "Approved";

            if (invoice is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForUpdate(ppdmEntity, approverId);
            }

            dataSource.UpdateEntity(AR_INVOICE_TABLE, invoice);

            return new ARApprovalResult
            {
                ArInvoiceId = invoiceId,
                IsApproved = true,
                ApproverId = approverId,
                ApprovalDate = DateTime.UtcNow,
                Status = "Approved"
            };
        }

        /// <summary>
        /// Gets AR aging summary.
        /// </summary>
        public async Task<List<ARAgingSummary>> GetARAgingAsync(string? customerId, string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
                new AppFilter { FieldName = "STATUS", Operator = "IN", FilterValue = "Open,PartiallyPaid,Overdue" }
            };

            if (!string.IsNullOrEmpty(customerId))
            {
                filters.Add(new AppFilter { FieldName = "CUSTOMER_BA_ID", Operator = "=", FilterValue = customerId });
            }

            var results = await dataSource.GetEntityAsync(AR_INVOICE_TABLE, filters);
            var invoiceList = results?.Cast<AR_INVOICE>().ToList() ?? new List<AR_INVOICE>();

            var aging = new List<ARAgingSummary>();
            var today = DateTime.UtcNow.Date;

            foreach (var invoice in invoiceList)
            {
                if (invoice.BALANCE_DUE <= 0.01m)
                    continue;

                var dueDate = invoice.DUE_DATE ?? invoice.INVOICE_DATE ?? today;
                int daysPastDue = (today - dueDate).Days;
                string agingBucket = GetAgingBucket(daysPastDue);

                aging.Add(new ARAgingSummary
                {
                    ArInvoiceId = invoice.AR_INVOICE_ID ?? string.Empty,
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
        /// Gets customer summary.
        /// </summary>
        public async Task<CustomerSummary> GetCustomerSummaryAsync(string customerId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(customerId))
                throw new ArgumentException("Customer ID is required.", nameof(customerId));

            var connName = connectionName ?? _connectionName;
            var invoices = await GetInvoicesByCustomerAsync(customerId, null, null, connName);

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
            var paidInvoices = invoices.Where(i => (i.PAID_AMOUNT ?? 0m) > 0 && i.STATUS == ReceivableStatus.Paid.ToString()).ToList();
            decimal averageDaysToPay = 0m;

            if (paidInvoices.Any())
            {
                var daysToPay = paidInvoices.Select(i =>
                {
                    var payments = GetPaymentsByInvoiceAsync(i.AR_INVOICE_ID, connName).GetAwaiter().GetResult();
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

            return new CustomerSummary
            {
                CustomerBaId = customerId,
                TotalInvoices = totalInvoices,
                TotalPaid = totalPaid,
                TotalOutstanding = totalOutstanding,
                NumberOfInvoices = numberOfInvoices,
                NumberOfOverdueInvoices = numberOfOverdueInvoices,
                AverageDaysToPay = averageDaysToPay
            };
        }

        /// <summary>
        /// Applies a payment to multiple invoices.
        /// </summary>
        public async Task<PaymentApplicationResult> ApplyPaymentToInvoicesAsync(PaymentApplicationRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.PaymentId))
                throw new ArgumentException("Payment ID is required.", nameof(request));
            if (request.Applications == null || !request.Applications.Any())
                throw new ArgumentException("At least one payment application is required.", nameof(request));

            var connName = connectionName ?? _connectionName;

            var result = new PaymentApplicationResult
            {
                PaymentId = request.PaymentId
            };

            foreach (var application in request.Applications)
            {
                try
                {
                    await ApplyPaymentAsync(request.PaymentId, application.InvoiceId, application.Amount, userId, connName);
                    result.SuccessfulApplications++;
                    result.TotalApplied += application.Amount;
                    result.AppliedInvoiceIds.Add(application.InvoiceId);
                }
                catch (Exception ex)
                {
                    result.FailedApplications++;
                    result.FailedInvoiceIds.Add(application.InvoiceId);
                    _logger?.LogError(ex, "Failed to apply payment to invoice {InvoiceId}", application.InvoiceId);
                }
            }

            _logger?.LogDebug("Applied payment {PaymentId} to {Successful} invoices, {Failed} failed",
                request.PaymentId, result.SuccessfulApplications, result.FailedApplications);

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
