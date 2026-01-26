using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class GasLiftAnalysisOptions : ModelEntityBase
    {
        public decimal? GasInjectionPressure { get; set; }
        public int? NumberOfValves { get; set; }
        public bool? UseSiUnits { get; set; }
    }
}
