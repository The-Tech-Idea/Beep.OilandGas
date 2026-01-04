using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

    /// <summary>
    /// Request for calculating Formation Volume Factor (FVF)
    /// </summary>
    public class CalculateFVFRequest
    {
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Pressure must be greater than or equal to 0")]
        public decimal Pressure { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Temperature must be greater than or equal to 0")]
        public decimal Temperature { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "GasOilRatio must be greater than or equal to 0")]
        public decimal GasOilRatio { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "OilGravity must be between 0 and 100")]
        public decimal OilGravity { get; set; }

        public string Correlation { get; set; } = "Standing";
    }

    /// <summary>
    /// Request for calculating oil density
    /// </summary>
    public class CalculateDensityRequest
    {
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Pressure must be greater than or equal to 0")]
        public decimal Pressure { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Temperature must be greater than or equal to 0")]
        public decimal Temperature { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "OilGravity must be between 0 and 100")]
        public decimal OilGravity { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "GasOilRatio must be greater than or equal to 0")]
        public decimal GasOilRatio { get; set; }
    }

    /// <summary>
    /// Request for calculating oil viscosity
    /// </summary>
    public class CalculateViscosityRequest
    {
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Pressure must be greater than or equal to 0")]
        public decimal Pressure { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Temperature must be greater than or equal to 0")]
        public decimal Temperature { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "OilGravity must be between 0 and 100")]
        public decimal OilGravity { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "GasOilRatio must be greater than or equal to 0")]
        public decimal GasOilRatio { get; set; }
    }

    /// <summary>
    /// Request for calculating comprehensive oil properties
    /// </summary>
    public class CalculateOilPropertiesRequest
    {
        [Required(ErrorMessage = "Composition is required")]
        public OilCompositionDto Composition { get; set; } = null!;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Pressure must be greater than or equal to 0")]
        public decimal Pressure { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Temperature must be greater than or equal to 0")]
        public decimal Temperature { get; set; }
    }
}

