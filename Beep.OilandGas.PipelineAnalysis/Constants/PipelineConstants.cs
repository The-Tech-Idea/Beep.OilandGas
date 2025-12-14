namespace Beep.OilandGas.PipelineAnalysis.Constants
{
    /// <summary>
    /// Constants used in pipeline calculations.
    /// </summary>
    public static class PipelineConstants
    {
        /// <summary>
        /// Standard pipeline roughness values in feet.
        /// </summary>
        public static class Roughness
        {
            /// <summary>
            /// Smooth pipe (drawn tubing).
            /// </summary>
            public const decimal Smooth = 0.000005m;

            /// <summary>
            /// Commercial steel.
            /// </summary>
            public const decimal CommercialSteel = 0.00015m;

            /// <summary>
            /// Cast iron.
            /// </summary>
            public const decimal CastIron = 0.00085m;

            /// <summary>
            /// Concrete.
            /// </summary>
            public const decimal Concrete = 0.001m;

            /// <summary>
            /// Riveted steel.
            /// </summary>
            public const decimal RivetedSteel = 0.003m;
        }

        /// <summary>
        /// Standard pipeline diameters in inches.
        /// </summary>
        public static readonly decimal[] StandardDiameters = { 2m, 3m, 4m, 6m, 8m, 10m, 12m, 16m, 20m, 24m, 30m, 36m, 42m, 48m };

        /// <summary>
        /// Standard acceleration due to gravity in ft/s².
        /// </summary>
        public const decimal Gravity = 32.174m;

        /// <summary>
        /// Conversion factor: bbl to ft³.
        /// </summary>
        public const decimal BarrelToCubicFeet = 5.615m;

        /// <summary>
        /// Conversion factor: cp to lb/(ft-s).
        /// </summary>
        public const decimal CentipoiseToLbPerFtS = 0.000672m;

        /// <summary>
        /// Standard base pressure in psia.
        /// </summary>
        public const decimal StandardBasePressure = 14.7m;

        /// <summary>
        /// Standard base temperature in Rankine.
        /// </summary>
        public const decimal StandardBaseTemperature = 520m;

        /// <summary>
        /// Standard gas volume at base conditions in ft³/lbmol.
        /// </summary>
        public const decimal StandardGasVolume = 379.0m;

        /// <summary>
        /// Minutes per day.
        /// </summary>
        public const decimal MinutesPerDay = 1440m;

        /// <summary>
        /// Seconds per day.
        /// </summary>
        public const decimal SecondsPerDay = 86400m;

        /// <summary>
        /// Conversion factor: Mscf to scf.
        /// </summary>
        public const decimal MscfToScf = 1000m;

        /// <summary>
        /// Conversion factor: inches to feet.
        /// </summary>
        public const decimal InchesToFeet = 12m;

        /// <summary>
        /// Reynolds number threshold for laminar flow.
        /// </summary>
        public const decimal LaminarFlowThreshold = 2000m;

        /// <summary>
        /// Reynolds number threshold for turbulent flow.
        /// </summary>
        public const decimal TurbulentFlowThreshold = 4000m;
    }
}

