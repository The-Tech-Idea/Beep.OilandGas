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

namespace Beep.OilandGas.ProductionAccounting.AccountsPayable
{
    /// <summary>
    /// Manages accounts payable invoices.
    /// Uses database access via IDataSource instead of in-memory dictionaries.
    /// </summary>
    public class APManager
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<APManager>? _logger;
        private readonly string _connectionName;
        private const string AP_INVOICE_TABLE = "AP_INVOICE";

        public APManager(
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
            _logger = loggerFactory?.CreateLogger<APManager>();
            _connectionName = connectionName ?? "PPDM39";
        }

        /// <summary>
        /// Creates a new AP invoice.
        /// </summary>
        public AP_INVOICE CreateAPInvoice(CreateAPInvoiceRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var invoice = new AP_INVOICE
            {
                AP_INVOICE_ID = Guid.NewGuid().ToString(),
                INVOICE_NUMBER = request.InvoiceNumber,
                VENDOR_BA_ID = request.VendorBaId,
                INVOICE_DATE = request.InvoiceDate,
                DUE_DATE = request.DueDate,
                TOTAL_AMOUNT = request.TotalAmount,
                BALANCE_DUE = request.TotalAmount,
                STATUS = "Open",
                ACTIVE_IND = "Y",
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var result = dataSource.InsertEntity(AP_INVOICE_TABLE, invoice);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to create AP invoice {InvoiceNumber}: {Error}", request.InvoiceNumber, errorMessage);
                throw new InvalidOperationException($"Failed to save AP invoice: {errorMessage}");
            }

            _logger?.LogDebug("Created AP invoice {InvoiceNumber} in database", request.InvoiceNumber);
            return invoice;
        }

        /// <summary>
        /// Gets an AP invoice by ID.
        /// </summary>
        public AP_INVOICE? GetAPInvoice(string apInvoiceId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(apInvoiceId))
                return null;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "AP_INVOICE_ID", Operator = "=", FilterValue = apInvoiceId }
            };

            var results = dataSource.GetEntityAsync(AP_INVOICE_TABLE, filters).GetAwaiter().GetResult();
            var invoiceData = results?.FirstOrDefault();
            
            if (invoiceData == null)
                return null;

            return invoiceData as AP_INVOICE;
        }

    }
}

