using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class QualityAdjustments : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the API gravity adjustment.
        /// </summary>
        private decimal ApiGravityAdjustmentValue;

        public decimal ApiGravityAdjustment

        {

            get { return this.ApiGravityAdjustmentValue; }

            set { SetProperty(ref ApiGravityAdjustmentValue, value); }

        }

        /// <summary>
        /// Gets or sets the sulfur content adjustment.
        /// </summary>
        private decimal SulfurAdjustmentValue;

        public decimal SulfurAdjustment

        {

            get { return this.SulfurAdjustmentValue; }

            set { SetProperty(ref SulfurAdjustmentValue, value); }

        }

        /// <summary>
        /// Gets or sets the BS&W adjustment.
        /// </summary>
        private decimal BSWAdjustmentValue;

        public decimal BSWAdjustment

        {

            get { return this.BSWAdjustmentValue; }

            set { SetProperty(ref BSWAdjustmentValue, value); }

        }

        /// <summary>
        /// Gets or sets other quality adjustments.
        /// </summary>
        private decimal OtherAdjustmentsValue;

        public decimal OtherAdjustments

        {

            get { return this.OtherAdjustmentsValue; }

            set { SetProperty(ref OtherAdjustmentsValue, value); }

        }

        /// <summary>
        /// Gets the total quality adjustment.
        /// </summary>
        public decimal TotalAdjustment =>
            ApiGravityAdjustment +
            SulfurAdjustment +
            BSWAdjustment +
            OtherAdjustments;
    }
}
