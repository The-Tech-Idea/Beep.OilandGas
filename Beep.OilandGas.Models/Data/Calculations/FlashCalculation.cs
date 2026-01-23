using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.FlashCalculations;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Calculations
{
    /// <summary>
    /// Request for multi-stage flash calculation
    /// </summary>
    public class FlashCalculationRequest : ModelEntityBase
    {
        /// <summary>
        /// Flash conditions
        /// </summary>
        private FlashConditions ConditionsValue = null!;

        [Required(ErrorMessage = "Conditions are required")]
        public FlashConditions Conditions

        {

            get { return this.ConditionsValue; }

            set { SetProperty(ref ConditionsValue, value); }

        }

        /// <summary>
        /// Number of flash stages
        /// </summary>
        private int StagesValue;

        [Required]
        [Range(1, 100, ErrorMessage = "Stages must be between 1 and 100")]
        public int Stages

        {

            get { return this.StagesValue; }

            set { SetProperty(ref StagesValue, value); }

        }
    }

    /// <summary>
    /// DTO for PVT envelope analysis
    /// </summary>
    public class PVTEnvelopeAnalysis : ModelEntityBase
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
        private decimal BubblePointPressureValue;

        public decimal BubblePointPressure

        {

            get { return this.BubblePointPressureValue; }

            set { SetProperty(ref BubblePointPressureValue, value); }

        }
        private decimal BubblePointTemperatureValue;

        public decimal BubblePointTemperature

        {

            get { return this.BubblePointTemperatureValue; }

            set { SetProperty(ref BubblePointTemperatureValue, value); }

        }
        private decimal DewPointPressureValue;

        public decimal DewPointPressure

        {

            get { return this.DewPointPressureValue; }

            set { SetProperty(ref DewPointPressureValue, value); }

        }
        private decimal DewPointTemperatureValue;

        public decimal DewPointTemperature

        {

            get { return this.DewPointTemperatureValue; }

            set { SetProperty(ref DewPointTemperatureValue, value); }

        }
        private decimal CriticalPressureValue;

        public decimal CriticalPressure

        {

            get { return this.CriticalPressureValue; }

            set { SetProperty(ref CriticalPressureValue, value); }

        }
        private decimal CriticalTemperatureValue;

        public decimal CriticalTemperature

        {

            get { return this.CriticalTemperatureValue; }

            set { SetProperty(ref CriticalTemperatureValue, value); }

        }
        private List<EnvelopePoint> EnvelopePointsValue = new();

        public List<EnvelopePoint> EnvelopePoints

        {

            get { return this.EnvelopePointsValue; }

            set { SetProperty(ref EnvelopePointsValue, value); }

        }
        private string EnvelopeTypeValue = string.Empty;

        public string EnvelopeType

        {

            get { return this.EnvelopeTypeValue; }

            set { SetProperty(ref EnvelopeTypeValue, value); }

        } // Type I, Type II, Type III, Type IV
    }

    /// <summary>
    /// DTO for individual envelope point
    /// </summary>
    public class EnvelopePoint : ModelEntityBase
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
        private string PhaseRegionValue = string.Empty;

        public string PhaseRegion

        {

            get { return this.PhaseRegionValue; }

            set { SetProperty(ref PhaseRegionValue, value); }

        }
        private decimal VaporFractionValue;

        public decimal VaporFraction

        {

            get { return this.VaporFractionValue; }

            set { SetProperty(ref VaporFractionValue, value); }

        }
    }

    /// <summary>
    /// DTO for bubble point analysis
    /// </summary>
    public class BubblePointAnalysis : ModelEntityBase
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
        private decimal BubblePointPressureValue;

        public decimal BubblePointPressure

        {

            get { return this.BubblePointPressureValue; }

            set { SetProperty(ref BubblePointPressureValue, value); }

        }
        public Dictionary<string, decimal> LiquidComposition { get; set; } = new();
        public Dictionary<string, decimal> KValues { get; set; } = new();
        private int IterationsValue;

        public int Iterations

        {

            get { return this.IterationsValue; }

            set { SetProperty(ref IterationsValue, value); }

        }
        private bool ConvergedValue;

        public bool Converged

        {

            get { return this.ConvergedValue; }

            set { SetProperty(ref ConvergedValue, value); }

        }
        private decimal ConvergenceErrorValue;

        public decimal ConvergenceError

        {

            get { return this.ConvergenceErrorValue; }

            set { SetProperty(ref ConvergenceErrorValue, value); }

        }
    }

    /// <summary>
    /// DTO for dew point analysis
    /// </summary>
    public class DewPointAnalysis : ModelEntityBase
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
        private decimal DewPointPressureValue;

        public decimal DewPointPressure

        {

            get { return this.DewPointPressureValue; }

            set { SetProperty(ref DewPointPressureValue, value); }

        }
        public Dictionary<string, decimal> VaporComposition { get; set; } = new();
        public Dictionary<string, decimal> KValues { get; set; } = new();
        private int IterationsValue;

        public int Iterations

        {

            get { return this.IterationsValue; }

            set { SetProperty(ref IterationsValue, value); }

        }
        private bool ConvergedValue;

        public bool Converged

        {

            get { return this.ConvergedValue; }

            set { SetProperty(ref ConvergedValue, value); }

        }
        private decimal ConvergenceErrorValue;

        public decimal ConvergenceError

        {

            get { return this.ConvergenceErrorValue; }

            set { SetProperty(ref ConvergenceErrorValue, value); }

        }
    }

    /// <summary>
    /// DTO for flash calculation result with extended analysis
    /// </summary>
    public class FlashCalculationPropertyResult : ModelEntityBase
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
        private DateTime CalculationDateValue;

        public DateTime CalculationDate

        {

            get { return this.CalculationDateValue; }

            set { SetProperty(ref CalculationDateValue, value); }

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
        private decimal VaporFractionValue;

        public decimal VaporFraction

        {

            get { return this.VaporFractionValue; }

            set { SetProperty(ref VaporFractionValue, value); }

        }
        private decimal LiquidFractionValue;

        public decimal LiquidFraction

        {

            get { return this.LiquidFractionValue; }

            set { SetProperty(ref LiquidFractionValue, value); }

        }
        public Dictionary<string, decimal> VaporComposition { get; set; } = new();
        public Dictionary<string, decimal> LiquidComposition { get; set; } = new();
        public Dictionary<string, decimal> KValues { get; set; } = new();
        private int IterationsValue;

        public int Iterations

        {

            get { return this.IterationsValue; }

            set { SetProperty(ref IterationsValue, value); }

        }
        private bool ConvergedValue;

        public bool Converged

        {

            get { return this.ConvergedValue; }

            set { SetProperty(ref ConvergedValue, value); }

        }
        private decimal ConvergenceErrorValue;

        public decimal ConvergenceError

        {

            get { return this.ConvergenceErrorValue; }

            set { SetProperty(ref ConvergenceErrorValue, value); }

        }
    }

    /// <summary>
    /// DTO for separator design and simulation
    /// </summary>
    public class SeparatorSimulation : ModelEntityBase
    {
        private string SimulationIdValue = string.Empty;

        public string SimulationId

        {

            get { return this.SimulationIdValue; }

            set { SetProperty(ref SimulationIdValue, value); }

        }
        private string CompositionIdValue = string.Empty;

        public string CompositionId

        {

            get { return this.CompositionIdValue; }

            set { SetProperty(ref CompositionIdValue, value); }

        }
        private DateTime SimulationDateValue;

        public DateTime SimulationDate

        {

            get { return this.SimulationDateValue; }

            set { SetProperty(ref SimulationDateValue, value); }

        }
        private decimal InletPressureValue;

        public decimal InletPressure

        {

            get { return this.InletPressureValue; }

            set { SetProperty(ref InletPressureValue, value); }

        }
        private decimal InletTemperatureValue;

        public decimal InletTemperature

        {

            get { return this.InletTemperatureValue; }

            set { SetProperty(ref InletTemperatureValue, value); }

        }
        private decimal SeparatorPressureValue;

        public decimal SeparatorPressure

        {

            get { return this.SeparatorPressureValue; }

            set { SetProperty(ref SeparatorPressureValue, value); }

        }
        private decimal SeparatorTemperatureValue;

        public decimal SeparatorTemperature

        {

            get { return this.SeparatorTemperatureValue; }

            set { SetProperty(ref SeparatorTemperatureValue, value); }

        }
        private decimal GasOilRatioValue;

        public decimal GasOilRatio

        {

            get { return this.GasOilRatioValue; }

            set { SetProperty(ref GasOilRatioValue, value); }

        }
        private decimal LiquidRecoveryValue;

        public decimal LiquidRecovery

        {

            get { return this.LiquidRecoveryValue; }

            set { SetProperty(ref LiquidRecoveryValue, value); }

        }
        private decimal GasRecoveryValue;

        public decimal GasRecovery

        {

            get { return this.GasRecoveryValue; }

            set { SetProperty(ref GasRecoveryValue, value); }

        }
        private List<SeparatorStage> StagesValue = new();

        public List<SeparatorStage> Stages

        {

            get { return this.StagesValue; }

            set { SetProperty(ref StagesValue, value); }

        }
    }

    /// <summary>
    /// DTO for individual separator stage
    /// </summary>
    public class SeparatorStage : ModelEntityBase
    {
        private int StageNumberValue;

        public int StageNumber

        {

            get { return this.StageNumberValue; }

            set { SetProperty(ref StageNumberValue, value); }

        }
        private decimal StagePressureValue;

        public decimal StagePressure

        {

            get { return this.StagePressureValue; }

            set { SetProperty(ref StagePressureValue, value); }

        }
        private decimal StageTemperatureValue;

        public decimal StageTemperature

        {

            get { return this.StageTemperatureValue; }

            set { SetProperty(ref StageTemperatureValue, value); }

        }
        private decimal VaporFractionValue;

        public decimal VaporFraction

        {

            get { return this.VaporFractionValue; }

            set { SetProperty(ref VaporFractionValue, value); }

        }
        private decimal LiquidRecoveryFractionValue;

        public decimal LiquidRecoveryFraction

        {

            get { return this.LiquidRecoveryFractionValue; }

            set { SetProperty(ref LiquidRecoveryFractionValue, value); }

        }
    }

    /// <summary>
    /// DTO for pressure-temperature phase diagram
    /// </summary>
    public class PhaseDiagram : ModelEntityBase
    {
        private string DiagramIdValue = string.Empty;

        public string DiagramId

        {

            get { return this.DiagramIdValue; }

            set { SetProperty(ref DiagramIdValue, value); }

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
        private List<PhaseRegion> PhaseRegionsValue = new();

        public List<PhaseRegion> PhaseRegions

        {

            get { return this.PhaseRegionsValue; }

            set { SetProperty(ref PhaseRegionsValue, value); }

        }
    }

    /// <summary>
    /// DTO for phase region in diagram
    /// </summary>
    public class PhaseRegion : ModelEntityBase
    {
        private string RegionNameValue = string.Empty;

        public string RegionName

        {

            get { return this.RegionNameValue; }

            set { SetProperty(ref RegionNameValue, value); }

        } // Single Phase, Two-Phase, Three-Phase
        private List<decimal> PressuresValue = new();

        public List<decimal> Pressures

        {

            get { return this.PressuresValue; }

            set { SetProperty(ref PressuresValue, value); }

        }
        private List<decimal> TemperaturesValue = new();

        public List<decimal> Temperatures

        {

            get { return this.TemperaturesValue; }

            set { SetProperty(ref TemperaturesValue, value); }

        }
    }

    /// <summary>
    /// DTO for stability analysis result
    /// </summary>
    public class StabilityAnalysis : ModelEntityBase
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
        private bool IsStableValue;

        public bool IsStable

        {

            get { return this.IsStableValue; }

            set { SetProperty(ref IsStableValue, value); }

        }
        private decimal TangentPlaneDistanceValue;

        public decimal TangentPlaneDistance

        {

            get { return this.TangentPlaneDistanceValue; }

            set { SetProperty(ref TangentPlaneDistanceValue, value); }

        }
        private string StabilityStatusValue = string.Empty;

        public string StabilityStatus

        {

            get { return this.StabilityStatusValue; }

            set { SetProperty(ref StabilityStatusValue, value); }

        } // Stable, Unstable, Critical
        public Dictionary<string, decimal> CriticalComposition { get; set; } = new();
    }

    /// <summary>
    /// DTO for equilibrium constant analysis
    /// </summary>
    public class EquilibriumConstantAnalysis : ModelEntityBase
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
        public Dictionary<string, decimal> KValues { get; set; } = new();
        private string CorrelationMethodValue = string.Empty;

        public string CorrelationMethod

        {

            get { return this.CorrelationMethodValue; }

            set { SetProperty(ref CorrelationMethodValue, value); }

        }
    }
    /// <summary>
    /// Request for Flash Calculation
    /// </summary>


    /// <summary>
    /// Component data for flash calculations
    /// </summary>
    public class ComponentData : ModelEntityBase
    {
        private string NameValue = string.Empty;

        public string Name

        {

            get { return this.NameValue; }

            set { SetProperty(ref NameValue, value); }

        }
        private decimal MoleFractionValue;

        public decimal MoleFraction

        {

            get { return this.MoleFractionValue; }

            set { SetProperty(ref MoleFractionValue, value); }

        }
        private decimal? CriticalTemperatureValue;

        public decimal? CriticalTemperature

        {

            get { return this.CriticalTemperatureValue; }

            set { SetProperty(ref CriticalTemperatureValue, value); }

        } // Rankine
        private decimal? CriticalPressureValue;

        public decimal? CriticalPressure

        {

            get { return this.CriticalPressureValue; }

            set { SetProperty(ref CriticalPressureValue, value); }

        } // psia
        private decimal? AcentricFactorValue;

        public decimal? AcentricFactor

        {

            get { return this.AcentricFactorValue; }

            set { SetProperty(ref AcentricFactorValue, value); }

        }
        private decimal? MolecularWeightValue;

        public decimal? MolecularWeight

        {

            get { return this.MolecularWeightValue; }

            set { SetProperty(ref MolecularWeightValue, value); }

        }
    }

    /// <summary>
    /// Result of Flash Calculation
    /// </summary>
    public class FlashCalculationResult : ModelEntityBase
    {
        private string CalculationIdValue = string.Empty;

        public string CalculationId

        {

            get { return this.CalculationIdValue; }

            set { SetProperty(ref CalculationIdValue, value); }

        }
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string? FacilityIdValue;

        public string? FacilityId

        {

            get { return this.FacilityIdValue; }

            set { SetProperty(ref FacilityIdValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string CalculationTypeValue = string.Empty;

        public string CalculationType

        {

            get { return this.CalculationTypeValue; }

            set { SetProperty(ref CalculationTypeValue, value); }

        }
        private DateTime CalculationDateValue;

        public DateTime CalculationDate

        {

            get { return this.CalculationDateValue; }

            set { SetProperty(ref CalculationDateValue, value); }

        }
        
        // Flash results
        private decimal VaporFractionValue;

        public decimal VaporFraction

        {

            get { return this.VaporFractionValue; }

            set { SetProperty(ref VaporFractionValue, value); }

        } // 0-1
        private decimal LiquidFractionValue;

        public decimal LiquidFraction

        {

            get { return this.LiquidFractionValue; }

            set { SetProperty(ref LiquidFractionValue, value); }

        } // 0-1
        
        // Phase compositions (mole fractions)
        public Dictionary<string, decimal> VaporComposition { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, decimal> LiquidComposition { get; set; } = new Dictionary<string, decimal>();
        
        // K-values (equilibrium ratios)
        public Dictionary<string, decimal> KValues { get; set; } = new Dictionary<string, decimal>();
        
        // Phase properties
        private PhasePropertiesData? VaporPropertiesValue;

        public PhasePropertiesData? VaporProperties

        {

            get { return this.VaporPropertiesValue; }

            set { SetProperty(ref VaporPropertiesValue, value); }

        }
        private PhasePropertiesData? LiquidPropertiesValue;

        public PhasePropertiesData? LiquidProperties

        {

            get { return this.LiquidPropertiesValue; }

            set { SetProperty(ref LiquidPropertiesValue, value); }

        }
        
        // Convergence information
        private int IterationsValue;

        public int Iterations

        {

            get { return this.IterationsValue; }

            set { SetProperty(ref IterationsValue, value); }

        }
        private bool ConvergedValue;

        public bool Converged

        {

            get { return this.ConvergedValue; }

            set { SetProperty(ref ConvergedValue, value); }

        }
        private decimal ConvergenceErrorValue;

        public decimal ConvergenceError

        {

            get { return this.ConvergenceErrorValue; }

            set { SetProperty(ref ConvergenceErrorValue, value); }

        }
        
        // Additional metadata
        public Dictionary<string, object>? AdditionalResults { get; set; }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        } // SUCCESS, FAILED, PARTIAL
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
        private bool IsSuccessfulValue;

        public bool IsSuccessful

        {

            get { return this.IsSuccessfulValue; }

            set { SetProperty(ref IsSuccessfulValue, value); }

        }
    }

    /// <summary>
    /// Phase properties data
    /// </summary>
    public class PhasePropertiesData : ModelEntityBase
    {
        private decimal DensityValue;

        public decimal Density

        {

            get { return this.DensityValue; }

            set { SetProperty(ref DensityValue, value); }

        } // lb/ft³
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
        private decimal? VolumeValue;

        public decimal? Volume

        {

            get { return this.VolumeValue; }

            set { SetProperty(ref VolumeValue, value); }

        } // ft³
    }
}







