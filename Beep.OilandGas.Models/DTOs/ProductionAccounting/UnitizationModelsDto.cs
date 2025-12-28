using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.Models.DTOs.ProductionAccounting
{
    /// <summary>
    /// Represents a unit agreement (DTO for calculations/reporting).
    /// </summary>
    public class UnitAgreementDto
    {
        /// <summary>
        /// Gets or sets the unit identifier.
        /// </summary>
        public string UnitId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the unit name.
        /// </summary>
        public string UnitName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the effective date.
        /// </summary>
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Gets or sets the expiration date.
        /// </summary>
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets the unit operator.
        /// </summary>
        public string UnitOperator { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the participating area.
        /// </summary>
        public ParticipatingAreaDto ParticipatingArea { get; set; } = new();

        /// <summary>
        /// Gets or sets the terms and conditions.
        /// </summary>
        public string? TermsAndConditions { get; set; }
    }

    /// <summary>
    /// Represents a participating area within a unit.
    /// </summary>
    public class ParticipatingAreaDto
    {
        /// <summary>
        /// Gets or sets the participating area identifier.
        /// </summary>
        public string ParticipatingAreaId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the unit identifier.
        /// </summary>
        public string UnitId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the participating area name.
        /// </summary>
        public string ParticipatingAreaName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the tracts included in this area.
        /// </summary>
        public List<TractParticipationDto> Tracts { get; set; } = new();

        /// <summary>
        /// Gets or sets the effective date.
        /// </summary>
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Gets or sets the expiration date.
        /// </summary>
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// Gets the total participation percentage.
        /// </summary>
        public decimal TotalParticipation => Tracts.Sum(t => t.ParticipationPercentage);
    }

    /// <summary>
    /// Represents tract participation in a unit.
    /// </summary>
    public class TractParticipationDto
    {
        /// <summary>
        /// Gets or sets the tract identifier.
        /// </summary>
        public string TractId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the unit identifier.
        /// </summary>
        public string UnitId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the participating area identifier.
        /// </summary>
        public string ParticipatingAreaId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the tract participation percentage (0-100).
        /// </summary>
        public decimal ParticipationPercentage { get; set; }

        /// <summary>
        /// Gets or sets the working interest (decimal, 0-1).
        /// </summary>
        public decimal WorkingInterest { get; set; }

        /// <summary>
        /// Gets or sets the net revenue interest (decimal, 0-1).
        /// </summary>
        public decimal NetRevenueInterest { get; set; }

        /// <summary>
        /// Gets or sets the tract acreage.
        /// </summary>
        public decimal? TractAcreage { get; set; }
    }

    /// <summary>
    /// Represents a unit operating agreement.
    /// </summary>
    public class UnitOperatingAgreementDto
    {
        /// <summary>
        /// Gets or sets the operating agreement identifier.
        /// </summary>
        public string OperatingAgreementId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the unit identifier.
        /// </summary>
        public string UnitId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the participants.
        /// </summary>
        public List<UnitParticipantDto> Participants { get; set; } = new();

        /// <summary>
        /// Gets or sets the voting rights provisions.
        /// </summary>
        public VotingRightsDto VotingRights { get; set; } = new();

        /// <summary>
        /// Gets or sets the cost sharing provisions.
        /// </summary>
        public CostSharingDto CostSharing { get; set; } = new();

        /// <summary>
        /// Gets or sets the revenue sharing provisions.
        /// </summary>
        public RevenueSharingDto RevenueSharing { get; set; } = new();
    }

    /// <summary>
    /// Represents a unit participant.
    /// </summary>
    public class UnitParticipantDto
    {
        /// <summary>
        /// Gets or sets the participant identifier.
        /// </summary>
        public string ParticipantId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the company name.
        /// </summary>
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the working interest (decimal, 0-1).
        /// </summary>
        public decimal WorkingInterest { get; set; }

        /// <summary>
        /// Gets or sets the net revenue interest (decimal, 0-1).
        /// </summary>
        public decimal NetRevenueInterest { get; set; }

        /// <summary>
        /// Gets or sets whether this participant is the operator.
        /// </summary>
        public bool IsOperator { get; set; }

        /// <summary>
        /// Gets or sets the voting percentage (0-100).
        /// </summary>
        public decimal VotingPercentage { get; set; }
    }

    /// <summary>
    /// Represents voting rights provisions.
    /// </summary>
    public class VotingRightsDto
    {
        /// <summary>
        /// Gets or sets whether voting is based on working interest.
        /// </summary>
        public bool BasedOnWorkingInterest { get; set; } = true;

        /// <summary>
        /// Gets or sets the minimum voting threshold percentage (0-100).
        /// </summary>
        public decimal MinimumVotingThreshold { get; set; } = 50m;

        /// <summary>
        /// Gets or sets whether unanimous consent is required for major decisions.
        /// </summary>
        public bool UnanimousConsentRequired { get; set; } = false;
    }

    /// <summary>
    /// Represents cost sharing provisions.
    /// </summary>
    public class CostSharingDto
    {
        /// <summary>
        /// Gets or sets whether costs are shared based on working interest.
        /// </summary>
        public bool BasedOnWorkingInterest { get; set; } = true;

        /// <summary>
        /// Gets or sets whether costs are shared based on tract participation.
        /// </summary>
        public bool BasedOnTractParticipation { get; set; } = false;

        /// <summary>
        /// Gets or sets the operator's overhead percentage (0-100).
        /// </summary>
        public decimal OperatorOverheadPercentage { get; set; } = 0m;
    }

    /// <summary>
    /// Represents revenue sharing provisions.
    /// </summary>
    public class RevenueSharingDto
    {
        /// <summary>
        /// Gets or sets whether revenue is shared based on net revenue interest.
        /// </summary>
        public bool BasedOnNetRevenueInterest { get; set; } = true;

        /// <summary>
        /// Gets or sets whether revenue is shared based on tract participation.
        /// </summary>
        public bool BasedOnTractParticipation { get; set; } = false;
    }
}

