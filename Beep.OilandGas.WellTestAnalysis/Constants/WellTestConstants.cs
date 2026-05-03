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

        /// <summary>
        /// Minimum |Δt| (hours) used when differencing pressure vs time for derivatives (avoids division by zero).
        /// Also used as a floor for log-coordinate spans and tiny regression denominators where divide-by-zero must be avoided.
        /// </summary>
        public const double DerivativeMinimumDeltaTime = 1e-12;

        /// <summary>
        /// Oil Horner semi-log permeability factor (field units): k (md) = factor × q B μ / (|m| h) with q in STB/d, m in psi/log cycle.
        /// </summary>
        public const double HornerOilPermeabilityFactor = 162.6;

        /// <summary>
        /// Horner / MDH skin-factor multiplier (log10 form).
        /// </summary>
        public const double HornerSkinFactorMultiplier = 1.151;

        /// <summary>
        /// Constant term in the Horner skin equation (log10 form, oil field units).
        /// </summary>
        public const double HornerSkinEquationOffset = 3.23;

        /// <summary>
        /// In derivative model identification, middle-time IARF is declared when
        /// relative std-dev of derivative / mean derivative is below this threshold.
        /// </summary>
        public const double DerivativeIdentifyInfiniteActingRelativeStdDev = 0.1;

        /// <summary>
        /// Late-time derivative trend (last − first) must exceed this fraction of the middle-time average
        /// to flag a closed boundary, or be more negative than this fraction for a constant-pressure boundary.
        /// </summary>
        public const double DerivativeIdentifyLateTrendVsMiddleAverage = 0.2;

        /// <summary>
        /// Dual-porosity dip recovery (post-minimum average minus minimum) must exceed this fraction of the dip depth.
        /// </summary>
        public const double DerivativeIdentifyDualPorosityRecoveryRatio = 0.3;

        /// <summary>Weights for three-point moving average smoothing of Bourdet derivative (outer / center / outer).</summary>
        public const double DerivativeSmoothingMovingAverageOuterWeight = 0.25;

        /// <summary>Center weight for three-point moving average smoothing of Bourdet derivative.</summary>
        public const double DerivativeSmoothingMovingAverageCenterWeight = 0.5;

        /// <summary>Target log-log slope for wellbore storage (unit slope).</summary>
        public const double FlowRegimeWellboreStorageLogLogSlope = 1.0;

        /// <summary>Tolerance when matching wellbore storage unit slope.</summary>
        public const double FlowRegimeWellboreStorageSlopeTolerance = 0.15;

        /// <summary>Target log-log slope for infinite-acting radial flow (flat derivative).</summary>
        public const double FlowRegimeIarfLogLogSlope = 0.0;

        /// <summary>Tolerance when matching IARF near-zero slope on log-log derivative.</summary>
        public const double FlowRegimeIarfSlopeTolerance = 0.1;

        /// <summary>|Slope| below this on IARF segment maps confidence label to High.</summary>
        public const double FlowRegimeIarfHighConfidenceSlopeAbsMax = 0.05;

        /// <summary>Target log-log slope for linear flow diagnostic.</summary>
        public const double FlowRegimeLinearFlowLogLogSlope = 0.5;

        /// <summary>Tolerance when matching linear-flow half slope.</summary>
        public const double FlowRegimeLinearFlowSlopeTolerance = 0.12;

        /// <summary>Target log-log slope for bilinear flow diagnostic.</summary>
        public const double FlowRegimeBilinearFlowLogLogSlope = 0.25;

        /// <summary>Tolerance when matching bilinear quarter slope.</summary>
        public const double FlowRegimeBilinearFlowSlopeTolerance = 0.08;

        /// <summary>Target log-log slope for pseudo-steady state (closed boundary).</summary>
        public const double FlowRegimePssLogLogSlope = 1.0;

        /// <summary>Tolerance when matching late-time unit slope (PSS).</summary>
        public const double FlowRegimePssSlopeTolerance = 0.15;

        /// <summary>Decline in log₁₀(|derivative|) over late points used to hint steady-state / constant-pressure boundary.</summary>
        public const double FlowRegimeSteadyStateLateLogDerivativeDecline = 0.3;

        /// <summary>Fraction of |average late derivative| used to classify constant-pressure vs closed boundary in drawdown late-time diagnostics.</summary>
        public const double DrawdownBoundaryDerivativeTrendVersusAverage = 0.1;

        /// <summary>Minimum |late semi-log slope| treated as zero when classifying pseudo-steady vs constant-pressure (drawdown extended analysis).</summary>
        public const double DrawdownPseudoSteadySlopeAbsFloor = 1e-6;
    }
}

