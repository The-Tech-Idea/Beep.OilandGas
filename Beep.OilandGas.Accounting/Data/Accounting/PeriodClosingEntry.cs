using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class PeriodClosingEntry : ModelEntityBase
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
}
