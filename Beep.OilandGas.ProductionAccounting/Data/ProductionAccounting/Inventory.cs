using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class Inventory : ModelEntityBase
    {
        private string InventoryIdValue = string.Empty;

        public string InventoryId

        {

            get { return this.InventoryIdValue; }

            set { SetProperty(ref InventoryIdValue, value); }

        }
        private string LeaseIdValue = string.Empty;

        public string LeaseId

        {

            get { return this.LeaseIdValue; }

            set { SetProperty(ref LeaseIdValue, value); }

        }
        private string? TankBatteryIdValue;

        public string? TankBatteryId

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
        private decimal GrossVolumeValue;

        public decimal GrossVolume

        {

            get { return this.GrossVolumeValue; }

            set { SetProperty(ref GrossVolumeValue, value); }

        }
        private decimal BSWVolumeValue;

        public decimal BSWVolume

        {

            get { return this.BSWVolumeValue; }

            set { SetProperty(ref BSWVolumeValue, value); }

        }
        private decimal NetVolumeValue;

        public decimal NetVolume

        {

            get { return this.NetVolumeValue; }

            set { SetProperty(ref NetVolumeValue, value); }

        }
        private decimal? ApiGravityValue;

        public decimal? ApiGravity

        {

            get { return this.ApiGravityValue; }

            set { SetProperty(ref ApiGravityValue, value); }

        }
    }
}
