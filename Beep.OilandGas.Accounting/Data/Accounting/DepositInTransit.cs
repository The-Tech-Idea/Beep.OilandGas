using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class DepositInTransit : ModelEntityBase
    {
        private System.Decimal AmountValue;
        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        public System.Decimal Amount
        {
            get { return this.AmountValue; }
            set { SetProperty(ref AmountValue, value); }
        }

        private System.DateTime DepositDateValue;
        /// <summary>
        /// Gets or sets the deposit date.
        /// </summary>
        public System.DateTime DepositDate
        {
            get { return this.DepositDateValue; }
            set { SetProperty(ref DepositDateValue, value); }
        }
    }
}
