
namespace Beep.OilandGas.Models.Data.Pricing
{
    public class RegulatedPrice : ModelEntityBase
    {
        private string? RegulatedPriceIdValue;

        public string? RegulatedPriceId

        {

            get { return this.RegulatedPriceIdValue; }

            set { SetProperty(ref RegulatedPriceIdValue, value); }

        }
        private string? RegulationNameValue;

        public string? RegulationName

        {

            get { return this.RegulationNameValue; }

            set { SetProperty(ref RegulationNameValue, value); }

        }
        private decimal? PriceValue;

        public decimal? Price

        {

            get { return this.PriceValue; }

            set { SetProperty(ref PriceValue, value); }

        }
        private DateTime? EffectiveDateValue;

        public DateTime? EffectiveDate

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }
        private string? CommodityValue;

        public string? Commodity

        {

            get { return this.CommodityValue; }

            set { SetProperty(ref CommodityValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }
}
