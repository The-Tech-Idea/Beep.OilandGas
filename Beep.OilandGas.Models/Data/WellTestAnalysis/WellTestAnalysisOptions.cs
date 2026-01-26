using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.WellTestAnalysis
{
    public class WellTestAnalysisOptions : ModelEntityBase
    {
        public bool? UsePressureDerivative { get; set; }
        public decimal? PressureDerivativeSmoothing { get; set; }
        public bool? UseSuperposition { get; set; }
        public bool? UsePseudopressure { get; set; }
    }
}
