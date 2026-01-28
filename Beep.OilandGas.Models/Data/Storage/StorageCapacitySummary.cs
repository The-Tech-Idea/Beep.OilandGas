using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Storage
{
    public class StorageCapacitySummary : ModelEntityBase
    {
        private string StorageFacilityIdValue;

        public string StorageFacilityId

        {

            get { return this.StorageFacilityIdValue; }

            set { SetProperty(ref StorageFacilityIdValue, value); }

        }
        private string FacilityNameValue;

        public string FacilityName

        {

            get { return this.FacilityNameValue; }

            set { SetProperty(ref FacilityNameValue, value); }

        }
        private decimal TotalCapacityValue;

        public decimal TotalCapacity

        {

            get { return this.TotalCapacityValue; }

            set { SetProperty(ref TotalCapacityValue, value); }

        }
        private decimal CurrentInventoryValue;

        public decimal CurrentInventory

        {

            get { return this.CurrentInventoryValue; }

            set { SetProperty(ref CurrentInventoryValue, value); }

        }
        private decimal AvailableCapacityValue;

        public decimal AvailableCapacity

        {

            get { return this.AvailableCapacityValue; }

            set { SetProperty(ref AvailableCapacityValue, value); }

        }
        private decimal UtilizationPercentageValue;

        public decimal UtilizationPercentage

        {

            get { return this.UtilizationPercentageValue; }

            set { SetProperty(ref UtilizationPercentageValue, value); }

        }
        private int TankBatteryCountValue;

        public int TankBatteryCount

        {

            get { return this.TankBatteryCountValue; }

            set { SetProperty(ref TankBatteryCountValue, value); }

        }
    }
}
