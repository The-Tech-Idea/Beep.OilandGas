using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.ProductionAccounting.Constants;
using Beep.OilandGas.ProductionAccounting.Exceptions;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// Inventory Service - Manages tank and pipeline inventory.
    /// Tracks volume levels, updates inventory, and validates amounts.
    /// </summary>
    public class InventoryService : IInventoryService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<InventoryService> _logger;
        private const string ConnectionName = "PPDM39";

        public InventoryService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<InventoryService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        /// <summary>
        /// Updates tank inventory with additional volume.
        /// Formula: New Closing Balance = Current Balance + Receipts - Deliveries
        /// </summary>
        public async Task<TANK_INVENTORY> UpdateInventoryAsync(
            string tankId,
            decimal volume,
            string userId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(tankId))
                throw new ArgumentNullException(nameof(tankId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            _logger?.LogInformation(
                "Updating inventory for tank {TankId} by volume {Volume} for user {UserId}",
                tankId, volume, userId);

            try
            {
                // Get metadata
                var metadata = await _metadata.GetTableMetadataAsync("TANK_INVENTORY");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(TANK_INVENTORY);

                // Create repository
                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "TANK_INVENTORY");

                // Get current inventory
                var inventory = await repo.GetByIdAsync(tankId);

                if (inventory == null)
                {
                    _logger?.LogWarning("Tank inventory not found for tank {TankId}", tankId);
                    throw new InvalidOperationException($"Tank inventory not found for tank ID: {tankId}");
                }

                // Cast to TANK_INVENTORY for type safety
                var tankInventory = inventory as TANK_INVENTORY;
                if (tankInventory == null)
                {
                    _logger?.LogError("Failed to cast inventory to TANK_INVENTORY for tank {TankId}", tankId);
                    throw new InvalidOperationException("Inventory type mismatch");
                }

                // Calculate new closing inventory
                var currentClosing = tankInventory.ACTUAL_CLOSING_INVENTORY ?? 0;
                var newClosing = currentClosing + volume;

                // Validate new volume is not negative
                if (newClosing < 0)
                {
                    _logger?.LogError(
                        "Inventory update would result in negative closing inventory for tank {TankId}: {NewClosing}",
                        tankId, newClosing);
                    throw new InvalidOperationException(
                        $"Inventory update would result in negative balance. Current: {currentClosing}, Change: {volume}");
                }

                // Update closing inventory
                tankInventory.ACTUAL_CLOSING_INVENTORY = newClosing;

                // Save changes
                await repo.UpdateAsync(tankInventory, userId);

                _logger?.LogInformation(
                    "Tank inventory updated successfully. Tank: {TankId}, New Closing: {NewClosing}",
                    tankId, newClosing);

                return tankInventory;
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "Error updating inventory for tank {TankId}: {ErrorMessage}",
                    tankId, ex.Message);
                throw new ProductionAccountingException(
                    $"Failed to update inventory for tank {tankId}", ex);
            }
        }

        /// <summary>
        /// Gets current inventory level for a tank.
        /// </summary>
        public async Task<TANK_INVENTORY?> GetInventoryAsync(
            string tankId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(tankId))
                throw new ArgumentNullException(nameof(tankId));

            _logger?.LogInformation("Retrieving inventory for tank {TankId}", tankId);

            try
            {
                // Get metadata
                var metadata = await _metadata.GetTableMetadataAsync("TANK_INVENTORY");
                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                    ?? typeof(TANK_INVENTORY);

                // Create repository
                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, cn, "TANK_INVENTORY");

                // Get inventory
                var inventory = await repo.GetByIdAsync(tankId);

                if (inventory == null)
                {
                    _logger?.LogError("Tank inventory not found for tank {TankId}", tankId);
                    throw new ProductionAccountingException($"Tank inventory not found for tank {tankId}");
                }

                var tankInventory = inventory as TANK_INVENTORY;
                _logger?.LogInformation(
                    "Retrieved inventory for tank {TankId}: {ClosingVolume} units",
                    tankId, tankInventory?.ACTUAL_CLOSING_INVENTORY ?? 0);

                return tankInventory;
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "Error retrieving inventory for tank {TankId}: {ErrorMessage}",
                    tankId, ex.Message);
                throw new ProductionAccountingException(
                    $"Failed to retrieve inventory for tank {tankId}", ex);
            }
        }

        /// <summary>
        /// Validates inventory data for business rule compliance.
        /// Checks: ACTUAL_CLOSING_INVENTORY >= 0, TANK_BATTERY_ID not empty, etc.
        /// </summary>
        public async Task<bool> ValidateAsync(
            TANK_INVENTORY inventory,
            string cn = "PPDM39")
        {
            if (inventory == null)
                throw new ArgumentNullException(nameof(inventory));

            _logger?.LogInformation(
                "Validating inventory for tank {TankBatteryId}",
                inventory.TANK_BATTERY_ID);

            try
            {
                // Tank Battery ID required
                if (string.IsNullOrWhiteSpace(inventory.TANK_BATTERY_ID))
                {
                    _logger?.LogWarning("Validation failed: TANK_BATTERY_ID is empty");
                    throw new InvalidOperationException("TANK_BATTERY_ID is required");
                }

                // Opening inventory should be non-negative if set
                if (inventory.OPENING_INVENTORY.HasValue && inventory.OPENING_INVENTORY < 0)
                {
                    _logger?.LogWarning(
                        "Validation failed: Negative opening inventory for tank {TankId}: {Volume}",
                        inventory.TANK_BATTERY_ID, inventory.OPENING_INVENTORY);
                    throw new InvalidOperationException(
                        $"OPENING_INVENTORY cannot be negative. Current value: {inventory.OPENING_INVENTORY}");
                }

                // Closing inventory must be non-negative if set
                if (inventory.ACTUAL_CLOSING_INVENTORY.HasValue && inventory.ACTUAL_CLOSING_INVENTORY < 0)
                {
                    _logger?.LogWarning(
                        "Validation failed: Negative closing inventory for tank {TankId}: {Volume}",
                        inventory.TANK_BATTERY_ID, inventory.ACTUAL_CLOSING_INVENTORY);
                    throw new InvalidOperationException(
                        $"ACTUAL_CLOSING_INVENTORY cannot be negative. Current value: {inventory.ACTUAL_CLOSING_INVENTORY}");
                }

                _logger?.LogInformation(
                    "Inventory validation passed for tank {TankBatteryId}",
                    inventory.TANK_BATTERY_ID);

                return true;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "Error validating inventory for tank {TankBatteryId}: {ErrorMessage}",
                    inventory.TANK_BATTERY_ID, ex.Message);
                throw new ProductionAccountingException(
                    $"Failed to validate inventory for tank {inventory.TANK_BATTERY_ID}", ex);
            }
        }

        public async Task<INVENTORY_VALUATION> CalculateValuationAsync(
            string inventoryItemId,
            DateTime valuationDate,
            string method,
            string userId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(inventoryItemId))
                throw new ArgumentNullException(nameof(inventoryItemId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var transactions = await GetInventoryTransactionsAsync(inventoryItemId, valuationDate, cn);
            var valuationMethod = string.IsNullOrWhiteSpace(method) ? "WEIGHTED_AVG" : method.ToUpperInvariant();

            var valuation = CalculateValuation(transactions, valuationMethod);
            var record = new INVENTORY_VALUATION
            {
                INVENTORY_VALUATION_ID = Guid.NewGuid().ToString(),
                INVENTORY_ITEM_ID = inventoryItemId,
                VALUATION_DATE = valuationDate,
                VALUATION_METHOD = valuationMethod,
                QUANTITY = valuation.Quantity,
                UNIT_COST = valuation.UnitCost,
                TOTAL_VALUE = valuation.TotalValue,
                DESCRIPTION = $"{valuationMethod} valuation as of {valuationDate:yyyy-MM-dd}",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<INVENTORY_VALUATION>("INVENTORY_VALUATION", cn);
            await repo.InsertAsync(record, userId);
            return record;
        }

        public async Task<INVENTORY_REPORT_SUMMARY> GenerateReconciliationReportAsync(
            string inventoryItemId,
            DateTime periodStart,
            DateTime periodEnd,
            string userId,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(inventoryItemId))
                throw new ArgumentNullException(nameof(inventoryItemId));
            if (periodStart > periodEnd)
                throw new ArgumentException("periodStart must be <= periodEnd", nameof(periodStart));

            var allTransactions = await GetInventoryTransactionsAsync(inventoryItemId, periodEnd, cn);
            var openingTransactions = allTransactions.Where(t => t.TRANSACTION_DATE.HasValue && t.TRANSACTION_DATE.Value.Date < periodStart.Date).ToList();
            var periodTransactions = allTransactions.Where(t => t.TRANSACTION_DATE.HasValue &&
                t.TRANSACTION_DATE.Value.Date >= periodStart.Date &&
                t.TRANSACTION_DATE.Value.Date <= periodEnd.Date).ToList();

            var opening = CalculateValuation(openingTransactions, "WEIGHTED_AVG");
            var receipts = periodTransactions.Where(t => IsReceipt(t)).Sum(t => t.QUANTITY ?? 0m);
            var deliveries = periodTransactions.Where(t => IsIssue(t)).Sum(t => Math.Abs(t.QUANTITY ?? 0m));
            var closing = opening.Quantity + receipts - deliveries;

            var summary = new INVENTORY_REPORT_SUMMARY
            {
                INVENTORY_REPORT_SUMMARY_ID = Guid.NewGuid().ToString(),
                OPENING_INVENTORY = opening.Quantity,
                RECEIPTS = receipts,
                DELIVERIES = deliveries,
                CLOSING_INVENTORY = closing,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                REMARK = $"InventoryItemId: {inventoryItemId} Period: {periodStart:yyyy-MM-dd} to {periodEnd:yyyy-MM-dd}",
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            var repo = await GetRepoAsync<INVENTORY_REPORT_SUMMARY>("INVENTORY_REPORT_SUMMARY", cn);
            await repo.InsertAsync(summary, userId);
            return summary;
        }

        private async Task<List<INVENTORY_TRANSACTION>> GetInventoryTransactionsAsync(string inventoryItemId, DateTime asOfDate, string cn)
        {
            var repo = await GetRepoAsync<INVENTORY_TRANSACTION>("INVENTORY_TRANSACTION", cn);
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "INVENTORY_ITEM_ID", Operator = "=", FilterValue = inventoryItemId },
                new AppFilter { FieldName = "TRANSACTION_DATE", Operator = "<=", FilterValue = asOfDate.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results?.Cast<INVENTORY_TRANSACTION>().OrderBy(t => t.TRANSACTION_DATE).ToList()
                ?? new List<INVENTORY_TRANSACTION>();
        }

        private InventoryValuationResult CalculateValuation(List<INVENTORY_TRANSACTION> transactions, string method)
        {
            if (!transactions.Any())
                return new InventoryValuationResult();

            var layers = new List<InventoryLayer>();
            decimal totalQuantity = 0m;
            decimal totalCost = 0m;

            foreach (var tx in transactions)
            {
                var quantity = tx.QUANTITY ?? 0m;
                if (quantity == 0m)
                    continue;

                if (IsReceipt(tx))
                {
                    var unitCost = tx.UNIT_COST ?? (quantity != 0m ? (tx.TOTAL_COST ?? 0m) / quantity : 0m);
                    layers.Add(new InventoryLayer { Quantity = quantity, UnitCost = unitCost });
                    totalQuantity += quantity;
                    totalCost += quantity * unitCost;
                }
                else if (IsIssue(tx))
                {
                    var issueQuantity = Math.Abs(quantity);
                    if (method == "WEIGHTED_AVG")
                    {
                        var avgCost = totalQuantity == 0m ? 0m : totalCost / totalQuantity;
                        totalQuantity -= issueQuantity;
                        totalCost -= issueQuantity * avgCost;
                    }
                    else
                    {
                        ConsumeLayers(layers, method, issueQuantity, ref totalQuantity, ref totalCost);
                    }
                }
            }

            var unitCostFinal = totalQuantity == 0m ? 0m : totalCost / totalQuantity;
            return new InventoryValuationResult
            {
                Quantity = totalQuantity,
                UnitCost = unitCostFinal,
                TotalValue = totalQuantity * unitCostFinal
            };
        }

        private void ConsumeLayers(
            List<InventoryLayer> layers,
            string method,
            decimal issueQuantity,
            ref decimal totalQuantity,
            ref decimal totalCost)
        {
            var orderedLayers = method == "LIFO"
                ? layers.OrderByDescending(l => l.Sequence).ToList()
                : layers.OrderBy(l => l.Sequence).ToList();

            foreach (var layer in orderedLayers)
            {
                if (issueQuantity <= 0m)
                    break;

                var taken = Math.Min(layer.Quantity, issueQuantity);
                layer.Quantity -= taken;
                issueQuantity -= taken;
                totalQuantity -= taken;
                totalCost -= taken * layer.UnitCost;
            }

            layers.RemoveAll(l => l.Quantity <= 0m);
        }

        private bool IsReceipt(INVENTORY_TRANSACTION tx)
        {
            if (tx == null)
                return false;
            if ((tx.QUANTITY ?? 0m) > 0m)
                return true;
            return string.Equals(tx.TRANSACTION_TYPE, "RECEIPT", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(tx.TRANSACTION_TYPE, "PURCHASE", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(tx.TRANSACTION_TYPE, "IN", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsIssue(INVENTORY_TRANSACTION tx)
        {
            if (tx == null)
                return false;
            if ((tx.QUANTITY ?? 0m) < 0m)
                return true;
            return string.Equals(tx.TRANSACTION_TYPE, "ISSUE", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(tx.TRANSACTION_TYPE, "SALE", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(tx.TRANSACTION_TYPE, "OUT", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<PPDMGenericRepository> GetRepoAsync<T>(string tableName, string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(T);

            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, tableName);
        }

        private sealed class InventoryLayer
        {
            private static int _sequenceSeed = 1;
            public int Sequence { get; } = _sequenceSeed++;
            public decimal Quantity { get; set; }
            public decimal UnitCost { get; set; }
        }

        private sealed class InventoryValuationResult
        {
            public decimal Quantity { get; set; }
            public decimal UnitCost { get; set; }
            public decimal TotalValue { get; set; }
        }
    }
}
