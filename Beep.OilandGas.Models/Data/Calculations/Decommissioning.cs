using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.Calculations
{
    /// <summary>
    /// Comprehensive set of DTOs for well decommissioning and abandonment operations.
    /// Covers well plugging analysis, cost estimation, environmental remediation, regulatory compliance, and portfolio analysis.
    /// </summary>

    /// <summary>
    /// DTO for well plugging plan and requirements
    /// </summary>
    public class WellPluggingPlan : ModelEntityBase
    {
        public string WellUWI { get; set; }
        public double WellDepth { get; set; }
        public int ZonesIdentified { get; set; }
        public double FreshwaterAquiferDepth { get; set; }
        public DateTime AnalysisDate { get; set; }
        public List<string> CriticalZones { get; set; } = new();
        public string PluggingStrategy { get; set; }
        public double CementRequirements { get; set; }
        public List<string> PlugSpecifications { get; set; } = new();
        public int EstimatedDaysRequired { get; set; }
        public List<string> PotentialIssues { get; set; } = new();
    }

    /// <summary>
    /// DTO for decommissioning cost analysis
    /// </summary>
    public class DecommissioningCostAnalysis : ModelEntityBase
    {
        public string WellUWI { get; set; }
        public double WellDepth { get; set; }
        public string WellType { get; set; }
        public string Location { get; set; }
        public DateTime AnalysisDate { get; set; }
        public double WellPluggingCost { get; set; }
        public double WellheadRemovalCost { get; set; }
        public double SiteRestorationCost { get; set; }
        public double EnvironmentalRemediationCost { get; set; }
        public double AbandonmentBondCost { get; set; }
        public double TotalEstimatedCost { get; set; }
        public double PluggingCostPercentage { get; set; }
        public double WellheadRemovalPercentage { get; set; }
        public double SiteRestorationPercentage { get; set; }
        public double ContingencyAmount { get; set; }
        public double TotalWithContingency { get; set; }
    }

    /// <summary>
    /// DTO for environmental remediation analysis
    /// </summary>
    public class EnvironmentalRemediationAnalysis : ModelEntityBase
    {
        public string WellUWI { get; set; }
        public string Location { get; set; }
        public DateTime AnalysisDate { get; set; }
        public List<string> PotentialContaminants { get; set; } = new();
        public string EnvironmentalRiskLevel { get; set; }
        public List<string> RemediationActivities { get; set; } = new();
        public int EstimatedRemediationMonths { get; set; }
        public int MonitoringPeriodYears { get; set; }
        public double LongTermLiabilityCost { get; set; }
        public List<string> RegulatoryRequirements { get; set; } = new();
    }

    /// <summary>
    /// DTO for regulatory compliance analysis
    /// </summary>
    public class RegulatoryComplianceAnalysis : ModelEntityBase
    {
        public string WellUWI { get; set; }
        public string Jurisdiction { get; set; }
        public string WellClass { get; set; }
        public DateTime AbandonmentDate { get; set; }
        public DateTime AnalysisDate { get; set; }
        public List<string> ApplicableRegulations { get; set; } = new();
        public List<string> ComplianceRequirements { get; set; } = new();
        public DateTime ComplianceDeadlineDate { get; set; }
        public List<string> RequiredDocumentation { get; set; } = new();
        public string BondingRequirements { get; set; }
        public List<string> InspectionRequirements { get; set; } = new();
        public double ComplianceCostEstimate { get; set; }
    }

    /// <summary>
    /// DTO for salvage value analysis
    /// </summary>
    public class SalvageValueAnalysis : ModelEntityBase
    {
        public string WellUWI { get; set; }
        public string WellType { get; set; }
        public double WellDepth { get; set; }
        public DateTime AnalysisDate { get; set; }
        public double EquipmentSalvageValue { get; set; }
        public double MetalScrapValue { get; set; }
        public double WellheadEquipmentValue { get; set; }
        public double TotalSalvageValue { get; set; }
        public double SalvageValuePercentageOfDecomCost { get; set; }
        public List<string> SalvageableItems { get; set; } = new();
        public double TransportationCost { get; set; }
        public double NetSalvageValue { get; set; }
    }

    /// <summary>
    /// DTO for decommissioning project schedule
    /// </summary>
    public class DecommissioningSchedule : ModelEntityBase
    {
        public string WellUWI { get; set; }
        public double WellDepth { get; set; }
        public int PriorityLevel { get; set; }
        public DateTime PlannedStartDate { get; set; }
        public DateTime AnalysisDate { get; set; }
        public List<DecommissioningPhase> ProjectPhases { get; set; } = new();
        public int EstimatedTotalDays { get; set; }
        public DateTime EstimatedCompletionDate { get; set; }
        public List<string> CriticalPathItems { get; set; } = new();
        public string ScheduleRiskLevel { get; set; }
        public int ContingencyDays { get; set; }
        public DateTime FinalEstimatedDate { get; set; }
        public int EstimatedCrewSize { get; set; }
        public List<string> EstimatedEquipmentNeeds { get; set; } = new();
    }

    /// <summary>
    /// DTO for individual decommissioning phase
    /// </summary>
    public class DecommissioningPhase : ModelEntityBase
    {
        public string Phase { get; set; }
        public int Duration { get; set; }
    }

    /// <summary>
    /// DTO for abandonment feasibility assessment
    /// </summary>
    public class AbandonmentFeasibility : ModelEntityBase
    {
        public string WellUWI { get; set; }
        public double WellDepth { get; set; }
        public string WellStatus { get; set; }
        public DateTime LastProductionDate { get; set; }
        public DateTime AnalysisDate { get; set; }
        public string WellConditionStatus { get; set; }
        public bool AbandonmentFeasible { get; set; }
        public List<string> AbandonmentChallenges { get; set; } = new();
        public string RecommendedApproach { get; set; }
        public bool CanAbandonWithin12Months { get; set; }
        public double AbandonmentBenefit { get; set; }
        public double AbandonmentCost { get; set; }
        public double NetBenefit { get; set; }
        public string AbandonmentRiskLevel { get; set; }
    }

    /// <summary>
    /// DTO for portfolio-level decommissioning analysis
    /// </summary>
    public class PortfolioDecommissioningAnalysis : ModelEntityBase
    {
        public string FieldId { get; set; }
        public int WellsToDecommission { get; set; }
        public DateTime AnalysisDate { get; set; }
        public List<PortfolioWellDecommissioning> WellAnalyses { get; set; } = new();
        public double TotalEstimatedCost { get; set; }
        public int TotalEstimatedDays { get; set; }
        public double AverageCostPerWell { get; set; }
        public int AverageDaysPerWell { get; set; }
        public int PhaseCount { get; set; }
        public int WellsPerPhase { get; set; }
        public double ContingencyPercentage { get; set; }
        public double TotalWithContingency { get; set; }
    }

    /// <summary>
    /// DTO for individual well in portfolio analysis
    /// </summary>
    public class PortfolioWellDecommissioning : ModelEntityBase
    {
        public string WellUWI { get; set; }
        public double WellDepth { get; set; }
        public double EstimatedCost { get; set; }
        public int EstimatedDays { get; set; }
    }
}

