using System;
using System.Collections.Generic;
using System.Linq;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// Represents a unit agreement (DTO for calculations/reporting).
    /// </summary>
    public class UnitAgreement : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the unit identifier.
        /// </summary>
        private string UnitIdValue = string.Empty;

        public string UnitId

        {

            get { return this.UnitIdValue; }

            set { SetProperty(ref UnitIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the unit name.
        /// </summary>
        private string UnitNameValue = string.Empty;

        public string UnitName

        {

            get { return this.UnitNameValue; }

            set { SetProperty(ref UnitNameValue, value); }

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
        /// Gets or sets the unit operator.
        /// </summary>
        private string UnitOperatorValue = string.Empty;

        public string UnitOperator

        {

            get { return this.UnitOperatorValue; }

            set { SetProperty(ref UnitOperatorValue, value); }

        }

        /// <summary>
        /// Gets or sets the participating area.
        /// </summary>
        private ParticipatingArea ParticipatingAreaValue = new();

        public ParticipatingArea ParticipatingArea

        {

            get { return this.ParticipatingAreaValue; }

            set { SetProperty(ref ParticipatingAreaValue, value); }

        }

        /// <summary>
        /// Gets or sets the terms and conditions.
        /// </summary>
        private string? TermsAndConditionsValue;

        public string? TermsAndConditions

        {

            get { return this.TermsAndConditionsValue; }

            set { SetProperty(ref TermsAndConditionsValue, value); }

        }
    }

    /// <summary>
    /// Represents a participating area within a unit.
    /// </summary>
    public class ParticipatingArea : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the participating area identifier.
        /// </summary>
        private string ParticipatingAreaIdValue = string.Empty;

        public string ParticipatingAreaId

        {

            get { return this.ParticipatingAreaIdValue; }

            set { SetProperty(ref ParticipatingAreaIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the unit identifier.
        /// </summary>
        private string UnitIdValue = string.Empty;

        public string UnitId

        {

            get { return this.UnitIdValue; }

            set { SetProperty(ref UnitIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the participating area name.
        /// </summary>
        private string ParticipatingAreaNameValue = string.Empty;

        public string ParticipatingAreaName

        {

            get { return this.ParticipatingAreaNameValue; }

            set { SetProperty(ref ParticipatingAreaNameValue, value); }

        }

        /// <summary>
        /// Gets or sets the tracts included in this area.
        /// </summary>
        private List<TractParticipation> TractsValue = new();

        public List<TractParticipation> Tracts

        {

            get { return this.TractsValue; }

            set { SetProperty(ref TractsValue, value); }

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
        /// Gets the total participation percentage.
        /// </summary>
        public decimal TotalParticipation => Tracts.Sum(t => t.ParticipationPercentage);
    }

    /// <summary>
    /// Represents tract participation in a unit.
    /// </summary>
    public class TractParticipation : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the tract identifier.
        /// </summary>
        private string TractIdValue = string.Empty;

        public string TractId

        {

            get { return this.TractIdValue; }

            set { SetProperty(ref TractIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the unit identifier.
        /// </summary>
        private string UnitIdValue = string.Empty;

        public string UnitId

        {

            get { return this.UnitIdValue; }

            set { SetProperty(ref UnitIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the participating area identifier.
        /// </summary>
        private string ParticipatingAreaIdValue = string.Empty;

        public string ParticipatingAreaId

        {

            get { return this.ParticipatingAreaIdValue; }

            set { SetProperty(ref ParticipatingAreaIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the tract participation percentage (0-100).
        /// </summary>
        private decimal ParticipationPercentageValue;

        public decimal ParticipationPercentage

        {

            get { return this.ParticipationPercentageValue; }

            set { SetProperty(ref ParticipationPercentageValue, value); }

        }

        /// <summary>
        /// Gets or sets the working interest (decimal, 0-1).
        /// </summary>
        private decimal WorkingInterestValue;

        public decimal WorkingInterest

        {

            get { return this.WorkingInterestValue; }

            set { SetProperty(ref WorkingInterestValue, value); }

        }

        /// <summary>
        /// Gets or sets the net revenue interest (decimal, 0-1).
        /// </summary>
        private decimal NetRevenueInterestValue;

        public decimal NetRevenueInterest

        {

            get { return this.NetRevenueInterestValue; }

            set { SetProperty(ref NetRevenueInterestValue, value); }

        }

        /// <summary>
        /// Gets or sets the tract acreage.
        /// </summary>
        private decimal? TractAcreageValue;

        public decimal? TractAcreage

        {

            get { return this.TractAcreageValue; }

            set { SetProperty(ref TractAcreageValue, value); }

        }
    }

    /// <summary>
    /// Represents a unit operating agreement.
    /// </summary>
    public class UnitOperatingAgreement : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the operating agreement identifier.
        /// </summary>
        private string OperatingAgreementIdValue = string.Empty;

        public string OperatingAgreementId

        {

            get { return this.OperatingAgreementIdValue; }

            set { SetProperty(ref OperatingAgreementIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the unit identifier.
        /// </summary>
        private string UnitIdValue = string.Empty;

        public string UnitId

        {

            get { return this.UnitIdValue; }

            set { SetProperty(ref UnitIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the participants.
        /// </summary>
        private List<UnitParticipant> ParticipantsValue = new();

        public List<UnitParticipant> Participants

        {

            get { return this.ParticipantsValue; }

            set { SetProperty(ref ParticipantsValue, value); }

        }

        /// <summary>
        /// Gets or sets the voting rights provisions.
        /// </summary>
        private VotingRights VotingRightsValue = new();

        public VotingRights VotingRights

        {

            get { return this.VotingRightsValue; }

            set { SetProperty(ref VotingRightsValue, value); }

        }

        /// <summary>
        /// Gets or sets the cost sharing provisions.
        /// </summary>
        private CostSharing CostSharingValue = new();

        public CostSharing CostSharing

        {

            get { return this.CostSharingValue; }

            set { SetProperty(ref CostSharingValue, value); }

        }

        /// <summary>
        /// Gets or sets the revenue sharing provisions.
        /// </summary>
        private RevenueSharing RevenueSharingValue = new();

        public RevenueSharing RevenueSharing

        {

            get { return this.RevenueSharingValue; }

            set { SetProperty(ref RevenueSharingValue, value); }

        }
    }

    /// <summary>
    /// Represents a unit participant.
    /// </summary>
    public class UnitParticipant : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the participant identifier.
        /// </summary>
        private string ParticipantIdValue = string.Empty;

        public string ParticipantId

        {

            get { return this.ParticipantIdValue; }

            set { SetProperty(ref ParticipantIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the company name.
        /// </summary>
        private string CompanyNameValue = string.Empty;

        public string CompanyName

        {

            get { return this.CompanyNameValue; }

            set { SetProperty(ref CompanyNameValue, value); }

        }

        /// <summary>
        /// Gets or sets the working interest (decimal, 0-1).
        /// </summary>
        private decimal WorkingInterestValue;

        public decimal WorkingInterest

        {

            get { return this.WorkingInterestValue; }

            set { SetProperty(ref WorkingInterestValue, value); }

        }

        /// <summary>
        /// Gets or sets the net revenue interest (decimal, 0-1).
        /// </summary>
        private decimal NetRevenueInterestValue;

        public decimal NetRevenueInterest

        {

            get { return this.NetRevenueInterestValue; }

            set { SetProperty(ref NetRevenueInterestValue, value); }

        }

        /// <summary>
        /// Gets or sets whether this participant is the operator.
        /// </summary>
        private bool IsOperatorValue;

        public bool IsOperator

        {

            get { return this.IsOperatorValue; }

            set { SetProperty(ref IsOperatorValue, value); }

        }

        /// <summary>
        /// Gets or sets the voting percentage (0-100).
        /// </summary>
        private decimal VotingPercentageValue;

        public decimal VotingPercentage

        {

            get { return this.VotingPercentageValue; }

            set { SetProperty(ref VotingPercentageValue, value); }

        }
    }

    /// <summary>
    /// Represents voting rights provisions.
    /// </summary>
    public class VotingRights : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets whether voting is based on working interest.
        /// </summary>
        private bool BasedOnWorkingInterestValue = true;

        public bool BasedOnWorkingInterest

        {

            get { return this.BasedOnWorkingInterestValue; }

            set { SetProperty(ref BasedOnWorkingInterestValue, value); }

        }

        /// <summary>
        /// Gets or sets the minimum voting threshold percentage (0-100).
        /// </summary>
        private decimal MinimumVotingThresholdValue = 50m;

        public decimal MinimumVotingThreshold

        {

            get { return this.MinimumVotingThresholdValue; }

            set { SetProperty(ref MinimumVotingThresholdValue, value); }

        }

        /// <summary>
        /// Gets or sets whether unanimous consent is required for major decisions.
        /// </summary>
        private bool UnanimousConsentRequiredValue = false;

        public bool UnanimousConsentRequired

        {

            get { return this.UnanimousConsentRequiredValue; }

            set { SetProperty(ref UnanimousConsentRequiredValue, value); }

        }
    }

    /// <summary>
    /// Represents cost sharing provisions.
    /// </summary>
    public class CostSharing : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets whether costs are shared based on working interest.
        /// </summary>
        private bool BasedOnWorkingInterestValue = true;

        public bool BasedOnWorkingInterest

        {

            get { return this.BasedOnWorkingInterestValue; }

            set { SetProperty(ref BasedOnWorkingInterestValue, value); }

        }

        /// <summary>
        /// Gets or sets whether costs are shared based on tract participation.
        /// </summary>
        private bool BasedOnTractParticipationValue = false;

        public bool BasedOnTractParticipation

        {

            get { return this.BasedOnTractParticipationValue; }

            set { SetProperty(ref BasedOnTractParticipationValue, value); }

        }

        /// <summary>
        /// Gets or sets the operator's overhead percentage (0-100).
        /// </summary>
        private decimal OperatorOverheadPercentageValue = 0m;

        public decimal OperatorOverheadPercentage

        {

            get { return this.OperatorOverheadPercentageValue; }

            set { SetProperty(ref OperatorOverheadPercentageValue, value); }

        }
    }

    /// <summary>
    /// Represents revenue sharing provisions.
    /// </summary>
    public class RevenueSharing : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets whether revenue is shared based on net revenue interest.
        /// </summary>
        private bool BasedOnNetRevenueInterestValue = true;

        public bool BasedOnNetRevenueInterest

        {

            get { return this.BasedOnNetRevenueInterestValue; }

            set { SetProperty(ref BasedOnNetRevenueInterestValue, value); }

        }

        /// <summary>
        /// Gets or sets whether revenue is shared based on tract participation.
        /// </summary>
        private bool BasedOnTractParticipationValue = false;

        public bool BasedOnTractParticipation

        {

            get { return this.BasedOnTractParticipationValue; }

            set { SetProperty(ref BasedOnTractParticipationValue, value); }

        }
    }
}








