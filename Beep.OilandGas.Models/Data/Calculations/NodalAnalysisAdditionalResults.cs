using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class NodalAnalysisAdditionalResults : ModelEntityBase
    {
        public string? WellUwi { get; set; }
        public decimal? ReservoirPressure { get; set; }
        public decimal? ProductivityIndex { get; set; }
        public decimal? TubingDiameter { get; set; }
        public decimal? TubingLength { get; set; }
        public decimal? WellheadPressure { get; set; }
        public string? IprMethod { get; set; }
        public string? VlpModel { get; set; }
    }
}
