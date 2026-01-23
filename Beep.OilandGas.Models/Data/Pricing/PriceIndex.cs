#nullable enable

namespace Beep.OilandGas.Models.Data.Pricing
{
    /// <summary>
    /// Represents a price index reference in the oil and gas domain.
    /// </summary>
    public class PriceIndex : ModelEntityBase
    {
        private string? PriceIndexIdValue;

        public string? PriceIndexId

        {

            get { return this.PriceIndexIdValue; }

            set { SetProperty(ref PriceIndexIdValue, value); }

        }
        private string? IndexNameValue;

        public string? IndexName

        {

            get { return this.IndexNameValue; }

            set { SetProperty(ref IndexNameValue, value); }

        }
        private string? IndexCodeValue;

        public string? IndexCode

        {

            get { return this.IndexCodeValue; }

            set { SetProperty(ref IndexCodeValue, value); }

        }
        private decimal? CurrentPriceValue;

        public decimal? CurrentPrice

        {

            get { return this.CurrentPriceValue; }

            set { SetProperty(ref CurrentPriceValue, value); }

        }
        private decimal? PreviousPriceValue;

        public decimal? PreviousPrice

        {

            get { return this.PreviousPriceValue; }

            set { SetProperty(ref PreviousPriceValue, value); }

        }
        private DateTime? PriceDateValue;

        public DateTime? PriceDate

        {

            get { return this.PriceDateValue; }

            set { SetProperty(ref PriceDateValue, value); }

        }
        private string? UnitValue;

        public string? Unit

        {

            get { return this.UnitValue; }

            set { SetProperty(ref UnitValue, value); }

        }

    }
}


