namespace Beep.OilandGas.Models.Data.Accounting
{
    public class InventoryItem : ModelEntityBase
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
        private decimal? QuantityOnHandValue;

        public decimal? QuantityOnHand

        {

            get { return this.QuantityOnHandValue; }

            set { SetProperty(ref QuantityOnHandValue, value); }

        }
        private decimal? UnitCostValue;

        public decimal? UnitCost

        {

            get { return this.UnitCostValue; }

            set { SetProperty(ref UnitCostValue, value); }

        }
        private decimal? TotalValueValue;

        public decimal? TotalValue

        {

            get { return this.TotalValueValue; }

            set { SetProperty(ref TotalValueValue, value); }

        }
        private string ValuationMethodValue;

        public string ValuationMethod

        {

            get { return this.ValuationMethodValue; }

            set { SetProperty(ref ValuationMethodValue, value); }

        }
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
        private string ValuationMethodValue;

        public string ValuationMethod

        {

            get { return this.ValuationMethodValue; }

            set { SetProperty(ref ValuationMethodValue, value); }

        }
    }

    public class InventoryItemResponse : InventoryItem
    {
    }
}






