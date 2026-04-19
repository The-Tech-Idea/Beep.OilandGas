using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class GLSummaryLine : ModelEntityBase
    {
        private System.String AccountNumberValue = string.Empty;
        /// <summary>
        /// Gets or sets account number.
        /// </summary>
        public System.String AccountNumber
        {
            get { return this.AccountNumberValue; }
            set { SetProperty(ref AccountNumberValue, value); }
        }

        private System.String AccountNameValue = string.Empty;
        /// <summary>
        /// Gets or sets account name.
        /// </summary>
        public System.String AccountName
        {
            get { return this.AccountNameValue; }
            set { SetProperty(ref AccountNameValue, value); }
        }

        private System.String AccountTypeValue = string.Empty;
        /// <summary>
        /// Gets or sets account type.
        /// </summary>
        public System.String AccountType
        {
            get { return this.AccountTypeValue; }
            set { SetProperty(ref AccountTypeValue, value); }
        }

        private System.String NormalBalanceValue = string.Empty;
        /// <summary>
        /// Gets or sets normal balance.
        /// </summary>
        public System.String NormalBalance
        {
            get { return this.NormalBalanceValue; }
            set { SetProperty(ref NormalBalanceValue, value); }
        }

        private System.Decimal BalanceValue;
        /// <summary>
        /// Gets or sets balance.
        /// </summary>
        public System.Decimal Balance
        {
            get { return this.BalanceValue; }
            set { SetProperty(ref BalanceValue, value); }
        }
    }
}
