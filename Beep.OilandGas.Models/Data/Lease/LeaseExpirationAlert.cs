using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Lease
{
    public class LeaseExpirationAlert : ModelEntityBase 
    {
        private string LeaseIdValue = string.Empty;

        public string LeaseId

        {

            get { return this.LeaseIdValue; }

            set { SetProperty(ref LeaseIdValue, value); }

        }
        private DateTime ExpirationDateValue;

        public DateTime ExpirationDate

        {

            get { return this.ExpirationDateValue; }

            set { SetProperty(ref ExpirationDateValue, value); }

        }
        private int DaysUntilExpirationValue;

        public int DaysUntilExpiration

        {

            get { return this.DaysUntilExpirationValue; }

            set { SetProperty(ref DaysUntilExpirationValue, value); }

        }
        private string AlertMessageValue = string.Empty;

        public string AlertMessage

        {

            get { return this.AlertMessageValue; }

            set { SetProperty(ref AlertMessageValue, value); }

        }
    }
}
