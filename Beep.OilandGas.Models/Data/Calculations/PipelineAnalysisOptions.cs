using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class PipelineAnalysisOptions : ModelEntityBase
    {
        public decimal? BaseTemperature { get; set; }
        public decimal? BasePressure { get; set; }
        public decimal? GasViscosity { get; set; }
    }
}
