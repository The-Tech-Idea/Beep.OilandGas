using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class ClosingChecklist : ModelEntityBase
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
}
