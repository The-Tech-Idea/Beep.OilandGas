using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Inventory
{
    public enum ValuationMethod
    {
        FIFO,           // First In, First Out
        LIFO,           // Last In, First Out
        WeightedAverage, // Weighted Average Cost
        LCM             // Lower of Cost or Market
    }

    public class CreateInventoryItemRequest : ModelEntityBase
    {
        private string ItemNumberValue;

        public string ItemNumber

        {

            get { return this.ItemNumberValue; }

            set { SetProperty(ref ItemNumberValue, value); }

        }
        private string ItemNameValue;

        public string ItemName

        {

            get { return this.ItemNameValue; }

            set { SetProperty(ref ItemNameValue, value); }

        }
        private string ItemTypeValue;

        public string ItemType

        {

            get { return this.ItemTypeValue; }

            set { SetProperty(ref ItemTypeValue, value); }

        }
        private string UnitOfMeasureValue;

        public string UnitOfMeasure

        {

            get { return this.UnitOfMeasureValue; }

            set { SetProperty(ref UnitOfMeasureValue, value); }

        }
        private decimal? UnitCostValue;

        public decimal? UnitCost

        {

            get { return this.UnitCostValue; }

            set { SetProperty(ref UnitCostValue, value); }

        }
        private string ValuationMethodValue = "FIFO";

        public string ValuationMethod

        {

            get { return this.ValuationMethodValue; }

            set { SetProperty(ref ValuationMethodValue, value); }

        }
    }

    public class UpdateInventoryItemRequest : ModelEntityBase
    {
        private string InventoryItemIdValue;

        public string InventoryItemId

        {

            get { return this.InventoryItemIdValue; }

            set { SetProperty(ref InventoryItemIdValue, value); }

        }
        private string ItemNumberValue;

        public string ItemNumber

        {

            get { return this.ItemNumberValue; }

            set { SetProperty(ref ItemNumberValue, value); }

        }
        private string ItemNameValue;

        public string ItemName

        {

            get { return this.ItemNameValue; }

            set { SetProperty(ref ItemNameValue, value); }

        }
        private string ItemTypeValue;

        public string ItemType

        {

            get { return this.ItemTypeValue; }

            set { SetProperty(ref ItemTypeValue, value); }

        }
        private string UnitOfMeasureValue;

        public string UnitOfMeasure

        {

            get { return this.UnitOfMeasureValue; }

            set { SetProperty(ref UnitOfMeasureValue, value); }

        }
        private decimal? UnitCostValue;

        public decimal? UnitCost

        {

            get { return this.UnitCostValue; }

            set { SetProperty(ref UnitCostValue, value); }

        }
        private string ValuationMethodValue;

        public string ValuationMethod

        {

            get { return this.ValuationMethodValue; }

            set { SetProperty(ref ValuationMethodValue, value); }

        }
    }

    public class CreateInventoryTransactionRequest : ModelEntityBase
    {
        private string InventoryItemIdValue;

        public string InventoryItemId

        {

            get { return this.InventoryItemIdValue; }

            set { SetProperty(ref InventoryItemIdValue, value); }

        }
        private string TransactionTypeValue;

        public string TransactionType

        {

            get { return this.TransactionTypeValue; }

            set { SetProperty(ref TransactionTypeValue, value); }

        } // Receipt, Issue, Transfer, Adjustment
        private DateTime TransactionDateValue;

        public DateTime TransactionDate

        {

            get { return this.TransactionDateValue; }

            set { SetProperty(ref TransactionDateValue, value); }

        }
        private decimal QuantityValue;

        public decimal Quantity

        {

            get { return this.QuantityValue; }

            set { SetProperty(ref QuantityValue, value); }

        }
        private decimal? UnitCostValue;

        public decimal? UnitCost

        {

            get { return this.UnitCostValue; }

            set { SetProperty(ref UnitCostValue, value); }

        }
        private string ReferenceNumberValue;

        public string ReferenceNumber

        {

            get { return this.ReferenceNumberValue; }

            set { SetProperty(ref ReferenceNumberValue, value); }

        }
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }

    public class CreateInventoryAdjustmentRequest : ModelEntityBase
    {
        private string InventoryItemIdValue;

        public string InventoryItemId

        {

            get { return this.InventoryItemIdValue; }

            set { SetProperty(ref InventoryItemIdValue, value); }

        }
        private DateTime AdjustmentDateValue;

        public DateTime AdjustmentDate

        {

            get { return this.AdjustmentDateValue; }

            set { SetProperty(ref AdjustmentDateValue, value); }

        }
        private string AdjustmentTypeValue;

        public string AdjustmentType

        {

            get { return this.AdjustmentTypeValue; }

            set { SetProperty(ref AdjustmentTypeValue, value); }

        } // Increase, Decrease
        private decimal QuantityAdjustmentValue;

        public decimal QuantityAdjustment

        {

            get { return this.QuantityAdjustmentValue; }

            set { SetProperty(ref QuantityAdjustmentValue, value); }

        }
        private decimal? UnitCostAdjustmentValue;

        public decimal? UnitCostAdjustment

        {

            get { return this.UnitCostAdjustmentValue; }

            set { SetProperty(ref UnitCostAdjustmentValue, value); }

        }
        private string ReasonValue;

        public string Reason

        {

            get { return this.ReasonValue; }

            set { SetProperty(ref ReasonValue, value); }

        }
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }

    public class InventoryReconciliationResult : ModelEntityBase
    {
        private string InventoryItemIdValue;

        public string InventoryItemId

        {

            get { return this.InventoryItemIdValue; }

            set { SetProperty(ref InventoryItemIdValue, value); }

        }
        private DateTime ReconciliationDateValue;

        public DateTime ReconciliationDate

        {

            get { return this.ReconciliationDateValue; }

            set { SetProperty(ref ReconciliationDateValue, value); }

        }
        private decimal BookQuantityValue;

        public decimal BookQuantity

        {

            get { return this.BookQuantityValue; }

            set { SetProperty(ref BookQuantityValue, value); }

        }
        private decimal PhysicalQuantityValue;

        public decimal PhysicalQuantity

        {

            get { return this.PhysicalQuantityValue; }

            set { SetProperty(ref PhysicalQuantityValue, value); }

        }
        private decimal VarianceValue;

        public decimal Variance

        {

            get { return this.VarianceValue; }

            set { SetProperty(ref VarianceValue, value); }

        }
        private decimal VarianceAmountValue;

        public decimal VarianceAmount

        {

            get { return this.VarianceAmountValue; }

            set { SetProperty(ref VarianceAmountValue, value); }

        }
        private bool RequiresAdjustmentValue;

        public bool RequiresAdjustment

        {

            get { return this.RequiresAdjustmentValue; }

            set { SetProperty(ref RequiresAdjustmentValue, value); }

        }
        private string AdjustmentIdValue;

        public string AdjustmentId

        {

            get { return this.AdjustmentIdValue; }

            set { SetProperty(ref AdjustmentIdValue, value); }

        }
    }

    public class InventorySummary : ModelEntityBase
    {
        private string InventoryItemIdValue;

        public string InventoryItemId

        {

            get { return this.InventoryItemIdValue; }

            set { SetProperty(ref InventoryItemIdValue, value); }

        }
        private string ItemNumberValue;

        public string ItemNumber

        {

            get { return this.ItemNumberValue; }

            set { SetProperty(ref ItemNumberValue, value); }

        }
        private string ItemNameValue;

        public string ItemName

        {

            get { return this.ItemNameValue; }

            set { SetProperty(ref ItemNameValue, value); }

        }
        private decimal QuantityOnHandValue;

        public decimal QuantityOnHand

        {

            get { return this.QuantityOnHandValue; }

            set { SetProperty(ref QuantityOnHandValue, value); }

        }
        private decimal UnitCostValue;

        public decimal UnitCost

        {

            get { return this.UnitCostValue; }

            set { SetProperty(ref UnitCostValue, value); }

        }
        private decimal TotalValueValue;

        public decimal TotalValue

        {

            get { return this.TotalValueValue; }

            set { SetProperty(ref TotalValueValue, value); }

        }
        private int TransactionCountValue;

        public int TransactionCount

        {

            get { return this.TransactionCountValue; }

            set { SetProperty(ref TransactionCountValue, value); }

        }
        private DateTime? LastTransactionDateValue;

        public DateTime? LastTransactionDate

        {

            get { return this.LastTransactionDateValue; }

            set { SetProperty(ref LastTransactionDateValue, value); }

        }
        private DateTime? LastReconciliationDateValue;

        public DateTime? LastReconciliationDate

        {

            get { return this.LastReconciliationDateValue; }

            set { SetProperty(ref LastReconciliationDateValue, value); }

        }
        private bool RequiresReconciliationValue;

        public bool RequiresReconciliation

        {

            get { return this.RequiresReconciliationValue; }

            set { SetProperty(ref RequiresReconciliationValue, value); }

        }
    }
}








