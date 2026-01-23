namespace Beep.OilandGas.Models.Data.Accounting
{
    public class AmortizationRecord : ModelEntityBase
    {
        private string AmortizationRecordIdValue;

        public string AmortizationRecordId

        {

            get { return this.AmortizationRecordIdValue; }

            set { SetProperty(ref AmortizationRecordIdValue, value); }

        }
        private string PropertyIdValue;

        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private string CostCenterIdValue;

        public string CostCenterId

        {

            get { return this.CostCenterIdValue; }

            set { SetProperty(ref CostCenterIdValue, value); }

        }
        private DateTime? PeriodStartDateValue;

        public DateTime? PeriodStartDate

        {

            get { return this.PeriodStartDateValue; }

            set { SetProperty(ref PeriodStartDateValue, value); }

        }
        private DateTime? PeriodEndDateValue;

        public DateTime? PeriodEndDate

        {

            get { return this.PeriodEndDateValue; }

            set { SetProperty(ref PeriodEndDateValue, value); }

        }
        private decimal? NetCapitalizedCostsValue;

        public decimal? NetCapitalizedCosts

        {

            get { return this.NetCapitalizedCostsValue; }

            set { SetProperty(ref NetCapitalizedCostsValue, value); }

        }
        private decimal? TotalReservesBOEValue;

        public decimal? TotalReservesBOE

        {

            get { return this.TotalReservesBOEValue; }

            set { SetProperty(ref TotalReservesBOEValue, value); }

        }
        private decimal? ProductionBOEValue;

        public decimal? ProductionBOE

        {

            get { return this.ProductionBOEValue; }

            set { SetProperty(ref ProductionBOEValue, value); }

        }
        private decimal? AmortizationAmountValue;

        public decimal? AmortizationAmount

        {

            get { return this.AmortizationAmountValue; }

            set { SetProperty(ref AmortizationAmountValue, value); }

        }
        private string AccountingMethodValue;

        public string AccountingMethod

        {

            get { return this.AccountingMethodValue; }

            set { SetProperty(ref AccountingMethodValue, value); }

        }
    }

    public class CreateAmortizationRecordRequest : ModelEntityBase
    {
        private string PropertyIdValue;

        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private string CostCenterIdValue;

        public string CostCenterId

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
        private decimal TotalReservesBOEValue;

        public decimal TotalReservesBOE

        {

            get { return this.TotalReservesBOEValue; }

            set { SetProperty(ref TotalReservesBOEValue, value); }

        }
        private decimal ProductionBOEValue;

        public decimal ProductionBOE

        {

            get { return this.ProductionBOEValue; }

            set { SetProperty(ref ProductionBOEValue, value); }

        }
        private string AccountingMethodValue;

        public string AccountingMethod

        {

            get { return this.AccountingMethodValue; }

            set { SetProperty(ref AccountingMethodValue, value); }

        }
    }

    public class AmortizationRecordResponse : AmortizationRecord
    {
    }
}






