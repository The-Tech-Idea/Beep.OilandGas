using System;

namespace Beep.OilandGas.Models.Data.EnhancedRecovery
{
    public class EnhancedRecoveryAnalysis
    {
        public string FieldId { get; set; } = string.Empty;
        public string Method { get; set; } = "WaterInjection"; // WaterInjection, GasInjection, etc.
        public decimal CurrentRecoveryFactor { get; set; }
        public decimal TargetRecoveryFactor { get; set; }
        public DateTime AnalyzedDate { get; set; }
        public string Remarks { get; set; } = string.Empty;
    }
}
