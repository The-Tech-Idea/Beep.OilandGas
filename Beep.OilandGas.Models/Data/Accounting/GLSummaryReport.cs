using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class GLSummaryReport : ModelEntityBase
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

        private System.DateTime AsOfDateValue;
        /// <summary>
        /// Gets or sets as of date.
        /// </summary>
        public System.DateTime AsOfDate
        {
            get { return this.AsOfDateValue; }
            set { SetProperty(ref AsOfDateValue, value); }
        }

        private List<GLSummaryLine> AccountsValue = new List<GLSummaryLine>();
        /// <summary>
        /// Gets or sets accounts.
        /// </summary>
        public List<GLSummaryLine> Accounts
        {
            get { return this.AccountsValue; }
            set { SetProperty(ref AccountsValue, value); }
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

        private System.Boolean IsBalancedValue;
        /// <summary>
        /// Gets or sets balanced flag.
        /// </summary>
        public System.Boolean IsBalanced
        {
            get { return this.IsBalancedValue; }
            set { SetProperty(ref IsBalancedValue, value); }
        }
    }
}
