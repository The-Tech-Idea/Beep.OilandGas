using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Accounting.Operational.Trading
{
    /// <summary>
    /// Type of exchange contract.
    /// </summary>
    public enum ExchangeContractType
    {
        /// <summary>
        /// Physical exchange (crude for crude).
        /// </summary>
        PhysicalExchange,

        /// <summary>
        /// Buy/sell exchange (buy at one location, sell at another).
        /// </summary>
        BuySellExchange,

        /// <summary>
        /// Multi-party exchange.
        /// </summary>
        MultiPartyExchange,

        /// <summary>
        /// Time exchange (exchange volumes across time periods).
        /// </summary>
        TimeExchange
    }

    /// <summary>
    /// Represents an exchange contract.
    /// </summary>
    public class ExchangeContract
    {
        /// <summary>
        /// Gets or sets the contract identifier.
        /// </summary>
        public string ContractId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the contract name or description.
        /// </summary>
        public string ContractName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the contract type.
        /// </summary>
        public ExchangeContractType ContractType { get; set; }

        /// <summary>
        /// Gets or sets the parties to the contract.
        /// </summary>
        public List<ExchangeParty> Parties { get; set; } = new();

        /// <summary>
        /// Gets or sets the effective date.
        /// </summary>
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Gets or sets the expiration date.
        /// </summary>
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets the exchange terms.
        /// </summary>
        public ExchangeTerms Terms { get; set; } = new();

        /// <summary>
        /// Gets or sets the delivery points.
        /// </summary>
        public List<ExchangeDeliveryPoint> DeliveryPoints { get; set; } = new();

        /// <summary>
        /// Gets or sets the pricing terms.
        /// </summary>
        public ExchangePricingTerms PricingTerms { get; set; } = new();
    }

    /// <summary>
    /// Represents a party to an exchange contract.
    /// </summary>
    public class ExchangeParty
    {
        /// <summary>
        /// Gets or sets the party name.
        /// </summary>
        public string PartyName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether this party is the initiator.
        /// </summary>
        public bool IsInitiator { get; set; }

        /// <summary>
        /// Gets or sets the role (Buyer, Seller, Exchanger).
        /// </summary>
        public string Role { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents exchange terms.
    /// </summary>
    public class ExchangeTerms
    {
        /// <summary>
        /// Gets or sets the exchange ratio (if applicable).
        /// </summary>
        public decimal? ExchangeRatio { get; set; }

        /// <summary>
        /// Gets or sets the minimum volume per exchange in barrels.
        /// </summary>
        public decimal? MinimumVolume { get; set; }

        /// <summary>
        /// Gets or sets the maximum volume per exchange in barrels.
        /// </summary>
        public decimal? MaximumVolume { get; set; }

        /// <summary>
        /// Gets or sets the quality specifications.
        /// </summary>
        public string? QualitySpecifications { get; set; }

        /// <summary>
        /// Gets or sets the notice period in days.
        /// </summary>
        public int NoticePeriodDays { get; set; } = 30;
    }

    /// <summary>
    /// Represents an exchange delivery point.
    /// </summary>
    public class ExchangeDeliveryPoint
    {
        /// <summary>
        /// Gets or sets the delivery point identifier.
        /// </summary>
        public string DeliveryPointId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the delivery point name.
        /// </summary>
        public string DeliveryPointName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether this is a receipt point.
        /// </summary>
        public bool IsReceiptPoint { get; set; }

        /// <summary>
        /// Gets or sets whether this is a delivery point.
        /// </summary>
        public bool IsDeliveryPoint { get; set; }
    }

    /// <summary>
    /// Represents exchange pricing terms.
    /// </summary>
    public class ExchangePricingTerms
    {
        /// <summary>
        /// Gets or sets the base price index.
        /// </summary>
        public string? BasePriceIndex { get; set; }

        /// <summary>
        /// Gets or sets the location differential.
        /// </summary>
        public decimal LocationDifferential { get; set; }

        /// <summary>
        /// Gets or sets the quality differential.
        /// </summary>
        public decimal QualityDifferential { get; set; }

        /// <summary>
        /// Gets or sets the time differential.
        /// </summary>
        public decimal TimeDifferential { get; set; }
    }

    /// <summary>
    /// Type of exchange commitment.
    /// </summary>
    public enum ExchangeCommitmentType
    {
        /// <summary>
        /// Current month commitment.
        /// </summary>
        CurrentMonth,

        /// <summary>
        /// Forward month commitment.
        /// </summary>
        ForwardMonth,

        /// <summary>
        /// Annual commitment.
        /// </summary>
        Annual
    }

    /// <summary>
    /// Represents an exchange commitment.
    /// </summary>
    public class ExchangeCommitment
    {
        /// <summary>
        /// Gets or sets the commitment identifier.
        /// </summary>
        public string CommitmentId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the contract reference.
        /// </summary>
        public string ContractId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the commitment type.
        /// </summary>
        public ExchangeCommitmentType CommitmentType { get; set; }

        /// <summary>
        /// Gets or sets the committed volume in barrels.
        /// </summary>
        public decimal CommittedVolume { get; set; }

        /// <summary>
        /// Gets or sets the delivery period start date.
        /// </summary>
        public DateTime DeliveryPeriodStart { get; set; }

        /// <summary>
        /// Gets or sets the delivery period end date.
        /// </summary>
        public DateTime DeliveryPeriodEnd { get; set; }

        /// <summary>
        /// Gets or sets the actual volume delivered in barrels.
        /// </summary>
        public decimal ActualVolumeDelivered { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public ExchangeCommitmentStatus Status { get; set; } = ExchangeCommitmentStatus.Pending;

        /// <summary>
        /// Gets the remaining commitment.
        /// </summary>
        public decimal RemainingCommitment => CommittedVolume - ActualVolumeDelivered;

        /// <summary>
        /// Gets the fulfillment percentage.
        /// </summary>
        public decimal FulfillmentPercentage => CommittedVolume > 0 
            ? (ActualVolumeDelivered / CommittedVolume) * 100m 
            : 0m;
    }

    /// <summary>
    /// Exchange commitment status.
    /// </summary>
    public enum ExchangeCommitmentStatus
    {
        /// <summary>
        /// Pending fulfillment.
        /// </summary>
        Pending,

        /// <summary>
        /// Partially fulfilled.
        /// </summary>
        PartiallyFulfilled,

        /// <summary>
        /// Fully fulfilled.
        /// </summary>
        Fulfilled,

        /// <summary>
        /// Cancelled.
        /// </summary>
        Cancelled
    }
}

