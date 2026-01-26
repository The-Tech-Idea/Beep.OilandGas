using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class PipelineAnalysisAdditionalResults : ModelEntityBase
    {
        public decimal? AverageVelocity { get; set; }
        public decimal? MachNumber { get; set; }
        public decimal? LinePack { get; set; }
        public string? Notes { get; set; }
    }
}
