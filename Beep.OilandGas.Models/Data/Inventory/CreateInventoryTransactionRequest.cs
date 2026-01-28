using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Inventory
{
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
}
