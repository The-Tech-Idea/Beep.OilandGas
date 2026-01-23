using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// Represents run ticket valuation (DTO for calculations/reporting).
    /// </summary>
    public class RunTicketValuationModel : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the valuation identifier.
        /// </summary>
        private string ValuationIdValue = string.Empty;

        public string ValuationId

        {

            get { return this.ValuationIdValue; }

            set { SetProperty(ref ValuationIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the run ticket reference.
        /// </summary>
        private string RunTicketNumberValue = string.Empty;

        public string RunTicketNumber

        {

            get { return this.RunTicketNumberValue; }

            set { SetProperty(ref RunTicketNumberValue, value); }

        }

        /// <summary>
        /// Gets or sets the valuation date.
        /// </summary>
        private DateTime ValuationDateValue;

        public DateTime ValuationDate

        {

            get { return this.ValuationDateValue; }

            set { SetProperty(ref ValuationDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the base price per barrel.
        /// </summary>
        private decimal BasePriceValue;

        public decimal BasePrice

        {

            get { return this.BasePriceValue; }

            set { SetProperty(ref BasePriceValue, value); }

        }

        /// <summary>
        /// Gets or sets the quality adjustments.
        /// </summary>
        private QualityAdjustments QualityAdjustmentsValue = new();

        public QualityAdjustments QualityAdjustments

        {

            get { return this.QualityAdjustmentsValue; }

            set { SetProperty(ref QualityAdjustmentsValue, value); }

        }

        /// <summary>
        /// Gets or sets the location adjustments.
        /// </summary>
        private LocationAdjustments LocationAdjustmentsValue = new();

        public LocationAdjustments LocationAdjustments

        {

            get { return this.LocationAdjustmentsValue; }

            set { SetProperty(ref LocationAdjustmentsValue, value); }

        }

        /// <summary>
        /// Gets or sets the time adjustments.
        /// </summary>
        private TimeAdjustments TimeAdjustmentsValue = new();

        public TimeAdjustments TimeAdjustments

        {

            get { return this.TimeAdjustmentsValue; }

            set { SetProperty(ref TimeAdjustmentsValue, value); }

        }

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
        private decimal NetVolumeValue;

        public decimal NetVolume

        {

            get { return this.NetVolumeValue; }

            set { SetProperty(ref NetVolumeValue, value); }

        }

        /// <summary>
        /// Gets the total value.
        /// </summary>
        public decimal TotalValue => NetVolume * AdjustedPrice;

        /// <summary>
        /// Gets or sets the pricing method used.
        /// </summary>
        private PricingMethod PricingMethodValue;

        public PricingMethod PricingMethod

        {

            get { return this.PricingMethodValue; }

            set { SetProperty(ref PricingMethodValue, value); }

        }
    }

    /// <summary>
    /// Represents quality adjustments to pricing.
    /// </summary>
    public class QualityAdjustments : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the API gravity adjustment.
        /// </summary>
        private decimal ApiGravityAdjustmentValue;

        public decimal ApiGravityAdjustment

        {

            get { return this.ApiGravityAdjustmentValue; }

            set { SetProperty(ref ApiGravityAdjustmentValue, value); }

        }

        /// <summary>
        /// Gets or sets the sulfur content adjustment.
        /// </summary>
        private decimal SulfurAdjustmentValue;

        public decimal SulfurAdjustment

        {

            get { return this.SulfurAdjustmentValue; }

            set { SetProperty(ref SulfurAdjustmentValue, value); }

        }

        /// <summary>
        /// Gets or sets the BS&W adjustment.
        /// </summary>
        private decimal BSWAdjustmentValue;

        public decimal BSWAdjustment

        {

            get { return this.BSWAdjustmentValue; }

            set { SetProperty(ref BSWAdjustmentValue, value); }

        }

        /// <summary>
        /// Gets or sets other quality adjustments.
        /// </summary>
        private decimal OtherAdjustmentsValue;

        public decimal OtherAdjustments

        {

            get { return this.OtherAdjustmentsValue; }

            set { SetProperty(ref OtherAdjustmentsValue, value); }

        }

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
    public class LocationAdjustments : ModelEntityBase
    {
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
        /// Gets or sets the transportation adjustment.
        /// </summary>
        private decimal TransportationAdjustmentValue;

        public decimal TransportationAdjustment

        {

            get { return this.TransportationAdjustmentValue; }

            set { SetProperty(ref TransportationAdjustmentValue, value); }

        }

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
    public class TimeAdjustments : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the time differential.
        /// </summary>
        private decimal TimeDifferentialValue;

        public decimal TimeDifferential

        {

            get { return this.TimeDifferentialValue; }

            set { SetProperty(ref TimeDifferentialValue, value); }

        }

        /// <summary>
        /// Gets or sets the interest adjustment (if applicable).
        /// </summary>
        private decimal InterestAdjustmentValue;

        public decimal InterestAdjustment

        {

            get { return this.InterestAdjustmentValue; }

            set { SetProperty(ref InterestAdjustmentValue, value); }

        }

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
    public class PriceIndex : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the index identifier.
        /// </summary>
        private string IndexIdValue = string.Empty;

        public string IndexId

        {

            get { return this.IndexIdValue; }

            set { SetProperty(ref IndexIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the index name (e.g., "WTI", "Brent", "LLS").
        /// </summary>
        private string IndexNameValue = string.Empty;

        public string IndexName

        {

            get { return this.IndexNameValue; }

            set { SetProperty(ref IndexNameValue, value); }

        }

        /// <summary>
        /// Gets or sets the pricing point.
        /// </summary>
        private string PricingPointValue = string.Empty;

        public string PricingPoint

        {

            get { return this.PricingPointValue; }

            set { SetProperty(ref PricingPointValue, value); }

        }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        private DateTime DateValue;

        public DateTime Date

        {

            get { return this.DateValue; }

            set { SetProperty(ref DateValue, value); }

        }

        /// <summary>
        /// Gets or sets the price per barrel.
        /// </summary>
        private decimal PriceValue;

        public decimal Price

        {

            get { return this.PriceValue; }

            set { SetProperty(ref PriceValue, value); }

        }

        /// <summary>
        /// Gets or sets the unit of measurement.
        /// </summary>
        private string UnitValue = "USD/Barrel";

        public string Unit

        {

            get { return this.UnitValue; }

            set { SetProperty(ref UnitValue, value); }

        }

        /// <summary>
        /// Gets or sets the source of the price.
        /// </summary>

    }

    /// <summary>
    /// Represents regulated pricing information (DTO for calculations/reporting).
    /// Note: For database operations, use REGULATED_PRICE entity class.
    /// </summary>
    public class RegulatedPrice : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the regulated price identifier.
        /// </summary>
        private string RegulatedPriceIdValue = string.Empty;

        public string RegulatedPriceId

        {

            get { return this.RegulatedPriceIdValue; }

            set { SetProperty(ref RegulatedPriceIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the regulatory authority.
        /// </summary>
        private string RegulatoryAuthorityValue = string.Empty;

        public string RegulatoryAuthority

        {

            get { return this.RegulatoryAuthorityValue; }

            set { SetProperty(ref RegulatoryAuthorityValue, value); }

        }

        /// <summary>
        /// Gets or sets the price formula.
        /// </summary>
        private string PriceFormulaValue = string.Empty;

        public string PriceFormula

        {

            get { return this.PriceFormulaValue; }

            set { SetProperty(ref PriceFormulaValue, value); }

        }

        /// <summary>
        /// Gets or sets the effective start date.
        /// </summary>
        private DateTime EffectiveStartDateValue;

        public DateTime EffectiveStartDate

        {

            get { return this.EffectiveStartDateValue; }

            set { SetProperty(ref EffectiveStartDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the effective end date.
        /// </summary>
        private DateTime? EffectiveEndDateValue;

        public DateTime? EffectiveEndDate

        {

            get { return this.EffectiveEndDateValue; }

            set { SetProperty(ref EffectiveEndDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the price cap per barrel.
        /// </summary>
        private decimal? PriceCapValue;

        public decimal? PriceCap

        {

            get { return this.PriceCapValue; }

            set { SetProperty(ref PriceCapValue, value); }

        }

        /// <summary>
        /// Gets or sets the price floor per barrel.
        /// </summary>
        private decimal? PriceFloorValue;

        public decimal? PriceFloor

        {

            get { return this.PriceFloorValue; }

            set { SetProperty(ref PriceFloorValue, value); }

        }

        /// <summary>
        /// Gets or sets the base price per barrel.
        /// </summary>
        private decimal BasePriceValue;

        public decimal BasePrice

        {

            get { return this.BasePriceValue; }

            set { SetProperty(ref BasePriceValue, value); }

        }

        /// <summary>
        /// Gets or sets the adjustment factors.
        /// </summary>
        public Dictionary<string, decimal> AdjustmentFactors { get; set; } = new();
    }
}


