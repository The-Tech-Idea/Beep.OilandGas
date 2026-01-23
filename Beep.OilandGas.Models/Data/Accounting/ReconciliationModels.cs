using System;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Accounting
{
    /// <summary>
    /// Reconciliation summary.
    /// </summary>
    public partial class ReconciliationSummary : ModelEntityBase
    {
        private System.String AccountKeyValue = string.Empty;
        /// <summary>
        /// Gets or sets account key.
        /// </summary>
        public System.String AccountKey
        {
            get { return this.AccountKeyValue; }
            set { SetProperty(ref AccountKeyValue, value); }
        }

        private System.String AccountIdValue = string.Empty;
        /// <summary>
        /// Gets or sets account id.
        /// </summary>
        public System.String AccountId
        {
            get { return this.AccountIdValue; }
            set { SetProperty(ref AccountIdValue, value); }
        }

        private System.Nullable<System.DateTime> AsOfDateValue;
        /// <summary>
        /// Gets or sets as of date.
        /// </summary>
        public System.Nullable<System.DateTime> AsOfDate
        {
            get { return this.AsOfDateValue; }
            set { SetProperty(ref AsOfDateValue, value); }
        }

        private System.Decimal SubledgerTotalValue;
        /// <summary>
        /// Gets or sets subledger total.
        /// </summary>
        public System.Decimal SubledgerTotal
        {
            get { return this.SubledgerTotalValue; }
            set { SetProperty(ref SubledgerTotalValue, value); }
        }

        private System.Decimal GlBalanceValue;
        /// <summary>
        /// Gets or sets GL balance.
        /// </summary>
        public System.Decimal GlBalance
        {
            get { return this.GlBalanceValue; }
            set { SetProperty(ref GlBalanceValue, value); }
        }

        private System.Decimal DifferenceValue;
        /// <summary>
        /// Gets or sets difference.
        /// </summary>
        public System.Decimal Difference
        {
            get { return this.DifferenceValue; }
            set { SetProperty(ref DifferenceValue, value); }
        }
    }
}


