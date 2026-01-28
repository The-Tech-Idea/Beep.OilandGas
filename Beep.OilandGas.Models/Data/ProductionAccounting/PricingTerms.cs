using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class PricingTerms : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the pricing method.
        /// </summary>
        private PricingMethod PricingMethodValue;

        public PricingMethod PricingMethod

        {

            get { return this.PricingMethodValue; }

            set { SetProperty(ref PricingMethodValue, value); }

        }

        /// <summary>
        /// Gets or sets the base price (if fixed).
        /// </summary>
        private decimal? BasePriceValue;

        public decimal? BasePrice

        {

            get { return this.BasePriceValue; }

            set { SetProperty(ref BasePriceValue, value); }

        }

        /// <summary>
        /// Gets or sets the price index (if index-based).
        /// </summary>
        private string? PriceIndexValue;

        public string? PriceIndex

        {

            get { return this.PriceIndexValue; }

            set { SetProperty(ref PriceIndexValue, value); }

        }

        /// <summary>
        /// Gets or sets the differential (premium or discount).
        /// </summary>
        private decimal DifferentialValue;

        public decimal Differential

        {

            get { return this.DifferentialValue; }

            set { SetProperty(ref DifferentialValue, value); }

        }

        /// <summary>
        /// Gets or sets the API gravity differential per degree.
        /// </summary>
        private decimal? ApiGravityDifferentialValue;

        public decimal? ApiGravityDifferential

        {

            get { return this.ApiGravityDifferentialValue; }

            set { SetProperty(ref ApiGravityDifferentialValue, value); }

        }

        /// <summary>
        /// Gets or sets the sulfur content differential per 0.1%.
        /// </summary>
        private decimal? SulfurDifferentialValue;

        public decimal? SulfurDifferential

        {

            get { return this.SulfurDifferentialValue; }

            set { SetProperty(ref SulfurDifferentialValue, value); }

        }

        /// <summary>
        /// Gets or sets the location differential.
        /// </summary>
        private decimal? LocationDifferentialValue;

        public decimal? LocationDifferential

        {

            get { return this.LocationDifferentialValue; }

            set { SetProperty(ref LocationDifferentialValue, value); }

        }
    }
}
