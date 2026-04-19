using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class ProductionTax : ModelEntityBase
    {
        private string TaxTypeValue = string.Empty;

        public string TaxType

        {

            get { return this.TaxTypeValue; }

            set { SetProperty(ref TaxTypeValue, value); }

        }
        private string TaxNameValue = string.Empty;

        public string TaxName

        {

            get { return this.TaxNameValue; }

            set { SetProperty(ref TaxNameValue, value); }

        }
        private decimal TaxRateValue;

        public decimal TaxRate

        {

            get { return this.TaxRateValue; }

            set { SetProperty(ref TaxRateValue, value); }

        }
        private decimal AmountValue;

        public decimal Amount

        {

            get { return this.AmountValue; }

            set { SetProperty(ref AmountValue, value); }

        }
    }
}
