using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class TimeAdjustments : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the time differential.
        /// </summary>
        private decimal TimeDifferentialValue;

        public decimal TimeDifferential

        {

            get { return this.TimeDifferentialValue; }

            set { SetProperty(ref TimeDifferentialValue, value); }

        }

        /// <summary>
        /// Gets or sets the interest adjustment (if applicable).
        /// </summary>
        private decimal InterestAdjustmentValue;

        public decimal InterestAdjustment

        {

            get { return this.InterestAdjustmentValue; }

            set { SetProperty(ref InterestAdjustmentValue, value); }

        }

        /// <summary>
        /// Gets the total time adjustment.
        /// </summary>
        public decimal TotalAdjustment =>
            TimeDifferential +
            InterestAdjustment;
    }
}
