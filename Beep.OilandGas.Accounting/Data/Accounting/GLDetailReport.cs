using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class GLDetailReport : ModelEntityBase
    {
        private System.String ReportNameValue = string.Empty;
        /// <summary>
        /// Gets or sets report name.
        /// </summary>
        public System.String ReportName
        {
            get { return this.ReportNameValue; }
            set { SetProperty(ref ReportNameValue, value); }
        }

        private System.DateTime GeneratedDateValue;
        /// <summary>
        /// Gets or sets generated date.
        /// </summary>
        public System.DateTime GeneratedDate
        {
            get { return this.GeneratedDateValue; }
            set { SetProperty(ref GeneratedDateValue, value); }
        }

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

        private System.Nullable<System.DateTime> StartDateValue;
        /// <summary>
        /// Gets or sets start date.
        /// </summary>
        public System.Nullable<System.DateTime> StartDate
        {
            get { return this.StartDateValue; }
            set { SetProperty(ref StartDateValue, value); }
        }

        private System.Nullable<System.DateTime> EndDateValue;
        /// <summary>
        /// Gets or sets end date.
        /// </summary>
        public System.Nullable<System.DateTime> EndDate
        {
            get { return this.EndDateValue; }
            set { SetProperty(ref EndDateValue, value); }
        }

        private List<GLEntryLine> GLEntriesValue = new List<GLEntryLine>();
        /// <summary>
        /// Gets or sets GL entries.
        /// </summary>
        public List<GLEntryLine> GLEntries
        {
            get { return this.GLEntriesValue; }
            set { SetProperty(ref GLEntriesValue, value); }
        }

        private System.Decimal TotalDebitsValue;
        /// <summary>
        /// Gets or sets total debits.
        /// </summary>
        public System.Decimal TotalDebits
        {
            get { return this.TotalDebitsValue; }
            set { SetProperty(ref TotalDebitsValue, value); }
        }

        private System.Decimal TotalCreditsValue;
        /// <summary>
        /// Gets or sets total credits.
        /// </summary>
        public System.Decimal TotalCredits
        {
            get { return this.TotalCreditsValue; }
            set { SetProperty(ref TotalCreditsValue, value); }
        }

        private System.Decimal EndingBalanceValue;
        /// <summary>
        /// Gets or sets ending balance.
        /// </summary>
        public System.Decimal EndingBalance
        {
            get { return this.EndingBalanceValue; }
            set { SetProperty(ref EndingBalanceValue, value); }
        }
    }
}
