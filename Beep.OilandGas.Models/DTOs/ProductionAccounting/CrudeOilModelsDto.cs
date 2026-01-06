using System;

namespace Beep.OilandGas.Models.DTOs.ProductionAccounting
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
    public class CrudeOilPropertiesDto
    {
        /// <summary>
        /// Gets or sets the API gravity at 60°F.
        /// </summary>
        public decimal ApiGravity { get; set; }

        /// <summary>
        /// Gets or sets the sulfur content as weight percentage.
        /// </summary>
        public decimal SulfurContent { get; set; }

        /// <summary>
        /// Gets or sets the viscosity in centipoise at standard temperature.
        /// </summary>
        public decimal Viscosity { get; set; }

        /// <summary>
        /// Gets or sets the pour point in degrees Fahrenheit.
        /// </summary>
        public decimal PourPoint { get; set; }

        /// <summary>
        /// Gets or sets the flash point in degrees Fahrenheit.
        /// </summary>
        public decimal FlashPoint { get; set; }

        /// <summary>
        /// Gets or sets the water content as volume percentage.
        /// </summary>
        public decimal WaterContent { get; set; }

        /// <summary>
        /// Gets or sets the BS&W (Basic Sediment and Water) as volume percentage.
        /// </summary>
        public decimal BSW { get; set; }

        /// <summary>
        /// Gets or sets the salt content in pounds per thousand barrels (PTB).
        /// </summary>
        public decimal SaltContent { get; set; }

        /// <summary>
        /// Gets or sets the Reid Vapor Pressure (RVP) in psi.
        /// </summary>
        public decimal? ReidVaporPressure { get; set; }

        /// <summary>
        /// Gets or sets the density in pounds per gallon.
        /// </summary>
        public decimal? Density { get; set; }

        /// <summary>
        /// Gets or sets the temperature at which properties were measured in degrees Fahrenheit.
        /// </summary>
        public decimal MeasurementTemperature { get; set; } = 60m;

        /// <summary>
        /// Gets or sets the date of measurement.
        /// </summary>
        public DateTime MeasurementDate { get; set; } = DateTime.Now;

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
    public class CrudeOilSpecificationsDto
    {
        /// <summary>
        /// Gets or sets the minimum API gravity.
        /// </summary>
        public decimal? MinimumApiGravity { get; set; }

        /// <summary>
        /// Gets or sets the maximum API gravity.
        /// </summary>
        public decimal? MaximumApiGravity { get; set; }

        /// <summary>
        /// Gets or sets the maximum sulfur content (weight %).
        /// </summary>
        public decimal? MaximumSulfurContent { get; set; }

        /// <summary>
        /// Gets or sets the maximum BS&W (volume %).
        /// </summary>
        public decimal? MaximumBSW { get; set; }

        /// <summary>
        /// Gets or sets the maximum water content (volume %).
        /// </summary>
        public decimal? MaximumWaterContent { get; set; }

        /// <summary>
        /// Gets or sets the maximum salt content (PTB).
        /// </summary>
        public decimal? MaximumSaltContent { get; set; }

        /// <summary>
        /// Gets or sets the maximum viscosity (centipoise).
        /// </summary>
        public decimal? MaximumViscosity { get; set; }

        /// <summary>
        /// Validates crude oil properties against specifications.
        /// </summary>
        public bool Validate(CrudeOilPropertiesDto properties)
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
    public class CrudeOilClassificationDto
    {
        /// <summary>
        /// Gets or sets the crude oil type.
        /// </summary>
        public CrudeOilType Type { get; set; }

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
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets standard classifications.
        /// </summary>
        public static CrudeOilClassificationDto[] GetStandardClassifications()
        {
            return new[]
            {
                new CrudeOilClassificationDto
                {
                    Type = CrudeOilType.Condensate,
                    ApiGravityRange = (45m, 100m),
                    Description = "Very light, high API gravity, typically low sulfur"
                },
                new CrudeOilClassificationDto
                {
                    Type = CrudeOilType.Light,
                    ApiGravityRange = (31.1m, 45m),
                    Description = "Light crude, easy to refine, typically sweet"
                },
                new CrudeOilClassificationDto
                {
                    Type = CrudeOilType.Medium,
                    ApiGravityRange = (22.3m, 31.1m),
                    Description = "Medium crude, moderate refining complexity"
                },
                new CrudeOilClassificationDto
                {
                    Type = CrudeOilType.Heavy,
                    ApiGravityRange = (10m, 22.3m),
                    Description = "Heavy crude, requires specialized refining"
                },
                new CrudeOilClassificationDto
                {
                    Type = CrudeOilType.ExtraHeavy,
                    ApiGravityRange = (0m, 10m),
                    Description = "Extra heavy crude, bitumen-like, very difficult to refine"
                }
            };
        }
    }
}




