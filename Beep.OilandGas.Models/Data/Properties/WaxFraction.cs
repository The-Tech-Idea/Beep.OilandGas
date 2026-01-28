using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class WaxFraction : ModelEntityBase
    {
        private string FractionNameValue = string.Empty;

        public string FractionName

        {

            get { return this.FractionNameValue; }

            set { SetProperty(ref FractionNameValue, value); }

        }
        private decimal CarbonNumberValue;

        public decimal CarbonNumber

        {

            get { return this.CarbonNumberValue; }

            set { SetProperty(ref CarbonNumberValue, value); }

        }
        private decimal WeightFractionValue;

        public decimal WeightFraction

        {

            get { return this.WeightFractionValue; }

            set { SetProperty(ref WeightFractionValue, value); }

        }
        private decimal MeltingPointValue;

        public decimal MeltingPoint

        {

            get { return this.MeltingPointValue; }

            set { SetProperty(ref MeltingPointValue, value); }

        }
    }
}
