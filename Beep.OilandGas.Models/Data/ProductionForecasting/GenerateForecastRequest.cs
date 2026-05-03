using Beep.OilandGas.Models.Data.ProductionForecasting;

namespace Beep.OilandGas.Models.Data.ProductionForecasting
{
    /// <summary>
    /// Request parameters for generating a production forecast.
    /// </summary>
    public class GenerateForecastRequest
    {
        /// <summary>Well UWI to forecast. Null to forecast at field level.</summary>
        public string? WellUWI { get; set; }

        /// <summary>Field identifier for field-level forecasting.</summary>
        public string? FieldId { get; set; }

        /// <summary>Decline curve or forecasting method to apply.</summary>
        public ForecastType ForecastMethod { get; set; }

        /// <summary>Number of months to forecast.</summary>
        public int ForecastPeriod { get; set; } = 12;

        /// <summary>Optional initial oil rate (STB/day). When set with decline parameters, skips history fit for qi.</summary>
        public decimal? InitialOilRateQi { get; set; }

        /// <summary>Optional initial decline Di (1/day). When set with qi, skips history fit for Di.</summary>
        public decimal? InitialDeclineDi { get; set; }

        /// <summary>Optional Arps b for hyperbolic / auto paths. Clamped to BDF policy bounds when fitted or supplied.</summary>
        public decimal? DeclineExponentB { get; set; }

        /// <summary>Optional economic limit oil rate (STB/day). Truncates harmonic/hyperbolic tails when above zero and below qi.</summary>
        public decimal? EconomicLimitOilRate { get; set; }

        /// <summary>Terminal decline Dlim (1/day) for modified hyperbolic when <see cref="UseModifiedHyperbolic"/> is true.</summary>
        public decimal? TerminalDeclineDi { get; set; }

        /// <summary>Use modified hyperbolic (switch to terminal exponential at Dlim) when decline method is hyperbolic or auto.</summary>
        public bool UseModifiedHyperbolic { get; set; }

        /// <summary>When false and <see cref="WellUWI"/> is set, attempts PPDM history fit for missing qi/Di/b.</summary>
        public bool SkipHistoryFit { get; set; }
    }
}
