using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class FlashCalculationOptions : ModelEntityBase
    {
        public int? MaxIterations { get; set; }
        public decimal? ConvergenceTolerance { get; set; }
        /// <summary>Wire EOS label (e.g. PR, SRK, SRK_MODIFIED, IDEAL_K, or friendly text). Normalized to <c>FLASH_EOS_MODEL</c> codes in FlashCalculations / LifeCycle.</summary>
        public string? EquationOfState { get; set; }
    }
}
