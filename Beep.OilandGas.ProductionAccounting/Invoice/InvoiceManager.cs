using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.DTOs.Accounting;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ProductionAccounting.Invoice
{
    /// <summary>
    /// Manages customer invoices.
    /// Uses database access via IDataSource instead of in-memory dictionaries.
    /// </summary>
    public class InvoiceManager
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<InvoiceManager>? _logger;
        private readonly string _connectionName;
        private const string INVOICE_TABLE = "INVOICE";

        public InvoiceManager(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILoggerFactory loggerFactory,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = loggerFactory?.CreateLogger<InvoiceManager>();
            _connectionName = connectionName ?? "PPDM39";
        }

        /// <summary>
        /// Creates a new invoice.
        /// </summary>
        public INVOICE CreateInvoice(CreateInvoiceRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

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
                STATUS = "Draft",
                CURRENCY_CODE = request.CurrencyCode,
                DESCRIPTION = request.Description,
                ACTIVE_IND = "Y",
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var result = dataSource.InsertEntity(INVOICE_TABLE, invoice);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to create invoice {InvoiceNumber}: {Error}", request.InvoiceNumber, errorMessage);
                throw new InvalidOperationException($"Failed to save invoice: {errorMessage}");
            }

            _logger?.LogDebug("Created invoice {InvoiceNumber} in database", request.InvoiceNumber);
            return invoice;
        }

        /// <summary>
        /// Gets an invoice by ID.
        /// </summary>
        public INVOICE? GetInvoice(string invoiceId, string? connectionName = null)
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

            var results = dataSource.GetEntityAsync(INVOICE_TABLE, filters).GetAwaiter().GetResult();
            var invoiceData = results?.FirstOrDefault();
            
            if (invoiceData == null)
                return null;

            return invoiceData as INVOICE;
        }

        private Dictionary<string, object> ConvertInvoiceToDictionary(INVOICE invoice)
        {
            var dict = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(invoice.INVOICE_ID)) dict["INVOICE_ID"] = invoice.INVOICE_ID;
            if (!string.IsNullOrEmpty(invoice.INVOICE_NUMBER)) dict["INVOICE_NUMBER"] = invoice.INVOICE_NUMBER;
            if (!string.IsNullOrEmpty(invoice.CUSTOMER_BA_ID)) dict["CUSTOMER_BA_ID"] = invoice.CUSTOMER_BA_ID;
            if (invoice.INVOICE_DATE.HasValue) dict["INVOICE_DATE"] = invoice.INVOICE_DATE.Value;
            if (invoice.DUE_DATE.HasValue) dict["DUE_DATE"] = invoice.DUE_DATE.Value;
            if (invoice.SUBTOTAL.HasValue) dict["SUBTOTAL"] = invoice.SUBTOTAL.Value;
            if (invoice.TAX_AMOUNT.HasValue) dict["TAX_AMOUNT"] = invoice.TAX_AMOUNT.Value;
            if (invoice.TOTAL_AMOUNT.HasValue) dict["TOTAL_AMOUNT"] = invoice.TOTAL_AMOUNT.Value;
            if (invoice.PAID_AMOUNT.HasValue) dict["PAID_AMOUNT"] = invoice.PAID_AMOUNT.Value;
            if (invoice.BALANCE_DUE.HasValue) dict["BALANCE_DUE"] = invoice.BALANCE_DUE.Value;
            if (!string.IsNullOrEmpty(invoice.STATUS)) dict["STATUS"] = invoice.STATUS;
            if (!string.IsNullOrEmpty(invoice.CURRENCY_CODE)) dict["CURRENCY_CODE"] = invoice.CURRENCY_CODE;
            if (!string.IsNullOrEmpty(invoice.DESCRIPTION)) dict["DESCRIPTION"] = invoice.DESCRIPTION;
            if (!string.IsNullOrEmpty(invoice.ACTIVE_IND)) dict["ACTIVE_IND"] = invoice.ACTIVE_IND;
            if (!string.IsNullOrEmpty(invoice.ROW_CREATED_BY)) dict["ROW_CREATED_BY"] = invoice.ROW_CREATED_BY;
            if (invoice.ROW_CREATED_DATE.HasValue) dict["ROW_CREATED_DATE"] = invoice.ROW_CREATED_DATE.Value;
            if (!string.IsNullOrEmpty(invoice.ROW_CHANGED_BY)) dict["ROW_CHANGED_BY"] = invoice.ROW_CHANGED_BY;
            if (invoice.ROW_CHANGED_DATE.HasValue) dict["ROW_CHANGED_DATE"] = invoice.ROW_CHANGED_DATE.Value;
            return dict;
        }

        private INVOICE ConvertDictionaryToInvoice(Dictionary<string, object> dict)
        {
            var invoice = new INVOICE();
            if (dict.TryGetValue("INVOICE_ID", out var invoiceId)) invoice.INVOICE_ID = invoiceId?.ToString();
            if (dict.TryGetValue("INVOICE_NUMBER", out var invoiceNumber)) invoice.INVOICE_NUMBER = invoiceNumber?.ToString();
            if (dict.TryGetValue("CUSTOMER_BA_ID", out var customerBaId)) invoice.CUSTOMER_BA_ID = customerBaId?.ToString();
            if (dict.TryGetValue("INVOICE_DATE", out var invoiceDate)) invoice.INVOICE_DATE = invoiceDate != null ? Convert.ToDateTime(invoiceDate) : (DateTime?)null;
            if (dict.TryGetValue("DUE_DATE", out var dueDate)) invoice.DUE_DATE = dueDate != null ? Convert.ToDateTime(dueDate) : (DateTime?)null;
            if (dict.TryGetValue("SUBTOTAL", out var subtotal)) invoice.SUBTOTAL = subtotal != null ? Convert.ToDecimal(subtotal) : (decimal?)null;
            if (dict.TryGetValue("TAX_AMOUNT", out var taxAmount)) invoice.TAX_AMOUNT = taxAmount != null ? Convert.ToDecimal(taxAmount) : (decimal?)null;
            if (dict.TryGetValue("TOTAL_AMOUNT", out var totalAmount)) invoice.TOTAL_AMOUNT = totalAmount != null ? Convert.ToDecimal(totalAmount) : (decimal?)null;
            if (dict.TryGetValue("PAID_AMOUNT", out var paidAmount)) invoice.PAID_AMOUNT = paidAmount != null ? Convert.ToDecimal(paidAmount) : (decimal?)null;
            if (dict.TryGetValue("BALANCE_DUE", out var balanceDue)) invoice.BALANCE_DUE = balanceDue != null ? Convert.ToDecimal(balanceDue) : (decimal?)null;
            if (dict.TryGetValue("STATUS", out var status)) invoice.STATUS = status?.ToString();
            if (dict.TryGetValue("CURRENCY_CODE", out var currencyCode)) invoice.CURRENCY_CODE = currencyCode?.ToString();
            if (dict.TryGetValue("DESCRIPTION", out var description)) invoice.DESCRIPTION = description?.ToString();
            if (dict.TryGetValue("ACTIVE_IND", out var activeInd)) invoice.ACTIVE_IND = activeInd?.ToString();
            if (dict.TryGetValue("ROW_CREATED_BY", out var createdBy)) invoice.ROW_CREATED_BY = createdBy?.ToString();
            if (dict.TryGetValue("ROW_CREATED_DATE", out var createdDate)) invoice.ROW_CREATED_DATE = createdDate != null ? Convert.ToDateTime(createdDate) : (DateTime?)null;
            if (dict.TryGetValue("ROW_CHANGED_BY", out var changedBy)) invoice.ROW_CHANGED_BY = changedBy?.ToString();
            if (dict.TryGetValue("ROW_CHANGED_DATE", out var changedDate)) invoice.ROW_CHANGED_DATE = changedDate != null ? Convert.ToDateTime(changedDate) : (DateTime?)null;
            return invoice;
        }
    }
}

