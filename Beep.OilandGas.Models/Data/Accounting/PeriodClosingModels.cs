using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Accounting
{
    /// <summary>
    /// Period close readiness check result
    /// </summary>
    public partial class PeriodCloseReadiness : AccountingEntityBase
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

    /// <summary>
    /// Period close result
    /// </summary>
    public partial class PeriodCloseResult : AccountingEntityBase
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

    /// <summary>
    /// Individual closing entry
    /// </summary>
    public partial class PeriodClosingEntry : AccountingEntityBase
    {
        private System.String SourceAccountValue = string.Empty;
        /// <summary>
        /// Gets or sets source account.
        /// </summary>
        public System.String SourceAccount
        {
            get { return this.SourceAccountValue; }
            set { SetProperty(ref SourceAccountValue, value); }
        }

        private System.String TargetAccountValue = string.Empty;
        /// <summary>
        /// Gets or sets target account.
        /// </summary>
        public System.String TargetAccount
        {
            get { return this.TargetAccountValue; }
            set { SetProperty(ref TargetAccountValue, value); }
        }

        private System.Decimal AmountValue;
        /// <summary>
        /// Gets or sets amount.
        /// </summary>
        public System.Decimal Amount
        {
            get { return this.AmountValue; }
            set { SetProperty(ref AmountValue, value); }
        }

        private System.String EntryTypeValue = string.Empty;
        /// <summary>
        /// Gets or sets entry type.
        /// </summary>
        public System.String EntryType
        {
            get { return this.EntryTypeValue; }
            set { SetProperty(ref EntryTypeValue, value); }
        }

        private System.String DescriptionValue = string.Empty;
        /// <summary>
        /// Gets or sets description.
        /// </summary>
        public System.String Description
        {
            get { return this.DescriptionValue; }
            set { SetProperty(ref DescriptionValue, value); }
        }
    }

    /// <summary>
    /// Period closing checklist
    /// </summary>
    public partial class ClosingChecklist : AccountingEntityBase
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

        private List<ChecklistItem> ItemsValue = new List<ChecklistItem>();
        /// <summary>
        /// Gets or sets items.
        /// </summary>
        public List<ChecklistItem> Items
        {
            get { return this.ItemsValue; }
            set { SetProperty(ref ItemsValue, value); }
        }

        private System.Boolean IsReadyToCloseValue;
        /// <summary>
        /// Gets or sets readiness.
        /// </summary>
        public System.Boolean IsReadyToClose
        {
            get { return this.IsReadyToCloseValue; }
            set { SetProperty(ref IsReadyToCloseValue, value); }
        }

        private System.Decimal CompletionPercentageValue;
        /// <summary>
        /// Gets or sets completion percentage.
        /// </summary>
        public System.Decimal CompletionPercentage
        {
            get { return this.CompletionPercentageValue; }
            set { SetProperty(ref CompletionPercentageValue, value); }
        }
    }

    /// <summary>
    /// Individual checklist item
    /// </summary>
    public partial class ChecklistItem : AccountingEntityBase
    {
        private System.String TaskValue = string.Empty;
        /// <summary>
        /// Gets or sets task.
        /// </summary>
        public System.String Task
        {
            get { return this.TaskValue; }
            set { SetProperty(ref TaskValue, value); }
        }

        private System.Boolean IsCompleteValue;
        /// <summary>
        /// Gets or sets completion flag.
        /// </summary>
        public System.Boolean IsComplete
        {
            get { return this.IsCompleteValue; }
            set { SetProperty(ref IsCompleteValue, value); }
        }

        private System.String DetailsValue = string.Empty;
        /// <summary>
        /// Gets or sets details.
        /// </summary>
        public System.String Details
        {
            get { return this.DetailsValue; }
            set { SetProperty(ref DetailsValue, value); }
        }
    }
}

