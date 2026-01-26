using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class GasLiftAnalysisAdditionalResults : ModelEntityBase
    {
        public decimal? GasInjectionPressure { get; set; }
        public int? NumberOfValves { get; set; }
        public decimal? TotalDepthCoverage { get; set; }
        public decimal? SystemEfficiency { get; set; }
    }
}
