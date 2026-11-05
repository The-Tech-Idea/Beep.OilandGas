using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public class PaymentApplicationRequest : ModelEntityBase
    {
        private string PaymentIdValue;

        public string PaymentId

        {

            get { return this.PaymentIdValue; }

            set { SetProperty(ref PaymentIdValue, value); }

        }
        private List<PaymentApplicationItem> ApplicationsValue = new List<PaymentApplicationItem>();

        public List<PaymentApplicationItem> Applications

        {

            get { return this.ApplicationsValue; }

            set { SetProperty(ref ApplicationsValue, value); }

        }
    }
}
