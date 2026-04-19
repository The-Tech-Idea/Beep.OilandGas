
namespace Beep.OilandGas.Models.Data.Accounting
{
    public class CreateInvoicePaymentRequest : ModelEntityBase
    {
        private string InvoiceIdValue;

        public string InvoiceId

        {

            get { return this.InvoiceIdValue; }

            set { SetProperty(ref InvoiceIdValue, value); }

        }
        private DateTime PaymentDateValue;

        public DateTime PaymentDate

        {

            get { return this.PaymentDateValue; }

            set { SetProperty(ref PaymentDateValue, value); }

        }
        private decimal PaymentAmountValue;

        public decimal PaymentAmount

        {

            get { return this.PaymentAmountValue; }

            set { SetProperty(ref PaymentAmountValue, value); }

        }
        private string PaymentMethodValue;

        public string PaymentMethod

        {

            get { return this.PaymentMethodValue; }

            set { SetProperty(ref PaymentMethodValue, value); }

        }
        private string ReferenceNumberValue;

        public string ReferenceNumber

        {

            get { return this.ReferenceNumberValue; }

            set { SetProperty(ref ReferenceNumberValue, value); }

        }
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }
}
