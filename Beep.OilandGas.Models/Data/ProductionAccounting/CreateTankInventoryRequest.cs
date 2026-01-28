using System;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class CreateTankInventoryRequest : ModelEntityBase
    {
        private string TankBatteryIdValue = string.Empty;

        [Required]
        public string TankBatteryId

        {

            get { return this.TankBatteryIdValue; }

            set { SetProperty(ref TankBatteryIdValue, value); }

        }
        
        private DateTime InventoryDateValue;

        
        [Required]
        public DateTime InventoryDate

        
        {

        
            get { return this.InventoryDateValue; }

        
            set { SetProperty(ref InventoryDateValue, value); }

        
        }
        
        private decimal OpeningInventoryValue;

        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal OpeningInventory

        
        {

        
            get { return this.OpeningInventoryValue; }

        
            set { SetProperty(ref OpeningInventoryValue, value); }

        
        }
        
        private decimal ReceiptsValue;

        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Receipts

        
        {

        
            get { return this.ReceiptsValue; }

        
            set { SetProperty(ref ReceiptsValue, value); }

        
        }
        
        private decimal DeliveriesValue;

        
        [Required]
        [Range(0, double.MaxValue)]
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
        
        private decimal? ActualClosingInventoryValue;

        
        public decimal? ActualClosingInventory

        
        {

        
            get { return this.ActualClosingInventoryValue; }

        
            set { SetProperty(ref ActualClosingInventoryValue, value); }

        
        }
    }
}
