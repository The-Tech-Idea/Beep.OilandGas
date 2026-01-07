using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs.ProductionAccounting
{
    /// <summary>
    /// Represents run ticket valuation (DTO for calculations/reporting).
    /// </summary>
    public class RUN_TICKET_VALUATIONDto
    {
        /// <summary>
        /// Gets or sets the valuation identifier.
        /// </summary>
        public string ValuationId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the run ticket reference.
        /// </summary>
        public string RunTicketNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the valuation date.
        /// </summary>
        public DateTime ValuationDate { get; set; }

        /// <summary>
        /// Gets or sets the base price per barrel.
        /// </summary>
        public decimal BasePrice { get; set; }

        /// <summary>
        /// Gets or sets the quality adjustments.
        /// </summary>
        public QualityAdjustmentsDto QualityAdjustments { get; set; } = new();

        /// <summary>
        /// Gets or sets the location adjustments.
        /// </summary>
        public LocationAdjustmentsDto LocationAdjustments { get; set; } = new();

        /// <summary>
        /// Gets or sets the time adjustments.
        /// </summary>
        public TimeAdjustmentsDto TimeAdjustments { get; set; } = new();

        /// <summary>
        /// Gets the total adjustments.
        /// </summary>
        public decimal TotalAdjustments =>
            QualityAdjustments.TotalAdjustment +
            LocationAdjustments.TotalAdjustment +
            TimeAdjustments.TotalAdjustment;

        /// <summary>
        /// Gets the adjusted price per barrel.
        /// </summary>
        public decimal AdjustedPrice => BasePrice + TotalAdjustments;

        /// <summary>
        /// Gets or sets the net volume in barrels.
        /// </summary>
        public decimal NetVolume { get; set; }

        /// <summary>
        /// Gets the total value.
        /// </summary>
        public decimal TotalValue => NetVolume * AdjustedPrice;

        /// <summary>
        /// Gets or sets the pricing method used.
        /// </summary>
        public PricingMethod PricingMethod { get; set; }
    }

    /// <summary>
    /// Represents quality adjustments to pricing.
    /// </summary>
    public class QualityAdjustmentsDto
    {
        /// <summary>
        /// Gets or sets the API gravity adjustment.
        /// </summary>
        public decimal ApiGravityAdjustment { get; set; }

        /// <summary>
        /// Gets or sets the sulfur content adjustment.
        /// </summary>
        public decimal SulfurAdjustment { get; set; }

        /// <summary>
        /// Gets or sets the BS&W adjustment.
        /// </summary>
        public decimal BSWAdjustment { get; set; }

        /// <summary>
        /// Gets or sets other quality adjustments.
        /// </summary>
        public decimal OtherAdjustments { get; set; }

        /// <summary>
        /// Gets the total quality adjustment.
        /// </summary>
        public decimal TotalAdjustment =>
            ApiGravityAdjustment +
            SulfurAdjustment +
            BSWAdjustment +
            OtherAdjustments;
    }

    /// <summary>
    /// Represents location adjustments to pricing.
    /// </summary>
    public class LocationAdjustmentsDto
    {
        /// <summary>
        /// Gets or sets the location differential.
        /// </summary>
        public decimal LocationDifferential { get; set; }

        /// <summary>
        /// Gets or sets the transportation adjustment.
        /// </summary>
        public decimal TransportationAdjustment { get; set; }

        /// <summary>
        /// Gets the total location adjustment.
        /// </summary>
        public decimal TotalAdjustment =>
            LocationDifferential +
            TransportationAdjustment;
    }

    /// <summary>
    /// Represents time adjustments to pricing.
    /// </summary>
    public class TimeAdjustmentsDto
    {
        /// <summary>
        /// Gets or sets the time differential.
        /// </summary>
        public decimal TimeDifferential { get; set; }

        /// <summary>
        /// Gets or sets the interest adjustment (if applicable).
        /// </summary>
        public decimal InterestAdjustment { get; set; }

        /// <summary>
        /// Gets the total time adjustment.
        /// </summary>
        public decimal TotalAdjustment =>
            TimeDifferential +
            InterestAdjustment;
    }

    /// <summary>
    /// Represents a price index (DTO for calculations/reporting).
    /// Note: For database operations, use PRICE_INDEX entity class if it exists.
    /// </summary>
    public class PriceIndexDto
    {
        /// <summary>
        /// Gets or sets the index identifier.
        /// </summary>
        public string IndexId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the index name (e.g., "WTI", "Brent", "LLS").
        /// </summary>
        public string IndexName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the pricing point.
        /// </summary>
        public string PricingPoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the price per barrel.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the unit of measurement.
        /// </summary>
        public string Unit { get; set; } = "USD/Barrel";

        /// <summary>
        /// Gets or sets the source of the price.
        /// </summary>
        public string Source { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents regulated pricing information (DTO for calculations/reporting).
    /// Note: For database operations, use REGULATED_PRICE entity class.
    /// </summary>
    public class RegulatedPriceDto
    {
        /// <summary>
        /// Gets or sets the regulated price identifier.
        /// </summary>
        public string RegulatedPriceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the regulatory authority.
        /// </summary>
        public string RegulatoryAuthority { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the price formula.
        /// </summary>
        public string PriceFormula { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the effective start date.
        /// </summary>
        public DateTime EffectiveStartDate { get; set; }

        /// <summary>
        /// Gets or sets the effective end date.
        /// </summary>
        public DateTime? EffectiveEndDate { get; set; }

        /// <summary>
        /// Gets or sets the price cap per barrel.
        /// </summary>
        public decimal? PriceCap { get; set; }

        /// <summary>
        /// Gets or sets the price floor per barrel.
        /// </summary>
        public decimal? PriceFloor { get; set; }

        /// <summary>
        /// Gets or sets the base price per barrel.
        /// </summary>
        public decimal BasePrice { get; set; }

        /// <summary>
        /// Gets or sets the adjustment factors.
        /// </summary>
        public Dictionary<string, decimal> AdjustmentFactors { get; set; } = new();
    }
}




