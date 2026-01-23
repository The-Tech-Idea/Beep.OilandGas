using System;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Accounting.Traditional
{
    /// <summary>
    /// Request DTO for creating an inventory transaction
    /// </summary>
    public class CreateInventoryTransactionRequest : ModelEntityBase
    {
        private string InventoryItemIdValue = string.Empty;

        public string InventoryItemId

        {

            get { return this.InventoryItemIdValue; }

            set { SetProperty(ref InventoryItemIdValue, value); }

        }
        private string TransactionTypeValue = string.Empty;

        public string TransactionType

        {

            get { return this.TransactionTypeValue; }

            set { SetProperty(ref TransactionTypeValue, value); }

        } // Receipt, Issue, Adjustment
        private DateTime? TransactionDateValue;

        public DateTime? TransactionDate

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
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }
}







