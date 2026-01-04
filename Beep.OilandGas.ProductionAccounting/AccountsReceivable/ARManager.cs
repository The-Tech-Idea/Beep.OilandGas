
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
    /// Uses Entity classes directly with IDataSource - no dictionary conversions.
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
                STATUS = ReceivableStatus.Open.ToString()
            };

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            _commonColumnHandler.PrepareForInsert(invoice, userId);
            var result = dataSource.InsertEntity(AR_INVOICE_TABLE, invoice);
            
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
            return results?.FirstOrDefault() as AR_INVOICE;
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

            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            _commonColumnHandler.PrepareForUpdate(invoice, userId);
            var result = dataSource.UpdateEntity(AR_INVOICE_TABLE, invoice);
            
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

            return results.Cast<AR_INVOICE>()
                .Where(inv => inv != null && (inv.BALANCE_DUE ?? 0m) > 0 && inv.DUE_DATE.HasValue && DateTime.Now > inv.DUE_DATE.Value)!;
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

            return results.Cast<AR_INVOICE>()
                .Where(inv => inv != null)
                .Sum(inv => inv.BALANCE_DUE ?? 0m);
        }
    }
}
