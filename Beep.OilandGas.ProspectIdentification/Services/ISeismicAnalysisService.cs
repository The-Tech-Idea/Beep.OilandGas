using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs;

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
        Task<List<SeismicSurveyDto>> GetSeismicSurveysAsync(string? prospectId = null, string? fieldId = null, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Gets a specific seismic survey by ID
        /// </summary>
        Task<SeismicSurveyDto?> GetSeismicSurveyAsync(string surveyId);

        /// <summary>
        /// Creates a new seismic survey
        /// </summary>
        Task<SeismicSurveyDto> CreateSeismicSurveyAsync(CreateSeismicSurveyDto createDto, string userId);

        /// <summary>
        /// Updates an existing seismic survey
        /// </summary>
        Task<SeismicSurveyDto> UpdateSeismicSurveyAsync(string surveyId, UpdateSeismicSurveyDto updateDto, string userId);

        /// <summary>
        /// Deletes a seismic survey
        /// </summary>
        Task DeleteSeismicSurveyAsync(string surveyId, string userId);

        #endregion

        #region Seismic Interpretation

        /// <summary>
        /// Performs comprehensive seismic interpretation
        /// </summary>
        Task<SeismicInterpretationResultDto> PerformSeismicInterpretationAsync(string surveyId, SeismicInterpretationRequestDto request);

        /// <summary>
        /// Identifies structural features from seismic data
        /// </summary>
        Task<List<StructuralFeatureDto>> IdentifyStructuralFeaturesAsync(string surveyId, SeismicInterpretationRequestDto request);

        /// <summary>
        /// Performs stratigraphic interpretation
        /// </summary>
        Task<StratigraphicInterpretationDto> PerformStratigraphicInterpretationAsync(string surveyId, SeismicInterpretationRequestDto request);

        /// <summary>
        /// Identifies seismic anomalies and potential hydrocarbon indicators
        /// </summary>
        Task<List<SeismicAnomalyDto>> IdentifySeismicAnomaliesAsync(string surveyId, SeismicInterpretationRequestDto request);

        #endregion

        #region Seismic Attributes Analysis

        /// <summary>
        /// Calculates seismic attributes (amplitude, frequency, phase, etc.)
        /// </summary>
        Task<SeismicAttributesResultDto> CalculateSeismicAttributesAsync(string surveyId, SeismicAttributesRequestDto request);

        /// <summary>
        /// Performs spectral decomposition analysis
        /// </summary>
        Task<SpectralDecompositionResultDto> PerformSpectralDecompositionAsync(string surveyId, SpectralDecompositionRequestDto request);

        /// <summary>
        /// Generates seismic inversion results
        /// </summary>
        Task<SeismicInversionResultDto> PerformSeismicInversionAsync(string surveyId, SeismicInversionRequestDto request);

        /// <summary>
        /// Performs coherence analysis
        /// </summary>
        Task<CoherenceAnalysisResultDto> PerformCoherenceAnalysisAsync(string surveyId, CoherenceAnalysisRequestDto request);

        #endregion

        #region AVO Analysis

        /// <summary>
        /// Performs Amplitude Versus Offset (AVO) analysis
        /// </summary>
        Task<AVOAnalysisResultDto> PerformAVOAnalysisAsync(string surveyId, AVOAnalysisRequestDto request);

        /// <summary>
        /// Generates AVO crossplots and classification
        /// </summary>
        Task<AVOCrossplotResultDto> GenerateAVOCrossplotAsync(string surveyId, AVOCrossplotRequestDto request);

        /// <summary>
        /// Performs fluid substitution modeling
        /// </summary>
        Task<FluidSubstitutionResultDto> PerformFluidSubstitutionAsync(string surveyId, FluidSubstitutionRequestDto request);

        #endregion

        #region Prospect Identification

        /// <summary>
        /// Identifies drilling targets from seismic analysis
        /// </summary>
        Task<List<DrillingTargetDto>> IdentifyDrillingTargetsAsync(string surveyId, TargetIdentificationRequestDto request);

        /// <summary>
        /// Performs volumetric analysis for prospects
        /// </summary>
        Task<VolumetricAnalysisResultDto> PerformVolumetricAnalysisAsync(string prospectId, VolumetricAnalysisRequestDto request);

        /// <summary>
        /// Generates prospect risk assessment
        /// </summary>
        Task<ProspectRiskAssessmentDto> AssessProspectRiskAsync(string prospectId, RiskAssessmentRequestDto request);

        #endregion

        #region Quality Control & Validation

        /// <summary>
        /// Validates seismic data quality
        /// </summary>
        Task<SeismicDataQualityDto> ValidateSeismicDataQualityAsync(string surveyId);

        /// <summary>
        /// Performs seismic-well tie analysis
        /// </summary>
        Task<SeismicWellTieResultDto> PerformSeismicWellTieAsync(string surveyId, string wellUWI, SeismicWellTieRequestDto request);

        #endregion

        #region Reporting & Export

        /// <summary>
        /// Generates seismic interpretation report
        /// </summary>
        Task<SeismicReportDto> GenerateSeismicReportAsync(string surveyId, SeismicReportRequestDto request);

        /// <summary>
        /// Exports seismic data and results
        /// </summary>
        Task<byte[]> ExportSeismicDataAsync(string surveyId, string format = "SEG-Y");

        #endregion
    }

}

