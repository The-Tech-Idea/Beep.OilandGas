using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class RunTicketValuationModel : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the valuation identifier.
        /// </summary>
        private string ValuationIdValue = string.Empty;

        public string ValuationId

        {

            get { return this.ValuationIdValue; }

            set { SetProperty(ref ValuationIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the run ticket reference.
        /// </summary>
        private string RunTicketNumberValue = string.Empty;

        public string RunTicketNumber

        {

            get { return this.RunTicketNumberValue; }

            set { SetProperty(ref RunTicketNumberValue, value); }

        }

        /// <summary>
        /// Gets or sets the valuation date.
        /// </summary>
        private DateTime ValuationDateValue;

        public DateTime ValuationDate

        {

            get { return this.ValuationDateValue; }

            set { SetProperty(ref ValuationDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the base price per barrel.
        /// </summary>
        private decimal BasePriceValue;

        public decimal BasePrice

        {

            get { return this.BasePriceValue; }

            set { SetProperty(ref BasePriceValue, value); }

        }

        /// <summary>
        /// Gets or sets the quality adjustments.
        /// </summary>
        private QUALITY_ADJUSTMENTS QualityAdjustmentsValue = new();

        public QUALITY_ADJUSTMENTS QUALITY_ADJUSTMENTS

        {

            get { return this.QualityAdjustmentsValue; }

            set { SetProperty(ref QualityAdjustmentsValue, value); }

        }

        /// <summary>
        /// Gets or sets the location adjustments.
        /// </summary>
        private LOCATION_ADJUSTMENTS LocationAdjustmentsValue = new();

        public LOCATION_ADJUSTMENTS LOCATION_ADJUSTMENTS

        {

            get { return this.LocationAdjustmentsValue; }

            set { SetProperty(ref LocationAdjustmentsValue, value); }

        }

        /// <summary>
        /// Gets or sets the time adjustments.
        /// </summary>
        private TIME_ADJUSTMENTS TimeAdjustmentsValue = new();

        public TIME_ADJUSTMENTS TIME_ADJUSTMENTS

        {

            get { return this.TimeAdjustmentsValue; }

            set { SetProperty(ref TimeAdjustmentsValue, value); }

        }

        /// <summary>
        /// Gets the total adjustments.
        /// </summary>
        public decimal TotalAdjustments =>
            (QUALITY_ADJUSTMENTS.TOTAL_ADJUSTMENT ) +
            (LOCATION_ADJUSTMENTS.TOTAL_ADJUSTMENT ) +
            (TIME_ADJUSTMENTS.TOTAL_ADJUSTMENT );

        /// <summary>
        /// Gets the adjusted price per barrel.
        /// </summary>
        public decimal AdjustedPrice => BasePrice + TotalAdjustments;

        /// <summary>
        /// Gets or sets the net volume in barrels.
        /// </summary>
        private decimal NetVolumeValue;

        public decimal NetVolume

        {

            get { return this.NetVolumeValue; }

            set { SetProperty(ref NetVolumeValue, value); }

        }

        /// <summary>
        /// Gets the total value.
        /// </summary>
        public decimal TotalValue => NetVolume * AdjustedPrice;

        /// <summary>
        /// Gets or sets the pricing method used.
        /// </summary>
        private PricingMethod PricingMethodValue;

        public PricingMethod PricingMethod

        {

            get { return this.PricingMethodValue; }

            set { SetProperty(ref PricingMethodValue, value); }

        }
    }
}
