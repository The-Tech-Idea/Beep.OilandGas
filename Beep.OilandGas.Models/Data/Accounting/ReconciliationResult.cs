using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public class ReconciliationResult : ModelEntityBase
    {
        private bool ReconciliationBalanceValue;

        public bool ReconciliationBalance

        {

            get { return this.ReconciliationBalanceValue; }

            set { SetProperty(ref ReconciliationBalanceValue, value); }

        }
        private decimal VarianceValue;

        public decimal Variance

        {

            get { return this.VarianceValue; }

            set { SetProperty(ref VarianceValue, value); }

        }
        private List<UnreconciledItem> UnreconciledItemsValue = new();

        public List<UnreconciledItem> UnreconciledItems

        {

            get { return this.UnreconciledItemsValue; }

            set { SetProperty(ref UnreconciledItemsValue, value); }

        }
    }
}
