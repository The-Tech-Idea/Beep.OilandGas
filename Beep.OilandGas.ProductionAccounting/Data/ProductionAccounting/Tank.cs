using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class Tank : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the tank number.
        /// </summary>
        private string TankNumberValue = string.Empty;

        public string TankNumber

        {

            get { return this.TankNumberValue; }

            set { SetProperty(ref TankNumberValue, value); }

        }

        /// <summary>
        /// Gets or sets the tank capacity in barrels.
        /// </summary>
        private decimal CapacityValue;

        public decimal Capacity

        {

            get { return this.CapacityValue; }

            set { SetProperty(ref CapacityValue, value); }

        }

        /// <summary>
        /// Gets or sets the current volume in barrels.
        /// </summary>
        private decimal CurrentVolumeValue;

        public decimal CurrentVolume

        {

            get { return this.CurrentVolumeValue; }

            set { SetProperty(ref CurrentVolumeValue, value); }

        }

        /// <summary>
        /// Gets or sets the temperature in degrees Fahrenheit.
        /// </summary>
        private decimal TemperatureValue = 60m;

        public decimal Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        }

        /// <summary>
        /// Gets or sets the BS&W content as percentage (0-100).
        /// </summary>
        private decimal BSWValue;

        public decimal BSW

        {

            get { return this.BSWValue; }

            set { SetProperty(ref BSWValue, value); }

        }

        /// <summary>
        /// Gets or sets the API gravity of the oil in the tank.
        /// </summary>
        private decimal? ApiGravityValue;

        public decimal? ApiGravity

        {

            get { return this.ApiGravityValue; }

            set { SetProperty(ref ApiGravityValue, value); }

        }

        /// <summary>
        /// Gets the available capacity in barrels.
        /// </summary>
        public decimal AvailableCapacity => Capacity - CurrentVolume;

        /// <summary>
        /// Gets the utilization percentage (0-100).
        /// </summary>
        public decimal UtilizationPercentage => Capacity > 0
            ? (CurrentVolume / Capacity) * 100m
            : 0m;
    }
}
