using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class RegulatedPrice : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the regulated price identifier.
        /// </summary>
        private string RegulatedPriceIdValue = string.Empty;

        public string RegulatedPriceId

        {

            get { return this.RegulatedPriceIdValue; }

            set { SetProperty(ref RegulatedPriceIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the regulatory authority.
        /// </summary>
        private string RegulatoryAuthorityValue = string.Empty;

        public string RegulatoryAuthority

        {

            get { return this.RegulatoryAuthorityValue; }

            set { SetProperty(ref RegulatoryAuthorityValue, value); }

        }

        /// <summary>
        /// Gets or sets the price formula.
        /// </summary>
        private string PriceFormulaValue = string.Empty;

        public string PriceFormula

        {

            get { return this.PriceFormulaValue; }

            set { SetProperty(ref PriceFormulaValue, value); }

        }

        /// <summary>
        /// Gets or sets the effective start date.
        /// </summary>
        private DateTime EffectiveStartDateValue;

        public DateTime EffectiveStartDate

        {

            get { return this.EffectiveStartDateValue; }

            set { SetProperty(ref EffectiveStartDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the effective end date.
        /// </summary>
        private DateTime? EffectiveEndDateValue;

        public DateTime? EffectiveEndDate

        {

            get { return this.EffectiveEndDateValue; }

            set { SetProperty(ref EffectiveEndDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the price cap per barrel.
        /// </summary>
        private decimal? PriceCapValue;

        public decimal? PriceCap

        {

            get { return this.PriceCapValue; }

            set { SetProperty(ref PriceCapValue, value); }

        }

        /// <summary>
        /// Gets or sets the price floor per barrel.
        /// </summary>
        private decimal? PriceFloorValue;

        public decimal? PriceFloor

        {

            get { return this.PriceFloorValue; }

            set { SetProperty(ref PriceFloorValue, value); }

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
        /// Gets or sets the adjustment factors.
        /// </summary>
        public Dictionary<string, decimal> AdjustmentFactors { get; set; } = new();
    }
}
