using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Properties.DTOs
{
    #region Core Property DTOs

    /// <summary>
    /// Base property calculation result DTO
    /// </summary>
    public class PropertyResult
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
    public class FluidPropertyAnalysis
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
    public class PVTAnaysisResult
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
    public class PVTDataPoint
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
    public class PVTParameters
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
    public class PhaseEnvelope
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
    public class PhaseEnvelopePoint
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
    public class FlashCalculationPropertyResult
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
    public class FluidComposition
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
    public class FluidComponent
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
    public class SaturationTestResult
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
    public class SaturationPoint
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
    public class EOSResult
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
    public class EOSComponent
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
    public class EOSPhase
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
    public class ViscosityCorrelation
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
    public class AsphalteneAnalysis
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
    public class AsphaltenePoint
    {
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal AsphaltenePrecipitated { get; set; }
        public decimal OpticalDensity { get; set; }
    }

    /// <summary>
    /// Wax analysis result DTO
    /// </summary>
    public class WaxAnalysis
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
    public class WaxFraction
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
    public class LabTestResult
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
    public class TestMeasurement
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
    public class SampleInfo
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
    public class SampleChainOfCustody
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
    public class CustodyTransfer
    {
        public DateTime TransferDate { get; set; }
        public string FromCustodian { get; set; } = string.Empty;
        public string ToCustodian { get; set; } = string.Empty;
        public string TransferReason { get; set; } = string.Empty;
        public string Documentation { get; set; } = string.Empty;
        public string TransferCondition { get; set; } = string.Empty;
    }

    #endregion

    #region Quality Control DTOs

    /// <summary>
    /// Quality control result DTO
    /// </summary>
    public class QCResult
    {
        public string QCId { get; set; } = string.Empty;
        public string SampleId { get; set; } = string.Empty;
        public string TestType { get; set; } = string.Empty;
        public DateTime QCDate { get; set; }
        public string QCMethod { get; set; } = string.Empty;
        public List<QCCheck> Checks { get; set; } = new();
        public string OverallResult { get; set; } = string.Empty;
        public List<string> Recommendations { get; set; } = new();
        public string PerformedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// QC check DTO
    /// </summary>
    public class QCCheck
    {
        public string CheckType { get; set; } = string.Empty;
        public string Parameter { get; set; } = string.Empty;
        public decimal ExpectedValue { get; set; }
        public decimal MeasuredValue { get; set; }
        public decimal Tolerance { get; set; }
        public decimal Deviation { get; set; }
        public string Result { get; set; } = string.Empty; // Pass, Fail, Warning
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Uncertainty analysis result DTO
    /// </summary>
    public class UncertaintyAnalysis
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string PropertyType { get; set; } = string.Empty;
        public decimal MeanValue { get; set; }
        public decimal StandardDeviation { get; set; }
        public decimal Variance { get; set; }
        public decimal P10Value { get; set; }
        public decimal P50Value { get; set; }
        public decimal P90Value { get; set; }
        public decimal ConfidenceInterval { get; set; }
        public string DistributionType { get; set; } = string.Empty;
        public List<UncertaintyFactor> ContributingFactors { get; set; } = new();
        public DateTime AnalysisDate { get; set; }
    }

    /// <summary>
    /// Uncertainty factor DTO
    /// </summary>
    public class UncertaintyFactor
    {
        public string FactorName { get; set; } = string.Empty;
        public string FactorType { get; set; } = string.Empty; // Measurement, Model, Sampling
        public decimal StandardDeviation { get; set; }
        public decimal Contribution { get; set; } // Percentage contribution to total uncertainty
        public string MitigationStrategy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Validation result DTO
    /// </summary>
    public class ValidationResult
    {
        public string ValidationId { get; set; } = string.Empty;
        public string PropertyType { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public List<string> ValidationErrors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public decimal ConfidenceScore { get; set; }
        public List<ValidationRuleResult> RuleResults { get; set; } = new();
        public string ValidationSummary { get; set; } = string.Empty;
        public DateTime ValidationDate { get; set; }
    }

    /// <summary>
    /// Validation rule result DTO
    /// </summary>
    public class ValidationRuleResult
    {
        public string RuleName { get; set; } = string.Empty;
        public string RuleDescription { get; set; } = string.Empty;
        public bool Passed { get; set; }
        public decimal Score { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
    }

    #endregion

    #region Reporting DTOs

    /// <summary>
    /// PVT report DTO
    /// </summary>
    public class PVTReport
    {
        public string ReportId { get; set; } = string.Empty;
        public string SampleId { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public SampleInfo SampleInfo { get; set; } = new();
        public FluidPropertyAnalysis PropertyAnalysis { get; set; } = new();
        public PVTAnaysisResult PVTResults { get; set; } = new();
        public List<byte[]> Charts { get; set; } = new();
        public List<byte[]> Tables { get; set; } = new();
        public string ExecutiveSummary { get; set; } = string.Empty;
        public List<string> Conclusions { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// Fluid analysis report DTO
    /// </summary>
    public class FluidAnalysisReport
    {
        public string ReportId { get; set; } = string.Empty;
        public string SampleId { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public FluidComposition Composition { get; set; } = new();
        public List<PropertyResult> PhysicalProperties { get; set; } = new();
        public List<PropertyResult> TransportProperties { get; set; } = new();
        public List<PropertyResult> ThermalProperties { get; set; } = new();
        public PhaseEnvelope PhaseEnvelope { get; set; } = new();
        public List<SpecialAnalysis> SpecialAnalyses { get; set; } = new();
        public string FluidClassification { get; set; } = string.Empty;
        public string ReservoirImplications { get; set; } = string.Empty;
    }

    /// <summary>
    /// Special analysis DTO
    /// </summary>
    public class SpecialAnalysis
    {
        public string AnalysisType { get; set; } = string.Empty;
        public string AnalysisName { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public string Method { get; set; } = string.Empty;
        public string Results { get; set; } = string.Empty;
        public List<PropertyResult> DerivedProperties { get; set; } = new();
        public string Interpretation { get; set; } = string.Empty;
    }

    /// <summary>
    /// Correlation study report DTO
    /// </summary>
    public class CorrelationStudyReport
    {
        public string ReportId { get; set; } = string.Empty;
        public string StudyName { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public List<CorrelationComparison> Comparisons { get; set; } = new();
        public List<CorrelationRecommendation> Recommendations { get; set; } = new();
        public List<byte[]> PerformanceCharts { get; set; } = new();
        public string StudyConclusions { get; set; } = string.Empty;
    }

    /// <summary>
    /// Correlation recommendation DTO
    /// </summary>
    public class CorrelationRecommendation
    {
        public string PropertyType { get; set; } = string.Empty;
        public string RecommendedCorrelation { get; set; } = string.Empty;
        public decimal Accuracy { get; set; }
        public string ApplicabilityRange { get; set; } = string.Empty;
        public List<string> Limitations { get; set; } = new();
        public string Rationale { get; set; } = string.Empty;
    }

    #endregion

    #region Request DTOs

    /// <summary>
    /// Fluid analysis request DTO
    /// </summary>
    public class FluidAnalysisRequest
    {
        public string SampleId { get; set; } = string.Empty;
        public List<string> AnalysisTypes { get; set; } = new();
        public bool IncludePVT { get; set; } = true;
        public bool IncludePhaseBehavior { get; set; } = true;
        public bool IncludeSpecialAnalyses { get; set; } = true;
        public string Priority { get; set; } = "Standard";
        public DateTime? RequiredByDate { get; set; }
    }

    /// <summary>
    /// PVT analysis request DTO
    /// </summary>
    public class PVTAnaysisRequest
    {
        public string SampleId { get; set; } = string.Empty;
        public string AnalysisType { get; set; } = string.Empty;
        public decimal ReservoirTemperature { get; set; }
        public decimal ReservoirPressure { get; set; }
        public bool IncludeSaturationTest { get; set; } = true;
        public bool IncludeDifferentialLiberation { get; set; } = true;
        public bool IncludeConstantComposition { get; set; } = true;
        public List<decimal> TestPressures { get; set; } = new();
        public string LabInstructions { get; set; } = string.Empty;
    }

    /// <summary>
    /// Correlation comparison request DTO
    /// </summary>
    public class CorrelationComparisonRequest
    {
        public string PropertyType { get; set; } = string.Empty;
        public Dictionary<string, decimal> InputParameters { get; set; } = new();
        public List<string> CorrelationIds { get; set; } = new();
        public decimal? MeasuredValue { get; set; }
        public bool IncludeStatisticalAnalysis { get; set; } = true;
        public bool GenerateCharts { get; set; } = true;
    }

    /// <summary>
    /// Uncertainty analysis request DTO
    /// </summary>
    public class UncertaintyAnalysisRequest
    {
        public string PropertyType { get; set; } = string.Empty;
        public Dictionary<string, decimal> BaseParameters { get; set; } = new();
        public Dictionary<string, Tuple<decimal, decimal>> ParameterUncertainties { get; set; } = new();
        public int MonteCarloIterations { get; set; } = 10000;
        public decimal ConfidenceLevel { get; set; } = 0.95m;
        public string DistributionType { get; set; } = "Normal";
    }

    /// <summary>
    /// Validation request DTO
    /// </summary>
    public class ValidationRequest
    {
        public PropertyResult CalculationResult { get; set; } = new();
        public List<string> ValidationRules { get; set; } = new();
        public decimal? MeasuredValue { get; set; }
        public decimal Tolerance { get; set; } = 0.05m; // 5% tolerance
        public bool StrictValidation { get; set; } = false;
    }

    /// <summary>
    /// Report generation request DTO
    /// </summary>
    public class ReportGenerationRequest
    {
        public string SampleId { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public List<string> IncludeSections { get; set; } = new();
        public bool IncludeCharts { get; set; } = true;
        public bool IncludeRawData { get; set; } = false;
        public string Format { get; set; } = "PDF";
        public string Template { get; set; } = "Standard";
    }

    /// <summary>
    /// Data export request DTO
    /// </summary>
    public class DataExportRequest
    {
        public string SampleId { get; set; } = string.Empty;
        public List<string> DataTypes { get; set; } = new();
        public string Format { get; set; } = "Excel";
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string> Filters { get; set; } = new();
        public bool IncludeMetadata { get; set; } = true;
    }

    /// <summary>
    /// Chart generation request DTO
    /// </summary>
    public class ChartGenerationRequest
    {
        public string SampleId { get; set; } = string.Empty;
        public string ChartType { get; set; } = string.Empty;
        public List<string> Properties { get; set; } = new();
        public int Width { get; set; } = 800;
        public int Height { get; set; } = 600;
        public string Format { get; set; } = "PNG";
        public bool IncludeLegend { get; set; } = true;
        public bool IncludeGrid { get; set; } = true;
        public string Title { get; set; } = string.Empty;
    }

    #endregion
}