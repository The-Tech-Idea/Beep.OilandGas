using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class GovernmentalTaxData : ModelEntityBase
    {
        private decimal SeveranceTaxValue;

        public decimal SeveranceTax

        {

            get { return this.SeveranceTaxValue; }

            set { SetProperty(ref SeveranceTaxValue, value); }

        }
        private decimal AdValoremTaxValue;

        public decimal AdValoremTax

        {

            get { return this.AdValoremTaxValue; }

            set { SetProperty(ref AdValoremTaxValue, value); }

        }
        public decimal TotalTaxes => SeveranceTax + AdValoremTax;
    }
}
