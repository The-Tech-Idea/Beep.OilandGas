using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public class ReceivableResponse : ModelEntityBase
    {
        private string ReceivableIdValue;

        public string ReceivableId

        {

            get { return this.ReceivableIdValue; }

            set { SetProperty(ref ReceivableIdValue, value); }

        }
        private string SalesTransactionIdValue;

        public string SalesTransactionId

        {

            get { return this.SalesTransactionIdValue; }

            set { SetProperty(ref SalesTransactionIdValue, value); }

        }
        private string CustomerBaIdValue;

        public string CustomerBaId

        {

            get { return this.CustomerBaIdValue; }

            set { SetProperty(ref CustomerBaIdValue, value); }

        }
        private string? InvoiceNumberValue;

        public string? InvoiceNumber

        {

            get { return this.InvoiceNumberValue; }

            set { SetProperty(ref InvoiceNumberValue, value); }

        }
        private DateTime InvoiceDateValue;

        public DateTime InvoiceDate

        {

            get { return this.InvoiceDateValue; }

            set { SetProperty(ref InvoiceDateValue, value); }

        }
        private DateTime DueDateValue;

        public DateTime DueDate

        {

            get { return this.DueDateValue; }

            set { SetProperty(ref DueDateValue, value); }

        }
        private decimal OriginalAmountValue;

        public decimal OriginalAmount

        {

            get { return this.OriginalAmountValue; }

            set { SetProperty(ref OriginalAmountValue, value); }

        }
        private decimal AmountPaidValue;

        public decimal AmountPaid

        {

            get { return this.AmountPaidValue; }

            set { SetProperty(ref AmountPaidValue, value); }

        }
        private decimal OutstandingBalanceValue;

        public decimal OutstandingBalance

        {

            get { return this.OutstandingBalanceValue; }

            set { SetProperty(ref OutstandingBalanceValue, value); }

        }
        private string StatusValue;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }
}
