using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.HydraulicPumps
{
    public class HydraulicPumpAnalysisAdditionalResults : ModelEntityBase
    {
        public decimal? JetEfficiency { get; set; }
        public decimal? PowerFluidRate { get; set; }
        public string? Notes { get; set; }
    }
}
