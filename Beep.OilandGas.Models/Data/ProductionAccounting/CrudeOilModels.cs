using System;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// Classification of crude oil by API gravity.
    /// </summary>
    public enum CrudeOilType
    {
        /// <summary>
        /// Light crude oil (API gravity > 31.1°)
        /// </summary>
        Light,

        /// <summary>
        /// Medium crude oil (API gravity 22.3° - 31.1°)
        /// </summary>
        Medium,

        /// <summary>
        /// Heavy crude oil (API gravity 10° - 22.3°)
        /// </summary>
        Heavy,

        /// <summary>
        /// Extra heavy crude oil (API gravity < 10°)
        /// </summary>
        ExtraHeavy,

        /// <summary>
        /// Condensate (very light, API gravity > 45°)
        /// </summary>
        Condensate
    }

    /// <summary>
    /// Represents physical and chemical properties of crude oil (DTO for calculations/reporting).
    /// </summary>
    public class CrudeOilProperties : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the API gravity at 60°F.
        /// </summary>
        private decimal ApiGravityValue;

        public decimal ApiGravity

        {

            get { return this.ApiGravityValue; }

            set { SetProperty(ref ApiGravityValue, value); }

        }

        /// <summary>
        /// Gets or sets the sulfur content as weight percentage.
        /// </summary>
        private decimal SulfurContentValue;

        public decimal SulfurContent

        {

            get { return this.SulfurContentValue; }

            set { SetProperty(ref SulfurContentValue, value); }

        }

        /// <summary>
        /// Gets or sets the viscosity in centipoise at standard temperature.
        /// </summary>
        private decimal ViscosityValue;

        public decimal Viscosity

        {

            get { return this.ViscosityValue; }

            set { SetProperty(ref ViscosityValue, value); }

        }

        /// <summary>
        /// Gets or sets the pour point in degrees Fahrenheit.
        /// </summary>
        private decimal PourPointValue;

        public decimal PourPoint

        {

            get { return this.PourPointValue; }

            set { SetProperty(ref PourPointValue, value); }

        }

        /// <summary>
        /// Gets or sets the flash point in degrees Fahrenheit.
        /// </summary>
        private decimal FlashPointValue;

        public decimal FlashPoint

        {

            get { return this.FlashPointValue; }

            set { SetProperty(ref FlashPointValue, value); }

        }

        /// <summary>
        /// Gets or sets the water content as volume percentage.
        /// </summary>
        private decimal WaterContentValue;

        public decimal WaterContent

        {

            get { return this.WaterContentValue; }

            set { SetProperty(ref WaterContentValue, value); }

        }

        /// <summary>
        /// Gets or sets the BS&W (Basic Sediment and Water) as volume percentage.
        /// </summary>
        private decimal BSWValue;

        public decimal BSW

        {

            get { return this.BSWValue; }

            set { SetProperty(ref BSWValue, value); }

        }

        /// <summary>
        /// Gets or sets the salt content in pounds per thousand barrels (PTB).
        /// </summary>
        private decimal SaltContentValue;

        public decimal SaltContent

        {

            get { return this.SaltContentValue; }

            set { SetProperty(ref SaltContentValue, value); }

        }

        /// <summary>
        /// Gets or sets the Reid Vapor Pressure (RVP) in psi.
        /// </summary>
        private decimal? ReidVaporPressureValue;

        public decimal? ReidVaporPressure

        {

            get { return this.ReidVaporPressureValue; }

            set { SetProperty(ref ReidVaporPressureValue, value); }

        }

        /// <summary>
        /// Gets or sets the density in pounds per gallon.
        /// </summary>
        private decimal? DensityValue;

        public decimal? Density

        {

            get { return this.DensityValue; }

            set { SetProperty(ref DensityValue, value); }

        }

        /// <summary>
        /// Gets or sets the temperature at which properties were measured in degrees Fahrenheit.
        /// </summary>
        private decimal MeasurementTemperatureValue = 60m;

        public decimal MeasurementTemperature

        {

            get { return this.MeasurementTemperatureValue; }

            set { SetProperty(ref MeasurementTemperatureValue, value); }

        }

        /// <summary>
        /// Gets or sets the date of measurement.
        /// </summary>
        private DateTime MeasurementDateValue = DateTime.Now;

        public DateTime MeasurementDate

        {

            get { return this.MeasurementDateValue; }

            set { SetProperty(ref MeasurementDateValue, value); }

        }

        /// <summary>
        /// Gets the crude oil type based on API gravity.
        /// </summary>
        public CrudeOilType GetCrudeOilType()
        {
            if (ApiGravity >= 45m)
                return CrudeOilType.Condensate;
            if (ApiGravity > 31.1m)
                return CrudeOilType.Light;
            if (ApiGravity >= 22.3m)
                return CrudeOilType.Medium;
            if (ApiGravity >= 10m)
                return CrudeOilType.Heavy;
            return CrudeOilType.ExtraHeavy;
        }
    }

    /// <summary>
    /// Represents quality specifications for crude oil.
    /// </summary>
    public class CrudeOilSpecifications : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the minimum API gravity.
        /// </summary>
        private decimal? MinimumApiGravityValue;

        public decimal? MinimumApiGravity

        {

            get { return this.MinimumApiGravityValue; }

            set { SetProperty(ref MinimumApiGravityValue, value); }

        }

        /// <summary>
        /// Gets or sets the maximum API gravity.
        /// </summary>
        private decimal? MaximumApiGravityValue;

        public decimal? MaximumApiGravity

        {

            get { return this.MaximumApiGravityValue; }

            set { SetProperty(ref MaximumApiGravityValue, value); }

        }

        /// <summary>
        /// Gets or sets the maximum sulfur content (weight %).
        /// </summary>
        private decimal? MaximumSulfurContentValue;

        public decimal? MaximumSulfurContent

        {

            get { return this.MaximumSulfurContentValue; }

            set { SetProperty(ref MaximumSulfurContentValue, value); }

        }

        /// <summary>
        /// Gets or sets the maximum BS&W (volume %).
        /// </summary>
        private decimal? MaximumBSWValue;

        public decimal? MaximumBSW

        {

            get { return this.MaximumBSWValue; }

            set { SetProperty(ref MaximumBSWValue, value); }

        }

        /// <summary>
        /// Gets or sets the maximum water content (volume %).
        /// </summary>
        private decimal? MaximumWaterContentValue;

        public decimal? MaximumWaterContent

        {

            get { return this.MaximumWaterContentValue; }

            set { SetProperty(ref MaximumWaterContentValue, value); }

        }

        /// <summary>
        /// Gets or sets the maximum salt content (PTB).
        /// </summary>
        private decimal? MaximumSaltContentValue;

        public decimal? MaximumSaltContent

        {

            get { return this.MaximumSaltContentValue; }

            set { SetProperty(ref MaximumSaltContentValue, value); }

        }

        /// <summary>
        /// Gets or sets the maximum viscosity (centipoise).
        /// </summary>
        private decimal? MaximumViscosityValue;

        public decimal? MaximumViscosity

        {

            get { return this.MaximumViscosityValue; }

            set { SetProperty(ref MaximumViscosityValue, value); }

        }

        /// <summary>
        /// Validates crude oil properties against specifications.
        /// </summary>
        public bool Validate(CrudeOilProperties properties)
        {
            if (MinimumApiGravity.HasValue && properties.ApiGravity < MinimumApiGravity.Value)
                return false;

            if (MaximumApiGravity.HasValue && properties.ApiGravity > MaximumApiGravity.Value)
                return false;

            if (MaximumSulfurContent.HasValue && properties.SulfurContent > MaximumSulfurContent.Value)
                return false;

            if (MaximumBSW.HasValue && properties.BSW > MaximumBSW.Value)
                return false;

            if (MaximumWaterContent.HasValue && properties.WaterContent > MaximumWaterContent.Value)
                return false;

            if (MaximumSaltContent.HasValue && properties.SaltContent > MaximumSaltContent.Value)
                return false;

            if (MaximumViscosity.HasValue && properties.Viscosity > MaximumViscosity.Value)
                return false;

            return true;
        }
    }

    /// <summary>
    /// Represents a classification of crude oil.
    /// </summary>
    public class CrudeOilClassification : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the crude oil type.
        /// </summary>
        private CrudeOilType TypeValue;

        public CrudeOilType Type

        {

            get { return this.TypeValue; }

            set { SetProperty(ref TypeValue, value); }

        }

        /// <summary>
        /// Gets or sets the API gravity range.
        /// </summary>
        public (decimal Min, decimal Max) ApiGravityRange { get; set; }

        /// <summary>
        /// Gets or sets the typical sulfur content range.
        /// </summary>
        public (decimal Min, decimal Max)? SulfurContentRange { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        private string DescriptionValue = string.Empty;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }

        /// <summary>
        /// Gets standard classifications.
        /// </summary>
        public static CrudeOilClassification[] GetStandardClassifications()
        {
            return new[]
            {
                new CrudeOilClassification
                {
                    Type = CrudeOilType.Condensate,
                    ApiGravityRange = (45m, 100m),
                    Description = "Very light, high API gravity, typically low sulfur"
                },
                new CrudeOilClassification
                {
                    Type = CrudeOilType.Light,
                    ApiGravityRange = (31.1m, 45m),
                    Description = "Light crude, easy to refine, typically sweet"
                },
                new CrudeOilClassification
                {
                    Type = CrudeOilType.Medium,
                    ApiGravityRange = (22.3m, 31.1m),
                    Description = "Medium crude, moderate refining complexity"
                },
                new CrudeOilClassification
                {
                    Type = CrudeOilType.Heavy,
                    ApiGravityRange = (10m, 22.3m),
                    Description = "Heavy crude, requires specialized refining"
                },
                new CrudeOilClassification
                {
                    Type = CrudeOilType.ExtraHeavy,
                    ApiGravityRange = (0m, 10m),
                    Description = "Extra heavy crude, bitumen-like, very difficult to refine"
                }
            };
        }
    }
}








