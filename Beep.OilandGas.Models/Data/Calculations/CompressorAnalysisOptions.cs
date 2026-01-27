using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class CompressorAnalysisOptions : ModelEntityBase
    {
        public decimal? CylinderDiameter { get; set; }
        public decimal? StrokeLength { get; set; }
        public decimal? RotationalSpeed { get; set; }
        public decimal? Speed { get; set; }
        public string? CompressorType { get; set; }
        public string? AnalysisType { get; set; }
    }
}
