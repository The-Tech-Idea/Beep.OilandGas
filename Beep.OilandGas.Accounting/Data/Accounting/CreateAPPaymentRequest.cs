using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public class CreateAPPaymentRequest : ModelEntityBase
    {
        private string ApInvoiceIdValue;

        public string ApInvoiceId

        {

            get { return this.ApInvoiceIdValue; }

            set { SetProperty(ref ApInvoiceIdValue, value); }

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

        } // Check, ACH, Wire
        private string CheckNumberValue;

        public string CheckNumber

        {

            get { return this.CheckNumberValue; }

            set { SetProperty(ref CheckNumberValue, value); }

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
