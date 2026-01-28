using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting.Revenue
{
    public class CreateRevenueTransactionRequest : ModelEntityBase
    {
        private decimal RevenueAmountValue;

        public decimal RevenueAmount

        {

            get { return this.RevenueAmountValue; }

            set { SetProperty(ref RevenueAmountValue, value); }

        }
        private DateTime? TransactionDateValue;

        public DateTime? TransactionDate

        {

            get { return this.TransactionDateValue; }

            set { SetProperty(ref TransactionDateValue, value); }

        }
        private string? RunTicketNumberValue;

        public string? RunTicketNumber

        {

            get { return this.RunTicketNumberValue; }

            set { SetProperty(ref RunTicketNumberValue, value); }

        }
        private string? PropertyIdValue;

        public string? PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
    }
}
