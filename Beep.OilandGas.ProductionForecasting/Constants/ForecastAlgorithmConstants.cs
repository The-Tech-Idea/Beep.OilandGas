namespace Beep.OilandGas.ProductionForecasting.Constants
{
    /// <summary>
    /// Policy constants for production forecasting algorithms (Arps BDF guardrails, history rules, terminal decline).
    /// </summary>
    public static class ForecastAlgorithmConstants
    {
        /// <summary>Minimum number of distinct production periods required before attempting a history fit.</summary>
        public const int MinHistoryPointsForFit = 6;

        /// <summary>Decline exponent lower bound for hyperbolic / modified-hyperbolic API paths (Arps BDF discipline).</summary>
        public const decimal ArpsBdfMinB = 0.0m;

        /// <summary>Decline exponent upper bound for hyperbolic forecasts (pure Arps b &lt;= 1).</summary>
        public const decimal ArpsBdfMaxB = 1.0m;

        /// <summary>Recommended upper b for boundary-dominated interpretation (fits above this are flagged in notes only).</summary>
        public const decimal ArpsBdfSoftMaxB = 0.95m;

        /// <summary>Default initial oil rate (STB/day) when no history and no request override.</summary>
        public const decimal DefaultQiOilStbPerDay = 1000m;

        /// <summary>Default initial decline Di (1/day) when no history and no request override.</summary>
        public const decimal DefaultDiPerDay = 0.01m;

        /// <summary>Default hyperbolic b when not fitted or supplied.</summary>
        public const decimal DefaultHyperbolicB = 0.5m;

        /// <summary>Default terminal decline Dlim (1/day) for modified hyperbolic.</summary>
        public const decimal DefaultTerminalDeclineDiPerDay = 0.0001m;

        /// <summary>Average days per month for month-count to duration conversion.</summary>
        public const decimal DaysPerMonth = 30.437m;
    }
}
