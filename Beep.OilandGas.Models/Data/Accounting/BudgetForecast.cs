using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class BudgetForecast : ModelEntityBase
    {
        private System.String BudgetNameValue = string.Empty;
        public System.String BudgetName
        {
            get { return this.BudgetNameValue; }
            set { SetProperty(ref BudgetNameValue, value); }
        }

        private System.DateTime AsOfDateValue;
        public System.DateTime AsOfDate
        {
            get { return this.AsOfDateValue; }
            set { SetProperty(ref AsOfDateValue, value); }
        }

        private System.DateTime ForecastDateValue;
        public System.DateTime ForecastDate
        {
            get { return this.ForecastDateValue; }
            set { SetProperty(ref ForecastDateValue, value); }
        }

        private System.DateTime YearEndValue;
        public System.DateTime YearEnd
        {
            get { return this.YearEndValue; }
            set { SetProperty(ref YearEndValue, value); }
        }

        private System.Int32 DaysRemainingValue;
        public System.Int32 DaysRemaining
        {
            get { return this.DaysRemainingValue; }
            set { SetProperty(ref DaysRemainingValue, value); }
        }

        private System.String ForecastMethodValue = string.Empty;
        public System.String ForecastMethod
        {
            get { return this.ForecastMethodValue; }
            set { SetProperty(ref ForecastMethodValue, value); }
        }

        private List<ForecastItem> ItemsValue = new List<ForecastItem>();
        public List<ForecastItem> Items
        {
            get { return this.ItemsValue; }
            set { SetProperty(ref ItemsValue, value); }
        }

        private System.Decimal TotalBudgetValue;
        public System.Decimal TotalBudget
        {
            get { return this.TotalBudgetValue; }
            set { SetProperty(ref TotalBudgetValue, value); }
        }

        private System.Decimal TotalYTDActualValue;
        public System.Decimal TotalYTDActual
        {
            get { return this.TotalYTDActualValue; }
            set { SetProperty(ref TotalYTDActualValue, value); }
        }

        private System.Decimal TotalForecastedYearEndValue;
        public System.Decimal TotalForecastedYearEnd
        {
            get { return this.TotalForecastedYearEndValue; }
            set { SetProperty(ref TotalForecastedYearEndValue, value); }
        }

        private System.Decimal TotalForecastedVarianceValue;
        public System.Decimal TotalForecastedVariance
        {
            get { return this.TotalForecastedVarianceValue; }
            set { SetProperty(ref TotalForecastedVarianceValue, value); }
        }

        private System.Int32 ExceedingBudgetCountValue;
        public System.Int32 ExceedingBudgetCount
        {
            get { return this.ExceedingBudgetCountValue; }
            set { SetProperty(ref ExceedingBudgetCountValue, value); }
        }
    }
}
