using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class FlashCalculationOptions : ModelEntityBase
    {
        public int? MaxIterations { get; set; }
        public decimal? ConvergenceTolerance { get; set; }
        public string? EquationOfState { get; set; }
    }
}
