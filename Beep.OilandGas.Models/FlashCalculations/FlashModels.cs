using System.Collections.Generic;

namespace Beep.OilandGas.Models.FlashCalculations
{
    /// <summary>
    /// Represents a component in a mixture.
    /// </summary>
    public class Component
    {
        /// <summary>
        /// Component name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mole fraction in feed.
        /// </summary>
        public decimal MoleFraction { get; set; }

        /// <summary>
        /// Critical temperature in Rankine.
        /// </summary>
        public decimal CriticalTemperature { get; set; }

        /// <summary>
        /// Critical pressure in psia.
        /// </summary>
        public decimal CriticalPressure { get; set; }

        /// <summary>
        /// Acentric factor.
        /// </summary>
        public decimal AcentricFactor { get; set; }

        /// <summary>
        /// Molecular weight.
        /// </summary>
        public decimal MolecularWeight { get; set; }
    }

    /// <summary>
    /// Represents flash calculation conditions.
    /// </summary>
    public class FlashConditions
    {
        /// <summary>
        /// System pressure in psia.
        /// </summary>
        public decimal Pressure { get; set; }

        /// <summary>
        /// System temperature in Rankine.
        /// </summary>
        public decimal Temperature { get; set; }

        /// <summary>
        /// Feed composition (mole fractions).
        /// </summary>
        public List<Component> FeedComposition { get; set; } = new();
    }

    /// <summary>
    /// Represents flash calculation results.
    /// </summary>
    public class FlashResult
    {
        /// <summary>
        /// Vapor fraction (0-1).
        /// </summary>
        public decimal VaporFraction { get; set; }

        /// <summary>
        /// Liquid fraction (0-1).
        /// </summary>
        public decimal LiquidFraction { get; set; }

        /// <summary>
        /// Vapor phase composition (mole fractions).
        /// </summary>
        public Dictionary<string, decimal> VaporComposition { get; set; } = new();

        /// <summary>
        /// Liquid phase composition (mole fractions).
        /// </summary>
        public Dictionary<string, decimal> LiquidComposition { get; set; } = new();

        /// <summary>
        /// K-values (equilibrium ratios).
        /// </summary>
        public Dictionary<string, decimal> KValues { get; set; } = new();

        /// <summary>
        /// Number of iterations required.
        /// </summary>
        public int Iterations { get; set; }

        /// <summary>
        /// Convergence achieved.
        /// </summary>
        public bool Converged { get; set; }

        /// <summary>
        /// Convergence error.
        /// </summary>
        public decimal ConvergenceError { get; set; }
    }

    /// <summary>
    /// Represents phase properties.
    /// </summary>
    public class PhaseProperties
    {
        /// <summary>
        /// Phase density in lb/ft³.
        /// </summary>
        public decimal Density { get; set; }

        /// <summary>
        /// Phase molecular weight.
        /// </summary>
        public decimal MolecularWeight { get; set; }

        /// <summary>
        /// Phase specific gravity.
        /// </summary>
        public decimal SpecificGravity { get; set; }

        /// <summary>
        /// Phase volume in ft³.
        /// </summary>
        public decimal Volume { get; set; }
    }
}

