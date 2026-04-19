using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class Form1099Info : ModelEntityBase
    {
        private string TaxIdValue = string.Empty;

        public string TaxId

        {

            get { return this.TaxIdValue; }

            set { SetProperty(ref TaxIdValue, value); }

        }
        private string OwnerNameValue = string.Empty;

        public string OwnerName

        {

            get { return this.OwnerNameValue; }

            set { SetProperty(ref OwnerNameValue, value); }

        }
        private int TaxYearValue;

        public int TaxYear

        {

            get { return this.TaxYearValue; }

            set { SetProperty(ref TaxYearValue, value); }

        }
        private decimal TotalPaymentsValue;

        public decimal TotalPayments

        {

            get { return this.TotalPaymentsValue; }

            set { SetProperty(ref TotalPaymentsValue, value); }

        }
        private decimal TotalWithholdingsValue;

        public decimal TotalWithholdings

        {

            get { return this.TotalWithholdingsValue; }

            set { SetProperty(ref TotalWithholdingsValue, value); }

        }
        private string? AddressValue;

        public string? Address

        {

            get { return this.AddressValue; }

            set { SetProperty(ref AddressValue, value); }

        }
    }
}
