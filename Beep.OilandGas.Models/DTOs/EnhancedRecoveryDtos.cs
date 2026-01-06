using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs
{
    /// <summary>
    /// DTO for enhanced recovery operation.
    /// </summary>
    public class EnhancedRecoveryOperationDto
    {
        public string OperationId { get; set; } = string.Empty;
        public string FieldId { get; set; } = string.Empty;
        public string EORType { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public string? Status { get; set; }
        public decimal? InjectionRate { get; set; }
        public string? InjectionRateUnit { get; set; }
        public decimal? ProductionIncrease { get; set; }
        public string? ProductionUnit { get; set; }
        public decimal? Efficiency { get; set; }
        public string? Remarks { get; set; }
        public List<InjectionWellDto> InjectionWells { get; set; } = new();
    }

    /// <summary>
    /// DTO for injection operation.
    /// </summary>
    public class InjectionOperationDto
    {
        public string OperationId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public string InjectionType { get; set; } = string.Empty;
        public DateTime OperationDate { get; set; }
        public decimal? InjectionRate { get; set; }
        public string? InjectionRateUnit { get; set; }
        public decimal? InjectionPressure { get; set; }
        public string? PressureUnit { get; set; }
        public decimal? CumulativeInjection { get; set; }
        public string? CumulativeUnit { get; set; }
        public string? Status { get; set; }
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// DTO for injection well.
    /// </summary>
    public class InjectionWellDto
    {
        public string WellUWI { get; set; } = string.Empty;
        public string WellName { get; set; } = string.Empty;
        public string InjectionType { get; set; } = string.Empty;
        public string? InjectionZone { get; set; }
        public decimal? InjectionRate { get; set; }
        public string? InjectionRateUnit { get; set; }
        public string? Status { get; set; }
    }

    /// <summary>
    /// DTO for water flooding operation.
    /// </summary>
    public class WaterFloodingDto
    {
        public string OperationId { get; set; } = string.Empty;
        public string FieldId { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public string? Status { get; set; }
        public decimal? WaterInjectionRate { get; set; }
        public string? InjectionRateUnit { get; set; }
        public string? WaterSource { get; set; }
        public decimal? ProductionIncrease { get; set; }
        public string? ProductionUnit { get; set; }
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// DTO for gas injection operation.
    /// </summary>
    public class GasInjectionDto
    {
        public string OperationId { get; set; } = string.Empty;
        public string FieldId { get; set; } = string.Empty;
        public string GasType { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public string? Status { get; set; }
        public decimal? GasInjectionRate { get; set; }
        public string? InjectionRateUnit { get; set; }
        public decimal? ProductionIncrease { get; set; }
        public string? ProductionUnit { get; set; }
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// DTO for creating an enhanced recovery operation.
    /// </summary>
    public class CreateEnhancedRecoveryOperationDto
    {
        public string FieldId { get; set; } = string.Empty;
        public string EORType { get; set; } = string.Empty;
        public DateTime? PlannedStartDate { get; set; }
        public decimal? PlannedInjectionRate { get; set; }
        public string? InjectionRateUnit { get; set; }
    }
}




