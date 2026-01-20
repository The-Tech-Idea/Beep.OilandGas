using System;

namespace Beep.OilandGas.Models.Data.GasProperties
{
    /// <summary>
    /// Represents gas composition with mole fractions
    /// DTO for calculations - Entity class: GAS_COMPOSITION
    /// </summary>
    public class GasComposition : ModelEntityBase
    {
        /// <summary>
        /// Mole fraction of methane (C1)
        /// </summary>
        public decimal MethaneFraction { get; set; }

        /// <summary>
        /// Mole fraction of ethane (C2)
        /// </summary>
        public decimal EthaneFraction { get; set; }

        /// <summary>
        /// Mole fraction of propane (C3)
        /// </summary>
        public decimal PropaneFraction { get; set; }

        /// <summary>
        /// Mole fraction of i-butane (iC4)
        /// </summary>
        public decimal IButaneFraction { get; set; }

        /// <summary>
        /// Mole fraction of n-butane (nC4)
        /// </summary>
        public decimal NButaneFraction { get; set; }

        /// <summary>
        /// Mole fraction of i-pentane (iC5)
        /// </summary>
        public decimal IPentaneFraction { get; set; }

        /// <summary>
        /// Mole fraction of n-pentane (nC5)
        /// </summary>
        public decimal NPentaneFraction { get; set; }

        /// <summary>
        /// Mole fraction of hexane plus (C6+)
        /// </summary>
        public decimal HexanePlusFraction { get; set; }

        /// <summary>
        /// Mole fraction of nitrogen (N2)
        /// </summary>
        public decimal NitrogenFraction { get; set; }

        /// <summary>
        /// Mole fraction of carbon dioxide (CO2)
        /// </summary>
        public decimal CarbonDioxideFraction { get; set; }

        /// <summary>
        /// Mole fraction of hydrogen sulfide (H2S)
        /// </summary>
        public decimal HydrogenSulfideFraction { get; set; }

        /// <summary>
        /// Validates that all fractions sum to 1.0 (within tolerance)
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
}



