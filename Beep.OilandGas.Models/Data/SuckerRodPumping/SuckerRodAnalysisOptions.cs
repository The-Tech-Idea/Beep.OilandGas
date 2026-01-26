using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.SuckerRodPumping
{
    public class SuckerRodAnalysisOptions : ModelEntityBase
    {
        public decimal? RodDiameter { get; set; }
        public decimal? StrokesPerMinute { get; set; }
        public decimal? WellheadPressure { get; set; }
        public decimal? BottomHolePressure { get; set; }
        public decimal? GasOilRatio { get; set; }
    }
}
