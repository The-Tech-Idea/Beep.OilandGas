using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
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
}
