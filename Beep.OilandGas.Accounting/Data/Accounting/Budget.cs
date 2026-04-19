using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class Budget : ModelEntityBase
    {
        private System.String BudgetNameValue = string.Empty;
        public System.String BudgetName
        {
            get { return this.BudgetNameValue; }
            set { SetProperty(ref BudgetNameValue, value); }
        }

        private System.DateTime BudgetStartValue;
        public System.DateTime BudgetStart
        {
            get { return this.BudgetStartValue; }
            set { SetProperty(ref BudgetStartValue, value); }
        }

        private System.DateTime BudgetEndValue;
        public System.DateTime BudgetEnd
        {
            get { return this.BudgetEndValue; }
            set { SetProperty(ref BudgetEndValue, value); }
        }

        private System.DateTime CreatedDateValue;
        public System.DateTime CreatedDate
        {
            get { return this.CreatedDateValue; }
            set { SetProperty(ref CreatedDateValue, value); }
        }

        private System.String CreatedByValue = string.Empty;
        public System.String CreatedBy
        {
            get { return this.CreatedByValue; }
            set { SetProperty(ref CreatedByValue, value); }
        }

        private List<BudgetLine> BudgetLinesValue = new List<BudgetLine>();
        public List<BudgetLine> BudgetLines
        {
            get { return this.BudgetLinesValue; }
            set { SetProperty(ref BudgetLinesValue, value); }
        }

        private System.Decimal TotalBudgetedValue;
        public System.Decimal TotalBudgeted
        {
            get { return this.TotalBudgetedValue; }
            set { SetProperty(ref TotalBudgetedValue, value); }
        }

        private System.Int32 BudgetMonthsValue;
        public System.Int32 BudgetMonths
        {
            get { return this.BudgetMonthsValue; }
            set { SetProperty(ref BudgetMonthsValue, value); }
        }

        private System.String StatusValue = string.Empty;
        public System.String Status
        {
            get { return this.StatusValue; }
            set { SetProperty(ref StatusValue, value); }
        }
    }
}
