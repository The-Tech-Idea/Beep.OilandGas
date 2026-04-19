using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class CalculateAmortizationRequest : ModelEntityBase
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
        private decimal NetCapitalizedCostsValue;

        public decimal NetCapitalizedCosts

        {

            get { return this.NetCapitalizedCostsValue; }

            set { SetProperty(ref NetCapitalizedCostsValue, value); }

        }
        private decimal TotalProvedReservesBOEValue;

        public decimal TotalProvedReservesBOE

        {

            get { return this.TotalProvedReservesBOEValue; }

            set { SetProperty(ref TotalProvedReservesBOEValue, value); }

        }
        private decimal ProductionBOEValue;

        public decimal ProductionBOE

        {

            get { return this.ProductionBOEValue; }

            set { SetProperty(ref ProductionBOEValue, value); }

        }
        private string AccountingMethodValue = "Successful Efforts";

        public string AccountingMethod

        {

            get { return this.AccountingMethodValue; }

            set { SetProperty(ref AccountingMethodValue, value); }

        } // or "Full Cost"
    }
}
