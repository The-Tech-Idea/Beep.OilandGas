namespace Beep.OilandGas.Models.ProductionForecasting
{
    /// <summary>
    /// Represents reservoir properties for production forecasting
    /// DTO for calculations - Entity class: RESERVOIR_FORECAST_PROPERTIES
    /// </summary>
    public class ReservoirForecastProperties
    {
        /// <summary>
        /// Initial reservoir pressure in psia
        /// </summary>
        public decimal InitialPressure { get; set; }

        /// <summary>
        /// Reservoir permeability in md
        /// </summary>
        public decimal Permeability { get; set; }

        /// <summary>
        /// Reservoir thickness in feet
        /// </summary>
        public decimal Thickness { get; set; }

        /// <summary>
        /// Drainage radius in feet
        /// </summary>
        public decimal DrainageRadius { get; set; }

        /// <summary>
        /// Wellbore radius in feet
        /// </summary>
        public decimal WellboreRadius { get; set; }

        /// <summary>
        /// Formation volume factor in RB/STB
        /// </summary>
        public decimal FormationVolumeFactor { get; set; }

        /// <summary>
        /// Oil viscosity in cp
        /// </summary>
        public decimal OilViscosity { get; set; }

        /// <summary>
        /// Total compressibility in 1/psi
        /// </summary>
        public decimal TotalCompressibility { get; set; }

        /// <summary>
        /// Porosity (fraction)
        /// </summary>
        public decimal Porosity { get; set; }

        /// <summary>
        /// Skin factor
        /// </summary>
        public decimal SkinFactor { get; set; }

        /// <summary>
        /// Gas specific gravity (relative to air)
        /// </summary>
        public decimal GasSpecificGravity { get; set; }

        /// <summary>
        /// Reservoir temperature in Rankine
        /// </summary>
        public decimal Temperature { get; set; }
    }
}
