using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class BudgetVarianceReport : ModelEntityBase
    {
        private System.String ReportNameValue = string.Empty;
        public System.String ReportName
        {
            get { return this.ReportNameValue; }
            set { SetProperty(ref ReportNameValue, value); }
        }

        private System.DateTime GeneratedDateValue;
        public System.DateTime GeneratedDate
        {
            get { return this.GeneratedDateValue; }
            set { SetProperty(ref GeneratedDateValue, value); }
        }

        private System.DateTime AsOfDateValue;
        public System.DateTime AsOfDate
        {
            get { return this.AsOfDateValue; }
            set { SetProperty(ref AsOfDateValue, value); }
        }

        private System.String BudgetNameValue = string.Empty;
        public System.String BudgetName
        {
            get { return this.BudgetNameValue; }
            set { SetProperty(ref BudgetNameValue, value); }
        }

        private System.String BudgetPeriodValue = string.Empty;
        public System.String BudgetPeriod
        {
            get { return this.BudgetPeriodValue; }
            set { SetProperty(ref BudgetPeriodValue, value); }
        }

        private List<BudgetVarianceLine> VarianceLinesValue = new List<BudgetVarianceLine>();
        public List<BudgetVarianceLine> VarianceLines
        {
            get { return this.VarianceLinesValue; }
            set { SetProperty(ref VarianceLinesValue, value); }
        }

        private System.Decimal TotalBudgetValue;
        public System.Decimal TotalBudget
        {
            get { return this.TotalBudgetValue; }
            set { SetProperty(ref TotalBudgetValue, value); }
        }

        private System.Decimal TotalActualValue;
        public System.Decimal TotalActual
        {
            get { return this.TotalActualValue; }
            set { SetProperty(ref TotalActualValue, value); }
        }

        private System.Decimal TotalVarianceValue;
        public System.Decimal TotalVariance
        {
            get { return this.TotalVarianceValue; }
            set { SetProperty(ref TotalVarianceValue, value); }
        }

        private System.Decimal VariancePercentValue;
        public System.Decimal VariancePercent
        {
            get { return this.VariancePercentValue; }
            set { SetProperty(ref VariancePercentValue, value); }
        }

        private System.String VarianceStatusValue = string.Empty;
        public System.String VarianceStatus
        {
            get { return this.VarianceStatusValue; }
            set { SetProperty(ref VarianceStatusValue, value); }
        }
    }
}
