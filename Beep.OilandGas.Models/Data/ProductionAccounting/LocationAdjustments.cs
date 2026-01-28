using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class LocationAdjustments : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the location differential.
        /// </summary>
        private decimal LocationDifferentialValue;

        public decimal LocationDifferential

        {

            get { return this.LocationDifferentialValue; }

            set { SetProperty(ref LocationDifferentialValue, value); }

        }

        /// <summary>
        /// Gets or sets the transportation adjustment.
        /// </summary>
        private decimal TransportationAdjustmentValue;

        public decimal TransportationAdjustment

        {

            get { return this.TransportationAdjustmentValue; }

            set { SetProperty(ref TransportationAdjustmentValue, value); }

        }

        /// <summary>
        /// Gets the total location adjustment.
        /// </summary>
        public decimal TotalAdjustment =>
            LocationDifferential +
            TransportationAdjustment;
    }
}
