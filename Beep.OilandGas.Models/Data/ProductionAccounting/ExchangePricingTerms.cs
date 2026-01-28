using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class ExchangePricingTerms : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the base price index.
        /// </summary>
        private string? BasePriceIndexValue;

        public string? BasePriceIndex

        {

            get { return this.BasePriceIndexValue; }

            set { SetProperty(ref BasePriceIndexValue, value); }

        }

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
        /// Gets or sets the quality differential.
        /// </summary>
        private decimal QualityDifferentialValue;

        public decimal QualityDifferential

        {

            get { return this.QualityDifferentialValue; }

            set { SetProperty(ref QualityDifferentialValue, value); }

        }

        /// <summary>
        /// Gets or sets the time differential.
        /// </summary>
        private decimal TimeDifferentialValue;

        public decimal TimeDifferential

        {

            get { return this.TimeDifferentialValue; }

            set { SetProperty(ref TimeDifferentialValue, value); }

        }
    }
}
