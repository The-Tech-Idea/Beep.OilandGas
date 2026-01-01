
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Manages accounts receivable invoices.
    /// Uses database access via IDataSource instead of in-memory dictionaries.
    /// </summary>
    public class ARManager
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly ILogger<ARManager>? _logger;
        private readonly string _connectionName;
        private const string AR_INVOICE_TABLE = "AR_INVOICE";

        public ARManager(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            ILogger<ARManager>? logger = null,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _logger = logger;
            _connectionName = connectionName ?? "PPDM39";
        }

        /// <summary>
        /// Creates a new AR invoice.
        /// </summary>
        public AR_INVOICE CreateARInvoice(CreateARInvoiceRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var invoice = new AR_INVOICE
            {
                AR_INVOICE_ID = Guid.NewGuid().ToString(),
                INVOICE_NUMBER = request.InvoiceNumber,
                CUSTOMER_BA_ID = request.CustomerBaId,
                INVOICE_DATE = request.InvoiceDate,
                DUE_DATE = request.DueDate,
                TOTAL_AMOUNT = request.TotalAmount,
                PAID_AMOUNT = 0m,
                BALANCE_DUE = request.TotalAmount,
                STATUS = ReceivableStatus.Open.ToString(),
                ACTIVE_IND = "Y",
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var invoiceData = ConvertARInvoiceToDictionary(invoice);
            var result = dataSource.InsertEntity(AR_INVOICE_TABLE, invoiceData);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to create AR invoice {InvoiceNumber}: {Error}", request.InvoiceNumber, errorMessage);
                throw new InvalidOperationException($"Failed to save AR invoice: {errorMessage}");
            }

            _logger?.LogDebug("Created AR invoice {InvoiceNumber} in database", request.InvoiceNumber);
            return invoice;
        }

        /// <summary>
        /// Gets an AR invoice by ID.
        /// </summary>
        public AR_INVOICE? GetARInvoice(string arInvoiceId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(arInvoiceId))
                return null;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "AR_INVOICE_ID", Operator = "=", FilterValue = arInvoiceId }
            };

            var results = dataSource.GetEntityAsync(AR_INVOICE_TABLE, filters).GetAwaiter().GetResult();
            var invoiceData = results?.OfType<Dictionary<string, object>>().FirstOrDefault();
            
            if (invoiceData == null)
                return null;

            return ConvertDictionaryToARInvoice(invoiceData);
        }

        /// <summary>
        /// Creates an AR invoice from a sales transaction.
        /// </summary>
        public AR_INVOICE CreateARInvoiceFromSalesTransaction(SalesTransaction transaction, string customerBaId, int paymentTermsDays = 30, string userId = "", string? connectionName = null)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            var request = new CreateARInvoiceRequest
            {
                InvoiceNumber = transaction.TransactionId,
                CustomerBaId = customerBaId,
                InvoiceDate = transaction.TransactionDate,
                DueDate = transaction.TransactionDate.AddDays(paymentTermsDays),
                TotalAmount = transaction.TotalValue
            };

            return CreateARInvoice(request, userId, connectionName);
        }

        /// <summary>
        /// Records a payment against an AR invoice.
        /// </summary>
        public void RecordPayment(string arInvoiceId, decimal paymentAmount, DateTime paymentDate, string userId, string? connectionName = null)
        {
            if (paymentAmount <= 0)
                throw new ArgumentException("Payment amount must be greater than zero.", nameof(paymentAmount));

            var connName = connectionName ?? _connectionName;
            var invoice = GetARInvoice(arInvoiceId, connName);
            if (invoice == null)
                throw new ArgumentException($"AR Invoice {arInvoiceId} not found.", nameof(arInvoiceId));

            var currentPaidAmount = invoice.PAID_AMOUNT ?? 0m;
            var newPaidAmount = currentPaidAmount + paymentAmount;
            var totalAmount = invoice.TOTAL_AMOUNT ?? 0m;
            var newBalanceDue = totalAmount - newPaidAmount;

            invoice.PAID_AMOUNT = newPaidAmount;
            invoice.BALANCE_DUE = newBalanceDue;

            // Update status based on balance
            if (newBalanceDue <= 0)
                invoice.STATUS = ReceivableStatus.Paid.ToString();
            else if (newPaidAmount > 0)
                invoice.STATUS = ReceivableStatus.PartiallyPaid.ToString();

            // Check if overdue
            if (invoice.DUE_DATE.HasValue && DateTime.Now > invoice.DUE_DATE.Value && newBalanceDue > 0)
                invoice.STATUS = ReceivableStatus.Overdue.ToString();

            invoice.ROW_CHANGED_BY = userId;
            invoice.ROW_CHANGED_DATE = DateTime.UtcNow;

            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var invoiceData = ConvertARInvoiceToDictionary(invoice);
            var result = dataSource.UpdateEntity(AR_INVOICE_TABLE, invoiceData);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to update AR invoice {InvoiceId}: {Error}", arInvoiceId, errorMessage);
                throw new InvalidOperationException($"Failed to update AR invoice: {errorMessage}");
            }

            _logger?.LogDebug("Recorded payment {PaymentAmount} for AR invoice {InvoiceId}", paymentAmount, arInvoiceId);
        }

        /// <summary>
        /// Gets all AR invoices for a customer (by customer BA ID).
        /// </summary>
        public IEnumerable<AR_INVOICE> GetARInvoicesByCustomer(string customerBaId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(customerBaId))
                return Enumerable.Empty<AR_INVOICE>();

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "CUSTOMER_BA_ID", Operator = "=", FilterValue = customerBaId }
            };

            var results = dataSource.GetEntityAsync(AR_INVOICE_TABLE, filters).GetAwaiter().GetResult();
            if (results == null || !results.Any())
                return Enumerable.Empty<AR_INVOICE>();

            return results.Cast<AR_INVOICE>().Where(inv => inv != null)!;
        }

        /// <summary>
        /// Gets overdue AR invoices.
        /// </summary>
        public IEnumerable<AR_INVOICE> GetOverdueARInvoices(string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var today = DateTime.Now;
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "DUE_DATE", Operator = "<", FilterValue = today.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "STATUS", Operator = "!=", FilterValue = ReceivableStatus.Paid.ToString() }
            };

            var results = dataSource.GetEntityAsync(AR_INVOICE_TABLE, filters).GetAwaiter().GetResult();
            if (results == null || !results.Any())
                return Enumerable.Empty<AR_INVOICE>();

            return results.OfType<Dictionary<string, object>>().Select(ConvertDictionaryToARInvoice)
                .Where(inv => inv != null && inv.BALANCE_DUE > 0 && inv.DUE_DATE.HasValue && DateTime.Now > inv.DUE_DATE.Value)!;
        }

        /// <summary>
        /// Gets total outstanding AR invoices balance.
        /// </summary>
        public decimal GetTotalOutstanding(string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "STATUS", Operator = "!=", FilterValue = ReceivableStatus.Paid.ToString() }
            };

            var results = dataSource.GetEntityAsync(AR_INVOICE_TABLE, filters).GetAwaiter().GetResult();
            if (results == null || !results.Any())
                return 0m;

            return results.OfType<Dictionary<string, object>>().Select(ConvertDictionaryToARInvoice)
                .Where(inv => inv != null)
                .Sum(inv => inv.BALANCE_DUE ?? 0m);
        }

        private Dictionary<string, object> ConvertARInvoiceToDictionary(AR_INVOICE invoice)
        {
            var dict = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(invoice.AR_INVOICE_ID)) dict["AR_INVOICE_ID"] = invoice.AR_INVOICE_ID;
            if (!string.IsNullOrEmpty(invoice.INVOICE_NUMBER)) dict["INVOICE_NUMBER"] = invoice.INVOICE_NUMBER;
            if (!string.IsNullOrEmpty(invoice.CUSTOMER_BA_ID)) dict["CUSTOMER_BA_ID"] = invoice.CUSTOMER_BA_ID;
            if (invoice.INVOICE_DATE.HasValue) dict["INVOICE_DATE"] = invoice.INVOICE_DATE.Value;
            if (invoice.DUE_DATE.HasValue) dict["DUE_DATE"] = invoice.DUE_DATE.Value;
            if (invoice.TOTAL_AMOUNT.HasValue) dict["TOTAL_AMOUNT"] = invoice.TOTAL_AMOUNT.Value;
            if (invoice.PAID_AMOUNT.HasValue) dict["PAID_AMOUNT"] = invoice.PAID_AMOUNT.Value;
            if (invoice.BALANCE_DUE.HasValue) dict["BALANCE_DUE"] = invoice.BALANCE_DUE.Value;
            if (!string.IsNullOrEmpty(invoice.STATUS)) dict["STATUS"] = invoice.STATUS;
            if (!string.IsNullOrEmpty(invoice.ACTIVE_IND)) dict["ACTIVE_IND"] = invoice.ACTIVE_IND;
            if (!string.IsNullOrEmpty(invoice.ROW_CREATED_BY)) dict["ROW_CREATED_BY"] = invoice.ROW_CREATED_BY;
            if (invoice.ROW_CREATED_DATE.HasValue) dict["ROW_CREATED_DATE"] = invoice.ROW_CREATED_DATE.Value;
            if (!string.IsNullOrEmpty(invoice.ROW_CHANGED_BY)) dict["ROW_CHANGED_BY"] = invoice.ROW_CHANGED_BY;
            if (invoice.ROW_CHANGED_DATE.HasValue) dict["ROW_CHANGED_DATE"] = invoice.ROW_CHANGED_DATE.Value;
            return dict;
        }

        private AR_INVOICE ConvertDictionaryToARInvoice(Dictionary<string, object> dict)
        {
            var invoice = new AR_INVOICE();
            if (dict.TryGetValue("AR_INVOICE_ID", out var invoiceId)) invoice.AR_INVOICE_ID = invoiceId?.ToString();
            if (dict.TryGetValue("INVOICE_NUMBER", out var invoiceNumber)) invoice.INVOICE_NUMBER = invoiceNumber?.ToString();
            if (dict.TryGetValue("CUSTOMER_BA_ID", out var customerBaId)) invoice.CUSTOMER_BA_ID = customerBaId?.ToString();
            if (dict.TryGetValue("INVOICE_DATE", out var invoiceDate)) invoice.INVOICE_DATE = invoiceDate != null ? Convert.ToDateTime(invoiceDate) : (DateTime?)null;
            if (dict.TryGetValue("DUE_DATE", out var dueDate)) invoice.DUE_DATE = dueDate != null ? Convert.ToDateTime(dueDate) : (DateTime?)null;
            if (dict.TryGetValue("TOTAL_AMOUNT", out var totalAmount)) invoice.TOTAL_AMOUNT = totalAmount != null ? Convert.ToDecimal(totalAmount) : (decimal?)null;
            if (dict.TryGetValue("PAID_AMOUNT", out var paidAmount)) invoice.PAID_AMOUNT = paidAmount != null ? Convert.ToDecimal(paidAmount) : (decimal?)null;
            if (dict.TryGetValue("BALANCE_DUE", out var balanceDue)) invoice.BALANCE_DUE = balanceDue != null ? Convert.ToDecimal(balanceDue) : (decimal?)null;
            if (dict.TryGetValue("STATUS", out var status)) invoice.STATUS = status?.ToString();
            if (dict.TryGetValue("ACTIVE_IND", out var activeInd)) invoice.ACTIVE_IND = activeInd?.ToString();
            if (dict.TryGetValue("ROW_CREATED_BY", out var createdBy)) invoice.ROW_CREATED_BY = createdBy?.ToString();
            if (dict.TryGetValue("ROW_CREATED_DATE", out var createdDate)) invoice.ROW_CREATED_DATE = createdDate != null ? Convert.ToDateTime(createdDate) : (DateTime?)null;
            if (dict.TryGetValue("ROW_CHANGED_BY", out var changedBy)) invoice.ROW_CHANGED_BY = changedBy?.ToString();
            if (dict.TryGetValue("ROW_CHANGED_DATE", out var changedDate)) invoice.ROW_CHANGED_DATE = changedDate != null ? Convert.ToDateTime(changedDate) : (DateTime?)null;
            return invoice;
        }
    }
}
