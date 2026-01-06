using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.DTOs.ProductionAccounting
{
    /// <summary>
    /// DTO for measurement record
    /// </summary>
    public class MeasurementDto
    {
        public string MeasurementId { get; set; } = string.Empty;
        public DateTime MeasurementDateTime { get; set; }
        public string Method { get; set; } = string.Empty;
        public string Standard { get; set; } = string.Empty;
        public decimal GrossVolume { get; set; }
        public decimal BSW { get; set; }
        public decimal NetVolume { get; set; }
        public decimal Temperature { get; set; }
        public decimal? Pressure { get; set; }
        public decimal? ApiGravity { get; set; }
    }

    /// <summary>
    /// Request to create a manual measurement
    /// </summary>
    public class CreateManualMeasurementRequest
    {
        [Required]
        public string LeaseId { get; set; } = string.Empty;
        public string? WellId { get; set; }
        public string? TankBatteryId { get; set; }
        [Required]
        public decimal GrossVolume { get; set; }
        [Range(0, 100)]
        public decimal BSW { get; set; }
        public decimal Temperature { get; set; } = 60m;
        public decimal? Pressure { get; set; }
        public decimal? ApiGravity { get; set; }
        public DateTime? MeasurementDateTime { get; set; }
    }

    /// <summary>
    /// Request to create an automatic measurement
    /// </summary>
    public class CreateAutomaticMeasurementRequest
    {
        [Required]
        public string LeaseId { get; set; } = string.Empty;
        public string? WellId { get; set; }
        public string? TankBatteryId { get; set; }
        [Required]
        public decimal GrossVolume { get; set; }
        [Range(0, 100)]
        public decimal BSW { get; set; }
        public decimal Temperature { get; set; } = 60m;
        public decimal? Pressure { get; set; }
        public decimal? ApiGravity { get; set; }
        public DateTime? MeasurementDateTime { get; set; }
        public string? MeterId { get; set; }
    }
}




