using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public class PaymentBatchResult : ModelEntityBase
    {
        private string BatchIdValue = Guid.NewGuid().ToString();

        public string BatchId

        {

            get { return this.BatchIdValue; }

            set { SetProperty(ref BatchIdValue, value); }

        }
        private DateTime BatchDateValue;

        public DateTime BatchDate

        {

            get { return this.BatchDateValue; }

            set { SetProperty(ref BatchDateValue, value); }

        }
        private int TotalPaymentsValue;

        public int TotalPayments

        {

            get { return this.TotalPaymentsValue; }

            set { SetProperty(ref TotalPaymentsValue, value); }

        }
        private decimal TotalAmountValue;

        public decimal TotalAmount

        {

            get { return this.TotalAmountValue; }

            set { SetProperty(ref TotalAmountValue, value); }

        }
        private int SuccessfulPaymentsValue;

        public int SuccessfulPayments

        {

            get { return this.SuccessfulPaymentsValue; }

            set { SetProperty(ref SuccessfulPaymentsValue, value); }

        }
        private int FailedPaymentsValue;

        public int FailedPayments

        {

            get { return this.FailedPaymentsValue; }

            set { SetProperty(ref FailedPaymentsValue, value); }

        }
        private List<string> ProcessedPaymentIdsValue = new List<string>();

        public List<string> ProcessedPaymentIds

        {

            get { return this.ProcessedPaymentIdsValue; }

            set { SetProperty(ref ProcessedPaymentIdsValue, value); }

        }
        private List<string> FailedPaymentIdsValue = new List<string>();

        public List<string> FailedPaymentIds

        {

            get { return this.FailedPaymentIdsValue; }

            set { SetProperty(ref FailedPaymentIdsValue, value); }

        }
    }
}
