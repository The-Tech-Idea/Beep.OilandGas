using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    /// <summary>
    /// DTO for enhanced recovery operation.
    /// </summary>
    public class EnhancedRecoveryOperation : ModelEntityBase
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
        public List<InjectionWell> InjectionWells { get; set; } = new();
    }

    /// <summary>
    /// DTO for injection operation.
    /// </summary>
    public class InjectionOperation : ModelEntityBase
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
    public class InjectionWell : ModelEntityBase
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
    public class WaterFlooding : ModelEntityBase
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
    public class GasInjection : ModelEntityBase
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
    public class CreateEnhancedRecoveryOperation : ModelEntityBase
    {
        public string FieldId { get; set; } = string.Empty;
        public string EORType { get; set; } = string.Empty;
        public DateTime? PlannedStartDate { get; set; }
        public decimal? PlannedInjectionRate { get; set; }
        public string? InjectionRateUnit { get; set; }
    }
}





