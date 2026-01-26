using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Pumps
{
    public class PumpAnalysisAdditionalResults : ModelEntityBase
    {
        public decimal? NpshMargin { get; set; }
        public string? Notes { get; set; }
    }
}
