using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class QuarterlyPayment : ModelEntityBase
    {
        private System.Int32 QuarterValue;
        /// <summary>
        /// Gets or sets quarter.
        /// </summary>
        public System.Int32 Quarter
        {
            get { return this.QuarterValue; }
            set { SetProperty(ref QuarterValue, value); }
        }

        private System.DateTime DueDateValue;
        /// <summary>
        /// Gets or sets due date.
        /// </summary>
        public System.DateTime DueDate
        {
            get { return this.DueDateValue; }
            set { SetProperty(ref DueDateValue, value); }
        }

        private System.Decimal BasePaymentValue;
        /// <summary>
        /// Gets or sets base payment.
        /// </summary>
        public System.Decimal BasePayment
        {
            get { return this.BasePaymentValue; }
            set { SetProperty(ref BasePaymentValue, value); }
        }

        private System.Decimal AdjustmentsValue;
        /// <summary>
        /// Gets or sets adjustments.
        /// </summary>
        public System.Decimal Adjustments
        {
            get { return this.AdjustmentsValue; }
            set { SetProperty(ref AdjustmentsValue, value); }
        }

        private System.Decimal TotalPaymentValue;
        /// <summary>
        /// Gets or sets total payment.
        /// </summary>
        public System.Decimal TotalPayment
        {
            get { return this.TotalPaymentValue; }
            set { SetProperty(ref TotalPaymentValue, value); }
        }
    }
}
