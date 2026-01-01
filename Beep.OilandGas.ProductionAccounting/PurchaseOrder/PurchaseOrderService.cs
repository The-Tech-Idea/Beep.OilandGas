using Beep.OilandGas.Models.DTOs.Accounting;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.ProductionAccounting.PurchaseOrder
{
    /// <summary>
    /// Service for managing purchase orders.
    /// Uses PPDMGenericRepository for database operations.
    /// </summary>
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<PurchaseOrderService>? _logger;
        private readonly string _connectionName;
        private const string PURCHASE_ORDER_TABLE = "PURCHASE_ORDER";
        private const string PO_LINE_ITEM_TABLE = "PO_LINE_ITEM";
        private const string PO_RECEIPT_TABLE = "PO_RECEIPT";

        public PurchaseOrderService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<PurchaseOrderService>? logger = null,
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
        public async Task<PURCHASE_ORDER> CreatePurchaseOrderAsync(CreatePurchaseOrderRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var connName = connectionName ?? _connectionName;
            var repo = await GetPORepositoryAsync(connName);

            var po = new PURCHASE_ORDER
            {
                PURCHASE_ORDER_ID = Guid.NewGuid().ToString(),
                PO_NUMBER = request.PoNumber,
                VENDOR_BA_ID = request.VendorBaId,
                PO_DATE = request.PoDate,
                EXPECTED_DELIVERY_DATE = request.ExpectedDeliveryDate,
                STATUS = "Draft",
                DESCRIPTION = request.Description,
                ACTIVE_IND = "Y"
            };

            if (po is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForCreateAsync(ppdmEntity, userId, connName);
            }

            await repo.InsertAsync(po);
            _logger?.LogDebug("Created purchase order {PoNumber}", request.PoNumber);

            return po;
        }

        /// <summary>
        /// Gets a purchase order by ID.
        /// </summary>
        public async Task<PURCHASE_ORDER?> GetPurchaseOrderAsync(string poId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(poId))
                return null;

            var connName = connectionName ?? _connectionName;
            var repo = await GetPORepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PURCHASE_ORDER_ID", Operator = "=", FilterValue = poId }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<PURCHASE_ORDER>().FirstOrDefault();
        }

        /// <summary>
        /// Gets purchase orders by vendor.
        /// </summary>
        public async Task<List<PURCHASE_ORDER>> GetPurchaseOrdersByVendorAsync(string vendorId, DateTime? startDate, DateTime? endDate, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(vendorId))
                return new List<PURCHASE_ORDER>();

            var connName = connectionName ?? _connectionName;
            var repo = await GetPORepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "VENDOR_BA_ID", Operator = "=", FilterValue = vendorId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (startDate.HasValue)
                filters.Add(new AppFilter { FieldName = "PO_DATE", Operator = ">=", FilterValue = startDate.Value });
            if (endDate.HasValue)
                filters.Add(new AppFilter { FieldName = "PO_DATE", Operator = "<=", FilterValue = endDate.Value });

            var results = await repo.GetAsync(filters);
            return results.Cast<PURCHASE_ORDER>().OrderByDescending(po => po.PO_DATE).ToList();
        }

        /// <summary>
        /// Updates a purchase order.
        /// </summary>
        public async Task<PURCHASE_ORDER> UpdatePurchaseOrderAsync(UpdatePurchaseOrderRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.PurchaseOrderId))
                throw new ArgumentException("Purchase Order ID is required.", nameof(request));

            var connName = connectionName ?? _connectionName;
            var po = await GetPurchaseOrderAsync(request.PurchaseOrderId, connName);

            if (po == null)
                throw new InvalidOperationException($"Purchase order {request.PurchaseOrderId} not found.");

            // Update properties
            po.PO_NUMBER = request.PoNumber;
            po.VENDOR_BA_ID = request.VendorBaId;
            po.PO_DATE = request.PoDate;
            po.EXPECTED_DELIVERY_DATE = request.ExpectedDeliveryDate;
            po.DESCRIPTION = request.Description;
            po.STATUS = request.Status;

            if (po is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForUpdateAsync(ppdmEntity, userId, connName);
            }

            var repo = await GetPORepositoryAsync(connName);
            await repo.UpdateAsync(po);

            _logger?.LogDebug("Updated purchase order {PoId}", request.PurchaseOrderId);

            return po;
        }

        /// <summary>
        /// Deletes a purchase order (soft delete by setting ACTIVE_IND = 'N').
        /// </summary>
        public async Task<bool> DeletePurchaseOrderAsync(string poId, string userId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(poId))
                return false;

            var connName = connectionName ?? _connectionName;
            var po = await GetPurchaseOrderAsync(poId, connName);

            if (po == null)
                return false;

            po.ACTIVE_IND = "N";

            if (po is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForUpdateAsync(ppdmEntity, userId, connName);
            }

            var repo = await GetPORepositoryAsync(connName);
            await repo.UpdateAsync(po);

            _logger?.LogDebug("Deleted purchase order {PoId}", poId);

            return true;
        }

        /// <summary>
        /// Gets purchase order line items.
        /// </summary>
        public async Task<List<PO_LINE_ITEM>> GetPOLineItemsAsync(string poId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(poId))
                return new List<PO_LINE_ITEM>();

            var connName = connectionName ?? _connectionName;
            var repo = await GetLineItemRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PURCHASE_ORDER_ID", Operator = "=", FilterValue = poId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<PO_LINE_ITEM>().OrderBy(l => l.LINE_NUMBER).ToList();
        }

        /// <summary>
        /// Creates a PO receipt.
        /// </summary>
        public async Task<PO_RECEIPT> CreateReceiptAsync(CreatePOReceiptRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.PurchaseOrderId))
                throw new ArgumentException("Purchase Order ID is required.", nameof(request));

            var connName = connectionName ?? _connectionName;
            var po = await GetPurchaseOrderAsync(request.PurchaseOrderId, connName);

            if (po == null)
                throw new InvalidOperationException($"Purchase order {request.PurchaseOrderId} not found.");

            var receiptRepo = await GetReceiptRepositoryAsync(connName);

            var receipt = new PO_RECEIPT
            {
                PO_RECEIPT_ID = Guid.NewGuid().ToString(),
                PURCHASE_ORDER_ID = request.PurchaseOrderId,
                PO_LINE_ITEM_ID = request.PoLineItemId,
                RECEIPT_DATE = request.ReceiptDate,
                RECEIVED_QUANTITY = request.QuantityReceived,
                RECEIVED_BY = userId,
                RECEIPT_STATUS = "Received",
                DESCRIPTION = request.Description,
                ACTIVE_IND = "Y"
            };

            if (receipt is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForCreateAsync(ppdmEntity, userId, connName);
            }

            await receiptRepo.InsertAsync(receipt);

            // Update PO status if needed
            var receipts = await GetReceiptsByPOAsync(request.PurchaseOrderId, connName);
            var lineItems = await GetPOLineItemsAsync(request.PurchaseOrderId, connName);

            // Check if all line items are fully received
            bool allReceived = lineItems.All(li =>
            {
                var lineReceipts = receipts.Where(r => r.PO_LINE_ITEM_ID == li.PO_LINE_ITEM_ID);
                decimal totalReceived = lineReceipts.Sum(r => r.RECEIVED_QUANTITY ?? 0m);
                return totalReceived >= (li.QUANTITY ?? 0m);
            });

            if (allReceived && lineItems.Any())
            {
                po.STATUS = "Received";

                if (po is IPPDMEntity poPpdmEntity)
                {
                    await _commonColumnHandler.SetCommonColumnsForUpdateAsync(poPpdmEntity, userId, connName);
                }

                var poRepo = await GetPORepositoryAsync(connName);
                await poRepo.UpdateAsync(po);
            }

            _logger?.LogDebug("Created receipt for purchase order {PoId}", request.PurchaseOrderId);

            return receipt;
        }

        /// <summary>
        /// Gets receipts for a purchase order.
        /// </summary>
        public async Task<List<PO_RECEIPT>> GetReceiptsByPOAsync(string poId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(poId))
                return new List<PO_RECEIPT>();

            var connName = connectionName ?? _connectionName;
            var repo = await GetReceiptRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PURCHASE_ORDER_ID", Operator = "=", FilterValue = poId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<PO_RECEIPT>().OrderBy(r => r.RECEIPT_DATE).ToList();
        }

        /// <summary>
        /// Approves a purchase order.
        /// </summary>
        public async Task<POApprovalResult> ApprovePurchaseOrderAsync(string poId, string approverId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(poId))
                throw new ArgumentException("Purchase Order ID is required.", nameof(poId));

            var connName = connectionName ?? _connectionName;
            var po = await GetPurchaseOrderAsync(poId, connName);

            if (po == null)
                throw new InvalidOperationException($"Purchase order {poId} not found.");

            po.STATUS = "Approved";

            if (po is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForUpdateAsync(ppdmEntity, approverId, connName);
            }

            var repo = await GetPORepositoryAsync(connName);
            await repo.UpdateAsync(po);

            return new POApprovalResult
            {
                PurchaseOrderId = poId,
                IsApproved = true,
                ApproverId = approverId,
                ApprovalDate = DateTime.UtcNow,
                Status = "Approved"
            };
        }

        /// <summary>
        /// Gets purchase order status summary.
        /// </summary>
        public async Task<POStatusSummary> GetPOStatusAsync(string poId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(poId))
                throw new ArgumentException("Purchase Order ID is required.", nameof(poId));

            var connName = connectionName ?? _connectionName;
            var po = await GetPurchaseOrderAsync(poId, connName);

            if (po == null)
                throw new InvalidOperationException($"Purchase order {poId} not found.");

            var lineItems = await GetPOLineItemsAsync(poId, connName);
            var receipts = await GetReceiptsByPOAsync(poId, connName);

            decimal totalAmount = lineItems.Sum(li => li.LINE_TOTAL ?? 0m);
            decimal receivedAmount = receipts.Sum(r =>
            {
                var lineItem = lineItems.FirstOrDefault(li => li.PO_LINE_ITEM_ID == r.PO_LINE_ITEM_ID);
                if (lineItem != null)
                {
                    return (r.RECEIVED_QUANTITY ?? 0m) * (lineItem.UNIT_PRICE ?? 0m);
                }
                return 0m;
            });

            decimal pendingAmount = totalAmount - receivedAmount;
            int totalLineItems = lineItems.Count;

            int receivedLineItems = lineItems.Count(li =>
            {
                var lineReceipts = receipts.Where(r => r.PO_LINE_ITEM_ID == li.PO_LINE_ITEM_ID);
                decimal totalReceived = lineReceipts.Sum(r => r.RECEIVED_QUANTITY ?? 0m);
                return totalReceived >= (li.QUANTITY ?? 0m);
            });

            int pendingLineItems = totalLineItems - receivedLineItems;
            bool isFullyReceived = receivedLineItems == totalLineItems && totalLineItems > 0;
            bool isApproved = po.STATUS == "Approved";

            return new POStatusSummary
            {
                PurchaseOrderId = poId,
                PoNumber = po.PO_NUMBER ?? string.Empty,
                Status = po.STATUS ?? string.Empty,
                TotalAmount = totalAmount,
                ReceivedAmount = receivedAmount,
                PendingAmount = pendingAmount,
                TotalLineItems = totalLineItems,
                ReceivedLineItems = receivedLineItems,
                PendingLineItems = pendingLineItems,
                IsFullyReceived = isFullyReceived,
                IsApproved = isApproved,
                ApprovalDate = isApproved ? po.ROW_CHANGED_DATE : null
            };
        }

        /// <summary>
        /// Gets purchase orders requiring approval.
        /// </summary>
        public async Task<List<PURCHASE_ORDER>> GetPOsRequiringApprovalAsync(string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var repo = await GetPORepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "STATUS", Operator = "=", FilterValue = "Draft" },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<PURCHASE_ORDER>().OrderBy(po => po.PO_DATE).ToList();
        }

        // Repository helper methods
        private async Task<PPDMGenericRepository> GetPORepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(PURCHASE_ORDER_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(PURCHASE_ORDER);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, PURCHASE_ORDER_TABLE,
                null);
        }

        private async Task<PPDMGenericRepository> GetLineItemRepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(PO_LINE_ITEM_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(PO_LINE_ITEM);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, PO_LINE_ITEM_TABLE,
                null);
        }

        private async Task<PPDMGenericRepository> GetReceiptRepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(PO_RECEIPT_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(PO_RECEIPT);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, PO_RECEIPT_TABLE,
                null);
        }
    }
}
