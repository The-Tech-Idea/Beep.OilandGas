using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beep.OilandGas.Properties.Services
{
    /// <summary>
    /// Comprehensive oil properties service interface
    /// Provides industry-standard correlations and calculations for oil PVT properties
    /// </summary>
    public interface IOilPropertiesService
    {
        #region PVT Properties

        /// <summary>
        /// Calculates oil formation volume factor (FVF)
        /// </summary>
        Task<OilPropertyResultDto> CalculateFormationVolumeFactorAsync(CalculateFVFRequest request);

        /// <summary>
        /// Calculates oil density at reservoir conditions
        /// </summary>
        Task<OilPropertyResultDto> CalculateDensityAsync(CalculateDensityRequest request);

        /// <summary>
        /// Calculates oil viscosity at reservoir conditions
        /// </summary>
        Task<OilPropertyResultDto> CalculateViscosityAsync(CalculateViscosityRequest request);

        /// <summary>
        /// Calculates oil compressibility
        /// </summary>
        Task<OilPropertyResultDto> CalculateCompressibilityAsync(CalculateCompressibilityRequest request);

        /// <summary>
        /// Calculates bubble point pressure
        /// </summary>
        Task<OilPropertyResultDto> CalculateBubblePointPressureAsync(CalculateBubblePointRequest request);

        /// <summary>
        /// Calculates solution gas-oil ratio (GOR)
        /// </summary>
        Task<OilPropertyResultDto> CalculateSolutionGORAsync(CalculateSolutionGORRequest request);

        #endregion

        #region Phase Behavior

        /// <summary>
        /// Performs flash calculation for oil system
        /// </summary>
        Task<FlashCalculationResultDto> PerformFlashCalculationAsync(FlashCalculationRequest request);

        /// <summary>
        /// Calculates saturation pressures
        /// </summary>
        Task<SaturationPressureResultDto> CalculateSaturationPressureAsync(SaturationPressureRequest request);

        /// <summary>
        /// Performs differential liberation test simulation
        /// </summary>
        Task<DifferentialLiberationResultDto> PerformDifferentialLiberationAsync(DifferentialLiberationRequest request);

        /// <summary>
        /// Performs constant composition expansion
        /// </summary>
        Task<ConstantCompositionResultDto> PerformConstantCompositionExpansionAsync(ConstantCompositionRequest request);

        #endregion

        #region Thermal Properties

        /// <summary>
        /// Calculates oil thermal conductivity
        /// </summary>
        Task<OilPropertyResultDto> CalculateThermalConductivityAsync(CalculateThermalConductivityRequest request);

        /// <summary>
        /// Calculates oil specific heat capacity
        /// </summary>
        Task<OilPropertyResultDto> CalculateSpecificHeatAsync(CalculateSpecificHeatRequest request);

        /// <summary>
        /// Calculates oil thermal expansion coefficient
        /// </summary>
        Task<OilPropertyResultDto> CalculateThermalExpansionAsync(CalculateThermalExpansionRequest request);

        #endregion

        #region Advanced PVT

        /// <summary>
        /// Performs equation of state (EOS) calculations
        /// </summary>
        Task<EOSResultDto> PerformEOSCalculationAsync(EOSRequest request);

        /// <summary>
        /// Calculates asphaltene onset pressure
        /// </summary>
        Task<OilPropertyResultDto> CalculateAsphalteneOnsetAsync(CalculateAsphalteneOnsetRequest request);

        /// <summary>
        /// Calculates wax appearance temperature
        /// </summary>
        Task<OilPropertyResultDto> CalculateWaxAppearanceAsync(CalculateWaxAppearanceRequest request);

        /// <summary>
        /// Performs viscosity blending calculations
        /// </summary>
        Task<ViscosityBlendResultDto> PerformViscosityBlendingAsync(ViscosityBlendRequest request);

        #endregion

        #region Surface Properties

        /// <summary>
        /// Calculates oil-gas interfacial tension
        /// </summary>
        Task<OilPropertyResultDto> CalculateInterfacialTensionAsync(CalculateInterfacialTensionRequest request);

        /// <summary>
        /// Calculates oil wettability
        /// </summary>
        Task<WettabilityResultDto> CalculateWettabilityAsync(WettabilityRequest request);

        /// <summary>
        /// Calculates oil pour point
        /// </summary>
        Task<OilPropertyResultDto> CalculatePourPointAsync(CalculatePourPointRequest request);

        /// <summary>
        /// Calculates oil cloud point
        /// </summary>
        Task<OilPropertyResultDto> CalculateCloudPointAsync(CalculateCloudPointRequest request);

        #endregion

        #region Correlations Management

        /// <summary>
        /// Gets available correlations for property calculation
        /// </summary>
        Task<List<CorrelationInfoDto>> GetAvailableCorrelationsAsync(string propertyType);

        /// <summary>
        /// Validates correlation applicability
        /// </summary>
        Task<CorrelationValidationDto> ValidateCorrelationAsync(CorrelationValidationRequest request);

        /// <summary>
        /// Compares different correlations for the same property
        /// </summary>
        Task<CorrelationComparisonDto> CompareCorrelationsAsync(CorrelationComparisonRequest request);

        #endregion

        #region Compositional Analysis

        /// <summary>
        /// Performs compositional analysis
        /// </summary>
        Task<CompositionalAnalysisDto> PerformCompositionalAnalysisAsync(CompositionalAnalysisRequest request);

        /// <summary>
        /// Calculates molecular weight from composition
        /// </summary>
        Task<OilPropertyResultDto> CalculateMolecularWeightAsync(CalculateMolecularWeightRequest request);

        /// <summary>
        /// Performs SARA analysis (Saturates, Aromatics, Resins, Asphaltenes)
        /// </summary>
        Task<SaraAnalysisDto> PerformSaraAnalysisAsync(SaraAnalysisRequest request);

        #endregion

        #region Laboratory Data Management

        /// <summary>
        /// Stores laboratory PVT data
        /// </summary>
        Task<PVTDataDto> StorePVTDataAsync(PVTDataDto pvtData, string userId);

        /// <summary>
        /// Retrieves laboratory PVT data
        /// </summary>
        Task<List<PVTDataDto>> GetPVTDataAsync(string sampleId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Matches laboratory data with correlations
        /// </summary>
        Task<CorrelationMatchingDto> MatchLabDataWithCorrelationsAsync(CorrelationMatchingRequest request);

        /// <summary>
        /// Validates laboratory data quality
        /// </summary>
        Task<DataQualityDto> ValidateLabDataQualityAsync(string sampleId);

        #endregion

        #region Multiphase Flow

        /// <summary>
        /// Calculates oil-water relative permeability
        /// </summary>
        Task<RelativePermeabilityDto> CalculateRelativePermeabilityAsync(RelativePermeabilityRequest request);

        /// <summary>
        /// Calculates capillary pressure
        /// </summary>
        Task<CapillaryPressureDto> CalculateCapillaryPressureAsync(CapillaryPressureRequest request);

        /// <summary>
        /// Performs emulsion viscosity calculation
        /// </summary>
        Task<OilPropertyResultDto> CalculateEmulsionViscosityAsync(CalculateEmulsionViscosityRequest request);

        #endregion

        #region Quality Control & Validation

        /// <summary>
        /// Validates oil property calculation results
        /// </summary>
        Task<ValidationResultDto> ValidateCalculationResultsAsync(ValidationRequest request);

        /// <summary>
        /// Performs uncertainty analysis on property calculations
        /// </summary>
        Task<UncertaintyAnalysisDto> PerformUncertaintyAnalysisAsync(UncertaintyAnalysisRequest request);

        /// <summary>
        /// Generates quality assurance report
        /// </summary>
        Task<QAReportDto> GenerateQAReportAsync(QAReportRequest request);

        #endregion

        #region Reporting & Export

        /// <summary>
        /// Generates PVT report
        /// </summary>
        Task<PVTReportDto> GeneratePVTReportAsync(PVTReportRequest request);

        /// <summary>
        /// Exports oil properties data
        /// </summary>
        Task<byte[]> ExportOilPropertiesDataAsync(ExportRequest request);

        /// <summary>
        /// Generates property correlation charts
        /// </summary>
        Task<byte[]> GeneratePropertyChartsAsync(ChartRequest request);

        #endregion
    }

    #region Oil Properties DTOs

    /// <summary>
    /// Oil property calculation result DTO
    /// </summary>
    public class OilPropertyResultDto
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
    /// Flash calculation result DTO
    /// </summary>
    public class FlashCalculationResultDto
    {
        public string CalculationId { get; set; } = string.Empty;
        public decimal Temperature { get; set; }
        public decimal Pressure { get; set; }
        public decimal LiquidFraction { get; set; }
        public decimal VaporFraction { get; set; }
        public OilCompositionDto LiquidComposition { get; set; } = new();
        public GasCompositionDto VaporComposition { get; set; } = new();
        public string FlashType { get; set; } = string.Empty;
        public DateTime CalculationDate { get; set; }
    }

    /// <summary>
    /// Saturation pressure result DTO
    /// </summary>
    public class SaturationPressureResultDto
    {
        public string CalculationId { get; set; } = string.Empty;
        public decimal BubblePointPressure { get; set; }
        public decimal DewPointPressure { get; set; }
        public decimal SaturationPressure { get; set; }
        public string Method { get; set; } = string.Empty;
        public DateTime CalculationDate { get; set; }
    }

    /// <summary>
    /// Differential liberation result DTO
    /// </summary>
    public class DifferentialLiberationResultDto
    {
        public string TestId { get; set; } = string.Empty;
        public List<DifferentialLiberationPointDto> LiberationPoints { get; set; } = new();
        public decimal InitialGOR { get; set; }
        public decimal ResidualOilVolume { get; set; }
        public string TestConditions { get; set; } = string.Empty;
        public DateTime TestDate { get; set; }
    }

    /// <summary>
    /// Differential liberation point DTO
    /// </summary>
    public class DifferentialLiberationPointDto
    {
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal OilVolume { get; set; }
        public decimal GasVolume { get; set; }
        public decimal GOR { get; set; }
        public decimal Density { get; set; }
    }

    /// <summary>
    /// Constant composition result DTO
    /// </summary>
    public class ConstantCompositionResultDto
    {
        public string TestId { get; set; } = string.Empty;
        public List<ConstantCompositionPointDto> ExpansionPoints { get; set; } = new();
        public decimal SaturationPressure { get; set; }
        public decimal InitialVolume { get; set; }
        public string TestConditions { get; set; } = string.Empty;
        public DateTime TestDate { get; set; }
    }

    /// <summary>
    /// Constant composition point DTO
    /// </summary>
    public class ConstantCompositionPointDto
    {
        public decimal Pressure { get; set; }
        public decimal RelativeVolume { get; set; }
        public decimal Density { get; set; }
        public decimal Compressibility { get; set; }
    }

    /// <summary>
    /// EOS result DTO
    /// </summary>
    public class EOSResultDto
    {
        public string CalculationId { get; set; } = string.Empty;
        public string EquationOfState { get; set; } = string.Empty;
        public List<EOSComponentDto> Components { get; set; } = new();
        public decimal CriticalPressure { get; set; }
        public decimal CriticalTemperature { get; set; }
        public decimal AcentricFactor { get; set; }
        public List<EOSPhaseDto> Phases { get; set; } = new();
        public DateTime CalculationDate { get; set; }
    }

    /// <summary>
    /// EOS component DTO
    /// </summary>
    public class EOSComponentDto
    {
        public string ComponentName { get; set; } = string.Empty;
        public decimal MoleFraction { get; set; }
        public decimal CriticalPressure { get; set; }
        public decimal CriticalTemperature { get; set; }
        public decimal AcentricFactor { get; set; }
        public decimal MolecularWeight { get; set; }
    }

    /// <summary>
    /// EOS phase DTO
    /// </summary>
    public class EOSPhaseDto
    {
        public string PhaseType { get; set; } = string.Empty;
        public decimal MoleFraction { get; set; }
        public decimal Density { get; set; }
        public decimal Compressibility { get; set; }
        public OilCompositionDto Composition { get; set; } = new();
    }

    /// <summary>
    /// Viscosity blend result DTO
    /// </summary>
    public class ViscosityBlendResultDto
    {
        public string BlendId { get; set; } = string.Empty;
        public List<ViscosityComponentDto> Components { get; set; } = new();
        public decimal BlendedViscosity { get; set; }
        public string BlendMethod { get; set; } = string.Empty;
        public decimal Temperature { get; set; }
        public DateTime CalculationDate { get; set; }
    }

    /// <summary>
    /// Viscosity component DTO
    /// </summary>
    public class ViscosityComponentDto
    {
        public string ComponentName { get; set; } = string.Empty;
        public decimal VolumeFraction { get; set; }
        public decimal Viscosity { get; set; }
        public decimal Density { get; set; }
    }

    /// <summary>
    /// Wettability result DTO
    /// </summary>
    public class WettabilityResultDto
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string WettabilityIndex { get; set; } = string.Empty;
        public decimal ContactAngle { get; set; }
        public string WettabilityClass { get; set; } = string.Empty;
        public string MeasurementMethod { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
    }

    /// <summary>
    /// Correlation info DTO
    /// </summary>
    public class CorrelationInfoDto
    {
        public string CorrelationId { get; set; } = string.Empty;
        public string CorrelationName { get; set; } = string.Empty;
        public string PropertyType { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int PublicationYear { get; set; }
        public string ApplicabilityRange { get; set; } = string.Empty;
        public decimal Accuracy { get; set; }
        public string Reference { get; set; } = string.Empty;
    }

    /// <summary>
    /// Correlation validation DTO
    /// </summary>
    public class CorrelationValidationDto
    {
        public string ValidationId { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
        public bool IsApplicable { get; set; }
        public List<string> ApplicabilityIssues { get; set; } = new();
        public decimal ConfidenceLevel { get; set; }
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// Correlation comparison DTO
    /// </summary>
    public class CorrelationComparisonDto
    {
        public string ComparisonId { get; set; } = string.Empty;
        public string PropertyType { get; set; } = string.Empty;
        public List<CorrelationResultDto> CorrelationResults { get; set; } = new();
        public string BestCorrelation { get; set; } = string.Empty;
        public string ComparisonSummary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Correlation result DTO
    /// </summary>
    public class CorrelationResultDto
    {
        public string CorrelationId { get; set; } = string.Empty;
        public decimal CalculatedValue { get; set; }
        public decimal Deviation { get; set; }
        public decimal AccuracyScore { get; set; }
    }

    /// <summary>
    /// Compositional analysis DTO
    /// </summary>
    public class CompositionalAnalysisDto
    {
        public string AnalysisId { get; set; } = string.Empty;
        public OilCompositionDto Composition { get; set; } = new();
        public decimal MolecularWeight { get; set; }
        public decimal SpecificGravity { get; set; }
        public decimal APIGravity { get; set; }
        public List<CompositionComponentDto> Components { get; set; } = new();
        public DateTime AnalysisDate { get; set; }
    }

    /// <summary>
    /// Composition component DTO
    /// </summary>
    public class CompositionComponentDto
    {
        public string ComponentName { get; set; } = string.Empty;
        public decimal MoleFraction { get; set; }
        public decimal MassFraction { get; set; }
        public decimal MolecularWeight { get; set; }
        public decimal BoilingPoint { get; set; }
    }

    /// <summary>
    /// SARA analysis DTO
    /// </summary>
    public class SaraAnalysisDto
    {
        public string AnalysisId { get; set; } = string.Empty;
        public decimal SaturatesFraction { get; set; }
        public decimal AromaticsFraction { get; set; }
        public decimal ResinsFraction { get; set; }
        public decimal AsphaltenesFraction { get; set; }
        public string AnalysisMethod { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
    }

    /// <summary>
    /// PVT data DTO
    /// </summary>
    public class PVTDataDto
    {
        public string SampleId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public DateTime SampleDate { get; set; }
        public decimal ReservoirTemperature { get; set; }
        public decimal BubblePointPressure { get; set; }
        public decimal InitialGOR { get; set; }
        public OilCompositionDto OilComposition { get; set; } = new();
        public List<PVTMeasurementDto> Measurements { get; set; } = new();
        public string LabName { get; set; } = string.Empty;
        public string ReportNumber { get; set; } = string.Empty;
    }

    /// <summary>
    /// PVT measurement DTO
    /// </summary>
    public class PVTMeasurementDto
    {
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal OilFVF { get; set; }
        public decimal OilDensity { get; set; }
        public decimal OilViscosity { get; set; }
        public decimal GOR { get; set; }
        public decimal GasDensity { get; set; }
        public decimal GasViscosity { get; set; }
    }

    /// <summary>
    /// Correlation matching DTO
    /// </summary>
    public class CorrelationMatchingDto
    {
        public string MatchingId { get; set; } = string.Empty;
        public string SampleId { get; set; } = string.Empty;
        public List<CorrelationMatchDto> Matches { get; set; } = new();
        public string BestMatchCorrelation { get; set; } = string.Empty;
        public decimal BestMatchAccuracy { get; set; }
    }

    /// <summary>
    /// Correlation match DTO
    /// </summary>
    public class CorrelationMatchDto
    {
        public string CorrelationId { get; set; } = string.Empty;
        public decimal AverageError { get; set; }
        public decimal MaxError { get; set; }
        public decimal R2 { get; set; }
        public string MatchQuality { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data quality DTO
    /// </summary>
    public class DataQualityDto
    {
        public string AssessmentId { get; set; } = string.Empty;
        public string SampleId { get; set; } = string.Empty;
        public decimal OverallQualityScore { get; set; }
        public List<DataQualityIssueDto> Issues { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public string QualityRating { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data quality issue DTO
    /// </summary>
    public class DataQualityIssueDto
    {
        public string IssueType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string AffectedProperty { get; set; } = string.Empty;
        public decimal Impact { get; set; }
    }

    /// <summary>
    /// Relative permeability DTO
    /// </summary>
    public class RelativePermeabilityDto
    {
        public string CalculationId { get; set; } = string.Empty;
        public List<RelativePermeabilityPointDto> Points { get; set; } = new();
        public string Correlation { get; set; } = string.Empty;
        public decimal ResidualOilSaturation { get; set; }
        public decimal ResidualWaterSaturation { get; set; }
        public DateTime CalculationDate { get; set; }
    }

    /// <summary>
    /// Relative permeability point DTO
    /// </summary>
    public class RelativePermeabilityPointDto
    {
        public decimal WaterSaturation { get; set; }
        public decimal OilRelativePermeability { get; set; }
        public decimal WaterRelativePermeability { get; set; }
    }

    /// <summary>
    /// Capillary pressure DTO
    /// </summary>
    public class CapillaryPressureDto
    {
        public string CalculationId { get; set; } = string.Empty;
        public List<CapillaryPressurePointDto> Points { get; set; } = new();
        public string Correlation { get; set; } = string.Empty;
        public decimal InterfacialTension { get; set; }
        public decimal ContactAngle { get; set; }
        public DateTime CalculationDate { get; set; }
    }

    /// <summary>
    /// Capillary pressure point DTO
    /// </summary>
    public class CapillaryPressurePointDto
    {
        public decimal WaterSaturation { get; set; }
        public decimal CapillaryPressure { get; set; }
        public decimal HeightAboveFWL { get; set; }
    }

    /// <summary>
    /// Validation result DTO
    /// </summary>
    public class ValidationResultDto
    {
        public string ValidationId { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public List<string> ValidationErrors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public decimal ConfidenceScore { get; set; }
        public string ValidationSummary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Uncertainty analysis DTO
    /// </summary>
    public class UncertaintyAnalysisDto
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string PropertyType { get; set; } = string.Empty;
        public decimal MeanValue { get; set; }
        public decimal StandardDeviation { get; set; }
        public decimal P10Value { get; set; }
        public decimal P50Value { get; set; }
        public decimal P90Value { get; set; }
        public string DistributionType { get; set; } = string.Empty;
        public List<string> KeyUncertaintyFactors { get; set; } = new();
    }

    /// <summary>
    /// QA report DTO
    /// </summary>
    public class QAReportDto
    {
        public string ReportId { get; set; } = string.Empty;
        public string SampleId { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public List<QASectionDto> Sections { get; set; } = new();
        public string OverallAssessment { get; set; } = string.Empty;
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// QA section DTO
    /// </summary>
    public class QASectionDto
    {
        public string SectionName { get; set; } = string.Empty;
        public string Assessment { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public List<string> Issues { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// PVT report DTO
    /// </summary>
    public class PVTReportDto
    {
        public string ReportId { get; set; } = string.Empty;
        public string SampleId { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public PVTDataDto PVTData { get; set; } = new();
        public List<OilPropertyResultDto> CalculatedProperties { get; set; } = new();
        public byte[] ReportContent { get; set; } = Array.Empty<byte>();
        public List<byte[]> Charts { get; set; } = new();
    }

    #endregion

    #region Composition DTOs

    /// <summary>
    /// Oil composition DTO
    /// </summary>
    public class OilCompositionDto
    {
        public string CompositionId { get; set; } = string.Empty;
        public string SampleId { get; set; } = string.Empty;
        public decimal API { get; set; }
        public decimal SpecificGravity { get; set; }
        public decimal MolecularWeight { get; set; }
        public decimal SulfurContent { get; set; }
        public decimal NitrogenContent { get; set; }
        public List<CompositionComponentDto> Components { get; set; } = new();
        public DateTime AnalysisDate { get; set; }
    }

    /// <summary>
    /// Gas composition DTO
    /// </summary>
    public class GasCompositionDto
    {
        public string CompositionId { get; set; } = string.Empty;
        public string SampleId { get; set; } = string.Empty;
        public decimal MolecularWeight { get; set; }
        public decimal SpecificGravity { get; set; }
        public decimal HeatingValue { get; set; }
        public List<CompositionComponentDto> Components { get; set; } = new();
        public DateTime AnalysisDate { get; set; }
    }

    #endregion

    #region Request DTOs

    /// <summary>
    /// Calculate FVF request DTO
    /// </summary>
    public class CalculateFVFRequest
    {
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal GOR { get; set; }
        public decimal OilGravity { get; set; }
        public string Correlation { get; set; } = "Standing";
    }

    /// <summary>
    /// Calculate density request DTO
    /// </summary>
    public class CalculateDensityRequest
    {
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal GOR { get; set; }
        public decimal OilGravity { get; set; }
        public string Correlation { get; set; } = "Standing";
    }

    /// <summary>
    /// Calculate viscosity request DTO
    /// </summary>
    public class CalculateViscosityRequest
    {
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal OilGravity { get; set; }
        public string Correlation { get; set; } = "Beggs-Robinson";
    }

    /// <summary>
    /// Calculate compressibility request DTO
    /// </summary>
    public class CalculateCompressibilityRequest
    {
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal GOR { get; set; }
        public decimal OilGravity { get; set; }
        public string Correlation { get; set; } = "Vasquez-Beggs";
    }

    /// <summary>
    /// Calculate bubble point request DTO
    /// </summary>
    public class CalculateBubblePointRequest
    {
        public decimal Temperature { get; set; }
        public decimal GOR { get; set; }
        public decimal OilGravity { get; set; }
        public string Correlation { get; set; } = "Standing";
    }

    /// <summary>
    /// Calculate solution GOR request DTO
    /// </summary>
    public class CalculateSolutionGORRequest
    {
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal OilGravity { get; set; }
        public string Correlation { get; set; } = "Standing";
    }

    /// <summary>
    /// Flash calculation request DTO
    /// </summary>
    public class FlashCalculationRequest
    {
        public OilCompositionDto FeedComposition { get; set; } = new();
        public decimal Temperature { get; set; }
        public decimal Pressure { get; set; }
        public string FlashType { get; set; } = "PT";
    }

    /// <summary>
    /// Saturation pressure request DTO
    /// </summary>
    public class SaturationPressureRequest
    {
        public OilCompositionDto Composition { get; set; } = new();
        public decimal Temperature { get; set; }
        public string Method { get; set; } = "EOS";
    }

    /// <summary>
    /// Differential liberation request DTO
    /// </summary>
    public class DifferentialLiberationRequest
    {
        public OilCompositionDto Composition { get; set; } = new();
        public decimal InitialPressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal DepletionStep { get; set; }
    }

    /// <summary>
    /// Constant composition request DTO
    /// </summary>
    public class ConstantCompositionRequest
    {
        public OilCompositionDto Composition { get; set; } = new();
        public decimal SaturationPressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal PressureStep { get; set; }
    }

    /// <summary>
    /// Calculate thermal conductivity request DTO
    /// </summary>
    public class CalculateThermalConductivityRequest
    {
        public decimal Temperature { get; set; }
        public decimal Pressure { get; set; }
        public decimal OilGravity { get; set; }
        public string Correlation { get; set; } = "Sloan";
    }

    /// <summary>
    /// Calculate specific heat request DTO
    /// </summary>
    public class CalculateSpecificHeatRequest
    {
        public decimal Temperature { get; set; }
        public decimal OilGravity { get; set; }
        public string Correlation { get; set; } = "Kesler-Lee";
    }

    /// <summary>
    /// Calculate thermal expansion request DTO
    /// </summary>
    public class CalculateThermalExpansionRequest
    {
        public decimal Temperature { get; set; }
        public decimal Pressure { get; set; }
        public decimal OilGravity { get; set; }
        public string Correlation { get; set; } = "Standing";
    }

    /// <summary>
    /// EOS request DTO
    /// </summary>
    public class EOSRequest
    {
        public OilCompositionDto Composition { get; set; } = new();
        public decimal Temperature { get; set; }
        public decimal Pressure { get; set; }
        public string EquationOfState { get; set; } = "Peng-Robinson";
        public string MixingRule { get; set; } = "VanDerWaals";
    }

    /// <summary>
    /// Calculate asphaltene onset request DTO
    /// </summary>
    public class CalculateAsphalteneOnsetRequest
    {
        public OilCompositionDto Composition { get; set; } = new();
        public decimal Temperature { get; set; }
        public string Method { get; set; } = "Flory-Huggins";
    }

    /// <summary>
    /// Calculate wax appearance request DTO
    /// </summary>
    public class CalculateWaxAppearanceRequest
    {
        public OilCompositionDto Composition { get; set; } = new();
        public string Method { get; set; } = "CloudPoint";
    }

    /// <summary>
    /// Viscosity blend request DTO
    /// </summary>
    public class ViscosityBlendRequest
    {
        public List<ViscosityComponentDto> Components { get; set; } = new();
        public decimal Temperature { get; set; }
        public string BlendMethod { get; set; } = "Refutas";
    }

    /// <summary>
    /// Calculate interfacial tension request DTO
    /// </summary>
    public class CalculateInterfacialTensionRequest
    {
        public decimal Temperature { get; set; }
        public decimal OilGravity { get; set; }
        public decimal GasGravity { get; set; }
        public string Correlation { get; set; } = "Bacu";
    }

    /// <summary>
    /// Wettability request DTO
    /// </summary>
    public class WettabilityRequest
    {
        public string SampleId { get; set; } = string.Empty;
        public decimal ContactAngle { get; set; }
        public string MeasurementMethod { get; set; } = string.Empty;
    }

    /// <summary>
    /// Calculate pour point request DTO
    /// </summary>
    public class CalculatePourPointRequest
    {
        public OilCompositionDto Composition { get; set; } = new();
        public string Method { get; set; } = "API";
    }

    /// <summary>
    /// Calculate cloud point request DTO
    /// </summary>
    public class CalculateCloudPointRequest
    {
        public OilCompositionDto Composition { get; set; } = new();
        public string Method { get; set; } = "API";
    }

    /// <summary>
    /// Correlation validation request DTO
    /// </summary>
    public class CorrelationValidationRequest
    {
        public string CorrelationId { get; set; } = string.Empty;
        public string PropertyType { get; set; } = string.Empty;
        public Dictionary<string, decimal> InputParameters { get; set; } = new();
        public decimal? MeasuredValue { get; set; }
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
    }

    /// <summary>
    /// Compositional analysis request DTO
    /// </summary>
    public class CompositionalAnalysisRequest
    {
        public OilCompositionDto Composition { get; set; } = new();
        public string AnalysisType { get; set; } = "Full";
    }

    /// <summary>
    /// Calculate molecular weight request DTO
    /// </summary>
    public class CalculateMolecularWeightRequest
    {
        public OilCompositionDto Composition { get; set; } = new();
        public string Method { get; set; } = "WeightedAverage";
    }

    /// <summary>
    /// SARA analysis request DTO
    /// </summary>
    public class SaraAnalysisRequest
    {
        public string SampleId { get; set; } = string.Empty;
        public string AnalysisMethod { get; set; } = "ASTM";
    }

    /// <summary>
    /// Correlation matching request DTO
    /// </summary>
    public class CorrelationMatchingRequest
    {
        public string SampleId { get; set; } = string.Empty;
        public string PropertyType { get; set; } = string.Empty;
        public List<string> CorrelationIds { get; set; } = new();
    }

    /// <summary>
    /// Relative permeability request DTO
    /// </summary>
    public class RelativePermeabilityRequest
    {
        public decimal ResidualOilSaturation { get; set; }
        public decimal ResidualWaterSaturation { get; set; }
        public string Correlation { get; set; } = "Corey";
    }

    /// <summary>
    /// Capillary pressure request DTO
    /// </summary>
    public class CapillaryPressureRequest
    {
        public decimal InterfacialTension { get; set; }
        public decimal ContactAngle { get; set; }
        public decimal Porosity { get; set; }
        public decimal Permeability { get; set; }
        public string Correlation { get; set; } = "LeverettJ";
    }

    /// <summary>
    /// Calculate emulsion viscosity request DTO
    /// </summary>
    public class CalculateEmulsionViscosityRequest
    {
        public decimal OilViscosity { get; set; }
        public decimal WaterViscosity { get; set; }
        public decimal WaterCut { get; set; }
        public string EmulsionType { get; set; } = "OilInWater";
    }

    /// <summary>
    /// Validation request DTO
    /// </summary>
    public class ValidationRequest
    {
        public OilPropertyResultDto CalculationResult { get; set; } = new();
        public List<string> ValidationRules { get; set; } = new();
        public decimal? MeasuredValue { get; set; }
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
    }

    /// <summary>
    /// QA report request DTO
    /// </summary>
    public class QAReportRequest
    {
        public string SampleId { get; set; } = string.Empty;
        public List<string> QASections { get; set; } = new();
        public bool IncludeRecommendations { get; set; } = true;
    }

    /// <summary>
    /// PVT report request DTO
    /// </summary>
    public class PVTReportRequest
    {
        public string SampleId { get; set; } = string.Empty;
        public List<string> IncludeProperties { get; set; } = new();
        public bool IncludeCharts { get; set; } = true;
        public string Format { get; set; } = "PDF";
    }

    /// <summary>
    /// Export request DTO
    /// </summary>
    public class ExportRequest
    {
        public string SampleId { get; set; } = string.Empty;
        public List<string> DataTypes { get; set; } = new();
        public string Format { get; set; } = "Excel";
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// Chart request DTO
    /// </summary>
    public class ChartRequest
    {
        public string SampleId { get; set; } = string.Empty;
        public List<string> Properties { get; set; } = new();
        public string ChartType { get; set; } = "PVT";
        public int Width { get; set; } = 800;
        public int Height { get; set; } = 600;
    }

    #endregion
}