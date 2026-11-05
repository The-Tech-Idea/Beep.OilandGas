using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public class ARAgingSummary : ModelEntityBase
    {
        private string ArInvoiceIdValue;

        public string ArInvoiceId

        {

            get { return this.ArInvoiceIdValue; }

            set { SetProperty(ref ArInvoiceIdValue, value); }

        }
        private string InvoiceNumberValue;

        public string InvoiceNumber

        {

            get { return this.InvoiceNumberValue; }

            set { SetProperty(ref InvoiceNumberValue, value); }

        }
        private string CustomerBaIdValue;

        public string CustomerBaId

        {

            get { return this.CustomerBaIdValue; }

            set { SetProperty(ref CustomerBaIdValue, value); }

        }
        private DateTime? InvoiceDateValue;

        public DateTime? InvoiceDate

        {

            get { return this.InvoiceDateValue; }

            set { SetProperty(ref InvoiceDateValue, value); }

        }
        private DateTime? DueDateValue;

        public DateTime? DueDate

        {

            get { return this.DueDateValue; }

            set { SetProperty(ref DueDateValue, value); }

        }
        private decimal? TotalAmountValue;

        public decimal? TotalAmount

        {

            get { return this.TotalAmountValue; }

            set { SetProperty(ref TotalAmountValue, value); }

        }
        private decimal? PaidAmountValue;

        public decimal? PaidAmount

        {

            get { return this.PaidAmountValue; }

            set { SetProperty(ref PaidAmountValue, value); }

        }
        private decimal? BalanceDueValue;

        public decimal? BalanceDue

        {

            get { return this.BalanceDueValue; }

            set { SetProperty(ref BalanceDueValue, value); }

        }
        private int DaysPastDueValue;

        public int DaysPastDue

        {

            get { return this.DaysPastDueValue; }

            set { SetProperty(ref DaysPastDueValue, value); }

        }
        private string AgingBucketValue;

        public string AgingBucket

        {

            get { return this.AgingBucketValue; }

            set { SetProperty(ref AgingBucketValue, value); }

        } // Current, 1-30, 31-60, 61-90, 90+
    }
}
