using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public class UpdateGLAccountRequest : ModelEntityBase
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
        private string AccountTypeValue;

        public string AccountType

        {

            get { return this.AccountTypeValue; }

            set { SetProperty(ref AccountTypeValue, value); }

        }
        private string ParentAccountIdValue;

        public string ParentAccountId

        {

            get { return this.ParentAccountIdValue; }

            set { SetProperty(ref ParentAccountIdValue, value); }

        }
        private string NormalBalanceValue;

        public string NormalBalance

        {

            get { return this.NormalBalanceValue; }

            set { SetProperty(ref NormalBalanceValue, value); }

        }
        private decimal? OpeningBalanceValue;

        public decimal? OpeningBalance

        {

            get { return this.OpeningBalanceValue; }

            set { SetProperty(ref OpeningBalanceValue, value); }

        }
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }
}
