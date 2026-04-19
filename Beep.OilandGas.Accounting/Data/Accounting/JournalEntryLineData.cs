using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public class JournalEntryLineData : ModelEntityBase
    {
        private string GlAccountIdValue;

        public string GlAccountId

        {

            get { return this.GlAccountIdValue; }

            set { SetProperty(ref GlAccountIdValue, value); }

        }
        private decimal? DebitAmountValue;

        public decimal? DebitAmount

        {

            get { return this.DebitAmountValue; }

            set { SetProperty(ref DebitAmountValue, value); }

        }
        private decimal? CreditAmountValue;

        public decimal? CreditAmount

        {

            get { return this.CreditAmountValue; }

            set { SetProperty(ref CreditAmountValue, value); }

        }
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }
}
