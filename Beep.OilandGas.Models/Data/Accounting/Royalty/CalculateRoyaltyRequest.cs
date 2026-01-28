using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting.Royalty
{
    public class CalculateRoyaltyRequest : ModelEntityBase
    {
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? PoolIdValue;

        public string? PoolId

        {

            get { return this.PoolIdValue; }

            set { SetProperty(ref PoolIdValue, value); }

        }
        private DateTime CalculationDateValue;

        public DateTime CalculationDate

        {

            get { return this.CalculationDateValue; }

            set { SetProperty(ref CalculationDateValue, value); }

        }
        private decimal? OilRoyaltyRateValue;

        public decimal? OilRoyaltyRate

        {

            get { return this.OilRoyaltyRateValue; }

            set { SetProperty(ref OilRoyaltyRateValue, value); }

        }
        private decimal? GasRoyaltyRateValue;

        public decimal? GasRoyaltyRate

        {

            get { return this.GasRoyaltyRateValue; }

            set { SetProperty(ref GasRoyaltyRateValue, value); }

        }
        private decimal? OilPriceValue;

        public decimal? OilPrice

        {

            get { return this.OilPriceValue; }

            set { SetProperty(ref OilPriceValue, value); }

        }
        private decimal? GasPriceValue;

        public decimal? GasPrice

        {

            get { return this.GasPriceValue; }

            set { SetProperty(ref GasPriceValue, value); }

        }
    }
}
