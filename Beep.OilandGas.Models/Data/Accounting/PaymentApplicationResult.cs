using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public class PaymentApplicationResult : ModelEntityBase
    {
        private string PaymentIdValue;

        public string PaymentId

        {

            get { return this.PaymentIdValue; }

            set { SetProperty(ref PaymentIdValue, value); }

        }
        private decimal TotalAppliedValue;

        public decimal TotalApplied

        {

            get { return this.TotalAppliedValue; }

            set { SetProperty(ref TotalAppliedValue, value); }

        }
        private int SuccessfulApplicationsValue;

        public int SuccessfulApplications

        {

            get { return this.SuccessfulApplicationsValue; }

            set { SetProperty(ref SuccessfulApplicationsValue, value); }

        }
        private int FailedApplicationsValue;

        public int FailedApplications

        {

            get { return this.FailedApplicationsValue; }

            set { SetProperty(ref FailedApplicationsValue, value); }

        }
        private List<string> AppliedInvoiceIdsValue = new List<string>();

        public List<string> AppliedInvoiceIds

        {

            get { return this.AppliedInvoiceIdsValue; }

            set { SetProperty(ref AppliedInvoiceIdsValue, value); }

        }
        private List<string> FailedInvoiceIdsValue = new List<string>();

        public List<string> FailedInvoiceIds

        {

            get { return this.FailedInvoiceIdsValue; }

            set { SetProperty(ref FailedInvoiceIdsValue, value); }

        }
    }
}
