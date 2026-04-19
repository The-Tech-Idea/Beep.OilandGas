using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public class ReconciliationVariance : ModelEntityBase
    {
        private string VarianceIdValue = string.Empty;

        public string VarianceId

        {

            get { return this.VarianceIdValue; }

            set { SetProperty(ref VarianceIdValue, value); }

        }
        private string AccountIdValue = string.Empty;

        public string AccountId

        {

            get { return this.AccountIdValue; }

            set { SetProperty(ref AccountIdValue, value); }

        }
        private DateTime TransactionDateValue;

        public DateTime TransactionDate

        {

            get { return this.TransactionDateValue; }

            set { SetProperty(ref TransactionDateValue, value); }

        }
        private string DescriptionValue = string.Empty;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private decimal AmountValue;

        public decimal Amount

        {

            get { return this.AmountValue; }

            set { SetProperty(ref AmountValue, value); }

        }
        private string GLReferenceValue = string.Empty;

        public string GLReference

        {

            get { return this.GLReferenceValue; }

            set { SetProperty(ref GLReferenceValue, value); }

        }
        private string SubledgerReferenceValue = string.Empty;

        public string SubledgerReference

        {

            get { return this.SubledgerReferenceValue; }

            set { SetProperty(ref SubledgerReferenceValue, value); }

        }
        private ReconciliationVarianceType TypeValue;

        public ReconciliationVarianceType Type

        {

            get { return this.TypeValue; }

            set { SetProperty(ref TypeValue, value); }

        }
        private string StatusValue = string.Empty;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        } // Open, Resolved, Investigated
    }
}
