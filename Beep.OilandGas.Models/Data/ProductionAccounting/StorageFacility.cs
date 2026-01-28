using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class StorageFacility : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the facility identifier.
        /// </summary>
        private string FacilityIdValue = string.Empty;

        public string FacilityId

        {

            get { return this.FacilityIdValue; }

            set { SetProperty(ref FacilityIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the facility name.
        /// </summary>
        private string FacilityNameValue = string.Empty;

        public string FacilityName

        {

            get { return this.FacilityNameValue; }

            set { SetProperty(ref FacilityNameValue, value); }

        }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        private string LocationValue = string.Empty;

        public string Location

        {

            get { return this.LocationValue; }

            set { SetProperty(ref LocationValue, value); }

        }

        /// <summary>
        /// Gets or sets the total capacity in barrels.
        /// </summary>
        private decimal TotalCapacityValue;

        public decimal TotalCapacity

        {

            get { return this.TotalCapacityValue; }

            set { SetProperty(ref TotalCapacityValue, value); }

        }

        /// <summary>
        /// Gets or sets the tanks in this facility.
        /// </summary>
        private List<Tank> TanksValue = new();

        public List<Tank> Tanks

        {

            get { return this.TanksValue; }

            set { SetProperty(ref TanksValue, value); }

        }

        /// <summary>
        /// Gets the current total inventory in barrels.
        /// </summary>
        public decimal CurrentInventory => Tanks.Sum(t => t.CurrentVolume);

        /// <summary>
        /// Gets the available capacity in barrels.
        /// </summary>
        public decimal AvailableCapacity => TotalCapacity - CurrentInventory;

        /// <summary>
        /// Gets the utilization percentage (0-100).
        /// </summary>
        public decimal UtilizationPercentage => TotalCapacity > 0
            ? (CurrentInventory / TotalCapacity) * 100m
            : 0m;
    }
}
