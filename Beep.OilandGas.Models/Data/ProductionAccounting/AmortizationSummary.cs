using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class AmortizationSummary : ModelEntityBase
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
        private DateTime? AsOfDateValue;

        public DateTime? AsOfDate

        {

            get { return this.AsOfDateValue; }

            set { SetProperty(ref AsOfDateValue, value); }

        }
        private decimal TotalCapitalizedCostsValue;

        public decimal TotalCapitalizedCosts

        {

            get { return this.TotalCapitalizedCostsValue; }

            set { SetProperty(ref TotalCapitalizedCostsValue, value); }

        }
        private decimal AccumulatedAmortizationValue;

        public decimal AccumulatedAmortization

        {

            get { return this.AccumulatedAmortizationValue; }

            set { SetProperty(ref AccumulatedAmortizationValue, value); }

        }
        private decimal NetCapitalizedCostsValue;

        public decimal NetCapitalizedCosts

        {

            get { return this.NetCapitalizedCostsValue; }

            set { SetProperty(ref NetCapitalizedCostsValue, value); }

        }
        private decimal TotalReservesBOEValue;

        public decimal TotalReservesBOE

        {

            get { return this.TotalReservesBOEValue; }

            set { SetProperty(ref TotalReservesBOEValue, value); }

        }
        private decimal RemainingReservesBOEValue;

        public decimal RemainingReservesBOE

        {

            get { return this.RemainingReservesBOEValue; }

            set { SetProperty(ref RemainingReservesBOEValue, value); }

        }
        private decimal AmortizationRateValue;

        public decimal AmortizationRate

        {

            get { return this.AmortizationRateValue; }

            set { SetProperty(ref AmortizationRateValue, value); }

        }
        private int NumberOfRecordsValue;

        public int NumberOfRecords

        {

            get { return this.NumberOfRecordsValue; }

            set { SetProperty(ref NumberOfRecordsValue, value); }

        }
        private DateTime? FirstAmortizationDateValue;

        public DateTime? FirstAmortizationDate

        {

            get { return this.FirstAmortizationDateValue; }

            set { SetProperty(ref FirstAmortizationDateValue, value); }

        }
        private DateTime? LastAmortizationDateValue;

        public DateTime? LastAmortizationDate

        {

            get { return this.LastAmortizationDateValue; }

            set { SetProperty(ref LastAmortizationDateValue, value); }

        }
    }
}
