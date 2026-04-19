using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class TankBattery : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the tank battery identifier.
        /// </summary>
        private string TankBatteryIdValue = string.Empty;

        public string TankBatteryId

        {

            get { return this.TankBatteryIdValue; }

            set { SetProperty(ref TankBatteryIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the tank battery name.
        /// </summary>
        private string TankBatteryNameValue = string.Empty;

        public string TankBatteryName

        {

            get { return this.TankBatteryNameValue; }

            set { SetProperty(ref TankBatteryNameValue, value); }

        }

        /// <summary>
        /// Gets or sets the property or lease identifier.
        /// </summary>
        private string? PropertyOrLeaseIdValue;

        public string? PropertyOrLeaseId

        {

            get { return this.PropertyOrLeaseIdValue; }

            set { SetProperty(ref PropertyOrLeaseIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the tanks in this battery.
        /// </summary>
        private List<Tank> TanksValue = new();

        public List<Tank> Tanks

        {

            get { return this.TanksValue; }

            set { SetProperty(ref TanksValue, value); }

        }

        /// <summary>
        /// Gets the total capacity in barrels.
        /// </summary>
        public decimal TotalCapacity => Tanks.Sum(t => t.Capacity);

        /// <summary>
        /// Gets the current total inventory in barrels.
        /// </summary>
        public decimal CurrentInventory => Tanks.Sum(t => t.CurrentVolume);
    }
}
