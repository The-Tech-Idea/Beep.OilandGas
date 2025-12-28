using System;

namespace Beep.OilandGas.Models.DTOs.ProductionAccounting
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
    public class UnprovedPropertyDto
    {
        /// <summary>
        /// Gets or sets the property identifier.
        /// </summary>
        public string PropertyId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the property name or description.
        /// </summary>
        public string PropertyName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the acquisition cost.
        /// </summary>
        public decimal AcquisitionCost { get; set; }

        /// <summary>
        /// Gets or sets the acquisition date.
        /// </summary>
        public DateTime AcquisitionDate { get; set; }

        /// <summary>
        /// Gets or sets the property type (lease, concession, fee interest, etc.).
        /// </summary>
        public PropertyType PropertyType { get; set; } = PropertyType.Lease;

        /// <summary>
        /// Gets or sets the working interest percentage.
        /// </summary>
        public decimal WorkingInterest { get; set; } = 1.0m;

        /// <summary>
        /// Gets or sets the net revenue interest percentage.
        /// </summary>
        public decimal NetRevenueInterest { get; set; } = 1.0m;

        /// <summary>
        /// Gets or sets the accumulated impairment.
        /// </summary>
        public decimal AccumulatedImpairment { get; set; }

        /// <summary>
        /// Gets or sets whether the property has been classified as proved.
        /// </summary>
        public bool IsProved { get; set; }

        /// <summary>
        /// Gets or sets the date when property was classified as proved.
        /// </summary>
        public DateTime? ProvedDate { get; set; }
    }

    /// <summary>
    /// Represents a proved property (DTO for calculations/reporting).
    /// Note: For database operations, use PROVED_PROPERTY entity class.
    /// </summary>
    public class ProvedPropertyDto
    {
        /// <summary>
        /// Gets or sets the property identifier.
        /// </summary>
        public string PropertyId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the acquisition cost.
        /// </summary>
        public decimal AcquisitionCost { get; set; }

        /// <summary>
        /// Gets or sets the exploration costs capitalized.
        /// </summary>
        public decimal ExplorationCosts { get; set; }

        /// <summary>
        /// Gets or sets the development costs capitalized.
        /// </summary>
        public decimal DevelopmentCosts { get; set; }

        /// <summary>
        /// Gets or sets the accumulated amortization.
        /// </summary>
        public decimal AccumulatedAmortization { get; set; }

        /// <summary>
        /// Gets or sets the proved reserves.
        /// </summary>
        public ProvedReservesDto? Reserves { get; set; }

        /// <summary>
        /// Gets or sets the proved date.
        /// </summary>
        public DateTime ProvedDate { get; set; }
    }
}

