using System;
using System.Collections.Generic;

namespace Beep.OilandGas.ProductionAccounting.Models
{
    /// <summary>
    /// Type of lease agreement.
    /// </summary>
    public enum LeaseType
    {
        /// <summary>
        /// Fee (private) mineral estate lease.
        /// </summary>
        Fee,

        /// <summary>
        /// Government lease (federal, state, or local).
        /// </summary>
        Government,

        /// <summary>
        /// Net profit interest lease.
        /// </summary>
        NetProfit,

        /// <summary>
        /// Joint interest lease (joint operating agreement).
        /// </summary>
        JointInterest
    }

    /// <summary>
    /// Base class for lease agreements.
    /// </summary>
    public abstract class LeaseAgreement
    {
        /// <summary>
        /// Gets or sets the lease identifier.
        /// </summary>
        public string LeaseId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the lease name or description.
        /// </summary>
        public string LeaseName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the lease type.
        /// </summary>
        public LeaseType LeaseType { get; set; }

        /// <summary>
        /// Gets or sets the effective date of the lease.
        /// </summary>
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Gets or sets the expiration date of the lease.
        /// </summary>
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets the primary term in months.
        /// </summary>
        public int PrimaryTermMonths { get; set; }

        /// <summary>
        /// Gets or sets the working interest (decimal, 0-1).
        /// </summary>
        public decimal WorkingInterest { get; set; }

        /// <summary>
        /// Gets or sets the net revenue interest (decimal, 0-1).
        /// </summary>
        public decimal NetRevenueInterest { get; set; }

        /// <summary>
        /// Gets or sets the royalty rate (decimal, 0-1).
        /// </summary>
        public decimal RoyaltyRate { get; set; }

        /// <summary>
        /// Gets or sets the lease provisions.
        /// </summary>
        public LeaseProvisions Provisions { get; set; } = new();

        /// <summary>
        /// Gets or sets the location information.
        /// </summary>
        public LeaseLocation Location { get; set; } = new();
    }

    /// <summary>
    /// Fee (private) mineral estate lease.
    /// </summary>
    public class FeeMineralLease : LeaseAgreement
    {
        /// <summary>
        /// Gets or sets the mineral owner information.
        /// </summary>
        public string MineralOwner { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the surface owner information.
        /// </summary>
        public string? SurfaceOwner { get; set; }

        public FeeMineralLease()
        {
            LeaseType = LeaseType.Fee;
        }
    }

    /// <summary>
    /// Government lease (federal, state, or Indian).
    /// </summary>
    public class GovernmentLease : LeaseAgreement
    {
        /// <summary>
        /// Gets or sets the government agency (BLM, State, etc.).
        /// </summary>
        public string GovernmentAgency { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the lease number assigned by the agency.
        /// </summary>
        public string GovernmentLeaseNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether this is a federal lease.
        /// </summary>
        public bool IsFederal { get; set; }

        /// <summary>
        /// Gets or sets whether this is an Indian lease.
        /// </summary>
        public bool IsIndian { get; set; }

        public GovernmentLease()
        {
            LeaseType = LeaseType.Government;
        }
    }

    /// <summary>
    /// Net profit interest lease.
    /// </summary>
    public class NetProfitLease : LeaseAgreement
    {
        /// <summary>
        /// Gets or sets the net profit interest percentage (decimal, 0-1).
        /// </summary>
        public decimal NetProfitInterest { get; set; }

        /// <summary>
        /// Gets or sets the cost recovery provisions.
        /// </summary>
        public NetProfitCostRecovery CostRecovery { get; set; } = new();

        public NetProfitLease()
        {
            LeaseType = LeaseType.NetProfit;
        }
    }

    /// <summary>
    /// Joint interest lease (joint operating agreement).
    /// </summary>
    public class JointInterestLease : LeaseAgreement
    {
        /// <summary>
        /// Gets or sets the operator company.
        /// </summary>
        public string Operator { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the non-operator participants.
        /// </summary>
        public List<JointInterestParticipant> Participants { get; set; } = new();

        /// <summary>
        /// Gets or sets the joint operating agreement reference.
        /// </summary>
        public string JointOperatingAgreementId { get; set; } = string.Empty;

        public JointInterestLease()
        {
            LeaseType = LeaseType.JointInterest;
        }
    }

    /// <summary>
    /// Represents lease provisions and terms.
    /// </summary>
    public class LeaseProvisions
    {
        /// <summary>
        /// Gets or sets the delay rental amount per acre per year.
        /// </summary>
        public decimal? DelayRentalPerAcre { get; set; }

        /// <summary>
        /// Gets or sets the shut-in royalty amount.
        /// </summary>
        public decimal? ShutInRoyalty { get; set; }

        /// <summary>
        /// Gets or sets the minimum royalty amount.
        /// </summary>
        public decimal? MinimumRoyalty { get; set; }

        /// <summary>
        /// Gets or sets whether pooling is allowed.
        /// </summary>
        public bool AllowPooling { get; set; }

        /// <summary>
        /// Gets or sets whether unitization is allowed.
        /// </summary>
        public bool AllowUnitization { get; set; }

        /// <summary>
        /// Gets or sets the force majeure provisions.
        /// </summary>
        public string? ForceMajeureProvisions { get; set; }

        /// <summary>
        /// Gets or sets the assignment provisions.
        /// </summary>
        public string? AssignmentProvisions { get; set; }

        /// <summary>
        /// Gets or sets whether the lease is held by production (HBP).
        /// </summary>
        public bool IsHeldByProduction { get; set; }

        /// <summary>
        /// Gets or sets the date the lease became HBP.
        /// </summary>
        public DateTime? HeldByProductionDate { get; set; }
    }

    /// <summary>
    /// Represents lease location information.
    /// </summary>
    public class LeaseLocation
    {
        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        public string State { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the county.
        /// </summary>
        public string? County { get; set; }

        /// <summary>
        /// Gets or sets the township.
        /// </summary>
        public string? Township { get; set; }

        /// <summary>
        /// Gets or sets the range.
        /// </summary>
        public string? Range { get; set; }

        /// <summary>
        /// Gets or sets the section.
        /// </summary>
        public string? Section { get; set; }

        /// <summary>
        /// Gets or sets the number of acres.
        /// </summary>
        public decimal? Acres { get; set; }

        /// <summary>
        /// Gets or sets the API number (if assigned).
        /// </summary>
        public string? ApiNumber { get; set; }
    }

    /// <summary>
    /// Represents net profit cost recovery provisions.
    /// </summary>
    public class NetProfitCostRecovery
    {
        /// <summary>
        /// Gets or sets whether costs are recoverable.
        /// </summary>
        public bool CostsRecoverable { get; set; }

        /// <summary>
        /// Gets or sets the recovery percentage (decimal, 0-1).
        /// </summary>
        public decimal RecoveryPercentage { get; set; }

        /// <summary>
        /// Gets or sets the types of costs that are recoverable.
        /// </summary>
        public List<string> RecoverableCostTypes { get; set; } = new();
    }

    /// <summary>
    /// Represents a joint interest participant.
    /// </summary>
    public class JointInterestParticipant
    {
        /// <summary>
        /// Gets or sets the participant company name.
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
    }
}

