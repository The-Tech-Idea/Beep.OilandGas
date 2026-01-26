using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ChokeAnalysisAdditionalResults : ModelEntityBase
    {
        public decimal? RequiredChokeSize { get; set; }
        public string? Notes { get; set; }
    }
}
