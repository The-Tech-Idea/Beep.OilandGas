using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public class CreateARCreditMemoRequest : ModelEntityBase
    {
        private string ArInvoiceIdValue;

        public string ArInvoiceId

        {

            get { return this.ArInvoiceIdValue; }

            set { SetProperty(ref ArInvoiceIdValue, value); }

        }
        private string CreditMemoNumberValue;

        public string CreditMemoNumber

        {

            get { return this.CreditMemoNumberValue; }

            set { SetProperty(ref CreditMemoNumberValue, value); }

        }
        private DateTime CreditMemoDateValue;

        public DateTime CreditMemoDate

        {

            get { return this.CreditMemoDateValue; }

            set { SetProperty(ref CreditMemoDateValue, value); }

        }
        private decimal CreditAmountValue;

        public decimal CreditAmount

        {

            get { return this.CreditAmountValue; }

            set { SetProperty(ref CreditAmountValue, value); }

        }
        private string ReasonValue;

        public string Reason

        {

            get { return this.ReasonValue; }

            set { SetProperty(ref ReasonValue, value); }

        }
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }
}
