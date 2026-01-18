#nullable enable

namespace Beep.OilandGas.Models.Models.Measurement
{
    /// <summary>
    /// Represents measurement accuracy standards.
    /// </summary>
    public class MeasurementAccuracy
    {
        public string? AccuracyId { get; set; }
        public string? AccuracyLevel { get; set; }
        public decimal? TolerancePercentage { get; set; }
        public string? MeasurementType { get; set; }
        public string? Standard { get; set; }
    }
}
