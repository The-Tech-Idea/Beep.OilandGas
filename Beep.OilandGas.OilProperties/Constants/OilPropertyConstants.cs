namespace Beep.OilandGas.OilProperties.Constants
{
    /// <summary>
    /// Constants used in oil property calculations.
    /// </summary>
    public static class OilPropertyConstants
    {
        /// <summary>
        /// Standard API gravity conversion constant.
        /// </summary>
        public const decimal ApiGravityConstant = 141.5m;

        /// <summary>
        /// Standard API gravity offset.
        /// </summary>
        public const decimal ApiGravityOffset = 131.5m;

        /// <summary>
        /// Standard water density in lb/ft³ at 60°F.
        /// </summary>
        public const decimal WaterDensity = 62.4m;

        /// <summary>
        /// Standard temperature in Rankine (60°F).
        /// </summary>
        public const decimal StandardTemperature = 520m;

        /// <summary>
        /// Standard pressure in psia.
        /// </summary>
        public const decimal StandardPressure = 14.7m;

        /// <summary>
        /// Minimum oil viscosity in cp.
        /// </summary>
        public const decimal MinimumViscosity = 0.1m;

        /// <summary>
        /// Maximum oil viscosity in cp.
        /// </summary>
        public const decimal MaximumViscosity = 10000m;

        /// <summary>
        /// Minimum formation volume factor.
        /// </summary>
        public const decimal MinimumFormationVolumeFactor = 1.0m;

        /// <summary>
        /// Maximum formation volume factor.
        /// </summary>
        public const decimal MaximumFormationVolumeFactor = 3.0m;

        /// <summary>
        /// Minimum API gravity.
        /// </summary>
        public const decimal MinimumApiGravity = 0m;

        /// <summary>
        /// Maximum API gravity.
        /// </summary>
        public const decimal MaximumApiGravity = 100m;

        /// <summary>
        /// Minimum solution GOR in scf/STB.
        /// </summary>
        public const decimal MinimumSolutionGOR = 0m;

        /// <summary>
        /// Maximum solution GOR in scf/STB.
        /// </summary>
        public const decimal MaximumSolutionGOR = 10000m;

        /// <summary>
        /// Minimum bubble point pressure in psia.
        /// </summary>
        public const decimal MinimumBubblePointPressure = 0m;

        /// <summary>
        /// Maximum bubble point pressure in psia.
        /// </summary>
        public const decimal MaximumBubblePointPressure = 10000m;

        /// <summary>
        /// Minimum oil compressibility in 1/psi.
        /// </summary>
        public const decimal MinimumCompressibility = 0.000001m;

        /// <summary>
        /// Maximum oil compressibility in 1/psi.
        /// </summary>
        public const decimal MaximumCompressibility = 0.001m;
    }
}

