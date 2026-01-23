using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.FlashCalculations
{
    /// <summary>
    /// Result of flash calculation
    /// DTO for calculations - Entity class: FLASH_CALCULATION_RESULT
    /// </summary>
    public class FlashResult : ModelEntityBase
    {
        /// <summary>
        /// Vapor fraction (0 to 1)
        /// </summary>
        private decimal VaporFractionValue;

        public decimal VaporFraction

        {

            get { return this.VaporFractionValue; }

            set { SetProperty(ref VaporFractionValue, value); }

        }

        /// <summary>
        /// Liquid fraction (0 to 1)
        /// </summary>
        private decimal LiquidFractionValue;

        public decimal LiquidFraction

        {

            get { return this.LiquidFractionValue; }

            set { SetProperty(ref LiquidFractionValue, value); }

        }

        /// <summary>
        /// Number of iterations required for convergence
        /// </summary>
        private int IterationsValue;

        public int Iterations

        {

            get { return this.IterationsValue; }

            set { SetProperty(ref IterationsValue, value); }

        }

        /// <summary>
        /// Whether the calculation converged
        /// </summary>
        private bool ConvergedValue;

        public bool Converged

        {

            get { return this.ConvergedValue; }

            set { SetProperty(ref ConvergedValue, value); }

        }

        /// <summary>
        /// Convergence error
        /// </summary>
        private decimal ConvergenceErrorValue;

        public decimal ConvergenceError

        {

            get { return this.ConvergenceErrorValue; }

            set { SetProperty(ref ConvergenceErrorValue, value); }

        }

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





