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

namespace Beep.OilandGas.ProductionAccounting.PurchaseOrder
{
    /// <summary>
    /// Manages purchase orders.
    /// Uses database access via IDataSource instead of in-memory dictionaries.
    /// </summary>
    public class PurchaseOrderManager
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<PurchaseOrderManager>? _logger;
        private readonly string _connectionName;
        private const string PURCHASE_ORDER_TABLE = "PURCHASE_ORDER";

        public PurchaseOrderManager(
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
            _logger = loggerFactory?.CreateLogger<PurchaseOrderManager>();
            _connectionName = connectionName ?? "PPDM39";
        }

        /// <summary>
        /// Creates a new purchase order.
        /// </summary>
        public PURCHASE_ORDER CreatePurchaseOrder(CreatePurchaseOrderRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var po = new PURCHASE_ORDER
            {
                PURCHASE_ORDER_ID = Guid.NewGuid().ToString(),
                PO_NUMBER = request.PoNumber,
                VENDOR_BA_ID = request.VendorBaId,
                PO_DATE = request.PoDate,
                EXPECTED_DELIVERY_DATE = request.ExpectedDeliveryDate,
                STATUS = "Draft",
                DESCRIPTION = request.Description,
                ACTIVE_IND = "Y",
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

      
            var result = dataSource.InsertEntity(PURCHASE_ORDER_TABLE, po);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to create purchase order {PoNumber}: {Error}", request.PoNumber, errorMessage);
                throw new InvalidOperationException($"Failed to save purchase order: {errorMessage}");
            }

            _logger?.LogDebug("Created purchase order {PoNumber} in database", request.PoNumber);
            return po;
        }

        /// <summary>
        /// Gets a purchase order by ID.
        /// </summary>
        public PURCHASE_ORDER? GetPurchaseOrder(string purchaseOrderId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(purchaseOrderId))
                return null;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PURCHASE_ORDER_ID", Operator = "=", FilterValue = purchaseOrderId }
            };

            var results = dataSource.GetEntityAsync(PURCHASE_ORDER_TABLE, filters).GetAwaiter().GetResult();
            var poData = results?.FirstOrDefault();
            
            if (poData == null)
                return null;

            return poData as PURCHASE_ORDER;
        }

     
    }
}

