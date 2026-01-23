using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    #region Core Property DTOs

    /// <summary>
    /// Base property calculation result DTO
    /// </summary>
    public class PropertyResult : ModelEntityBase
    {
        private string CalculationIdValue = string.Empty;

        public string CalculationId

        {

            get { return this.CalculationIdValue; }

            set { SetProperty(ref CalculationIdValue, value); }

        }
        private string PropertyTypeValue = string.Empty;

        public string PropertyType

        {

            get { return this.PropertyTypeValue; }

            set { SetProperty(ref PropertyTypeValue, value); }

        }
        private decimal ValueValue;

        public decimal Value

        {

            get { return this.ValueValue; }

            set { SetProperty(ref ValueValue, value); }

        }
        private string UnitValue = string.Empty;

        public string Unit

        {

            get { return this.UnitValue; }

            set { SetProperty(ref UnitValue, value); }

        }
        private string CorrelationUsedValue = string.Empty;

        public string CorrelationUsed

        {

            get { return this.CorrelationUsedValue; }

            set { SetProperty(ref CorrelationUsedValue, value); }

        }
        private DateTime CalculationDateValue;

        public DateTime CalculationDate

        {

            get { return this.CalculationDateValue; }

            set { SetProperty(ref CalculationDateValue, value); }

        }
        public Dictionary<string, decimal> InputParameters { get; set; } = new();
        private decimal? UncertaintyValue;

        public decimal? Uncertainty

        {

            get { return this.UncertaintyValue; }

            set { SetProperty(ref UncertaintyValue, value); }

        }
        private string NotesValue = string.Empty;

        public string Notes

        {

            get { return this.NotesValue; }

            set { SetProperty(ref NotesValue, value); }

        }
    }

    /// <summary>
    /// Fluid property analysis DTO
    /// </summary>
    public class FluidPropertyAnalysis : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private string SampleIdValue = string.Empty;

        public string SampleId

        {

            get { return this.SampleIdValue; }

            set { SetProperty(ref SampleIdValue, value); }

        }
        private string FluidTypeValue = string.Empty;

        public string FluidType

        {

            get { return this.FluidTypeValue; }

            set { SetProperty(ref FluidTypeValue, value); }

        } // Oil, Gas, Condensate
        private decimal ReservoirTemperatureValue;

        public decimal ReservoirTemperature

        {

            get { return this.ReservoirTemperatureValue; }

            set { SetProperty(ref ReservoirTemperatureValue, value); }

        }
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
        private decimal DewPointPressureValue;

        public decimal DewPointPressure

        {

            get { return this.DewPointPressureValue; }

            set { SetProperty(ref DewPointPressureValue, value); }

        }
        private decimal GORValue;

        public decimal GOR

        {

            get { return this.GORValue; }

            set { SetProperty(ref GORValue, value); }

        }
        private decimal CGRValue;

        public decimal CGR

        {

            get { return this.CGRValue; }

            set { SetProperty(ref CGRValue, value); }

        }
        private decimal WaterCutValue;

        public decimal WaterCut

        {

            get { return this.WaterCutValue; }

            set { SetProperty(ref WaterCutValue, value); }

        }
        private string FluidClassificationValue = string.Empty;

        public string FluidClassification

        {

            get { return this.FluidClassificationValue; }

            set { SetProperty(ref FluidClassificationValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private List<PropertyResult> CalculatedPropertiesValue = new();

        public List<PropertyResult> CalculatedProperties

        {

            get { return this.CalculatedPropertiesValue; }

            set { SetProperty(ref CalculatedPropertiesValue, value); }

        }
    }

    /// <summary>
    /// PVT analysis result DTO
    /// </summary>
    public class PVTAnaysisResult : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private string SampleIdValue = string.Empty;

        public string SampleId

        {

            get { return this.SampleIdValue; }

            set { SetProperty(ref SampleIdValue, value); }

        }
        private string AnalysisTypeValue = string.Empty;

        public string AnalysisType

        {

            get { return this.AnalysisTypeValue; }

            set { SetProperty(ref AnalysisTypeValue, value); }

        }
        private List<PVTDataPoint> DataPointsValue = new();

        public List<PVTDataPoint> DataPoints

        {

            get { return this.DataPointsValue; }

            set { SetProperty(ref DataPointsValue, value); }

        }
        private PVTParameters ParametersValue = new();

        public PVTParameters Parameters

        {

            get { return this.ParametersValue; }

            set { SetProperty(ref ParametersValue, value); }

        }
        private List<PropertyResult> DerivedPropertiesValue = new();

        public List<PropertyResult> DerivedProperties

        {

            get { return this.DerivedPropertiesValue; }

            set { SetProperty(ref DerivedPropertiesValue, value); }

        }
        private string QualityAssessmentValue = string.Empty;

        public string QualityAssessment

        {

            get { return this.QualityAssessmentValue; }

            set { SetProperty(ref QualityAssessmentValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
    }

    /// <summary>
    /// PVT data point DTO
    /// </summary>
    public class PVTDataPoint : ModelEntityBase
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
        private decimal VolumeValue;

        public decimal Volume

        {

            get { return this.VolumeValue; }

            set { SetProperty(ref VolumeValue, value); }

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
        private decimal GORValue;

        public decimal GOR

        {

            get { return this.GORValue; }

            set { SetProperty(ref GORValue, value); }

        }
        private string MeasurementTypeValue = string.Empty;

        public string MeasurementType

        {

            get { return this.MeasurementTypeValue; }

            set { SetProperty(ref MeasurementTypeValue, value); }

        }
    }

    /// <summary>
    /// PVT parameters DTO
    /// </summary>
    public class PVTParameters : ModelEntityBase
    {
        private decimal SaturationPressureValue;

        public decimal SaturationPressure

        {

            get { return this.SaturationPressureValue; }

            set { SetProperty(ref SaturationPressureValue, value); }

        }
        private decimal ReservoirTemperatureValue;

        public decimal ReservoirTemperature

        {

            get { return this.ReservoirTemperatureValue; }

            set { SetProperty(ref ReservoirTemperatureValue, value); }

        }
        private decimal InitialGORValue;

        public decimal InitialGOR

        {

            get { return this.InitialGORValue; }

            set { SetProperty(ref InitialGORValue, value); }

        }
        private decimal OilDensityValue;

        public decimal OilDensity

        {

            get { return this.OilDensityValue; }

            set { SetProperty(ref OilDensityValue, value); }

        }
        private decimal GasDensityValue;

        public decimal GasDensity

        {

            get { return this.GasDensityValue; }

            set { SetProperty(ref GasDensityValue, value); }

        }
        private decimal WaterDensityValue;

        public decimal WaterDensity

        {

            get { return this.WaterDensityValue; }

            set { SetProperty(ref WaterDensityValue, value); }

        }
        private decimal OilViscosityValue;

        public decimal OilViscosity

        {

            get { return this.OilViscosityValue; }

            set { SetProperty(ref OilViscosityValue, value); }

        }
        private decimal GasViscosityValue;

        public decimal GasViscosity

        {

            get { return this.GasViscosityValue; }

            set { SetProperty(ref GasViscosityValue, value); }

        }
        private decimal WaterViscosityValue;

        public decimal WaterViscosity

        {

            get { return this.WaterViscosityValue; }

            set { SetProperty(ref WaterViscosityValue, value); }

        }
        private decimal FormationVolumeFactorValue;

        public decimal FormationVolumeFactor

        {

            get { return this.FormationVolumeFactorValue; }

            set { SetProperty(ref FormationVolumeFactorValue, value); }

        }
        private decimal SolutionGasOilRatioValue;

        public decimal SolutionGasOilRatio

        {

            get { return this.SolutionGasOilRatioValue; }

            set { SetProperty(ref SolutionGasOilRatioValue, value); }

        }
    }

    #endregion

    #region Phase Behavior DTOs

    /// <summary>
    /// Phase envelope DTO
    /// </summary>
    public class PhaseEnvelope : ModelEntityBase
    {
        private string EnvelopeIdValue = string.Empty;

        public string EnvelopeId

        {

            get { return this.EnvelopeIdValue; }

            set { SetProperty(ref EnvelopeIdValue, value); }

        }
        private string SampleIdValue = string.Empty;

        public string SampleId

        {

            get { return this.SampleIdValue; }

            set { SetProperty(ref SampleIdValue, value); }

        }
        private List<PhaseEnvelopePoint> BubblePointCurveValue = new();

        public List<PhaseEnvelopePoint> BubblePointCurve

        {

            get { return this.BubblePointCurveValue; }

            set { SetProperty(ref BubblePointCurveValue, value); }

        }
        private List<PhaseEnvelopePoint> DewPointCurveValue = new();

        public List<PhaseEnvelopePoint> DewPointCurve

        {

            get { return this.DewPointCurveValue; }

            set { SetProperty(ref DewPointCurveValue, value); }

        }
        private PhaseEnvelopePoint CriticalPointValue = new();

        public PhaseEnvelopePoint CriticalPoint

        {

            get { return this.CriticalPointValue; }

            set { SetProperty(ref CriticalPointValue, value); }

        }
        private decimal CricondenthermValue;

        public decimal Cricondentherm

        {

            get { return this.CricondenthermValue; }

            set { SetProperty(ref CricondenthermValue, value); }

        }
        private decimal CricondenbarValue;

        public decimal Cricondenbar

        {

            get { return this.CricondenbarValue; }

            set { SetProperty(ref CricondenbarValue, value); }

        }
        private string QualityAssessmentValue = string.Empty;

        public string QualityAssessment

        {

            get { return this.QualityAssessmentValue; }

            set { SetProperty(ref QualityAssessmentValue, value); }

        }
        private DateTime GeneratedDateValue;

        public DateTime GeneratedDate

        {

            get { return this.GeneratedDateValue; }

            set { SetProperty(ref GeneratedDateValue, value); }

        }
    }

    /// <summary>
    /// Phase envelope point DTO
    /// </summary>
    public class PhaseEnvelopePoint : ModelEntityBase
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
        private decimal LiquidFractionValue;

        public decimal LiquidFraction

        {

            get { return this.LiquidFractionValue; }

            set { SetProperty(ref LiquidFractionValue, value); }

        }
        private decimal VaporFractionValue;

        public decimal VaporFraction

        {

            get { return this.VaporFractionValue; }

            set { SetProperty(ref VaporFractionValue, value); }

        }
        private string PhaseTypeValue = string.Empty;

        public string PhaseType

        {

            get { return this.PhaseTypeValue; }

            set { SetProperty(ref PhaseTypeValue, value); }

        }
    }

    /// <summary>
    /// Flash calculation result DTO
    /// </summary>
    public class FlashCalculationPropertyResult : ModelEntityBase
    {
        private string CalculationIdValue = string.Empty;

        public string CalculationId

        {

            get { return this.CalculationIdValue; }

            set { SetProperty(ref CalculationIdValue, value); }

        }
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
        private decimal LiquidFractionValue;

        public decimal LiquidFraction

        {

            get { return this.LiquidFractionValue; }

            set { SetProperty(ref LiquidFractionValue, value); }

        }
        private decimal VaporFractionValue;

        public decimal VaporFraction

        {

            get { return this.VaporFractionValue; }

            set { SetProperty(ref VaporFractionValue, value); }

        }
        private FluidComposition LiquidCompositionValue = new();

        public FluidComposition LiquidComposition

        {

            get { return this.LiquidCompositionValue; }

            set { SetProperty(ref LiquidCompositionValue, value); }

        }
        private FluidComposition VaporCompositionValue = new();

        public FluidComposition VaporComposition

        {

            get { return this.VaporCompositionValue; }

            set { SetProperty(ref VaporCompositionValue, value); }

        }
        private string FlashTypeValue = string.Empty;

        public string FlashType

        {

            get { return this.FlashTypeValue; }

            set { SetProperty(ref FlashTypeValue, value); }

        }
        private string CalculationMethodValue = string.Empty;

        public string CalculationMethod

        {

            get { return this.CalculationMethodValue; }

            set { SetProperty(ref CalculationMethodValue, value); }

        }
        private DateTime CalculationDateValue;

        public DateTime CalculationDate

        {

            get { return this.CalculationDateValue; }

            set { SetProperty(ref CalculationDateValue, value); }

        }
    }

    /// <summary>
    /// Fluid composition DTO
    /// </summary>
    public class FluidComposition : ModelEntityBase
    {
        private string CompositionIdValue = string.Empty;

        public string CompositionId

        {

            get { return this.CompositionIdValue; }

            set { SetProperty(ref CompositionIdValue, value); }

        }
        private string FluidTypeValue = string.Empty;

        public string FluidType

        {

            get { return this.FluidTypeValue; }

            set { SetProperty(ref FluidTypeValue, value); }

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
        private decimal APIValue;

        public decimal API

        {

            get { return this.APIValue; }

            set { SetProperty(ref APIValue, value); }

        }
        private List<FluidComponent> ComponentsValue = new();

        public List<FluidComponent> Components

        {

            get { return this.ComponentsValue; }

            set { SetProperty(ref ComponentsValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
    }

    /// <summary>
    /// Fluid component DTO
    /// </summary>
    public class FluidComponent : ModelEntityBase
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
        private decimal MassFractionValue;

        public decimal MassFraction

        {

            get { return this.MassFractionValue; }

            set { SetProperty(ref MassFractionValue, value); }

        }
        private decimal VolumeFractionValue;

        public decimal VolumeFraction

        {

            get { return this.VolumeFractionValue; }

            set { SetProperty(ref VolumeFractionValue, value); }

        }
        private decimal MolecularWeightValue;

        public decimal MolecularWeight

        {

            get { return this.MolecularWeightValue; }

            set { SetProperty(ref MolecularWeightValue, value); }

        }
        private decimal BoilingPointValue;

        public decimal BoilingPoint

        {

            get { return this.BoilingPointValue; }

            set { SetProperty(ref BoilingPointValue, value); }

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
        private decimal AcentricFactorValue;

        public decimal AcentricFactor

        {

            get { return this.AcentricFactorValue; }

            set { SetProperty(ref AcentricFactorValue, value); }

        }
    }

    /// <summary>
    /// Saturation test result DTO
    /// </summary>
    public class SaturationTestResult : ModelEntityBase
    {
        private string TestIdValue = string.Empty;

        public string TestId

        {

            get { return this.TestIdValue; }

            set { SetProperty(ref TestIdValue, value); }

        }
        private string SampleIdValue = string.Empty;

        public string SampleId

        {

            get { return this.SampleIdValue; }

            set { SetProperty(ref SampleIdValue, value); }

        }
        private decimal SaturationPressureValue;

        public decimal SaturationPressure

        {

            get { return this.SaturationPressureValue; }

            set { SetProperty(ref SaturationPressureValue, value); }

        }
        private decimal SaturationTemperatureValue;

        public decimal SaturationTemperature

        {

            get { return this.SaturationTemperatureValue; }

            set { SetProperty(ref SaturationTemperatureValue, value); }

        }
        private string SaturationTypeValue = string.Empty;

        public string SaturationType

        {

            get { return this.SaturationTypeValue; }

            set { SetProperty(ref SaturationTypeValue, value); }

        } // Bubble or Dew
        private List<SaturationPoint> TestPointsValue = new();

        public List<SaturationPoint> TestPoints

        {

            get { return this.TestPointsValue; }

            set { SetProperty(ref TestPointsValue, value); }

        }
        private decimal CompressibilityAboveSaturationValue;

        public decimal CompressibilityAboveSaturation

        {

            get { return this.CompressibilityAboveSaturationValue; }

            set { SetProperty(ref CompressibilityAboveSaturationValue, value); }

        }
        private decimal CompressibilityBelowSaturationValue;

        public decimal CompressibilityBelowSaturation

        {

            get { return this.CompressibilityBelowSaturationValue; }

            set { SetProperty(ref CompressibilityBelowSaturationValue, value); }

        }
        private DateTime TestDateValue;

        public DateTime TestDate

        {

            get { return this.TestDateValue; }

            set { SetProperty(ref TestDateValue, value); }

        }
    }

    /// <summary>
    /// Saturation point DTO
    /// </summary>
    public class SaturationPoint : ModelEntityBase
    {
        private decimal PressureValue;

        public decimal Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }
        private decimal RelativeVolumeValue;

        public decimal RelativeVolume

        {

            get { return this.RelativeVolumeValue; }

            set { SetProperty(ref RelativeVolumeValue, value); }

        }
        private decimal CompressibilityValue;

        public decimal Compressibility

        {

            get { return this.CompressibilityValue; }

            set { SetProperty(ref CompressibilityValue, value); }

        }
        private string PhaseValue = string.Empty;

        public string Phase

        {

            get { return this.PhaseValue; }

            set { SetProperty(ref PhaseValue, value); }

        }
    }

    #endregion

    #region Advanced PVT DTOs

    /// <summary>
    /// Equation of state result DTO
    /// </summary>
    public class EOSResult : ModelEntityBase
    {
        private string CalculationIdValue = string.Empty;

        public string CalculationId

        {

            get { return this.CalculationIdValue; }

            set { SetProperty(ref CalculationIdValue, value); }

        }
        private string EquationOfStateValue = string.Empty;

        public string EquationOfState

        {

            get { return this.EquationOfStateValue; }

            set { SetProperty(ref EquationOfStateValue, value); }

        }
        private string MixingRuleValue = string.Empty;

        public string MixingRule

        {

            get { return this.MixingRuleValue; }

            set { SetProperty(ref MixingRuleValue, value); }

        }
        private List<EOSComponent> ComponentsValue = new();

        public List<EOSComponent> Components

        {

            get { return this.ComponentsValue; }

            set { SetProperty(ref ComponentsValue, value); }

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
        private decimal CriticalVolumeValue;

        public decimal CriticalVolume

        {

            get { return this.CriticalVolumeValue; }

            set { SetProperty(ref CriticalVolumeValue, value); }

        }
        private decimal AcentricFactorValue;

        public decimal AcentricFactor

        {

            get { return this.AcentricFactorValue; }

            set { SetProperty(ref AcentricFactorValue, value); }

        }
        private List<EOSPhase> PhasesValue = new();

        public List<EOSPhase> Phases

        {

            get { return this.PhasesValue; }

            set { SetProperty(ref PhasesValue, value); }

        }
        private decimal BinaryInteractionParameterValue;

        public decimal BinaryInteractionParameter

        {

            get { return this.BinaryInteractionParameterValue; }

            set { SetProperty(ref BinaryInteractionParameterValue, value); }

        }
        private string ConvergenceStatusValue = string.Empty;

        public string ConvergenceStatus

        {

            get { return this.ConvergenceStatusValue; }

            set { SetProperty(ref ConvergenceStatusValue, value); }

        }
        private DateTime CalculationDateValue;

        public DateTime CalculationDate

        {

            get { return this.CalculationDateValue; }

            set { SetProperty(ref CalculationDateValue, value); }

        }
    }

    /// <summary>
    /// EOS component DTO
    /// </summary>
    public class EOSComponent : ModelEntityBase
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
        private decimal CriticalVolumeValue;

        public decimal CriticalVolume

        {

            get { return this.CriticalVolumeValue; }

            set { SetProperty(ref CriticalVolumeValue, value); }

        }
        private decimal AcentricFactorValue;

        public decimal AcentricFactor

        {

            get { return this.AcentricFactorValue; }

            set { SetProperty(ref AcentricFactorValue, value); }

        }
        private decimal MolecularWeightValue;

        public decimal MolecularWeight

        {

            get { return this.MolecularWeightValue; }

            set { SetProperty(ref MolecularWeightValue, value); }

        }
        private decimal OmegaAValue;

        public decimal OmegaA

        {

            get { return this.OmegaAValue; }

            set { SetProperty(ref OmegaAValue, value); }

        }
        private decimal OmegaBValue;

        public decimal OmegaB

        {

            get { return this.OmegaBValue; }

            set { SetProperty(ref OmegaBValue, value); }

        }
    }

    /// <summary>
    /// EOS phase DTO
    /// </summary>
    public class EOSPhase : ModelEntityBase
    {
        private string PhaseTypeValue = string.Empty;

        public string PhaseType

        {

            get { return this.PhaseTypeValue; }

            set { SetProperty(ref PhaseTypeValue, value); }

        }
        private decimal MoleFractionValue;

        public decimal MoleFraction

        {

            get { return this.MoleFractionValue; }

            set { SetProperty(ref MoleFractionValue, value); }

        }
        private decimal DensityValue;

        public decimal Density

        {

            get { return this.DensityValue; }

            set { SetProperty(ref DensityValue, value); }

        }
        private decimal CompressibilityValue;

        public decimal Compressibility

        {

            get { return this.CompressibilityValue; }

            set { SetProperty(ref CompressibilityValue, value); }

        }
        private decimal FugacityValue;

        public decimal Fugacity

        {

            get { return this.FugacityValue; }

            set { SetProperty(ref FugacityValue, value); }

        }
        private FluidComposition CompositionValue = new();

        public FluidComposition Composition

        {

            get { return this.CompositionValue; }

            set { SetProperty(ref CompositionValue, value); }

        }
    }

    /// <summary>
    /// Viscosity correlation result DTO
    /// </summary>
    public class ViscosityCorrelation : ModelEntityBase
    {
        private string CorrelationIdValue = string.Empty;

        public string CorrelationId

        {

            get { return this.CorrelationIdValue; }

            set { SetProperty(ref CorrelationIdValue, value); }

        }
        private string CorrelationNameValue = string.Empty;

        public string CorrelationName

        {

            get { return this.CorrelationNameValue; }

            set { SetProperty(ref CorrelationNameValue, value); }

        }
        private decimal ViscosityValue;

        public decimal Viscosity

        {

            get { return this.ViscosityValue; }

            set { SetProperty(ref ViscosityValue, value); }

        }
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
        public Dictionary<string, decimal> Parameters { get; set; } = new();
        private decimal AccuracyValue;

        public decimal Accuracy

        {

            get { return this.AccuracyValue; }

            set { SetProperty(ref AccuracyValue, value); }

        }
        private string ApplicabilityValue = string.Empty;

        public string Applicability

        {

            get { return this.ApplicabilityValue; }

            set { SetProperty(ref ApplicabilityValue, value); }

        }
        private DateTime CalculatedDateValue;

        public DateTime CalculatedDate

        {

            get { return this.CalculatedDateValue; }

            set { SetProperty(ref CalculatedDateValue, value); }

        }
    }

    /// <summary>
    /// Asphaltene analysis result DTO
    /// </summary>
    public class AsphalteneAnalysis : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private string SampleIdValue = string.Empty;

        public string SampleId

        {

            get { return this.SampleIdValue; }

            set { SetProperty(ref SampleIdValue, value); }

        }
        private decimal OnsetPressureValue;

        public decimal OnsetPressure

        {

            get { return this.OnsetPressureValue; }

            set { SetProperty(ref OnsetPressureValue, value); }

        }
        private decimal OnsetTemperatureValue;

        public decimal OnsetTemperature

        {

            get { return this.OnsetTemperatureValue; }

            set { SetProperty(ref OnsetTemperatureValue, value); }

        }
        private decimal AsphalteneContentValue;

        public decimal AsphalteneContent

        {

            get { return this.AsphalteneContentValue; }

            set { SetProperty(ref AsphalteneContentValue, value); }

        }
        private string PrecipitationMechanismValue = string.Empty;

        public string PrecipitationMechanism

        {

            get { return this.PrecipitationMechanismValue; }

            set { SetProperty(ref PrecipitationMechanismValue, value); }

        }
        private List<AsphaltenePoint> PrecipitationPointsValue = new();

        public List<AsphaltenePoint> PrecipitationPoints

        {

            get { return this.PrecipitationPointsValue; }

            set { SetProperty(ref PrecipitationPointsValue, value); }

        }
        private string AnalysisMethodValue = string.Empty;

        public string AnalysisMethod

        {

            get { return this.AnalysisMethodValue; }

            set { SetProperty(ref AnalysisMethodValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
    }

    /// <summary>
    /// Asphaltene point DTO
    /// </summary>
    public class AsphaltenePoint : ModelEntityBase
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
        private decimal AsphaltenePrecipitatedValue;

        public decimal AsphaltenePrecipitated

        {

            get { return this.AsphaltenePrecipitatedValue; }

            set { SetProperty(ref AsphaltenePrecipitatedValue, value); }

        }
        private decimal OpticalDensityValue;

        public decimal OpticalDensity

        {

            get { return this.OpticalDensityValue; }

            set { SetProperty(ref OpticalDensityValue, value); }

        }
    }

    /// <summary>
    /// Wax analysis result DTO
    /// </summary>
    public class WaxAnalysis : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private string SampleIdValue = string.Empty;

        public string SampleId

        {

            get { return this.SampleIdValue; }

            set { SetProperty(ref SampleIdValue, value); }

        }
        private decimal WaxAppearanceTemperatureValue;

        public decimal WaxAppearanceTemperature

        {

            get { return this.WaxAppearanceTemperatureValue; }

            set { SetProperty(ref WaxAppearanceTemperatureValue, value); }

        }
        private decimal PourPointValue;

        public decimal PourPoint

        {

            get { return this.PourPointValue; }

            set { SetProperty(ref PourPointValue, value); }

        }
        private decimal CloudPointValue;

        public decimal CloudPoint

        {

            get { return this.CloudPointValue; }

            set { SetProperty(ref CloudPointValue, value); }

        }
        private decimal WaxContentValue;

        public decimal WaxContent

        {

            get { return this.WaxContentValue; }

            set { SetProperty(ref WaxContentValue, value); }

        }
        private List<WaxFraction> WaxFractionsValue = new();

        public List<WaxFraction> WaxFractions

        {

            get { return this.WaxFractionsValue; }

            set { SetProperty(ref WaxFractionsValue, value); }

        }
        private string AnalysisMethodValue = string.Empty;

        public string AnalysisMethod

        {

            get { return this.AnalysisMethodValue; }

            set { SetProperty(ref AnalysisMethodValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
    }

    /// <summary>
    /// Wax fraction DTO
    /// </summary>
    public class WaxFraction : ModelEntityBase
    {
        private string FractionNameValue = string.Empty;

        public string FractionName

        {

            get { return this.FractionNameValue; }

            set { SetProperty(ref FractionNameValue, value); }

        }
        private decimal CarbonNumberValue;

        public decimal CarbonNumber

        {

            get { return this.CarbonNumberValue; }

            set { SetProperty(ref CarbonNumberValue, value); }

        }
        private decimal WeightFractionValue;

        public decimal WeightFraction

        {

            get { return this.WeightFractionValue; }

            set { SetProperty(ref WeightFractionValue, value); }

        }
        private decimal MeltingPointValue;

        public decimal MeltingPoint

        {

            get { return this.MeltingPointValue; }

            set { SetProperty(ref MeltingPointValue, value); }

        }
    }

    #endregion

    #region Laboratory Data DTOs

    /// <summary>
    /// Laboratory test result DTO
    /// </summary>
    public class LabTestResult : ModelEntityBase
    {
        private string TestIdValue = string.Empty;

        public string TestId

        {

            get { return this.TestIdValue; }

            set { SetProperty(ref TestIdValue, value); }

        }
        private string SampleIdValue = string.Empty;

        public string SampleId

        {

            get { return this.SampleIdValue; }

            set { SetProperty(ref SampleIdValue, value); }

        }
        private string TestTypeValue = string.Empty;

        public string TestType

        {

            get { return this.TestTypeValue; }

            set { SetProperty(ref TestTypeValue, value); }

        }
        private string TestMethodValue = string.Empty;

        public string TestMethod

        {

            get { return this.TestMethodValue; }

            set { SetProperty(ref TestMethodValue, value); }

        }
        private DateTime TestDateValue;

        public DateTime TestDate

        {

            get { return this.TestDateValue; }

            set { SetProperty(ref TestDateValue, value); }

        }
        private string LabNameValue = string.Empty;

        public string LabName

        {

            get { return this.LabNameValue; }

            set { SetProperty(ref LabNameValue, value); }

        }
        private string AnalystValue = string.Empty;

        public string Analyst

        {

            get { return this.AnalystValue; }

            set { SetProperty(ref AnalystValue, value); }

        }
        private List<TestMeasurement> MeasurementsValue = new();

        public List<TestMeasurement> Measurements

        {

            get { return this.MeasurementsValue; }

            set { SetProperty(ref MeasurementsValue, value); }

        }
        private string QualityControlValue = string.Empty;

        public string QualityControl

        {

            get { return this.QualityControlValue; }

            set { SetProperty(ref QualityControlValue, value); }

        }
        private List<string> NotesValue = new();

        public List<string> Notes

        {

            get { return this.NotesValue; }

            set { SetProperty(ref NotesValue, value); }

        }
    }

    /// <summary>
    /// Test measurement DTO
    /// </summary>
    public class TestMeasurement : ModelEntityBase
    {
        private string ParameterValue = string.Empty;

        public string Parameter

        {

            get { return this.ParameterValue; }

            set { SetProperty(ref ParameterValue, value); }

        }
        private decimal ValueValue;

        public decimal Value

        {

            get { return this.ValueValue; }

            set { SetProperty(ref ValueValue, value); }

        }
        private string UnitValue = string.Empty;

        public string Unit

        {

            get { return this.UnitValue; }

            set { SetProperty(ref UnitValue, value); }

        }
        private decimal? UncertaintyValue;

        public decimal? Uncertainty

        {

            get { return this.UncertaintyValue; }

            set { SetProperty(ref UncertaintyValue, value); }

        }
        private string MethodValue = string.Empty;

        public string Method

        {

            get { return this.MethodValue; }

            set { SetProperty(ref MethodValue, value); }

        }
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
    }

    /// <summary>
    /// Sample information DTO
    /// </summary>
    public class SampleInfo : ModelEntityBase
    {
        private string SampleIdValue = string.Empty;

        public string SampleId

        {

            get { return this.SampleIdValue; }

            set { SetProperty(ref SampleIdValue, value); }

        }
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private string FieldNameValue = string.Empty;

        public string FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }
        private decimal DepthValue;

        public decimal Depth

        {

            get { return this.DepthValue; }

            set { SetProperty(ref DepthValue, value); }

        }
        private decimal ReservoirTemperatureValue;

        public decimal ReservoirTemperature

        {

            get { return this.ReservoirTemperatureValue; }

            set { SetProperty(ref ReservoirTemperatureValue, value); }

        }
        private decimal ReservoirPressureValue;

        public decimal ReservoirPressure

        {

            get { return this.ReservoirPressureValue; }

            set { SetProperty(ref ReservoirPressureValue, value); }

        }
        private string FluidTypeValue = string.Empty;

        public string FluidType

        {

            get { return this.FluidTypeValue; }

            set { SetProperty(ref FluidTypeValue, value); }

        }
        private DateTime SamplingDateValue;

        public DateTime SamplingDate

        {

            get { return this.SamplingDateValue; }

            set { SetProperty(ref SamplingDateValue, value); }

        }
        private string SamplingMethodValue = string.Empty;

        public string SamplingMethod

        {

            get { return this.SamplingMethodValue; }

            set { SetProperty(ref SamplingMethodValue, value); }

        }
        private string ContainerTypeValue = string.Empty;

        public string ContainerType

        {

            get { return this.ContainerTypeValue; }

            set { SetProperty(ref ContainerTypeValue, value); }

        }
        private string PreservationMethodValue = string.Empty;

        public string PreservationMethod

        {

            get { return this.PreservationMethodValue; }

            set { SetProperty(ref PreservationMethodValue, value); }

        }
        private List<string> AnalysisRequiredValue = new();

        public List<string> AnalysisRequired

        {

            get { return this.AnalysisRequiredValue; }

            set { SetProperty(ref AnalysisRequiredValue, value); }

        }
    }

    /// <summary>
    /// Sample chain of custody DTO
    /// </summary>
    public class SampleChainOfCustody : ModelEntityBase
    {
        private string SampleIdValue = string.Empty;

        public string SampleId

        {

            get { return this.SampleIdValue; }

            set { SetProperty(ref SampleIdValue, value); }

        }
        private List<CustodyTransfer> TransfersValue = new();

        public List<CustodyTransfer> Transfers

        {

            get { return this.TransfersValue; }

            set { SetProperty(ref TransfersValue, value); }

        }
        private string CurrentLocationValue = string.Empty;

        public string CurrentLocation

        {

            get { return this.CurrentLocationValue; }

            set { SetProperty(ref CurrentLocationValue, value); }

        }
        private string CurrentCustodianValue = string.Empty;

        public string CurrentCustodian

        {

            get { return this.CurrentCustodianValue; }

            set { SetProperty(ref CurrentCustodianValue, value); }

        }
        private DateTime LastTransferDateValue;

        public DateTime LastTransferDate

        {

            get { return this.LastTransferDateValue; }

            set { SetProperty(ref LastTransferDateValue, value); }

        }
        private string ChainIntegrityValue = string.Empty;

        public string ChainIntegrity

        {

            get { return this.ChainIntegrityValue; }

            set { SetProperty(ref ChainIntegrityValue, value); }

        }
    }

    /// <summary>
    /// Custody transfer DTO
    /// </summary>
    public class CustodyTransfer : ModelEntityBase
    {
        private DateTime TransferDateValue;

        public DateTime TransferDate

        {

            get { return this.TransferDateValue; }

            set { SetProperty(ref TransferDateValue, value); }

        }
        private string FromCustodianValue = string.Empty;

        public string FromCustodian

        {

            get { return this.FromCustodianValue; }

            set { SetProperty(ref FromCustodianValue, value); }

        }
        private string ToCustodianValue = string.Empty;

        public string ToCustodian

        {

            get { return this.ToCustodianValue; }

            set { SetProperty(ref ToCustodianValue, value); }

        }
        private string TransferReasonValue = string.Empty;

        public string TransferReason

        {

            get { return this.TransferReasonValue; }

            set { SetProperty(ref TransferReasonValue, value); }

        }
        private string DocumentationValue = string.Empty;

        public string Documentation

        {

            get { return this.DocumentationValue; }

            set { SetProperty(ref DocumentationValue, value); }

        }
        private string TransferConditionValue = string.Empty;

        public string TransferCondition

        {

            get { return this.TransferConditionValue; }

            set { SetProperty(ref TransferConditionValue, value); }

        }
    }

    #endregion
}



