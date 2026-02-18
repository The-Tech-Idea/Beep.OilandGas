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
    /// Accounts receivable service for invoices, payments, and credit memos.
    /// </summary>
    public class ARService : IARService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<ARService> _logger;
        private const string ConnectionName = "PPDM39";

        public ARService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            AccountingBasisPostingService basisPosting,
            ILogger<ARService> logger = null,
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

        public async Task<AR_INVOICE> CreateInvoiceAsync(CreateARInvoiceRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.CustomerBaId))
                throw new InvalidOperationException("Customer BA ID is required");
            if (request.TotalAmount <= 0m)
                throw new InvalidOperationException("Invoice amount must be positive");
            if (request.DueDate.Date < request.InvoiceDate.Date)
                throw new InvalidOperationException("Due date cannot be earlier than invoice date");

            var invoice = new AR_INVOICE
            {
                AR_INVOICE_ID = Guid.NewGuid().ToString(),
                INVOICE_NUMBER = string.IsNullOrWhiteSpace(request.InvoiceNumber)
                    ? await GenerateInvoiceNumberAsync(connectionName)
                    : request.InvoiceNumber,
                CUSTOMER_BA_ID = request.CustomerBaId,
                INVOICE_DATE = request.InvoiceDate,
                DUE_DATE = request.DueDate,
                TOTAL_AMOUNT = request.TotalAmount,
                PAID_AMOUNT = 0m,
                BALANCE_DUE = request.TotalAmount,
                STATUS = InvoiceStatuses.Draft,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<AR_INVOICE>("AR_INVOICE", connectionName);
            await repo.InsertAsync(invoice, userId);
            return invoice;
        }

        public async Task<AR_INVOICE?> GetInvoiceAsync(string invoiceId, string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(invoiceId))
                throw new ArgumentNullException(nameof(invoiceId));

            var repo = await GetRepoAsync<AR_INVOICE>("AR_INVOICE", connectionName);
            var invoice = await repo.GetByIdAsync(invoiceId);
            return invoice as AR_INVOICE;
        }

        public async Task<List<AR_INVOICE>> GetInvoicesByCustomerAsync(string customerId, DateTime? startDate, DateTime? endDate, string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                throw new ArgumentNullException(nameof(customerId));

            var repo = await GetRepoAsync<AR_INVOICE>("AR_INVOICE", connectionName);
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
            return results?.Cast<AR_INVOICE>().ToList() ?? new List<AR_INVOICE>();
        }

        public async Task<AR_INVOICE> UpdateInvoiceAsync(UpdateARInvoiceRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.ArInvoiceId))
                throw new ArgumentNullException(nameof(request.ArInvoiceId));

            var invoice = await GetInvoiceAsync(request.ArInvoiceId, connectionName);
            if (invoice == null)
                throw new InvalidOperationException($"AR invoice not found: {request.ArInvoiceId}");

            invoice.INVOICE_NUMBER = request.InvoiceNumber ?? invoice.INVOICE_NUMBER;
            invoice.CUSTOMER_BA_ID = request.CustomerBaId ?? invoice.CUSTOMER_BA_ID;
            invoice.INVOICE_DATE = request.InvoiceDate ?? invoice.INVOICE_DATE;
            var updatedDueDate = request.DueDate ?? invoice.DUE_DATE;
            var updatedInvoiceDate = request.InvoiceDate ?? invoice.INVOICE_DATE;
            if (updatedDueDate.HasValue && updatedInvoiceDate.HasValue
                && updatedDueDate.Value.Date < updatedInvoiceDate.Value.Date)
                throw new InvalidOperationException("Due date cannot be earlier than invoice date");
            invoice.DUE_DATE = updatedDueDate;
            if (request.TotalAmount.HasValue)
            {
                invoice.TOTAL_AMOUNT = request.TotalAmount.Value;
                invoice.BALANCE_DUE = Math.Max(0m, request.TotalAmount.Value - (invoice.PAID_AMOUNT ?? 0m));
            }
            invoice.STATUS = request.Status ?? invoice.STATUS;
            invoice.ROW_CHANGED_BY = userId;
            invoice.ROW_CHANGED_DATE = DateTime.UtcNow;

            var repo = await GetRepoAsync<AR_INVOICE>("AR_INVOICE", connectionName);
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

            var repo = await GetRepoAsync<AR_INVOICE>("AR_INVOICE", connectionName);
            await repo.UpdateAsync(invoice, userId);
            return true;
        }

        public async Task<AR_PAYMENT> CreatePaymentAsync(CreateARPaymentRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.CustomerBaId))
                throw new InvalidOperationException("Customer BA ID is required");
            if (request.PaymentAmount <= 0m)
                throw new InvalidOperationException("Payment amount must be positive");

            var payment = new AR_PAYMENT
            {
                AR_PAYMENT_ID = Guid.NewGuid().ToString(),
                PAYMENT_NUMBER = string.IsNullOrWhiteSpace(request.ReferenceNumber)
                    ? request.CheckNumber
                    : request.ReferenceNumber,
                PAYMENT_DATE = request.PaymentDate,
                PAYMENT_AMOUNT = request.PaymentAmount,
                PAYMENT_METHOD = request.PaymentMethod,
                CHECK_NUMBER = request.CheckNumber,
                REMARK = request.Description,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<AR_PAYMENT>("AR_PAYMENT", connectionName);
            await repo.InsertAsync(payment, userId);
            return payment;
        }

        public async Task<AR_PAYMENT> ApplyPaymentAsync(string paymentId, string invoiceId, decimal amount, string userId, string? connectionName = null)
        {
            if (amount <= 0m)
                throw new InvalidOperationException("Applied amount must be positive");

            var paymentRepo = await GetRepoAsync<AR_PAYMENT>("AR_PAYMENT", connectionName);
            var payment = await paymentRepo.GetByIdAsync(paymentId) as AR_PAYMENT;
            if (payment == null)
                throw new InvalidOperationException($"Payment not found: {paymentId}");

            var invoice = await GetInvoiceAsync(invoiceId, connectionName);
            if (invoice == null)
                throw new InvalidOperationException($"AR invoice not found: {invoiceId}");

            var remainingBalance = invoice.BALANCE_DUE ?? 0m;
            var appliedAmount = Math.Min(amount, remainingBalance);

            payment.AR_INVOICE_ID = invoiceId;
            paymentRepo = await GetRepoAsync<AR_PAYMENT>("AR_PAYMENT", connectionName);
            await paymentRepo.UpdateAsync(payment, userId);

            invoice.PAID_AMOUNT = (invoice.PAID_AMOUNT ?? 0m) + appliedAmount;
            invoice.BALANCE_DUE = Math.Max(0m, (invoice.TOTAL_AMOUNT ?? 0m) - (invoice.PAID_AMOUNT ?? 0m));
            invoice.STATUS = invoice.BALANCE_DUE == 0m ? InvoiceStatuses.Paid : InvoiceStatuses.PartiallyPaid;
            invoice.ROW_CHANGED_BY = userId;
            invoice.ROW_CHANGED_DATE = DateTime.UtcNow;

            var invoiceRepo = await GetRepoAsync<AR_INVOICE>("AR_INVOICE", connectionName);
            await invoiceRepo.UpdateAsync(invoice, userId);

            await _basisPosting.PostBalancedEntryByAccountAsync(
                GetAccountId(AccountMappingKeys.Cash, DefaultGlAccounts.Cash),
                GetAccountId(AccountMappingKeys.AccountsReceivable, DefaultGlAccounts.AccountsReceivable),
                appliedAmount,
                $"AR payment applied {invoice.INVOICE_NUMBER}",
                userId,
                connectionName ?? ConnectionName);

            return payment;
        }

        public async Task<List<AR_PAYMENT>> GetPaymentsByInvoiceAsync(string invoiceId, string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(invoiceId))
                throw new ArgumentNullException(nameof(invoiceId));

            var repo = await GetRepoAsync<AR_PAYMENT>("AR_PAYMENT", connectionName);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "AR_INVOICE_ID", Operator = "=", FilterValue = invoiceId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results?.Cast<AR_PAYMENT>().ToList() ?? new List<AR_PAYMENT>();
        }

        public async Task<AR_CREDIT_MEMO> CreateCreditMemoAsync(CreateARCreditMemoRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.ArInvoiceId))
                throw new InvalidOperationException("Invoice ID is required");
            if (request.CreditAmount <= 0m)
                throw new InvalidOperationException("Credit amount must be positive");

            var invoice = await GetInvoiceAsync(request.ArInvoiceId, connectionName);
            if (invoice == null)
                throw new InvalidOperationException($"AR invoice not found: {request.ArInvoiceId}");

            var creditMemo = new AR_CREDIT_MEMO
            {
                AR_CREDIT_MEMO_ID = Guid.NewGuid().ToString(),
                AR_INVOICE_ID = request.ArInvoiceId,
                CREDIT_MEMO_NUMBER = request.CreditMemoNumber,
                CREDIT_MEMO_DATE = request.CreditMemoDate,
                CREDIT_AMOUNT = request.CreditAmount,
                REASON = request.Reason,
                DESCRIPTION = request.Description,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<AR_CREDIT_MEMO>("AR_CREDIT_MEMO", connectionName);
            await repo.InsertAsync(creditMemo, userId);

            invoice.TOTAL_AMOUNT = Math.Max(0m, (invoice.TOTAL_AMOUNT ?? 0m) - request.CreditAmount);
            invoice.BALANCE_DUE = Math.Max(0m, (invoice.TOTAL_AMOUNT ?? 0m) - (invoice.PAID_AMOUNT ?? 0m));
            invoice.STATUS = invoice.BALANCE_DUE == 0m ? "PAID" : "ADJUSTED";
            invoice.ROW_CHANGED_BY = userId;
            invoice.ROW_CHANGED_DATE = DateTime.UtcNow;

            var invoiceRepo = await GetRepoAsync<AR_INVOICE>("AR_INVOICE", connectionName);
            await invoiceRepo.UpdateAsync(invoice, userId);

            await _basisPosting.PostBalancedEntryByAccountAsync(
                DefaultGlAccounts.Revenue,
                DefaultGlAccounts.AccountsReceivable,
                request.CreditAmount,
                $"AR credit memo {request.CreditMemoNumber}",
                userId,
                connectionName ?? ConnectionName);

            return creditMemo;
        }

        public async Task<ARApprovalResult> ApproveInvoiceAsync(string invoiceId, string approverId, string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(invoiceId))
                throw new ArgumentNullException(nameof(invoiceId));
            if (string.IsNullOrWhiteSpace(approverId))
                throw new ArgumentNullException(nameof(approverId));

            var invoice = await GetInvoiceAsync(invoiceId, connectionName);
            if (invoice == null)
                throw new InvalidOperationException($"AR invoice not found: {invoiceId}");

            if (!string.Equals(invoice.STATUS, "DRAFT", StringComparison.OrdinalIgnoreCase))
            {
                return new ARApprovalResult
                {
                    ArInvoiceId = invoice.AR_INVOICE_ID,
                    IsApproved = false,
                    ApproverId = approverId,
                    Status = invoice.STATUS
                };
            }

            await _basisPosting.PostBalancedEntryByAccountAsync(
                DefaultGlAccounts.AccountsReceivable,
                DefaultGlAccounts.Revenue,
                invoice.TOTAL_AMOUNT ?? 0m,
                $"AR invoice approved {invoice.INVOICE_NUMBER}",
                approverId,
                connectionName ?? ConnectionName);

            invoice.STATUS = "ISSUED";
            invoice.ROW_CHANGED_BY = approverId;
            invoice.ROW_CHANGED_DATE = DateTime.UtcNow;

            var repo = await GetRepoAsync<AR_INVOICE>("AR_INVOICE", connectionName);
            await repo.UpdateAsync(invoice, approverId);

            return new ARApprovalResult
            {
                ArInvoiceId = invoice.AR_INVOICE_ID,
                IsApproved = true,
                ApproverId = approverId,
                Status = invoice.STATUS
            };
        }

        public async Task<List<ARAgingSummary>> GetARAgingAsync(string? customerId, string? connectionName = null)
        {
            var repo = await GetRepoAsync<AR_INVOICE>("AR_INVOICE", connectionName);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            if (!string.IsNullOrWhiteSpace(customerId))
                filters.Add(new AppFilter { FieldName = "CUSTOMER_BA_ID", Operator = "=", FilterValue = customerId });

            var results = await repo.GetAsync(filters);
            var invoices = results?.Cast<AR_INVOICE>().ToList() ?? new List<AR_INVOICE>();
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

                return new ARAgingSummary
                {
                    ArInvoiceId = invoice.AR_INVOICE_ID,
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

        public async Task<CustomerSummary> GetCustomerSummaryAsync(string customerId, string? connectionName = null)
        {
            var invoices = await GetInvoicesByCustomerAsync(customerId, null, null, connectionName);
            var totalInvoices = invoices.Sum(i => i.TOTAL_AMOUNT ?? 0m);
            var totalPaid = invoices.Sum(i => i.PAID_AMOUNT ?? 0m);
            var totalOutstanding = invoices.Sum(i => i.BALANCE_DUE ?? 0m);
            var overdueCount = invoices.Count(i =>
                (i.DUE_DATE ?? i.INVOICE_DATE ?? DateTime.UtcNow) < DateTime.UtcNow &&
                (i.BALANCE_DUE ?? 0m) > 0m);

            return new CustomerSummary
            {
                CustomerBaId = customerId,
                TotalInvoices = totalInvoices,
                TotalPaid = totalPaid,
                TotalOutstanding = totalOutstanding,
                NumberOfInvoices = invoices.Count,
                NumberOfOverdueInvoices = overdueCount,
                AverageDaysToPay = CalculateAverageDaysToPay(invoices)
            };
        }

        public async Task<PaymentApplicationResult> ApplyPaymentToInvoicesAsync(PaymentApplicationRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.PaymentId))
                throw new ArgumentNullException(nameof(request.PaymentId));
            if (request.Applications == null || request.Applications.Count == 0)
                throw new InvalidOperationException("At least one application is required");

            var result = new PaymentApplicationResult
            {
                PaymentId = request.PaymentId
            };

            foreach (var application in request.Applications)
            {
                try
                {
                    await ApplyPaymentAsync(request.PaymentId, application.InvoiceId, application.Amount, userId, connectionName);
                    result.SuccessfulApplications++;
                    result.TotalApplied += application.Amount;
                    result.AppliedInvoiceIds.Add(application.InvoiceId);
                }
                catch
                {
                    result.FailedApplications++;
                    result.FailedInvoiceIds.Add(application.InvoiceId);
                }
            }

            return result;
        }

        private async Task<string> GenerateInvoiceNumberAsync(string? connectionName)
        {
            var repo = await GetRepoAsync<AR_INVOICE>("AR_INVOICE", connectionName);
            var existing = await repo.GetAsync(new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            });

            var count = existing?.Cast<AR_INVOICE>().Count() ?? 0;
            return $"AR-{DateTime.UtcNow:yyyyMMdd}-{(count + 1):D4}";
        }

        private async Task<string> GenerateCreditMemoNumberAsync(string? connectionName)
        {
            var repo = await GetRepoAsync<AR_CREDIT_MEMO>("AR_CREDIT_MEMO", connectionName);
            var existing = await repo.GetAsync(new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            });

            var count = existing?.Cast<AR_CREDIT_MEMO>().Count() ?? 0;
            return $"CM-{DateTime.UtcNow:yyyyMMdd}-{(count + 1):D4}";
        }

        private static decimal CalculateAverageDaysToPay(IEnumerable<AR_INVOICE> invoices)
        {
            var paidInvoices = invoices.Where(i => (i.PAID_AMOUNT ?? 0m) > 0m && i.INVOICE_DATE.HasValue).ToList();
            if (!paidInvoices.Any())
                return 0m;

            var totalDays = paidInvoices.Sum(i =>
            {
                var paidDate = i.ROW_CHANGED_DATE ?? DateTime.UtcNow;
                return (decimal)(paidDate - (i.INVOICE_DATE ?? paidDate)).TotalDays;
            });

            return totalDays / paidInvoices.Count;
        }

        private async Task<PPDMGenericRepository> GetRepoAsync<T>(string tableName, string? connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            // Use strongly typed class directly
            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(T), connectionName ?? ConnectionName, tableName);
        }

        public async Task<bool> HasUnpostedInvoicesAsync(DateTime periodEndDate)
        {
             try
            {
                var repo = await GetRepoAsync<AR_INVOICE>("AR_INVOICE", null);
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "STATUS", Operator = "=", FilterValue = InvoiceStatuses.Draft },
                    new AppFilter { FieldName = "INVOICE_DATE", Operator = "<=", FilterValue = periodEndDate.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                return results != null && results.Any();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking for unposted AR invoices");
                return true;
            }
        }

        private string GetAccountId(string key, string fallback)
        {
            return _accountMapping?.GetAccountId(key) ?? fallback;
        }
    }
}



