using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Pricing
{
    public class CreatePriceIndexRequest : ModelEntityBase
    {
        private string IndexNameValue;

        public string IndexName

        {

            get { return this.IndexNameValue; }

            set { SetProperty(ref IndexNameValue, value); }

        }
        private string CommodityTypeValue;

        public string CommodityType

        {

            get { return this.CommodityTypeValue; }

            set { SetProperty(ref CommodityTypeValue, value); }

        }
        private DateTime PriceDateValue;

        public DateTime PriceDate

        {

            get { return this.PriceDateValue; }

            set { SetProperty(ref PriceDateValue, value); }

        }
        private decimal PriceValueValue;

        public decimal PriceValue

        {

            get { return this.PriceValueValue; }

            set { SetProperty(ref PriceValueValue, value); }

        }
        private string CurrencyCodeValue;

        public string CurrencyCode

        {

            get { return this.CurrencyCodeValue; }

            set { SetProperty(ref CurrencyCodeValue, value); }

        }
        private string PricingPointValue;

        public string PricingPoint

        {

            get { return this.PricingPointValue; }

            set { SetProperty(ref PricingPointValue, value); }

        }
        private string UnitValue;

        public string Unit

        {

            get { return this.UnitValue; }

            set { SetProperty(ref UnitValue, value); }

        }

    }
}
