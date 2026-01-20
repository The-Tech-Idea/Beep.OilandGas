using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.ProspectIdentification.Services
{
    /// <summary>
    /// Comprehensive seismic analysis service interface
    /// Provides seismic data interpretation, attribute analysis, AVO analysis, and prospect identification
    /// </summary>
    public interface ISeismicAnalysisService
    {
        #region Seismic Data Management

        /// <summary>
        /// Gets seismic surveys for a prospect or field
        /// </summary>
        Task<List<SeismicSurvey>> GetSeismicSurveysAsync(string? prospectId = null, string? fieldId = null, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Gets a specific seismic survey by ID
        /// </summary>
        Task<SeismicSurvey?> GetSeismicSurveyAsync(string surveyId);

        /// <summary>
        /// Creates a new seismic survey
        /// </summary>
        Task<SeismicSurvey> CreateSeismicSurveyAsync(CreateSeismicSurvey createDto, string userId);

        /// <summary>
        /// Updates an existing seismic survey
        /// </summary>
        Task<SeismicSurvey> UpdateSeismicSurveyAsync(string surveyId, UpdateSeismicSurvey updateDto, string userId);

        /// <summary>
        /// Deletes a seismic survey
        /// </summary>
        Task DeleteSeismicSurveyAsync(string surveyId, string userId);

        #endregion

        #region Seismic Interpretation

        /// <summary>
        /// Performs comprehensive seismic interpretation
        /// </summary>
        Task<SeismicInterpretationResult> PerformSeismicInterpretationAsync(string surveyId, SeismicInterpretationRequest request);

        /// <summary>
        /// Identifies structural features from seismic data
        /// </summary>
        Task<List<StructuralFeature>> IdentifyStructuralFeaturesAsync(string surveyId, SeismicInterpretationRequest request);

        /// <summary>
        /// Performs stratigraphic interpretation
        /// </summary>
        Task<StratigraphicInterpretation> PerformStratigraphicInterpretationAsync(string surveyId, SeismicInterpretationRequest request);

        /// <summary>
        /// Identifies seismic anomalies and potential hydrocarbon indicators
        /// </summary>
        Task<List<SeismicAnomaly>> IdentifySeismicAnomaliesAsync(string surveyId, SeismicInterpretationRequest request);

        #endregion

        #region Seismic Attributes Analysis

        /// <summary>
        /// Calculates seismic attributes (amplitude, frequency, phase, etc.)
        /// </summary>
        Task<SeismicAttributesResult> CalculateSeismicAttributesAsync(string surveyId, SeismicAttributesRequest request);

        /// <summary>
        /// Performs spectral decomposition analysis
        /// </summary>
        Task<SpectralDecompositionResult> PerformSpectralDecompositionAsync(string surveyId, SpectralDecompositionRequest request);

        /// <summary>
        /// Generates seismic inversion results
        /// </summary>
        Task<SeismicInversionResult> PerformSeismicInversionAsync(string surveyId, SeismicInversionRequest request);

        /// <summary>
        /// Performs coherence analysis
        /// </summary>
        Task<CoherenceAnalysisResult> PerformCoherenceAnalysisAsync(string surveyId, CoherenceAnalysisRequest request);

        #endregion

        #region AVO Analysis

        /// <summary>
        /// Performs Amplitude Versus Offset (AVO) analysis
        /// </summary>
        Task<AVOAnalysisResult> PerformAVOAnalysisAsync(string surveyId, AVOAnalysisRequest request);

        /// <summary>
        /// Generates AVO crossplots and classification
        /// </summary>
        Task<AVOCrossplotResult> GenerateAVOCrossplotAsync(string surveyId, AVOCrossplotRequest request);

        /// <summary>
        /// Performs fluid substitution modeling
        /// </summary>
        Task<FluidSubstitutionResult> PerformFluidSubstitutionAsync(string surveyId, FluidSubstitutionRequest request);

        #endregion

        #region Prospect Identification

        /// <summary>
        /// Identifies drilling targets from seismic analysis
        /// </summary>
        Task<List<DrillingTarget>> IdentifyDrillingTargetsAsync(string surveyId, TargetIdentificationRequest request);

        /// <summary>
        /// Performs volumetric analysis for prospects
        /// </summary>
        Task<VolumetricAnalysisResult> PerformVolumetricAnalysisAsync(string prospectId, VolumetricAnalysisRequest request);

        /// <summary>
        /// Generates prospect risk assessment
        /// </summary>
        Task<ProspectRiskAssessment> AssessProspectRiskAsync(string prospectId, ProspectRiskAssessmentRequest request);

        #endregion

        #region Quality Control & Validation

        /// <summary>
        /// Validates seismic data quality
        /// </summary>
        Task<SeismicDataQuality> ValidateSeismicDataQualityAsync(string surveyId);

        /// <summary>
        /// Performs seismic-well tie analysis
        /// </summary>
        Task<SeismicWellTieResult> PerformSeismicWellTieAsync(string surveyId, string wellUWI, SeismicWellTieRequest request);

        #endregion

        #region Reporting & Export

        /// <summary>
        /// Generates seismic interpretation report
        /// </summary>
        Task<SeismicReport> GenerateSeismicReportAsync(string surveyId, SeismicReportRequest request);

        /// <summary>
        /// Exports seismic data and results
        /// </summary>
        Task<byte[]> ExportSeismicDataAsync(string surveyId, string format = "SEG-Y");

        #endregion
    }

}

