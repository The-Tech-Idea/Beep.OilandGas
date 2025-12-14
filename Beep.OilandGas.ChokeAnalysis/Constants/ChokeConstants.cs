namespace Beep.OilandGas.ChokeAnalysis.Constants
{
    /// <summary>
    /// Constants used in choke flow calculations.
    /// </summary>
    public static class ChokeConstants
    {
        /// <summary>
        /// Specific heat ratio for natural gas.
        /// </summary>
        public const decimal GasSpecificHeatRatio = 1.3m;

        /// <summary>
        /// Gravitational acceleration in ft/s².
        /// </summary>
        public const decimal GravitationalAcceleration = 32.174m;

        /// <summary>
        /// Universal gas constant in psia·ft³/(lbmol·°R).
        /// </summary>
        public const decimal UniversalGasConstant = 10.7316m;

        /// <summary>
        /// Standard discharge coefficient for chokes.
        /// </summary>
        public const decimal StandardDischargeCoefficient = 0.85m;

        /// <summary>
        /// Minimum choke diameter in inches.
        /// </summary>
        public const decimal MinimumChokeDiameter = 0.01m;

        /// <summary>
        /// Maximum choke diameter in inches.
        /// </summary>
        public const decimal MaximumChokeDiameter = 2.0m;

        /// <summary>
        /// Minimum pressure ratio.
        /// </summary>
        public const decimal MinimumPressureRatio = 0.01m;

        /// <summary>
        /// Maximum pressure ratio.
        /// </summary>
        public const decimal MaximumPressureRatio = 1.0m;

        /// <summary>
        /// Conversion factor: square inches to square feet.
        /// </summary>
        public const decimal SquareInchesToSquareFeet = 144m;

        /// <summary>
        /// Conversion factor: Mscf to scf.
        /// </summary>
        public const decimal MscfToScf = 1000m;

        /// <summary>
        /// Conversion factor: scf to bbl (for gas).
        /// </summary>
        public const decimal ScfToBbl = 5.614m;
    }
}

