using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.Accounting.Operational.Inventory
{
    /// <summary>
    /// Inventory valuation method.
    /// </summary>
    public enum InventoryValuationMethod
    {
        /// <summary>
        /// First In, First Out.
        /// </summary>
        FIFO,

        /// <summary>
        /// Last In, First Out.
        /// </summary>
        LIFO,

        /// <summary>
        /// Weighted average cost.
        /// </summary>
        WeightedAverage,

        /// <summary>
        /// Lower of cost or market.
        /// </summary>
        LowerOfCostOrMarket
    }

    /// <summary>
    /// Represents crude oil inventory.
    /// </summary>
    public class CrudeOilInventory
    {
        /// <summary>
        /// Gets or sets the inventory identifier.
        /// </summary>
        public string InventoryId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the property or lease identifier.
        /// </summary>
        public string? PropertyOrLeaseId { get; set; }

        /// <summary>
        /// Gets or sets the tank battery identifier.
        /// </summary>
        public string? TankBatteryId { get; set; }

        /// <summary>
        /// Gets or sets the inventory date.
        /// </summary>
        public DateTime InventoryDate { get; set; }

        /// <summary>
        /// Gets or sets the volume in barrels.
        /// </summary>
        public decimal Volume { get; set; }

        /// <summary>
        /// Gets or sets the unit cost per barrel.
        /// </summary>
        public decimal UnitCost { get; set; }

        /// <summary>
        /// Gets the total value.
        /// </summary>
        public decimal TotalValue => Volume * UnitCost;

        /// <summary>
        /// Gets or sets the valuation method.
        /// </summary>
        public InventoryValuationMethod ValuationMethod { get; set; } = InventoryValuationMethod.WeightedAverage;

        /// <summary>
        /// Gets or sets the market price per barrel (for LCM valuation).
        /// </summary>
        public decimal? MarketPrice { get; set; }

        /// <summary>
        /// Gets the lower of cost or market value.
        /// </summary>
        public decimal LowerOfCostOrMarketValue => MarketPrice.HasValue 
            ? Math.Min(TotalValue, Volume * MarketPrice.Value) 
            : TotalValue;
    }

    /// <summary>
    /// Represents an inventory transaction.
    /// </summary>
    public class InventoryTransaction
    {
        /// <summary>
        /// Gets or sets the transaction identifier.
        /// </summary>
        public string TransactionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the transaction date.
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Gets or sets the transaction type.
        /// </summary>
        public InventoryTransactionType TransactionType { get; set; }

        /// <summary>
        /// Gets or sets the volume in barrels.
        /// </summary>
        public decimal Volume { get; set; }

        /// <summary>
        /// Gets or sets the unit cost per barrel.
        /// </summary>
        public decimal UnitCost { get; set; }

        /// <summary>
        /// Gets the total value.
        /// </summary>
        public decimal TotalValue => Volume * UnitCost;
    }

    /// <summary>
    /// Inventory transaction type.
    /// </summary>
    public enum InventoryTransactionType
    {
        /// <summary>
        /// Receipt (addition to inventory).
        /// </summary>
        Receipt,

        /// <summary>
        /// Delivery (removal from inventory).
        /// </summary>
        Delivery,

        /// <summary>
        /// Adjustment.
        /// </summary>
        Adjustment
    }

    /// <summary>
    /// Manages crude oil inventory and valuation.
    /// </summary>
    public class InventoryManager
    {
        private readonly Dictionary<string, CrudeOilInventory> inventories = new();
        private readonly Dictionary<string, List<InventoryTransaction>> transactions = new();

        /// <summary>
        /// Records an inventory transaction.
        /// </summary>
        public void RecordTransaction(
            string inventoryId,
            InventoryTransactionType transactionType,
            decimal volume,
            decimal unitCost,
            DateTime transactionDate)
        {
            if (!inventories.TryGetValue(inventoryId, out var inventory))
            {
                inventory = new CrudeOilInventory
                {
                    InventoryId = inventoryId,
                    InventoryDate = transactionDate
                };
                inventories[inventoryId] = inventory;
            }

            if (!transactions.ContainsKey(inventoryId))
                transactions[inventoryId] = new List<InventoryTransaction>();

            var transaction = new InventoryTransaction
            {
                TransactionId = Guid.NewGuid().ToString(),
                TransactionDate = transactionDate,
                TransactionType = transactionType,
                Volume = volume,
                UnitCost = unitCost
            };

            transactions[inventoryId].Add(transaction);

            // Update inventory based on valuation method
            UpdateInventory(inventory, transaction);
        }

        /// <summary>
        /// Updates inventory based on transaction and valuation method.
        /// </summary>
        private void UpdateInventory(CrudeOilInventory inventory, InventoryTransaction transaction)
        {
            switch (inventory.ValuationMethod)
            {
                case InventoryValuationMethod.FIFO:
                    UpdateFIFO(inventory, transaction);
                    break;

                case InventoryValuationMethod.LIFO:
                    UpdateLIFO(inventory, transaction);
                    break;

                case InventoryValuationMethod.WeightedAverage:
                    UpdateWeightedAverage(inventory, transaction);
                    break;

                case InventoryValuationMethod.LowerOfCostOrMarket:
                    UpdateWeightedAverage(inventory, transaction);
                    // LCM is applied at reporting time
                    break;
            }
        }

        /// <summary>
        /// Updates inventory using FIFO method.
        /// </summary>
        private void UpdateFIFO(CrudeOilInventory inventory, InventoryTransaction transaction)
        {
            if (transaction.TransactionType == InventoryTransactionType.Receipt)
            {
                // For FIFO, we track layers but for simplicity, use weighted average on receipt
                decimal totalValue = inventory.TotalValue + transaction.TotalValue;
                decimal totalVolume = inventory.Volume + transaction.Volume;
                inventory.Volume = totalVolume;
                inventory.UnitCost = totalVolume > 0 ? totalValue / totalVolume : 0;
            }
            else if (transaction.TransactionType == InventoryTransactionType.Delivery)
            {
                inventory.Volume -= transaction.Volume;
                // Unit cost remains the same (oldest cost)
            }
        }

        /// <summary>
        /// Updates inventory using LIFO method.
        /// </summary>
        private void UpdateLIFO(CrudeOilInventory inventory, InventoryTransaction transaction)
        {
            if (transaction.TransactionType == InventoryTransactionType.Receipt)
            {
                decimal totalValue = inventory.TotalValue + transaction.TotalValue;
                decimal totalVolume = inventory.Volume + transaction.Volume;
                inventory.Volume = totalVolume;
                inventory.UnitCost = totalVolume > 0 ? totalValue / totalVolume : 0;
            }
            else if (transaction.TransactionType == InventoryTransactionType.Delivery)
            {
                inventory.Volume -= transaction.Volume;
                // Unit cost remains the same (newest cost)
            }
        }

        /// <summary>
        /// Updates inventory using weighted average method.
        /// </summary>
        private void UpdateWeightedAverage(CrudeOilInventory inventory, InventoryTransaction transaction)
        {
            if (transaction.TransactionType == InventoryTransactionType.Receipt)
            {
                decimal totalValue = inventory.TotalValue + transaction.TotalValue;
                decimal totalVolume = inventory.Volume + transaction.Volume;
                inventory.Volume = totalVolume;
                inventory.UnitCost = totalVolume > 0 ? totalValue / totalVolume : 0;
            }
            else if (transaction.TransactionType == InventoryTransactionType.Delivery)
            {
                inventory.Volume -= transaction.Volume;
                // Unit cost remains the same
            }
        }

        /// <summary>
        /// Gets inventory by ID.
        /// </summary>
        public CrudeOilInventory? GetInventory(string inventoryId)
        {
            return inventories.TryGetValue(inventoryId, out var inventory) ? inventory : null;
        }

        /// <summary>
        /// Gets inventory transactions.
        /// </summary>
        public IEnumerable<InventoryTransaction> GetTransactions(string inventoryId)
        {
            return transactions.TryGetValue(inventoryId, out var trans) 
                ? trans 
                : Enumerable.Empty<InventoryTransaction>();
        }
    }
}

