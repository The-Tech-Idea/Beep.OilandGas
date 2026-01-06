using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs
{
    /// <summary>
    /// DTO for hydraulic pump design.
    /// </summary>
    public class HydraulicPumpDesignDto
    {
        public string DesignId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public string PumpType { get; set; } = string.Empty;
        public DateTime DesignDate { get; set; }
        public decimal PumpDepth { get; set; }
        public decimal PumpSize { get; set; }
        public decimal OperatingPressure { get; set; }
        public decimal FlowRate { get; set; }
        public decimal Efficiency { get; set; }
        public string? Status { get; set; }
    }

    /// <summary>
    /// DTO for pump performance analysis result.
    /// </summary>
    public class PumpPerformanceAnalysisDto
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string PumpId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public decimal CurrentFlowRate { get; set; }
        public decimal CurrentEfficiency { get; set; }
        public decimal RecommendedFlowRate { get; set; }
        public List<string> Recommendations { get; set; } = new();
        public string? Status { get; set; }
    }

    /// <summary>
    /// DTO for pump performance history point.
    /// </summary>
    public class PumpPerformanceHistoryDto
    {
        public DateTime PerformanceDate { get; set; }
        public decimal FlowRate { get; set; }
        public decimal Efficiency { get; set; }
        public decimal PowerConsumption { get; set; }
        public string? Status { get; set; }
    }
}




