using Beep.OilandGas.Models.DTOs.Inventory;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ProductionAccounting.Inventory
{
    /// <summary>
    /// Service for managing inventory items and transactions.
    /// Uses PPDMGenericRepository for database operations.
    /// </summary>
    public class InventoryService : IInventoryService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<InventoryService>? _logger;
        private readonly string _connectionName;
        private const string INVENTORY_ITEM_TABLE = "INVENTORY_ITEM";
        private const string INVENTORY_TRANSACTION_TABLE = "INVENTORY_TRANSACTION";
        private const string INVENTORY_ADJUSTMENT_TABLE = "INVENTORY_ADJUSTMENT";
        private const string INVENTORY_VALUATION_TABLE = "INVENTORY_VALUATION";

        public InventoryService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<InventoryService>? logger = null,
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
        /// Creates a new inventory item.
        /// </summary>
        public async Task<INVENTORY_ITEM> CreateItemAsync(CreateInventoryItemRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var connName = connectionName ?? _connectionName;
            var repo = await GetItemRepositoryAsync(connName);

            var item = new INVENTORY_ITEM
            {
                INVENTORY_ITEM_ID = Guid.NewGuid().ToString(),
                ITEM_NUMBER = request.ItemNumber,
                ITEM_NAME = request.ItemName,
                ITEM_TYPE = request.ItemType,
                UNIT_OF_MEASURE = request.UnitOfMeasure,
                UNIT_COST = request.UnitCost,
                QUANTITY_ON_HAND = 0m,
                TOTAL_VALUE = 0m,
                VALUATION_METHOD = request.ValuationMethod,
                ACTIVE_IND = "Y"
            };

            if (item is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForCreateAsync(ppdmEntity, userId, connName);
            }

            await repo.InsertAsync(item);
            _logger?.LogDebug("Created inventory item {ItemNumber}", request.ItemNumber);
            return item;
        }

        /// <summary>
        /// Gets an inventory item by ID.
        /// </summary>
        public async Task<INVENTORY_ITEM?> GetItemAsync(string itemId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(itemId))
                return null;

            var connName = connectionName ?? _connectionName;
            var repo = await GetItemRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "INVENTORY_ITEM_ID", Operator = "=", FilterValue = itemId }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<INVENTORY_ITEM>().FirstOrDefault();
        }

        /// <summary>
        /// Gets all inventory items.
        /// </summary>
        public async Task<List<INVENTORY_ITEM>> GetItemsAsync(string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var repo = await GetItemRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<INVENTORY_ITEM>().OrderBy(i => i.ITEM_NUMBER).ToList();
        }

        /// <summary>
        /// Updates an inventory item.
        /// </summary>
        public async Task<INVENTORY_ITEM> UpdateItemAsync(UpdateInventoryItemRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.InventoryItemId))
                throw new ArgumentException("Inventory Item ID is required.", nameof(request));

            var connName = connectionName ?? _connectionName;
            var item = await GetItemAsync(request.InventoryItemId, connName);

            if (item == null)
                throw new InvalidOperationException($"Inventory item {request.InventoryItemId} not found.");

            // Update properties
            item.ITEM_NUMBER = request.ItemNumber;
            item.ITEM_NAME = request.ItemName;
            item.ITEM_TYPE = request.ItemType;
            item.UNIT_OF_MEASURE = request.UnitOfMeasure;
            item.UNIT_COST = request.UnitCost;
            item.VALUATION_METHOD = request.ValuationMethod;

            // Recalculate total value
            item.TOTAL_VALUE = (item.QUANTITY_ON_HAND ?? 0m) * (item.UNIT_COST ?? 0m);

            if (item is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForUpdateAsync(ppdmEntity, userId, connName);
            }

            var repo = await GetItemRepositoryAsync(connName);
            await repo.UpdateAsync(item);

            _logger?.LogDebug("Updated inventory item {ItemId}", request.InventoryItemId);
            return item;
        }

        /// <summary>
        /// Creates an inventory transaction.
        /// </summary>
        public async Task<INVENTORY_TRANSACTION> CreateTransactionAsync(CreateInventoryTransactionRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.InventoryItemId))
                throw new ArgumentException("Inventory Item ID is required.", nameof(request));

            var connName = connectionName ?? _connectionName;
            var item = await GetItemAsync(request.InventoryItemId, connName);

            if (item == null)
                throw new InvalidOperationException($"Inventory item {request.InventoryItemId} not found.");

            var transactionRepo = await GetTransactionRepositoryAsync(connName);

            var transaction = new INVENTORY_TRANSACTION
            {
                INVENTORY_TRANSACTION_ID = Guid.NewGuid().ToString(),
                INVENTORY_ITEM_ID = request.InventoryItemId,
                TRANSACTION_TYPE = request.TransactionType,
                TRANSACTION_DATE = request.TransactionDate,
                QUANTITY = request.Quantity,
                UNIT_COST = request.UnitCost ?? item.UNIT_COST,
                TOTAL_COST = request.Quantity * (request.UnitCost ?? item.UNIT_COST ?? 0m),
                REFERENCE_NUMBER = request.ReferenceNumber,
                DESCRIPTION = request.Description,
                ACTIVE_IND = "Y"
            };

            if (transaction is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForCreateAsync(ppdmEntity, userId, connName);
            }

            await transactionRepo.InsertAsync(transaction);

            // Update inventory item quantity and value
            await UpdateInventoryItemQuantitiesAsync(item, connName);

            _logger?.LogDebug("Created inventory transaction {TransactionType} for item {ItemId}", request.TransactionType, request.InventoryItemId);
            return transaction;
        }

        /// <summary>
        /// Gets transactions by inventory item.
        /// </summary>
        public async Task<List<INVENTORY_TRANSACTION>> GetTransactionsByItemAsync(string itemId, DateTime? startDate, DateTime? endDate, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(itemId))
                return new List<INVENTORY_TRANSACTION>();

            var connName = connectionName ?? _connectionName;
            var repo = await GetTransactionRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "INVENTORY_ITEM_ID", Operator = "=", FilterValue = itemId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (startDate.HasValue)
                filters.Add(new AppFilter { FieldName = "TRANSACTION_DATE", Operator = ">=", FilterValue = startDate.Value });
            if (endDate.HasValue)
                filters.Add(new AppFilter { FieldName = "TRANSACTION_DATE", Operator = "<=", FilterValue = endDate.Value });

            var results = await repo.GetAsync(filters);
            return results.Cast<INVENTORY_TRANSACTION>().OrderByDescending(t => t.TRANSACTION_DATE).ToList();
        }

        /// <summary>
        /// Creates an inventory adjustment.
        /// </summary>
        public async Task<INVENTORY_ADJUSTMENT> CreateAdjustmentAsync(CreateInventoryAdjustmentRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.InventoryItemId))
                throw new ArgumentException("Inventory Item ID is required.", nameof(request));

            var connName = connectionName ?? _connectionName;
            var item = await GetItemAsync(request.InventoryItemId, connName);

            if (item == null)
                throw new InvalidOperationException($"Inventory item {request.InventoryItemId} not found.");

            var adjustmentRepo = await GetAdjustmentRepositoryAsync(connName);

            var adjustment = new INVENTORY_ADJUSTMENT
            {
                INVENTORY_ADJUSTMENT_ID = Guid.NewGuid().ToString(),
                INVENTORY_ITEM_ID = request.InventoryItemId,
                ADJUSTMENT_DATE = request.AdjustmentDate,
                ADJUSTMENT_TYPE = request.AdjustmentType,
                QUANTITY_ADJUSTMENT = request.QuantityAdjustment,
                UNIT_COST_ADJUSTMENT = request.UnitCostAdjustment,
                REASON = request.Reason,
                DESCRIPTION = request.Description,
                ACTIVE_IND = "Y"
            };

            if (adjustment is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForCreateAsync(ppdmEntity, userId, connName);
            }

            await adjustmentRepo.InsertAsync(adjustment);

            // Update inventory item quantities
            await UpdateInventoryItemQuantitiesAsync(item, connName);

            // Create a transaction record for the adjustment
            var transactionRequest = new CreateInventoryTransactionRequest
            {
                InventoryItemId = request.InventoryItemId,
                TransactionType = "Adjustment",
                TransactionDate = request.AdjustmentDate,
                Quantity = request.QuantityAdjustment,
                UnitCost = request.UnitCostAdjustment,
                ReferenceNumber = adjustment.INVENTORY_ADJUSTMENT_ID,
                Description = $"Adjustment: {request.Reason}"
            };

            await CreateTransactionAsync(transactionRequest, userId, connName);

            _logger?.LogDebug("Created inventory adjustment for item {ItemId}", request.InventoryItemId);
            return adjustment;
        }

        /// <summary>
        /// Calculates inventory valuation.
        /// </summary>
        public async Task<INVENTORY_VALUATION> CalculateValuationAsync(string itemId, ValuationMethod method, DateTime valuationDate, string userId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(itemId))
                throw new ArgumentException("Inventory Item ID is required.", nameof(itemId));

            var connName = connectionName ?? _connectionName;
            var item = await GetItemAsync(itemId, connName);

            if (item == null)
                throw new InvalidOperationException($"Inventory item {itemId} not found.");

            var transactions = await GetTransactionsByItemAsync(itemId, null, valuationDate, connName);
            var transactionsList = transactions.Where(t => t.TRANSACTION_DATE <= valuationDate).OrderBy(t => t.TRANSACTION_DATE).ToList();

            decimal quantity = 0m;
            decimal unitCost = 0m;
            decimal totalValue = 0m;

            switch (method)
            {
                case ValuationMethod.FIFO:
                    (quantity, unitCost, totalValue) = CalculateFIFO(transactionsList);
                    break;
                case ValuationMethod.LIFO:
                    (quantity, unitCost, totalValue) = CalculateLIFO(transactionsList);
                    break;
                case ValuationMethod.WeightedAverage:
                    (quantity, unitCost, totalValue) = CalculateWeightedAverage(transactionsList);
                    break;
                case ValuationMethod.LCM:
                    (quantity, unitCost, totalValue) = CalculateLCM(transactionsList, item.UNIT_COST ?? 0m);
                    break;
            }

            var valuationRepo = await GetValuationRepositoryAsync(connName);

            var valuation = new INVENTORY_VALUATION
            {
                INVENTORY_VALUATION_ID = Guid.NewGuid().ToString(),
                INVENTORY_ITEM_ID = itemId,
                VALUATION_DATE = valuationDate,
                VALUATION_METHOD = method.ToString(),
                QUANTITY = quantity,
                UNIT_COST = unitCost,
                TOTAL_VALUE = totalValue,
                ACTIVE_IND = "Y"
            };

            if (valuation is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForCreateAsync(ppdmEntity, userId, connName);
            }

            await valuationRepo.InsertAsync(valuation);

            _logger?.LogDebug("Calculated inventory valuation for item {ItemId} using {Method}", itemId, method);
            return valuation;
        }

        /// <summary>
        /// Reconciles inventory (book vs physical).
        /// </summary>
        public async Task<InventoryReconciliationResult> ReconcileInventoryAsync(string itemId, DateTime reconciliationDate, decimal physicalQuantity, string userId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(itemId))
                throw new ArgumentException("Inventory Item ID is required.", nameof(itemId));

            var connName = connectionName ?? _connectionName;
            var item = await GetItemAsync(itemId, connName);

            if (item == null)
                throw new InvalidOperationException($"Inventory item {itemId} not found.");

            decimal bookQuantity = item.QUANTITY_ON_HAND ?? 0m;
            decimal variance = physicalQuantity - bookQuantity;
            decimal varianceAmount = variance * (item.UNIT_COST ?? 0m);

            var result = new InventoryReconciliationResult
            {
                InventoryItemId = itemId,
                ReconciliationDate = reconciliationDate,
                BookQuantity = bookQuantity,
                PhysicalQuantity = physicalQuantity,
                Variance = variance,
                VarianceAmount = varianceAmount,
                RequiresAdjustment = Math.Abs(variance) > 0.01m
            };

            // Create adjustment if variance exists
            if (result.RequiresAdjustment)
            {
                var adjustmentRequest = new CreateInventoryAdjustmentRequest
                {
                    InventoryItemId = itemId,
                    AdjustmentDate = reconciliationDate,
                    AdjustmentType = variance > 0 ? "Increase" : "Decrease",
                    QuantityAdjustment = Math.Abs(variance),
                    Reason = "Reconciliation",
                    Description = $"Reconciliation adjustment: Book={bookQuantity}, Physical={physicalQuantity}, Variance={variance}"
                };

                var adjustment = await CreateAdjustmentAsync(adjustmentRequest, userId, connName);
                result.AdjustmentId = adjustment.INVENTORY_ADJUSTMENT_ID;
            }

            _logger?.LogDebug("Reconciled inventory for item {ItemId}: Book={BookQty}, Physical={PhysicalQty}, Variance={Variance}",
                 itemId, bookQuantity, physicalQuantity, variance);

            return result;
        }

        /// <summary>
        /// Gets inventory summary.
        /// </summary>
        public async Task<InventorySummary> GetInventorySummaryAsync(string? itemId, string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            INVENTORY_ITEM item;

            if (!string.IsNullOrEmpty(itemId))
            {
                item = await GetItemAsync(itemId, connName);

                if (item == null)
                    throw new InvalidOperationException($"Inventory item {itemId} not found.");
            }
            else
            {
                // Get summary for all items
                var items = await GetItemsAsync(connName);

                if (!items.Any())
                    return new InventorySummary();

                // For now, return summary for first item
                item = items.First();
            }

            var transactions = await GetTransactionsByItemAsync(item.INVENTORY_ITEM_ID, null, null, connName);
            var lastTransaction = transactions.OrderByDescending(t => t.TRANSACTION_DATE).FirstOrDefault();

            return new InventorySummary
            {
                InventoryItemId = item.INVENTORY_ITEM_ID ?? string.Empty,
                ItemNumber = item.ITEM_NUMBER ?? string.Empty,
                ItemName = item.ITEM_NAME ?? string.Empty,
                QuantityOnHand = item.QUANTITY_ON_HAND ?? 0m,
                UnitCost = item.UNIT_COST ?? 0m,
                TotalValue = item.TOTAL_VALUE ?? 0m,
                TransactionCount = transactions.Count,
                LastTransactionDate = lastTransaction?.TRANSACTION_DATE,
                RequiresReconciliation = false
            };
        }

        /// <summary>
        /// Gets items requiring reconciliation.
        /// </summary>
        public async Task<List<INVENTORY_ITEM>> GetItemsRequiringReconciliationAsync(string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var items = await GetItemsAsync(connName);
            return items.Where(i => (i.QUANTITY_ON_HAND ?? 0m) > 0).ToList();
        }

        // Helper methods for valuation calculations
        private (decimal quantity, decimal unitCost, decimal totalValue) CalculateFIFO(List<INVENTORY_TRANSACTION> transactions)
        {
            decimal quantity = 0m;
            decimal totalCost = 0m;

            foreach (var transaction in transactions)
            {
                if (transaction.TRANSACTION_TYPE == "Receipt" || transaction.TRANSACTION_TYPE == "Adjustment" && (transaction.QUANTITY ?? 0m) > 0)
                {
                    quantity += transaction.QUANTITY ?? 0m;
                    totalCost += transaction.TOTAL_COST ?? 0m;
                }
                else if (transaction.TRANSACTION_TYPE == "Issue" || transaction.TRANSACTION_TYPE == "Adjustment" && (transaction.QUANTITY ?? 0m) < 0)
                {
                    decimal issueQty = Math.Abs(transaction.QUANTITY ?? 0m);
                    while (issueQty > 0 && quantity > 0)
                    {
                        decimal removeQty = Math.Min(issueQty, quantity);
                        decimal avgCost = quantity > 0 ? totalCost / quantity : 0m;
                        totalCost -= removeQty * avgCost;
                        quantity -= removeQty;
                        issueQty -= removeQty;
                    }
                }
            }

            decimal unitCost = quantity > 0 ? totalCost / quantity : 0m;
            return (quantity, unitCost, totalCost);
        }

        private (decimal quantity, decimal unitCost, decimal totalValue) CalculateLIFO(List<INVENTORY_TRANSACTION> transactions)
        {
            var reversedTransactions = transactions.OrderByDescending(t => t.TRANSACTION_DATE).ToList();
            return CalculateFIFO(reversedTransactions);
        }

        private (decimal quantity, decimal unitCost, decimal totalValue) CalculateWeightedAverage(List<INVENTORY_TRANSACTION> transactions)
        {
            decimal quantity = 0m;
            decimal totalCost = 0m;

            foreach (var transaction in transactions)
            {
                if (transaction.TRANSACTION_TYPE == "Receipt" || transaction.TRANSACTION_TYPE == "Adjustment")
                {
                    quantity += transaction.QUANTITY ?? 0m;
                    totalCost += transaction.TOTAL_COST ?? 0m;
                }
                else if (transaction.TRANSACTION_TYPE == "Issue")
                {
                    quantity -= transaction.QUANTITY ?? 0m;
                    decimal avgCost = quantity > 0 ? totalCost / quantity : 0m;
                    totalCost -= (transaction.QUANTITY ?? 0m) * avgCost;
                }
            }

            decimal unitCost = quantity > 0 ? totalCost / quantity : 0m;
            return (quantity, unitCost, totalCost);
        }

        private (decimal quantity, decimal unitCost, decimal totalValue) CalculateLCM(List<INVENTORY_TRANSACTION> transactions, decimal marketValue)
        {
            var (quantity, unitCost, totalValue) = CalculateWeightedAverage(transactions);
            decimal marketTotalValue = quantity * marketValue;

            if (marketTotalValue < totalValue)
            {
                totalValue = marketTotalValue;
                unitCost = marketValue;
            }

            return (quantity, unitCost, totalValue);
        }

        /// <summary>
        /// Updates inventory item quantities based on transactions.
        /// </summary>
        private async Task UpdateInventoryItemQuantitiesAsync(INVENTORY_ITEM item, string connectionName)
        {
            var transactions = await GetTransactionsByItemAsync(item.INVENTORY_ITEM_ID, null, null, connectionName);

            decimal quantity = 0m;
            decimal totalCost = 0m;

            foreach (var transaction in transactions.OrderBy(t => t.TRANSACTION_DATE))
            {
                if (transaction.TRANSACTION_TYPE == "Receipt" || (transaction.TRANSACTION_TYPE == "Adjustment" && (transaction.QUANTITY ?? 0m) > 0))
                {
                    quantity += transaction.QUANTITY ?? 0m;
                    totalCost += transaction.TOTAL_COST ?? 0m;
                }
                else if (transaction.TRANSACTION_TYPE == "Issue" || (transaction.TRANSACTION_TYPE == "Adjustment" && (transaction.QUANTITY ?? 0m) < 0))
                {
                    quantity -= Math.Abs(transaction.QUANTITY ?? 0m);
                    if (quantity > 0)
                    {
                        decimal avgCost = totalCost / (quantity + Math.Abs(transaction.QUANTITY ?? 0m));
                        totalCost -= Math.Abs(transaction.QUANTITY ?? 0m) * avgCost;
                    }
                    else
                    {
                        totalCost = 0m;
                    }
                }
            }

            item.QUANTITY_ON_HAND = quantity;
            item.UNIT_COST = quantity > 0 ? totalCost / quantity : item.UNIT_COST;
            item.TOTAL_VALUE = totalCost;

            if (item is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForUpdateAsync(ppdmEntity, "System", connectionName);
            }

            var repo = await GetItemRepositoryAsync(connectionName);
            await repo.UpdateAsync(item);
        }

        // Repository helper methods
        private async Task<PPDMGenericRepository> GetItemRepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(INVENTORY_ITEM_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(INVENTORY_ITEM);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, INVENTORY_ITEM_TABLE,
                null);
        }

        private async Task<PPDMGenericRepository> GetTransactionRepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(INVENTORY_TRANSACTION_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(INVENTORY_TRANSACTION);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, INVENTORY_TRANSACTION_TABLE,
                null);
        }

        private async Task<PPDMGenericRepository> GetAdjustmentRepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(INVENTORY_ADJUSTMENT_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(INVENTORY_ADJUSTMENT);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, INVENTORY_ADJUSTMENT_TABLE,
                null);
        }

        private async Task<PPDMGenericRepository> GetValuationRepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(INVENTORY_VALUATION_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(INVENTORY_VALUATION);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, INVENTORY_VALUATION_TABLE,
                null);
        }
    }
}
