using System;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Agreement
{
    public class CreateSalesAgreementRequest : ModelEntityBase
    {
        private string AgreementNameValue;

        public string AgreementName

        {

            get { return this.AgreementNameValue; }

            set { SetProperty(ref AgreementNameValue, value); }

        }
        private string SellerBaIdValue;

        public string SellerBaId

        {

            get { return this.SellerBaIdValue; }

            set { SetProperty(ref SellerBaIdValue, value); }

        }
        private string PurchaserBaIdValue;

        public string PurchaserBaId

        {

            get { return this.PurchaserBaIdValue; }

            set { SetProperty(ref PurchaserBaIdValue, value); }

        }
        private DateTime EffectiveDateValue;

        public DateTime EffectiveDate

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }
        private DateTime? ExpirationDateValue;

        public DateTime? ExpirationDate

        {

            get { return this.ExpirationDateValue; }

            set { SetProperty(ref ExpirationDateValue, value); }

        }
        private string PricingMethodValue;

        public string PricingMethod

        {

            get { return this.PricingMethodValue; }

            set { SetProperty(ref PricingMethodValue, value); }

        }
        private decimal? BasePriceValue;

        public decimal? BasePrice

        {

            get { return this.BasePriceValue; }

            set { SetProperty(ref BasePriceValue, value); }

        }
        private string PriceIndexValue;

        public string PriceIndex

        {

            get { return this.PriceIndexValue; }

            set { SetProperty(ref PriceIndexValue, value); }

        }
        private decimal? DifferentialValue;

        public decimal? Differential

        {

            get { return this.DifferentialValue; }

            set { SetProperty(ref DifferentialValue, value); }

        }
        private int PaymentTermsDaysValue;

        public int PaymentTermsDays

        {

            get { return this.PaymentTermsDaysValue; }

            set { SetProperty(ref PaymentTermsDaysValue, value); }

        }
    }

    public class CreateTransportationAgreementRequest : ModelEntityBase
    {
        private string CarrierBaIdValue;

        public string CarrierBaId

        {

            get { return this.CarrierBaIdValue; }

            set { SetProperty(ref CarrierBaIdValue, value); }

        }
        private string OriginPointValue;

        public string OriginPoint

        {

            get { return this.OriginPointValue; }

            set { SetProperty(ref OriginPointValue, value); }

        }
        private string DestinationPointValue;

        public string DestinationPoint

        {

            get { return this.DestinationPointValue; }

            set { SetProperty(ref DestinationPointValue, value); }

        }
        private DateTime EffectiveDateValue;

        public DateTime EffectiveDate

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }
        private DateTime? ExpirationDateValue;

        public DateTime? ExpirationDate

        {

            get { return this.ExpirationDateValue; }

            set { SetProperty(ref ExpirationDateValue, value); }

        }
        private decimal TariffRateValue;

        public decimal TariffRate

        {

            get { return this.TariffRateValue; }

            set { SetProperty(ref TariffRateValue, value); }

        }
        private decimal? MinimumVolumeCommitmentValue;

        public decimal? MinimumVolumeCommitment

        {

            get { return this.MinimumVolumeCommitmentValue; }

            set { SetProperty(ref MinimumVolumeCommitmentValue, value); }

        }
        private decimal? MaximumCapacityValue;

        public decimal? MaximumCapacity

        {

            get { return this.MaximumCapacityValue; }

            set { SetProperty(ref MaximumCapacityValue, value); }

        }
    }
}







