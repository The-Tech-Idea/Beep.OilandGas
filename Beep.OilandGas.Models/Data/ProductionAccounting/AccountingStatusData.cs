using System;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// Accounting status data for reporting.
    /// </summary>
    public class AccountingStatusData : ModelEntityBase
    {
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private DateTime? AsOfDateValue;

        public DateTime? AsOfDate

        {

            get { return this.AsOfDateValue; }

            set { SetProperty(ref AsOfDateValue, value); }

        }
        private decimal TotalProductionValue;

        public decimal TotalProduction

        {

            get { return this.TotalProductionValue; }

            set { SetProperty(ref TotalProductionValue, value); }

        }
        private decimal TotalRevenueValue;

        public decimal TotalRevenue

        {

            get { return this.TotalRevenueValue; }

            set { SetProperty(ref TotalRevenueValue, value); }

        }
        private decimal TotalRoyaltyValue;

        public decimal TotalRoyalty

        {

            get { return this.TotalRoyaltyValue; }

            set { SetProperty(ref TotalRoyaltyValue, value); }

        }
        private decimal TotalCostsValue;

        public decimal TotalCosts

        {

            get { return this.TotalCostsValue; }

            set { SetProperty(ref TotalCostsValue, value); }

        }
        private decimal NetIncomeValue;

        public decimal NetIncome

        {

            get { return this.NetIncomeValue; }

            set { SetProperty(ref NetIncomeValue, value); }

        }
        private string AccountingMethodValue = string.Empty;

        public string AccountingMethod

        {

            get { return this.AccountingMethodValue; }

            set { SetProperty(ref AccountingMethodValue, value); }

        }
        private string PeriodStatusValue = string.Empty;

        public string PeriodStatus

        {

            get { return this.PeriodStatusValue; }

            set { SetProperty(ref PeriodStatusValue, value); }

        }
    }
}



