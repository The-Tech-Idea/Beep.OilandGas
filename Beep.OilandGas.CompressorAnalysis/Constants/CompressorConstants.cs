namespace Beep.OilandGas.CompressorAnalysis.Constants
{
    /// <summary>
    /// Constants used in compressor calculations.
    /// </summary>
    public static class CompressorConstants
    {
        /// <summary>
        /// Gas constant in ft-lbf/(lbmol-R).
        /// </summary>
        public const decimal GasConstant = 1545.0m;

        /// <summary>
        /// Standard molecular weight of air in lb/lbmol.
        /// </summary>
        public const decimal AirMolecularWeight = 28.9645m;

        /// <summary>
        /// Standard volume of gas at standard conditions in ft³/lbmol.
        /// </summary>
        public const decimal StandardGasVolume = 379.0m;

        /// <summary>
        /// Conversion factor: HP to kW.
        /// </summary>
        public const decimal HorsepowerToKilowatts = 0.746m;

        /// <summary>
        /// Conversion factor: ft-lbf/min to HP.
        /// </summary>
        public const decimal FootPoundsPerMinuteToHorsepower = 33000m;

        /// <summary>
        /// Standard specific heat ratio for natural gas.
        /// </summary>
        public const decimal StandardSpecificHeatRatio = 1.3m;

        /// <summary>
        /// Standard polytropic efficiency for centrifugal compressors.
        /// </summary>
        public const decimal StandardPolytropicEfficiency = 0.75m;

        /// <summary>
        /// Standard efficiency for reciprocating compressors.
        /// </summary>
        public const decimal StandardReciprocatingEfficiency = 0.85m;

        /// <summary>
        /// Standard mechanical efficiency.
        /// </summary>
        public const decimal StandardMechanicalEfficiency = 0.95m;

        /// <summary>
        /// Standard volumetric efficiency for reciprocating compressors.
        /// </summary>
        public const decimal StandardVolumetricEfficiency = 0.85m;

        /// <summary>
        /// Standard clearance factor for reciprocating compressors.
        /// </summary>
        public const decimal StandardClearanceFactor = 0.05m;

        /// <summary>
        /// Maximum compression ratio.
        /// </summary>
        public const decimal MaximumCompressionRatio = 10.0m;

        /// <summary>
        /// Minimum compression ratio.
        /// </summary>
        public const decimal MinimumCompressionRatio = 1.0m;

        /// <summary>
        /// Minutes per day.
        /// </summary>
        public const decimal MinutesPerDay = 1440m;

        /// <summary>
        /// Conversion factor: Mscf to scf.
        /// </summary>
        public const decimal MscfToScf = 1000m;
    }
}

