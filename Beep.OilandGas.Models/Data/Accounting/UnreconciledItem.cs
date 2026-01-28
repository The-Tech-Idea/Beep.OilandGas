using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public class UnreconciledItem : ModelEntityBase
    {
        private string AccountIdValue = string.Empty;

        public string AccountId

        {

            get { return this.AccountIdValue; }

            set { SetProperty(ref AccountIdValue, value); }

        }
        private decimal ExpectedBalanceValue;

        public decimal ExpectedBalance

        {

            get { return this.ExpectedBalanceValue; }

            set { SetProperty(ref ExpectedBalanceValue, value); }

        }
        private decimal ActualBalanceValue;

        public decimal ActualBalance

        {

            get { return this.ActualBalanceValue; }

            set { SetProperty(ref ActualBalanceValue, value); }

        }
        private decimal VarianceValue;

        public decimal Variance

        {

            get { return this.VarianceValue; }

            set { SetProperty(ref VarianceValue, value); }

        }
    }
}
