using Beep.OilandGas.Models.Constants;
using Beep.OilandGas.Models.Core.Refusals;
using TheTechIdea.Data.OilGas;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;

namespace Beep.OilandGas.Accounting.Services
{
    /// <summary>
    /// Purchase Order Service
    /// Manages purchase orders, goods receipt, and 3-way matching
    /// Uses: PURCHASE_ORDER, PO_RECEIPT entities
    /// </summary>
    public partial class PurchaseOrderService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<PurchaseOrderService> _logger;
        private readonly Func<Task<string>>? _resolveConnection;

        public PurchaseOrderService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<PurchaseOrderService> logger,
            Func<Task<string>>? resolveConnection = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _resolveConnection = resolveConnection;
        }

        /// <summary>
        /// Create a new purchase order (DRAFT status)
        /// </summary>
        public async Task<PURCHASE_ORDER> CreatePOAsync(
            CompanyScope company, string vendorBaId,
            decimal totalAmount,
            DateTime poDate,
            DateTime? expectedDeliveryDate = null,
            string? poNumber = null,
            string? description = null,
            string userId = "SYSTEM")
        {
            PurchaseOrderOwnership.RequireScope(company);
            if (string.IsNullOrWhiteSpace(vendorBaId))
                throw new ArgumentNullException(nameof(vendorBaId));
            if (totalAmount <= 0)
                throw RefusalException.Invalid("PO amount must be greater than zero.");

            _logger?.LogInformation("Creating PO for vendor {VendorId}, amount {Amount:C}", vendorBaId, totalAmount);

            try
            {
                var po = new PURCHASE_ORDER
                {
                    PURCHASE_ORDER_ID = Guid.NewGuid().ToString(),
                    OWNER_TENANT_ID = company.TenantId,
                    OWNER_COMPANY_BA_ID = company.BusinessAssociateId,
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

                var repo = await GetRepoAsync<PURCHASE_ORDER>("PURCHASE_ORDER");

                await repo.InsertAsync(po, userId);
                _logger?.LogInformation("PO {PONumber} created (DRAFT)", po.PO_NUMBER);
                return po;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating PO");
                throw;
            }
        }

        /// <summary>
        /// Approve a purchase order (DRAFT -> APPROVED)
        /// </summary>
        public async Task<PURCHASE_ORDER> ApprovePOAsync(CompanyScope company, string poId, string userId = "SYSTEM")
        {
            if (string.IsNullOrWhiteSpace(poId))
                throw new ArgumentNullException(nameof(poId));

            _logger?.LogInformation("Approving PO {POId}", poId);

            try
            {
                var po = await GetPOByIdAsync(company,poId);
                if (po == null)
                    throw RefusalException.NotFound($"Purchase order {poId} was not found.");

                if (po.STATUS != "DRAFT")
                    throw RefusalException.Conflict($"Only a DRAFT purchase order can be approved; this one is {po.STATUS}.");

                po.STATUS = AccountingReferenceCodes.POStatusCodes.Approved;
                po.ROW_CHANGED_BY = userId;
                po.ROW_CHANGED_DATE = DateTime.UtcNow;

                var repo = await GetRepoAsync<PURCHASE_ORDER>("PURCHASE_ORDER");

                await repo.UpdateAsync(po, userId);
                _logger?.LogInformation("PO {PONumber} approved", po.PO_NUMBER);
                return po;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error approving PO {POId}", poId);
                throw;
            }
        }

        /// <summary>
        /// Record goods receipt for a PO line item (creates PO_RECEIPT)
        /// </summary>
        public async Task<PO_RECEIPT> ReceiveGoodsAsync(
            CompanyScope company, string poLineItemId,
            decimal receivedQuantity,
            DateTime receiptDate,
            string receivedBy,
            string? description = null,
            string userId = "SYSTEM")
        {
            PurchaseOrderOwnership.RequireScope(company);
            if (string.IsNullOrWhiteSpace(poLineItemId))
                throw new ArgumentNullException(nameof(poLineItemId));
            if (receivedQuantity <= 0)
                throw RefusalException.Invalid("Received quantity must be greater than zero.");

            _logger?.LogInformation("Recording goods receipt for PO line {LineId}: Qty {Qty}",
                poLineItemId, receivedQuantity);

            try
            {
                var lineItem = await GetPoLineItemAsync(poLineItemId);
                if (lineItem == null || lineItem.PO_LINE_ITEM_ID!=poLineItemId || string.IsNullOrWhiteSpace(lineItem.PURCHASE_ORDER_ID))
                    throw RefusalException.NotFound($"Purchase order line item {poLineItemId} was not found, or names no purchase order.");

                _ = await GetPOByIdAsync(company,lineItem.PURCHASE_ORDER_ID) ?? throw RefusalException.NotFound("The owning purchase order is unavailable.");
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

                var repo = await GetRepoAsync<PO_RECEIPT>("PO_RECEIPT");

                await repo.InsertAsync(receipt, userId);

                _logger?.LogInformation("Goods receipt recorded for PO line {LineId}: Qty {Qty}",
                    poLineItemId, receivedQuantity);

                return receipt;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error recording goods receipt for PO line {LineId}", poLineItemId);
                throw;
            }
        }

        /// <summary>
        /// Get PO by ID
        /// </summary>
        public async Task<PURCHASE_ORDER?> GetPOByIdAsync(CompanyScope company, string poId)
        {
            PurchaseOrderOwnership.RequireScope(company);
            if (string.IsNullOrWhiteSpace(poId))
                return null;

            try
            {
                var repo = await GetRepoAsync<PURCHASE_ORDER>("PURCHASE_ORDER");

                var matches = await repo.GetAsync(new List<AppFilter>
                {
                    new(){FieldName="PURCHASE_ORDER_ID",Operator="=",FilterValue=poId},
                    new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},
                    new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId}
                });
                if(matches is null)throw new InvalidOperationException("The recorded purchase-order read is unavailable.");
                var records=matches.ToList();if(records.Count==0)return null;
                if(records.Count!=1 || records[0] is not PURCHASE_ORDER order)throw new InvalidOperationException("The provider returned an incompatible or ambiguous purchase-order header.");
                PurchaseOrderOwnership.Require(company,poId,order);return order;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting PO {POId}", poId);
                throw;
            }
        }

        /// <summary>
        /// Get all POs for a vendor
        /// </summary>
        public async Task<List<PURCHASE_ORDER>> GetVendorPOsAsync(CompanyScope company, string vendorBaId)
        {
            PurchaseOrderOwnership.RequireScope(company);
            if(string.IsNullOrWhiteSpace(vendorBaId))return new List<PURCHASE_ORDER>();
            return await GetCompanyPOsAsync(company,vendorBaId);
        }
        /// <summary>Recorded active company orders, optionally restricted to an exact supplier.</summary>
        public async Task<List<PURCHASE_ORDER>> GetCompanyPOsAsync(CompanyScope company,string? vendorBaId=null)
        {
            PurchaseOrderOwnership.RequireScope(company);
            if(vendorBaId is not null && string.IsNullOrWhiteSpace(vendorBaId))throw new InvalidOperationException("A supplied PO supplier restriction cannot be empty.");
            var repo=await GetRepoAsync<PURCHASE_ORDER>("PURCHASE_ORDER");
            var filters=new List<AppFilter>{
                new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},
                new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},
                new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}};
            if(vendorBaId is not null)filters.Add(new(){FieldName="VENDOR_BA_ID",Operator="=",FilterValue=vendorBaId});
            var pos=await repo.GetAsync(filters);
            if(pos is null || pos.Any(row=>row is not PURCHASE_ORDER))throw new InvalidOperationException("The recorded purchase-order list is unavailable.");
            var orders=pos.Cast<PURCHASE_ORDER>().ToList();PurchaseOrderOwnership.RequireRows(company,orders);
            if(vendorBaId is not null)PurchaseOrderOwnership.RequireVendorRows(company,vendorBaId,orders);
            return orders;
        }

        /// <summary>Read recorded lines, including inactive records needed to establish receipt history membership.</summary>
        public async Task<List<PO_LINE_ITEM>> GetLinesForPOAsync(CompanyScope company,string poId)
        {
            _ = await GetPOByIdAsync(company,poId) ?? throw RefusalException.NotFound("The owning purchase order is unavailable.");
            var repo=await GetRepoAsync<PO_LINE_ITEM>("PO_LINE_ITEM");
            var raw=await repo.GetAsync(new List<AppFilter>{new(){FieldName="PURCHASE_ORDER_ID",Operator="=",FilterValue=poId}})
                ?? throw new InvalidOperationException("Recorded purchase-order lines are unavailable.");
            var rows=raw.ToList();if(rows.Any(row=>row is not PO_LINE_ITEM))throw new InvalidOperationException("The provider returned incompatible purchase-order lines.");
            var lines=rows.Cast<PO_LINE_ITEM>().ToList();PurchaseOrderOwnership.RequireLines(poId,lines);return lines;
        }
        public async Task<PurchaseOrderSourceReview?> GetSourceReviewAsync(CompanyScope company,string poId,string actor)
        {
            var header=await GetPOByIdAsync(company,poId);if(header is null)return null;
            var lines=await GetLinesForPOAsync(company,poId);var receipts=await GetReceiptsForPOAsync(company,poId);
            return PurchaseOrderSourceReview.Create(company,actor,header,lines,receipts);
        }

        /// <summary>
        /// Get all receipts for a PO
        /// </summary>
        public async Task<List<PO_RECEIPT>> GetReceiptsForPOAsync(CompanyScope company, string poId)
        {
            if (string.IsNullOrWhiteSpace(poId))
                return new List<PO_RECEIPT>();

            _ = await GetPOByIdAsync(company,poId) ?? throw RefusalException.NotFound("The owning purchase order is unavailable.");
            // A failed read propagates rather than reading as "nothing was received".
            var repo = await GetRepoAsync<PO_RECEIPT>("PO_RECEIPT");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PURCHASE_ORDER_ID", Operator = "=", FilterValue = poId }
            };

            var receipts = await repo.GetAsync(filters);
            if(receipts is null || receipts.Any(row=>row is not PO_RECEIPT))throw new InvalidOperationException("The recorded receipt list is unavailable.");
            var rows=receipts.Cast<PO_RECEIPT>().ToList();PurchaseOrderOwnership.RequireReceipts(poId,rows);return rows;
        }

        /// <summary>
        /// Close a PO (mark as CLOSED)
        /// </summary>
        public async Task<PURCHASE_ORDER> ClosePOAsync(CompanyScope company, string poId, string userId = "SYSTEM")
        {
            if (string.IsNullOrWhiteSpace(poId))
                throw new ArgumentNullException(nameof(poId));

            try
            {
                var po = await GetPOByIdAsync(company,poId);
                if (po == null)
                    throw RefusalException.NotFound($"Purchase order {poId} was not found.");

                po.STATUS = AccountingReferenceCodes.POStatusCodes.Closed;
                po.ROW_CHANGED_BY = userId;
                po.ROW_CHANGED_DATE = DateTime.UtcNow;

                var repo = await GetRepoAsync<PURCHASE_ORDER>("PURCHASE_ORDER");

                await repo.UpdateAsync(po, userId);
                _logger?.LogInformation("PO {PONumber} closed", po.PO_NUMBER);
                return po;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error closing PO {POId}", poId);
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

            var repo = await GetRepoAsync<PO_LINE_ITEM>("PO_LINE_ITEM");

            var lineItem = await repo.GetByIdAsync(poLineItemId);
            return lineItem as PO_LINE_ITEM;
        }
        private async Task<PPDMGenericRepository> GetRepoAsync<T>(string tableName)
        {
            if(_resolveConnection is null)throw new InvalidOperationException("A reviewed request-bound PRODUCTION connection resolver is required.");
            var connection = await _resolveConnection();
            if (string.IsNullOrWhiteSpace(connection)) throw new InvalidOperationException("A PRODUCTION database binding is required.");

            if(tableName is "PURCHASE_ORDER" or "PO_LINE_ITEM" or "PO_RECEIPT")
            {
                var source=_editor.GetDataSource(connection) ?? throw new InvalidOperationException("The procurement datasource is unavailable.");
                var schema=source.GetEntityStructure(new EntityStructure{EntityName=tableName,DatasourceEntityName=tableName},true);
                var keys=schema?.Fields?.Where(field=>field.IsKey).ToArray();
                if(keys is not {Length:1} || !string.Equals(keys[0].FieldName,tableName+"_ID",StringComparison.OrdinalIgnoreCase) || keys[0].AllowDBNull)
                    throw new InvalidOperationException("The procurement record key is unconfirmed. A unique nonnullable identity is required for recorded ownership and history.");
                var required=tableName=="PURCHASE_ORDER" ? new[]{"OWNER_TENANT_ID","OWNER_COMPANY_BA_ID","CURRENCY_CODE"} : tableName=="PO_LINE_ITEM"?new[]{"PURCHASE_ORDER_ID","QUANTITY_UOM","INVENTORY_ITEM_ID"}:new[]{"PURCHASE_ORDER_ID","QUANTITY_UOM"};
                foreach(var column in required)
                    if(schema?.Fields is null || schema.Fields.Count(field=>string.Equals(field.FieldName,column,StringComparison.OrdinalIgnoreCase))!=1)
                        throw new InvalidOperationException("The procurement ownership or measurement schema is unavailable. Review the PRODUCTION Beep migration before using procurement.");
            }
            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(T), connection, tableName);
        }
    }
}
