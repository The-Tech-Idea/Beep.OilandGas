using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.HydraulicPumps
{
    public class HydraulicPumpAnalysisOptions : ModelEntityBase
    {
        public decimal? PowerFluidTemperature { get; set; }
        public decimal? FluidViscosity { get; set; }
        public string? Notes { get; set; }
    }
}
