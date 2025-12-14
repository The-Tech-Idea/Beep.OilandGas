using System;
using System.Collections.Generic;

namespace Beep.OilandGas.ProductionForecasting.Models
{
    /// <summary>
    /// Represents reservoir properties for production forecasting.
    /// </summary>
    public class ReservoirForecastProperties
    {
        /// <summary>
        /// Initial reservoir pressure in psia.
        /// </summary>
        public decimal InitialPressure { get; set; }

        /// <summary>
        /// Reservoir permeability in md.
        /// </summary>
        public decimal Permeability { get; set; }

        /// <summary>
        /// Reservoir thickness in feet.
        /// </summary>
        public decimal Thickness { get; set; }

        /// <summary>
        /// Drainage radius in feet.
        /// </summary>
        public decimal DrainageRadius { get; set; }

        /// <summary>
        /// Wellbore radius in feet.
        /// </summary>
        public decimal WellboreRadius { get; set; }

        /// <summary>
        /// Formation volume factor in RB/STB.
        /// </summary>
        public decimal FormationVolumeFactor { get; set; }

        /// <summary>
        /// Oil viscosity in cp.
        /// </summary>
        public decimal OilViscosity { get; set; }

        /// <summary>
        /// Total compressibility in 1/psi.
        /// </summary>
        public decimal TotalCompressibility { get; set; }

        /// <summary>
        /// Porosity (fraction).
        /// </summary>
        public decimal Porosity { get; set; }

        /// <summary>
        /// Skin factor.
        /// </summary>
        public decimal SkinFactor { get; set; }

        /// <summary>
        /// Gas specific gravity (relative to air).
        /// </summary>
        public decimal GasSpecificGravity { get; set; }

        /// <summary>
        /// Reservoir temperature in Rankine.
        /// </summary>
        public decimal Temperature { get; set; }
    }

    /// <summary>
    /// Represents a production forecast point.
    /// </summary>
    public class ForecastPoint
    {
        /// <summary>
        /// Time in days from start.
        /// </summary>
        public decimal Time { get; set; }

        /// <summary>
        /// Production rate in bbl/day or Mscf/day.
        /// </summary>
        public decimal ProductionRate { get; set; }

        /// <summary>
        /// Cumulative production in bbl or Mscf.
        /// </summary>
        public decimal CumulativeProduction { get; set; }

        /// <summary>
        /// Reservoir pressure in psia.
        /// </summary>
        public decimal ReservoirPressure { get; set; }

        /// <summary>
        /// Bottom hole pressure in psia.
        /// </summary>
        public decimal BottomHolePressure { get; set; }
    }

    /// <summary>
    /// Represents production forecast results.
    /// </summary>
    public class ProductionForecast
    {
        /// <summary>
        /// Forecast type.
        /// </summary>
        public ForecastType ForecastType { get; set; }

        /// <summary>
        /// Forecast points.
        /// </summary>
        public List<ForecastPoint> ForecastPoints { get; set; } = new();

        /// <summary>
        /// Total forecast duration in days.
        /// </summary>
        public decimal ForecastDuration { get; set; }

        /// <summary>
        /// Initial production rate.
        /// </summary>
        public decimal InitialProductionRate { get; set; }

        /// <summary>
        /// Final production rate.
        /// </summary>
        public decimal FinalProductionRate { get; set; }

        /// <summary>
        /// Total cumulative production.
        /// </summary>
        public decimal TotalCumulativeProduction { get; set; }
    }

    /// <summary>
    /// Forecast type enumeration.
    /// </summary>
    public enum ForecastType
    {
        /// <summary>
        /// Pseudo-steady state single-phase.
        /// </summary>
        PseudoSteadyStateSinglePhase,

        /// <summary>
        /// Pseudo-steady state two-phase.
        /// </summary>
        PseudoSteadyStateTwoPhase,

        /// <summary>
        /// Transient flow.
        /// </summary>
        Transient,

        /// <summary>
        /// Gas well forecast.
        /// </summary>
        GasWell
    }
}

