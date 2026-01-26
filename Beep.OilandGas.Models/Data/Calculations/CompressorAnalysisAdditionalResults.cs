using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class CompressorAnalysisAdditionalResults : ModelEntityBase
    {
        public decimal? CompressionRatio { get; set; }
        public decimal? HorsepowerPerStage { get; set; }
        public string? Notes { get; set; }
    }
}
