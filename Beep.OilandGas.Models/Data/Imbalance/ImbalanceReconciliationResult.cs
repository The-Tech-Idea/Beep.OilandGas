using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Imbalance
{
    public class ImbalanceReconciliationResult : ModelEntityBase
    {
        private string ReconciliationIdValue;

        public string ReconciliationId

        {

            get { return this.ReconciliationIdValue; }

            set { SetProperty(ref ReconciliationIdValue, value); }

        }
        private string ImbalanceIdValue;

        public string ImbalanceId

        {

            get { return this.ImbalanceIdValue; }

            set { SetProperty(ref ImbalanceIdValue, value); }

        }
        private decimal ImbalanceBeforeValue;

        public decimal ImbalanceBefore

        {

            get { return this.ImbalanceBeforeValue; }

            set { SetProperty(ref ImbalanceBeforeValue, value); }

        }
        private decimal ImbalanceAfterValue;

        public decimal ImbalanceAfter

        {

            get { return this.ImbalanceAfterValue; }

            set { SetProperty(ref ImbalanceAfterValue, value); }

        }
        private bool IsReconciledValue;

        public bool IsReconciled

        {

            get { return this.IsReconciledValue; }

            set { SetProperty(ref IsReconciledValue, value); }

        }
        private DateTime ReconciliationDateValue = DateTime.UtcNow;

        public DateTime ReconciliationDate

        {

            get { return this.ReconciliationDateValue; }

            set { SetProperty(ref ReconciliationDateValue, value); }

        }
        private string ReconciledByValue;

        public string ReconciledBy

        {

            get { return this.ReconciledByValue; }

            set { SetProperty(ref ReconciledByValue, value); }

        }
    }
}
