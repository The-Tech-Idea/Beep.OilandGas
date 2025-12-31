using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs.Inventory
{
    public enum ValuationMethod
    {
        FIFO,           // First In, First Out
        LIFO,           // Last In, First Out
        WeightedAverage, // Weighted Average Cost
        LCM             // Lower of Cost or Market
    }

    public class CreateInventoryItemRequest
    {
        public string ItemNumber { get; set; }
        public string ItemName { get; set; }
        public string ItemType { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal? UnitCost { get; set; }
        public string ValuationMethod { get; set; } = "FIFO";
    }

    public class UpdateInventoryItemRequest
    {
        public string InventoryItemId { get; set; }
        public string ItemNumber { get; set; }
        public string ItemName { get; set; }
        public string ItemType { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal? UnitCost { get; set; }
        public string ValuationMethod { get; set; }
    }

    public class CreateInventoryTransactionRequest
    {
        public string InventoryItemId { get; set; }
        public string TransactionType { get; set; } // Receipt, Issue, Transfer, Adjustment
        public DateTime TransactionDate { get; set; }
        public decimal Quantity { get; set; }
        public decimal? UnitCost { get; set; }
        public string ReferenceNumber { get; set; }
        public string Description { get; set; }
    }

    public class CreateInventoryAdjustmentRequest
    {
        public string InventoryItemId { get; set; }
        public DateTime AdjustmentDate { get; set; }
        public string AdjustmentType { get; set; } // Increase, Decrease
        public decimal QuantityAdjustment { get; set; }
        public decimal? UnitCostAdjustment { get; set; }
        public string Reason { get; set; }
        public string Description { get; set; }
    }

    public class InventoryReconciliationResult
    {
        public string InventoryItemId { get; set; }
        public DateTime ReconciliationDate { get; set; }
        public decimal BookQuantity { get; set; }
        public decimal PhysicalQuantity { get; set; }
        public decimal Variance { get; set; }
        public decimal VarianceAmount { get; set; }
        public bool RequiresAdjustment { get; set; }
        public string AdjustmentId { get; set; }
    }

    public class InventorySummary
    {
        public string InventoryItemId { get; set; }
        public string ItemNumber { get; set; }
        public string ItemName { get; set; }
        public decimal QuantityOnHand { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalValue { get; set; }
        public int TransactionCount { get; set; }
        public DateTime? LastTransactionDate { get; set; }
        public DateTime? LastReconciliationDate { get; set; }
        public bool RequiresReconciliation { get; set; }
    }
}

