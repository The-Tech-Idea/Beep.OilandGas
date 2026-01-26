using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ChokeAnalysisOptions : ModelEntityBase
    {
        public decimal? CriticalPressureRatioOverride { get; set; }
        public string? CorrelationMethod { get; set; }
    }
}
