using Beep.OilandGas.Models.Data.Accounting;
using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Accounting.Models
{
    public class PaymentApplicationItem : ModelEntityBase
    {
        private string InvoiceIdValue;

        public string InvoiceId

        {

            get { return this.InvoiceIdValue; }

            set { SetProperty(ref InvoiceIdValue, value); }

        }
        private decimal AmountValue;

        public decimal Amount

        {

            get { return this.AmountValue; }

            set { SetProperty(ref AmountValue, value); }

        }
    }
}
