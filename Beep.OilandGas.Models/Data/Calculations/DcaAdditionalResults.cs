using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    /// <summary>
    /// Additional DCA result metadata for forecast summary.
    /// </summary>
    public class DcaAdditionalResults : ModelEntityBase
    {
        public string? ForecastType { get; set; }
        public decimal? ForecastDuration { get; set; }
        public decimal? InitialProductionRate { get; set; }
        public decimal? FinalProductionRate { get; set; }
        public decimal? TotalCumulativeProduction { get; set; }
        public int? ForecastPointCount { get; set; }
        public double? AdjustedRSquared { get; set; }
        public double? Mae { get; set; }
        public double? Aic { get; set; }
        public double? Bic { get; set; }
        public int? Iterations { get; set; }
        public bool? Converged { get; set; }
        public int? DataPointCount { get; set; }
    }
}
