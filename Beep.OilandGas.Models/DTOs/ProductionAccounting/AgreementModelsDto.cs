using System;

namespace Beep.OilandGas.Models.DTOs.ProductionAccounting
{
    /// <summary>
    /// Pricing method enumeration.
    /// </summary>
    public enum PricingMethod
    {
        /// <summary>
        /// Fixed price per barrel.
        /// </summary>
        Fixed,

        /// <summary>
        /// Index-based pricing (WTI, Brent, etc.).
        /// </summary>
        IndexBased,

        /// <summary>
        /// Posted price.
        /// </summary>
        PostedPrice,

        /// <summary>
        /// Spot price.
        /// </summary>
        SpotPrice
    }

    /// <summary>
    /// Represents an oil sales agreement (DTO for calculations/reporting).
    /// </summary>
    public class OilSalesAgreementDto
    {
        /// <summary>
        /// Gets or sets the agreement identifier.
        /// </summary>
        public string AgreementId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the agreement name or description.
        /// </summary>
        public string AgreementName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the seller (producer).
        /// </summary>
        public string Seller { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the purchaser.
        /// </summary>
        public string Purchaser { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the effective date.
        /// </summary>
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Gets or sets the expiration date.
        /// </summary>
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets the pricing terms.
        /// </summary>
        public PricingTermsDto PricingTerms { get; set; } = new();

        /// <summary>
        /// Gets or sets the delivery terms.
        /// </summary>
        public DeliveryTermsDto DeliveryTerms { get; set; } = new();

        /// <summary>
        /// Gets or sets the quality specifications.
        /// </summary>
        public CrudeOilSpecificationsDto? QualitySpecifications { get; set; }

        /// <summary>
        /// Gets or sets the minimum volume commitment (barrels).
        /// </summary>
        public decimal? MinimumVolumeCommitment { get; set; }

        /// <summary>
        /// Gets or sets the maximum volume commitment (barrels).
        /// </summary>
        public decimal? MaximumVolumeCommitment { get; set; }

        /// <summary>
        /// Gets or sets the payment terms (days).
        /// </summary>
        public int PaymentTermsDays { get; set; } = 30;
    }

    /// <summary>
    /// Represents pricing terms in a sales agreement.
    /// </summary>
    public class PricingTermsDto
    {
        /// <summary>
        /// Gets or sets the pricing method.
        /// </summary>
        public PricingMethod PricingMethod { get; set; }

        /// <summary>
        /// Gets or sets the base price (if fixed).
        /// </summary>
        public decimal? BasePrice { get; set; }

        /// <summary>
        /// Gets or sets the price index (if index-based).
        /// </summary>
        public string? PriceIndex { get; set; }

        /// <summary>
        /// Gets or sets the differential (premium or discount).
        /// </summary>
        public decimal Differential { get; set; }

        /// <summary>
        /// Gets or sets the API gravity differential per degree.
        /// </summary>
        public decimal? ApiGravityDifferential { get; set; }

        /// <summary>
        /// Gets or sets the sulfur content differential per 0.1%.
        /// </summary>
        public decimal? SulfurDifferential { get; set; }

        /// <summary>
        /// Gets or sets the location differential.
        /// </summary>
        public decimal? LocationDifferential { get; set; }
    }

    /// <summary>
    /// Represents delivery terms in a sales agreement.
    /// </summary>
    public class DeliveryTermsDto
    {
        /// <summary>
        /// Gets or sets the delivery point.
        /// </summary>
        public string DeliveryPoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the delivery method (pipeline, truck, etc.).
        /// </summary>
        public string DeliveryMethod { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the delivery frequency.
        /// </summary>
        public string DeliveryFrequency { get; set; } = "Daily";

        /// <summary>
        /// Gets or sets whether title transfers at delivery point.
        /// </summary>
        public bool TitleTransferAtDeliveryPoint { get; set; } = true;
    }

    /// <summary>
    /// Represents a transportation agreement.
    /// </summary>
    public class TransportationAgreementDto
    {
        /// <summary>
        /// Gets or sets the agreement identifier.
        /// </summary>
        public string AgreementId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the carrier or pipeline company.
        /// </summary>
        public string Carrier { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the origin point.
        /// </summary>
        public string OriginPoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the destination point.
        /// </summary>
        public string DestinationPoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the effective date.
        /// </summary>
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Gets or sets the expiration date.
        /// </summary>
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets the tariff rate per barrel.
        /// </summary>
        public decimal TariffRate { get; set; }

        /// <summary>
        /// Gets or sets the minimum volume commitment (barrels).
        /// </summary>
        public decimal? MinimumVolumeCommitment { get; set; }

        /// <summary>
        /// Gets or sets the maximum volume capacity (barrels per day).
        /// </summary>
        public decimal? MaximumCapacity { get; set; }
    }

    /// <summary>
    /// Represents a processing agreement.
    /// </summary>
    public class ProcessingAgreementDto
    {
        /// <summary>
        /// Gets or sets the agreement identifier.
        /// </summary>
        public string AgreementId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the processor company.
        /// </summary>
        public string Processor { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the processing facility.
        /// </summary>
        public string ProcessingFacility { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the effective date.
        /// </summary>
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Gets or sets the processing fee per barrel.
        /// </summary>
        public decimal ProcessingFee { get; set; }

        /// <summary>
        /// Gets or sets the processing percentage (if percentage-based).
        /// </summary>
        public decimal? ProcessingPercentage { get; set; }
    }

    /// <summary>
    /// Represents a storage agreement.
    /// </summary>
    public class StorageAgreementDto
    {
        /// <summary>
        /// Gets or sets the agreement identifier.
        /// </summary>
        public string AgreementId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the storage facility.
        /// </summary>
        public string StorageFacility { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the effective date.
        /// </summary>
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Gets or sets the storage fee per barrel per month.
        /// </summary>
        public decimal StorageFee { get; set; }

        /// <summary>
        /// Gets or sets the reserved capacity (barrels).
        /// </summary>
        public decimal? ReservedCapacity { get; set; }
    }
}

