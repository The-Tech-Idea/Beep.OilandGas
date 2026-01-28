using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Storage
{
    public class StorageUtilizationDetail : ModelEntityBase
    {
        private string TankBatteryIdValue;

        public string TankBatteryId

        {

            get { return this.TankBatteryIdValue; }

            set { SetProperty(ref TankBatteryIdValue, value); }

        }
        private string BatteryNameValue;

        public string BatteryName

        {

            get { return this.BatteryNameValue; }

            set { SetProperty(ref BatteryNameValue, value); }

        }
        private decimal CapacityValue;

        public decimal Capacity

        {

            get { return this.CapacityValue; }

            set { SetProperty(ref CapacityValue, value); }

        }
        private decimal CurrentInventoryValue;

        public decimal CurrentInventory

        {

            get { return this.CurrentInventoryValue; }

            set { SetProperty(ref CurrentInventoryValue, value); }

        }
        private decimal UtilizationPercentageValue;

        public decimal UtilizationPercentage

        {

            get { return this.UtilizationPercentageValue; }

            set { SetProperty(ref UtilizationPercentageValue, value); }

        }
    }
}
