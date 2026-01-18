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

    /// <summary>
    /// DTO for gas viscosity analysis
    /// </summary>
    public class GasViscosityAnalysisDto
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string CompositionId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal Viscosity { get; set; }
        public decimal ViscosityAtSC { get; set; } // Standard conditions
        public decimal PressureCoefficient { get; set; }
        public decimal TemperatureCoefficient { get; set; }
        public string CorrelationMethod { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for gas compressibility analysis
    /// </summary>
    public class GasCompressibilityAnalysisDto
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string CompositionId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal IsothermalCompressibility { get; set; }
        public decimal AdiabaticCompressibility { get; set; }
        public decimal ZFactor { get; set; }
        public decimal CompressibilityFactor { get; set; }
    }

    /// <summary>
    /// DTO for virial equation analysis
    /// </summary>
    public class VirialCoefficientAnalysisDto
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string CompositionId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal SecondVirialCoefficient { get; set; } // B
        public decimal ThirdVirialCoefficient { get; set; } // C
        public decimal ReducedTemperature { get; set; }
        public decimal ReducedPressure { get; set; }
    }

    /// <summary>
    /// DTO for gas mixture analysis
    /// </summary>
    public class GasMixtureAnalysisDto
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string CompositionId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public decimal AverageMolecularWeight { get; set; }
        public decimal PseudoCriticalTemperature { get; set; }
        public decimal PseudoCriticalPressure { get; set; }
        public decimal ReducedTemperature { get; set; }
        public decimal ReducedPressure { get; set; }
        public List<MixtureComponentAnalysisDto> ComponentAnalysis { get; set; } = new();
    }

    /// <summary>
    /// DTO for mixture component properties
    /// </summary>
    public class MixtureComponentAnalysisDto
    {
        public string ComponentName { get; set; } = string.Empty;
        public decimal MoleFraction { get; set; }
        public decimal CriticalTemperature { get; set; }
        public decimal CriticalPressure { get; set; }
        public decimal AccentricityFactor { get; set; }
    }

    /// <summary>
    /// DTO for gas thermal conductivity analysis
    /// </summary>
    public class ThermalConductivityAnalysisDto
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string CompositionId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal ThermalConductivity { get; set; } // BTU/(hr·ft·°R)
        public decimal TemperatureDependence { get; set; }
        public decimal PressureDependence { get; set; }
        public string CorrelationMethod { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for gas solubility analysis
    /// </summary>
    public class GasSolubilityAnalysisDto
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string CompositionId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal Solubility { get; set; } // scf/stb
        public decimal SolubilityIndex { get; set; }
        public string Phase { get; set; } = string.Empty; // Oil-saturated, dry gas
    }

    /// <summary>
    /// DTO for pseudocritical property analysis
    /// </summary>
    public class PseudocriticalPropertyAnalysisDto
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string CompositionId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public decimal PseudoCriticalTemperature { get; set; }
        public decimal PseudoCriticalPressure { get; set; }
        public decimal PseudoReducedTemperature { get; set; }
        public decimal PseudoReducedPressure { get; set; }
        public decimal AccentricityFactor { get; set; }
        public List<ComponentContributionDto> ComponentContributions { get; set; } = new();
    }

    /// <summary>
    /// DTO for component contribution to pseudocritical properties
    /// </summary>
    public class ComponentContributionDto
    {
        public string ComponentName { get; set; } = string.Empty;
        public decimal MoleFraction { get; set; }
        public decimal CriticalTemperatureContribution { get; set; }
        public decimal CriticalPressureContribution { get; set; }
    }

    /// <summary>
    /// DTO for gas property correlation matrix
    /// </summary>
    public class GasPropertyMatrixDto
    {
        public string MatrixId { get; set; } = string.Empty;
        public string CompositionId { get; set; } = string.Empty;
        public DateTime GenerationDate { get; set; }
        public decimal MinPressure { get; set; }
        public decimal MaxPressure { get; set; }
        public decimal MinTemperature { get; set; }
        public decimal MaxTemperature { get; set; }
        public List<PropertyValueDto> PropertyValues { get; set; } = new();
    }

    /// <summary>
    /// DTO for individual property values in matrix
    /// </summary>
    public class PropertyValueDto
    {
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal ZFactor { get; set; }
        public decimal Density { get; set; }
        public decimal Viscosity { get; set; }
        public decimal ThermalConductivity { get; set; }
        public decimal CompressibilityFactor { get; set; }
    }
}




