using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;

namespace Beep.OilandGas.Accounting.Services
{
    /// <summary>
    /// Purchase Order Service
    /// Manages purchase orders, goods receipt, and 3-way matching
    /// Uses: PURCHASE_ORDER, PO_RECEIPT entities
    /// </summary>
    public class PurchaseOrderService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<PurchaseOrderService> _logger;
        private const string ConnectionName = "PPDM39";

        public PurchaseOrderService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<PurchaseOrderService> logger)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Create a new purchase order (DRAFT status)
        /// </summary>
        public async Task<PURCHASE_ORDER> CreatePOAsync(
            string vendorBaId,
            decimal totalAmount,
            DateTime poDate,
            DateTime? expectedDeliveryDate = null,
            string? poNumber = null,
            string? description = null,
            string userId = "SYSTEM")
        {
            if (string.IsNullOrWhiteSpace(vendorBaId))
                throw new ArgumentNullException(nameof(vendorBaId));
            if (totalAmount <= 0)
                throw new ArgumentException("PO amount must be greater than zero", nameof(totalAmount));

            _logger?.LogInformation("Creating PO for vendor {VendorId}, amount {Amount:C}", vendorBaId, totalAmount);

            try
            {
                var po = new PURCHASE_ORDER
                {
                    PURCHASE_ORDER_ID = Guid.NewGuid().ToString(),
                    PO_NUMBER = poNumber ?? await GeneratePONumberAsync(),
                    VENDOR_BA_ID = vendorBaId,
                    PO_DATE = poDate,
                    EXPECTED_DELIVERY_DATE = expectedDeliveryDate,
                    TOTAL_AMOUNT = totalAmount,
                    STATUS = "DRAFT",
                    DESCRIPTION = description,
                    ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                    PPDM_GUID = Guid.NewGuid().ToString(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                var repo = await GetRepoAsync<PURCHASE_ORDER>("PURCHASE_ORDER", ConnectionName);

                await repo.InsertAsync(po, userId);
                _logger?.LogInformation("PO {PONumber} created (DRAFT)", po.PO_NUMBER);
                return po;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating PO: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Approve a purchase order (DRAFT -> APPROVED)
        /// </summary>
        public async Task<PURCHASE_ORDER> ApprovePOAsync(string poId, string userId = "SYSTEM")
        {
            if (string.IsNullOrWhiteSpace(poId))
                throw new ArgumentNullException(nameof(poId));

            _logger?.LogInformation("Approving PO {POId}", poId);

            try
            {
                var po = await GetPOByIdAsync(poId);
                if (po == null)
                    throw new InvalidOperationException($"PO {poId} not found");

                if (po.STATUS != "DRAFT")
                    throw new InvalidOperationException($"Only DRAFT POs can be approved (current: {po.STATUS})");

                po.STATUS = "APPROVED";
                po.ROW_CHANGED_BY = userId;
                po.ROW_CHANGED_DATE = DateTime.UtcNow;

                var repo = await GetRepoAsync<PURCHASE_ORDER>("PURCHASE_ORDER", ConnectionName);

                await repo.UpdateAsync(po, userId);
                _logger?.LogInformation("PO {PONumber} approved", po.PO_NUMBER);
                return po;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error approving PO {POId}: {Message}", poId, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Record goods receipt for a PO line item (creates PO_RECEIPT)
        /// </summary>
        public async Task<PO_RECEIPT> ReceiveGoodsAsync(
            string poLineItemId,
            decimal receivedQuantity,
            DateTime receiptDate,
            string receivedBy,
            string? description = null,
            string userId = "SYSTEM")
        {
            if (string.IsNullOrWhiteSpace(poLineItemId))
                throw new ArgumentNullException(nameof(poLineItemId));
            if (receivedQuantity <= 0)
                throw new ArgumentException("Received quantity must be greater than zero", nameof(receivedQuantity));

            _logger?.LogInformation("Recording goods receipt for PO line {LineId}: Qty {Qty}",
                poLineItemId, receivedQuantity);

            try
            {
                var lineItem = await GetPoLineItemAsync(poLineItemId);
                if (lineItem == null || string.IsNullOrWhiteSpace(lineItem.PURCHASE_ORDER_ID))
                    throw new InvalidOperationException($"PO line item {poLineItemId} not found or missing PO reference");

                var receipt = new PO_RECEIPT
                {
                    PO_RECEIPT_ID = Guid.NewGuid().ToString(),
                    PURCHASE_ORDER_ID = lineItem.PURCHASE_ORDER_ID,
                    PO_LINE_ITEM_ID = poLineItemId,
                    RECEIPT_DATE = receiptDate,
                    RECEIVED_QUANTITY = receivedQuantity,
                    RECEIVED_BY = receivedBy,
                    RECEIPT_STATUS = "RECEIVED",
                    DESCRIPTION = description,
                    ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                    PPDM_GUID = Guid.NewGuid().ToString(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                var repo = await GetRepoAsync<PO_RECEIPT>("PO_RECEIPT", ConnectionName);

                await repo.InsertAsync(receipt, userId);

                _logger?.LogInformation("Goods receipt recorded for PO line {LineId}: Qty {Qty}",
                    poLineItemId, receivedQuantity);

                return receipt;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error recording goods receipt for PO line {LineId}: {Message}", poLineItemId, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get PO by ID
        /// </summary>
        public async Task<PURCHASE_ORDER?> GetPOByIdAsync(string poId)
        {
            if (string.IsNullOrWhiteSpace(poId))
                return null;

            try
            {
                var repo = await GetRepoAsync<PURCHASE_ORDER>("PURCHASE_ORDER", ConnectionName);

                var po = await repo.GetByIdAsync(poId);
                return po as PURCHASE_ORDER;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting PO {POId}", poId);
                return null;
            }
        }

        /// <summary>
        /// Get all POs for a vendor
        /// </summary>
        public async Task<List<PURCHASE_ORDER>> GetVendorPOsAsync(string vendorBaId)
        {
            if (string.IsNullOrWhiteSpace(vendorBaId))
                return new List<PURCHASE_ORDER>();

            try
            {
                var repo = await GetRepoAsync<PURCHASE_ORDER>("PURCHASE_ORDER", ConnectionName);

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "VENDOR_BA_ID", Operator = "=", FilterValue = vendorBaId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var pos = await repo.GetAsync(filters);
                return pos?.Cast<PURCHASE_ORDER>().ToList() ?? new List<PURCHASE_ORDER>();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting POs for vendor {VendorId}", vendorBaId);
                return new List<PURCHASE_ORDER>();
            }
        }

        /// <summary>
        /// Get all receipts for a PO
        /// </summary>
        public async Task<List<PO_RECEIPT>> GetReceiptsForPOAsync(string poId)
        {
            if (string.IsNullOrWhiteSpace(poId))
                return new List<PO_RECEIPT>();

            try
            {
                var repo = await GetRepoAsync<PO_RECEIPT>("PO_RECEIPT", ConnectionName);

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "PURCHASE_ORDER_ID", Operator = "=", FilterValue = poId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var receipts = await repo.GetAsync(filters);
                return receipts?.Cast<PO_RECEIPT>().ToList() ?? new List<PO_RECEIPT>();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting receipts for PO {POId}", poId);
                return new List<PO_RECEIPT>();
            }
        }

        /// <summary>
        /// Close a PO (mark as CLOSED)
        /// </summary>
        public async Task<PURCHASE_ORDER> ClosePOAsync(string poId, string userId = "SYSTEM")
        {
            if (string.IsNullOrWhiteSpace(poId))
                throw new ArgumentNullException(nameof(poId));

            try
            {
                var po = await GetPOByIdAsync(poId);
                if (po == null)
                    throw new InvalidOperationException($"PO {poId} not found");

                po.STATUS = "CLOSED";
                po.ROW_CHANGED_BY = userId;
                po.ROW_CHANGED_DATE = DateTime.UtcNow;

                var repo = await GetRepoAsync<PURCHASE_ORDER>("PURCHASE_ORDER", ConnectionName);

                await repo.UpdateAsync(po, userId);
                _logger?.LogInformation("PO {PONumber} closed", po.PO_NUMBER);
                return po;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error closing PO {POId}: {Message}", poId, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Generate next PO number
        /// </summary>
        private async Task<string> GeneratePONumberAsync()
        {
            try
            {
                return $"PO-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString().Substring(0, 8)}";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating PO number");
                throw;
            }
        }

        /// <summary>
        /// Generate next receipt number
        /// </summary>
        private async Task<string> GenerateReceiptNumberAsync()
        {
            try
            {
                return $"RCPT-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString().Substring(0, 8)}";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating receipt number");
                throw;
            }
        }

        private async Task<PO_LINE_ITEM?> GetPoLineItemAsync(string poLineItemId)
        {
            if (string.IsNullOrWhiteSpace(poLineItemId))
                return null;

            var repo = await GetRepoAsync<PO_LINE_ITEM>("PO_LINE_ITEM", ConnectionName);

            var lineItem = await repo.GetByIdAsync(poLineItemId);
            return lineItem as PO_LINE_ITEM;
        }
        private async Task<PPDMGenericRepository> GetRepoAsync<T>(string tableName, string? connectionName = null)
        {
            var cn = connectionName ?? ConnectionName;
            var metadata = await _metadata.GetTableMetadataAsync(tableName);

            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(T), cn, tableName);
        }
    }
}
