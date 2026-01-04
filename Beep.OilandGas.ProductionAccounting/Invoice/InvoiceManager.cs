
using Beep.OilandGas.Models.DTOs.Accounting;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.ProductionAccounting.Invoice
{
    /// <summary>
    /// Manages customer invoices.
    /// Uses Entity classes directly with IDataSource - no dictionary conversions.
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
            ILogger<InvoiceManager>? logger = null,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
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
                DESCRIPTION = request.Description
            };

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            _commonColumnHandler.PrepareForInsert(invoice, userId);
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
    }
}
