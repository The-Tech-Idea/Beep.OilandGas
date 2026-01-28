using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class CheckClearingAnalysis : ModelEntityBase
    {
        private System.String AccountNumberValue = string.Empty;
        /// <summary>
        /// Gets or sets the account number.
        /// </summary>
        public System.String AccountNumber
        {
            get { return this.AccountNumberValue; }
            set { SetProperty(ref AccountNumberValue, value); }
        }

        private System.DateTime PeriodStartValue;
        /// <summary>
        /// Gets or sets the period start.
        /// </summary>
        public System.DateTime PeriodStart
        {
            get { return this.PeriodStartValue; }
            set { SetProperty(ref PeriodStartValue, value); }
        }

        private System.DateTime PeriodEndValue;
        /// <summary>
        /// Gets or sets the period end.
        /// </summary>
        public System.DateTime PeriodEnd
        {
            get { return this.PeriodEndValue; }
            set { SetProperty(ref PeriodEndValue, value); }
        }

        private System.DateTime AnalysisDateValue;
        /// <summary>
        /// Gets or sets the analysis date.
        /// </summary>
        public System.DateTime AnalysisDate
        {
            get { return this.AnalysisDateValue; }
            set { SetProperty(ref AnalysisDateValue, value); }
        }

        private System.Int32 TotalPaymentsValue;
        /// <summary>
        /// Gets or sets total payment count.
        /// </summary>
        public System.Int32 TotalPayments
        {
            get { return this.TotalPaymentsValue; }
            set { SetProperty(ref TotalPaymentsValue, value); }
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

        private System.Int32 AverageProcessingTimeValue;
        /// <summary>
        /// Gets or sets average processing time.
        /// </summary>
        public System.Int32 AverageProcessingTime
        {
            get { return this.AverageProcessingTimeValue; }
            set { SetProperty(ref AverageProcessingTimeValue, value); }
        }

        private List<PaymentTypeAnalysis> PaymentsByTypeValue = new List<PaymentTypeAnalysis>();
        /// <summary>
        /// Gets or sets payment type analysis.
        /// </summary>
        public List<PaymentTypeAnalysis> PaymentsByType
        {
            get { return this.PaymentsByTypeValue; }
            set { SetProperty(ref PaymentsByTypeValue, value); }
        }
    }
}
