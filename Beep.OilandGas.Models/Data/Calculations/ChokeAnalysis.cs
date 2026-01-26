using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Calculations
{
    /// <summary>
    /// Comprehensive DTO for advanced choke analysis results.
    /// Includes bean choke, venturi, and specialized flow models.
    /// </summary>
    public class AdvancedChokeAnalysis : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private string ChokeTypeValue = string.Empty;

        public string ChokeType

        {

            get { return this.ChokeTypeValue; }

            set { SetProperty(ref ChokeTypeValue, value); }

        } // Bean, Venturi, Orifice
        private decimal ChokeDiameterValue;

        public decimal ChokeDiameter

        {

            get { return this.ChokeDiameterValue; }

            set { SetProperty(ref ChokeDiameterValue, value); }

        }
        private decimal UpstreamPressureValue;

        public decimal UpstreamPressure

        {

            get { return this.UpstreamPressureValue; }

            set { SetProperty(ref UpstreamPressureValue, value); }

        }
        private decimal DownstreamPressureValue;

        public decimal DownstreamPressure

        {

            get { return this.DownstreamPressureValue; }

            set { SetProperty(ref DownstreamPressureValue, value); }

        }
        private decimal TemperatureValue;

        public decimal Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        }
        private decimal GasFlowRateValue;

        public decimal GasFlowRate

        {

            get { return this.GasFlowRateValue; }

            set { SetProperty(ref GasFlowRateValue, value); }

        }
        private decimal CalculatedFlowRateValue;

        public decimal CalculatedFlowRate

        {

            get { return this.CalculatedFlowRateValue; }

            set { SetProperty(ref CalculatedFlowRateValue, value); }

        }
        private decimal DischargeCoefficientValue;

        public decimal DischargeCoefficient

        {

            get { return this.DischargeCoefficientValue; }

            set { SetProperty(ref DischargeCoefficientValue, value); }

        }
        private decimal PressureRatioValue;

        public decimal PressureRatio

        {

            get { return this.PressureRatioValue; }

            set { SetProperty(ref PressureRatioValue, value); }

        }
        private decimal CriticalPressureRatioValue;

        public decimal CriticalPressureRatio

        {

            get { return this.CriticalPressureRatioValue; }

            set { SetProperty(ref CriticalPressureRatioValue, value); }

        }
        private string FlowRegimeValue = string.Empty;

        public string FlowRegime

        {

            get { return this.FlowRegimeValue; }

            set { SetProperty(ref FlowRegimeValue, value); }

        } // Sonic, Subsonic
        private decimal EfficiencyValue;

        public decimal Efficiency

        {

            get { return this.EfficiencyValue; }

            set { SetProperty(ref EfficiencyValue, value); }

        }
        private string ModelUsedValue = string.Empty;

        public string ModelUsed

        {

            get { return this.ModelUsedValue; }

            set { SetProperty(ref ModelUsedValue, value); }

        } // API-43, ISO 6149, etc.
    }

    /// <summary>
    /// DTO for multiphase flow choke analysis.
    /// Handles oil, water, and gas simultaneously.
    /// </summary>
    public class MultiphaseChokeAnalysis : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private decimal OilFlowRateValue;

        public decimal OilFlowRate

        {

            get { return this.OilFlowRateValue; }

            set { SetProperty(ref OilFlowRateValue, value); }

        } // STB/day
        private decimal WaterFlowRateValue;

        public decimal WaterFlowRate

        {

            get { return this.WaterFlowRateValue; }

            set { SetProperty(ref WaterFlowRateValue, value); }

        } // STB/day
        private decimal GasFlowRateValue;

        public decimal GasFlowRate

        {

            get { return this.GasFlowRateValue; }

            set { SetProperty(ref GasFlowRateValue, value); }

        } // Mscf/day
        private decimal OilDensityValue;

        public decimal OilDensity

        {

            get { return this.OilDensityValue; }

            set { SetProperty(ref OilDensityValue, value); }

        } // lb/ft³
        private decimal WaterDensityValue;

        public decimal WaterDensity

        {

            get { return this.WaterDensityValue; }

            set { SetProperty(ref WaterDensityValue, value); }

        } // lb/ft³
        private decimal GasDensityValue;

        public decimal GasDensity

        {

            get { return this.GasDensityValue; }

            set { SetProperty(ref GasDensityValue, value); }

        } // lb/ft³
        private decimal OilViscosityValue;

        public decimal OilViscosity

        {

            get { return this.OilViscosityValue; }

            set { SetProperty(ref OilViscosityValue, value); }

        } // cp
        private decimal WaterViscosityValue;

        public decimal WaterViscosity

        {

            get { return this.WaterViscosityValue; }

            set { SetProperty(ref WaterViscosityValue, value); }

        } // cp
        private decimal GasViscosityValue;

        public decimal GasViscosity

        {

            get { return this.GasViscosityValue; }

            set { SetProperty(ref GasViscosityValue, value); }

        } // cp
        private decimal SurfaceTensionValue;

        public decimal SurfaceTension

        {

            get { return this.SurfaceTensionValue; }

            set { SetProperty(ref SurfaceTensionValue, value); }

        } // dyne/cm
        private string FlowPatternValue = string.Empty;

        public string FlowPattern

        {

            get { return this.FlowPatternValue; }

            set { SetProperty(ref FlowPatternValue, value); }

        } // Bubbly, Slug, Annular, Dispersed
        private decimal MixtureDensityValue;

        public decimal MixtureDensity

        {

            get { return this.MixtureDensityValue; }

            set { SetProperty(ref MixtureDensityValue, value); }

        }
        private decimal MixtureViscosityValue;

        public decimal MixtureViscosity

        {

            get { return this.MixtureViscosityValue; }

            set { SetProperty(ref MixtureViscosityValue, value); }

        }
        private decimal HomogeneousVoidFractionValue;

        public decimal HomogeneousVoidFraction

        {

            get { return this.HomogeneousVoidFractionValue; }

            set { SetProperty(ref HomogeneousVoidFractionValue, value); }

        } // Quality
        private decimal TotalPressureDropValue;

        public decimal TotalPressureDrop

        {

            get { return this.TotalPressureDropValue; }

            set { SetProperty(ref TotalPressureDropValue, value); }

        }
        private decimal AccelerationPressureDropValue;

        public decimal AccelerationPressureDrop

        {

            get { return this.AccelerationPressureDropValue; }

            set { SetProperty(ref AccelerationPressureDropValue, value); }

        }
        private decimal FrictionalPressureDropValue;

        public decimal FrictionalPressureDrop

        {

            get { return this.FrictionalPressureDropValue; }

            set { SetProperty(ref FrictionalPressureDropValue, value); }

        }
        private decimal ElevationPressureDropValue;

        public decimal ElevationPressureDrop

        {

            get { return this.ElevationPressureDropValue; }

            set { SetProperty(ref ElevationPressureDropValue, value); }

        }
        private decimal DownstreamPressureValue;

        public decimal DownstreamPressure

        {

            get { return this.DownstreamPressureValue; }

            set { SetProperty(ref DownstreamPressureValue, value); }

        }
    }

    /// <summary>
    /// DTO for choke erosion and wear prediction.
    /// Based on sand rate, particle size, and flow velocity.
    /// </summary>
    public class ChokeErosionPrediction : ModelEntityBase
    {
        private string PredictionIdValue = string.Empty;

        public string PredictionId

        {

            get { return this.PredictionIdValue; }

            set { SetProperty(ref PredictionIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private decimal SandProductionRateValue;

        public decimal SandProductionRate

        {

            get { return this.SandProductionRateValue; }

            set { SetProperty(ref SandProductionRateValue, value); }

        } // lb/day
        private decimal SandParticleSizeValue;

        public decimal SandParticleSize

        {

            get { return this.SandParticleSizeValue; }

            set { SetProperty(ref SandParticleSizeValue, value); }

        } // microns
        private decimal ParticleVelocityValue;

        public decimal ParticleVelocity

        {

            get { return this.ParticleVelocityValue; }

            set { SetProperty(ref ParticleVelocityValue, value); }

        } // ft/sec
        private decimal ChokeMaterialValue;

        public decimal ChokeMaterial

        {

            get { return this.ChokeMaterialValue; }

            set { SetProperty(ref ChokeMaterialValue, value); }

        } // Hardness rating
        private decimal ErosionRateValue;

        public decimal ErosionRate

        {

            get { return this.ErosionRateValue; }

            set { SetProperty(ref ErosionRateValue, value); }

        } // mils/year (0.001 inch/year)
        private decimal EstimatedChokeLifeValue;

        public decimal EstimatedChokeLife

        {

            get { return this.EstimatedChokeLifeValue; }

            set { SetProperty(ref EstimatedChokeLifeValue, value); }

        } // years
        private int DaysUntilReplacementValue;

        public int DaysUntilReplacement

        {

            get { return this.DaysUntilReplacementValue; }

            set { SetProperty(ref DaysUntilReplacementValue, value); }

        }
        private decimal CumulativeWearDepthValue;

        public decimal CumulativeWearDepth

        {

            get { return this.CumulativeWearDepthValue; }

            set { SetProperty(ref CumulativeWearDepthValue, value); }

        } // mils
        private string WearStatusValue = string.Empty;

        public string WearStatus

        {

            get { return this.WearStatusValue; }

            set { SetProperty(ref WearStatusValue, value); }

        } // Good, Fair, Poor, Critical
        private decimal ErosionSeverityValue;

        public decimal ErosionSeverity

        {

            get { return this.ErosionSeverityValue; }

            set { SetProperty(ref ErosionSeverityValue, value); }

        } // 0-100 scale
        private List<string> RecommendedActionsValue = new();

        public List<string> RecommendedActions

        {

            get { return this.RecommendedActionsValue; }

            set { SetProperty(ref RecommendedActionsValue, value); }

        }
    }

    /// <summary>
    /// DTO for choke back-pressure optimization.
    /// Analyzes production vs. pressure drop trade-offs.
    /// </summary>
    public class ChokeBackPressureOptimization : ModelEntityBase
    {
        private string OptimizationIdValue = string.Empty;

        public string OptimizationId

        {

            get { return this.OptimizationIdValue; }

            set { SetProperty(ref OptimizationIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private decimal CurrentChokeDiameterValue;

        public decimal CurrentChokeDiameter

        {

            get { return this.CurrentChokeDiameterValue; }

            set { SetProperty(ref CurrentChokeDiameterValue, value); }

        }
        private decimal CurrentBackPressureValue;

        public decimal CurrentBackPressure

        {

            get { return this.CurrentBackPressureValue; }

            set { SetProperty(ref CurrentBackPressureValue, value); }

        }
        private decimal CurrentProductionRateValue;

        public decimal CurrentProductionRate

        {

            get { return this.CurrentProductionRateValue; }

            set { SetProperty(ref CurrentProductionRateValue, value); }

        }
        private decimal OptimalChokeDiameterValue;

        public decimal OptimalChokeDiameter

        {

            get { return this.OptimalChokeDiameterValue; }

            set { SetProperty(ref OptimalChokeDiameterValue, value); }

        }
        private decimal OptimalBackPressureValue;

        public decimal OptimalBackPressure

        {

            get { return this.OptimalBackPressureValue; }

            set { SetProperty(ref OptimalBackPressureValue, value); }

        }
        private decimal OptimalProductionRateValue;

        public decimal OptimalProductionRate

        {

            get { return this.OptimalProductionRateValue; }

            set { SetProperty(ref OptimalProductionRateValue, value); }

        }
        private decimal ProductionIncreaseValue;

        public decimal ProductionIncrease

        {

            get { return this.ProductionIncreaseValue; }

            set { SetProperty(ref ProductionIncreaseValue, value); }

        } // percent
        private decimal PressureDropReductionValue;

        public decimal PressureDropReduction

        {

            get { return this.PressureDropReductionValue; }

            set { SetProperty(ref PressureDropReductionValue, value); }

        } // psi
        private decimal ReservoirPressureValue;

        public decimal ReservoirPressure

        {

            get { return this.ReservoirPressureValue; }

            set { SetProperty(ref ReservoirPressureValue, value); }

        }
        private decimal BubblePointPressureValue;

        public decimal BubblePointPressure

        {

            get { return this.BubblePointPressureValue; }

            set { SetProperty(ref BubblePointPressureValue, value); }

        }
        private string OptimizationStrategyValue = string.Empty;

        public string OptimizationStrategy

        {

            get { return this.OptimizationStrategyValue; }

            set { SetProperty(ref OptimizationStrategyValue, value); }

        } // Maximize Production, Minimize Backpressure, etc.
        private List<ChokeOpeningPoint> OpeningCurveValue = new();

        public List<ChokeOpeningPoint> OpeningCurve

        {

            get { return this.OpeningCurveValue; }

            set { SetProperty(ref OpeningCurveValue, value); }

        }
    }

    /// <summary>
    /// Individual data point for choke opening vs. production curve.
    /// </summary>
    public class ChokeOpeningPoint : ModelEntityBase
    {
        private decimal ChokeDiameterValue;

        public decimal ChokeDiameter

        {

            get { return this.ChokeDiameterValue; }

            set { SetProperty(ref ChokeDiameterValue, value); }

        }
        private decimal ProductionRateValue;

        public decimal ProductionRate

        {

            get { return this.ProductionRateValue; }

            set { SetProperty(ref ProductionRateValue, value); }

        }
        private decimal PressureDropValue;

        public decimal PressureDrop

        {

            get { return this.PressureDropValue; }

            set { SetProperty(ref PressureDropValue, value); }

        }
        private decimal EfficiencyValue;

        public decimal Efficiency

        {

            get { return this.EfficiencyValue; }

            set { SetProperty(ref EfficiencyValue, value); }

        }
    }

    /// <summary>
    /// DTO for artificial lift choke interaction analysis.
    /// Shows how choke affects ESP, GasLift, or Sucker Rod systems.
    /// </summary>
    public class LiftSystemChokeInteraction : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private string LiftSystemTypeValue = string.Empty;

        public string LiftSystemType

        {

            get { return this.LiftSystemTypeValue; }

            set { SetProperty(ref LiftSystemTypeValue, value); }

        } // ESP, GasLift, SuckerRod, Plunger
        private decimal CurrentChokeSizeValue;

        public decimal CurrentChokeSize

        {

            get { return this.CurrentChokeSizeValue; }

            set { SetProperty(ref CurrentChokeSizeValue, value); }

        }
        private decimal CurrentDischargeValue;

        public decimal CurrentDischarge

        {

            get { return this.CurrentDischargeValue; }

            set { SetProperty(ref CurrentDischargeValue, value); }

        }
        private decimal LiftSystemPowerValue;

        public decimal LiftSystemPower

        {

            get { return this.LiftSystemPowerValue; }

            set { SetProperty(ref LiftSystemPowerValue, value); }

        } // HP or scf/day
        private decimal RequiredHeadOrPressureValue;

        public decimal RequiredHeadOrPressure

        {

            get { return this.RequiredHeadOrPressureValue; }

            set { SetProperty(ref RequiredHeadOrPressureValue, value); }

        }
        private decimal ChokeBackPressureValue;

        public decimal ChokeBackPressure

        {

            get { return this.ChokeBackPressureValue; }

            set { SetProperty(ref ChokeBackPressureValue, value); }

        }
        private decimal SystemEfficiencyValue;

        public decimal SystemEfficiency

        {

            get { return this.SystemEfficiencyValue; }

            set { SetProperty(ref SystemEfficiencyValue, value); }

        }
        private decimal OptimalChokeSizeValue;

        public decimal OptimalChokeSize

        {

            get { return this.OptimalChokeSizeValue; }

            set { SetProperty(ref OptimalChokeSizeValue, value); }

        }
        private decimal EfficiencyGainValue;

        public decimal EfficiencyGain

        {

            get { return this.EfficiencyGainValue; }

            set { SetProperty(ref EfficiencyGainValue, value); }

        }
        private decimal PowerSavingsValue;

        public decimal PowerSavings

        {

            get { return this.PowerSavingsValue; }

            set { SetProperty(ref PowerSavingsValue, value); }

        } // HP or scf/day
        private string RecommendationValue = string.Empty;

        public string Recommendation

        {

            get { return this.RecommendationValue; }

            set { SetProperty(ref RecommendationValue, value); }

        }
        private List<string> OperatingConstraintsValue = new();

        public List<string> OperatingConstraints

        {

            get { return this.OperatingConstraintsValue; }

            set { SetProperty(ref OperatingConstraintsValue, value); }

        }
    }

    /// <summary>
    /// DTO for well nodal analysis with choke effects.
    /// Integrates IPR, VLP (tubing), and choke into unified analysis.
    /// </summary>
    public class ChokeNodalAnalysis : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private string NodalPointValue = string.Empty;

        public string NodalPoint

        {

            get { return this.NodalPointValue; }

            set { SetProperty(ref NodalPointValue, value); }

        } // Reservoir, Downhole, Wellhead, Surface
        private decimal ReservoirPressureValue;

        public decimal ReservoirPressure

        {

            get { return this.ReservoirPressureValue; }

            set { SetProperty(ref ReservoirPressureValue, value); }

        }
        private decimal BubblePointPressureValue;

        public decimal BubblePointPressure

        {

            get { return this.BubblePointPressureValue; }

            set { SetProperty(ref BubblePointPressureValue, value); }

        }
        private decimal TubingHeadPressureValue;

        public decimal TubingHeadPressure

        {

            get { return this.TubingHeadPressureValue; }

            set { SetProperty(ref TubingHeadPressureValue, value); }

        }
        private decimal SeparatorPressureValue;

        public decimal SeparatorPressure

        {

            get { return this.SeparatorPressureValue; }

            set { SetProperty(ref SeparatorPressureValue, value); }

        }
        private decimal CurrentProductionValue;

        public decimal CurrentProduction

        {

            get { return this.CurrentProductionValue; }

            set { SetProperty(ref CurrentProductionValue, value); }

        }
        private decimal OptimalProductionValue;

        public decimal OptimalProduction

        {

            get { return this.OptimalProductionValue; }

            set { SetProperty(ref OptimalProductionValue, value); }

        }
        private decimal ChokeBackPressureValue;

        public decimal ChokeBackPressure

        {

            get { return this.ChokeBackPressureValue; }

            set { SetProperty(ref ChokeBackPressureValue, value); }

        }
        private decimal TubingFrictionLossValue;

        public decimal TubingFrictionLoss

        {

            get { return this.TubingFrictionLossValue; }

            set { SetProperty(ref TubingFrictionLossValue, value); }

        }
        private decimal ElevationChangeValue;

        public decimal ElevationChange

        {

            get { return this.ElevationChangeValue; }

            set { SetProperty(ref ElevationChangeValue, value); }

        }
        private decimal AccelerationLossValue;

        public decimal AccelerationLoss

        {

            get { return this.AccelerationLossValue; }

            set { SetProperty(ref AccelerationLossValue, value); }

        }
        private string ConstrainedByValue = string.Empty;

        public string ConstrainedBy

        {

            get { return this.ConstrainedByValue; }

            set { SetProperty(ref ConstrainedByValue, value); }

        } // Reservoir, Choke, Tubing, Separator
        private string RecommendationValue = string.Empty;

        public string Recommendation

        {

            get { return this.RecommendationValue; }

            set { SetProperty(ref RecommendationValue, value); }

        }
        private List<NodalPoint> NodalPointDataValue = new();

        public List<NodalPoint> NodalPointData

        {

            get { return this.NodalPointDataValue; }

            set { SetProperty(ref NodalPointDataValue, value); }

        }
    }

    /// <summary>
    /// Individual nodal point data for performance analysis.
    /// </summary>
    public class NodalPoint : ModelEntityBase
    {
        private string PointNameValue = string.Empty;

        public string PointName

        {

            get { return this.PointNameValue; }

            set { SetProperty(ref PointNameValue, value); }

        }
        private decimal PressureValue;

        public decimal Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }
        private decimal FlowRateValue;

        public decimal FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        }
        private decimal RestrictionTypeValue;

        public decimal RestrictionType

        {

            get { return this.RestrictionTypeValue; }

            set { SetProperty(ref RestrictionTypeValue, value); }

        } // 0=Capacity, 1=Restriction
    }

    /// <summary>
    /// DTO for pressure and temperature transient effects on choke flow.
    /// Analyzes how transient conditions affect performance.
    /// </summary>
    public class ChokeTransientAnalysis : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private decimal InitialUpstreamPressureValue;

        public decimal InitialUpstreamPressure

        {

            get { return this.InitialUpstreamPressureValue; }

            set { SetProperty(ref InitialUpstreamPressureValue, value); }

        }
        private decimal InitialTemperatureValue;

        public decimal InitialTemperature

        {

            get { return this.InitialTemperatureValue; }

            set { SetProperty(ref InitialTemperatureValue, value); }

        }
        private decimal FinalUpstreamPressureValue;

        public decimal FinalUpstreamPressure

        {

            get { return this.FinalUpstreamPressureValue; }

            set { SetProperty(ref FinalUpstreamPressureValue, value); }

        }
        private decimal FinalTemperatureValue;

        public decimal FinalTemperature

        {

            get { return this.FinalTemperatureValue; }

            set { SetProperty(ref FinalTemperatureValue, value); }

        }
        private decimal ChangeRateValue;

        public decimal ChangeRate

        {

            get { return this.ChangeRateValue; }

            set { SetProperty(ref ChangeRateValue, value); }

        } // psi/hour or °R/hour
        private decimal TransientDurationValue;

        public decimal TransientDuration

        {

            get { return this.TransientDurationValue; }

            set { SetProperty(ref TransientDurationValue, value); }

        } // hours
        private decimal AverageFlowRateValue;

        public decimal AverageFlowRate

        {

            get { return this.AverageFlowRateValue; }

            set { SetProperty(ref AverageFlowRateValue, value); }

        }
        private decimal PeakFlowRateValue;

        public decimal PeakFlowRate

        {

            get { return this.PeakFlowRateValue; }

            set { SetProperty(ref PeakFlowRateValue, value); }

        }
        private decimal MinimumFlowRateValue;

        public decimal MinimumFlowRate

        {

            get { return this.MinimumFlowRateValue; }

            set { SetProperty(ref MinimumFlowRateValue, value); }

        }
        private decimal TemperatureEffectValue;

        public decimal TemperatureEffect

        {

            get { return this.TemperatureEffectValue; }

            set { SetProperty(ref TemperatureEffectValue, value); }

        } // psi equivalent
        private decimal PressureEffectValue;

        public decimal PressureEffect

        {

            get { return this.PressureEffectValue; }

            set { SetProperty(ref PressureEffectValue, value); }

        } // psi
        private List<TransientPoint> TransientCurveValue = new();

        public List<TransientPoint> TransientCurve

        {

            get { return this.TransientCurveValue; }

            set { SetProperty(ref TransientCurveValue, value); }

        }
        private string TransientTypeValue = string.Empty;

        public string TransientType

        {

            get { return this.TransientTypeValue; }

            set { SetProperty(ref TransientTypeValue, value); }

        } // Pressure, Temperature, Both
    }

    /// <summary>
    /// Individual data point along transient curve.
    /// </summary>
    public class TransientPoint : ModelEntityBase
    {
        private decimal TimeElapsedValue;

        public decimal TimeElapsed

        {

            get { return this.TimeElapsedValue; }

            set { SetProperty(ref TimeElapsedValue, value); }

        } // hours
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
        private decimal FlowRateValue;

        public decimal FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        }
        private decimal DischargeCoefficientValue;

        public decimal DischargeCoefficient

        {

            get { return this.DischargeCoefficientValue; }

            set { SetProperty(ref DischargeCoefficientValue, value); }

        }
    }

    /// <summary>
    /// DTO for bean choke design and optimization per API RP 43.
    /// </summary>
    public class BeanChokeDesign : ModelEntityBase
    {
        private string DesignIdValue = string.Empty;

        public string DesignId

        {

            get { return this.DesignIdValue; }

            set { SetProperty(ref DesignIdValue, value); }

        }
        private DateTime DesignDateValue;

        public DateTime DesignDate

        {

            get { return this.DesignDateValue; }

            set { SetProperty(ref DesignDateValue, value); }

        }
        private decimal DesiredFlowRateValue;

        public decimal DesiredFlowRate

        {

            get { return this.DesiredFlowRateValue; }

            set { SetProperty(ref DesiredFlowRateValue, value); }

        }
        private decimal UpstreamPressureValue;

        public decimal UpstreamPressure

        {

            get { return this.UpstreamPressureValue; }

            set { SetProperty(ref UpstreamPressureValue, value); }

        }
        private decimal DownstreamPressureValue;

        public decimal DownstreamPressure

        {

            get { return this.DownstreamPressureValue; }

            set { SetProperty(ref DownstreamPressureValue, value); }

        }
        private decimal TemperatureValue;

        public decimal Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        }
        private decimal GasSpecificGravityValue;

        public decimal GasSpecificGravity

        {

            get { return this.GasSpecificGravityValue; }

            set { SetProperty(ref GasSpecificGravityValue, value); }

        }
        private string TrimMaterialValue = string.Empty;

        public string TrimMaterial

        {

            get { return this.TrimMaterialValue; }

            set { SetProperty(ref TrimMaterialValue, value); }

        } // WC, Tungsten Carbide, Steel
        private decimal RecommendedChokeDiameterValue;

        public decimal RecommendedChokeDiameter

        {

            get { return this.RecommendedChokeDiameterValue; }

            set { SetProperty(ref RecommendedChokeDiameterValue, value); }

        }
        private decimal MinimumChokeDiameterValue;

        public decimal MinimumChokeDiameter

        {

            get { return this.MinimumChokeDiameterValue; }

            set { SetProperty(ref MinimumChokeDiameterValue, value); }

        }
        private decimal MaximumChokeDiameterValue;

        public decimal MaximumChokeDiameter

        {

            get { return this.MaximumChokeDiameterValue; }

            set { SetProperty(ref MaximumChokeDiameterValue, value); }

        }
        private decimal DischargeCoefficientValue;

        public decimal DischargeCoefficient

        {

            get { return this.DischargeCoefficientValue; }

            set { SetProperty(ref DischargeCoefficientValue, value); }

        }
        private decimal SurfaceAreaValue;

        public decimal SurfaceArea

        {

            get { return this.SurfaceAreaValue; }

            set { SetProperty(ref SurfaceAreaValue, value); }

        }
        private string RecommendedSeriesValue = string.Empty;

        public string RecommendedSeries

        {

            get { return this.RecommendedSeriesValue; }

            set { SetProperty(ref RecommendedSeriesValue, value); }

        } // AX, BX, CX, DX
        private decimal EstimatedErosionRateValue;

        public decimal EstimatedErosionRate

        {

            get { return this.EstimatedErosionRateValue; }

            set { SetProperty(ref EstimatedErosionRateValue, value); }

        } // mils/year
        private decimal DesignLifeValue;

        public decimal DesignLife

        {

            get { return this.DesignLifeValue; }

            set { SetProperty(ref DesignLifeValue, value); }

        } // years
        private string ManufacturerRecommendationValue = string.Empty;

        public string ManufacturerRecommendation

        {

            get { return this.ManufacturerRecommendationValue; }

            set { SetProperty(ref ManufacturerRecommendationValue, value); }

        }
    }

    /// <summary>
    /// DTO for choke opening vs. production forecasting.
    /// Predicts production decline and required choke adjustments.
    /// </summary>
    public class ChokeProductionForecast : ModelEntityBase
    {
        private string ForecastIdValue = string.Empty;

        public string ForecastId

        {

            get { return this.ForecastIdValue; }

            set { SetProperty(ref ForecastIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private decimal CurrentProductionValue;

        public decimal CurrentProduction

        {

            get { return this.CurrentProductionValue; }

            set { SetProperty(ref CurrentProductionValue, value); }

        }
        private decimal CurrentChokeDiameterValue;

        public decimal CurrentChokeDiameter

        {

            get { return this.CurrentChokeDiameterValue; }

            set { SetProperty(ref CurrentChokeDiameterValue, value); }

        }
        private decimal ReservoirDeclineRateValue;

        public decimal ReservoirDeclineRate

        {

            get { return this.ReservoirDeclineRateValue; }

            set { SetProperty(ref ReservoirDeclineRateValue, value); }

        } // decimal/month
        private int ForecastMonthsValue;

        public int ForecastMonths

        {

            get { return this.ForecastMonthsValue; }

            set { SetProperty(ref ForecastMonthsValue, value); }

        }
        private List<ChokeProductionPoint> ProductionScenariosValue = new();

        public List<ChokeProductionPoint> ProductionScenarios

        {

            get { return this.ProductionScenariosValue; }

            set { SetProperty(ref ProductionScenariosValue, value); }

        }
        private List<ChokeOpeningAdjustment> RecommendedAdjustmentsValue = new();

        public List<ChokeOpeningAdjustment> RecommendedAdjustments

        {

            get { return this.RecommendedAdjustmentsValue; }

            set { SetProperty(ref RecommendedAdjustmentsValue, value); }

        }
        private decimal RequiredChokeOpeningByMonth12Value;

        public decimal RequiredChokeOpeningByMonth12

        {

            get { return this.RequiredChokeOpeningByMonth12Value; }

            set { SetProperty(ref RequiredChokeOpeningByMonth12Value, value); }

        }
        private decimal CumulativeProductionGainValue;

        public decimal CumulativeProductionGain

        {

            get { return this.CumulativeProductionGainValue; }

            set { SetProperty(ref CumulativeProductionGainValue, value); }

        } // BOPD-months
        private string StrategyValue = string.Empty;

        public string Strategy

        {

            get { return this.StrategyValue; }

            set { SetProperty(ref StrategyValue, value); }

        } // Conservative, Moderate, Aggressive
    }

    /// <summary>
    /// Production forecast point with choke size.
    /// </summary>
    public class ChokeProductionPoint : ModelEntityBase
    {
        private int MonthValue;

        public int Month

        {

            get { return this.MonthValue; }

            set { SetProperty(ref MonthValue, value); }

        }
        private decimal ReservoirPressureValue;

        public decimal ReservoirPressure

        {

            get { return this.ReservoirPressureValue; }

            set { SetProperty(ref ReservoirPressureValue, value); }

        }
        private decimal NaturalProductionValue;

        public decimal NaturalProduction

        {

            get { return this.NaturalProductionValue; }

            set { SetProperty(ref NaturalProductionValue, value); }

        }
        private decimal ProductionWithCurrentChokeValue;

        public decimal ProductionWithCurrentChoke

        {

            get { return this.ProductionWithCurrentChokeValue; }

            set { SetProperty(ref ProductionWithCurrentChokeValue, value); }

        }
        private decimal RecommendedChokeDiameterValue;

        public decimal RecommendedChokeDiameter

        {

            get { return this.RecommendedChokeDiameterValue; }

            set { SetProperty(ref RecommendedChokeDiameterValue, value); }

        }
        private decimal ProductionWithOptimalChokeValue;

        public decimal ProductionWithOptimalChoke

        {

            get { return this.ProductionWithOptimalChokeValue; }

            set { SetProperty(ref ProductionWithOptimalChokeValue, value); }

        }
    }

    /// <summary>
    /// Recommended choke opening adjustment.
    /// </summary>
    public class ChokeOpeningAdjustment : ModelEntityBase
    {
        private int MonthValue;

        public int Month

        {

            get { return this.MonthValue; }

            set { SetProperty(ref MonthValue, value); }

        }
        private decimal CurrentChokeDiameterValue;

        public decimal CurrentChokeDiameter

        {

            get { return this.CurrentChokeDiameterValue; }

            set { SetProperty(ref CurrentChokeDiameterValue, value); }

        }
        private decimal AdjustToChokeDiameterValue;

        public decimal AdjustToChokeDiameter

        {

            get { return this.AdjustToChokeDiameterValue; }

            set { SetProperty(ref AdjustToChokeDiameterValue, value); }

        }
        private decimal PercentChangeValue;

        public decimal PercentChange

        {

            get { return this.PercentChangeValue; }

            set { SetProperty(ref PercentChangeValue, value); }

        }
        private string ReasonValue = string.Empty;

        public string Reason

        {

            get { return this.ReasonValue; }

            set { SetProperty(ref ReasonValue, value); }

        }
        private decimal ExpectedProductionChangeValue;

        public decimal ExpectedProductionChange

        {

            get { return this.ExpectedProductionChangeValue; }

            set { SetProperty(ref ExpectedProductionChangeValue, value); }

        }
    }

    /// <summary>
    /// DTO for venturi choke analysis.
    /// Specialized model for venturi-type chokes with recovery sections.
    /// </summary>
    public class VenturiChokeAnalysis : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private decimal ThroatDiameterValue;

        public decimal ThroatDiameter

        {

            get { return this.ThroatDiameterValue; }

            set { SetProperty(ref ThroatDiameterValue, value); }

        }
        private decimal UpstreamDiameterValue;

        public decimal UpstreamDiameter

        {

            get { return this.UpstreamDiameterValue; }

            set { SetProperty(ref UpstreamDiameterValue, value); }

        }
        private decimal DownstreamDiameterValue;

        public decimal DownstreamDiameter

        {

            get { return this.DownstreamDiameterValue; }

            set { SetProperty(ref DownstreamDiameterValue, value); }

        }
        private decimal RecoveryLengthValue;

        public decimal RecoveryLength

        {

            get { return this.RecoveryLengthValue; }

            set { SetProperty(ref RecoveryLengthValue, value); }

        }
        private decimal UpstreamPressureValue;

        public decimal UpstreamPressure

        {

            get { return this.UpstreamPressureValue; }

            set { SetProperty(ref UpstreamPressureValue, value); }

        }
        private decimal ThroatPressureValue;

        public decimal ThroatPressure

        {

            get { return this.ThroatPressureValue; }

            set { SetProperty(ref ThroatPressureValue, value); }

        }
        private decimal DownstreamPressureValue;

        public decimal DownstreamPressure

        {

            get { return this.DownstreamPressureValue; }

            set { SetProperty(ref DownstreamPressureValue, value); }

        }
        private decimal RecoveryPressureValue;

        public decimal RecoveryPressure

        {

            get { return this.RecoveryPressureValue; }

            set { SetProperty(ref RecoveryPressureValue, value); }

        }
        private decimal GasFlowRateValue;

        public decimal GasFlowRate

        {

            get { return this.GasFlowRateValue; }

            set { SetProperty(ref GasFlowRateValue, value); }

        }
        private decimal ThroatVelocityValue;

        public decimal ThroatVelocity

        {

            get { return this.ThroatVelocityValue; }

            set { SetProperty(ref ThroatVelocityValue, value); }

        }
        private decimal RecoveryFractionValue;

        public decimal RecoveryFraction

        {

            get { return this.RecoveryFractionValue; }

            set { SetProperty(ref RecoveryFractionValue, value); }

        }
        private decimal EffectivePressureDropValue;

        public decimal EffectivePressureDrop

        {

            get { return this.EffectivePressureDropValue; }

            set { SetProperty(ref EffectivePressureDropValue, value); }

        }
        private decimal CoefficientOfRecoveryValue;

        public decimal CoefficientOfRecovery

        {

            get { return this.CoefficientOfRecoveryValue; }

            set { SetProperty(ref CoefficientOfRecoveryValue, value); }

        }
        private string AdvantageValue = string.Empty;

        public string Advantage

        {

            get { return this.AdvantageValue; }

            set { SetProperty(ref AdvantageValue, value); }

        } // Lower erosion, Higher recovery, etc.
    }

    /// <summary>
    /// DTO for trim and material selection recommendations.
    /// Based on well conditions and erosion predictions.
    /// </summary>
    public class ChokeTrimSelection : ModelEntityBase
    {
        private string SelectionIdValue = string.Empty;

        public string SelectionId

        {

            get { return this.SelectionIdValue; }

            set { SetProperty(ref SelectionIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private string ChokeTypeValue = string.Empty;

        public string ChokeType

        {

            get { return this.ChokeTypeValue; }

            set { SetProperty(ref ChokeTypeValue, value); }

        } // Bean, Venturi, Orifice
        private decimal DesiredFlowRateValue;

        public decimal DesiredFlowRate

        {

            get { return this.DesiredFlowRateValue; }

            set { SetProperty(ref DesiredFlowRateValue, value); }

        }
        private decimal ExpectedSandRateValue;

        public decimal ExpectedSandRate

        {

            get { return this.ExpectedSandRateValue; }

            set { SetProperty(ref ExpectedSandRateValue, value); }

        }
        private decimal PredictedErosionRateValue;

        public decimal PredictedErosionRate

        {

            get { return this.PredictedErosionRateValue; }

            set { SetProperty(ref PredictedErosionRateValue, value); }

        }
        private List<TrimOption> TrimOptionsValue = new();

        public List<TrimOption> TrimOptions

        {

            get { return this.TrimOptionsValue; }

            set { SetProperty(ref TrimOptionsValue, value); }

        }
        private string RecommendedTrimValue = string.Empty;

        public string RecommendedTrim

        {

            get { return this.RecommendedTrimValue; }

            set { SetProperty(ref RecommendedTrimValue, value); }

        }
        private string RecommendedMaterialValue = string.Empty;

        public string RecommendedMaterial

        {

            get { return this.RecommendedMaterialValue; }

            set { SetProperty(ref RecommendedMaterialValue, value); }

        } // WC, Tungsten Carbide, 17-4 Steel
        private decimal EstimatedChokeLifeValue;

        public decimal EstimatedChokeLife

        {

            get { return this.EstimatedChokeLifeValue; }

            set { SetProperty(ref EstimatedChokeLifeValue, value); }

        } // years
        private decimal CostPerChokeValue;

        public decimal CostPerChoke

        {

            get { return this.CostPerChokeValue; }

            set { SetProperty(ref CostPerChokeValue, value); }

        }
        private decimal LifetimeCostValue;

        public decimal LifetimeCost

        {

            get { return this.LifetimeCostValue; }

            set { SetProperty(ref LifetimeCostValue, value); }

        } // $/year
        private string JustificationValue = string.Empty;

        public string Justification

        {

            get { return this.JustificationValue; }

            set { SetProperty(ref JustificationValue, value); }

        }
    }

    /// <summary>
    /// Individual trim option for selection analysis.
    /// </summary>
    public class TrimOption : ModelEntityBase
    {
        private string TrimSizeValue = string.Empty;

        public string TrimSize

        {

            get { return this.TrimSizeValue; }

            set { SetProperty(ref TrimSizeValue, value); }

        } // AX, BX, CX, DX
        private decimal ChokeDiameterValue;

        public decimal ChokeDiameter

        {

            get { return this.ChokeDiameterValue; }

            set { SetProperty(ref ChokeDiameterValue, value); }

        }
        private decimal FlowCapacityValue;

        public decimal FlowCapacity

        {

            get { return this.FlowCapacityValue; }

            set { SetProperty(ref FlowCapacityValue, value); }

        }
        private string MaterialValue = string.Empty;

        public string Material

        {

            get { return this.MaterialValue; }

            set { SetProperty(ref MaterialValue, value); }

        }
        private decimal EstimatedLifeValue;

        public decimal EstimatedLife

        {

            get { return this.EstimatedLifeValue; }

            set { SetProperty(ref EstimatedLifeValue, value); }

        }
        private decimal CostValue;

        public decimal Cost

        {

            get { return this.CostValue; }

            set { SetProperty(ref CostValue, value); }

        }
        private decimal ErosionRatingValue;

        public decimal ErosionRating

        {

            get { return this.ErosionRatingValue; }

            set { SetProperty(ref ErosionRatingValue, value); }

        } // 0-100, 100=Best
        private string ProsValue = string.Empty;

        public string Pros

        {

            get { return this.ProsValue; }

            set { SetProperty(ref ProsValue, value); }

        }
        private string ConsValue = string.Empty;

        public string Cons

        {

            get { return this.ConsValue; }

            set { SetProperty(ref ConsValue, value); }

        }
    }

    /// <summary>
    /// DTO for choke performance monitoring and diagnostics.
    /// Identifies operational issues and performance degradation.
    /// </summary>
    public class ChokePerformanceDiagnostics : ModelEntityBase
    {
        private string DiagnosticsIdValue = string.Empty;

        public string DiagnosticsId

        {

            get { return this.DiagnosticsIdValue; }

            set { SetProperty(ref DiagnosticsIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private decimal MeasuredFlowRateValue;

        public decimal MeasuredFlowRate

        {

            get { return this.MeasuredFlowRateValue; }

            set { SetProperty(ref MeasuredFlowRateValue, value); }

        }
        private decimal ExpectedFlowRateValue;

        public decimal ExpectedFlowRate

        {

            get { return this.ExpectedFlowRateValue; }

            set { SetProperty(ref ExpectedFlowRateValue, value); }

        }
        private decimal FlowDeviationValue;

        public decimal FlowDeviation

        {

            get { return this.FlowDeviationValue; }

            set { SetProperty(ref FlowDeviationValue, value); }

        } // percent
        private decimal MeasuredDownstreamPressureValue;

        public decimal MeasuredDownstreamPressure

        {

            get { return this.MeasuredDownstreamPressureValue; }

            set { SetProperty(ref MeasuredDownstreamPressureValue, value); }

        }
        private decimal ExpectedDownstreamPressureValue;

        public decimal ExpectedDownstreamPressure

        {

            get { return this.ExpectedDownstreamPressureValue; }

            set { SetProperty(ref ExpectedDownstreamPressureValue, value); }

        }
        private decimal PressureDeviationValue;

        public decimal PressureDeviation

        {

            get { return this.PressureDeviationValue; }

            set { SetProperty(ref PressureDeviationValue, value); }

        } // psi
        private string StatusCodeValue = string.Empty;

        public string StatusCode

        {

            get { return this.StatusCodeValue; }

            set { SetProperty(ref StatusCodeValue, value); }

        } // Normal, Warning, Critical
        private List<string> IdentifiedIssuesValue = new();

        public List<string> IdentifiedIssues

        {

            get { return this.IdentifiedIssuesValue; }

            set { SetProperty(ref IdentifiedIssuesValue, value); }

        }
        private List<string> DiagnosticsDetailsValue = new();

        public List<string> DiagnosticsDetails

        {

            get { return this.DiagnosticsDetailsValue; }

            set { SetProperty(ref DiagnosticsDetailsValue, value); }

        }
        private decimal DischargeCoefficientDegradationValue;

        public decimal DischargeCoefficientDegradation

        {

            get { return this.DischargeCoefficientDegradationValue; }

            set { SetProperty(ref DischargeCoefficientDegradationValue, value); }

        } // percent
        private string ProbableCauseValue = string.Empty;

        public string ProbableCause

        {

            get { return this.ProbableCauseValue; }

            set { SetProperty(ref ProbableCauseValue, value); }

        }
        private string RecommendedActionValue = string.Empty;

        public string RecommendedAction

        {

            get { return this.RecommendedActionValue; }

            set { SetProperty(ref RecommendedActionValue, value); }

        }
    }

    /// <summary>
    /// DTO for choke sand cut risk assessment.
    /// Predicts sand production impact and migration through choke.
    /// </summary>
    public class ChokeSandCutRiskAssessment : ModelEntityBase
    {
        private string AssessmentIdValue = string.Empty;

        public string AssessmentId

        {

            get { return this.AssessmentIdValue; }

            set { SetProperty(ref AssessmentIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private decimal EstimatedSandRateValue;

        public decimal EstimatedSandRate

        {

            get { return this.EstimatedSandRateValue; }

            set { SetProperty(ref EstimatedSandRateValue, value); }

        } // lb/day
        private decimal SandGrainSizeValue;

        public decimal SandGrainSize

        {

            get { return this.SandGrainSizeValue; }

            set { SetProperty(ref SandGrainSizeValue, value); }

        } // microns
        private decimal WellProdDepthValue;

        public decimal WellProdDepth

        {

            get { return this.WellProdDepthValue; }

            set { SetProperty(ref WellProdDepthValue, value); }

        }
        private decimal ChokeDepthValue;

        public decimal ChokeDepth

        {

            get { return this.ChokeDepthValue; }

            set { SetProperty(ref ChokeDepthValue, value); }

        }
        private decimal SettlingVelocityValue;

        public decimal SettlingVelocity

        {

            get { return this.SettlingVelocityValue; }

            set { SetProperty(ref SettlingVelocityValue, value); }

        } // ft/sec
        private decimal FlowVelocityValue;

        public decimal FlowVelocity

        {

            get { return this.FlowVelocityValue; }

            set { SetProperty(ref FlowVelocityValue, value); }

        } // ft/sec
        private decimal SandMigrationRiskValue;

        public decimal SandMigrationRisk

        {

            get { return this.SandMigrationRiskValue; }

            set { SetProperty(ref SandMigrationRiskValue, value); }

        } // 0-100 scale
        private string SandStatusValue = string.Empty;

        public string SandStatus

        {

            get { return this.SandStatusValue; }

            set { SetProperty(ref SandStatusValue, value); }

        } // Low, Moderate, High, Severe
        private List<string> SandMigrationPointsValue = new();

        public List<string> SandMigrationPoints

        {

            get { return this.SandMigrationPointsValue; }

            set { SetProperty(ref SandMigrationPointsValue, value); }

        }
        private decimal PredictedChokeDamageRateValue;

        public decimal PredictedChokeDamageRate

        {

            get { return this.PredictedChokeDamageRateValue; }

            set { SetProperty(ref PredictedChokeDamageRateValue, value); }

        } // mils/year
        private int DaysUntilChokeReplacementValue;

        public int DaysUntilChokeReplacement

        {

            get { return this.DaysUntilChokeReplacementValue; }

            set { SetProperty(ref DaysUntilChokeReplacementValue, value); }

        }
        private string RecommendationValue = string.Empty;

        public string Recommendation

        {

            get { return this.RecommendationValue; }

            set { SetProperty(ref RecommendationValue, value); }

        }
    }

    /// <summary>
    /// DTO for temperature effect analysis on choke flow.
    /// Shows how temperature changes impact flow rates and efficiency.
    /// </summary>
    public class ChokeTemperatureEffectAnalysis : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private decimal BaselineTemperatureValue;

        public decimal BaselineTemperature

        {

            get { return this.BaselineTemperatureValue; }

            set { SetProperty(ref BaselineTemperatureValue, value); }

        }
        private decimal BaselineFlowRateValue;

        public decimal BaselineFlowRate

        {

            get { return this.BaselineFlowRateValue; }

            set { SetProperty(ref BaselineFlowRateValue, value); }

        }
        private decimal TemperatureChangeRangeValue;

        public decimal TemperatureChangeRange

        {

            get { return this.TemperatureChangeRangeValue; }

            set { SetProperty(ref TemperatureChangeRangeValue, value); }

        } // °R
        private List<TemperatureFlowPoint> TemperatureEffectCurveValue = new();

        public List<TemperatureFlowPoint> TemperatureEffectCurve

        {

            get { return this.TemperatureEffectCurveValue; }

            set { SetProperty(ref TemperatureEffectCurveValue, value); }

        }
        private decimal FlowSensitivityValue;

        public decimal FlowSensitivity

        {

            get { return this.FlowSensitivityValue; }

            set { SetProperty(ref FlowSensitivityValue, value); }

        } // %change/°R
        private decimal PressureDropSensitivityValue;

        public decimal PressureDropSensitivity

        {

            get { return this.PressureDropSensitivityValue; }

            set { SetProperty(ref PressureDropSensitivityValue, value); }

        } // psi/°R
        private decimal DischargeCoefficientTemperatureCoeffValue;

        public decimal DischargeCoefficientTemperatureCoeff

        {

            get { return this.DischargeCoefficientTemperatureCoeffValue; }

            set { SetProperty(ref DischargeCoefficientTemperatureCoeffValue, value); }

        } // 1/°R
        private string TemperatureControlRecommendationValue = string.Empty;

        public string TemperatureControlRecommendation

        {

            get { return this.TemperatureControlRecommendationValue; }

            set { SetProperty(ref TemperatureControlRecommendationValue, value); }

        }
    }

    /// <summary>
    /// Individual data point for temperature vs. flow analysis.
    /// </summary>
    public class TemperatureFlowPoint : ModelEntityBase
    {
        private decimal TemperatureValue;

        public decimal Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        }
        private decimal FlowRateValue;

        public decimal FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        }
        private decimal PressureDropValue;

        public decimal PressureDrop

        {

            get { return this.PressureDropValue; }

            set { SetProperty(ref PressureDropValue, value); }

        }
        private decimal DischargeCoefficientValue;

        public decimal DischargeCoefficient

        {

            get { return this.DischargeCoefficientValue; }

            set { SetProperty(ref DischargeCoefficientValue, value); }

        }
        private decimal EfficiencyValue;

        public decimal Efficiency

        {

            get { return this.EfficiencyValue; }

            set { SetProperty(ref EfficiencyValue, value); }

        }
    }

    /// <summary>
    /// DTO for comprehensive choke system analysis report.
    /// Combines multiple analyses into unified well overview.
    /// </summary>
    public class ChokeSystemComprehensiveReport : ModelEntityBase
    {
        private string ReportIdValue = string.Empty;

        public string ReportId

        {

            get { return this.ReportIdValue; }

            set { SetProperty(ref ReportIdValue, value); }

        }
        private DateTime ReportDateValue;

        public DateTime ReportDate

        {

            get { return this.ReportDateValue; }

            set { SetProperty(ref ReportDateValue, value); }

        }
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private string OperatingStatusValue = string.Empty;

        public string OperatingStatus

        {

            get { return this.OperatingStatusValue; }

            set { SetProperty(ref OperatingStatusValue, value); }

        } // Active, Shut-in, Suspended
        
        // Current conditions
        private AdvancedChokeAnalysis CurrentChokeAnalysisValue = new();

        public AdvancedChokeAnalysis CurrentChokeAnalysis

        {

            get { return this.CurrentChokeAnalysisValue; }

            set { SetProperty(ref CurrentChokeAnalysisValue, value); }

        }
        private ChokePerformanceDiagnostics PerformanceDiagnosticsValue = new();

        public ChokePerformanceDiagnostics PerformanceDiagnostics

        {

            get { return this.PerformanceDiagnosticsValue; }

            set { SetProperty(ref PerformanceDiagnosticsValue, value); }

        }
        private ChokeErosionPrediction ErosionPredictionValue = new();

        public ChokeErosionPrediction ErosionPrediction

        {

            get { return this.ErosionPredictionValue; }

            set { SetProperty(ref ErosionPredictionValue, value); }

        }
        private ChokeSandCutRiskAssessment SandRiskAssessmentValue = new();

        public ChokeSandCutRiskAssessment SandRiskAssessment

        {

            get { return this.SandRiskAssessmentValue; }

            set { SetProperty(ref SandRiskAssessmentValue, value); }

        }
        
        // Optimization
        private ChokeBackPressureOptimization OptimizationAnalysisValue = new();

        public ChokeBackPressureOptimization OptimizationAnalysis

        {

            get { return this.OptimizationAnalysisValue; }

            set { SetProperty(ref OptimizationAnalysisValue, value); }

        }
        private ChokeProductionForecast ProductionForecastValue = new();

        public ChokeProductionForecast ProductionForecast

        {

            get { return this.ProductionForecastValue; }

            set { SetProperty(ref ProductionForecastValue, value); }

        }
        
        // Equipment
        private ChokeTrimSelection EquipmentRecommendationValue = new();

        public ChokeTrimSelection EquipmentRecommendation

        {

            get { return this.EquipmentRecommendationValue; }

            set { SetProperty(ref EquipmentRecommendationValue, value); }

        }
        
        // Overall summary
        private decimal CurrentProductionValue;

        public decimal CurrentProduction

        {

            get { return this.CurrentProductionValue; }

            set { SetProperty(ref CurrentProductionValue, value); }

        }
        private decimal OptimizedProductionValue;

        public decimal OptimizedProduction

        {

            get { return this.OptimizedProductionValue; }

            set { SetProperty(ref OptimizedProductionValue, value); }

        }
        private decimal ProductionPotentialValue;

        public decimal ProductionPotential

        {

            get { return this.ProductionPotentialValue; }

            set { SetProperty(ref ProductionPotentialValue, value); }

        } // percent increase
        private string OverallHealthStatusValue = string.Empty;

        public string OverallHealthStatus

        {

            get { return this.OverallHealthStatusValue; }

            set { SetProperty(ref OverallHealthStatusValue, value); }

        } // Excellent, Good, Fair, Poor
        private List<string> KeyFindingsValue = new();

        public List<string> KeyFindings

        {

            get { return this.KeyFindingsValue; }

            set { SetProperty(ref KeyFindingsValue, value); }

        }
        private List<string> PriorityActionsValue = new();

        public List<string> PriorityActions

        {

            get { return this.PriorityActionsValue; }

            set { SetProperty(ref PriorityActionsValue, value); }

        }
        private decimal EstimatedRevenueImpactValue;

        public decimal EstimatedRevenueImpact

        {

            get { return this.EstimatedRevenueImpactValue; }

            set { SetProperty(ref EstimatedRevenueImpactValue, value); }

        } // $/day
    }
    /// <summary>
    /// Request for Choke Analysis calculation
    /// </summary>
    public class ChokeAnalysisRequest : ModelEntityBase
    {
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string? EquipmentIdValue;

        public string? EquipmentId

        {

            get { return this.EquipmentIdValue; }

            set { SetProperty(ref EquipmentIdValue, value); }

        } // WELL_EQUIPMENT ROW_ID
        private string AnalysisTypeValue = "DOWNHOLE";

        public string AnalysisType

        {

            get { return this.AnalysisTypeValue; }

            set { SetProperty(ref AnalysisTypeValue, value); }

        } // DOWNHOLE, UPHOLE, SIZING, PRESSURE
        
        // Choke properties (optional, will be retrieved from WELL_EQUIPMENT if not provided)
        private decimal? ChokeDiameterValue;

        public decimal? ChokeDiameter

        {

            get { return this.ChokeDiameterValue; }

            set { SetProperty(ref ChokeDiameterValue, value); }

        } // inches
        private string? ChokeTypeValue;

        public string? ChokeType

        {

            get { return this.ChokeTypeValue; }

            set { SetProperty(ref ChokeTypeValue, value); }

        } // BEAN, ADJUSTABLE, POSITIVE
        private decimal? DischargeCoefficientValue;

        public decimal? DischargeCoefficient

        {

            get { return this.DischargeCoefficientValue; }

            set { SetProperty(ref DischargeCoefficientValue, value); }

        }
        
        // Gas properties (optional, will be retrieved from WELL if not provided)
        private decimal? UpstreamPressureValue;

        public decimal? UpstreamPressure

        {

            get { return this.UpstreamPressureValue; }

            set { SetProperty(ref UpstreamPressureValue, value); }

        } // psia
        private decimal? DownstreamPressureValue;

        public decimal? DownstreamPressure

        {

            get { return this.DownstreamPressureValue; }

            set { SetProperty(ref DownstreamPressureValue, value); }

        } // psia
        private decimal? TemperatureValue;

        public decimal? Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        } // Rankine
        private decimal? GasSpecificGravityValue;

        public decimal? GasSpecificGravity

        {

            get { return this.GasSpecificGravityValue; }

            set { SetProperty(ref GasSpecificGravityValue, value); }

        }
        private decimal? ZFactorValue;

        public decimal? ZFactor

        {

            get { return this.ZFactorValue; }

            set { SetProperty(ref ZFactorValue, value); }

        }
        private decimal? FlowRateValue;

        public decimal? FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        } // Mscf/day (for pressure calculation)
        
        // Additional parameters
        public ChokeAnalysisOptions? AdditionalParameters { get; set; }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Result of Choke Analysis calculation
    /// </summary>
    public class ChokeAnalysisResult : ModelEntityBase
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
        private string? EquipmentIdValue;

        public string? EquipmentId

        {

            get { return this.EquipmentIdValue; }

            set { SetProperty(ref EquipmentIdValue, value); }

        }
        private string AnalysisTypeValue = string.Empty;

        public string AnalysisType

        {

            get { return this.AnalysisTypeValue; }

            set { SetProperty(ref AnalysisTypeValue, value); }

        }
        private DateTime CalculationDateValue;

        public DateTime CalculationDate

        {

            get { return this.CalculationDateValue; }

            set { SetProperty(ref CalculationDateValue, value); }

        }
        
        // Choke properties used
        private decimal ChokeDiameterValue;

        public decimal ChokeDiameter

        {

            get { return this.ChokeDiameterValue; }

            set { SetProperty(ref ChokeDiameterValue, value); }

        } // inches
        private string ChokeTypeValue = string.Empty;

        public string ChokeType

        {

            get { return this.ChokeTypeValue; }

            set { SetProperty(ref ChokeTypeValue, value); }

        }
        private decimal DischargeCoefficientValue;

        public decimal DischargeCoefficient

        {

            get { return this.DischargeCoefficientValue; }

            set { SetProperty(ref DischargeCoefficientValue, value); }

        }
        
        // Flow results
        private decimal FlowRateValue;

        public decimal FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        } // Mscf/day
        private decimal UpstreamPressureValue;

        public decimal UpstreamPressure

        {

            get { return this.UpstreamPressureValue; }

            set { SetProperty(ref UpstreamPressureValue, value); }

        } // psia
        private decimal DownstreamPressureValue;

        public decimal DownstreamPressure

        {

            get { return this.DownstreamPressureValue; }

            set { SetProperty(ref DownstreamPressureValue, value); }

        } // psia
        private decimal PressureRatioValue;

        public decimal PressureRatio

        {

            get { return this.PressureRatioValue; }

            set { SetProperty(ref PressureRatioValue, value); }

        }
        private string FlowRegimeValue = string.Empty;

        public string FlowRegime

        {

            get { return this.FlowRegimeValue; }

            set { SetProperty(ref FlowRegimeValue, value); }

        } // SONIC, SUBSONIC
        private decimal CriticalPressureRatioValue;

        public decimal CriticalPressureRatio

        {

            get { return this.CriticalPressureRatioValue; }

            set { SetProperty(ref CriticalPressureRatioValue, value); }

        }
        
        // Additional metadata
        public ChokeAnalysisAdditionalResults? AdditionalResults { get; set; }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        } // SUCCESS, FAILED
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
    }
}




