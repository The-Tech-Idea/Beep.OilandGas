using System;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Accounting.Pricing
{
    /// <summary>
    /// Request DTO for price index
    /// </summary>
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

    /// <summary>
    /// Request DTO for valuing a run ticket
    /// </summary>
    public class ValueRunTicketRequest : ModelEntityBase
    {
        private string RunTicketNumberValue = string.Empty;

        public string RunTicketNumber

        {

            get { return this.RunTicketNumberValue; }

            set { SetProperty(ref RunTicketNumberValue, value); }

        }
        private string PricingMethodValue = string.Empty;

        public string PricingMethod

        {

            get { return this.PricingMethodValue; }

            set { SetProperty(ref PricingMethodValue, value); }

        }
        private decimal? FixedPriceValue;

        public decimal? FixedPrice

        {

            get { return this.FixedPriceValue; }

            set { SetProperty(ref FixedPriceValue, value); }

        }
        private string? IndexNameValue;

        public string? IndexName

        {

            get { return this.IndexNameValue; }

            set { SetProperty(ref IndexNameValue, value); }

        }
        private decimal? DifferentialValue;

        public decimal? Differential

        {

            get { return this.DifferentialValue; }

            set { SetProperty(ref DifferentialValue, value); }

        }
    }
}







