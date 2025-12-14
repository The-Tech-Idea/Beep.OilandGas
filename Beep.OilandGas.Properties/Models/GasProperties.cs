using System;

namespace Beep.OilandGas.GasProperties.Models
{
    /// <summary>
    /// Represents gas composition and properties.
    /// </summary>
    public class GasComposition
    {
        /// <summary>
        /// Mole fraction of methane (C1).
        /// </summary>
        public decimal MethaneFraction { get; set; }

        /// <summary>
        /// Mole fraction of ethane (C2).
        /// </summary>
        public decimal EthaneFraction { get; set; }

        /// <summary>
        /// Mole fraction of propane (C3).
        /// </summary>
        public decimal PropaneFraction { get; set; }

        /// <summary>
        /// Mole fraction of i-butane (iC4).
        /// </summary>
        public decimal IButaneFraction { get; set; }

        /// <summary>
        /// Mole fraction of n-butane (nC4).
        /// </summary>
        public decimal NButaneFraction { get; set; }

        /// <summary>
        /// Mole fraction of i-pentane (iC5).
        /// </summary>
        public decimal IPentaneFraction { get; set; }

        /// <summary>
        /// Mole fraction of n-pentane (nC5).
        /// </summary>
        public decimal NPentaneFraction { get; set; }

        /// <summary>
        /// Mole fraction of hexane plus (C6+).
        /// </summary>
        public decimal HexanePlusFraction { get; set; }

        /// <summary>
        /// Mole fraction of nitrogen (N2).
        /// </summary>
        public decimal NitrogenFraction { get; set; }

        /// <summary>
        /// Mole fraction of carbon dioxide (CO2).
        /// </summary>
        public decimal CarbonDioxideFraction { get; set; }

        /// <summary>
        /// Mole fraction of hydrogen sulfide (H2S).
        /// </summary>
        public decimal HydrogenSulfideFraction { get; set; }

        /// <summary>
        /// Validates that all fractions sum to 1.0 (within tolerance).
        /// </summary>
        public bool IsValid(decimal tolerance = 0.01m)
        {
            decimal total = MethaneFraction + EthaneFraction + PropaneFraction +
                           IButaneFraction + NButaneFraction + IPentaneFraction +
                           NPentaneFraction + HexanePlusFraction + NitrogenFraction +
                           CarbonDioxideFraction + HydrogenSulfideFraction;

            return Math.Abs(total - 1.0m) <= tolerance;
        }
    }

    /// <summary>
    /// Represents gas properties at specific conditions.
    /// </summary>
    public class GasProperties
    {
        /// <summary>
        /// Pressure in psia.
        /// </summary>
        public decimal Pressure { get; set; }

        /// <summary>
        /// Temperature in Rankine.
        /// </summary>
        public decimal Temperature { get; set; }

        /// <summary>
        /// Z-factor (compressibility factor).
        /// </summary>
        public decimal ZFactor { get; set; }

        /// <summary>
        /// Gas viscosity in centipoise.
        /// </summary>
        public decimal Viscosity { get; set; }

        /// <summary>
        /// Gas density in lb/ft³.
        /// </summary>
        public decimal Density { get; set; }

        /// <summary>
        /// Specific gravity (relative to air).
        /// </summary>
        public decimal SpecificGravity { get; set; }

        /// <summary>
        /// Molecular weight in lb/lbmol.
        /// </summary>
        public decimal MolecularWeight { get; set; }

        /// <summary>
        /// Pseudo-reduced pressure.
        /// </summary>
        public decimal PseudoReducedPressure { get; set; }

        /// <summary>
        /// Pseudo-reduced temperature.
        /// </summary>
        public decimal PseudoReducedTemperature { get; set; }

        /// <summary>
        /// Pseudo-critical pressure in psia.
        /// </summary>
        public decimal PseudoCriticalPressure { get; set; }

        /// <summary>
        /// Pseudo-critical temperature in Rankine.
        /// </summary>
        public decimal PseudoCriticalTemperature { get; set; }
    }

    /// <summary>
    /// Represents average gas properties over a range.
    /// </summary>
    public class AverageGasProperties
    {
        /// <summary>
        /// Average pressure in psia.
        /// </summary>
        public decimal AveragePressure { get; set; }

        /// <summary>
        /// Average temperature in Rankine.
        /// </summary>
        public decimal AverageTemperature { get; set; }

        /// <summary>
        /// Average Z-factor.
        /// </summary>
        public decimal AverageZFactor { get; set; }

        /// <summary>
        /// Average viscosity in centipoise.
        /// </summary>
        public decimal AverageViscosity { get; set; }
    }

    /// <summary>
    /// Represents pseudo-pressure calculation result.
    /// </summary>
    public class PseudoPressureResult
    {
        /// <summary>
        /// Pressure in psia.
        /// </summary>
        public decimal Pressure { get; set; }

        /// <summary>
        /// Pseudo-pressure in psia²/cp.
        /// </summary>
        public decimal PseudoPressure { get; set; }

        /// <summary>
        /// Z-factor at this pressure.
        /// </summary>
        public decimal ZFactor { get; set; }

        /// <summary>
        /// Viscosity at this pressure in centipoise.
        /// </summary>
        public decimal Viscosity { get; set; }
    }
}

