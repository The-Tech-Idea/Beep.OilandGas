namespace Beep.OilandGas.NodalAnalysis.Constants
{
    /// <summary>
    /// Constants used in nodal analysis calculations.
    /// </summary>
    public static class NodalConstants
    {
        /// <summary>
        /// Conversion factor: BPD to ft³/s.
        /// </summary>
        public const double BPDToFt3PerSec = 5.615 / 86400.0;

        /// <summary>
        /// Conversion factor: psi to ft of water.
        /// </summary>
        public const double PsiToFeetOfWater = 2.307;

        /// <summary>
        /// Conversion factor: cp to lb-s/ft².
        /// </summary>
        public const double CpToLbSPerFt2 = 0.000672;

        /// <summary>
        /// Gravitational acceleration in ft/s².
        /// </summary>
        public const double Gravity = 32.174;

        /// <summary>
        /// Water density in lb/ft³.
        /// </summary>
        public const double WaterDensity = 62.4;

        /// <summary>
        /// Standard atmospheric pressure in psi.
        /// </summary>
        public const double AtmosphericPressure = 14.7;

        /// <summary>
        /// Standard temperature in Rankine.
        /// </summary>
        public const double StandardTemperature = 520.0;

        /// <summary>
        /// Gas constant in ft-lbf/(lbmol-R).
        /// </summary>
        public const double GasConstant = 10.73;

        /// <summary>
        /// Minimum flow rate for valid analysis (BPD).
        /// </summary>
        public const double MinFlowRate = 0.1;

        /// <summary>
        /// Maximum flow rate for valid analysis (BPD).
        /// </summary>
        public const double MaxFlowRate = 50000.0;

        /// <summary>
        /// Minimum pressure for valid analysis (psi).
        /// </summary>
        public const double MinPressure = 0.1;

        /// <summary>
        /// Maximum pressure for valid analysis (psi).
        /// </summary>
        public const double MaxPressure = 20000.0;

        /// <summary>
        /// Minimum tubing diameter (inches).
        /// </summary>
        public const double MinTubingDiameter = 0.5;

        /// <summary>
        /// Maximum tubing diameter (inches).
        /// </summary>
        public const double MaxTubingDiameter = 10.0;

        /// <summary>
        /// Minimum tubing length (feet).
        /// </summary>
        public const double MinTubingLength = 100.0;

        /// <summary>
        /// Maximum tubing length (feet).
        /// </summary>
        public const double MaxTubingLength = 30000.0;

        /// <summary>
        /// Epsilon for floating point comparisons.
        /// </summary>
        public const double Epsilon = 1e-10;
    }
}

