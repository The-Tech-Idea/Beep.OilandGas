using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.General
{
    public class ReconciliationSummary : ModelEntityBase
    {
        private string AccountIdValue;

        public string AccountId

        {

            get { return this.AccountIdValue; }

            set { SetProperty(ref AccountIdValue, value); }

        }
        private DateTime PeriodEndValue;

        public DateTime PeriodEnd

        {

            get { return this.PeriodEndValue; }

            set { SetProperty(ref PeriodEndValue, value); }

        }
        private decimal GLBalanceValue;

        public decimal GLBalance

        {

            get { return this.GLBalanceValue; }

            set { SetProperty(ref GLBalanceValue, value); }

        }
        private decimal SubledgerBalanceValue;

        public decimal SubledgerBalance

        {

            get { return this.SubledgerBalanceValue; }

            set { SetProperty(ref SubledgerBalanceValue, value); }

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
        private List<ReconciliationVariance> VariancesValue = new();

        public List<ReconciliationVariance> Variances

        {

            get { return this.VariancesValue; }

            set { SetProperty(ref VariancesValue, value); }

        }
    }
}
