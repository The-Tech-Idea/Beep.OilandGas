using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.Measurement
{
    public class RecordManualMeasurementRequest : ModelEntityBase
    {
        public string WellId { get; set; }
        public string LeaseId { get; set; }
        public string TankId { get; set; }
        public decimal GaugeHeight { get; set; }
        public decimal Temperature { get; set; }
        public decimal BswSample { get; set; }
        public decimal? ApiGravity { get; set; }
        public string Operator { get; set; }
        public string Notes { get; set; }
    }

    public class RecordAutomaticMeasurementRequest : ModelEntityBase
    {
        public string WellId { get; set; }
        public string LeaseId { get; set; }
        public string MeterId { get; set; }
        public decimal MeterReading { get; set; }
        public decimal MeterFactor { get; set; }
        public decimal Temperature { get; set; }
        public decimal Bsw { get; set; }
        public decimal? ApiGravity { get; set; }
        public string MeasurementDevice { get; set; }
        public string Notes { get; set; }
    }

    public class MeasurementValidationResult : ModelEntityBase
    {
        public string MeasurementId { get; set; }
        public bool IsValid { get; set; }
        public List<string> ValidationErrors { get; set; } = new();
        public List<string> ValidationWarnings { get; set; } = new();
        public DateTime ValidationDate { get; set; } = DateTime.UtcNow;
        public string ValidatedBy { get; set; }
    }

    public class MeasurementHistory : ModelEntityBase
    {
        public string MeasurementId { get; set; }
        public DateTime MeasurementDateTime { get; set; }
        public string MeasurementMethod { get; set; }
        public decimal GrossVolume { get; set; }
        public decimal NetVolume { get; set; }
        public decimal? ApiGravity { get; set; }
        public string ValidationStatus { get; set; }
    }

    public class MeasurementSummary : ModelEntityBase
    {
        public string WellId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int MeasurementCount { get; set; }
        public decimal TotalGrossVolume { get; set; }
        public decimal TotalNetVolume { get; set; }
        public decimal AverageApiGravity { get; set; }
        public decimal AverageBsw { get; set; }
        public int ValidatedCount { get; set; }
        public int PendingValidationCount { get; set; }
    }
}





