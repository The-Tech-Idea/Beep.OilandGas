using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data
{
    /// <summary>
    /// DTO for oil composition.
    /// </summary>
    public class OilComposition : ModelEntityBase
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
        private decimal OilGravityValue;

        public decimal OilGravity

        {

            get { return this.OilGravityValue; }

            set { SetProperty(ref OilGravityValue, value); }

        } // API gravity
        private decimal GasOilRatioValue;

        public decimal GasOilRatio

        {

            get { return this.GasOilRatioValue; }

            set { SetProperty(ref GasOilRatioValue, value); }

        } // scf/stb
        private decimal WaterCutValue;

        public decimal WaterCut

        {

            get { return this.WaterCutValue; }

            set { SetProperty(ref WaterCutValue, value); }

        } // fraction
        private decimal BubblePointPressureValue;

        public decimal BubblePointPressure

        {

            get { return this.BubblePointPressureValue; }

            set { SetProperty(ref BubblePointPressureValue, value); }

        } // psia
        private string? RemarksValue;

        public string? Remarks

        {

            get { return this.RemarksValue; }

            set { SetProperty(ref RemarksValue, value); }

        }
    }

    /// <summary>
    /// DTO for oil property calculation result.
    /// </summary>
    public class OilPropertyResult : ModelEntityBase
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
        private decimal FormationVolumeFactorValue;

        public decimal FormationVolumeFactor

        {

            get { return this.FormationVolumeFactorValue; }

            set { SetProperty(ref FormationVolumeFactorValue, value); }

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

        public decimal ApiGravity { get; set; }
        public decimal SpecificGravity { get; set; }
        public decimal SolutionGasOilRatio { get; set; }
    }

    /// <summary>
    /// Request for calculating Formation Volume Factor (FVF)
    /// </summary>
    public class CalculateFVFRequest : ModelEntityBase
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

        private decimal GasOilRatioValue;


        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "GasOilRatio must be greater than or equal to 0")]
        public decimal GasOilRatio


        {


            get { return this.GasOilRatioValue; }


            set { SetProperty(ref GasOilRatioValue, value); }


        }

        private decimal OilGravityValue;


        [Required]
        [Range(0, 100, ErrorMessage = "OilGravity must be between 0 and 100")]
        public decimal OilGravity


        {


            get { return this.OilGravityValue; }


            set { SetProperty(ref OilGravityValue, value); }


        }

        private string CorrelationValue = "Standing";


        public string Correlation


        {


            get { return this.CorrelationValue; }


            set { SetProperty(ref CorrelationValue, value); }


        }
    }

    /// <summary>
    /// Request for calculating oil density
    /// </summary>
    public class CalculateDensityRequest : ModelEntityBase
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

        private decimal OilGravityValue;


        [Required]
        [Range(0, 100, ErrorMessage = "OilGravity must be between 0 and 100")]
        public decimal OilGravity


        {


            get { return this.OilGravityValue; }


            set { SetProperty(ref OilGravityValue, value); }


        }

        private decimal GasOilRatioValue;


        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "GasOilRatio must be greater than or equal to 0")]
        public decimal GasOilRatio


        {


            get { return this.GasOilRatioValue; }


            set { SetProperty(ref GasOilRatioValue, value); }


        }
    }

    /// <summary>
    /// Request for calculating oil viscosity
    /// </summary>
    public class CalculateViscosityRequest : ModelEntityBase
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

        private decimal OilGravityValue;


        [Required]
        [Range(0, 100, ErrorMessage = "OilGravity must be between 0 and 100")]
        public decimal OilGravity


        {


            get { return this.OilGravityValue; }


            set { SetProperty(ref OilGravityValue, value); }


        }

        private decimal GasOilRatioValue;


        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "GasOilRatio must be greater than or equal to 0")]
        public decimal GasOilRatio


        {


            get { return this.GasOilRatioValue; }


            set { SetProperty(ref GasOilRatioValue, value); }


        }
    }

    /// <summary>
    /// Request for calculating comprehensive oil properties
    /// </summary>
    public class CalculateOilPropertiesRequest : ModelEntityBase
    {
        private OilComposition CompositionValue = null!;

        [Required(ErrorMessage = "Composition is required")]
        public OilComposition Composition

        {

            get { return this.CompositionValue; }

            set { SetProperty(ref CompositionValue, value); }

        }

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
     }

     /// <summary>
     /// DTO for phase diagram analysis
     /// </summary>
     public class PhaseDiagramAnalysis : ModelEntityBase
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
         private decimal CriticalDensityValue;

         public decimal CriticalDensity

         {

             get { return this.CriticalDensityValue; }

             set { SetProperty(ref CriticalDensityValue, value); }

         }
         private decimal TriplePointTemperatureValue;

         public decimal TriplePointTemperature

         {

             get { return this.TriplePointTemperatureValue; }

             set { SetProperty(ref TriplePointTemperatureValue, value); }

         }
         private decimal TriplePointPressureValue;

         public decimal TriplePointPressure

         {

             get { return this.TriplePointPressureValue; }

             set { SetProperty(ref TriplePointPressureValue, value); }

         }
         private List<PhasePoint> PhasePointsValue = new();

         public List<PhasePoint> PhasePoints

         {

             get { return this.PhasePointsValue; }

             set { SetProperty(ref PhasePointsValue, value); }

         }
         private string PhaseValue = string.Empty;

         public string Phase

         {

             get { return this.PhaseValue; }

             set { SetProperty(ref PhaseValue, value); }

         } // Single Phase, Two-Phase, Three-Phase
     }

     /// <summary>
     /// DTO for phase envelope point
     /// </summary>
     public class PhasePoint : ModelEntityBase
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
         private string PhaseValue = string.Empty;

         public string Phase

         {

             get { return this.PhaseValue; }

             set { SetProperty(ref PhaseValue, value); }

         } // Gas, Oil, Two-Phase
         private decimal DensityValue;

         public decimal Density

         {

             get { return this.DensityValue; }

             set { SetProperty(ref DensityValue, value); }

         }
     }

     /// <summary>
     /// DTO for compressibility factor analysis
     /// </summary>
     public class CompressibilityFactorAnalysis : ModelEntityBase
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
         private decimal CompressibilityFactorValue;

         public decimal CompressibilityFactor

         {

             get { return this.CompressibilityFactorValue; }

             set { SetProperty(ref CompressibilityFactorValue, value); }

         }
         private decimal ReducedPressureValue;

         public decimal ReducedPressure

         {

             get { return this.ReducedPressureValue; }

             set { SetProperty(ref ReducedPressureValue, value); }

         }
         private decimal ReducedTemperatureValue;

         public decimal ReducedTemperature

         {

             get { return this.ReducedTemperatureValue; }

             set { SetProperty(ref ReducedTemperatureValue, value); }

         }
         private string CorrelationMethodValue = string.Empty;

         public string CorrelationMethod

         {

             get { return this.CorrelationMethodValue; }

             set { SetProperty(ref CorrelationMethodValue, value); }

         }
         private decimal DeviationFromIdealValue;

         public decimal DeviationFromIdeal

         {

             get { return this.DeviationFromIdealValue; }

             set { SetProperty(ref DeviationFromIdealValue, value); }

         }
     }

     /// <summary>
     /// DTO for interfacial tension analysis
     /// </summary>
     public class InterfacialTensionAnalysis : ModelEntityBase
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
         private decimal InterfacialTensionValue;

         public decimal InterfacialTension

         {

             get { return this.InterfacialTensionValue; }

             set { SetProperty(ref InterfacialTensionValue, value); }

         } // dyne/cm
         private string Phase1Value = string.Empty;

         public string Phase1

         {

             get { return this.Phase1Value; }

             set { SetProperty(ref Phase1Value, value); }

         }
         private string Phase2Value = string.Empty;

         public string Phase2

         {

             get { return this.Phase2Value; }

             set { SetProperty(ref Phase2Value, value); }

         }
         private decimal TemperatureDependenceValue;

         public decimal TemperatureDependence

         {

             get { return this.TemperatureDependenceValue; }

             set { SetProperty(ref TemperatureDependenceValue, value); }

         }
     }

     /// <summary>
     /// DTO for fluid behavior analysis
     /// </summary>
     public class FluidBehaviorAnalysis : ModelEntityBase
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
         private string FluidTypeValue = string.Empty;

         public string FluidType

         {

             get { return this.FluidTypeValue; }

             set { SetProperty(ref FluidTypeValue, value); }

         } // Black Oil, Volatile Oil, Condensate, etc.
         private decimal BubblePointPressureValue;

         public decimal BubblePointPressure

         {

             get { return this.BubblePointPressureValue; }

             set { SetProperty(ref BubblePointPressureValue, value); }

         }
         private decimal DewPointPressureValue;

         public decimal DewPointPressure

         {

             get { return this.DewPointPressureValue; }

             set { SetProperty(ref DewPointPressureValue, value); }

         }
         private decimal CriticalSolveGORValue;

         public decimal CriticalSolveGOR

         {

             get { return this.CriticalSolveGORValue; }

             set { SetProperty(ref CriticalSolveGORValue, value); }

         }
         private decimal DissolvedGORValue;

         public decimal DissolvedGOR

         {

             get { return this.DissolvedGORValue; }

             set { SetProperty(ref DissolvedGORValue, value); }

         }
         private string CharacteristicsValue = string.Empty;

         public string Characteristics

         {

             get { return this.CharacteristicsValue; }

             set { SetProperty(ref CharacteristicsValue, value); }

         }
         private List<string> BehaviorClassificationsValue = new();

         public List<string> BehaviorClassifications

         {

             get { return this.BehaviorClassificationsValue; }

             set { SetProperty(ref BehaviorClassificationsValue, value); }

         }
     }

     /// <summary>
     /// DTO for property correlation matrix
     /// </summary>
     public class PropertyCorrelationMatrix : ModelEntityBase
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
         private DateTime AnalysisDateValue;

         public DateTime AnalysisDate

         {

             get { return this.AnalysisDateValue; }

             set { SetProperty(ref AnalysisDateValue, value); }

         }
         private List<PressureRangeProperty> PropertyByPressureValue = new();

         public List<PressureRangeProperty> PropertyByPressure

         {

             get { return this.PropertyByPressureValue; }

             set { SetProperty(ref PropertyByPressureValue, value); }

         }
         private List<TemperatureRangeProperty> PropertyByTemperatureValue = new();

         public List<TemperatureRangeProperty> PropertyByTemperature

         {

             get { return this.PropertyByTemperatureValue; }

             set { SetProperty(ref PropertyByTemperatureValue, value); }

         }
         public Dictionary<string, decimal> CorrelationCoefficients { get; set; } = new();
     }

     /// <summary>
     /// DTO for property variation with pressure
     /// </summary>
     public class PressureRangeProperty : ModelEntityBase
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
         private decimal ViscosityValue;

         public decimal Viscosity

         {

             get { return this.ViscosityValue; }

             set { SetProperty(ref ViscosityValue, value); }

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
         private decimal CompressibilityValue;

         public decimal Compressibility

         {

             get { return this.CompressibilityValue; }

             set { SetProperty(ref CompressibilityValue, value); }

         }
     }

     /// <summary>
     /// DTO for property variation with temperature
     /// </summary>
     public class TemperatureRangeProperty : ModelEntityBase
     {
         private decimal TemperatureValue;

         public decimal Temperature

         {

             get { return this.TemperatureValue; }

             set { SetProperty(ref TemperatureValue, value); }

         }
         private decimal PressureValue;

         public decimal Pressure

         {

             get { return this.PressureValue; }

             set { SetProperty(ref PressureValue, value); }

         }
         private decimal ViscosityValue;

         public decimal Viscosity

         {

             get { return this.ViscosityValue; }

             set { SetProperty(ref ViscosityValue, value); }

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
         private decimal CompressibilityValue;

         public decimal Compressibility

         {

             get { return this.CompressibilityValue; }

             set { SetProperty(ref CompressibilityValue, value); }

         }
     }

     /// <summary>
     /// DTO for PVT surface property prediction
     /// </summary>
     public class PVTSurfaceProperty : ModelEntityBase
     {
         private string PropertyIdValue = string.Empty;

         public string PropertyId

         {

             get { return this.PropertyIdValue; }

             set { SetProperty(ref PropertyIdValue, value); }

         }
         private string CompositionIdValue = string.Empty;

         public string CompositionId

         {

             get { return this.CompositionIdValue; }

             set { SetProperty(ref CompositionIdValue, value); }

         }
         private DateTime PredictionDateValue;

         public DateTime PredictionDate

         {

             get { return this.PredictionDateValue; }

             set { SetProperty(ref PredictionDateValue, value); }

         }
         private decimal StockTankOilGravityValue;

         public decimal StockTankOilGravity

         {

             get { return this.StockTankOilGravityValue; }

             set { SetProperty(ref StockTankOilGravityValue, value); }

         }
         private decimal StockTankOilDensityValue;

         public decimal StockTankOilDensity

         {

             get { return this.StockTankOilDensityValue; }

             set { SetProperty(ref StockTankOilDensityValue, value); }

         }
         private decimal ResidualGasGravityValue;

         public decimal ResidualGasGravity

         {

             get { return this.ResidualGasGravityValue; }

             set { SetProperty(ref ResidualGasGravityValue, value); }

         }
         private decimal SeparationRatioValue;

         public decimal SeparationRatio

         {

             get { return this.SeparationRatioValue; }

             set { SetProperty(ref SeparationRatioValue, value); }

         }
         private decimal SolubilityAtSurfaceConditionsValue;

         public decimal SolubilityAtSurfaceConditions

         {

             get { return this.SolubilityAtSurfaceConditionsValue; }

             set { SetProperty(ref SolubilityAtSurfaceConditionsValue, value); }

         }
         private string AnalysisMethodValue = string.Empty;

         public string AnalysisMethod

         {

             get { return this.AnalysisMethodValue; }

             set { SetProperty(ref AnalysisMethodValue, value); }

         }
     }

     /// <summary>
     /// DTO for property trend analysis
     /// </summary>
     public class PropertyTrendAnalysis : ModelEntityBase
     {
         private string TrendIdValue = string.Empty;

         public string TrendId

         {

             get { return this.TrendIdValue; }

             set { SetProperty(ref TrendIdValue, value); }

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
         private string PropertyNameValue = string.Empty;

         public string PropertyName

         {

             get { return this.PropertyNameValue; }

             set { SetProperty(ref PropertyNameValue, value); }

         }
         private List<decimal> PropertyValuesValue = new();

         public List<decimal> PropertyValues

         {

             get { return this.PropertyValuesValue; }

             set { SetProperty(ref PropertyValuesValue, value); }

         }
         private List<decimal> PressureRangeValue = new();

         public List<decimal> PressureRange

         {

             get { return this.PressureRangeValue; }

             set { SetProperty(ref PressureRangeValue, value); }

         }
         private decimal TrendSlopeValue;

         public decimal TrendSlope

         {

             get { return this.TrendSlopeValue; }

             set { SetProperty(ref TrendSlopeValue, value); }

         }
         private string TrendDirectionValue = string.Empty;

         public string TrendDirection

         {

             get { return this.TrendDirectionValue; }

             set { SetProperty(ref TrendDirectionValue, value); }

         } // Increasing, Decreasing, Linear
         private decimal RSquaredValue;

         public decimal RSquared

         {

             get { return this.RSquaredValue; }

             set { SetProperty(ref RSquaredValue, value); }

         } // Fit quality
     }
}







