using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class GenerateScheduleRequest : ModelEntityBase
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
        private int NumberOfPeriodsValue;

        public int NumberOfPeriods

        {

            get { return this.NumberOfPeriodsValue; }

            set { SetProperty(ref NumberOfPeriodsValue, value); }

        }
        private string PeriodTypeValue = "Monthly";

        public string PeriodType

        {

            get { return this.PeriodTypeValue; }

            set { SetProperty(ref PeriodTypeValue, value); }

        } // Monthly, Quarterly, Annual
        private decimal EstimatedProductionPerPeriodValue;

        public decimal EstimatedProductionPerPeriod

        {

            get { return this.EstimatedProductionPerPeriodValue; }

            set { SetProperty(ref EstimatedProductionPerPeriodValue, value); }

        }
        private string AccountingMethodValue = "Successful Efforts";

        public string AccountingMethod

        {

            get { return this.AccountingMethodValue; }

            set { SetProperty(ref AccountingMethodValue, value); }

        }
    }
}
