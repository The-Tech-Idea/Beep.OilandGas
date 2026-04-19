using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class GLEntryLine : ModelEntityBase
    {
        private System.DateTime EntryDateValue;
        /// <summary>
        /// Gets or sets entry date.
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

        private System.String ReferenceValue = string.Empty;
        /// <summary>
        /// Gets or sets reference.
        /// </summary>
        public System.String Reference
        {
            get { return this.ReferenceValue; }
            set { SetProperty(ref ReferenceValue, value); }
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

        private System.Decimal DebitAmountValue;
        /// <summary>
        /// Gets or sets debit amount.
        /// </summary>
        public System.Decimal DebitAmount
        {
            get { return this.DebitAmountValue; }
            set { SetProperty(ref DebitAmountValue, value); }
        }

        private System.Decimal CreditAmountValue;
        /// <summary>
        /// Gets or sets credit amount.
        /// </summary>
        public System.Decimal CreditAmount
        {
            get { return this.CreditAmountValue; }
            set { SetProperty(ref CreditAmountValue, value); }
        }

        private System.Decimal RunningBalanceValue;
        /// <summary>
        /// Gets or sets running balance.
        /// </summary>
        public System.Decimal RunningBalance
        {
            get { return this.RunningBalanceValue; }
            set { SetProperty(ref RunningBalanceValue, value); }
        }
    }
}
