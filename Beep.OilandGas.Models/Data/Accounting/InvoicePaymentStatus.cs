
namespace Beep.OilandGas.Models.Data.Accounting
{
    public class InvoicePaymentStatus : ModelEntityBase
    {
        private string InvoiceIdValue;

        public string InvoiceId

        {

            get { return this.InvoiceIdValue; }

            set { SetProperty(ref InvoiceIdValue, value); }

        }
        private string InvoiceNumberValue;

        public string InvoiceNumber

        {

            get { return this.InvoiceNumberValue; }

            set { SetProperty(ref InvoiceNumberValue, value); }

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
        private string PaymentStatusValue;

        public string PaymentStatus

        {

            get { return this.PaymentStatusValue; }

            set { SetProperty(ref PaymentStatusValue, value); }

        } // Paid, Partial, Unpaid, Overpaid
        private int NumberOfPaymentsValue;

        public int NumberOfPayments

        {

            get { return this.NumberOfPaymentsValue; }

            set { SetProperty(ref NumberOfPaymentsValue, value); }

        }
        private DateTime? LastPaymentDateValue;

        public DateTime? LastPaymentDate

        {

            get { return this.LastPaymentDateValue; }

            set { SetProperty(ref LastPaymentDateValue, value); }

        }
        private DateTime? DueDateValue;

        public DateTime? DueDate

        {

            get { return this.DueDateValue; }

            set { SetProperty(ref DueDateValue, value); }

        }
        private bool IsOverdueValue;

        public bool IsOverdue

        {

            get { return this.IsOverdueValue; }

            set { SetProperty(ref IsOverdueValue, value); }

        }
        private int DaysPastDueValue;

        public int DaysPastDue

        {

            get { return this.DaysPastDueValue; }

            set { SetProperty(ref DaysPastDueValue, value); }

        }
    }
}
