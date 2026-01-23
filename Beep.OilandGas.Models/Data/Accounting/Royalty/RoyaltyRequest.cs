using System;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Accounting.Royalty
{
    /// <summary>
    /// Request DTO for creating a royalty payment
    /// </summary>
    public class CreateRoyaltyPaymentRequest : ModelEntityBase
    {
        private string RoyaltyOwnerIdValue = string.Empty;

        public string RoyaltyOwnerId

        {

            get { return this.RoyaltyOwnerIdValue; }

            set { SetProperty(ref RoyaltyOwnerIdValue, value); }

        }
        private decimal RoyaltyAmountValue;

        public decimal RoyaltyAmount

        {

            get { return this.RoyaltyAmountValue; }

            set { SetProperty(ref RoyaltyAmountValue, value); }

        }
        private decimal? RoyaltyInterestValue;

        public decimal? RoyaltyInterest

        {

            get { return this.RoyaltyInterestValue; }

            set { SetProperty(ref RoyaltyInterestValue, value); }

        }
        private DateTime? PaymentDateValue;

        public DateTime? PaymentDate

        {

            get { return this.PaymentDateValue; }

            set { SetProperty(ref PaymentDateValue, value); }

        }
    }

    /// <summary>
    /// Request DTO for calculating royalty
    /// </summary>
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







