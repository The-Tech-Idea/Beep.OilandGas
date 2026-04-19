using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class PaymentTypeAnalysis : ModelEntityBase
    {
        private System.String PaymentMethodValue = string.Empty;
        /// <summary>
        /// Gets or sets the payment method.
        /// </summary>
        public System.String PaymentMethod
        {
            get { return this.PaymentMethodValue; }
            set { SetProperty(ref PaymentMethodValue, value); }
        }

        private System.Int32 CountValue;
        /// <summary>
        /// Gets or sets count.
        /// </summary>
        public System.Int32 Count
        {
            get { return this.CountValue; }
            set { SetProperty(ref CountValue, value); }
        }

        private System.Decimal TotalAmountValue;
        /// <summary>
        /// Gets or sets total amount.
        /// </summary>
        public System.Decimal TotalAmount
        {
            get { return this.TotalAmountValue; }
            set { SetProperty(ref TotalAmountValue, value); }
        }

        private System.Decimal AverageAmountValue;
        /// <summary>
        /// Gets or sets average amount.
        /// </summary>
        public System.Decimal AverageAmount
        {
            get { return this.AverageAmountValue; }
            set { SetProperty(ref AverageAmountValue, value); }
        }
    }
}
