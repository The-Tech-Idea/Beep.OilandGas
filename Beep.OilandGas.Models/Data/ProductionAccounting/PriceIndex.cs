using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class PriceIndex : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the index identifier.
        /// </summary>
        private string IndexIdValue = string.Empty;

        public string IndexId

        {

            get { return this.IndexIdValue; }

            set { SetProperty(ref IndexIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the index name (e.g., "WTI", "Brent", "LLS").
        /// </summary>
        private string IndexNameValue = string.Empty;

        public string IndexName

        {

            get { return this.IndexNameValue; }

            set { SetProperty(ref IndexNameValue, value); }

        }

        /// <summary>
        /// Gets or sets the pricing point.
        /// </summary>
        private string PricingPointValue = string.Empty;

        public string PricingPoint

        {

            get { return this.PricingPointValue; }

            set { SetProperty(ref PricingPointValue, value); }

        }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        private DateTime DateValue;

        public DateTime Date

        {

            get { return this.DateValue; }

            set { SetProperty(ref DateValue, value); }

        }

        /// <summary>
        /// Gets or sets the price per barrel.
        /// </summary>
        private decimal PriceValue;

        public decimal Price

        {

            get { return this.PriceValue; }

            set { SetProperty(ref PriceValue, value); }

        }

        /// <summary>
        /// Gets or sets the unit of measurement.
        /// </summary>
        private string UnitValue = "USD/Barrel";

        public string Unit

        {

            get { return this.UnitValue; }

            set { SetProperty(ref UnitValue, value); }

        }

        /// <summary>
        /// Gets or sets the source of the price.
        /// </summary>

    }
}
