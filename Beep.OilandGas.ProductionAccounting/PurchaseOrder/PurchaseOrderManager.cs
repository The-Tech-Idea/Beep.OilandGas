
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
            ILogger<PurchaseOrderManager>? logger = null,
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

            var poData = ConvertPurchaseOrderToDictionary(po);
            var result = dataSource.InsertEntity(PURCHASE_ORDER_TABLE, poData);
            
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

        private Dictionary<string, object> ConvertPurchaseOrderToDictionary(PURCHASE_ORDER po)
        {
            var dict = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(po.PURCHASE_ORDER_ID)) dict["PURCHASE_ORDER_ID"] = po.PURCHASE_ORDER_ID;
            if (!string.IsNullOrEmpty(po.PO_NUMBER)) dict["PO_NUMBER"] = po.PO_NUMBER;
            if (!string.IsNullOrEmpty(po.VENDOR_BA_ID)) dict["VENDOR_BA_ID"] = po.VENDOR_BA_ID;
            if (po.PO_DATE.HasValue) dict["PO_DATE"] = po.PO_DATE.Value;
            if (po.EXPECTED_DELIVERY_DATE.HasValue) dict["EXPECTED_DELIVERY_DATE"] = po.EXPECTED_DELIVERY_DATE.Value;
            if (!string.IsNullOrEmpty(po.STATUS)) dict["STATUS"] = po.STATUS;
            if (!string.IsNullOrEmpty(po.DESCRIPTION)) dict["DESCRIPTION"] = po.DESCRIPTION;
            if (!string.IsNullOrEmpty(po.ACTIVE_IND)) dict["ACTIVE_IND"] = po.ACTIVE_IND;
            if (!string.IsNullOrEmpty(po.ROW_CREATED_BY)) dict["ROW_CREATED_BY"] = po.ROW_CREATED_BY;
            if (po.ROW_CREATED_DATE.HasValue) dict["ROW_CREATED_DATE"] = po.ROW_CREATED_DATE.Value;
            if (!string.IsNullOrEmpty(po.ROW_CHANGED_BY)) dict["ROW_CHANGED_BY"] = po.ROW_CHANGED_BY;
            if (po.ROW_CHANGED_DATE.HasValue) dict["ROW_CHANGED_DATE"] = po.ROW_CHANGED_DATE.Value;
            return dict;
        }

        private PURCHASE_ORDER ConvertDictionaryToPurchaseOrder(Dictionary<string, object> dict)
        {
            var po = new PURCHASE_ORDER();
            if (dict.TryGetValue("PURCHASE_ORDER_ID", out var poId)) po.PURCHASE_ORDER_ID = poId?.ToString();
            if (dict.TryGetValue("PO_NUMBER", out var poNumber)) po.PO_NUMBER = poNumber?.ToString();
            if (dict.TryGetValue("VENDOR_BA_ID", out var vendorBaId)) po.VENDOR_BA_ID = vendorBaId?.ToString();
            if (dict.TryGetValue("PO_DATE", out var poDate)) po.PO_DATE = poDate != null ? Convert.ToDateTime(poDate) : (DateTime?)null;
            if (dict.TryGetValue("EXPECTED_DELIVERY_DATE", out var expectedDate)) po.EXPECTED_DELIVERY_DATE = expectedDate != null ? Convert.ToDateTime(expectedDate) : (DateTime?)null;
            if (dict.TryGetValue("STATUS", out var status)) po.STATUS = status?.ToString();
            if (dict.TryGetValue("DESCRIPTION", out var description)) po.DESCRIPTION = description?.ToString();
            if (dict.TryGetValue("ACTIVE_IND", out var activeInd)) po.ACTIVE_IND = activeInd?.ToString();
            if (dict.TryGetValue("ROW_CREATED_BY", out var createdBy)) po.ROW_CREATED_BY = createdBy?.ToString();
            if (dict.TryGetValue("ROW_CREATED_DATE", out var createdDate)) po.ROW_CREATED_DATE = createdDate != null ? Convert.ToDateTime(createdDate) : (DateTime?)null;
            if (dict.TryGetValue("ROW_CHANGED_BY", out var changedBy)) po.ROW_CHANGED_BY = changedBy?.ToString();
            if (dict.TryGetValue("ROW_CHANGED_DATE", out var changedDate)) po.ROW_CHANGED_DATE = changedDate != null ? Convert.ToDateTime(changedDate) : (DateTime?)null;
            return po;
        }
    }
}
