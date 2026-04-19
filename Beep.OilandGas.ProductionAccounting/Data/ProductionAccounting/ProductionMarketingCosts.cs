using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class ProductionMarketingCosts : ModelEntityBase
    {
        private decimal LiftingCostsValue;

        public decimal LiftingCosts

        {

            get { return this.LiftingCostsValue; }

            set { SetProperty(ref LiftingCostsValue, value); }

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
        private decimal OtherCostsValue;

        public decimal OtherCosts

        {

            get { return this.OtherCostsValue; }

            set { SetProperty(ref OtherCostsValue, value); }

        }
        private decimal TotalCostsValue;

        public decimal TotalCosts

        {

            get { return this.TotalCostsValue; }

            set { SetProperty(ref TotalCostsValue, value); }

        }
    }
}
