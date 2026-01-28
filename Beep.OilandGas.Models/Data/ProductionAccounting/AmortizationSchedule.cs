using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class AmortizationSchedule : ModelEntityBase
    {
        private string? PropertyIdValue;

        public string? PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private string? CostCenterIdValue;

        public string? CostCenterId

        {

            get { return this.CostCenterIdValue; }

            set { SetProperty(ref CostCenterIdValue, value); }

        }
        private DateTime StartDateValue;

        public DateTime StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private string PeriodTypeValue;

        public string PeriodType

        {

            get { return this.PeriodTypeValue; }

            set { SetProperty(ref PeriodTypeValue, value); }

        }
        private decimal BeginningNetCapitalizedCostsValue;

        public decimal BeginningNetCapitalizedCosts

        {

            get { return this.BeginningNetCapitalizedCostsValue; }

            set { SetProperty(ref BeginningNetCapitalizedCostsValue, value); }

        }
        private decimal TotalReservesBOEValue;

        public decimal TotalReservesBOE

        {

            get { return this.TotalReservesBOEValue; }

            set { SetProperty(ref TotalReservesBOEValue, value); }

        }
        private List<AmortizationSchedulePeriod> PeriodsValue = new List<AmortizationSchedulePeriod>();

        public List<AmortizationSchedulePeriod> Periods

        {

            get { return this.PeriodsValue; }

            set { SetProperty(ref PeriodsValue, value); }

        }
        private decimal TotalProjectedAmortizationValue;

        public decimal TotalProjectedAmortization

        {

            get { return this.TotalProjectedAmortizationValue; }

            set { SetProperty(ref TotalProjectedAmortizationValue, value); }

        }
        private decimal EndingNetCapitalizedCostsValue;

        public decimal EndingNetCapitalizedCosts

        {

            get { return this.EndingNetCapitalizedCostsValue; }

            set { SetProperty(ref EndingNetCapitalizedCostsValue, value); }

        }
    }
}
