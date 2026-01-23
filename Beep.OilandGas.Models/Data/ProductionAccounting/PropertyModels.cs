using System;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// Property type enumeration.
    /// </summary>
    public enum PropertyType
    {
        /// <summary>
        /// Oil and gas lease.
        /// </summary>
        Lease,

        /// <summary>
        /// Concession.
        /// </summary>
        Concession,

        /// <summary>
        /// Fee interest.
        /// </summary>
        FeeInterest,

        /// <summary>
        /// Royalty interest.
        /// </summary>
        RoyaltyInterest,

        /// <summary>
        /// Production payment.
        /// </summary>
        ProductionPayment
    }

    /// <summary>
    /// Represents an unproved property (DTO for calculations/reporting).
    /// Note: For database operations, use UNPROVED_PROPERTY entity class.
    /// </summary>
    public class UnprovedProperty : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the property identifier.
        /// </summary>
        private string PropertyIdValue = string.Empty;

        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the property name or description.
        /// </summary>
        private string PropertyNameValue = string.Empty;

        public string PropertyName

        {

            get { return this.PropertyNameValue; }

            set { SetProperty(ref PropertyNameValue, value); }

        }

        /// <summary>
        /// Gets or sets the acquisition cost.
        /// </summary>
        private decimal AcquisitionCostValue;

        public decimal AcquisitionCost

        {

            get { return this.AcquisitionCostValue; }

            set { SetProperty(ref AcquisitionCostValue, value); }

        }

        /// <summary>
        /// Gets or sets the acquisition date.
        /// </summary>
        private DateTime AcquisitionDateValue;

        public DateTime AcquisitionDate

        {

            get { return this.AcquisitionDateValue; }

            set { SetProperty(ref AcquisitionDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the property type (lease, concession, fee interest, etc.).
        /// </summary>
        private PropertyType PropertyTypeValue = PropertyType.Lease;

        public PropertyType PropertyType

        {

            get { return this.PropertyTypeValue; }

            set { SetProperty(ref PropertyTypeValue, value); }

        }

        /// <summary>
        /// Gets or sets the working interest percentage.
        /// </summary>
        private decimal WorkingInterestValue = 1.0m;

        public decimal WorkingInterest

        {

            get { return this.WorkingInterestValue; }

            set { SetProperty(ref WorkingInterestValue, value); }

        }

        /// <summary>
        /// Gets or sets the net revenue interest percentage.
        /// </summary>
        private decimal NetRevenueInterestValue = 1.0m;

        public decimal NetRevenueInterest

        {

            get { return this.NetRevenueInterestValue; }

            set { SetProperty(ref NetRevenueInterestValue, value); }

        }

        /// <summary>
        /// Gets or sets the accumulated impairment.
        /// </summary>
        private decimal AccumulatedImpairmentValue;

        public decimal AccumulatedImpairment

        {

            get { return this.AccumulatedImpairmentValue; }

            set { SetProperty(ref AccumulatedImpairmentValue, value); }

        }

        /// <summary>
        /// Gets or sets whether the property has been classified as proved.
        /// </summary>
        private bool IsProvedValue;

        public bool IsProved

        {

            get { return this.IsProvedValue; }

            set { SetProperty(ref IsProvedValue, value); }

        }

        /// <summary>
        /// Gets or sets the date when property was classified as proved.
        /// </summary>
        private DateTime? ProvedDateValue;

        public DateTime? ProvedDate

        {

            get { return this.ProvedDateValue; }

            set { SetProperty(ref ProvedDateValue, value); }

        }
    }

    /// <summary>
    /// Represents a proved property (DTO for calculations/reporting).
    /// Note: For database operations, use PROVED_PROPERTY entity class.
    /// </summary>
    public class ProvedProperty : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the property identifier.
        /// </summary>
        private string PropertyIdValue = string.Empty;

        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the acquisition cost.
        /// </summary>
        private decimal AcquisitionCostValue;

        public decimal AcquisitionCost

        {

            get { return this.AcquisitionCostValue; }

            set { SetProperty(ref AcquisitionCostValue, value); }

        }

        /// <summary>
        /// Gets or sets the exploration costs capitalized.
        /// </summary>
        private decimal ExplorationCostsValue;

        public decimal ExplorationCosts

        {

            get { return this.ExplorationCostsValue; }

            set { SetProperty(ref ExplorationCostsValue, value); }

        }

        /// <summary>
        /// Gets or sets the development costs capitalized.
        /// </summary>
        private decimal DevelopmentCostsValue;

        public decimal DevelopmentCosts

        {

            get { return this.DevelopmentCostsValue; }

            set { SetProperty(ref DevelopmentCostsValue, value); }

        }

        /// <summary>
        /// Gets or sets the accumulated amortization.
        /// </summary>
        private decimal AccumulatedAmortizationValue;

        public decimal AccumulatedAmortization

        {

            get { return this.AccumulatedAmortizationValue; }

            set { SetProperty(ref AccumulatedAmortizationValue, value); }

        }

        /// <summary>
        /// Gets or sets the proved reserves.
        /// </summary>
        private ProvedReserves? ReservesValue;

        public ProvedReserves? Reserves

        {

            get { return this.ReservesValue; }

            set { SetProperty(ref ReservesValue, value); }

        }

        /// <summary>
        /// Gets or sets the proved date.
        /// </summary>
        private DateTime ProvedDateValue;

        public DateTime ProvedDate

        {

            get { return this.ProvedDateValue; }

            set { SetProperty(ref ProvedDateValue, value); }

        }
    }

    /// <summary>
    /// Result of impairment testing for an unproved property.
    /// </summary>
    public class ImpairmentResult : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the property identifier.
        /// </summary>
        private string PropertyIdValue = string.Empty;

        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the property name.
        /// </summary>
        private string PropertyNameValue = string.Empty;

        public string PropertyName

        {

            get { return this.PropertyNameValue; }

            set { SetProperty(ref PropertyNameValue, value); }

        }

        /// <summary>
        /// Gets or sets the acquisition cost.
        /// </summary>
        private decimal AcquisitionCostValue;

        public decimal AcquisitionCost

        {

            get { return this.AcquisitionCostValue; }

            set { SetProperty(ref AcquisitionCostValue, value); }

        }

        /// <summary>
        /// Gets or sets the current accumulated impairment.
        /// </summary>
        private decimal AccumulatedImpairmentValue;

        public decimal AccumulatedImpairment

        {

            get { return this.AccumulatedImpairmentValue; }

            set { SetProperty(ref AccumulatedImpairmentValue, value); }

        }

        /// <summary>
        /// Gets or sets the calculated impairment amount.
        /// </summary>
        private decimal CalculatedImpairmentValue;

        public decimal CalculatedImpairment

        {

            get { return this.CalculatedImpairmentValue; }

            set { SetProperty(ref CalculatedImpairmentValue, value); }

        }

        /// <summary>
        /// Gets or sets whether impairment is required.
        /// </summary>
        private bool ImpairmentRequiredValue;

        public bool ImpairmentRequired

        {

            get { return this.ImpairmentRequiredValue; }

            set { SetProperty(ref ImpairmentRequiredValue, value); }

        }

        /// <summary>
        /// Gets or sets the impairment reason.
        /// </summary>
        private string ImpairmentReasonValue = string.Empty;

        public string ImpairmentReason

        {

            get { return this.ImpairmentReasonValue; }

            set { SetProperty(ref ImpairmentReasonValue, value); }

        }

        /// <summary>
        /// Gets or sets the test date.
        /// </summary>
        private DateTime TestDateValue = DateTime.UtcNow;

        public DateTime TestDate

        {

            get { return this.TestDateValue; }

            set { SetProperty(ref TestDateValue, value); }

        }
    }
}








