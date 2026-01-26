using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class FlashCalculationAdditionalResults : ModelEntityBase
    {
        public decimal? Pressure { get; set; }
        public decimal? Temperature { get; set; }
        public int? ComponentCount { get; set; }
    }
}
