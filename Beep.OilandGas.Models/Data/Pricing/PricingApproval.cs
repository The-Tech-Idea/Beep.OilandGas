using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Pricing
{
    public class PricingApproval : ModelEntityBase
    {
        private string ValuationIdValue;

        public string ValuationId

        {

            get { return this.ValuationIdValue; }

            set { SetProperty(ref ValuationIdValue, value); }

        }
        private string RunTicketNumberValue;

        public string RunTicketNumber

        {

            get { return this.RunTicketNumberValue; }

            set { SetProperty(ref RunTicketNumberValue, value); }

        }
        private decimal TotalValueValue;

        public decimal TotalValue

        {

            get { return this.TotalValueValue; }

            set { SetProperty(ref TotalValueValue, value); }

        }
        private string PricingMethodValue;

        public string PricingMethod

        {

            get { return this.PricingMethodValue; }

            set { SetProperty(ref PricingMethodValue, value); }

        }
        private string StatusValue;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private DateTime ValuationDateValue;

        public DateTime ValuationDate

        {

            get { return this.ValuationDateValue; }

            set { SetProperty(ref ValuationDateValue, value); }

        }
    }
}
