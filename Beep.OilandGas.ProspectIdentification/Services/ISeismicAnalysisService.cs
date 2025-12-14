using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs;

namespace Beep.OilandGas.ProspectIdentification.Services
{
    /// <summary>
    /// Service for analyzing seismic data.
    /// </summary>
    public interface ISeismicAnalysisService
    {
        /// <summary>
        /// Gets seismic surveys for a prospect.
        /// </summary>
        Task<List<SeismicSurveyDto>> GetSeismicSurveysAsync(string prospectId);

        /// <summary>
        /// Analyzes seismic data and returns interpretation results.
        /// </summary>
        Task<SeismicAnalysisResult> AnalyzeSeismicDataAsync(string surveyId);

        /// <summary>
        /// Creates a new seismic survey.
        /// </summary>
        Task<SeismicSurveyDto> CreateSeismicSurveyAsync(string prospectId, CreateSeismicSurveyDto createDto);
    }

    /// <summary>
    /// DTO for creating a seismic survey.
    /// </summary>
    public class CreateSeismicSurveyDto
    {
        public string SurveyName { get; set; } = string.Empty;
        public string? SurveyType { get; set; }
        public DateTime? SurveyDate { get; set; }
        public string? Contractor { get; set; }
        public decimal? AreaCovered { get; set; }
        public string? AreaUnit { get; set; }
    }

    /// <summary>
    /// Result of seismic analysis.
    /// </summary>
    public class SeismicAnalysisResult
    {
        public string SurveyId { get; set; } = string.Empty;
        public string? Interpretation { get; set; }
        public List<SeismicAnomaly> Anomalies { get; set; } = new();
        public List<DrillingTarget> DrillingTargets { get; set; } = new();
    }

    /// <summary>
    /// Seismic anomaly identified in analysis.
    /// </summary>
    public class SeismicAnomaly
    {
        public string AnomalyId { get; set; } = string.Empty;
        public string AnomalyType { get; set; } = string.Empty;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Depth { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// Drilling target identified from seismic analysis.
    /// </summary>
    public class DrillingTarget
    {
        public string TargetId { get; set; } = string.Empty;
        public string TargetName { get; set; } = string.Empty;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? TargetDepth { get; set; }
        public string? TargetFormation { get; set; }
        public decimal? Confidence { get; set; }
    }

    /// <summary>
    /// DTO for creating a seismic survey.
    /// </summary>
 
}

