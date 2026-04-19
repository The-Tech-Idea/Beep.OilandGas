using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class PeriodCloseReadiness : ModelEntityBase
    {
        private System.DateTime PeriodEndDateValue;
        /// <summary>
        /// Gets or sets period end date.
        /// </summary>
        public System.DateTime PeriodEndDate
        {
            get { return this.PeriodEndDateValue; }
            set { SetProperty(ref PeriodEndDateValue, value); }
        }

        private System.DateTime CheckDateValue;
        /// <summary>
        /// Gets or sets check date.
        /// </summary>
        public System.DateTime CheckDate
        {
            get { return this.CheckDateValue; }
            set { SetProperty(ref CheckDateValue, value); }
        }

        private System.Boolean IsReadyToCloseValue;
        /// <summary>
        /// Gets or sets readiness flag.
        /// </summary>
        public System.Boolean IsReadyToClose
        {
            get { return this.IsReadyToCloseValue; }
            set { SetProperty(ref IsReadyToCloseValue, value); }
        }

        private System.String MessageValue = string.Empty;
        /// <summary>
        /// Gets or sets message.
        /// </summary>
        public System.String Message
        {
            get { return this.MessageValue; }
            set { SetProperty(ref MessageValue, value); }
        }

        private List<System.String> IssuesValue = new List<System.String>();
        /// <summary>
        /// Gets or sets issues.
        /// </summary>
        public List<System.String> Issues
        {
            get { return this.IssuesValue; }
            set { SetProperty(ref IssuesValue, value); }
        }
    }
}
