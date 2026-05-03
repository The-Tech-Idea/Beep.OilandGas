namespace Beep.OilandGas.NodalAnalysis.Constants
{
    internal static class NodalArtificialLiftPolicyConstants
    {
        public const decimal MaxDepthForPcpFt = 5000m;
        public const decimal DeepWellThresholdFt = 10000m;
        public const decimal HighWaterCutThreshold = 0.50m;
        public const decimal ExtremeWaterCutThreshold = 0.70m;
        public const decimal HighRateThresholdBpd = 1000m;
        public const decimal EspPreferredRateBpd = 500m;
    }
}
