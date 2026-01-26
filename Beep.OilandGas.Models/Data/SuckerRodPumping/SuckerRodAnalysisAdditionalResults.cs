using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.SuckerRodPumping
{
    public class SuckerRodAnalysisAdditionalResults : ModelEntityBase
    {
        public decimal? RodStringLoad { get; set; }
        public decimal? PumpFillage { get; set; }
        public string? Notes { get; set; }
    }
}
