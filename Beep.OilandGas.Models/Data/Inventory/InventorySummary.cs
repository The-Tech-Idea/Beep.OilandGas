using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Inventory
{
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
