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
        public string CalculationId { get; set; } = string.Empty;
        public string PropertyType { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string CorrelationUsed { get; set; } = string.Empty;
        public DateTime CalculationDate { get; set; }
        public Dictionary<string, decimal> InputParameters { get; set; } = new();
        public decimal? Uncertainty { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Fluid property analysis DTO
    /// </summary>
    public class FluidPropertyAnalysis : ModelEntityBase
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string SampleId { get; set; } = string.Empty;
        public string FluidType { get; set; } = string.Empty; // Oil, Gas, Condensate
        public decimal ReservoirTemperature { get; set; }
        public decimal ReservoirPressure { get; set; }
        public decimal BubblePointPressure { get; set; }
        public decimal DewPointPressure { get; set; }
        public decimal GOR { get; set; }
        public decimal CGR { get; set; }
        public decimal WaterCut { get; set; }
        public string FluidClassification { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public List<PropertyResult> CalculatedProperties { get; set; } = new();
    }

    /// <summary>
    /// PVT analysis result DTO
    /// </summary>
    public class PVTAnaysisResult : ModelEntityBase
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string SampleId { get; set; } = string.Empty;
        public string AnalysisType { get; set; } = string.Empty;
        public List<PVTDataPoint> DataPoints { get; set; } = new();
        public PVTParameters Parameters { get; set; } = new();
        public List<PropertyResult> DerivedProperties { get; set; } = new();
        public string QualityAssessment { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
    }

    /// <summary>
    /// PVT data point DTO
    /// </summary>
    public class PVTDataPoint : ModelEntityBase
    {
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal Volume { get; set; }
        public decimal Density { get; set; }
        public decimal Viscosity { get; set; }
        public decimal Compressibility { get; set; }
        public decimal GOR { get; set; }
        public string MeasurementType { get; set; } = string.Empty;
    }

    /// <summary>
    /// PVT parameters DTO
    /// </summary>
    public class PVTParameters : ModelEntityBase
    {
        public decimal SaturationPressure { get; set; }
        public decimal ReservoirTemperature { get; set; }
        public decimal InitialGOR { get; set; }
        public decimal OilDensity { get; set; }
        public decimal GasDensity { get; set; }
        public decimal WaterDensity { get; set; }
        public decimal OilViscosity { get; set; }
        public decimal GasViscosity { get; set; }
        public decimal WaterViscosity { get; set; }
        public decimal FormationVolumeFactor { get; set; }
        public decimal SolutionGasOilRatio { get; set; }
    }

    #endregion

    #region Phase Behavior DTOs

    /// <summary>
    /// Phase envelope DTO
    /// </summary>
    public class PhaseEnvelope : ModelEntityBase
    {
        public string EnvelopeId { get; set; } = string.Empty;
        public string SampleId { get; set; } = string.Empty;
        public List<PhaseEnvelopePoint> BubblePointCurve { get; set; } = new();
        public List<PhaseEnvelopePoint> DewPointCurve { get; set; } = new();
        public PhaseEnvelopePoint CriticalPoint { get; set; } = new();
        public decimal Cricondentherm { get; set; }
        public decimal Cricondenbar { get; set; }
        public string QualityAssessment { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
    }

    /// <summary>
    /// Phase envelope point DTO
    /// </summary>
    public class PhaseEnvelopePoint : ModelEntityBase
    {
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal LiquidFraction { get; set; }
        public decimal VaporFraction { get; set; }
        public string PhaseType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Flash calculation result DTO
    /// </summary>
    public class FlashCalculationPropertyResult : ModelEntityBase
    {
        public string CalculationId { get; set; } = string.Empty;
        public decimal Temperature { get; set; }
        public decimal Pressure { get; set; }
        public decimal LiquidFraction { get; set; }
        public decimal VaporFraction { get; set; }
        public FluidComposition LiquidComposition { get; set; } = new();
        public FluidComposition VaporComposition { get; set; } = new();
        public string FlashType { get; set; } = string.Empty;
        public string CalculationMethod { get; set; } = string.Empty;
        public DateTime CalculationDate { get; set; }
    }

    /// <summary>
    /// Fluid composition DTO
    /// </summary>
    public class FluidComposition : ModelEntityBase
    {
        public string CompositionId { get; set; } = string.Empty;
        public string FluidType { get; set; } = string.Empty;
        public decimal MolecularWeight { get; set; }
        public decimal SpecificGravity { get; set; }
        public decimal API { get; set; }
        public List<FluidComponent> Components { get; set; } = new();
        public DateTime AnalysisDate { get; set; }
    }

    /// <summary>
    /// Fluid component DTO
    /// </summary>
    public class FluidComponent : ModelEntityBase
    {
        public string ComponentName { get; set; } = string.Empty;
        public decimal MoleFraction { get; set; }
        public decimal MassFraction { get; set; }
        public decimal VolumeFraction { get; set; }
        public decimal MolecularWeight { get; set; }
        public decimal BoilingPoint { get; set; }
        public decimal CriticalPressure { get; set; }
        public decimal CriticalTemperature { get; set; }
        public decimal AcentricFactor { get; set; }
    }

    /// <summary>
    /// Saturation test result DTO
    /// </summary>
    public class SaturationTestResult : ModelEntityBase
    {
        public string TestId { get; set; } = string.Empty;
        public string SampleId { get; set; } = string.Empty;
        public decimal SaturationPressure { get; set; }
        public decimal SaturationTemperature { get; set; }
        public string SaturationType { get; set; } = string.Empty; // Bubble or Dew
        public List<SaturationPoint> TestPoints { get; set; } = new();
        public decimal CompressibilityAboveSaturation { get; set; }
        public decimal CompressibilityBelowSaturation { get; set; }
        public DateTime TestDate { get; set; }
    }

    /// <summary>
    /// Saturation point DTO
    /// </summary>
    public class SaturationPoint : ModelEntityBase
    {
        public decimal Pressure { get; set; }
        public decimal RelativeVolume { get; set; }
        public decimal Compressibility { get; set; }
        public string Phase { get; set; } = string.Empty;
    }

    #endregion

    #region Advanced PVT DTOs

    /// <summary>
    /// Equation of state result DTO
    /// </summary>
    public class EOSResult : ModelEntityBase
    {
        public string CalculationId { get; set; } = string.Empty;
        public string EquationOfState { get; set; } = string.Empty;
        public string MixingRule { get; set; } = string.Empty;
        public List<EOSComponent> Components { get; set; } = new();
        public decimal CriticalPressure { get; set; }
        public decimal CriticalTemperature { get; set; }
        public decimal CriticalVolume { get; set; }
        public decimal AcentricFactor { get; set; }
        public List<EOSPhase> Phases { get; set; } = new();
        public decimal BinaryInteractionParameter { get; set; }
        public string ConvergenceStatus { get; set; } = string.Empty;
        public DateTime CalculationDate { get; set; }
    }

    /// <summary>
    /// EOS component DTO
    /// </summary>
    public class EOSComponent : ModelEntityBase
    {
        public string ComponentName { get; set; } = string.Empty;
        public decimal MoleFraction { get; set; }
        public decimal CriticalPressure { get; set; }
        public decimal CriticalTemperature { get; set; }
        public decimal CriticalVolume { get; set; }
        public decimal AcentricFactor { get; set; }
        public decimal MolecularWeight { get; set; }
        public decimal OmegaA { get; set; }
        public decimal OmegaB { get; set; }
    }

    /// <summary>
    /// EOS phase DTO
    /// </summary>
    public class EOSPhase : ModelEntityBase
    {
        public string PhaseType { get; set; } = string.Empty;
        public decimal MoleFraction { get; set; }
        public decimal Density { get; set; }
        public decimal Compressibility { get; set; }
        public decimal Fugacity { get; set; }
        public FluidComposition Composition { get; set; } = new();
    }

    /// <summary>
    /// Viscosity correlation result DTO
    /// </summary>
    public class ViscosityCorrelation : ModelEntityBase
    {
        public string CorrelationId { get; set; } = string.Empty;
        public string CorrelationName { get; set; } = string.Empty;
        public decimal Viscosity { get; set; }
        public decimal Temperature { get; set; }
        public decimal Pressure { get; set; }
        public Dictionary<string, decimal> Parameters { get; set; } = new();
        public decimal Accuracy { get; set; }
        public string Applicability { get; set; } = string.Empty;
        public DateTime CalculatedDate { get; set; }
    }

    /// <summary>
    /// Asphaltene analysis result DTO
    /// </summary>
    public class AsphalteneAnalysis : ModelEntityBase
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string SampleId { get; set; } = string.Empty;
        public decimal OnsetPressure { get; set; }
        public decimal OnsetTemperature { get; set; }
        public decimal AsphalteneContent { get; set; }
        public string PrecipitationMechanism { get; set; } = string.Empty;
        public List<AsphaltenePoint> PrecipitationPoints { get; set; } = new();
        public string AnalysisMethod { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
    }

    /// <summary>
    /// Asphaltene point DTO
    /// </summary>
    public class AsphaltenePoint : ModelEntityBase
    {
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal AsphaltenePrecipitated { get; set; }
        public decimal OpticalDensity { get; set; }
    }

    /// <summary>
    /// Wax analysis result DTO
    /// </summary>
    public class WaxAnalysis : ModelEntityBase
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string SampleId { get; set; } = string.Empty;
        public decimal WaxAppearanceTemperature { get; set; }
        public decimal PourPoint { get; set; }
        public decimal CloudPoint { get; set; }
        public decimal WaxContent { get; set; }
        public List<WaxFraction> WaxFractions { get; set; } = new();
        public string AnalysisMethod { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
    }

    /// <summary>
    /// Wax fraction DTO
    /// </summary>
    public class WaxFraction : ModelEntityBase
    {
        public string FractionName { get; set; } = string.Empty;
        public decimal CarbonNumber { get; set; }
        public decimal WeightFraction { get; set; }
        public decimal MeltingPoint { get; set; }
    }

    #endregion

    #region Laboratory Data DTOs

    /// <summary>
    /// Laboratory test result DTO
    /// </summary>
    public class LabTestResult : ModelEntityBase
    {
        public string TestId { get; set; } = string.Empty;
        public string SampleId { get; set; } = string.Empty;
        public string TestType { get; set; } = string.Empty;
        public string TestMethod { get; set; } = string.Empty;
        public DateTime TestDate { get; set; }
        public string LabName { get; set; } = string.Empty;
        public string Analyst { get; set; } = string.Empty;
        public List<TestMeasurement> Measurements { get; set; } = new();
        public string QualityControl { get; set; } = string.Empty;
        public List<string> Notes { get; set; } = new();
    }

    /// <summary>
    /// Test measurement DTO
    /// </summary>
    public class TestMeasurement : ModelEntityBase
    {
        public string Parameter { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal? Uncertainty { get; set; }
        public string Method { get; set; } = string.Empty;
        public decimal Temperature { get; set; }
        public decimal Pressure { get; set; }
    }

    /// <summary>
    /// Sample information DTO
    /// </summary>
    public class SampleInfo : ModelEntityBase
    {
        public string SampleId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public decimal Depth { get; set; }
        public decimal ReservoirTemperature { get; set; }
        public decimal ReservoirPressure { get; set; }
        public string FluidType { get; set; } = string.Empty;
        public DateTime SamplingDate { get; set; }
        public string SamplingMethod { get; set; } = string.Empty;
        public string ContainerType { get; set; } = string.Empty;
        public string PreservationMethod { get; set; } = string.Empty;
        public List<string> AnalysisRequired { get; set; } = new();
    }

    /// <summary>
    /// Sample chain of custody DTO
    /// </summary>
    public class SampleChainOfCustody : ModelEntityBase
    {
        public string SampleId { get; set; } = string.Empty;
        public List<CustodyTransfer> Transfers { get; set; } = new();
        public string CurrentLocation { get; set; } = string.Empty;
        public string CurrentCustodian { get; set; } = string.Empty;
        public DateTime LastTransferDate { get; set; }
        public string ChainIntegrity { get; set; } = string.Empty;
    }

    /// <summary>
    /// Custody transfer DTO
    /// </summary>
    public class CustodyTransfer : ModelEntityBase
    {
        public DateTime TransferDate { get; set; }
        public string FromCustodian { get; set; } = string.Empty;
        public string ToCustodian { get; set; } = string.Empty;
        public string TransferReason { get; set; } = string.Empty;
        public string Documentation { get; set; } = string.Empty;
        public string TransferCondition { get; set; } = string.Empty;
    }

    #endregion
}

