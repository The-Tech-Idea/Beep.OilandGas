using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.General
{
    public class BankReconciliation : ModelEntityBase
    {
        private string CashAccountIdValue;

        public string CashAccountId

        {

            get { return this.CashAccountIdValue; }

            set { SetProperty(ref CashAccountIdValue, value); }

        }
        private DateTime PeriodEndValue;

        public DateTime PeriodEnd

        {

            get { return this.PeriodEndValue; }

            set { SetProperty(ref PeriodEndValue, value); }

        }
        private decimal GLBalanceValue;

        public decimal GLBalance

        {

            get { return this.GLBalanceValue; }

            set { SetProperty(ref GLBalanceValue, value); }

        }
        private decimal BankBalanceValue;

        public decimal BankBalance

        {

            get { return this.BankBalanceValue; }

            set { SetProperty(ref BankBalanceValue, value); }

        }
        private decimal OutstandingDepositsValue;

        public decimal OutstandingDeposits

        {

            get { return this.OutstandingDepositsValue; }

            set { SetProperty(ref OutstandingDepositsValue, value); }

        }
        private decimal OutstandingChecksValue;

        public decimal OutstandingChecks

        {

            get { return this.OutstandingChecksValue; }

            set { SetProperty(ref OutstandingChecksValue, value); }

        }
        private decimal ReconciledBalanceValue;

        public decimal ReconciledBalance

        {

            get { return this.ReconciledBalanceValue; }

            set { SetProperty(ref ReconciledBalanceValue, value); }

        }
        private bool IsReconciledValue;

        public bool IsReconciled

        {

            get { return this.IsReconciledValue; }

            set { SetProperty(ref IsReconciledValue, value); }

        }
        private List<OutstandingItem> OutstandingItemsValue = new();

        public List<OutstandingItem> OutstandingItems

        {

            get { return this.OutstandingItemsValue; }

            set { SetProperty(ref OutstandingItemsValue, value); }

        }
    }
}


