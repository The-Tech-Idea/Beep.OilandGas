using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class DcaAnalysisOptions : ModelEntityBase
    {
        public string? ForecastType { get; set; }
        public double? InitialQi { get; set; }
        public double? InitialDi { get; set; }
        public bool? UseAsync { get; set; }
        public double? ConfidenceLevel { get; set; }
        public bool? GenerateForecast { get; set; }
        public int? ForecastMonths { get; set; }
        public decimal? BottomHolePressure { get; set; }
        public decimal? ForecastDuration { get; set; }
        public int? TimeSteps { get; set; }
        public decimal? BubblePointPressure { get; set; }
        public string? TestId { get; set; }
        public DateTime? AsOfDate { get; set; }
        public string? DeclineModel { get; set; }
    }
}
