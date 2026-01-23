using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// Type of exchange contract.
    /// </summary>


    /// <summary>
    /// Exchange commitment status.
    /// </summary>


    /// <summary>
    /// Type of exchange commitment.
    /// </summary>


    /// <summary>
    /// Represents an exchange contract (DTO for calculations/reporting).
    /// </summary>
    public class ExchangeContract : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the contract identifier.
        /// </summary>
        private string ContractIdValue = string.Empty;

        public string ContractId

        {

            get { return this.ContractIdValue; }

            set { SetProperty(ref ContractIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the contract name or description.
        /// </summary>
        private string ContractNameValue = string.Empty;

        public string ContractName

        {

            get { return this.ContractNameValue; }

            set { SetProperty(ref ContractNameValue, value); }

        }

        /// <summary>
        /// Gets or sets the contract type.
        /// </summary>
        private ExchangeContractType ContractTypeValue;

        public ExchangeContractType ContractType

        {

            get { return this.ContractTypeValue; }

            set { SetProperty(ref ContractTypeValue, value); }

        }

        /// <summary>
        /// Gets or sets the parties to the contract.
        /// </summary>
        private List<ExchangeParty> PartiesValue = new();

        public List<ExchangeParty> Parties

        {

            get { return this.PartiesValue; }

            set { SetProperty(ref PartiesValue, value); }

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
        /// Gets or sets the exchange terms.
        /// </summary>
        private ExchangeTerms TermsValue = new();

        public ExchangeTerms Terms

        {

            get { return this.TermsValue; }

            set { SetProperty(ref TermsValue, value); }

        }

        /// <summary>
        /// Gets or sets the delivery points.
        /// </summary>
        private List<ExchangeDeliveryPoint> DeliveryPointsValue = new();

        public List<ExchangeDeliveryPoint> DeliveryPoints

        {

            get { return this.DeliveryPointsValue; }

            set { SetProperty(ref DeliveryPointsValue, value); }

        }

        /// <summary>
        /// Gets or sets the pricing terms.
        /// </summary>
        private ExchangePricingTerms PricingTermsValue = new();

        public ExchangePricingTerms PricingTerms

        {

            get { return this.PricingTermsValue; }

            set { SetProperty(ref PricingTermsValue, value); }

        }
    }

    /// <summary>
    /// Represents a party to an exchange contract.
    /// </summary>
    public class ExchangeParty : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the party name.
        /// </summary>
        private string PartyNameValue = string.Empty;

        public string PartyName

        {

            get { return this.PartyNameValue; }

            set { SetProperty(ref PartyNameValue, value); }

        }

        /// <summary>
        /// Gets or sets whether this party is the initiator.
        /// </summary>
        private bool IsInitiatorValue;

        public bool IsInitiator

        {

            get { return this.IsInitiatorValue; }

            set { SetProperty(ref IsInitiatorValue, value); }

        }

        /// <summary>
        /// Gets or sets the role (Buyer, Seller, Exchanger).
        /// </summary>
        private string RoleValue = string.Empty;

        public string Role

        {

            get { return this.RoleValue; }

            set { SetProperty(ref RoleValue, value); }

        }
    }

    /// <summary>
    /// Represents exchange terms.
    /// </summary>
    public class ExchangeTerms : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the exchange ratio (if applicable).
        /// </summary>
        private decimal? ExchangeRatioValue;

        public decimal? ExchangeRatio

        {

            get { return this.ExchangeRatioValue; }

            set { SetProperty(ref ExchangeRatioValue, value); }

        }

        /// <summary>
        /// Gets or sets the minimum volume per exchange in barrels.
        /// </summary>
        private decimal? MinimumVolumeValue;

        public decimal? MinimumVolume

        {

            get { return this.MinimumVolumeValue; }

            set { SetProperty(ref MinimumVolumeValue, value); }

        }

        /// <summary>
        /// Gets or sets the maximum volume per exchange in barrels.
        /// </summary>
        private decimal? MaximumVolumeValue;

        public decimal? MaximumVolume

        {

            get { return this.MaximumVolumeValue; }

            set { SetProperty(ref MaximumVolumeValue, value); }

        }

        /// <summary>
        /// Gets or sets the quality specifications.
        /// </summary>
        private string? QualitySpecificationsValue;

        public string? QualitySpecifications

        {

            get { return this.QualitySpecificationsValue; }

            set { SetProperty(ref QualitySpecificationsValue, value); }

        }

        /// <summary>
        /// Gets or sets the notice period in days.
        /// </summary>
        private int NoticePeriodDaysValue = 30;

        public int NoticePeriodDays

        {

            get { return this.NoticePeriodDaysValue; }

            set { SetProperty(ref NoticePeriodDaysValue, value); }

        }
    }

    /// <summary>
    /// Represents an exchange delivery point.
    /// </summary>
    public class ExchangeDeliveryPoint : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the delivery point identifier.
        /// </summary>
        private string DeliveryPointIdValue = string.Empty;

        public string DeliveryPointId

        {

            get { return this.DeliveryPointIdValue; }

            set { SetProperty(ref DeliveryPointIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the delivery point name.
        /// </summary>
        private string DeliveryPointNameValue = string.Empty;

        public string DeliveryPointName

        {

            get { return this.DeliveryPointNameValue; }

            set { SetProperty(ref DeliveryPointNameValue, value); }

        }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        private string LocationValue = string.Empty;

        public string Location

        {

            get { return this.LocationValue; }

            set { SetProperty(ref LocationValue, value); }

        }

        /// <summary>
        /// Gets or sets whether this is a receipt point.
        /// </summary>
        private bool IsReceiptPointValue;

        public bool IsReceiptPoint

        {

            get { return this.IsReceiptPointValue; }

            set { SetProperty(ref IsReceiptPointValue, value); }

        }

        /// <summary>
        /// Gets or sets whether this is a delivery point.
        /// </summary>
        private bool IsDeliveryPointValue;

        public bool IsDeliveryPoint

        {

            get { return this.IsDeliveryPointValue; }

            set { SetProperty(ref IsDeliveryPointValue, value); }

        }
    }

    /// <summary>
    /// Represents exchange pricing terms.
    /// </summary>
    public class ExchangePricingTerms : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the base price index.
        /// </summary>
        private string? BasePriceIndexValue;

        public string? BasePriceIndex

        {

            get { return this.BasePriceIndexValue; }

            set { SetProperty(ref BasePriceIndexValue, value); }

        }

        /// <summary>
        /// Gets or sets the location differential.
        /// </summary>
        private decimal LocationDifferentialValue;

        public decimal LocationDifferential

        {

            get { return this.LocationDifferentialValue; }

            set { SetProperty(ref LocationDifferentialValue, value); }

        }

        /// <summary>
        /// Gets or sets the quality differential.
        /// </summary>
        private decimal QualityDifferentialValue;

        public decimal QualityDifferential

        {

            get { return this.QualityDifferentialValue; }

            set { SetProperty(ref QualityDifferentialValue, value); }

        }

        /// <summary>
        /// Gets or sets the time differential.
        /// </summary>
        private decimal TimeDifferentialValue;

        public decimal TimeDifferential

        {

            get { return this.TimeDifferentialValue; }

            set { SetProperty(ref TimeDifferentialValue, value); }

        }
    }

    /// <summary>
    /// Represents an exchange commitment.
    /// </summary>
    public class ExchangeCommitment : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the commitment identifier.
        /// </summary>
        private string CommitmentIdValue = string.Empty;

        public string CommitmentId

        {

            get { return this.CommitmentIdValue; }

            set { SetProperty(ref CommitmentIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the contract reference.
        /// </summary>
        private string ContractIdValue = string.Empty;

        public string ContractId

        {

            get { return this.ContractIdValue; }

            set { SetProperty(ref ContractIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the commitment type.
        /// </summary>
        private ExchangeCommitmentType CommitmentTypeValue;

        public ExchangeCommitmentType CommitmentType

        {

            get { return this.CommitmentTypeValue; }

            set { SetProperty(ref CommitmentTypeValue, value); }

        }

        /// <summary>
        /// Gets or sets the committed volume in barrels.
        /// </summary>
        private decimal CommittedVolumeValue;

        public decimal CommittedVolume

        {

            get { return this.CommittedVolumeValue; }

            set { SetProperty(ref CommittedVolumeValue, value); }

        }

        /// <summary>
        /// Gets or sets the delivery period start date.
        /// </summary>
        private DateTime DeliveryPeriodStartValue;

        public DateTime DeliveryPeriodStart

        {

            get { return this.DeliveryPeriodStartValue; }

            set { SetProperty(ref DeliveryPeriodStartValue, value); }

        }

        /// <summary>
        /// Gets or sets the delivery period end date.
        /// </summary>
        private DateTime DeliveryPeriodEndValue;

        public DateTime DeliveryPeriodEnd

        {

            get { return this.DeliveryPeriodEndValue; }

            set { SetProperty(ref DeliveryPeriodEndValue, value); }

        }

        /// <summary>
        /// Gets or sets the actual volume delivered in barrels.
        /// </summary>
        private decimal ActualVolumeDeliveredValue;

        public decimal ActualVolumeDelivered

        {

            get { return this.ActualVolumeDeliveredValue; }

            set { SetProperty(ref ActualVolumeDeliveredValue, value); }

        }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        private ExchangeCommitmentStatus StatusValue = ExchangeCommitmentStatus.Pending;

        public ExchangeCommitmentStatus Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }

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
}








