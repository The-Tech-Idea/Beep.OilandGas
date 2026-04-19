using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class ForecastItem : ModelEntityBase
    {
        private System.String AccountNumberValue = string.Empty;
        public System.String AccountNumber
        {
            get { return this.AccountNumberValue; }
            set { SetProperty(ref AccountNumberValue, value); }
        }

        private System.String AccountNameValue = string.Empty;
        public System.String AccountName
        {
            get { return this.AccountNameValue; }
            set { SetProperty(ref AccountNameValue, value); }
        }

        private System.Decimal FullYearBudgetValue;
        public System.Decimal FullYearBudget
        {
            get { return this.FullYearBudgetValue; }
            set { SetProperty(ref FullYearBudgetValue, value); }
        }

        private System.Decimal YTDActualValue;
        public System.Decimal YTDActual
        {
            get { return this.YTDActualValue; }
            set { SetProperty(ref YTDActualValue, value); }
        }

        private System.Decimal ForecastedYearEndValue;
        public System.Decimal ForecastedYearEnd
        {
            get { return this.ForecastedYearEndValue; }
            set { SetProperty(ref ForecastedYearEndValue, value); }
        }

        private System.Decimal ForecastedVarianceValue;
        public System.Decimal ForecastedVariance
        {
            get { return this.ForecastedVarianceValue; }
            set { SetProperty(ref ForecastedVarianceValue, value); }
        }

        private System.Decimal ForecastedVariancePercentValue;
        public System.Decimal ForecastedVariancePercent
        {
            get { return this.ForecastedVariancePercentValue; }
            set { SetProperty(ref ForecastedVariancePercentValue, value); }
        }

        private System.Boolean WillExceedBudgetValue;
        public System.Boolean WillExceedBudget
        {
            get { return this.WillExceedBudgetValue; }
            set { SetProperty(ref WillExceedBudgetValue, value); }
        }
    }
}
