using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class CompressorAnalysisOptions : ModelEntityBase
    {
        /// <summary>Reciprocating cylinder bore (in); overrides facility defaults when set.</summary>
        public decimal? CylinderDiameter { get; set; }
        /// <summary>Reciprocating stroke (in); overrides facility defaults when set.</summary>
        public decimal? StrokeLength { get; set; }
        /// <summary>Reciprocating RPM; overrides facility defaults when set.</summary>
        public decimal? RotationalSpeed { get; set; }
        /// <summary>Centrifugal shaft speed (RPM); overrides facility defaults when set.</summary>
        public decimal? Speed { get; set; }
        /// <summary>Optional compressor kind when top-level <c>CompressorAnalysisRequest.CompressorType</c> is null or whitespace.</summary>
        public string? CompressorType { get; set; }
        /// <summary>Optional analysis mode when top-level <c>CompressorAnalysisRequest.AnalysisType</c> is null or whitespace.</summary>
        public string? AnalysisType { get; set; }
        /// <summary>HP cap for iterative <see cref="CompressorAnalysisWellKnown.AnalysisType.Pressure"/> search; default 1000 in orchestration.</summary>
        public decimal? MaxDriverHorsepower { get; set; }
    }
}
