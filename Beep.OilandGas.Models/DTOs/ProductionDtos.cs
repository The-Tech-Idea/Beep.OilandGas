using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs
{
    /// <summary>
    /// DTO for production operation.
    /// </summary>
    public class ProductionOperationDto
    {
        public string OperationId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public string WellName { get; set; } = string.Empty;
        public DateTime OperationDate { get; set; }
        public decimal? OilProduction { get; set; }
        public decimal? GasProduction { get; set; }
        public decimal? WaterProduction { get; set; }
        public string? ProductionUnit { get; set; }
        public decimal? FlowingPressure { get; set; }
        public decimal? FlowingTemperature { get; set; }
        public string? Status { get; set; }
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// DTO for production report.
    /// </summary>
    public class ProductionReportDto
    {
        public string ReportId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public DateTime ReportDate { get; set; }
        public decimal? OilProduction { get; set; }
        public decimal? GasProduction { get; set; }
        public decimal? WaterProduction { get; set; }
        public string? ProductionUnit { get; set; }
        public decimal? OperatingHours { get; set; }
        public decimal? DowntimeHours { get; set; }
        public string? DowntimeReason { get; set; }
        public string? Status { get; set; }
    }

    /// <summary>
    /// DTO for well operation.
    /// </summary>
    public class WellOperationDto
    {
        public string OperationId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public string OperationType { get; set; } = string.Empty;
        public DateTime OperationDate { get; set; }
        public string? Status { get; set; }
        public string? Description { get; set; }
        public decimal? Cost { get; set; }
        public string? Currency { get; set; }
        public string? Contractor { get; set; }
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// DTO for facility operation.
    /// </summary>
    public class FacilityOperationDto
    {
        public string OperationId { get; set; } = string.Empty;
        public string FacilityId { get; set; } = string.Empty;
        public string FacilityName { get; set; } = string.Empty;
        public DateTime OperationDate { get; set; }
        public string? OperationType { get; set; }
        public string? Status { get; set; }
        public decimal? Throughput { get; set; }
        public string? ThroughputUnit { get; set; }
        public decimal? Efficiency { get; set; }
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// DTO for creating a production operation.
    /// </summary>
    public class CreateProductionOperationDto
    {
        public string WellUWI { get; set; } = string.Empty;
        public DateTime OperationDate { get; set; }
        public decimal? OilProduction { get; set; }
        public decimal? GasProduction { get; set; }
        public decimal? WaterProduction { get; set; }
        public string? ProductionUnit { get; set; }
    }
}

