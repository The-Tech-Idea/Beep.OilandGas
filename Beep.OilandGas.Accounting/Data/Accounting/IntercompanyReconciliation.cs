using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public class IntercompanyReconciliation : ModelEntityBase
    {
        private string Company1IdValue = string.Empty;

        public string Company1Id

        {

            get { return this.Company1IdValue; }

            set { SetProperty(ref Company1IdValue, value); }

        }
        private string Company2IdValue = string.Empty;

        public string Company2Id

        {

            get { return this.Company2IdValue; }

            set { SetProperty(ref Company2IdValue, value); }

        }
        private DateTime PeriodEndValue;

        public DateTime PeriodEnd

        {

            get { return this.PeriodEndValue; }

            set { SetProperty(ref PeriodEndValue, value); }

        }
        private decimal Company1AmountValue;

        public decimal Company1Amount

        {

            get { return this.Company1AmountValue; }

            set { SetProperty(ref Company1AmountValue, value); }

        }
        private decimal Company2AmountValue;

        public decimal Company2Amount

        {

            get { return this.Company2AmountValue; }

            set { SetProperty(ref Company2AmountValue, value); }

        }
        private decimal VarianceValue;

        public decimal Variance

        {

            get { return this.VarianceValue; }

            set { SetProperty(ref VarianceValue, value); }

        }
        private bool IsReconciledValue;

        public bool IsReconciled

        {

            get { return this.IsReconciledValue; }

            set { SetProperty(ref IsReconciledValue, value); }

        }
        private List<UnmatchedTransaction> UnmatchedTransactionsValue = new();

        public List<UnmatchedTransaction> UnmatchedTransactions

        {

            get { return this.UnmatchedTransactionsValue; }

            set { SetProperty(ref UnmatchedTransactionsValue, value); }

        }
    }
}
