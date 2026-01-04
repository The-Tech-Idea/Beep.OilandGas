using System.Collections.Generic;

namespace Beep.OilandGas.Models.FlashCalculations
{
    /// <summary>
    /// Result of flash calculation
    /// DTO for calculations - Entity class: FLASH_CALCULATION_RESULT
    /// </summary>
    public class FlashResult
    {
        /// <summary>
        /// Vapor fraction (0 to 1)
        /// </summary>
        public decimal VaporFraction { get; set; }

        /// <summary>
        /// Liquid fraction (0 to 1)
        /// </summary>
        public decimal LiquidFraction { get; set; }

        /// <summary>
        /// Number of iterations required for convergence
        /// </summary>
        public int Iterations { get; set; }

        /// <summary>
        /// Whether the calculation converged
        /// </summary>
        public bool Converged { get; set; }

        /// <summary>
        /// Convergence error
        /// </summary>
        public decimal ConvergenceError { get; set; }

        /// <summary>
        /// K-values for each component
        /// </summary>
        public Dictionary<string, decimal> KValues { get; set; } = new Dictionary<string, decimal>();

        /// <summary>
        /// Vapor phase composition (mole fractions)
        /// </summary>
        public Dictionary<string, decimal> VaporComposition { get; set; } = new Dictionary<string, decimal>();

        /// <summary>
        /// Liquid phase composition (mole fractions)
        /// </summary>
        public Dictionary<string, decimal> LiquidComposition { get; set; } = new Dictionary<string, decimal>();
    }
}
