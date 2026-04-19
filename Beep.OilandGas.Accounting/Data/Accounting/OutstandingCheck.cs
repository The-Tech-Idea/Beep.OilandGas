using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class OutstandingCheck : ModelEntityBase
    {
        private System.String CheckNumberValue = string.Empty;
        /// <summary>
        /// Gets or sets the check number.
        /// </summary>
        public System.String CheckNumber
        {
            get { return this.CheckNumberValue; }
            set { SetProperty(ref CheckNumberValue, value); }
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

        private System.DateTime CheckDateValue;
        /// <summary>
        /// Gets or sets the check date.
        /// </summary>
        public System.DateTime CheckDate
        {
            get { return this.CheckDateValue; }
            set { SetProperty(ref CheckDateValue, value); }
        }
    }
}
