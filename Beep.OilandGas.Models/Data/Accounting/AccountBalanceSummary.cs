using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public class AccountBalanceSummary : ModelEntityBase
    {
        private string GlAccountIdValue;

        public string GlAccountId

        {

            get { return this.GlAccountIdValue; }

            set { SetProperty(ref GlAccountIdValue, value); }

        }
        private string AccountNumberValue;

        public string AccountNumber

        {

            get { return this.AccountNumberValue; }

            set { SetProperty(ref AccountNumberValue, value); }

        }
        private string AccountNameValue;

        public string AccountName

        {

            get { return this.AccountNameValue; }

            set { SetProperty(ref AccountNameValue, value); }

        }
        private decimal? OpeningBalanceValue;

        public decimal? OpeningBalance

        {

            get { return this.OpeningBalanceValue; }

            set { SetProperty(ref OpeningBalanceValue, value); }

        }
        private decimal? DebitTotalValue;

        public decimal? DebitTotal

        {

            get { return this.DebitTotalValue; }

            set { SetProperty(ref DebitTotalValue, value); }

        }
        private decimal? CreditTotalValue;

        public decimal? CreditTotal

        {

            get { return this.CreditTotalValue; }

            set { SetProperty(ref CreditTotalValue, value); }

        }
        private decimal? CurrentBalanceValue;

        public decimal? CurrentBalance

        {

            get { return this.CurrentBalanceValue; }

            set { SetProperty(ref CurrentBalanceValue, value); }

        }
        private decimal? DifferenceValue;

        public decimal? Difference

        {

            get { return this.DifferenceValue; }

            set { SetProperty(ref DifferenceValue, value); }

        }
        private DateTime? AsOfDateValue;

        public DateTime? AsOfDate

        {

            get { return this.AsOfDateValue; }

            set { SetProperty(ref AsOfDateValue, value); }

        }
    }
}
