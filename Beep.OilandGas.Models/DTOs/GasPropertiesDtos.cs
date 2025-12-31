using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs
{
    /// <summary>
    /// DTO for gas composition.
    /// </summary>
    public class GasCompositionDto
    {
        public string CompositionId { get; set; } = string.Empty;
        public string CompositionName { get; set; } = string.Empty;
        public DateTime CompositionDate { get; set; }
        public List<GasComponentDto> Components { get; set; } = new();
        public decimal TotalMoleFraction { get; set; }
        public decimal MolecularWeight { get; set; }
        public decimal SpecificGravity { get; set; }
    }

    /// <summary>
    /// DTO for gas component.
    /// </summary>
    public class GasComponentDto
    {
        public string ComponentName { get; set; } = string.Empty;
        public decimal MoleFraction { get; set; }
        public decimal MolecularWeight { get; set; }
    }

    /// <summary>
    /// DTO for gas property calculation result.
    /// </summary>
    public class GasPropertyResultDto
    {
        public string CalculationId { get; set; } = string.Empty;
        public string CompositionId { get; set; } = string.Empty;
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal ZFactor { get; set; }
        public decimal Density { get; set; }
        public decimal FormationVolumeFactor { get; set; }
        public decimal Viscosity { get; set; }
        public decimal Compressibility { get; set; }
        public DateTime CalculationDate { get; set; }
        public string CorrelationMethod { get; set; } = string.Empty;
    }
}

