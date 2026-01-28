using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class AgedItem : ModelEntityBase
    {
        private System.DateTime EntryDateValue;
        /// <summary>
        /// Gets or sets the entry date.
        /// </summary>
        public System.DateTime EntryDate
        {
            get { return this.EntryDateValue; }
            set { SetProperty(ref EntryDateValue, value); }
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

        private System.Decimal AmountValue;
        /// <summary>
        /// Gets or sets amount.
        /// </summary>
        public System.Decimal Amount
        {
            get { return this.AmountValue; }
            set { SetProperty(ref AmountValue, value); }
        }

        private System.Int32 AgeInDaysValue;
        /// <summary>
        /// Gets or sets age in days.
        /// </summary>
        public System.Int32 AgeInDays
        {
            get { return this.AgeInDaysValue; }
            set { SetProperty(ref AgeInDaysValue, value); }
        }

        private System.String ReferenceValue = string.Empty;
        /// <summary>
        /// Gets or sets reference.
        /// </summary>
        public System.String Reference
        {
            get { return this.ReferenceValue; }
            set { SetProperty(ref ReferenceValue, value); }
        }
    }
}
