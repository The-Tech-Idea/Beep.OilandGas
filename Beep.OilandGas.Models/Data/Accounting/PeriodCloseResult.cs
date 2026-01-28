using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class PeriodCloseResult : ModelEntityBase
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

        private System.String PeriodNameValue = string.Empty;
        /// <summary>
        /// Gets or sets period name.
        /// </summary>
        public System.String PeriodName
        {
            get { return this.PeriodNameValue; }
            set { SetProperty(ref PeriodNameValue, value); }
        }

        private System.DateTime CloseDateValue;
        /// <summary>
        /// Gets or sets close date.
        /// </summary>
        public System.DateTime CloseDate
        {
            get { return this.CloseDateValue; }
            set { SetProperty(ref CloseDateValue, value); }
        }

        private System.Boolean SuccessValue;
        /// <summary>
        /// Gets or sets success.
        /// </summary>
        public System.Boolean Success
        {
            get { return this.SuccessValue; }
            set { SetProperty(ref SuccessValue, value); }
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

        private List<System.String> StepsCompletedValue = new List<System.String>();
        /// <summary>
        /// Gets or sets steps completed.
        /// </summary>
        public List<System.String> StepsCompleted
        {
            get { return this.StepsCompletedValue; }
            set { SetProperty(ref StepsCompletedValue, value); }
        }

        private List<System.String> ErrorsValue = new List<System.String>();
        /// <summary>
        /// Gets or sets errors.
        /// </summary>
        public List<System.String> Errors
        {
            get { return this.ErrorsValue; }
            set { SetProperty(ref ErrorsValue, value); }
        }

        private System.Int32 ClosingEntriesCountValue;
        /// <summary>
        /// Gets or sets closing entries count.
        /// </summary>
        public System.Int32 ClosingEntriesCount
        {
            get { return this.ClosingEntriesCountValue; }
            set { SetProperty(ref ClosingEntriesCountValue, value); }
        }

        private System.Decimal FinalBalanceValue;
        /// <summary>
        /// Gets or sets final balance.
        /// </summary>
        public System.Decimal FinalBalance
        {
            get { return this.FinalBalanceValue; }
            set { SetProperty(ref FinalBalanceValue, value); }
        }
    }
}
