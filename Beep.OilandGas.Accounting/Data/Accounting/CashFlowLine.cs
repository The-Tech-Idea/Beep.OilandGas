using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class CashFlowLine : ModelEntityBase
    {
        private System.String JournalEntryIdValue = string.Empty;
        /// <summary>
        /// Gets or sets the journal entry id.
        /// </summary>
        public System.String JournalEntryId
        {
            get { return this.JournalEntryIdValue; }
            set { SetProperty(ref JournalEntryIdValue, value); }
        }

        private CashFlowCategory CategoryValue;
        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        public CashFlowCategory Category
        {
            get { return this.CategoryValue; }
            set { SetProperty(ref CategoryValue, value); }
        }

        private System.Decimal AmountValue;
        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        public System.Decimal Amount
        {
            get { return this.AmountValue; }
            set { SetProperty(ref AmountValue, value); }
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
}
