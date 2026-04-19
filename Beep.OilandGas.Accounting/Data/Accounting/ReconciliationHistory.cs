using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public class ReconciliationHistory : ModelEntityBase
    {
        private string ReconciliationIdValue = string.Empty;

        public string ReconciliationId

        {

            get { return this.ReconciliationIdValue; }

            set { SetProperty(ref ReconciliationIdValue, value); }

        }
        private DateTime ReconciliationDateValue;

        public DateTime ReconciliationDate

        {

            get { return this.ReconciliationDateValue; }

            set { SetProperty(ref ReconciliationDateValue, value); }

        }
        private DateTime PeriodEndValue;

        public DateTime PeriodEnd

        {

            get { return this.PeriodEndValue; }

            set { SetProperty(ref PeriodEndValue, value); }

        }
        private string PerformedByValue = string.Empty;

        public string PerformedBy

        {

            get { return this.PerformedByValue; }

            set { SetProperty(ref PerformedByValue, value); }

        }
        private decimal VarianceAmountValue;

        public decimal VarianceAmount

        {

            get { return this.VarianceAmountValue; }

            set { SetProperty(ref VarianceAmountValue, value); }

        }
        private bool WasReconciledValue;

        public bool WasReconciled

        {

            get { return this.WasReconciledValue; }

            set { SetProperty(ref WasReconciledValue, value); }

        }
    }
}
