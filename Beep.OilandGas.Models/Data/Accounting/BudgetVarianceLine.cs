using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class BudgetVarianceLine : ModelEntityBase
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

        private System.Decimal BudgetAmountValue;
        public System.Decimal BudgetAmount
        {
            get { return this.BudgetAmountValue; }
            set { SetProperty(ref BudgetAmountValue, value); }
        }

        private System.Decimal ActualAmountValue;
        public System.Decimal ActualAmount
        {
            get { return this.ActualAmountValue; }
            set { SetProperty(ref ActualAmountValue, value); }
        }

        private System.Decimal VarianceValue;
        public System.Decimal Variance
        {
            get { return this.VarianceValue; }
            set { SetProperty(ref VarianceValue, value); }
        }

        private System.Decimal VariancePercentValue;
        public System.Decimal VariancePercent
        {
            get { return this.VariancePercentValue; }
            set { SetProperty(ref VariancePercentValue, value); }
        }

        private System.String StatusValue = string.Empty;
        public System.String Status
        {
            get { return this.StatusValue; }
            set { SetProperty(ref StatusValue, value); }
        }
    }
}
