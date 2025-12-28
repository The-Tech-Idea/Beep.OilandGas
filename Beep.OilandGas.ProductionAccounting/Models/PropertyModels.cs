using System;
using System.Collections.Generic;

namespace Beep.OilandGas.ProductionAccounting.Models
{
    /// <summary>
    /// Represents an unproved property (lease, concession, etc.).
    /// </summary>
    public class UnprovedProperty
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
    /// Represents a proved property.
    /// </summary>
    public class ProvedProperty
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
        public ProvedReserves Reserves { get; set; }

        /// <summary>
        /// Gets or sets the proved date.
        /// </summary>
        public DateTime ProvedDate { get; set; }
    }

    /// <summary>
    /// Represents proved oil and gas reserves.
    /// </summary>
    public class ProvedReserves
    {
        /// <summary>
        /// Gets or sets the property identifier.
        /// </summary>
        public string PropertyId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the proved developed oil reserves in barrels.
        /// </summary>
        public decimal ProvedDevelopedOilReserves { get; set; }

        /// <summary>
        /// Gets or sets the proved undeveloped oil reserves in barrels.
        /// </summary>
        public decimal ProvedUndevelopedOilReserves { get; set; }

        /// <summary>
        /// Gets or sets the total proved oil reserves in barrels.
        /// </summary>
        public decimal TotalProvedOilReserves => ProvedDevelopedOilReserves + ProvedUndevelopedOilReserves;

        /// <summary>
        /// Gets or sets the proved developed gas reserves in MCF.
        /// </summary>
        public decimal ProvedDevelopedGasReserves { get; set; }

        /// <summary>
        /// Gets or sets the proved undeveloped gas reserves in MCF.
        /// </summary>
        public decimal ProvedUndevelopedGasReserves { get; set; }

        /// <summary>
        /// Gets or sets the total proved gas reserves in MCF.
        /// </summary>
        public decimal TotalProvedGasReserves => ProvedDevelopedGasReserves + ProvedUndevelopedGasReserves;

        /// <summary>
        /// Gets or sets the reserve date.
        /// </summary>
        public DateTime ReserveDate { get; set; }

        /// <summary>
        /// Gets or sets the oil price used for reserve valuation ($/barrel).
        /// </summary>
        public decimal OilPrice { get; set; }

        /// <summary>
        /// Gets or sets the gas price used for reserve valuation ($/MCF).
        /// </summary>
        public decimal GasPrice { get; set; }
    }

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
}

