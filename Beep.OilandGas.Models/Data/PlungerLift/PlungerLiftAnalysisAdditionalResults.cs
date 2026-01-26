using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PlungerLift
{
    public class PlungerLiftAnalysisAdditionalResults : ModelEntityBase
    {
        public decimal? CasingPressure { get; set; }
        public decimal? GasOilRatio { get; set; }
        public string? Notes { get; set; }
    }
}
