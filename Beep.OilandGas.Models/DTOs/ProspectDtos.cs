using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs
{
    /// <summary>
    /// DTO for prospect information.
    /// </summary>
    public class ProspectDto
    {
        public string ProspectId { get; set; } = string.Empty;
        public string FieldId { get; set; } = string.Empty;
        public string ProspectName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? Country { get; set; }
        public string? StateProvince { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? EvaluationDate { get; set; }
        public string? EvaluatedBy { get; set; }
        public decimal? EstimatedResources { get; set; }
        public string? ResourceUnit { get; set; }
        public decimal? RiskScore { get; set; }
        public string? RiskLevel { get; set; }
        public List<SeismicSurveyDto> SeismicSurveys { get; set; } = new();
        public ProspectEvaluationDto? Evaluation { get; set; }
    }

    /// <summary>
    /// DTO for seismic survey information.
    /// </summary>
    public class SeismicSurveyDto
    {
        public string SurveyId { get; set; } = string.Empty;
        public string ProspectId { get; set; } = string.Empty;
        public string SurveyName { get; set; } = string.Empty;
        public string? SurveyType { get; set; }
        public DateTime? SurveyDate { get; set; }
        public string? Contractor { get; set; }
        public decimal? AreaCovered { get; set; }
        public string? AreaUnit { get; set; }
        public string? Status { get; set; }
        public string? InterpretationStatus { get; set; }
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// DTO for prospect evaluation results.
    /// </summary>
    public class ProspectEvaluationDto
    {
        public string EvaluationId { get; set; } = string.Empty;
        public string ProspectId { get; set; } = string.Empty;
        public DateTime EvaluationDate { get; set; }
        public string EvaluatedBy { get; set; } = string.Empty;
        public decimal? EstimatedOilResources { get; set; }
        public decimal? EstimatedGasResources { get; set; }
        public string? ResourceUnit { get; set; }
        public decimal? ProbabilityOfSuccess { get; set; }
        public decimal? RiskScore { get; set; }
        public string? RiskLevel { get; set; }
        public string? Recommendation { get; set; }
        public string? Remarks { get; set; }
        public List<RiskFactorDto> RiskFactors { get; set; } = new();
    }

    /// <summary>
    /// DTO for risk factors in prospect evaluation.
    /// </summary>
    public class RiskFactorDto
    {
        public string RiskFactorId { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal? RiskScore { get; set; }
        public string? Mitigation { get; set; }
    }

    /// <summary>
    /// DTO for creating a new prospect.
    /// </summary>
    public class CreateProspectDto
    {
        public string ProspectName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? Country { get; set; }
        public string? StateProvince { get; set; }
        public string? FieldId { get; set; }
    }

    /// <summary>
    /// DTO for updating a prospect.
    /// </summary>
    public class UpdateProspectDto
    {
        public string? ProspectName { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public decimal? EstimatedResources { get; set; }
        public string? ResourceUnit { get; set; }
    }
}




