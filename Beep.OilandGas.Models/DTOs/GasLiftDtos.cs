using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs
{
    /// <summary>
    /// DTO for gas lift design.
    /// </summary>
    public class GasLiftDesignDto
    {
        public string DesignId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public DateTime DesignDate { get; set; }
        public decimal GasInjectionPressure { get; set; }
        public int NumberOfValves { get; set; }
        public List<GasLiftValveDto> Valves { get; set; } = new();
        public decimal TotalGasInjectionRate { get; set; }
        public decimal ExpectedProductionRate { get; set; }
        public decimal SystemEfficiency { get; set; }
        public string? Status { get; set; }
    }

    /// <summary>
    /// DTO for gas lift valve.
    /// </summary>
    public class GasLiftValveDto
    {
        public string ValveId { get; set; } = string.Empty;
        public decimal Depth { get; set; }
        public decimal PortSize { get; set; }
        public decimal OpeningPressure { get; set; }
        public decimal ClosingPressure { get; set; }
        public decimal GasInjectionRate { get; set; }
    }

    /// <summary>
    /// DTO for gas lift performance.
    /// </summary>
    public class GasLiftPerformanceDto
    {
        public string WellUWI { get; set; } = string.Empty;
        public DateTime PerformanceDate { get; set; }
        public decimal GasInjectionRate { get; set; }
        public decimal ProductionRate { get; set; }
        public decimal GasLiquidRatio { get; set; }
        public decimal Efficiency { get; set; }
        public string? Status { get; set; }
    }
}




