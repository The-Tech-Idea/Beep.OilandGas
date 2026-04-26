using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class EconomicEvaluationRequest : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private decimal OilPriceValue;

        public decimal OilPrice

        {

            get { return this.OilPriceValue; }

            set { SetProperty(ref OilPriceValue, value); }

        }
        private decimal GasPriceValue;

        public decimal GasPrice

        {

            get { return this.GasPriceValue; }

            set { SetProperty(ref GasPriceValue, value); }

        }
        private decimal DiscountRateValue;

        public decimal DiscountRate

        {

            get { return this.DiscountRateValue; }

            set { SetProperty(ref DiscountRateValue, value); }

        }
        private decimal OperatingCostValue;

        public decimal OperatingCost

        {

            get { return this.OperatingCostValue; }

            set { SetProperty(ref OperatingCostValue, value); }

        }
        private decimal CapitalCostValue;

        public decimal CapitalCost

        {

            get { return this.CapitalCostValue; }

            set { SetProperty(ref CapitalCostValue, value); }

        }
        private string EconomicModelValue = "Standard";

        public string EconomicModel

        {

            get { return this.EconomicModelValue; }

            set { SetProperty(ref EconomicModelValue, value); }

        }
    }
}
