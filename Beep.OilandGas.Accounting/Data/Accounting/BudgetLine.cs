using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class BudgetLine : ModelEntityBase
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
    }
}
