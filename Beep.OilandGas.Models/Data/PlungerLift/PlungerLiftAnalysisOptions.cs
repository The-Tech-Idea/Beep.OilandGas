using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PlungerLift
{
    public class PlungerLiftAnalysisOptions : ModelEntityBase
    {
        public decimal? CasingPressure { get; set; }
        public decimal? GasOilRatio { get; set; }
    }
}
