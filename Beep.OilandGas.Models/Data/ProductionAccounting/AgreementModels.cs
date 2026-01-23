using System;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// Pricing method enumeration.
    /// </summary>


    /// <summary>
    /// Represents an oil sales agreement (DTO for calculations/reporting).
    /// </summary>
    public class OilSalesAgreement : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the agreement identifier.
        /// </summary>
        private string AgreementIdValue = string.Empty;

        public string AgreementId

        {

            get { return this.AgreementIdValue; }

            set { SetProperty(ref AgreementIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the agreement name or description.
        /// </summary>
        private string AgreementNameValue = string.Empty;

        public string AgreementName

        {

            get { return this.AgreementNameValue; }

            set { SetProperty(ref AgreementNameValue, value); }

        }

        /// <summary>
        /// Gets or sets the seller (producer).
        /// </summary>
        private string SellerValue = string.Empty;

        public string Seller

        {

            get { return this.SellerValue; }

            set { SetProperty(ref SellerValue, value); }

        }

        /// <summary>
        /// Gets or sets the purchaser.
        /// </summary>
        private string PurchaserValue = string.Empty;

        public string Purchaser

        {

            get { return this.PurchaserValue; }

            set { SetProperty(ref PurchaserValue, value); }

        }

        /// <summary>
        /// Gets or sets the effective date.
        /// </summary>
        private DateTime EffectiveDateValue;

        public DateTime EffectiveDate

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the expiration date.
        /// </summary>
        private DateTime? ExpirationDateValue;

        public DateTime? ExpirationDate

        {

            get { return this.ExpirationDateValue; }

            set { SetProperty(ref ExpirationDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the pricing terms.
        /// </summary>
        private PricingTerms PricingTermsValue = new();

        public PricingTerms PricingTerms

        {

            get { return this.PricingTermsValue; }

            set { SetProperty(ref PricingTermsValue, value); }

        }

        /// <summary>
        /// Gets or sets the delivery terms.
        /// </summary>
        private DeliveryTerms DeliveryTermsValue = new();

        public DeliveryTerms DeliveryTerms

        {

            get { return this.DeliveryTermsValue; }

            set { SetProperty(ref DeliveryTermsValue, value); }

        }

        /// <summary>
        /// Gets or sets the quality specifications.
        /// </summary>
        private CrudeOilSpecifications? QualitySpecificationsValue;

        public CrudeOilSpecifications? QualitySpecifications

        {

            get { return this.QualitySpecificationsValue; }

            set { SetProperty(ref QualitySpecificationsValue, value); }

        }

        /// <summary>
        /// Gets or sets the minimum volume commitment (barrels).
        /// </summary>
        private decimal? MinimumVolumeCommitmentValue;

        public decimal? MinimumVolumeCommitment

        {

            get { return this.MinimumVolumeCommitmentValue; }

            set { SetProperty(ref MinimumVolumeCommitmentValue, value); }

        }

        /// <summary>
        /// Gets or sets the maximum volume commitment (barrels).
        /// </summary>
        private decimal? MaximumVolumeCommitmentValue;

        public decimal? MaximumVolumeCommitment

        {

            get { return this.MaximumVolumeCommitmentValue; }

            set { SetProperty(ref MaximumVolumeCommitmentValue, value); }

        }

        /// <summary>
        /// Gets or sets the payment terms (days).
        /// </summary>
        private int PaymentTermsDaysValue = 30;

        public int PaymentTermsDays

        {

            get { return this.PaymentTermsDaysValue; }

            set { SetProperty(ref PaymentTermsDaysValue, value); }

        }
    }

    /// <summary>
    /// Represents pricing terms in a sales agreement.
    /// </summary>
    public class PricingTerms : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the pricing method.
        /// </summary>
        private PricingMethod PricingMethodValue;

        public PricingMethod PricingMethod

        {

            get { return this.PricingMethodValue; }

            set { SetProperty(ref PricingMethodValue, value); }

        }

        /// <summary>
        /// Gets or sets the base price (if fixed).
        /// </summary>
        private decimal? BasePriceValue;

        public decimal? BasePrice

        {

            get { return this.BasePriceValue; }

            set { SetProperty(ref BasePriceValue, value); }

        }

        /// <summary>
        /// Gets or sets the price index (if index-based).
        /// </summary>
        private string? PriceIndexValue;

        public string? PriceIndex

        {

            get { return this.PriceIndexValue; }

            set { SetProperty(ref PriceIndexValue, value); }

        }

        /// <summary>
        /// Gets or sets the differential (premium or discount).
        /// </summary>
        private decimal DifferentialValue;

        public decimal Differential

        {

            get { return this.DifferentialValue; }

            set { SetProperty(ref DifferentialValue, value); }

        }

        /// <summary>
        /// Gets or sets the API gravity differential per degree.
        /// </summary>
        private decimal? ApiGravityDifferentialValue;

        public decimal? ApiGravityDifferential

        {

            get { return this.ApiGravityDifferentialValue; }

            set { SetProperty(ref ApiGravityDifferentialValue, value); }

        }

        /// <summary>
        /// Gets or sets the sulfur content differential per 0.1%.
        /// </summary>
        private decimal? SulfurDifferentialValue;

        public decimal? SulfurDifferential

        {

            get { return this.SulfurDifferentialValue; }

            set { SetProperty(ref SulfurDifferentialValue, value); }

        }

        /// <summary>
        /// Gets or sets the location differential.
        /// </summary>
        private decimal? LocationDifferentialValue;

        public decimal? LocationDifferential

        {

            get { return this.LocationDifferentialValue; }

            set { SetProperty(ref LocationDifferentialValue, value); }

        }
    }

    /// <summary>
    /// Represents delivery terms in a sales agreement.
    /// </summary>
    public class DeliveryTerms : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the delivery point.
        /// </summary>
        private string DeliveryPointValue = string.Empty;

        public string DeliveryPoint

        {

            get { return this.DeliveryPointValue; }

            set { SetProperty(ref DeliveryPointValue, value); }

        }

        /// <summary>
        /// Gets or sets the delivery method (pipeline, truck, etc.).
        /// </summary>
        private string DeliveryMethodValue = string.Empty;

        public string DeliveryMethod

        {

            get { return this.DeliveryMethodValue; }

            set { SetProperty(ref DeliveryMethodValue, value); }

        }

        /// <summary>
        /// Gets or sets the delivery frequency.
        /// </summary>
        private string DeliveryFrequencyValue = "Daily";

        public string DeliveryFrequency

        {

            get { return this.DeliveryFrequencyValue; }

            set { SetProperty(ref DeliveryFrequencyValue, value); }

        }

        /// <summary>
        /// Gets or sets whether title transfers at delivery point.
        /// </summary>
        private bool TitleTransferAtDeliveryPointValue = true;

        public bool TitleTransferAtDeliveryPoint

        {

            get { return this.TitleTransferAtDeliveryPointValue; }

            set { SetProperty(ref TitleTransferAtDeliveryPointValue, value); }

        }
    }

    /// <summary>
    /// Represents a transportation agreement.
    /// </summary>
    public class TransportationAgreement : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the agreement identifier.
        /// </summary>
        private string AgreementIdValue = string.Empty;

        public string AgreementId

        {

            get { return this.AgreementIdValue; }

            set { SetProperty(ref AgreementIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the carrier or pipeline company.
        /// </summary>
        private string CarrierValue = string.Empty;

        public string Carrier

        {

            get { return this.CarrierValue; }

            set { SetProperty(ref CarrierValue, value); }

        }

        /// <summary>
        /// Gets or sets the origin point.
        /// </summary>
        private string OriginPointValue = string.Empty;

        public string OriginPoint

        {

            get { return this.OriginPointValue; }

            set { SetProperty(ref OriginPointValue, value); }

        }

        /// <summary>
        /// Gets or sets the destination point.
        /// </summary>
        private string DestinationPointValue = string.Empty;

        public string DestinationPoint

        {

            get { return this.DestinationPointValue; }

            set { SetProperty(ref DestinationPointValue, value); }

        }

        /// <summary>
        /// Gets or sets the effective date.
        /// </summary>
        private DateTime EffectiveDateValue;

        public DateTime EffectiveDate

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the expiration date.
        /// </summary>
        private DateTime? ExpirationDateValue;

        public DateTime? ExpirationDate

        {

            get { return this.ExpirationDateValue; }

            set { SetProperty(ref ExpirationDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the tariff rate per barrel.
        /// </summary>
        private decimal TariffRateValue;

        public decimal TariffRate

        {

            get { return this.TariffRateValue; }

            set { SetProperty(ref TariffRateValue, value); }

        }

        /// <summary>
        /// Gets or sets the minimum volume commitment (barrels).
        /// </summary>
        private decimal? MinimumVolumeCommitmentValue;

        public decimal? MinimumVolumeCommitment

        {

            get { return this.MinimumVolumeCommitmentValue; }

            set { SetProperty(ref MinimumVolumeCommitmentValue, value); }

        }

        /// <summary>
        /// Gets or sets the maximum volume capacity (barrels per day).
        /// </summary>
        private decimal? MaximumCapacityValue;

        public decimal? MaximumCapacity

        {

            get { return this.MaximumCapacityValue; }

            set { SetProperty(ref MaximumCapacityValue, value); }

        }
    }

    /// <summary>
    /// Represents a processing agreement.
    /// </summary>
    public class ProcessingAgreement : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the agreement identifier.
        /// </summary>
        private string AgreementIdValue = string.Empty;

        public string AgreementId

        {

            get { return this.AgreementIdValue; }

            set { SetProperty(ref AgreementIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the processor company.
        /// </summary>
        private string ProcessorValue = string.Empty;

        public string Processor

        {

            get { return this.ProcessorValue; }

            set { SetProperty(ref ProcessorValue, value); }

        }

        /// <summary>
        /// Gets or sets the processing facility.
        /// </summary>
        private string ProcessingFacilityValue = string.Empty;

        public string ProcessingFacility

        {

            get { return this.ProcessingFacilityValue; }

            set { SetProperty(ref ProcessingFacilityValue, value); }

        }

        /// <summary>
        /// Gets or sets the effective date.
        /// </summary>
        private DateTime EffectiveDateValue;

        public DateTime EffectiveDate

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the processing fee per barrel.
        /// </summary>
        private decimal ProcessingFeeValue;

        public decimal ProcessingFee

        {

            get { return this.ProcessingFeeValue; }

            set { SetProperty(ref ProcessingFeeValue, value); }

        }

        /// <summary>
        /// Gets or sets the processing percentage (if percentage-based).
        /// </summary>
        private decimal? ProcessingPercentageValue;

        public decimal? ProcessingPercentage

        {

            get { return this.ProcessingPercentageValue; }

            set { SetProperty(ref ProcessingPercentageValue, value); }

        }
    }

    /// <summary>
    /// Represents a storage agreement.
    /// </summary>
    public class StorageAgreement : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the agreement identifier.
        /// </summary>
        private string AgreementIdValue = string.Empty;

        public string AgreementId

        {

            get { return this.AgreementIdValue; }

            set { SetProperty(ref AgreementIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the storage facility.
        /// </summary>
        private string StorageFacilityValue = string.Empty;

        public string StorageFacility

        {

            get { return this.StorageFacilityValue; }

            set { SetProperty(ref StorageFacilityValue, value); }

        }

        /// <summary>
        /// Gets or sets the effective date.
        /// </summary>
        private DateTime EffectiveDateValue;

        public DateTime EffectiveDate

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the storage fee per barrel per month.
        /// </summary>
        private decimal StorageFeeValue;

        public decimal StorageFee

        {

            get { return this.StorageFeeValue; }

            set { SetProperty(ref StorageFeeValue, value); }

        }

        /// <summary>
        /// Gets or sets the reserved capacity (barrels).
        /// </summary>
        private decimal? ReservedCapacityValue;

        public decimal? ReservedCapacity

        {

            get { return this.ReservedCapacityValue; }

            set { SetProperty(ref ReservedCapacityValue, value); }

        }
    }
}








