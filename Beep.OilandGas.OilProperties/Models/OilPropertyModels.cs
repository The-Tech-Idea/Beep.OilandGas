using System;

namespace Beep.OilandGas.OilProperties.Models
{
    /// <summary>
    /// Represents oil properties at given conditions.
    /// </summary>
    public class OilPropertyResult
    {
        /// <summary>
        /// Oil density in lb/ft³.
        /// </summary>
        public decimal Density { get; set; }

        /// <summary>
        /// Oil specific gravity.
        /// </summary>
        public decimal SpecificGravity { get; set; }

        /// <summary>
        /// Oil API gravity.
        /// </summary>
        public decimal ApiGravity { get; set; }

        /// <summary>
        /// Oil viscosity in cp.
        /// </summary>
        public decimal Viscosity { get; set; }

        /// <summary>
        /// Oil formation volume factor (Bo) in bbl/STB.
        /// </summary>
        public decimal FormationVolumeFactor { get; set; }

        /// <summary>
        /// Solution gas-oil ratio (Rs) in scf/STB.
        /// </summary>
        public decimal SolutionGasOilRatio { get; set; }

        /// <summary>
        /// Oil compressibility in 1/psi.
        /// </summary>
        public decimal Compressibility { get; set; }
    }

    /// <summary>
    /// Represents oil property calculation conditions.
    /// </summary>
    public class OilPropertyConditions
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
        /// Oil API gravity at standard conditions.
        /// </summary>
        public decimal ApiGravity { get; set; }

        /// <summary>
        /// Gas specific gravity (relative to air).
        /// </summary>
        public decimal GasSpecificGravity { get; set; } = 0.65m;

        /// <summary>
        /// Solution gas-oil ratio in scf/STB (if known).
        /// </summary>
        public decimal? SolutionGasOilRatio { get; set; }

        /// <summary>
        /// Bubble point pressure in psia (if known).
        /// </summary>
        public decimal? BubblePointPressure { get; set; }
    }

    /// <summary>
    /// Represents bubble point pressure calculation result.
    /// </summary>
    public class BubblePointResult
    {
        /// <summary>
        /// Bubble point pressure in psia.
        /// </summary>
        public decimal BubblePointPressure { get; set; }

        /// <summary>
        /// Solution gas-oil ratio at bubble point in scf/STB.
        /// </summary>
        public decimal SolutionGasOilRatio { get; set; }
    }

    /// <summary>
    /// Represents solution GOR calculation result.
    /// </summary>
    public class SolutionGORResult
    {
        /// <summary>
        /// Solution gas-oil ratio in scf/STB.
        /// </summary>
        public decimal SolutionGasOilRatio { get; set; }

        /// <summary>
        /// Bubble point pressure in psia.
        /// </summary>
        public decimal BubblePointPressure { get; set; }
    }
}

