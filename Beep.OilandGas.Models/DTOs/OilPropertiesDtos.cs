using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs
{
    /// <summary>
    /// DTO for oil composition.
    /// </summary>
    public class OilCompositionDto
    {
        public string CompositionId { get; set; } = string.Empty;
        public string CompositionName { get; set; } = string.Empty;
        public DateTime CompositionDate { get; set; }
        public decimal OilGravity { get; set; } // API gravity
        public decimal GasOilRatio { get; set; } // scf/stb
        public decimal WaterCut { get; set; } // fraction
        public decimal BubblePointPressure { get; set; } // psia
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// DTO for oil property calculation result.
    /// </summary>
    public class OilPropertyResultDto
    {
        public string CalculationId { get; set; } = string.Empty;
        public string CompositionId { get; set; } = string.Empty;
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal FormationVolumeFactor { get; set; }
        public decimal Density { get; set; }
        public decimal Viscosity { get; set; }
        public decimal Compressibility { get; set; }
        public DateTime CalculationDate { get; set; }
        public string CorrelationMethod { get; set; } = string.Empty;
    }
}

