using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class RoyaltyDeductions : ModelEntityBase
    {
        private decimal ProductionTaxesValue;

        public decimal ProductionTaxes

        {

            get { return this.ProductionTaxesValue; }

            set { SetProperty(ref ProductionTaxesValue, value); }

        }
        private decimal TransportationCostsValue;

        public decimal TransportationCosts

        {

            get { return this.TransportationCostsValue; }

            set { SetProperty(ref TransportationCostsValue, value); }

        }
        private decimal ProcessingCostsValue;

        public decimal ProcessingCosts

        {

            get { return this.ProcessingCostsValue; }

            set { SetProperty(ref ProcessingCostsValue, value); }

        }
        private decimal MarketingCostsValue;

        public decimal MarketingCosts

        {

            get { return this.MarketingCostsValue; }

            set { SetProperty(ref MarketingCostsValue, value); }

        }
        private decimal OtherDeductionsValue;

        public decimal OtherDeductions

        {

            get { return this.OtherDeductionsValue; }

            set { SetProperty(ref OtherDeductionsValue, value); }

        }
        private decimal TotalDeductionsValue;

        public decimal TotalDeductions

        {

            get { return this.TotalDeductionsValue; }

            set { SetProperty(ref TotalDeductionsValue, value); }

        }
    }
}
