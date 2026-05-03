namespace Beep.OilandGas.ChokeAnalysis.Constants
{
    /// <summary>
    /// Constants used in choke flow calculations.
    /// </summary>
    public static class ChokeConstants
    {
        // ─────────────────────────────────────────────────────────────────
        // Lift System Type Identifiers
        // Used in choke-lift interaction analysis
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Electrical Submersible Pump lift system.</summary>
        public const string LiftSystemESP = "ESP";

        /// <summary>Gas lift system.</summary>
        public const string LiftSystemGasLift = "GasLift";

        /// <summary>
        /// Checks if the given lift system type is an ESP.
        /// </summary>
        public static bool IsESP(string? liftSystemType)
        {
            return !string.IsNullOrWhiteSpace(liftSystemType)
                && liftSystemType.Contains("ESP", System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if the given lift system type is Gas Lift.
        /// </summary>
        public static bool IsGasLift(string? liftSystemType)
        {
            return !string.IsNullOrWhiteSpace(liftSystemType)
                && liftSystemType.Contains("GasLift", System.StringComparison.OrdinalIgnoreCase);
        }

        // ─────────────────────────────────────────────────────────────────
        // Lift System Efficiency Defaults
        // Base efficiency percentages for different lift types
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Base efficiency for ESP systems (%).</summary>
        public const decimal LiftEfficiencyESP = 70m;

        /// <summary>Base efficiency for Gas Lift systems (%).</summary>
        public const decimal LiftEfficiencyGasLift = 60m;

        /// <summary>Default base efficiency for unknown lift systems (%).</summary>
        public const decimal LiftEfficiencyDefault = 50m;

        /// <summary>Minimum allowable lift efficiency (%).</summary>
        public const decimal LiftEfficiencyMin = 20m;

        /// <summary>Maximum allowable lift efficiency (%).</summary>
        public const decimal LiftEfficiencyMax = 90m;

        /// <summary>Efficiency penalty per 1000 psi backpressure (%).</summary>
        public const decimal LiftEfficiencyBackpressurePenalty = 5m;

        // ─────────────────────────────────────────────────────────────────
        // ESP Operating Constraints
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Maximum ESP discharge pressure (psi).</summary>
        public const decimal ESPMaxDischargePressure = 5000m;

        /// <summary>Maximum ESP inlet GOR (scf/bbl).</summary>
        public const decimal ESPMaxInletGOR = 3000m;

        // ─────────────────────────────────────────────────────────────────
        // Choke Adjustment Factors
        // Used in lift-choke interaction optimization
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Choke increase factor when efficiency is below threshold.</summary>
        public const decimal ChokeAdjustmentIncrease = 1.1m;

        /// <summary>Choke decrease factor when efficiency is acceptable.</summary>
        public const decimal ChokeAdjustmentDecrease = 0.95m;

        /// <summary>Efficiency threshold for choke adjustment (%).</summary>
        public const decimal ChokeAdjustmentEfficiencyThreshold = 80m;

        /// <summary>Minimum choke size for optimization (inches).</summary>
        public const decimal ChokeOptimizationMinSize = 0.2m;

        /// <summary>Maximum choke size for optimization (inches).</summary>
        public const decimal ChokeOptimizationMaxSize = 1.5m;

        // ─────────────────────────────────────────────────────────────────
        // Erosion and Settling Velocity Factors
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Erosion formula velocity squared multiplier.</summary>
        public const decimal ErosionVelocityFactor = 0.7m;

        /// <summary>Erosion formula denominator factor.</summary>
        public const decimal ErosionDenominatorFactor = 500m;

        /// <summary>Settling velocity numerator factor.</summary>
        public const decimal SettlingVelocityNumerator = 0.0001m;

        /// <summary>Settling velocity denominator factor.</summary>
        public const decimal SettlingVelocityDenominator = 100m;

        // ─────────────────────────────────────────────────────────────────
        // Standard Flow Constants
        // ─────────────────────────────────────────────────────────────────

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

