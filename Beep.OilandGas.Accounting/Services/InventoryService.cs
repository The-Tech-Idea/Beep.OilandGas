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
    /// Inventory Service
    /// Manages inventory items, stock transactions, and valuations
    /// Uses: INVENTORY_ITEM, INVENTORY_TRANSACTION entities
    /// GL Posting: Debit Inventory (1300), Credit AP (2000) on receipt
    ///            Debit COGS (5000), Credit Inventory (1300) on sale/use
    /// </summary>
    public class InventoryService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly JournalEntryService _journalEntryService;
        private readonly ILogger<InventoryService> _logger;
        private const string ConnectionName = "PPDM39";

        public InventoryService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            JournalEntryService journalEntryService,
            ILogger<InventoryService> logger)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _journalEntryService = journalEntryService ?? throw new ArgumentNullException(nameof(journalEntryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Create a new inventory item
        /// </summary>
        public async Task<INVENTORY_ITEM> CreateInventoryItemAsync(
            string itemNumber,
            string itemName,
            string itemType,
            string uom,
            decimal unitCost,
            string? description = null,
            string userId = "SYSTEM")
        {
            if (string.IsNullOrWhiteSpace(itemNumber))
                throw new ArgumentNullException(nameof(itemNumber));
            if (string.IsNullOrWhiteSpace(itemName))
                throw new ArgumentNullException(nameof(itemName));

            _logger?.LogInformation("Creating inventory item {ItemNumber} - {ItemName}",
                itemNumber, itemName);

            try
            {
                var item = new INVENTORY_ITEM
                {
                    INVENTORY_ITEM_ID = Guid.NewGuid().ToString(),
                    ITEM_NUMBER = itemNumber,
                    ITEM_NAME = itemName,
                    ITEM_TYPE = itemType,
                    UNIT_OF_MEASURE = uom,
                    UNIT_COST = unitCost,
                    QUANTITY_ON_HAND = 0m,
                    DESCRIPTION = description,
                    ACTIVE_IND = "Y",
                    PPDM_GUID = Guid.NewGuid().ToString(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                var metadata = await _metadata.GetTableMetadataAsync("INVENTORY_ITEM");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{metadata.EntityTypeName}")
                    ?? typeof(INVENTORY_ITEM);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, ConnectionName, "INVENTORY_ITEM");

                await repo.InsertAsync(item, userId);
                _logger?.LogInformation("Inventory item {ItemNumber} created", itemNumber);
                return item;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating inventory item {ItemNumber}: {Message}",
                    itemNumber, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Record stock receipt (increases inventory, posts GL)
        /// GL Entry: Debit Inventory (1300), Credit AP (2000)
        /// </summary>
        public async Task<INVENTORY_TRANSACTION> ReceiveStockAsync(
            string inventoryItemId,
            decimal quantity,
            decimal costPerUnit,
            DateTime transactionDate,
            string? referenceNumber = null,
            string? notes = null,
            string userId = "SYSTEM")
        {
            if (string.IsNullOrWhiteSpace(inventoryItemId))
                throw new ArgumentNullException(nameof(inventoryItemId));
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

            _logger?.LogInformation("Recording stock receipt: Item {ItemId}, Qty {Qty}",
                inventoryItemId, quantity);

            try
            {
                // Update inventory item
                var item = await GetInventoryItemByIdAsync(inventoryItemId);
                if (item == null)
                    throw new InvalidOperationException($"Inventory item {inventoryItemId} not found");

                decimal transactionAmount = quantity * costPerUnit;

                // Create GL entry: Debit Inventory, Credit AP
                var lineItems = new List<JOURNAL_ENTRY_LINE>
                {
                    new JOURNAL_ENTRY_LINE
                    {
                        GL_ACCOUNT_ID = "1300", // Inventory account
                        DEBIT_AMOUNT = transactionAmount,
                        CREDIT_AMOUNT = 0m,
                        DESCRIPTION = $"Stock receipt: {item.ITEM_NAME} x {quantity}"
                    },
                    new JOURNAL_ENTRY_LINE
                    {
                        GL_ACCOUNT_ID = "2000", // AP account
                        DEBIT_AMOUNT = 0m,
                        CREDIT_AMOUNT = transactionAmount,
                        DESCRIPTION = $"Purchase liability for {item.ITEM_NAME}"
                    }
                };

                var glEntry = await _journalEntryService.CreateEntryAsync(
                    transactionDate,
                    $"Stock receipt: {item.ITEM_NAME}",
                    lineItems,
                    userId,
                    referenceNumber ?? $"RCPT-{inventoryItemId}",
                    "INVENTORY");

                await _journalEntryService.PostEntryAsync(glEntry.JOURNAL_ENTRY_ID, userId);

                // Create inventory transaction record
                var transaction = new INVENTORY_TRANSACTION
                {
                    INVENTORY_TRANSACTION_ID = Guid.NewGuid().ToString(),
                    INVENTORY_ITEM_ID = inventoryItemId,
                    TRANSACTION_DATE = transactionDate,
                    TRANSACTION_TYPE = "RECEIPT",
                    QUANTITY = quantity,
                    UNIT_COST = costPerUnit,
                    TOTAL_COST = transactionAmount,
                    REFERENCE_NUMBER = referenceNumber,
                    NOTES = notes,
                    ACTIVE_IND = "Y",
                    PPDM_GUID = Guid.NewGuid().ToString(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                var txnMetadata = await _metadata.GetTableMetadataAsync("INVENTORY_TRANSACTION");
                var txnEntityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{txnMetadata.EntityTypeName}")
                    ?? typeof(INVENTORY_TRANSACTION);

                var txnRepo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    txnEntityType, ConnectionName, "INVENTORY_TRANSACTION");

                await txnRepo.InsertAsync(transaction, userId);

                // Update on-hand quantity
                item.QUANTITY_ON_HAND = (item.QUANTITY_ON_HAND ?? 0m) + quantity;
                item.ROW_CHANGED_BY = userId;
                item.ROW_CHANGED_DATE = DateTime.UtcNow;

                var itemMetadata = await _metadata.GetTableMetadataAsync("INVENTORY_ITEM");
                var itemEntityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{itemMetadata.EntityTypeName}")
                    ?? typeof(INVENTORY_ITEM);

                var itemRepo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    itemEntityType, ConnectionName, "INVENTORY_ITEM");

                await itemRepo.UpdateAsync(item, userId);

                _logger?.LogInformation("Stock receipt recorded: Item {ItemId}, Qty {Qty}, Amount {Amount:C}",
                    inventoryItemId, quantity, transactionAmount);

                return transaction;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error recording stock receipt for item {ItemId}: {Message}",
                    inventoryItemId, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Record stock usage/sale (decreases inventory, posts GL)
        /// GL Entry: Debit COGS (5000), Credit Inventory (1300)
        /// </summary>
        public async Task<INVENTORY_TRANSACTION> UseStockAsync(
            string inventoryItemId,
            decimal quantity,
            string valuationMethod,
            DateTime transactionDate,
            string? referenceNumber = null,
            string? notes = null,
            string userId = "SYSTEM")
        {
            if (string.IsNullOrWhiteSpace(inventoryItemId))
                throw new ArgumentNullException(nameof(inventoryItemId));
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

            _logger?.LogInformation("Recording stock usage: Item {ItemId}, Qty {Qty}, Method {Method}",
                inventoryItemId, quantity, valuationMethod);

            try
            {
                var item = await GetInventoryItemByIdAsync(inventoryItemId);
                if (item == null)
                    throw new InvalidOperationException($"Inventory item {inventoryItemId} not found");

                if ((item.QUANTITY_ON_HAND ?? 0m) < quantity)
                    throw new InvalidOperationException(
                        $"Insufficient stock. On hand: {item.QUANTITY_ON_HAND}, Requested: {quantity}");

                // Calculate COGS based on valuation method
                decimal costOfGoods = CalculateCOGS(item, quantity, valuationMethod);

                // Create GL entry: Debit COGS, Credit Inventory
                var lineItems = new List<JOURNAL_ENTRY_LINE>
                {
                    new JOURNAL_ENTRY_LINE
                    {
                        GL_ACCOUNT_ID = "5000", // COGS account
                        DEBIT_AMOUNT = costOfGoods,
                        CREDIT_AMOUNT = 0m,
                        DESCRIPTION = $"COGS: {item.ITEM_NAME} x {quantity}"
                    },
                    new JOURNAL_ENTRY_LINE
                    {
                        GL_ACCOUNT_ID = "1300", // Inventory account
                        DEBIT_AMOUNT = 0m,
                        CREDIT_AMOUNT = costOfGoods,
                        DESCRIPTION = $"Inventory reduction: {item.ITEM_NAME}"
                    }
                };

                var glEntry = await _journalEntryService.CreateEntryAsync(
                    transactionDate,
                    $"Stock usage: {item.ITEM_NAME}",
                    lineItems,
                    userId,
                    referenceNumber ?? $"USE-{inventoryItemId}",
                    "INVENTORY");

                await _journalEntryService.PostEntryAsync(glEntry.JOURNAL_ENTRY_ID, userId);

                // Create inventory transaction record
                var transaction = new INVENTORY_TRANSACTION
                {
                    INVENTORY_TRANSACTION_ID = Guid.NewGuid().ToString(),
                    INVENTORY_ITEM_ID = inventoryItemId,
                    TRANSACTION_DATE = transactionDate,
                    TRANSACTION_TYPE = "USAGE",
                    QUANTITY = -quantity, // Negative for deduction
                    UNIT_COST = costOfGoods / quantity,
                    TOTAL_COST = -costOfGoods, // Negative for deduction
                    REFERENCE_NUMBER = referenceNumber,
                    NOTES = notes,
                    ACTIVE_IND = "Y",
                    PPDM_GUID = Guid.NewGuid().ToString(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                var txnMetadata = await _metadata.GetTableMetadataAsync("INVENTORY_TRANSACTION");
                var txnEntityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{txnMetadata.EntityTypeName}")
                    ?? typeof(INVENTORY_TRANSACTION);

                var txnRepo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    txnEntityType, ConnectionName, "INVENTORY_TRANSACTION");

                await txnRepo.InsertAsync(transaction, userId);

                // Update on-hand quantity
                item.QUANTITY_ON_HAND = (item.QUANTITY_ON_HAND ?? 0m) - quantity;
                item.ROW_CHANGED_BY = userId;
                item.ROW_CHANGED_DATE = DateTime.UtcNow;

                var itemMetadata = await _metadata.GetTableMetadataAsync("INVENTORY_ITEM");
                var itemEntityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{itemMetadata.EntityTypeName}")
                    ?? typeof(INVENTORY_ITEM);

                var itemRepo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    itemEntityType, ConnectionName, "INVENTORY_ITEM");

                await itemRepo.UpdateAsync(item, userId);

                _logger?.LogInformation("Stock usage recorded: Item {ItemId}, Qty {Qty}, COGS {COGS:C}",
                    inventoryItemId, quantity, costOfGoods);

                return transaction;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error recording stock usage for item {ItemId}: {Message}",
                    inventoryItemId, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get inventory item by ID
        /// </summary>
        public async Task<INVENTORY_ITEM?> GetInventoryItemByIdAsync(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                return null;

            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("INVENTORY_ITEM");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{metadata.EntityTypeName}")
                    ?? typeof(INVENTORY_ITEM);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, ConnectionName, "INVENTORY_ITEM");

                var item = await repo.GetByIdAsync(itemId);
                return item as INVENTORY_ITEM;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting inventory item {ItemId}", itemId);
                return null;
            }
        }

        /// <summary>
        /// Get all active inventory items
        /// </summary>
        public async Task<List<INVENTORY_ITEM>> GetAllInventoryItemsAsync()
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("INVENTORY_ITEM");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{metadata.EntityTypeName}")
                    ?? typeof(INVENTORY_ITEM);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, ConnectionName, "INVENTORY_ITEM");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var items = await repo.GetAsync(filters);
                return items?.Cast<INVENTORY_ITEM>().ToList() ?? new List<INVENTORY_ITEM>();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting all inventory items");
                return new List<INVENTORY_ITEM>();
            }
        }

        /// <summary>
        /// Get transactions for an inventory item
        /// </summary>
        public async Task<List<INVENTORY_TRANSACTION>> GetItemTransactionsAsync(string inventoryItemId)
        {
            if (string.IsNullOrWhiteSpace(inventoryItemId))
                return new List<INVENTORY_TRANSACTION>();

            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("INVENTORY_TRANSACTION");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{metadata.EntityTypeName}")
                    ?? typeof(INVENTORY_TRANSACTION);

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, ConnectionName, "INVENTORY_TRANSACTION");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "INVENTORY_ITEM_ID", Operator = "=", FilterValue = inventoryItemId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var transactions = await repo.GetAsync(filters);
                return transactions?.Cast<INVENTORY_TRANSACTION>().ToList() ?? new List<INVENTORY_TRANSACTION>();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting transactions for item {ItemId}", inventoryItemId);
                return new List<INVENTORY_TRANSACTION>();
            }
        }

        /// <summary>
        /// Calculate COGS based on valuation method
        /// </summary>
        private decimal CalculateCOGS(INVENTORY_ITEM item, decimal quantityUsed, string valuationMethod)
        {
            // Default: Weighted Average Cost
            decimal avgCost = item.UNIT_COST ?? 0m;
            return quantityUsed * avgCost;
        }
    }
}
