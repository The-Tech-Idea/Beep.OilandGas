namespace Beep.OilandGas.WellTestAnalysis.Constants
{
    /// <summary>
    /// Constants used in well test analysis calculations.
    /// </summary>
    public static class WellTestConstants
    {
        /// <summary>
        /// Conversion factor for permeability (md to ft²).
        /// </summary>
        public const double PermeabilityConversionFactor = 1.127e-3;

        /// <summary>
        /// Conversion factor for time (hours to days).
        /// </summary>
        public const double HoursToDays = 24.0;

        /// <summary>
        /// Conversion factor for pressure (psi to atm).
        /// </summary>
        public const double PsiToAtm = 0.068046;

        /// <summary>
        /// Conversion factor for temperature (Fahrenheit to Rankine).
        /// </summary>
        public const double FahrenheitToRankine = 459.67;

        /// <summary>
        /// Euler's number.
        /// </summary>
        public const double E = 2.718281828459045;

        /// <summary>
        /// Pi constant.
        /// </summary>
        public const double Pi = 3.141592653589793;

        /// <summary>
        /// Minimum time for valid analysis (hours).
        /// </summary>
        public const double MinTime = 0.001;

        /// <summary>
        /// Maximum time for valid analysis (hours).
        /// </summary>
        public const double MaxTime = 100000.0;

        /// <summary>
        /// Minimum pressure for valid analysis (psi).
        /// </summary>
        public const double MinPressure = 0.1;

        /// <summary>
        /// Maximum pressure for valid analysis (psi).
        /// </summary>
        public const double MaxPressure = 20000.0;

        /// <summary>
        /// Minimum flow rate for valid analysis (BPD).
        /// </summary>
        public const double MinFlowRate = 0.1;

        /// <summary>
        /// Maximum flow rate for valid analysis (BPD).
        /// </summary>
        public const double MaxFlowRate = 100000.0;

        /// <summary>
        /// Minimum porosity (fraction).
        /// </summary>
        public const double MinPorosity = 0.01;

        /// <summary>
        /// Maximum porosity (fraction).
        /// </summary>
        public const double MaxPorosity = 0.50;

        /// <summary>
        /// Minimum compressibility (psi^-1).
        /// </summary>
        public const double MinCompressibility = 1e-7;

        /// <summary>
        /// Maximum compressibility (psi^-1).
        /// </summary>
        public const double MaxCompressibility = 1e-3;

        /// <summary>
        /// Minimum viscosity (cp).
        /// </summary>
        public const double MinViscosity = 0.1;

        /// <summary>
        /// Maximum viscosity (cp).
        /// </summary>
        public const double MaxViscosity = 1000.0;

        /// <summary>
        /// Minimum formation thickness (feet).
        /// </summary>
        public const double MinFormationThickness = 1.0;

        /// <summary>
        /// Maximum formation thickness (feet).
        /// </summary>
        public const double MaxFormationThickness = 1000.0;

        /// <summary>
        /// Minimum wellbore radius (feet).
        /// </summary>
        public const double MinWellboreRadius = 0.1;

        /// <summary>
        /// Maximum wellbore radius (feet).
        /// </summary>
        public const double MaxWellboreRadius = 10.0;

        /// <summary>
        /// Epsilon for floating point comparisons.
        /// </summary>
        public const double Epsilon = 1e-10;

        /// <summary>
        /// Default smoothing factor for derivative calculation.
        /// </summary>
        public const double DefaultDerivativeSmoothing = 0.1;
    }
}

