using System;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class TankInventoryResponse : ModelEntityBase
    {
        private string InventoryIdValue = string.Empty;

        public string InventoryId

        {

            get { return this.InventoryIdValue; }

            set { SetProperty(ref InventoryIdValue, value); }

        }
        private string TankBatteryIdValue = string.Empty;

        public string TankBatteryId

        {

            get { return this.TankBatteryIdValue; }

            set { SetProperty(ref TankBatteryIdValue, value); }

        }
        private DateTime InventoryDateValue;

        public DateTime InventoryDate

        {

            get { return this.InventoryDateValue; }

            set { SetProperty(ref InventoryDateValue, value); }

        }
        private decimal OpeningInventoryValue;

        public decimal OpeningInventory

        {

            get { return this.OpeningInventoryValue; }

            set { SetProperty(ref OpeningInventoryValue, value); }

        }
        private decimal ReceiptsValue;

        public decimal Receipts

        {

            get { return this.ReceiptsValue; }

            set { SetProperty(ref ReceiptsValue, value); }

        }
        private decimal DeliveriesValue;

        public decimal Deliveries

        {

            get { return this.DeliveriesValue; }

            set { SetProperty(ref DeliveriesValue, value); }

        }
        private decimal AdjustmentsValue;

        public decimal Adjustments

        {

            get { return this.AdjustmentsValue; }

            set { SetProperty(ref AdjustmentsValue, value); }

        }
        private decimal ShrinkageValue;

        public decimal Shrinkage

        {

            get { return this.ShrinkageValue; }

            set { SetProperty(ref ShrinkageValue, value); }

        }
        private decimal TheftLossValue;

        public decimal TheftLoss

        {

            get { return this.TheftLossValue; }

            set { SetProperty(ref TheftLossValue, value); }

        }
        private decimal ClosingInventoryValue;

        public decimal ClosingInventory

        {

            get { return this.ClosingInventoryValue; }

            set { SetProperty(ref ClosingInventoryValue, value); }

        }
        private decimal? ActualClosingInventoryValue;

        public decimal? ActualClosingInventory

        {

            get { return this.ActualClosingInventoryValue; }

            set { SetProperty(ref ActualClosingInventoryValue, value); }

        }
        private decimal? InventoryVarianceValue;

        public decimal? InventoryVariance

        {

            get { return this.InventoryVarianceValue; }

            set { SetProperty(ref InventoryVarianceValue, value); }

        }
    }
}
