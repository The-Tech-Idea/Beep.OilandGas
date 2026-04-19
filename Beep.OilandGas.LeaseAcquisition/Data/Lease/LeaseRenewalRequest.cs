using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Lease
{
    public class LeaseRenewalRequest : ModelEntityBase 
    {
        private string LeaseIdValue = string.Empty;

        public string LeaseId

        {

            get { return this.LeaseIdValue; }

            set { SetProperty(ref LeaseIdValue, value); }

        }
        private DateTime NewExpirationDateValue;

        public DateTime NewExpirationDate

        {

            get { return this.NewExpirationDateValue; }

            set { SetProperty(ref NewExpirationDateValue, value); }

        }
        private string RenewalTermsValue = string.Empty;

        public string RenewalTerms

        {

            get { return this.RenewalTermsValue; }

            set { SetProperty(ref RenewalTermsValue, value); }

        }
    }
}
