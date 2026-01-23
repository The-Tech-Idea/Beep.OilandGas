using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data
{
    /// <summary>
    /// DTO for gas composition.
    /// </summary>
    public class GasComposition : ModelEntityBase
    {
        private string CompositionIdValue = string.Empty;

        public string CompositionId

        {

            get { return this.CompositionIdValue; }

            set { SetProperty(ref CompositionIdValue, value); }

        }
        private string CompositionNameValue = string.Empty;

        public string CompositionName

        {

            get { return this.CompositionNameValue; }

            set { SetProperty(ref CompositionNameValue, value); }

        }
        private DateTime CompositionDateValue;

        public DateTime CompositionDate

        {

            get { return this.CompositionDateValue; }

            set { SetProperty(ref CompositionDateValue, value); }

        }
        private List<GasComponent> ComponentsValue = new();

        public List<GasComponent> Components

        {

            get { return this.ComponentsValue; }

            set { SetProperty(ref ComponentsValue, value); }

        }
        private decimal TotalMoleFractionValue;

        public decimal TotalMoleFraction

        {

            get { return this.TotalMoleFractionValue; }

            set { SetProperty(ref TotalMoleFractionValue, value); }

        }
        private decimal MolecularWeightValue;

        public decimal MolecularWeight

        {

            get { return this.MolecularWeightValue; }

            set { SetProperty(ref MolecularWeightValue, value); }

        }
        private decimal SpecificGravityValue;

        public decimal SpecificGravity

        {

            get { return this.SpecificGravityValue; }

            set { SetProperty(ref SpecificGravityValue, value); }

        }
    }

    /// <summary>
    /// DTO for gas component.
    /// </summary>
    public class GasComponent : ModelEntityBase
    {
        private string ComponentNameValue = string.Empty;

        public string ComponentName

        {

            get { return this.ComponentNameValue; }

            set { SetProperty(ref ComponentNameValue, value); }

        }
        private decimal MoleFractionValue;

        public decimal MoleFraction

        {

            get { return this.MoleFractionValue; }

            set { SetProperty(ref MoleFractionValue, value); }

        }
        private decimal MolecularWeightValue;

        public decimal MolecularWeight

        {

            get { return this.MolecularWeightValue; }

            set { SetProperty(ref MolecularWeightValue, value); }

        }
    }

    /// <summary>
    /// DTO for gas property calculation result.
    /// </summary>
    public class GasPropertyResult : ModelEntityBase
    {
        private string CalculationIdValue = string.Empty;

        public string CalculationId

        {

            get { return this.CalculationIdValue; }

            set { SetProperty(ref CalculationIdValue, value); }

        }
        private string CompositionIdValue = string.Empty;

        public string CompositionId

        {

            get { return this.CompositionIdValue; }

            set { SetProperty(ref CompositionIdValue, value); }

        }
        private decimal PressureValue;

        public decimal Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }
        private decimal TemperatureValue;

        public decimal Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        }
        private decimal ZFactorValue;

        public decimal ZFactor

        {

            get { return this.ZFactorValue; }

            set { SetProperty(ref ZFactorValue, value); }

        }
        private decimal DensityValue;

        public decimal Density

        {

            get { return this.DensityValue; }

            set { SetProperty(ref DensityValue, value); }

        }
        private decimal FormationVolumeFactorValue;

        public decimal FormationVolumeFactor

        {

            get { return this.FormationVolumeFactorValue; }

            set { SetProperty(ref FormationVolumeFactorValue, value); }

        }
        private decimal ViscosityValue;

        public decimal Viscosity

        {

            get { return this.ViscosityValue; }

            set { SetProperty(ref ViscosityValue, value); }

        }
        private decimal CompressibilityValue;

        public decimal Compressibility

        {

            get { return this.CompressibilityValue; }

            set { SetProperty(ref CompressibilityValue, value); }

        }
        private DateTime CalculationDateValue;

        public DateTime CalculationDate

        {

            get { return this.CalculationDateValue; }

            set { SetProperty(ref CalculationDateValue, value); }

        }
        private string CorrelationMethodValue = string.Empty;

        public string CorrelationMethod

        {

            get { return this.CorrelationMethodValue; }

            set { SetProperty(ref CorrelationMethodValue, value); }

        }
    }

    /// <summary>
    /// Request for calculating Z-factor
    /// </summary>
    public class CalculateZFactorRequest : ModelEntityBase
    {
        private decimal PressureValue;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Pressure must be greater than or equal to 0")]
        public decimal Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }

        private decimal TemperatureValue;


        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Temperature must be greater than or equal to 0")]
        public decimal Temperature


        {


            get { return this.TemperatureValue; }


            set { SetProperty(ref TemperatureValue, value); }


        }

        private decimal SpecificGravityValue;


        [Required]
        [Range(0, 2, ErrorMessage = "SpecificGravity must be between 0 and 2")]
        public decimal SpecificGravity


        {


            get { return this.SpecificGravityValue; }


            set { SetProperty(ref SpecificGravityValue, value); }


        }

        private string CorrelationValue = "Standing-Katz";


        public string Correlation


        {


            get { return this.CorrelationValue; }


            set { SetProperty(ref CorrelationValue, value); }


        }
    }

    /// <summary>
    /// Request for calculating gas density
    /// </summary>
    public class CalculateGasDensityRequest : ModelEntityBase
    {
        private decimal PressureValue;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Pressure must be greater than or equal to 0")]
        public decimal Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }

        private decimal TemperatureValue;


        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Temperature must be greater than or equal to 0")]
        public decimal Temperature


        {


            get { return this.TemperatureValue; }


            set { SetProperty(ref TemperatureValue, value); }


        }

        private decimal ZFactorValue;


        [Required]
        [Range(0, 2, ErrorMessage = "ZFactor must be between 0 and 2")]
        public decimal ZFactor


        {


            get { return this.ZFactorValue; }


            set { SetProperty(ref ZFactorValue, value); }


        }

        private decimal MolecularWeightValue;


        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "MolecularWeight must be greater than or equal to 0")]
        public decimal MolecularWeight


        {


            get { return this.MolecularWeightValue; }


            set { SetProperty(ref MolecularWeightValue, value); }


        }
    }

    /// <summary>
    /// Request for calculating gas Formation Volume Factor (FVF)
    /// </summary>
    public class CalculateGasFVFRequest : ModelEntityBase
    {
        private decimal PressureValue;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Pressure must be greater than or equal to 0")]
        public decimal Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }

        private decimal TemperatureValue;


        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Temperature must be greater than or equal to 0")]
        public decimal Temperature


        {


            get { return this.TemperatureValue; }


            set { SetProperty(ref TemperatureValue, value); }


        }

        private decimal ZFactorValue;


        [Required]
        [Range(0, 2, ErrorMessage = "ZFactor must be between 0 and 2")]
        public decimal ZFactor


        {


            get { return this.ZFactorValue; }


            set { SetProperty(ref ZFactorValue, value); }


        }
    }

    /// <summary>
    /// DTO for gas viscosity analysis
    /// </summary>
    public class GasViscosityAnalysis : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private string CompositionIdValue = string.Empty;

        public string CompositionId

        {

            get { return this.CompositionIdValue; }

            set { SetProperty(ref CompositionIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private decimal PressureValue;

        public decimal Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }
        private decimal TemperatureValue;

        public decimal Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        }
        private decimal ViscosityValue;

        public decimal Viscosity

        {

            get { return this.ViscosityValue; }

            set { SetProperty(ref ViscosityValue, value); }

        }
        private decimal ViscosityAtSCValue;

        public decimal ViscosityAtSC

        {

            get { return this.ViscosityAtSCValue; }

            set { SetProperty(ref ViscosityAtSCValue, value); }

        } // Standard conditions
        private decimal PressureCoefficientValue;

        public decimal PressureCoefficient

        {

            get { return this.PressureCoefficientValue; }

            set { SetProperty(ref PressureCoefficientValue, value); }

        }
        private decimal TemperatureCoefficientValue;

        public decimal TemperatureCoefficient

        {

            get { return this.TemperatureCoefficientValue; }

            set { SetProperty(ref TemperatureCoefficientValue, value); }

        }
        private string CorrelationMethodValue = string.Empty;

        public string CorrelationMethod

        {

            get { return this.CorrelationMethodValue; }

            set { SetProperty(ref CorrelationMethodValue, value); }

        }
    }

    /// <summary>
    /// DTO for gas compressibility analysis
    /// </summary>
    public class GasCompressibilityAnalysis : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private string CompositionIdValue = string.Empty;

        public string CompositionId

        {

            get { return this.CompositionIdValue; }

            set { SetProperty(ref CompositionIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private decimal PressureValue;

        public decimal Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }
        private decimal TemperatureValue;

        public decimal Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        }
        private decimal IsothermalCompressibilityValue;

        public decimal IsothermalCompressibility

        {

            get { return this.IsothermalCompressibilityValue; }

            set { SetProperty(ref IsothermalCompressibilityValue, value); }

        }
        private decimal AdiabaticCompressibilityValue;

        public decimal AdiabaticCompressibility

        {

            get { return this.AdiabaticCompressibilityValue; }

            set { SetProperty(ref AdiabaticCompressibilityValue, value); }

        }
        private decimal ZFactorValue;

        public decimal ZFactor

        {

            get { return this.ZFactorValue; }

            set { SetProperty(ref ZFactorValue, value); }

        }
        private decimal CompressibilityFactorValue;

        public decimal CompressibilityFactor

        {

            get { return this.CompressibilityFactorValue; }

            set { SetProperty(ref CompressibilityFactorValue, value); }

        }
    }

    /// <summary>
    /// DTO for virial equation analysis
    /// </summary>
    public class VirialCoefficientAnalysis : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private string CompositionIdValue = string.Empty;

        public string CompositionId

        {

            get { return this.CompositionIdValue; }

            set { SetProperty(ref CompositionIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private decimal PressureValue;

        public decimal Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }
        private decimal TemperatureValue;

        public decimal Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        }
        private decimal SecondVirialCoefficientValue;

        public decimal SecondVirialCoefficient

        {

            get { return this.SecondVirialCoefficientValue; }

            set { SetProperty(ref SecondVirialCoefficientValue, value); }

        } // B
        private decimal ThirdVirialCoefficientValue;

        public decimal ThirdVirialCoefficient

        {

            get { return this.ThirdVirialCoefficientValue; }

            set { SetProperty(ref ThirdVirialCoefficientValue, value); }

        } // C
        private decimal ReducedTemperatureValue;

        public decimal ReducedTemperature

        {

            get { return this.ReducedTemperatureValue; }

            set { SetProperty(ref ReducedTemperatureValue, value); }

        }
        private decimal ReducedPressureValue;

        public decimal ReducedPressure

        {

            get { return this.ReducedPressureValue; }

            set { SetProperty(ref ReducedPressureValue, value); }

        }
    }

    /// <summary>
    /// DTO for gas mixture analysis
    /// </summary>
    public class GasMixtureAnalysis : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private string CompositionIdValue = string.Empty;

        public string CompositionId

        {

            get { return this.CompositionIdValue; }

            set { SetProperty(ref CompositionIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private decimal AverageMolecularWeightValue;

        public decimal AverageMolecularWeight

        {

            get { return this.AverageMolecularWeightValue; }

            set { SetProperty(ref AverageMolecularWeightValue, value); }

        }
        private decimal PseudoCriticalTemperatureValue;

        public decimal PseudoCriticalTemperature

        {

            get { return this.PseudoCriticalTemperatureValue; }

            set { SetProperty(ref PseudoCriticalTemperatureValue, value); }

        }
        private decimal PseudoCriticalPressureValue;

        public decimal PseudoCriticalPressure

        {

            get { return this.PseudoCriticalPressureValue; }

            set { SetProperty(ref PseudoCriticalPressureValue, value); }

        }
        private decimal ReducedTemperatureValue;

        public decimal ReducedTemperature

        {

            get { return this.ReducedTemperatureValue; }

            set { SetProperty(ref ReducedTemperatureValue, value); }

        }
        private decimal ReducedPressureValue;

        public decimal ReducedPressure

        {

            get { return this.ReducedPressureValue; }

            set { SetProperty(ref ReducedPressureValue, value); }

        }
        private List<MixtureComponentAnalysis> ComponentAnalysisValue = new();

        public List<MixtureComponentAnalysis> ComponentAnalysis

        {

            get { return this.ComponentAnalysisValue; }

            set { SetProperty(ref ComponentAnalysisValue, value); }

        }
    }

    /// <summary>
    /// DTO for mixture component properties
    /// </summary>
    public class MixtureComponentAnalysis : ModelEntityBase
    {
        private string ComponentNameValue = string.Empty;

        public string ComponentName

        {

            get { return this.ComponentNameValue; }

            set { SetProperty(ref ComponentNameValue, value); }

        }
        private decimal MoleFractionValue;

        public decimal MoleFraction

        {

            get { return this.MoleFractionValue; }

            set { SetProperty(ref MoleFractionValue, value); }

        }
        private decimal CriticalTemperatureValue;

        public decimal CriticalTemperature

        {

            get { return this.CriticalTemperatureValue; }

            set { SetProperty(ref CriticalTemperatureValue, value); }

        }
        private decimal CriticalPressureValue;

        public decimal CriticalPressure

        {

            get { return this.CriticalPressureValue; }

            set { SetProperty(ref CriticalPressureValue, value); }

        }
        private decimal AccentricityFactorValue;

        public decimal AccentricityFactor

        {

            get { return this.AccentricityFactorValue; }

            set { SetProperty(ref AccentricityFactorValue, value); }

        }
    }

    /// <summary>
    /// DTO for gas thermal conductivity analysis
    /// </summary>
    public class ThermalConductivityAnalysis : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private string CompositionIdValue = string.Empty;

        public string CompositionId

        {

            get { return this.CompositionIdValue; }

            set { SetProperty(ref CompositionIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private decimal PressureValue;

        public decimal Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }
        private decimal TemperatureValue;

        public decimal Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        }
        private decimal ThermalConductivityValue;

        public decimal ThermalConductivity

        {

            get { return this.ThermalConductivityValue; }

            set { SetProperty(ref ThermalConductivityValue, value); }

        } // BTU/(hr·ft·°R)
        private decimal TemperatureDependenceValue;

        public decimal TemperatureDependence

        {

            get { return this.TemperatureDependenceValue; }

            set { SetProperty(ref TemperatureDependenceValue, value); }

        }
        private decimal PressureDependenceValue;

        public decimal PressureDependence

        {

            get { return this.PressureDependenceValue; }

            set { SetProperty(ref PressureDependenceValue, value); }

        }
        private string CorrelationMethodValue = string.Empty;

        public string CorrelationMethod

        {

            get { return this.CorrelationMethodValue; }

            set { SetProperty(ref CorrelationMethodValue, value); }

        }
    }

    /// <summary>
    /// DTO for gas solubility analysis
    /// </summary>
    public class GasSolubilityAnalysis : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private string CompositionIdValue = string.Empty;

        public string CompositionId

        {

            get { return this.CompositionIdValue; }

            set { SetProperty(ref CompositionIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private decimal PressureValue;

        public decimal Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }
        private decimal TemperatureValue;

        public decimal Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        }
        private decimal SolubilityValue;

        public decimal Solubility

        {

            get { return this.SolubilityValue; }

            set { SetProperty(ref SolubilityValue, value); }

        } // scf/stb
        private decimal SolubilityIndexValue;

        public decimal SolubilityIndex

        {

            get { return this.SolubilityIndexValue; }

            set { SetProperty(ref SolubilityIndexValue, value); }

        }
        private string PhaseValue = string.Empty;

        public string Phase

        {

            get { return this.PhaseValue; }

            set { SetProperty(ref PhaseValue, value); }

        } // Oil-saturated, dry gas
    }

    /// <summary>
    /// DTO for pseudocritical property analysis
    /// </summary>
    public class PseudocriticalPropertyAnalysis : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private string CompositionIdValue = string.Empty;

        public string CompositionId

        {

            get { return this.CompositionIdValue; }

            set { SetProperty(ref CompositionIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private decimal PseudoCriticalTemperatureValue;

        public decimal PseudoCriticalTemperature

        {

            get { return this.PseudoCriticalTemperatureValue; }

            set { SetProperty(ref PseudoCriticalTemperatureValue, value); }

        }
        private decimal PseudoCriticalPressureValue;

        public decimal PseudoCriticalPressure

        {

            get { return this.PseudoCriticalPressureValue; }

            set { SetProperty(ref PseudoCriticalPressureValue, value); }

        }
        private decimal PseudoReducedTemperatureValue;

        public decimal PseudoReducedTemperature

        {

            get { return this.PseudoReducedTemperatureValue; }

            set { SetProperty(ref PseudoReducedTemperatureValue, value); }

        }
        private decimal PseudoReducedPressureValue;

        public decimal PseudoReducedPressure

        {

            get { return this.PseudoReducedPressureValue; }

            set { SetProperty(ref PseudoReducedPressureValue, value); }

        }
        private decimal AccentricityFactorValue;

        public decimal AccentricityFactor

        {

            get { return this.AccentricityFactorValue; }

            set { SetProperty(ref AccentricityFactorValue, value); }

        }
        private List<ComponentContribution> ComponentContributionsValue = new();

        public List<ComponentContribution> ComponentContributions

        {

            get { return this.ComponentContributionsValue; }

            set { SetProperty(ref ComponentContributionsValue, value); }

        }
    }

    /// <summary>
    /// DTO for component contribution to pseudocritical properties
    /// </summary>
    public class ComponentContribution : ModelEntityBase
    {
        private string ComponentNameValue = string.Empty;

        public string ComponentName

        {

            get { return this.ComponentNameValue; }

            set { SetProperty(ref ComponentNameValue, value); }

        }
        private decimal MoleFractionValue;

        public decimal MoleFraction

        {

            get { return this.MoleFractionValue; }

            set { SetProperty(ref MoleFractionValue, value); }

        }
        private decimal CriticalTemperatureContributionValue;

        public decimal CriticalTemperatureContribution

        {

            get { return this.CriticalTemperatureContributionValue; }

            set { SetProperty(ref CriticalTemperatureContributionValue, value); }

        }
        private decimal CriticalPressureContributionValue;

        public decimal CriticalPressureContribution

        {

            get { return this.CriticalPressureContributionValue; }

            set { SetProperty(ref CriticalPressureContributionValue, value); }

        }
    }

    /// <summary>
    /// DTO for gas property correlation matrix
    /// </summary>
    public class GasPropertyMatrix : ModelEntityBase
    {
        private string MatrixIdValue = string.Empty;

        public string MatrixId

        {

            get { return this.MatrixIdValue; }

            set { SetProperty(ref MatrixIdValue, value); }

        }
        private string CompositionIdValue = string.Empty;

        public string CompositionId

        {

            get { return this.CompositionIdValue; }

            set { SetProperty(ref CompositionIdValue, value); }

        }
        private DateTime GenerationDateValue;

        public DateTime GenerationDate

        {

            get { return this.GenerationDateValue; }

            set { SetProperty(ref GenerationDateValue, value); }

        }
        private decimal MinPressureValue;

        public decimal MinPressure

        {

            get { return this.MinPressureValue; }

            set { SetProperty(ref MinPressureValue, value); }

        }
        private decimal MaxPressureValue;

        public decimal MaxPressure

        {

            get { return this.MaxPressureValue; }

            set { SetProperty(ref MaxPressureValue, value); }

        }
        private decimal MinTemperatureValue;

        public decimal MinTemperature

        {

            get { return this.MinTemperatureValue; }

            set { SetProperty(ref MinTemperatureValue, value); }

        }
        private decimal MaxTemperatureValue;

        public decimal MaxTemperature

        {

            get { return this.MaxTemperatureValue; }

            set { SetProperty(ref MaxTemperatureValue, value); }

        }
        private List<PropertyValue> PropertyValuesValue = new();

        public List<PropertyValue> PropertyValues

        {

            get { return this.PropertyValuesValue; }

            set { SetProperty(ref PropertyValuesValue, value); }

        }
    }

    /// <summary>
    /// DTO for individual property values in matrix
    /// </summary>
    public class PropertyValue : ModelEntityBase
    {
        private decimal PressureValue;

        public decimal Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }
        private decimal TemperatureValue;

        public decimal Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        }
        private decimal ZFactorValue;

        public decimal ZFactor

        {

            get { return this.ZFactorValue; }

            set { SetProperty(ref ZFactorValue, value); }

        }
        private decimal DensityValue;

        public decimal Density

        {

            get { return this.DensityValue; }

            set { SetProperty(ref DensityValue, value); }

        }
        private decimal ViscosityValue;

        public decimal Viscosity

        {

            get { return this.ViscosityValue; }

            set { SetProperty(ref ViscosityValue, value); }

        }
        private decimal ThermalConductivityValue;

        public decimal ThermalConductivity

        {

            get { return this.ThermalConductivityValue; }

            set { SetProperty(ref ThermalConductivityValue, value); }

        }
        private decimal CompressibilityFactorValue;

        public decimal CompressibilityFactor

        {

            get { return this.CompressibilityFactorValue; }

            set { SetProperty(ref CompressibilityFactorValue, value); }

        }
    }
}







