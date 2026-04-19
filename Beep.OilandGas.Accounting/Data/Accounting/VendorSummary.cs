using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public class VendorSummary : ModelEntityBase
    {
        private string VendorBaIdValue;

        public string VendorBaId

        {

            get { return this.VendorBaIdValue; }

            set { SetProperty(ref VendorBaIdValue, value); }

        }
        private decimal TotalInvoicesValue;

        public decimal TotalInvoices

        {

            get { return this.TotalInvoicesValue; }

            set { SetProperty(ref TotalInvoicesValue, value); }

        }
        private decimal TotalPaidValue;

        public decimal TotalPaid

        {

            get { return this.TotalPaidValue; }

            set { SetProperty(ref TotalPaidValue, value); }

        }
        private decimal TotalOutstandingValue;

        public decimal TotalOutstanding

        {

            get { return this.TotalOutstandingValue; }

            set { SetProperty(ref TotalOutstandingValue, value); }

        }
        private int NumberOfInvoicesValue;

        public int NumberOfInvoices

        {

            get { return this.NumberOfInvoicesValue; }

            set { SetProperty(ref NumberOfInvoicesValue, value); }

        }
        private int NumberOfOverdueInvoicesValue;

        public int NumberOfOverdueInvoices

        {

            get { return this.NumberOfOverdueInvoicesValue; }

            set { SetProperty(ref NumberOfOverdueInvoicesValue, value); }

        }
        private decimal AverageDaysToPayValue;

        public decimal AverageDaysToPay

        {

            get { return this.AverageDaysToPayValue; }

            set { SetProperty(ref AverageDaysToPayValue, value); }

        }
    }
}
