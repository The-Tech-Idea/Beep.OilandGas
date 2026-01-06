using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

    /// <summary>
    /// Request for calculating Z-factor
    /// </summary>
    public class CalculateZFactorRequest
    {
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Pressure must be greater than or equal to 0")]
        public decimal Pressure { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Temperature must be greater than or equal to 0")]
        public decimal Temperature { get; set; }

        [Required]
        [Range(0, 2, ErrorMessage = "SpecificGravity must be between 0 and 2")]
        public decimal SpecificGravity { get; set; }

        public string Correlation { get; set; } = "Standing-Katz";
    }

    /// <summary>
    /// Request for calculating gas density
    /// </summary>
    public class CalculateGasDensityRequest
    {
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Pressure must be greater than or equal to 0")]
        public decimal Pressure { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Temperature must be greater than or equal to 0")]
        public decimal Temperature { get; set; }

        [Required]
        [Range(0, 2, ErrorMessage = "ZFactor must be between 0 and 2")]
        public decimal ZFactor { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "MolecularWeight must be greater than or equal to 0")]
        public decimal MolecularWeight { get; set; }
    }

    /// <summary>
    /// Request for calculating gas Formation Volume Factor (FVF)
    /// </summary>
    public class CalculateGasFVFRequest
    {
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Pressure must be greater than or equal to 0")]
        public decimal Pressure { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Temperature must be greater than or equal to 0")]
        public decimal Temperature { get; set; }

        [Required]
        [Range(0, 2, ErrorMessage = "ZFactor must be between 0 and 2")]
        public decimal ZFactor { get; set; }
    }
}




