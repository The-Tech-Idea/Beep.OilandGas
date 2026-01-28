using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting.Pricing
{
    public class PriceIndexRequest : ModelEntityBase
    {
        private string IndexNameValue = string.Empty;

        public string IndexName

        {

            get { return this.IndexNameValue; }

            set { SetProperty(ref IndexNameValue, value); }

        }
        private DateTime IndexDateValue;

        public DateTime IndexDate

        {

            get { return this.IndexDateValue; }

            set { SetProperty(ref IndexDateValue, value); }

        }
        private decimal PriceValue;

        public decimal Price

        {

            get { return this.PriceValue; }

            set { SetProperty(ref PriceValue, value); }

        }
        private string? CurrencyValue;

        public string? Currency

        {

            get { return this.CurrencyValue; }

            set { SetProperty(ref CurrencyValue, value); }

        }
    }
}
