using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.WellTestAnalysis
{
    public class WellTestAnalysisAdditionalResults : ModelEntityBase
    {
        public string? AnalysisMethod { get; set; }
        public decimal? FlowRate { get; set; }
        public decimal? WellboreRadius { get; set; }
        public decimal? FormationThickness { get; set; }
    }
}
