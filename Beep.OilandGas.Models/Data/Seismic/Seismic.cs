using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    // Minimal placeholder DTOs for seismic/prospect identification features

    public class CreateSeismicSurvey : ModelEntityBase
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

    public class UpdateSeismicSurvey : ModelEntityBase
    {
        public string? SurveyId { get; init; }
        public string? Name { get; init; }
        public string? Description { get; init; }
        public DateTime? SurveyDate { get; init; }
    }

    public class SeismicInterpretationRequest : ModelEntityBase
    {
        public string? SurveyId { get; init; }
        public string? Payload { get; init; }
        public Dictionary<string, object>? Parameters { get; init; }
    }

    public class SeismicInterpretationResult : ModelEntityBase
    {
        public string? SurveyId { get; init; }
        public string? InterpretationId { get; init; }
        public DateTime? InterpretationDate { get; init; }
        public string? Interpreter { get; init; }
        public string? InterpretationSummary { get; init; }
        public List<StratigraphicUnit>? StratigraphicUnits { get; init; }
        public List<StructuralFeature>? StructuralFeatures { get; init; }
        public List<SeismicAnomaly>? Anomalies { get; init; }
        public string? OverallAssessment { get; init; }
        public string? ConfidenceLevel { get; init; }
    }

    public class StructuralFeature : ModelEntityBase
    {
        public string? Id { get; init; }
        public string? Name { get; init; }
        public double? Depth { get; init; }
        public string? Description { get; init; }
    }

    public class StratigraphicUnit : ModelEntityBase
    {
        public string? UnitId { get; init; }
        public string? Name { get; init; }
        public double? TopDepth { get; init; }
        public double? BaseDepth { get; init; }
        public string? Description { get; init; }
    }

    public class StratigraphicInterpretation : ModelEntityBase
    {
        public string? Summary { get; init; }
        public List<string>? Layers { get; init; }
    }

    public class SeismicAnomaly : ModelEntityBase
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

    public class SeismicAttributesRequest : ModelEntityBase
    {
        public string? SurveyId { get; init; }
        public string? Attribute { get; init; }
    }

    public class SeismicAttributesResult : ModelEntityBase
    {
        public string? SurveyId { get; init; }
        public string? Attribute { get; init; }
        public object? Values { get; init; }
    }

    public class SpectralDecompositionRequest : ModelEntityBase
    {
        public string? SurveyId { get; init; }
        public string? Parameters { get; init; }
    }

    public class SpectralDecompositionResult : ModelEntityBase
    {
        public string? SurveyId { get; init; }
        public object? Result { get; init; }
    }

    public class SeismicInversionRequest : ModelEntityBase
    {
        public string? SurveyId { get; init; }
        public string? Parameters { get; init; }
    }

    public class SeismicInversionResult : ModelEntityBase
    {
        public string? SurveyId { get; init; }
        public object? InversionVolume { get; init; }
    }

    public class CoherenceAnalysisRequest : ModelEntityBase
    {
        public string? SurveyId { get; init; }
    }

    public class CoherenceAnalysisResult : ModelEntityBase
    {
        public string? SurveyId { get; init; }
        public object? CoherenceVolume { get; init; }
    }

    public class AVOAnalysisRequest : ModelEntityBase
    {
        public string? SurveyId { get; init; }
    }

    public class AVOAnalysisResult : ModelEntityBase
    {
        public string? SurveyId { get; init; }
        public object? Result { get; init; }
    }

    public class AVOCrossplotRequest : ModelEntityBase
    {
        public string? SurveyId { get; init; }
    }

    public class AVOCrossplotResult : ModelEntityBase
    {
        public string? SurveyId { get; init; }
        public object? Plot { get; init; }
    }

    public class FluidSubstitutionRequest : ModelEntityBase
    {
        public string? SurveyId { get; init; }
        public string? FluidProperties { get; init; }
    }

    public class FluidSubstitutionResult : ModelEntityBase
    {
        public string? SurveyId { get; init; }
        public object? Result { get; init; }
    }

    public class TargetIdentificationRequest : ModelEntityBase
    {
        public string? SurveyId { get; init; }
        public string? Criteria { get; init; }
    }

    public class DrillingTarget : ModelEntityBase
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

    public class VolumetricAnalysisResult : ModelEntityBase
    {
        public double? STOIIP { get; init; }
        public double? OOIP { get; init; }
        public double? EstimatedRecoverable { get; init; }
    }

    public class ProspectRiskAssessment : ModelEntityBase
    {
        public string? ProspectId { get; init; }
        public double? RiskScore { get; init; }
        public List<RiskFactor>? RiskFactors { get; init; }
        public string? Summary { get; init; }
    }

    public class SeismicDataQuality : ModelEntityBase
    {
        public string? SurveyId { get; init; }
        public double? SignalToNoiseRatio { get; init; }
        public string? Notes { get; init; }
    }

    public class SeismicWellTieRequest : ModelEntityBase
    {
        public string? WellId { get; init; }
        public string? SurveyId { get; init; }
    }

    public class SeismicWellTieResult : ModelEntityBase
    {
        public string? WellId { get; init; }
        public string? SurveyId { get; init; }
        public string? ResultSummary { get; init; }
    }

    public class SeismicReportRequest : ModelEntityBase
    {
        public string? SurveyId { get; init; }
        public string? RequestedBy { get; init; }
    }

    public class SeismicReport : ModelEntityBase
    {
        public string? SurveyId { get; init; }
        public string? ReportText { get; init; }
        public List<string>? Attachments { get; init; }
    }
}


