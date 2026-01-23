using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
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
    /// Base class for lease agreements (DTO for calculations/reporting).
    /// </summary>
    public abstract class LeaseAgreement : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the lease identifier.
        /// </summary>
        private string LeaseIdValue = string.Empty;

        public string LeaseId

        {

            get { return this.LeaseIdValue; }

            set { SetProperty(ref LeaseIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the lease name or description.
        /// </summary>
        private string LeaseNameValue = string.Empty;

        public string LeaseName

        {

            get { return this.LeaseNameValue; }

            set { SetProperty(ref LeaseNameValue, value); }

        }

        /// <summary>
        /// Gets or sets the lease type.
        /// </summary>
        private LeaseType LeaseTypeValue;

        public LeaseType LeaseType

        {

            get { return this.LeaseTypeValue; }

            set { SetProperty(ref LeaseTypeValue, value); }

        }

        /// <summary>
        /// Gets or sets the effective date of the lease.
        /// </summary>
        private DateTime EffectiveDateValue;

        public DateTime EffectiveDate

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the expiration date of the lease.
        /// </summary>
        private DateTime? ExpirationDateValue;

        public DateTime? ExpirationDate

        {

            get { return this.ExpirationDateValue; }

            set { SetProperty(ref ExpirationDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the primary term in months.
        /// </summary>
        private int PrimaryTermMonthsValue;

        public int PrimaryTermMonths

        {

            get { return this.PrimaryTermMonthsValue; }

            set { SetProperty(ref PrimaryTermMonthsValue, value); }

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
        /// Gets or sets the royalty rate (decimal, 0-1).
        /// </summary>
        private decimal RoyaltyRateValue;

        public decimal RoyaltyRate

        {

            get { return this.RoyaltyRateValue; }

            set { SetProperty(ref RoyaltyRateValue, value); }

        }

        /// <summary>
        /// Gets or sets the lease provisions.
        /// </summary>
        private LeaseProvisions ProvisionsValue = new();

        public LeaseProvisions Provisions

        {

            get { return this.ProvisionsValue; }

            set { SetProperty(ref ProvisionsValue, value); }

        }

        /// <summary>
        /// Gets or sets the location information.
        /// </summary>
        private LeaseLocation LocationValue = new();

        public LeaseLocation Location

        {

            get { return this.LocationValue; }

            set { SetProperty(ref LocationValue, value); }

        }
    }

    /// <summary>
    /// Fee (private) mineral estate lease.
    /// </summary>
    public class FeeMineralLease : LeaseAgreement
    {
        /// <summary>
        /// Gets or sets the mineral owner information.
        /// </summary>
        private string MineralOwnerValue = string.Empty;

        public string MineralOwner

        {

            get { return this.MineralOwnerValue; }

            set { SetProperty(ref MineralOwnerValue, value); }

        }

        /// <summary>
        /// Gets or sets the surface owner information.
        /// </summary>
        private string? SurfaceOwnerValue;

        public string? SurfaceOwner

        {

            get { return this.SurfaceOwnerValue; }

            set { SetProperty(ref SurfaceOwnerValue, value); }

        }

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
        private string GovernmentAgencyValue = string.Empty;

        public string GovernmentAgency

        {

            get { return this.GovernmentAgencyValue; }

            set { SetProperty(ref GovernmentAgencyValue, value); }

        }

        /// <summary>
        /// Gets or sets the lease number assigned by the agency.
        /// </summary>
        private string GovernmentLeaseNumberValue = string.Empty;

        public string GovernmentLeaseNumber

        {

            get { return this.GovernmentLeaseNumberValue; }

            set { SetProperty(ref GovernmentLeaseNumberValue, value); }

        }

        /// <summary>
        /// Gets or sets whether this is a federal lease.
        /// </summary>
        private bool IsFederalValue;

        public bool IsFederal

        {

            get { return this.IsFederalValue; }

            set { SetProperty(ref IsFederalValue, value); }

        }

        /// <summary>
        /// Gets or sets whether this is an Indian lease.
        /// </summary>
        private bool IsIndianValue;

        public bool IsIndian

        {

            get { return this.IsIndianValue; }

            set { SetProperty(ref IsIndianValue, value); }

        }

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
        private decimal NetProfitInterestValue;

        public decimal NetProfitInterest

        {

            get { return this.NetProfitInterestValue; }

            set { SetProperty(ref NetProfitInterestValue, value); }

        }

        /// <summary>
        /// Gets or sets the cost recovery provisions.
        /// </summary>
        private NetProfitCostRecovery CostRecoveryValue = new();

        public NetProfitCostRecovery CostRecovery

        {

            get { return this.CostRecoveryValue; }

            set { SetProperty(ref CostRecoveryValue, value); }

        }

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
        private string OperatorValue = string.Empty;

        public string Operator

        {

            get { return this.OperatorValue; }

            set { SetProperty(ref OperatorValue, value); }

        }

        /// <summary>
        /// Gets or sets the non-operator participants.
        /// </summary>
        private List<JointInterestParticipant> ParticipantsValue = new();

        public List<JointInterestParticipant> Participants

        {

            get { return this.ParticipantsValue; }

            set { SetProperty(ref ParticipantsValue, value); }

        }

        /// <summary>
        /// Gets or sets the joint operating agreement reference.
        /// </summary>
        private string JointOperatingAgreementIdValue = string.Empty;

        public string JointOperatingAgreementId

        {

            get { return this.JointOperatingAgreementIdValue; }

            set { SetProperty(ref JointOperatingAgreementIdValue, value); }

        }

        public JointInterestLease()
        {
            LeaseType = LeaseType.JointInterest;
        }
    }

    /// <summary>
    /// Represents lease provisions and terms.
    /// </summary>
    public class LeaseProvisions : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the delay rental amount per acre per year.
        /// </summary>
        private decimal? DelayRentalPerAcreValue;

        public decimal? DelayRentalPerAcre

        {

            get { return this.DelayRentalPerAcreValue; }

            set { SetProperty(ref DelayRentalPerAcreValue, value); }

        }

        /// <summary>
        /// Gets or sets the shut-in royalty amount.
        /// </summary>
        private decimal? ShutInRoyaltyValue;

        public decimal? ShutInRoyalty

        {

            get { return this.ShutInRoyaltyValue; }

            set { SetProperty(ref ShutInRoyaltyValue, value); }

        }

        /// <summary>
        /// Gets or sets the minimum royalty amount.
        /// </summary>
        private decimal? MinimumRoyaltyValue;

        public decimal? MinimumRoyalty

        {

            get { return this.MinimumRoyaltyValue; }

            set { SetProperty(ref MinimumRoyaltyValue, value); }

        }

        /// <summary>
        /// Gets or sets whether pooling is allowed.
        /// </summary>
        private bool AllowPoolingValue;

        public bool AllowPooling

        {

            get { return this.AllowPoolingValue; }

            set { SetProperty(ref AllowPoolingValue, value); }

        }

        /// <summary>
        /// Gets or sets whether unitization is allowed.
        /// </summary>
        private bool AllowUnitizationValue;

        public bool AllowUnitization

        {

            get { return this.AllowUnitizationValue; }

            set { SetProperty(ref AllowUnitizationValue, value); }

        }

        /// <summary>
        /// Gets or sets the force majeure provisions.
        /// </summary>
        private string? ForceMajeureProvisionsValue;

        public string? ForceMajeureProvisions

        {

            get { return this.ForceMajeureProvisionsValue; }

            set { SetProperty(ref ForceMajeureProvisionsValue, value); }

        }

        /// <summary>
        /// Gets or sets the assignment provisions.
        /// </summary>
        private string? AssignmentProvisionsValue;

        public string? AssignmentProvisions

        {

            get { return this.AssignmentProvisionsValue; }

            set { SetProperty(ref AssignmentProvisionsValue, value); }

        }

        /// <summary>
        /// Gets or sets whether the lease is held by production (HBP).
        /// </summary>
        private bool IsHeldByProductionValue;

        public bool IsHeldByProduction

        {

            get { return this.IsHeldByProductionValue; }

            set { SetProperty(ref IsHeldByProductionValue, value); }

        }

        /// <summary>
        /// Gets or sets the date the lease became HBP.
        /// </summary>
        private DateTime? HeldByProductionDateValue;

        public DateTime? HeldByProductionDate

        {

            get { return this.HeldByProductionDateValue; }

            set { SetProperty(ref HeldByProductionDateValue, value); }

        }
    }

    /// <summary>
    /// Represents lease location information.
    /// </summary>
    public class LeaseLocation : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        private string StateValue = string.Empty;

        public string State

        {

            get { return this.StateValue; }

            set { SetProperty(ref StateValue, value); }

        }

        /// <summary>
        /// Gets or sets the county.
        /// </summary>
        private string? CountyValue;

        public string? County

        {

            get { return this.CountyValue; }

            set { SetProperty(ref CountyValue, value); }

        }

        /// <summary>
        /// Gets or sets the township.
        /// </summary>
        private string? TownshipValue;

        public string? Township

        {

            get { return this.TownshipValue; }

            set { SetProperty(ref TownshipValue, value); }

        }

        /// <summary>
        /// Gets or sets the range.
        /// </summary>
        private string? RangeValue;

        public string? Range

        {

            get { return this.RangeValue; }

            set { SetProperty(ref RangeValue, value); }

        }

        /// <summary>
        /// Gets or sets the section.
        /// </summary>
        private string? SectionValue;

        public string? Section

        {

            get { return this.SectionValue; }

            set { SetProperty(ref SectionValue, value); }

        }

        /// <summary>
        /// Gets or sets the number of acres.
        /// </summary>
        private decimal? AcresValue;

        public decimal? Acres

        {

            get { return this.AcresValue; }

            set { SetProperty(ref AcresValue, value); }

        }

        /// <summary>
        /// Gets or sets the API number (if assigned).
        /// </summary>
        private string? ApiNumberValue;

        public string? ApiNumber

        {

            get { return this.ApiNumberValue; }

            set { SetProperty(ref ApiNumberValue, value); }

        }
    }

    /// <summary>
    /// Represents net profit cost recovery provisions.
    /// </summary>
    public class NetProfitCostRecovery : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets whether costs are recoverable.
        /// </summary>
        private bool CostsRecoverableValue;

        public bool CostsRecoverable

        {

            get { return this.CostsRecoverableValue; }

            set { SetProperty(ref CostsRecoverableValue, value); }

        }

        /// <summary>
        /// Gets or sets the recovery percentage (decimal, 0-1).
        /// </summary>
        private decimal RecoveryPercentageValue;

        public decimal RecoveryPercentage

        {

            get { return this.RecoveryPercentageValue; }

            set { SetProperty(ref RecoveryPercentageValue, value); }

        }

        /// <summary>
        /// Gets or sets the types of costs that are recoverable.
        /// </summary>
        private List<string> RecoverableCostTypesValue = new();

        public List<string> RecoverableCostTypes

        {

            get { return this.RecoverableCostTypesValue; }

            set { SetProperty(ref RecoverableCostTypesValue, value); }

        }
    }

    /// <summary>
    /// Represents a joint interest participant.
    /// </summary>
    public class JointInterestParticipant : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the participant company name.
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
    }
}








