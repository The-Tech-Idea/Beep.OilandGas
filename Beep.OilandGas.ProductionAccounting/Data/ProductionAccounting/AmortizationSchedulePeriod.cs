using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class AmortizationSchedulePeriod : ModelEntityBase
    {
        private int PeriodNumberValue;

        public int PeriodNumber

        {

            get { return this.PeriodNumberValue; }

            set { SetProperty(ref PeriodNumberValue, value); }

        }
        private DateTime PeriodStartDateValue;

        public DateTime PeriodStartDate

        {

            get { return this.PeriodStartDateValue; }

            set { SetProperty(ref PeriodStartDateValue, value); }

        }
        private DateTime PeriodEndDateValue;

        public DateTime PeriodEndDate

        {

            get { return this.PeriodEndDateValue; }

            set { SetProperty(ref PeriodEndDateValue, value); }

        }
        private decimal BeginningNetCapitalizedCostsValue;

        public decimal BeginningNetCapitalizedCosts

        {

            get { return this.BeginningNetCapitalizedCostsValue; }

            set { SetProperty(ref BeginningNetCapitalizedCostsValue, value); }

        }
        private decimal EstimatedProductionBOEValue;

        public decimal EstimatedProductionBOE

        {

            get { return this.EstimatedProductionBOEValue; }

            set { SetProperty(ref EstimatedProductionBOEValue, value); }

        }
        private decimal AmortizationRateValue;

        public decimal AmortizationRate

        {

            get { return this.AmortizationRateValue; }

            set { SetProperty(ref AmortizationRateValue, value); }

        }
        private decimal ProjectedAmortizationValue;

        public decimal ProjectedAmortization

        {

            get { return this.ProjectedAmortizationValue; }

            set { SetProperty(ref ProjectedAmortizationValue, value); }

        }
        private decimal EndingNetCapitalizedCostsValue;

        public decimal EndingNetCapitalizedCosts

        {

            get { return this.EndingNetCapitalizedCostsValue; }

            set { SetProperty(ref EndingNetCapitalizedCostsValue, value); }

        }
    }
}
