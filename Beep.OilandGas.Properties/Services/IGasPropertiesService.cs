using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beep.OilandGas.Properties.Services
{
    /// <summary>
    /// Comprehensive gas properties service interface
    /// Provides industry-standard correlations and calculations for gas PVT properties
    /// </summary>
    public interface IGasPropertiesService
    {
        #region PVT Properties

        /// <summary>
        /// Calculates gas compressibility factor (Z-factor)
        /// </summary>
        Task<GasPropertyResult> CalculateZFactorAsync(CalculateZFactorRequest request);

        /// <summary>
        /// Calculates gas density at reservoir conditions
        /// </summary>
        Task<GasPropertyResult> CalculateDensityAsync(CalculateGasDensityRequest request);

        /// <summary>
        /// Calculates gas viscosity at reservoir conditions
        /// </summary>
        Task<GasPropertyResult> CalculateViscosityAsync(CalculateGasViscosityRequest request);

        /// <summary>
        /// Calculates gas compressibility
        /// </summary>
        Task<GasPropertyResult> CalculateCompressibilityAsync(CalculateGasCompressibilityRequest request);

        /// <summary>
        /// Calculates gas formation volume factor
        /// </summary>
        Task<GasPropertyResult> CalculateFormationVolumeFactorAsync(CalculateGasFVFRequest request);

        #endregion

        #region Phase Behavior

        /// <summary>
        /// Calculates dew point pressure
        /// </summary>
        Task<GasPropertyResult> CalculateDewPointPressureAsync(CalculateDewPointRequest request);

        /// <summary>
        /// Performs gas condensate flash calculation
        /// </summary>
        Task<GasCondensateFlashResult> PerformCondensateFlashAsync(GasCondensateFlashRequest request);

        /// <summary>
        /// Calculates gas condensate properties
        /// </summary>
        Task<GasCondensateProperties> CalculateCondensatePropertiesAsync(GasCondensatePropertiesRequest request);

        /// <summary>
        /// Performs constant volume depletion (CVD) test simulation
        /// </summary>
        Task<ConstantVolumeDepletionResult> PerformCVDTestAsync(ConstantVolumeDepletionRequest request);

        #endregion

        #region Transport Properties

        /// <summary>
        /// Calculates gas thermal conductivity
        /// </summary>
        Task<GasPropertyResult> CalculateThermalConductivityAsync(CalculateGasThermalConductivityRequest request);

        /// <summary>
        /// Calculates gas specific heat capacity
        /// </summary>
        Task<GasPropertyResult> CalculateSpecificHeatAsync(CalculateGasSpecificHeatRequest request);

        /// <summary>
        /// Calculates gas Joule-Thomson coefficient
        /// </summary>
        Task<GasPropertyResult> CalculateJouleThomsonCoefficientAsync(CalculateJouleThomsonRequest request);

        #endregion

        #region Advanced PVT

        /// <summary>
        /// Performs equation of state calculations for gas
        /// </summary>
        Task<GasEOSResult> PerformGasEOSCalculationAsync(GasEOSRequest request);

        /// <summary>
        /// Calculates gas hydrate formation conditions
        /// </summary>
        Task<GasHydrateResult> CalculateHydrateFormationAsync(GasHydrateRequest request);

        /// <summary>
        /// Calculates gas water content (dew point)
        /// </summary>
        Task<GasPropertyResult> CalculateWaterContentAsync(CalculateWaterContentRequest request);

        /// <summary>
        /// Performs gas mixing calculations
        /// </summary>
        Task<GasMixtureResult> PerformGasMixingAsync(GasMixtureRequest request);

        #endregion

        #region Flow Properties

        /// <summary>
        /// Calculates gas pseudo-critical properties
        /// </summary>
        Task<GasPseudoCritical> CalculatePseudoCriticalPropertiesAsync(CalculatePseudoCriticalRequest request);

        /// <summary>
        /// Calculates gas pseudo-reduced properties
        /// </summary>
        Task<GasPseudoReduced> CalculatePseudoReducedPropertiesAsync(CalculatePseudoReducedRequest request);

        /// <summary>
        /// Performs gas slippage calculations
        /// </summary>
        Task<GasSlippageResult> CalculateGasSlippageAsync(GasSlippageRequest request);

        #endregion

        #region Surface Properties

        /// <summary>
        /// Calculates gas-oil interfacial tension
        /// </summary>
        Task<GasPropertyResult> CalculateGasOilIFTAsync(CalculateGasOilIFTRequest request);

        /// <summary>
        /// Calculates gas-water interfacial tension
        /// </summary>
        Task<GasPropertyResult> CalculateGasWaterIFTAsync(CalculateGasWaterIFTRequest request);

        /// <summary>
        /// Calculates gas surface tension
        /// </summary>
        Task<GasPropertyResult> CalculateSurfaceTensionAsync(CalculateGasSurfaceTensionRequest request);

        #endregion

        #region Correlations Management

        /// <summary>
        /// Gets available correlations for gas property calculation
        /// </summary>
        Task<List<GasCorrelationInfo>> GetAvailableCorrelationsAsync(string propertyType);

        /// <summary>
        /// Validates correlation applicability for gas systems
        /// </summary>
        Task<GasCorrelationValidation> ValidateCorrelationAsync(GasCorrelationValidationRequest request);

        /// <summary>
        /// Compares different correlations for the same gas property
        /// </summary>
        Task<GasCorrelationComparison> CompareCorrelationsAsync(GasCorrelationComparisonRequest request);

        #endregion

        #region Compositional Analysis

        /// <summary>
        /// Performs gas compositional analysis
        /// </summary>
        Task<GasCompositionalAnalysis> PerformGasCompositionalAnalysisAsync(GasCompositionalAnalysisRequest request);

        /// <summary>
        /// Calculates gas molecular weight
        /// </summary>
        Task<GasPropertyResult> CalculateMolecularWeightAsync(CalculateGasMolecularWeightRequest request);

        /// <summary>
        /// Performs gas chromatography analysis
        /// </summary>
        Task<GasChromatography> PerformGasChromatographyAsync(GasChromatographyRequest request);

        #endregion

        #region Laboratory Data Management

        /// <summary>
        /// Stores laboratory gas PVT data
        /// </summary>
        Task<GasPVTData> StoreGasPVTDataAsync(GasPVTData pvtData, string userId);

        /// <summary>
        /// Retrieves laboratory gas PVT data
        /// </summary>
        Task<List<GasPVTData>> GetGasPVTDataAsync(string sampleId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Matches laboratory data with gas correlations
        /// </summary>
        Task<GasCorrelationMatching> MatchLabDataWithCorrelationsAsync(GasCorrelationMatchingRequest request);

        /// <summary>
        /// Validates gas laboratory data quality
        /// </summary>
        Task<GasDataQuality> ValidateGasLabDataQualityAsync(string sampleId);

        #endregion

        #region Multiphase Flow

        /// <summary>
        /// Calculates gas-liquid relative permeability
        /// </summary>
        Task<GasRelativePermeability> CalculateGasRelativePermeabilityAsync(GasRelativePermeabilityRequest request);

        /// <summary>
        /// Performs gas slippage factor calculations
        /// </summary>
        Task<GasSlippageFactor> CalculateGasSlippageFactorAsync(GasSlippageFactorRequest request);

        #endregion

        #region Quality Control & Validation

        /// <summary>
        /// Validates gas property calculation results
        /// </summary>
        Task<GasValidationResult> ValidateCalculationResultsAsync(GasValidationRequest request);

        /// <summary>
        /// Performs uncertainty analysis on gas property calculations
        /// </summary>
        Task<GasUncertaintyAnalysis> PerformUncertaintyAnalysisAsync(GasUncertaintyAnalysisRequest request);

        /// <summary>
        /// Generates gas properties quality assurance report
        /// </summary>
        Task<GasQAReport> GenerateGasQAReportAsync(GasQAReportRequest request);

        #endregion

        #region Reporting & Export

        /// <summary>
        /// Generates gas PVT report
        /// </summary>
        Task<GasPVTReport> GenerateGasPVTReportAsync(GasPVTReportRequest request);

        /// <summary>
        /// Exports gas properties data
        /// </summary>
        Task<byte[]> ExportGasPropertiesDataAsync(GasExportRequest request);

        /// <summary>
        /// Generates gas property correlation charts
        /// </summary>
        Task<byte[]> GenerateGasPropertyChartsAsync(GasChartRequest request);

        #endregion
    }

    #region Gas Properties DTOs

    /// <summary>
    /// Gas property calculation result DTO
    /// </summary>
    public class GasPropertyResult
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
    /// Gas condensate flash result DTO
    /// </summary>
    public class GasCondensateFlashResult
    {
        public string CalculationId { get; set; } = string.Empty;
        public decimal Temperature { get; set; }
        public decimal Pressure { get; set; }
        public decimal DewPointPressure { get; set; }
        public decimal CGR { get; set; } // Condensate Gas Ratio
        public GasComposition VaporComposition { get; set; } = new();
        public OilComposition LiquidComposition { get; set; } = new();
        public string FlashType { get; set; } = string.Empty;
        public DateTime CalculationDate { get; set; }
    }

    /// <summary>
    /// Gas condensate properties DTO
    /// </summary>
    public class GasCondensateProperties
    {
        public string AnalysisId { get; set; } = string.Empty;
        public decimal StockTankAPI { get; set; }
        public decimal StockTankDensity { get; set; }
        public decimal Viscosity { get; set; }
        public decimal MolecularWeight { get; set; }
        public List<CondensateFraction> Fractions { get; set; } = new();
        public DateTime AnalysisDate { get; set; }
    }

    /// <summary>
    /// Condensate fraction DTO
    /// </summary>
    public class CondensateFraction
    {
        public string FractionName { get; set; } = string.Empty;
        public decimal BoilingPoint { get; set; }
        public decimal MassFraction { get; set; }
        public decimal MolecularWeight { get; set; }
        public decimal SpecificGravity { get; set; }
    }

    /// <summary>
    /// Constant volume depletion result DTO
    /// </summary>
    public class ConstantVolumeDepletionResult
    {
        public string TestId { get; set; } = string.Empty;
        public List<CVDPoint> DepletionPoints { get; set; } = new();
        public decimal InitialPressure { get; set; }
        public decimal DewPointPressure { get; set; }
        public decimal MaximumLiquidDropout { get; set; }
        public string TestConditions { get; set; } = string.Empty;
        public DateTime TestDate { get; set; }
    }

    /// <summary>
    /// CVD point DTO
    /// </summary>
    public class CVDPoint
    {
        public decimal Pressure { get; set; }
        public decimal RelativeVolume { get; set; }
        public decimal LiquidDropout { get; set; }
        public decimal ZFactor { get; set; }
        public decimal GOR { get; set; }
    }

    /// <summary>
    /// Gas EOS result DTO
    /// </summary>
    public class GasEOSResult
    {
        public string CalculationId { get; set; } = string.Empty;
        public string EquationOfState { get; set; } = string.Empty;
        public List<GasEOSComponent> Components { get; set; } = new();
        public decimal CriticalPressure { get; set; }
        public decimal CriticalTemperature { get; set; }
        public decimal AcentricFactor { get; set; }
        public List<GasEOSPhase> Phases { get; set; } = new();
        public DateTime CalculationDate { get; set; }
    }

    /// <summary>
    /// Gas EOS component DTO
    /// </summary>
    public class GasEOSComponent
    {
        public string ComponentName { get; set; } = string.Empty;
        public decimal MoleFraction { get; set; }
        public decimal CriticalPressure { get; set; }
        public decimal CriticalTemperature { get; set; }
        public decimal AcentricFactor { get; set; }
        public decimal MolecularWeight { get; set; }
        public decimal Tc { get; set; }
        public decimal Pc { get; set; }
        public decimal Omega { get; set; }
    }

    /// <summary>
    /// Gas EOS phase DTO
    /// </summary>
    public class GasEOSPhase
    {
        public string PhaseType { get; set; } = string.Empty;
        public decimal MoleFraction { get; set; }
        public decimal Density { get; set; }
        public decimal Compressibility { get; set; }
        public GasComposition Composition { get; set; } = new();
    }

    /// <summary>
    /// Gas hydrate result DTO
    /// </summary>
    public class GasHydrateResult
    {
        public string CalculationId { get; set; } = string.Empty;
        public decimal HydrateFormationPressure { get; set; }
        public decimal HydrateFormationTemperature { get; set; }
        public string HydrateStructure { get; set; } = string.Empty;
        public decimal WaterContent { get; set; }
        public List<HydrateInhibitor> Inhibitors { get; set; } = new();
        public string Correlation { get; set; } = string.Empty;
        public DateTime CalculationDate { get; set; }
    }

    /// <summary>
    /// Hydrate inhibitor DTO
    /// </summary>
    public class HydrateInhibitor
    {
        public string InhibitorName { get; set; } = string.Empty;
        public decimal Concentration { get; set; }
        public decimal SuppressionTemperature { get; set; }
        public string Unit { get; set; } = string.Empty;
    }

    /// <summary>
    /// Gas mixture result DTO
    /// </summary>
    public class GasMixtureResult
    {
        public string MixtureId { get; set; } = string.Empty;
        public List<GasMixtureComponent> Components { get; set; } = new();
        public decimal MixtureMolecularWeight { get; set; }
        public decimal MixtureSpecificGravity { get; set; }
        public decimal MixtureCriticalPressure { get; set; }
        public decimal MixtureCriticalTemperature { get; set; }
        public decimal MixtureAcentricFactor { get; set; }
        public DateTime CalculationDate { get; set; }
    }

    /// <summary>
    /// Gas mixture component DTO
    /// </summary>
    public class GasMixtureComponent
    {
        public string ComponentName { get; set; } = string.Empty;
        public decimal MoleFraction { get; set; }
        public decimal VolumeFraction { get; set; }
        public decimal MassFraction { get; set; }
    }

    /// <summary>
    /// Gas pseudo-critical properties DTO
    /// </summary>
    public class GasPseudoCritical
    {
        public string CalculationId { get; set; } = string.Empty;
        public decimal PseudoCriticalPressure { get; set; }
        public decimal PseudoCriticalTemperature { get; set; }
        public decimal PseudoCriticalVolume { get; set; }
        public decimal PseudoCriticalCompressibility { get; set; }
        public string CalculationMethod { get; set; } = string.Empty;
        public DateTime CalculationDate { get; set; }
    }

    /// <summary>
    /// Gas pseudo-reduced properties DTO
    /// </summary>
    public class GasPseudoReduced
    {
        public string CalculationId { get; set; } = string.Empty;
        public decimal PseudoReducedPressure { get; set; }
        public decimal PseudoReducedTemperature { get; set; }
        public decimal PseudoReducedVolume { get; set; }
        public decimal PseudoReducedCompressibility { get; set; }
        public DateTime CalculationDate { get; set; }
    }

    /// <summary>
    /// Gas slippage result DTO
    /// </summary>
    public class GasSlippageResult
    {
        public string CalculationId { get; set; } = string.Empty;
        public decimal SlippageFactor { get; set; }
        public decimal TurbulentSlippage { get; set; }
        public decimal MolecularSlippage { get; set; }
        public string Correlation { get; set; } = string.Empty;
        public DateTime CalculationDate { get; set; }
    }

    /// <summary>
    /// Gas correlation info DTO
    /// </summary>
    public class GasCorrelationInfo
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
    /// Gas correlation validation DTO
    /// </summary>
    public class GasCorrelationValidation
    {
        public string ValidationId { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
        public bool IsApplicable { get; set; }
        public List<string> ApplicabilityIssues { get; set; } = new();
        public decimal ConfidenceLevel { get; set; }
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// Gas correlation comparison DTO
    /// </summary>
    public class GasCorrelationComparison
    {
        public string ComparisonId { get; set; } = string.Empty;
        public string PropertyType { get; set; } = string.Empty;
        public List<GasCorrelationResult> CorrelationResults { get; set; } = new();
        public string BestCorrelation { get; set; } = string.Empty;
        public string ComparisonSummary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Gas correlation result DTO
    /// </summary>
    public class GasCorrelationResult
    {
        public string CorrelationId { get; set; } = string.Empty;
        public decimal CalculatedValue { get; set; }
        public decimal Deviation { get; set; }
        public decimal AccuracyScore { get; set; }
    }

    /// <summary>
    /// Gas compositional analysis DTO
    /// </summary>
    public class GasCompositionalAnalysis
    {
        public string AnalysisId { get; set; } = string.Empty;
        public GasComposition Composition { get; set; } = new();
        public decimal MolecularWeight { get; set; }
        public decimal SpecificGravity { get; set; }
        public decimal HeatingValue { get; set; }
        public decimal WobbeIndex { get; set; }
        public List<GasComponentAnalysis> Components { get; set; } = new();
        public DateTime AnalysisDate { get; set; }
    }

    /// <summary>
    /// Gas component analysis DTO
    /// </summary>
    public class GasComponentAnalysis
    {
        public string ComponentName { get; set; } = string.Empty;
        public decimal MoleFraction { get; set; }
        public decimal VolumeFraction { get; set; }
        public decimal MolecularWeight { get; set; }
        public decimal BoilingPoint { get; set; }
        public decimal CriticalPressure { get; set; }
        public decimal CriticalTemperature { get; set; }
    }

    /// <summary>
    /// Gas chromatography DTO
    /// </summary>
    public class GasChromatography
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string SampleId { get; set; } = string.Empty;
        public List<GasChromatogramPeak> Peaks { get; set; } = new();
        public decimal TotalMoles { get; set; }
        public decimal UnidentifiedFraction { get; set; }
        public string InstrumentType { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
    }

    /// <summary>
    /// Gas chromatogram peak DTO
    /// </summary>
    public class GasChromatogramPeak
    {
        public string ComponentName { get; set; } = string.Empty;
        public decimal RetentionTime { get; set; }
        public decimal PeakArea { get; set; }
        public decimal MoleFraction { get; set; }
        public decimal Concentration { get; set; }
    }

    /// <summary>
    /// Gas PVT data DTO
    /// </summary>
    public class GasPVTData
    {
        public string SampleId { get; set; } = string.Empty;
        public string WellUWI { get; set; } = string.Empty;
        public DateTime SampleDate { get; set; }
        public decimal ReservoirTemperature { get; set; }
        public GasComposition GasComposition { get; set; } = new();
        public List<GasPVTMeasurement> Measurements { get; set; } = new();
        public string LabName { get; set; } = string.Empty;
        public string ReportNumber { get; set; } = string.Empty;
    }

    /// <summary>
    /// Gas PVT measurement DTO
    /// </summary>
    public class GasPVTMeasurement
    {
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal ZFactor { get; set; }
        public decimal Density { get; set; }
        public decimal Viscosity { get; set; }
        public decimal Compressibility { get; set; }
        public decimal Deviation { get; set; }
    }

    /// <summary>
    /// Gas correlation matching DTO
    /// </summary>
    public class GasCorrelationMatching
    {
        public string MatchingId { get; set; } = string.Empty;
        public string SampleId { get; set; } = string.Empty;
        public List<GasCorrelationMatch> Matches { get; set; } = new();
        public string BestMatchCorrelation { get; set; } = string.Empty;
        public decimal BestMatchAccuracy { get; set; }
    }

    /// <summary>
    /// Gas correlation match DTO
    /// </summary>
    public class GasCorrelationMatch
    {
        public string CorrelationId { get; set; } = string.Empty;
        public decimal AverageError { get; set; }
        public decimal MaxError { get; set; }
        public decimal R2 { get; set; }
        public string MatchQuality { get; set; } = string.Empty;
    }

    /// <summary>
    /// Gas data quality DTO
    /// </summary>
    public class GasDataQuality
    {
        public string AssessmentId { get; set; } = string.Empty;
        public string SampleId { get; set; } = string.Empty;
        public decimal OverallQualityScore { get; set; }
        public List<GasDataQualityIssue> Issues { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public string QualityRating { get; set; } = string.Empty;
    }

    /// <summary>
    /// Gas data quality issue DTO
    /// </summary>
    public class GasDataQualityIssue
    {
        public string IssueType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string AffectedProperty { get; set; } = string.Empty;
        public decimal Impact { get; set; }
    }

    /// <summary>
    /// Gas relative permeability DTO
    /// </summary>
    public class GasRelativePermeability
    {
        public string CalculationId { get; set; } = string.Empty;
        public List<GasRelativePermeabilityPoint> Points { get; set; } = new();
        public string Correlation { get; set; } = string.Empty;
        public decimal ResidualLiquidSaturation { get; set; }
        public decimal CriticalGasSaturation { get; set; }
        public DateTime CalculationDate { get; set; }
    }

    /// <summary>
    /// Gas relative permeability point DTO
    /// </summary>
    public class GasRelativePermeabilityPoint
    {
        public decimal GasSaturation { get; set; }
        public decimal GasRelativePermeability { get; set; }
        public decimal LiquidRelativePermeability { get; set; }
    }

    /// <summary>
    /// Gas slippage factor DTO
    /// </summary>
    public class GasSlippageFactor
    {
        public string CalculationId { get; set; } = string.Empty;
        public decimal SlippageFactor { get; set; }
        public decimal KnudsenNumber { get; set; }
        public decimal RarefactionParameter { get; set; }
        public string Correlation { get; set; } = string.Empty;
        public DateTime CalculationDate { get; set; }
    }

    /// <summary>
    /// Gas validation result DTO
    /// </summary>
    public class GasValidationResult
    {
        public string ValidationId { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public List<string> ValidationErrors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public decimal ConfidenceScore { get; set; }
        public string ValidationSummary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Gas uncertainty analysis DTO
    /// </summary>
    public class GasUncertaintyAnalysis
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
    /// Gas QA report DTO
    /// </summary>
    public class GasQAReport
    {
        public string ReportId { get; set; } = string.Empty;
        public string SampleId { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public List<GasQASection> Sections { get; set; } = new();
        public string OverallAssessment { get; set; } = string.Empty;
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// Gas QA section DTO
    /// </summary>
    public class GasQASection
    {
        public string SectionName { get; set; } = string.Empty;
        public string Assessment { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public List<string> Issues { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// Gas PVT report DTO
    /// </summary>
    public class GasPVTReport
    {
        public string ReportId { get; set; } = string.Empty;
        public string SampleId { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public GasPVTData PVTData { get; set; } = new();
        public List<GasPropertyResult> CalculatedProperties { get; set; } = new();
        public byte[] ReportContent { get; set; } = Array.Empty<byte>();
        public List<byte[]> Charts { get; set; } = new();
    }

    #endregion

    #region Gas Composition DTOs

    /// <summary>
    /// Gas composition DTO
    /// </summary>
    public class GasComposition
    {
        public string CompositionId { get; set; } = string.Empty;
        public string SampleId { get; set; } = string.Empty;
        public decimal MolecularWeight { get; set; }
        public decimal SpecificGravity { get; set; }
        public decimal HeatingValue { get; set; }
        public decimal WobbeIndex { get; set; }
        public List<GasComponent> Components { get; set; } = new();
        public DateTime AnalysisDate { get; set; }
    }

    /// <summary>
    /// Gas component DTO
    /// </summary>
    public class GasComponent
    {
        public string ComponentName { get; set; } = string.Empty;
        public decimal MoleFraction { get; set; }
        public decimal VolumeFraction { get; set; }
        public decimal MassFraction { get; set; }
        public decimal MolecularWeight { get; set; }
        public decimal CriticalPressure { get; set; }
        public decimal CriticalTemperature { get; set; }
        public decimal AcentricFactor { get; set; }
        public decimal BoilingPoint { get; set; }
    }

    #endregion

    #region Request DTOs

    /// <summary>
    /// Calculate Z-factor request DTO
    /// </summary>
    public class CalculateZFactorRequest
    {
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal PseudoReducedPressure { get; set; }
        public decimal PseudoReducedTemperature { get; set; }
        public string Correlation { get; set; } = "Dranchuk-AbouKassem";
    }

    /// <summary>
    /// Calculate gas density request DTO
    /// </summary>
    public class CalculateGasDensityRequest
    {
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal ZFactor { get; set; }
        public decimal MolecularWeight { get; set; }
        public string Correlation { get; set; } = "RealGas";
    }

    /// <summary>
    /// Calculate gas viscosity request DTO
    /// </summary>
    public class CalculateGasViscosityRequest
    {
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal Density { get; set; }
        public decimal MolecularWeight { get; set; }
        public string Correlation { get; set; } = "Lee-Gonzalez-Eakin";
    }

    /// <summary>
    /// Calculate gas compressibility request DTO
    /// </summary>
    public class CalculateGasCompressibilityRequest
    {
        public decimal Pressure { get; set; }
        public decimal ZFactor { get; set; }
        public string Correlation { get; set; } = "RealGas";
    }

    /// <summary>
    /// Calculate gas FVF request DTO
    /// </summary>
    public class CalculateGasFVFRequest
    {
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal ZFactor { get; set; }
        public string Correlation { get; set; } = "RealGas";
    }

    /// <summary>
    /// Calculate dew point request DTO
    /// </summary>
    public class CalculateDewPointRequest
    {
        public GasComposition Composition { get; set; } = new();
        public decimal Temperature { get; set; }
        public string Method { get; set; } = "Wilson";
    }

    /// <summary>
    /// Gas condensate flash request DTO
    /// </summary>
    public class GasCondensateFlashRequest
    {
        public GasComposition FeedComposition { get; set; } = new();
        public decimal Temperature { get; set; }
        public decimal Pressure { get; set; }
        public string FlashType { get; set; } = "PT";
    }

    /// <summary>
    /// Gas condensate properties request DTO
    /// </summary>
    public class GasCondensatePropertiesRequest
    {
        public string SampleId { get; set; } = string.Empty;
        public decimal DewPointPressure { get; set; }
        public decimal DewPointTemperature { get; set; }
    }

    /// <summary>
    /// Constant volume depletion request DTO
    /// </summary>
    public class ConstantVolumeDepletionRequest
    {
        public GasComposition Composition { get; set; } = new();
        public decimal InitialPressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal DepletionStep { get; set; }
    }

    /// <summary>
    /// Calculate gas thermal conductivity request DTO
    /// </summary>
    public class CalculateGasThermalConductivityRequest
    {
        public decimal Temperature { get; set; }
        public decimal Pressure { get; set; }
        public decimal MolecularWeight { get; set; }
        public string Correlation { get; set; } = "Stiel-Thodos";
    }

    /// <summary>
    /// Calculate gas specific heat request DTO
    /// </summary>
    public class CalculateGasSpecificHeatRequest
    {
        public decimal Temperature { get; set; }
        public GasComposition Composition { get; set; } = new();
        public string Correlation { get; set; } = "IdealGas";
    }

    /// <summary>
    /// Calculate Joule-Thomson request DTO
    /// </summary>
    public class CalculateJouleThomsonRequest
    {
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal ZFactor { get; set; }
        public string Correlation { get; set; } = "Katz";
    }

    /// <summary>
    /// Gas EOS request DTO
    /// </summary>
    public class GasEOSRequest
    {
        public GasComposition Composition { get; set; } = new();
        public decimal Temperature { get; set; }
        public decimal Pressure { get; set; }
        public string EquationOfState { get; set; } = "Peng-Robinson";
        public string MixingRule { get; set; } = "VanDerWaals";
    }

    /// <summary>
    /// Gas hydrate request DTO
    /// </summary>
    public class GasHydrateRequest
    {
        public GasComposition Composition { get; set; } = new();
        public decimal Temperature { get; set; }
        public decimal Pressure { get; set; }
        public string Correlation { get; set; } = "Katz";
    }

    /// <summary>
    /// Calculate water content request DTO
    /// </summary>
    public class CalculateWaterContentRequest
    {
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public string Correlation { get; set; } = "McKetta-Wehe";
    }

    /// <summary>
    /// Gas mixture request DTO
    /// </summary>
    public class GasMixtureRequest
    {
        public List<GasMixtureComponent> Components { get; set; } = new();
        public string MixingRule { get; set; } = "Kay";
    }

    /// <summary>
    /// Calculate pseudo-critical request DTO
    /// </summary>
    public class CalculatePseudoCriticalRequest
    {
        public GasComposition Composition { get; set; } = new();
        public string Method { get; set; } = "Kay";
    }

    /// <summary>
    /// Calculate pseudo-reduced request DTO
    /// </summary>
    public class CalculatePseudoReducedRequest
    {
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal PseudoCriticalPressure { get; set; }
        public decimal PseudoCriticalTemperature { get; set; }
    }

    /// <summary>
    /// Gas slippage request DTO
    /// </summary>
    public class GasSlippageRequest
    {
        public decimal Pressure { get; set; }
        public decimal Temperature { get; set; }
        public decimal PoreRadius { get; set; }
        public decimal MeanFreePath { get; set; }
        public string Correlation { get; set; } = "Klinkenberg";
    }

    /// <summary>
    /// Calculate gas-oil IFT request DTO
    /// </summary>
    public class CalculateGasOilIFTRequest
    {
        public decimal Temperature { get; set; }
        public decimal GasDensity { get; set; }
        public decimal OilDensity { get; set; }
        public string Correlation { get; set; } = "Firoozabadi";
    }

    /// <summary>
    /// Calculate gas-water IFT request DTO
    /// </summary>
    public class CalculateGasWaterIFTRequest
    {
        public decimal Temperature { get; set; }
        public decimal Pressure { get; set; }
        public decimal Salinity { get; set; }
        public string Correlation { get; set; } = "Sutton";
    }

    /// <summary>
    /// Calculate gas surface tension request DTO
    /// </summary>
    public class CalculateGasSurfaceTensionRequest
    {
        public decimal Temperature { get; set; }
        public decimal CriticalTemperature { get; set; }
        public string Correlation { get; set; } = "CorrespondingStates";
    }

    /// <summary>
    /// Gas correlation validation request DTO
    /// </summary>
    public class GasCorrelationValidationRequest
    {
        public string CorrelationId { get; set; } = string.Empty;
        public string PropertyType { get; set; } = string.Empty;
        public Dictionary<string, decimal> InputParameters { get; set; } = new();
        public decimal? MeasuredValue { get; set; }
    }

    /// <summary>
    /// Gas correlation comparison request DTO
    /// </summary>
    public class GasCorrelationComparisonRequest
    {
        public string PropertyType { get; set; } = string.Empty;
        public Dictionary<string, decimal> InputParameters { get; set; } = new();
        public List<string> CorrelationIds { get; set; } = new();
        public decimal? MeasuredValue { get; set; }
    }

    /// <summary>
    /// Gas compositional analysis request DTO
    /// </summary>
    public class GasCompositionalAnalysisRequest
    {
        public GasComposition Composition { get; set; } = new();
        public string AnalysisType { get; set; } = "Full";
    }

    /// <summary>
    /// Calculate gas molecular weight request DTO
    /// </summary>
    public class CalculateGasMolecularWeightRequest
    {
        public GasComposition Composition { get; set; } = new();
        public string Method { get; set; } = "WeightedAverage";
    }

    /// <summary>
    /// Gas chromatography request DTO
    /// </summary>
    public class GasChromatographyRequest
    {
        public string SampleId { get; set; } = string.Empty;
        public string AnalysisMethod { get; set; } = "CapillaryGC";
    }

    /// <summary>
    /// Gas correlation matching request DTO
    /// </summary>
    public class GasCorrelationMatchingRequest
    {
        public string SampleId { get; set; } = string.Empty;
        public string PropertyType { get; set; } = string.Empty;
        public List<string> CorrelationIds { get; set; } = new();
    }

    /// <summary>
    /// Gas relative permeability request DTO
    /// </summary>
    public class GasRelativePermeabilityRequest
    {
        public decimal ResidualLiquidSaturation { get; set; }
        public decimal CriticalGasSaturation { get; set; }
        public string Correlation { get; set; } = "Corey";
    }

    /// <summary>
    /// Gas slippage factor request DTO
    /// </summary>
    public class GasSlippageFactorRequest
    {
        public decimal KnudsenNumber { get; set; }
        public decimal RarefactionParameter { get; set; }
        public string Correlation { get; set; } = "Beskok-Karniadakis";
    }

    /// <summary>
    /// Gas validation request DTO
    /// </summary>
    public class GasValidationRequest
    {
        public GasPropertyResult CalculationResult { get; set; } = new();
        public List<string> ValidationRules { get; set; } = new();
        public decimal? MeasuredValue { get; set; }
    }

    /// <summary>
    /// Gas uncertainty analysis request DTO
    /// </summary>
    public class GasUncertaintyAnalysisRequest
    {
        public string PropertyType { get; set; } = string.Empty;
        public Dictionary<string, decimal> BaseParameters { get; set; } = new();
        public Dictionary<string, Tuple<decimal, decimal>> ParameterUncertainties { get; set; } = new();
        public int MonteCarloIterations { get; set; } = 10000;
    }

    /// <summary>
    /// Gas QA report request DTO
    /// </summary>
    public class GasQAReportRequest
    {
        public string SampleId { get; set; } = string.Empty;
        public List<string> QASections { get; set; } = new();
        public bool IncludeRecommendations { get; set; } = true;
    }

    /// <summary>
    /// Gas PVT report request DTO
    /// </summary>
    public class GasPVTReportRequest
    {
        public string SampleId { get; set; } = string.Empty;
        public List<string> IncludeProperties { get; set; } = new();
        public bool IncludeCharts { get; set; } = true;
        public string Format { get; set; } = "PDF";
    }

    /// <summary>
    /// Gas export request DTO
    /// </summary>
    public class GasExportRequest
    {
        public string SampleId { get; set; } = string.Empty;
        public List<string> DataTypes { get; set; } = new();
        public string Format { get; set; } = "Excel";
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// Gas chart request DTO
    /// </summary>
    public class GasChartRequest
    {
        public string SampleId { get; set; } = string.Empty;
        public List<string> Properties { get; set; } = new();
        public string ChartType { get; set; } = "PVT";
        public int Width { get; set; } = 800;
        public int Height { get; set; } = 600;
    }

    #endregion
}