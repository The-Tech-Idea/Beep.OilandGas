using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs
{
    // Minimal placeholder DTOs for seismic/prospect identification features

    public record CreateSeismicSurveyDto
    {
        public string? SurveyId { get; init; }
        public string? Name { get; init; }
        public string? SurveyName { get; init; }
        public string? SurveyType { get; init; }
        public string? Description { get; init; }
        public DateTime? SurveyDate { get; init; }
        public DateTime? AcquisitionDate { get; init; }
        public string? ProspectId { get; init; }
        public string? FieldId { get; init; }
        public string? CreatedBy { get; init; }
    }

    public record UpdateSeismicSurveyDto
    {
        public string? SurveyId { get; init; }
        public string? Name { get; init; }
        public string? Description { get; init; }
        public DateTime? SurveyDate { get; init; }
    }

    public record SeismicInterpretationRequestDto
    {
        public string? SurveyId { get; init; }
        public string? Payload { get; init; }
        public Dictionary<string, object>? Parameters { get; init; }
    }

    public record SeismicInterpretationResultDto
    {
        public string? SurveyId { get; init; }
        public string? InterpretationId { get; init; }
        public DateTime? InterpretationDate { get; init; }
        public string? Interpreter { get; init; }
        public string? InterpretationSummary { get; init; }
        public List<StratigraphicUnitDto>? StratigraphicUnits { get; init; }
        public List<StructuralFeatureDto>? StructuralFeatures { get; init; }
        public List<SeismicAnomalyDto>? Anomalies { get; init; }
        public string? OverallAssessment { get; init; }
        public string? ConfidenceLevel { get; init; }
    }

    public record StructuralFeatureDto
    {
        public string? Id { get; init; }
        public string? Name { get; init; }
        public double? Depth { get; init; }
        public string? Description { get; init; }
    }

    public record StratigraphicUnitDto
    {
        public string? UnitId { get; init; }
        public string? Name { get; init; }
        public double? TopDepth { get; init; }
        public double? BaseDepth { get; init; }
        public string? Description { get; init; }
    }

    public record StratigraphicInterpretationDto
    {
        public string? Summary { get; init; }
        public List<string>? Layers { get; init; }
    }

    public record SeismicAnomalyDto
    {
        public string? Id { get; init; }
        public string? AnomalyId { get; init; }
        public string? AnomalyType { get; init; }
        public string? Location { get; init; }
        public double? Magnitude { get; init; }
        public string? Description { get; init; }
        public double? Confidence { get; init; }
        public string? Notes { get; init; }
    }

    public record SeismicAttributesRequestDto
    {
        public string? SurveyId { get; init; }
        public string? Attribute { get; init; }
    }

    public record SeismicAttributesResultDto
    {
        public string? SurveyId { get; init; }
        public string? Attribute { get; init; }
        public object? Values { get; init; }
    }

    public record SpectralDecompositionRequestDto
    {
        public string? SurveyId { get; init; }
        public string? Parameters { get; init; }
    }

    public record SpectralDecompositionResultDto
    {
        public string? SurveyId { get; init; }
        public object? Result { get; init; }
    }

    public record SeismicInversionRequestDto
    {
        public string? SurveyId { get; init; }
        public string? Parameters { get; init; }
    }

    public record SeismicInversionResultDto
    {
        public string? SurveyId { get; init; }
        public object? InversionVolume { get; init; }
    }

    public record CoherenceAnalysisRequestDto
    {
        public string? SurveyId { get; init; }
    }

    public record CoherenceAnalysisResultDto
    {
        public string? SurveyId { get; init; }
        public object? CoherenceVolume { get; init; }
    }

    public record AVOAnalysisRequestDto
    {
        public string? SurveyId { get; init; }
    }

    public record AVOAnalysisResultDto
    {
        public string? SurveyId { get; init; }
        public object? Result { get; init; }
    }

    public record AVOCrossplotRequestDto
    {
        public string? SurveyId { get; init; }
    }

    public record AVOCrossplotResultDto
    {
        public string? SurveyId { get; init; }
        public object? Plot { get; init; }
    }

    public record FluidSubstitutionRequestDto
    {
        public string? SurveyId { get; init; }
        public string? FluidProperties { get; init; }
    }

    public record FluidSubstitutionResultDto
    {
        public string? SurveyId { get; init; }
        public object? Result { get; init; }
    }

    public record TargetIdentificationRequestDto
    {
        public string? SurveyId { get; init; }
        public string? Criteria { get; init; }
    }

    public record DrillingTargetDto
    {
        public string? TargetId { get; init; }
        public string? TargetName { get; init; }
        public string? Name { get; init; }
        public double? TargetDepth { get; init; }
        public double? Depth { get; init; }
        public double? Confidence { get; init; }
        public double? Latitude { get; init; }
        public double? Longitude { get; init; }
        public double? RiskScore { get; init; }
    }

    public record VolumetricAnalysisResultDto
    {
        public double? STOIIP { get; init; }
        public double? OOIP { get; init; }
        public double? EstimatedRecoverable { get; init; }
    }

    public record ProspectRiskAssessmentDto
    {
        public string? ProspectId { get; init; }
        public double? RiskScore { get; init; }
        public List<RiskFactorDto>? RiskFactors { get; init; }
        public string? Summary { get; init; }
    }

    public record SeismicDataQualityDto
    {
        public string? SurveyId { get; init; }
        public double? SignalToNoiseRatio { get; init; }
        public string? Notes { get; init; }
    }

    public record SeismicWellTieRequestDto
    {
        public string? WellId { get; init; }
        public string? SurveyId { get; init; }
    }

    public record SeismicWellTieResultDto
    {
        public string? WellId { get; init; }
        public string? SurveyId { get; init; }
        public string? ResultSummary { get; init; }
    }

    public record SeismicReportRequestDto
    {
        public string? SurveyId { get; init; }
        public string? RequestedBy { get; init; }
    }

    public record SeismicReportDto
    {
        public string? SurveyId { get; init; }
        public string? ReportText { get; init; }
        public List<string>? Attachments { get; init; }
    }
}
