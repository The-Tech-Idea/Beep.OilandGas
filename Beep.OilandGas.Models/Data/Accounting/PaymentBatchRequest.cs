using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public class PaymentBatchRequest : ModelEntityBase
    {
        private List<string> PaymentIdsValue = new List<string>();

        public List<string> PaymentIds

        {

            get { return this.PaymentIdsValue; }

            set { SetProperty(ref PaymentIdsValue, value); }

        }
        private DateTime BatchDateValue;

        public DateTime BatchDate

        {

            get { return this.BatchDateValue; }

            set { SetProperty(ref BatchDateValue, value); }

        }
        private string PaymentMethodValue;

        public string PaymentMethod

        {

            get { return this.PaymentMethodValue; }

            set { SetProperty(ref PaymentMethodValue, value); }

        }
        private string BatchDescriptionValue;

        public string BatchDescription

        {

            get { return this.BatchDescriptionValue; }

            set { SetProperty(ref BatchDescriptionValue, value); }

        }
    }
}
