using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public class AccountReconciliationResult : ModelEntityBase
    {
        private string ReconciliationIdValue = Guid.NewGuid().ToString();

        public string ReconciliationId

        {

            get { return this.ReconciliationIdValue; }

            set { SetProperty(ref ReconciliationIdValue, value); }

        }
        private string GlAccountIdValue;

        public string GlAccountId

        {

            get { return this.GlAccountIdValue; }

            set { SetProperty(ref GlAccountIdValue, value); }

        }
        private DateTime ReconciliationDateValue;

        public DateTime ReconciliationDate

        {

            get { return this.ReconciliationDateValue; }

            set { SetProperty(ref ReconciliationDateValue, value); }

        }
        private decimal? BookBalanceValue;

        public decimal? BookBalance

        {

            get { return this.BookBalanceValue; }

            set { SetProperty(ref BookBalanceValue, value); }

        }
        private decimal? ReconciledBalanceValue;

        public decimal? ReconciledBalance

        {

            get { return this.ReconciledBalanceValue; }

            set { SetProperty(ref ReconciledBalanceValue, value); }

        }
        private decimal? DifferenceValue;

        public decimal? Difference

        {

            get { return this.DifferenceValue; }

            set { SetProperty(ref DifferenceValue, value); }

        }
        private bool IsReconciledValue;

        public bool IsReconciled

        {

            get { return this.IsReconciledValue; }

            set { SetProperty(ref IsReconciledValue, value); }

        }
        private string StatusValue;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private string UserIdValue;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }
}
