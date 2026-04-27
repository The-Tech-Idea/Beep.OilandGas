using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
    /// <summary>
    /// Request body for seismic interpretation analysis (workflow maturation API).
    /// </summary>
    public class SeismicInterpretationAnalysisRequest
    {
        public string ProspectId { get; set; } = string.Empty;
        public string SurveyId { get; set; } = string.Empty;
        public List<Horizon> Horizons { get; set; } = new();
        public List<Fault> Faults { get; set; } = new();
    }

    /// <summary>
    /// Request body for volumetric resource estimation (workflow maturation API).
    /// </summary>
    public class ResourceEstimationRequest
    {
        public string ProspectId { get; set; } = string.Empty;
        public decimal GrossRockVolume { get; set; }
        public decimal NetToGrossRatio { get; set; }
        public decimal Porosity { get; set; }
        public decimal WaterSaturation { get; set; }
        public string EstimatedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request body for trap geometry analysis (workflow maturation API).
    /// </summary>
    public class TrapGeometryAnalysisRequest
    {
        public string ProspectId { get; set; } = string.Empty;
        public string TrapType { get; set; } = string.Empty;
        public decimal CrestDepth { get; set; }
        public decimal SpillPointDepth { get; set; }
        public decimal Area { get; set; }
        public decimal Volume { get; set; }
    }

    /// <summary>
    /// Request body for migration path analysis (workflow maturation API).
    /// </summary>
    public class MigrationPathAnalysisRequest
    {
        public string ProspectId { get; set; } = string.Empty;
        public string SourceRockId { get; set; } = string.Empty;
        public decimal MaturityLevel { get; set; }
        public decimal DistanceKm { get; set; }
    }

    /// <summary>
    /// Request body for seal and source assessment (workflow maturation API).
    /// </summary>
    public class SealSourceAssessmentRequest
    {
        public string ProspectId { get; set; } = string.Empty;
        public string SealRockType { get; set; } = string.Empty;
        public decimal SealThickness { get; set; }
        public string SourceRockType { get; set; } = string.Empty;
        public decimal SourceMaturity { get; set; }
    }

    /// <summary>
    /// Request body for prospect risk assessment (workflow risk/economics API).
    /// </summary>
    public class WorkflowRiskAssessmentRequest
    {
        public string ProspectId { get; set; } = string.Empty;
        public string AssessedBy { get; set; } = string.Empty;
        public Dictionary<string, decimal> RiskScores { get; set; } = new();
    }

    /// <summary>
    /// Request body for economic viability screening (workflow risk/economics API).
    /// </summary>
    public class EconomicViabilityRequest
    {
        public string ProspectId { get; set; } = string.Empty;
        public decimal EstimatedOil { get; set; }
        public decimal EstimatedGas { get; set; }
        public decimal CapitalCost { get; set; }
        public decimal OperatingCost { get; set; }
        public decimal OilPrice { get; set; }
        public decimal GasPrice { get; set; }
    }

    /// <summary>
    /// Request body for portfolio optimization on ranked prospects (workflow portfolio API).
    /// </summary>
    public class PortfolioOptimizationWorkflowRequest
    {
        public List<ProspectRanking> RankedProspects { get; set; } = new();
        public decimal RiskTolerance { get; set; }
        public decimal CapitalBudget { get; set; }
    }
}
