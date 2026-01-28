using System;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class PriceIndexRequest : ModelEntityBase
    {
        private string IndexNameValue = string.Empty;

        [Required]
        public string IndexName

        {

            get { return this.IndexNameValue; }

            set { SetProperty(ref IndexNameValue, value); }

        }
        private DateTime IndexDateValue;

        [Required]
        public DateTime IndexDate

        {

            get { return this.IndexDateValue; }

            set { SetProperty(ref IndexDateValue, value); }

        }
        private decimal PriceValue;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price

        {

            get { return this.PriceValue; }

            set { SetProperty(ref PriceValue, value); }

        }
        private string? CurrencyValue = "USD";

        public string? Currency

        {

            get { return this.CurrencyValue; }

            set { SetProperty(ref CurrencyValue, value); }

        }
    }
}
